namespace NewSchool.Web.Domain;

public class InterventionPlaybookEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public LearningDomain Domain { get; set; }
    public string GoalTrack { get; set; } = string.Empty;
    public string TriggerCode { get; set; } = string.Empty;
    public string TriggerLabel { get; set; } = string.Empty;
    public string MatchKeywords { get; set; } = string.Empty;
    public string StageScope { get; set; } = "all";
    public string Headline { get; set; } = string.Empty;
    public string HowToSpot { get; set; } = string.Empty;
    public string LikelyCause { get; set; } = string.Empty;
    public string WhatToSay { get; set; } = string.Empty;
    public string WhatToAvoid { get; set; } = string.Empty;
    public string QuickActivity { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string SuccessSignal { get; set; } = string.Empty;
    public string RepeatPlan { get; set; } = string.Empty;
    public string FallbackAction { get; set; } = string.Empty;
}
