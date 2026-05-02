namespace NewSchool.Web.Models;

public class EvidenceStorageSummaryViewModel
{
    public string CurrentPlanCode { get; set; } = string.Empty;
    public string CurrentPlanName { get; set; } = string.Empty;
    public string CurrentPlanPriceLabel { get; set; } = string.Empty;
    public string CurrentPlanStatusLabel { get; set; } = string.Empty;
    public string CurrentPlanSummary { get; set; } = string.Empty;
    public int UsedFiles { get; set; }
    public int FileLimit { get; set; }
    public int RemainingFiles { get; set; }
    public int UsagePercent { get; set; }
    public bool CanUpload { get; set; }
    public bool HasPaidPlan { get; set; }
    public bool SupportsExtraBlocks { get; set; }
    public int ExtraBlocks { get; set; }
    public int ExtraFiles { get; set; }
    public string ExtraBlockSummary { get; set; } = string.Empty;
    public string UploadNotice { get; set; } = string.Empty;
    public string UpgradeHint { get; set; } = string.Empty;
    public bool CanManageBilling { get; set; }
    public List<EvidenceStoragePlanCardViewModel> Plans { get; set; } = new();
}

public class EvidenceStoragePlanCardViewModel
{
    public string PlanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PriceLabel { get; set; } = string.Empty;
    public string CapacityLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool SupportsExtraBlocks { get; set; }
    public string ButtonLabel { get; set; } = string.Empty;
}

public class CreateStorageCheckoutRequest
{
    public string PlanCode { get; set; } = string.Empty;
    public int ExtraBlocks { get; set; }
    public string? ReturnUrl { get; set; }
}
