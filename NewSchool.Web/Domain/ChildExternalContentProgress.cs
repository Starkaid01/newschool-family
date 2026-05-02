namespace NewSchool.Web.Domain;

public class ChildExternalContentProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public string ContentSlug { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
