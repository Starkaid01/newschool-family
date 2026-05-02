namespace NewSchool.Web.Models;

public class AdaptiveRoutineSnapshotViewModel
{
    public string Summary { get; set; } = string.Empty;
    public int RecommendedWorkBlockMinutes { get; set; }
    public int RecommendedBreakMinutes { get; set; }
    public string SupportIntensityLabel { get; set; } = string.Empty;
    public string SupportIntensityChipClass { get; set; } = "neutral";
    public string VisualSupportRecommendation { get; set; } = string.Empty;
    public string TransitionRecommendation { get; set; } = string.Empty;
    public string TomorrowAdjustment { get; set; } = string.Empty;
    public string PlanBRecommendation { get; set; } = string.Empty;
    public List<AdaptiveRoutinePillViewModel> HelpfulSupports { get; set; } = new();
    public List<AdaptiveRoutinePillViewModel> CommonTriggers { get; set; } = new();
}

public class AdaptiveRoutinePillViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
}

public class ChildAdaptiveRoutineViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string TeaProfileUrl { get; set; } = string.Empty;
    public string DossierUrl { get; set; } = string.Empty;
    public AdaptiveRoutineSnapshotViewModel Snapshot { get; set; } = new();
    public List<AdaptiveOperationalBlockViewModel> Blocks { get; set; } = new();
    public List<AdaptiveObservationHistoryViewModel> Observations { get; set; } = new();
}

public class AdaptiveOperationalBlockViewModel
{
    public string Title { get; set; } = string.Empty;
    public string TrackLabel { get; set; } = string.Empty;
    public string TrackChipClass { get; set; } = "neutral";
    public int PlannedMinutes { get; set; }
    public int SuggestedTimerMinutes { get; set; }
    public string SupportCue { get; set; } = string.Empty;
    public string PlanB { get; set; } = string.Empty;
    public bool IsSensitiveTransition { get; set; }
}

public class AdaptiveObservationHistoryViewModel
{
    public DateTime ObservedAt { get; set; }
    public string ContextLabel { get; set; } = string.Empty;
    public string Antecedent { get; set; } = string.Empty;
    public string ChildReaction { get; set; } = string.Empty;
    public string WhatHelped { get; set; } = string.Empty;
    public string SupportUsed { get; set; } = string.Empty;
    public int DistressLevel { get; set; }
    public int TaskToleranceMinutes { get; set; }
    public bool NeededPlanB { get; set; }
}
