using NewSchool.Web.Domain;

namespace NewSchool.Web.Models;

public class TeaTrackQuickLinkViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
    public string Url { get; set; } = string.Empty;
}

public class TeaTrackHubViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public List<TeaTrackCardViewModel> Tracks { get; set; } = new();
}

public class TeaTrackCardViewModel
{
    public FunctionalSupportTrack Track { get; set; }
    public string Label { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalSkills { get; set; }
    public int PracticedSkills { get; set; }
    public int StrongSkills { get; set; }
    public int WeakSkills { get; set; }
    public int ProgressPercent { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public string NextAction { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class TeaTrackDetailViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string TeachingMethodologyLabel { get; set; } = string.Empty;
    public string LearningProfileLabel { get; set; } = string.Empty;
    public string GuidanceStyleLabel { get; set; } = string.Empty;
    public FunctionalSupportTrack Track { get; set; }
    public string TrackLabel { get; set; } = string.Empty;
    public string TrackChipClass { get; set; } = "neutral";
    public string TrackSummary { get; set; } = string.Empty;
    public string TrackDescription { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string ActionMessage { get; set; } = string.Empty;
    public string ActionMessageStyle { get; set; } = "neutral";
    public int TotalSkills { get; set; }
    public int PracticedSkills { get; set; }
    public int StrongSkills { get; set; }
    public int WeakSkills { get; set; }
    public int ProgressPercent { get; set; }
    public bool HasTomorrowPlan { get; set; }
    public string TomorrowPlanSummary { get; set; } = string.Empty;
    public string TomorrowFocusLabel { get; set; } = string.Empty;
    public string TomorrowFocusChipClass { get; set; } = "neutral";
    public List<string> TomorrowPinnedActivities { get; set; } = new();
    public string HubUrl { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public List<TeaTrackQuickLinkViewModel> TrackLinks { get; set; } = new();
    public List<TeaTrackMaterialViewModel> MaterialKit { get; set; } = new();
    public List<TeaTrackActivityViewModel> Activities { get; set; } = new();
    public List<PlanBlockViewModel> TodayBlocks { get; set; } = new();
    public List<TeaTrackRoadmapItemViewModel> Roadmap { get; set; } = new();
    public List<CurriculumEntryViewModel> Entries { get; set; } = new();
}

public class TeaTrackMaterialViewModel
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public string SuggestedUse { get; set; } = string.Empty;
}

public class TeaTrackActivityViewModel
{
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildMission { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string RecommendationLabel { get; set; } = string.Empty;
    public string RecommendationChipClass { get; set; } = "neutral";
    public int ProgressPercent { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsCurrentFocus { get; set; }
    public bool IsQueuedForTomorrow { get; set; }
}

public class TeaTrackRoadmapItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public int ProgressPercent { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public bool IsCurrentFocus { get; set; }
    public bool HasProgress { get; set; }
}
