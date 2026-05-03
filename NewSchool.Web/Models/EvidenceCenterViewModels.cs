using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NewSchool.Web.Models;

public class ChildEvidenceCenterViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ChildUrl { get; set; } = string.Empty;
    public string CurriculumUrl { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
    public string SelectedType { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int TotalMatchingItems { get; set; }
    public int PhotoCount { get; set; }
    public int VideoCount { get; set; }
    public int DocumentCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalPages { get; set; } = 1;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public EvidenceStorageSummaryViewModel Storage { get; set; } = new();
    public UploadEvidenceViewModel Upload { get; set; } = new();
    public List<EvidenceAssetViewModel> Items { get; set; } = new();
}

public class UploadEvidenceViewModel
{
    public Guid ChildId { get; set; }
    public Guid? DailyPlanId { get; set; }

    [Display(Name = "Titulo")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Observacao")]
    public string Notes { get; set; } = string.Empty;

    public string ThumbnailDataUrl { get; set; } = string.Empty;

    [Display(Name = "Arquivo")]
    public IFormFile? File { get; set; }
}

public class UploadEvidenceChunkViewModel
{
    public Guid ChildId { get; set; }
    public Guid? DailyPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string UploadId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public long TotalSize { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ThumbnailDataUrl { get; set; } = string.Empty;
    public IFormFile? Chunk { get; set; }
}

public class EvidenceAssetViewModel
{
    public Guid ChildId { get; set; }
    public Guid SessionId { get; set; }
    public DateTime LoggedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ResolvedMediaKind => ResolveMediaKind(MediaContentType, FileName, MediaUrl);
    public bool IsImage => string.Equals(ResolvedMediaKind, "image", StringComparison.OrdinalIgnoreCase);
    public bool IsVideo => string.Equals(ResolvedMediaKind, "video", StringComparison.OrdinalIgnoreCase);
    public bool IsDocument => string.Equals(ResolvedMediaKind, "document", StringComparison.OrdinalIgnoreCase);
    public string TypeLabel => IsImage ? "Foto" : IsVideo ? "Vídeo" : "Documento";
    public bool HasThumbnail => !string.IsNullOrWhiteSpace(ThumbnailUrl);

    private static string ResolveMediaKind(string contentType, string fileName, string mediaUrl)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "image";
            }

            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return "video";
            }

            if (contentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                contentType.StartsWith("application/msword", StringComparison.OrdinalIgnoreCase) ||
                contentType.StartsWith("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase))
            {
                return "document";
            }
        }

        var extension = Path.GetExtension(!string.IsNullOrWhiteSpace(fileName) ? fileName : mediaUrl)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".heic" or ".heif" => "image",
            ".mp4" or ".mov" or ".m4v" or ".webm" or ".3gp" or ".3g2" => "video",
            _ => "document"
        };
    }
}

public class SaveEvidenceThumbnailViewModel
{
    public Guid ChildId { get; set; }
    public Guid SessionId { get; set; }
    public string ThumbnailDataUrl { get; set; } = string.Empty;
}
