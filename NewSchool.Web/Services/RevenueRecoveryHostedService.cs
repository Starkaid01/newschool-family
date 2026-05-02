using Microsoft.Extensions.DependencyInjection;

namespace NewSchool.Web.Services;

public class RevenueRecoveryHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<RevenueRecoveryHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

        await RunPassAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunPassAsync(stoppingToken);
        }
    }

    private async Task RunPassAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var automationService = scope.ServiceProvider.GetRequiredService<EmailAutomationService>();
            var sent = await automationService.RunRecoveryAutomationAsync();
            logger.LogInformation("Revenue recovery automation executed. Emails sent: {Count}", sent);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Revenue recovery automation failed.");
        }
    }
}
