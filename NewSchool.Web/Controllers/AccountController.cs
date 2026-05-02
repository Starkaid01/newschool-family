using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NewSchool.Web.Models;
using NewSchool.Web.Services;

namespace NewSchool.Web.Controllers;

public class AccountController(
    ReferralService referralService,
    AuthService authService,
    EmailAutomationService emailAutomationService) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }

        return View(new LoginViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Register(string? refCode = null, string? @ref = null, string? track = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }

        var incomingCode = string.IsNullOrWhiteSpace(@ref) ? refCode : @ref;
        var normalizedTrack = NormalizeTrack(track);
        if (!string.IsNullOrWhiteSpace(incomingCode))
        {
            var referrer = await referralService.FindReferrerAsync(incomingCode);
            return View(new RegisterViewModel
            {
                ReferralCode = referrer?.ReferralCode ?? incomingCode,
                IntendedTrack = normalizedTrack
            });
        }

        return View(new RegisterViewModel
        {
            IntendedTrack = normalizedTrack
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await authService.AuthenticateAsync(model.Email, model.Password);
        if (user is null)
        {
            model.ErrorMessage = "Email ou senha invalidos.";
            return View(model);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            authService.CreatePrincipal(user));

        return user.Role == Domain.UserRole.Admin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Parent");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.RegisterParentAsync(
            model.FullName,
            model.Email,
            model.Password,
            model.ReferralCode,
            model.IntendedTrack);
        if (!result.Success || result.User is null)
        {
            model.ErrorMessage = result.Error ?? "Nao foi possivel criar sua conta agora.";
            return View(model);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            authService.CreatePrincipal(result.User));
        await emailAutomationService.SendOnboardingEmailIfNeededAsync(result.User);
        if (!string.IsNullOrWhiteSpace(model.IntendedTrack))
        {
            return RedirectToAction("CreateChild", "Parent", new { goalTrack = model.IntendedTrack });
        }

        return RedirectToAction("Index", "Parent");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToRoleHome()
    {
        return User.IsInRole("Admin")
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Parent");
    }

    private static string NormalizeTrack(string? track) => track switch
    {
        "literacy" or "math_foundations" or "autonomy" or "science_discovery" => track,
        _ => string.Empty
    };
}
