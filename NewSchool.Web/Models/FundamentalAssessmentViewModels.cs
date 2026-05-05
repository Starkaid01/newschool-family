namespace NewSchool.Web.Models;

public class FundamentalAssessmentSummaryViewModel
{
    public bool IsAvailable { get; set; }
    public bool IsPrinted { get; set; }
    public bool IsCorrected { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string QuestionCountLabel { get; set; } = string.Empty;
    public string PrintableUrl { get; set; } = string.Empty;
    public string CorrectionUrl { get; set; } = string.Empty;
    public string ReinforcementUrl { get; set; } = string.Empty;
    public string ScoreLabel { get; set; } = string.Empty;
}

public class FundamentalAssessmentPrintableViewModel
{
    public Guid ChildId { get; set; }
    public Guid DailyPlanBlockId { get; set; }
    public Guid AssessmentId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string UnitTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public string PrintableHeadline { get; set; } = string.Empty;
    public string PrintableSummary { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public string PrintUrl { get; set; } = string.Empty;
    public string CorrectionUrl { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public List<FundamentalAssessmentQuestionViewModel> Questions { get; set; } = new();
}

public class FundamentalAssessmentCorrectionViewModel
{
    public Guid AssessmentId { get; set; }
    public Guid ChildId { get; set; }
    public Guid DailyPlanBlockId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string SaveUrl { get; set; } = string.Empty;
    public string PrintableUrl { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string ScoreLabel { get; set; } = string.Empty;
    public List<FundamentalAssessmentQuestionViewModel> Questions { get; set; } = new();
}

public class FundamentalAssessmentQuestionViewModel
{
    public Guid ItemId { get; set; }
    public int SortOrder { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
    public string TeacherNote { get; set; } = string.Empty;
    public bool? IsCorrect { get; set; }
}

public class SaveFundamentalAssessmentCorrectionViewModel
{
    public Guid AssessmentId { get; set; }
    public Guid ChildId { get; set; }
    public Guid DailyPlanBlockId { get; set; }
    public List<SaveFundamentalAssessmentCorrectionItemViewModel> Items { get; set; } = new();
}

public class SaveFundamentalAssessmentCorrectionItemViewModel
{
    public Guid ItemId { get; set; }
    public bool? IsCorrect { get; set; }
}

public class ChildReinforcementViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string SchoolReportUrl { get; set; } = string.Empty;
    public List<ReinforcementSubjectViewModel> Subjects { get; set; } = new();
}

public class ReinforcementSubjectViewModel
{
    public string SubjectLabel { get; set; } = string.Empty;
    public string SubjectChipClass { get; set; } = "neutral";
    public decimal AverageScoreValue { get; set; }
    public string AverageScoreLabel { get; set; } = string.Empty;
    public int AssessmentCount { get; set; }
    public string ReinforcementSummary { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
}
