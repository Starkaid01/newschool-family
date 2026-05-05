namespace NewSchool.Web.Models;

public class AzureOperationsSettings
{
    public const string SectionName = "AzureOperations";

    public decimal EstimatedBlobCostPerGbMonthly { get; set; } = 0.12m;
}
