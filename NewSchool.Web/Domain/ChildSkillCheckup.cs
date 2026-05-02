namespace NewSchool.Web.Domain;

public class ChildSkillCheckup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public LearningDomain Domain { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string PromptTitle { get; set; } = string.Empty;
    public string ParentPrompt { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public DateTime? CompletedAt { get; set; }
    public SkillFeedbackLevel? Rating { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int RecalibratedScore { get; set; }
}
