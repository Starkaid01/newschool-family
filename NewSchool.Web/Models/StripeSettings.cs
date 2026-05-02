namespace NewSchool.Web.Models;

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public string PriceId20 { get; set; } = string.Empty;
    public string PriceId30 { get; set; } = string.Empty;
    public string PriceId80 { get; set; } = string.Empty;
    public string PriceId120 { get; set; } = string.Empty;
    public string PriceIdExtra100 { get; set; } = string.Empty;
    public string WebhookSecretSnapshot { get; set; } = string.Empty;
    public string WebhookSecretMin { get; set; } = string.Empty;
}
