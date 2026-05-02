namespace NewSchool.Web.Domain;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public Guid? ReferredByUserId { get; set; }
    public AppUser? ReferredByUser { get; set; }
    public string SubscriptionStatus { get; set; } = "inactive";
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string StoragePlanCode { get; set; } = "free";
    public int StorageExtraFileBlocks { get; set; }
    public string AcquisitionTrack { get; set; } = string.Empty;
    public DateTime? FirstSubscribedAt { get; set; }
    public string PreferredReminderChannel { get; set; } = "email";
    public DateTime? SubscriptionCurrentPeriodEnd { get; set; }
    public DateTime? TrialStartedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime? OnboardingEmailSentAt { get; set; }
    public DateTime? TrialReminderSentAt { get; set; }
    public DateTime? ReactivationEmailSentAt { get; set; }
    public DateTime? PaymentRecoveryEmailSentAt { get; set; }
    public DateTime? DailyReminderMessageSentAt { get; set; }
    public DateTime? ProgressRiskMessageSentAt { get; set; }
    public ICollection<ChildProfile> Children { get; set; } = new List<ChildProfile>();
    public ICollection<AppUser> Referrals { get; set; } = new List<AppUser>();
}
