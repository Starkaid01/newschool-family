using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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

    public async Task<(bool Success, string? Error, AppUser? User)> UpdateProfileAsync(Guid userId, string fullName, string email)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return (false, "Usuário não encontrado.", null);
        }

        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return (false, "Informe um email válido.", null);
        }

        var emailInUse = await db.Users.AnyAsync(x => x.Id != userId && x.Email == normalizedEmail);
        if (emailInUse)
        {
            return (false, "Já existe outra conta usando esse email.", null);
        }

        user.FullName = (fullName ?? string.Empty).Trim();
        user.Email = normalizedEmail;
        await db.SaveChangesAsync();
        return (true, null, user);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return (false, "Usuário não encontrado.");
        }

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            return (false, "A senha atual não confere.");
        }

        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        user.LastActiveAt = DateTime.UtcNow;
        user.PasswordResetTokenHash = string.Empty;
        user.PasswordResetRequestedAt = null;
        user.PasswordResetExpiresAt = null;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error, AppUser? User, string TemporaryPassword)> AdminResetPasswordAsync(Guid userId)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return (false, "Usuário não encontrado.", null, string.Empty);
        }

        var temporaryPassword = GenerateStrongPassword();
        user.PasswordHash = passwordHasher.HashPassword(user, temporaryPassword);
        user.PasswordResetTokenHash = string.Empty;
        user.PasswordResetRequestedAt = null;
        user.PasswordResetExpiresAt = null;
        await db.SaveChangesAsync();

        return (true, null, user, temporaryPassword);
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

    public async Task<(AppUser? User, string? Token)> GeneratePasswordResetTokenAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null)
        {
            return (null, null);
        }

        var rawTokenBytes = RandomNumberGenerator.GetBytes(48);
        var rawToken = WebEncoders.Base64UrlEncode(rawTokenBytes);
        user.PasswordResetTokenHash = ComputeTokenHash(rawToken);
        user.PasswordResetRequestedAt = DateTime.UtcNow;
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(2);

        await db.SaveChangesAsync();
        return (user, rawToken);
    }

    public async Task<bool> IsPasswordResetTokenValidAsync(string email, string token)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null)
        {
            return false;
        }

        return IsPasswordResetTokenValid(user, token);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null || !IsPasswordResetTokenValid(user, token))
        {
            return (false, "O link de redefinição expirou ou não é mais válido.");
        }

        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        user.PasswordResetTokenHash = string.Empty;
        user.PasswordResetRequestedAt = null;
        user.PasswordResetExpiresAt = null;
        user.LastActiveAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return (true, null);
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

    private static string NormalizeEmail(string email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static bool IsPasswordResetTokenValid(AppUser user, string token)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(user.PasswordResetTokenHash) ||
            !user.PasswordResetExpiresAt.HasValue ||
            user.PasswordResetExpiresAt.Value < DateTime.UtcNow)
        {
            return false;
        }

        var incomingHash = ComputeTokenHash(token);
        return string.Equals(user.PasswordResetTokenHash, incomingHash, StringComparison.Ordinal);
    }

    private static string ComputeTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateStrongPassword()
    {
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        const string symbols = "!@#$%*?";
        var all = lower + upper + digits + symbols;

        var chars = new List<char>
        {
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
        };

        while (chars.Count < 14)
        {
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

}
