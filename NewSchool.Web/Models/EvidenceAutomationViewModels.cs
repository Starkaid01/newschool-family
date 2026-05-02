namespace NewSchool.Web.Models;

public class EvidenceAutomationViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusChipClass { get; set; } = "neutral";
    public string ProofTargetLabel { get; set; } = string.Empty;
    public string PreferredCaptureLabel { get; set; } = string.Empty;
    public string QuickTextPrompt { get; set; } = string.Empty;
    public string FileHint { get; set; } = string.Empty;
    public string WinsPlaceholder { get; set; } = string.Empty;
    public string ChallengesPlaceholder { get; set; } = string.Empty;
    public string NotesPlaceholder { get; set; } = string.Empty;
    public string SaveActionLabel { get; set; } = string.Empty;
    public string SaveActionUrl { get; set; } = string.Empty;
    public List<EvidenceCaptureSuggestionViewModel> CaptureIdeas { get; set; } = new();
    public List<string> ChecklistItems { get; set; } = new();
    public bool HasSaveAction => !string.IsNullOrWhiteSpace(SaveActionUrl);
    public bool HasContent => !string.IsNullOrWhiteSpace(Headline);
}

public class EvidenceCaptureSuggestionViewModel
{
    public string TypeLabel { get; set; } = string.Empty;
    public string TypeChipClass { get; set; } = "neutral";
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string WhyItWorks { get; set; } = string.Empty;
}
