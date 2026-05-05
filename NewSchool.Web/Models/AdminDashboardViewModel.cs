namespace NewSchool.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalRegisteredUsers { get; set; }
    public int TotalParents { get; set; }
    public int TotalChildren { get; set; }
    public int UsersOnline { get; set; }
    public int ActiveSubscribers { get; set; }
    public int InactiveParents { get; set; }
    public int CanceledParents { get; set; }
    public decimal EstimatedMrr { get; set; }
    public decimal TotalRevenueCollected { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal RetentionRate { get; set; }
    public decimal ChurnRate { get; set; }
    public int NewParentsThisWeek { get; set; }
    public int TrialsEndingSoon { get; set; }
    public int PastDueParents { get; set; }
    public int HighRiskParents { get; set; }
    public int PedagogicalRiskParents { get; set; }
    public int OffTrackGoalCycles { get; set; }
    public int PlansGeneratedToday { get; set; }
    public int SessionsLoggedThisWeek { get; set; }
    public int MinutesDeliveredThisWeek { get; set; }
    public AzureOperationsUsageViewModel AzureUsage { get; set; } = new();
    public List<TrackPerformanceViewModel> TrackPerformance { get; set; } = new();
    public string SelectedSubscription { get; set; } = string.Empty;
    public string SelectedActivity { get; set; } = string.Empty;
    public string SelectedRisk { get; set; } = string.Empty;
    public List<UserOverviewViewModel> Parents { get; set; } = new();
}

public class TrackPerformanceViewModel
{
    public string TrackCode { get; set; } = string.Empty;
    public string TrackLabel { get; set; } = string.Empty;
    public int Leads { get; set; }
    public int Subscribers { get; set; }
    public int RetainedFamilies { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal RetentionRate { get; set; }
}

public class UserOverviewViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public string CurrentPlanName { get; set; } = string.Empty;
    public string AcquisitionTrackLabel { get; set; } = string.Empty;
    public string ActivityLabel { get; set; } = string.Empty;
    public string ActivityChipClass { get; set; } = "neutral";
    public string ChurnRiskLabel { get; set; } = string.Empty;
    public string ChurnRiskChipClass { get; set; } = "neutral";
    public string PedagogicalRiskLabel { get; set; } = string.Empty;
    public string PedagogicalRiskChipClass { get; set; } = "neutral";
    public string LastActiveLabel { get; set; } = string.Empty;
    public int OffTrackGoals { get; set; }
    public int ChildrenCount { get; set; }
    public int SessionsCount { get; set; }
    public int MinutesCompleted { get; set; }
    public int UnreadNotifications { get; set; }
}

public class AzureOperationsUsageViewModel
{
    public bool BlobStorageEnabled { get; set; }
    public bool DirectUploadEnabled { get; set; }
    public string ContainerName { get; set; } = string.Empty;
    public int TotalBlobs { get; set; }
    public int VideoBlobs { get; set; }
    public int ImageBlobs { get; set; }
    public long TotalBytes { get; set; }
    public string TotalSizeLabel { get; set; } = string.Empty;
    public decimal EstimatedMonthlyCost { get; set; }
    public string EstimatedMonthlyCostLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime MeasuredAtUtc { get; set; }
}

public class AdminManageUserViewModel
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public string CurrentPlanName { get; set; } = string.Empty;
    public string LastActiveLabel { get; set; } = string.Empty;
    public int ChildrenCount { get; set; }
    public int SessionsCount { get; set; }
    public int UnreadNotifications { get; set; }
    public string BackUrl { get; set; } = string.Empty;
    public List<AdminManagedChildViewModel> Children { get; set; } = new();
    public SendUserNotificationInput NotificationForm { get; set; } = new();
}

public class AdminManagedChildViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public int SessionsCount { get; set; }
}
