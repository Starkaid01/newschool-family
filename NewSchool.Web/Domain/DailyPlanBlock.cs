namespace NewSchool.Web.Domain;

public class DailyPlanBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DailyPlanId { get; set; }
    public DailyPlan DailyPlan { get; set; } = null!;
    public LearningDomain Domain { get; set; }
    public CurriculumSupportScope SupportScope { get; set; } = CurriculumSupportScope.General;
    public FunctionalSupportTrack FunctionalTrack { get; set; } = FunctionalSupportTrack.Base;
    public Guid? SourceTemplateId { get; set; }
    public CurriculumTemplate? SourceTemplate { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public bool IsRecoveryFocus { get; set; }
    public string RecoveryNote { get; set; } = string.Empty;
    public bool IsSpacedReview { get; set; }
    public string ReviewNote { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public ICollection<LearningBlockFeedback> Feedbacks { get; set; } = new List<LearningBlockFeedback>();
    public ICollection<DailyPlanBlockCompletion> Completions { get; set; } = new List<DailyPlanBlockCompletion>();
}
