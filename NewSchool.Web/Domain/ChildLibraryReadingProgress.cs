namespace NewSchool.Web.Domain;

public class ChildLibraryReadingProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid ParentUserId { get; set; }
    public AppUser ParentUser { get; set; } = null!;
    public Guid MaterialId { get; set; }
    public FamilyLibraryMaterial Material { get; set; } = null!;
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public string CompletionKind { get; set; } = "weekly_reading";
    public string GoalLabel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
