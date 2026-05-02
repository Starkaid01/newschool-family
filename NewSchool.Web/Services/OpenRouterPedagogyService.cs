using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class OpenRouterPedagogyService(
    IHttpClientFactory httpClientFactory,
    IOptions<OpenRouterSettings> settings)
{
    private static int _requestSeed;
    private static DateTime _cachedModelsAtUtc = DateTime.MinValue;
    private static List<string> _cachedModels = new();

    public async Task<string?> TryBuildAdultCoachNoteAsync(
        string childName,
        string skillName,
        string challengeSummary,
        string defaultCoachNote,
        CancellationToken cancellationToken = default)
    {
        var config = settings.Value;
        if (!config.Enabled)
        {
            return null;
        }

        var apiKeys = new[]
            {
                config.PrimaryApiKey?.Trim(),
                config.SecondaryApiKey?.Trim(),
                config.TertiaryApiKey?.Trim()
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (apiKeys.Count == 0)
        {
            return null;
        }

        var modelCandidates = await GetCandidateModelsAsync(config, cancellationToken);
        if (modelCandidates.Count == 0)
        {
            modelCandidates.Add("openrouter/free");
        }

        var rotation = Math.Abs(Interlocked.Increment(ref _requestSeed));
        var orderedKeys = Rotate(apiKeys, rotation);
        var orderedModels = Rotate(modelCandidates, rotation);

        foreach (var apiKey in orderedKeys)
        {
            foreach (var model in orderedModels)
            {
                var note = await TryCreateCompletionAsync(
                    config,
                    apiKey,
                    model,
                    childName,
                    skillName,
                    challengeSummary,
                    defaultCoachNote,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(note))
                {
                    return note;
                }
            }
        }

        return null;
    }

    private async Task<List<string>> GetCandidateModelsAsync(OpenRouterSettings config, CancellationToken cancellationToken)
    {
        if (_cachedModels.Count > 0 && DateTime.UtcNow - _cachedModelsAtUtc < TimeSpan.FromHours(6))
        {
            return _cachedModels;
        }

        var overrides = config.FreeModelOverrides
            .Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (overrides.Count > 0)
        {
            if (!overrides.Contains("openrouter/free", StringComparer.OrdinalIgnoreCase))
            {
                overrides.Insert(0, "openrouter/free");
            }

            _cachedModels = overrides;
            _cachedModelsAtUtc = DateTime.UtcNow;
            return overrides;
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(config.ModelsApiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ["openrouter/free"];
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return ["openrouter/free"];
            }

            var preferredFamilies = new[] { "google/", "meta-llama/", "qwen/", "mistralai/", "deepseek/" };

            var discovered = data.EnumerateArray()
                .Select(x => x.TryGetProperty("id", out var idNode) ? idNode.GetString() : null)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x!.EndsWith(":free", StringComparison.OrdinalIgnoreCase))
                .Cast<string>()
                .OrderBy(x =>
                {
                    var index = Array.FindIndex(preferredFamilies, family => x.StartsWith(family, StringComparison.OrdinalIgnoreCase));
                    return index < 0 ? int.MaxValue : index;
                })
                .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();

            discovered.Insert(0, "openrouter/free");
            _cachedModels = discovered.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _cachedModelsAtUtc = DateTime.UtcNow;
            return _cachedModels;
        }
        catch
        {
            return ["openrouter/free"];
        }
    }

    private async Task<string?> TryCreateCompletionAsync(
        OpenRouterSettings config,
        string apiKey,
        string model,
        string childName,
        string skillName,
        string challengeSummary,
        string defaultCoachNote,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, config.BaseUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.TryAddWithoutValidation("HTTP-Referer", config.SiteUrl);
        request.Headers.TryAddWithoutValidation("X-OpenRouter-Title", config.AppName);

        var payload = new
        {
            model,
            max_tokens = Math.Clamp(config.MaxTokens, 80, 320),
            temperature = 0.4,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Voce e um coach pedagogico para familias brasileiras de ensino domiciliar. Responda em portugues, com no maximo 90 palavras, tom calmo, pratico e sem termos tecnicos desnecessarios."
                },
                new
                {
                    role = "user",
                    content = $"Crie uma orientacao curta para o adulto. Crianca: {childName}. Habilidade: {skillName}. Dificuldade observada: {challengeSummary}. Base sugerida: {defaultCoachNote}. Entregue 3 frases: o que dizer, o que fazer hoje e como saber se melhorou."
                }
            }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.PaymentRequired or HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    return null;
                }

                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            {
                return null;
            }

            var content = choices[0].GetProperty("message").GetProperty("content").GetString();
            return string.IsNullOrWhiteSpace(content) ? null : content.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static List<string> Rotate(List<string> source, int seed)
    {
        if (source.Count <= 1)
        {
            return source;
        }

        var shift = seed % source.Count;
        return source.Skip(shift).Concat(source.Take(shift)).ToList();
    }
}
