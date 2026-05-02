namespace NewSchool.Web.Models;

public class ReferralSummaryViewModel
{
    public string ReferralCode { get; set; } = string.Empty;
    public string ReferralUrl { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
    public string ShareMessage { get; set; } = string.Empty;
    public string WhatsAppShareUrl { get; set; } = string.Empty;
    public int TotalReferrals { get; set; }
    public int ActiveReferrals { get; set; }
    public List<ReferralActivityViewModel> RecentReferrals { get; set; } = new();
}

public class ReferralActivityViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReferralLandingViewModel
{
    public string ReferrerName { get; set; } = string.Empty;
    public string ReferralCode { get; set; } = string.Empty;
    public string InvitationHeadline { get; set; } = string.Empty;
    public string InvitationMessage { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
    public int ActiveReferrals { get; set; }
    public int TotalReferrals { get; set; }
    public int ChildrenInRoutine { get; set; }
    public int SnapshotsRecorded { get; set; }
    public List<string> ProofPoints { get; set; } = new();
}

public class AchievementShareCardViewModel
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string ParentFirstName { get; set; } = string.Empty;
    public string BadgeTitle { get; set; } = string.Empty;
    public string BadgeDescription { get; set; } = string.Empty;
    public string MomentumSummary { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public string ReferralLandingUrl { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
    public string ShareMessage { get; set; } = string.Empty;
    public string WhatsAppShareUrl { get; set; } = string.Empty;
    public int SessionsThisMonth { get; set; }
    public int EvidenceCount { get; set; }
    public int ProgressPercent { get; set; }
}
