namespace NewSchool.Web.Domain;

public class LearningBlockFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public LearningSession Session { get; set; } = null!;
    public Guid DailyPlanBlockId { get; set; }
    public DailyPlanBlock DailyPlanBlock { get; set; } = null!;
    public string SkillCode { get; set; } = string.Empty;
    public SkillFeedbackLevel Rating { get; set; }
    public string Notes { get; set; } = string.Empty;
}
