namespace NewSchool.Web.Models;

public class ChildDossierViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public string TeaProfileUrl { get; set; } = string.Empty;
    public string AdaptiveRoutineUrl { get; set; } = string.Empty;
    public string ParentPrimaryGoal { get; set; } = string.Empty;
    public string SchoolBarrierSummary { get; set; } = string.Empty;
    public string DocumentationSummary { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int TotalMinutes { get; set; }
    public int EvidenceCount { get; set; }
    public int ObservationCount { get; set; }
    public AdaptiveRoutineSnapshotViewModel Snapshot { get; set; } = new();
    public List<SkillProgressViewModel> PrioritySkills { get; set; } = new();
    public List<DossierEvidenceViewModel> EvidenceTimeline { get; set; } = new();
    public List<string> RecommendedDocuments { get; set; } = new();
    public string WeeklyImproved { get; set; } = string.Empty;
    public string WeeklyReview { get; set; } = string.Empty;
    public string WeeklyAdvance { get; set; } = string.Empty;
}

public class DossierEvidenceViewModel
{
    public DateTime LoggedAt { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);
    public bool IsVideo => MediaContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsImage => MediaContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
