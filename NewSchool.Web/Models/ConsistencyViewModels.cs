namespace NewSchool.Web.Models;

public class FamilyConsistencyViewModel
{
    public int CurrentStreakDays { get; set; }
    public int BestStreakDays { get; set; }
    public int ActiveDaysThisMonth { get; set; }
    public int ExpectedActiveDays { get; set; }
    public int ConsistencyPercent { get; set; }
    public List<ConsistencyCalendarDayViewModel> CalendarDays { get; set; } = new();
    public List<ConsistencyRankingItemViewModel> Ranking { get; set; } = new();
}

public class ConsistencyCalendarDayViewModel
{
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }
    public bool IsToday { get; set; }
    public int SessionsCount { get; set; }
}

public class ConsistencyRankingItemViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int Position { get; set; }
    public int ActiveDaysThisMonth { get; set; }
    public int CurrentStreakDays { get; set; }
    public int MinutesThisMonth { get; set; }
    public string MomentumLabel { get; set; } = string.Empty;
}
