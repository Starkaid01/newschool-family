using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class FamilyLibraryService(
    ApplicationDbContext db,
    IMemoryCache memoryCache)
{
    private const string WeeklyReadingCompletionKind = "weekly_reading";
    private static readonly TimeSpan LibraryIndexCacheDuration = TimeSpan.FromMinutes(3);

    private static readonly string[] CollectionFilterOrder =
    [
        ProprietaryFamilyLibraryCatalog.CollectionLabel,
        "Histórias Infantis",
        "Histórias Bíblicas",
        "Princípios Bíblicos",
        "Outros"
    ];

    private static readonly string[] StageFilterOrder =
    [
        "Educação Infantil",
        "Ensino Fundamental",
        "Ensino Médio",
        "Família"
    ];

    private static readonly Regex SchoolYearRegex = new(@"(?<!\d)([1-9])\s*(?:º|o)?\s*ano\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AgeRegex = new(@"(?<!\d)(\d{1,2})\s*(?:anos?|ano)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> IgnoredFocusTokens =
    [
        "para",
        "com",
        "dos",
        "das",
        "uma",
        "mais",
        "menos",
        "hoje",
        "casa",
        "real",
        "vida",
        "atividade",
        "atividades",
        "sistema",
        "guia",
        "etapa",
        "ano"
    ];

    private static readonly Dictionary<LearningDomain, string[]> DomainKeywords = new()
    {
        [LearningDomain.Language] =
        [
            "alfabeto", "leitura", "literatura", "linguagem", "historia", "historias", "fabulas", "livrinhos",
            "poesia", "poema", "voz alta", "fonologica", "silabas", "escrita", "copywork", "reconto", "texto"
        ],
        [LearningDomain.Math] =
        [
            "matematica", "numero", "numeros", "contagem", "soma", "subtracao", "formas", "medida", "padrao",
            "tabuada", "flashcards", "calculo", "quantidade", "tempo", "horas"
        ],
        [LearningDomain.World] =
        [
            "ciencias", "natureza", "historia", "geografia", "biografia", "timeline", "brasil", "animais",
            "plantas", "experimento", "mundo", "descoberta", "rio", "mapa"
        ],
        [LearningDomain.Science] =
        [
            "ciencias", "natureza", "animais", "plantas", "experimento", "descoberta", "corpo", "agua",
            "clima", "mistura", "observacao", "fenomeno", "bioma"
        ],
        [LearningDomain.History] =
        [
            "historia", "biografia", "timeline", "tempo", "memoria", "passado", "familia", "acontecimento",
            "fonte", "personagem", "linha do tempo"
        ],
        [LearningDomain.Geography] =
        [
            "geografia", "brasil", "rio", "mapa", "territorio", "regiao", "bairro", "cidade",
            "paisagem", "ambiente", "localizacao", "orientacao"
        ],
        [LearningDomain.ExecutiveFunction] =
        [
            "brincadeiras", "desenvolvimento motor", "rotina", "autonomia", "coordenação", "coordenacao",
            "organizacao", "organização", "atencao", "atenção", "responsabilidade"
        ]
    };

    public async Task<FamilyLibraryHomeViewModel> BuildHomeAsync(
        Guid userId,
        ChildProfile? child,
        string? searchTerm,
        string? collection,
        string? stage,
        string? ageFilter,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var library = await LoadLibraryIndexAsync(userId, cancellationToken);
        var cards = library.Books;
        var resolvedAgeFilter = ResolveAgeFilter(ageFilter, child, cards.Select(book => book.AgeLabel));

        var filteredBooks = cards
            .Where(book => MatchesFilters(book, searchTerm, collection, stage, resolvedAgeFilter))
            .OrderByDescending(book => book.IsStarted)
            .ThenByDescending(book => book.LastReadAtUtc)
            .ThenBy(book => GetStageSortOrder(book.EducationStage))
            .ThenBy(book => book.Category)
            .ThenBy(book => book.Title)
            .ToList();
        var preferredDomain = child is null ? (LearningDomain?)null : GetPreferredLibraryDomain(child.FamilyGoalTrack);
        var childAge = child is null ? 0 : CalculateAge(child.BirthDate, DateTime.Today);

        var spotlightBook = child is null
            ? cards.OrderByDescending(book => book.IsStarted).ThenBy(book => book.Title).FirstOrDefault() is { } genericBook
                ? MapRecommendation(genericBook, url, "Abrir livro", "Abrir biblioteca", url.Action("Index", "Library") ?? string.Empty, "Leitura pronta para a família começar sem procurar muito.")
                : null
            : PickBestBook(cards, childAge, preferredDomain, [], url, "Abrir livro recomendado", child.Id);
        var spotlightPrintable = child is null
            ? library.Printables.FirstOrDefault() is { } genericPrintable
                ? MapRecommendation(genericPrintable, url, "Abrir imprimível", "Ver todos os imprimíveis", url.Action("Printables", "Library") ?? string.Empty, "Imprimível para usar quando a semana pedir mais prática no papel.")
                : null
            : PickBestPrintable(
                library.Printables,
                childAge,
                preferredDomain,
                [],
                url,
                "Abrir imprimível recomendado",
                child.Id);
        var currentBook = child is null
            ? cards
                .Where(book => book.IsStarted && !book.IsCompleted)
                .OrderByDescending(book => book.LastReadAtUtc)
                .ThenBy(book => book.Title)
                .FirstOrDefault()
            : PickCurrentBookForChild(cards, childAge, preferredDomain);

        return new FamilyLibraryHomeViewModel
        {
            TotalBooks = cards.Count,
            ProprietaryBooks = cards.Count(book => string.Equals(book.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase)),
            ProprietaryPrintables = library.Printables.Count(card => string.Equals(card.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase)),
            FavoriteBooks = cards.Count(book => book.IsFavorite),
            CompletedBooks = cards.Count(book => book.IsCompleted),
            InProgressBooks = cards.Count(book => book.IsStarted && !book.IsCompleted),
            SelectedChildId = child?.Id,
            SelectedChildName = child?.FullName ?? string.Empty,
            PersonalizationHeadline = child is null
                ? "Biblioteca da família"
                : $"Biblioteca de {child.FullName}",
            PersonalizationSummary = child is null
                ? "Abra primeiro um livro ou um imprimível sem sair procurando no acervo inteiro."
                : $"Abra primeiro o livro certo para {child.FullName}. O restante do acervo fica abaixo só quando realmente precisar.",
            CurrentBook = currentBook,
            SpotlightBook = spotlightBook,
            SpotlightPrintable = spotlightPrintable,
            SearchTerm = searchTerm ?? string.Empty,
            SelectedCollection = collection ?? string.Empty,
            SelectedStage = stage ?? string.Empty,
            SelectedAgeFilter = resolvedAgeFilter,
            CollectionFilters = CollectionFilterOrder
                .Where(filter => cards.Any(book => string.Equals(book.CollectionLabel, filter, StringComparison.OrdinalIgnoreCase)))
                .ToList(),
            StageFilters = StageFilterOrder
                .Where(filter => cards.Any(book => string.Equals(book.EducationStage, filter, StringComparison.OrdinalIgnoreCase)))
                .ToList(),
            AgeFilters = cards
                .Select(book => book.AgeLabel)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetAgeFilterSortOrder)
                .ToList(),
            CurriculumShelves = [],
            FavoriteItems = cards
                .Where(book => book.IsFavorite)
                .OrderByDescending(book => book.LastReadAtUtc)
                .ThenBy(book => book.Title)
                .ToList(),
            CompletedItems = cards
                .Where(book => book.IsCompleted)
                .OrderByDescending(book => book.LastReadAtUtc)
                .ThenBy(book => book.Title)
                .ToList(),
            Books = filteredBooks
        };
    }

    public async Task<FamilyLibraryCurriculumBridgeViewModel> BuildCurriculumBridgeAsync(
        Guid userId,
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var library = await LoadLibraryIndexAsync(userId, cancellationToken);
        var readingHistory = await LoadChildReadingHistoryAsync(userId, child.Id, url, cancellationToken);
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        var dominantDomain = GetDominantDomain(blocks);
        var focusTokens = ExtractFocusTokens(blocks);
        var dominantDomainLabel = dominantDomain.HasValue ? FormatDomain(dominantDomain.Value) : "o foco de hoje";

        return new FamilyLibraryCurriculumBridgeViewModel
        {
            Headline = "Livro e atividade da etapa",
            Summary = $"Se a família quiser reforçar a rotina com leitura ou papel, o sistema já separou abaixo o que mais combina com {dominantDomainLabel.ToLowerInvariant()} neste momento.",
            LibraryUrl = url.Action("Index", "Library", new { childId = child.Id }) ?? string.Empty,
            RecommendedBook = PickBestBook(library.Books, age, dominantDomain, focusTokens, url, "Abrir livro agora", child.Id),
            RecommendedPrintable = PickBestPrintable(library.Printables, age, dominantDomain, focusTokens, url, "Abrir atividade de papel", child.Id),
            WeeklyReading = BuildWeeklyReading(library, child, dominantDomain, focusTokens, url, readingHistory)
        };
    }

    public async Task<FamilyLibraryAnnualSpineViewModel> BuildAnnualReadingSpineAsync(
        Guid userId,
        ChildProfile child,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var library = await LoadLibraryIndexAsync(userId, cancellationToken);
        var readingHistory = await LoadChildReadingHistoryAsync(userId, child.Id, url, cancellationToken);
        return BuildAnnualReadingSpine(library, child, url, readingHistory);
    }

    public async Task MarkWeeklyReadingCompleteAsync(
        Guid userId,
        Guid childId,
        Guid materialId,
        int phaseNumber,
        string? periodKey,
        int weekNumber,
        string? phaseLabel,
        string? goalLabel,
        CancellationToken cancellationToken = default)
    {
        if (childId == Guid.Empty || materialId == Guid.Empty || phaseNumber <= 0)
        {
            return;
        }

        var childExists = await db.Children
            .AnyAsync(child => child.Id == childId && child.ParentId == userId, cancellationToken);
        if (!childExists)
        {
            return;
        }

        var material = await db.FamilyLibraryMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == materialId, cancellationToken);
        if (material is null || !IsBookMaterial(material))
        {
            return;
        }

        var normalizedPhaseLabel = string.IsNullOrWhiteSpace(phaseLabel)
            ? $"{phaseNumber}ª etapa"
            : phaseLabel.Trim();
        var normalizedPeriodKey = string.IsNullOrWhiteSpace(periodKey)
            ? BuildPeriodKey(DateTime.Today, Math.Clamp(weekNumber, 1, 4))
            : periodKey.Trim();
        var normalizedWeekNumber = Math.Clamp(weekNumber <= 0 ? GetWeekOfMonth(DateTime.Today.Day) : weekNumber, 1, 4);
        var normalizedGoalLabel = (goalLabel ?? string.Empty).Trim();
        var now = DateTime.UtcNow;

        var progressEntry = await db.ChildLibraryReadingProgressEntries
            .FirstOrDefaultAsync(
                item => item.ChildId == childId
                        && item.ParentUserId == userId
                        && item.MaterialId == materialId
                        && item.PhaseNumber == phaseNumber
                        && item.CompletionKind == WeeklyReadingCompletionKind
                        && item.PeriodKey == normalizedPeriodKey,
                cancellationToken);

        if (progressEntry is null)
        {
            progressEntry = new ChildLibraryReadingProgress
            {
                ChildId = childId,
                ParentUserId = userId,
                MaterialId = materialId,
                PhaseNumber = phaseNumber,
                PhaseLabel = normalizedPhaseLabel,
                PeriodKey = normalizedPeriodKey,
                WeekNumber = normalizedWeekNumber,
                CompletionKind = WeeklyReadingCompletionKind,
                GoalLabel = normalizedGoalLabel,
                CompletedAtUtc = now,
                UpdatedAtUtc = now
            };

            db.ChildLibraryReadingProgressEntries.Add(progressEntry);
        }
        else
        {
            progressEntry.PhaseLabel = normalizedPhaseLabel;
            progressEntry.PeriodKey = normalizedPeriodKey;
            progressEntry.WeekNumber = normalizedWeekNumber;
            progressEntry.GoalLabel = normalizedGoalLabel;
            progressEntry.CompletedAtUtc = now;
            progressEntry.UpdatedAtUtc = now;
        }

        var state = await db.FamilyLibraryUserStates
            .FirstOrDefaultAsync(item => item.UserId == userId && item.MaterialId == materialId, cancellationToken);

        if (state is null)
        {
            state = new FamilyLibraryUserState
            {
                UserId = userId,
                MaterialId = materialId,
                CurrentPageNumber = 1,
                StartedAtUtc = now,
                LastReadAtUtc = now,
                UpdatedAtUtc = now
            };

            db.FamilyLibraryUserStates.Add(state);
        }
        else
        {
            state.CurrentPageNumber = Math.Max(1, state.CurrentPageNumber);
            state.StartedAtUtc ??= now;
            state.LastReadAtUtc = now;
            state.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        InvalidateLibraryIndex(userId);
    }

    public async Task<Dictionary<string, FamilyLibraryCurriculumBridgeViewModel>> BuildSubjectSupportMapAsync(
        Guid userId,
        ChildProfile child,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var library = await LoadLibraryIndexAsync(userId, cancellationToken);
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        var result = new Dictionary<string, FamilyLibraryCurriculumBridgeViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
        {
            var domainLabel = FormatDomain(domain);
            result[domainLabel] = new FamilyLibraryCurriculumBridgeViewModel
            {
                Headline = $"Material de apoio para {domainLabel.ToLowerInvariant()}",
                Summary = $"Esses materiais ajudam quando a família quer revisar {domainLabel.ToLowerInvariant()} sem sair procurando por conta própria.",
                LibraryUrl = url.Action("Index", "Library", new { childId = child.Id }) ?? string.Empty,
                RecommendedBook = PickBestBook(library.Books, age, domain, [], url, "Abrir livro", child.Id),
                RecommendedPrintable = PickBestPrintable(library.Printables, age, domain, [], url, "Abrir imprimível", child.Id)
            };
        }

        return result;
    }

    public async Task<Dictionary<Guid, FamilyLibraryRecommendationViewModel>> BuildBlockPrintableMapAsync(
        Guid userId,
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IUrlHelper url,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel>? suggestions = null,
        CancellationToken cancellationToken = default)
    {
        if (blocks.Count == 0)
        {
            return new Dictionary<Guid, FamilyLibraryRecommendationViewModel>();
        }

        var library = await LoadLibraryIndexAsync(userId, cancellationToken);
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        var result = new Dictionary<Guid, FamilyLibraryRecommendationViewModel>();

        foreach (var block in blocks)
        {
            var suggestion = suggestions is not null && suggestions.TryGetValue(block.Id, out var taskSuggestion)
                ? taskSuggestion
                : null;
            var focusTokens = ExtractFocusTokens(block, suggestion);
            var lessonCandidates = FilterPrintableCandidatesForLesson(library.Printables, age, block.Domain, focusTokens);
            var printable = PickBestPrintable(
                lessonCandidates,
                age,
                block.Domain,
                focusTokens,
                url,
                "Abrir trabalho para imprimir",
                child.Id);

            if (printable is not null)
            {
                result[block.Id] = printable;
            }
        }

        return result;
    }

    public async Task<FamilyLibraryReaderViewModel?> BuildReaderAsync(Guid userId, Guid materialId, int? requestedPageNumber, CancellationToken cancellationToken = default)
    {
        var material = await db.FamilyLibraryMaterials
            .Include(item => item.Pages.OrderBy(page => page.PageNumber))
            .FirstOrDefaultAsync(item => item.Id == materialId, cancellationToken);

        if (material is null || !IsBookMaterial(material))
        {
            return null;
        }

        var state = await db.FamilyLibraryUserStates
            .FirstOrDefaultAsync(item => item.UserId == userId && item.MaterialId == materialId, cancellationToken);

        var pageNumber = requestedPageNumber ?? state?.CurrentPageNumber ?? 1;
        pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, material.PageCount));

        if (state is null)
        {
            state = new FamilyLibraryUserState
            {
                UserId = userId,
                MaterialId = materialId,
                CurrentPageNumber = pageNumber,
                StartedAtUtc = DateTime.UtcNow,
                LastReadAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            if (pageNumber >= material.PageCount)
            {
                state.IsCompleted = true;
                state.CompletedAtUtc = DateTime.UtcNow;
            }

            db.FamilyLibraryUserStates.Add(state);
        }
        else
        {
            state.CurrentPageNumber = pageNumber;
            state.StartedAtUtc ??= DateTime.UtcNow;
            state.LastReadAtUtc = DateTime.UtcNow;
            state.UpdatedAtUtc = DateTime.UtcNow;

            if (pageNumber >= material.PageCount)
            {
                state.IsCompleted = true;
                state.CompletedAtUtc ??= DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        InvalidateLibraryIndex(userId);

        return new FamilyLibraryReaderViewModel
        {
            Id = material.Id,
            Title = material.Title,
            Category = material.Category,
            EducationStage = material.EducationStage,
            CollectionLabel = material.CollectionLabel,
            SkillFocus = material.SkillFocus,
            Description = material.Description,
            PageCount = material.PageCount,
            CurrentPageNumber = pageNumber,
            IsFavorite = state.IsFavorite,
            IsCompleted = state.IsCompleted,
            CoverImageUrl = BuildImageUrl(material.CoverImageRelativePath),
            Pages = material.Pages
                .OrderBy(page => page.PageNumber)
                .Select(page => new FamilyLibraryPageViewModel
                {
                    PageNumber = page.PageNumber,
                    TextContent = page.TextContent,
                    ImageUrl = BuildImageUrl(page.ImageRelativePath)
                })
                .ToList()
        };
    }

    public async Task<FamilyLibraryPrintablesViewModel> BuildPrintablesAsync(
        ChildProfile? child,
        string? searchTerm,
        string? category,
        string? stage,
        string? ageFilter,
        CancellationToken cancellationToken = default)
    {
        var materials = await db.FamilyLibraryMaterials
            .OrderBy(material => material.Title)
            .ToListAsync(cancellationToken);

        var cards = materials
            .Where(IsPrintableMaterial)
            .Select(material => new FamilyLibraryPrintableCardViewModel
            {
                Id = material.Id,
                Title = material.Title,
                Category = material.Category,
                EducationStage = material.EducationStage,
                CollectionLabel = material.CollectionLabel,
                Description = material.Description,
                SkillFocus = material.SkillFocus,
                RecommendedMinAge = material.RecommendedMinAge,
                RecommendedMaxAge = material.RecommendedMaxAge,
                PageCount = material.PageCount,
                AgeLabel = GetAgeLabel(material.Title, material.RecommendedMinAge, material.RecommendedMaxAge)
            })
            .ToList();

        var resolvedAgeFilter = ResolveAgeFilter(ageFilter, child, cards.Select(card => card.AgeLabel));
        var filteredCards = cards
            .Where(card =>
                (string.IsNullOrWhiteSpace(searchTerm) || NormalizeText(card.Title).Contains(NormalizeText(searchTerm), StringComparison.Ordinal))
                && (string.IsNullOrWhiteSpace(category) || string.Equals(card.Category, category, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(stage) || string.Equals(card.EducationStage, stage, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(resolvedAgeFilter) || string.Equals(card.AgeLabel, resolvedAgeFilter, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(card => GetStageSortOrder(card.EducationStage))
            .ThenBy(card => card.Category)
            .ThenBy(card => card.Title)
            .ToList();

        return new FamilyLibraryPrintablesViewModel
        {
            SelectedChildId = child?.Id,
            SelectedChildName = child?.FullName ?? string.Empty,
            PersonalizationSummary = child is null
                ? "Aqui ficam só folhas, cadernos e atividades pensadas para papel."
                : $"As atividades abaixo já priorizam a etapa e a idade de {child.FullName}, sem misturar livro digital com imprimível.",
            SearchTerm = searchTerm ?? string.Empty,
            SelectedCategory = category ?? string.Empty,
            SelectedStage = stage ?? string.Empty,
            SelectedAgeFilter = resolvedAgeFilter,
            CategoryFilters = cards
                .Select(card => card.Category)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList(),
            StageFilters = StageFilterOrder
                .Where(filter => cards.Any(card => string.Equals(card.EducationStage, filter, StringComparison.OrdinalIgnoreCase)))
                .ToList(),
            AgeFilters = cards
                .Select(card => card.AgeLabel)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetAgeFilterSortOrder)
                .ToList(),
            Materials = filteredCards
        };
    }

    public async Task<FamilyLibraryReaderViewModel?> BuildPrintableAsync(Guid materialId, CancellationToken cancellationToken = default)
    {
        var material = await db.FamilyLibraryMaterials
            .Include(item => item.Pages.OrderBy(page => page.PageNumber))
            .FirstOrDefaultAsync(item => item.Id == materialId, cancellationToken);

        if (material is null || !IsPrintableMaterial(material))
        {
            return null;
        }

        return new FamilyLibraryReaderViewModel
        {
            Id = material.Id,
            Title = material.Title,
            Category = material.Category,
            EducationStage = material.EducationStage,
            CollectionLabel = material.CollectionLabel,
            SkillFocus = material.SkillFocus,
            Description = material.Description,
            PageCount = material.PageCount,
            CurrentPageNumber = 1,
            CoverImageUrl = BuildImageUrl(material.CoverImageRelativePath),
            Pages = material.Pages
                .OrderBy(page => page.PageNumber)
                .Select(page => new FamilyLibraryPageViewModel
                {
                    PageNumber = page.PageNumber,
                    TextContent = page.TextContent,
                    ImageUrl = BuildImageUrl(page.ImageRelativePath)
                })
                .ToList()
        };
    }

    public async Task ToggleFavoriteAsync(Guid userId, Guid materialId, CancellationToken cancellationToken = default)
    {
        var material = await db.FamilyLibraryMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == materialId, cancellationToken);

        if (material is null || !IsBookMaterial(material))
        {
            return;
        }

        var state = await db.FamilyLibraryUserStates
            .FirstOrDefaultAsync(item => item.UserId == userId && item.MaterialId == materialId, cancellationToken);

        if (state is null)
        {
            state = new FamilyLibraryUserState
            {
                UserId = userId,
                MaterialId = materialId,
                IsFavorite = true,
                CurrentPageNumber = 1,
                UpdatedAtUtc = DateTime.UtcNow
            };

            db.FamilyLibraryUserStates.Add(state);
        }
        else
        {
            state.IsFavorite = !state.IsFavorite;
            state.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        InvalidateLibraryIndex(userId);
    }

    public static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private async Task<LibraryIndexData> LoadLibraryIndexAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = BuildLibraryIndexCacheKey(userId);
        if (memoryCache.TryGetValue(cacheKey, out LibraryIndexData? cachedIndex) && cachedIndex is not null)
        {
            return cachedIndex;
        }

        var materials = await db.FamilyLibraryMaterials
            .AsNoTracking()
            .OrderBy(material => material.Title)
            .ToListAsync(cancellationToken);

        var states = await db.FamilyLibraryUserStates
            .AsNoTracking()
            .Where(state => state.UserId == userId)
            .ToDictionaryAsync(state => state.MaterialId, cancellationToken);

        var books = materials
            .Where(IsBookMaterial)
            .Select(material => MapBookCard(material, states.GetValueOrDefault(material.Id)))
            .ToList();

        var printables = materials
            .Where(IsPrintableMaterial)
            .Select(material => new FamilyLibraryPrintableCardViewModel
            {
                Id = material.Id,
                Title = material.Title,
                Category = material.Category,
                EducationStage = material.EducationStage,
                CollectionLabel = material.CollectionLabel,
                Description = material.Description,
                SkillFocus = material.SkillFocus,
                RecommendedMinAge = material.RecommendedMinAge,
                RecommendedMaxAge = material.RecommendedMaxAge,
                PageCount = material.PageCount,
                AgeLabel = GetAgeLabel(material.Title, material.RecommendedMinAge, material.RecommendedMaxAge)
            })
            .ToList();

        var index = new LibraryIndexData(books, printables);
        memoryCache.Set(cacheKey, index, LibraryIndexCacheDuration);
        return index;
    }

    private async Task<List<ChildReadingHistoryItemViewModel>> LoadChildReadingHistoryAsync(
        Guid userId,
        Guid childId,
        IUrlHelper url,
        CancellationToken cancellationToken)
    {
        var entries = await db.ChildLibraryReadingProgressEntries
            .Where(item => item.ParentUserId == userId
                           && item.ChildId == childId
                           && item.CompletionKind == WeeklyReadingCompletionKind)
            .Include(item => item.Material)
            .OrderByDescending(item => item.CompletedAtUtc)
            .ToListAsync(cancellationToken);

        return entries
            .Select(item => new ChildReadingHistoryItemViewModel
            {
                MaterialId = item.MaterialId,
                PhaseNumber = item.PhaseNumber,
                PhaseLabel = string.IsNullOrWhiteSpace(item.PhaseLabel) ? $"{item.PhaseNumber}ª etapa" : item.PhaseLabel,
                PeriodKey = string.IsNullOrWhiteSpace(item.PeriodKey)
                    ? BuildPeriodKey(item.CompletedAtUtc.ToLocalTime(), item.WeekNumber <= 0 ? GetWeekOfMonth(item.CompletedAtUtc.ToLocalTime().Day) : item.WeekNumber)
                    : item.PeriodKey,
                WeekNumber = item.WeekNumber <= 0 ? GetWeekOfMonth(item.CompletedAtUtc.ToLocalTime().Day) : item.WeekNumber,
                WeekLabel = $"Semana {(item.WeekNumber <= 0 ? GetWeekOfMonth(item.CompletedAtUtc.ToLocalTime().Day) : item.WeekNumber)}",
                MonthLabel = item.CompletedAtUtc.ToLocalTime().ToString("MMMM", new CultureInfo("pt-BR")),
                Title = item.Material.Title,
                Category = item.Material.Category,
                GoalLabel = item.GoalLabel,
                CompletedAtUtc = item.CompletedAtUtc,
                CompletedAtLabel = item.CompletedAtUtc.ToLocalTime().ToString("dd/MM/yyyy"),
                AccessUrl = url.Action("Book", "Library", new { id = item.MaterialId, childId }) ?? string.Empty
            })
            .ToList();
    }

    private static string BuildLibraryIndexCacheKey(Guid userId)
    {
        return $"family-library:index:{userId:N}";
    }

    private void InvalidateLibraryIndex(Guid userId)
    {
        memoryCache.Remove(BuildLibraryIndexCacheKey(userId));
    }

    private static FamilyLibraryWeeklyReadingViewModel BuildWeeklyReading(
        LibraryIndexData library,
        ChildProfile child,
        LearningDomain? dominantDomain,
        IReadOnlyCollection<string> focusTokens,
        IUrlHelper url,
        IReadOnlyList<ChildReadingHistoryItemViewModel> readingHistory)
    {
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        var domain = dominantDomain ?? GetPreferredLibraryDomain(child.FamilyGoalTrack);
        var currentPhaseNumber = GetCurrentPhaseIndex(DateTime.Today.Month) + 1;
        var currentPhaseLabel = $"{currentPhaseNumber}ª etapa";
        var currentWeekNumber = GetWeekOfMonth(DateTime.Today.Day);
        var currentPeriodKey = BuildPeriodKey(DateTime.Today, currentWeekNumber);
        var currentMonthLabel = DateTime.Today.ToString("MMMM", new CultureInfo("pt-BR"));
        var currentUnitNumber = GetPhaseMonthSlot(DateTime.Today.Month);
        var currentPhaseBlueprint = BuildReadingPhaseBlueprints(age)
            .First(item => item.PhaseNumber == currentPhaseNumber);
        var book = PickBestBook(library.Books, age, domain, focusTokens, url, "Abrir leitura da semana", child.Id);
        var printable = PickBestPrintable(library.Printables, age, domain, focusTokens, url, "Abrir folha da semana", child.Id);
        var domainLabel = FormatDomain(domain).ToLowerInvariant();
        var currentProgress = book is null
            ? null
            : readingHistory.FirstOrDefault(item =>
                item.MaterialId == book.Id
                && item.PhaseNumber == currentPhaseNumber
                && string.Equals(item.PeriodKey, currentPeriodKey, StringComparison.OrdinalIgnoreCase));
        var completedCount = readingHistory.Count;
        var monthlyGoal = BuildMonthlyGoal(
            age,
            domain,
            currentPhaseNumber,
            currentMonthLabel,
            currentWeekNumber,
            readingHistory,
            currentProgress is not null);
        var currentUnit = BuildLiteratureUnitsForPhase(
                library,
                child,
                age,
                currentPhaseBlueprint,
                currentPhaseNumber,
                currentUnitNumber,
                url,
                book,
                printable)
            .FirstOrDefault(item => item.IsCurrent);

        return new FamilyLibraryWeeklyReadingViewModel
        {
            ChildId = child.Id,
            MaterialId = book?.Id,
            PhaseNumber = currentPhaseNumber,
            PhaseLabel = currentPhaseLabel,
            PeriodKey = currentPeriodKey,
            WeekNumber = currentWeekNumber,
            WeekLabel = $"Semana {currentWeekNumber}",
            MonthLabel = currentMonthLabel,
            Headline = age <= 5
                ? "Leitura da semana em 3 encontros curtos"
                : "Leitura da semana com começo, conversa e fechamento",
            Summary = $"Esta leitura entra para reforçar {domainLabel} sem virar mais uma tela para o adulto organizar.",
            WeeklyGoal = age <= 5
                ? "Ler o mesmo livro em voz alta durante a semana, pedir que a criança conte o que viu e nomeie as partes mais importantes."
                : "Ler aos poucos durante a semana, conversar sobre a ideia central e fechar com uma resposta curta usando as próprias palavras.",
            ParentGuide = age <= 5
                ? "Escolha um momento calmo, leia apontando para as imagens e pare duas ou três vezes para a criança prever ou recontar."
                : "Divida a leitura em blocos curtos, retome o que foi lido no dia anterior e peça sempre uma resposta simples antes de avançar.",
            CompletionSignal = age <= 5
                ? "Considere a leitura da semana feita quando a criança conseguir recontar a história com ajuda das imagens ou responder o que aconteceu primeiro, depois e no fim."
                : "Considere a leitura da semana feita quando a criança conseguir dizer o tema do trecho, citar uma prova do texto e responder à pergunta final com clareza.",
            ReflectionPrompt = domain switch
            {
                LearningDomain.Math => "Pergunte: quais números, quantidades ou comparações apareceram nesta leitura?",
                LearningDomain.World => "Pergunte: o que essa leitura ensinou sobre o mundo, a natureza, o Brasil ou as pessoas?",
                LearningDomain.ExecutiveFunction => "Pergunte: o que a criança pode copiar dessa história para a rotina dela nesta semana?",
                _ => "Pergunte: o que aconteceu, qual foi a ideia principal e o que mais chamou a atenção?"
            },
            IsCompleted = currentProgress is not null,
            CompletionStatusLabel = currentProgress is not null
                ? $"Leitura registrada em {currentProgress.CompletedAtLabel}"
                : $"Ainda não registrada na {currentPhaseLabel.ToLowerInvariant()}",
            CompletionActionLabel = currentProgress is not null
                ? "Registrar leitura novamente"
                : "Marcar leitura da semana",
            CompletedAtLabel = currentProgress?.CompletedAtLabel ?? string.Empty,
            CompletedReadingsCount = completedCount,
            CompletedReadingsLabel = completedCount switch
            {
                0 => "Nenhuma leitura registrada ainda",
                1 => "1 leitura registrada",
                _ => $"{completedCount} leituras registradas"
            },
            WeekSteps = BuildWeeklyReadingSteps(age, domain),
            Book = book,
            Printable = printable,
            MonthlyGoal = monthlyGoal,
            CurrentUnit = currentUnit,
            RecentHistory = readingHistory.Take(3).ToList()
        };
    }

    private static FamilyLibraryAnnualSpineViewModel BuildAnnualReadingSpine(
        LibraryIndexData library,
        ChildProfile child,
        IUrlHelper url,
        IReadOnlyList<ChildReadingHistoryItemViewModel> readingHistory)
    {
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        var phaseIndex = GetCurrentPhaseIndex(DateTime.Today.Month);
        var usedBookIds = new HashSet<Guid>();
        var usedPrintableIds = new HashSet<Guid>();
        var phases = BuildReadingPhaseBlueprints(age)
            .Select((blueprint, index) =>
            {
                var book = PickBestDistinctBook(
                    library.Books,
                    age,
                    blueprint.Domain,
                    blueprint.FocusTokens,
                    url,
                    child.Id,
                    usedBookIds,
                    "Abrir livro desta etapa");
                var printable = PickBestDistinctPrintable(
                    library.Printables,
                    age,
                    blueprint.Domain,
                    blueprint.FocusTokens,
                    url,
                    child.Id,
                    usedPrintableIds,
                    "Abrir atividade desta etapa");
                var completedItems = readingHistory
                    .Where(item => item.PhaseNumber == blueprint.PhaseNumber)
                    .OrderByDescending(item => item.CompletedAtUtc)
                    .ToList();
                var targetReadingsToClose = GetPhaseClosureTarget(age);
                var unitSequences = BuildLiteratureUnitsForPhase(
                    library,
                    child,
                    age,
                    blueprint,
                    phaseIndex + 1,
                    GetPhaseMonthSlot(DateTime.Today.Month),
                    url,
                    book,
                    printable);

                return new FamilyLibraryAnnualSpinePhaseViewModel
                {
                    PhaseNumber = blueprint.PhaseNumber,
                    PhaseLabel = $"{blueprint.PhaseNumber}ª etapa",
                    Title = blueprint.Title,
                    Summary = blueprint.Summary,
                    WeeklyRhythm = blueprint.WeeklyRhythm,
                    ParentGuide = blueprint.ParentGuide,
                    CompletionSignal = blueprint.CompletionSignal,
                    IsCurrent = index == phaseIndex,
                    CompletedCount = completedItems.Count,
                    CompletedCountLabel = completedItems.Count switch
                    {
                        0 => "Nenhuma leitura concluída",
                        1 => "1 leitura concluída",
                        _ => $"{completedItems.Count} leituras concluídas"
                    },
                    TargetReadingsToClose = targetReadingsToClose,
                    AutoCloseLabel = completedItems.Count >= targetReadingsToClose
                        ? $"Etapa fechada automaticamente com {completedItems.Count} leituras registradas"
                        : $"Faltam {Math.Max(0, targetReadingsToClose - completedItems.Count)} leitura(s) para o fechamento automático",
                    IsClosed = completedItems.Count >= targetReadingsToClose,
                    Book = book,
                    Printable = printable,
                    UnitSequences = unitSequences,
                    CompletedItems = completedItems
                };
            })
            .ToList();

        var currentPhase = phases.FirstOrDefault(item => item.IsCurrent) ?? phases.FirstOrDefault();
        var ageBand = age <= 5 ? "primeiras leituras" : age <= 8 ? "leitura em crescimento" : "leitura com mais autonomia";
        var completedCount = readingHistory.Count;

        return new FamilyLibraryAnnualSpineViewModel
        {
            Headline = "Espinha de literatura do ano",
            Summary = $"O acervo deixa de ser solto e passa a acompanhar o currículo em quatro etapas de {ageBand}, com um livro central e uma atividade de papel quando fizer sentido.",
            CurrentPhaseLabel = currentPhase?.PhaseLabel ?? "1ª etapa",
            CompletedReadingsCount = completedCount,
            CompletedReadingsLabel = completedCount switch
            {
                0 => "Nenhuma leitura registrada no ano",
                1 => "1 leitura registrada no ano",
                _ => $"{completedCount} leituras registradas no ano"
            },
            RecentHistory = readingHistory.Take(4).ToList(),
            Phases = phases
        };
    }

    private static List<string> BuildWeeklyReadingSteps(int age, LearningDomain domain)
    {
        if (age <= 5)
        {
            return
            [
                "No primeiro encontro, leia em voz alta sem pressa e mostre as imagens.",
                "No segundo encontro, pare em pontos-chave e peça para a criança dizer o que está acontecendo.",
                "No terceiro encontro, feche com um reconto curto e use a folha indicada para fixar a ideia da semana."
            ];
        }

        return domain switch
        {
            LearningDomain.Math =>
            [
                "Leia o trecho escolhido e destaque quantidades, medidas, tempo ou comparações que aparecem no texto.",
                "Peça para a criança explicar com as próprias palavras o que o texto mostra em números ou relações.",
                "Feche com a atividade de papel ou com uma resposta curta registrada no caderno."
            ],
            LearningDomain.World =>
            [
                "Leia um bloco por dia e ligue a leitura a algo do mundo real: mapa, natureza, pessoa ou fato histórico.",
                "Peça para a criança contar o que aprendeu e qual detalhe mais importante apareceu.",
                "Feche com uma frase, desenho ou atividade de papel que ajude a guardar o tema."
            ],
            LearningDomain.ExecutiveFunction =>
            [
                "Leia a história e pergunte qual atitude, rotina ou decisão apareceu na leitura.",
                "Converse sobre como levar essa ideia para a vida da criança durante a semana.",
                "Feche com uma pequena ação prática ou atividade de papel ligada ao hábito trabalhado."
            ],
            _ =>
            [
                "No primeiro encontro, leia o trecho do começo ao fim sem interromper demais.",
                "No segundo encontro, volte ao texto e peça ideia principal, personagem, problema ou detalhe mais importante.",
                "No terceiro encontro, feche com resposta oral ou escrita curta e, se ajudar, use a atividade de papel."
            ]
        };
    }

    private static FamilyLibraryMonthlyGoalViewModel BuildMonthlyGoal(
        int age,
        LearningDomain domain,
        int currentPhaseNumber,
        string currentMonthLabel,
        int currentWeekNumber,
        IReadOnlyList<ChildReadingHistoryItemViewModel> readingHistory,
        bool currentWeekCompleted)
    {
        var targetReadings = age <= 5 ? 3 : 4;
        var phaseClosureTarget = GetPhaseClosureTarget(age);
        var currentMonthEntries = readingHistory
            .Where(item =>
                item.PhaseNumber == currentPhaseNumber
                && item.CompletedAtUtc.ToLocalTime().Year == DateTime.Today.Year
                && item.CompletedAtUtc.ToLocalTime().Month == DateTime.Today.Month)
            .ToList();
        var completedWeeks = currentMonthEntries
            .Select(item => item.WeekNumber)
            .Where(week => week > 0)
            .Distinct()
            .Count();

        return new FamilyLibraryMonthlyGoalViewModel
        {
            MonthLabel = currentMonthLabel,
            Headline = $"Meta de leitura de {currentMonthLabel}",
            Summary = "O sistema organiza o mês em quatro semanas curtas para a família só abrir, ler, conversar e marcar.",
            TargetReadings = targetReadings,
            CompletedReadings = completedWeeks,
            IsCompleted = completedWeeks >= targetReadings,
            ProgressLabel = $"{completedWeeks}/{targetReadings} semana(s) registradas",
            AutoCloseProjectionLabel = completedWeeks >= targetReadings
                ? "Meta mensal fechada. Continue registrando para aproximar o fechamento automático da etapa."
                : $"Ao bater {targetReadings} semanas no mês, a meta mensal fecha sozinha. A etapa fecha automaticamente ao chegar em {phaseClosureTarget} leituras na fase.",
            GoalBullets = BuildMonthlyGoalBullets(age, domain),
            WeeklySequence = BuildMonthlySequence(age, domain, currentWeekNumber, currentMonthEntries, currentWeekCompleted)
        };
    }

    private static List<string> BuildMonthlyGoalBullets(int age, LearningDomain domain)
    {
        if (age <= 5)
        {
            return
            [
                "Repetir a leitura em voz alta sem pressa.",
                "Pedir reconto curto com apoio das imagens.",
                "Fechar com uma pequena atividade ou conversa de valor."
            ];
        }

        return domain switch
        {
            LearningDomain.World =>
            [
                "Ler e ligar o texto a fatos, pessoas, lugares ou valores.",
                "Fazer uma resposta curta com base no que foi lido.",
                "Guardar constância semanal sem transformar tudo em trabalho longo."
            ],
            LearningDomain.ExecutiveFunction =>
            [
                "Ler com rotina estável no mesmo horário.",
                "Pedir uma resposta curta e organizada.",
                "Registrar semanas concluídas para o sistema fechar a etapa sozinho."
            ],
            _ =>
            [
                "Ler toda semana com começo, conversa e fechamento.",
                "Treinar ideia central, personagem ou detalhe importante.",
                "Chegar ao fim do mês com respostas mais claras e menos ajuda."
            ]
        };
    }

    private static List<FamilyLibraryWeeklySequenceItemViewModel> BuildMonthlySequence(
        int age,
        LearningDomain domain,
        int currentWeekNumber,
        IReadOnlyCollection<ChildReadingHistoryItemViewModel> monthEntries,
        bool currentWeekCompleted)
    {
        var blueprints = age <= 5
            ? new[]
            {
                ("Semana 1", "Ouvir e gostar da história", "Leia com calma e mostre imagens ou personagens principais."),
                ("Semana 2", "Recontar o que aconteceu", "Volte à leitura e peça começo, meio e fim com palavras simples."),
                ("Semana 3", "Ligar a história à vida", "Pergunte qual atitude, valor ou cena ficou marcada."),
                ("Semana 4", "Fechar e registrar", "Faça o fechamento do mês e guarde a leitura como vivida pela família.")
            }
            : domain switch
            {
                LearningDomain.World =>
                [
                    ("Semana 1", "Abrir repertório", "Leia o texto e localize pessoas, lugares, fatos ou temas do mundo real."),
                    ("Semana 2", "Entender o que importa", "Peça o fato principal e um detalhe que ajuda a explicar o trecho."),
                    ("Semana 3", "Conectar com o mundo", "Ligue a leitura a mapa, linha do tempo, natureza, Bíblia ou história."),
                    ("Semana 4", "Responder e guardar", "Feche o mês com uma resposta curta e registre a leitura feita.")
                ],
                LearningDomain.ExecutiveFunction =>
                [
                    ("Semana 1", "Entrar no ritmo", "Abrir a semana de leitura no mesmo horário para criar rotina."),
                    ("Semana 2", "Responder com organização", "Pedir resposta curta, clara e sem fugir do trecho."),
                    ("Semana 3", "Ganhar autonomia", "Reduzir ajuda do adulto e pedir mais iniciativa da criança."),
                    ("Semana 4", "Fechar e consolidar", "Registrar a semana feita e deixar a etapa mais perto do fechamento automático.")
                ],
                _ =>
                [
                    ("Semana 1", "Abrir o livro do mês", "Ler o primeiro trecho sem pressa e garantir entendimento geral."),
                    ("Semana 2", "Personagem e ideia central", "Pedir quem aparece, o que aconteceu e qual é a ideia do trecho."),
                    ("Semana 3", "Detalhes e prova do texto", "Voltar ao texto para mostrar o detalhe que sustenta a resposta."),
                    ("Semana 4", "Fechar e registrar", "Fazer o fechamento curto do mês e marcar a leitura como concluída.")
                ]
            };

        return blueprints
            .Select((item, index) =>
            {
                var weekNumber = index + 1;
                var completed = monthEntries.Any(entry => entry.WeekNumber == weekNumber) || (weekNumber == currentWeekNumber && currentWeekCompleted);
                return new FamilyLibraryWeeklySequenceItemViewModel
                {
                    WeekNumber = weekNumber,
                    WeekLabel = item.Item1,
                    Title = item.Item2,
                    Summary = item.Item3,
                    IsCurrent = weekNumber == currentWeekNumber,
                    IsCompleted = completed,
                    StatusLabel = completed
                        ? "feito"
                        : weekNumber == currentWeekNumber
                            ? "agora"
                            : "depois"
                };
            })
            .ToList();
    }

    private static List<FamilyLibraryLiteratureUnitViewModel> BuildLiteratureUnitsForPhase(
        LibraryIndexData library,
        ChildProfile child,
        int age,
        ReadingPhaseBlueprint blueprint,
        int currentPhaseNumber,
        int currentUnitNumber,
        IUrlHelper url,
        FamilyLibraryRecommendationViewModel? fallbackBook,
        FamilyLibraryRecommendationViewModel? fallbackPrintable)
    {
        var usedBookIds = new HashSet<Guid>();
        var usedPrintableIds = new HashSet<Guid>();

        if (fallbackBook is not null)
        {
            usedBookIds.Add(fallbackBook.Id);
        }

        if (fallbackPrintable is not null)
        {
            usedPrintableIds.Add(fallbackPrintable.Id);
        }

        return BuildLiteratureUnitBlueprints(age, blueprint)
            .Select((unit, index) =>
            {
                var unitNumber = index + 1;
                var focusTokens = blueprint.FocusTokens
                    .Concat(unit.FocusTokens)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var unitBook = PickBestDistinctBook(
                        library.Books,
                        age,
                        blueprint.Domain,
                        focusTokens,
                        url,
                        child.Id,
                        usedBookIds,
                        "Abrir livro da unidade")
                    ?? fallbackBook;
                var unitPrintable = PickBestDistinctPrintable(
                        library.Printables,
                        age,
                        blueprint.Domain,
                        focusTokens,
                        url,
                        child.Id,
                        usedPrintableIds,
                        "Abrir apoio da unidade")
                    ?? fallbackPrintable;

                return new FamilyLibraryLiteratureUnitViewModel
                {
                    PhaseNumber = blueprint.PhaseNumber,
                    UnitNumber = unitNumber,
                    UnitLabel = $"Unidade {unitNumber}",
                    Title = unit.Title,
                    Summary = unit.Summary,
                    ParentGuide = unit.ParentGuide,
                    WritingTaskTitle = unit.WritingTaskTitle,
                    WritingTaskPrompt = unit.WritingTaskPrompt,
                    WritingCompletionSignal = unit.WritingCompletionSignal,
                    OptionalEvidencePrompt = unit.OptionalEvidencePrompt,
                    IsCurrent = blueprint.PhaseNumber == currentPhaseNumber && unitNumber == currentUnitNumber,
                    UnitFlow = unit.UnitFlow.ToList(),
                    Book = unitBook,
                    Printable = unitPrintable
                };
            })
            .ToList();
    }

    private static int GetCurrentPhaseIndex(int month)
    {
        if (month <= 3)
        {
            return 0;
        }

        if (month <= 6)
        {
            return 1;
        }

        if (month <= 9)
        {
            return 2;
        }

        return 3;
    }

    private static int GetWeekOfMonth(int day)
    {
        return Math.Clamp(((Math.Max(day, 1) - 1) / 7) + 1, 1, 4);
    }

    private static int GetPhaseMonthSlot(int month)
    {
        return Math.Clamp(((Math.Max(month, 1) - 1) % 3) + 1, 1, 3);
    }

    private static string BuildPeriodKey(DateTime referenceDate, int weekNumber)
    {
        return $"{referenceDate.Year:D4}-{referenceDate.Month:D2}-W{Math.Clamp(weekNumber, 1, 4)}";
    }

    private static int GetPhaseClosureTarget(int age)
    {
        return age <= 5 ? 6 : 8;
    }

    private static List<ReadingPhaseBlueprint> BuildReadingPhaseBlueprints(int age)
    {
        if (age <= 5)
        {
            return
            [
                new ReadingPhaseBlueprint(
                    1,
                    "Encantar e criar o hábito",
                    "Começar o ano com leitura em voz alta, previsibilidade e prazer em ouvir histórias.",
                    "Leia 3 vezes por semana, sempre em blocos curtos.",
                    "Releia sem medo. O objetivo desta fase é criar vínculo com a leitura, não correr para trocar de título.",
                    "A etapa está bem encaminhada quando a criança pede a história de novo ou consegue recontar partes com apoio das imagens.",
                    LearningDomain.Language,
                    ["voz", "alta", "historia", "livrinho", "familia"]),
                new ReadingPhaseBlueprint(
                    2,
                    "Reconto e detalhes",
                    "Usar fábulas, pequenas histórias e perguntas simples para a criança aprender a observar melhor o texto.",
                    "Leia 2 ou 3 vezes na semana e feche com reconto.",
                    "Faça perguntas curtas, aceite resposta oral e valorize começo, meio e fim.",
                    "A etapa amadurece quando a criança consegue contar a história em ordem e citar um detalhe importante.",
                    LearningDomain.Language,
                    ["fabula", "esopo", "reconto", "animais", "detalhes"]),
                new ReadingPhaseBlueprint(
                    3,
                    "Valores e mundo de Deus",
                    "Trazer histórias bíblicas e temas do mundo real para formar repertório, valores e conversa em família.",
                    "Leia 2 vezes por semana e ligue a história à rotina da casa.",
                    "Converse sobre atitudes, criação, cuidado, coragem e obediência com exemplos simples do dia a dia.",
                    "A etapa está cumprindo o papel quando a criança conecta a leitura a uma atitude ou valor concreto.",
                    LearningDomain.World,
                    ["biblica", "jesus", "noe", "criacao", "davi"]),
                new ReadingPhaseBlueprint(
                    4,
                    "Leitura com mais independência",
                    "Fechar o ano incentivando a criança a participar mais da leitura, nomear palavras conhecidas e completar pequenas respostas.",
                    "Mantenha 3 encontros leves por semana.",
                    "Mostre que a leitura também gera ação: apontar, ligar, nomear, circular, pintar e guardar no acervo da família.",
                    "A etapa fecha bem quando a criança já entra na leitura sabendo o que precisa observar ou responder.",
                    LearningDomain.ExecutiveFunction,
                    ["rotina", "autonomia", "brincadeiras", "livrinho", "coordenação"])
            ];
        }

        return
        [
            new ReadingPhaseBlueprint(
                1,
                "Hábito e fluidez",
                "Abrir o ano com ritmo de leitura consistente para a criança ganhar fôlego, foco e constância.",
                "Faça 3 encontros curtos por semana com o mesmo livro ou série de trechos.",
                "Priorize manter o hábito antes de cobrar análise longa. O adulto conduz e a criança responde com frases simples.",
                "A etapa vai bem quando a leitura já entra na semana sem resistência e a criança consegue contar o que leu.",
                LearningDomain.Language,
                ["literatura", "leitura", "conto", "historia", "capitulo"]),
            new ReadingPhaseBlueprint(
                2,
                "Compreensão e personagem",
                "Aprofundar personagem, conflito, ideia central e detalhes que sustentam a compreensão.",
                "Faça 2 ou 3 encontros por semana com pausa para conversa.",
                "Depois de cada trecho, peça uma resposta curta: quem, o que aconteceu, por que isso importa.",
                "A etapa amadurece quando a criança começa a sustentar resposta com prova do texto.",
                LearningDomain.Language,
                ["personagem", "ideia", "detalhes", "resumo", "literatura"]),
            new ReadingPhaseBlueprint(
                3,
                "Repertório, mundo e valores",
                "Usar leitura para ampliar repertório cultural, histórico, bíblico e de mundo.",
                "Alterne literatura, repertório e uma atividade de papel para fixar.",
                "Conecte a leitura a mapa, linha do tempo, pessoas reais, valores e fatos do mundo.",
                "A etapa cumpre o papel quando a criança usa a leitura para explicar algo além da própria história.",
                LearningDomain.World,
                ["cronologia", "historia", "biografia", "brasil", "biblica"]),
            new ReadingPhaseBlueprint(
                4,
                "Autonomia e resposta pessoal",
                "Fechar o ano com mais independência para ler, responder e guardar um registro simples do que aprendeu.",
                "Faça 3 encontros curtos por semana e um fechamento final.",
                "Reduza a fala do adulto aos poucos e peça respostas mais claras, sem transformar tudo em redação longa.",
                "A etapa fecha bem quando a criança consegue ler, pensar e entregar uma resposta curta com menos ajuda.",
                LearningDomain.ExecutiveFunction,
                ["autonomia", "opiniao", "resposta", "registro", "literatura"])
        ];
    }

    private static List<LiteratureUnitBlueprint> BuildLiteratureUnitBlueprints(int age, ReadingPhaseBlueprint blueprint)
    {
        if (age <= 5)
        {
            return blueprint.PhaseNumber switch
            {
                1 =>
                [
                    new LiteratureUnitBlueprint(
                        "Ouvir e nomear",
                        "Abrir a etapa com leitura em voz alta, imagens e nomeacao do que aparece na historia.",
                        "Leia apontando para personagens e cenarios. Aceite resposta oral curta e sem pressa.",
                        "Desenho com frase ditada",
                        "Peça um desenho da parte favorita e dite uma frase curta com as palavras da crianca.",
                        "A tarefa fica pronta quando o desenho e a frase mostram que a crianca reconheceu um momento importante da leitura.",
                        "Se quiser guardar memoria, salve uma foto do desenho ou da pagina do caderno.",
                        [
                            "Ouvir a leitura com calma",
                            "Apontar personagem, objeto ou cena principal",
                            "Fechar com desenho e frase curta"
                        ],
                        ["imagem", "personagem", "historia"]),
                    new LiteratureUnitBlueprint(
                        "Repetir e antecipar",
                        "Usar releitura para a crianca prever o que vem e ganhar seguranca dentro da historia.",
                        "Volte ao mesmo livro duas ou tres vezes na semana e pare antes de uma cena importante para a crianca dizer o que acha que vai acontecer.",
                        "Resposta oral com apoio de imagem",
                        "Mostre uma ilustracao e peça que a crianca diga quem aparece e o que acredita que vem depois.",
                        "Pode marcar como pronta quando a crianca fizer uma previsao simples ligada a cena e depois comparar com o que aconteceu.",
                        "Opcionalmente, grave um audio curto contando a previsao e a confirmacao.",
                        [
                            "Reabrir a historia conhecida",
                            "Parar antes de uma cena importante",
                            "Ouvir a previsao e comparar com o texto"
                        ],
                        ["previsao", "imagem", "repeticao"]),
                    new LiteratureUnitBlueprint(
                        "Guardar a historia em familia",
                        "Fechar a unidade com uma memoria simples da leitura para a familia lembrar depois.",
                        "Converse sobre o que mais gostaram na historia e transforme isso em um registro afetivo curto.",
                        "Bilhete da leitura",
                        "Ajude a crianca a completar a frase: Hoje eu gostei quando... ou Hoje eu aprendi que....",
                        "A tarefa pode ser concluida quando a crianca conseguir registrar uma lembranca curta da historia.",
                        "Se fizer sentido, guarde o bilhete ou uma foto dele no acervo da familia.",
                        [
                            "Conversar sobre a parte favorita",
                            "Completar um bilhete curto",
                            "Guardar a memoria se a familia quiser"
                        ],
                        ["familia", "memoria", "historia"])
                ],
                2 =>
                [
                    new LiteratureUnitBlueprint(
                        "Recontar em ordem",
                        "Ajudar a crianca a perceber que as historias seguem uma sequencia clara.",
                        "Use começo, meio e fim com frases curtas e, se precisar, volte as paginas para apoiar o reconto.",
                        "Tres cenas em sequencia",
                        "Desenhe ou escreva tres quadros: primeiro, depois e no fim.",
                        "A tarefa fica pronta quando a crianca consegue mostrar a ordem da historia sem embaralhar tudo.",
                        "Se quiser, salve a foto dos tres quadros ou um audio do reconto.",
                        [
                            "Lembrar como a historia comecou",
                            "Registrar o que aconteceu no meio",
                            "Fechar com o que aconteceu no fim"
                        ],
                        ["reconto", "sequencia", "detalhes"]),
                    new LiteratureUnitBlueprint(
                        "Detalhe importante",
                        "Treinar a observacao de um elemento que realmente sustenta a historia.",
                        "Pergunte qual objeto, fala ou atitude foi importante para a historia acontecer daquele jeito.",
                        "Objeto ou cena que importa",
                        "Peça para a crianca escolher um objeto, fala ou cena e dizer por que aquilo foi importante.",
                        "A unidade avanca quando a crianca consegue apontar um detalhe concreto da historia.",
                        "A evidencia opcional pode ser uma foto do desenho do detalhe ou um audio curto da explicacao.",
                        [
                            "Voltar ao trecho importante",
                            "Escolher um detalhe concreto",
                            "Dizer por que ele importa"
                        ],
                        ["detalhe", "apoio", "historia"]),
                    new LiteratureUnitBlueprint(
                        "Bilhete da historia",
                        "Fechar a fase com uma resposta curta que ligue historia e aprendizagem.",
                        "Ajude a crianca a transformar a conversa em uma frase simples sobre o que entendeu.",
                        "Hoje aprendi a...",
                        "Complete com a crianca uma frase curta sobre o que ela aprendeu, gostou ou quer imitar.",
                        "Considere pronta quando a frase fizer sentido para a leitura da semana e puder ser lida de volta sem confusao.",
                        "Se a familia quiser, guarde o bilhete no acervo ou tire foto do registro.",
                        [
                            "Conversar sobre a leitura",
                            "Completar a frase guiada",
                            "Reler o bilhete juntos"
                        ],
                        ["bilhete", "aprendi", "familia"])
                ],
                3 =>
                [
                    new LiteratureUnitBlueprint(
                        "Valor da leitura",
                        "Usar a historia para nomear um valor ou atitude que a familia quer fortalecer.",
                        "Pergunte qual atitude apareceu: cuidado, coragem, obediencia, generosidade ou paciencia.",
                        "Valor em uma frase",
                        "Escreva ou dite uma frase curta mostrando o valor principal da leitura.",
                        "A tarefa pode ser concluida quando a crianca consegue ligar a historia a um valor concreto.",
                        "Uma foto do caderno ou um audio dizendo o valor ja basta como evidencia opcional.",
                        [
                            "Ler e conversar sobre atitudes",
                            "Escolher um valor da historia",
                            "Registrar em uma frase curta"
                        ],
                        ["valor", "familia", "biblica"]),
                    new LiteratureUnitBlueprint(
                        "Biblia e vida",
                        "Ligar a leitura a uma acao simples da semana dentro de casa.",
                        "Depois de ler, pergunte onde esse valor aparece na rotina da familia hoje.",
                        "Acao da semana",
                        "Defina com a crianca uma pequena acao: ajudar, guardar, obedecer, falar com gentileza ou agradecer.",
                        "Marque como pronta quando a leitura terminar com uma acao clara que a crianca sabe repetir.",
                        "A evidencia opcional pode ser uma foto da acao ou uma anotacao curta do adulto.",
                        [
                            "Conversar sobre o valor",
                            "Escolher uma acao pratica",
                            "Combinar quando viver isso em casa"
                        ],
                        ["biblica", "acao", "rotina"]),
                    new LiteratureUnitBlueprint(
                        "Memoria da familia",
                        "Fechar a unidade guardando uma lembranca pequena do que a leitura produziu na casa.",
                        "Nao transforme em tarefa longa. Basta um registro curto que a familia consiga encontrar depois.",
                        "Cartao da memoria",
                        "Monte um cartao simples com titulo do livro e uma frase do que a leitura deixou na semana.",
                        "A tarefa esta pronta quando o cartao resume a memoria da leitura e pode ser guardado no sistema ou em papel.",
                        "Se quiser guardar evidencia, use foto do cartao ou pequeno video da crianca mostrando o registro.",
                        [
                            "Retomar o livro lido",
                            "Escolher a lembranca principal",
                            "Montar um cartao curto"
                        ],
                        ["memoria", "familia", "registro"])
                ],
                _ =>
                [
                    new LiteratureUnitBlueprint(
                        "Palavra conhecida",
                        "Fechar o ano mostrando que a crianca ja reconhece elementos da leitura com mais autonomia.",
                        "Leia em voz alta, mas deixe a crianca apontar nomes, palavras conhecidas ou personagens que ja reconhece.",
                        "Circular e nomear",
                        "Peça para a crianca circular ou apontar palavras e nomes que reconhece na leitura ou no apoio.",
                        "A unidade avanca quando a crianca participa com menos dependencia do adulto e reconhece algo do texto.",
                        "Se a familia quiser, guarde foto da pagina marcada ou do caderno.",
                        [
                            "Ler juntos",
                            "Apontar o que a crianca ja reconhece",
                            "Fechar com nomeacao curta"
                        ],
                        ["palavra", "autonomia", "leitura"]),
                    new LiteratureUnitBlueprint(
                        "Responder com apoio",
                        "Pedir uma resposta curta sem alongar demais nem tornar a leitura cansativa.",
                        "Faça uma pergunta simples sobre quem, o que aconteceu ou qual parte foi mais importante.",
                        "Resposta curta da leitura",
                        "Registre uma frase curta dita ou copiada com apoio do adulto.",
                        "Pode marcar como pronta quando a crianca entrega uma resposta coerente com o texto lido.",
                        "A evidencia opcional pode ser a foto da resposta ou um audio curto lendo o que registrou.",
                        [
                            "Fazer uma pergunta clara",
                            "Ouvir a resposta da crianca",
                            "Registrar em uma frase"
                        ],
                        ["resposta", "apoio", "clareza"]),
                    new LiteratureUnitBlueprint(
                        "Fechar com autonomia",
                        "Encerrar a etapa mostrando que a leitura ja faz parte da rotina da familia.",
                        "Retome com a crianca o que ela ja sabe fazer melhor hoje do que no comeco do ano.",
                        "Minha leitura do ano",
                        "Monte com a crianca um pequeno fechamento: livro preferido, personagem preferido ou o que aprendeu a observar.",
                        "Considere concluida quando a crianca consegue falar ou registrar uma memoria clara do proprio crescimento.",
                        "Se quiser, guarde foto do registro final ou um pequeno video contando a experiencia.",
                        [
                            "Escolher a leitura mais marcante",
                            "Contar o que melhorou no ano",
                            "Guardar o fechamento final"
                        ],
                        ["autonomia", "memoria", "ano"])
                ]
            };
        }

        return blueprint.PhaseNumber switch
        {
            1 =>
            [
                new LiteratureUnitBlueprint(
                    "Personagem e comeco do texto",
                    "Abrir a etapa com leitura fluida e uma resposta curta sobre quem aparece e como o texto comeca.",
                    "Leia o trecho inicial sem interromper demais. Depois peça personagem, objetivo e o que mais chamou atencao.",
                    "Ficha curta de personagem",
                    "Escreva tres linhas sobre quem e o personagem principal, o que ele quer ou enfrenta e o que mais chamou atencao.",
                    "A tarefa pode ser marcada como pronta quando a crianca responder sem copiar o trecho inteiro e sem fugir do texto.",
                    "Como evidencia opcional, guarde a foto da ficha ou a pagina do caderno.",
                    [
                        "Ler o trecho inicial",
                        "Identificar o personagem principal",
                        "Escrever tres linhas claras"
                    ],
                    ["personagem", "comeco", "leitura"]),
                new LiteratureUnitBlueprint(
                    "Conflito e detalhe importante",
                    "Treinar atencao ao problema do texto e ao detalhe que ajuda a provar a resposta.",
                    "Leia o trecho e pergunte o que mudou, travou ou criou tensao para o personagem.",
                    "Resposta curta com prova",
                    "Escreva quatro linhas explicando o conflito e copie uma palavra ou frase do texto que ajude a provar a resposta.",
                    "Considere pronta quando a crianca der a resposta e apontar uma prova real do texto.",
                    "A evidencia opcional pode ser foto da resposta ou marcador mostrando a prova no texto.",
                    [
                        "Localizar o conflito",
                        "Voltar ao texto para achar a prova",
                        "Escrever a resposta com clareza"
                    ],
                    ["conflito", "detalhe", "prova"]),
                new LiteratureUnitBlueprint(
                    "Fechamento com sintese",
                    "Fechar a fase mostrando que a crianca ja consegue resumir o essencial sem escrever demais.",
                    "Nao peça redacao longa. Oriente a separar o que e essencial do que e detalhe.",
                    "Resumo de cinco linhas",
                    "Feche a unidade com um resumo de ate cinco linhas contando o essencial do trecho ou capitulo.",
                    "Pode marcar como pronta quando o resumo mantiver a linha principal da leitura sem se perder em detalhes soltos.",
                    "Se quiser guardar memoria, salve a foto do resumo ou um audio da leitura final.",
                    [
                        "Relembrar o trecho lido",
                        "Selecionar o essencial",
                        "Fechar com resumo breve"
                    ],
                    ["resumo", "sintese", "fluidez"])
            ],
            2 =>
            [
                new LiteratureUnitBlueprint(
                    "Ideia central do capitulo",
                    "Aprofundar a capacidade de enxergar o tema do trecho sem transformar isso em pergunta abstrata.",
                    "Depois da leitura, pergunte sobre o que esse trecho realmente tratou e o que o autor quis destacar.",
                    "Titulo novo para o texto",
                    "Crie um novo titulo para o capitulo ou trecho e justifique em duas ou tres linhas.",
                    "A tarefa fica pronta quando o novo titulo combina com a leitura e a justificativa explica essa escolha.",
                    "A evidencia opcional pode ser uma foto do titulo criado ou um audio da justificativa.",
                    [
                        "Ler o capitulo",
                        "Escolher a ideia central",
                        "Escrever titulo novo e justificativa"
                    ],
                    ["ideia", "titulo", "capitulo"]),
                new LiteratureUnitBlueprint(
                    "Detalhes que sustentam resposta",
                    "Mostrar para a crianca que responder bem depende de voltar ao texto e buscar apoio.",
                    "Faça uma pergunta simples e peça que a resposta venha junto de dois detalhes claros do texto.",
                    "Duas provas do texto",
                    "Responda a pergunta principal e registre dois detalhes que sustentam essa resposta.",
                    "Pode marcar como pronta quando a crianca entrega resposta curta e consegue mostrar duas pistas do texto.",
                    "A evidencia opcional pode ser foto do caderno com os dois detalhes destacados.",
                    [
                        "Responder a pergunta principal",
                        "Buscar dois detalhes de apoio",
                        "Registrar a prova no caderno"
                    ],
                    ["detalhes", "prova", "resposta"]),
                new LiteratureUnitBlueprint(
                    "Resposta pessoal com clareza",
                    "Fechar a fase ligando leitura e opiniao sem perder a base do texto.",
                    "Peça uma opiniao curta, mas ligada ao que foi lido, nunca solta da leitura.",
                    "Paragrafo curto de opiniao",
                    "Escreva um paragrafo curto dizendo o que voce pensa sobre uma atitude, decisao ou ideia do texto.",
                    "A unidade fecha bem quando a crianca toma posicao e se apoia no que leu para explicar.",
                    "Se quiser, guarde foto do paragrafo ou audio lendo a propria opiniao.",
                    [
                        "Ler e conversar sobre o texto",
                        "Tomar posicao sobre uma ideia",
                        "Escrever opiniao curta ligada a leitura"
                    ],
                    ["opiniao", "clareza", "texto"])
            ],
            3 =>
            [
                new LiteratureUnitBlueprint(
                    "Leitura que amplia repertorio",
                    "Usar a literatura para abrir conversa sobre mundo, historia, pessoas, Brasil e valores.",
                    "Leia perguntando o que isso ensina para alem da historia em si.",
                    "Fato ou valor aprendido",
                    "Registre em tres linhas um fato novo, um valor ou uma descoberta que nasceu da leitura.",
                    "A tarefa pode ser marcada como pronta quando a crianca consegue explicar o que aprendeu para alem da trama.",
                    "Como evidencia opcional, salve o registro ou a explicacao oral da crianca.",
                    [
                        "Ler e observar o que amplia repertorio",
                        "Escolher um fato ou valor",
                        "Registrar em tres linhas"
                    ],
                    ["valor", "mundo", "repertorio"]),
                new LiteratureUnitBlueprint(
                    "Comparar leitura e realidade",
                    "Fazer a crianca comparar o que leu com um apoio simples de historia, geografia ou biografia.",
                    "Use um mapa, linha do tempo, biografia curta ou apoio visual para mostrar como a leitura conversa com o mundo real.",
                    "Quadro de comparacao",
                    "Monte um quadro curto com duas colunas: no texto e na vida real.",
                    "Considere pronta quando a crianca fizer pelo menos uma comparacao coerente entre leitura e realidade.",
                    "A evidencia opcional pode ser a foto do quadro ou do material de apoio usado.",
                    [
                        "Ler o trecho principal",
                        "Abrir um apoio simples",
                        "Completar o quadro de comparacao"
                    ],
                    ["comparar", "biografia", "brasil"]),
                new LiteratureUnitBlueprint(
                    "Fechar com explicacao propria",
                    "Encerrar a fase pedindo uma explicacao final clara, curta e pessoal.",
                    "Ajude a crianca a sair do reconto e entrar em uma explicacao do que a leitura ensinou.",
                    "Mini explicacao final",
                    "Escreva ate cinco linhas explicando o principal aprendizado dessa unidade de leitura.",
                    "Pode marcar como pronta quando a crianca explicar o aprendizado com as proprias palavras e sem fugir do foco.",
                    "A evidencia opcional pode ser foto do texto final ou video curto explicando o aprendizado.",
                    [
                        "Retomar a ideia principal",
                        "Explicar o aprendizado",
                        "Registrar o fechamento em ate cinco linhas"
                    ],
                    ["explicacao", "aprendizado", "sintese"])
            ],
            _ =>
            [
                new LiteratureUnitBlueprint(
                    "Ler com mais autonomia",
                    "Comecar a fase final com leitura mais independente e menos fala do adulto.",
                    "Diminua as interrupcoes e deixe a crianca ler, pensar e responder com mais iniciativa.",
                    "Resposta independente",
                    "Leia o trecho e responda sozinho a pergunta principal da unidade antes de revisar com o adulto.",
                    "A tarefa fica pronta quando a crianca entrega uma resposta coerente com pouca intervencao.",
                    "A evidencia opcional pode ser foto da resposta ou nota curta do adulto sobre a autonomia observada.",
                    [
                        "Ler com menos interrupcao",
                        "Planejar a resposta",
                        "Escrever com mais independencia"
                    ],
                    ["autonomia", "independente", "resposta"]),
                new LiteratureUnitBlueprint(
                    "Revisar e melhorar escrita",
                    "Mostrar que escrever bem tambem envolve revisar o que ja foi escrito.",
                    "Depois da primeira resposta, ajude a crianca a perceber o que ficou solto, repetido ou faltando.",
                    "Reescrita curta",
                    "Reescreva a propria resposta em uma versao mais clara, mais curta ou mais organizada.",
                    "Pode marcar como pronta quando a segunda versao estiver mais clara do que a primeira.",
                    "A evidencia opcional pode ser foto das duas versoes para mostrar crescimento.",
                    [
                        "Ler a primeira versao",
                        "Revisar o que faltou ou sobrou",
                        "Reescrever com mais clareza"
                    ],
                    ["revisao", "reescrita", "clareza"]),
                new LiteratureUnitBlueprint(
                    "Fechar e guardar memoria do ano",
                    "Encerrar o ciclo com uma memoria clara do que a leitura construiu na crianca ao longo do ano.",
                    "Nao transforme em trabalho longo. O foco e fechar com identidade, memoria e clareza do caminho percorrido.",
                    "Texto final do ciclo",
                    "Escreva um pequeno texto sobre a leitura mais marcante do ano, o que aprendeu e por que quer guardar isso.",
                    "A unidade fecha quando a crianca consegue olhar para o ano e registrar uma memoria propria da jornada.",
                    "Se a familia quiser, guarde o texto final como evidencia opcional da etapa.",
                    [
                        "Escolher a leitura mais marcante",
                        "Explicar por que ela importa",
                        "Registrar e guardar o fechamento"
                    ],
                    ["fechamento", "memoria", "ano"])
            ]
        };
    }

    private static FamilyLibraryRecommendationViewModel? PickBestDistinctBook(
        IReadOnlyList<FamilyLibraryBookCardViewModel> cards,
        int age,
        LearningDomain domain,
        IReadOnlyCollection<string> focusTokens,
        IUrlHelper url,
        Guid childId,
        ISet<Guid> usedIds,
        string accessLabel)
    {
        var recommendation = cards
            .Where(card => !usedIds.Contains(card.Id))
            .Select(card => new
            {
                Card = card,
                Score = ScoreBook(card, age, domain, focusTokens)
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Card.IsStarted)
            .ThenBy(item => item.Card.Title)
            .Where(item => item.Score > 0)
            .Select(item => MapRecommendation(
                item.Card,
                url,
                accessLabel,
                "Abrir biblioteca",
                url.Action("Index", "Library", new { childId }) ?? string.Empty,
                BuildFitReason(item.Card.Title, domain, false),
                childId))
            .FirstOrDefault();

        if (recommendation is not null)
        {
            usedIds.Add(recommendation.Id);
        }

        return recommendation;
    }

    private static FamilyLibraryRecommendationViewModel? PickBestDistinctPrintable(
        IReadOnlyList<FamilyLibraryPrintableCardViewModel> cards,
        int age,
        LearningDomain domain,
        IReadOnlyCollection<string> focusTokens,
        IUrlHelper url,
        Guid childId,
        ISet<Guid> usedIds,
        string accessLabel)
    {
        var recommendation = cards
            .Where(card => !usedIds.Contains(card.Id))
            .Select(card => new
            {
                Card = card,
                Score = ScorePrintable(card, age, domain, focusTokens)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Card.Title)
            .Where(item => item.Score > 0)
            .Select(item => MapRecommendation(
                item.Card,
                url,
                accessLabel,
                "Ver mais imprimíveis",
                url.Action("Printables", "Library", new { childId }) ?? string.Empty,
                BuildFitReason(item.Card.Title, domain, true),
                childId))
            .FirstOrDefault();

        if (recommendation is not null)
        {
            usedIds.Add(recommendation.Id);
        }

        return recommendation;
    }

    private static List<FamilyLibraryShelfViewModel> BuildShelves(
        IReadOnlyList<FamilyLibraryBookCardViewModel> books,
        IReadOnlyList<FamilyLibraryPrintableCardViewModel> printables,
        ChildProfile? child,
        IUrlHelper url)
    {
        if (child is not null)
        {
            var age = CalculateAge(child.BirthDate, DateTime.Today);
            var shelves = new List<FamilyLibraryShelfViewModel>();
            var authoredBooks = books
                .Where(book => string.Equals(book.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var authoredPrintables = printables
                .Where(printable => string.Equals(printable.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var preferredDomain = GetPreferredLibraryDomain(child.FamilyGoalTrack);

            if (authoredBooks.Count > 0 || authoredPrintables.Count > 0)
            {
                var ownedItems = new List<FamilyLibraryRecommendationViewModel>();
                var ownedBook = PickBestBook(authoredBooks, age, preferredDomain, [], url, "Abrir livro autoral", child.Id);
                var ownedPrintable = PickBestPrintable(authoredPrintables, age, preferredDomain, [], url, "Abrir folha autoral", child.Id);

                if (ownedBook is not null)
                {
                    ownedItems.Add(ownedBook);
                }

                if (ownedPrintable is not null)
                {
                    ownedItems.Add(ownedPrintable);
                }

                if (ownedItems.Count > 0)
                {
                    shelves.Add(new FamilyLibraryShelfViewModel
                    {
                        Title = "Currículo autoral NewSchool",
                        Summary = "Aqui ficam os livros e as folhas criados para a idade desta criança, já alinhados ao currículo principal do ano.",
                        ActionLabel = "Abrir coleção autoral",
                        ActionUrl = url.Action("Index", "Library", new { childId = child.Id, collection = ProprietaryFamilyLibraryCatalog.CollectionLabel }) ?? string.Empty,
                        Items = ownedItems
                    });
                }
            }

            foreach (var domain in new[]
                     {
                         LearningDomain.Language,
                         LearningDomain.Math,
                         LearningDomain.World,
                         LearningDomain.ExecutiveFunction
                     })
            {
                var book = PickBestBook(books, age, domain, [], url, "Abrir livro", child.Id);
                var printable = PickBestPrintable(printables, age, domain, [], url, "Abrir atividade", child.Id);
                var items = new[] { book, printable }.Where(item => item is not null).Cast<FamilyLibraryRecommendationViewModel>().ToList();
                if (items.Count == 0)
                {
                    continue;
                }

                shelves.Add(new FamilyLibraryShelfViewModel
                {
                    Title = BuildShelfTitle(domain),
                    Summary = BuildShelfSummary(domain),
                    ActionLabel = "Abrir todos desta rota",
                    ActionUrl = url.Action("Index", "Library", new { childId = child.Id }) ?? string.Empty,
                    Items = items
                });
            }

            return shelves;
        }

        var generalShelves = new List<FamilyLibraryShelfViewModel>();
        foreach (var collectionLabel in CollectionFilterOrder)
        {
            var items = books
                .Where(book => string.Equals(book.CollectionLabel, collectionLabel, StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .Select(book => MapRecommendation(
                    book,
                    url,
                    "Abrir livro",
                    "Filtrar esta coleção",
                    url.Action("Index", "Library", new { collection = collectionLabel }) ?? string.Empty,
                    $"Use esta coleção quando a família quiser focar em {collectionLabel.ToLowerInvariant()}."))
                .ToList();

            if (items.Count == 0)
            {
                continue;
            }

            generalShelves.Add(new FamilyLibraryShelfViewModel
            {
                Title = collectionLabel,
                Summary = "Uma forma rápida de começar sem procurar título por título.",
                ActionLabel = "Ver coleção",
                ActionUrl = url.Action("Index", "Library", new { collection = collectionLabel }) ?? string.Empty,
                Items = items
            });
        }

        var printableItems = printables
            .Take(4)
            .Select(printable => MapRecommendation(
                printable,
                url,
                "Abrir atividade",
                "Ver imprimíveis",
                url.Action("Printables", "Library") ?? string.Empty,
                "Quando a família precisar de algo rápido no papel, comece por aqui."))
            .ToList();

        if (printableItems.Count > 0)
        {
            generalShelves.Add(new FamilyLibraryShelfViewModel
            {
                Title = "Imprimíveis prontos",
                Summary = "Atividades de papel separadas da estante de leitura.",
                ActionLabel = "Abrir imprimíveis",
                ActionUrl = url.Action("Printables", "Library") ?? string.Empty,
                Items = printableItems
            });
        }

        return generalShelves;
    }

    private static FamilyLibraryRecommendationViewModel? PickBestBook(
        IReadOnlyList<FamilyLibraryBookCardViewModel> cards,
        int age,
        LearningDomain? domain,
        IReadOnlyCollection<string> focusTokens,
        IUrlHelper url,
        string accessLabel,
        Guid? childId = null)
    {
        var candidateCards = FilterBookCandidates(cards, age);

        return candidateCards
            .Select(card => new
            {
                Card = card,
                Score = ScoreBook(card, age, domain, focusTokens)
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Card.IsStarted)
            .ThenBy(item => item.Card.Title)
            .Where(item => item.Score > 0)
            .Select(item => MapRecommendation(
                item.Card,
                url,
                accessLabel,
                "Abrir estante",
                url.Action("Index", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty,
                BuildFitReason(item.Card.Title, domain, false),
                childId))
            .FirstOrDefault();
    }

    private static IReadOnlyList<FamilyLibraryBookCardViewModel> FilterBookCandidates(
        IReadOnlyList<FamilyLibraryBookCardViewModel> cards,
        int age)
    {
        var preferred = cards
            .Where(card => IsBookCompatibleWithAge(card, age))
            .ToList();

        return preferred.Count > 0 ? preferred : cards;
    }

    private static FamilyLibraryRecommendationViewModel? PickBestPrintable(
        IReadOnlyList<FamilyLibraryPrintableCardViewModel> cards,
        int age,
        LearningDomain? domain,
        IReadOnlyCollection<string> focusTokens,
        IUrlHelper url,
        string accessLabel,
        Guid? childId = null)
    {
        var candidateCards = FilterPrintableCandidates(cards, age);

        return candidateCards
            .Select(card => new
            {
                Card = card,
                Score = ScorePrintable(card, age, domain, focusTokens)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Card.Title)
            .Where(item => item.Score > 0)
            .Select(item => MapRecommendation(
                item.Card,
                url,
                accessLabel,
                "Ver mais imprimíveis",
                url.Action("Printables", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty,
                BuildFitReason(item.Card.Title, domain, true),
                childId))
            .FirstOrDefault();
    }

    private static IReadOnlyList<FamilyLibraryPrintableCardViewModel> FilterPrintableCandidates(
        IReadOnlyList<FamilyLibraryPrintableCardViewModel> cards,
        int age)
    {
        var preferred = cards
            .Where(card => IsPrintableCompatibleWithAge(card, age))
            .ToList();

        return preferred.Count > 0 ? preferred : cards;
    }

    private static IReadOnlyList<FamilyLibraryPrintableCardViewModel> FilterPrintableCandidatesForLesson(
        IReadOnlyList<FamilyLibraryPrintableCardViewModel> cards,
        int age,
        LearningDomain domain,
        IReadOnlyCollection<string> focusTokens)
    {
        var ageCandidates = FilterPrintableCandidates(cards, age);
        var domainKeywords = DomainKeywords.GetValueOrDefault(domain) ?? [];
        var focused = ageCandidates
            .Where(card =>
            {
                var searchable = BuildSearchableText(card.Title, card.Category, card.SkillFocus, card.Description, card.CollectionLabel);
                return ScoreKeywords(searchable, domainKeywords) > 0
                       || ScoreFocusTokens(searchable, focusTokens) > 0;
            })
            .ToList();

        return focused;
    }

    private static bool IsPrintableCompatibleWithAge(FamilyLibraryPrintableCardViewModel printable, int age)
    {
        if (printable.RecommendedMinAge > 0 && printable.RecommendedMaxAge > 0)
        {
            return age >= printable.RecommendedMinAge && age <= printable.RecommendedMaxAge;
        }

        if (TryParseAgeBand(printable.AgeLabel, out var minAge, out var maxAge))
        {
            return age >= minAge && age <= maxAge;
        }

        return string.Equals(
            printable.EducationStage,
            age <= 5 ? "Educação Infantil" : "Ensino Fundamental",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBookCompatibleWithAge(FamilyLibraryBookCardViewModel book, int age)
    {
        if (TryResolveBookAgeRange(book, out var minAge, out var maxAge))
        {
            return age >= minAge && age <= maxAge;
        }

        return string.Equals(
            book.EducationStage,
            age <= 5 ? "Educação Infantil" : "Ensino Fundamental",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveBookAgeRange(FamilyLibraryBookCardViewModel book, out int minAge, out int maxAge)
    {
        minAge = 0;
        maxAge = 0;

        var resolvedAgeLabel = !string.IsNullOrWhiteSpace(book.AgeLabel)
            ? book.AgeLabel
            : GetAgeLabel(book.Title, book.RecommendedMinAge, book.RecommendedMaxAge);

        if (TryParseAgeBand(resolvedAgeLabel, out minAge, out maxAge))
        {
            return true;
        }

        if (book.RecommendedMinAge > 0 && book.RecommendedMaxAge > 0)
        {
            minAge = book.RecommendedMinAge;
            maxAge = book.RecommendedMaxAge;
            return true;
        }

        return false;
    }

    private static bool TryParseAgeBand(string ageLabel, out int minAge, out int maxAge)
    {
        minAge = 0;
        maxAge = 0;

        if (string.IsNullOrWhiteSpace(ageLabel))
        {
            return false;
        }

        var digits = Regex.Matches(ageLabel, @"\d+")
            .Select(match => int.TryParse(match.Value, out var value) ? value : 0)
            .Where(value => value > 0)
            .ToList();

        if (digits.Count == 0)
        {
            return false;
        }

        if (ageLabel.Contains("º ano", StringComparison.OrdinalIgnoreCase) ||
            ageLabel.Contains("o ano", StringComparison.OrdinalIgnoreCase))
        {
            var schoolYear = digits[0];
            minAge = schoolYear + 5;
            maxAge = schoolYear + 6;
            return true;
        }

        if (digits.Count == 1)
        {
            minAge = digits[0];
            maxAge = digits[0];
            return true;
        }

        minAge = Math.Min(digits[0], digits[1]);
        maxAge = Math.Max(digits[0], digits[1]);
        return true;
    }

    private static int ScoreBook(
        FamilyLibraryBookCardViewModel book,
        int age,
        LearningDomain? domain,
        IReadOnlyCollection<string> focusTokens)
    {
        var score = TryResolveBookAgeRange(book, out var minAge, out var maxAge)
            ? ScoreAge(minAge, maxAge, age)
            : ScoreAge(book.RecommendedMinAge, book.RecommendedMaxAge, age);
        var searchable = BuildSearchableText(book.Title, book.Category, book.SkillFocus, book.Description, book.CollectionLabel);

        if (book.IsStarted && !book.IsCompleted)
        {
            score += 12;
        }

        if (book.IsFavorite)
        {
            score += 6;
        }

        if (string.Equals(book.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase))
        {
            score += 14;
        }

        if (domain.HasValue)
        {
            score += ScoreKeywords(searchable, DomainKeywords.GetValueOrDefault(domain.Value) ?? []);
        }

        score += ScoreFocusTokens(searchable, focusTokens);
        score += ScoreStage(book.EducationStage, age);
        return score;
    }

    private static FamilyLibraryBookCardViewModel? PickCurrentBookForChild(
        IReadOnlyList<FamilyLibraryBookCardViewModel> cards,
        int age,
        LearningDomain? domain)
    {
        var candidates = FilterBookCandidates(cards, age);

        return candidates
            .Where(book => book.IsStarted && !book.IsCompleted)
            .Select(book => new
            {
                Book = book,
                Score = ScoreBook(book, age, domain, [])
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Book.LastReadAtUtc)
            .ThenBy(item => item.Book.Title)
            .Select(item => item.Book)
            .FirstOrDefault();
    }

    private static int ScorePrintable(
        FamilyLibraryPrintableCardViewModel printable,
        int age,
        LearningDomain? domain,
        IReadOnlyCollection<string> focusTokens)
    {
        var score = ScoreAge(printable.RecommendedMinAge, printable.RecommendedMaxAge, age) + 4;
        var searchable = BuildSearchableText(printable.Title, printable.Category, printable.SkillFocus, printable.Description, printable.CollectionLabel);

        if (domain.HasValue)
        {
            score += ScoreKeywords(searchable, DomainKeywords.GetValueOrDefault(domain.Value) ?? []);
        }

        score += ScoreFocusTokens(searchable, focusTokens);
        score += ScoreStage(printable.EducationStage, age);

        if (string.Equals(printable.CollectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase))
        {
            score += 18;
        }

        return score;
    }

    private static int ScoreAge(int minAge, int maxAge, int age)
    {
        if (minAge <= 0 || maxAge <= 0)
        {
            return 8;
        }

        if (age >= minAge && age <= maxAge)
        {
            return 32;
        }

        var distance = age < minAge ? minAge - age : age - maxAge;
        return Math.Max(0, 24 - (distance * 8));
    }

    private static int ScoreStage(string stage, int age)
    {
        var expectedStage = age <= 5 ? "Educação Infantil" : "Ensino Fundamental";
        return string.Equals(stage, expectedStage, StringComparison.OrdinalIgnoreCase) ? 10 : 0;
    }

    private static int ScoreKeywords(string searchable, IReadOnlyCollection<string> keywords)
    {
        var score = 0;
        foreach (var keyword in keywords)
        {
            if (searchable.Contains(NormalizeText(keyword), StringComparison.Ordinal))
            {
                score += 9;
            }
        }

        return score;
    }

    private static int ScoreFocusTokens(string searchable, IReadOnlyCollection<string> focusTokens)
    {
        var score = 0;
        foreach (var token in focusTokens)
        {
            if (searchable.Contains(token, StringComparison.Ordinal))
            {
                score += 5;
            }
        }

        return score;
    }

    private static IReadOnlyCollection<string> ExtractFocusTokens(IReadOnlyCollection<DailyPlanBlock> blocks)
    {
        return blocks
            .SelectMany(block => $"{block.Title} {block.Goal} {block.SkillName}".Split([' ', ',', ';', '.', ':', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(NormalizeText)
            .Where(token => token.Length >= 4)
            .Where(token => !IgnoredFocusTokens.Contains(token))
            .Distinct(StringComparer.Ordinal)
            .Take(12)
            .ToList();
    }

    private static IReadOnlyCollection<string> ExtractFocusTokens(
        DailyPlanBlock block,
        CuratedTaskSuggestionViewModel? suggestion)
    {
        var baseTokens = ExtractFocusTokens([block]).ToList();
        if (suggestion is null)
        {
            return baseTokens;
        }

        var suggestionTokens = new List<string>();
        suggestionTokens.AddRange(suggestion.FocusTokens);

        if (suggestion.LessonPacket is not null)
        {
            suggestionTokens.AddRange(
                SplitSearchTerms(
                    $"{suggestion.LessonPacket.CurriculumPlacement} {suggestion.LessonPacket.UnitTitle} {suggestion.LessonPacket.CoreMaterialTitle} {suggestion.LessonPacket.PracticeTask}"));
        }

        if (!string.IsNullOrWhiteSpace(suggestion.Title))
        {
            suggestionTokens.AddRange(SplitSearchTerms(suggestion.Title));
        }

        if (!string.IsNullOrWhiteSpace(suggestion.MaterialsSummary))
        {
            suggestionTokens.AddRange(SplitSearchTerms(suggestion.MaterialsSummary));
        }

        return baseTokens
            .Concat(suggestionTokens.Select(NormalizeText))
            .Where(token => token.Length >= 4)
            .Where(token => !IgnoredFocusTokens.Contains(token))
            .Distinct(StringComparer.Ordinal)
            .Take(18)
            .ToList();
    }

    private static string BuildSearchableText(params string[] values)
    {
        return NormalizeText(string.Join(' ', values.Where(value => !string.IsNullOrWhiteSpace(value))));
    }

    private static IEnumerable<string> SplitSearchTerms(string value)
    {
        return value
            .Split([' ', ',', ';', '.', ':', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeText);
    }

    private static LearningDomain? GetDominantDomain(IReadOnlyCollection<DailyPlanBlock> blocks)
    {
        return blocks
            .GroupBy(block => block.Domain)
            .OrderByDescending(group => group.Count())
            .Select(group => (LearningDomain?)group.Key)
            .FirstOrDefault();
    }

    private static FamilyLibraryRecommendationViewModel MapRecommendation(
        FamilyLibraryBookCardViewModel book,
        IUrlHelper url,
        string accessLabel,
        string secondaryActionLabel,
        string secondaryActionUrl,
        string fitReason,
        Guid? childId = null)
    {
        object routeValues = childId.HasValue
            ? new { id = book.Id, childId }
            : new { id = book.Id };

        return new FamilyLibraryRecommendationViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Category = book.Category,
            EducationStage = book.EducationStage,
            CollectionLabel = book.CollectionLabel,
            SkillFocus = book.SkillFocus,
            Description = book.Description,
            AgeLabel = book.AgeLabel,
            CoverImageUrl = book.CoverImageUrl,
            ProgressLabel = book.IsCompleted
                ? "Leitura concluída"
                : book.IsStarted
                    ? $"Parou na página {book.CurrentPageNumber} de {book.PageCount}"
                    : $"{book.PageCount} página(s)",
            FitReason = DecorateFitReason(fitReason, book.CollectionLabel, false),
            AccessLabel = accessLabel,
            AccessUrl = url.Action("Book", "Library", routeValues) ?? string.Empty,
            SecondaryActionLabel = secondaryActionLabel,
            SecondaryActionUrl = secondaryActionUrl,
            PageCount = book.PageCount,
            CurrentPageNumber = book.CurrentPageNumber,
            IsPrintable = false,
            IsStarted = book.IsStarted,
            IsCompleted = book.IsCompleted,
            IsFavorite = book.IsFavorite
        };
    }

    private static FamilyLibraryRecommendationViewModel MapRecommendation(
        FamilyLibraryPrintableCardViewModel printable,
        IUrlHelper url,
        string accessLabel,
        string secondaryActionLabel,
        string secondaryActionUrl,
        string fitReason,
        Guid? childId = null)
    {
        object routeValues = childId.HasValue
            ? new { id = printable.Id, childId }
            : new { id = printable.Id };

        return new FamilyLibraryRecommendationViewModel
        {
            Id = printable.Id,
            Title = printable.Title,
            Category = printable.Category,
            EducationStage = printable.EducationStage,
            CollectionLabel = printable.CollectionLabel,
            SkillFocus = printable.SkillFocus,
            Description = printable.Description,
            AgeLabel = printable.AgeLabel,
            ProgressLabel = $"{printable.PageCount} página(s)",
            FitReason = DecorateFitReason(fitReason, printable.CollectionLabel, true),
            AccessLabel = accessLabel,
            AccessUrl = url.Action("Printable", "Library", routeValues) ?? string.Empty,
            SecondaryActionLabel = secondaryActionLabel,
            SecondaryActionUrl = secondaryActionUrl,
            PageCount = printable.PageCount,
            CurrentPageNumber = 1,
            IsPrintable = true
        };
    }

    private static string BuildFitReason(string title, LearningDomain? domain, bool printable)
    {
        var domainText = domain.HasValue
            ? FormatDomain(domain.Value).ToLowerInvariant()
            : "a etapa atual";
        var formatText = printable ? "folha para papel" : "leitura guiada";
        return $"{title} entrou como {formatText} porque conversa bem com {domainText} sem aumentar a complexidade da rotina.";
    }

    private static string DecorateFitReason(string fitReason, string collectionLabel, bool printable)
    {
        if (!string.Equals(collectionLabel, ProprietaryFamilyLibraryCatalog.CollectionLabel, StringComparison.OrdinalIgnoreCase))
        {
            return fitReason;
        }

        var prefix = printable
            ? "Conteúdo autoral do NewSchool: "
            : "Livro autoral do NewSchool: ";

        return $"{prefix}{fitReason}";
    }

    private static string BuildShelfTitle(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem e leitura",
        LearningDomain.Math => "Matemática em papel e leitura",
        LearningDomain.Science => "Ciências e descoberta",
        LearningDomain.History => "História e memória",
        LearningDomain.Geography => "Geografia e território",
        LearningDomain.ExecutiveFunction => "Autonomia e coordenação",
        _ => "Currículo por área"
    };

    private static string BuildShelfSummary(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Um livro para ler junto e uma atividade para consolidar.",
        LearningDomain.Math => "Material concreto e atividade curta para praticar sem improviso.",
        LearningDomain.Science => "Leitura e folha para ligar o currículo à observação e à investigação.",
        LearningDomain.History => "Leitura e folha para ligar o currículo à memória, ao tempo e às fontes.",
        LearningDomain.Geography => "Leitura e folha para ligar o currículo a mapa, lugar e território.",
        LearningDomain.ExecutiveFunction => "Apoios leves para rotina, coordenação e independência.",
        _ => "Seleção pronta para esta área."
    };

    private static LearningDomain GetPreferredLibraryDomain(string familyGoalTrack) => CurriculumStructure.GetPreferredLibraryDomain(familyGoalTrack);

    private static FamilyLibraryBookCardViewModel MapBookCard(FamilyLibraryMaterial material, FamilyLibraryUserState? state)
    {
        var isCompleted = state?.IsCompleted == true;
        var isStarted = state is not null && (state.CurrentPageNumber > 1 || state.LastReadAtUtc.HasValue || state.StartedAtUtc.HasValue);
        var statusLabel = isCompleted
            ? "Lido"
            : isStarted
                ? "Lendo"
                : "Novo";
        var statusCssClass = isCompleted
            ? "complete"
            : isStarted
                ? "reading"
                : "new";

        return new FamilyLibraryBookCardViewModel
        {
            Id = material.Id,
            Title = material.Title,
            Category = material.Category,
            EducationStage = material.EducationStage,
            RecommendedMinAge = material.RecommendedMinAge,
            RecommendedMaxAge = material.RecommendedMaxAge,
            SkillFocus = material.SkillFocus,
            Description = material.Description,
            CollectionLabel = material.CollectionLabel,
            PageCount = material.PageCount,
            CurrentPageNumber = Math.Max(1, state?.CurrentPageNumber ?? 1),
            IsStarted = isStarted,
            IsFavorite = state?.IsFavorite == true,
            IsCompleted = isCompleted,
            LastReadAtUtc = state?.LastReadAtUtc,
            CoverImageUrl = BuildImageUrl(material.CoverImageRelativePath),
            StatusLabel = statusLabel,
            StatusCssClass = statusCssClass,
            AgeLabel = GetAgeLabel(material.Title, material.RecommendedMinAge, material.RecommendedMaxAge)
        };
    }

    private static bool MatchesFilters(FamilyLibraryBookCardViewModel book, string? searchTerm, string? collection, string? stage, string? ageFilter)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm)
            && !NormalizeText(book.Title).Contains(NormalizeText(searchTerm), StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(collection)
            && !string.Equals(book.CollectionLabel, collection, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(stage)
            && !string.Equals(book.EducationStage, stage, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(ageFilter)
            && !string.Equals(book.AgeLabel, ageFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string ResolveAgeFilter(string? explicitAgeFilter, ChildProfile? child, IEnumerable<string> availableFilters)
    {
        if (!string.IsNullOrWhiteSpace(explicitAgeFilter))
        {
            return explicitAgeFilter;
        }

        if (child is null)
        {
            return string.Empty;
        }

        var childAgeLabel = BuildAgeLabelFromAge(CalculateAge(child.BirthDate, DateTime.Today));
        return availableFilters.Contains(childAgeLabel, StringComparer.OrdinalIgnoreCase)
            ? childAgeLabel
            : string.Empty;
    }

    private static string BuildAgeLabelFromAge(int age) => age == 1 ? "1 ano" : $"{age} anos";

    private static string GetAgeLabel(string title, int minAge, int maxAge)
    {
        var normalizedTitle = NormalizeText(title);
        var schoolYearMatch = SchoolYearRegex.Match(normalizedTitle);
        if (schoolYearMatch.Success && int.TryParse(schoolYearMatch.Groups[1].Value, out var schoolYear))
        {
            return $"{schoolYear}º ano";
        }

        var ageMatch = AgeRegex.Match(normalizedTitle);
        if (ageMatch.Success && int.TryParse(ageMatch.Groups[1].Value, out var age))
        {
            return age == 1 ? "1 ano" : $"{age} anos";
        }

        if (minAge > 0 && maxAge > 0)
        {
            return minAge == maxAge
                ? (minAge == 1 ? "1 ano" : $"{minAge} anos")
                : $"{minAge} a {maxAge} anos";
        }

        return string.Empty;
    }

    private static int GetAgeFilterSortOrder(string filter)
    {
        var digits = new string(filter.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : int.MaxValue;
    }

    private static int GetStageSortOrder(string stage) => stage switch
    {
        "Educação Infantil" => 0,
        "Ensino Fundamental" => 1,
        "Ensino Médio" => 2,
        "Família" => 3,
        _ => 4
    };

    private static string FormatDomain(LearningDomain domain) => CurriculumStructure.FormatDomainLabel(domain);

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static string BuildImageUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        return $"/family-library-assets/{relativePath.Replace('\\', '/').TrimStart('/')}";
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

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    private static bool IsPrintableMaterial(FamilyLibraryMaterial material)
    {
        return FamilyLibraryMaterialClassifier.IsPrintable(
            material.IsPrintable,
            material.Title,
            material.Category,
            material.SourceRelativePath,
            material.SkillFocus,
            material.Description);
    }

    private static bool IsBookMaterial(FamilyLibraryMaterial material)
    {
        return FamilyLibraryMaterialClassifier.IsBook(
            material.IsPrintable,
            material.Title,
            material.Category,
            material.SourceRelativePath,
            material.SkillFocus,
            material.Description);
    }

    private sealed record ReadingPhaseBlueprint(
        int PhaseNumber,
        string Title,
        string Summary,
        string WeeklyRhythm,
        string ParentGuide,
        string CompletionSignal,
        LearningDomain Domain,
        IReadOnlyList<string> FocusTokens);

    private sealed record LiteratureUnitBlueprint(
        string Title,
        string Summary,
        string ParentGuide,
        string WritingTaskTitle,
        string WritingTaskPrompt,
        string WritingCompletionSignal,
        string OptionalEvidencePrompt,
        IReadOnlyList<string> UnitFlow,
        IReadOnlyList<string> FocusTokens);

    private sealed record LibraryIndexData(
        List<FamilyLibraryBookCardViewModel> Books,
        List<FamilyLibraryPrintableCardViewModel> Printables);
}
