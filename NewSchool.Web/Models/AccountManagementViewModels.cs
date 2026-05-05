namespace NewSchool.Web.Models;

public class ParentAccountViewModel
{
    public string ParentName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CurrentPlanName { get; set; } = string.Empty;
    public string CurrentPlanStatusLabel { get; set; } = string.Empty;
    public string CurrentPlanPriceLabel { get; set; } = string.Empty;
    public string CurrentPlanSummary { get; set; } = string.Empty;
    public bool HasPaidPlan { get; set; }
    public bool CanManageBilling { get; set; }
    public bool CanCancelPlan { get; set; }
    public string PlansUrl { get; set; } = string.Empty;
    public string NotificationsUrl { get; set; } = string.Empty;
    public string DashboardUrl { get; set; } = string.Empty;
    public EvidenceStorageSummaryViewModel Storage { get; set; } = new();
    public UpdateAccountProfileViewModel ProfileForm { get; set; } = new();
    public ChangeAccountPasswordViewModel PasswordForm { get; set; } = new();
    public CancelSubscriptionConfirmationViewModel CancelForm { get; set; } = new();
}

public class UpdateAccountProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ChangeAccountPasswordViewModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CancelSubscriptionConfirmationViewModel
{
    public bool ConfirmCancellation { get; set; }
}
