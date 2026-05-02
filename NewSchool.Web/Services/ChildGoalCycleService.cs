using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class ChildGoalCycleService(ApplicationDbContext db)
{
    public async Task SyncCurrentCycleAsync(Guid childId, Guid parentId)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.DevelopmentProfile)
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.LearningSessions)
            .Include(x => x.MonthlyGoalCycles)
            .ThenInclude(x => x.Items)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var age = Math.Clamp(CalculateAge(child.BirthDate, today), 3, 14);
        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var cycle = child.MonthlyGoalCycles.FirstOrDefault(x => x.Year == today.Year && x.Month == today.Month);
        if (cycle is null)
        {
            cycle = new ChildMonthlyGoalCycle
            {
                ChildId = child.Id,
                Year = today.Year,
                Month = today.Month,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in BuildInitialItems(child, templates))
            {
                cycle.Items.Add(item);
            }

            db.ChildMonthlyGoalCycles.Add(cycle);
        }

        UpdateCycle(child, cycle, today);
        cycle.GoalHeadline = BuildGoalHeadline(child, cycle);
        await db.SaveChangesAsync();
    }

    private static List<ChildMonthlyGoalItem> BuildInitialItems(ChildProfile child, IReadOnlyList<CurriculumTemplate> templates)
    {
        var preferredTrack = child.FamilyGoalTrack;
        var progressCandidates = child.SkillProgressEntries
            .OrderByDescending(x => GetGoalTrackAffinity(preferredTrack, x.Domain))
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .Take(5)
            .ToList();

        var items = progressCandidates
            .Select((progress, index) => new ChildMonthlyGoalItem
            {
                Domain = progress.Domain,
                SkillCode = progress.SkillCode,
                SkillName = progress.SkillName,
                StartScore = progress.MasteryScore,
                CurrentScore = progress.MasteryScore,
                TargetScore = Math.Clamp(progress.MasteryScore < 50 ? 65 : progress.MasteryScore + 15, 60, 90),
                PriorityOrder = index + 1
            })
            .ToList();

        items = items
            .OrderByDescending(x => GetGoalTrackAffinity(preferredTrack, x.Domain))
            .ThenBy(x => x.CurrentScore)
            .Take(3)
            .Select((item, index) =>
            {
                item.PriorityOrder = index + 1;
                return item;
            })
            .ToList();

        if (items.Count >= 3)
        {
            return items;
        }

        var profile = child.DevelopmentProfile;
        var domains = new List<(LearningDomain Domain, int Level)>
        {
            (LearningDomain.Language, profile?.LanguageLevel ?? 3),
            (LearningDomain.Math, profile?.MathLevel ?? 3),
            (LearningDomain.World, profile?.WorldLevel ?? 3),
            (LearningDomain.ExecutiveFunction, profile?.ExecutiveFunctionLevel ?? 3)
        };

        foreach (var domain in domains.OrderByDescending(x => GetGoalTrackAffinity(preferredTrack, x.Domain)).ThenBy(x => x.Level))
        {
            if (items.Count >= 3)
            {
                break;
            }

            if (items.Any(x => x.Domain == domain.Domain))
            {
                continue;
            }

            var template = templates
                .OrderByDescending(x => string.Equals(x.GoalTrack, preferredTrack, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(x => x.Domain == domain.Domain);
            if (template is null)
            {
                continue;
            }

            items.Add(new ChildMonthlyGoalItem
            {
                Domain = domain.Domain,
                SkillCode = template.SkillCode,
                SkillName = template.SkillName,
                StartScore = 20,
                CurrentScore = 20,
                TargetScore = 60,
                PriorityOrder = items.Count + 1
            });
        }

        return items;
    }

    private static void UpdateCycle(ChildProfile child, ChildMonthlyGoalCycle cycle, DateTime today)
    {
        foreach (var item in cycle.Items.OrderBy(x => x.PriorityOrder))
        {
            var progress = child.SkillProgressEntries.FirstOrDefault(x => x.SkillCode == item.SkillCode);
            item.CurrentScore = progress?.MasteryScore ?? item.StartScore;

            var ratio = item.TargetScore == 0
                ? 0
                : (int)Math.Round((double)item.CurrentScore / item.TargetScore * 100);
            ratio = Math.Clamp(ratio, 0, 100);

            item.Status = item.CurrentScore >= item.TargetScore
                ? "completed"
                : ratio >= 70
                    ? "on_track"
                    : "at_risk";
        }

        var monthSessions = child.LearningSessions
            .Where(x => x.LoggedAt.Year == cycle.Year && x.LoggedAt.Month == cycle.Month)
            .ToList();

        cycle.TotalGoals = cycle.Items.Count;
        cycle.GoalsOnTrack = cycle.Items.Count(x => x.Status is "on_track" or "completed");
        cycle.ProgressPercent = cycle.Items.Count == 0
            ? 0
            : (int)Math.Round(cycle.Items.Average(x => Math.Clamp((double)x.CurrentScore / Math.Max(x.TargetScore, 1) * 100, 0, 100)));
        cycle.LastSessionAt = monthSessions.OrderByDescending(x => x.LoggedAt).Select(x => (DateTime?)x.LoggedAt).FirstOrDefault();
        cycle.RiskLevel = BuildRiskLevel(cycle, monthSessions.Count, today);
        cycle.Summary = BuildSummary(child.FullName, cycle, monthSessions.Count);
        cycle.UpdatedAt = DateTime.UtcNow;
    }

    private static string BuildGoalHeadline(ChildProfile child, ChildMonthlyGoalCycle cycle)
    {
        var focus = child.FamilyGoalTrack switch
        {
            "literacy" => "Trilha do mes: alfabetizacao e linguagem",
            "math_foundations" => "Trilha do mes: matematica base e raciocinio",
            "autonomy" => "Trilha do mes: autonomia, foco e organizacao",
            "science_discovery" => "Trilha do mes: ciencias, observacao e descoberta",
            _ => "Trilha do mes: crescimento equilibrado"
        };

        var goals = string.Join(" + ", cycle.Items.OrderBy(x => x.PriorityOrder).Take(2).Select(x => x.SkillName));
        return string.IsNullOrWhiteSpace(goals) ? focus : $"{focus} • {goals}";
    }

    private static int GetGoalTrackAffinity(string track, LearningDomain domain) => track switch
    {
        "literacy" when domain == LearningDomain.Language => 4,
        "math_foundations" when domain == LearningDomain.Math => 4,
        "autonomy" when domain == LearningDomain.ExecutiveFunction => 4,
        "science_discovery" when domain == LearningDomain.World => 4,
        "balanced_growth" => 2,
        _ when domain == LearningDomain.Language || domain == LearningDomain.Math => 2,
        _ => 1
    };

    private static string BuildRiskLevel(ChildMonthlyGoalCycle cycle, int sessionsThisMonth, DateTime today)
    {
        var daysWithoutSession = cycle.LastSessionAt.HasValue
            ? (today.Date - cycle.LastSessionAt.Value.Date).Days
            : 99;

        if ((today.Day >= 10 && sessionsThisMonth == 0) || daysWithoutSession >= 5 || (today.Day >= 12 && cycle.ProgressPercent < 45))
        {
            return "high";
        }

        if (daysWithoutSession >= 3 || cycle.ProgressPercent < 70)
        {
            return "medium";
        }

        return "low";
    }

    private static string BuildSummary(string childName, ChildMonthlyGoalCycle cycle, int sessionsThisMonth)
    {
        var nextAction = cycle.Items
            .OrderBy(x => x.Status == "completed" ? 1 : 0)
            .ThenBy(x => x.PriorityOrder)
            .FirstOrDefault()?.SkillName ?? "as habilidades prioritarias";

        return sessionsThisMonth == 0
            ? $"{childName} ainda nao registrou sessao neste ciclo. A familia precisa reengatar rapido para nao esfriar a rotina."
            : $"{childName} tem {cycle.GoalsOnTrack} meta(s) em rota de {cycle.TotalGoals}. O foco mais urgente agora e {nextAction.ToLowerInvariant()}.";
    }

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
