using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class WeeklyRoadmapService(
    LearningPlanService learningPlanService,
    CuratedLearningLibraryService curatedLearningLibraryService,
    DailyTrailComposerService dailyTrailComposerService,
    ExternalContentHubService externalContentHubService,
    SystemCurriculumLibraryService systemCurriculumLibraryService)
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public async Task<WeeklyRoadmapViewModel> BuildAsync(
        ChildProfile child,
        DateTime referenceDate,
        IUrlHelper url)
    {
        var studyDates = BuildStudyDates(referenceDate.Date, 5);
        var days = new List<WeeklyRoadmapDayViewModel>();
        var systemTracks = await systemCurriculumLibraryService.BuildAsync(child);

        foreach (var studyDate in studyDates)
        {
            var plan = await learningPlanService.EnsurePlanAsync(child, studyDate);
            var orderedBlocks = plan.Blocks
                .OrderBy(x => x.SortOrder)
                .ToList();
            var suggestions = await curatedLearningLibraryService.BuildBlockSuggestionsAsync(child, orderedBlocks);
            var externalRecommendations = await externalContentHubService.BuildChildRecommendationsAsync(child, orderedBlocks, url);
            var trail = dailyTrailComposerService.Build(child, orderedBlocks, suggestions, externalRecommendations);
            var bestExternal = externalRecommendations
                .OrderBy(x => x.Completed)
                .ThenBy(x => x.Title)
                .FirstOrDefault();

            days.Add(new WeeklyRoadmapDayViewModel
            {
                PlannedDate = studyDate,
                RelativeLabel = BuildRelativeLabel(studyDate, referenceDate.Date),
                DateLabel = BuildDateLabel(studyDate),
                Theme = plan.Theme,
                Summary = trail.Summary,
                FocusAreaLabel = trail.FocusAreaLabel,
                FocusAreaChipClass = trail.FocusAreaChipClass,
                TotalMinutes = trail.TotalMinutes == 0 ? orderedBlocks.Sum(x => x.DurationMinutes) : trail.TotalMinutes,
                StageCount = trail.BlockCount == 0 ? orderedBlocks.Count : trail.BlockCount,
                TopTasks = BuildTopTasks(orderedBlocks, suggestions),
                Materials = trail.MaterialChecklist.Take(4).ToList(),
                EvidenceItems = trail.EvidenceChecklist.Take(3).ToList(),
                ExternalGuideLabel = bestExternal is null ? string.Empty : "Abrir apoio externo filtrado",
                ExternalGuideUrl = bestExternal?.GuideUrl ?? string.Empty,
                ExternalGuideReason = bestExternal?.RecommendedReason ?? string.Empty
            });
        }

        var focusMix = days
            .Select(x => x.FocusAreaLabel)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new WeeklyRoadmapViewModel
        {
            Headline = BuildHeadline(child.FamilyGoalTrack),
            Summary = BuildSummary(days),
            FocusMixLabel = focusMix.Count == 0 ? "semana automatica" : string.Join(" • ", focusMix.Take(3)),
            StudyDaysCount = days.Count,
            TotalMinutes = days.Sum(x => x.TotalMinutes),
            EvidenceTargetCount = days.Sum(x => Math.Max(1, x.EvidenceItems.Count)),
            Days = days,
            SubjectRuns = BuildSubjectRuns(systemTracks, referenceDate.Date)
        };
    }

    private static List<WeeklyRoadmapSubjectRunViewModel> BuildSubjectRuns(
        IReadOnlyCollection<SystemCurriculumTrackViewModel> tracks,
        DateTime referenceDate)
    {
        var currentWeekStepNumber = Math.Clamp(GetWeekdayStepNumber(referenceDate.DayOfWeek), 1, 5);
        var currentMonthWeekNumber = Math.Clamp(GetWeekOfMonth(referenceDate.Day), 1, 4);

        return tracks
            .Where(track => track.CurrentUnit is not null)
            .Select(track => new WeeklyRoadmapSubjectRunViewModel
            {
                SubjectLabel = track.DomainLabel,
                SubjectChipClass = track.DomainChipClass,
                CurrentUnitLabel = track.CurrentUnit!.UnitLabel,
                CurrentUnitTitle = track.CurrentUnit.Title,
                WeekHeadline = BuildWeekHeadline(track.DomainLabel, track.CurrentUnit.Title),
                MonthHeadline = BuildMonthHeadline(track.DomainLabel, track.CurrentPhaseLabel),
                CompletionSignal = track.CurrentUnit.CompletionSignal,
                CurrentWeekStepNumber = currentWeekStepNumber,
                CurrentMonthWeekNumber = currentMonthWeekNumber,
                WeekSteps = BuildWeekSteps(track.DomainLabel, track.CurrentUnit.Title),
                MonthSteps = BuildMonthSteps(track.DomainLabel, track.CurrentUnit.Title)
            })
            .ToList();
    }

    private static List<DateTime> BuildStudyDates(DateTime referenceDate, int count)
    {
        var dates = new List<DateTime>();
        var cursor = referenceDate.Date;

        while (dates.Count < count)
        {
            if (cursor.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                dates.Add(cursor);
            }

            cursor = cursor.AddDays(1);
        }

        return dates;
    }

    private static string BuildRelativeLabel(DateTime plannedDate, DateTime referenceDate)
    {
        var offset = (plannedDate.Date - referenceDate.Date).Days;
        return offset switch
        {
            0 => "Hoje",
            1 => "Amanha",
            2 => "Depois",
            _ => PtBr.TextInfo.ToTitleCase(plannedDate.ToString("dddd", PtBr))
        };
    }

    private static string BuildDateLabel(DateTime plannedDate)
    {
        var weekday = PtBr.TextInfo.ToTitleCase(plannedDate.ToString("dddd", PtBr));
        return $"{weekday} • {plannedDate:dd/MM}";
    }

    private static List<string> BuildTopTasks(
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel> suggestions)
    {
        return blocks
            .OrderBy(x => x.SortOrder)
            .Select(block => suggestions.TryGetValue(block.Id, out var suggestion) ? suggestion.Title : block.Title)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();
    }

    private static string BuildHeadline(string familyGoalTrack)
    {
        return familyGoalTrack switch
        {
            "literacy" => "Semana automatica com foco em leitura, escrita e prova do que foi aprendido",
            "math_foundations" => "Semana automatica com foco em matematica concreta e rotina segura",
            "science_discovery" => "Semana automatica com curiosidade, ciencias, historia e geografia com registro simples",
            "autonomy" => "Semana automatica com autonomia, constancia e tarefas mastigadas",
            _ => "Semana automatica organizada para a familia nao improvisar"
        };
    }

    private static string BuildSummary(IReadOnlyCollection<WeeklyRoadmapDayViewModel> days)
    {
        if (days.Count == 0)
        {
            return "Assim que houver dias gerados, o sistema distribui aula, tarefa, prova e apoio externo em uma semana simples de seguir.";
        }

        var totalMinutes = days.Sum(x => x.TotalMinutes);
        var evidenceTargets = days.Sum(x => Math.Max(1, x.EvidenceItems.Count));
        var focusAreas = days
            .Select(x => x.FocusAreaLabel)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        var focusCopy = focusAreas.Count == 0
            ? "com metas distribuidas ao longo da semana"
            : $"misturando {string.Join(", ", focusAreas).ToLowerInvariant()}";

        return $"O sistema ja preparou {days.Count} dias de estudo, somando {totalMinutes} minutos e pelo menos {evidenceTargets} evidencias uteis, {focusCopy}.";
    }

    private static string BuildWeekHeadline(string domainLabel, string currentUnitTitle)
    {
        return $"{domainLabel} nesta semana • {currentUnitTitle}";
    }

    private static string BuildMonthHeadline(string domainLabel, string currentPhaseLabel)
    {
        return $"{domainLabel} neste mes • {currentPhaseLabel}";
    }

    private static List<string> BuildWeekSteps(string domainLabel, string currentUnitTitle)
    {
        return domainLabel switch
        {
            "Linguagem" =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com leitura ou trecho-chave.",
                "Destacar a ideia mais importante e registrar palavras de apoio.",
                "Produzir resposta curta com base no texto, e nao em chute.",
                "Revisar clareza, prova textual e fechamento da resposta.",
                "Fechar a semana com sintese oral ou escrita do que ficou claro."
            ],
            "Matemática" =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com situacao real e dados organizados.",
                "Modelar a situacao em tabela, desenho ou representacao visual.",
                "Resolver com estrategia nomeada e acompanhada no caderno.",
                "Justificar o resultado em fala ou registro curto.",
                "Fechar a semana revisando metodo, erro comum e acerto principal."
            ],
            "Ciências" =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com pergunta central e observacao do fenomeno.",
                "Coletar dado, fonte ou observacao principal do tema.",
                "Comparar o que mudou, o que se repetiu e o que a experiencia mostrou.",
                "Registrar descoberta e explicar a relacao entre pergunta e conclusao.",
                "Fechar a semana com conclusao do experimento ou da investigacao."
            ],
            "História" =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com fato, personagem ou fonte inicial.",
                "Organizar a ordem do acontecimento e localizar o contexto principal.",
                "Comparar causa, consequência ou mudança no tempo.",
                "Registrar a ideia central da narrativa histórica com uma prova.",
                "Fechar a semana com síntese do fato estudado e do que ele ajuda a entender."
            ],
            "Geografia" =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com mapa, lugar ou territorio em destaque.",
                "Ler referencia, regiao, paisagem ou circulacao ligada ao tema.",
                "Comparar espaços, usos do territorio ou impacto no cotidiano.",
                "Registrar o que o mapa ou o lugar ajuda a explicar.",
                "Fechar a semana com conclusão sobre territorio, pessoas e ambiente."
            ],
            _ =>
            [
                $"Abrir a unidade {currentUnitTitle.ToLowerInvariant()} com meta clara e material pronto.",
                "Executar um bloco curto com foco unico e sem trocar de missao.",
                "Fazer checkpoint no meio para revisar rumo ou prioridade.",
                "Concluir com checklist e prova do que foi feito.",
                "Fechar a semana com reflexao e proximo passo definido."
            ]
        };
    }

    private static List<string> BuildMonthSteps(string domainLabel, string currentUnitTitle)
    {
        return domainLabel switch
        {
            "Linguagem" =>
            [
                $"Semana 1: entrar em {currentUnitTitle.ToLowerInvariant()} com leitura e repertorio.",
                "Semana 2: praticar resposta escrita ou oral com apoio guiado.",
                "Semana 3: consolidar clareza, prova textual e organizacao.",
                "Semana 4: fechar a unidade com sintese mais autoral."
            ],
            "Matemática" =>
            [
                $"Semana 1: entrar em {currentUnitTitle.ToLowerInvariant()} com concreto e organizacao dos dados.",
                "Semana 2: praticar estrategia e representacao visual.",
                "Semana 3: consolidar calculo e justificativa do metodo.",
                "Semana 4: fechar a unidade com problema de aplicacao e revisao."
            ],
            "Ciências" =>
            [
                $"Semana 1: abrir {currentUnitTitle.ToLowerInvariant()} com pergunta, observacao e repertorio.",
                "Semana 2: ampliar o tema com dado, experimento ou comparacao.",
                "Semana 3: consolidar registro, conclusao e linguagem cientifica da serie.",
                "Semana 4: fechar a unidade com sintese, prova ou aplicacao do que foi observado."
            ],
            "História" =>
            [
                $"Semana 1: abrir {currentUnitTitle.ToLowerInvariant()} com fato, fonte e contexto inicial.",
                "Semana 2: ampliar o tema com ordem temporal, causa ou personagem.",
                "Semana 3: consolidar explicacao historica e prova do texto ou da fonte.",
                "Semana 4: fechar a unidade com conclusao, linha do tempo ou sintese."
            ],
            "Geografia" =>
            [
                $"Semana 1: abrir {currentUnitTitle.ToLowerInvariant()} com mapa, paisagem ou lugar de partida.",
                "Semana 2: ampliar o tema com regiao, circulacao, ambiente ou territorio.",
                "Semana 3: consolidar leitura espacial e relacao com a vida real.",
                "Semana 4: fechar a unidade com sintese sobre espaço, sociedade e ambiente."
            ],
            _ =>
            [
                $"Semana 1: abrir {currentUnitTitle.ToLowerInvariant()} com rotina e combinados claros.",
                "Semana 2: praticar constancia, foco e qualidade de entrega.",
                "Semana 3: consolidar revisao, checkpoint e ajuste de rota.",
                "Semana 4: fechar a unidade com mais autonomia e reflexao."
            ]
        };
    }

    private static int GetWeekdayStepNumber(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            _ => 1
        };
    }

    private static int GetWeekOfMonth(int day)
    {
        return day switch
        {
            <= 7 => 1,
            <= 14 => 2,
            <= 21 => 3,
            _ => 4
        };
    }
}
