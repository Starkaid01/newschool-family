using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public sealed class EvidenceMediaStorageService(
    IWebHostEnvironment environment,
    IDataProtectionProvider dataProtectionProvider,
    IOptions<EvidenceStorageOptions> storageOptions,
    ILogger<EvidenceMediaStorageService> logger)
{
    public const string LocalProvider = "Local";
    public const string AzureBlobProvider = "AzureBlob";

    private readonly EvidenceStorageOptions _options = storageOptions.Value;
    private readonly IDataProtector _uploadProtector =
        dataProtectionProvider.CreateProtector("NewSchool.Evidence.DirectUpload");

    private BlobContainerClient? _containerClient;
    private bool _containerEnsured;

    public bool BlobStorageEnabled =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(_options.AzureConnectionString) &&
        !string.IsNullOrWhiteSpace(_options.AzureContainerName);

    public bool DirectUploadEnabled => BlobStorageEnabled && _options.UseDirectUpload;

    public async Task<EvidenceDirectUploadGrant?> CreateDirectUploadGrantAsync(
        Guid parentId,
        Guid childId,
        string fileName,
        string? contentType,
        long totalSize,
        CancellationToken cancellationToken = default)
    {
        if (!DirectUploadEnabled)
        {
            return null;
        }

        var normalized = NormalizeIncomingFile(fileName, contentType);
        var blobName = BuildEvidenceBlobName(parentId, childId, normalized.Extension);
        var blobClient = await GetBlobClientAsync(blobName, cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Max(_options.UploadSasMinutes, 5));

        var ticket = new EvidenceDirectUploadTicket
        {
            ParentId = parentId,
            ChildId = childId,
            BlobName = blobName,
            FileName = normalized.SafeOriginalFileName,
            ContentType = normalized.ContentType,
            TotalSize = totalSize,
            ExpiresAt = expiresAt
        };

        var protectedToken = _uploadProtector.Protect(JsonSerializer.Serialize(ticket));
        var uploadUrl = BuildUploadSasUri(blobClient, expiresAt).ToString();

        return new EvidenceDirectUploadGrant(
            protectedToken,
            uploadUrl,
            blobClient.Uri.ToString(),
            AzureBlobProvider,
            blobName,
            normalized.SafeOriginalFileName,
            normalized.ContentType,
            expiresAt);
    }

    public async Task<EvidenceStoredObject?> FinalizeDirectUploadAsync(
        Guid parentId,
        Guid childId,
        string uploadToken,
        string? thumbnailDataUrl,
        CancellationToken cancellationToken = default)
    {
        if (!DirectUploadEnabled)
        {
            return null;
        }

        var ticket = TryReadUploadTicket(uploadToken);
        if (ticket is null ||
            ticket.ParentId != parentId ||
            ticket.ChildId != childId ||
            ticket.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return null;
        }

        var blobClient = await GetBlobClientAsync(ticket.BlobName, cancellationToken);
        if (!(await blobClient.ExistsAsync(cancellationToken)).Value)
        {
            return null;
        }

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        if (properties.Value.ContentLength <= 0)
        {
            return null;
        }

        if (ticket.TotalSize > 0 && properties.Value.ContentLength != ticket.TotalSize)
        {
            logger.LogWarning(
                "Evidence blob size mismatch for {BlobName}. Expected {ExpectedSize}, found {ActualSize}.",
                ticket.BlobName,
                ticket.TotalSize,
                properties.Value.ContentLength);
            return null;
        }

        var stored = new EvidenceStoredObject
        {
            MediaUrl = blobClient.Uri.ToString(),
            MediaContentType = string.IsNullOrWhiteSpace(ticket.ContentType)
                ? properties.Value.ContentType ?? "application/octet-stream"
                : ticket.ContentType,
            FileName = ticket.FileName,
            StorageProvider = AzureBlobProvider,
            StorageKey = ticket.BlobName
        };

        var thumbnail = await SaveThumbnailAsync(stored, thumbnailDataUrl, cancellationToken);
        if (thumbnail is not null)
        {
            stored.MediaThumbnailUrl = thumbnail.Url;
            stored.MediaThumbnailStorageKey = thumbnail.StorageKey;
        }

        return stored;
    }

    public async Task<EvidenceStoredObject?> SaveUploadedFileAsync(
        Guid parentId,
        Guid childId,
        Stream sourceStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeIncomingFile(fileName, contentType);
        if (BlobStorageEnabled)
        {
            var blobName = BuildEvidenceBlobName(parentId, childId, normalized.Extension);
            var blobClient = await GetBlobClientAsync(blobName, cancellationToken);
            sourceStream.Position = 0;
            await blobClient.UploadAsync(
                sourceStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = normalized.ContentType
                    }
                },
                cancellationToken);

            return new EvidenceStoredObject
            {
                MediaUrl = blobClient.Uri.ToString(),
                MediaContentType = normalized.ContentType,
                FileName = normalized.SafeOriginalFileName,
                StorageProvider = AzureBlobProvider,
                StorageKey = blobName
            };
        }

        return await SaveLocalFileAsync(sourceStream, normalized, cancellationToken);
    }

    public async Task<EvidenceStoredThumbnail?> SaveThumbnailAsync(
        EvidenceStoredObject media,
        string? thumbnailDataUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(thumbnailDataUrl))
        {
            return null;
        }

        var thumbnailBytes = TryDecodeThumbnail(thumbnailDataUrl);
        if (thumbnailBytes is null)
        {
            return null;
        }

        if (string.Equals(media.StorageProvider, AzureBlobProvider, StringComparison.OrdinalIgnoreCase) &&
            BlobStorageEnabled &&
            !string.IsNullOrWhiteSpace(media.StorageKey))
        {
            var thumbnailBlobName = BuildThumbnailBlobName(media.StorageKey);
            var blobClient = await GetBlobClientAsync(thumbnailBlobName, cancellationToken);
            await using var stream = new MemoryStream(thumbnailBytes, writable: false);
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg"
                    }
                },
                cancellationToken);

            return new EvidenceStoredThumbnail(blobClient.Uri.ToString(), thumbnailBlobName);
        }

        var localThumbnail = await SaveLocalThumbnailAsync(media.MediaUrl, thumbnailBytes, cancellationToken);
        return string.IsNullOrWhiteSpace(localThumbnail)
            ? null
            : new EvidenceStoredThumbnail(localThumbnail, string.Empty);
    }

    public string ResolveReadUrl(string? mediaUrl, string? storageProvider, string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(mediaUrl))
        {
            return string.Empty;
        }

        if (string.Equals(storageProvider, AzureBlobProvider, StringComparison.OrdinalIgnoreCase) &&
            BlobStorageEnabled)
        {
            var blobName = !string.IsNullOrWhiteSpace(storageKey)
                ? storageKey
                : TryExtractBlobNameFromUrl(mediaUrl);

            if (!string.IsNullOrWhiteSpace(blobName))
            {
                try
                {
                    var blobClient = GetBlobClient(blobName);
                    return BuildReadSasUri(blobClient).ToString();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to generate read SAS for {BlobName}. Falling back to canonical URL.", blobName);
                }
            }
        }

        return mediaUrl;
    }

    public async Task DeleteAsync(
        string? storageProvider,
        string? mediaUrl,
        string? storageKey,
        string? thumbnailUrl = null,
        string? thumbnailStorageKey = null,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(storageProvider, AzureBlobProvider, StringComparison.OrdinalIgnoreCase) &&
            BlobStorageEnabled)
        {
            var mediaBlobName = !string.IsNullOrWhiteSpace(storageKey)
                ? storageKey
                : TryExtractBlobNameFromUrl(mediaUrl);

            if (!string.IsNullOrWhiteSpace(mediaBlobName))
            {
                var blobClient = await GetBlobClientAsync(mediaBlobName, cancellationToken);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }

            var thumbnailBlobName = !string.IsNullOrWhiteSpace(thumbnailStorageKey)
                ? thumbnailStorageKey
                : TryExtractBlobNameFromUrl(thumbnailUrl);

            if (!string.IsNullOrWhiteSpace(thumbnailBlobName))
            {
                var blobClient = await GetBlobClientAsync(thumbnailBlobName, cancellationToken);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }

            return;
        }

        DeleteLocalEvidence(mediaUrl);
    }

    private async Task<EvidenceStoredObject> SaveLocalFileAsync(
        Stream sourceStream,
        NormalizedEvidenceFile normalized,
        CancellationToken cancellationToken)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "session-evidence");
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{normalized.Extension}";
        var physicalPath = Path.Combine(uploadsFolder, safeFileName);

        if (sourceStream.CanSeek)
        {
            sourceStream.Position = 0;
        }

        await using var stream = System.IO.File.Create(physicalPath);
        await sourceStream.CopyToAsync(stream, cancellationToken);

        return new EvidenceStoredObject
        {
            MediaUrl = $"/uploads/session-evidence/{safeFileName}",
            MediaContentType = normalized.ContentType,
            FileName = normalized.SafeOriginalFileName,
            StorageProvider = LocalProvider,
            StorageKey = $"session-evidence/{safeFileName}"
        };
    }

    private async Task<string?> SaveLocalThumbnailAsync(string mediaUrl, byte[] thumbnailBytes, CancellationToken cancellationToken)
    {
        if (!TryResolveLocalPhysicalPath(mediaUrl, out var mediaPhysicalPath))
        {
            return null;
        }

        var thumbnailPhysicalPath = BuildLocalThumbnailPhysicalPath(mediaPhysicalPath);
        await System.IO.File.WriteAllBytesAsync(thumbnailPhysicalPath, thumbnailBytes, cancellationToken);
        return BuildLocalThumbnailUrl(mediaUrl);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        if (_containerClient is null)
        {
            var serviceClient = new BlobServiceClient(_options.AzureConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(_options.AzureContainerName);
        }

        if (!_containerEnsured)
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            _containerEnsured = true;
        }

        return _containerClient;
    }

    private async Task<BlobClient> GetBlobClientAsync(string blobName, CancellationToken cancellationToken)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        return containerClient.GetBlobClient(blobName);
    }

    private BlobClient GetBlobClient(string blobName)
    {
        var serviceClient = new BlobServiceClient(_options.AzureConnectionString);
        return serviceClient.GetBlobContainerClient(_options.AzureContainerName).GetBlobClient(blobName);
    }

    private Uri BuildUploadSasUri(BlobClient blobClient, DateTimeOffset expiresAt)
    {
        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("A conexão do Azure Blob não permite gerar SAS com a configuração atual.");
        }

        var builder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = expiresAt
        };

        builder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);
        return blobClient.GenerateSasUri(builder);
    }

    private Uri BuildReadSasUri(BlobClient blobClient)
    {
        if (!blobClient.CanGenerateSasUri)
        {
            return blobClient.Uri;
        }

        var builder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(Math.Max(_options.ReadSasMinutes, 15))
        };

        builder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(builder);
    }

    private EvidenceDirectUploadTicket? TryReadUploadTicket(string protectedToken)
    {
        if (string.IsNullOrWhiteSpace(protectedToken))
        {
            return null;
        }

        try
        {
            var json = _uploadProtector.Unprotect(protectedToken);
            return JsonSerializer.Deserialize<EvidenceDirectUploadTicket>(json);
        }
        catch
        {
            return null;
        }
    }

    private static NormalizedEvidenceFile NormalizeIncomingFile(string fileName, string? contentType)
    {
        fileName = string.IsNullOrWhiteSpace(fileName)
            ? BuildFallbackFileName(contentType)
            : fileName;

        var safeOriginalFileName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeOriginalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = Path.GetExtension(BuildFallbackFileName(contentType));
            safeOriginalFileName += extension;
        }

        return new NormalizedEvidenceFile(
            safeOriginalFileName,
            extension,
            NormalizeContentType(extension, contentType));
    }

    private static string NormalizeContentType(string? extension, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return contentType;
        }

        return (extension ?? string.Empty).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".m4v" => "video/x-m4v",
            ".3gp" => "video/3gpp",
            ".3g2" => "video/3gpp2",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    private static string BuildFallbackFileName(string? contentType)
    {
        var extension = (contentType ?? string.Empty).ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            "video/mp4" => ".mp4",
            "video/quicktime" => ".mov",
            "video/webm" => ".webm",
            "video/x-m4v" => ".m4v",
            "video/3gpp" => ".3gp",
            "video/3gpp2" => ".3g2",
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            _ => ".bin"
        };

        return $"arquivo{extension}";
    }

    private static byte[]? TryDecodeThumbnail(string thumbnailDataUrl)
    {
        var commaIndex = thumbnailDataUrl.IndexOf(',');
        if (commaIndex <= 0 || commaIndex >= thumbnailDataUrl.Length - 1)
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(thumbnailDataUrl[(commaIndex + 1)..]);
            return bytes.Length == 0 || bytes.Length > 2 * 1024 * 1024
                ? null
                : bytes;
        }
        catch
        {
            return null;
        }
    }

    private string BuildEvidenceBlobName(Guid parentId, Guid childId, string extension)
    {
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant();
        return $"evidence/{parentId:N}/{childId:N}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{normalizedExtension}";
    }

    private static string BuildThumbnailBlobName(string mediaBlobName)
    {
        var extension = Path.GetExtension(mediaBlobName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return $"{mediaBlobName}.thumb.jpg";
        }

        return mediaBlobName[..^extension.Length] + ".thumb.jpg";
    }

    private string? TryExtractBlobNameFromUrl(string? mediaUrl)
    {
        if (string.IsNullOrWhiteSpace(mediaUrl) ||
            !Uri.TryCreate(mediaUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var absolutePath = uri.AbsolutePath.Trim('/');
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            return null;
        }

        var prefix = $"{_options.AzureContainerName.Trim('/')}/";
        return absolutePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? absolutePath[prefix.Length..]
            : null;
    }

    private bool TryResolveLocalPhysicalPath(string mediaUrl, out string physicalPath)
    {
        physicalPath = string.Empty;
        if (string.IsNullOrWhiteSpace(environment.WebRootPath) ||
            string.IsNullOrWhiteSpace(mediaUrl) ||
            !mediaUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = mediaUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var candidatePath = Path.GetFullPath(Path.Combine(environment.WebRootPath, relativePath));
        var webRoot = Path.GetFullPath(environment.WebRootPath);
        if (!candidatePath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        physicalPath = candidatePath;
        return true;
    }

    private static string BuildLocalThumbnailPhysicalPath(string mediaPhysicalPath)
    {
        var directory = Path.GetDirectoryName(mediaPhysicalPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaPhysicalPath);
        return Path.Combine(directory, $"{fileNameWithoutExtension}.thumb.jpg");
    }

    private static string BuildLocalThumbnailUrl(string mediaUrl)
    {
        var extension = Path.GetExtension(mediaUrl);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return $"{mediaUrl}.thumb.jpg";
        }

        return mediaUrl[..^extension.Length] + ".thumb.jpg";
    }

    private void DeleteLocalEvidence(string? mediaUrl)
    {
        if (!TryResolveLocalPhysicalPath(mediaUrl ?? string.Empty, out var physicalPath) ||
            !System.IO.File.Exists(physicalPath))
        {
            return;
        }

        System.IO.File.Delete(physicalPath);

        var thumbnailPhysicalPath = BuildLocalThumbnailPhysicalPath(physicalPath);
        if (System.IO.File.Exists(thumbnailPhysicalPath))
        {
            System.IO.File.Delete(thumbnailPhysicalPath);
        }
    }

    private sealed record NormalizedEvidenceFile(string SafeOriginalFileName, string Extension, string ContentType);

    private sealed class EvidenceDirectUploadTicket
    {
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public string BlobName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}

public sealed record EvidenceDirectUploadGrant(
    string UploadToken,
    string UploadUrl,
    string MediaUrl,
    string StorageProvider,
    string StorageKey,
    string FileName,
    string MediaContentType,
    DateTimeOffset ExpiresAt);

public sealed class EvidenceStoredObject
{
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string MediaThumbnailUrl { get; set; } = string.Empty;
    public string MediaThumbnailStorageKey { get; set; } = string.Empty;
}

public sealed record EvidenceStoredThumbnail(string Url, string StorageKey);
