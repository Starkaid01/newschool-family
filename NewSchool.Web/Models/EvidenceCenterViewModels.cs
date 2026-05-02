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
    public int PhotoCount { get; set; }
    public int VideoCount { get; set; }
    public int DocumentCount { get; set; }
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

    [Display(Name = "Arquivo")]
    public IFormFile? File { get; set; }
}

public class EvidenceAssetViewModel
{
    public Guid SessionId { get; set; }
    public DateTime LoggedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsImage => MediaContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    public bool IsVideo => MediaContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsDocument =>
        MediaContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/msword", StringComparison.OrdinalIgnoreCase) ||
        MediaContentType.StartsWith("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);
    public string TypeLabel => IsImage ? "Foto" : IsVideo ? "Vídeo" : "Documento";
}
