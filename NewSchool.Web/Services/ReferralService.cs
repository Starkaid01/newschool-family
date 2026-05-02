using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ReferralService(ApplicationDbContext db)
{
    public async Task EnsureReferralCodeAsync(Guid userId)
    {
        var user = await db.Users.FirstAsync(x => x.Id == userId);
        if (!string.IsNullOrWhiteSpace(user.ReferralCode))
        {
            return;
        }

        user.ReferralCode = await GenerateUniqueCodeAsync(user.FullName);
        await db.SaveChangesAsync();
    }

    public async Task<AppUser?> FindReferrerAsync(string referralCode)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
        {
            return null;
        }

        var normalized = referralCode.Trim().ToUpperInvariant();
        return await db.Users.FirstOrDefaultAsync(x => x.ReferralCode == normalized);
    }

    public async Task<ReferralSummaryViewModel> BuildReferralSummaryAsync(Guid userId, string baseUrl)
    {
        var user = await db.Users
            .Include(x => x.Referrals)
            .FirstAsync(x => x.Id == userId);

        if (string.IsNullOrWhiteSpace(user.ReferralCode))
        {
            user.ReferralCode = await GenerateUniqueCodeAsync(user.FullName);
            await db.SaveChangesAsync();
        }

        var registerUrl = $"{baseUrl.TrimEnd('/')}/Account/Register?ref={user.ReferralCode}";
        var referralUrl = $"{baseUrl.TrimEnd('/')}/convite/{user.ReferralCode}";
        var shareMessage = $"Estamos usando o NewSchool para organizar ensino domiciliar com plano diario, evolucao e relatorios. Se quiser testar, abra meu convite: {referralUrl}";
        return new ReferralSummaryViewModel
        {
            ReferralCode = user.ReferralCode,
            ReferralUrl = referralUrl,
            RegisterUrl = registerUrl,
            ShareMessage = shareMessage,
            WhatsAppShareUrl = $"https://wa.me/?text={Uri.EscapeDataString(shareMessage)}",
            TotalReferrals = user.Referrals.Count,
            ActiveReferrals = user.Referrals.Count(x => x.SubscriptionStatus == "active"),
            RecentReferrals = user.Referrals
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new ReferralActivityViewModel
                {
                    FullName = x.FullName,
                    SubscriptionStatus = x.SubscriptionStatus,
                    CreatedAt = x.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<ReferralLandingViewModel?> BuildReferralLandingAsync(string referralCode, string baseUrl)
    {
        var referrer = await db.Users
            .Include(x => x.Referrals)
            .Include(x => x.Children)
            .ThenInclude(x => x.MonthlySnapshots)
            .FirstOrDefaultAsync(x => x.ReferralCode == referralCode.Trim().ToUpperInvariant());

        if (referrer is null)
        {
            return null;
        }

        var snapshotsRecorded = referrer.Children.Sum(x => x.MonthlySnapshots.Count);
        var firstName = FirstName(referrer.FullName);

        return new ReferralLandingViewModel
        {
            ReferrerName = firstName,
            ReferralCode = referrer.ReferralCode,
            InvitationHeadline = $"{firstName} usa o NewSchool para dar mais clareza ao ensino domiciliar em casa.",
            InvitationMessage = "Voce nao precisa improvisar tudo sozinho. O NewSchool organiza o plano diario, mostra a evolucao da crianca e ajuda a familia a sentir que esta no caminho certo.",
            RegisterUrl = $"{baseUrl.TrimEnd('/')}/Account/Register?ref={referrer.ReferralCode}",
            ActiveReferrals = referrer.Referrals.Count(x => x.SubscriptionStatus == "active"),
            TotalReferrals = referrer.Referrals.Count,
            ChildrenInRoutine = referrer.Children.Count,
            SnapshotsRecorded = snapshotsRecorded,
            ProofPoints =
            [
                "Plano diario automatico com explicacao simples para o adulto",
                "Diagnostico inicial, metas mensais e historico de evolucao",
                "Registro com fotos, videos e portfolio guardavel",
                "Intervencao automatica quando a rotina ou as metas entram em risco"
            ]
        };
    }

    public async Task<AchievementShareCardViewModel> BuildAchievementShareCardAsync(Guid parentId, Guid childId, string baseUrl)
    {
        var parent = await db.Users
            .Include(x => x.Children)
            .ThenInclude(x => x.MonthlyGoalCycles)
            .Include(x => x.Children)
            .ThenInclude(x => x.LearningSessions)
            .Include(x => x.Children)
            .ThenInclude(x => x.Achievements)
            .FirstAsync(x => x.Id == parentId);

        if (string.IsNullOrWhiteSpace(parent.ReferralCode))
        {
            parent.ReferralCode = await GenerateUniqueCodeAsync(parent.FullName);
            await db.SaveChangesAsync();
        }

        var child = parent.Children.First(x => x.Id == childId);
        var badge = child.Achievements
            .OrderByDescending(x => x.EarnedAt)
            .FirstOrDefault();
        var currentCycle = child.MonthlyGoalCycles
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .FirstOrDefault();

        var landingUrl = $"{baseUrl.TrimEnd('/')}/convite/{parent.ReferralCode}";
        var registerUrl = $"{baseUrl.TrimEnd('/')}/Account/Register?ref={parent.ReferralCode}";
        var badgeTitle = badge?.Title ?? "Evolucao em movimento";
        var badgeDescription = badge?.Description ?? "A rotina da familia esta virando progresso visivel.";
        var progressPercent = currentCycle?.ProgressPercent ?? 0;
        var sessionsThisMonth = child.LearningSessions.Count(x => x.LoggedAt.Year == DateTime.UtcNow.Year && x.LoggedAt.Month == DateTime.UtcNow.Month);
        var evidenceCount = child.LearningSessions.Count(x => !string.IsNullOrWhiteSpace(x.MediaUrl));
        var shareMessage = $"{child.FullName} acaba de registrar \"{badgeTitle}\" no NewSchool. Estamos usando a plataforma para organizar ensino domiciliar com rotina, evolucao e portfolio. Veja o convite: {landingUrl}";

        return new AchievementShareCardViewModel
        {
            ChildId = child.Id,
            ChildName = child.FullName,
            ParentFirstName = FirstName(parent.FullName),
            BadgeTitle = badgeTitle,
            BadgeDescription = badgeDescription,
            MomentumSummary = progressPercent >= 70
                ? "A crianca esta com ritmo forte neste ciclo e a familia consegue provar isso com registros e metas."
                : "A familia esta construindo consistencia com plano diario, evidencias e acompanhamento claro.",
            PortfolioUrl = $"{baseUrl.TrimEnd('/')}/Parent/PremiumPortfolio/{child.Id}",
            ReferralLandingUrl = landingUrl,
            RegisterUrl = registerUrl,
            ShareMessage = shareMessage,
            WhatsAppShareUrl = $"https://wa.me/?text={Uri.EscapeDataString(shareMessage)}",
            SessionsThisMonth = sessionsThisMonth,
            EvidenceCount = evidenceCount,
            ProgressPercent = progressPercent
        };
    }

    private async Task<string> GenerateUniqueCodeAsync(string fullName)
    {
        var seed = string.Concat((fullName ?? "FAMILIA")
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(6));
        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = "FAMILIA";
        }

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var suffix = Random.Shared.Next(100, 999);
            var code = $"{seed}{suffix}";
            var exists = await db.Users.AnyAsync(x => x.ReferralCode == code);
            if (!exists)
            {
                return code;
            }
        }

        return $"{Guid.NewGuid():N}"[..10].ToUpperInvariant();
    }

    private static string FirstName(string fullName)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? "Uma familia"
            : fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? fullName;
    }
}
