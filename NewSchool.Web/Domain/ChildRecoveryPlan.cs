namespace NewSchool.Web.Domain;

public class ChildRecoveryPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid? GoalCycleId { get; set; }
    public ChildMonthlyGoalCycle? GoalCycle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChildRecoveryPlanDay> Days { get; set; } = new List<ChildRecoveryPlanDay>();
}
