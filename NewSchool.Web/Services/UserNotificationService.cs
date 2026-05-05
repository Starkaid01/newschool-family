using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class UserNotificationService(ApplicationDbContext db)
{
    public async Task<NotificationHeaderViewModel> BuildHeaderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadCount = await db.UserNotifications
            .AsNoTracking()
            .CountAsync(item => item.UserId == userId && item.ReadAt == null, cancellationToken);

        return new NotificationHeaderViewModel
        {
            UnreadCount = unreadCount
        };
    }

    public async Task<ParentNotificationsViewModel> BuildCenterAsync(Guid userId, string backUrl, CancellationToken cancellationToken = default)
    {
        var notifications = await db.UserNotifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(80)
            .Select(item => new UserNotificationListItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Message = item.Message,
                NotificationLevel = item.NotificationLevel,
                ActionUrl = item.ActionUrl,
                CreatedAt = item.CreatedAt,
                ReadAt = item.ReadAt
            })
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.RelativeTimeLabel = BuildRelativeTimeLabel(notification.CreatedAt);
        }

        return new ParentNotificationsViewModel
        {
            UnreadCount = notifications.Count(item => item.IsUnread),
            TotalCount = notifications.Count,
            BackUrl = backUrl,
            Notifications = notifications
        };
    }

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        string level = "info",
        string? actionUrl = null,
        Guid? sentByAdminId = null,
        bool isSystemGenerated = true,
        CancellationToken cancellationToken = default)
    {
        db.UserNotifications.Add(new UserNotification
        {
            UserId = userId,
            Title = (title ?? string.Empty).Trim(),
            Message = (message ?? string.Empty).Trim(),
            NotificationLevel = NormalizeLevel(level),
            ActionUrl = actionUrl?.Trim() ?? string.Empty,
            SentByAdminId = sentByAdminId,
            IsSystemGenerated = isSystemGenerated,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await db.UserNotifications
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId, cancellationToken);

        if (notification is null || notification.ReadAt.HasValue)
        {
            return;
        }

        notification.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await db.UserNotifications
            .Where(item => item.UserId == userId && item.ReadAt == null)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.ReadAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.UserNotifications
            .AsNoTracking()
            .CountAsync(item => item.UserId == userId && item.ReadAt == null, cancellationToken);
    }

    private static string NormalizeLevel(string? level) => level?.Trim().ToLowerInvariant() switch
    {
        "success" => "success",
        "warning" => "warning",
        "danger" => "danger",
        _ => "info"
    };

    private static string BuildRelativeTimeLabel(DateTime createdAtUtc)
    {
        var elapsed = DateTime.UtcNow - createdAtUtc;
        if (elapsed.TotalMinutes < 1)
        {
            return "agora";
        }

        if (elapsed.TotalMinutes < 60)
        {
            return $"{Math.Max(1, (int)Math.Floor(elapsed.TotalMinutes))} min atrás";
        }

        if (elapsed.TotalHours < 24)
        {
            return $"{Math.Max(1, (int)Math.Floor(elapsed.TotalHours))} h atrás";
        }

        if (elapsed.TotalDays < 7)
        {
            return $"{Math.Max(1, (int)Math.Floor(elapsed.TotalDays))} dia(s) atrás";
        }

        return createdAtUtc.ToLocalTime().ToString("dd/MM/yyyy");
    }
}
