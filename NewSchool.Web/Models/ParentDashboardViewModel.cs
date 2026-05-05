namespace NewSchool.Web.Models;

public class ParentDashboardViewModel
{
    public string ParentName { get; set; } = string.Empty;
    public int TotalChildren { get; set; }
    public int SessionsThisWeek { get; set; }
    public int MinutesThisWeek { get; set; }
    public string SubscriptionStatus { get; set; } = "inactive";
    public DateTime? TrialEndsAt { get; set; }
    public int? TrialDaysLeft { get; set; }
    public DateTime? SubscriptionCurrentPeriodEnd { get; set; }
    public bool IsSubscriber { get; set; }
    public string? GateMessage { get; set; }
    public EvidenceStorageSummaryViewModel Storage { get; set; } = new();
    public RevenueBannerViewModel? RevenueBanner { get; set; }
    public List<RecoveryPlanCardViewModel> RecoveryPlans { get; set; } = new();
    public FamilyConsistencyViewModel Consistency { get; set; } = new();
    public ReferralSummaryViewModel Referral { get; set; } = new();
    public WeeklyFamilyReportViewModel WeeklyReport { get; set; } = new();
    public List<OnboardingStepViewModel> OnboardingSteps { get; set; } = new();
    public List<AgeTrackPreviewViewModel> AgeTracks { get; set; } = new();
    public List<ChildCardViewModel> Children { get; set; } = new();
}

public class RevenueBannerViewModel
{
    public string Style { get; set; } = "neutral";
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
    public string PrimaryActionKind { get; set; } = string.Empty;
    public string? SecondaryActionLabel { get; set; }
    public string? SecondaryActionKind { get; set; }
}

public class ChildCardViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public int DailyStudyMinutes { get; set; }
    public string CurrentFocus { get; set; } = string.Empty;
    public bool IsPreparingTodayPlan { get; set; }
    public bool PlanCompletedToday { get; set; }
    public int TotalSessions { get; set; }
    public string WeeklyHeadline { get; set; } = string.Empty;
    public string WeeklyReviewSkill { get; set; } = string.Empty;
    public string WeeklyAdvanceSkill { get; set; } = string.Empty;
    public string EvolutionHeadline { get; set; } = string.Empty;
    public string MonthlyFocus { get; set; } = string.Empty;
    public int EvolutionProgressPercent { get; set; }
    public RecoveryPlanCardViewModel? RecoveryPlan { get; set; }
    public List<ChildAchievementViewModel> Achievements { get; set; } = new();
    public int ExternalContentCompletedCount { get; set; }
    public string ExternalContentHeadline { get; set; } = string.Empty;
    public string AlertChipLabel { get; set; } = string.Empty;
    public string AlertChipClass { get; set; } = "neutral";
}

public class OnboardingStepViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AgeTrackPreviewViewModel
{
    public string AgeBand { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
    public string ParentExperience { get; set; } = string.Empty;
}

public class WeeklyFamilyReportViewModel
{
    public string ImprovedSummary { get; set; } = string.Empty;
    public string ReviewSummary { get; set; } = string.Empty;
    public string AdvanceSummary { get; set; } = string.Empty;
    public List<WeeklyChildReportItemViewModel> Children { get; set; } = new();
}

public class WeeklyChildReportItemViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string ImprovedSkill { get; set; } = string.Empty;
    public string ReviewSkill { get; set; } = string.Empty;
    public string AdvanceSkill { get; set; } = string.Empty;
    public string NextWeekRecommendation { get; set; } = string.Empty;
}
