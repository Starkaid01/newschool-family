using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ConsistencyService(ApplicationDbContext db)
{
    public async Task<FamilyConsistencyViewModel> BuildFamilyConsistencyAsync(Guid parentId, Guid? focusChildId = null)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var children = await db.Children
            .Where(x => x.ParentId == parentId)
            .Include(x => x.LearningSessions)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        if (focusChildId.HasValue)
        {
            children = children.Where(x => x.Id == focusChildId.Value).ToList();
        }

        var familySessionDays = children
            .SelectMany(child => child.LearningSessions)
            .GroupBy(session => session.LoggedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var activeDaysThisMonth = familySessionDays.Keys.Count(date => date >= monthStart && date <= monthEnd);
        var expectedActiveDays = Math.Max(1, today.Day);

        return new FamilyConsistencyViewModel
        {
            CurrentStreakDays = CalculateCurrentStreak(familySessionDays.Keys, today),
            BestStreakDays = CalculateBestStreak(familySessionDays.Keys),
            ActiveDaysThisMonth = activeDaysThisMonth,
            ExpectedActiveDays = expectedActiveDays,
            ConsistencyPercent = (int)Math.Round((double)activeDaysThisMonth / expectedActiveDays * 100),
            CalendarDays = BuildCalendarDays(monthStart, monthEnd, familySessionDays, today),
            Ranking = BuildRanking(children, monthStart, monthEnd)
        };
    }

    private static List<ConsistencyCalendarDayViewModel> BuildCalendarDays(
        DateTime monthStart,
        DateTime monthEnd,
        IReadOnlyDictionary<DateTime, int> sessionDays,
        DateTime today)
    {
        var days = new List<ConsistencyCalendarDayViewModel>();
        for (var date = monthStart; date <= monthEnd; date = date.AddDays(1))
        {
            sessionDays.TryGetValue(date, out var sessionsCount);
            days.Add(new ConsistencyCalendarDayViewModel
            {
                Date = date,
                IsActive = sessionsCount > 0,
                IsToday = date == today,
                SessionsCount = sessionsCount
            });
        }

        return days;
    }

    private static List<ConsistencyRankingItemViewModel> BuildRanking(
        IReadOnlyList<Domain.ChildProfile> children,
        DateTime monthStart,
        DateTime monthEnd)
    {
        return children
            .Select(child =>
            {
                var activeDays = child.LearningSessions
                    .Where(x => x.LoggedAt.Date >= monthStart && x.LoggedAt.Date <= monthEnd)
                    .Select(x => x.LoggedAt.Date)
                    .Distinct()
                    .ToHashSet();

                var currentStreak = CalculateCurrentStreak(activeDays, DateTime.Today);
                return new
                {
                    Child = child,
                    ActiveDays = activeDays.Count,
                    CurrentStreak = currentStreak,
                    Minutes = child.LearningSessions
                        .Where(x => x.LoggedAt.Date >= monthStart && x.LoggedAt.Date <= monthEnd)
                        .Sum(x => x.MinutesCompleted)
                };
            })
            .OrderByDescending(x => x.ActiveDays)
            .ThenByDescending(x => x.CurrentStreak)
            .ThenByDescending(x => x.Minutes)
            .Select((item, index) => new ConsistencyRankingItemViewModel
            {
                ChildId = item.Child.Id,
                ChildName = item.Child.FullName,
                Position = index + 1,
                ActiveDaysThisMonth = item.ActiveDays,
                CurrentStreakDays = item.CurrentStreak,
                MinutesThisMonth = item.Minutes,
                MomentumLabel = item.CurrentStreak switch
                {
                    >= 7 => "Ritmo fortissimo",
                    >= 4 => "Em grande fase",
                    >= 2 => "Constancia subindo",
                    _ => "Pedindo ritmo"
                }
            })
            .ToList();
    }

    private static int CalculateCurrentStreak(IEnumerable<DateTime> activeDates, DateTime today)
    {
        var activeSet = activeDates.Select(x => x.Date).ToHashSet();
        var streak = 0;
        var cursor = activeSet.Contains(today.Date) ? today.Date : today.Date.AddDays(-1);

        while (activeSet.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static int CalculateBestStreak(IEnumerable<DateTime> activeDates)
    {
        var ordered = activeDates.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
        if (ordered.Count == 0)
        {
            return 0;
        }

        var best = 1;
        var current = 1;

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i] == ordered[i - 1].AddDays(1))
            {
                current++;
                best = Math.Max(best, current);
            }
            else
            {
                current = 1;
            }
        }

        return best;
    }
}
