using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class AuthService(
    ApplicationDbContext db,
    IPasswordHasher<AppUser> passwordHasher)
{
    public ClaimsPrincipal CreatePrincipal(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("subscription_status", user.SubscriptionStatus ?? "inactive")
        };

        if (user.TrialEndsAt.HasValue)
        {
            claims.Add(new Claim("trial_ends_at", user.TrialEndsAt.Value.ToString("O")));
        }

        if (user.SubscriptionCurrentPeriodEnd.HasValue)
        {
            claims.Add(new Claim("subscription_period_end", user.SubscriptionCurrentPeriodEnd.Value.ToString("O")));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public async Task<AppUser?> AuthenticateAsync(string email, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email.Trim().ToLower());
        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result != PasswordVerificationResult.Failed)
        {
            user.LastActiveAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task<(bool Success, string? Error, AppUser? User)> RegisterParentAsync(
        string fullName,
        string email,
        string password,
        string? referralCode = null,
        string? acquisitionTrack = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            return (false, "Ja existe uma conta com esse email.", null);
        }

        AppUser? referrer = null;
        if (!string.IsNullOrWhiteSpace(referralCode))
        {
            var normalizedReferral = referralCode.Trim().ToUpperInvariant();
            referrer = await db.Users.FirstOrDefaultAsync(x => x.ReferralCode == normalizedReferral);
        }

        var user = new AppUser
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            AcquisitionTrack = NormalizeAcquisitionTrack(acquisitionTrack),
            ReferralCode = await GenerateReferralCodeAsync(fullName),
            ReferredByUserId = referrer?.Id,
            Role = UserRole.Parent,
            SubscriptionStatus = "inactive",
            TrialStartedAt = DateTime.UtcNow.Date,
            TrialEndsAt = DateTime.UtcNow.Date.AddDays(7),
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (true, null, user);
    }

    private async Task<string> GenerateReferralCodeAsync(string fullName)
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
            var candidate = $"{seed}{Random.Shared.Next(100, 999)}";
            if (!await db.Users.AnyAsync(x => x.ReferralCode == candidate))
            {
                return candidate;
            }
        }

        return $"{Guid.NewGuid():N}"[..10].ToUpperInvariant();
    }

    private static string NormalizeAcquisitionTrack(string? track)
    {
        var candidate = track;
        if (candidate is "literacy" or "math_foundations" or "autonomy" or "science_discovery")
        {
            return candidate;
        }

        return string.Empty;
    }

}
