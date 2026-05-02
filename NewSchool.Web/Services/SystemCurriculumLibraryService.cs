using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class SystemCurriculumLibraryService(ApplicationDbContext db)
{
    public async Task<List<SystemCurriculumTrackViewModel>> BuildAsync(ChildProfile? child)
    {
        var age = child is null
            ? 6
            : Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14);
        var goalTrack = child?.FamilyGoalTrack ?? "balanced_growth";
        var currentPhaseIndex = GetCurrentPhaseIndex(DateTime.Today.Month);
        var currentUnitNumber = GetPhaseMonthSlot(DateTime.Today.Month);

        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .Where(x => x.SupportScope == CurriculumSupportScope.General)
            .Where(x => x.FunctionalTrack == FunctionalSupportTrack.Base)
            .OrderBy(x => x.Domain)
            .ThenBy(x => x.GoalTrack)
            .ThenBy(x => x.SortOrder)
            .ToListAsync();

        var tracks = new List<SystemCurriculumTrackViewModel>();
        foreach (var domain in new[]
                 {
                     LearningDomain.Language,
                     LearningDomain.Math,
                     LearningDomain.World,
                     LearningDomain.ExecutiveFunction
                 })
        {
            var chosenTemplates = SelectTrackTemplates(templates, domain, goalTrack);
            var phaseTemplates = GetPhaseTemplates(chosenTemplates, currentPhaseIndex);
            var leadTemplate = phaseTemplates.FirstOrDefault() ?? chosenTemplates.FirstOrDefault();
            if (leadTemplate is null)
            {
                continue;
            }

            var phaseSequences = BuildPhaseSequences(chosenTemplates, currentPhaseIndex, currentUnitNumber);
            var currentUnit = phaseSequences
                .FirstOrDefault(sequence => sequence.IsCurrent)?
                .Units.FirstOrDefault(unit => unit.IsCurrent)
                ?? phaseSequences.FirstOrDefault(sequence => sequence.IsCurrent)?.Units.FirstOrDefault();

            tracks.Add(new SystemCurriculumTrackViewModel
            {
                Title = BuildTrackTitle(domain),
                DomainLabel = FormatDomain(domain),
                DomainChipClass = GetChip(domain),
                AgeBandLabel = age <= 5
                    ? "educação infantil"
                    : age <= 8
                        ? "fundamental inicial"
                        : age <= 10
                            ? "fundamental em crescimento"
                            : "fundamental final",
                CurrentPhaseLabel = $"{currentPhaseIndex + 1}ª etapa do ano",
                YearGoal = BuildYearGoal(domain, age),
                CurrentFocus = currentUnit is null
                    ? BuildCurrentFocus(phaseTemplates, domain, age)
                    : $"{currentUnit.UnitLabel} • {currentUnit.Title}",
                ParentMethod = currentUnit?.ParentGuide ?? leadTemplate.ParentGuide,
                TotalLessons = chosenTemplates.Count,
                ExampleSkills = phaseTemplates.Select(x => x.Title).Distinct().Take(3).ToList(),
                Materials = currentUnit?.Materials.Count > 0
                    ? currentUnit.Materials.Take(4).ToList()
                    : phaseTemplates
                        .SelectMany(x => SplitMaterials(x.Materials))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(4)
                        .ToList(),
                CurrentUnit = currentUnit,
                PhaseSequences = phaseSequences
            });
        }

        return tracks;
    }

    private static List<CurriculumTemplate> SelectTrackTemplates(
        IReadOnlyCollection<CurriculumTemplate> templates,
        LearningDomain domain,
        string goalTrack)
    {
        var domainTemplates = templates
            .Where(x => x.Domain == domain)
            .ToList();
        var preferred = domainTemplates
            .Where(x => string.Equals(x.GoalTrack, goalTrack, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return preferred.Count > 0
            ? preferred
            : domainTemplates.Where(x => string.Equals(x.GoalTrack, "balanced_growth", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static List<CurriculumTemplate> GetPhaseTemplates(
        IReadOnlyList<CurriculumTemplate> templates,
        int phaseIndex)
    {
        if (templates.Count == 0)
        {
            return [];
        }

        var chunkSize = (int)Math.Ceiling(templates.Count / 4d);
        return templates
            .Skip(phaseIndex * chunkSize)
            .Take(chunkSize)
            .ToList();
    }

    private static List<SystemCurriculumPhaseSequenceViewModel> BuildPhaseSequences(
        IReadOnlyList<CurriculumTemplate> templates,
        int currentPhaseIndex,
        int currentUnitNumber)
    {
        if (templates.Count == 0)
        {
            return [];
        }

        var chunkSize = (int)Math.Ceiling(templates.Count / 4d);
        var result = new List<SystemCurriculumPhaseSequenceViewModel>();
        for (var index = 0; index < 4; index++)
        {
            var chunk = templates
                .Skip(index * chunkSize)
                .Take(chunkSize)
                .ToList();
            if (chunk.Count == 0)
            {
                continue;
            }

            var units = BuildUnits(chunk, index, currentPhaseIndex, currentUnitNumber);

            result.Add(new SystemCurriculumPhaseSequenceViewModel
            {
                PhaseLabel = $"{index + 1}ª etapa",
                Summary = BuildPhaseSummary(index, chunk),
                IsCurrent = index == currentPhaseIndex,
                UnitCount = units.Count,
                LessonCount = chunk.Count,
                Skills = chunk.Select(x => x.Title).Distinct().Take(4).ToList(),
                Units = units
            });
        }

        return result;
    }

    private static List<SystemCurriculumUnitViewModel> BuildUnits(
        IReadOnlyList<CurriculumTemplate> phaseTemplates,
        int phaseIndex,
        int currentPhaseIndex,
        int currentUnitNumber)
    {
        if (phaseTemplates.Count == 0)
        {
            return [];
        }

        var chunkSize = (int)Math.Ceiling(phaseTemplates.Count / 3d);
        var units = new List<SystemCurriculumUnitViewModel>();
        for (var index = 0; index < 3; index++)
        {
            var unitTemplates = phaseTemplates
                .Skip(index * chunkSize)
                .Take(chunkSize)
                .ToList();
            if (unitTemplates.Count == 0)
            {
                continue;
            }

            var leadTemplate = unitTemplates.First();
            var lessonTitles = unitTemplates
                .Select(template => template.Title)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();
            var materials = unitTemplates
                .SelectMany(template => SplitMaterials(template.Materials))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
            var unitLabel = $"Unidade {units.Count + 1}";

            units.Add(new SystemCurriculumUnitViewModel
            {
                PhaseLabel = $"{phaseIndex + 1}ª etapa",
                UnitNumber = units.Count + 1,
                UnitLabel = unitLabel,
                Title = leadTemplate.Title,
                Summary = BuildUnitSummary(unitTemplates),
                ParentGuide = leadTemplate.ParentGuide,
                TaskTitle = leadTemplate.Title,
                TaskPrompt = leadTemplate.ChildMission,
                CompletionSignal = BuildUnitCompletionSignal(leadTemplate),
                OptionalEvidenceNote = string.IsNullOrWhiteSpace(leadTemplate.EvidencePrompt)
                    ? "Se a família quiser guardar memória depois, pode salvar foto, vídeo curto ou observação na página de Evidências."
                    : leadTemplate.EvidencePrompt,
                IsCurrent = phaseIndex == currentPhaseIndex && units.Count + 1 == Math.Clamp(currentUnitNumber, 1, 3),
                SkillCodes = unitTemplates
                    .Select(template => template.SkillCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                LessonTitles = lessonTitles,
                Materials = materials
            });
        }

        if (units.Count > 0 && units.All(unit => !unit.IsCurrent) && phaseIndex == currentPhaseIndex)
        {
            units[0].IsCurrent = true;
        }

        return units;
    }

    private static IEnumerable<string> SplitMaterials(string value)
    {
        return value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 2);
    }

    private static string BuildUnitSummary(IReadOnlyList<CurriculumTemplate> templates)
    {
        var lessonTitles = templates
            .Select(template => template.Title)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();

        if (lessonTitles.Count == 0)
        {
            return "Sequência curta da etapa organizada para a família só abrir e conduzir.";
        }

        if (lessonTitles.Count == 1)
        {
            return $"Nesta unidade a criança trabalha {lessonTitles[0].ToLowerInvariant()} com passos curtos e repetição guiada.";
        }

        return $"Nesta unidade entram {lessonTitles[0].ToLowerInvariant()} e {lessonTitles[1].ToLowerInvariant()}, sem pular etapa nem misturar matéria demais.";
    }

    private static string BuildUnitCompletionSignal(CurriculumTemplate leadTemplate)
    {
        return leadTemplate.Domain switch
        {
            LearningDomain.Language => $"A unidade avança quando a criança consegue {leadTemplate.Title.ToLowerInvariant()} em resposta oral, leitura guiada ou escrita curta com menos ajuda.",
            LearningDomain.Math => $"A unidade avança quando a criança consegue {leadTemplate.Title.ToLowerInvariant()} usando material concreto ou registro curto sem travar no meio.",
            LearningDomain.World => $"A unidade avança quando a criança consegue {leadTemplate.Title.ToLowerInvariant()} e explicar a principal descoberta da atividade.",
            LearningDomain.ExecutiveFunction => $"A unidade avança quando a criança consegue {leadTemplate.Title.ToLowerInvariant()} até o fim da rotina combinada, com mais autonomia.",
            _ => $"A unidade avança quando a criança conclui {leadTemplate.Title.ToLowerInvariant()} com clareza."
        };
    }

    private static string BuildTrackTitle(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem em casa",
        LearningDomain.Math => "Matemática no cotidiano",
        LearningDomain.World => "Brasil, mundo e descoberta",
        LearningDomain.ExecutiveFunction => "Autonomia para estudar",
        _ => "Trilha do sistema"
    };

    private static string BuildCurrentFocus(
        IReadOnlyList<CurriculumTemplate> templates,
        LearningDomain domain,
        int age)
    {
        var examples = templates.Select(x => x.Title).Distinct().Take(2).ToList();
        if (examples.Count > 0)
        {
            return string.Join(" • ", examples);
        }

        return domain switch
        {
            LearningDomain.Language when age <= 5 => "sons, letras e oralidade guiada",
            LearningDomain.Language => "leitura, escrita e reconto com clareza",
            LearningDomain.Math when age <= 5 => "contagem, comparação e material concreto",
            LearningDomain.Math => "cálculo com sentido e problemas simples",
            LearningDomain.World when age <= 5 => "natureza, Brasil e observação",
            LearningDomain.World => "história, geografia e investigação",
            LearningDomain.ExecutiveFunction when age <= 5 => "rotina, foco e pequenas responsabilidades",
            LearningDomain.ExecutiveFunction => "planejamento, conclusão e revisão",
            _ => "avanço equilibrado"
        };
    }

    private static string BuildYearGoal(LearningDomain domain, int age)
    {
        return domain switch
        {
            LearningDomain.Language when age <= 5 => "Construir base de fala, escuta, letras e início da leitura.",
            LearningDomain.Language => "Fortalecer leitura, escrita guiada e compreensão.",
            LearningDomain.Math when age <= 5 => "Construir número, quantidade e comparação com material real.",
            LearningDomain.Math => "Consolidar cálculo, raciocínio e resolução de problemas.",
            LearningDomain.World when age <= 5 => "Ampliar repertório de Brasil, natureza e vida cotidiana.",
            LearningDomain.World => "Conectar história, ciência, geografia e cultura de forma viva.",
            LearningDomain.ExecutiveFunction when age <= 5 => "Criar rotina, atenção e autonomia básica.",
            LearningDomain.ExecutiveFunction => "Ganhar autonomia para iniciar, concluir e revisar o estudo.",
            _ => "Crescer com constância ao longo do ano."
        };
    }

    private static string BuildPhaseSummary(int phaseIndex, IReadOnlyList<CurriculumTemplate> templates)
    {
        var lead = templates.FirstOrDefault();
        if (lead is null)
        {
            return string.Empty;
        }

        var prefix = phaseIndex switch
        {
            0 => "Começo do ano",
            1 => "Ampliação",
            2 => "Consolidação",
            _ => "Fechamento"
        };

        return $"{prefix}: {lead.Goal}";
    }

    private static string FormatDomain(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem",
        LearningDomain.Math => "Matemática",
        LearningDomain.World => "Brasil e mundo",
        LearningDomain.ExecutiveFunction => "Autonomia",
        _ => "Geral"
    };

    private static string GetChip(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "track-communication",
        LearningDomain.Math => "track-academic",
        LearningDomain.World => "success",
        LearningDomain.ExecutiveFunction => "track-dailyliving",
        _ => "neutral"
    };

    private static int GetPhaseMonthSlot(int month)
    {
        return Math.Clamp(((Math.Max(month, 1) - 1) % 3) + 1, 1, 3);
    }

    private static int GetCurrentPhaseIndex(int month) => month switch
    {
        <= 3 => 0,
        <= 6 => 1,
        <= 9 => 2,
        _ => 3
    };

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
