using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ChildRecoveryPlanService(ApplicationDbContext db)
{
    public async Task SyncRecoveryPlanAsync(Guid childId, Guid parentId)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.MonthlyGoalCycles)
            .ThenInclude(x => x.Items)
            .Include(x => x.RecoveryPlans)
            .ThenInclude(x => x.Days)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var currentCycle = child.MonthlyGoalCycles
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .FirstOrDefault(x => x.Year == today.Year && x.Month == today.Month);

        if (currentCycle is null)
        {
            return;
        }

        var activePlan = child.RecoveryPlans
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == "active");

        if (currentCycle.RiskLevel == "high")
        {
            if (activePlan is null)
            {
                activePlan = BuildPlan(child, currentCycle, today);
                db.ChildRecoveryPlans.Add(activePlan);
            }
            else
            {
                RefreshPlan(activePlan, currentCycle);
            }
        }
        else if (activePlan is not null && activePlan.Days.All(x => x.CompletedAt.HasValue))
        {
            activePlan.Status = "completed";
            activePlan.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task AdvanceRecoveryPlanAsync(Guid childId, Guid parentId)
    {
        var child = await db.Children
            .Include(x => x.RecoveryPlans)
            .ThenInclude(x => x.Days)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var activePlan = child.RecoveryPlans
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == "active");

        if (activePlan is null)
        {
            return;
        }

        var nextDay = activePlan.Days
            .OrderBy(x => x.DayNumber)
            .FirstOrDefault(x => !x.CompletedAt.HasValue);

        if (nextDay is null)
        {
            activePlan.Status = "completed";
            activePlan.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return;
        }

        nextDay.CompletedAt = DateTime.UtcNow;
        activePlan.UpdatedAt = DateTime.UtcNow;

        if (activePlan.Days.All(x => x.CompletedAt.HasValue))
        {
            activePlan.Status = "completed";
        }

        await db.SaveChangesAsync();
    }

    public async Task<RecoveryPlanCardViewModel?> BuildRecoveryPlanCardAsync(Guid childId, Guid parentId, IUrlHelper url)
    {
        var child = await db.Children
            .Include(x => x.RecoveryPlans)
            .ThenInclude(x => x.Days)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var activePlan = child.RecoveryPlans
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == "active");

        if (activePlan is null)
        {
            return null;
        }

        return new RecoveryPlanCardViewModel
        {
            ChildId = child.Id,
            ChildName = child.FullName,
            Title = activePlan.Title,
            Summary = activePlan.Summary,
            CompletedDays = activePlan.Days.Count(x => x.CompletedAt.HasValue),
            TotalDays = activePlan.Days.Count,
            Url = url.Action("RecoveryPlan", "Parent", new { id = child.Id }) ?? string.Empty
        };
    }

    public async Task<RecoveryPlanDetailsViewModel?> BuildRecoveryPlanDetailsAsync(Guid childId, Guid parentId, IUrlHelper url)
    {
        var child = await db.Children
            .Include(x => x.RecoveryPlans)
            .ThenInclude(x => x.Days)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var activePlan = child.RecoveryPlans
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == "active");

        if (activePlan is null)
        {
            return null;
        }

        return new RecoveryPlanDetailsViewModel
        {
            ChildId = child.Id,
            ChildName = child.FullName,
            Title = activePlan.Title,
            Summary = activePlan.Summary,
            EvolutionUrl = url.Action("Evolution", "Parent", new { id = child.Id }) ?? string.Empty,
            CompletedDays = activePlan.Days.Count(x => x.CompletedAt.HasValue),
            TotalDays = activePlan.Days.Count,
            Days = activePlan.Days
                .OrderBy(x => x.DayNumber)
                .Select(day => new RecoveryPlanDayViewModel
                {
                    DayNumber = day.DayNumber,
                    SuggestedDate = day.SuggestedDate,
                    FocusSkill = day.FocusSkill,
                    GoalText = day.GoalText,
                    ParentTip = day.ParentTip,
                    Completed = day.CompletedAt.HasValue
                })
                .ToList()
        };
    }

    private static ChildRecoveryPlan BuildPlan(ChildProfile child, ChildMonthlyGoalCycle cycle, DateTime today)
    {
        var plan = new ChildRecoveryPlan
        {
            ChildId = child.Id,
            GoalCycleId = cycle.Id,
            Title = $"Plano de retomada de 7 dias para {child.FullName}",
            Summary = $"Ainda da tempo de salvar o mes. Este plano foca nas metas em risco com vitorias curtas e ritmo diario.",
            Status = "active",
            StartDate = today,
            EndDate = today.AddDays(6),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var riskyItems = cycle.Items
            .OrderBy(x => x.Status == "completed" ? 1 : 0)
            .ThenBy(x => x.PriorityOrder)
            .Take(3)
            .ToList();

        for (var day = 1; day <= 7; day++)
        {
            var item = riskyItems[(day - 1) % Math.Max(riskyItems.Count, 1)];
            plan.Days.Add(new ChildRecoveryPlanDay
            {
                DayNumber = day,
                SuggestedDate = today.AddDays(day - 1),
                FocusSkill = item.SkillName,
                GoalText = $"Retome {item.SkillName.ToLowerInvariant()} com uma atividade curta, concreta e bem guiada.",
                ParentTip = $"Mantenha a sessao simples, celebre pequenas vitórias e registre a evidencia ao final para mostrar retomada real."
            });
        }

        return plan;
    }

    private static void RefreshPlan(ChildRecoveryPlan plan, ChildMonthlyGoalCycle cycle)
    {
        plan.GoalCycleId = cycle.Id;
        plan.Summary = $"Ainda da tempo de salvar o mes. O foco continua em recuperar consistencia nas metas prioritarias e voltar ao ritmo.";
        plan.UpdatedAt = DateTime.UtcNow;
    }
}
