using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class DailyTrailComposerService
{
    public DailyTrailViewModel Build(
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel> suggestions,
        IReadOnlyCollection<ExternalContentProgressCardViewModel> externalRecommendations)
    {
        if (blocks.Count == 0)
        {
            return new DailyTrailViewModel
            {
                Headline = "A trilha do dia sera montada automaticamente",
                Summary = "Assim que houver blocos no plano, o sistema organiza materia, tarefa, evidencia e apoio externo sem improviso."
            };
        }

        var orderedBlocks = blocks
            .OrderBy(x => x.SortOrder)
            .ToList();
        var dominantDomain = orderedBlocks
            .GroupBy(x => x.Domain)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => GetDomainPriority(x.Key))
            .Select(x => x.Key)
            .First();

        var orderedDomains = orderedBlocks
            .Select(x => FormatDomain(x.Domain))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var preferredSupport = orderedBlocks
            .Select(block => suggestions.TryGetValue(block.Id, out var task) ? task : null)
            .Where(task => task?.SupportLink is not null && task.SupportLink.Url.StartsWith("/", StringComparison.Ordinal))
            .Select(task => task!.SupportLink!)
            .FirstOrDefault();
        var fallbackExternal = externalRecommendations
            .OrderBy(x => x.Completed)
            .ThenBy(x => x.Title)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GuideUrl));

        var expectedOutcomes = orderedBlocks
            .Select(block => suggestions.TryGetValue(block.Id, out var task) ? task.ExpectedOutcome : string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();

        var materials = orderedBlocks
            .SelectMany(block => SplitChecklistValue(
                suggestions.TryGetValue(block.Id, out var task)
                    ? task.MaterialsSummary
                    : block.Materials))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();

        var evidences = orderedBlocks
            .Select(block => suggestions.TryGetValue(block.Id, out var task) ? task.EvidencePrompt : block.EvidencePrompt)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToList();

        return new DailyTrailViewModel
        {
            Headline = BuildHeadline(child.FamilyGoalTrack, dominantDomain),
            Summary = BuildSummary(orderedDomains, child.DailyStudyMinutes, orderedBlocks.Count),
            FocusAreaLabel = FormatDomain(dominantDomain),
            FocusAreaChipClass = GetDomainChipClass(dominantDomain),
            GoalTrackLabel = FormatGoalTrack(child.FamilyGoalTrack),
            TotalMinutes = orderedBlocks.Sum(x => x.DurationMinutes),
            BlockCount = orderedBlocks.Count,
            DailyOutcome = expectedOutcomes.Count == 0
                ? "Ao final da aula, a crianca deve sair com uma pequena vitoria concreta e pelo menos uma evidencia salva."
                : $"Ao final da trilha, buscamos: {string.Join(" Depois disso, ", expectedOutcomes)}",
            ExternalGuideLabel = preferredSupport is not null
                ? preferredSupport.Label
                : fallbackExternal?.Title ?? string.Empty,
            ExternalGuideUrl = preferredSupport?.Url ?? fallbackExternal?.GuideUrl ?? string.Empty,
            ExternalGuideReason = preferredSupport is not null
                ? "Esse apoio ja foi encaixado pelo sistema porque conversa diretamente com a tarefa de hoje."
                : fallbackExternal?.RecommendedReason ?? string.Empty,
            MaterialChecklist = materials,
            EvidenceChecklist = evidences,
            Stages = orderedBlocks.Select((block, index) =>
            {
                var suggestion = suggestions.TryGetValue(block.Id, out var task) ? task : null;
                return new DailyTrailStageViewModel
                {
                    OrderLabel = $"{index + 1}. {BuildStageVerb(block.Domain)}",
                    DomainLabel = FormatDomain(block.Domain),
                    DomainChipClass = GetDomainChipClass(block.Domain),
                    Title = suggestion?.Title ?? block.Title,
                    Goal = suggestion?.Goal ?? block.Goal,
                    EvidenceLabel = suggestion?.EvidencePrompt ?? block.EvidencePrompt
                };
            }).ToList()
        };
    }

    private static string BuildHeadline(string familyGoalTrack, LearningDomain dominantDomain) => familyGoalTrack switch
    {
        "literacy" => "Alfabetizacao guiada e mastigada para hoje",
        "math_foundations" => "Matematica do dia pronta, sem improviso",
        "autonomy" => "Rotina, foco e autonomia fechados para hoje",
        "science_discovery" => "Descoberta guiada e registro prontos para hoje",
        _ => dominantDomain switch
        {
            LearningDomain.Language => "Linguagem do dia organizada passo a passo",
            LearningDomain.Math => "Matematica do dia organizada passo a passo",
            LearningDomain.Science => "Ciências do dia organizadas passo a passo",
            LearningDomain.History => "História do dia organizada passo a passo",
            LearningDomain.Geography => "Geografia do dia organizada passo a passo",
            LearningDomain.ExecutiveFunction => "Autonomia do dia organizada passo a passo",
            _ => "Trilha inteligente pronta para aplicar hoje"
        }
    };

    private static string BuildSummary(IReadOnlyList<string> orderedDomains, int targetMinutes, int blockCount)
    {
        var domainSummary = orderedDomains.Count switch
        {
            0 => "uma sequencia enxuta e clara",
            1 => orderedDomains[0].ToLowerInvariant(),
            2 => $"{orderedDomains[0].ToLowerInvariant()} e {orderedDomains[1].ToLowerInvariant()}",
            _ => string.Join(", ", orderedDomains.Take(orderedDomains.Count - 1).Select(x => x.ToLowerInvariant())) +
                 $" e {orderedDomains[^1].ToLowerInvariant()}"
        };

        return $"Hoje o sistema montou {blockCount} etapa(s) cobrindo {domainSummary}, com meta de cerca de {targetMinutes} minutos. O adulto so precisa seguir uma etapa por vez, usar o material separado e salvar a prova combinada.";
    }

    private static IEnumerable<string> SplitChecklistValue(string value)
    {
        return value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Trim().TrimEnd('.'))
            .Where(item => item.Length >= 4)
            .Select(item => item.Length > 95 ? $"{item[..92]}..." : item);
    }

    private static string BuildStageVerb(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Abrir linguagem",
        LearningDomain.Math => "Consolidar matematica",
        LearningDomain.Science => "Investigar ciências",
        LearningDomain.History => "Ler e organizar a história",
        LearningDomain.Geography => "Ler espaço e território",
        LearningDomain.ExecutiveFunction => "Fechar com autonomia",
        _ => "Executar etapa"
    };

    private static string FormatDomain(LearningDomain domain) => CurriculumStructure.FormatDomainLabel(domain);

    private static string GetDomainChipClass(LearningDomain domain) => CurriculumStructure.GetDomainChipClass(domain);

    private static string FormatGoalTrack(string goalTrack) => goalTrack switch
    {
        "literacy" => "Trilha de alfabetizacao",
        "math_foundations" => "Trilha de matematica base",
        "autonomy" => "Trilha de autonomia e foco",
        "science_discovery" => "Trilha de ciencias em casa",
        _ => "Trilha de crescimento equilibrado"
    };

    private static int GetDomainPriority(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => 1,
        LearningDomain.Math => 2,
        LearningDomain.Science => 3,
        LearningDomain.History => 4,
        LearningDomain.Geography => 5,
        LearningDomain.ExecutiveFunction => 6,
        _ => 7
    };
}
