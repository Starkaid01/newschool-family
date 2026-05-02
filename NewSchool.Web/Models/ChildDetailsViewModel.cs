namespace NewSchool.Web.Models;

public class ChildDetailsViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public int DailyStudyMinutes { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string ParentSummary { get; set; } = string.Empty;
    public string ChildNarrative { get; set; } = string.Empty;
    public string DailyRecommendation { get; set; } = string.Empty;
    public bool CompletedToday { get; set; }
    public bool IsRecoveryPlan { get; set; }
    public string RecoveryHeadline { get; set; } = string.Empty;
    public string EvolutionUrl { get; set; } = string.Empty;
    public List<ChildAchievementViewModel> Achievements { get; set; } = new();
    public List<PlanBlockViewModel> Blocks { get; set; } = new();
    public List<SkillProgressViewModel> SkillProgress { get; set; } = new();
    public List<SkillProgressViewModel> WeakSkills { get; set; } = new();
    public List<SkillProgressViewModel> DevelopingSkills { get; set; } = new();
    public List<SkillProgressViewModel> StrongSkills { get; set; } = new();
    public List<PedagogicalTimelineItemViewModel> Timeline { get; set; } = new();
    public List<SessionHistoryViewModel> SessionHistory { get; set; } = new();
    public List<SkillCheckupViewModel> DueCheckups { get; set; } = new();
    public List<SkillReadinessCheckViewModel> DueReadinessChecks { get; set; } = new();
    public string CurriculumUrl { get; set; } = string.Empty;
    public bool HasTeaTracks { get; set; }
    public string TeaTracksUrl { get; set; } = string.Empty;
    public List<TeaTrackQuickLinkViewModel> TeaTrackQuickLinks { get; set; } = new();
    public string TeaProfileUrl { get; set; } = string.Empty;
    public string AdaptiveRoutineUrl { get; set; } = string.Empty;
    public string DossierUrl { get; set; } = string.Empty;
    public string FavoritesUrl { get; set; } = string.Empty;
    public int FavoriteActivitiesCount { get; set; }
    public string LessonFlowHeadline { get; set; } = string.Empty;
    public string LessonFlowMessage { get; set; } = string.Empty;
    public string LessonFlowStyle { get; set; } = "neutral";
    public int ExternalContentCompletedCount { get; set; }
    public List<ExternalContentProgressCardViewModel> ExternalContentRecommendations { get; set; } = new();
    public GuidedDailyLessonViewModel GuidedLesson { get; set; } = new();
    public TomorrowLessonPreviewViewModel TomorrowLesson { get; set; } = new();
    public WeeklyStudySnapshotViewModel WeeklyStudy { get; set; } = new();
    public string EvidenceCenterUrl { get; set; } = string.Empty;
    public FamilyLibraryCurriculumBridgeViewModel LibraryBridge { get; set; } = new();
    public List<SystemCurriculumTrackViewModel> SystemCurriculumTracks { get; set; } = new();
    public DailyTrailViewModel TodayTrail { get; set; } = new();
    public EvidenceAutomationViewModel DailyEvidenceAssistant { get; set; } = new();
    public EvidenceAutomationViewModel ExternalContentEvidenceAssistant { get; set; } = new();
    public WeeklyRoadmapViewModel WeeklyRoadmap { get; set; } = new();
    public string TomorrowPreviewUrl { get; set; } = string.Empty;
    public bool HasTomorrowPlanPreview { get; set; }
    public int TomorrowPlanBlocksCount { get; set; }
    public AdaptiveRoutineSnapshotViewModel AdaptiveSnapshot { get; set; } = new();
    public LogSessionViewModel LogSession { get; set; } = new();
    public SkillCheckupFormViewModel SkillCheckups { get; set; } = new();
    public SkillReadinessFormViewModel ReadinessChecks { get; set; } = new();
}

public class PlanBlockViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string SkillName { get; set; } = string.Empty;
    public string SkillStageLabel { get; set; } = string.Empty;
    public string SkillStageChipClass { get; set; } = "neutral";
    public string NextMilestone { get; set; } = string.Empty;
    public string InterventionTip { get; set; } = string.Empty;
    public string FocusLabel { get; set; } = string.Empty;
    public string FocusChipClass { get; set; } = "neutral";
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public bool IsRecoveryFocus { get; set; }
    public string RecoveryNote { get; set; } = string.Empty;
    public bool IsSpacedReview { get; set; }
    public string ReviewNote { get; set; } = string.Empty;
    public CuratedTaskSuggestionViewModel? SuggestedTask { get; set; }
    public AdultInterventionGuideViewModel? AdultIntervention { get; set; }
    public int DurationMinutes { get; set; }
}

public class AdultInterventionGuideViewModel
{
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
}

public class SkillCheckupViewModel
{
    public Guid Id { get; set; }
    public string DomainLabel { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string PromptTitle { get; set; } = string.Empty;
    public string ParentPrompt { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
}

public class SkillCheckupFormViewModel
{
    public Guid ChildId { get; set; }
    public List<SkillCheckupInputViewModel> Items { get; set; } = new();
}

public class SkillCheckupInputViewModel
{
    public Guid CheckupId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string PromptTitle { get; set; } = string.Empty;
    public int Rating { get; set; } = 2;
    public string Notes { get; set; } = string.Empty;
}

public class SkillReadinessCheckViewModel
{
    public Guid Id { get; set; }
    public string DomainLabel { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string ParentPrompt { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public string UnlocksSkillName { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
}

public class SkillReadinessFormViewModel
{
    public Guid ChildId { get; set; }
    public List<SkillReadinessInputViewModel> Items { get; set; } = new();
}

public class SkillReadinessInputViewModel
{
    public Guid ReadinessCheckId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public int Rating { get; set; } = 2;
    public string Notes { get; set; } = string.Empty;
}

public class SessionHistoryViewModel
{
    public Guid Id { get; set; }
    public DateTime LoggedAt { get; set; }
    public int MinutesCompleted { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Wins { get; set; } = string.Empty;
    public string Challenges { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string MediaFileName { get; set; } = string.Empty;
    public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);
    public bool IsVideo => MediaContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsImage => MediaContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    public bool IsDocument =>
        MediaContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/msword", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);
}

public class SkillProgressViewModel
{
    public string DomainLabel { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string SkillName { get; set; } = string.Empty;
    public string SkillStageLabel { get; set; } = string.Empty;
    public string SkillStageChipClass { get; set; } = "neutral";
    public int MasteryScore { get; set; }
    public int TimesPracticed { get; set; }
    public bool ReadyToAdvance { get; set; }
    public bool NeedsReadinessCheck { get; set; }
    public bool ReadinessApproved { get; set; }
    public bool NeedsRemediation { get; set; }
    public DateTime? NextReviewAt { get; set; }
    public string ReadinessStatusLabel { get; set; } = string.Empty;
    public string NextMilestone { get; set; } = string.Empty;
    public string RemediationPlan { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public int ProgressPercent => Math.Clamp(MasteryScore, 0, 100);
}

public class PedagogicalTimelineItemViewModel
{
    public DateTime When { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ChildCurriculumViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int TotalMinutes { get; set; }
    public int EvidenceCount { get; set; }
    public string SelectedArea { get; set; } = string.Empty;
    public List<string> AvailableAreas { get; set; } = new();
    public string PrintUrl { get; set; } = string.Empty;
    public string EvidenceCenterUrl { get; set; } = string.Empty;
    public string AcademyUrl { get; set; } = string.Empty;
    public FamilyLibraryCurriculumBridgeViewModel LibraryBridge { get; set; } = new();
    public FamilyLibraryAnnualSpineViewModel AnnualReadingSpine { get; set; } = new();
    public bool HasTeaTracks { get; set; }
    public string TeaTracksUrl { get; set; } = string.Empty;
    public List<TeaTrackQuickLinkViewModel> TeaTrackQuickLinks { get; set; } = new();
    public AnnualCurriculumViewModel AnnualPlan { get; set; } = new();
    public List<SystemCurriculumTrackViewModel> SystemCurriculumTracks { get; set; } = new();
    public WeeklyRoadmapViewModel WeeklyRoadmap { get; set; } = new();
    public List<SkillProgressViewModel> SkillProgress { get; set; } = new();
    public List<CurriculumEntryViewModel> Entries { get; set; } = new();
}

public class CurriculumEntryViewModel
{
    public Guid SessionId { get; set; }
    public DateTime LoggedAt { get; set; }
    public string Theme { get; set; } = string.Empty;
    public int MinutesCompleted { get; set; }
    public string Wins { get; set; } = string.Empty;
    public string Challenges { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string MediaFileName { get; set; } = string.Empty;
    public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);
    public bool IsVideo => MediaContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsImage => MediaContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    public bool IsDocument =>
        MediaContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/msword", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);
    public List<EntryBadgeViewModel> ContextBadges { get; set; } = new();
    public List<CurriculumSkillEntryViewModel> Skills { get; set; } = new();
}

public class CurriculumSkillEntryViewModel
{
    public string SkillName { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string PerformanceLabel { get; set; } = string.Empty;
    public string PerformanceChipClass { get; set; } = "neutral";
}

public class EntryBadgeViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
}
