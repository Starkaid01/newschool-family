namespace NewSchool.Web.Domain;

public class ChildMonthlySnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public int SessionsCount { get; set; }
    public int MinutesCount { get; set; }
    public int EvidenceCount { get; set; }
    public int LanguageScore { get; set; }
    public int MathScore { get; set; }
    public int WorldScore { get; set; }
    public int ExecutiveFunctionScore { get; set; }
    public int OverallScore { get; set; }
    public string StrongestArea { get; set; } = string.Empty;
    public string AttentionArea { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime SnapshotMonth { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
