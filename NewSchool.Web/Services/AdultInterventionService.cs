using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class AdultInterventionService(
    ApplicationDbContext db,
    OpenRouterPedagogyService openRouterPedagogyService)
{
    public async Task ApplyInterventionAsync(
        ChildProfile child,
        ChildSkillProgress progress,
        DailyPlanBlock block,
        SkillFeedbackLevel rating,
        string? sessionChallenges,
        CancellationToken cancellationToken = default)
    {
        var guide = await BuildGuideAsync(child, progress, block, sessionChallenges, rating, allowAiRewrite: true, cancellationToken);
        if (guide is null)
        {
            return;
        }

        if (progress.NeedsRemediation || rating == SkillFeedbackLevel.NeedsSupport)
        {
            progress.RemediationPlan = guide.ParentCoachSummary;
        }

        progress.Recommendation = guide.NextAction;
    }

    public async Task<AdultInterventionGuide?> BuildGuideAsync(
        ChildProfile child,
        ChildSkillProgress? progress,
        DailyPlanBlock block,
        string? sessionChallenges,
        SkillFeedbackLevel? latestRating,
        bool allowAiRewrite,
        CancellationToken cancellationToken = default)
    {
        var playbook = await ResolvePlaybookEntryAsync(child, progress, block, sessionChallenges, latestRating, cancellationToken);
        if (playbook is null)
        {
            return null;
        }

        var challengeLabel = playbook.TriggerLabel.ToLowerInvariant();
        var defaultCoachNote = $"Diga: {playbook.WhatToSay} Faca hoje: {playbook.QuickActivity} Observe melhora quando {playbook.SuccessSignal.ToLowerInvariant()}.";
        var shouldUseAi = allowAiRewrite &&
            (latestRating == SkillFeedbackLevel.NeedsSupport || progress?.NeedsRemediation == true) &&
            !string.IsNullOrWhiteSpace(sessionChallenges);

        var aiCoachNote = shouldUseAi
            ? await openRouterPedagogyService.TryBuildAdultCoachNoteAsync(
                child.FullName,
                block.SkillName,
                $"{challengeLabel}. {sessionChallenges}".Trim(),
                defaultCoachNote,
                cancellationToken)
            : null;

        return new AdultInterventionGuide
        {
            TriggerCode = playbook.TriggerCode,
            Headline = playbook.Headline,
            TriggerLabel = playbook.TriggerLabel,
            HowToSpot = playbook.HowToSpot,
            LikelyCause = playbook.LikelyCause,
            WhatToSay = playbook.WhatToSay,
            WhatToAvoid = playbook.WhatToAvoid,
            QuickActivity = playbook.QuickActivity,
            Materials = playbook.Materials,
            SuccessSignal = playbook.SuccessSignal,
            RepeatPlan = playbook.RepeatPlan,
            FallbackAction = playbook.FallbackAction,
            ParentCoachSummary = aiCoachNote ?? defaultCoachNote,
            NextAction = $"Hoje priorize {playbook.QuickActivity.ToLowerInvariant()} e repita {playbook.RepeatPlan.ToLowerInvariant()}."
        };
    }

    private async Task<InterventionPlaybookEntry?> ResolvePlaybookEntryAsync(
        ChildProfile child,
        ChildSkillProgress? progress,
        DailyPlanBlock block,
        string? sessionChallenges,
        SkillFeedbackLevel? latestRating,
        CancellationToken cancellationToken)
    {
        var entries = await db.InterventionPlaybookEntries
            .Where(x => x.Domain == block.Domain)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return null;
        }

        var detectedTrigger = DetectTriggerCode(block.Domain, block.SkillName, sessionChallenges, progress, latestRating);
        return entries
            .Select(entry => new
            {
                Entry = entry,
                Score = ScoreEntry(entry, child, progress, detectedTrigger)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Entry.TriggerCode)
            .Select(x => x.Entry)
            .FirstOrDefault();
    }

    private static int ScoreEntry(
        InterventionPlaybookEntry entry,
        ChildProfile child,
        ChildSkillProgress? progress,
        string detectedTrigger)
    {
        var score = 0;
        if (string.Equals(entry.TriggerCode, detectedTrigger, StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
        }

        if (string.IsNullOrWhiteSpace(entry.GoalTrack) ||
            string.Equals(entry.GoalTrack, child.FamilyGoalTrack, StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        if (entry.StageScope.Equals("all", StringComparison.OrdinalIgnoreCase) || progress is null)
        {
            score += 10;
        }
        else
        {
            var scopes = entry.StageScope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (scopes.Contains(progress.SkillStage, StringComparer.OrdinalIgnoreCase))
            {
                score += 25;
            }
        }

        if (progress?.NeedsRemediation == true)
        {
            score += 10;
        }

        return score;
    }

    private static string DetectTriggerCode(
        LearningDomain domain,
        string skillName,
        string? sessionChallenges,
        ChildSkillProgress? progress,
        SkillFeedbackLevel? latestRating)
    {
        var text = $"{skillName} {sessionChallenges} {progress?.Recommendation} {progress?.RemediationPlan}"
            .ToLowerInvariant();

        if (text.Contains("frustra") || text.Contains("chora") || text.Contains("resiste") || text.Contains("nao quer"))
        {
            return "error_resistance";
        }

        if (text.Contains("foco") || text.Contains("atenc") || text.Contains("distra"))
        {
            return "short_attention_span";
        }

        if (text.Contains("inicia") || text.Contains("comec") || text.Contains("sozinh"))
        {
            return "task_initiation_block";
        }

        return domain switch
        {
            LearningDomain.Language when text.Contains("silab") => "syllable_segmentation_block",
            LearningDomain.Language when text.Contains("letra") || text.Contains("som") || text.Contains("rima") || text.Contains("fon") => "phonemic_confusion",
            LearningDomain.Language when text.Contains("leitura") || text.Contains("texto") || text.Contains("compreen") => "reading_comprehension_break",
            LearningDomain.Language when text.Contains("escrev") || text.Contains("traco") || text.Contains("espelh") || text.Contains("copia") => "writing_transcription_friction",
            LearningDomain.Math when text.Contains("cont") || text.Contains("quant") || text.Contains("numero") => "quantity_without_meaning",
            LearningDomain.Math when text.Contains("soma") || text.Contains("subtra") || text.Contains("oper") || text.Contains("problema") => "operation_sequence_break",
            LearningDomain.Math when text.Contains("tabuada") || text.Contains("multip") || text.Contains("grupo") => "multiplication_without_groups",
            LearningDomain.World when text.Contains("observ") || text.Contains("compar") || text.Contains("explica") => "observation_to_language_gap",
            LearningDomain.World => "research_without_focus",
            LearningDomain.ExecutiveFunction when text.Contains("foco") || text.Contains("atenc") => "short_attention_span",
            LearningDomain.ExecutiveFunction when text.Contains("inicia") || text.Contains("comec") => "task_initiation_block",
            LearningDomain.ExecutiveFunction => "error_resistance",
            _ when latestRating == SkillFeedbackLevel.NeedsSupport && progress?.SkillStage is "starting" or "guided_practice" => "task_initiation_block",
            _ => "short_attention_span"
        };
    }
}

public class AdultInterventionGuide
{
    public string TriggerCode { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string TriggerLabel { get; set; } = string.Empty;
    public string HowToSpot { get; set; } = string.Empty;
    public string LikelyCause { get; set; } = string.Empty;
    public string WhatToSay { get; set; } = string.Empty;
    public string WhatToAvoid { get; set; } = string.Empty;
    public string QuickActivity { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string SuccessSignal { get; set; } = string.Empty;
    public string RepeatPlan { get; set; } = string.Empty;
    public string FallbackAction { get; set; } = string.Empty;
    public string ParentCoachSummary { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
}
