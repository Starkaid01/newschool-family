namespace NewSchool.Web.Domain;

public class LearningSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid DailyPlanId { get; set; }
    public DailyPlan DailyPlan { get; set; } = null!;
    public DateTime LoggedAt { get; set; }
    public int MinutesCompleted { get; set; }
    public string Wins { get; set; } = string.Empty;
    public string Challenges { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string MediaFileName { get; set; } = string.Empty;
    public string MediaStorageProvider { get; set; } = string.Empty;
    public string MediaStorageKey { get; set; } = string.Empty;
    public string MediaThumbnailUrl { get; set; } = string.Empty;
    public string MediaThumbnailStorageKey { get; set; } = string.Empty;
    public ICollection<LearningBlockFeedback> BlockFeedbacks { get; set; } = new List<LearningBlockFeedback>();
    public ICollection<ChildRoutineObservation> RoutineObservations { get; set; } = new List<ChildRoutineObservation>();
}
