namespace NewSchool.Web.Domain;

public class DailyPlanBlockCompletion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid DailyPlanId { get; set; }
    public DailyPlan DailyPlan { get; set; } = null!;
    public Guid DailyPlanBlockId { get; set; }
    public DailyPlanBlock DailyPlanBlock { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CompletionSource { get; set; } = "guided_flow";
}
