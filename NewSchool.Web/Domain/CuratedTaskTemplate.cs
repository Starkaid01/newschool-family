namespace NewSchool.Web.Domain;

public class CuratedTaskTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public LearningDomain Domain { get; set; } = LearningDomain.Language;
    public FunctionalSupportTrack FunctionalTrack { get; set; } = FunctionalSupportTrack.Base;
    public int AgeMin { get; set; } = 3;
    public int AgeMax { get; set; } = 10;
    public string GoalTrack { get; set; } = "balanced_growth";
    public string MatchKeywords { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildPrompt { get; set; } = string.Empty;
    public string TaskSteps { get; set; } = string.Empty;
    public string MaterialsSummary { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public string ExpectedOutcome { get; set; } = string.Empty;
    public int SuggestedMinutes { get; set; } = 15;
    public Guid? PrimaryResourceId { get; set; }
    public CuratedLearningResource? PrimaryResource { get; set; }
    public string SupportLinkLabel { get; set; } = string.Empty;
    public string SupportLinkUrl { get; set; } = string.Empty;
    public string SupportLinkSource { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
