namespace NewSchool.Web.Models;

public class GuidedDailyLessonViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int CompletedLessonsCount { get; set; }
    public int PlannedLessonsCount { get; set; }
    public int ProgressPercent { get; set; }
    public string CompletionBanner { get; set; } = string.Empty;
    public List<GuidedLessonCardViewModel> Lessons { get; set; } = new();
}

public class GuidedLessonCardViewModel
{
    public Guid DailyPlanBlockId { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;
    public string SubjectChipClass { get; set; } = "neutral";
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentSummary { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string MaterialsSummary { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string WhyThisLesson { get; set; } = string.Empty;
    public int SuggestedMinutes { get; set; }
    public bool IsCompleted { get; set; }
    public string CompletionLabel { get; set; } = string.Empty;
    public int UnitStepNumber { get; set; }
    public int UnitStepCount { get; set; }
    public string UnitStepLabel { get; set; } = string.Empty;
    public string UnitStepSummary { get; set; } = string.Empty;
    public string NextUnitStepHint { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public SystemCurriculumUnitViewModel? CurrentSystemUnit { get; set; }
    public FamilyLibraryRecommendationViewModel? RecommendedPrintable { get; set; }
    public string CurriculumOriginSummary { get; set; } = string.Empty;
    public string PrintableReason { get; set; } = string.Empty;
    public ProprietaryLessonPacketViewModel? LessonPacket { get; set; }
    public CuratedResourceCardViewModel? PrimaryResource { get; set; }
    public CuratedSupportLinkViewModel? SupportLink { get; set; }
    public FundamentalAssessmentSummaryViewModel? Assessment { get; set; }
}

public class TomorrowLessonPreviewViewModel
{
    public bool HasPlan { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public List<string> TopLessons { get; set; } = new();
    public List<GuidedLessonCardViewModel> Lessons { get; set; } = new();
}

public class WeeklyStudySnapshotViewModel
{
    public string Headline { get; set; } = string.Empty;
    public int CompletedLessonsCount { get; set; }
    public int PlannedLessonsCount { get; set; }
    public int ProgressPercent { get; set; }
    public List<WeeklyStudyDayCardViewModel> Days { get; set; } = new();
}

public class WeeklyStudyDayCardViewModel
{
    public string DayLabel { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string MainTask { get; set; } = string.Empty;
    public string MaterialHint { get; set; } = string.Empty;
    public string OutcomeHint { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsToday { get; set; }
}
