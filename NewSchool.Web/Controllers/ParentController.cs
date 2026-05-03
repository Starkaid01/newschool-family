using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using NewSchool.Web.Services;
using Stripe;
using Stripe.Checkout;

namespace NewSchool.Web.Controllers;

[Authorize(Roles = "Parent")]
public class ParentController(
    ApplicationDbContext db,
    IWebHostEnvironment environment,
    AuthService authService,
    LearningPlanService learningPlanService,
    ChildGoalCycleService childGoalCycleService,
    ChildEvolutionService childEvolutionService,
    ChildRecoveryPlanService childRecoveryPlanService,
    ChildAchievementService childAchievementService,
    ConsistencyService consistencyService,
    ReferralService referralService,
    TrackAnalyticsService trackAnalyticsService,
    SkillProgressionService skillProgressionService,
    AdultInterventionService adultInterventionService,
    AdaptiveRoutineService adaptiveRoutineService,
    SkillCheckupService skillCheckupService,
    SkillReadinessService skillReadinessService,
    CuratedLearningLibraryService curatedLearningLibraryService,
    GuidedLessonExperienceService guidedLessonExperienceService,
    EvidenceAutomationService evidenceAutomationService,
    EvidenceStoragePlanService evidenceStoragePlanService,
    WeeklyRoadmapService weeklyRoadmapService,
    AnnualCurriculumPlannerService annualCurriculumPlannerService,
    SystemCurriculumLibraryService systemCurriculumLibraryService,
    ExternalContentHubService externalContentHubService,
    PortuguesePlanningService portuguesePlanningService,
    FamilyLibraryService familyLibraryService) : Controller
{
    private const long EvidenceUploadMaxBytes = 95L * 1024 * 1024;
    private const int EvidenceUploadMaxMegabytes = 95;
    private const long ChunkedEvidenceUploadMaxBytes = 350L * 1024 * 1024;
    private const int ChunkedEvidenceUploadMaxMegabytes = 350;
    private const int EvidenceUploadChunkSizeBytes = 8 * 1024 * 1024;
    private const int EvidenceUploadChunkRequestMaxBytes = 12 * 1024 * 1024;

    private static readonly HashSet<string> AllowedEvidenceContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/heic",
        "image/heif",
        "video/mp4",
        "video/webm",
        "video/quicktime",
        "video/x-m4v",
        "video/3gpp",
        "video/3gpp2",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/octet-stream"
    };

    private static readonly HashSet<string> AllowedEvidenceExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".heic",
        ".heif",
        ".mp4",
        ".mov",
        ".m4v",
        ".webm",
        ".3gp",
        ".3g2",
        ".pdf",
        ".doc",
        ".docx"
    };

    private static readonly string[] StrengthLabels =
    [
        "curiosidade e investigacao",
        "boa comunicacao",
        "criatividade",
        "memoriza com facilidade",
        "gosta de logica e padroes"
    ];

    private static readonly string[] SupportLabels =
    [
        "precisa reforcar foco",
        "precisa de ajuda nas transicoes",
        "precisa reforcar linguagem",
        "precisa reforcar autorregulacao"
    ];

    private static readonly string[] CommunicationLabels =
    [
        "entende melhor linguagem direta",
        "responde melhor com apoio visual",
        "precisa de modelagem antes de tentar",
        "precisa de mais tempo para processar"
    ];

    private static readonly string[] TriggerLabels =
    [
        "mudanca inesperada",
        "barulho",
        "escrita ou tarefa no papel",
        "espera",
        "demanda social"
    ];

    private static readonly string[] CalmingLabels =
    [
        "pausa curta",
        "movimento",
        "lugar silencioso",
        "desenhar",
        "musica calma"
    ];

    private static readonly string[] SchoolBarrierLabels =
    [
        "crises de ansiedade ligadas a escola",
        "sobrecarga sensorial no ambiente escolar",
        "rigidez cognitiva alta",
        "demanda social alta demais"
    ];

    private static readonly string[] TransitionSupportLabels =
    [
        "rotina visual",
        "first-then",
        "timer visual",
        "aviso antecipado antes da troca"
    ];

    private static readonly string[] ReinforcerLabels =
    [
        "pausa curta",
        "movimento",
        "desenhar",
        "tempo com interesse especial"
    ];

    public async Task<IActionResult> Index()
    {
        var parentId = GetCurrentUserId();
        var today = DateTime.Today;
        var weekStart = GetWeekStart(today);
        var checkoutState = Request.Query["checkout"].ToString();
        var sessionId = Request.Query["session_id"].ToString();
        var gate = Request.Query["gate"].ToString();

        if (checkoutState == "success" && !string.IsNullOrWhiteSpace(sessionId))
        {
            await SyncCheckoutSessionAsync(parentId, sessionId);
        }

        var parent = await db.Users.FirstAsync(x => x.Id == parentId);

        var inactiveDaysBeforeVisit = parent.LastActiveAt.HasValue
            ? (today - parent.LastActiveAt.Value.Date).Days
            : 99;

        parent.LastActiveAt = DateTime.UtcNow;
        var authCookieNeedsRefresh = false;
        if (string.Equals(parent.SubscriptionStatus, "trial_expired", StringComparison.OrdinalIgnoreCase) &&
            !evidenceStoragePlanService.IsPaidStorageActive(parent))
        {
            parent.SubscriptionStatus = "inactive";
            authCookieNeedsRefresh = true;
        }

        if (inactiveDaysBeforeVisit >= 7)
        {
            parent.ReactivationEmailSentAt = null;
        }

        if (parent.SubscriptionStatus == "active")
        {
            parent.PaymentRecoveryEmailSentAt = null;
        }

        await db.SaveChangesAsync();
        if (authCookieNeedsRefresh || checkoutState == "success")
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authService.CreatePrincipal(parent));
        }
        var children = await db.Children
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        foreach (var child in children)
        {
            await learningPlanService.EnsurePlanAsync(child, today);
        }

        var childIds = children.Select(x => x.Id).ToList();
        var todayPlans = childIds.Count == 0
            ? new Dictionary<Guid, (string Theme, bool Completed)>()
            : await db.DailyPlans
                .AsNoTracking()
                .Where(x => childIds.Contains(x.ChildId) && x.PlannedDate == today)
                .Select(x => new
                {
                    x.ChildId,
                    x.Theme,
                    x.Completed
                })
                .ToDictionaryAsync(x => x.ChildId, x => (x.Theme, x.Completed));

        var totalSessionsByChild = childIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await db.LearningSessions
                .AsNoTracking()
                .Where(x => childIds.Contains(x.ChildId))
                .GroupBy(x => x.ChildId)
                .Select(group => new
                {
                    ChildId = group.Key,
                    TotalSessions = group.Count()
                })
                .ToDictionaryAsync(x => x.ChildId, x => x.TotalSessions);

        var weeklyTotals = childIds.Count == 0
            ? null
            : await db.LearningSessions
                .AsNoTracking()
                .Where(x => childIds.Contains(x.ChildId) && x.LoggedAt >= weekStart)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    Sessions = group.Count(),
                    Minutes = group.Sum(x => x.MinutesCompleted)
                })
                .FirstOrDefaultAsync();

        var childCards = new List<ChildCardViewModel>();
        foreach (var child in children)
        {
            var age = CalculateAge(child.BirthDate, today);
            todayPlans.TryGetValue(child.Id, out var todayPlan);
            totalSessionsByChild.TryGetValue(child.Id, out var totalSessions);
            var hasTodayPlan = todayPlans.ContainsKey(child.Id);
            var planCompletedToday = hasTodayPlan && todayPlan.Completed;

            childCards.Add(new ChildCardViewModel
            {
                Id = child.Id,
                FullName = child.FullName,
                Age = age,
                DailyStudyMinutes = child.DailyStudyMinutes,
                CurrentFocus = hasTodayPlan
                    ? todayPlan.Theme
                    : "A aula do dia fica pronta aqui",
                PlanCompletedToday = planCompletedToday,
                TotalSessions = totalSessions,
                WeeklyHeadline = planCompletedToday
                    ? "Dia concluido. O proximo passo agora e guardar evidencias ou abrir o curriculo."
                    : "Abra a aula do dia e siga as licoes em ordem, sem precisar montar nada manualmente.",
                AlertChipLabel = planCompletedToday ? "Em rota" : "Próxima ação",
                AlertChipClass = planCompletedToday ? "success" : "warning"
            });
        }

        var storageSummary = await evidenceStoragePlanService.BuildSummaryAsync(parentId);

        var vm = new ParentDashboardViewModel
        {
            ParentName = parent.FullName,
            TotalChildren = children.Count,
            SessionsThisWeek = weeklyTotals?.Sessions ?? 0,
            MinutesThisWeek = weeklyTotals?.Minutes ?? 0,
            SubscriptionStatus = parent.SubscriptionStatus,
            TrialEndsAt = parent.TrialEndsAt,
            TrialDaysLeft = parent.TrialEndsAt.HasValue ? (parent.TrialEndsAt.Value.Date - today).Days : null,
            SubscriptionCurrentPeriodEnd = parent.SubscriptionCurrentPeriodEnd,
            IsSubscriber = storageSummary.HasPaidPlan,
            GateMessage = gate == "premium"
                ? "O sistema inteiro segue livre. A cobrança agora vale apenas para ampliar o acervo de evidências."
                : null,
            Storage = storageSummary,
            Consistency = await consistencyService.BuildDashboardSnapshotAsync(parentId),
            Children = childCards
        };

        ViewBag.Checkout = checkoutState;
        ViewBag.Billing = Request.Query["billing"].ToString();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> WeeklyReport()
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var parentId = GetCurrentUserId();
        var weekStart = GetWeekStart(DateTime.Today);
        var weekEnd = weekStart.AddDays(6);

        var parent = await db.Users.FirstAsync(x => x.Id == parentId);
        var children = await db.Children
            .Where(x => x.ParentId == parentId)
            .Include(x => x.SkillProgressEntries)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        var weeklyReportChildren = children
            .Select(child => BuildWeeklyChildReport(child))
            .ToList();

        var vm = new WeeklyReportPrintViewModel
        {
            ParentName = parent.FullName,
            WeekLabel = $"{weekStart:dd/MM/yyyy} a {weekEnd:dd/MM/yyyy}",
            WeeklyReport = BuildWeeklyFamilyReport(weeklyReportChildren)
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult CreateChild(string? goalTrack = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        PrepareChildFormView(isEditMode: false);
        return View(new CreateChildViewModel
        {
            FamilyGoalTrack = NormalizeFamilyGoalTrack(goalTrack),
            TeachingMethodology = "eclectic",
            EstimatedAgeYears = 6,
            DailyStudyMinutes = 45
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChild(CreateChildViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        if (!ModelState.IsValid)
        {
            PrepareChildFormView(isEditMode: false);
            return View(model);
        }

        var parentId = GetCurrentUserId();
        var existingChildrenCount = await db.Children.CountAsync(x => x.ParentId == parentId);
        var resolvedName = ResolveChildDisplayName(model.FullName, existingChildrenCount);
        var resolvedBirthDate = ResolveBirthDate(model.BirthDate, model.EstimatedAgeYears);
        var child = new ChildProfile
        {
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow
        };
        var developmentProfile = new ChildDevelopmentProfile
        {
            Child = child,
            AssessedAt = DateTime.UtcNow
        };
        var teaProfile = new ChildTeaProfile
        {
            Child = child,
            UpdatedAt = DateTime.UtcNow
        };

        ApplyChildFormValues(
            model,
            child,
            developmentProfile,
            teaProfile,
            resolvedName,
            resolvedBirthDate,
            preserveHiddenValues: false);

        db.Children.Add(child);
        db.ChildDevelopmentProfiles.Add(developmentProfile);
        db.ChildTeaProfiles.Add(teaProfile);
        await db.SaveChangesAsync();
        await trackAnalyticsService.CaptureTrackForChildAsync(parentId, child.Id, child.FamilyGoalTrack);
        await learningPlanService.EnsurePlanAsync(child, DateTime.Today);

        return RedirectToAction(nameof(Teach), new { id = child.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditChild(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildWithProfilesAsync(id);
        PrepareChildFormView(isEditMode: true, child.Id, child.FullName);
        return View("CreateChild", BuildChildFormViewModel(child));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditChild(Guid id, CreateChildViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildWithProfilesAsync(id);

        if (!ModelState.IsValid)
        {
            PrepareChildFormView(isEditMode: true, child.Id, child.FullName);
            return View("CreateChild", model);
        }

        var developmentProfile = child.DevelopmentProfile ?? new ChildDevelopmentProfile
        {
            ChildId = child.Id,
            AssessedAt = DateTime.UtcNow
        };
        var teaProfile = child.TeaProfile ?? new ChildTeaProfile
        {
            ChildId = child.Id,
            UpdatedAt = DateTime.UtcNow
        };

        if (child.DevelopmentProfile is null)
        {
            db.ChildDevelopmentProfiles.Add(developmentProfile);
        }

        if (child.TeaProfile is null)
        {
            db.ChildTeaProfiles.Add(teaProfile);
        }

        var resolvedName = string.IsNullOrWhiteSpace(model.FullName)
            ? child.FullName
            : model.FullName.Trim();
        var resolvedBirthDate = ResolveBirthDate(model.BirthDate, model.EstimatedAgeYears);

        ApplyChildFormValues(
            model,
            child,
            developmentProfile,
            teaProfile,
            resolvedName,
            resolvedBirthDate,
            preserveHiddenValues: true);

        await db.SaveChangesAsync();
        await trackAnalyticsService.CaptureTrackForChildAsync(GetCurrentUserId(), child.Id, child.FamilyGoalTrack);
        await ResetUpcomingPlansAsync(child.Id);
        await learningPlanService.EnsurePlanAsync(child, DateTime.Today);

        TempData["StatusMessage"] = $"{resolvedName} foi atualizada e o plano futuro foi reorganizado.";
        TempData["StatusKind"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChild(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildWithProfilesAsync(id);
        var childName = child.FullName;

        await DeleteChildGraphAsync(child.Id);

        TempData["StatusMessage"] = $"{childName} foi excluída com currículo, histórico e evidências ligadas a essa criança.";
        TempData["StatusKind"] = "warning";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Teach(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await GetOwnedChildAsync(id);
        return RedirectToAction(nameof(Child), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> TeachPrint(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await GetOwnedChildAsync(id);
        return RedirectToAction(nameof(Child), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Child(Guid id, string? flow = null, string? nextLesson = null, bool dayDone = false)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        var plan = await learningPlanService.EnsurePlanAsync(child, DateTime.Today);

        var reloadedPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .Include(x => x.Sessions.OrderByDescending(s => s.LoggedAt))
            .FirstAsync(x => x.Id == plan.Id);
        var completedBlockIds = (await db.DailyPlanBlockCompletions
                .Where(x => x.ChildId == child.Id && x.DailyPlanId == reloadedPlan.Id)
                .Select(x => x.DailyPlanBlockId)
                .ToListAsync())
            .ToHashSet();

        var tomorrowDate = DateTime.Today.AddDays(1).Date;
        var tomorrowPreviewPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .FirstOrDefaultAsync(x => x.ChildId == child.Id && x.PlannedDate == tomorrowDate);
        var curatedSuggestions = await curatedLearningLibraryService.BuildBlockSuggestionsAsync(child, reloadedPlan.Blocks);
        var externalRecommendations = await externalContentHubService.BuildChildRecommendationsAsync(child, reloadedPlan.Blocks.ToList(), Url);
        var libraryBridge = await familyLibraryService.BuildCurriculumBridgeAsync(GetCurrentUserId(), child, reloadedPlan.Blocks.ToList(), Url);
        var systemCurriculumTracks = await systemCurriculumLibraryService.BuildAsync(child);
        var printableSuggestions = await familyLibraryService.BuildBlockPrintableMapAsync(GetCurrentUserId(), child, reloadedPlan.Blocks.ToList(), Url, curatedSuggestions);
        var guidedLesson = guidedLessonExperienceService.BuildGuidedLesson(
            child,
            reloadedPlan.Blocks.ToList(),
            curatedSuggestions,
            completedBlockIds,
            systemCurriculumTracks,
            printableSuggestions);
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel>? tomorrowSuggestions = null;
        IReadOnlyDictionary<Guid, FamilyLibraryRecommendationViewModel> tomorrowPrintableSuggestions = new Dictionary<Guid, FamilyLibraryRecommendationViewModel>();
        if (tomorrowPreviewPlan is not null && tomorrowPreviewPlan.Blocks.Count > 0)
        {
            tomorrowSuggestions = await curatedLearningLibraryService.BuildBlockSuggestionsAsync(child, tomorrowPreviewPlan.Blocks);
            tomorrowPrintableSuggestions = await familyLibraryService.BuildBlockPrintableMapAsync(GetCurrentUserId(), child, tomorrowPreviewPlan.Blocks.ToList(), Url, tomorrowSuggestions);
        }
        var tomorrowLesson = guidedLessonExperienceService.BuildTomorrowPreview(
            child,
            tomorrowPreviewPlan,
            tomorrowSuggestions,
            systemCurriculumTracks,
            tomorrowPrintableSuggestions);

        var vm = new ChildDetailsViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            DailyStudyMinutes = child.DailyStudyMinutes,
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            LessonFlowHeadline = dayDone
                ? "Dia concluído"
                : string.Equals(flow, "advanced", StringComparison.OrdinalIgnoreCase)
                    ? "Próxima lição já pronta"
                    : string.Empty,
            LessonFlowMessage = dayDone
                ? "Todas as lições de hoje foram registradas. Se quiser, o próximo passo agora é só guardar evidências depois."
                : string.Equals(flow, "advanced", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(nextLesson)
                    ? $"'{nextLesson}' já subiu para Faça agora. O responsável pode seguir direto, sem procurar a próxima atividade."
                    : string.Empty,
            LessonFlowStyle = dayDone ? "neutral" : "success",
            Theme = reloadedPlan.Theme,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            ExternalContentRecommendations = externalRecommendations,
            LibraryBridge = libraryBridge,
            SystemCurriculumTracks = systemCurriculumTracks,
            GuidedLesson = guidedLesson,
            TomorrowLesson = tomorrowLesson,
            EvidenceCenterUrl = Url.Action(nameof(Evidences), new { id = child.Id }) ?? string.Empty,
            TomorrowPreviewUrl = Url.Action(nameof(TomorrowPreview), new { id = child.Id }) ?? string.Empty,
            SessionHistory = reloadedPlan.Sessions.Select(session => new SessionHistoryViewModel
            {
                Id = session.Id,
                LoggedAt = session.LoggedAt,
                MinutesCompleted = session.MinutesCompleted,
                Theme = reloadedPlan.Theme,
                Wins = session.Wins,
                Challenges = session.Challenges,
                Notes = session.Notes,
                MediaUrl = session.MediaUrl,
                MediaContentType = session.MediaContentType,
                MediaFileName = session.MediaFileName
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkTaskComplete(Guid childId, Guid blockId, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(childId);
        var block = await db.DailyPlanBlocks
            .Include(x => x.DailyPlan)
            .FirstOrDefaultAsync(x => x.Id == blockId && x.DailyPlan.ChildId == childId);

        if (block is null)
        {
            return RedirectToAction(nameof(Child), new { id = childId });
        }

        var existingCompletion = await db.DailyPlanBlockCompletions
            .FirstOrDefaultAsync(x => x.ChildId == childId && x.DailyPlanBlockId == blockId);

        if (existingCompletion is null)
        {
            db.DailyPlanBlockCompletions.Add(new DailyPlanBlockCompletion
            {
                ChildId = childId,
                DailyPlanId = block.DailyPlanId,
                DailyPlanBlockId = blockId,
                CompletedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrWhiteSpace(block.SkillCode))
            {
                var progress = await db.ChildSkillProgressEntries
                    .FirstOrDefaultAsync(x => x.ChildId == childId && x.SkillCode == block.SkillCode);

                if (progress is null)
                {
                    progress = new ChildSkillProgress
                    {
                        ChildId = childId,
                        Age = block.DailyPlan.AgeAtGeneration,
                        Domain = block.Domain,
                        SupportScope = block.SupportScope,
                        FunctionalTrack = block.FunctionalTrack,
                        SkillCode = block.SkillCode,
                        SkillName = block.SkillName,
                        MasteryScore = 45
                    };
                    skillProgressionService.InitializeProgress(progress);
                    db.ChildSkillProgressEntries.Add(progress);
                }

                progress.SupportScope = block.SupportScope;
                progress.FunctionalTrack = block.FunctionalTrack;
                skillProgressionService.ApplyFeedback(progress, block, SkillFeedbackLevel.Developing);
            }
        }

        var blockIds = await db.DailyPlanBlocks
            .Where(x => x.DailyPlanId == block.DailyPlanId)
            .Select(x => x.Id)
            .ToListAsync();
        var completedCount = await db.DailyPlanBlockCompletions
            .CountAsync(x => x.ChildId == childId && x.DailyPlanId == block.DailyPlanId);
        if (existingCompletion is null)
        {
            completedCount++;
        }
        block.DailyPlan.Completed = completedCount >= blockIds.Count;

        var parent = await db.Users.FirstAsync(x => x.Id == GetCurrentUserId());
        parent.LastActiveAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await childGoalCycleService.SyncCurrentCycleAsync(childId, GetCurrentUserId());
        await childEvolutionService.SyncMonthlySnapshotsAsync(childId, GetCurrentUserId());
        var completedBlockIds = await db.DailyPlanBlockCompletions
            .Where(x => x.ChildId == childId && x.DailyPlanId == block.DailyPlanId)
            .Select(x => x.DailyPlanBlockId)
            .ToListAsync();
        var nextBlock = await db.DailyPlanBlocks
            .Where(x => x.DailyPlanId == block.DailyPlanId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.Title
            })
            .FirstOrDefaultAsync(x => !completedBlockIds.Contains(x.Id));

        if (nextBlock is not null)
        {
            var nextLessonUrl = Url.Action(nameof(Child), new
            {
                id = childId,
                flow = "advanced",
                nextLesson = nextBlock.Title
            }) ?? $"/Parent/Child/{childId}";
            return Redirect($"{nextLessonUrl}#guided-lessons");
        }

        var completedDayUrl = Url.Action(nameof(Child), new
        {
            id = childId,
            flow = "done",
            dayDone = true
        }) ?? $"/Parent/Child/{childId}";
        return Redirect($"{completedDayUrl}#guided-lessons");
    }

    [HttpGet]
    public async Task<IActionResult> EvidenceHome()
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var parentId = GetCurrentUserId();
        var nextChildId = await db.Children
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.FullName)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!nextChildId.HasValue)
        {
            TempData["StatusMessage"] = "Cadastre uma criança primeiro. Depois o acervo da família aparece no lugar certo.";
            TempData["StatusKind"] = "warning";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Evidences), new { id = nextChildId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Evidences(Guid id, string? q = null, string? type = null, int page = 1)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildEvidenceCenterViewModelAsync(id, q, type, page));
    }

    [HttpGet]
    public async Task<IActionResult> UploadEvidence(Guid? id = null, Guid? childId = null)
    {
        var requestedChildId = childId ?? id;
        var ownerId = GetCurrentUserId();

        if (requestedChildId.HasValue)
        {
            var ownedChildId = await db.Children
                .Where(x => x.ParentId == ownerId && x.Id == requestedChildId.Value)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync();

            if (ownedChildId.HasValue)
            {
                TempData["StatusMessage"] = "O envio de arquivos fica dentro de Evidências. O sistema já levou você para a página certa.";
                TempData["StatusKind"] = "info";
                return RedirectToAction(nameof(Evidences), new { id = ownedChildId.Value });
            }
        }

        var firstChildId = await db.Children
            .Where(x => x.ParentId == ownerId)
            .OrderBy(x => x.FullName)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (firstChildId.HasValue)
        {
            TempData["StatusMessage"] = "O envio de arquivos fica dentro de Evidências. O sistema já abriu a criança certa para você.";
            TempData["StatusKind"] = "info";
            return RedirectToAction(nameof(Evidences), new { id = firstChildId.Value });
        }

        TempData["StatusMessage"] = "Cadastre uma criança primeiro. Depois o acervo de evidências fica disponível no lugar certo.";
        TempData["StatusKind"] = "warning";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Plans(string? billing = null, string? checkout = null, string? session_id = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var parentId = GetCurrentUserId();
        var authCookieNeedsRefresh = false;
        if (checkout == "success" && !string.IsNullOrWhiteSpace(session_id))
        {
            await SyncCheckoutSessionAsync(parentId, session_id);
            authCookieNeedsRefresh = true;
        }

        var parent = await db.Users.FirstAsync(x => x.Id == parentId);
        if (string.Equals(parent.SubscriptionStatus, "trial_expired", StringComparison.OrdinalIgnoreCase) &&
            !evidenceStoragePlanService.IsPaidStorageActive(parent))
        {
            parent.SubscriptionStatus = "inactive";
            await db.SaveChangesAsync();
            authCookieNeedsRefresh = true;
        }

        if (authCookieNeedsRefresh)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authService.CreatePrincipal(parent));
        }

        var storageSummary = await evidenceStoragePlanService.BuildSummaryAsync(parentId);
        return View(new ParentPlansViewModel
        {
            ParentName = parent.FullName,
            ReturnUrl = Url.Action(nameof(Plans), "Parent") ?? "/Parent/Plans",
            BillingState = billing,
            CheckoutState = checkout,
            Storage = storageSummary
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = EvidenceUploadMaxBytes)]
    [RequestSizeLimit(EvidenceUploadMaxBytes)]
    public async Task<IActionResult> UploadEvidence([Bind(Prefix = "Upload")] UploadEvidenceViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var allowance = await evidenceStoragePlanService.BuildAllowanceAsync(GetCurrentUserId());
        if (!allowance.CanUpload)
        {
            TempData["StatusMessage"] = allowance.Message;
            TempData["StatusKind"] = "warning";
            return RedirectToAction(nameof(Evidences), new { id = model.ChildId });
        }

        var child = await GetOwnedChildAsync(model.ChildId);
        if (model.File is not null && model.File.Length > EvidenceUploadMaxBytes)
        {
            TempData["StatusMessage"] = $"O video ou documento passou de {EvidenceUploadMaxMegabytes} MB. Envie um arquivo menor para guardar nesta versão do sistema.";
            TempData["StatusKind"] = "warning";
            return RedirectToAction(nameof(Evidences), new { id = model.ChildId });
        }

        var uploadedMedia = await SaveEvidenceFileAsync(model.File);
        if (uploadedMedia is null)
        {
            TempData["StatusMessage"] = $"Nao foi possivel salvar esse arquivo. Use foto, video ou documento em formato aceito e com ate {EvidenceUploadMaxMegabytes} MB.";
            TempData["StatusKind"] = "warning";
            return RedirectToAction(nameof(Evidences), new { id = model.ChildId });
        }

        var dailyPlanId = model.DailyPlanId
            ?? await db.DailyPlans
                .Where(x => x.ChildId == child.Id)
                .OrderByDescending(x => x.PlannedDate)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync();

        if (!dailyPlanId.HasValue)
        {
            var plan = await learningPlanService.EnsurePlanAsync(child, DateTime.Today);
            dailyPlanId = plan.Id;
        }

        var safeTitle = string.IsNullOrWhiteSpace(model.Title) ? "Evidência salva" : model.Title.Trim();
        var safeNotes = model.Notes?.Trim() ?? string.Empty;

        db.LearningSessions.Add(new LearningSession
        {
            ChildId = child.Id,
            DailyPlanId = dailyPlanId.Value,
            LoggedAt = DateTime.UtcNow,
            MinutesCompleted = 0,
            Wins = safeTitle,
            Challenges = string.Empty,
            Notes = safeNotes,
            MediaUrl = uploadedMedia.Value.Url,
            MediaContentType = uploadedMedia.Value.ContentType,
            MediaFileName = uploadedMedia.Value.FileName
        });

        if (IsVideoEvidenceFile(uploadedMedia.Value.ContentType, uploadedMedia.Value.FileName, uploadedMedia.Value.Url))
        {
            await TrySaveEvidenceThumbnailAsync(uploadedMedia.Value.Url, model.ThumbnailDataUrl);
        }

        var parent = await db.Users.FirstAsync(x => x.Id == GetCurrentUserId());
        parent.LastActiveAt = DateTime.UtcNow;
        var evidenceCommit = await TryCommitEvidenceChangesAsync(
            GetCurrentUserId(),
            uploadedMedia is not null,
            uploadedMedia is not null ? [uploadedMedia.Value.Url] : [],
            "O arquivo foi recebido, mas o sistema não conseguiu registrar essa evidência agora. Tente novamente.");

        if (!evidenceCommit.Success)
        {
            TempData["StatusMessage"] = evidenceCommit.Message;
            TempData["StatusKind"] = "warning";
            return RedirectToAction(nameof(Evidences), new { id = model.ChildId });
        }
        TempData["StatusMessage"] = "Evidência salva no acervo da família.";
        TempData["StatusKind"] = "success";

        return RedirectToAction(nameof(Evidences), new { id = model.ChildId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = EvidenceUploadChunkRequestMaxBytes)]
    [RequestSizeLimit(EvidenceUploadChunkRequestMaxBytes)]
    public async Task<IActionResult> UploadEvidenceChunk([FromForm] UploadEvidenceChunkViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return Unauthorized(new { error = "A conta precisa estar ativa para guardar evidências." });
        }

        if (model.Chunk is null || model.Chunk.Length == 0)
        {
            return BadRequest(new { error = "Nenhum trecho do arquivo foi recebido." });
        }

        if (string.IsNullOrWhiteSpace(model.UploadId))
        {
            return BadRequest(new { error = "O identificador do upload não foi informado." });
        }

        if (model.TotalChunks <= 0 || model.ChunkIndex < 0 || model.ChunkIndex >= model.TotalChunks)
        {
            return BadRequest(new { error = "Os dados do envio em partes vieram inválidos." });
        }

        if (model.TotalSize <= 0 || model.TotalSize > ChunkedEvidenceUploadMaxBytes)
        {
            return BadRequest(new { error = $"Envie arquivos com até {ChunkedEvidenceUploadMaxMegabytes} MB no acervo de evidências." });
        }

        if (model.Chunk.Length > EvidenceUploadChunkSizeBytes)
        {
            return BadRequest(new { error = "Um dos trechos do vídeo ficou maior que o permitido para envio." });
        }

        if (!IsAcceptedEvidenceFile(model.FileName, model.ContentType))
        {
            return BadRequest(new { error = "Esse formato não é aceito no acervo. Use foto, vídeo ou documento compatível." });
        }

        var allowance = await evidenceStoragePlanService.BuildAllowanceAsync(GetCurrentUserId());
        if (!allowance.CanUpload)
        {
            return Conflict(new { error = allowance.Message });
        }

        var child = await GetOwnedChildAsync(model.ChildId);
        CleanupExpiredEvidenceChunks();

        var uploadFolder = GetEvidenceChunkFolder(model.UploadId);
        Directory.CreateDirectory(uploadFolder);

        var chunkPath = Path.Combine(uploadFolder, $"{model.ChunkIndex:D5}.part");
        await using (var chunkStream = System.IO.File.Create(chunkPath))
        {
            await model.Chunk.CopyToAsync(chunkStream);
        }

        var receivedChunks = Directory
            .EnumerateFiles(uploadFolder, "*.part", SearchOption.TopDirectoryOnly)
            .Count();

        if (receivedChunks < model.TotalChunks)
        {
            return Json(new
            {
                ok = true,
                completed = false,
                receivedChunks,
                totalChunks = model.TotalChunks
            });
        }

        var uploadedMedia = await SaveChunkedEvidenceFileAsync(uploadFolder, model.FileName, model.ContentType, model.TotalChunks);
        if (uploadedMedia is null)
        {
            DeleteDirectoryIfExists(uploadFolder);
            return BadRequest(new { error = "O sistema não conseguiu montar o arquivo final. Tente enviar novamente." });
        }

        var dailyPlanId = model.DailyPlanId
            ?? await db.DailyPlans
                .Where(x => x.ChildId == child.Id)
                .OrderByDescending(x => x.PlannedDate)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync();

        if (!dailyPlanId.HasValue)
        {
            var plan = await learningPlanService.EnsurePlanAsync(child, DateTime.Today);
            dailyPlanId = plan.Id;
        }

        var safeTitle = string.IsNullOrWhiteSpace(model.Title) ? "Evidência salva" : model.Title.Trim();
        var safeNotes = model.Notes?.Trim() ?? string.Empty;

        db.LearningSessions.Add(new LearningSession
        {
            ChildId = child.Id,
            DailyPlanId = dailyPlanId.Value,
            LoggedAt = DateTime.UtcNow,
            MinutesCompleted = 0,
            Wins = safeTitle,
            Challenges = string.Empty,
            Notes = safeNotes,
            MediaUrl = uploadedMedia.Value.Url,
            MediaContentType = uploadedMedia.Value.ContentType,
            MediaFileName = uploadedMedia.Value.FileName
        });

        if (IsVideoEvidenceFile(uploadedMedia.Value.ContentType, uploadedMedia.Value.FileName, uploadedMedia.Value.Url))
        {
            await TrySaveEvidenceThumbnailAsync(uploadedMedia.Value.Url, model.ThumbnailDataUrl);
        }

        var parent = await db.Users.FirstAsync(x => x.Id == GetCurrentUserId());
        parent.LastActiveAt = DateTime.UtcNow;
        var chunkCommit = await TryCommitEvidenceChangesAsync(
            GetCurrentUserId(),
            uploadedMedia is not null,
            uploadedMedia is not null ? [uploadedMedia.Value.Url] : [],
            "O arquivo foi recebido, mas o sistema não conseguiu finalizar o registro dessa evidência. Tente novamente.");

        if (!chunkCommit.Success)
        {
            DeleteDirectoryIfExists(uploadFolder);
            return StatusCode(chunkCommit.StatusCode, new
            {
                error = chunkCommit.Message
            });
        }

        DeleteDirectoryIfExists(uploadFolder);

        return Json(new
        {
            ok = true,
            completed = true,
            message = "Evidência salva no acervo da família.",
            redirectUrl = Url.Action(nameof(Evidences), new { id = model.ChildId }) ?? $"/Parent/Evidences/{model.ChildId}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEvidenceThumbnail([FromForm] SaveEvidenceThumbnailViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return Unauthorized(new { error = "A conta precisa estar ativa para salvar miniaturas." });
        }

        if (model.ChildId == Guid.Empty || model.SessionId == Guid.Empty || string.IsNullOrWhiteSpace(model.ThumbnailDataUrl))
        {
            return BadRequest(new { error = "Os dados da miniatura vieram incompletos." });
        }

        var session = await db.LearningSessions
            .Include(x => x.Child)
            .FirstOrDefaultAsync(x =>
                x.Id == model.SessionId &&
                x.ChildId == model.ChildId &&
                x.Child.ParentId == GetCurrentUserId());

        if (session is null)
        {
            return NotFound(new { error = "A evidência não foi encontrada para salvar a miniatura." });
        }

        if (!IsVideoEvidenceFile(session.MediaContentType, session.MediaFileName, session.MediaUrl))
        {
            return BadRequest(new { error = "A miniatura automática só vale para vídeo." });
        }

        var thumbnailUrl = await TrySaveEvidenceThumbnailAsync(session.MediaUrl, model.ThumbnailDataUrl);
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            return BadRequest(new { error = "Não foi possível salvar essa miniatura agora." });
        }

        return Json(new
        {
            ok = true,
            thumbnailUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvidence(Guid childId, Guid sessionId, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var session = await db.LearningSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildId == childId && x.Child.ParentId == GetCurrentUserId());

        if (session is null)
        {
            TempData["StatusMessage"] = "A evidência que você tentou excluir não foi encontrada nesta criança.";
            TempData["StatusKind"] = "warning";
            return RedirectToLocalOrAction(returnUrl, nameof(Evidences), new { id = childId });
        }

        var mediaUrls = new[] { session.MediaUrl };
        db.LearningSessions.Remove(session);
        await db.SaveChangesAsync();
        DeleteEvidenceFiles(mediaUrls);

        TempData["StatusMessage"] = "Evidência removida do acervo da família.";
        TempData["StatusKind"] = "success";
        return RedirectToLocalOrAction(returnUrl, nameof(Evidences), new { id = childId });
    }

    [HttpGet]
    public async Task<IActionResult> Evolution(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await childGoalCycleService.SyncCurrentCycleAsync(id, GetCurrentUserId());
        await childRecoveryPlanService.SyncRecoveryPlanAsync(id, GetCurrentUserId());
        await childAchievementService.SyncAchievementsAsync(id, GetCurrentUserId());
        var vm = await childEvolutionService.BuildEvolutionCenterAsync(id, GetCurrentUserId(), Url);
        vm.RecoveryPlan = await childRecoveryPlanService.BuildRecoveryPlanCardAsync(id, GetCurrentUserId(), Url);
        vm.CelebrationBadges = await childAchievementService.BuildAchievementsAsync(id, GetCurrentUserId());
        vm.Consistency = await consistencyService.BuildFamilyConsistencyAsync(GetCurrentUserId(), id);
        vm.AchievementShareUrl = Url.Action(nameof(AchievementShare), new { id }) ?? string.Empty;
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> EvolutionReport(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await childGoalCycleService.SyncCurrentCycleAsync(id, GetCurrentUserId());
        return View(await childEvolutionService.BuildMonthlyReportAsync(id, GetCurrentUserId()));
    }

    [HttpGet]
    public async Task<IActionResult> PremiumPortfolio(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await childGoalCycleService.SyncCurrentCycleAsync(id, GetCurrentUserId());
        await childRecoveryPlanService.SyncRecoveryPlanAsync(id, GetCurrentUserId());
        await childAchievementService.SyncAchievementsAsync(id, GetCurrentUserId());
        return View(await childEvolutionService.BuildPremiumPortfolioAsync(id, GetCurrentUserId()));
    }

    [HttpGet]
    public async Task<IActionResult> AchievementShare(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await GetOwnedChildAsync(id);
        return View(await referralService.BuildAchievementShareCardAsync(GetCurrentUserId(), id, $"{Request.Scheme}://{Request.Host}"));
    }

    [HttpGet]
    public async Task<IActionResult> RecoveryPlan(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await childGoalCycleService.SyncCurrentCycleAsync(id, GetCurrentUserId());
        await childRecoveryPlanService.SyncRecoveryPlanAsync(id, GetCurrentUserId());
        var vm = await childRecoveryPlanService.BuildRecoveryPlanDetailsAsync(id, GetCurrentUserId(), Url);
        if (vm is null)
        {
            return RedirectToAction(nameof(Evolution), new { id });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Curriculum(Guid id, string? area)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildCurriculumViewModelAsync(id, area));
    }

    [HttpGet]
    public async Task<IActionResult> CurriculumPrint(Guid id, string? area)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildCurriculumViewModelAsync(id, area));
    }

    [HttpGet]
    public async Task<IActionResult> TeaProfile(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildTeaProfileViewModelAsync(id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TeaProfile(ChildTeaProfileInputViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(model.ChildId);
        var profile = await db.ChildTeaProfiles.FirstOrDefaultAsync(x => x.ChildId == child.Id);
        if (profile is null)
        {
            profile = new ChildTeaProfile
            {
                ChildId = child.Id
            };
            db.ChildTeaProfiles.Add(profile);
        }

        profile.CommunicationProfile = model.CommunicationProfile;
        profile.CommunicationNotes = model.CommunicationNotes;
        profile.AnxietyLevel = model.AnxietyLevel;
        profile.CognitiveRigidityLevel = model.CognitiveRigidityLevel;
        profile.SensorySensitivityLevel = model.SensorySensitivityLevel;
        profile.TransitionDifficultyLevel = model.TransitionDifficultyLevel;
        profile.SupportIntensityLevel = model.SupportIntensityLevel;
        profile.NeedsVisualRoutine = model.NeedsVisualRoutine;
        profile.NeedsFirstThen = model.NeedsFirstThen;
        profile.NeedsTimer = model.NeedsTimer;
        profile.NeedsPlanB = model.NeedsPlanB;
        profile.SpecialInterests = model.SpecialInterests;
        profile.EffectiveReinforcers = model.EffectiveReinforcers;
        profile.CommonTriggers = model.CommonTriggers;
        profile.OverloadSignals = model.OverloadSignals;
        profile.CalmingStrategies = model.CalmingStrategies;
        profile.TransitionSupports = model.TransitionSupports;
        profile.DailyLivingPriorities = model.DailyLivingPriorities;
        profile.ParentPrimaryGoal = model.ParentPrimaryGoal;
        profile.SchoolBarrierSummary = model.SchoolBarrierSummary;
        profile.DocumentationNotes = model.DocumentationNotes;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        TempData["TeaProfileSaved"] = "true";
        return RedirectToAction(nameof(TeaProfile), new { id = child.Id });
    }

    [HttpGet]
    public async Task<IActionResult> AdaptiveRoutine(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildAdaptiveRoutineViewModelAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> Dossier(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildDossierViewModelAsync(id));
    }

    [HttpGet]
    public IActionResult Academy(Guid? childId = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }
        return RedirectToAction("Index", "Library", new { childId });
    }

    [HttpGet]
    public async Task<IActionResult> ExternalContent(string slug, Guid? childId = null, string? focus = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var parentId = GetCurrentUserId();
        var children = await db.Children
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.FullName)
            .Select(x => new ParentAcademyChildOptionViewModel
            {
                ChildId = x.Id,
                FullName = x.FullName
            })
            .ToListAsync();

        ChildProfile? selectedChild = null;
        if (childId.HasValue)
        {
            selectedChild = await db.Children.FirstOrDefaultAsync(x => x.Id == childId.Value && x.ParentId == parentId);
        }

        var vm = await externalContentHubService.BuildGuideAsync(slug, selectedChild, children, focus, Url);
        if (selectedChild is not null)
        {
            var evidenceAssistSource = TempData["EvidenceAssistSource"]?.ToString();
            var evidenceAssistSlug = TempData["EvidenceAssistSlug"]?.ToString();
            var evidenceAssistFocus = TempData["EvidenceAssistFocus"]?.ToString();
            var externalItem = ExternalContentCatalog.GetRequired(slug);
            var resolvedFocusSlug = !string.IsNullOrWhiteSpace(focus) ? focus : evidenceAssistFocus;
            var resolvedFocus = externalItem.FocusOptions.FirstOrDefault(option =>
                string.Equals(option.Slug, resolvedFocusSlug, StringComparison.OrdinalIgnoreCase));
            var saveActionUrl = $"{Url.Action(nameof(Child), new { id = selectedChild.Id })}#session-log";

            vm.CompletionEvidenceAssistant = evidenceAutomationService.BuildForExternalContent(
                selectedChild,
                externalItem,
                resolvedFocus?.Title ?? vm.SelectedFocusTitle,
                resolvedFocus?.EvidenceIdea ?? vm.SelectedFocusEvidenceIdea,
                vm.EvidenceIdeas,
                vm.IsCompletedForSelectedChild,
                string.Equals(evidenceAssistSource, "external", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(evidenceAssistSlug, slug, StringComparison.OrdinalIgnoreCase),
                saveActionUrl);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkExternalContentComplete(Guid childId, string slug, string? focus = null, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await externalContentHubService.MarkCompletedAsync(childId, GetCurrentUserId(), slug);
        TempData["EvidenceAssistSource"] = "external";
        TempData["EvidenceAssistSlug"] = slug;
        if (!string.IsNullOrWhiteSpace(focus))
        {
            TempData["EvidenceAssistFocus"] = focus;
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Child), new { id = childId });
    }

    [HttpGet]
    public async Task<IActionResult> PortuguesePlanning(Guid? childId = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await portuguesePlanningService.BuildAsync(GetCurrentUserId(), childId));
    }

    [HttpGet]
    public async Task<IActionResult> Favorites(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildFavoritesViewModelAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> TomorrowPreview(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        return View(await BuildTomorrowPreviewViewModelAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> TeaTracks(Guid id)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        if (!SupportsTeaNavigation(child.SupportProfile))
        {
            return RedirectToAction(nameof(Child), new { id });
        }

        return View(await BuildTeaTrackHubViewModelAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> TeaTrack(Guid id, FunctionalSupportTrack track)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        if (!SupportsTeaNavigation(child.SupportProfile) || track == FunctionalSupportTrack.Base)
        {
            return RedirectToAction(nameof(Child), new { id });
        }

        if (!GetAvailableTeaTracks(child.SupportProfile).Contains(track))
        {
            return RedirectToAction(nameof(TeaTracks), new { id });
        }

        return View(await BuildTeaTrackDetailViewModelAsync(id, track));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteActivity(Guid id, FunctionalSupportTrack track, Guid templateId, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        if (!SupportsTeaNavigation(child.SupportProfile) || !GetAvailableTeaTracks(child.SupportProfile).Contains(track))
        {
            return RedirectToAction(nameof(Child), new { id });
        }

        var template = await GetTrackTemplateAsync(child, track, templateId);
        if (template is null)
        {
            SetTeaTrackActionMessage("Nao foi possivel localizar essa atividade da trilha.", "warning");
            return RedirectToLocalOrAction(returnUrl, nameof(TeaTrack), new { id, track });
        }

        var favorite = await db.ChildFavoriteActivities
            .FirstOrDefaultAsync(x => x.ChildId == child.Id && x.TemplateId == template.Id);

        if (favorite is null)
        {
            db.ChildFavoriteActivities.Add(new ChildFavoriteActivity
            {
                ChildId = child.Id,
                TemplateId = template.Id,
                CreatedAt = DateTime.UtcNow
            });
            SetTeaTrackActionMessage($"Atividade \"{template.Title}\" favoritada.", "success");
        }
        else
        {
            db.ChildFavoriteActivities.Remove(favorite);
            SetTeaTrackActionMessage($"Atividade \"{template.Title}\" removida dos favoritos.", "neutral");
        }

        await db.SaveChangesAsync();
        return RedirectToLocalOrAction(returnUrl, nameof(TeaTrack), new { id, track });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QueueTrackActivityForTomorrow(Guid id, FunctionalSupportTrack track, Guid templateId, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        if (!SupportsTeaNavigation(child.SupportProfile) || !GetAvailableTeaTracks(child.SupportProfile).Contains(track))
        {
            return RedirectToAction(nameof(Child), new { id });
        }

        var template = await GetTrackTemplateAsync(child, track, templateId);
        if (template is null)
        {
            SetTeaTrackActionMessage("Nao foi possivel localizar essa atividade da trilha.", "warning");
            return RedirectToLocalOrAction(returnUrl, nameof(TeaTrack), new { id, track });
        }

        var tomorrow = DateTime.Today.AddDays(1).Date;
        var tomorrowPlan = await db.DailyPlans
            .Include(x => x.Blocks)
            .FirstOrDefaultAsync(x => x.ChildId == child.Id && x.PlannedDate == tomorrow);

        var alreadyPlanned = tomorrowPlan?.Blocks.Any(x => x.SourceTemplateId == template.Id) == true;
        if (!alreadyPlanned)
        {
            var existingDirective = await db.ChildPlanDirectives.FirstOrDefaultAsync(x =>
                x.ChildId == child.Id &&
                x.PlannedDate == tomorrow &&
                x.DirectiveType == PlanDirectiveType.PinnedActivity &&
                x.TemplateId == template.Id &&
                !x.AppliedAt.HasValue);

            if (existingDirective is null)
            {
                db.ChildPlanDirectives.Add(new ChildPlanDirective
                {
                    ChildId = child.Id,
                    PlannedDate = tomorrow,
                    DirectiveType = PlanDirectiveType.PinnedActivity,
                    TemplateId = template.Id,
                    FunctionalTrack = template.FunctionalTrack,
                    Note = $"Puxada manualmente da trilha {GetFunctionalTrackLabel(track)}.",
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            await learningPlanService.EnsurePlanAsync(child, tomorrow);
            SetTeaTrackActionMessage($"A atividade \"{template.Title}\" foi puxada para o plano de amanha.", "success");
        }
        else
        {
            SetTeaTrackActionMessage($"A atividade \"{template.Title}\" ja esta no plano de amanha.", "neutral");
        }

        return RedirectToLocalOrAction(returnUrl, nameof(TeaTrack), new { id, track });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateTrackPlanForTomorrow(Guid id, FunctionalSupportTrack track, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(id);
        if (!SupportsTeaNavigation(child.SupportProfile) || !GetAvailableTeaTracks(child.SupportProfile).Contains(track))
        {
            return RedirectToAction(nameof(Child), new { id });
        }

        var tomorrow = DateTime.Today.AddDays(1).Date;
        var pendingTrackDirectives = await db.ChildPlanDirectives
            .Where(x => x.ChildId == child.Id &&
                        x.PlannedDate == tomorrow &&
                        x.DirectiveType == PlanDirectiveType.TrackFocus &&
                        !x.AppliedAt.HasValue)
            .ToListAsync();

        if (pendingTrackDirectives.Count > 0)
        {
            db.ChildPlanDirectives.RemoveRange(pendingTrackDirectives);
        }

        db.ChildPlanDirectives.Add(new ChildPlanDirective
        {
            ChildId = child.Id,
            PlannedDate = tomorrow,
            DirectiveType = PlanDirectiveType.TrackFocus,
            FunctionalTrack = track,
            Note = $"Plano manual focado em {GetFunctionalTrackLabel(track)}.",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        await learningPlanService.EnsurePlanAsync(child, tomorrow);
        SetTeaTrackActionMessage($"O plano de amanha foi regenerado com foco em {GetFunctionalTrackLabel(track).ToLowerInvariant()}.", "success");

        return RedirectToLocalOrAction(returnUrl, nameof(TeaTrack), new { id, track });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitSkillCheckups([Bind(Prefix = "SkillCheckups")] SkillCheckupFormViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await skillCheckupService.ApplyCheckupsAsync(
            model.ChildId,
            GetCurrentUserId(),
            model.Items.Select(x => new SkillCheckupSubmissionItem
            {
                CheckupId = x.CheckupId,
                Rating = x.Rating,
                Notes = x.Notes
            }));
        await skillReadinessService.SyncReadinessChecksAsync(model.ChildId, GetCurrentUserId());

        return RedirectToAction(nameof(Child), new { id = model.ChildId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReadinessChecks([Bind(Prefix = "ReadinessChecks")] SkillReadinessFormViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        await skillReadinessService.ApplyReadinessChecksAsync(
            model.ChildId,
            GetCurrentUserId(),
            model.Items.Select(x => new SkillReadinessSubmissionItem
            {
                ReadinessCheckId = x.ReadinessCheckId,
                Rating = x.Rating,
                Notes = x.Notes
            }));

        return RedirectToAction(nameof(Child), new { id = model.ChildId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePlan(Guid childId, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        var child = await GetOwnedChildAsync(childId);
        await learningPlanService.EnsurePlanAsync(child, DateTime.Today.AddDays(1));
        return RedirectToLocalOrAction(returnUrl, nameof(Child), new { id = childId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = EvidenceUploadMaxBytes)]
    [RequestSizeLimit(EvidenceUploadMaxBytes)]
    public async Task<IActionResult> LogSession(LogSessionViewModel model)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction(nameof(Index), new { gate = "premium" });
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Child), new { id = model.ChildId });
        }

        var child = await GetOwnedChildAsync(model.ChildId);

        var plan = await db.DailyPlans.FirstAsync(x => x.Id == model.DailyPlanId && x.ChildId == model.ChildId);
        var blocks = await db.DailyPlanBlocks
            .Where(x => x.DailyPlanId == model.DailyPlanId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var session = new LearningSession
        {
            ChildId = model.ChildId,
            DailyPlanId = model.DailyPlanId,
            LoggedAt = DateTime.UtcNow,
            MinutesCompleted = model.MinutesCompleted,
            Wins = model.Wins,
            Challenges = model.Challenges,
            Notes = model.Notes
        };

        var uploadedMedia = default((string Url, string ContentType, string FileName)?);
        if (model.EvidenceFile is not null && model.EvidenceFile.Length > 0)
        {
            if (model.EvidenceFile.Length > EvidenceUploadMaxBytes)
            {
                TempData["StatusMessage"] = $"A sessão foi salva, mas o anexo passou de {EvidenceUploadMaxMegabytes} MB e ficou fora do acervo.";
                TempData["StatusKind"] = "warning";
            }
            else
            {
            var allowance = await evidenceStoragePlanService.BuildAllowanceAsync(GetCurrentUserId());
            if (allowance.CanUpload)
            {
                uploadedMedia = await SaveEvidenceFileAsync(model.EvidenceFile);
                if (uploadedMedia is null)
                {
                    TempData["StatusMessage"] = $"A sessão foi salva, mas o arquivo anexado nao entrou no acervo. Use formato aceito e ate {EvidenceUploadMaxMegabytes} MB.";
                    TempData["StatusKind"] = "warning";
                }
            }
            else
            {
                TempData["StatusMessage"] = $"{allowance.Message} A sessão foi registrada, mas o arquivo ficou de fora.";
                TempData["StatusKind"] = "warning";
            }
            }
        }

        if (uploadedMedia is not null)
        {
            session.MediaUrl = uploadedMedia.Value.Url;
            session.MediaContentType = uploadedMedia.Value.ContentType;
            session.MediaFileName = uploadedMedia.Value.FileName;
        }

        db.LearningSessions.Add(session);
        var shouldSaveObservation =
            !string.IsNullOrWhiteSpace(model.Antecedent) ||
            !string.IsNullOrWhiteSpace(model.ChildReaction) ||
            !string.IsNullOrWhiteSpace(model.WhatHelped) ||
            !string.IsNullOrWhiteSpace(model.SupportUsed) ||
            model.NeededPlanB ||
            model.VisualSupportHelped ||
            model.BreakHelped ||
            model.CoRegulationHelped;

        if (shouldSaveObservation)
        {
            db.ChildRoutineObservations.Add(new ChildRoutineObservation
            {
                ChildId = model.ChildId,
                Session = session,
                DailyPlanId = model.DailyPlanId,
                ObservedAt = DateTime.UtcNow,
                ContextPeriod = "Sessao do dia",
                Antecedent = model.Antecedent,
                ChildReaction = model.ChildReaction,
                WhatHelped = model.WhatHelped,
                SupportUsed = model.SupportUsed,
                DistressLevel = model.DistressLevel,
                TaskToleranceMinutes = model.TaskToleranceMinutes,
                NeededPlanB = model.NeededPlanB,
                VisualSupportHelped = model.VisualSupportHelped,
                BreakHelped = model.BreakHelped,
                CoRegulationHelped = model.CoRegulationHelped,
                Notes = model.Notes
            });
        }

        foreach (var feedback in model.BlockFeedbacks)
        {
            var block = blocks.FirstOrDefault(x => x.Id == feedback.DailyPlanBlockId);
            if (block is null || string.IsNullOrWhiteSpace(block.SkillCode))
            {
                continue;
            }

            var rating = Enum.IsDefined(typeof(SkillFeedbackLevel), feedback.Rating)
                ? (SkillFeedbackLevel)feedback.Rating
                : SkillFeedbackLevel.Developing;

            session.BlockFeedbacks.Add(new LearningBlockFeedback
            {
                DailyPlanBlockId = block.Id,
                SkillCode = block.SkillCode,
                Rating = rating,
                Notes = feedback.SkillName
            });

            var progress = await db.ChildSkillProgressEntries
                .FirstOrDefaultAsync(x => x.ChildId == model.ChildId && x.SkillCode == block.SkillCode);

            if (progress is null)
            {
                progress = new ChildSkillProgress
                {
                    ChildId = model.ChildId,
                    Age = plan.AgeAtGeneration,
                    Domain = block.Domain,
                    SupportScope = block.SupportScope,
                    FunctionalTrack = block.FunctionalTrack,
                    SkillCode = block.SkillCode,
                    SkillName = block.SkillName,
                    MasteryScore = 45
                };
                skillProgressionService.InitializeProgress(progress);
                db.ChildSkillProgressEntries.Add(progress);
            }

            progress.SupportScope = block.SupportScope;
            progress.FunctionalTrack = block.FunctionalTrack;

            skillProgressionService.ApplyFeedback(progress, block, rating);
            await adultInterventionService.ApplyInterventionAsync(child, progress, block, rating, model.Challenges);
        }

        plan.Completed = true;

        var parent = await db.Users.FirstAsync(x => x.Id == GetCurrentUserId());
        parent.LastActiveAt = DateTime.UtcNow;
        var sessionCommit = await TryCommitEvidenceChangesAsync(
            GetCurrentUserId(),
            uploadedMedia is not null,
            uploadedMedia is not null ? [uploadedMedia.Value.Url] : [],
            "A sessão foi salva, mas o sistema não conseguiu anexar esse arquivo agora.");

        if (!sessionCommit.Success)
        {
            if (uploadedMedia is not null)
            {
                session.MediaUrl = string.Empty;
                session.MediaContentType = string.Empty;
                session.MediaFileName = string.Empty;

                await db.SaveChangesAsync();
                TempData["StatusMessage"] = $"{sessionCommit.Message} A sessão foi registrada, mas o arquivo ficou de fora.";
                TempData["StatusKind"] = "warning";
            }
            else
            {
                TempData["StatusMessage"] = sessionCommit.Message;
                TempData["StatusKind"] = "warning";
                return RedirectToAction(nameof(Child), new { id = model.ChildId });
            }
        }

        await childGoalCycleService.SyncCurrentCycleAsync(model.ChildId, GetCurrentUserId());
        await childEvolutionService.SyncMonthlySnapshotsAsync(model.ChildId, GetCurrentUserId());
        await childRecoveryPlanService.SyncRecoveryPlanAsync(model.ChildId, GetCurrentUserId());
        await childRecoveryPlanService.AdvanceRecoveryPlanAsync(model.ChildId, GetCurrentUserId());
        await childAchievementService.SyncAchievementsAsync(model.ChildId, GetCurrentUserId());
        await skillReadinessService.SyncReadinessChecksAsync(model.ChildId, GetCurrentUserId());
        await learningPlanService.EnsurePlanAsync(child, DateTime.Today.AddDays(1));
        TempData["EvidenceAssistSource"] = "session";

        return RedirectToAction(nameof(Child), new { id = model.ChildId });
    }

    private async Task<(string Url, string ContentType, string FileName)?> SaveEvidenceFileAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        if (file.Length > EvidenceUploadMaxBytes)
        {
            return null;
        }

        if (!IsAcceptedEvidenceFile(file.FileName, file.ContentType))
        {
            return null;
        }

        await using var sourceStream = file.OpenReadStream();
        return await SaveEvidenceFileStreamAsync(sourceStream, file.FileName, file.ContentType);
    }

    private async Task<(string Url, string ContentType, string FileName)?> SaveChunkedEvidenceFileAsync(
        string uploadFolder,
        string fileName,
        string contentType,
        int totalChunks)
    {
        fileName = string.IsNullOrWhiteSpace(fileName)
            ? BuildFallbackEvidenceFileName(contentType)
            : fileName;

        if (!Directory.Exists(uploadFolder))
        {
            return null;
        }

        if (!IsAcceptedEvidenceFile(fileName, contentType))
        {
            return null;
        }

        var chunkFiles = Enumerable.Range(0, totalChunks)
            .Select(index => Path.Combine(uploadFolder, $"{index:D5}.part"))
            .ToList();

        if (chunkFiles.Any(path => !System.IO.File.Exists(path)))
        {
            return null;
        }

        var combinedPath = Path.Combine(uploadFolder, "combined-upload.bin");
        await using (var combinedStream = System.IO.File.Create(combinedPath))
        {
            foreach (var chunkFile in chunkFiles)
            {
                await using var readStream = System.IO.File.OpenRead(chunkFile);
                await readStream.CopyToAsync(combinedStream);
            }
        }

        var combinedInfo = new FileInfo(combinedPath);
        if (!combinedInfo.Exists || combinedInfo.Length == 0 || combinedInfo.Length > ChunkedEvidenceUploadMaxBytes)
        {
            return null;
        }

        await using var finalReadStream = System.IO.File.OpenRead(combinedPath);
        return await SaveEvidenceFileStreamAsync(finalReadStream, fileName, contentType);
    }

    private async Task<(string Url, string ContentType, string FileName)?> SaveEvidenceFileStreamAsync(
        Stream sourceStream,
        string fileName,
        string? contentType)
    {
        fileName = string.IsNullOrWhiteSpace(fileName)
            ? BuildFallbackEvidenceFileName(contentType)
            : fileName;

        var safeOriginalFileName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeOriginalFileName);
        var normalizedContentType = NormalizeEvidenceContentType(extension, contentType);

        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "session-evidence");
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsFolder, safeFileName);

        await using var stream = System.IO.File.Create(physicalPath);
        await sourceStream.CopyToAsync(stream);

        return ($"/uploads/session-evidence/{safeFileName}", normalizedContentType, safeOriginalFileName);
    }

    private static string BuildFallbackEvidenceFileName(string? contentType)
    {
        var extension = contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            "video/mp4" => ".mp4",
            "video/quicktime" => ".mov",
            "video/webm" => ".webm",
            "video/x-m4v" => ".m4v",
            "video/3gpp" => ".3gp",
            "video/3gpp2" => ".3g2",
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            _ => ".bin"
        };

        return $"arquivo{extension}";
    }

    private async Task<string?> TrySaveEvidenceThumbnailAsync(string mediaUrl, string? thumbnailDataUrl)
    {
        if (!TryResolveEvidencePhysicalPath(mediaUrl, out var mediaPhysicalPath) ||
            string.IsNullOrWhiteSpace(thumbnailDataUrl))
        {
            return null;
        }

        var commaIndex = thumbnailDataUrl.IndexOf(',');
        if (commaIndex <= 0 || commaIndex >= thumbnailDataUrl.Length - 1)
        {
            return null;
        }

        var base64Payload = thumbnailDataUrl[(commaIndex + 1)..];
        byte[] thumbnailBytes;
        try
        {
            thumbnailBytes = Convert.FromBase64String(base64Payload);
        }
        catch
        {
            return null;
        }

        if (thumbnailBytes.Length == 0 || thumbnailBytes.Length > 2 * 1024 * 1024)
        {
            return null;
        }

        var thumbnailPhysicalPath = BuildEvidenceThumbnailPhysicalPath(mediaPhysicalPath);
        await System.IO.File.WriteAllBytesAsync(thumbnailPhysicalPath, thumbnailBytes);
        return BuildEvidenceThumbnailUrl(mediaUrl);
    }

    private static bool IsAcceptedEvidenceFile(string? fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        var hasAcceptedExtension = AllowedEvidenceExtensions.Contains(extension);
        var hasAcceptedContentType =
            !string.IsNullOrWhiteSpace(contentType) &&
            AllowedEvidenceContentTypes.Contains(contentType);

        return hasAcceptedExtension || hasAcceptedContentType;
    }

    private static bool IsVideoContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType) &&
               contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVideoEvidenceFile(string? contentType, string? fileName, string? mediaUrl = null)
    {
        if (IsVideoContentType(contentType))
        {
            return true;
        }

        var extension = Path.GetExtension(!string.IsNullOrWhiteSpace(fileName) ? fileName : mediaUrl ?? string.Empty)
            ?.ToLowerInvariant();

        return extension is ".mp4" or ".mov" or ".m4v" or ".webm" or ".3gp" or ".3g2";
    }

    private static bool IsImageEvidenceFile(string? contentType, string? fileName, string? mediaUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var extension = Path.GetExtension(!string.IsNullOrWhiteSpace(fileName) ? fileName : mediaUrl ?? string.Empty)
            ?.ToLowerInvariant();

        return extension is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".heic" or ".heif";
    }

    private static string NormalizeEvidenceContentType(string? extension, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return contentType;
        }

        return (extension ?? string.Empty).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            ".mp4" or ".m4v" => "video/mp4",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".3gp" => "video/3gpp",
            ".3g2" => "video/3gpp2",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => contentType ?? "application/octet-stream"
        };
    }

    private string GetEvidenceChunkRoot()
    {
        return Path.Combine(environment.ContentRootPath, "App_Data", "evidence-chunks", GetCurrentUserId().ToString("N"));
    }

    private string GetEvidenceChunkFolder(string uploadId)
    {
        return Path.Combine(GetEvidenceChunkRoot(), uploadId);
    }

    private void CleanupExpiredEvidenceChunks()
    {
        var chunkRoot = GetEvidenceChunkRoot();
        if (!Directory.Exists(chunkRoot))
        {
            return;
        }

        foreach (var folder in Directory.GetDirectories(chunkRoot))
        {
            try
            {
                var lastWriteUtc = Directory.GetLastWriteTimeUtc(folder);
                if (lastWriteUtc < DateTime.UtcNow.AddHours(-12))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup issues so they do not block the upload itself.
            }
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        Directory.Delete(path, recursive: true);
    }

    private string GetEvidenceThumbnailUrl(string mediaUrl)
    {
        if (!TryResolveEvidencePhysicalPath(mediaUrl, out var mediaPhysicalPath))
        {
            return string.Empty;
        }

        var thumbnailPhysicalPath = BuildEvidenceThumbnailPhysicalPath(mediaPhysicalPath);
        if (!System.IO.File.Exists(thumbnailPhysicalPath))
        {
            return string.Empty;
        }

        return BuildEvidenceThumbnailUrl(mediaUrl);
    }

    private bool TryResolveEvidencePhysicalPath(string mediaUrl, out string physicalPath)
    {
        physicalPath = string.Empty;
        if (string.IsNullOrWhiteSpace(environment.WebRootPath) ||
            string.IsNullOrWhiteSpace(mediaUrl) ||
            !mediaUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = mediaUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var candidatePath = Path.GetFullPath(Path.Combine(environment.WebRootPath, relativePath));
        var webRoot = Path.GetFullPath(environment.WebRootPath);
        if (!candidatePath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        physicalPath = candidatePath;
        return true;
    }

    private static string BuildEvidenceThumbnailPhysicalPath(string mediaPhysicalPath)
    {
        var directory = Path.GetDirectoryName(mediaPhysicalPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaPhysicalPath);
        return Path.Combine(directory, $"{fileNameWithoutExtension}.thumb.jpg");
    }

    private static string BuildEvidenceThumbnailUrl(string mediaUrl)
    {
        var extension = Path.GetExtension(mediaUrl);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return $"{mediaUrl}.thumb.jpg";
        }

        return mediaUrl[..^extension.Length] + ".thumb.jpg";
    }

    private IActionResult? ResolveLocalReturn(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl!)
            : null;
    }

    private async Task<ChildEvidenceCenterViewModel> BuildEvidenceCenterViewModelAsync(Guid childId, string? searchQuery, string? type, int page = 1)
    {
        const int pageSize = 24;
        var child = await GetOwnedChildAsync(childId);
        var normalizedQuery = (searchQuery ?? string.Empty).Trim();
        var normalizedType = (type ?? string.Empty).Trim().ToLowerInvariant();
        var todayPlanId = await db.DailyPlans
            .Where(x => x.ChildId == child.Id && x.PlannedDate == DateTime.Today)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        var mediaInventory = await db.LearningSessions
            .AsNoTracking()
            .Where(x => x.ChildId == child.Id && x.MediaUrl != "")
            .Select(x => new
            {
                x.MediaContentType,
                x.MediaFileName,
                x.MediaUrl
            })
            .ToListAsync();

        var filteredQuery = db.LearningSessions
            .AsNoTracking()
            .Where(x => x.ChildId == child.Id && x.MediaUrl != "");

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filteredQuery = filteredQuery.Where(x =>
                (x.Wins ?? string.Empty).Contains(normalizedQuery) ||
                (x.Notes ?? string.Empty).Contains(normalizedQuery) ||
                (x.MediaFileName ?? string.Empty).Contains(normalizedQuery));
        }

        filteredQuery = normalizedType switch
        {
            "foto" => filteredQuery.Where(x =>
                (x.MediaContentType ?? string.Empty).StartsWith("image/") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".jpg") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".jpeg") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".png") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".webp") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".gif") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".heic") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".heif") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".jpg") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".jpeg") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".png") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".webp") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".gif") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".heic") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".heif")),
            "video" => filteredQuery.Where(x =>
                (x.MediaContentType ?? string.Empty).StartsWith("video/") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".mp4") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".mov") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".m4v") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".webm") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".3gp") ||
                (x.MediaFileName ?? string.Empty).EndsWith(".3g2") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".mp4") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".mov") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".m4v") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".webm") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".3gp") ||
                (x.MediaUrl ?? string.Empty).EndsWith(".3g2")),
            "documento" => filteredQuery.Where(x =>
                !(x.MediaContentType ?? string.Empty).StartsWith("image/") &&
                !(x.MediaContentType ?? string.Empty).StartsWith("video/") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".jpg") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".jpeg") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".png") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".webp") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".gif") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".heic") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".heif") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".mp4") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".mov") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".m4v") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".webm") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".3gp") &&
                !(x.MediaFileName ?? string.Empty).EndsWith(".3g2") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".jpg") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".jpeg") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".png") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".webp") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".gif") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".heic") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".heif") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".mp4") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".mov") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".m4v") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".webm") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".3gp") &&
                !(x.MediaUrl ?? string.Empty).EndsWith(".3g2")),
            _ => filteredQuery
        };

        var totalMatchingItems = await filteredQuery.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalMatchingItems / (double)pageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);

        var sessions = await filteredQuery
            .OrderByDescending(x => x.LoggedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = sessions
            .Select(session =>
            {
                var resolvedContentType = string.IsNullOrWhiteSpace(session.MediaContentType)
                    ? NormalizeEvidenceContentType(
                        Path.GetExtension(!string.IsNullOrWhiteSpace(session.MediaFileName) ? session.MediaFileName : session.MediaUrl),
                        session.MediaContentType)
                    : session.MediaContentType;

                return new EvidenceAssetViewModel
                {
                    ChildId = child.Id,
                    SessionId = session.Id,
                    LoggedAt = session.LoggedAt,
                    Title = string.IsNullOrWhiteSpace(session.Wins) ? "Evidência salva" : session.Wins,
                    Theme = session.Notes,
                    Notes = session.Notes,
                    MediaUrl = session.MediaUrl,
                    ThumbnailUrl = GetEvidenceThumbnailUrl(session.MediaUrl),
                    MediaContentType = resolvedContentType,
                    FileName = session.MediaFileName
                };
            })
            .ToList();

        var totalItems = mediaInventory.Count;
        var photoCount = mediaInventory.Count(item => IsImageEvidenceFile(item.MediaContentType, item.MediaFileName, item.MediaUrl));
        var videoCount = mediaInventory.Count(item => IsVideoEvidenceFile(item.MediaContentType, item.MediaFileName, item.MediaUrl));
        var documentCount = Math.Max(totalItems - photoCount - videoCount, 0);

        return new ChildEvidenceCenterViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            SearchQuery = normalizedQuery,
            SelectedType = normalizedType,
            TotalItems = totalItems,
            TotalMatchingItems = totalMatchingItems,
            PhotoCount = photoCount,
            VideoCount = videoCount,
            DocumentCount = documentCount,
            PageNumber = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages,
            Storage = await evidenceStoragePlanService.BuildSummaryAsync(GetCurrentUserId()),
            Upload = new UploadEvidenceViewModel
            {
                ChildId = child.Id,
                DailyPlanId = todayPlanId
            },
            Items = items
        };
    }

    private async Task<ChildCurriculumViewModel> BuildCurriculumViewModelAsync(Guid childId, string? area)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .ThenInclude(x => x.DailyPlanBlock)
            .Include(x => x.TaskCompletions)
            .ThenInclude(x => x.DailyPlan)
            .Include(x => x.TaskCompletions)
            .ThenInclude(x => x.DailyPlanBlock)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());
        var weeklyRoadmap = await weeklyRoadmapService.BuildAsync(child, DateTime.Today, Url);

        var sessionPlans = await db.DailyPlans
            .Where(x => x.ChildId == childId)
            .ToDictionaryAsync(x => x.Id, x => x.Theme);

        var skillDomainMap = child.SkillProgressEntries
            .GroupBy(x => x.SkillName)
            .ToDictionary(x => x.Key, x => FormatDomain(x.First().Domain));

        var allSkillProgress = child.SkillProgressEntries
            .OrderByDescending(x => x.MasteryScore)
            .ThenByDescending(x => x.TimesPracticed)
            .Select(progress => new SkillProgressViewModel
            {
                DomainLabel = FormatDomain(progress.Domain),
                SupportSourceLabel = GetSupportScopeLabel(progress.SupportScope),
                SupportSourceChipClass = GetSupportScopeChip(progress.SupportScope),
                FunctionalTrackLabel = GetFunctionalTrackLabel(progress.FunctionalTrack),
                FunctionalTrackChipClass = GetFunctionalTrackChip(progress.FunctionalTrack),
                SkillName = progress.SkillName,
                MasteryScore = progress.MasteryScore,
                TimesPracticed = progress.TimesPracticed,
                Recommendation = progress.Recommendation,
                StatusLabel = GetSkillStatusLabel(progress.MasteryScore),
                StatusChipClass = GetSkillStatusChip(progress.MasteryScore)
            })
            .ToList();

        var sessionCoveredBlockIds = child.LearningSessions
            .SelectMany(x => x.BlockFeedbacks)
            .Select(x => x.DailyPlanBlockId)
            .ToHashSet();

        var completionEntries = child.TaskCompletions
            .Where(x => !sessionCoveredBlockIds.Contains(x.DailyPlanBlockId))
            .OrderByDescending(x => x.CompletedAt)
            .Select(completion => BuildCurriculumEntryFromCompletion(completion, sessionPlans))
            .ToList();

        var allEntries = child.LearningSessions
            .OrderByDescending(x => x.LoggedAt)
            .Select(session => BuildCurriculumEntryViewModel(session, sessionPlans, skillDomainMap))
            .Concat(completionEntries)
            .OrderByDescending(x => x.LoggedAt)
            .ToList();

        var availableAreas = allSkillProgress
            .Select(x => x.DomainLabel)
            .Concat(allEntries.SelectMany(x => x.Skills.Select(s => s.DomainLabel)))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var selectedArea = string.IsNullOrWhiteSpace(area) ? string.Empty : area;
        var filteredSkills = string.IsNullOrWhiteSpace(selectedArea)
            ? allSkillProgress
            : allSkillProgress.Where(x => string.Equals(x.DomainLabel, selectedArea, StringComparison.OrdinalIgnoreCase)).ToList();

        var filteredEntries = string.IsNullOrWhiteSpace(selectedArea)
            ? allEntries
            : allEntries.Where(x => x.Skills.Any(s => string.Equals(s.DomainLabel, selectedArea, StringComparison.OrdinalIgnoreCase))).ToList();
        var annualPlan = await annualCurriculumPlannerService.BuildAsync(child, allSkillProgress, allEntries, weeklyRoadmap);
        var systemCurriculumTracks = await systemCurriculumLibraryService.BuildAsync(child);
        var systemTrackMap = systemCurriculumTracks.ToDictionary(track => track.DomainLabel, StringComparer.OrdinalIgnoreCase);
        var librarySupportMap = await familyLibraryService.BuildSubjectSupportMapAsync(GetCurrentUserId(), child, Url);
        var annualReadingSpine = await familyLibraryService.BuildAnnualReadingSpineAsync(GetCurrentUserId(), child, Url);
        foreach (var subject in annualPlan.Subjects)
        {
            if (systemTrackMap.TryGetValue(subject.Title, out var systemTrack) && systemTrack.CurrentUnit is not null)
            {
                subject.CurrentGoal = $"{systemTrack.CurrentUnit.UnitLabel} • {systemTrack.CurrentUnit.Title}";
                if (systemTrack.CurrentUnit.LessonTitles.Count > 0)
                {
                    subject.Milestones = systemTrack.CurrentUnit.LessonTitles.Take(3).ToList();
                }
            }

            if (!librarySupportMap.TryGetValue(subject.Title, out var support))
            {
                continue;
            }

            subject.RecommendedBook = support.RecommendedBook;
            subject.RecommendedPrintable = support.RecommendedPrintable;
        }

        return new ChildCurriculumViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            TeachingMethodologyLabel = GetTeachingMethodologyLabel(child.TeachingMethodology),
            LearningProfileLabel = GetLearningProfileLabel(child.LearningProfile),
            GuidanceStyleLabel = GetGuidanceStyleLabel(child.GuidanceStyle),
            TotalSessions = allEntries.Count,
            TotalMinutes = allEntries.Sum(x => x.MinutesCompleted),
            EvidenceCount = child.LearningSessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl)),
            SelectedArea = selectedArea,
            AvailableAreas = availableAreas,
            PrintUrl = Url.Action(nameof(CurriculumPrint), new { id = child.Id, area = selectedArea }) ?? string.Empty,
            EvidenceCenterUrl = Url.Action(nameof(Evidences), new { id = child.Id }) ?? string.Empty,
            AcademyUrl = Url.Action(nameof(Academy), new { childId = child.Id }) ?? string.Empty,
            HasTeaTracks = SupportsTeaNavigation(child.SupportProfile),
            TeaTracksUrl = Url.Action(nameof(TeaTracks), new { id = child.Id }) ?? string.Empty,
            TeaTrackQuickLinks = BuildTeaTrackQuickLinks(child.Id, child.SupportProfile),
            AnnualPlan = annualPlan,
            AnnualReadingSpine = annualReadingSpine,
            LibraryBridge = await familyLibraryService.BuildCurriculumBridgeAsync(GetCurrentUserId(), child, [], Url),
            SystemCurriculumTracks = systemCurriculumTracks,
            WeeklyRoadmap = weeklyRoadmap,
            SkillProgress = filteredSkills,
            Entries = filteredEntries
        };
    }

    private async Task<ChildTeaProfileViewModel> BuildTeaProfileViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.TeaProfile)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var snapshot = await adaptiveRoutineService.BuildSnapshotAsync(child.Id, DateTime.Today);
        return new ChildTeaProfileViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            TeaTracksUrl = Url.Action(nameof(TeaTracks), new { id = child.Id }) ?? string.Empty,
            AdaptiveRoutineUrl = Url.Action(nameof(AdaptiveRoutine), new { id = child.Id }) ?? string.Empty,
            DossierUrl = Url.Action(nameof(Dossier), new { id = child.Id }) ?? string.Empty,
            Snapshot = MapAdaptiveSnapshot(snapshot),
            Insights = BuildTeaProfileInsights(child.TeaProfile, child.SupportProfile),
            Form = MapTeaProfileInput(child.TeaProfile, child.Id)
        };
    }

    private async Task<ChildAdaptiveRoutineViewModel> BuildAdaptiveRoutineViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.TeaProfile)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var plan = await learningPlanService.EnsurePlanAsync(child, DateTime.Today);
        var todayPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .FirstAsync(x => x.Id == plan.Id);
        var snapshot = await adaptiveRoutineService.BuildSnapshotAsync(child.Id, DateTime.Today);
        var observations = await adaptiveRoutineService.GetRecentObservationsAsync(child.Id);

        return new ChildAdaptiveRoutineViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            TeaProfileUrl = Url.Action(nameof(TeaProfile), new { id = child.Id }) ?? string.Empty,
            DossierUrl = Url.Action(nameof(Dossier), new { id = child.Id }) ?? string.Empty,
            Snapshot = MapAdaptiveSnapshot(snapshot),
            Blocks = todayPlan.Blocks
                .Select((block, index) => new AdaptiveOperationalBlockViewModel
                {
                    Title = block.Title,
                    TrackLabel = GetFunctionalTrackLabel(block.FunctionalTrack),
                    TrackChipClass = GetFunctionalTrackChip(block.FunctionalTrack),
                    PlannedMinutes = block.DurationMinutes,
                    SuggestedTimerMinutes = Math.Min(block.DurationMinutes, snapshot.WorkBlockMinutes),
                    SupportCue = BuildAdaptiveBlockCue(block, snapshot, index),
                    PlanB = snapshot.PlanBRecommendation,
                    IsSensitiveTransition = index > 0 && snapshot.Intensity != AdaptiveSupportIntensity.Light
                })
                .ToList(),
            Observations = observations.Select(MapObservation).ToList()
        };
    }

    private async Task<ChildDossierViewModel> BuildDossierViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.TeaProfile)
            .Include(x => x.LearningSessions)
            .Include(x => x.SkillProgressEntries)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var sessionPlans = await db.DailyPlans
            .Where(x => x.ChildId == childId)
            .ToDictionaryAsync(x => x.Id, x => x.Theme);
        var snapshot = await adaptiveRoutineService.BuildSnapshotAsync(child.Id, DateTime.Today);
        var observations = await adaptiveRoutineService.GetRecentObservationsAsync(child.Id);
        var weekly = BuildWeeklyChildReport(child);

        var prioritySkills = child.SkillProgressEntries
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .Take(8)
            .Select(progress => new SkillProgressViewModel
            {
                DomainLabel = FormatDomain(progress.Domain),
                SupportSourceLabel = GetSupportScopeLabel(progress.SupportScope),
                SupportSourceChipClass = GetSupportScopeChip(progress.SupportScope),
                FunctionalTrackLabel = GetFunctionalTrackLabel(progress.FunctionalTrack),
                FunctionalTrackChipClass = GetFunctionalTrackChip(progress.FunctionalTrack),
                SkillName = progress.SkillName,
                SkillStageLabel = skillProgressionService.GetStageLabel(progress.SkillStage),
                SkillStageChipClass = skillProgressionService.GetStageChip(progress.SkillStage, progress.NeedsRemediation, progress.ReadyToAdvance),
                MasteryScore = progress.MasteryScore,
                TimesPracticed = progress.TimesPracticed,
                ReadyToAdvance = progress.ReadyToAdvance,
                NeedsReadinessCheck = progress.NeedsReadinessCheck,
                ReadinessApproved = progress.ReadinessApproved,
                NeedsRemediation = progress.NeedsRemediation,
                NextReviewAt = progress.NextReviewAt,
                ReadinessStatusLabel = skillProgressionService.GetReadinessStatusLabel(progress),
                NextMilestone = progress.NextMilestone,
                RemediationPlan = progress.RemediationPlan,
                Recommendation = progress.Recommendation,
                StatusLabel = GetSkillStatusLabel(progress.MasteryScore),
                StatusChipClass = GetSkillStatusChip(progress.MasteryScore)
            })
            .ToList();

        return new ChildDossierViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = CalculateAge(child.BirthDate, DateTime.Today),
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            TeaProfileUrl = Url.Action(nameof(TeaProfile), new { id = child.Id }) ?? string.Empty,
            AdaptiveRoutineUrl = Url.Action(nameof(AdaptiveRoutine), new { id = child.Id }) ?? string.Empty,
            ParentPrimaryGoal = child.TeaProfile?.ParentPrimaryGoal ?? "Ainda nao definido.",
            SchoolBarrierSummary = child.TeaProfile?.SchoolBarrierSummary ?? "Ainda nao registrado.",
            DocumentationSummary = BuildDocumentationSummary(child, snapshot, observations.Count),
            TotalSessions = child.LearningSessions.Count,
            TotalMinutes = child.LearningSessions.Sum(x => x.MinutesCompleted),
            EvidenceCount = child.LearningSessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl)),
            ObservationCount = observations.Count,
            Snapshot = MapAdaptiveSnapshot(snapshot),
            PrioritySkills = prioritySkills,
            EvidenceTimeline = child.LearningSessions
                .OrderByDescending(x => x.LoggedAt)
                .Take(12)
                .Select(session => new DossierEvidenceViewModel
                {
                    LoggedAt = session.LoggedAt,
                    Theme = sessionPlans.TryGetValue(session.DailyPlanId, out var theme) ? theme : "Sessao registrada",
                    Summary = $"Foi bem em {session.Wins}. Revisar {session.Challenges}.",
                    MediaUrl = session.MediaUrl,
                    MediaContentType = session.MediaContentType
                })
                .ToList(),
            RecommendedDocuments = BuildRecommendedDocuments(),
            WeeklyImproved = weekly.ImprovedSkill,
            WeeklyReview = string.IsNullOrWhiteSpace(weekly.ReviewSkill) ? "Nenhum alerta forte nesta semana." : weekly.ReviewSkill,
            WeeklyAdvance = string.IsNullOrWhiteSpace(weekly.AdvanceSkill) ? "Ainda nao ha habilidade pronta para avancar." : weekly.AdvanceSkill
        };
    }

    private async Task<ChildFavoritesViewModel> BuildFavoritesViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var age = CalculateAge(child.BirthDate, DateTime.Today);
        age = Math.Clamp(age, 3, 14);

        var tomorrow = DateTime.Today.AddDays(1).Date;
        var tomorrowPlan = await db.DailyPlans
            .Include(x => x.Blocks)
            .FirstOrDefaultAsync(x => x.ChildId == childId && x.PlannedDate == tomorrow);
        var tomorrowDirectives = await db.ChildPlanDirectives
            .Where(x => x.ChildId == childId && x.PlannedDate == tomorrow)
            .ToListAsync();
        var queuedTemplateIds = tomorrowDirectives
            .Where(x => x.DirectiveType == PlanDirectiveType.PinnedActivity && x.TemplateId.HasValue)
            .Select(x => x.TemplateId!.Value)
            .Concat(tomorrowPlan?.Blocks.Where(x => x.SourceTemplateId.HasValue).Select(x => x.SourceTemplateId!.Value) ?? [])
            .ToHashSet();

        var favorites = await db.ChildFavoriteActivities
            .Include(x => x.Template)
            .Where(x => x.ChildId == childId)
            .OrderBy(x => x.Template.FunctionalTrack)
            .ThenBy(x => x.Template.SortOrder)
            .ToListAsync();

        var progressMap = child.SkillProgressEntries
            .ToDictionary(x => x.SkillCode, x => x, StringComparer.OrdinalIgnoreCase);

        var groups = favorites
            .GroupBy(x => x.Template.FunctionalTrack)
            .OrderBy(x => GetTrackDisplayOrder(x.Key))
            .Select(group => new FavoriteTrackGroupViewModel
            {
                Track = group.Key,
                TrackLabel = GetFunctionalTrackLabel(group.Key),
                TrackChipClass = GetFunctionalTrackChip(group.Key),
                SupportSummary = GetTeaTrackSummary(group.Key, child.SupportProfile),
                TrackUrl = group.Key != FunctionalSupportTrack.Base && SupportsTeaNavigation(child.SupportProfile)
                    ? Url.Action(nameof(TeaTrack), new { id = child.Id, track = group.Key }) ?? string.Empty
                    : string.Empty,
                Activities = group
                    .Select(favorite =>
                    {
                        progressMap.TryGetValue(favorite.Template.SkillCode, out var progress);
                        return new FavoriteActivityViewModel
                        {
                            TemplateId = favorite.TemplateId,
                            Track = favorite.Template.FunctionalTrack,
                            TrackLabel = GetFunctionalTrackLabel(favorite.Template.FunctionalTrack),
                            TrackChipClass = GetFunctionalTrackChip(favorite.Template.FunctionalTrack),
                            SupportSourceLabel = GetSupportScopeLabel(favorite.Template.SupportScope),
                            SupportSourceChipClass = GetSupportScopeChip(favorite.Template.SupportScope),
                            DomainLabel = FormatDomain(favorite.Template.Domain),
                            Title = favorite.Template.Title,
                            SkillName = favorite.Template.SkillName,
                            Goal = favorite.Template.Goal,
                            ParentGuide = favorite.Template.ParentGuide,
                            ChildMission = favorite.Template.ChildMission,
                            Materials = favorite.Template.Materials,
                            EvidencePrompt = favorite.Template.EvidencePrompt,
                            ProgressPercent = progress?.MasteryScore ?? 0,
                            RecommendationLabel = GetTeaTrackActivityRecommendationLabel(progress, false),
                            RecommendationChipClass = GetTeaTrackActivityRecommendationChip(progress, false),
                            IsQueuedForTomorrow = queuedTemplateIds.Contains(favorite.TemplateId),
                            TrackUrl = favorite.Template.FunctionalTrack != FunctionalSupportTrack.Base && SupportsTeaNavigation(child.SupportProfile)
                                ? Url.Action(nameof(TeaTrack), new { id = child.Id, track = favorite.Template.FunctionalTrack }) ?? string.Empty
                                : string.Empty
                        };
                    })
                    .OrderByDescending(x => x.IsQueuedForTomorrow)
                    .ThenByDescending(x => x.ProgressPercent)
                    .ThenBy(x => x.Title)
                    .ToList()
            })
            .ToList();

        return new ChildFavoritesViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = age,
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            TeachingMethodologyLabel = GetTeachingMethodologyLabel(child.TeachingMethodology),
            LearningProfileLabel = GetLearningProfileLabel(child.LearningProfile),
            GuidanceStyleLabel = GetGuidanceStyleLabel(child.GuidanceStyle),
            HasTeaTracks = SupportsTeaNavigation(child.SupportProfile),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            TeaTracksUrl = Url.Action(nameof(TeaTracks), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            TomorrowPreviewUrl = Url.Action(nameof(TomorrowPreview), new { id = child.Id }) ?? string.Empty,
            FavoriteCount = favorites.Count,
            QueuedForTomorrowCount = queuedTemplateIds.Count,
            ActionMessage = TempData["TeaTrackActionMessage"]?.ToString() ?? string.Empty,
            ActionMessageStyle = TempData["TeaTrackActionStyle"]?.ToString() ?? "neutral",
            Groups = groups
        };
    }

    private async Task<TomorrowPlanPreviewViewModel> BuildTomorrowPreviewViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var tomorrow = DateTime.Today.AddDays(1).Date;
        var age = CalculateAge(child.BirthDate, tomorrow);
        age = Math.Clamp(age, 3, 14);

        await learningPlanService.EnsurePlanAsync(child, tomorrow);

        var tomorrowPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .ThenInclude(x => x.SourceTemplate)
            .FirstAsync(x => x.ChildId == childId && x.PlannedDate == tomorrow);
        var tomorrowDirectives = await db.ChildPlanDirectives
            .Include(x => x.Template)
            .Where(x => x.ChildId == childId && x.PlannedDate == tomorrow)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        var favoriteTemplateIds = await db.ChildFavoriteActivities
            .Where(x => x.ChildId == childId)
            .Select(x => x.TemplateId)
            .ToListAsync();
        var favoriteTemplateSet = favoriteTemplateIds.ToHashSet();
        var pinnedTemplateIds = tomorrowDirectives
            .Where(x => x.DirectiveType == PlanDirectiveType.PinnedActivity && x.TemplateId.HasValue)
            .Select(x => x.TemplateId!.Value)
            .ToHashSet();
        var focusDirective = tomorrowDirectives
            .Where(x => x.DirectiveType == PlanDirectiveType.TrackFocus && x.FunctionalTrack.HasValue)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        var progressLookup = child.SkillProgressEntries
            .ToDictionary(x => x.SkillCode, x => x, StringComparer.OrdinalIgnoreCase);
        var systemCurriculumTracks = await systemCurriculumLibraryService.BuildAsync(child);
        var tomorrowSuggestions = await curatedLearningLibraryService.BuildBlockSuggestionsAsync(child, tomorrowPlan.Blocks);
        var tomorrowPrintableSuggestions = await familyLibraryService.BuildBlockPrintableMapAsync(GetCurrentUserId(), child, tomorrowPlan.Blocks.ToList(), Url, tomorrowSuggestions);
        var pinnedActivities = tomorrowPlan.Blocks
            .Where(x => x.SourceTemplateId.HasValue && pinnedTemplateIds.Contains(x.SourceTemplateId.Value))
            .Select(x => x.SourceTemplate?.Title ?? x.Title)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var blocks = tomorrowPlan.Blocks
            .OrderBy(x => x.SortOrder)
            .Select(block => BuildTomorrowPreviewBlockViewModel(
                child.Id,
                child.SupportProfile,
                block,
                progressLookup,
                favoriteTemplateSet,
                pinnedTemplateIds,
                focusDirective?.FunctionalTrack,
                systemCurriculumTracks,
                tomorrowPrintableSuggestions,
                tomorrowSuggestions))
            .ToList();

        var directives = new List<TomorrowDirectiveViewModel>();
        if (focusDirective?.FunctionalTrack is { } focusTrack)
        {
            directives.Add(new TomorrowDirectiveViewModel
            {
                Label = $"Foco manual em {GetFunctionalTrackLabel(focusTrack)}",
                ChipClass = GetFunctionalTrackChip(focusTrack)
            });
        }

        foreach (var pinned in tomorrowDirectives
                     .Where(x => x.DirectiveType == PlanDirectiveType.PinnedActivity && x.Template is not null)
                     .Select(x => x.Template!.Title)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            directives.Add(new TomorrowDirectiveViewModel
            {
                Label = $"Atividade puxada: {pinned}",
                ChipClass = "neutral"
            });
        }

        if (directives.Count == 0)
        {
            directives.Add(new TomorrowDirectiveViewModel
            {
                Label = "Plano montado pelo motor automatico da plataforma",
                ChipClass = "neutral"
            });
        }

        return new TomorrowPlanPreviewViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = age,
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            TeachingMethodologyLabel = GetTeachingMethodologyLabel(child.TeachingMethodology),
            LearningProfileLabel = GetLearningProfileLabel(child.LearningProfile),
            GuidanceStyleLabel = GetGuidanceStyleLabel(child.GuidanceStyle),
            DateLabel = tomorrow.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("pt-BR")),
            Theme = tomorrowPlan.Theme,
            ParentSummary = tomorrowPlan.ParentSummary,
            ChildNarrative = tomorrowPlan.ChildNarrative,
            IsRecoveryPlan = tomorrowPlan.IsRecoveryPlan,
            RecoveryHeadline = tomorrowPlan.RecoveryHeadline,
            Summary = BuildTomorrowPlanSummary(tomorrowPlan, pinnedActivities, focusDirective?.FunctionalTrack),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            FavoritesUrl = Url.Action(nameof(Favorites), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            HasTeaTracks = SupportsTeaNavigation(child.SupportProfile),
            TeaTracksUrl = Url.Action(nameof(TeaTracks), new { id = child.Id }) ?? string.Empty,
            TeaTrackQuickLinks = BuildTeaTrackQuickLinks(child.Id, child.SupportProfile),
            TotalMinutes = tomorrowPlan.Blocks.Sum(x => x.DurationMinutes),
            BlockCount = tomorrowPlan.Blocks.Count,
            ActionMessage = TempData["TeaTrackActionMessage"]?.ToString() ?? string.Empty,
            ActionMessageStyle = TempData["TeaTrackActionStyle"]?.ToString() ?? "neutral",
            PinnedActivities = pinnedActivities,
            Directives = directives,
            Blocks = blocks
        };
    }

    private TomorrowPlanPreviewBlockViewModel BuildTomorrowPreviewBlockViewModel(
        Guid childId,
        SupportProfile supportProfile,
        DailyPlanBlock block,
        IReadOnlyDictionary<string, ChildSkillProgress> progressLookup,
        IReadOnlySet<Guid> favoriteTemplateIds,
        IReadOnlySet<Guid> pinnedTemplateIds,
        FunctionalSupportTrack? focusedTrack,
        IReadOnlyCollection<SystemCurriculumTrackViewModel> systemTracks,
        IReadOnlyDictionary<Guid, FamilyLibraryRecommendationViewModel> printableMap,
        IReadOnlyDictionary<Guid, CuratedTaskSuggestionViewModel> suggestions)
    {
        var blockViewModel = BuildTrackPlanBlockViewModel(block, progressLookup);
        var subjectLabel = FormatDomain(block.Domain);
        var currentTrack = systemTracks.FirstOrDefault(track =>
            string.Equals(track.DomainLabel, subjectLabel, StringComparison.OrdinalIgnoreCase));
        var currentUnit = currentTrack?.CurrentUnit;
        var printable = printableMap.TryGetValue(block.Id, out var printableMatch) ? printableMatch : null;
        var suggestion = suggestions.TryGetValue(block.Id, out var task) ? task : null;
        var lessonPacket = suggestion?.LessonPacket;
        var isFavorite = block.SourceTemplateId.HasValue && favoriteTemplateIds.Contains(block.SourceTemplateId.Value);
        var isPinnedActivity = block.SourceTemplateId.HasValue && pinnedTemplateIds.Contains(block.SourceTemplateId.Value);
        var isManualTrackFocus = focusedTrack.HasValue && block.FunctionalTrack == focusedTrack.Value;

        var originLabel = isPinnedActivity
            ? "Favorito puxado para amanha"
            : isManualTrackFocus
                ? "Bloco reforcado pelo foco manual"
                : "Bloco do motor automatico";
        var originChipClass = isPinnedActivity
            ? "success"
            : isManualTrackFocus && block.FunctionalTrack != FunctionalSupportTrack.Base
                ? GetFunctionalTrackChip(block.FunctionalTrack)
                : "neutral";

        return new TomorrowPlanPreviewBlockViewModel
        {
            Id = block.Id,
            Title = suggestion?.Title ?? currentUnit?.TaskTitle ?? blockViewModel.Title,
            DomainLabel = blockViewModel.DomainLabel,
            SupportSourceLabel = blockViewModel.SupportSourceLabel,
            SupportSourceChipClass = blockViewModel.SupportSourceChipClass,
            FunctionalTrackLabel = blockViewModel.FunctionalTrackLabel,
            FunctionalTrackChipClass = blockViewModel.FunctionalTrackChipClass,
            SkillName = blockViewModel.SkillName,
            FocusLabel = blockViewModel.FocusLabel,
            FocusChipClass = blockViewModel.FocusChipClass,
            Goal = suggestion?.LessonPacket?.PracticeTask ?? currentUnit?.TaskPrompt ?? blockViewModel.Goal,
            ParentGuide = suggestion?.LessonPacket?.OpeningForAdult ?? currentUnit?.ParentGuide ?? blockViewModel.ParentGuide,
            ChildPrompt = suggestion?.LessonPacket?.AnchorQuestion ?? currentUnit?.TaskTitle ?? blockViewModel.ChildPrompt,
            Materials = blockViewModel.Materials,
            EvidencePrompt = suggestion?.LessonPacket?.CompletionDefinition ?? currentUnit?.CompletionSignal ?? blockViewModel.EvidencePrompt,
            DurationMinutes = blockViewModel.DurationMinutes,
            IsFavorite = isFavorite,
            IsPinnedActivity = isPinnedActivity,
            IsManualTrackFocus = isManualTrackFocus,
            OriginLabel = originLabel,
            OriginChipClass = originChipClass,
            CurrentSystemUnit = currentUnit,
            RecommendedPrintable = printable,
            CurriculumOriginSummary = lessonPacket is not null
                ? $"Dentro do currículo anual, a lição de amanhã trabalha {lessonPacket.CurriculumPlacement.ToLowerInvariant()} e pratica {lessonPacket.UnitTitle.ToLowerInvariant()}."
                : currentUnit is null
                    ? string.Empty
                    : $"{subjectLabel} continua em {currentUnit.UnitLabel.ToLowerInvariant()} do ano e a lição de amanhã pratica {currentUnit.Title.ToLowerInvariant()}.",
            PrintableReason = printable is null
                ? string.Empty
                : !string.IsNullOrWhiteSpace(printable.FitReason)
                    ? printable.FitReason
                    : lessonPacket is not null
                        ? $"Essa folha entra porque ajuda a praticar {lessonPacket.UnitTitle.ToLowerInvariant()} com apoio em papel."
                        : string.Empty,
            LessonPacket = lessonPacket,
            TrackUrl = block.FunctionalTrack != FunctionalSupportTrack.Base && SupportsTeaNavigation(supportProfile)
                ? Url.Action(nameof(TeaTrack), new { id = childId, track = block.FunctionalTrack }) ?? string.Empty
                : string.Empty
        };
    }

    private async Task<TeaTrackHubViewModel> BuildTeaTrackHubViewModelAsync(Guid childId)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .ThenInclude(x => x.DailyPlanBlock)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var age = CalculateAge(child.BirthDate, DateTime.Today);
        age = Math.Clamp(age, 3, 14);
        var tracks = GetAvailableTeaTracks(child.SupportProfile);
        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .Where(x => tracks.Contains(x.FunctionalTrack))
            .ToListAsync();
        templates = templates
            .Where(x => IsTemplateAllowedForProfile(x.SupportScope, child.SupportProfile))
            .ToList();

        var trackCards = tracks
            .Select(track =>
            {
                var trackProgress = child.SkillProgressEntries
                    .Where(x => x.FunctionalTrack == track)
                    .ToList();
                var trackTemplates = templates
                    .Where(x => x.FunctionalTrack == track)
                    .ToList();
                var totalSkills = trackTemplates.Select(x => x.SkillCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                if (totalSkills == 0)
                {
                    totalSkills = trackProgress.Select(x => x.SkillCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                }

                var practicedSkills = trackProgress.Select(x => x.SkillCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                var strongSkills = trackProgress.Count(x => x.MasteryScore >= 75);
                var weakSkills = trackProgress.Count(x => x.MasteryScore < 45);
                var progressPercent = trackProgress.Count == 0
                    ? 0
                    : (int)Math.Round(trackProgress.Average(x => x.MasteryScore));

                return new TeaTrackCardViewModel
                {
                    Track = track,
                    Label = GetFunctionalTrackLabel(track),
                    ChipClass = GetFunctionalTrackChip(track),
                    Summary = GetTeaTrackSummary(track, child.SupportProfile),
                    Description = GetTeaTrackDescription(track, child.SupportProfile),
                    TotalSkills = totalSkills,
                    PracticedSkills = practicedSkills,
                    StrongSkills = strongSkills,
                    WeakSkills = weakSkills,
                    ProgressPercent = progressPercent,
                    StatusLabel = GetTeaTrackStatusLabel(practicedSkills, weakSkills, strongSkills),
                    StatusChipClass = GetTeaTrackStatusChip(practicedSkills, weakSkills, strongSkills),
                    NextAction = BuildTeaTrackRecommendation(trackProgress, trackTemplates, track),
                    Url = Url.Action(nameof(TeaTrack), new { id = child.Id, track }) ?? string.Empty
                };
            })
            .ToList();

        return new TeaTrackHubViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = age,
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            TeachingMethodologyLabel = GetTeachingMethodologyLabel(child.TeachingMethodology),
            LearningProfileLabel = GetLearningProfileLabel(child.LearningProfile),
            GuidanceStyleLabel = GetGuidanceStyleLabel(child.GuidanceStyle),
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            Tracks = trackCards
        };
    }

    private async Task<TeaTrackDetailViewModel> BuildTeaTrackDetailViewModelAsync(Guid childId, FunctionalSupportTrack track)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.LearningSessions)
            .ThenInclude(x => x.BlockFeedbacks)
            .ThenInclude(x => x.DailyPlanBlock)
            .FirstAsync(x => x.Id == childId && x.ParentId == GetCurrentUserId());

        var age = CalculateAge(child.BirthDate, DateTime.Today);
        age = Math.Clamp(age, 3, 14);

        await learningPlanService.EnsurePlanAsync(child, DateTime.Today);
        var todayPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .FirstAsync(x => x.ChildId == childId && x.PlannedDate == DateTime.Today);

        var sessionPlans = await db.DailyPlans
            .Where(x => x.ChildId == childId)
            .ToDictionaryAsync(x => x.Id, x => x.Theme);

        var skillDomainMap = child.SkillProgressEntries
            .GroupBy(x => x.SkillName)
            .ToDictionary(x => x.Key, x => FormatDomain(x.First().Domain));

        var trackTemplates = await db.CurriculumTemplates
            .Where(x => x.Age == age)
            .Where(x => x.FunctionalTrack == track)
            .ToListAsync();
        trackTemplates = trackTemplates
            .Where(x => IsTemplateAllowedForProfile(x.SupportScope, child.SupportProfile))
            .ToList();
        var favoriteTemplateIds = await db.ChildFavoriteActivities
            .Where(x => x.ChildId == childId)
            .Select(x => x.TemplateId)
            .ToListAsync();
        var favoriteTemplateSet = favoriteTemplateIds.ToHashSet();
        var tomorrow = DateTime.Today.AddDays(1).Date;
        var tomorrowPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .FirstOrDefaultAsync(x => x.ChildId == childId && x.PlannedDate == tomorrow);
        var tomorrowDirectives = await db.ChildPlanDirectives
            .Where(x => x.ChildId == childId && x.PlannedDate == tomorrow)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        var queuedTemplateSet = tomorrowDirectives
            .Where(x => x.DirectiveType == PlanDirectiveType.PinnedActivity && x.TemplateId.HasValue)
            .Select(x => x.TemplateId!.Value)
            .Concat(tomorrowPlan?.Blocks.Where(x => x.SourceTemplateId.HasValue).Select(x => x.SourceTemplateId!.Value) ?? [])
            .ToHashSet();
        var tomorrowTrackFocus = tomorrowDirectives
            .Where(x => x.DirectiveType == PlanDirectiveType.TrackFocus && x.FunctionalTrack.HasValue)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.FunctionalTrack)
            .FirstOrDefault();

        var orderedTrackTemplates = trackTemplates
            .OrderBy(x => GetTeaTrackScopeOrder(x.SupportScope, child.SupportProfile))
            .ThenBy(x => x.SortOrder)
            .ToList();

        var trackProgress = child.SkillProgressEntries
            .Where(x => x.FunctionalTrack == track)
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .ToList();
        var trackProgressMap = trackProgress.ToDictionary(x => x.SkillCode, StringComparer.OrdinalIgnoreCase);

        var roadmap = orderedTrackTemplates
            .Select(template =>
            {
                trackProgressMap.TryGetValue(template.SkillCode, out var progress);
                return new TeaTrackRoadmapItemViewModel
                {
                    Title = template.Title,
                    SkillName = template.SkillName,
                    Goal = template.Goal,
                    ParentGuide = template.ParentGuide,
                    SupportSourceLabel = GetSupportScopeLabel(template.SupportScope),
                    SupportSourceChipClass = GetSupportScopeChip(template.SupportScope),
                    FunctionalTrackLabel = GetFunctionalTrackLabel(template.FunctionalTrack),
                    FunctionalTrackChipClass = GetFunctionalTrackChip(template.FunctionalTrack),
                    StatusLabel = progress is null ? "Nao iniciada" : GetSkillStatusLabel(progress.MasteryScore),
                    StatusChipClass = progress is null ? "neutral" : GetSkillStatusChip(progress.MasteryScore),
                    ProgressPercent = progress?.MasteryScore ?? 0,
                    Recommendation = progress?.Recommendation ?? "Ainda nao praticada nesta trilha.",
                    IsCurrentFocus = todayPlan.Blocks.Any(x => x.SkillCode == template.SkillCode),
                    HasProgress = progress is not null
                };
            })
            .ToList();

        var activities = orderedTrackTemplates
            .Select(template =>
            {
                trackProgressMap.TryGetValue(template.SkillCode, out var progress);
                var isCurrentFocus = todayPlan.Blocks.Any(x => x.SkillCode == template.SkillCode);
                return new TeaTrackActivityViewModel
                {
                    TemplateId = template.Id,
                    Title = template.Title,
                    SkillName = template.SkillName,
                    Goal = template.Goal,
                    ParentGuide = template.ParentGuide,
                    ChildMission = template.ChildMission,
                    Materials = template.Materials,
                    EvidencePrompt = template.EvidencePrompt,
                    SupportSourceLabel = GetSupportScopeLabel(template.SupportScope),
                    SupportSourceChipClass = GetSupportScopeChip(template.SupportScope),
                    FunctionalTrackLabel = GetFunctionalTrackLabel(template.FunctionalTrack),
                    FunctionalTrackChipClass = GetFunctionalTrackChip(template.FunctionalTrack),
                    RecommendationLabel = GetTeaTrackActivityRecommendationLabel(progress, isCurrentFocus),
                    RecommendationChipClass = GetTeaTrackActivityRecommendationChip(progress, isCurrentFocus),
                    ProgressPercent = progress?.MasteryScore ?? 0,
                    IsFavorite = favoriteTemplateSet.Contains(template.Id),
                    IsCurrentFocus = isCurrentFocus,
                    IsQueuedForTomorrow = queuedTemplateSet.Contains(template.Id)
                };
            })
            .OrderByDescending(x => x.IsCurrentFocus)
            .ThenByDescending(x => x.IsFavorite)
            .ThenByDescending(x => x.IsQueuedForTomorrow)
            .ThenBy(x => x.ProgressPercent == 0 ? 0 : x.ProgressPercent < 45 ? 1 : x.ProgressPercent < 75 ? 2 : 3)
            .ThenBy(x => x.Title)
            .ToList();

        var materialKit = BuildTeaTrackMaterialKit(orderedTrackTemplates, todayPlan.Blocks.Where(x => x.FunctionalTrack == track).ToList());

        var todayBlocks = todayPlan.Blocks
            .Where(x => x.FunctionalTrack == track)
            .OrderBy(x => x.SortOrder)
            .Select(block => BuildTrackPlanBlockViewModel(block, trackProgressMap))
            .ToList();

        var entries = child.LearningSessions
            .OrderByDescending(x => x.LoggedAt)
            .Select(session => BuildCurriculumEntryViewModel(session, sessionPlans, skillDomainMap, track))
            .Where(x => x.Skills.Count > 0)
            .ToList();

        var practicedSkills = trackProgress.Select(x => x.SkillCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var totalSkills = orderedTrackTemplates.Select(x => x.SkillCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (totalSkills == 0)
        {
            totalSkills = practicedSkills;
        }

        var strongSkills = trackProgress.Count(x => x.MasteryScore >= 75);
        var weakSkills = trackProgress.Count(x => x.MasteryScore < 45);
        var progressPercent = trackProgress.Count == 0
            ? 0
            : (int)Math.Round(trackProgress.Average(x => x.MasteryScore));
        var tomorrowPinnedActivities = orderedTrackTemplates
            .Where(x => queuedTemplateSet.Contains(x.Id))
            .Select(x => x.Title)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TeaTrackDetailViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = age,
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            TeachingMethodologyLabel = GetTeachingMethodologyLabel(child.TeachingMethodology),
            LearningProfileLabel = GetLearningProfileLabel(child.LearningProfile),
            GuidanceStyleLabel = GetGuidanceStyleLabel(child.GuidanceStyle),
            Track = track,
            TrackLabel = GetFunctionalTrackLabel(track),
            TrackChipClass = GetFunctionalTrackChip(track),
            TrackSummary = GetTeaTrackSummary(track, child.SupportProfile),
            TrackDescription = GetTeaTrackDescription(track, child.SupportProfile),
            Recommendation = BuildTeaTrackRecommendation(trackProgress, orderedTrackTemplates, track),
            ActionMessage = TempData["TeaTrackActionMessage"]?.ToString() ?? string.Empty,
            ActionMessageStyle = TempData["TeaTrackActionStyle"]?.ToString() ?? "neutral",
            TotalSkills = totalSkills,
            PracticedSkills = practicedSkills,
            StrongSkills = strongSkills,
            WeakSkills = weakSkills,
            ProgressPercent = progressPercent,
            HasTomorrowPlan = tomorrowPlan is not null || tomorrowDirectives.Count > 0,
            TomorrowPlanSummary = BuildTomorrowPlanSummary(tomorrowPlan, tomorrowPinnedActivities, tomorrowTrackFocus),
            TomorrowFocusLabel = tomorrowTrackFocus.HasValue ? GetFunctionalTrackLabel(tomorrowTrackFocus.Value) : string.Empty,
            TomorrowFocusChipClass = tomorrowTrackFocus.HasValue ? GetFunctionalTrackChip(tomorrowTrackFocus.Value) : "neutral",
            TomorrowPinnedActivities = tomorrowPinnedActivities,
            HubUrl = Url.Action(nameof(TeaTracks), new { id = child.Id }) ?? string.Empty,
            ChildUrl = Url.Action(nameof(Child), new { id = child.Id }) ?? string.Empty,
            CurriculumUrl = Url.Action(nameof(Curriculum), new { id = child.Id }) ?? string.Empty,
            TrackLinks = BuildTeaTrackQuickLinks(child.Id, child.SupportProfile),
            MaterialKit = materialKit,
            Activities = activities,
            TodayBlocks = todayBlocks,
            Roadmap = roadmap,
            Entries = entries
        };
    }

    private CurriculumEntryViewModel BuildCurriculumEntryViewModel(
        LearningSession session,
        IReadOnlyDictionary<Guid, string> sessionPlans,
        IReadOnlyDictionary<string, string> skillDomainMap,
        FunctionalSupportTrack? trackFilter = null)
    {
        var relevantFeedbacks = session.BlockFeedbacks
            .Where(feedback => trackFilter is null || feedback.DailyPlanBlock?.FunctionalTrack == trackFilter)
            .ToList();

        return new CurriculumEntryViewModel
        {
            SessionId = session.Id,
            LoggedAt = session.LoggedAt,
            Theme = sessionPlans.TryGetValue(session.DailyPlanId, out var theme) ? theme : "Rotina registrada",
            MinutesCompleted = session.MinutesCompleted,
            Wins = session.Wins,
            Challenges = session.Challenges,
            Notes = session.Notes,
            MediaUrl = session.MediaUrl,
            MediaContentType = session.MediaContentType,
            MediaFileName = session.MediaFileName,
            ContextBadges = relevantFeedbacks
                .SelectMany(feedback => BuildCurriculumContextBadges(feedback.DailyPlanBlock))
                .GroupBy(x => x.Label)
                .Select(x => x.First())
                .ToList(),
            Skills = relevantFeedbacks
                .Select(feedback => new CurriculumSkillEntryViewModel
                {
                    SkillName = feedback.DailyPlanBlock?.SkillName ?? feedback.Notes,
                    DomainLabel = feedback.DailyPlanBlock is not null
                        ? FormatDomain(feedback.DailyPlanBlock.Domain)
                        : skillDomainMap.TryGetValue(feedback.Notes, out var domainLabel) ? domainLabel : "Geral",
                    SupportSourceLabel = GetSupportScopeLabel(feedback.DailyPlanBlock?.SupportScope ?? CurriculumSupportScope.General),
                    SupportSourceChipClass = GetSupportScopeChip(feedback.DailyPlanBlock?.SupportScope ?? CurriculumSupportScope.General),
                    FunctionalTrackLabel = GetFunctionalTrackLabel(feedback.DailyPlanBlock?.FunctionalTrack ?? FunctionalSupportTrack.Base),
                    FunctionalTrackChipClass = GetFunctionalTrackChip(feedback.DailyPlanBlock?.FunctionalTrack ?? FunctionalSupportTrack.Base),
                    PerformanceLabel = GetFeedbackLabel(feedback.Rating),
                    PerformanceChipClass = GetFeedbackChip(feedback.Rating)
                })
                .ToList()
        };
    }

    private CurriculumEntryViewModel BuildCurriculumEntryFromCompletion(
        DailyPlanBlockCompletion completion,
        IReadOnlyDictionary<Guid, string> sessionPlans)
    {
        var block = completion.DailyPlanBlock;
        var theme = sessionPlans.TryGetValue(completion.DailyPlanId, out var plannedTheme)
            ? plannedTheme
            : "Lição concluída";

        return new CurriculumEntryViewModel
        {
            SessionId = completion.Id,
            LoggedAt = completion.CompletedAt,
            Theme = theme,
            MinutesCompleted = block?.DurationMinutes ?? 0,
            Wins = "Lição concluída pelo fluxo simples.",
            Challenges = string.Empty,
            Notes = string.IsNullOrWhiteSpace(completion.Notes)
                ? block?.Title ?? string.Empty
                : completion.Notes,
            MediaUrl = string.Empty,
            MediaContentType = string.Empty,
            MediaFileName = string.Empty,
            ContextBadges = block is null
                ? new List<EntryBadgeViewModel>()
                : BuildCurriculumContextBadges(block),
            Skills = block is null
                ? new List<CurriculumSkillEntryViewModel>()
                : new List<CurriculumSkillEntryViewModel>
                {
                    new()
                    {
                        SkillName = string.IsNullOrWhiteSpace(block.SkillName) ? block.Title : block.SkillName,
                        DomainLabel = FormatDomain(block.Domain),
                        SupportSourceLabel = GetSupportScopeLabel(block.SupportScope),
                        SupportSourceChipClass = GetSupportScopeChip(block.SupportScope),
                        FunctionalTrackLabel = GetFunctionalTrackLabel(block.FunctionalTrack),
                        FunctionalTrackChipClass = GetFunctionalTrackChip(block.FunctionalTrack),
                        PerformanceLabel = "Concluída",
                        PerformanceChipClass = "success"
                    }
                }
        };
    }

    private PlanBlockViewModel BuildTrackPlanBlockViewModel(
        DailyPlanBlock block,
        IReadOnlyDictionary<string, ChildSkillProgress> progressLookup)
    {
        progressLookup.TryGetValue(block.SkillCode, out var progress);

        return new PlanBlockViewModel
        {
            Id = block.Id,
            Title = block.Title,
            DomainLabel = FormatDomain(block.Domain),
            SupportSourceLabel = GetSupportScopeLabel(block.SupportScope),
            SupportSourceChipClass = GetSupportScopeChip(block.SupportScope),
            FunctionalTrackLabel = GetFunctionalTrackLabel(block.FunctionalTrack),
            FunctionalTrackChipClass = GetFunctionalTrackChip(block.FunctionalTrack),
            SkillName = block.SkillName,
            SkillStageLabel = skillProgressionService.GetStageLabel(progress?.SkillStage ?? "starting"),
            SkillStageChipClass = skillProgressionService.GetStageChip(
                progress?.SkillStage ?? "starting",
                progress?.NeedsRemediation ?? false,
                progress?.ReadyToAdvance ?? false),
            NextMilestone = progress?.NextMilestone ?? "Comecar a praticar com apoio",
            InterventionTip = progress?.RemediationPlan ?? string.Empty,
            FocusLabel = GetBlockFocusLabel(progress?.MasteryScore),
            FocusChipClass = GetBlockFocusChip(progress?.MasteryScore),
            Goal = block.Goal,
            ParentGuide = block.ParentGuide,
            ChildPrompt = block.ChildPrompt,
            Materials = block.Materials,
            EvidencePrompt = block.EvidencePrompt,
            IsRecoveryFocus = block.IsRecoveryFocus,
            RecoveryNote = block.RecoveryNote,
            IsSpacedReview = block.IsSpacedReview,
            ReviewNote = block.ReviewNote,
            DurationMinutes = block.DurationMinutes
        };
    }

    private async Task<CurriculumTemplate?> GetTrackTemplateAsync(
        ChildProfile child,
        FunctionalSupportTrack track,
        Guid templateId)
    {
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        age = Math.Clamp(age, 3, 14);

        var templates = await db.CurriculumTemplates
            .Where(x => x.Id == templateId && x.Age == age && x.FunctionalTrack == track)
            .ToListAsync();

        return templates.FirstOrDefault(x => IsTemplateAllowedForProfile(x.SupportScope, child.SupportProfile));
    }

    private void SetTeaTrackActionMessage(string message, string style)
    {
        TempData["TeaTrackActionMessage"] = message;
        TempData["TeaTrackActionStyle"] = style;
    }

    private static string BuildTomorrowPlanSummary(
        DailyPlan? tomorrowPlan,
        IReadOnlyCollection<string> tomorrowPinnedActivities,
        FunctionalSupportTrack? tomorrowTrackFocus)
    {
        if (tomorrowPlan is null && tomorrowPinnedActivities.Count == 0 && !tomorrowTrackFocus.HasValue)
        {
            return "Amanha ainda segue o motor automatico da plataforma.";
        }

        var planText = tomorrowPlan is null
            ? "O plano de amanha sera montado com base nas diretivas salvas."
            : $"O plano de amanha ja esta preparado com {tomorrowPlan.Blocks.Count} bloco(s).";
        var focusText = tomorrowTrackFocus.HasValue
            ? $" Foco manual: {GetFunctionalTrackLabel(tomorrowTrackFocus.Value)}."
            : string.Empty;
        var pinnedText = tomorrowPinnedActivities.Count == 0
            ? string.Empty
            : $" Atividades puxadas: {string.Join(", ", tomorrowPinnedActivities)}.";

        return $"{planText}{focusText}{pinnedText}";
    }

    private static int GetTrackDisplayOrder(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => 0,
        FunctionalSupportTrack.Regulation => 1,
        FunctionalSupportTrack.Sensory => 2,
        FunctionalSupportTrack.DailyLiving => 3,
        FunctionalSupportTrack.AcademicAdapted => 4,
        _ => 5
    };

    private static bool SupportsTeaNavigation(SupportProfile supportProfile) => supportProfile != SupportProfile.General;

    private List<TeaTrackQuickLinkViewModel> BuildTeaTrackQuickLinks(Guid childId, SupportProfile supportProfile) => SupportsTeaNavigation(supportProfile)
        ? GetAvailableTeaTracks(supportProfile)
            .Select(track => new TeaTrackQuickLinkViewModel
            {
                Label = GetFunctionalTrackLabel(track),
                ChipClass = GetFunctionalTrackChip(track),
                Url = Url.Action(nameof(TeaTrack), new { id = childId, track }) ?? string.Empty
            })
            .ToList()
        : [];

    private static List<FunctionalSupportTrack> GetAvailableTeaTracks(SupportProfile supportProfile) => SupportsTeaNavigation(supportProfile)
        ?
        [
            FunctionalSupportTrack.Communication,
            FunctionalSupportTrack.Regulation,
            FunctionalSupportTrack.Sensory,
            FunctionalSupportTrack.DailyLiving,
            FunctionalSupportTrack.AcademicAdapted
        ]
        : [];

    private static bool IsTemplateAllowedForProfile(CurriculumSupportScope supportScope, SupportProfile supportProfile) => supportProfile switch
    {
        SupportProfile.General => supportScope == CurriculumSupportScope.General,
        SupportProfile.TeaLevel1 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel1,
        SupportProfile.TeaLevel2 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel2,
        SupportProfile.TeaLevel3 => supportScope is CurriculumSupportScope.General or CurriculumSupportScope.TeaCommon or CurriculumSupportScope.TeaLevel3,
        _ => supportScope == CurriculumSupportScope.General
    };

    private static int GetTeaTrackScopeOrder(CurriculumSupportScope supportScope, SupportProfile supportProfile) => (supportProfile, supportScope) switch
    {
        (SupportProfile.TeaLevel1, CurriculumSupportScope.TeaCommon) => 0,
        (SupportProfile.TeaLevel2, CurriculumSupportScope.TeaCommon) => 0,
        (SupportProfile.TeaLevel3, CurriculumSupportScope.TeaCommon) => 0,
        (SupportProfile.TeaLevel1, CurriculumSupportScope.TeaLevel1) => 1,
        (SupportProfile.TeaLevel2, CurriculumSupportScope.TeaLevel2) => 1,
        (SupportProfile.TeaLevel3, CurriculumSupportScope.TeaLevel3) => 1,
        (_, CurriculumSupportScope.General) => 2,
        _ => 3
    };

    private static string GetTeaTrackSummary(FunctionalSupportTrack track, SupportProfile supportProfile) => (supportProfile, track) switch
    {
        (_, FunctionalSupportTrack.Communication) => "Linguagem funcional, resposta, pedido e ampliacao de repertorio comunicativo.",
        (_, FunctionalSupportTrack.Regulation) => "Previsibilidade, autorregulacao, transicao e entrada mais segura na rotina.",
        (_, FunctionalSupportTrack.Sensory) => "Leitura do ambiente, ajuste sensorial e retorno para a tarefa com menos atrito.",
        (_, FunctionalSupportTrack.DailyLiving) => "Sequencias funcionais, autocuidado, organizacao e autonomia no cotidiano.",
        (_, FunctionalSupportTrack.AcademicAdapted) => "Entrada academica com apoio ajustado, linguagem reduzida e mais concretude.",
        _ => "Trilha funcional de apoio ao ensino domiciliar."
    };

    private static string GetTeaTrackDescription(FunctionalSupportTrack track, SupportProfile supportProfile) => (supportProfile, track) switch
    {
        (SupportProfile.TeaLevel1, FunctionalSupportTrack.Communication) => "Foco em pragmatica, clareza de resposta e flexibilidade de linguagem em contexto.",
        (SupportProfile.TeaLevel2, FunctionalSupportTrack.Communication) => "Foco em comunicacao funcional com mediação visual, pedidos claros e ampliacao gradual.",
        (SupportProfile.TeaLevel3, FunctionalSupportTrack.Communication) => "Foco em comunicacao funcional altamente estruturada, com resposta concreta e previsivel.",
        (SupportProfile.TeaLevel1, FunctionalSupportTrack.Regulation) => "Foco em flexibilidade, autonomia e organizacao da rotina com menos rigidez.",
        (SupportProfile.TeaLevel2, FunctionalSupportTrack.Regulation) => "Foco em previsibilidade, passos menores e apoio visual intenso para sustentar o bloco.",
        (SupportProfile.TeaLevel3, FunctionalSupportTrack.Regulation) => "Foco em seguranca, rotina altamente estruturada e regulacao antes de exigir academico.",
        (SupportProfile.TeaLevel1, FunctionalSupportTrack.Sensory) => "Foco em reconhecer desconforto, pedir ajuste e voltar para a meta com autonomia crescente.",
        (SupportProfile.TeaLevel2, FunctionalSupportTrack.Sensory) => "Foco em pausa sensorial guiada, preparo corporal e reorganizacao com apoio concreto.",
        (SupportProfile.TeaLevel3, FunctionalSupportTrack.Sensory) => "Foco em seguranca sensorial como prerequisito para qualquer aprendizagem.",
        (SupportProfile.TeaLevel1, FunctionalSupportTrack.DailyLiving) => "Foco em autonomia funcional com planejamento e pequenas adaptacoes de rotina.",
        (SupportProfile.TeaLevel2, FunctionalSupportTrack.DailyLiving) => "Foco em sequencias curtas de vida diaria com passos visuais e reforco previsivel.",
        (SupportProfile.TeaLevel3, FunctionalSupportTrack.DailyLiving) => "Foco em vida diaria muito concreta, com ajuda alta e fade gradual.",
        (SupportProfile.TeaLevel1, FunctionalSupportTrack.AcademicAdapted) => "Foco em transferir estrategia e manter desempenho mesmo com pequena variacao de formato.",
        (SupportProfile.TeaLevel2, FunctionalSupportTrack.AcademicAdapted) => "Foco em pareamento concreto, apoio visual intenso e resposta academica em microetapas.",
        (SupportProfile.TeaLevel3, FunctionalSupportTrack.AcademicAdapted) => "Foco em entrada academica altamente estruturada, concreta e observavel.",
        _ => GetTeaTrackSummary(track, supportProfile)
    };

    private static string GetTeaTrackStatusLabel(int practicedSkills, int weakSkills, int strongSkills)
    {
        if (practicedSkills == 0)
        {
            return "Nao iniciada";
        }

        if (weakSkills > 0)
        {
            return "Pede reforco";
        }

        if (strongSkills > 0)
        {
            return "Consolidando";
        }

        return "Em progresso";
    }

    private static string GetTeaTrackStatusChip(int practicedSkills, int weakSkills, int strongSkills)
    {
        if (practicedSkills == 0)
        {
            return "neutral";
        }

        if (weakSkills > 0)
        {
            return "warning";
        }

        if (strongSkills > 0)
        {
            return "success";
        }

        return "neutral";
    }

    private static string BuildTeaTrackRecommendation(
        IReadOnlyList<ChildSkillProgress> trackProgress,
        IReadOnlyList<CurriculumTemplate> templates,
        FunctionalSupportTrack track)
    {
        var weakest = trackProgress
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .FirstOrDefault();
        var strongest = trackProgress
            .Where(x => x.MasteryScore >= 75)
            .OrderByDescending(x => x.MasteryScore)
            .ThenByDescending(x => x.TimesSuccessful)
            .FirstOrDefault();
        var nextTemplate = templates
            .FirstOrDefault(x => trackProgress.All(p => !string.Equals(p.SkillCode, x.SkillCode, StringComparison.OrdinalIgnoreCase)));

        if (weakest is not null && strongest is not null)
        {
            return $"Revisar {weakest.SkillName.ToLowerInvariant()} antes de ampliar {strongest.SkillName.ToLowerInvariant()} nesta trilha.";
        }

        if (weakest is not null)
        {
            return $"Voltar em {weakest.SkillName.ToLowerInvariant()} com apoio mais concreto e repeticao curta.";
        }

        if (nextTemplate is not null)
        {
            return $"A proxima entrada sugerida nesta trilha e {nextTemplate.SkillName.ToLowerInvariant()}.";
        }

        return $"A trilha de {GetFunctionalTrackLabel(track).ToLowerInvariant()} ainda precisa de mais registros para sugerir o proximo passo.";
    }

    private static List<TeaTrackMaterialViewModel> BuildTeaTrackMaterialKit(
        IReadOnlyList<CurriculumTemplate> templates,
        IReadOnlyList<DailyPlanBlock> todayBlocks)
    {
        var skillHints = templates
            .GroupBy(x => x.SkillCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().SkillName, StringComparer.OrdinalIgnoreCase);

        var rawMaterials = templates
            .Select(x => x.Materials)
            .Concat(todayBlocks.Select(x => x.Materials))
            .SelectMany(SplitMaterialTokens)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(group => new TeaTrackMaterialViewModel
            {
                Name = group.First(),
                UsageCount = group.Count(),
                SuggestedUse = BuildMaterialSuggestedUse(group.Key, templates, skillHints)
            })
            .OrderByDescending(x => x.UsageCount)
            .ThenBy(x => x.Name)
            .ToList();

        return rawMaterials;
    }

    private static IEnumerable<string> SplitMaterialTokens(string? rawMaterials)
    {
        if (string.IsNullOrWhiteSpace(rawMaterials))
        {
            yield break;
        }

        var normalized = rawMaterials
            .Replace(";", ",", StringComparison.Ordinal)
            .Replace(" / ", ",", StringComparison.Ordinal)
            .Replace(" e ", ",", StringComparison.OrdinalIgnoreCase);

        foreach (var token in normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (token.Length < 3)
            {
                continue;
            }

            yield return token;
        }
    }

    private static string BuildMaterialSuggestedUse(
        string materialName,
        IReadOnlyList<CurriculumTemplate> templates,
        IReadOnlyDictionary<string, string> skillHints)
    {
        var relatedSkill = templates
            .FirstOrDefault(x => x.Materials.Contains(materialName, StringComparison.OrdinalIgnoreCase));

        if (relatedSkill is not null && skillHints.TryGetValue(relatedSkill.SkillCode, out var skillName))
        {
            return $"Ajuda em {skillName.ToLowerInvariant()}.";
        }

        return "Pode ser usado em mais de uma atividade desta trilha.";
    }

    private static string GetTeaTrackActivityRecommendationLabel(ChildSkillProgress? progress, bool isCurrentFocus)
    {
        if (isCurrentFocus)
        {
            return "No plano de hoje";
        }

        if (progress is null || progress.TimesPracticed == 0)
        {
            return "Boa para introduzir";
        }

        if (progress.MasteryScore < 45)
        {
            return "Boa para reforco";
        }

        if (progress.MasteryScore < 75)
        {
            return "Boa para consolidar";
        }

        return "Boa para avancar";
    }

    private static string GetTeaTrackActivityRecommendationChip(ChildSkillProgress? progress, bool isCurrentFocus)
    {
        if (isCurrentFocus)
        {
            return "success";
        }

        if (progress is null || progress.TimesPracticed == 0)
        {
            return "neutral";
        }

        if (progress.MasteryScore < 45)
        {
            return "warning";
        }

        if (progress.MasteryScore < 75)
        {
            return "neutral";
        }

        return "success";
    }

    private async Task<ChildProfile> GetOwnedChildAsync(Guid childId)
    {
        var parentId = GetCurrentUserId();
        return await db.Children.FirstAsync(x => x.Id == childId && x.ParentId == parentId);
    }

    private async Task<ChildProfile> GetOwnedChildWithProfilesAsync(Guid childId)
    {
        var parentId = GetCurrentUserId();
        return await db.Children
            .Include(x => x.DevelopmentProfile)
            .Include(x => x.TeaProfile)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);
    }

    private void PrepareChildFormView(bool isEditMode, Guid? childId = null, string? childName = null)
    {
        ViewData["Title"] = isEditMode ? "Editar criança" : "Nova criança";
        ViewBag.IsEditMode = isEditMode;
        ViewBag.ChildId = childId;
        ViewBag.FormKicker = isEditMode ? "Editar criança" : "Nova criança";
        ViewBag.FormHeading = isEditMode
            ? $"Atualize {childName ?? "esta criança"} sem perder o histórico importante."
            : "Cadastre uma criança e deixe o sistema montar o resto.";
        ViewBag.FormDescription = isEditMode
            ? "Ajuste nome, idade, ritmo e apoios. O NewSchool reorganiza os próximos planos sem mexer no que já foi registrado."
            : "Depois deste cadastro, a plataforma já deve organizar currículo por idade, aula do dia, tarefas, histórico, evolução e espaço para provas com fotos e vídeos.";
        ViewBag.FlowStatus = isEditMode ? "Gerenciamento da família" : "Fluxo de entrada";
        ViewBag.FlowStatusTitle = isEditMode ? "Mudanças seguras" : "O mínimo para começar";
        ViewBag.FlowStatusDescription = isEditMode
            ? "O histórico concluído continua guardado. O sistema só reorganiza os próximos passos dessa criança."
            : "Nome, idade, ritmo diário e direção principal já são suficientes para o primeiro plano.";
        ViewBag.FormSubmitLabel = isEditMode ? "Salvar alterações" : "Criar rotina desta criança";
    }

    private CreateChildViewModel BuildChildFormViewModel(ChildProfile child)
    {
        var developmentProfile = child.DevelopmentProfile;
        var teaProfile = child.TeaProfile;

        return new CreateChildViewModel
        {
            FullName = child.FullName,
            BirthDate = child.BirthDate,
            EstimatedAgeYears = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14),
            DailyStudyMinutes = child.DailyStudyMinutes,
            Notes = ExtractUnstructuredText(child.Notes, SchoolBarrierLabels),
            SupportProfile = child.SupportProfile.ToString(),
            FamilyGoalTrack = child.FamilyGoalTrack,
            TeachingMethodology = child.TeachingMethodology,
            LearningProfile = child.LearningProfile,
            GuidanceStyle = child.GuidanceStyle,
            LanguageLevel = developmentProfile?.LanguageLevel ?? 3,
            MathLevel = developmentProfile?.MathLevel ?? 3,
            WorldLevel = developmentProfile?.WorldLevel ?? 3,
            ExecutiveFunctionLevel = developmentProfile?.ExecutiveFunctionLevel ?? 3,
            StrengthCurious = ContainsStructuredLabel(developmentProfile?.StrengthsSummary, "curiosidade e investigacao"),
            StrengthCommunicative = ContainsStructuredLabel(developmentProfile?.StrengthsSummary, "boa comunicacao"),
            StrengthCreative = ContainsStructuredLabel(developmentProfile?.StrengthsSummary, "criatividade"),
            StrengthMemorizes = ContainsStructuredLabel(developmentProfile?.StrengthsSummary, "memoriza com facilidade"),
            StrengthLogical = ContainsStructuredLabel(developmentProfile?.StrengthsSummary, "gosta de logica e padroes"),
            SupportNeedsFocus = ContainsStructuredLabel(developmentProfile?.SupportSummary, "precisa reforcar foco"),
            SupportNeedsTransitions = ContainsStructuredLabel(developmentProfile?.SupportSummary, "precisa de ajuda nas transicoes"),
            SupportNeedsLanguage = ContainsStructuredLabel(developmentProfile?.SupportSummary, "precisa reforcar linguagem"),
            SupportNeedsRegulation = ContainsStructuredLabel(developmentProfile?.SupportSummary, "precisa reforcar autorregulacao"),
            NeedsVisualRoutine = teaProfile?.NeedsVisualRoutine ?? false,
            NeedsFirstThen = teaProfile?.NeedsFirstThen ?? false,
            NeedsTimer = teaProfile?.NeedsTimer ?? false,
            NeedsPlanB = teaProfile?.NeedsPlanB ?? false,
            ParentPrimaryGoal = teaProfile?.ParentPrimaryGoal ?? string.Empty,
            SpecialInterests = teaProfile?.SpecialInterests ?? string.Empty,
            DocumentationNotes = teaProfile?.DocumentationNotes ?? string.Empty,
            CommunicationDirectLanguage = ContainsStructuredLabel(teaProfile?.CommunicationProfile, "entende melhor linguagem direta"),
            CommunicationVisualSupport = ContainsStructuredLabel(teaProfile?.CommunicationProfile, "responde melhor com apoio visual"),
            CommunicationModeling = ContainsStructuredLabel(teaProfile?.CommunicationProfile, "precisa de modelagem antes de tentar"),
            CommunicationProcessingTime = ContainsStructuredLabel(teaProfile?.CommunicationProfile, "precisa de mais tempo para processar"),
            TriggerUnexpectedChange = ContainsStructuredLabel(teaProfile?.CommonTriggers, "mudanca inesperada"),
            TriggerNoise = ContainsStructuredLabel(teaProfile?.CommonTriggers, "barulho"),
            TriggerWritingDemand = ContainsStructuredLabel(teaProfile?.CommonTriggers, "escrita ou tarefa no papel"),
            TriggerWaiting = ContainsStructuredLabel(teaProfile?.CommonTriggers, "espera"),
            TriggerSocialDemand = ContainsStructuredLabel(teaProfile?.CommonTriggers, "demanda social"),
            CalmingPause = ContainsStructuredLabel(teaProfile?.CalmingStrategies, "pausa curta"),
            CalmingMovement = ContainsStructuredLabel(teaProfile?.CalmingStrategies, "movimento"),
            CalmingQuietCorner = ContainsStructuredLabel(teaProfile?.CalmingStrategies, "lugar silencioso"),
            CalmingDrawing = ContainsStructuredLabel(teaProfile?.CalmingStrategies, "desenhar"),
            CalmingMusic = ContainsStructuredLabel(teaProfile?.CalmingStrategies, "musica calma"),
            BarrierAnxietyCrises = ContainsStructuredLabel(teaProfile?.SchoolBarrierSummary, "crises de ansiedade ligadas a escola"),
            BarrierSchoolOverload = ContainsStructuredLabel(teaProfile?.SchoolBarrierSummary, "sobrecarga sensorial no ambiente escolar"),
            BarrierRigidity = ContainsStructuredLabel(teaProfile?.SchoolBarrierSummary, "rigidez cognitiva alta"),
            BarrierSocialDifficulty = ContainsStructuredLabel(teaProfile?.SchoolBarrierSummary, "demanda social alta demais")
        };
    }

    private static void ApplyChildFormValues(
        CreateChildViewModel model,
        ChildProfile child,
        ChildDevelopmentProfile developmentProfile,
        ChildTeaProfile teaProfile,
        string resolvedName,
        DateTime resolvedBirthDate,
        bool preserveHiddenValues)
    {
        var strengthsSummary = BuildStrengthsSummary(model);
        var supportSummary = BuildSupportSummary(model);
        var communicationProfile = BuildCommunicationProfile(model);
        var commonTriggers = BuildCommonTriggers(model);
        var calmingStrategies = BuildCalmingStrategies(model);
        var schoolBarrierSummary = BuildSchoolBarrierSummary(model);
        var transitionSupports = BuildTransitionSupports(model);
        var effectiveReinforcers = BuildEffectiveReinforcers(model);
        var supportProfile = Enum.TryParse<SupportProfile>(model.SupportProfile, out var parsedSupportProfile)
            ? parsedSupportProfile
            : SupportProfile.General;
        var supportIntensityLevel = GetSupportIntensityLevel(supportProfile);

        var preservedStrengthsText = preserveHiddenValues
            ? ExtractUnstructuredText(developmentProfile.StrengthsSummary, StrengthLabels)
            : model.StrengthsSummary;
        var preservedSupportText = preserveHiddenValues
            ? ExtractUnstructuredText(developmentProfile.SupportSummary, SupportLabels)
            : model.SupportSummary;
        var preservedCommunicationText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.CommunicationProfile, CommunicationLabels)
            : model.CommunicationProfile;
        var preservedTriggerText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.CommonTriggers, TriggerLabels)
            : model.CommonTriggers;
        var preservedCalmingText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.CalmingStrategies, CalmingLabels)
            : model.CalmingStrategies;
        var preservedBarrierText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.SchoolBarrierSummary, SchoolBarrierLabels)
            : model.SchoolBarrierSummary;
        var preservedTransitionText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.TransitionSupports, TransitionSupportLabels)
            : model.TransitionSupports;
        var preservedReinforcerText = preserveHiddenValues
            ? ExtractUnstructuredText(teaProfile.EffectiveReinforcers, ReinforcerLabels)
            : model.EffectiveReinforcers;

        child.FullName = resolvedName;
        child.BirthDate = resolvedBirthDate;
        child.DailyStudyMinutes = model.DailyStudyMinutes;
        child.Notes = CombineOptionalText(model.Notes, schoolBarrierSummary);
        child.SupportProfile = supportProfile;
        child.FamilyGoalTrack = NormalizeFamilyGoalTrack(model.FamilyGoalTrack);
        child.TeachingMethodology = NormalizeTeachingMethodology(model.TeachingMethodology);
        child.LearningProfile = NormalizeLearningProfile(model.LearningProfile);
        child.GuidanceStyle = NormalizeGuidanceStyle(model.GuidanceStyle);

        developmentProfile.LanguageLevel = preserveHiddenValues && developmentProfile.LanguageLevel > 0
            ? developmentProfile.LanguageLevel
            : model.LanguageLevel;
        developmentProfile.MathLevel = preserveHiddenValues && developmentProfile.MathLevel > 0
            ? developmentProfile.MathLevel
            : model.MathLevel;
        developmentProfile.WorldLevel = preserveHiddenValues && developmentProfile.WorldLevel > 0
            ? developmentProfile.WorldLevel
            : model.WorldLevel;
        developmentProfile.ExecutiveFunctionLevel = preserveHiddenValues && developmentProfile.ExecutiveFunctionLevel > 0
            ? developmentProfile.ExecutiveFunctionLevel
            : model.ExecutiveFunctionLevel;
        developmentProfile.StrengthsSummary = CombineOptionalText(strengthsSummary, preservedStrengthsText);
        developmentProfile.SupportSummary = CombineOptionalText(supportSummary, preservedSupportText);
        developmentProfile.AssessedAt = DateTime.UtcNow;

        teaProfile.CommunicationProfile = CombineOptionalText(communicationProfile, preservedCommunicationText);
        teaProfile.CommunicationNotes = preserveHiddenValues ? teaProfile.CommunicationNotes : model.CommunicationNotes;
        teaProfile.AnxietyLevel = preserveHiddenValues && teaProfile.AnxietyLevel > 0
            ? Math.Max(teaProfile.AnxietyLevel, model.BarrierAnxietyCrises ? 4 : 3)
            : Math.Max(model.AnxietyLevel, model.BarrierAnxietyCrises ? 4 : 3);
        teaProfile.CognitiveRigidityLevel = preserveHiddenValues && teaProfile.CognitiveRigidityLevel > 0
            ? Math.Max(teaProfile.CognitiveRigidityLevel, model.BarrierRigidity ? 4 : 3)
            : Math.Max(model.CognitiveRigidityLevel, model.BarrierRigidity ? 4 : 3);
        teaProfile.SensorySensitivityLevel = preserveHiddenValues && teaProfile.SensorySensitivityLevel > 0
            ? Math.Max(teaProfile.SensorySensitivityLevel, model.BarrierSchoolOverload || model.TriggerNoise ? 4 : 3)
            : Math.Max(model.SensorySensitivityLevel, model.BarrierSchoolOverload || model.TriggerNoise ? 4 : 3);
        teaProfile.TransitionDifficultyLevel = preserveHiddenValues && teaProfile.TransitionDifficultyLevel > 0
            ? Math.Max(teaProfile.TransitionDifficultyLevel, model.TriggerUnexpectedChange || model.SupportNeedsTransitions ? 4 : 3)
            : Math.Max(model.TransitionDifficultyLevel, model.TriggerUnexpectedChange || model.SupportNeedsTransitions ? 4 : 3);
        teaProfile.SupportIntensityLevel = preserveHiddenValues && teaProfile.SupportIntensityLevel > 0
            ? Math.Max(teaProfile.SupportIntensityLevel, supportIntensityLevel)
            : Math.Max(model.SupportIntensityLevel, supportIntensityLevel);
        teaProfile.NeedsVisualRoutine = model.NeedsVisualRoutine;
        teaProfile.NeedsFirstThen = model.NeedsFirstThen;
        teaProfile.NeedsTimer = model.NeedsTimer;
        teaProfile.NeedsPlanB = model.NeedsPlanB;
        teaProfile.SpecialInterests = model.SpecialInterests;
        teaProfile.EffectiveReinforcers = CombineOptionalText(effectiveReinforcers, preservedReinforcerText);
        teaProfile.CommonTriggers = CombineOptionalText(commonTriggers, preservedTriggerText);
        teaProfile.OverloadSignals = preserveHiddenValues ? teaProfile.OverloadSignals : model.OverloadSignals;
        teaProfile.CalmingStrategies = CombineOptionalText(calmingStrategies, preservedCalmingText);
        teaProfile.TransitionSupports = CombineOptionalText(transitionSupports, preservedTransitionText);
        teaProfile.DailyLivingPriorities = preserveHiddenValues ? teaProfile.DailyLivingPriorities : model.DailyLivingPriorities;
        teaProfile.ParentPrimaryGoal = model.ParentPrimaryGoal;
        teaProfile.SchoolBarrierSummary = CombineOptionalText(schoolBarrierSummary, preservedBarrierText);
        teaProfile.DocumentationNotes = model.DocumentationNotes;
        teaProfile.UpdatedAt = DateTime.UtcNow;
    }

    private async Task ResetUpcomingPlansAsync(Guid childId)
    {
        var planIds = await db.DailyPlans
            .Where(x => x.ChildId == childId && x.PlannedDate >= DateTime.Today && !x.Sessions.Any())
            .Select(x => x.Id)
            .ToListAsync();

        if (planIds.Count == 0)
        {
            return;
        }

        await db.ChildRoutineObservations
            .Where(x => x.ChildId == childId && x.DailyPlanId.HasValue && planIds.Contains(x.DailyPlanId.Value))
            .ExecuteDeleteAsync();

        await db.DailyPlanBlockCompletions
            .Where(x => x.ChildId == childId && planIds.Contains(x.DailyPlanId))
            .ExecuteDeleteAsync();

        await db.DailyPlans
            .Where(x => planIds.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    private async Task DeleteChildGraphAsync(Guid childId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var mediaUrls = await db.LearningSessions
            .Where(x => x.ChildId == childId && x.MediaUrl != "")
            .Select(x => x.MediaUrl)
            .ToListAsync();

        DeleteEvidenceFiles(mediaUrls);

        await db.ChildRoutineObservations
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.DailyPlanBlockCompletions
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.TrackAcquisitionSnapshots
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.ChildRecoveryPlans
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.ChildMonthlyGoalCycles
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.LearningSessions
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.DailyPlans
            .Where(x => x.ChildId == childId)
            .ExecuteDeleteAsync();

        await db.Children
            .Where(x => x.Id == childId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    private void DeleteEvidenceFiles(IEnumerable<string?> mediaUrls)
    {
        if (string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            return;
        }

        var webRoot = Path.GetFullPath(environment.WebRootPath);
        foreach (var mediaUrl in mediaUrls
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (mediaUrl is null || !mediaUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relativePath = mediaUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.GetFullPath(Path.Combine(environment.WebRootPath, relativePath));
            if (!physicalPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(physicalPath))
            {
                continue;
            }

            System.IO.File.Delete(physicalPath);

            var thumbnailPhysicalPath = BuildEvidenceThumbnailPhysicalPath(physicalPath);
            if (System.IO.File.Exists(thumbnailPhysicalPath))
            {
                System.IO.File.Delete(thumbnailPhysicalPath);
            }
        }
    }

    private IActionResult RedirectToLocalOrAction(string? returnUrl, string actionName, object routeValues)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(actionName, routeValues)!;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsSubscriptionActive() => User.Identity?.IsAuthenticated == true && User.IsInRole("Parent");

    private async Task SyncCheckoutSessionAsync(Guid parentId, string sessionId)
    {
        var sessionService = new SessionService();
        Session? session = null;

        try
        {
            session = await sessionService.GetAsync(sessionId);
        }
        catch (Stripe.StripeException)
        {
            var recentSessions = await sessionService.ListAsync(new SessionListOptions
            {
                Limit = 20
            });

            session = recentSessions.Data
                .Where(x => x.Mode == "subscription")
                .Where(x => string.Equals(x.ClientReferenceId, parentId.ToString(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();
        }

        if (session is null)
        {
            return;
        }

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(session.Status, "complete", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == parentId);
        if (user is null)
        {
            return;
        }

        user.StripeCustomerId = session.CustomerId;
        user.StripeSubscriptionId = session.SubscriptionId;
        if (!string.IsNullOrWhiteSpace(session.SubscriptionId))
        {
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(session.SubscriptionId);
            evidenceStoragePlanService.ApplySubscriptionToUser(user, subscription);
        }
        else
        {
            user.SubscriptionStatus = "active";
        }

        await db.SaveChangesAsync();
        await trackAnalyticsService.SyncSubscriptionAttributionAsync(user.Id);
    }

    private static RevenueBannerViewModel? BuildRevenueBanner(AppUser parent, DateTime today, int inactiveDaysBeforeVisit)
    {
        var trialDaysLeft = parent.TrialEndsAt.HasValue
            ? (parent.TrialEndsAt.Value.Date - today).Days
            : (int?)null;

        if (parent.SubscriptionStatus is "past_due" or "unpaid")
        {
            return new RevenueBannerViewModel
            {
                Style = "warning",
                Eyebrow = "Pagamento pendente",
                Title = "Seu acesso premium corre risco de pausar.",
                Message = "Atualize o cartao agora para nao interromper os planos diarios, os relatorios e o acompanhamento pedagogico da familia.",
                PrimaryActionLabel = "Atualizar pagamento",
                PrimaryActionKind = "portal",
                SecondaryActionLabel = "Tentar nova assinatura",
                SecondaryActionKind = "checkout"
            };
        }

        if (parent.SubscriptionStatus == "canceled")
        {
            return new RevenueBannerViewModel
            {
                Style = "warning",
                Eyebrow = "Reativacao",
                Title = "Sua familia perdeu o acesso premium.",
                Message = "Reative agora para voltar a usar a rotina diaria por idade, os relatorios semanais e o plano automatico de amanha.",
                PrimaryActionLabel = "Reativar assinatura",
                PrimaryActionKind = "checkout",
                SecondaryActionLabel = "Gerenciar cobranca",
                SecondaryActionKind = string.IsNullOrWhiteSpace(parent.StripeCustomerId) ? null : "portal"
            };
        }

        if (parent.SubscriptionStatus == "trial_expired")
        {
            return new RevenueBannerViewModel
            {
                Style = "warning",
                Eyebrow = "Trial encerrado",
                Title = "Seu trial terminou e os recursos premium foram bloqueados.",
                Message = "Ative a assinatura para continuar com cadastro de criancas, rotinas diarias adaptativas e historico pedagogico completo.",
                PrimaryActionLabel = "Assinar agora",
                PrimaryActionKind = "checkout"
            };
        }

        if (parent.SubscriptionStatus != "active" && trialDaysLeft.HasValue && trialDaysLeft.Value <= 2)
        {
            return new RevenueBannerViewModel
            {
                Style = "neutral",
                Eyebrow = "Trial acabando",
                Title = $"Faltam {Math.Max(trialDaysLeft.Value, 0)} dia(s) para o fim do trial.",
                Message = "Garanta continuidade na rotina da familia antes do bloqueio. A assinatura libera o uso completo e mantem a evolucao registrada.",
                PrimaryActionLabel = "Garantir assinatura",
                PrimaryActionKind = "checkout"
            };
        }

        if (parent.SubscriptionStatus == "active" && inactiveDaysBeforeVisit >= 7)
        {
            return new RevenueBannerViewModel
            {
                Style = "success",
                Eyebrow = "Retomar rotina",
                Title = "Sua assinatura esta ativa e vale a pena reaproveitar isso hoje.",
                Message = "Volte a registrar a rotina para o sistema reforcar habilidades fracas, atualizar os relatorios e gerar o plano de amanha automaticamente.",
                PrimaryActionLabel = "Abrir rotina das criancas",
                PrimaryActionKind = "children"
            };
        }

        return null;
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

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string FormatDomain(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem",
        LearningDomain.Math => "Matematica",
        LearningDomain.World => "Mundo real",
        _ => "Funcao executiva"
    };

    private static string GetSupportProfileLabel(SupportProfile supportProfile) => supportProfile switch
    {
        SupportProfile.TeaLevel1 => "Perfil TEA nivel 1",
        SupportProfile.TeaLevel2 => "Perfil TEA nivel 2",
        SupportProfile.TeaLevel3 => "Perfil TEA nivel 3",
        _ => "Perfil geral"
    };

    private static string GetSupportScopeLabel(CurriculumSupportScope supportScope) => supportScope switch
    {
        CurriculumSupportScope.TeaCommon => "TEA comum",
        CurriculumSupportScope.TeaLevel1 => "TEA nivel 1",
        CurriculumSupportScope.TeaLevel2 => "TEA nivel 2",
        CurriculumSupportScope.TeaLevel3 => "TEA nivel 3",
        _ => "Base comum"
    };

    private static string GetSupportScopeChip(CurriculumSupportScope supportScope) => supportScope switch
    {
        CurriculumSupportScope.TeaCommon => "support-common",
        CurriculumSupportScope.TeaLevel1 => "support-level1",
        CurriculumSupportScope.TeaLevel2 => "support-level2",
        CurriculumSupportScope.TeaLevel3 => "support-level3",
        _ => "support-base"
    };

    private static string GetFunctionalTrackLabel(FunctionalSupportTrack functionalTrack) => functionalTrack switch
    {
        FunctionalSupportTrack.Communication => "Comunicacao",
        FunctionalSupportTrack.Regulation => "Regulacao",
        FunctionalSupportTrack.Sensory => "Sensorial",
        FunctionalSupportTrack.DailyLiving => "Vida diaria",
        FunctionalSupportTrack.AcademicAdapted => "Academico adaptado",
        _ => "Base academica"
    };

    private static string GetFunctionalTrackChip(FunctionalSupportTrack functionalTrack) => functionalTrack switch
    {
        FunctionalSupportTrack.Communication => "track-communication",
        FunctionalSupportTrack.Regulation => "track-regulation",
        FunctionalSupportTrack.Sensory => "track-sensory",
        FunctionalSupportTrack.DailyLiving => "track-dailyliving",
        FunctionalSupportTrack.AcademicAdapted => "track-academic",
        _ => "track-base"
    };

    private static List<EntryBadgeViewModel> BuildCurriculumContextBadges(DailyPlanBlock? block)
    {
        if (block is null)
        {
            return [];
        }

        return
        [
            new EntryBadgeViewModel
            {
                Label = GetSupportScopeLabel(block.SupportScope),
                ChipClass = GetSupportScopeChip(block.SupportScope)
            },
            new EntryBadgeViewModel
            {
                Label = GetFunctionalTrackLabel(block.FunctionalTrack),
                ChipClass = GetFunctionalTrackChip(block.FunctionalTrack)
            }
        ];
    }

    private static string GetSkillStatusLabel(int score) => score switch
    {
        < 45 => "Fraca",
        < 75 => "Em desenvolvimento",
        _ => "Forte"
    };

    private static string GetSkillStatusChip(int score) => score switch
    {
        < 45 => "warning",
        < 75 => "neutral",
        _ => "success"
    };

    private static string GetBlockFocusLabel(int? score) => score switch
    {
        null => "Nova habilidade",
        < 45 => "Reforco",
        < 75 => "Consolidacao",
        _ => "Avanco"
    };

    private static string GetBlockFocusChip(int? score) => score switch
    {
        null => "neutral",
        < 45 => "warning",
        < 75 => "neutral",
        _ => "success"
    };

    private static string GetFeedbackLabel(SkillFeedbackLevel rating) => rating switch
    {
        SkillFeedbackLevel.NeedsSupport => "Precisa de reforco",
        SkillFeedbackLevel.Developing => "Conseguiu com ajuda",
        _ => "Conseguiu com seguranca"
    };

    private static string GetFeedbackChip(SkillFeedbackLevel rating) => rating switch
    {
        SkillFeedbackLevel.NeedsSupport => "warning",
        SkillFeedbackLevel.Developing => "neutral",
        _ => "success"
    };

    private static string BuildDailyRecommendation(
        IReadOnlyList<SkillProgressViewModel> weakSkills,
        IReadOnlyList<SkillProgressViewModel> developingSkills,
        IReadOnlyList<SkillProgressViewModel> strongSkills)
    {
        var revise = weakSkills.FirstOrDefault()?.SkillName ?? developingSkills.FirstOrDefault()?.SkillName;
        var advance = strongSkills.FirstOrDefault()?.SkillName ?? developingSkills.Skip(1).FirstOrDefault()?.SkillName;

        if (!string.IsNullOrWhiteSpace(revise) && !string.IsNullOrWhiteSpace(advance))
        {
            return $"Hoje vale revisar {revise} antes de empurrar a crianca para {advance}.";
        }

        if (!string.IsNullOrWhiteSpace(revise))
        {
            return $"Hoje vale retomar {revise} com material concreto e repeticao guiada.";
        }

        if (!string.IsNullOrWhiteSpace(advance))
        {
            return $"Hoje a crianca pode consolidar e avancar em {advance}.";
        }

        return "Depois das primeiras sessoes registradas, o sistema vai mostrar automaticamente o que revisar, o que avancar e qual apoio externo encaixa melhor.";
    }

    private static string ResolveChildDisplayName(string? fullName, int existingChildrenCount)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? $"Crianca {existingChildrenCount + 1}"
            : fullName.Trim();
    }

    private static DateTime ResolveBirthDate(DateTime? birthDate, int estimatedAgeYears)
    {
        if (birthDate.HasValue)
        {
            return birthDate.Value.Date;
        }

        var safeAge = Math.Clamp(estimatedAgeYears, 3, 14);
        return DateTime.Today.AddYears(-safeAge);
    }

    private static int GetSupportIntensityLevel(SupportProfile supportProfile) => supportProfile switch
    {
        SupportProfile.TeaLevel3 => 5,
        SupportProfile.TeaLevel2 => 4,
        SupportProfile.TeaLevel1 => 3,
        _ => 2
    };

    private static string BuildStrengthsSummary(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.StrengthCurious, "curiosidade e investigacao"),
            (model.StrengthCommunicative, "boa comunicacao"),
            (model.StrengthCreative, "criatividade"),
            (model.StrengthMemorizes, "memoriza com facilidade"),
            (model.StrengthLogical, "gosta de logica e padroes"));
    }

    private static string BuildSupportSummary(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.SupportNeedsFocus, "precisa reforcar foco"),
            (model.SupportNeedsTransitions, "precisa de ajuda nas transicoes"),
            (model.SupportNeedsLanguage, "precisa reforcar linguagem"),
            (model.SupportNeedsRegulation, "precisa reforcar autorregulacao"));
    }

    private static string BuildCommunicationProfile(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.CommunicationDirectLanguage, "entende melhor linguagem direta"),
            (model.CommunicationVisualSupport, "responde melhor com apoio visual"),
            (model.CommunicationModeling, "precisa de modelagem antes de tentar"),
            (model.CommunicationProcessingTime, "precisa de mais tempo para processar"));
    }

    private static string BuildCommonTriggers(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.TriggerUnexpectedChange, "mudanca inesperada"),
            (model.TriggerNoise, "barulho"),
            (model.TriggerWritingDemand, "escrita ou tarefa no papel"),
            (model.TriggerWaiting, "espera"),
            (model.TriggerSocialDemand, "demanda social"));
    }

    private static string BuildCalmingStrategies(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.CalmingPause, "pausa curta"),
            (model.CalmingMovement, "movimento"),
            (model.CalmingQuietCorner, "lugar silencioso"),
            (model.CalmingDrawing, "desenhar"),
            (model.CalmingMusic, "musica calma"));
    }

    private static string BuildSchoolBarrierSummary(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.BarrierAnxietyCrises, "crises de ansiedade ligadas a escola"),
            (model.BarrierSchoolOverload, "sobrecarga sensorial no ambiente escolar"),
            (model.BarrierRigidity, "rigidez cognitiva alta"),
            (model.BarrierSocialDifficulty, "demanda social alta demais"));
    }

    private static string BuildTransitionSupports(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.NeedsVisualRoutine, "rotina visual"),
            (model.NeedsFirstThen, "first-then"),
            (model.NeedsTimer, "timer visual"),
            (model.SupportNeedsTransitions, "aviso antecipado antes da troca"));
    }

    private static string BuildEffectiveReinforcers(CreateChildViewModel model)
    {
        return JoinSelectedLabels(
            (model.CalmingPause, "pausa curta"),
            (model.CalmingMovement, "movimento"),
            (model.CalmingDrawing, "desenhar"),
            (!string.IsNullOrWhiteSpace(model.SpecialInterests), "tempo com interesse especial"));
    }

    private static string JoinSelectedLabels(params (bool IsSelected, string Label)[] items)
    {
        return string.Join("; ", items.Where(item => item.IsSelected).Select(item => item.Label));
    }

    private static bool ContainsStructuredLabel(string? combinedValue, string label)
    {
        return !string.IsNullOrWhiteSpace(combinedValue) &&
               combinedValue.Contains(label, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractUnstructuredText(string? combinedValue, params string[] structuredLabels)
    {
        if (string.IsNullOrWhiteSpace(combinedValue))
        {
            return string.Empty;
        }

        var labels = structuredLabels
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var parts = combinedValue
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment =>
            {
                var tokens = segment
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(token => !labels.Contains(token))
                    .ToList();

                return tokens.Count == 0 ? string.Empty : string.Join("; ", tokens);
            })
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return string.Join(" | ", parts);
    }

    private static string CombineOptionalText(params string?[] values)
    {
        return string.Join(" | ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
    }

    private static List<OnboardingStepViewModel> BuildOnboardingSteps()
    {
        return
        [
            new() { Title = "1. Cadastre a crianca", Description = "Preencha o essencial primeiro. O resto pode ficar para depois." },
            new() { Title = "2. Abra so o Ensinar hoje", Description = "Ignore relatorios e outras areas no inicio. O foco e seguir o passo a passo do dia." },
            new() { Title = "3. Registre em menos de 1 minuto", Description = "Salve minutos, principal acerto, principal dificuldade e uma evidencia." }
        ];
    }

    private static List<AgeTrackPreviewViewModel> BuildAgeTracks()
    {
        return
        [
            new()
            {
                AgeBand = "3 a 4 anos",
                Focus = "linguagem oral, contagem concreta, atencao e autonomia",
                ParentExperience = "rotinas curtas, brincadeiras guiadas e missões simples para criar base cognitiva."
            },
            new()
            {
                AgeBand = "5 a 6 anos",
                Focus = "pre-alfabetizacao, raciocinio matematico e organizacao",
                ParentExperience = "orientacoes objetivas para tirar a familia da improvisacao e criar progresso real."
            },
            new()
            {
                AgeBand = "7 a 8 anos",
                Focus = "leitura, escrita, problemas com estrategia e ciencia em casa",
                ParentExperience = "plano mais forte, com explicacao para o adulto, metas mensais e registro do desenvolvimento."
            },
            new()
            {
                AgeBand = "9 a 10 anos",
                Focus = "compreensao profunda, producao escrita, projetos e autonomia academica",
                ParentExperience = "trilhas mais maduras para familias que querem continuidade no ensino domiciliar com prova de evolucao."
            },
            new()
            {
                AgeBand = "11 a 12 anos",
                Focus = "tese e prova textual, razao e porcentagem, comparacao de fontes e planejamento semanal real",
                ParentExperience = "curriculo proprietario mais forte, com tarefas autorais, unidades anuais claras e mais independencia na rotina."
            },
            new()
            {
                AgeBand = "13 a 14 anos",
                Focus = "argumentacao madura, orcamento e decisao, causa e consequencia e estudo independente com revisao",
                ParentExperience = "anos finais organizados para ensino domiciliar serio, com mais densidade academica sem perder clareza para a familia."
            }
        ];
    }

    private static string GetLearningProfileLabel(string learningProfile) => learningProfile switch
    {
        "hands_on" => "Aprende fazendo",
        "story_based" => "Aprende por historias e conversa",
        "visual" => "Aprende melhor vendo e organizando",
        "movement" => "Aprende melhor em movimento",
        _ => "Equilibrado"
    };

    private static string GetTeachingMethodologyLabel(string teachingMethodology) => teachingMethodology switch
    {
        "montessori" => "Montessori em casa",
        "charlotte_mason" => "Charlotte Mason",
        "classical" => "Classica",
        "singapore_math" => "Singapore Math",
        _ => "Ecletica estruturada"
    };

    private static string GetFamilyGoalTrackLabel(string familyGoalTrack) => familyGoalTrack switch
    {
        "literacy" => "Trilha de alfabetizacao",
        "math_foundations" => "Trilha de matematica base",
        "autonomy" => "Trilha de autonomia e foco",
        "science_discovery" => "Trilha de ciencias em casa",
        _ => "Trilha de crescimento equilibrado"
    };

    private static string GetGuidanceStyleLabel(string guidanceStyle) => guidanceStyle switch
    {
        "confidence" => "Precisa de vitorias rapidas",
        "autonomy" => "Responde bem com autonomia",
        "focus_support" => "Precisa de blocos curtos e foco apoiado",
        _ => "Precisa de conducao passo a passo"
    };

    private static string NormalizeLearningProfile(string? learningProfile) => learningProfile switch
    {
        "hands_on" or "story_based" or "visual" or "movement" => learningProfile,
        _ => "balanced"
    };

    private static string NormalizeTeachingMethodology(string? teachingMethodology) => teachingMethodology switch
    {
        "montessori" or "charlotte_mason" or "classical" or "singapore_math" => teachingMethodology,
        _ => "eclectic"
    };

    private static string NormalizeFamilyGoalTrack(string? familyGoalTrack) => familyGoalTrack switch
    {
        "literacy" or "math_foundations" or "autonomy" or "science_discovery" => familyGoalTrack,
        _ => "balanced_growth"
    };

    private static string NormalizeGuidanceStyle(string? guidanceStyle) => guidanceStyle switch
    {
        "confidence" or "autonomy" or "focus_support" => guidanceStyle,
        _ => "guided"
    };

    private static WeeklyChildReportItemViewModel BuildWeeklyChildReport(ChildProfile child)
    {
        var skills = child.SkillProgressEntries
            .OrderBy(x => x.MasteryScore)
            .ThenByDescending(x => x.TimesSuccessful)
            .ToList();

        var improved = skills
            .Where(x => x.MasteryScore >= 75)
            .OrderByDescending(x => x.TimesSuccessful)
            .ThenByDescending(x => x.MasteryScore)
            .FirstOrDefault();

        var review = skills
            .Where(x => x.MasteryScore < 45)
            .OrderBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .FirstOrDefault();

        var advance = skills
            .Where(x => x.MasteryScore >= 75)
            .OrderByDescending(x => x.MasteryScore)
            .FirstOrDefault()
            ?? skills
                .Where(x => x.MasteryScore >= 45)
                .OrderByDescending(x => x.MasteryScore)
                .FirstOrDefault();

        return new WeeklyChildReportItemViewModel
        {
            ChildId = child.Id,
            ChildName = child.FullName,
            ImprovedSkill = improved?.SkillName ?? "Nenhuma habilidade forte ainda registrada",
            ReviewSkill = review?.SkillName ?? string.Empty,
            AdvanceSkill = advance?.SkillName ?? string.Empty,
            NextWeekRecommendation = BuildChildWeeklyHeadline(new WeeklyChildReportItemViewModel
            {
                ReviewSkill = review?.SkillName ?? string.Empty,
                AdvanceSkill = advance?.SkillName ?? string.Empty
            })
        };
    }

    private static WeeklyFamilyReportViewModel BuildWeeklyFamilyReport(List<WeeklyChildReportItemViewModel> children)
    {
        var improved = children
            .Where(x => !string.IsNullOrWhiteSpace(x.ImprovedSkill) && x.ImprovedSkill != "Nenhuma habilidade forte ainda registrada")
            .Select(x => $"{x.ChildName}: {x.ImprovedSkill}")
            .Take(3)
            .ToList();

        var review = children
            .Where(x => !string.IsNullOrWhiteSpace(x.ReviewSkill))
            .Select(x => $"{x.ChildName}: {x.ReviewSkill}")
            .Take(3)
            .ToList();

        var advance = children
            .Where(x => !string.IsNullOrWhiteSpace(x.AdvanceSkill))
            .Select(x => $"{x.ChildName}: {x.AdvanceSkill}")
            .Take(3)
            .ToList();

        return new WeeklyFamilyReportViewModel
        {
            ImprovedSummary = improved.Count > 0
                ? string.Join(" • ", improved)
                : "Ainda nao ha habilidade suficientemente forte para destacar melhora nesta semana.",
            ReviewSummary = review.Count > 0
                ? string.Join(" • ", review)
                : "Nao ha alerta de reforco forte no momento.",
            AdvanceSummary = advance.Count > 0
                ? string.Join(" • ", advance)
                : "Assim que houver consolidacao, o sistema sugerira o que avancar.",
            Children = children
        };
    }

    private static string BuildChildWeeklyHeadline(WeeklyChildReportItemViewModel child)
    {
        if (!string.IsNullOrWhiteSpace(child.ReviewSkill) && !string.IsNullOrWhiteSpace(child.AdvanceSkill))
        {
            return $"Revisar {child.ReviewSkill} antes de avancar em {child.AdvanceSkill}.";
        }

        if (!string.IsNullOrWhiteSpace(child.ReviewSkill))
        {
            return $"Esta semana vale reforcar {child.ReviewSkill}.";
        }

        if (!string.IsNullOrWhiteSpace(child.AdvanceSkill))
        {
            return $"A crianca ja pode consolidar e avancar em {child.AdvanceSkill}.";
        }

        return "Depois das primeiras sessoes registradas, o sistema vai mostrar o proximo passo ideal.";
    }

    private AdaptiveRoutineSnapshotViewModel MapAdaptiveSnapshot(AdaptiveRoutineSnapshot snapshot)
    {
        return new AdaptiveRoutineSnapshotViewModel
        {
            Summary = snapshot.Summary,
            RecommendedWorkBlockMinutes = snapshot.WorkBlockMinutes,
            RecommendedBreakMinutes = snapshot.BreakMinutes,
            SupportIntensityLabel = AdaptiveRoutineService.GetSupportIntensityLabel(snapshot.Intensity),
            SupportIntensityChipClass = GetAdaptiveIntensityChip(snapshot.Intensity),
            VisualSupportRecommendation = snapshot.VisualSupportRecommendation,
            TransitionRecommendation = snapshot.TransitionRecommendation,
            TomorrowAdjustment = snapshot.TomorrowAdjustment,
            PlanBRecommendation = snapshot.PlanBRecommendation,
            HelpfulSupports = snapshot.HelpfulSupports
                .Select(item => new AdaptiveRoutinePillViewModel
                {
                    Label = item,
                    ChipClass = "success"
                })
                .ToList(),
            CommonTriggers = snapshot.CommonTriggers
                .Select(item => new AdaptiveRoutinePillViewModel
                {
                    Label = item,
                    ChipClass = "warning"
                })
                .ToList()
        };
    }

    private static ChildTeaProfileInputViewModel MapTeaProfileInput(ChildTeaProfile? profile, Guid childId)
    {
        return new ChildTeaProfileInputViewModel
        {
            ChildId = childId,
            CommunicationProfile = profile?.CommunicationProfile ?? string.Empty,
            CommunicationNotes = profile?.CommunicationNotes ?? string.Empty,
            AnxietyLevel = profile?.AnxietyLevel ?? 3,
            CognitiveRigidityLevel = profile?.CognitiveRigidityLevel ?? 3,
            SensorySensitivityLevel = profile?.SensorySensitivityLevel ?? 3,
            TransitionDifficultyLevel = profile?.TransitionDifficultyLevel ?? 3,
            SupportIntensityLevel = profile?.SupportIntensityLevel ?? 3,
            NeedsVisualRoutine = profile?.NeedsVisualRoutine ?? false,
            NeedsFirstThen = profile?.NeedsFirstThen ?? false,
            NeedsTimer = profile?.NeedsTimer ?? false,
            NeedsPlanB = profile?.NeedsPlanB ?? false,
            SpecialInterests = profile?.SpecialInterests ?? string.Empty,
            EffectiveReinforcers = profile?.EffectiveReinforcers ?? string.Empty,
            CommonTriggers = profile?.CommonTriggers ?? string.Empty,
            OverloadSignals = profile?.OverloadSignals ?? string.Empty,
            CalmingStrategies = profile?.CalmingStrategies ?? string.Empty,
            TransitionSupports = profile?.TransitionSupports ?? string.Empty,
            DailyLivingPriorities = profile?.DailyLivingPriorities ?? string.Empty,
            ParentPrimaryGoal = profile?.ParentPrimaryGoal ?? string.Empty,
            SchoolBarrierSummary = profile?.SchoolBarrierSummary ?? string.Empty,
            DocumentationNotes = profile?.DocumentationNotes ?? string.Empty
        };
    }

    private static List<TeaProfileInsightViewModel> BuildTeaProfileInsights(ChildTeaProfile? profile, SupportProfile supportProfile)
    {
        if (profile is null)
        {
            return
            [
                new TeaProfileInsightViewModel
                {
                    Label = "Perfil funcional",
                    Value = "Ainda nao preenchido. Complete a ficha para o sistema adaptar melhor a rotina.",
                    ChipClass = "neutral"
                }
            ];
        }

        return
        [
            new TeaProfileInsightViewModel
            {
                Label = "Suporte atual",
                Value = $"{GetSupportProfileLabel(supportProfile)} com intensidade {profile.SupportIntensityLevel}/5",
                ChipClass = "neutral"
            },
            new TeaProfileInsightViewModel
            {
                Label = "Rigidez cognitiva",
                Value = $"{profile.CognitiveRigidityLevel}/5",
                ChipClass = profile.CognitiveRigidityLevel >= 4 ? "warning" : "neutral"
            },
            new TeaProfileInsightViewModel
            {
                Label = "Ansiedade",
                Value = $"{profile.AnxietyLevel}/5",
                ChipClass = profile.AnxietyLevel >= 4 ? "warning" : "neutral"
            },
            new TeaProfileInsightViewModel
            {
                Label = "Transicoes",
                Value = $"{profile.TransitionDifficultyLevel}/5",
                ChipClass = profile.TransitionDifficultyLevel >= 4 ? "warning" : "neutral"
            },
            new TeaProfileInsightViewModel
            {
                Label = "Meta da familia",
                Value = string.IsNullOrWhiteSpace(profile.ParentPrimaryGoal) ? "Ainda nao definida." : profile.ParentPrimaryGoal,
                ChipClass = "success"
            }
        ];
    }

    private static AdaptiveObservationHistoryViewModel MapObservation(ChildRoutineObservation observation)
    {
        return new AdaptiveObservationHistoryViewModel
        {
            ObservedAt = observation.ObservedAt,
            ContextLabel = string.IsNullOrWhiteSpace(observation.ContextPeriod) ? "Registro adaptativo" : observation.ContextPeriod,
            Antecedent = observation.Antecedent,
            ChildReaction = observation.ChildReaction,
            WhatHelped = observation.WhatHelped,
            SupportUsed = observation.SupportUsed,
            DistressLevel = observation.DistressLevel,
            TaskToleranceMinutes = observation.TaskToleranceMinutes,
            NeededPlanB = observation.NeededPlanB
        };
    }

    private static string BuildAdaptiveBlockCue(DailyPlanBlock block, AdaptiveRoutineSnapshot snapshot, int index)
    {
        var transitionCue = index == 0
            ? "Comece mostrando o inicio e o fim do bloco."
            : "Anuncie a troca antes de sair da atividade anterior.";
        var trackCue = block.FunctionalTrack switch
        {
            FunctionalSupportTrack.Communication => "Mantenha frase curta, modelo direto e resposta esperada bem visivel.",
            FunctionalSupportTrack.Regulation => "Baixe o ritmo, nomeie a sensacao e mantenha um passo por vez.",
            FunctionalSupportTrack.Sensory => "Observe ambiente, som, textura e postura antes de aumentar a exigencia.",
            FunctionalSupportTrack.DailyLiving => "Demonstre primeiro, depois peça uma imitacao simples e funcional.",
            FunctionalSupportTrack.AcademicAdapted => "Preserve o objetivo academico, mas reduza a carga por tentativa.",
            _ => "Mantenha previsibilidade e final claro."
        };

        return $"{transitionCue} {trackCue} Timer sugerido: {snapshot.WorkBlockMinutes} min.";
    }

    private static string BuildDocumentationSummary(ChildProfile child, AdaptiveRoutineSnapshot snapshot, int observationCount)
    {
        var teaGoal = child.TeaProfile?.ParentPrimaryGoal;
        var barrier = child.TeaProfile?.SchoolBarrierSummary;
        return $"Este dossie consolida rotina, evidencias, progresso por habilidade e {observationCount} observacao(oes) adaptativa(s). Meta principal da familia: {(string.IsNullOrWhiteSpace(teaGoal) ? "ainda nao definida" : teaGoal)}. Contexto atual: {(string.IsNullOrWhiteSpace(barrier) ? "a familia ainda nao detalhou as barreiras escolares" : barrier)}. Ajuste adaptativo atual: {snapshot.TomorrowAdjustment}";
    }

    private static List<string> BuildRecommendedDocuments()
    {
        return
        [
            "Linha do tempo semanal com sessao, duracao e evidencias.",
            "Resumo das adaptacoes que reduziram crise, rigidez e travamento.",
            "Avaliacoes curtas por habilidade com data e criterio de acerto.",
            "Historico de rotina, faltas por crise e retomadas aplicadas.",
            "Portifolio visual com imagem, video ou producao da crianca."
        ];
    }

    private static string GetAdaptiveIntensityChip(AdaptiveSupportIntensity intensity) => intensity switch
    {
        AdaptiveSupportIntensity.High => "warning",
        AdaptiveSupportIntensity.Moderate => "neutral",
        _ => "success"
    };

    private async Task<EvidenceCommitResult> TryCommitEvidenceChangesAsync(
        Guid parentId,
        bool requiresStorageSlot,
        IReadOnlyCollection<string> uploadedMediaUrls,
        string persistenceFailureMessage)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (requiresStorageSlot)
        {
            var allowance = await evidenceStoragePlanService.BuildAllowanceAsync(parentId);
            if (!allowance.CanUpload)
            {
                await transaction.RollbackAsync();
                DeleteEvidenceFiles(uploadedMediaUrls);
                return EvidenceCommitResult.QuotaExceeded(allowance.Message);
            }
        }

        try
        {
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return EvidenceCommitResult.Ok();
        }
        catch
        {
            await transaction.RollbackAsync();
            DeleteEvidenceFiles(uploadedMediaUrls);
            return EvidenceCommitResult.PersistenceFailure(persistenceFailureMessage);
        }
    }

    private sealed record EvidenceCommitResult(bool Success, int StatusCode, string Message)
    {
        public static EvidenceCommitResult Ok() => new(true, StatusCodes.Status200OK, string.Empty);

        public static EvidenceCommitResult QuotaExceeded(string message) =>
            new(false, StatusCodes.Status409Conflict, message);

        public static EvidenceCommitResult PersistenceFailure(string message) =>
            new(false, StatusCodes.Status500InternalServerError, message);
    }

}
