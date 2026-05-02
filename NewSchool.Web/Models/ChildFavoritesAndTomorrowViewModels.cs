using NewSchool.Web.Domain;

namespace NewSchool.Web.Models;

public class ChildFavoritesViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public bool HasTeaTracks { get; set; }
    public string ChildUrl { get; set; } = string.Empty;
    public string TeaTracksUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public string TomorrowPreviewUrl { get; set; } = string.Empty;
    public int FavoriteCount { get; set; }
    public int QueuedForTomorrowCount { get; set; }
    public string ActionMessage { get; set; } = string.Empty;
    public string ActionMessageStyle { get; set; } = "neutral";
    public List<FavoriteTrackGroupViewModel> Groups { get; set; } = new();
}

public class FavoriteTrackGroupViewModel
{
    public FunctionalSupportTrack Track { get; set; }
    public string TrackLabel { get; set; } = string.Empty;
    public string TrackChipClass { get; set; } = "neutral";
    public string SupportSummary { get; set; } = string.Empty;
    public string TrackUrl { get; set; } = string.Empty;
    public List<FavoriteActivityViewModel> Activities { get; set; } = new();
}

public class FavoriteActivityViewModel
{
    public Guid TemplateId { get; set; }
    public FunctionalSupportTrack Track { get; set; }
    public string TrackLabel { get; set; } = string.Empty;
    public string TrackChipClass { get; set; } = "neutral";
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string DomainLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildMission { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public string RecommendationLabel { get; set; } = string.Empty;
    public string RecommendationChipClass { get; set; } = "neutral";
    public bool IsQueuedForTomorrow { get; set; }
    public string TrackUrl { get; set; } = string.Empty;
}

public class TomorrowPlanPreviewViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string ParentSummary { get; set; } = string.Empty;
    public string ChildNarrative { get; set; } = string.Empty;
    public bool IsRecoveryPlan { get; set; }
    public string RecoveryHeadline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string FavoritesUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public bool HasTeaTracks { get; set; }
    public string TeaTracksUrl { get; set; } = string.Empty;
    public List<TeaTrackQuickLinkViewModel> TeaTrackQuickLinks { get; set; } = new();
    public int TotalMinutes { get; set; }
    public int BlockCount { get; set; }
    public string ActionMessage { get; set; } = string.Empty;
    public string ActionMessageStyle { get; set; } = "neutral";
    public List<string> PinnedActivities { get; set; } = new();
    public List<TomorrowDirectiveViewModel> Directives { get; set; } = new();
    public List<TomorrowPlanPreviewBlockViewModel> Blocks { get; set; } = new();
}

public class TomorrowDirectiveViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
}

public class TomorrowPlanPreviewBlockViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string SkillName { get; set; } = string.Empty;
    public string FocusLabel { get; set; } = string.Empty;
    public string FocusChipClass { get; set; } = "neutral";
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPinnedActivity { get; set; }
    public bool IsManualTrackFocus { get; set; }
    public string OriginLabel { get; set; } = string.Empty;
    public string OriginChipClass { get; set; } = "neutral";
    public string TrackUrl { get; set; } = string.Empty;
    public SystemCurriculumUnitViewModel? CurrentSystemUnit { get; set; }
    public FamilyLibraryRecommendationViewModel? RecommendedPrintable { get; set; }
    public string CurriculumOriginSummary { get; set; } = string.Empty;
    public string PrintableReason { get; set; } = string.Empty;
    public ProprietaryLessonPacketViewModel? LessonPacket { get; set; }
}
