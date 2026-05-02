using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NewSchool.Web.Models;

namespace NewSchool.Web.ViewComponents;

public class AdBannerViewComponent(IOptions<AdvertisingOptions> advertisingOptionsAccessor) : ViewComponent
{
    public IViewComponentResult Invoke(
        string zone,
        string? label = null,
        string? description = null,
        string variant = "inline",
        string? adSlot = null,
        string? adClient = null,
        bool? showPlaceholder = null)
    {
        var options = advertisingOptionsAccessor.Value;
        var resolvedAdClient = string.IsNullOrWhiteSpace(adClient)
            ? options.AdSenseClientId
            : adClient;

        var shouldShowPlaceholder = showPlaceholder
            ?? !options.EnableAdScripts
            || !options.ShowPlaceholders
            || string.IsNullOrWhiteSpace(resolvedAdClient)
            || string.IsNullOrWhiteSpace(adSlot);

        var model = new AdBannerViewModel
        {
            Zone = zone,
            Label = string.IsNullOrWhiteSpace(label) ? "Espaco reservado para parceiros educacionais" : label,
            Description = string.IsNullOrWhiteSpace(description)
                ? "Quando a monetizacao entrar, este espaco pode receber anuncios sem quebrar a leitura no celular."
                : description,
            Variant = string.IsNullOrWhiteSpace(variant) ? "inline" : variant.Trim().ToLowerInvariant(),
            AdClient = resolvedAdClient ?? string.Empty,
            AdSlot = adSlot ?? string.Empty,
            ShowPlaceholder = shouldShowPlaceholder
        };

        return View(model);
    }
}
