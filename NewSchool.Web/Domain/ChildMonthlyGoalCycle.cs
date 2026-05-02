namespace NewSchool.Web.Domain;

public class ChildMonthlyGoalCycle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public string GoalHeadline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "low";
    public int ProgressPercent { get; set; }
    public int GoalsOnTrack { get; set; }
    public int TotalGoals { get; set; }
    public DateTime? LastSessionAt { get; set; }
    public DateTime? RetentionAlertSentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChildMonthlyGoalItem> Items { get; set; } = new List<ChildMonthlyGoalItem>();
}
