namespace NewSchool.Web.Domain;

public class DailyPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public DateTime PlannedDate { get; set; }
    public int AgeAtGeneration { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string ParentSummary { get; set; } = string.Empty;
    public string ChildNarrative { get; set; } = string.Empty;
    public bool IsRecoveryPlan { get; set; }
    public string RecoveryHeadline { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public ICollection<DailyPlanBlock> Blocks { get; set; } = new List<DailyPlanBlock>();
    public ICollection<LearningSession> Sessions { get; set; } = new List<LearningSession>();
    public ICollection<DailyPlanBlockCompletion> TaskCompletions { get; set; } = new List<DailyPlanBlockCompletion>();
}
