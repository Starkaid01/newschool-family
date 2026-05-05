using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ChildEvolutionService(ApplicationDbContext db)
{
    public async Task<ChildEvolutionCenterViewModel> BuildEvolutionCenterAsync(Guid childId, Guid parentId, IUrlHelper url)
    {
        var snapshot = await LoadSnapshotAsync(childId, parentId);
        var report = BuildReport(snapshot);

        return new ChildEvolutionCenterViewModel
        {
            ChildId = snapshot.Child.Id,
            FullName = snapshot.Child.FullName,
            Age = CalculateAge(snapshot.Child.BirthDate, DateTime.Today),
            MonthLabel = report.MonthLabel,
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(snapshot.Child.FamilyGoalTrack),
            PromiseHeadline = report.PromiseHeadline,
            DiagnosisSummary = BuildDiagnosisSummary(report.Diagnostics),
            ReassuranceSummary = report.ParentMessage,
            SessionsThisMonth = report.SessionsThisMonth,
            MinutesThisMonth = report.MinutesThisMonth,
            EvidenceCountThisMonth = report.EvidenceCountThisMonth,
            GoalsOnTrack = report.MonthlyGoals.Count(x => x.ProgressPercent >= 70),
            RoutineUrl = url.Action("Child", "Parent", new { id = snapshot.Child.Id }) ?? string.Empty,
            CurriculumUrl = url.Action("Curriculum", "Parent", new { id = snapshot.Child.Id }) ?? string.Empty,
            ReportUrl = url.Action("EvolutionReport", "Parent", new { id = snapshot.Child.Id }) ?? string.Empty,
            PortfolioUrl = url.Action("PremiumPortfolio", "Parent", new { id = snapshot.Child.Id }) ?? string.Empty,
            Diagnostics = report.Diagnostics,
            MonthlyGoals = report.MonthlyGoals,
            Achievements = report.Achievements,
            EvidenceItems = report.EvidenceItems,
            NextCycleRecommendations = report.NextCycleRecommendations,
            History = report.History
        };
    }

    public async Task<ChildMonthlyReportViewModel> BuildMonthlyReportAsync(Guid childId, Guid parentId)
    {
        var snapshot = await LoadSnapshotAsync(childId, parentId);
        return BuildReport(snapshot);
    }

    public async Task<PremiumPortfolioViewModel> BuildPremiumPortfolioAsync(Guid childId, Guid parentId)
    {
        var snapshot = await LoadSnapshotAsync(childId, parentId);
        var report = BuildReport(snapshot);
        var badges = snapshot.Child.Achievements
            .OrderByDescending(x => x.EarnedAt)
            .Take(6)
            .Select(x => new ChildAchievementViewModel
            {
                Title = x.Title,
                Description = x.Description,
                EarnedAt = x.EarnedAt
            })
            .ToList();

        var coverEvidence = report.EvidenceItems.FirstOrDefault();

        return new PremiumPortfolioViewModel
        {
            ChildId = snapshot.Child.Id,
            FullName = snapshot.Child.FullName,
            Age = CalculateAge(snapshot.Child.BirthDate, snapshot.Today),
            MonthLabel = report.MonthLabel,
            CoverTitle = $"{snapshot.Child.FullName}: portfolio de evolucao",
            CoverSubtitle = "Um registro bonito, guardavel e compartilhavel da jornada de aprendizagem em casa.",
            ParentLetter = BuildPortfolioLetter(snapshot.Child.FullName, report.ParentMessage, report.Achievements),
            PedagogicalSignature = "NewSchool • rotina, evolucao e acompanhamento para ensino domiciliar em casa",
            CoverEvidence = coverEvidence,
            SessionsThisMonth = report.SessionsThisMonth,
            MinutesThisMonth = report.MinutesThisMonth,
            EvidenceCountThisMonth = report.EvidenceCountThisMonth,
            Achievements = report.Achievements,
            CelebrationBadges = badges,
            MonthlyGoals = report.MonthlyGoals,
            EvidenceItems = report.EvidenceItems,
            History = report.History,
            NextCycleRecommendations = report.NextCycleRecommendations
        };
    }

    public async Task SyncMonthlySnapshotsAsync(Guid childId, Guid parentId)
    {
        var child = await db.Children
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .Include(x => x.MonthlySnapshots)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        if (EnsureMonthlySnapshots(child))
        {
            await db.SaveChangesAsync();
        }
    }

    private async Task<ChildEvolutionSnapshot> LoadSnapshotAsync(Guid childId, Guid parentId)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.DevelopmentProfile)
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .Include(x => x.MonthlySnapshots)
            .Include(x => x.MonthlyGoalCycles)
            .ThenInclude(x => x.Items)
            .Include(x => x.Achievements)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var snapshotsChanged = EnsureMonthlySnapshots(child);
        if (snapshotsChanged)
        {
            await db.SaveChangesAsync();
        }

        var age = Math.Clamp(CalculateAge(child.BirthDate, today), 3, 14);
        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var monthSessions = child.LearningSessions
            .Where(x => x.LoggedAt.Year == today.Year && x.LoggedAt.Month == today.Month)
            .OrderByDescending(x => x.LoggedAt)
            .ToList();

        var currentMonthSnapshot = child.MonthlySnapshots
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .FirstOrDefault(x => x.Year == today.Year && x.Month == today.Month);

        var currentGoalCycle = child.MonthlyGoalCycles
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .FirstOrDefault(x => x.Year == today.Year && x.Month == today.Month);

        return new ChildEvolutionSnapshot
        {
            Child = child,
            Profile = child.DevelopmentProfile ?? BuildFallbackProfile(child),
            Templates = templates,
            MonthSessions = monthSessions,
            CurrentMonthSnapshot = currentMonthSnapshot,
            CurrentGoalCycle = currentGoalCycle,
            History = child.MonthlySnapshots
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(6)
                .ToList(),
            Today = today
        };
    }

    private bool EnsureMonthlySnapshots(ChildProfile child)
    {
        var changed = false;
        var groupedSessions = child.LearningSessions
            .GroupBy(x => new { x.LoggedAt.Year, x.LoggedAt.Month })
            .ToList();

        foreach (var group in groupedSessions)
        {
            var snapshot = child.MonthlySnapshots.FirstOrDefault(x =>
                x.Year == group.Key.Year &&
                x.Month == group.Key.Month);

            if (snapshot is null)
            {
                snapshot = new ChildMonthlySnapshot
                {
                    ChildId = child.Id,
                    Year = group.Key.Year,
                    Month = group.Key.Month
                };
                db.ChildMonthlySnapshots.Add(snapshot);
                child.MonthlySnapshots.Add(snapshot);
            }

            ApplyMonthlySnapshot(snapshot, group.ToList(), child.FullName);
            changed = true;
        }

        return changed;
    }

    private static void ApplyMonthlySnapshot(
        ChildMonthlySnapshot snapshot,
        List<LearningSession> sessions,
        string childName)
    {
        var feedbackByDomain = sessions
            .SelectMany(session => session.BlockFeedbacks.Select(feedback => (
                Domain: CurriculumStructure.GetAnalyticsDomain(ResolveFeedbackDomain(feedback.SkillCode)),
                Score: ResolveFeedbackScore(feedback.Rating))))
            .ToList();

        var scores = new Dictionary<LearningDomain, int>
        {
            [LearningDomain.Language] = AverageDomainScore(feedbackByDomain, LearningDomain.Language),
            [LearningDomain.Math] = AverageDomainScore(feedbackByDomain, LearningDomain.Math),
            [LearningDomain.World] = AverageDomainScore(feedbackByDomain, LearningDomain.World),
            [LearningDomain.ExecutiveFunction] = AverageDomainScore(feedbackByDomain, LearningDomain.ExecutiveFunction)
        };

        snapshot.SessionsCount = sessions.Count;
        snapshot.MinutesCount = sessions.Sum(x => x.MinutesCompleted);
        snapshot.EvidenceCount = sessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl));
        snapshot.LanguageScore = scores[LearningDomain.Language];
        snapshot.MathScore = scores[LearningDomain.Math];
        snapshot.WorldScore = scores[LearningDomain.World];
        snapshot.ExecutiveFunctionScore = scores[LearningDomain.ExecutiveFunction];

        var nonZeroScores = scores.Where(x => x.Value > 0).ToList();
        snapshot.OverallScore = nonZeroScores.Count == 0
            ? 0
            : (int)Math.Round(nonZeroScores.Average(x => x.Value));

        var strongest = nonZeroScores.OrderByDescending(x => x.Value).FirstOrDefault();
        var attention = nonZeroScores.OrderBy(x => x.Value).FirstOrDefault();
        snapshot.StrongestArea = strongest.Value == 0 ? "Rotina em construcao" : FormatDomain(strongest.Key);
        snapshot.AttentionArea = attention.Value == 0 ? "Registrar mais sessoes" : FormatDomain(attention.Key);
        snapshot.SnapshotMonth = new DateTime(snapshot.Year, snapshot.Month, 1);
        snapshot.UpdatedAt = DateTime.UtcNow;
        snapshot.Summary = snapshot.SessionsCount == 0
            ? $"Ainda sem sessoes registradas para {childName} neste ciclo."
            : $"{childName} acumulou {snapshot.SessionsCount} sessao(oes), com melhor momento em {snapshot.StrongestArea.ToLowerInvariant()} e atencao maior em {snapshot.AttentionArea.ToLowerInvariant()}.";
    }

    private static int AverageDomainScore(IEnumerable<(LearningDomain Domain, int Score)> feedbackByDomain, LearningDomain domain)
    {
        var items = feedbackByDomain.Where(x => x.Domain == domain).Select(x => x.Score).ToList();
        return items.Count == 0 ? 0 : (int)Math.Round(items.Average());
    }

    private static ChildMonthlyReportViewModel BuildReport(ChildEvolutionSnapshot snapshot)
    {
        var diagnostics = BuildDiagnostics(snapshot.Profile, snapshot.Child.SkillProgressEntries);
        var monthlyGoals = BuildMonthlyGoals(snapshot, diagnostics);
        var achievements = BuildAchievements(snapshot, diagnostics);
        var evidenceItems = snapshot.MonthSessions
            .Where(x => !string.IsNullOrWhiteSpace(x.MediaUrl))
            .Take(4)
            .Select(session => new PortfolioEvidenceViewModel
            {
                LoggedAt = session.LoggedAt,
                Title = $"{session.MinutesCompleted} min de aprendizagem",
                Description = string.IsNullOrWhiteSpace(session.Wins)
                    ? "Sessao registrada com evidencia da rotina."
                    : $"Destaque da familia: {session.Wins}",
                MediaUrl = session.MediaUrl,
                MediaContentType = session.MediaContentType
            })
            .ToList();

        var recommendationPool = monthlyGoals
            .OrderBy(x => x.ProgressPercent)
            .Take(3)
            .Select(goal => $"Priorize {goal.SkillName.ToLowerInvariant()} em {goal.DomainLabel.ToLowerInvariant()} com pratica curta e consistente.")
            .ToList();

        if (recommendationPool.Count == 0)
        {
            recommendationPool.Add("Continue registrando as sessoes para o sistema consolidar metas mais precisas para o proximo ciclo.");
        }

        var monthLabel = FormatMonthLabel(snapshot.Today.Year, snapshot.Today.Month);
        var history = BuildHistory(snapshot.History);
        var currentMetrics = snapshot.CurrentMonthSnapshot;

        return new ChildMonthlyReportViewModel
        {
            FullName = snapshot.Child.FullName,
            MonthLabel = monthLabel,
            PromiseHeadline = BuildPromiseHeadline(snapshot.Child),
            ParentMessage = BuildParentMessage(snapshot, diagnostics, monthlyGoals),
            SessionsThisMonth = currentMetrics?.SessionsCount ?? snapshot.MonthSessions.Count,
            MinutesThisMonth = currentMetrics?.MinutesCount ?? snapshot.MonthSessions.Sum(x => x.MinutesCompleted),
            EvidenceCountThisMonth = currentMetrics?.EvidenceCount ?? snapshot.MonthSessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl)),
            Diagnostics = diagnostics,
            MonthlyGoals = monthlyGoals,
            Achievements = achievements,
            EvidenceItems = evidenceItems,
            NextCycleRecommendations = recommendationPool,
            History = history
        };
    }

    private static List<MonthlyHistoryViewModel> BuildHistory(IReadOnlyList<ChildMonthlySnapshot> snapshots)
    {
        var ordered = snapshots
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();

        var history = new List<MonthlyHistoryViewModel>();
        for (var i = 0; i < ordered.Count; i++)
        {
            var current = ordered[i];
            var previous = i < ordered.Count - 1 ? ordered[i + 1] : null;
            history.Add(new MonthlyHistoryViewModel
            {
                MonthLabel = FormatMonthLabel(current.Year, current.Month),
                OverallScore = current.OverallScore,
                SessionsCount = current.SessionsCount,
                DeltaFromPrevious = previous is null ? 0 : current.OverallScore - previous.OverallScore,
                StrongestArea = current.StrongestArea,
                AttentionArea = current.AttentionArea,
                Summary = current.Summary
            });
        }

        return history;
    }

    private static List<DomainDiagnosisCardViewModel> BuildDiagnostics(
        ChildDevelopmentProfile profile,
        IEnumerable<ChildSkillProgress> progressEntries)
    {
        var progressByDomain = progressEntries
            .GroupBy(x => CurriculumStructure.GetAnalyticsDomain(x.Domain))
            .ToDictionary(
                x => x.Key,
                x => x.Any() ? (int)Math.Round(x.Average(item => item.MasteryScore)) : 0);

        var items = new[]
        {
            BuildDiagnosisCard(LearningDomain.Language, profile.LanguageLevel, progressByDomain),
            BuildDiagnosisCard(LearningDomain.Math, profile.MathLevel, progressByDomain),
            BuildDiagnosisCard(LearningDomain.World, profile.WorldLevel, progressByDomain),
            BuildDiagnosisCard(LearningDomain.ExecutiveFunction, profile.ExecutiveFunctionLevel, progressByDomain)
        };

        return items.OrderByDescending(x => x.CurrentPercent).ToList();
    }

    private static DomainDiagnosisCardViewModel BuildDiagnosisCard(
        LearningDomain domain,
        int initialLevel,
        IReadOnlyDictionary<LearningDomain, int> progressByDomain)
    {
        var initialPercent = Math.Clamp(initialLevel, 1, 5) * 20;
        var currentPercent = progressByDomain.TryGetValue(domain, out var value) && value > 0 ? value : initialPercent;
        var delta = currentPercent - initialPercent;
        var statusLabel = currentPercent switch
        {
            < 45 => "Precisa de reforco",
            < 75 => "Em consolidacao",
            _ => "Ponto forte"
        };

        return new DomainDiagnosisCardViewModel
        {
            DomainLabel = FormatDomain(domain),
            InitialPercent = initialPercent,
            CurrentPercent = currentPercent,
            DeltaPercent = delta,
            StatusLabel = statusLabel,
            StatusChipClass = currentPercent switch
            {
                < 45 => "warning",
                < 75 => "neutral",
                _ => "success"
            },
            Summary = delta switch
            {
                >= 15 => $"Avanco consistente desde o diagnostico inicial em {FormatDomain(domain).ToLowerInvariant()}.",
                > 0 => $"A crianca esta ganhando confianca em {FormatDomain(domain).ToLowerInvariant()}.",
                _ => $"Esta area pede mais rotina guiada para acelerar a evolucao."
            }
        };
    }

    private static List<MonthlyGoalViewModel> BuildMonthlyGoals(
        ChildEvolutionSnapshot snapshot,
        IReadOnlyList<DomainDiagnosisCardViewModel> diagnostics)
    {
        if (snapshot.CurrentGoalCycle is not null && snapshot.CurrentGoalCycle.Items.Count > 0)
        {
            return snapshot.CurrentGoalCycle.Items
                .OrderBy(x => x.PriorityOrder)
                .Select(item =>
                {
                    var progressPercent = item.TargetScore == 0
                        ? 0
                        : (int)Math.Round((double)item.CurrentScore / item.TargetScore * 100);
                    progressPercent = Math.Clamp(progressPercent, 0, 100);

                    return new MonthlyGoalViewModel
                    {
                        DomainLabel = FormatDomain(item.Domain),
                        SkillName = item.SkillName,
                        WhyItMatters = BuildWhyItMatters(item.Domain),
                        CurrentPercent = item.CurrentScore,
                        TargetPercent = item.TargetScore,
                        ProgressPercent = progressPercent,
                        StatusLabel = item.Status switch
                        {
                            "completed" => "Concluida",
                            "on_track" => "Em rota",
                            _ => "Em risco"
                        },
                        StatusChipClass = item.Status switch
                        {
                            "completed" => "success",
                            "on_track" => "neutral",
                            _ => "warning"
                        }
                    };
                })
                .ToList();
        }

        var weakestDomains = diagnostics
            .OrderBy(x => x.CurrentPercent)
            .Select(x => ParseDomain(x.DomainLabel))
            .ToList();

        var goals = snapshot.Child.SkillProgressEntries
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .Take(3)
            .Select(BuildGoalFromProgress)
            .ToList();

        foreach (var domain in weakestDomains)
        {
            if (goals.Count >= 3)
            {
                break;
            }

            if (goals.Any(x => string.Equals(x.DomainLabel, FormatDomain(domain), StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var template = snapshot.Templates.FirstOrDefault(x => CurriculumStructure.DomainMatches(x.Domain, domain));
            if (template is null)
            {
                continue;
            }

            goals.Add(new MonthlyGoalViewModel
            {
                DomainLabel = FormatDomain(domain),
                SkillName = template.SkillName,
                WhyItMatters = BuildWhyItMatters(domain),
                CurrentPercent = 20,
                TargetPercent = 60,
                ProgressPercent = 34,
                StatusLabel = "Comecar agora",
                StatusChipClass = "warning"
            });
        }

        return goals
            .OrderBy(x => x.ProgressPercent)
            .Take(3)
            .ToList();
    }

    private static MonthlyGoalViewModel BuildGoalFromProgress(ChildSkillProgress progress)
    {
        var target = Math.Clamp(progress.MasteryScore < 50 ? 65 : progress.MasteryScore + 15, 60, 90);
        var progressPercent = (int)Math.Round((double)progress.MasteryScore / target * 100);
        progressPercent = Math.Clamp(progressPercent, 0, 100);

        return new MonthlyGoalViewModel
        {
            DomainLabel = FormatDomain(progress.Domain),
            SkillName = progress.SkillName,
            WhyItMatters = BuildWhyItMatters(progress.Domain),
            CurrentPercent = progress.MasteryScore,
            TargetPercent = target,
            ProgressPercent = progressPercent,
            StatusLabel = progressPercent switch
            {
                < 45 => "Precisa de ritmo",
                < 75 => "Em rota",
                _ => "Quase consolidada"
            },
            StatusChipClass = progressPercent switch
            {
                < 45 => "warning",
                < 75 => "neutral",
                _ => "success"
            }
        };
    }

    private static List<AchievementHighlightViewModel> BuildAchievements(
        ChildEvolutionSnapshot snapshot,
        IReadOnlyList<DomainDiagnosisCardViewModel> diagnostics)
    {
        var highlights = new List<AchievementHighlightViewModel>();
        var strongestDiagnostic = diagnostics.OrderByDescending(x => x.CurrentPercent).FirstOrDefault();
        if (strongestDiagnostic is not null)
        {
            highlights.Add(new AchievementHighlightViewModel
            {
                Title = strongestDiagnostic.DomainLabel,
                Description = $"{snapshot.Child.FullName} ja demonstra seguranca maior em {strongestDiagnostic.DomainLabel.ToLowerInvariant()}."
            });
        }

        var strongSkills = snapshot.Child.SkillProgressEntries
            .Where(x => x.MasteryScore >= 70)
            .OrderByDescending(x => x.MasteryScore)
            .Take(2)
            .ToList();

        foreach (var skill in strongSkills)
        {
            highlights.Add(new AchievementHighlightViewModel
            {
                Title = skill.SkillName,
                Description = $"Habilidade consolidando com {skill.MasteryScore}% de dominio e {skill.TimesPracticed} pratica(s)."
            });
        }

        var bestHistory = snapshot.History
            .OrderByDescending(x => x.OverallScore)
            .FirstOrDefault();
        if (bestHistory is not null)
        {
            highlights.Add(new AchievementHighlightViewModel
            {
                Title = bestHistory.StrongestArea,
                Description = $"No ciclo de {FormatMonthLabel(bestHistory.Year, bestHistory.Month)}, essa foi a frente mais forte registrada."
            });
        }

        if (highlights.Count == 0)
        {
            highlights.Add(new AchievementHighlightViewModel
            {
                Title = "Base em construcao",
                Description = "O portifolio vai ficar cada vez mais forte conforme a familia registra as proximas sessoes."
            });
        }

        return highlights.Take(3).ToList();
    }

    private static string BuildParentMessage(
        ChildEvolutionSnapshot snapshot,
        IReadOnlyList<DomainDiagnosisCardViewModel> diagnostics,
        IReadOnlyList<MonthlyGoalViewModel> monthlyGoals)
    {
        var bestDomain = diagnostics.OrderByDescending(x => x.DeltaPercent).FirstOrDefault()?.DomainLabel ?? "a rotina";
        var attentionGoal = monthlyGoals.OrderBy(x => x.ProgressPercent).FirstOrDefault()?.SkillName ?? "as habilidades prioritarias";
        var historyDelta = snapshot.History
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(2)
            .ToList();

        if (snapshot.MonthSessions.Count == 0)
        {
            return $"O diagnostico inicial ja mostra os primeiros focos da {GetFamilyGoalTrackLabel(snapshot.Child.FamilyGoalTrack).ToLowerInvariant()}. Agora basta registrar as sessoes para provar a evolucao de {snapshot.Child.FullName}.";
        }

        if (snapshot.CurrentGoalCycle?.RiskLevel == "high")
        {
            return $"{snapshot.Child.FullName} entrou em zona de atencao neste ciclo. O sistema detectou metas em risco dentro da {GetFamilyGoalTrackLabel(snapshot.Child.FamilyGoalTrack).ToLowerInvariant()} e pede uma retomada guiada imediata em {attentionGoal.ToLowerInvariant()}.";
        }

        if (historyDelta.Count == 2)
        {
            var delta = historyDelta[0].OverallScore - historyDelta[1].OverallScore;
            var trend = delta > 0
                ? $"subindo {delta} pontos em relacao ao ciclo anterior"
                : delta < 0
                    ? $"com oscilacao de {Math.Abs(delta)} pontos em relacao ao ciclo anterior"
                    : "estavel em relacao ao ciclo anterior";
            return $"{snapshot.Child.FullName} esta {trend}, com melhor consistencia em {bestDomain.ToLowerInvariant()} e foco atual em consolidar {attentionGoal.ToLowerInvariant()} dentro da {GetFamilyGoalTrackLabel(snapshot.Child.FamilyGoalTrack).ToLowerInvariant()}.";
        }

        return $"{snapshot.Child.FullName} avancou com mais consistencia em {bestDomain.ToLowerInvariant()} e agora o foco do ciclo e consolidar {attentionGoal.ToLowerInvariant()} com pratica intencional dentro da {GetFamilyGoalTrackLabel(snapshot.Child.FamilyGoalTrack).ToLowerInvariant()}.";
    }

    private static string BuildDiagnosisSummary(IReadOnlyList<DomainDiagnosisCardViewModel> diagnostics)
    {
        var strong = diagnostics.OrderByDescending(x => x.CurrentPercent).Take(2).Select(x => x.DomainLabel.ToLowerInvariant()).ToList();
        var support = diagnostics.OrderBy(x => x.CurrentPercent).FirstOrDefault()?.DomainLabel.ToLowerInvariant() ?? "rotina";
        return $"Pontos fortes em {string.Join(" e ", strong)}. Area que merece reforco agora: {support}.";
    }

    private static string BuildPortfolioLetter(
        string childName,
        string parentMessage,
        IReadOnlyList<AchievementHighlightViewModel> achievements)
    {
        var firstAchievement = achievements.FirstOrDefault()?.Title ?? "seu progresso";
        return $"{childName} viveu um mes de construcao real. {parentMessage} Entre as conquistas mais bonitas deste ciclo, vale destacar {firstAchievement.ToLowerInvariant()}, que representa o quanto a rotina da familia esta ganhando consistencia e significado.";
    }

    private static string BuildWhyItMatters(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Fortalece comunicacao, alfabetizacao e compreensao.",
        LearningDomain.Math => "Aumenta raciocinio e seguranca para resolver problemas.",
        LearningDomain.Science => "Fortalece observacao, investigacao e leitura de fenomenos do cotidiano.",
        LearningDomain.History => "Ajuda a entender tempo, memoria, causa e consequencia.",
        LearningDomain.Geography => "Organiza leitura de mapas, lugar, territorio e ambiente.",
        LearningDomain.World => "Amplia repertorio em ciências, história e geografia.",
        _ => "Sustenta foco, autonomia e constancia nas atividades."
    };

    private static string BuildPromiseHeadline(ChildProfile child) => child.FamilyGoalTrack switch
    {
        "literacy" => $"A evolucao de {child.FullName} agora prova avancos reais na trilha de alfabetizacao e linguagem.",
        "math_foundations" => $"A evolucao de {child.FullName} agora prova avancos reais na trilha de matematica base.",
        "autonomy" => $"A evolucao de {child.FullName} agora prova avancos reais na trilha de autonomia e foco.",
        "science_discovery" => $"A evolucao de {child.FullName} agora prova avancos reais na trilha de ciencias em casa.",
        _ => $"A evolucao de {child.FullName} agora fica registrada ciclo a ciclo, com metas persistidas e historico mensal real."
    };

    private static string GetFamilyGoalTrackLabel(string familyGoalTrack) => familyGoalTrack switch
    {
        "literacy" => "trilha de alfabetizacao",
        "math_foundations" => "trilha de matematica base",
        "autonomy" => "trilha de autonomia e foco",
        "science_discovery" => "trilha de ciencias em casa",
        _ => "trilha de crescimento equilibrado"
    };

    private static ChildDevelopmentProfile BuildFallbackProfile(ChildProfile child)
    {
        return new ChildDevelopmentProfile
        {
            ChildId = child.Id,
            LanguageLevel = 3,
            MathLevel = 3,
            WorldLevel = 3,
            ExecutiveFunctionLevel = 3,
            StrengthsSummary = string.IsNullOrWhiteSpace(child.Notes) ? "Familia em fase inicial de acompanhamento." : child.Notes,
            SupportSummary = "Diagnostico inicial preenchido automaticamente por falta de checklist estruturado."
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

    private static int ResolveFeedbackScore(SkillFeedbackLevel rating) => rating switch
    {
        SkillFeedbackLevel.NeedsSupport => 35,
        SkillFeedbackLevel.Developing => 60,
        _ => 85
    };

    private static LearningDomain ResolveFeedbackDomain(string skillCode)
    {
        if (skillCode.Contains("-Language-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.Language;
        }

        if (skillCode.Contains("-Math-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.Math;
        }

        if (skillCode.Contains("-Science-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.Science;
        }

        if (skillCode.Contains("-History-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.History;
        }

        if (skillCode.Contains("-Geography-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.Geography;
        }

        if (skillCode.Contains("-World-", StringComparison.OrdinalIgnoreCase))
        {
            return LearningDomain.World;
        }

        return LearningDomain.ExecutiveFunction;
    }

    private static string FormatDomain(LearningDomain domain) => domain switch
    {
        LearningDomain.World => "Ciências, história e geografia",
        LearningDomain.ExecutiveFunction => "Autonomia",
        _ => CurriculumStructure.FormatDomainLabel(domain)
    };

    private static string FormatMonthLabel(int year, int month)
    {
        var label = new DateTime(year, month, 1).ToString("MMMM 'de' yyyy");
        return char.ToUpperInvariant(label[0]) + label[1..];
    }

    private static LearningDomain ParseDomain(string domainLabel)
    {
        return domainLabel switch
        {
            "Linguagem" => LearningDomain.Language,
            "Matematica" => LearningDomain.Math,
            "Matemática" => LearningDomain.Math,
            "Ciências" => LearningDomain.Science,
            "Ciencias" => LearningDomain.Science,
            "História" => LearningDomain.History,
            "Historia" => LearningDomain.History,
            "Geografia" => LearningDomain.Geography,
            "Ciências, história e geografia" => LearningDomain.World,
            "Ciencias, historia e geografia" => LearningDomain.World,
            "Mundo real" => LearningDomain.World,
            "Autonomia" => LearningDomain.ExecutiveFunction,
            _ => LearningDomain.ExecutiveFunction
        };
    }

    private sealed class ChildEvolutionSnapshot
    {
        public required ChildProfile Child { get; init; }
        public required ChildDevelopmentProfile Profile { get; init; }
        public required List<CurriculumTemplate> Templates { get; init; }
        public required List<LearningSession> MonthSessions { get; init; }
        public required ChildMonthlySnapshot? CurrentMonthSnapshot { get; init; }
        public required ChildMonthlyGoalCycle? CurrentGoalCycle { get; init; }
        public required List<ChildMonthlySnapshot> History { get; init; }
        public required DateTime Today { get; init; }
    }
}
