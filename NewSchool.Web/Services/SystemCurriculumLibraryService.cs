using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class SystemCurriculumLibraryService(
    ApplicationDbContext db,
    IMemoryCache memoryCache,
    ProprietaryCurriculumBlueprintService proprietaryCurriculumBlueprintService)
{
    private static readonly TimeSpan TrackCacheDuration = TimeSpan.FromMinutes(10);

    public async Task<List<SystemCurriculumTrackViewModel>> BuildAsync(ChildProfile? child)
    {
        var age = child is null
            ? 6
            : Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14);
        var goalTrack = child?.FamilyGoalTrack ?? "balanced_growth";
        var currentPhaseIndex = GetCurrentPhaseIndex(DateTime.Today.Month);
        var currentUnitNumber = GetPhaseMonthSlot(DateTime.Today.Month);
        var cacheKey = $"system-curriculum:{age}:{goalTrack}:{currentPhaseIndex}:{currentUnitNumber}";

        if (memoryCache.TryGetValue(cacheKey, out List<SystemCurriculumTrackViewModel>? cachedTracks) && cachedTracks is not null)
        {
            return cachedTracks;
        }

        var templates = await db.CurriculumTemplates
            .AsNoTracking()
            .Where(x => x.Age == age)
            .Where(x => x.SupportScope == CurriculumSupportScope.General)
            .Where(x => x.FunctionalTrack == FunctionalSupportTrack.Base)
            .OrderBy(x => x.Domain)
            .ThenBy(x => x.GoalTrack)
            .ThenBy(x => x.SortOrder)
            .ToListAsync();

        var tracks = new List<SystemCurriculumTrackViewModel>();
        foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
        {
            var chosenTemplates = SelectTrackTemplates(templates, domain, goalTrack);
            var subjectBlueprint = proprietaryCurriculumBlueprintService.BuildSubject(age, domain);
            var currentPhaseBlueprint = subjectBlueprint.Phases.ElementAtOrDefault(currentPhaseIndex);
            var currentUnitBlueprint = currentPhaseBlueprint?.Units.FirstOrDefault();

            if (chosenTemplates.Count == 0 && currentUnitBlueprint is null)
            {
                continue;
            }

            var phaseSequences = BuildPhaseSequences(subjectBlueprint, chosenTemplates, currentPhaseIndex, currentUnitNumber);
            var currentUnit = phaseSequences
                .FirstOrDefault(sequence => sequence.IsCurrent)?
                .Units
                .FirstOrDefault(unit => unit.IsCurrent)
                ?? phaseSequences.FirstOrDefault(sequence => sequence.IsCurrent)?.Units.FirstOrDefault();

            tracks.Add(new SystemCurriculumTrackViewModel
            {
                Title = subjectBlueprint.TrackTitle,
                DomainLabel = subjectBlueprint.SubjectLabel,
                DomainChipClass = CurriculumStructure.GetDomainChipClass(domain),
                AgeBandLabel = subjectBlueprint.SchoolPlacementLabel,
                SchoolPlacementLabel = subjectBlueprint.SchoolPlacementLabel,
                CurrentPhaseLabel = $"{currentPhaseIndex + 1}ª etapa do ano",
                YearGoal = subjectBlueprint.YearGoal,
                CurrentFocus = currentUnit is null
                    ? BuildCurrentFocus(chosenTemplates, domain, age)
                    : $"{currentUnit.UnitLabel} • {currentUnit.Title}",
                ParentMethod = currentUnit?.ParentGuide ?? currentUnitBlueprint?.ParentGuide ?? subjectBlueprint.ParentMethod,
                TotalLessons = chosenTemplates.Count,
                ExampleSkills = currentUnit?.LessonTitles.Take(3).ToList()
                    ?? chosenTemplates.Select(x => x.Title).Distinct().Take(3).ToList(),
                Materials = currentUnit?.Materials.Count > 0
                    ? currentUnit.Materials.Take(4).ToList()
                    : chosenTemplates
                        .SelectMany(x => SplitMaterials(x.Materials))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(4)
                        .ToList(),
                CurrentUnit = currentUnit,
                PhaseSequences = phaseSequences
            });
        }

        memoryCache.Set(cacheKey, tracks, TrackCacheDuration);
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
        ProprietaryCurriculumSubjectBlueprintViewModel subjectBlueprint,
        IReadOnlyList<CurriculumTemplate> templates,
        int currentPhaseIndex,
        int currentUnitNumber)
    {
        if (subjectBlueprint.Phases.Count == 0)
        {
            return [];
        }

        var templateChunks = ChunkTemplatesByPhase(templates, subjectBlueprint.Phases.Count);
        var result = new List<SystemCurriculumPhaseSequenceViewModel>(subjectBlueprint.Phases.Count);
        foreach (var phase in subjectBlueprint.Phases)
        {
            var chunk = templateChunks.ElementAtOrDefault(phase.PhaseNumber - 1) ?? [];
            var units = phase.Units
                .Select(unit => new SystemCurriculumUnitViewModel
                {
                    PhaseLabel = unit.PhaseLabel,
                    SchoolPlacementLabel = unit.SchoolPlacementLabel,
                    SubjectLabel = unit.SubjectLabel,
                    UnitNumber = unit.UnitNumber,
                    UnitLabel = unit.UnitLabel,
                    Title = unit.Title,
                    Summary = unit.Summary,
                        Objective = unit.Objective,
                        ParentGuide = unit.ParentGuide,
                        TaskTitle = unit.LessonTitles.FirstOrDefault() ?? unit.Title,
                        TaskPrompt = unit.TaskPrompt,
                        CompletionSignal = unit.CompletionSignal,
                        OptionalEvidenceNote = unit.OptionalEvidenceNote,
                        IsCurrent = phase.PhaseNumber - 1 == currentPhaseIndex,
                    SkillCodes = chunk
                        .Where(template => template.Domain == subjectBlueprint.Domain)
                        .Select(template => template.SkillCode)
                        .Where(code => !string.IsNullOrWhiteSpace(code))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(6)
                        .ToList(),
                    LessonTitles = unit.LessonTitles.ToList(),
                    Materials = unit.Materials.ToList()
                })
                .ToList();

            if (units.Count > 0 && units.All(unit => !unit.IsCurrent) && phase.PhaseNumber - 1 == currentPhaseIndex)
            {
                units[0].IsCurrent = true;
            }

            result.Add(new SystemCurriculumPhaseSequenceViewModel
            {
                PhaseLabel = phase.PhaseLabel,
                Summary = phase.Summary,
                IsCurrent = phase.PhaseNumber - 1 == currentPhaseIndex,
                UnitCount = units.Count,
                LessonCount = units.Sum(unit => unit.LessonTitles.Count),
                Skills = units.SelectMany(unit => unit.LessonTitles).Distinct().Take(4).ToList(),
                Units = units
            });
        }

        return result;
    }

    private static IEnumerable<string> SplitMaterials(string value)
    {
        return value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 2);
    }

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
            LearningDomain.Science when age <= 5 => "natureza, corpo e observação",
            LearningDomain.Science => "pergunta, experimento e conclusão",
            LearningDomain.History when age <= 5 => "família, memória e sequência",
            LearningDomain.History => "tempo, fontes e acontecimentos",
            LearningDomain.Geography when age <= 5 => "casa, bairro e orientação",
            LearningDomain.Geography => "mapa, região e território",
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
            LearningDomain.Science when age <= 5 => "Explorar natureza, corpo, animais e fenômenos simples com curiosidade guiada.",
            LearningDomain.Science => "Construir investigação, observação e linguagem científica acessível.",
            LearningDomain.History when age <= 5 => "Perceber rotina, memória, tempo e mudanças da vida cotidiana.",
            LearningDomain.History => "Organizar fatos, fontes, sequência e interpretação histórica.",
            LearningDomain.Geography when age <= 5 => "Explorar casa, bairro, clima e referências do espaço próximo.",
            LearningDomain.Geography => "Ler mapas, regiões, território e relações entre lugar e sociedade.",
            LearningDomain.ExecutiveFunction when age <= 5 => "Criar rotina, atenção e autonomia básica.",
            LearningDomain.ExecutiveFunction => "Ganhar autonomia para iniciar, concluir e revisar o estudo.",
            _ => "Crescer com constância ao longo do ano."
        };
    }

    private static int GetPhaseMonthSlot(int month)
    {
        return Math.Clamp(((Math.Max(month, 1) - 1) % 3) + 1, 1, 3);
    }

    private static List<IReadOnlyList<CurriculumTemplate>> ChunkTemplatesByPhase(
        IReadOnlyList<CurriculumTemplate> templates,
        int phaseCount)
    {
        if (phaseCount <= 0)
        {
            return [];
        }

        if (templates.Count == 0)
        {
            return Enumerable.Range(0, phaseCount)
                .Select(_ => (IReadOnlyList<CurriculumTemplate>)[])
                .ToList();
        }

        var chunkSize = (int)Math.Ceiling(templates.Count / (double)phaseCount);
        var chunks = new List<IReadOnlyList<CurriculumTemplate>>(phaseCount);
        for (var index = 0; index < phaseCount; index++)
        {
            chunks.Add(templates
                .Skip(index * chunkSize)
                .Take(chunkSize)
                .ToList());
        }

        return chunks;
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
