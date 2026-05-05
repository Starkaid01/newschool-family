namespace NewSchool.Web.Domain;

public class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid? SentByAdminId { get; set; }
    public AppUser? SentByAdmin { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationLevel { get; set; } = "info";
    public string ActionUrl { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
