namespace NewSchool.Web.Models;

public class AnnualCurriculumViewModel
{
    public string SchoolYearLabel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string CurriculumBandLabel { get; set; } = string.Empty;
    public int TotalProprietaryLessons { get; set; }
    public string CurrentPhaseLabel { get; set; } = string.Empty;
    public string CurrentPhaseTitle { get; set; } = string.Empty;
    public string CurrentPhaseSummary { get; set; } = string.Empty;
    public string FamilyRoutineNote { get; set; } = string.Empty;
    public List<AnnualCurriculumPhaseViewModel> Phases { get; set; } = new();
    public List<AnnualCurriculumSubjectViewModel> Subjects { get; set; } = new();
}

public class AnnualCurriculumPhaseViewModel
{
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MonthsLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ParentAction { get; set; } = string.Empty;
    public string ChildGain { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public int PlannedTasksCount { get; set; }
    public int UnitCount { get; set; }
    public string LessonCountLabel { get; set; } = string.Empty;
    public List<string> FeaturedTasks { get; set; } = new();
    public List<string> MaterialsToKeepReady { get; set; } = new();
}

public class AnnualCurriculumSubjectViewModel
{
    public string Title { get; set; } = string.Empty;
    public string CurrentGoal { get; set; } = string.Empty;
    public string YearGoal { get; set; } = string.Empty;
    public string ProgressLabel { get; set; } = string.Empty;
    public string ProgressChipClass { get; set; } = "neutral";
    public int ProgressPercent { get; set; }
    public List<string> Milestones { get; set; } = new();
    public string PhaseAssessmentTitle { get; set; } = string.Empty;
    public string PhaseStatusLabel { get; set; } = string.Empty;
    public string PhaseStatusChipClass { get; set; } = "neutral";
    public string PhaseReviewSummary { get; set; } = string.Empty;
    public string PhaseClosureSummary { get; set; } = string.Empty;
    public string LessonsProgressLabel { get; set; } = string.Empty;
    public string AdvancementSignal { get; set; } = string.Empty;
    public FamilyLibraryRecommendationViewModel? RecommendedBook { get; set; }
    public FamilyLibraryRecommendationViewModel? RecommendedPrintable { get; set; }
}
