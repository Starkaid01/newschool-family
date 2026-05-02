namespace NewSchool.Web.Models;

public class AdvertisingOptions
{
    public const string SectionName = "Advertising";

    public bool EnableAdScripts { get; set; }
    public string AdSenseClientId { get; set; } = string.Empty;
    public bool ShowPlaceholders { get; set; } = true;
}

public class AdBannerViewModel
{
    public string Zone { get; set; } = string.Empty;
    public string Label { get; set; } = "Espaco reservado para parceiros educacionais";
    public string Description { get; set; } = "Quando a monetizacao entrar, este espaco pode receber anuncios sem quebrar a leitura no celular.";
    public string Variant { get; set; } = "inline";
    public string AdClient { get; set; } = string.Empty;
    public string AdSlot { get; set; } = string.Empty;
    public bool ShowPlaceholder { get; set; } = true;

    public bool RenderLiveAd =>
        !ShowPlaceholder
        && !string.IsNullOrWhiteSpace(AdClient)
        && !string.IsNullOrWhiteSpace(AdSlot);

    public string AccessibilityLabel =>
        string.IsNullOrWhiteSpace(Label)
            ? "Espaco reservado para anuncio"
            : $"Espaco reservado para anuncio: {Label}";
}
