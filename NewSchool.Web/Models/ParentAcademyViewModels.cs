namespace NewSchool.Web.Models;

public class ParentAcademyViewModel
{
    public string ParentName { get; set; } = string.Empty;
    public Guid? SelectedChildId { get; set; }
    public string SelectedChildName { get; set; } = string.Empty;
    public string PersonalizationNote { get; set; } = string.Empty;
    public List<ParentAcademyChildOptionViewModel> Children { get; set; } = new();
    public List<ParentAcademyQuickStartCardViewModel> QuickStartCards { get; set; } = new();
    public List<SystemCurriculumTrackViewModel> SystemCurriculumTracks { get; set; } = new();
    public string HostedLibraryNote { get; set; } = string.Empty;
    public List<ParentAcademyResourceViewModel> HostedLibrary { get; set; } = new();
    public List<ParentAcademyCategoryViewModel> Categories { get; set; } = new();
}

public class ParentAcademyQuickStartCardViewModel
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string SecondaryActionLabel { get; set; } = string.Empty;
    public string SecondaryActionUrl { get; set; } = string.Empty;
}

public class ParentAcademyChildOptionViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class ParentAcademyCategoryViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParentAcademyResourceViewModel> Resources { get; set; } = new();
}

public class ParentAcademyResourceViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string WhyItMatters { get; set; } = string.Empty;
    public string PortugueseGuideNote { get; set; } = string.Empty;
    public string FormatLabel { get; set; } = string.Empty;
    public string DurationLabel { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public string OwnershipLabel { get; set; } = "Conteúdo NewSchool";
    public bool IsThirdParty { get; set; }
    public string GuideLabel { get; set; } = string.Empty;
    public string GuideUrl { get; set; } = string.Empty;
    public bool HasGuide => !string.IsNullOrWhiteSpace(GuideUrl);
    public string AccessLabel { get; set; } = "Abrir recurso";
    public string Url { get; set; } = string.Empty;
    public bool HasUrl => !string.IsNullOrWhiteSpace(Url);
    public string LicenseLabel { get; set; } = string.Empty;
    public string Attribution { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string HostingLabel { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
    public string AudienceLabel { get; set; } = string.Empty;
    public string AudienceChipClass { get; set; } = "neutral";
}
