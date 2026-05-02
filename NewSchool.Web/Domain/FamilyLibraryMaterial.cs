namespace NewSchool.Web.Domain;

public class FamilyLibraryMaterial
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public int RecommendedMinAge { get; set; }
    public int RecommendedMaxAge { get; set; }
    public string SkillFocus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public bool IsPrintable { get; set; }
    public int PageCount { get; set; }
    public bool HasIllustrations { get; set; }
    public string CoverImageRelativePath { get; set; } = string.Empty;
    public string SourceRelativePath { get; set; } = string.Empty;
    public string SourceSyncToken { get; set; } = string.Empty;
    public DateTime SourceUpdatedAtUtc { get; set; }
    public DateTime SyncedAtUtc { get; set; }
    public ICollection<FamilyLibraryPage> Pages { get; set; } = new List<FamilyLibraryPage>();
    public ICollection<FamilyLibraryUserState> UserStates { get; set; } = new List<FamilyLibraryUserState>();
}
