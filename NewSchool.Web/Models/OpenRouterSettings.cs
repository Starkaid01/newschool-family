namespace NewSchool.Web.Models;

public class OpenRouterSettings
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    public string ModelsApiUrl { get; set; } = "https://openrouter.ai/api/v1/models";
    public string SiteUrl { get; set; } = "https://newschool.local";
    public string AppName { get; set; } = "NewSchool";
    public string PrimaryApiKey { get; set; } = string.Empty;
    public string SecondaryApiKey { get; set; } = string.Empty;
    public string TertiaryApiKey { get; set; } = string.Empty;
    public string FreeModelOverrides { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 140;
}
