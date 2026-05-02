namespace NewSchool.Web.Models;

public class DailyTrailViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FocusAreaLabel { get; set; } = string.Empty;
    public string FocusAreaChipClass { get; set; } = "neutral";
    public string GoalTrackLabel { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public int BlockCount { get; set; }
    public string DailyOutcome { get; set; } = string.Empty;
    public string ExternalGuideLabel { get; set; } = string.Empty;
    public string ExternalGuideUrl { get; set; } = string.Empty;
    public string ExternalGuideReason { get; set; } = string.Empty;
    public bool HasExternalGuide => !string.IsNullOrWhiteSpace(ExternalGuideUrl);
    public List<DailyTrailStageViewModel> Stages { get; set; } = new();
    public List<string> MaterialChecklist { get; set; } = new();
    public List<string> EvidenceChecklist { get; set; } = new();
}

public class DailyTrailStageViewModel
{
    public string OrderLabel { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;
    public string DomainChipClass { get; set; } = "neutral";
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string EvidenceLabel { get; set; } = string.Empty;
}
