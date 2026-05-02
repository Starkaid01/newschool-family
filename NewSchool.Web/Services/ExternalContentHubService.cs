using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using System.Globalization;
using System.Text;

namespace NewSchool.Web.Services;

public class ExternalContentHubService(ApplicationDbContext db)
{
    public Task<List<ParentAcademyCategoryViewModel>> BuildAcademyCategoriesAsync(
        ChildProfile? child,
        IUrlHelper url)
    {
        var age = child is null ? (int?)null : Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 0, 14);
        var items = ExternalContentCatalog.Build()
            .Where(item => age is null || (item.AgeMin <= age.Value && item.AgeMax >= age.Value))
            .OrderBy(item => item.SortOrder)
            .ToList();

        var categories = items
            .GroupBy(item => new { item.CategoryTitle, item.CategoryDescription })
            .Select(group => new ParentAcademyCategoryViewModel
            {
                Title = group.Key.CategoryTitle,
                Description = group.Key.CategoryDescription,
                Resources = group
                    .Select(item => MapAcademyResource(item, child?.Id, url))
                    .ToList()
            })
            .ToList();

        return Task.FromResult(categories);
    }

    public async Task<List<ExternalContentProgressCardViewModel>> BuildChildRecommendationsAsync(
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IUrlHelper url)
    {
        var age = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 0, 14);
        var progressMap = await db.ChildExternalContentProgressEntries
            .Where(x => x.ChildId == child.Id)
            .ToDictionaryAsync(x => x.ContentSlug, x => x);
        var searchText = NormalizeForSearch(string.Join(' ',
            blocks.SelectMany(block => new[]
            {
                block.Title,
                block.SkillName,
                block.Goal,
                block.Materials
            }).Append(child.FamilyGoalTrack)));
        var activeDomains = blocks
            .Select(x => x.Domain)
            .Distinct()
            .ToHashSet();

        return ExternalContentCatalog.Build()
            .Where(item => item.AgeMin <= age && item.AgeMax >= age)
            .OrderByDescending(item => ScoreItem(item, child, activeDomains, searchText, progressMap.ContainsKey(item.Slug)))
            .ThenBy(item => item.SortOrder)
            .Take(4)
            .Select(item =>
            {
                progressMap.TryGetValue(item.Slug, out var progress);
                return new ExternalContentProgressCardViewModel
                {
                    Slug = item.Slug,
                    Title = item.Title,
                    Summary = item.Summary,
                    RecommendedReason = BuildRecommendationReason(item, child, activeDomains, progress is not null),
                    Provider = item.SourceLabel,
                    AreaLabel = FormatDomain(item.Domain),
                    AudienceLabel = item.AudienceLabel,
                    AudienceChipClass = item.AudienceChipClass,
                    Completed = progress is not null,
                    CompletedAt = progress?.CompletedAt,
                    GuideUrl = url.Action("ExternalContent", "Parent", new { slug = item.Slug, childId = child.Id }) ?? string.Empty,
                    OfficialUrl = item.OfficialUrl,
                    OfficialActionLabel = item.OfficialActionLabel,
                    CompletionLabel = progress is null
                        ? "Ainda nao concluido"
                        : $"Concluido em {progress.CompletedAt.ToLocalTime():dd/MM}"
                };
            })
            .ToList();
    }

    public async Task<Dictionary<Guid, ExternalContentProgressSummary>> BuildProgressSummariesAsync(
        IReadOnlyCollection<Guid> childIds)
    {
        var result = childIds.ToDictionary(id => id, _ => new ExternalContentProgressSummary());
        if (childIds.Count == 0)
        {
            return result;
        }

        var progressEntries = await db.ChildExternalContentProgressEntries
            .Where(x => childIds.Contains(x.ChildId))
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync();

        foreach (var grouping in progressEntries.GroupBy(x => x.ChildId))
        {
            result[grouping.Key] = new ExternalContentProgressSummary
            {
                CompletedCount = grouping.Count(),
                LatestTitle = grouping.First().ContentTitle
            };
        }

        return result;
    }

    public async Task<ExternalContentGuideViewModel> BuildGuideAsync(
        string slug,
        ChildProfile? selectedChild,
        IReadOnlyList<ParentAcademyChildOptionViewModel> children,
        string? focus,
        IUrlHelper url)
    {
        var item = ExternalContentCatalog.GetRequired(slug);
        var selectedFocus = item.FocusOptions.FirstOrDefault(x =>
            string.Equals(x.Slug, focus, StringComparison.OrdinalIgnoreCase));
        var resolvedParentSteps = ResolveParentSteps(item, selectedFocus);
        var resolvedWeeklyRhythm = ResolveWeeklyRhythm(item, selectedFocus);
        var resolvedEvidenceIdeas = ResolveEvidenceIdeas(item, selectedFocus);
        var resolvedStarterMaterials = ResolveStarterMaterials(item, selectedFocus);
        ChildExternalContentProgress? progress = null;
        if (selectedChild is not null)
        {
            progress = await db.ChildExternalContentProgressEntries
                .FirstOrDefaultAsync(x => x.ChildId == selectedChild.Id && x.ContentSlug == slug);
        }

        return new ExternalContentGuideViewModel
        {
            Slug = item.Slug,
            Title = item.Title,
            Subtitle = item.FormatLabel,
            AudienceLabel = item.AudienceLabel,
            AudienceChipClass = item.AudienceChipClass,
            SourceLabel = item.SourceLabel,
            SourceUrl = item.OfficialUrl,
            Summary = item.Summary,
            AdaptedHeadline = string.IsNullOrWhiteSpace(item.AdaptedHeadline)
                ? "Como usar com seu filho"
                : item.AdaptedHeadline,
            Intro = string.IsNullOrWhiteSpace(item.Intro)
                ? item.PortugueseGuideNote
                : item.Intro,
            ParentSteps = resolvedParentSteps,
            WeeklyRhythm = resolvedWeeklyRhythm,
            EvidenceIdeas = resolvedEvidenceIdeas,
            FolderHighlights = (item.FolderHighlights.Count > 0 ? item.FolderHighlights : item.Highlights).ToList(),
            StarterMaterials = resolvedStarterMaterials,
            SelectedFocusSlug = selectedFocus?.Slug ?? string.Empty,
            SelectedFocusTitle = selectedFocus?.Title ?? string.Empty,
            SelectedFocusSummary = selectedFocus?.Summary ?? string.Empty,
            SelectedFocusWhenToUse = selectedFocus?.WhenToUse ?? string.Empty,
            SelectedFocusHowItEnters = selectedFocus?.HowItEnters ?? string.Empty,
            SelectedFocusEvidenceIdea = selectedFocus?.EvidenceIdea ?? string.Empty,
            SelectedFocusOfficialUrl = string.IsNullOrWhiteSpace(selectedFocus?.OfficialUrl)
                ? item.OfficialUrl
                : selectedFocus!.OfficialUrl,
            SelectedFocusActionLabel = string.IsNullOrWhiteSpace(selectedFocus?.ActionLabel)
                ? item.OfficialActionLabel
                : selectedFocus!.ActionLabel,
            FocusLinks = item.FocusOptions
                .Select(option => new ExternalContentFocusLinkViewModel
                {
                    Slug = option.Slug,
                    Title = option.Title,
                    Summary = option.Summary,
                    Url = url.Action("ExternalContent", "Parent", new
                    {
                        slug = item.Slug,
                        childId = selectedChild?.Id,
                        focus = option.Slug
                    }) ?? string.Empty
                })
                .ToList(),
            SelectedChildId = selectedChild?.Id,
            SelectedChildName = selectedChild?.FullName ?? string.Empty,
            Children = children.ToList(),
            IsCompletedForSelectedChild = progress is not null,
            CompletedAt = progress?.CompletedAt
        };
    }

    public async Task MarkCompletedAsync(Guid childId, Guid parentId, string slug)
    {
        var child = await db.Children.FirstOrDefaultAsync(x => x.Id == childId && x.ParentId == parentId)
            ?? throw new InvalidOperationException("Crianca nao encontrada para este responsavel.");

        var item = ExternalContentCatalog.GetRequired(slug);
        var progress = await db.ChildExternalContentProgressEntries
            .FirstOrDefaultAsync(x => x.ChildId == child.Id && x.ContentSlug == slug);

        if (progress is null)
        {
            progress = new ChildExternalContentProgress
            {
                ChildId = child.Id,
                ContentSlug = item.Slug,
                ContentTitle = item.Title,
                Provider = item.SourceLabel,
                AreaLabel = FormatDomain(item.Domain),
                CompletedAt = DateTime.UtcNow
            };
            db.ChildExternalContentProgressEntries.Add(progress);
        }
        else
        {
            progress.CompletedAt = DateTime.UtcNow;
            progress.ContentTitle = item.Title;
            progress.Provider = item.SourceLabel;
            progress.AreaLabel = FormatDomain(item.Domain);
        }

        await db.SaveChangesAsync();
    }

    private ParentAcademyResourceViewModel MapAcademyResource(
        ExternalContentCatalogItem item,
        Guid? childId,
        IUrlHelper url)
    {
        return new ParentAcademyResourceViewModel
        {
            Title = item.Title,
            Summary = item.Summary,
            WhyItMatters = item.WhyItMatters,
            PortugueseGuideNote = item.PortugueseGuideNote,
            FormatLabel = item.FormatLabel,
            DurationLabel = item.AudienceLabel,
            SourceLabel = item.SourceLabel,
            OwnershipLabel = "Produto de terceiro",
            IsThirdParty = true,
            GuideLabel = item.HasGuide ? item.GuideLabel : string.Empty,
            GuideUrl = item.HasGuide
                ? url.Action("ExternalContent", "Parent", new { slug = item.Slug, childId }) ?? string.Empty
                : string.Empty,
            AccessLabel = item.OfficialActionLabel,
            Url = item.OfficialUrl,
            Highlights = item.FolderHighlights.Count > 0 ? item.FolderHighlights.ToList() : item.Highlights.ToList(),
            AudienceLabel = item.AudienceLabel,
            AudienceChipClass = item.AudienceChipClass
        };
    }

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static string FormatDomain(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem",
        LearningDomain.Math => "Matematica",
        LearningDomain.World => "Mundo real",
        LearningDomain.ExecutiveFunction => "Autonomia",
        _ => "Geral"
    };

    private static int ScoreItem(
        ExternalContentCatalogItem item,
        ChildProfile child,
        IReadOnlySet<LearningDomain> activeDomains,
        string searchText,
        bool alreadyCompleted)
    {
        var score = 100;

        if (activeDomains.Contains(item.Domain))
        {
            score += 28;
        }

        if (item.GoalTracks.Contains(child.FamilyGoalTrack, StringComparer.OrdinalIgnoreCase))
        {
            score += 24;
        }

        var keywordMatches = SplitKeywords(item.MatchKeywords)
            .Count(keyword => searchText.Contains(keyword, StringComparison.Ordinal));
        score += keywordMatches * 7;

        if (!alreadyCompleted)
        {
            score += 6;
        }

        return score;
    }

    private static string BuildRecommendationReason(
        ExternalContentCatalogItem item,
        ChildProfile child,
        IReadOnlySet<LearningDomain> activeDomains,
        bool alreadyCompleted)
    {
        if (activeDomains.Contains(item.Domain) &&
            item.GoalTracks.Contains(child.FamilyGoalTrack, StringComparer.OrdinalIgnoreCase))
        {
            return $"Entrou porque a materia de hoje puxa {FormatDomain(item.Domain).ToLowerInvariant()} e isso combina com a trilha principal da familia.";
        }

        if (activeDomains.Contains(item.Domain))
        {
            return $"Entrou porque reforca a materia que apareceu na aula de hoje: {FormatDomain(item.Domain).ToLowerInvariant()}.";
        }

        if (item.GoalTracks.Contains(child.FamilyGoalTrack, StringComparer.OrdinalIgnoreCase))
        {
            return "Entrou porque conversa com o objetivo principal escolhido para esta crianca.";
        }

        return alreadyCompleted
            ? "Ja foi usado antes e continua servindo como reforco rapido para a idade atual."
            : "Entrou como apoio organizado em portugues para a fase atual da crianca.";
    }

    private static List<string> SplitKeywords(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeForSearch)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(chars)
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }

    private static List<string> ResolveParentSteps(
        ExternalContentCatalogItem item,
        ExternalContentFocusOption? selectedFocus)
    {
        if (item.ParentSteps.Count > 0)
        {
            return item.ParentSteps.ToList();
        }

        var focusTitle = selectedFocus?.Title ?? item.Title;
        return
        [
            $"Abra {focusTitle.ToLowerInvariant()} como apoio curto, sem transformar a rotina em excesso de tela.",
            string.IsNullOrWhiteSpace(selectedFocus?.HowItEnters)
                ? "Escolha uma parte pequena, explique com calma e feche a atividade no mesmo momento."
                : selectedFocus.HowItEnters,
            string.IsNullOrWhiteSpace(selectedFocus?.EvidenceIdea)
                ? "Se fizer sentido, guarde depois uma foto, um video curto ou uma observacao simples em Evidencias."
                : $"Feche com esta prova: {selectedFocus.EvidenceIdea}"
        ];
    }

    private static List<string> ResolveWeeklyRhythm(
        ExternalContentCatalogItem item,
        ExternalContentFocusOption? selectedFocus)
    {
        if (item.WeeklyRhythm.Count > 0)
        {
            return item.WeeklyRhythm.ToList();
        }

        var focusTitle = selectedFocus?.Title ?? item.Title;
        return
        [
            $"Dia 1: apresentar {focusTitle.ToLowerInvariant()} de forma curta e leve.",
            "Dia 2: repetir com a crianca participando mais e com menos ajuda.",
            "Dia 3: decidir se vale repetir, variar ou avancar para o proximo passo."
        ];
    }

    private static List<string> ResolveEvidenceIdeas(
        ExternalContentCatalogItem item,
        ExternalContentFocusOption? selectedFocus)
    {
        var ideas = item.EvidenceIdeas.ToList();
        if (!string.IsNullOrWhiteSpace(selectedFocus?.EvidenceIdea))
        {
            ideas.Insert(0, selectedFocus.EvidenceIdea);
        }

        if (ideas.Count > 0)
        {
            return ideas
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return
        [
            "Foto da atividade principal concluida.",
            "Video curto da crianca explicando o que fez.",
            "Texto simples do adulto dizendo o que conseguiu e onde precisou de ajuda."
        ];
    }

    private static List<string> ResolveStarterMaterials(
        ExternalContentCatalogItem item,
        ExternalContentFocusOption? selectedFocus)
    {
        if (item.StarterMaterials.Count > 0)
        {
            return item.StarterMaterials.ToList();
        }

        if (item.FormatLabel.Contains("PDF", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                "celular ou computador para abrir o PDF",
                "papel impresso se fizer sentido",
                "lapis ou canetinha",
                string.IsNullOrWhiteSpace(selectedFocus?.Title)
                    ? "10 a 15 minutos livres"
                    : $"10 a 15 minutos para trabalhar {selectedFocus.Title.ToLowerInvariant()}"
            ];
        }

        return
        [
            "celular aberto no material certo",
            "papel para resposta curta",
            "lapis de cor ou grafite",
            "rotina curta e sem pressa"
        ];
    }
}

public sealed class ExternalContentProgressSummary
{
    public int CompletedCount { get; init; }
    public string LatestTitle { get; init; } = string.Empty;
}
