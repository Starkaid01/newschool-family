namespace NewSchool.Web.Models;

public class RecoveryPlanCardViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int CompletedDays { get; set; }
    public int TotalDays { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class ChildAchievementViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}

public class RecoveryPlanDetailsViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string EvolutionUrl { get; set; } = string.Empty;
    public int CompletedDays { get; set; }
    public int TotalDays { get; set; }
    public List<RecoveryPlanDayViewModel> Days { get; set; } = new();
}

public class RecoveryPlanDayViewModel
{
    public int DayNumber { get; set; }
    public DateTime SuggestedDate { get; set; }
    public string FocusSkill { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public string ParentTip { get; set; } = string.Empty;
    public bool Completed { get; set; }
}
