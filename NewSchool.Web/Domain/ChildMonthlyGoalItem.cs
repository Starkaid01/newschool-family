namespace NewSchool.Web.Domain;

public class ChildMonthlyGoalItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CycleId { get; set; }
    public ChildMonthlyGoalCycle Cycle { get; set; } = null!;
    public LearningDomain Domain { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int StartScore { get; set; }
    public int CurrentScore { get; set; }
    public int TargetScore { get; set; }
    public int PriorityOrder { get; set; }
    public string Status { get; set; } = "at_risk";
}
