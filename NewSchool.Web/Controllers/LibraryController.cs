using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Services;

namespace NewSchool.Web.Controllers;

[Authorize(Roles = "Parent")]
public class LibraryController(
    ApplicationDbContext db,
    FamilyLibraryService familyLibraryService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? childId = null, string? q = null, string? collection = null, string? stage = null, string? age = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        var userId = GetCurrentUserId();
        var child = await ResolveChildAsync(userId, childId);
        var vm = await familyLibraryService.BuildHomeAsync(userId, child, q, collection, stage, age, Url);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Book(Guid id, Guid? childId = null, int? pageNumber = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        var userId = GetCurrentUserId();
        var vm = await familyLibraryService.BuildReaderAsync(userId, id, pageNumber);
        if (vm is null)
        {
            return NotFound();
        }

        vm.SelectedChildId = childId;
        vm.BackLibraryUrl = Url.Action("Index", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty;
        vm.BackPrintablesUrl = Url.Action("Printables", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(Guid id, string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        await familyLibraryService.ToggleFavoriteAsync(GetCurrentUserId(), id);
        return RedirectToLocalOrAction(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkWeeklyReadingComplete(
        Guid childId,
        Guid materialId,
        int phaseNumber,
        string? periodKey = null,
        int weekNumber = 1,
        string? phaseLabel = null,
        string? goalLabel = null,
        string? returnUrl = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        var userId = GetCurrentUserId();
        var child = await ResolveChildAsync(userId, childId);
        if (child is null)
        {
            return NotFound();
        }

        await familyLibraryService.MarkWeeklyReadingCompleteAsync(
            userId,
            childId,
            materialId,
            phaseNumber,
            periodKey,
            weekNumber,
            phaseLabel,
            goalLabel);

        return RedirectToLocalOrAction(returnUrl, nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Printables(Guid? childId = null, string? q = null, string? category = null, string? stage = null, string? age = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        var child = await ResolveChildAsync(GetCurrentUserId(), childId);
        var vm = await familyLibraryService.BuildPrintablesAsync(child, q, category, stage, age);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Printable(Guid id, Guid? childId = null)
    {
        if (!IsSubscriptionActive())
        {
            return RedirectToAction("Index", "Parent", new { gate = "premium" });
        }

        var vm = await familyLibraryService.BuildPrintableAsync(id);
        if (vm is null)
        {
            return NotFound();
        }

        vm.SelectedChildId = childId;
        vm.BackLibraryUrl = Url.Action("Index", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty;
        vm.BackPrintablesUrl = Url.Action("Printables", "Library", childId.HasValue ? new { childId } : null) ?? string.Empty;
        return View(vm);
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private bool IsSubscriptionActive()
    {
        return User.Identity?.IsAuthenticated == true && User.IsInRole("Parent");
    }

    private IActionResult RedirectToLocalOrAction(string? returnUrl, string fallbackAction)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(fallbackAction);
    }

    private Task<ChildProfile?> ResolveChildAsync(Guid userId, Guid? childId)
    {
        if (!childId.HasValue)
        {
            return Task.FromResult<ChildProfile?>(null);
        }

        return db.Children
            .FirstOrDefaultAsync(child => child.Id == childId.Value && child.ParentId == userId);
    }
}
