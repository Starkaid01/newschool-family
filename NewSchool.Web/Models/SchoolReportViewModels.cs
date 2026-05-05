namespace NewSchool.Web.Models;

public class SchoolPhaseClosureViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string SubjectChipClass { get; set; } = "neutral";
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string UnitTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public string ReviewHeadline { get; set; } = string.Empty;
    public string ReviewSummary { get; set; } = string.Empty;
    public string ClosureSummary { get; set; } = string.Empty;
    public string AdvancementSignal { get; set; } = string.Empty;
    public string ParentAction { get; set; } = string.Empty;
    public string EvidenceIdea { get; set; } = string.Empty;
    public string LessonsProgressLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public int ProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int SessionTouches { get; set; }
    public int EvidenceCount { get; set; }
    public bool IsCurrentPhase { get; set; }
    public bool IsPastPhase { get; set; }
    public bool IsFuturePhase { get; set; }
    public List<string> ReviewChecklist { get; set; } = new();
}

public class ChildSchoolReportViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string FamilyGoalTrackLabel { get; set; } = string.Empty;
    public string SchoolYearLabel { get; set; } = string.Empty;
    public string GeneratedAtLabel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string CurrentPhaseLabel { get; set; } = string.Empty;
    public string CurrentPhaseTitle { get; set; } = string.Empty;
    public string CurrentPhaseStatusLabel { get; set; } = string.Empty;
    public string CurrentPhaseStatusChipClass { get; set; } = "neutral";
    public string CurrentPhaseSummary { get; set; } = string.Empty;
    public int SessionsThisYear { get; set; }
    public int MinutesThisYear { get; set; }
    public int EvidenceCountThisYear { get; set; }
    public int OverallProgressPercent { get; set; }
    public string ChildUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public string EvidenceCenterUrl { get; set; } = string.Empty;
    public string ReinforcementUrl { get; set; } = string.Empty;
    public List<SchoolReportSubjectViewModel> Subjects { get; set; } = new();
    public List<SchoolPhaseClosureViewModel> PhaseClosures { get; set; } = new();
    public List<MonthlyHistoryViewModel> MonthlyHistory { get; set; } = new();
    public List<AchievementHighlightViewModel> AchievementHighlights { get; set; } = new();
    public List<string> NextActions { get; set; } = new();
}

public class SchoolReportSubjectViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string SubjectChipClass { get; set; } = "neutral";
    public string YearGoal { get; set; } = string.Empty;
    public string CurrentUnitTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public string AssessmentAverageLabel { get; set; } = string.Empty;
    public int AssessmentCount { get; set; }
    public bool NeedsReinforcement { get; set; }
    public string PhaseStatusLabel { get; set; } = string.Empty;
    public string PhaseStatusChipClass { get; set; } = "neutral";
    public int ProgressPercent { get; set; }
    public string ProgressLabel { get; set; } = string.Empty;
    public string LessonsProgressLabel { get; set; } = string.Empty;
    public string ReviewSummary { get; set; } = string.Empty;
    public string ClosureSummary { get; set; } = string.Empty;
    public string AdvancementSignal { get; set; } = string.Empty;
}
