namespace NewSchool.Web.Models;

public class NotificationHeaderViewModel
{
    public int UnreadCount { get; set; }
}

public class UserNotificationListItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationLevel { get; set; } = "info";
    public string ActionUrl { get; set; } = string.Empty;
    public string RelativeTimeLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsUnread => !ReadAt.HasValue;
    public bool HasAction => !string.IsNullOrWhiteSpace(ActionUrl);
}

public class ParentNotificationsViewModel
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string BackUrl { get; set; } = string.Empty;
    public List<UserNotificationListItemViewModel> Notifications { get; set; } = new();
}

public class SendUserNotificationInput
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationLevel { get; set; } = "info";
    public string ActionUrl { get; set; } = string.Empty;
}
