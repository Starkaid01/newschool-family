using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class PhaseClosureService(ProprietaryCurriculumBlueprintService proprietaryCurriculumBlueprintService)
{
    public List<SchoolPhaseClosureViewModel> Build(ChildProfile child, DateTime referenceDate)
    {
        var age = Math.Clamp(CalculateAge(child.BirthDate, referenceDate), 3, 14);
        var schoolYear = referenceDate.Year;
        var currentPhaseNumber = GetCurrentPhaseNumber(referenceDate.Month);
        var closures = new List<SchoolPhaseClosureViewModel>();

        foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
        {
            var subject = proprietaryCurriculumBlueprintService.BuildSubject(age, domain);
            var subjectMastery = ResolveDomainMastery(child, domain);
            var totalLessonsInSubject = subject.Phases.Sum(phase => phase.Units.Sum(unit => unit.Lessons.Count));
            var estimatedCompletedAcrossSubject = totalLessonsInSubject == 0
                ? 0
                : (int)Math.Round(totalLessonsInSubject * (subjectMastery / 100d));

            var cumulativeLessonCursor = 0;
            foreach (var phase in subject.Phases.OrderBy(item => item.PhaseNumber))
            {
                var unit = phase.Units.FirstOrDefault();
                if (unit is null)
                {
                    continue;
                }

                var phaseLessons = phase.Units.Sum(item => item.Lessons.Count);
                var phaseCompletedLessons = Math.Clamp(estimatedCompletedAcrossSubject - cumulativeLessonCursor, 0, phaseLessons);
                cumulativeLessonCursor += phaseLessons;

                var sessionTouches = CountSessionTouches(child, domain, phase.PhaseNumber, schoolYear);
                var evidenceCount = CountEvidenceTouches(child, domain, phase.PhaseNumber, schoolYear);
                var progressPercent = phaseLessons == 0
                    ? 0
                    : (int)Math.Round(phaseCompletedLessons / (double)phaseLessons * 100d);
                var lessonsProgressLabel = $"{phaseCompletedLessons}/{phaseLessons} lições desta etapa";

                var (statusLabel, statusChipClass) = ResolveStatus(
                    phase.PhaseNumber,
                    currentPhaseNumber,
                    progressPercent,
                    sessionTouches,
                    evidenceCount);

                closures.Add(new SchoolPhaseClosureViewModel
                {
                    SubjectLabel = subject.SubjectLabel,
                    SubjectChipClass = CurriculumStructure.GetDomainChipClass(domain),
                    PhaseNumber = phase.PhaseNumber,
                    PhaseLabel = phase.PhaseLabel,
                    SchoolPlacementLabel = subject.SchoolPlacementLabel,
                    UnitTitle = $"{unit.UnitLabel} • {unit.Title}",
                    AssessmentTitle = unit.AssessmentTitle,
                    ReviewHeadline = BuildReviewHeadline(phase.PhaseNumber, currentPhaseNumber, statusLabel, subject.SubjectLabel),
                    ReviewSummary = BuildReviewSummary(unit, phase.PhaseNumber, currentPhaseNumber, progressPercent, sessionTouches),
                    ClosureSummary = BuildClosureSummary(unit, statusLabel, phaseCompletedLessons, phaseLessons),
                    AdvancementSignal = unit.CompletionSignal,
                    ParentAction = unit.ParentGuide,
                    EvidenceIdea = unit.OptionalEvidenceNote,
                    LessonsProgressLabel = lessonsProgressLabel,
                    StatusLabel = statusLabel,
                    StatusChipClass = statusChipClass,
                    ProgressPercent = progressPercent,
                    CompletedLessons = phaseCompletedLessons,
                    TotalLessons = phaseLessons,
                    SessionTouches = sessionTouches,
                    EvidenceCount = evidenceCount,
                    IsCurrentPhase = phase.PhaseNumber == currentPhaseNumber,
                    IsPastPhase = phase.PhaseNumber < currentPhaseNumber,
                    IsFuturePhase = phase.PhaseNumber > currentPhaseNumber,
                    ReviewChecklist = unit.Lessons
                        .OrderBy(item => item.LessonNumber)
                        .Select(item => item.Title)
                        .Take(4)
                        .ToList()
                });
            }
        }

        return closures
            .OrderBy(item => item.PhaseNumber)
            .ThenBy(item => CurriculumStructure.AnnualSubjectOrder
                .ToList()
                .FindIndex(domain => string.Equals(CurriculumStructure.FormatDomainLabel(domain), item.SubjectLabel, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static int ResolveDomainMastery(ChildProfile child, LearningDomain targetDomain)
    {
        var matches = child.SkillProgressEntries
            .Where(entry => CurriculumStructure.DomainMatches(entry.Domain, targetDomain))
            .Select(entry => entry.MasteryScore)
            .ToList();

        if (matches.Count == 0)
        {
            return targetDomain == LearningDomain.ExecutiveFunction ? 30 : 24;
        }

        return (int)Math.Round(matches.Average());
    }

    private static int CountSessionTouches(ChildProfile child, LearningDomain targetDomain, int phaseNumber, int schoolYear)
    {
        return child.LearningSessions
            .Where(session => session.LoggedAt.Year == schoolYear && GetCurrentPhaseNumber(session.LoggedAt.Month) == phaseNumber)
            .Count(session => SessionMatchesDomain(child, session, targetDomain));
    }

    private static int CountEvidenceTouches(ChildProfile child, LearningDomain targetDomain, int phaseNumber, int schoolYear)
    {
        return child.LearningSessions
            .Where(session =>
                session.LoggedAt.Year == schoolYear &&
                GetCurrentPhaseNumber(session.LoggedAt.Month) == phaseNumber &&
                !string.IsNullOrWhiteSpace(session.MediaUrl))
            .Count(session => SessionMatchesDomain(child, session, targetDomain));
    }

    private static bool SessionMatchesDomain(ChildProfile child, LearningSession session, LearningDomain targetDomain)
    {
        return session.BlockFeedbacks.Any(feedback =>
        {
            var feedbackDomain = feedback.DailyPlanBlock?.Domain
                ?? child.SkillProgressEntries.FirstOrDefault(entry => string.Equals(entry.SkillCode, feedback.SkillCode, StringComparison.OrdinalIgnoreCase))?.Domain
                ?? LearningDomain.Language;

            return CurriculumStructure.DomainMatches(feedbackDomain, targetDomain);
        });
    }

    private static (string StatusLabel, string ChipClass) ResolveStatus(
        int phaseNumber,
        int currentPhaseNumber,
        int progressPercent,
        int sessionTouches,
        int evidenceCount)
    {
        if (phaseNumber > currentPhaseNumber)
        {
            return ("a iniciar", "neutral");
        }

        if (phaseNumber < currentPhaseNumber)
        {
            return progressPercent >= 70 || (sessionTouches >= 3 && evidenceCount >= 1)
                ? ("fechada", "success")
                : ("revisão pendente", "warning");
        }

        if (progressPercent >= 88 || (progressPercent >= 70 && evidenceCount >= 1))
        {
            return ("pronta para fechar", "success");
        }

        if (progressPercent >= 35 || sessionTouches > 0)
        {
            return ("em andamento", "warning");
        }

        return ("começo da etapa", "neutral");
    }

    private static string BuildReviewHeadline(int phaseNumber, int currentPhaseNumber, string statusLabel, string subjectLabel)
    {
        if (phaseNumber > currentPhaseNumber)
        {
            return $"Preparação futura de {subjectLabel.ToLowerInvariant()}";
        }

        return statusLabel switch
        {
            "fechada" => "Etapa já consolidada",
            "pronta para fechar" => "Fechamento imediato da etapa",
            "revisão pendente" => "Revisão obrigatória antes de seguir",
            "em andamento" => "O que ainda precisa aparecer",
            _ => "Primeiros sinais esperados"
        };
    }

    private static string BuildReviewSummary(
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        int phaseNumber,
        int currentPhaseNumber,
        int progressPercent,
        int sessionTouches)
    {
        if (phaseNumber > currentPhaseNumber)
        {
            return $"Quando esta etapa entrar, a família vai abrir {unit.UnitLabel.ToLowerInvariant()} com passos curtos, livro, apostila e uma prova final já definidos.";
        }

        if (progressPercent >= 88)
        {
            return $"A revisão agora é curta: retomar {unit.Lessons.FirstOrDefault()?.Title?.ToLowerInvariant() ?? "a primeira lição"} e seguir para {unit.AssessmentTitle.ToLowerInvariant()}.";
        }

        if (progressPercent >= 45 || sessionTouches > 0)
        {
            return $"Antes de fechar, retome {unit.Lessons.FirstOrDefault()?.Title?.ToLowerInvariant() ?? "a abertura da unidade"} e confirme se a criança sustenta a resposta sem ajuda excessiva.";
        }

        return $"A etapa ainda precisa começar de verdade. Abra {unit.Lessons.FirstOrDefault()?.Title?.ToLowerInvariant() ?? "a primeira lição"} e mantenha a mesma rotina por alguns dias antes de cobrar a prova.";
    }

    private static string BuildClosureSummary(
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        string statusLabel,
        int completedLessons,
        int totalLessons)
    {
        return statusLabel switch
        {
            "fechada" => $"A etapa já tem base suficiente para ser considerada concluída. Guarde a melhor evidência e use {unit.AssessmentTitle} só como registro final.",
            "pronta para fechar" => $"A criança já mostrou {completedLessons} de {totalLessons} lições desta etapa. Aplique {unit.AssessmentTitle.ToLowerInvariant()} e registre o fechamento.",
            "revisão pendente" => $"Ainda não vale avançar sem revisar. Feche a etapa somente depois de retomar as lacunas e reaplicar {unit.AssessmentTitle.ToLowerInvariant()}.",
            "em andamento" => $"A etapa ainda está em construção. O foco agora é completar a prática central antes de abrir a prova da etapa.",
            _ => $"Esta etapa ainda não começou com constância. O sistema já deixou a prova, a revisão e a entrega final ligadas à unidade."
        };
    }

    private static int GetCurrentPhaseNumber(int month) => month switch
    {
        <= 3 => 1,
        <= 6 => 2,
        <= 9 => 3,
        _ => 4
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
