using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class GuidedLessonExperienceService
{
    private sealed record UnitStepProgressInfo(
        int StepNumber,
        int StepCount,
        string StepLabel,
        string StepSummary,
        string NextStepHint);

    public GuidedDailyLessonViewModel BuildGuidedLesson(
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel> suggestions,
        IReadOnlySet<Guid> completedBlockIds,
        IReadOnlyCollection<SystemCurriculumTrackViewModel> systemTracks,
        IReadOnlyDictionary<Guid, FamilyLibraryRecommendationViewModel> printableMap)
    {
        var orderedBlocks = blocks
            .OrderBy(x => x.SortOrder)
            .ToList();

        if (orderedBlocks.Count == 0)
        {
            return new GuidedDailyLessonViewModel
            {
                Headline = "A aula de hoje vai aparecer aqui",
                Summary = "Assim que o plano for gerado, o sistema separa as matérias, os passos e o botão de concluir."
            };
        }

        var lessons = BuildLessonCards(
            orderedBlocks,
            suggestions,
            completedBlockIds,
            systemTracks,
            printableMap,
            isPreviewMode: false);
        var activeLesson = lessons.FirstOrDefault(x => !x.IsCompleted);

        var completedCount = lessons.Count(x => x.IsCompleted);
        var plannedCount = lessons.Count;
        var progressPercent = plannedCount == 0
            ? 0
            : (int)Math.Round((double)completedCount * 100 / plannedCount);

        return new GuidedDailyLessonViewModel
        {
            Headline = BuildHeadline(child.FamilyGoalTrack),
            Summary = activeLesson is null
                ? "O dia de hoje já foi fechado. Se precisar, guarde a prova depois em Evidências."
                : $"{activeLesson.SubjectLabel}: {activeLesson.Title}. Abra a atividade abaixo, faça só o que está nesta tela e conclua quando terminar.",
            CompletedLessonsCount = completedCount,
            PlannedLessonsCount = plannedCount,
            ProgressPercent = progressPercent,
            CompletionBanner = completedCount == plannedCount
                ? "Dia concluido. Se quiser, guarde foto, video ou documento depois em Evidencias."
                : "Faça uma lição por vez. Quando esta terminar, a próxima sobe automaticamente.",
            Lessons = lessons
        };
    }

    public TomorrowLessonPreviewViewModel BuildTomorrowPreview(
        ChildProfile child,
        DailyPlan? tomorrowPlan,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel>? tomorrowSuggestions,
        IReadOnlyCollection<SystemCurriculumTrackViewModel> systemTracks,
        IReadOnlyDictionary<Guid, FamilyLibraryRecommendationViewModel> printableMap)
    {
        if (tomorrowPlan is null)
        {
            return new TomorrowLessonPreviewViewModel
            {
                HasPlan = false
            };
        }

        var topLessons = tomorrowPlan.Blocks
            .OrderBy(x => x.SortOrder)
            .Select(block =>
            {
                if (tomorrowSuggestions is not null &&
                    tomorrowSuggestions.TryGetValue(block.Id, out var suggestion) &&
                    !string.IsNullOrWhiteSpace(suggestion.Title))
                {
                    return suggestion.Title;
                }

                return block.Title;
            })
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(3)
            .ToList();

        var lessons = BuildLessonCards(
            tomorrowPlan.Blocks.OrderBy(x => x.SortOrder).ToList(),
            tomorrowSuggestions ?? new Dictionary<Guid, CuratedTaskSuggestionViewModel>(),
            new HashSet<Guid>(),
            systemTracks,
            printableMap,
            isPreviewMode: true);

        return new TomorrowLessonPreviewViewModel
        {
            HasPlan = true,
            Theme = tomorrowPlan.Theme,
            TotalMinutes = tomorrowPlan.Blocks.Sum(x => x.DurationMinutes),
            Summary = "Amanhã já fica separado. Veja só a primeira lição e deixe o restante para a hora certa.",
            TopLessons = topLessons,
            Lessons = lessons
        };
    }

    public WeeklyStudySnapshotViewModel BuildWeeklySnapshot(
        WeeklyRoadmapViewModel roadmap,
        int completedLessonsCount,
        int plannedLessonsCount)
    {
        var progressPercent = plannedLessonsCount == 0
            ? 0
            : (int)Math.Round((double)completedLessonsCount * 100 / plannedLessonsCount);

        return new WeeklyStudySnapshotViewModel
        {
            Headline = "Semana pronta, mas sem excesso de tela",
            CompletedLessonsCount = completedLessonsCount,
            PlannedLessonsCount = plannedLessonsCount,
            ProgressPercent = progressPercent,
            Days = roadmap.Days
                .Take(3)
                .Select(day => new WeeklyStudyDayCardViewModel
                {
                    DayLabel = day.RelativeLabel,
                    DateLabel = day.DateLabel,
                    Theme = day.Theme,
                    MainTask = day.TopTasks.FirstOrDefault() ?? "Lição preparada automaticamente",
                    MaterialHint = day.Materials.FirstOrDefault() ?? "Material simples da casa",
                    OutcomeHint = day.EvidenceItems.FirstOrDefault() ?? "Resultado esperado já definido",
                    IsCompleted = string.Equals(day.RelativeLabel, "Hoje", StringComparison.OrdinalIgnoreCase)
                                  && progressPercent >= 100,
                    IsToday = string.Equals(day.RelativeLabel, "Hoje", StringComparison.OrdinalIgnoreCase)
                })
                .ToList()
        };
    }

    private static string BuildHeadline(string goalTrack) => "Aula do dia";

    private static string FormatDomain(LearningDomain domain) => CurriculumStructure.FormatDomainLabel(domain);

    private static string GetDomainChip(LearningDomain domain) => CurriculumStructure.GetDomainChipClass(domain);

    private static List<string> BuildFallbackSteps(
        string parentGuide,
        string childPrompt,
        string materials,
        SystemCurriculumUnitViewModel? currentUnit)
    {
        if (currentUnit is not null)
        {
            var materialHint = !string.IsNullOrWhiteSpace(materials)
                ? materials
                : currentUnit.Materials.Count > 0
                    ? string.Join(", ", currentUnit.Materials)
                    : string.Empty;

            return new List<string>
            {
                Shorten(currentUnit.ParentGuide, 120),
                Shorten(currentUnit.TaskPrompt, 120),
                $"Separe: {Shorten(materialHint, 80)}",
                Shorten(currentUnit.CompletionSignal, 120)
            }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        return new List<string>
        {
            Shorten(parentGuide, 120),
            Shorten(childPrompt, 120),
            $"Separe: {Shorten(materials, 80)}"
        }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static string BuildMaterialsSummary(string materials, SystemCurriculumUnitViewModel? currentUnit)
    {
        if (!string.IsNullOrWhiteSpace(materials))
        {
            return materials;
        }

        return currentUnit is not null && currentUnit.Materials.Count > 0
            ? string.Join(", ", currentUnit.Materials)
            : string.Empty;
    }

    private static string BuildLessonReason(
        LearningDomain domain,
        SystemCurriculumTrackViewModel? currentTrack,
        SystemCurriculumUnitViewModel? currentUnit,
        CuratedTaskSuggestionViewModel? suggestion)
    {
        if (suggestion?.LessonPacket is not null)
        {
            var phaseLabel = currentTrack?.CurrentPhaseLabel;
            return string.IsNullOrWhiteSpace(phaseLabel)
                ? $"Hoje a trilha de {FormatDomain(domain).ToLowerInvariant()} trabalha {suggestion.LessonPacket.CurriculumPlacement.ToLowerInvariant()}."
                : $"Hoje a trilha de {FormatDomain(domain).ToLowerInvariant()} está na {phaseLabel.ToLowerInvariant()} e a lição prática {suggestion.LessonPacket.UnitTitle.ToLowerInvariant()}.";
        }

        if (!string.IsNullOrWhiteSpace(suggestion?.FitReason))
        {
            return suggestion.FitReason;
        }

        if (currentUnit is not null)
        {
            return $"Hoje {FormatDomain(domain).ToLowerInvariant()} esta em {currentUnit.UnitLabel.ToLowerInvariant()} do ano, entao a licao pratica {currentUnit.Title.ToLowerInvariant()}.";
        }

        return "Essa lição foi escolhida porque combina com a idade da criança e com a matéria mais importante de hoje.";
    }

    private static string BuildCurriculumOriginSummary(
        LearningDomain domain,
        SystemCurriculumTrackViewModel? currentTrack,
        SystemCurriculumUnitViewModel? currentUnit,
        CuratedTaskSuggestionViewModel? suggestion)
    {
        if (suggestion?.LessonPacket is not null)
        {
            var phaseText = string.IsNullOrWhiteSpace(currentTrack?.CurrentPhaseLabel)
                ? "currículo anual"
                : currentTrack.CurrentPhaseLabel.ToLowerInvariant();
            return $"Dentro do {phaseText}, esta lição trabalha {suggestion.LessonPacket.CurriculumPlacement.ToLowerInvariant()} e leva a criança a praticar {suggestion.LessonPacket.UnitTitle.ToLowerInvariant()}.";
        }

        if (currentUnit is null)
        {
            return string.Empty;
        }

        return $"{FormatDomain(domain)} esta trabalhando {currentUnit.UnitLabel.ToLowerInvariant()} do ano. A proposta de hoje faz a crianca praticar {currentUnit.Title.ToLowerInvariant()} sem sair da mesma tela.";
    }

    private static string BuildPrintableReason(
        LearningDomain domain,
        FamilyLibraryRecommendationViewModel? printable,
        SystemCurriculumTrackViewModel? currentTrack,
        SystemCurriculumUnitViewModel? currentUnit,
        CuratedTaskSuggestionViewModel? suggestion)
    {
        if (printable is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(printable.FitReason))
        {
            return printable.FitReason;
        }

        if (suggestion?.LessonPacket is not null)
        {
            return $"Essa folha entra porque ajuda a praticar {suggestion.LessonPacket.UnitTitle.ToLowerInvariant()} com apoio em papel, sem improvisar material.";
        }

        if (currentUnit is null)
        {
            return $"Esse trabalho de papel entrou porque ajuda a praticar {FormatDomain(domain).ToLowerInvariant()} na idade da criança.";
        }

        return $"Esse trabalho de papel entrou porque conversa com {currentUnit.Title.ToLowerInvariant()} e com a idade da criança.";
    }

    private static string Shorten(string? value, int length)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compact = value.Replace('\n', ' ').Trim();
        return compact.Length <= length
            ? compact
            : $"{compact[..Math.Max(0, length - 3)].TrimEnd()}...";
    }

    private static List<GuidedLessonCardViewModel> BuildLessonCards(
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel> suggestions,
        IReadOnlySet<Guid> completedBlockIds,
        IReadOnlyCollection<SystemCurriculumTrackViewModel> systemTracks,
        IReadOnlyDictionary<Guid, FamilyLibraryRecommendationViewModel> printableMap,
        bool isPreviewMode)
    {
        return blocks
            .Select(block =>
            {
                var suggestion = suggestions.TryGetValue(block.Id, out var task) ? task : null;
                var subjectLabel = FormatDomain(block.Domain);
                var currentTrack = systemTracks.FirstOrDefault(track =>
                    string.Equals(track.DomainLabel, subjectLabel, StringComparison.OrdinalIgnoreCase));
                var currentUnit = currentTrack?.CurrentUnit;
                var printable = printableMap.TryGetValue(block.Id, out var printableMatch) ? printableMatch : null;
                var title = !string.IsNullOrWhiteSpace(suggestion?.CurriculumLessonTitle)
                    ? suggestion!.CurriculumLessonTitle
                    : suggestion?.LessonPacket?.UnitTitle
                      ?? suggestion?.Title
                      ?? currentUnit?.TaskTitle
                      ?? block.Title;
                var steps = suggestion?.Steps.Count > 0 == true
                    ? suggestion.Steps.Take(4).ToList()
                    : BuildFallbackSteps(block.ParentGuide, block.ChildPrompt, block.Materials, currentUnit);
                var isCompleted = !isPreviewMode && completedBlockIds.Contains(block.Id);
                var unitStep = BuildUnitStepProgress(block, currentUnit, title);

                return new GuidedLessonCardViewModel
                {
                    DailyPlanBlockId = block.Id,
                    SubjectLabel = subjectLabel,
                    SubjectChipClass = GetDomainChip(block.Domain),
                    Title = title,
                    Goal = suggestion?.LessonPacket?.PracticeTask ?? currentUnit?.TaskPrompt ?? suggestion?.Goal ?? block.Goal,
                    ParentSummary = Shorten(
                        suggestion?.LessonPacket?.OpeningForAdult
                        ?? currentUnit?.ParentGuide
                        ?? suggestion?.ParentGuide
                        ?? block.ParentGuide,
                        240),
                    ChildPrompt = suggestion?.LessonPacket?.AnchorQuestion ?? currentUnit?.TaskTitle ?? suggestion?.ChildPrompt ?? block.ChildPrompt,
                    MaterialsSummary = BuildMaterialsSummary(suggestion?.MaterialsSummary ?? block.Materials, currentUnit),
                    Outcome = suggestion?.LessonPacket?.CompletionDefinition ?? currentUnit?.CompletionSignal ?? suggestion?.ExpectedOutcome ?? block.EvidencePrompt,
                    WhyThisLesson = BuildLessonReason(block.Domain, currentTrack, currentUnit, suggestion),
                    SuggestedMinutes = suggestion?.SuggestedMinutes > 0 == true ? suggestion.SuggestedMinutes : block.DurationMinutes,
                    IsCompleted = isCompleted,
                    CompletionLabel = isCompleted
                        ? "Concluída"
                        : isPreviewMode
                            ? "Amanhã"
                            : "Pendente",
                    UnitStepNumber = unitStep.StepNumber,
                    UnitStepCount = unitStep.StepCount,
                    UnitStepLabel = unitStep.StepLabel,
                    UnitStepSummary = unitStep.StepSummary,
                    NextUnitStepHint = unitStep.NextStepHint,
                    Steps = steps,
                    CurrentSystemUnit = currentUnit,
                    RecommendedPrintable = printable,
                    CurriculumOriginSummary = BuildCurriculumOriginSummary(block.Domain, currentTrack, currentUnit, suggestion),
                    PrintableReason = BuildPrintableReason(block.Domain, printable, currentTrack, currentUnit, suggestion),
                    LessonPacket = suggestion?.LessonPacket,
                    PrimaryResource = suggestion?.PrimaryResource?.FamilyFacing == true ? suggestion.PrimaryResource : null,
                    SupportLink = suggestion?.SupportLink
                };
            })
            .ToList();
    }

    private static UnitStepProgressInfo BuildUnitStepProgress(
        DailyPlanBlock block,
        SystemCurriculumUnitViewModel? currentUnit,
        string lessonTitle)
    {
        if (currentUnit is null)
        {
            return new UnitStepProgressInfo(0, 0, string.Empty, string.Empty, string.Empty);
        }

        var orderedSteps = BuildOrderedUnitSteps(currentUnit);
        if (orderedSteps.Count == 0)
        {
            return new UnitStepProgressInfo(
                1,
                1,
                "Passo 1 da unidade",
                $"Hoje vocês estão no primeiro passo de {currentUnit.UnitLabel}. Basta concluir esta entrega antes de subir a próxima.",
                "Depois deste passo, o sistema escolhe a próxima entrega da mesma unidade.");
        }

        var stepIndex = FindUnitStepIndex(orderedSteps, block, lessonTitle);
        var stepNumber = stepIndex + 1;
        var stepCount = orderedSteps.Count;
        var currentStepTitle = orderedSteps[stepIndex];
        var nextStepHint = stepNumber < stepCount
            ? $"Depois deste passo, a próxima entrega da unidade será: {orderedSteps[stepIndex + 1]}."
            : $"Este passo fecha {currentUnit.UnitLabel}. Quando ele estiver firme, o sistema sobe a próxima unidade.";

        return new UnitStepProgressInfo(
            stepNumber,
            stepCount,
            $"Passo {stepNumber} da unidade",
            $"Hoje vocês estão em {currentStepTitle} dentro de {currentUnit.UnitLabel}. Façam só esta entrega agora, sem adiantar o restante.",
            nextStepHint);
    }

    private static List<string> BuildOrderedUnitSteps(SystemCurriculumUnitViewModel currentUnit)
    {
        if (currentUnit.LessonTitles.Count > 0)
        {
            return currentUnit.LessonTitles
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (currentUnit.SkillCodes.Count > 0)
        {
            return currentUnit.SkillCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return [currentUnit.Title];
    }

    private static int FindUnitStepIndex(
        IReadOnlyList<string> orderedSteps,
        DailyPlanBlock block,
        string lessonTitle)
    {
        var candidates = new List<string?>
        {
            lessonTitle,
            block.Title,
            block.SkillName,
            block.SkillCode
        };

        foreach (var candidate in candidates)
        {
            var index = FindNormalizedIndex(orderedSteps, candidate);
            if (index >= 0)
            {
                return index;
            }
        }

        var codeIndex = FindStepIndexByCode(orderedSteps, block.SkillCode);
        return codeIndex >= 0 ? codeIndex : 0;
    }

    private static int FindNormalizedIndex(IReadOnlyList<string> orderedSteps, string? candidate)
    {
        var normalizedCandidate = NormalizeStepKey(candidate);
        if (string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return -1;
        }

        for (var index = 0; index < orderedSteps.Count; index++)
        {
            if (NormalizeStepKey(orderedSteps[index]) == normalizedCandidate)
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindStepIndexByCode(IReadOnlyList<string> orderedSteps, string? skillCode)
    {
        var normalizedCode = NormalizeStepKey(skillCode);
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return -1;
        }

        for (var index = 0; index < orderedSteps.Count; index++)
        {
            if (NormalizeStepKey(orderedSteps[index]).Contains(normalizedCode, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static string NormalizeStepKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var filtered = value
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return new string(filtered);
    }
}
