namespace NewSchool.Web.Domain;

public class ChildRecoveryPlanDay
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecoveryPlanId { get; set; }
    public ChildRecoveryPlan RecoveryPlan { get; set; } = null!;
    public int DayNumber { get; set; }
    public DateTime SuggestedDate { get; set; }
    public string FocusSkill { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public string ParentTip { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}
