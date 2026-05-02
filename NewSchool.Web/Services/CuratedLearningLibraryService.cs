using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class CuratedLearningLibraryService(
    ApplicationDbContext db,
    ProprietaryLessonPacketService proprietaryLessonPacketService)
{
    public async Task<Dictionary<Guid, CuratedTaskSuggestionViewModel>> BuildBlockSuggestionsAsync(
        ChildProfile child,
        ICollection<DailyPlanBlock> blocks)
    {
        if (blocks.Count == 0)
        {
            return new Dictionary<Guid, CuratedTaskSuggestionViewModel>();
        }

        var age = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14);
        var tasks = await db.CuratedTaskTemplates
            .Include(x => x.PrimaryResource)
            .Where(x => x.AgeMin <= age && x.AgeMax >= age)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var suggestions = new Dictionary<Guid, CuratedTaskSuggestionViewModel>();
        var usedTaskIds = new HashSet<Guid>();
        foreach (var block in blocks.OrderBy(x => x.SortOrder))
        {
            var match = MatchTask(tasks, child, block, usedTaskIds);
            if (match is null)
            {
                continue;
            }

            usedTaskIds.Add(match.Id);
            var lessonPacket = proprietaryLessonPacketService.BuildPacket(match, child, block, age);

            suggestions[block.Id] = new CuratedTaskSuggestionViewModel
            {
                Slug = match.Slug,
                Title = match.Title,
                FitReason = BuildFitReason(match, child, block),
                Goal = match.Goal,
                ParentGuide = !string.IsNullOrWhiteSpace(lessonPacket.OpeningForAdult)
                    ? lessonPacket.OpeningForAdult
                    : match.ParentGuide,
                ChildPrompt = !string.IsNullOrWhiteSpace(lessonPacket.AnchorQuestion)
                    ? lessonPacket.AnchorQuestion
                    : match.ChildPrompt,
                MaterialsSummary = match.MaterialsSummary,
                EvidencePrompt = match.EvidencePrompt,
                ExpectedOutcome = match.ExpectedOutcome,
                SuggestedMinutes = match.SuggestedMinutes,
                Steps = SplitLines(match.TaskSteps),
                FocusTokens = SplitKeywords(match.MatchKeywords)
                    .Concat(ExtractPacketKeywords(lessonPacket))
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                LessonPacket = lessonPacket,
                PrimaryResource = match.PrimaryResource is null ? null : MapResource(match.PrimaryResource),
                SupportLink = !ShouldExposeSupportLink(match.SupportLinkUrl)
                    ? null
                    : new CuratedSupportLinkViewModel
                    {
                        Label = string.IsNullOrWhiteSpace(match.SupportLinkLabel) ? "Abrir apoio externo" : match.SupportLinkLabel,
                        Url = match.SupportLinkUrl,
                        SourceLabel = match.SupportLinkSource
                    }
            };
        }

        return suggestions;
    }

    public async Task<List<ParentAcademyResourceViewModel>> BuildHostedLibraryAsync(ChildProfile? child)
    {
        var query = db.CuratedLearningResources
            .Where(x => x.IsHostedLocally)
            .Where(x => x.LanguageCode.StartsWith("pt"))
            .OrderBy(x => x.SortOrder)
            .AsQueryable();

        if (child is not null)
        {
            var age = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14);
            query = query.Where(x => x.AgeMin <= age && x.AgeMax >= age);
        }

        var resources = await query.ToListAsync();
        return resources
            .Select(MapAcademyResource)
            .ToList();
    }

    private static CuratedTaskTemplate? MatchTask(
        IReadOnlyCollection<CuratedTaskTemplate> tasks,
        ChildProfile child,
        DailyPlanBlock block,
        IReadOnlySet<Guid> usedTaskIds)
    {
        var searchText = NormalizeForSearch($"{block.Title} {block.SkillName} {block.Goal} {block.Materials} {child.FamilyGoalTrack} {child.TeachingMethodology}");
        var orderedCandidates = tasks
            .Where(x => x.Domain == block.Domain)
            .Select(x => new
            {
                Task = x,
                Score = ScoreTask(x, child, block, searchText)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Task.SortOrder)
            .ToList();

        var freshCandidate = orderedCandidates
            .FirstOrDefault(x => !usedTaskIds.Contains(x.Task.Id));

        return freshCandidate?.Task ?? orderedCandidates
            .Select(x => x.Task)
            .FirstOrDefault();
    }

    private static int ScoreTask(
        CuratedTaskTemplate task,
        ChildProfile child,
        DailyPlanBlock block,
        string searchText)
    {
        var score = 100;

        if (task.FunctionalTrack == block.FunctionalTrack)
        {
            score += 28;
        }
        else if (task.FunctionalTrack == FunctionalSupportTrack.Base)
        {
            score += 12;
        }

        if (string.Equals(task.GoalTrack, child.FamilyGoalTrack, StringComparison.OrdinalIgnoreCase))
        {
            score += 18;
        }
        else if (string.Equals(task.GoalTrack, "balanced_growth", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }

        if (block.IsRecoveryFocus || block.IsSpacedReview)
        {
            score += 8;
        }

        var keywords = SplitKeywords(task.MatchKeywords);
        var keywordMatches = keywords.Count(keyword => searchText.Contains(keyword, StringComparison.Ordinal));
        score += keywordMatches * 8;

        var durationDelta = Math.Abs(block.DurationMinutes - task.SuggestedMinutes);
        score += durationDelta switch
        {
            <= 3 => 10,
            <= 6 => 7,
            <= 10 => 4,
            _ => 0
        };

        if (task.PrimaryResource is { LanguageCode: var languageCode } &&
            IsFamilyFacingLanguage(languageCode))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(task.SupportLinkUrl) &&
            task.SupportLinkUrl.StartsWith("/Parent/ExternalContent", StringComparison.Ordinal))
        {
            score += 5;
        }

        return score;
    }

    private static string BuildFitReason(
        CuratedTaskTemplate task,
        ChildProfile child,
        DailyPlanBlock block)
    {
        var reasons = new List<string>();

        if (task.Domain == block.Domain)
        {
            reasons.Add($"bate com {FormatDomain(block.Domain).ToLowerInvariant()}");
        }

        if (task.FunctionalTrack == block.FunctionalTrack)
        {
            reasons.Add($"respeita o foco funcional em {FormatFunctionalTrack(block.FunctionalTrack).ToLowerInvariant()}");
        }

        if (string.Equals(task.GoalTrack, child.FamilyGoalTrack, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add($"alinha com a {FormatGoalTrack(child.FamilyGoalTrack).ToLowerInvariant()}");
        }

        if (task.SuggestedMinutes <= block.DurationMinutes + 4 && task.SuggestedMinutes >= block.DurationMinutes - 6)
        {
            reasons.Add("cabe no tempo real deste bloco");
        }

        return reasons.Count == 0
            ? "Essa tarefa foi puxada porque combina com a idade da crianca e com a materia escolhida para hoje."
            : $"Essa tarefa entrou porque {string.Join(", ", reasons)}.";
    }

    private static ParentAcademyResourceViewModel MapAcademyResource(CuratedLearningResource resource)
    {
        return new ParentAcademyResourceViewModel
        {
            Title = resource.Title,
            Summary = resource.Summary,
            WhyItMatters = "Esse item pode ficar dentro do NewSchool porque a licença da fonte permite redistribuição com crédito claro.",
            PortugueseGuideNote = resource.UseNote,
            FormatLabel = resource.FormatLabel,
            DurationLabel = GetAgeBand(resource.AgeMin, resource.AgeMax),
            SourceLabel = resource.SourceName,
            OwnershipLabel = "Hospedado com licença clara",
            IsThirdParty = false,
            AccessLabel = resource.IsHostedLocally ? "Abrir material hospedado" : "Abrir fonte oficial",
            Url = resource.AccessUrl,
            AudienceLabel = FormatDomain(resource.Domain),
            AudienceChipClass = GetDomainChip(resource.Domain),
            LicenseLabel = resource.LicenseLabel,
            Attribution = resource.Attribution,
            SourceUrl = resource.SourceUrl,
            HostingLabel = resource.IsHostedLocally ? "Hospedado no NewSchool" : "Abrir na fonte oficial"
        };
    }

    private static CuratedResourceCardViewModel MapResource(CuratedLearningResource resource)
    {
        return new CuratedResourceCardViewModel
        {
            Title = resource.Title,
            Summary = resource.Summary,
            FormatLabel = resource.FormatLabel,
            SourceLabel = resource.SourceName,
            SourceUrl = resource.SourceUrl,
            AccessUrl = resource.AccessUrl,
            AccessLabel = resource.IsHostedLocally ? "Abrir material hospedado" : "Abrir fonte oficial",
            LicenseLabel = resource.LicenseLabel,
            Attribution = resource.Attribution,
            UseNote = resource.UseNote,
            IsHostedLocally = resource.IsHostedLocally,
            LanguageCode = resource.LanguageCode,
            FamilyFacing = IsFamilyFacingLanguage(resource.LanguageCode)
        };
    }

    private static string FormatDomain(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem",
        LearningDomain.Math => "Matemática",
        LearningDomain.World => "Mundo real",
        LearningDomain.ExecutiveFunction => "Autonomia",
        _ => "Geral"
    };

    private static string GetDomainChip(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "track-communication",
        LearningDomain.Math => "track-academic",
        LearningDomain.World => "success",
        LearningDomain.ExecutiveFunction => "track-dailyliving",
        _ => "neutral"
    };

    private static string GetAgeBand(int ageMin, int ageMax) => ageMin == ageMax
        ? $"{ageMin} anos"
        : $"{ageMin} a {ageMax} anos";

    private static string FormatGoalTrack(string goalTrack) => goalTrack switch
    {
        "literacy" => "trilha de alfabetizacao",
        "math_foundations" => "trilha de matematica base",
        "autonomy" => "trilha de autonomia e foco",
        "science_discovery" => "trilha de ciencias em casa",
        _ => "trilha equilibrada"
    };

    private static string FormatFunctionalTrack(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => "comunicacao",
        FunctionalSupportTrack.Regulation => "regulacao",
        FunctionalSupportTrack.Sensory => "sensorial",
        FunctionalSupportTrack.DailyLiving => "vida diaria",
        FunctionalSupportTrack.AcademicAdapted => "academico adaptado",
        _ => "base academica"
    };

    private static bool IsFamilyFacingLanguage(string? languageCode)
    {
        return !string.IsNullOrWhiteSpace(languageCode)
               && languageCode.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldExposeSupportLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (url.StartsWith("/", StringComparison.Ordinal))
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();
        return host.Contains("educalar.com.br", StringComparison.Ordinal)
               || host.Contains("baixelivros.com.br", StringComparison.Ordinal)
               || host.Contains("archivepublicdomain.com", StringComparison.Ordinal);
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

    private static List<string> SplitLines(string value)
    {
        return value
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static List<string> SplitKeywords(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeForSearch)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static IEnumerable<string> ExtractPacketKeywords(ProprietaryLessonPacketViewModel packet)
    {
        return NormalizeForSearch(
                string.Join(
                    ' ',
                    new[]
                    {
                        packet.CurriculumPlacement,
                        packet.UnitTitle,
                        packet.UnitSummary,
                        packet.CoreMaterialTitle,
                        packet.PracticeTask
                    }))
            .Split([' ', ',', ';', '.', ':', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 4)
            .Distinct(StringComparer.Ordinal);
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
}
