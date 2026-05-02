namespace NewSchool.Web.Models;

public class WeeklyRoadmapViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FocusMixLabel { get; set; } = string.Empty;
    public int StudyDaysCount { get; set; }
    public int TotalMinutes { get; set; }
    public int EvidenceTargetCount { get; set; }
    public List<WeeklyRoadmapDayViewModel> Days { get; set; } = new();
    public List<WeeklyRoadmapSubjectRunViewModel> SubjectRuns { get; set; } = new();
}

public class WeeklyRoadmapDayViewModel
{
    public DateTime PlannedDate { get; set; }
    public string RelativeLabel { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FocusAreaLabel { get; set; } = string.Empty;
    public string FocusAreaChipClass { get; set; } = "neutral";
    public int TotalMinutes { get; set; }
    public int StageCount { get; set; }
    public List<string> TopTasks { get; set; } = new();
    public List<string> Materials { get; set; } = new();
    public List<string> EvidenceItems { get; set; } = new();
    public string ExternalGuideLabel { get; set; } = string.Empty;
    public string ExternalGuideUrl { get; set; } = string.Empty;
    public string ExternalGuideReason { get; set; } = string.Empty;
    public bool HasExternalGuide => !string.IsNullOrWhiteSpace(ExternalGuideUrl);
}

public class WeeklyRoadmapSubjectRunViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string SubjectChipClass { get; set; } = "neutral";
    public string CurrentUnitLabel { get; set; } = string.Empty;
    public string CurrentUnitTitle { get; set; } = string.Empty;
    public string WeekHeadline { get; set; } = string.Empty;
    public string MonthHeadline { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public int CurrentWeekStepNumber { get; set; }
    public int CurrentMonthWeekNumber { get; set; }
    public List<string> WeekSteps { get; set; } = new();
    public List<string> MonthSteps { get; set; } = new();
}
