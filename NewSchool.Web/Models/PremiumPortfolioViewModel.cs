namespace NewSchool.Web.Models;

public class PremiumPortfolioViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public string CoverTitle { get; set; } = string.Empty;
    public string CoverSubtitle { get; set; } = string.Empty;
    public string ParentLetter { get; set; } = string.Empty;
    public string PedagogicalSignature { get; set; } = string.Empty;
    public PortfolioEvidenceViewModel? CoverEvidence { get; set; }
    public int SessionsThisMonth { get; set; }
    public int MinutesThisMonth { get; set; }
    public int EvidenceCountThisMonth { get; set; }
    public List<AchievementHighlightViewModel> Achievements { get; set; } = new();
    public List<ChildAchievementViewModel> CelebrationBadges { get; set; } = new();
    public List<MonthlyGoalViewModel> MonthlyGoals { get; set; } = new();
    public List<PortfolioEvidenceViewModel> EvidenceItems { get; set; } = new();
    public List<MonthlyHistoryViewModel> History { get; set; } = new();
    public List<string> NextCycleRecommendations { get; set; } = new();
}
