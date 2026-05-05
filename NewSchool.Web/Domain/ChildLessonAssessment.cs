namespace NewSchool.Web.Domain;

public class ChildLessonAssessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid ParentUserId { get; set; }
    public AppUser ParentUser { get; set; } = null!;
    public Guid DailyPlanId { get; set; }
    public DailyPlan DailyPlan { get; set; } = null!;
    public Guid DailyPlanBlockId { get; set; }
    public DailyPlanBlock DailyPlanBlock { get; set; } = null!;
    public LearningDomain Domain { get; set; }
    public int SchoolYearNumber { get; set; }
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string UnitTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public string PrintableHeadline { get; set; } = string.Empty;
    public string PrintableSummary { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int CorrectCount { get; set; }
    public int ScorePercent { get; set; }
    public decimal ScoreValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? PrintedAtUtc { get; set; }
    public DateTime? CorrectedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ChildLessonAssessmentItem> Items { get; set; } = new List<ChildLessonAssessmentItem>();
}

public class ChildLessonAssessmentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public ChildLessonAssessment Assessment { get; set; } = null!;
    public int SortOrder { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
    public string TeacherNote { get; set; } = string.Empty;
    public bool? IsCorrect { get; set; }
}
