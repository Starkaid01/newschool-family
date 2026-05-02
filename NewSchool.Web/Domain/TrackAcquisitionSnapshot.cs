namespace NewSchool.Web.Domain;

public class TrackAcquisitionSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrackCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid? ChildId { get; set; }
    public ChildProfile? Child { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public bool HasSubscription { get; set; }
    public DateTime? SubscriptionCapturedAt { get; set; }
    public bool IsRetained { get; set; }
    public DateTime? LastRetentionCheckAt { get; set; }
}
