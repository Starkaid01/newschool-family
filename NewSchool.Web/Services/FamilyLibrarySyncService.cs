using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class FamilyLibrarySyncService(
    ApplicationDbContext db,
    IWebHostEnvironment environment,
    IOptions<FamilyLibraryOptions> options,
    ILogger<FamilyLibrarySyncService> logger)
{
    private static readonly string[] CollectionOrder =
    [
        "Histórias Infantis",
        "Histórias Bíblicas",
        "Princípios Bíblicos",
        "Outros"
    ];

    private readonly FamilyLibraryOptions _options = options.Value;

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Sincronização da biblioteca da família desativada.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SourceCatalogConnection) || string.IsNullOrWhiteSpace(_options.SourceAssetsRootPath))
        {
            logger.LogWarning("Configuração da biblioteca da família incompleta. A sincronização foi ignorada.");
            return;
        }

        if (!Directory.Exists(_options.SourceAssetsRootPath))
        {
            logger.LogWarning("Pasta de assets da biblioteca não encontrada em {Path}.", _options.SourceAssetsRootPath);
            return;
        }

        var targetAssetsRootPath = ResolveTargetAssetsRootPath();
        Directory.CreateDirectory(targetAssetsRootPath);

        await using var sourceConnection = new SqlConnection(_options.SourceCatalogConnection);
        await sourceConnection.OpenAsync(cancellationToken);

        var sourceMaterials = await LoadSourceMaterialsAsync(sourceConnection, cancellationToken);
        if (sourceMaterials.Count == 0)
        {
            logger.LogInformation("Nenhum material foi encontrado na fonte da biblioteca.");
            return;
        }

        var sourceIds = sourceMaterials.Select(item => item.Id).ToHashSet();
        var existingMaterials = await db.FamilyLibraryMaterials
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var existingPageCounts = await db.FamilyLibraryPages
            .GroupBy(page => page.MaterialId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);

        var materialsToRemove = existingMaterials.Values
            .Where(item => !sourceIds.Contains(item.Id) && !ProprietaryFamilyLibraryCatalog.IsAuthoredMaterial(item))
            .ToList();

        if (materialsToRemove.Count > 0)
        {
            db.FamilyLibraryMaterials.RemoveRange(materialsToRemove);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var removedMaterial in materialsToRemove)
            {
                DeleteTargetAssetDirectory(targetAssetsRootPath, removedMaterial.Id);
            }
        }

        foreach (var sourceMaterial in sourceMaterials)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var targetMaterial = existingMaterials.GetValueOrDefault(sourceMaterial.Id);
            var isNewMaterial = targetMaterial is null;
            if (targetMaterial is null)
            {
                targetMaterial = new FamilyLibraryMaterial
                {
                    Id = sourceMaterial.Id
                };

                db.FamilyLibraryMaterials.Add(targetMaterial);
                existingMaterials[sourceMaterial.Id] = targetMaterial;
            }

            var normalizedCoverPath = NormalizeRelativePath(sourceMaterial.CoverImageRelativePath);
            var normalizedSourcePath = NormalizeRelativePath(sourceMaterial.SourceRelativePath);
            var previousSyncToken = targetMaterial.SourceSyncToken;
            var previousPageCount = targetMaterial.PageCount;
            var currentStoredPageCount = existingPageCounts.GetValueOrDefault(sourceMaterial.Id);

            var materialChanged = isNewMaterial
                || !string.Equals(targetMaterial.Title, sourceMaterial.Title, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.Category, sourceMaterial.Category, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.EducationStage, sourceMaterial.EducationStage, StringComparison.Ordinal)
                || targetMaterial.RecommendedMinAge != sourceMaterial.RecommendedMinAge
                || targetMaterial.RecommendedMaxAge != sourceMaterial.RecommendedMaxAge
                || !string.Equals(targetMaterial.SkillFocus, sourceMaterial.SkillFocus, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.Description, sourceMaterial.Description, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.CollectionLabel, sourceMaterial.CollectionLabel, StringComparison.Ordinal)
                || targetMaterial.IsPrintable != sourceMaterial.IsPrintable
                || targetMaterial.PageCount != sourceMaterial.PageCount
                || targetMaterial.HasIllustrations != sourceMaterial.HasIllustrations
                || !string.Equals(targetMaterial.CoverImageRelativePath, normalizedCoverPath, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.SourceRelativePath, normalizedSourcePath, StringComparison.Ordinal)
                || !string.Equals(targetMaterial.SourceSyncToken, sourceMaterial.SourceSyncToken, StringComparison.Ordinal)
                || targetMaterial.SourceUpdatedAtUtc != sourceMaterial.SourceUpdatedAtUtc;

            if (materialChanged)
            {
                targetMaterial.Title = sourceMaterial.Title;
                targetMaterial.Category = sourceMaterial.Category;
                targetMaterial.EducationStage = sourceMaterial.EducationStage;
                targetMaterial.RecommendedMinAge = sourceMaterial.RecommendedMinAge;
                targetMaterial.RecommendedMaxAge = sourceMaterial.RecommendedMaxAge;
                targetMaterial.SkillFocus = sourceMaterial.SkillFocus;
                targetMaterial.Description = sourceMaterial.Description;
                targetMaterial.CollectionLabel = sourceMaterial.CollectionLabel;
                targetMaterial.IsPrintable = sourceMaterial.IsPrintable;
                targetMaterial.PageCount = sourceMaterial.PageCount;
                targetMaterial.HasIllustrations = sourceMaterial.HasIllustrations;
                targetMaterial.CoverImageRelativePath = normalizedCoverPath;
                targetMaterial.SourceRelativePath = normalizedSourcePath;
                targetMaterial.SourceSyncToken = sourceMaterial.SourceSyncToken;
                targetMaterial.SourceUpdatedAtUtc = sourceMaterial.SourceUpdatedAtUtc;
                targetMaterial.SyncedAtUtc = DateTime.UtcNow;
            }

            var needsPageRefresh = isNewMaterial
                || !string.Equals(previousSyncToken, sourceMaterial.SourceSyncToken, StringComparison.Ordinal)
                || previousPageCount != sourceMaterial.PageCount
                || currentStoredPageCount != sourceMaterial.PageCount;

            if (needsPageRefresh)
            {
                await db.FamilyLibraryPages
                    .Where(page => page.MaterialId == sourceMaterial.Id)
                    .ExecuteDeleteAsync(cancellationToken);

                var sourcePages = await LoadSourcePagesAsync(sourceConnection, sourceMaterial.Id, cancellationToken);
                var targetPages = sourcePages
                    .OrderBy(page => page.PageNumber)
                    .Select(page => new FamilyLibraryPage
                    {
                        MaterialId = sourceMaterial.Id,
                        PageNumber = page.PageNumber,
                        TextContent = page.TextContent,
                        ImageRelativePath = NormalizeRelativePath(page.ImageRelativePath)
                    })
                    .ToList();

                await db.FamilyLibraryPages.AddRangeAsync(targetPages, cancellationToken);
                existingPageCounts[sourceMaterial.Id] = targetPages.Count;
                targetMaterial.SyncedAtUtc = DateTime.UtcNow;

                foreach (var targetPage in targetPages)
                {
                    CopyAssetIfNeeded(targetAssetsRootPath, targetPage.ImageRelativePath);
                }
            }

            CopyAssetIfNeeded(targetAssetsRootPath, normalizedCoverPath);

            if (materialChanged || needsPageRefresh)
            {
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        logger.LogInformation(
            "Biblioteca da família sincronizada com {MaterialCount} materiais em {CollectionCount} coleções.",
            sourceMaterials.Count,
            CollectionOrder.Count(collection => sourceMaterials.Any(material => string.Equals(material.CollectionLabel, collection, StringComparison.OrdinalIgnoreCase))));
    }

    private async Task<List<SourceLibraryMaterial>> LoadSourceMaterialsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT
    Id,
    Title,
    Category,
    EducationStage,
    RecommendedMinAge,
    RecommendedMaxAge,
    SkillFocus,
    Description,
    IsPrintable,
    PageCount,
    HasIllustrations,
    CoverImageRelativePath,
    RelativePath,
    ImportVersion,
    SourceUpdatedAtUtc,
    UpdatedAtUtc
FROM SourceMaterials
WHERE PageCount > 0
ORDER BY EducationStage, Category, Title;
""";

        var items = new List<SourceLibraryMaterial>();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var title = reader.GetString(1);
            var category = reader.GetString(2);
            var educationStage = reader.GetString(3);
            var recommendedMinAge = reader.GetInt32(4);
            var recommendedMaxAge = reader.GetInt32(5);
            var skillFocus = reader.GetString(6);
            var description = reader.GetString(7);
            var sourceIsPrintable = reader.GetBoolean(8);
            var pageCount = reader.GetInt32(9);
            var hasIllustrations = reader.GetBoolean(10);
            var coverImageRelativePath = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
            var sourceRelativePath = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);
            var importVersion = reader.GetInt32(13);
            var sourceUpdatedAtUtc = reader.IsDBNull(14) ? DateTime.MinValue : reader.GetDateTime(14);
            var updatedAtUtc = reader.IsDBNull(15) ? DateTime.MinValue : reader.GetDateTime(15);
            var resolvedIsPrintable = FamilyLibraryMaterialClassifier.IsPrintable(
                sourceIsPrintable,
                title,
                category,
                sourceRelativePath,
                skillFocus,
                description);

            var item = new SourceLibraryMaterial
            {
                Id = reader.GetGuid(0),
                Title = title,
                Category = category,
                EducationStage = educationStage,
                RecommendedMinAge = recommendedMinAge,
                RecommendedMaxAge = recommendedMaxAge,
                SkillFocus = skillFocus,
                Description = description,
                IsPrintable = resolvedIsPrintable,
                PageCount = pageCount,
                HasIllustrations = hasIllustrations,
                CoverImageRelativePath = coverImageRelativePath,
                SourceRelativePath = sourceRelativePath,
                ImportVersion = importVersion,
                SourceUpdatedAtUtc = sourceUpdatedAtUtc,
                UpdatedAtUtc = updatedAtUtc
            };

            item.CollectionLabel = ClassifyCollection(item.Title, item.Category, item.SkillFocus);
            item.SourceSyncToken = $"{item.ImportVersion}:{item.SourceUpdatedAtUtc:O}:{item.UpdatedAtUtc:O}:{item.PageCount}:{item.CoverImageRelativePath}:{(item.IsPrintable ? "P" : "B")}";
            items.Add(item);
        }

        return items;
    }

    private static async Task<List<SourceLibraryPage>> LoadSourcePagesAsync(SqlConnection connection, Guid materialId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT
    PageNumber,
    TextContent,
    ImageRelativePath
FROM SourceMaterialPages
WHERE SourceMaterialId = @materialId
ORDER BY PageNumber;
""";

        var items = new List<SourceLibraryPage>();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@materialId", materialId);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SourceLibraryPage
            {
                PageNumber = reader.GetInt32(0),
                TextContent = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                ImageRelativePath = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            });
        }

        return items;
    }

    private void CopyAssetIfNeeded(string targetAssetsRootPath, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var normalizedRelativePath = NormalizeRelativePath(relativePath);
        var sourceFilePath = Path.Combine(_options.SourceAssetsRootPath, normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(sourceFilePath))
        {
            return;
        }

        var targetFilePath = Path.Combine(targetAssetsRootPath, normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var targetDirectory = Path.GetDirectoryName(targetFilePath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        if (!File.Exists(targetFilePath) || File.GetLastWriteTimeUtc(sourceFilePath) > File.GetLastWriteTimeUtc(targetFilePath))
        {
            File.Copy(sourceFilePath, targetFilePath, overwrite: true);
        }
    }

    private void DeleteTargetAssetDirectory(string targetAssetsRootPath, Guid materialId)
    {
        var directoryPath = Path.Combine(targetAssetsRootPath, materialId.ToString("N"));
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    private string ResolveTargetAssetsRootPath()
    {
        var configuredPath = _options.TargetAssetsRootPath;
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(environment.ContentRootPath, configuredPath);
    }

    private static string ClassifyCollection(string title, string category, string skillFocus)
    {
        var normalizedTitle = NormalizeText(title);
        var normalizedCategory = NormalizeText(category);
        var normalizedSkillFocus = NormalizeText(skillFocus);

        if (normalizedCategory.Contains("historias biblicas", StringComparison.Ordinal)
            || normalizedTitle.Contains("jesus", StringComparison.Ordinal)
            || normalizedTitle.Contains("noe", StringComparison.Ordinal)
            || normalizedTitle.Contains("davi", StringComparison.Ordinal)
            || normalizedTitle.Contains("moises", StringComparison.Ordinal)
            || normalizedTitle.Contains("danie", StringComparison.Ordinal)
            || normalizedTitle.Contains("adao", StringComparison.Ordinal)
            || normalizedTitle.Contains("eva", StringComparison.Ordinal))
        {
            return "Histórias Bíblicas";
        }

        if (normalizedCategory.Contains("livrinhos", StringComparison.Ordinal)
            || normalizedCategory.Contains("fabulas", StringComparison.Ordinal)
            || normalizedCategory.Contains("literatura", StringComparison.Ordinal)
            || normalizedCategory.Contains("poemas", StringComparison.Ordinal)
            || normalizedCategory.Contains("poesias", StringComparison.Ordinal)
            || normalizedCategory.Contains("leitura em voz alta", StringComparison.Ordinal))
        {
            return "Histórias Infantis";
        }

        if (normalizedCategory.Contains("teologia", StringComparison.Ordinal)
            || normalizedTitle.Contains("biblia", StringComparison.Ordinal)
            || normalizedTitle.Contains("biblico", StringComparison.Ordinal)
            || normalizedTitle.Contains("biblica", StringComparison.Ordinal)
            || normalizedTitle.Contains("catecismo", StringComparison.Ordinal)
            || normalizedTitle.Contains("culto familiar", StringComparison.Ordinal)
            || normalizedSkillFocus.Contains("formacao biblica", StringComparison.Ordinal))
        {
            return "Princípios Bíblicos";
        }

        return "Outros";
    }

    private static string NormalizeRelativePath(string? relativePath)
    {
        return string.IsNullOrWhiteSpace(relativePath)
            ? string.Empty
            : relativePath.Replace('\\', '/').TrimStart('/');
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }

    private sealed class SourceLibraryMaterial
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string EducationStage { get; init; } = string.Empty;
        public int RecommendedMinAge { get; init; }
        public int RecommendedMaxAge { get; init; }
        public string SkillFocus { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string CollectionLabel { get; set; } = string.Empty;
        public bool IsPrintable { get; init; }
        public int PageCount { get; init; }
        public bool HasIllustrations { get; init; }
        public string CoverImageRelativePath { get; init; } = string.Empty;
        public string SourceRelativePath { get; init; } = string.Empty;
        public int ImportVersion { get; init; }
        public DateTime SourceUpdatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
        public string SourceSyncToken { get; set; } = string.Empty;
    }

    private sealed class SourceLibraryPage
    {
        public int PageNumber { get; init; }
        public string TextContent { get; init; } = string.Empty;
        public string ImageRelativePath { get; init; } = string.Empty;
    }
}
