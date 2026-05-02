namespace NewSchool.Web.Domain;

public class CuratedLearningResource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string UseNote { get; set; } = string.Empty;
    public LearningDomain Domain { get; set; } = LearningDomain.Language;
    public int AgeMin { get; set; } = 3;
    public int AgeMax { get; set; } = 10;
    public string FormatLabel { get; set; } = string.Empty;
    public string ResourceKind { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
    public bool IsHostedLocally { get; set; }
    public string LicenseLabel { get; set; } = string.Empty;
    public string Attribution { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "pt-BR";
    public int SortOrder { get; set; }
    public ICollection<CuratedTaskTemplate> PrimaryTasks { get; set; } = new List<CuratedTaskTemplate>();
}
