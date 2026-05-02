namespace NewSchool.Web.Domain;

public class ChildDevelopmentProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public int LanguageLevel { get; set; } = 3;
    public int MathLevel { get; set; } = 3;
    public int WorldLevel { get; set; } = 3;
    public int ExecutiveFunctionLevel { get; set; } = 3;
    public string StrengthsSummary { get; set; } = string.Empty;
    public string SupportSummary { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}
