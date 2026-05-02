using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class LearningPlanService(
    ApplicationDbContext db,
    AdaptiveRoutineService adaptiveRoutineService,
    PortuguesePlanningService portuguesePlanningService)
{
    private const int DailyPlanThemeMaxLength = 180;
    private const int DailyPlanParentSummaryMaxLength = 1500;
    private const int DailyPlanChildNarrativeMaxLength = 1500;
    private const int DailyPlanRecoveryHeadlineMaxLength = 220;
    private const int DailyPlanBlockSkillCodeMaxLength = 80;
    private const int DailyPlanBlockSkillNameMaxLength = 180;
    private const int DailyPlanBlockTitleMaxLength = 180;
    private const int DailyPlanBlockGoalMaxLength = 700;
    private const int DailyPlanBlockParentGuideMaxLength = 1500;
    private const int DailyPlanBlockChildPromptMaxLength = 700;
    private const int DailyPlanBlockMaterialsMaxLength = 700;
    private const int DailyPlanBlockEvidencePromptMaxLength = 700;
    private const int DailyPlanBlockRecoveryNoteMaxLength = 500;
    private const int DailyPlanBlockReviewNoteMaxLength = 500;

    public async Task<DailyPlan> EnsurePlanAsync(ChildProfile child, DateTime forDate)
    {
        var targetDate = forDate.Date;
        var age = CalculateAge(child.BirthDate, targetDate);
        age = Math.Clamp(age, 3, 14);

        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        templates = templates
            .Where(x => IsTemplateAllowedForProfile(x.SupportScope, child.SupportProfile))
            .ToList();

        var skillProgress = await db.ChildSkillProgressEntries
            .Where(x => x.ChildId == child.Id && x.Age == age)
            .ToListAsync();
        var recoveryDay = await GetRecoveryDayAsync(child.Id, targetDate);
        var adaptiveSnapshot = await adaptiveRoutineService.BuildSnapshotAsync(child.Id, targetDate);
        var portugueseGuidance = portuguesePlanningService.GetDailyGuidance(age, targetDate);
        var effectiveGuidanceStyle = adaptiveSnapshot.Intensity == AdaptiveSupportIntensity.High
            ? "focus_support"
            : child.GuidanceStyle;

        if (templates.Count == 0)
        {
            throw new InvalidOperationException($"Nao ha curriculo configurado para a idade {age}.");
        }

        var directiveContext = await GetDirectiveContextAsync(child.Id, targetDate, templates);

        var existing = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.ChildId == child.Id && x.PlannedDate == targetDate);

        if (existing is not null)
        {
            var needsUpgrade = existing.Blocks.Count < 4 ||
                               existing.Theme.StartsWith("Dia de crescimento", StringComparison.OrdinalIgnoreCase) ||
                               (child.SupportProfile != SupportProfile.General &&
                                existing.Blocks.All(x => x.FunctionalTrack == FunctionalSupportTrack.Base)) ||
                               (await HasActiveRecoveryPlanAsync(child.Id, targetDate) && !existing.IsRecoveryPlan) ||
                               RequiresDirectiveRefresh(existing, directiveContext, templates);

            if (!needsUpgrade || existing.Sessions.Count > 0)
            {
                if (!needsUpgrade && directiveContext.Directives.Count > 0)
                {
                    await MarkDirectivesAsAppliedAsync(directiveContext.Directives);
                }

                return existing;
            }

            db.DailyPlanBlocks.RemoveRange(existing.Blocks);
            db.DailyPlans.Remove(existing);
            await db.SaveChangesAsync();
        }

        var chosen = SelectDailyTemplates(
            templates,
            skillProgress,
            age,
            targetDate,
            child.FullName,
            child.SupportProfile,
            child.FamilyGoalTrack,
            child.TeachingMethodology,
            child.LearningProfile,
            effectiveGuidanceStyle,
            recoveryDay,
            directiveContext.PinnedTemplates,
            directiveContext.FocusTrack,
            portugueseGuidance);
        var dueReviewMap = BuildDueReviewMap(templates, skillProgress, targetDate);

        var totalMinutes = Math.Max(30, child.DailyStudyMinutes);
        var durations = BuildDurations(totalMinutes, chosen.Count, age, effectiveGuidanceStyle, adaptiveSnapshot.WorkBlockMinutes);
        var theme = directiveContext.FocusTrack.HasValue
            ? BuildFocusedTrackTheme(directiveContext.FocusTrack.Value)
            : BuildTheme(age, targetDate, portugueseGuidance);
        var resolvedTheme = FitForStorage(
            recoveryDay is null ? theme : $"Retomada com foco em {recoveryDay.FocusSkill}",
            DailyPlanThemeMaxLength);
        var resolvedParentSummary = FitForStorage(
            BuildParentSummary(chosen, age, child, theme, recoveryDay, directiveContext, adaptiveSnapshot, portugueseGuidance),
            DailyPlanParentSummaryMaxLength);
        var resolvedChildNarrative = FitForStorage(
            BuildChildNarrative(chosen, age, theme, child.LearningProfile, directiveContext.FocusTrack, portugueseGuidance),
            DailyPlanChildNarrativeMaxLength);
        var resolvedRecoveryHeadline = FitForStorage(
            recoveryDay is null
                ? string.Empty
                : $"Hoje a prioridade e recuperar ritmo em {recoveryDay.FocusSkill.ToLowerInvariant()}.",
            DailyPlanRecoveryHeadlineMaxLength);

        var plan = new DailyPlan
        {
            ChildId = child.Id,
            PlannedDate = targetDate,
            AgeAtGeneration = age,
            Theme = resolvedTheme,
            ParentSummary = resolvedParentSummary,
            ChildNarrative = resolvedChildNarrative,
            IsRecoveryPlan = recoveryDay is not null,
            RecoveryHeadline = resolvedRecoveryHeadline
        };

        for (var i = 0; i < chosen.Count; i++)
        {
            var item = chosen[i];
            var resolvedGoal = FitForStorage(
                $"{item.Goal} {BuildPortugueseGoalCue(item, portugueseGuidance)}".Trim(),
                DailyPlanBlockGoalMaxLength);
            var resolvedParentGuide = FitForStorage(
                $"{item.ParentGuide} {BuildMethodologyGuide(child.TeachingMethodology, item.Domain, item.GoalTrack)} {BuildAdaptiveCue(adaptiveSnapshot, i)} {BuildPortugueseGuideCue(item, portugueseGuidance)}".Trim(),
                DailyPlanBlockParentGuideMaxLength);
            var resolvedMaterials = FitForStorage(
                $"{item.Materials} {BuildMethodologyMaterials(child.TeachingMethodology, item.Domain)}".Trim(),
                DailyPlanBlockMaterialsMaxLength);
            var resolvedEvidencePrompt = FitForStorage(
                $"{item.EvidencePrompt} {BuildMethodologyEvaluation(child.TeachingMethodology, item.Domain)} {BuildPortugueseEvidenceCue(item, portugueseGuidance)}".Trim(),
                DailyPlanBlockEvidencePromptMaxLength);
            var resolvedRecoveryNote = FitForStorage(
                recoveryDay is not null && string.Equals(item.SkillCode, recoveryDay.SkillCode, StringComparison.OrdinalIgnoreCase)
                    ? recoveryDay.GoalText
                    : string.Empty,
                DailyPlanBlockRecoveryNoteMaxLength);
            var resolvedReviewNote = FitForStorage(
                dueReviewMap.TryGetValue(item.SkillCode, out var reviewNote)
                    ? reviewNote
                    : string.Empty,
                DailyPlanBlockReviewNoteMaxLength);

            plan.Blocks.Add(new DailyPlanBlock
            {
                Domain = item.Domain,
                SupportScope = item.SupportScope,
                FunctionalTrack = item.FunctionalTrack,
                SourceTemplateId = item.Id,
                SkillCode = FitForStorage(item.SkillCode, DailyPlanBlockSkillCodeMaxLength),
                SkillName = FitForStorage(item.SkillName, DailyPlanBlockSkillNameMaxLength),
                Title = FitForStorage(item.Title, DailyPlanBlockTitleMaxLength),
                Goal = resolvedGoal,
                ParentGuide = resolvedParentGuide,
                ChildPrompt = FitForStorage(item.ChildMission, DailyPlanBlockChildPromptMaxLength),
                Materials = resolvedMaterials,
                EvidencePrompt = resolvedEvidencePrompt,
                IsRecoveryFocus = recoveryDay is not null && string.Equals(item.SkillCode, recoveryDay.SkillCode, StringComparison.OrdinalIgnoreCase),
                RecoveryNote = resolvedRecoveryNote,
                IsSpacedReview = dueReviewMap.TryGetValue(item.SkillCode, out _),
                ReviewNote = resolvedReviewNote,
                DurationMinutes = durations[i],
                SortOrder = i + 1
            });
        }

        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync();
        await MarkDirectivesAsAppliedAsync(directiveContext.Directives);

        return await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .FirstAsync(x => x.Id == plan.Id);
    }

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static List<CurriculumTemplate> SelectDailyTemplates(
        List<CurriculumTemplate> templates,
        List<ChildSkillProgress> progressEntries,
        int age,
        DateTime targetDate,
        string childName,
        SupportProfile supportProfile,
        string familyGoalTrack,
        string teachingMethodology,
        string learningProfile,
        string guidanceStyle,
        RecoveryTemplateHint? recoveryDay,
        IReadOnlyList<CurriculumTemplate> pinnedTemplates,
        FunctionalSupportTrack? focusedTrack,
        PortuguesePlanningGuidance? portugueseGuidance)
    {
        var domainMap = templates
            .GroupBy(x => x.Domain)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SortOrder).ToList());
        var progressMap = progressEntries.ToDictionary(x => x.SkillCode, x => x);

        var chosen = new List<CurriculumTemplate>();
        var seed = Math.Abs(targetDate.DayOfYear + childName.Length + age);
        var orderedTemplates = templates
            .OrderByDescending(x => string.Equals(x.GoalTrack, familyGoalTrack, StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => GetSupportPreferenceWeight(x.SupportScope, supportProfile))
            .ThenBy(x => GetMethodologyPreferenceWeight(x, teachingMethodology))
            .ThenBy(x => x.SortOrder)
            .ToList();
        var reviewTemplates = BuildDueReviewMap(orderedTemplates, progressEntries, targetDate)
            .Keys
            .Select(skillCode => ResolveTemplateWithPrerequisite(orderedTemplates, progressMap, orderedTemplates.First(x => x.SkillCode == skillCode)))
            .Where(x => x is not null)
            .Cast<CurriculumTemplate>()
            .ToList();

        if (recoveryDay is not null)
        {
            var recoveryTemplate = orderedTemplates.FirstOrDefault(x => string.Equals(x.SkillCode, recoveryDay.SkillCode, StringComparison.OrdinalIgnoreCase))
                ?? orderedTemplates.FirstOrDefault(x => string.Equals(x.SkillName, recoveryDay.FocusSkill, StringComparison.OrdinalIgnoreCase));
            AddIfAvailable(chosen, recoveryTemplate);
        }

        foreach (var pinnedTemplate in pinnedTemplates
                     .Where(x => templates.Any(t => t.Id == x.Id))
                     .OrderBy(x => x.SortOrder)
                     .Take(3))
        {
            AddIfAvailable(chosen, pinnedTemplate);
            if (chosen.Count >= 3)
            {
                break;
            }
        }

        foreach (var reviewTemplate in reviewTemplates)
        {
            if (chosen.Count == 4)
            {
                break;
            }

            AddIfAvailable(chosen, reviewTemplate);
            if (chosen.Count >= 2)
            {
                break;
            }
        }

        if (focusedTrack.HasValue)
        {
            var focusedTemplates = orderedTemplates
                .Where(x => x.FunctionalTrack == focusedTrack.Value)
                .ToList();
            var focusedSequenceSource = BuildDomainSequenceSource(focusedTemplates, supportProfile, familyGoalTrack);
            var currentWindow = BuildCurrentSequenceWindow(focusedSequenceSource, progressMap, targetDate);
            var targetSequenceIndex = GetStepAlignedIndex(currentWindow.Count, GetWeekdayStepNumber(targetDate.DayOfWeek));
            var rankedFocusedTemplates = currentWindow
                .Select(x => ResolveTemplateWithPrerequisite(focusedSequenceSource, progressMap, x))
                .Where(x => x is not null)
                .Cast<CurriculumTemplate>()
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .OrderBy(x => GetSequenceBucket(x, currentWindow, focusedSequenceSource))
                .ThenBy(x => GetSequenceDistance(x, currentWindow, targetSequenceIndex))
                .ThenBy(x => GetProgressSequencePenalty(x, progressMap))
                .ThenBy(x => GetPriority(x, progressMap, seed, teachingMethodology, supportProfile, portugueseGuidance))
                .ThenBy(x => x.SortOrder)
                .ToList();

            foreach (var focusedTemplate in rankedFocusedTemplates)
            {
                AddIfAvailable(chosen, focusedTemplate);
                if (chosen.Count == 4)
                {
                    break;
                }
            }
        }
        else
        {
            var targetLanguageBlockCount = Math.Clamp(portugueseGuidance?.RecommendedDailyLanguageBlocks ?? 1, 1, 2);
            AddPortugueseGuidedTemplates(
                chosen,
                domainMap,
                progressMap,
                supportProfile,
                teachingMethodology,
                familyGoalTrack,
                portugueseGuidance,
                targetDate,
                seed,
                targetLanguageBlockCount);

            if (chosen.Count(x => x.Domain == LearningDomain.Language) < targetLanguageBlockCount)
            {
                AddIfAvailable(chosen, PickFromDomain(domainMap, progressMap, LearningDomain.Language, targetDate, seed, supportProfile, familyGoalTrack, teachingMethodology, portugueseGuidance));
            }

            AddIfAvailable(chosen, PickFromDomain(domainMap, progressMap, LearningDomain.Math, targetDate, seed + 1, supportProfile, familyGoalTrack, teachingMethodology, portugueseGuidance));

            var thirdDomain = PickProfileDomain(learningProfile, targetDate.DayOfWeek is DayOfWeek.Tuesday or DayOfWeek.Thursday
                ? LearningDomain.ExecutiveFunction
                : LearningDomain.World);
            AddIfAvailable(chosen, PickFromDomain(domainMap, progressMap, thirdDomain, targetDate, seed + 2, supportProfile, familyGoalTrack, teachingMethodology, portugueseGuidance));

            var fourthDomain = guidanceStyle == "autonomy"
                ? LearningDomain.ExecutiveFunction
                : age <= 4
                ? LearningDomain.ExecutiveFunction
                : targetDate.DayOfWeek is DayOfWeek.Wednesday or DayOfWeek.Friday
                    ? LearningDomain.World
                    : LearningDomain.Language;

            AddPortugueseGuidedTemplates(
                chosen,
                domainMap,
                progressMap,
                supportProfile,
                teachingMethodology,
                familyGoalTrack,
                portugueseGuidance,
                targetDate,
                seed + 3,
                targetLanguageBlockCount);

            if (chosen.Count < 4)
            {
                AddIfAvailable(chosen, PickFromDomain(domainMap, progressMap, fourthDomain, targetDate, seed + 3, supportProfile, familyGoalTrack, teachingMethodology, portugueseGuidance));
            }
        }

        if (chosen.Count < 4)
        {
            foreach (var item in orderedTemplates
                         .OrderByDescending(x => string.Equals(x.GoalTrack, familyGoalTrack, StringComparison.OrdinalIgnoreCase))
                         .ThenBy(x => GetSupportPreferenceWeight(x.SupportScope, supportProfile))
                         .ThenBy(x => GetPriority(x, progressMap, seed, teachingMethodology, supportProfile, portugueseGuidance))
                         .ThenBy(x => x.SortOrder))
            {
                AddIfAvailable(chosen, item);
                if (chosen.Count == 4)
                {
                    break;
                }
            }
        }

        return chosen;
    }

    private static LearningDomain PickProfileDomain(string learningProfile, LearningDomain fallback)
    {
        return learningProfile switch
        {
            "hands_on" => LearningDomain.World,
            "story_based" => LearningDomain.Language,
            "visual" => LearningDomain.Math,
            "movement" => LearningDomain.ExecutiveFunction,
            _ => fallback
        };
    }

    private static CurriculumTemplate? PickFromDomain(
        IReadOnlyDictionary<LearningDomain, List<CurriculumTemplate>> domainMap,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap,
        LearningDomain domain,
        DateTime targetDate,
        int seed,
        SupportProfile supportProfile,
        string familyGoalTrack,
        string teachingMethodology,
        PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (!domainMap.TryGetValue(domain, out var items) || items.Count == 0)
        {
            return null;
        }

        var sequenceSource = BuildDomainSequenceSource(items, supportProfile, familyGoalTrack);
        var currentWindow = BuildCurrentSequenceWindow(sequenceSource, progressMap, targetDate);
        var targetSequenceIndex = GetStepAlignedIndex(currentWindow.Count, GetWeekdayStepNumber(targetDate.DayOfWeek));

        var sequencedCandidate = currentWindow
            .Select(x => ResolveTemplateWithPrerequisite(sequenceSource, progressMap, x))
            .Where(x => x is not null)
            .Cast<CurriculumTemplate>()
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .OrderBy(x => GetSequenceBucket(x, currentWindow, sequenceSource))
            .ThenBy(x => GetSequenceDistance(x, currentWindow, targetSequenceIndex))
            .ThenBy(x => GetProgressSequencePenalty(x, progressMap))
            .ThenBy(x => GetPriority(x, progressMap, seed, teachingMethodology, supportProfile, portugueseGuidance))
            .ThenBy(x => x.SortOrder)
            .FirstOrDefault();

        if (sequencedCandidate is not null)
        {
            return sequencedCandidate;
        }

        return sequenceSource
            .OrderBy(x => GetProgressSequencePenalty(x, progressMap))
            .ThenBy(x => GetPriority(x, progressMap, seed, teachingMethodology, supportProfile, portugueseGuidance))
            .ThenBy(x => x.SortOrder)
            .Select(x => ResolveTemplateWithPrerequisite(sequenceSource, progressMap, x))
            .FirstOrDefault(x => x is not null);
    }

    private static void AddPortugueseGuidedTemplates(
        ICollection<CurriculumTemplate> chosen,
        IReadOnlyDictionary<LearningDomain, List<CurriculumTemplate>> domainMap,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap,
        SupportProfile supportProfile,
        string teachingMethodology,
        string familyGoalTrack,
        PortuguesePlanningGuidance? portugueseGuidance,
        DateTime targetDate,
        int seed,
        int targetLanguageBlockCount)
    {
        if (portugueseGuidance is null ||
            chosen.Count == 4 ||
            chosen.Count(x => x.Domain == LearningDomain.Language) >= targetLanguageBlockCount ||
            !domainMap.TryGetValue(LearningDomain.Language, out var languageItems) ||
            languageItems.Count == 0)
        {
            return;
        }

        var sequenceSource = BuildDomainSequenceSource(languageItems, supportProfile, familyGoalTrack);
        var currentWindow = BuildCurrentSequenceWindow(sequenceSource, progressMap, targetDate);
        var targetSequenceIndex = GetStepAlignedIndex(currentWindow.Count, GetWeekdayStepNumber(targetDate.DayOfWeek));

        var preferredTitles = portugueseGuidance.PreferredTemplateTitles
            .Select(NormalizeForSearch)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.Ordinal);

        var candidates = currentWindow
            .OrderBy(x => preferredTitles.Contains(NormalizeForSearch(x.Title)) ? 0 : 1)
            .ThenBy(x => GetSequenceDistance(x, currentWindow, targetSequenceIndex))
            .ThenBy(x => GetProgressSequencePenalty(x, progressMap))
            .ThenBy(x => GetMethodologyPreferenceWeight(x, teachingMethodology))
            .ThenBy(x => GetPriority(x, progressMap, seed, teachingMethodology, supportProfile, portugueseGuidance))
            .ThenBy(x => x.SortOrder)
            .Select(x => ResolveTemplateWithPrerequisite(sequenceSource, progressMap, x))
            .Where(x => x is not null)
            .Cast<CurriculumTemplate>()
            .GroupBy(x => x.Id)
            .Select(x => x.First());

        foreach (var candidate in candidates)
        {
            if (chosen.Any(x => x.Id == candidate.Id))
            {
                continue;
            }

            chosen.Add(candidate);
            if (chosen.Count == 4 || chosen.Count(x => x.Domain == LearningDomain.Language) >= targetLanguageBlockCount)
            {
                break;
            }
        }
    }

    private static List<CurriculumTemplate> BuildDomainSequenceSource(
        IReadOnlyList<CurriculumTemplate> items,
        SupportProfile supportProfile,
        string familyGoalTrack)
    {
        if (items.Count == 0)
        {
            return [];
        }

        var preferredTrackItems = items
            .Where(x => string.Equals(x.GoalTrack, familyGoalTrack, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var selectedTrackItems = preferredTrackItems.Count > 0
            ? preferredTrackItems
            : items
                .Where(x => string.Equals(x.GoalTrack, "balanced_growth", StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (selectedTrackItems.Count == 0)
        {
            selectedTrackItems = items.ToList();
        }

        var bestSupportWeight = selectedTrackItems.Min(x => GetSupportPreferenceWeight(x.SupportScope, supportProfile));
        var selectedSupportItems = selectedTrackItems
            .Where(x => GetSupportPreferenceWeight(x.SupportScope, supportProfile) == bestSupportWeight)
            .OrderBy(x => x.SortOrder)
            .ToList();

        return selectedSupportItems.Count > 0
            ? selectedSupportItems
            : selectedTrackItems.OrderBy(x => x.SortOrder).ToList();
    }

    private static List<CurriculumTemplate> BuildCurrentSequenceWindow(
        IReadOnlyList<CurriculumTemplate> sequenceSource,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap,
        DateTime targetDate)
    {
        if (sequenceSource.Count == 0)
        {
            return [];
        }

        var currentPhase = GetPhaseTemplates(sequenceSource, GetCurrentPhaseIndex(targetDate.Month));
        var currentUnit = GetUnitTemplates(currentPhase.Count > 0 ? currentPhase : sequenceSource, GetPhaseMonthSlot(targetDate.Month));

        var openUnit = currentUnit
            .Where(x => IsTemplateOpenForSequence(x, progressMap))
            .ToList();
        if (openUnit.Count > 0)
        {
            return openUnit;
        }

        var openPhase = currentPhase
            .Where(x => IsTemplateOpenForSequence(x, progressMap))
            .ToList();
        if (openPhase.Count > 0)
        {
            return openPhase;
        }

        var openYear = sequenceSource
            .Where(x => IsTemplateOpenForSequence(x, progressMap))
            .ToList();
        if (openYear.Count > 0)
        {
            return openYear;
        }

        if (currentUnit.Count > 0)
        {
            return currentUnit;
        }

        return currentPhase.Count > 0
            ? currentPhase
            : sequenceSource.ToList();
    }

    private static List<CurriculumTemplate> GetPhaseTemplates(
        IReadOnlyList<CurriculumTemplate> templates,
        int phaseIndex)
    {
        if (templates.Count == 0)
        {
            return [];
        }

        var chunkSize = (int)Math.Ceiling(templates.Count / 4d);
        return templates
            .Skip(phaseIndex * chunkSize)
            .Take(chunkSize)
            .ToList();
    }

    private static List<CurriculumTemplate> GetUnitTemplates(
        IReadOnlyList<CurriculumTemplate> phaseTemplates,
        int currentUnitNumber)
    {
        if (phaseTemplates.Count == 0)
        {
            return [];
        }

        var chunkSize = (int)Math.Ceiling(phaseTemplates.Count / 3d);
        return phaseTemplates
            .Skip((Math.Clamp(currentUnitNumber, 1, 3) - 1) * chunkSize)
            .Take(chunkSize)
            .ToList();
    }

    private static bool IsTemplateOpenForSequence(
        CurriculumTemplate template,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap)
    {
        if (!progressMap.TryGetValue(template.SkillCode, out var progress))
        {
            return true;
        }

        if (progress.NeedsRemediation)
        {
            return true;
        }

        if (progress.NeedsReadinessCheck && !progress.ReadinessApproved)
        {
            return true;
        }

        return !progress.ReadyToAdvance && !progress.ReadinessApproved;
    }

    private static int GetSequenceBucket(
        CurriculumTemplate template,
        IReadOnlyList<CurriculumTemplate> currentWindow,
        IReadOnlyList<CurriculumTemplate> sequenceSource)
    {
        if (currentWindow.Any(x => x.Id == template.Id))
        {
            return 0;
        }

        if (sequenceSource.Any(x => x.Id == template.Id))
        {
            return 1;
        }

        return 2;
    }

    private static int GetSequenceDistance(
        CurriculumTemplate template,
        IReadOnlyList<CurriculumTemplate> currentWindow,
        int targetSequenceIndex)
    {
        if (currentWindow.Count == 0)
        {
            return 999;
        }

        var index = -1;
        for (var i = 0; i < currentWindow.Count; i++)
        {
            if (currentWindow[i].Id != template.Id)
            {
                continue;
            }

            index = i;
            break;
        }

        return index >= 0
            ? Math.Abs(index - targetSequenceIndex)
            : 500 + template.SortOrder;
    }

    private static int GetProgressSequencePenalty(
        CurriculumTemplate template,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap)
    {
        if (!progressMap.TryGetValue(template.SkillCode, out var progress))
        {
            return 0;
        }

        if (progress.NeedsRemediation)
        {
            return -4;
        }

        if (progress.NeedsReadinessCheck && !progress.ReadinessApproved)
        {
            return -2;
        }

        if (progress.ReadinessApproved)
        {
            return 8;
        }

        if (progress.ReadyToAdvance)
        {
            return 5;
        }

        return 0;
    }

    private static int GetStepAlignedIndex(int itemCount, int weekStepNumber)
    {
        if (itemCount <= 1)
        {
            return 0;
        }

        var normalizedStep = Math.Clamp(weekStepNumber, 1, 5) - 1;
        return (int)Math.Round((itemCount - 1) * (normalizedStep / 4d), MidpointRounding.AwayFromZero);
    }

    private static int GetWeekdayStepNumber(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        _ => 1
    };

    private static int GetCurrentPhaseIndex(int month) => month switch
    {
        <= 3 => 0,
        <= 6 => 1,
        <= 9 => 2,
        _ => 3
    };

    private static int GetPhaseMonthSlot(int month)
    {
        return Math.Clamp(((Math.Max(month, 1) - 1) % 3) + 1, 1, 3);
    }

    private static CurriculumTemplate? ResolveTemplateWithPrerequisite(
        IReadOnlyList<CurriculumTemplate> templates,
        IReadOnlyDictionary<string, ChildSkillProgress> progressMap,
        CurriculumTemplate template)
    {
        if (string.IsNullOrWhiteSpace(template.PrerequisiteSkillCode))
        {
            return template;
        }

        if (progressMap.TryGetValue(template.PrerequisiteSkillCode, out var prerequisiteProgress) &&
            (prerequisiteProgress.ReadinessApproved ||
             (!prerequisiteProgress.NeedsReadinessCheck && prerequisiteProgress.ReadyToAdvance) ||
             prerequisiteProgress.MasteryScore >= 85))
        {
            return template;
        }

        return templates.FirstOrDefault(x => string.Equals(x.SkillCode, template.PrerequisiteSkillCode, StringComparison.OrdinalIgnoreCase))
            ?? template;
    }

    private static Dictionary<string, string> BuildDueReviewMap(
        IReadOnlyList<CurriculumTemplate> templates,
        IReadOnlyList<ChildSkillProgress> progressEntries,
        DateTime targetDate)
    {
        var templateMap = templates.ToDictionary(x => x.SkillCode, x => x);
        return progressEntries
            .Where(x => x.NextReviewAt.HasValue && x.NextReviewAt.Value.Date <= targetDate.Date)
            .Where(x => templateMap.ContainsKey(x.SkillCode))
            .OrderBy(x => x.NextReviewAt)
            .Take(2)
            .ToDictionary(
                x => x.SkillCode,
                x =>
                {
                    var template = templateMap[x.SkillCode];
                    return $"Revisao espaçada: esta habilidade voltou hoje para nao esfriar. Intervalo planejado de {template.ReviewAfterDays} dia(s).";
                });
    }

    private static int GetPriority(CurriculumTemplate template, IReadOnlyDictionary<string, ChildSkillProgress> progressMap, int seed, string teachingMethodology, SupportProfile supportProfile, PortuguesePlanningGuidance? portugueseGuidance)
    {
        var methodologyWeight = GetMethodologyPreferenceWeight(template, teachingMethodology);
        var supportWeight = GetSupportPreferenceWeight(template.SupportScope, supportProfile);
        var portugueseWeight = GetPortuguesePlanningWeight(template, portugueseGuidance);
        if (!progressMap.TryGetValue(template.SkillCode, out var progress))
        {
            return supportWeight + methodologyWeight + portugueseWeight + template.SortOrder + (seed % 5);
        }

        if (progress.NeedsRemediation)
        {
            return -40 + supportWeight + methodologyWeight + portugueseWeight + template.SortOrder;
        }

        if (progress.NeedsReadinessCheck && !progress.ReadinessApproved)
        {
            return -25 + supportWeight + methodologyWeight + portugueseWeight + template.SortOrder;
        }

        if (progress.ReadyToAdvance)
        {
            return 40 + supportWeight + methodologyWeight + portugueseWeight + template.SortOrder;
        }

        var recencyPenalty = progress.LastPracticedAt.HasValue
            ? Math.Max(0, 12 - (DateTime.UtcNow.Date - progress.LastPracticedAt.Value.Date).Days)
            : 0;

        return supportWeight + methodologyWeight + portugueseWeight + progress.MasteryScore - (progress.TimesPracticed * 3) + recencyPenalty;
    }

    private static void AddIfAvailable(ICollection<CurriculumTemplate> chosen, CurriculumTemplate? template)
    {
        if (template is null || chosen.Any(x => x.Id == template.Id))
        {
            return;
        }

        chosen.Add(template);
    }

    private static List<int> BuildDurations(int totalMinutes, int blockCount, int age, string guidanceStyle, int preferredCoreBlockMinutes)
    {
        var warmUp = age <= 4 ? 8 : 10;
        var closing = age <= 4 ? 6 : 8;
        if (guidanceStyle == "focus_support")
        {
            warmUp = Math.Max(6, warmUp - 2);
            closing = Math.Max(6, closing - 1);
        }

        var remaining = Math.Max(12, totalMinutes - warmUp - closing);
        var coreBlocks = Math.Max(1, blockCount - 2);
        var coreDuration = Math.Max(guidanceStyle == "focus_support" ? 8 : 10, preferredCoreBlockMinutes > 0 ? preferredCoreBlockMinutes : remaining / coreBlocks);

        var durations = new List<int> { warmUp };
        for (var i = 0; i < coreBlocks; i++)
        {
            durations.Add(coreDuration);
        }
        durations.Add(closing);

        while (durations.Count > blockCount)
        {
            durations.RemoveAt(durations.Count - 2);
        }

        while (durations.Count < blockCount)
        {
            durations.Insert(durations.Count - 1, Math.Max(10, coreDuration - 2));
        }

        var diff = totalMinutes - durations.Sum();
        durations[^1] += diff;
        return durations;
    }

    private static string BuildTheme(int age, DateTime targetDate, PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (portugueseGuidance is not null)
        {
            return portugueseGuidance.DailyTheme;
        }

        string[] themes = age switch
        {
            3 => ["Meu corpo e meus sentidos", "Minha casa e minha rotina", "Cores, sons e movimentos", "Natureza bem perto"],
            4 => ["Palavras que brincam", "Quantidades do cotidiano", "Memoria, atencao e combinados", "Animais, plantas e descobertas"],
            5 => ["Letras que ganham som", "Numeros que contam historias", "Meu mundo e minhas perguntas", "Jogos de foco e autonomia"],
            6 => ["Leio e explico", "Matematica para a vida real", "Organizo e resolvo", "Observo, comparo e descubro"],
            7 => ["Leio, escrevo e argumento", "Problemas com estrategia", "Ciencia em casa", "Autonomia com pensamento forte"],
            8 => ["Projetos e pesquisas guiadas", "Leitura com sentido", "Matematica com estrategia", "Explico o que descobri"],
            9 => ["Textos, ideias e repertorio", "Problemas em varias etapas", "Investigacao e registro", "Rotina com mais autonomia"],
            _ => ["Compreendo, produzo e apresento", "Raciocinio academico", "Projetos do mundo real", "Autonomia com responsabilidade"]
        };

        return themes[targetDate.DayOfYear % themes.Length];
    }

    private static string BuildParentSummary(
        IEnumerable<CurriculumTemplate> templates,
        int age,
        ChildProfile child,
        string theme,
        RecoveryTemplateHint? recoveryDay,
        PlanDirectiveContext directiveContext,
        AdaptiveRoutineSnapshot adaptiveSnapshot,
        PortuguesePlanningGuidance? portugueseGuidance)
    {
        var list = templates.ToList();
        var labels = templates.Select(x => x.Domain switch
        {
            LearningDomain.Language => "linguagem",
            LearningDomain.Math => "matematica",
            LearningDomain.World => "mundo real",
            _ => "funcao executiva"
        }).Distinct().ToList();

        var domains = string.Join(", ", labels);
        var reinforcement = list.FirstOrDefault();
        var childNote = string.IsNullOrWhiteSpace(child.Notes)
            ? string.Empty
            : $" Considere os interesses e desafios desta crianca: {child.Notes.Trim()}.";
        var learningProfileText = child.LearningProfile switch
        {
            "hands_on" => " Ela aprende melhor fazendo, manipulando materiais e vendo resultado concreto.",
            "story_based" => " Ela responde melhor quando a explicacao passa por historias, conversa e linguagem oral.",
            "visual" => " Ela ganha mais clareza com organizacao visual, comparacao e passos bem desenhados.",
            "movement" => " Ela tende a engajar melhor com movimento, alternancia de ritmo e tarefas mais ativas.",
            _ => " Ela costuma funcionar bem com uma combinacao equilibrada de linguagem, concretude e repeticao."
        };
        var guidanceText = child.GuidanceStyle switch
        {
            "confidence" => " Comece pelas vitorias mais simples e elogie rapido para construir confianca.",
            "autonomy" => " Entregue mais espaco para iniciativa, autoexplicacao e revisao da propria resposta.",
            "focus_support" => " Trabalhe com blocos curtos, pausas pequenas e um comando de cada vez.",
            _ => " Conduza a sessao passo a passo, com checagens frequentes e linguagem simples."
        };
        var methodologyText = BuildMethodologySummary(child.TeachingMethodology);

        var reinforcementText = reinforcement is null
            ? string.Empty
            : $" Hoje vale observar especialmente a habilidade {reinforcement.SkillName}, porque ela pode precisar de mais consolidacao.";
        var recoveryText = recoveryDay is null
            ? string.Empty
            : $" Este plano faz parte de uma retomada de 7 dias. Priorize {recoveryDay.FocusSkill}, mantenha a sessao leve e busque uma vitoria concreta antes de avancar.";
        var focusText = directiveContext.FocusTrack.HasValue
            ? $" O responsavel pediu um plano manual focado em {GetFunctionalTrackDisplayLabel(directiveContext.FocusTrack.Value).ToLowerInvariant()}, entao hoje a trilha foi intencionalmente concentrada nesse dominio."
            : string.Empty;
        var pinnedText = directiveContext.PinnedTemplates.Count == 0
            ? string.Empty
            : $" Atividades puxadas manualmente para hoje: {string.Join(", ", directiveContext.PinnedTemplates.Select(x => x.Title))}.";
        var adaptiveText = $" Rotina adaptativa sugerida: blocos de {adaptiveSnapshot.WorkBlockMinutes} min, pausa de {adaptiveSnapshot.BreakMinutes} min, {AdaptiveRoutineService.GetSupportIntensityLabel(adaptiveSnapshot.Intensity).ToLowerInvariant()}, com foco em {adaptiveSnapshot.VisualSupportRecommendation.ToLowerInvariant()} {adaptiveSnapshot.TransitionRecommendation.ToLowerInvariant()}";
        var portugueseText = portugueseGuidance is null
            ? string.Empty
            : $" Planejamento de Portugues guiando o dia: {portugueseGuidance.Summary} Busque evidencias alinhadas a esse foco nas atividades de linguagem.";

        return $"Tema do dia: {theme}. Comece com acolhida curta, conduza uma rotina forte em {domains} e termine com registro simples do que deu certo e do que precisa de reforco. Para a idade de {age} anos, priorize explicacoes breves, repeticao intencional, materiais concretos e linguagem clara.{learningProfileText}{guidanceText}{methodologyText}{reinforcementText}{recoveryText}{focusText}{pinnedText}{adaptiveText}{portugueseText}{childNote}";
    }

    private static string BuildAdaptiveCue(AdaptiveRoutineSnapshot adaptiveSnapshot, int blockIndex)
    {
        var opening = blockIndex == 0
            ? "Abra com previsibilidade visual e anuncie o encerramento antes de mudar."
            : "Mantenha a linguagem curta, uma instrucao de cada vez e uma saida simples se houver travamento.";
        return $"{opening} Meta operacional: bloco de {adaptiveSnapshot.WorkBlockMinutes} min, pausa de {adaptiveSnapshot.BreakMinutes} min. {adaptiveSnapshot.PlanBRecommendation}";
    }

    private static string BuildChildNarrative(
        IEnumerable<CurriculumTemplate> templates,
        int age,
        string theme,
        string learningProfile,
        FunctionalSupportTrack? focusedTrack,
        PortuguesePlanningGuidance? portugueseGuidance)
    {
        var titles = string.Join(" + ", templates.Select(x => x.Title));
        var profilePhrase = learningProfile switch
        {
            "hands_on" => "com materiais para mexer e descobrir",
            "story_based" => "com conversa, narrativa e sentido",
            "visual" => "com pistas visuais e organizacao clara",
            "movement" => "com energia, movimento e acao",
            _ => "com equilibrio entre fala, pratica e desafio"
        };
        var focusText = focusedTrack.HasValue
            ? $" Hoje vamos trabalhar bastante a trilha de {GetFunctionalTrackDisplayLabel(focusedTrack.Value).ToLowerInvariant()}."
            : string.Empty;
        var portugueseText = portugueseGuidance is null
            ? string.Empty
            : $" Na parte de Portugues, o foco do periodo e {portugueseGuidance.MainFocus.ToLowerInvariant()}.";
        return $"Hoje o nosso tema e {theme}. Sua jornada de {age} anos mistura {titles}, {profilePhrase}.{focusText}{portugueseText} Cada missao foi escolhida para fortalecer inteligencia, linguagem, raciocinio, foco e curiosidade.";
    }

    private static int GetPortuguesePlanningWeight(CurriculumTemplate template, PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (portugueseGuidance is null || template.Domain != LearningDomain.Language)
        {
            return 0;
        }

        var normalizedTitle = NormalizeForSearch(template.Title);
        var haystack = NormalizeForSearch($"{template.Title} {template.SkillName} {template.Goal}");
        var preferredTitleMatchCount = portugueseGuidance.PreferredTemplateTitles.Count(x =>
        {
            var normalizedPreference = NormalizeForSearch(x);
            return !string.IsNullOrWhiteSpace(normalizedPreference) &&
                   (normalizedTitle.Contains(normalizedPreference, StringComparison.Ordinal) ||
                    normalizedPreference.Contains(normalizedTitle, StringComparison.Ordinal));
        });

        if (preferredTitleMatchCount > 0)
        {
            return -32 - (preferredTitleMatchCount * 6);
        }

        var matchCount = portugueseGuidance.PriorityKeywords.Count(x =>
        {
            var normalizedKeyword = NormalizeForSearch(x);
            return !string.IsNullOrWhiteSpace(normalizedKeyword) &&
                   haystack.Contains(normalizedKeyword, StringComparison.Ordinal);
        });

        if (matchCount > 0)
        {
            return -18 - (matchCount * 4);
        }

        return 8;
    }

    private static string BuildPortugueseGuideCue(CurriculumTemplate template, PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (portugueseGuidance is null || template.Domain != LearningDomain.Language)
        {
            return string.Empty;
        }

        return $"Alinhamento BNCC de Portugues: {portugueseGuidance.TermLabel.ToLowerInvariant()} de {portugueseGuidance.SchoolPlacement.ToLowerInvariant()}, com foco em {portugueseGuidance.MainFocus.ToLowerInvariant()}.";
    }

    private static string BuildPortugueseEvidenceCue(CurriculumTemplate template, PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (portugueseGuidance is null || template.Domain != LearningDomain.Language)
        {
            return string.Empty;
        }

        return $"Relacione a evidencia ao foco bimestral de Portugues: {portugueseGuidance.MainFocus.ToLowerInvariant()}.";
    }

    private static string BuildPortugueseGoalCue(CurriculumTemplate template, PortuguesePlanningGuidance? portugueseGuidance)
    {
        if (portugueseGuidance is null || template.Domain != LearningDomain.Language)
        {
            return string.Empty;
        }

        return $"Foco BNCC atual: {portugueseGuidance.MainFocus}.";
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(chars)
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }

    private static int GetMethodologyPreferenceWeight(CurriculumTemplate template, string teachingMethodology) => teachingMethodology switch
    {
        "montessori" when template.Domain is LearningDomain.Math or LearningDomain.ExecutiveFunction => -8,
        "montessori" when template.GoalTrack is "math_foundations" or "autonomy" => -6,
        "charlotte_mason" when template.Domain is LearningDomain.Language or LearningDomain.World => -8,
        "charlotte_mason" when template.GoalTrack is "literacy" or "science_discovery" => -5,
        "classical" when template.Domain is LearningDomain.Language or LearningDomain.Math => -7,
        "classical" when template.GoalTrack is "literacy" or "math_foundations" => -5,
        "singapore_math" when template.Domain == LearningDomain.Math => -12,
        "singapore_math" when template.GoalTrack == "math_foundations" => -8,
        _ => 0
    };

    private static string BuildMethodologySummary(string teachingMethodology) => teachingMethodology switch
    {
        "montessori" => " Conduza com escolha limitada, material concreto, demonstracao silenciosa e autonomia progressiva.",
        "charlotte_mason" => " Use linguagem viva, narrativa curta, observacao atenta e reconto sincero no fechamento.",
        "classical" => " Trabalhe com memoria curta, linguagem precisa, repeticao deliberada e verbalizacao clara do raciocinio.",
        "singapore_math" => " Na matematica, siga a ordem concreto, pictorico e abstrato antes de acelerar para simbolos.",
        _ => " Combine explicacao clara, material simples e pratica intencional sem transformar a rotina em aula longa."
    };

    private static string BuildMethodologyGuide(string teachingMethodology, LearningDomain domain, string goalTrack) => teachingMethodology switch
    {
        "montessori" when domain == LearningDomain.Math => "Comece com manipulacao concreta e deixe a crianca repetir o gesto antes de nomear a regra.",
        "montessori" when domain == LearningDomain.Language => "Apresente pouco de cada vez, com convite curto e autonomia para repetir.",
        "charlotte_mason" when domain == LearningDomain.Language => "Prefira leitura viva, conversa breve e reconto com palavras proprias.",
        "charlotte_mason" when domain == LearningDomain.World => "Traga observacao real, natureza, imagem forte e narracao curta.",
        "classical" when domain == LearningDomain.Language => "Faça modelagem oral, repeticao curta e resposta em frase completa.",
        "classical" when domain == LearningDomain.Math => "Peça que a crianca verbalize o passo, a regra e a justificativa da resposta.",
        "singapore_math" when goalTrack == "math_foundations" || domain == LearningDomain.Math => "Respeite a sequencia concreto, desenho e simbolo antes de formalizar o algoritmo.",
        _ => string.Empty
    };

    private static string BuildMethodologyMaterials(string teachingMethodology, LearningDomain domain) => teachingMethodology switch
    {
        "montessori" when domain is LearningDomain.Math or LearningDomain.Language => "Separe bandeja simples, objetos manipulaveis e material em quantidade enxuta.",
        "charlotte_mason" when domain is LearningDomain.Language or LearningDomain.World => "Inclua livro vivo, imagem forte ou objeto real para conversa e narracao.",
        "classical" => "Tenha quadro de apoio, cartoes de memoria curta ou caderno de resposta estruturada.",
        "singapore_math" when domain == LearningDomain.Math => "Inclua objetos para agrupar, barras desenhadas ou esboço visual do problema.",
        _ => string.Empty
    };

    private static string BuildMethodologyEvaluation(string teachingMethodology, LearningDomain domain) => teachingMethodology switch
    {
        "montessori" => "Observe independencia, controle do erro e repeticao voluntaria.",
        "charlotte_mason" => "Avalie pelo reconto, pela observacao atenta e pela qualidade da narracao.",
        "classical" => "Avalie se a crianca explica a resposta com vocabulario preciso e sequencia clara.",
        "singapore_math" when domain == LearningDomain.Math => "Avalie se a crianca representou o raciocinio antes de responder so pelo simbolo.",
        _ => string.Empty
    };

    private static string FitForStorage(string? value, int maxLength)
    {
        if (maxLength <= 0)
        {
            return string.Empty;
        }

        var normalized = NormalizeStorageWhitespace(value);
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        var reserveForEllipsis = maxLength > 3 ? 3 : 0;
        var sliceLength = Math.Max(1, maxLength - reserveForEllipsis);
        var candidate = normalized[..sliceLength].TrimEnd();
        var lastSpace = candidate.LastIndexOf(' ');

        if (lastSpace >= Math.Max(8, sliceLength / 2))
        {
            candidate = candidate[..lastSpace].TrimEnd();
        }

        return reserveForEllipsis == 0
            ? candidate
            : $"{candidate}...";
    }

    private static string NormalizeStorageWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(" ", value
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private async Task<PlanDirectiveContext> GetDirectiveContextAsync(
        Guid childId,
        DateTime targetDate,
        IReadOnlyList<CurriculumTemplate> templates)
    {
        var directives = await db.ChildPlanDirectives
            .Where(x => x.ChildId == childId && x.PlannedDate == targetDate && !x.AppliedAt.HasValue)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var pinnedTemplates = directives
            .Where(x => x.DirectiveType == PlanDirectiveType.PinnedActivity && x.TemplateId.HasValue)
            .Select(x => templates.FirstOrDefault(t => t.Id == x.TemplateId!.Value))
            .Where(x => x is not null)
            .Cast<CurriculumTemplate>()
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

        var focusTrack = directives
            .Where(x => x.DirectiveType == PlanDirectiveType.TrackFocus && x.FunctionalTrack.HasValue)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.FunctionalTrack)
            .FirstOrDefault();

        return new PlanDirectiveContext
        {
            Directives = directives,
            PinnedTemplates = pinnedTemplates,
            FocusTrack = focusTrack
        };
    }

    private static bool RequiresDirectiveRefresh(
        DailyPlan existing,
        PlanDirectiveContext directiveContext,
        IReadOnlyList<CurriculumTemplate> templates)
    {
        if (directiveContext.Directives.Count == 0)
        {
            return false;
        }

        var plannedTemplateIds = existing.Blocks
            .Where(x => x.SourceTemplateId.HasValue)
            .Select(x => x.SourceTemplateId!.Value)
            .ToHashSet();

        if (directiveContext.PinnedTemplates.Any(x => !plannedTemplateIds.Contains(x.Id)))
        {
            return true;
        }

        if (!directiveContext.FocusTrack.HasValue)
        {
            return false;
        }

        var availableFocusTemplates = templates.Count(x => x.FunctionalTrack == directiveContext.FocusTrack.Value);
        if (availableFocusTemplates == 0)
        {
            return false;
        }

        var requiredFocusBlocks = Math.Min(3, availableFocusTemplates);
        var actualFocusBlocks = existing.Blocks.Count(x => x.FunctionalTrack == directiveContext.FocusTrack.Value);
        return actualFocusBlocks < requiredFocusBlocks;
    }

    private async Task MarkDirectivesAsAppliedAsync(IReadOnlyList<ChildPlanDirective> directives)
    {
        if (directives.Count == 0)
        {
            return;
        }

        var appliedAt = DateTime.UtcNow;
        foreach (var directive in directives)
        {
            directive.AppliedAt = appliedAt;
        }

        await db.SaveChangesAsync();
    }

    private static string BuildFocusedTrackTheme(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => "Foco especial em comunicacao",
        FunctionalSupportTrack.Regulation => "Foco especial em regulacao",
        FunctionalSupportTrack.Sensory => "Foco especial em sensorial",
        FunctionalSupportTrack.DailyLiving => "Foco especial em vida diaria",
        FunctionalSupportTrack.AcademicAdapted => "Foco especial em academico adaptado",
        _ => "Foco especial na trilha do dia"
    };

    private static string GetFunctionalTrackDisplayLabel(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => "Comunicacao",
        FunctionalSupportTrack.Regulation => "Regulacao",
        FunctionalSupportTrack.Sensory => "Sensorial",
        FunctionalSupportTrack.DailyLiving => "Vida diaria",
        FunctionalSupportTrack.AcademicAdapted => "Academico adaptado",
        _ => "Base academica"
    };

    private async Task<bool> HasActiveRecoveryPlanAsync(Guid childId, DateTime targetDate)
    {
        return await db.ChildRecoveryPlans.AnyAsync(x =>
            x.ChildId == childId &&
            x.Status == "active" &&
            x.StartDate.Date <= targetDate.Date &&
            x.EndDate.Date >= targetDate.Date);
    }

    private async Task<RecoveryTemplateHint?> GetRecoveryDayAsync(Guid childId, DateTime targetDate)
    {
        var plan = await db.ChildRecoveryPlans
            .Include(x => x.Days)
            .Where(x => x.ChildId == childId && x.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.StartDate.Date <= targetDate.Date && x.EndDate.Date >= targetDate.Date);

        var day = plan?.Days
            .OrderBy(x => x.DayNumber)
            .FirstOrDefault(x => x.SuggestedDate.Date == targetDate.Date && !x.CompletedAt.HasValue)
            ?? plan?.Days.OrderBy(x => x.DayNumber).FirstOrDefault(x => !x.CompletedAt.HasValue);

        if (day is null)
        {
            return null;
        }

        var cycle = await db.ChildMonthlyGoalCycles
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == plan!.GoalCycleId);
        var goalItem = cycle?.Items.FirstOrDefault(x => string.Equals(x.SkillName, day.FocusSkill, StringComparison.OrdinalIgnoreCase));

        return new RecoveryTemplateHint
        {
            FocusSkill = day.FocusSkill,
            SkillCode = goalItem?.SkillCode ?? string.Empty,
            GoalText = day.GoalText
        };
    }

    private sealed class RecoveryTemplateHint
    {
        public required string FocusSkill { get; init; }
        public required string SkillCode { get; init; }
        public required string GoalText { get; init; }
    }

    private sealed class PlanDirectiveContext
    {
        public required List<ChildPlanDirective> Directives { get; init; }
        public required List<CurriculumTemplate> PinnedTemplates { get; init; }
        public FunctionalSupportTrack? FocusTrack { get; init; }
    }

    private static bool IsTemplateAllowedForProfile(CurriculumSupportScope supportScope, SupportProfile supportProfile)
    {
        return supportProfile switch
        {
            SupportProfile.General => supportScope == CurriculumSupportScope.General,
            SupportProfile.TeaLevel1 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel1,
            SupportProfile.TeaLevel2 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel2,
            SupportProfile.TeaLevel3 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel3,
            _ => supportScope == CurriculumSupportScope.General
        };
    }

    private static int GetSupportPreferenceWeight(CurriculumSupportScope supportScope, SupportProfile supportProfile)
    {
        return (supportProfile, supportScope) switch
        {
            (SupportProfile.General, CurriculumSupportScope.General) => -20,
            (SupportProfile.TeaLevel1, CurriculumSupportScope.TeaLevel1) => -40,
            (SupportProfile.TeaLevel2, CurriculumSupportScope.TeaLevel2) => -40,
            (SupportProfile.TeaLevel3, CurriculumSupportScope.TeaLevel3) => -40,
            (SupportProfile.TeaLevel1, CurriculumSupportScope.TeaCommon) => -25,
            (SupportProfile.TeaLevel2, CurriculumSupportScope.TeaCommon) => -25,
            (SupportProfile.TeaLevel3, CurriculumSupportScope.TeaCommon) => -25,
            (_, CurriculumSupportScope.General) => -10,
            _ => 0
        };
    }
}
