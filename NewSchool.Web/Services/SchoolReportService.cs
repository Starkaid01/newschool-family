using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class SchoolReportService(
    ApplicationDbContext db,
    AnnualCurriculumPlannerService annualCurriculumPlannerService,
    SystemCurriculumLibraryService systemCurriculumLibraryService,
    ChildEvolutionService childEvolutionService,
    PhaseClosureService phaseClosureService,
    FundamentalAssessmentService fundamentalAssessmentService)
{
    public async Task<ChildSchoolReportViewModel> BuildAsync(Guid childId, Guid parentId, IUrlHelper url)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.Achievements)
            .Include(x => x.TaskCompletions)
            .ThenInclude(x => x.DailyPlanBlock)
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .ThenInclude(x => x.DailyPlanBlock)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var skillProgress = BuildSkillProgress(child);
        var entries = BuildEntries(child);
        var annualPlan = await annualCurriculumPlannerService.BuildAsync(
            child,
            skillProgress,
            entries,
            Math.Max(child.DailyStudyMinutes * 5, 0));
        var systemTracks = await systemCurriculumLibraryService.BuildAsync(child);
        var phaseClosures = phaseClosureService.Build(child, today);
        var monthlyReport = await childEvolutionService.BuildMonthlyReportAsync(childId, parentId);
        var currentPhaseClosures = phaseClosures.Where(item => item.IsCurrentPhase).ToList();
        var assessmentAverages = await fundamentalAssessmentService.BuildSubjectAveragesAsync(childId, parentId);

        var subjects = annualPlan.Subjects
            .Select(subject =>
            {
                var currentClosure = currentPhaseClosures.FirstOrDefault(item =>
                    string.Equals(item.SubjectLabel, subject.Title, StringComparison.OrdinalIgnoreCase));
                var systemTrack = systemTracks.FirstOrDefault(track =>
                    string.Equals(track.DomainLabel, subject.Title, StringComparison.OrdinalIgnoreCase));
                var assessmentAverage = assessmentAverages.FirstOrDefault(item =>
                    string.Equals(item.SubjectLabel, subject.Title, StringComparison.OrdinalIgnoreCase));

                return new SchoolReportSubjectViewModel
                {
                    SubjectLabel = subject.Title,
                    SubjectChipClass = systemTrack?.DomainChipClass ?? "neutral",
                    YearGoal = subject.YearGoal,
                    CurrentUnitTitle = systemTrack?.CurrentUnit is null
                        ? subject.CurrentGoal
                        : $"{systemTrack.CurrentUnit.UnitLabel} • {systemTrack.CurrentUnit.Title}",
                    AssessmentTitle = currentClosure?.AssessmentTitle ?? subject.PhaseAssessmentTitle,
                    AssessmentAverageLabel = assessmentAverage is null
                        ? "Sem notas lançadas"
                        : $"{assessmentAverage.AverageScoreLabel} de média",
                    AssessmentCount = assessmentAverage?.AssessmentCount ?? 0,
                    NeedsReinforcement = assessmentAverage?.NeedsReinforcement == true,
                    PhaseStatusLabel = currentClosure?.StatusLabel ?? subject.PhaseStatusLabel,
                    PhaseStatusChipClass = currentClosure?.StatusChipClass ?? subject.PhaseStatusChipClass,
                    ProgressPercent = subject.ProgressPercent,
                    ProgressLabel = subject.ProgressLabel,
                    LessonsProgressLabel = currentClosure?.LessonsProgressLabel ?? subject.LessonsProgressLabel,
                    ReviewSummary = currentClosure?.ReviewSummary ?? subject.PhaseReviewSummary,
                    ClosureSummary = currentClosure?.ClosureSummary ?? subject.PhaseClosureSummary,
                    AdvancementSignal = currentClosure?.AdvancementSignal ?? subject.AdvancementSignal
                };
            })
            .ToList();

        var sessionsThisYear = child.LearningSessions.Where(session => session.LoggedAt.Year == today.Year).ToList();
        var currentPhase = annualPlan.Phases.FirstOrDefault(phase => phase.IsCurrent) ?? annualPlan.Phases.FirstOrDefault();
        var overallProgress = subjects.Count == 0
            ? 0
            : (int)Math.Round(subjects.Average(item => item.ProgressPercent));

        return new ChildSchoolReportViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, today),
            SchoolPlacementLabel = CurriculumStructure.GetSchoolPlacementLabel(CalculateAge(child.BirthDate, today)),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            SchoolYearLabel = $"Ano letivo {today.Year}",
            GeneratedAtLabel = $"Gerado em {today:dd/MM/yyyy}",
            Headline = $"{child.FullName}: relatório escolar do {CurriculumStructure.GetSchoolPlacementLabel(CalculateAge(child.BirthDate, today)).ToLowerInvariant()}",
            Summary = BuildSummary(currentPhase, currentPhaseClosures, overallProgress),
            CurrentPhaseLabel = annualPlan.CurrentPhaseLabel,
            CurrentPhaseTitle = annualPlan.CurrentPhaseTitle,
            CurrentPhaseStatusLabel = ResolveOverallPhaseStatus(currentPhaseClosures),
            CurrentPhaseStatusChipClass = ResolveOverallPhaseStatusChip(currentPhaseClosures),
            CurrentPhaseSummary = annualPlan.CurrentPhaseSummary,
            SessionsThisYear = sessionsThisYear.Count,
            MinutesThisYear = sessionsThisYear.Sum(session => session.MinutesCompleted),
            EvidenceCountThisYear = sessionsThisYear.Count(session => !string.IsNullOrWhiteSpace(session.MediaUrl)),
            OverallProgressPercent = overallProgress,
            ChildUrl = url.Action("Child", "Parent", new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = url.Action("Curriculum", "Parent", new { id = child.Id }) ?? string.Empty,
            EvidenceCenterUrl = url.Action("Evidences", "Parent", new { id = child.Id }) ?? string.Empty,
            ReinforcementUrl = url.Action("Reinforcement", "Parent", new { id = child.Id }) ?? string.Empty,
            Subjects = subjects,
            PhaseClosures = phaseClosures,
            MonthlyHistory = monthlyReport.History,
            AchievementHighlights = monthlyReport.Achievements,
            NextActions = BuildNextActions(currentPhaseClosures, subjects, monthlyReport.NextCycleRecommendations)
        };
    }

    private static List<SkillProgressViewModel> BuildSkillProgress(ChildProfile child)
    {
        return child.SkillProgressEntries
            .OrderByDescending(progress => progress.MasteryScore)
            .ThenByDescending(progress => progress.TimesPracticed)
            .Select(progress => new SkillProgressViewModel
            {
                DomainLabel = CurriculumStructure.FormatDomainLabel(progress.Domain),
                SupportSourceLabel = "Currículo autoral",
                SupportSourceChipClass = "neutral",
                FunctionalTrackLabel = progress.FunctionalTrack.ToString(),
                FunctionalTrackChipClass = "neutral",
                SkillName = progress.SkillName,
                MasteryScore = progress.MasteryScore,
                TimesPracticed = progress.TimesPracticed,
                Recommendation = progress.Recommendation,
                StatusLabel = progress.MasteryScore >= 75 ? "Em rota" : progress.MasteryScore >= 45 ? "Consolidando" : "Pede reforço",
                StatusChipClass = progress.MasteryScore >= 75 ? "success" : progress.MasteryScore >= 45 ? "warning" : "neutral"
            })
            .ToList();
    }

    private static List<CurriculumEntryViewModel> BuildEntries(ChildProfile child)
    {
        return child.LearningSessions
            .OrderByDescending(session => session.LoggedAt)
            .Select(session => new CurriculumEntryViewModel
            {
                SessionId = session.Id,
                LoggedAt = session.LoggedAt,
                Theme = string.IsNullOrWhiteSpace(session.Wins) ? "Sessão registrada" : session.Wins,
                MinutesCompleted = session.MinutesCompleted,
                Wins = session.Wins,
                Challenges = session.Challenges,
                Notes = session.Notes,
                MediaUrl = session.MediaUrl,
                MediaContentType = session.MediaContentType,
                MediaFileName = session.MediaFileName,
                Skills = session.BlockFeedbacks
                    .Select(feedback => new CurriculumSkillEntryViewModel
                    {
                        SkillName = feedback.SkillCode,
                        DomainLabel = CurriculumStructure.FormatDomainLabel(
                            feedback.DailyPlanBlock?.Domain
                            ?? LearningDomain.Language),
                        PerformanceLabel = feedback.Rating switch
                        {
                            SkillFeedbackLevel.Secure => "Seguro",
                            SkillFeedbackLevel.Developing => "Em desenvolvimento",
                            _ => "Precisa de apoio"
                        },
                        PerformanceChipClass = feedback.Rating switch
                        {
                            SkillFeedbackLevel.Secure => "success",
                            SkillFeedbackLevel.Developing => "warning",
                            _ => "neutral"
                        }
                    })
                    .ToList()
            })
            .ToList();
    }

    private static string BuildSummary(
        AnnualCurriculumPhaseViewModel? currentPhase,
        IReadOnlyCollection<SchoolPhaseClosureViewModel> currentPhaseClosures,
        int overallProgress)
    {
        if (currentPhase is null)
        {
            return "O relatório organiza o ano por matérias, etapa atual, fechamento pedagógico e documentação visível.";
        }

        var readyCount = currentPhaseClosures.Count(item => string.Equals(item.StatusLabel, "pronta para fechar", StringComparison.OrdinalIgnoreCase));
        return $"{currentPhase.Title} está em andamento com {overallProgress}% de progresso médio entre as matérias. {readyCount} área(s) já podem fechar a etapa com prova e registro final.";
    }

    private static string ResolveOverallPhaseStatus(IReadOnlyCollection<SchoolPhaseClosureViewModel> currentPhaseClosures)
    {
        if (currentPhaseClosures.Count == 0)
        {
            return "sem leitura da etapa";
        }

        if (currentPhaseClosures.All(item => string.Equals(item.StatusLabel, "pronta para fechar", StringComparison.OrdinalIgnoreCase) || string.Equals(item.StatusLabel, "fechada", StringComparison.OrdinalIgnoreCase)))
        {
            return "etapa pronta para documento";
        }

        if (currentPhaseClosures.Any(item => string.Equals(item.StatusLabel, "revisão pendente", StringComparison.OrdinalIgnoreCase)))
        {
            return "etapa com revisão pendente";
        }

        if (currentPhaseClosures.Any(item => string.Equals(item.StatusLabel, "em andamento", StringComparison.OrdinalIgnoreCase)))
        {
            return "etapa em andamento";
        }

        return "etapa começando";
    }

    private static string ResolveOverallPhaseStatusChip(IReadOnlyCollection<SchoolPhaseClosureViewModel> currentPhaseClosures)
    {
        var label = ResolveOverallPhaseStatus(currentPhaseClosures);
        return label switch
        {
            "etapa pronta para documento" => "success",
            "etapa com revisão pendente" => "warning",
            "etapa em andamento" => "warning",
            _ => "neutral"
        };
    }

    private static List<string> BuildNextActions(
        IReadOnlyCollection<SchoolPhaseClosureViewModel> currentPhaseClosures,
        IReadOnlyCollection<SchoolReportSubjectViewModel> subjects,
        IReadOnlyCollection<string> fallbackRecommendations)
    {
        var actions = new List<string>();

        foreach (var closure in currentPhaseClosures
                     .OrderBy(item => item.StatusLabel == "revisão pendente" ? 0 : item.StatusLabel == "pronta para fechar" ? 1 : 2)
                     .ThenBy(item => item.SubjectLabel)
                     .Take(3))
        {
            actions.Add(closure.StatusLabel switch
            {
                "revisão pendente" => $"Retome {closure.SubjectLabel.ToLowerInvariant()} antes de seguir: {closure.ReviewSummary}",
                "pronta para fechar" => $"Feche {closure.SubjectLabel.ToLowerInvariant()} com {closure.AssessmentTitle.ToLowerInvariant()} e salve uma evidência final.",
                _ => $"Mantenha {closure.SubjectLabel.ToLowerInvariant()} em prática curta até a próxima revisão da etapa."
            });
        }

        if (actions.Count == 0)
        {
            actions.AddRange(fallbackRecommendations.Take(3));
        }

        if (actions.Count == 0 && subjects.Count > 0)
        {
            actions.Add($"Continue na matéria mais frágil agora: {subjects.OrderBy(item => item.ProgressPercent).First().SubjectLabel.ToLowerInvariant()}.");
        }

        return actions.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
    }

    private static string GetFamilyGoalTrackLabel(string familyGoalTrack) => familyGoalTrack switch
    {
        "literacy" => "Alfabetização e linguagem",
        "math_foundations" => "Matemática base",
        "science_discovery" => "Ciências, história e geografia",
        "autonomy" => "Autonomia e rotina",
        _ => "Trilha de crescimento equilibrado"
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
