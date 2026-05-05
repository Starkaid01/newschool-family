using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;

namespace NewSchool.Web.Services;

public sealed class DailyPlanWarmupQueue(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyPlanWarmupQueue> logger) : BackgroundService
{
    private readonly Channel<DailyPlanWarmupRequest> requests = Channel.CreateUnbounded<DailyPlanWarmupRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    private readonly ConcurrentDictionary<string, byte> pendingKeys = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask QueueAsync(Guid childId, DateTime plannedDate, CancellationToken cancellationToken = default)
    {
        if (childId == Guid.Empty)
        {
            return ValueTask.CompletedTask;
        }

        var request = new DailyPlanWarmupRequest(childId, plannedDate.Date);
        if (!pendingKeys.TryAdd(request.Key, 0))
        {
            return ValueTask.CompletedTask;
        }

        return requests.Writer.WriteAsync(request, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in requests.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var learningPlanService = scope.ServiceProvider.GetRequiredService<LearningPlanService>();

                var child = await db.Children
                    .FirstOrDefaultAsync(item => item.Id == request.ChildId, stoppingToken);

                if (child is null)
                {
                    continue;
                }

                var existingPlan = await db.DailyPlans
                    .Include(item => item.Blocks)
                    .FirstOrDefaultAsync(
                        item => item.ChildId == request.ChildId && item.PlannedDate == request.PlannedDate,
                        stoppingToken);

                if (existingPlan is not null && existingPlan.Blocks.Count > 0)
                {
                    continue;
                }

                await learningPlanService.EnsurePlanAsync(child, request.PlannedDate);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Falha ao preparar plano em segundo plano para a criança {ChildId} em {PlannedDate:yyyy-MM-dd}.",
                    request.ChildId,
                    request.PlannedDate);
            }
            finally
            {
                pendingKeys.TryRemove(request.Key, out _);
            }
        }
    }

    private readonly record struct DailyPlanWarmupRequest(Guid ChildId, DateTime PlannedDate)
    {
        public string Key => $"{ChildId:N}:{PlannedDate:yyyyMMdd}";
    }
}
