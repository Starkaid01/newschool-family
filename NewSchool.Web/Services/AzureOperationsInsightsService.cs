using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class AzureOperationsInsightsService(
    IOptions<EvidenceStorageOptions> evidenceStorageOptions,
    IOptions<AzureOperationsSettings> azureOptions,
    IMemoryCache memoryCache,
    ILogger<AzureOperationsInsightsService> logger)
{
    private readonly EvidenceStorageOptions _evidenceStorageOptions = evidenceStorageOptions.Value;
    private readonly AzureOperationsSettings _azureOptions = azureOptions.Value;

    public async Task<AzureOperationsUsageViewModel> BuildUsageAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "azure-ops-usage";
        if (memoryCache.TryGetValue(cacheKey, out AzureOperationsUsageViewModel? cached) && cached is not null)
        {
            return cached;
        }

        var result = new AzureOperationsUsageViewModel
        {
            BlobStorageEnabled = !string.IsNullOrWhiteSpace(_evidenceStorageOptions.AzureConnectionString) &&
                                 !string.IsNullOrWhiteSpace(_evidenceStorageOptions.AzureContainerName) &&
                                 _evidenceStorageOptions.Enabled,
            DirectUploadEnabled = _evidenceStorageOptions.Enabled && _evidenceStorageOptions.UseDirectUpload,
            ContainerName = _evidenceStorageOptions.AzureContainerName,
            StatusLabel = "Blob não configurado",
            MeasuredAtUtc = DateTime.UtcNow
        };

        if (!result.BlobStorageEnabled)
        {
            result.TotalSizeLabel = "0 B";
            result.EstimatedMonthlyCostLabel = "R$ 0,00";
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        try
        {
            var containerClient = new BlobContainerClient(
                _evidenceStorageOptions.AzureConnectionString,
                _evidenceStorageOptions.AzureContainerName);

            await foreach (var blob in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                result.TotalBlobs++;
                result.TotalBytes += blob.Properties.ContentLength ?? 0;

                if (blob.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                    blob.Name.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                    blob.Name.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
                    blob.Name.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase))
                {
                    result.VideoBlobs++;
                }
                else if (blob.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                         blob.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                         blob.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                         blob.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    result.ImageBlobs++;
                }
            }

            var totalGb = result.TotalBytes / 1024m / 1024m / 1024m;
            result.EstimatedMonthlyCost = Math.Round(totalGb * _azureOptions.EstimatedBlobCostPerGbMonthly, 2, MidpointRounding.AwayFromZero);
            result.TotalSizeLabel = FormatSize(result.TotalBytes);
            result.EstimatedMonthlyCostLabel = $"R$ {result.EstimatedMonthlyCost:0.00}";
            result.StatusLabel = result.TotalBlobs == 0
                ? "Blob ativo, sem arquivos agora"
                : "Blob ativo e recebendo evidências";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível medir o uso do Azure Blob para o admin.");
            result.TotalSizeLabel = "indisponível";
            result.EstimatedMonthlyCostLabel = "indisponível";
            result.StatusLabel = "Não foi possível medir agora";
        }

        memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        decimal value = bytes;
        var suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:0.##} {suffixes[suffixIndex]}";
    }
}
