using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class TrackAnalyticsService(ApplicationDbContext db)
{
    public async Task CaptureTrackForChildAsync(Guid userId, Guid childId, string trackCode)
    {
        var normalized = NormalizeTrack(trackCode);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var snapshot = await db.TrackAcquisitionSnapshots
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TrackCode == normalized);

        if (snapshot is null)
        {
            var user = await db.Users.FirstAsync(x => x.Id == userId);
            snapshot = new TrackAcquisitionSnapshot
            {
                UserId = userId,
                ChildId = childId,
                TrackCode = normalized,
                CapturedAt = DateTime.UtcNow,
                HasSubscription = string.Equals(user.SubscriptionStatus, "active", StringComparison.OrdinalIgnoreCase),
                SubscriptionCapturedAt = string.Equals(user.SubscriptionStatus, "active", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null,
                IsRetained = string.Equals(user.SubscriptionStatus, "active", StringComparison.OrdinalIgnoreCase),
                LastRetentionCheckAt = DateTime.UtcNow
            };
            db.TrackAcquisitionSnapshots.Add(snapshot);
        }
        else if (!snapshot.ChildId.HasValue)
        {
            snapshot.ChildId = childId;
            snapshot.LastRetentionCheckAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task SyncSubscriptionAttributionAsync(Guid userId)
    {
        var user = await db.Users.FirstAsync(x => x.Id == userId);
        var snapshots = await db.TrackAcquisitionSnapshots
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var isSubscribed = string.Equals(user.SubscriptionStatus, "active", StringComparison.OrdinalIgnoreCase);
        if (isSubscribed && !user.FirstSubscribedAt.HasValue)
        {
            user.FirstSubscribedAt = DateTime.UtcNow;
        }

        foreach (var snapshot in snapshots)
        {
            snapshot.HasSubscription = isSubscribed;
            snapshot.SubscriptionCapturedAt ??= isSubscribed ? DateTime.UtcNow : null;
            snapshot.IsRetained = isSubscribed && user.FirstSubscribedAt.HasValue && user.FirstSubscribedAt.Value <= DateTime.UtcNow.AddDays(-7);
            snapshot.LastRetentionCheckAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<TrackPerformanceViewModel>> BuildTrackPerformanceAsync()
    {
        var snapshots = await db.TrackAcquisitionSnapshots
            .Include(x => x.User)
            .ToListAsync();

        return snapshots
            .GroupBy(x => x.TrackCode)
            .Select(group =>
            {
                var totalLeads = group.Select(x => x.UserId).Distinct().Count();
                var subscribed = group.Where(x => x.SubscriptionCapturedAt.HasValue).Select(x => x.UserId).Distinct().Count();
                var retained = group.Where(x => x.IsRetained).Select(x => x.UserId).Distinct().Count();

                return new TrackPerformanceViewModel
                {
                    TrackCode = group.Key,
                    TrackLabel = GetTrackLabel(group.Key),
                    Leads = totalLeads,
                    Subscribers = subscribed,
                    RetainedFamilies = retained,
                    ConversionRate = totalLeads == 0 ? 0 : Math.Round((decimal)subscribed / totalLeads * 100m, 1),
                    RetentionRate = subscribed == 0 ? 0 : Math.Round((decimal)retained / subscribed * 100m, 1)
                };
            })
            .OrderByDescending(x => x.ConversionRate)
            .ThenByDescending(x => x.RetentionRate)
            .ToList();
    }

    private static string NormalizeTrack(string? track) => track switch
    {
        "literacy" or "math_foundations" or "autonomy" or "science_discovery" => track,
        _ => string.Empty
    };

    private static string GetTrackLabel(string track) => track switch
    {
        "literacy" => "Alfabetizacao",
        "math_foundations" => "Matematica base",
        "autonomy" => "Autonomia e foco",
        "science_discovery" => "Ciencias em casa",
        _ => "Crescimento equilibrado"
    };
}
