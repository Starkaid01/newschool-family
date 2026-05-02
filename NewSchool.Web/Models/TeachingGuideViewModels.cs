namespace NewSchool.Web.Models;

public class TeachingGuideViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string TodayLabel { get; set; } = string.Empty;
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string ParentSummary { get; set; } = string.Empty;
    public string DailyRecommendation { get; set; } = string.Empty;
    public string TomorrowAdjustment { get; set; } = string.Empty;
    public string EvidenceReminder { get; set; } = string.Empty;
    public bool CompletedToday { get; set; }
    public int TotalMinutesPlanned { get; set; }
    public string CurriculumUrl { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string RegisterSessionUrl { get; set; } = string.Empty;
    public string PrintUrl { get; set; } = string.Empty;
    public string AcademyUrl { get; set; } = string.Empty;
    public List<string> BeforeStartChecklist { get; set; } = new();
    public List<string> MaterialChecklist { get; set; } = new();
    public List<TeachingQuickTipViewModel> MaterialAlternatives { get; set; } = new();
    public List<TeachingLessonCardViewModel> Lessons { get; set; } = new();
    public List<TeachingPrintableActivityViewModel> PrintableActivities { get; set; } = new();
    public TeachingCurriculumSnapshotViewModel Curriculum { get; set; } = new();
    public List<TeachingQuickTipViewModel> QuickTips { get; set; } = new();
}

public class TeachingLessonCardViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string BlockTitle { get; set; } = string.Empty;
    public string SupportSourceLabel { get; set; } = string.Empty;
    public string SupportSourceChipClass { get; set; } = "neutral";
    public string FunctionalTrackLabel { get; set; } = string.Empty;
    public string FunctionalTrackChipClass { get; set; } = "neutral";
    public string FocusLabel { get; set; } = string.Empty;
    public string FocusChipClass { get; set; } = "neutral";
    public int DurationMinutes { get; set; }
    public string Goal { get; set; } = string.Empty;
    public string AdultInstruction { get; set; } = string.Empty;
    public string ExampleScript { get; set; } = string.Empty;
    public string ChildInstruction { get; set; } = string.Empty;
    public string IfStuck { get; set; } = string.Empty;
    public string SuccessSignal { get; set; } = string.Empty;
    public string EvidenceToSave { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public List<string> MaterialItems { get; set; } = new();
    public List<string> StepByStep { get; set; } = new();
    public bool IsPrintableFocus { get; set; }
}

public class TeachingPrintableActivityViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string StageLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string SampleText { get; set; } = string.Empty;
    public string SupportTip { get; set; } = string.Empty;
    public int WritingLines { get; set; } = 4;
    public bool ShowDrawingBox { get; set; }
    public List<string> Questions { get; set; } = new();
    public List<string> WordBank { get; set; } = new();
}

public class TeachingCurriculumSnapshotViewModel
{
    public string StageLabel { get; set; } = string.Empty;
    public string CurrentFocus { get; set; } = string.Empty;
    public string AnnualObjective { get; set; } = string.Empty;
    public List<string> EvidenceTargets { get; set; } = new();
}

public class TeachingQuickTipViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
