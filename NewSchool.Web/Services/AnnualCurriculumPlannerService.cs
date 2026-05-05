using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class AnnualCurriculumPlannerService(ApplicationDbContext db)
{
    public async Task<AnnualCurriculumViewModel> BuildAsync(
        ChildProfile child,
        IReadOnlyCollection<SkillProgressViewModel> skillProgress,
        IReadOnlyCollection<CurriculumEntryViewModel> entries,
        int weeklyTotalMinutes)
    {
        var today = DateTime.Today;
        var age = CalculateAge(child.BirthDate, today);
        var filteredAge = Math.Clamp(age, 3, 14);
        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == filteredAge)
            .Where(x => x.SupportScope == CurriculumSupportScope.General)
            .Where(x => x.FunctionalTrack == FunctionalSupportTrack.Base)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        var annualTemplates = SelectAnnualTemplates(templates, child.FamilyGoalTrack);

        var phaseIndex = GetCurrentPhaseIndex(today.Month);
        var blueprints = BuildPhaseBlueprints(age, child.FamilyGoalTrack);
        var chunks = ChunkTemplates(annualTemplates, blueprints.Count);
        var phases = blueprints
            .Select((blueprint, index) => BuildPhase(blueprint, chunks[index], index == phaseIndex))
            .ToList();

        var subjects = BuildSubjects(age, annualTemplates, skillProgress, entries, phaseIndex, chunks);
        var currentPhase = phases.FirstOrDefault(x => x.IsCurrent) ?? phases.First();

        return new AnnualCurriculumViewModel
        {
            SchoolYearLabel = $"Ano letivo {today.Year}",
            Headline = BuildHeadline(age, child.FamilyGoalTrack),
            Summary = BuildSummary(age, child.FamilyGoalTrack, weeklyTotalMinutes),
            CurriculumBandLabel = CurriculumStructure.GetCurriculumBandLabel(age),
            TotalProprietaryLessons = annualTemplates.Count,
            CurrentPhaseLabel = currentPhase.PhaseLabel,
            CurrentPhaseTitle = currentPhase.Title,
            CurrentPhaseSummary = currentPhase.Summary,
            FamilyRoutineNote = BuildRoutineNote(weeklyTotalMinutes),
            Phases = phases,
            Subjects = subjects
        };
    }

    private static AnnualCurriculumPhaseViewModel BuildPhase(
        AnnualPhaseBlueprint blueprint,
        IReadOnlyList<CurriculumTemplate> templates,
        bool isCurrent)
    {
        var materials = templates
            .SelectMany(template => SplitMaterials(template.Materials))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToList();

        return new AnnualCurriculumPhaseViewModel
        {
            PhaseNumber = blueprint.Index + 1,
            PhaseLabel = $"{blueprint.Index + 1}ª etapa",
            Title = blueprint.Title,
            MonthsLabel = blueprint.MonthsLabel,
            Summary = blueprint.Summary,
            ParentAction = blueprint.ParentAction,
            ChildGain = blueprint.ChildGain,
            CompletionSignal = blueprint.CompletionSignal,
            IsCurrent = isCurrent,
            PlannedTasksCount = templates.Count,
            UnitCount = Math.Max(1, (int)Math.Ceiling(Math.Max(templates.Count, 1) / 3d)),
            LessonCountLabel = templates.Count == 1 ? "1 lição" : $"{templates.Count} lições",
            FeaturedTasks = templates.Select(x => x.Title).Distinct().Take(3).ToList(),
            MaterialsToKeepReady = materials.Count > 0
                ? materials
                : ["papel", "lápis", "material simples da casa"]
        };
    }

    private static List<AnnualCurriculumSubjectViewModel> BuildSubjects(
        int age,
        IReadOnlyList<CurriculumTemplate> templates,
        IReadOnlyCollection<SkillProgressViewModel> skillProgress,
        IReadOnlyCollection<CurriculumEntryViewModel> entries,
        int currentPhaseIndex,
        IReadOnlyList<IReadOnlyList<CurriculumTemplate>> chunks)
    {
        var subjects = new List<AnnualCurriculumSubjectViewModel>();
        foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
        {
            var domainLabel = CurriculumStructure.FormatDomainLabel(domain);
            var progressItems = skillProgress
                .Where(x => CurriculumStructure.DomainMatches(ParseDomain(x.DomainLabel), domain))
                .ToList();
            var progressPercent = progressItems.Count > 0
                ? (int)Math.Round(progressItems.Average(x => x.ProgressPercent))
                : EstimateProgress(entries, domainLabel);
            var currentPhaseTasks = chunks[currentPhaseIndex]
                .Where(x => x.Domain == domain)
                .Select(x => x.Title)
                .Distinct()
                .Take(2)
                .ToList();
            var milestoneTasks = templates
                .Where(x => x.Domain == domain)
                .Select(x => x.Title)
                .Distinct()
                .Take(3)
                .ToList();

            subjects.Add(new AnnualCurriculumSubjectViewModel
            {
                Title = domainLabel,
                CurrentGoal = currentPhaseTasks.Count > 0
                    ? string.Join(" • ", currentPhaseTasks)
                    : BuildDefaultCurrentGoal(domain, age),
                YearGoal = BuildYearGoal(domain, age),
                ProgressPercent = progressPercent,
                ProgressLabel = GetProgressLabel(progressPercent),
                ProgressChipClass = GetProgressChip(progressPercent),
                Milestones = milestoneTasks.Count > 0
                    ? milestoneTasks
                    : [BuildDefaultCurrentGoal(domain, age)]
            });
        }

        return subjects;
    }

    private static int EstimateProgress(
        IReadOnlyCollection<CurriculumEntryViewModel> entries,
        string domainLabel)
    {
        var matchingEntries = entries
            .Count(x => x.Skills.Any(skill => string.Equals(skill.DomainLabel, domainLabel, StringComparison.OrdinalIgnoreCase)));

        return matchingEntries switch
        {
            >= 16 => 80,
            >= 10 => 62,
            >= 5 => 40,
            >= 1 => 20,
            _ => 8
        };
    }

    private static string BuildHeadline(int age, string goalTrack)
    {
        return $"{CurriculumStructure.GetSchoolPlacementLabel(age).ToLowerInvariant()} organizado por etapas, matérias e rotina real de casa";
    }

    private static string BuildSummary(int age, string goalTrack, int weeklyTotalMinutes)
    {
        var emphasis = goalTrack switch
        {
            "literacy" => "alfabetização, leitura e produção oral",
            "math_foundations" => "número, cálculo mental e raciocínio",
            "autonomy" => "autonomia, rotina e autorregulação",
            "science_discovery" => "ciências, história, geografia e investigação guiada",
            _ => "linguagem, matemática, ciências, história, geografia e autonomia"
        };

        return $"O ano foi dividido em quatro etapas para a família saber o que ensinar agora, o que vem depois e como avançar sem improviso. Nesta fase o foco maior está em {emphasis}, mantendo uma semana leve de {weeklyTotalMinutes} minutos no total.";
    }

    private static string BuildRoutineNote(int weeklyTotalMinutes)
    {
        return weeklyTotalMinutes <= 0
            ? "Abra a aula do dia e siga uma lição por vez. O sistema organiza o resto."
            : $"Sua semana já nasce pronta em blocos curtos. Hoje basta abrir a aula, seguir as etapas e marcar como concluído.";
    }

    private static List<AnnualPhaseBlueprint> BuildPhaseBlueprints(int age, string goalTrack)
    {
        var olderChild = age >= 7;
        var trackEmphasis = goalTrack switch
        {
            "literacy" => "com reforço maior em linguagem e leitura diária",
            "math_foundations" => "com reforço maior em matemática concreta e prática",
            "autonomy" => "com reforço maior em autonomia, atenção e rotina",
            "science_discovery" => "com reforço maior em ciências, história, geografia e investigação",
            _ => "com equilíbrio entre matérias e rotina"
        };

        return olderChild
            ? BuildOlderChildPhases(trackEmphasis)
            : BuildYoungerChildPhases(trackEmphasis);
    }

    private static List<AnnualPhaseBlueprint> BuildYoungerChildPhases(string emphasis)
    {
        return
        [
            new AnnualPhaseBlueprint(0, "Base e rotina", "jan • fev • mar",
                $"Entrar no ritmo da casa {emphasis}.",
                "Abrir uma lição por vez e repetir o que funcionou bem.",
                "Seguir instrução simples, participar com segurança e terminar blocos curtos.",
                "A criança reconhece o começo e o fim da aula e já espera o próximo passo."),
            new AnnualPhaseBlueprint(1, "Letras, números e repertório", "abr • mai • jun",
                "Expandir linguagem, contagem e observação do mundo com mais constância.",
                "Alternar oralidade, folha simples e atividade concreta na mesma semana.",
                "Nomear, comparar, contar e explicar o que percebe.",
                "A criança já responde melhor sem depender de ajuda em todos os momentos."),
            new AnnualPhaseBlueprint(2, "Consolidação e conexão", "jul • ago • set",
                "Revisar o que entrou, juntar matérias e aumentar a clareza do raciocínio.",
                "Retomar tarefas-chave e guardar registros melhores das atividades.",
                "Relacionar ideias, lembrar sequências e sustentar pequenas explicações.",
                "Os registros mostram mais constância e menos tentativa solta."),
            new AnnualPhaseBlueprint(3, "Autonomia e fechamento", "out • nov • dez",
                "Fechar o ano com revisão, autonomia e pequenas produções próprias.",
                "Diminuir ajuda excessiva e valorizar o que a criança já consegue fazer sozinha.",
                "Concluir tarefas com menos condução e contar o que aprendeu.",
                "A família enxerga um antes e depois claro no currículo e nas evidências.")
        ];
    }

    private static List<AnnualPhaseBlueprint> BuildOlderChildPhases(string emphasis)
    {
        return
        [
            new AnnualPhaseBlueprint(0, "Fundamentos do ano", "jan • fev • mar",
                $"Ajustar rotina, diagnóstico leve e fundamentos {emphasis}.",
                "Começar com metas pequenas, clareza de matéria e ordem de estudo.",
                "Entrar no hábito de abrir, concluir e revisar o que foi estudado.",
                "A criança passa a estudar com menos resistência e mais previsibilidade."),
            new AnnualPhaseBlueprint(1, "Ampliação por matéria", "abr • mai • jun",
                "Aprofundar linguagem, matemática, ciências, história e geografia com mais intenção.",
                "Variar leitura, exercícios concretos e registro curto sem sobrecarregar.",
                "Ler melhor, resolver mais passos e sustentar explicações curtas.",
                "Já existe avanço visível nas matérias centrais do ano."),
            new AnnualPhaseBlueprint(2, "Consolidação e projeto", "jul • ago • set",
                "Cruzar matérias, revisar pontos fracos e produzir com mais sentido.",
                "Usar projetos curtos, comparação de ideias e revisão do que ficou frágil.",
                "Explicar raciocínio, revisar erro e finalizar tarefas com mais autonomia.",
                "O currículo deixa de ser sequência solta e vira construção contínua."),
            new AnnualPhaseBlueprint(3, "Autonomia e síntese", "out • nov • dez",
                "Fechar o ano com síntese, revisão forte e autonomia crescente.",
                "Guiar menos e registrar melhor o que já virou conquista real.",
                "Executar partes da rotina quase sem ajuda e apresentar o que aprendeu.",
                "A jornada anual fica clara para a família e pronta para documentação.")
        ];
    }

    private static List<CurriculumTemplate> SelectAnnualTemplates(
        IReadOnlyList<CurriculumTemplate> templates,
        string familyGoalTrack)
    {
        var result = new List<CurriculumTemplate>();
        foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
        {
            var domainTemplates = templates
                .Where(x => x.Domain == domain)
                .OrderBy(x => x.SortOrder)
                .ToList();
            var preferred = domainTemplates
                .Where(x => string.Equals(x.GoalTrack, familyGoalTrack, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (preferred.Count == 0)
            {
                preferred = domainTemplates
                    .Where(x => string.Equals(x.GoalTrack, "balanced_growth", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            result.AddRange(preferred.Count > 0 ? preferred : domainTemplates);
        }

        return result;
    }

    private static IReadOnlyList<IReadOnlyList<CurriculumTemplate>> ChunkTemplates(
        IReadOnlyList<CurriculumTemplate> templates,
        int chunkCount)
    {
        if (chunkCount <= 0)
        {
            return [];
        }

        var result = new List<IReadOnlyList<CurriculumTemplate>>(chunkCount);
        var chunkSize = templates.Count == 0
            ? 0
            : (int)Math.Ceiling(templates.Count / (double)chunkCount);

        for (var index = 0; index < chunkCount; index++)
        {
            if (chunkSize == 0)
            {
                result.Add([]);
                continue;
            }

            result.Add(templates
                .Skip(index * chunkSize)
                .Take(chunkSize)
                .ToList());
        }

        return result;
    }

    private static IEnumerable<string> SplitMaterials(string materials)
    {
        if (string.IsNullOrWhiteSpace(materials))
        {
            return [];
        }

        return materials
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 2)
            .Take(4);
    }

    private static string BuildYearGoal(LearningDomain domain, int age)
    {
        return domain switch
        {
            LearningDomain.Language when age <= 5 => "Ampliar fala, consciência fonológica, reconhecimento de letras e início da leitura.",
            LearningDomain.Language => "Fortalecer leitura, escrita guiada, compreensão e expressão oral com clareza.",
            LearningDomain.Math when age <= 5 => "Construir noção de quantidade, comparação, contagem e padrões do cotidiano.",
            LearningDomain.Math => "Consolidar cálculo, resolução de problemas e uso prático da matemática em casa.",
            LearningDomain.Science when age <= 5 => "Explorar natureza, observação, corpo, animais e pequenas descobertas do cotidiano.",
            LearningDomain.Science => "Construir investigação, hipótese, experimento e conclusão com linguagem simples e rigor crescente.",
            LearningDomain.History when age <= 5 => "Perceber tempo, memória, família e mudanças do dia a dia com histórias e sequências.",
            LearningDomain.History => "Organizar linha do tempo, causa, consequência, fontes e leitura histórica em escala adequada à série.",
            LearningDomain.Geography when age <= 5 => "Explorar casa, bairro, clima, mapa simples e referências do mundo próximo.",
            LearningDomain.Geography => "Ler território, mapas, regiões, recursos e relações entre lugar, sociedade e ambiente.",
            LearningDomain.ExecutiveFunction when age <= 5 => "Criar rotina, atenção compartilhada, autonomia básica e perseverança.",
            LearningDomain.ExecutiveFunction => "Ganhar autonomia para iniciar, concluir, revisar e registrar o próprio estudo.",
            _ => "Avançar com constância durante o ano."
        };
    }

    private static string BuildDefaultCurrentGoal(LearningDomain domain, int age)
    {
        return domain switch
        {
            LearningDomain.Language when age <= 5 => "sons, letras e oralidade guiada",
            LearningDomain.Language => "leitura, compreensão e escrita curta",
            LearningDomain.Math when age <= 5 => "contagem concreta e comparação",
            LearningDomain.Math => "problemas simples e cálculo com material real",
            LearningDomain.Science when age <= 5 => "natureza, corpo e observação",
            LearningDomain.Science => "investigação, fenômenos e experiências",
            LearningDomain.History when age <= 5 => "sequência, memória e família",
            LearningDomain.History => "tempo, fontes e acontecimentos",
            LearningDomain.Geography when age <= 5 => "casa, bairro e orientação",
            LearningDomain.Geography => "mapa, território e regiões",
            LearningDomain.ExecutiveFunction when age <= 5 => "rotina, atenção e pequenas responsabilidades",
            LearningDomain.ExecutiveFunction => "organização, início de tarefa e revisão",
            _ => "avanço equilibrado"
        };
    }

    private static string GetProgressLabel(int progressPercent)
    {
        return progressPercent switch
        {
            >= 75 => "bem encaminhado",
            >= 50 => "avançando",
            >= 25 => "em construção",
            _ => "começando"
        };
    }

    private static string GetProgressChip(int progressPercent)
    {
        return progressPercent switch
        {
            >= 75 => "success",
            >= 50 => "track-academic",
            >= 25 => "warning",
            _ => "neutral"
        };
    }

    private static LearningDomain ParseDomain(string domainLabel) => domainLabel switch
    {
        "Linguagem" => LearningDomain.Language,
        "Matemática" => LearningDomain.Math,
        "Matematica" => LearningDomain.Math,
        "Ciências" => LearningDomain.Science,
        "Ciencias" => LearningDomain.Science,
        "História" => LearningDomain.History,
        "Historia" => LearningDomain.History,
        "Geografia" => LearningDomain.Geography,
        "Brasil e mundo" => LearningDomain.World,
        "Mundo real" => LearningDomain.World,
        _ => LearningDomain.ExecutiveFunction
    };

    private static int GetCurrentPhaseIndex(int month)
    {
        return month switch
        {
            <= 3 => 0,
            <= 6 => 1,
            <= 9 => 2,
            _ => 3
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

    private sealed record AnnualPhaseBlueprint(
        int Index,
        string Title,
        string MonthsLabel,
        string Summary,
        string ParentAction,
        string ChildGain,
        string CompletionSignal);
}
