namespace NewSchool.Web.Models;

public class EmailSettings
{
    public string Transport { get; set; } = "PickupFolder";
    public string FromName { get; set; } = "NewSchool";
    public string FromEmail { get; set; } = "noreply@newschool.local";
    public string? PublicBaseUrl { get; set; }
    public string? PickupFolder { get; set; }
    public string? ResendApiKey { get; set; }
    public string? ResendReplyTo { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool UseSsl { get; set; } = true;
}
