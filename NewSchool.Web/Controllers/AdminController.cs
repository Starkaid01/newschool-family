using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using NewSchool.Web.Services;

namespace NewSchool.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext db,
    ChildGoalCycleService childGoalCycleService,
    TrackAnalyticsService trackAnalyticsService,
    EmailAutomationService emailAutomationService) : Controller
{
    public async Task<IActionResult> Index(string? subscription, string? activity, string? risk)
    {
        var weekStart = DateTime.Today.AddDays(-7);
        var childOwnership = await db.Children
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync();

        foreach (var child in childOwnership)
        {
            await childGoalCycleService.SyncCurrentCycleAsync(child.Id, child.ParentId);
        }

        var parents = await db.Users
            .Where(x => x.Role == UserRole.Parent)
            .Include(x => x.Children)
            .ThenInclude(c => c.LearningSessions)
            .Include(x => x.Children)
            .ThenInclude(c => c.MonthlyGoalCycles)
            .ThenInclude(cycle => cycle.Items)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        var parentRows = parents.Select(parent =>
        {
            var lastSessionAt = parent.Children
                .SelectMany(c => c.LearningSessions)
                .OrderByDescending(s => s.LoggedAt)
                .Select(s => (DateTime?)s.LoggedAt)
                .FirstOrDefault();

            var activityLabel = lastSessionAt.HasValue && lastSessionAt.Value.Date >= weekStart
                ? "Ativa nesta semana"
                : lastSessionAt.HasValue
                    ? "Sem atividade recente"
                    : "Sem uso";

            var activityChipClass = lastSessionAt.HasValue && lastSessionAt.Value.Date >= weekStart
                ? "success"
                : "warning";

            var currentGoalCycles = parent.Children
                .SelectMany(c => c.MonthlyGoalCycles)
                .Where(cycle => cycle.Year == DateTime.Today.Year && cycle.Month == DateTime.Today.Month)
                .ToList();

            var pedagogicalRiskLabel = currentGoalCycles.Any(x => x.RiskLevel == "high")
                ? "Metas em risco"
                : currentGoalCycles.Any(x => x.RiskLevel == "medium")
                    ? "Acompanhamento"
                    : "Em rota";

            var pedagogicalRiskChipClass = pedagogicalRiskLabel switch
            {
                "Metas em risco" => "warning",
                "Acompanhamento" => "neutral",
                _ => "success"
            };

            var offTrackGoals = currentGoalCycles
                .SelectMany(x => x.Items)
                .Count(x => x.Status == "at_risk");

            var riskLabel = parent.SubscriptionStatus switch
            {
                "canceled" => "Cancelada",
                "trial_expired" => "Trial expirado",
                "inactive" when parent.TrialEndsAt.HasValue && parent.TrialEndsAt.Value.Date <= DateTime.Today.AddDays(2) => "Trial acabando",
                _ when !lastSessionAt.HasValue || lastSessionAt.Value.Date < weekStart => "Risco alto",
                _ => "Risco baixo"
            };

            var riskChipClass = riskLabel is "Risco baixo"
                ? "success"
                : riskLabel is "Risco alto" or "Cancelada" or "Trial expirado"
                    ? "warning"
                    : "neutral";

            return new UserOverviewViewModel
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Email = parent.Email,
                SubscriptionStatus = parent.SubscriptionStatus,
                AcquisitionTrackLabel = GetTrackLabel(parent.AcquisitionTrack),
                ActivityLabel = activityLabel,
                ActivityChipClass = activityChipClass,
                ChurnRiskLabel = riskLabel,
                ChurnRiskChipClass = riskChipClass,
                PedagogicalRiskLabel = pedagogicalRiskLabel,
                PedagogicalRiskChipClass = pedagogicalRiskChipClass,
                OffTrackGoals = offTrackGoals,
                ChildrenCount = parent.Children.Count,
                SessionsCount = parent.Children.SelectMany(c => c.LearningSessions).Count(),
                MinutesCompleted = parent.Children.SelectMany(c => c.LearningSessions).Sum(s => s.MinutesCompleted)
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(subscription))
        {
            parentRows = parentRows.Where(x => x.SubscriptionStatus == subscription).ToList();
        }

        if (!string.IsNullOrWhiteSpace(activity))
        {
            parentRows = activity switch
            {
                "active" => parentRows.Where(x => x.ActivityLabel == "Ativa nesta semana").ToList(),
                "inactive" => parentRows.Where(x => x.ActivityLabel != "Ativa nesta semana").ToList(),
                _ => parentRows
            };
        }

        if (!string.IsNullOrWhiteSpace(risk))
        {
            parentRows = risk switch
            {
                "low" => parentRows.Where(x => x.ChurnRiskLabel == "Risco baixo").ToList(),
                "high" => parentRows.Where(x => x.ChurnRiskLabel != "Risco baixo").ToList(),
                _ => parentRows
            };
        }

        var totalParents = parents.Count;
        var activeSubscribers = parents.Count(x => x.SubscriptionStatus == "active");
        var canceledParents = parents.Count(x => x.SubscriptionStatus == "canceled");
        var trialsEndingSoon = parents.Count(x => x.SubscriptionStatus != "active" && x.TrialEndsAt.HasValue && x.TrialEndsAt.Value.Date <= DateTime.Today.AddDays(2));
        var pastDueParents = parents.Count(x => x.SubscriptionStatus is "past_due" or "unpaid");
        var highRiskParents = parentRows.Count(x => x.ChurnRiskLabel != "Risco baixo");
        var pedagogicalRiskParents = parentRows.Count(x => x.PedagogicalRiskLabel != "Em rota");
        var offTrackGoalCycles = parents
            .SelectMany(x => x.Children)
            .SelectMany(x => x.MonthlyGoalCycles)
            .Count(x => x.Year == DateTime.Today.Year && x.Month == DateTime.Today.Month && x.RiskLevel != "low");

        var vm = new AdminDashboardViewModel
        {
            TotalParents = totalParents,
            TotalChildren = parents.Sum(x => x.Children.Count),
            ActiveSubscribers = activeSubscribers,
            InactiveParents = parents.Count(x => x.SubscriptionStatus == "inactive"),
            CanceledParents = canceledParents,
            EstimatedMrr = activeSubscribers * 30m,
            ConversionRate = totalParents == 0 ? 0 : Math.Round((decimal)activeSubscribers / totalParents * 100m, 1),
            RetentionRate = totalParents == 0 ? 0 : Math.Round((decimal)(totalParents - canceledParents) / totalParents * 100m, 1),
            ChurnRate = totalParents == 0 ? 0 : Math.Round((decimal)canceledParents / totalParents * 100m, 1),
            NewParentsThisWeek = parents.Count(x => x.CreatedAt.Date >= weekStart),
            TrialsEndingSoon = trialsEndingSoon,
            PastDueParents = pastDueParents,
            HighRiskParents = highRiskParents,
            PedagogicalRiskParents = pedagogicalRiskParents,
            OffTrackGoalCycles = offTrackGoalCycles,
            PlansGeneratedToday = await db.DailyPlans.CountAsync(x => x.PlannedDate == DateTime.Today),
            SessionsLoggedThisWeek = await db.LearningSessions.CountAsync(x => x.LoggedAt >= weekStart),
            MinutesDeliveredThisWeek = await db.LearningSessions.Where(x => x.LoggedAt >= weekStart).SumAsync(x => (int?)x.MinutesCompleted) ?? 0,
            TrackPerformance = await trackAnalyticsService.BuildTrackPerformanceAsync(),
            SelectedSubscription = subscription ?? string.Empty,
            SelectedActivity = activity ?? string.Empty,
            SelectedRisk = risk ?? string.Empty,
            Parents = parentRows
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunRecoveryAutomation()
    {
        var sent = await emailAutomationService.RunRecoveryAutomationAsync();
        TempData["AutomationStatus"] = $"Automacao de recuperacao executada. Emails enviados nesta passada: {sent}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCampaign(Guid userId, EmailCampaignType campaign)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == UserRole.Parent);
            if (user is null)
            {
                TempData["AutomationStatus"] = "Nao foi possivel localizar a familia para envio manual.";
                return RedirectToAction(nameof(Index));
            }

            await emailAutomationService.SendCampaignAsync(user, campaign, force: true);
            TempData["AutomationStatus"] = $"Campanha {campaign} enviada para {user.Email}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["AutomationStatus"] = $"Nao foi possivel enviar a campanha {campaign}. Erro: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private static string GetTrackLabel(string track) => track switch
    {
        "literacy" => "Alfabetizacao",
        "math_foundations" => "Matematica base",
        "autonomy" => "Autonomia e foco",
        "science_discovery" => "Ciencias em casa",
        _ => "Nao atribuido"
    };
}
