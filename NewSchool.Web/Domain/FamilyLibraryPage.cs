namespace NewSchool.Web.Domain;

public class FamilyLibraryPage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MaterialId { get; set; }
    public FamilyLibraryMaterial Material { get; set; } = null!;
    public int PageNumber { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string ImageRelativePath { get; set; } = string.Empty;
}
