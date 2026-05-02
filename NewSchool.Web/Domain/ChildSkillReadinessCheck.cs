namespace NewSchool.Web.Domain;

public class ChildSkillReadinessCheck
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public LearningDomain Domain { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string ParentPrompt { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public string UnlocksSkillCode { get; set; } = string.Empty;
    public string UnlocksSkillName { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public DateTime? CompletedAt { get; set; }
    public SkillFeedbackLevel? Rating { get; set; }
    public bool Passed { get; set; }
    public string Notes { get; set; } = string.Empty;
}
