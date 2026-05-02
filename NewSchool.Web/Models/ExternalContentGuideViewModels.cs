namespace NewSchool.Web.Models;

public class ExternalContentGuideViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string AudienceLabel { get; set; } = string.Empty;
    public string AudienceChipClass { get; set; } = "neutral";
    public string SourceLabel { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AdaptedHeadline { get; set; } = string.Empty;
    public string Intro { get; set; } = string.Empty;
    public List<string> ParentSteps { get; set; } = new();
    public List<string> WeeklyRhythm { get; set; } = new();
    public List<string> EvidenceIdeas { get; set; } = new();
    public List<string> FolderHighlights { get; set; } = new();
    public List<string> StarterMaterials { get; set; } = new();
    public string SelectedFocusSlug { get; set; } = string.Empty;
    public string SelectedFocusTitle { get; set; } = string.Empty;
    public string SelectedFocusSummary { get; set; } = string.Empty;
    public string SelectedFocusWhenToUse { get; set; } = string.Empty;
    public string SelectedFocusHowItEnters { get; set; } = string.Empty;
    public string SelectedFocusEvidenceIdea { get; set; } = string.Empty;
    public string SelectedFocusOfficialUrl { get; set; } = string.Empty;
    public string SelectedFocusActionLabel { get; set; } = string.Empty;
    public List<ExternalContentFocusLinkViewModel> FocusLinks { get; set; } = new();
    public Guid? SelectedChildId { get; set; }
    public string SelectedChildName { get; set; } = string.Empty;
    public List<ParentAcademyChildOptionViewModel> Children { get; set; } = new();
    public bool IsCompletedForSelectedChild { get; set; }
    public DateTime? CompletedAt { get; set; }
    public EvidenceAutomationViewModel CompletionEvidenceAssistant { get; set; } = new();
    public bool HasSelectedFocus => !string.IsNullOrWhiteSpace(SelectedFocusTitle);
}

public class ExternalContentFocusLinkViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ExternalContentProgressCardViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string RecommendedReason { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public string AudienceLabel { get; set; } = string.Empty;
    public string AudienceChipClass { get; set; } = "neutral";
    public bool Completed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string GuideUrl { get; set; } = string.Empty;
    public string OfficialUrl { get; set; } = string.Empty;
    public string OfficialActionLabel { get; set; } = string.Empty;
    public string CompletionLabel { get; set; } = string.Empty;
}
