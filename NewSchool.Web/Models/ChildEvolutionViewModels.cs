namespace NewSchool.Web.Models;

public class ChildEvolutionCenterViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string PromiseHeadline { get; set; } = string.Empty;
    public string DiagnosisSummary { get; set; } = string.Empty;
    public string ReassuranceSummary { get; set; } = string.Empty;
    public int SessionsThisMonth { get; set; }
    public int MinutesThisMonth { get; set; }
    public int EvidenceCountThisMonth { get; set; }
    public int GoalsOnTrack { get; set; }
    public string RoutineUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public string ReportUrl { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public string AchievementShareUrl { get; set; } = string.Empty;
    public List<DomainDiagnosisCardViewModel> Diagnostics { get; set; } = new();
    public List<MonthlyGoalViewModel> MonthlyGoals { get; set; } = new();
    public List<AchievementHighlightViewModel> Achievements { get; set; } = new();
    public List<ChildAchievementViewModel> CelebrationBadges { get; set; } = new();
    public List<PortfolioEvidenceViewModel> EvidenceItems { get; set; } = new();
    public List<string> NextCycleRecommendations { get; set; } = new();
    public List<MonthlyHistoryViewModel> History { get; set; } = new();
    public RecoveryPlanCardViewModel? RecoveryPlan { get; set; }
    public FamilyConsistencyViewModel? Consistency { get; set; }
}

public class DomainDiagnosisCardViewModel
{
    public string DomainLabel { get; set; } = string.Empty;
    public int InitialPercent { get; set; }
    public int CurrentPercent { get; set; }
    public int DeltaPercent { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public string Summary { get; set; } = string.Empty;
}

public class MonthlyGoalViewModel
{
    public string DomainLabel { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string WhyItMatters { get; set; } = string.Empty;
    public int CurrentPercent { get; set; }
    public int TargetPercent { get; set; }
    public int ProgressPercent { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
}

public class AchievementHighlightViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PortfolioEvidenceViewModel
{
    public DateTime LoggedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);
    public bool IsVideo => MediaContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsImage => MediaContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}

public class ChildMonthlyReportViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public string ParentMessage { get; set; } = string.Empty;
    public string PromiseHeadline { get; set; } = string.Empty;
    public int SessionsThisMonth { get; set; }
    public int MinutesThisMonth { get; set; }
    public int EvidenceCountThisMonth { get; set; }
    public List<DomainDiagnosisCardViewModel> Diagnostics { get; set; } = new();
    public List<MonthlyGoalViewModel> MonthlyGoals { get; set; } = new();
    public List<AchievementHighlightViewModel> Achievements { get; set; } = new();
    public List<PortfolioEvidenceViewModel> EvidenceItems { get; set; } = new();
    public List<string> NextCycleRecommendations { get; set; } = new();
    public List<MonthlyHistoryViewModel> History { get; set; } = new();
}

public class MonthlyHistoryViewModel
{
    public string MonthLabel { get; set; } = string.Empty;
    public int OverallScore { get; set; }
    public int SessionsCount { get; set; }
    public int DeltaFromPrevious { get; set; }
    public string StrongestArea { get; set; } = string.Empty;
    public string AttentionArea { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
