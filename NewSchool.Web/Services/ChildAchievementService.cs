using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ChildAchievementService(ApplicationDbContext db)
{
    public async Task SyncAchievementsAsync(Guid childId, Guid parentId)
    {
        var child = await db.Children
            .Include(x => x.LearningSessions)
            .Include(x => x.MonthlySnapshots)
            .Include(x => x.MonthlyGoalCycles)
            .ThenInclude(x => x.Items)
            .Include(x => x.RecoveryPlans)
            .ThenInclude(x => x.Days)
            .Include(x => x.Achievements)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        EnsureAchievement(child, "first_session",
            child.LearningSessions.Any(),
            "Primeira jornada registrada",
            "A familia registrou a primeira sessao e deu o primeiro passo com consistencia.");

        EnsureAchievement(child, "evidence_storyteller",
            child.LearningSessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl)) >= 3,
            "Portifolio em movimento",
            "A crianca ja tem tres evidencias registradas para mostrar sua evolucao.");

        EnsureAchievement(child, "monthly_momentum",
            child.MonthlySnapshots.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).FirstOrDefault()?.OverallScore >= 70,
            "Mes de impulso forte",
            "O ciclo mais recente mostrou um ritmo consistente e forte de aprendizagem.");

        EnsureAchievement(child, "goal_guardian",
            child.MonthlyGoalCycles.Any(cycle => cycle.GoalsOnTrack == cycle.TotalGoals && cycle.TotalGoals > 0),
            "Metas em rota",
            "Todas as metas do ciclo ficaram em rota, mostrando clareza e continuidade.");

        EnsureAchievement(child, "comeback_hero",
            child.RecoveryPlans.Any(plan => plan.Status == "completed" && plan.Days.All(day => day.CompletedAt.HasValue)),
            "Retomada concluida",
            "A familia completou um plano de retomada e recuperou o ritmo da crianca.");

        await db.SaveChangesAsync();
    }

    public async Task<List<ChildAchievementViewModel>> BuildAchievementsAsync(Guid childId, Guid parentId, int take = 3)
    {
        var child = await db.Children
            .Include(x => x.Achievements)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        return child.Achievements
            .OrderByDescending(x => x.EarnedAt)
            .Take(take)
            .Select(x => new ChildAchievementViewModel
            {
                Title = x.Title,
                Description = x.Description,
                EarnedAt = x.EarnedAt
            })
            .ToList();
    }

    private static void EnsureAchievement(
        ChildProfile child,
        string code,
        bool condition,
        string title,
        string description)
    {
        if (!condition || child.Achievements.Any(x => x.Code == code))
        {
            return;
        }

        child.Achievements.Add(new ChildAchievement
        {
            ChildId = child.Id,
            Code = code,
            Title = title,
            Description = description,
            EarnedAt = DateTime.UtcNow
        });
    }
}
