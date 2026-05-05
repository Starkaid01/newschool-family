namespace NewSchool.Web.Models;

public class CuratedTaskSuggestionViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string CurriculumSubjectLabel { get; set; } = string.Empty;
    public string CurriculumUnitLabel { get; set; } = string.Empty;
    public string CurriculumUnitTitle { get; set; } = string.Empty;
    public string CurriculumLessonTitle { get; set; } = string.Empty;
    public int CurriculumLessonNumber { get; set; }
    public int CurriculumLessonCount { get; set; }
    public string FitReason { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string MaterialsSummary { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public string ExpectedOutcome { get; set; } = string.Empty;
    public int SuggestedMinutes { get; set; }
    public List<string> Steps { get; set; } = new();
    public List<string> FocusTokens { get; set; } = new();
    public ProprietaryLessonPacketViewModel? LessonPacket { get; set; }
    public CuratedResourceCardViewModel? PrimaryResource { get; set; }
    public CuratedSupportLinkViewModel? SupportLink { get; set; }
}

public class CuratedResourceCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FormatLabel { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
    public string AccessLabel { get; set; } = string.Empty;
    public string LicenseLabel { get; set; } = string.Empty;
    public string Attribution { get; set; } = string.Empty;
    public string UseNote { get; set; } = string.Empty;
    public bool IsHostedLocally { get; set; }
    public string LanguageCode { get; set; } = "pt-BR";
    public bool FamilyFacing { get; set; } = true;
}

public class CuratedSupportLinkViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
}
