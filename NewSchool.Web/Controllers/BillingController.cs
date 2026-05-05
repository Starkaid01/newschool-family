using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using NewSchool.Web.Services;
using Stripe;
using Stripe.Checkout;

namespace NewSchool.Web.Controllers;

[ApiController]
public class BillingController(
    ApplicationDbContext db,
    IOptions<StripeSettings> stripeOptions,
    EvidenceStoragePlanService evidenceStoragePlanService,
    StripeBillingService stripeBillingService) : ControllerBase
{
    private readonly StripeSettings _stripe = stripeOptions.Value;

    [Authorize(Roles = "Parent")]
    [ValidateAntiForgeryToken]
    [HttpPost("/billing/create-checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateStorageCheckoutRequest? request)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.PlanCode))
            {
                return BadRequest(new { error = "Escolha um plano de armazenamento antes de continuar." });
            }

            var planCode = request.PlanCode.Trim().ToLowerInvariant();
            if (string.Equals(planCode, EvidenceStoragePlanService.FreePlanCode, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "O plano gratuito nao precisa de checkout." });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await db.Users.FirstAsync(x => x.Id == userId);
            var returnUrl = NormalizeLocalReturnUrl(request.ReturnUrl);
            var requestedExtraBlocks = request.ExtraBlocks;

            if (string.Equals(planCode, EvidenceStoragePlanService.Extra100FilesAddonCode, StringComparison.OrdinalIgnoreCase))
            {
                if (!evidenceStoragePlanService.CanPurchaseExtraStorage(user))
                {
                    return BadRequest(new { error = "O extra de 100 arquivos aparece para famílias que já estão no plano 5.000 arquivos." });
                }

                planCode = EvidenceStoragePlanService.Files5000PlanCode;
                requestedExtraBlocks = evidenceStoragePlanService.GetCurrentExtraBlocks(user) + 1;
            }

            if (HasManagedSubscription(user))
            {
                try
                {
                    await evidenceStoragePlanService.UpdateSubscriptionPlanAsync(user, planCode, requestedExtraBlocks);
                    var updatedUrl = BuildRelativeUrl(returnUrl, new Dictionary<string, string?>
                    {
                        ["billing"] = "updated"
                    });

                    return Ok(new
                    {
                        redirectUrl = updatedUrl,
                        message = "Plano de armazenamento atualizado."
                    });
                }
                catch (StripeException ex) when (IsMissingSubscription(ex))
                {
                    await ClearStaleSubscriptionAsync(user);
                }
            }

            var lineItems = await evidenceStoragePlanService.BuildCheckoutLineItemsAsync(planCode, requestedExtraBlocks);
            var domain = $"{Request.Scheme}://{Request.Host}";
            var successUrl = BuildAbsoluteUrl(domain, returnUrl, new Dictionary<string, string?>
            {
                ["checkout"] = "success",
                ["session_id"] = "{CHECKOUT_SESSION_ID}"
            });
            var cancelUrl = BuildAbsoluteUrl(domain, returnUrl, new Dictionary<string, string?>
            {
                ["checkout"] = "cancel"
            });

            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = user.Email,
                ClientReferenceId = user.Id.ToString(),
                LineItems = lineItems
            };

            if (!string.IsNullOrWhiteSpace(user.StripeCustomerId))
            {
                options.Customer = user.StripeCustomerId;
                options.CustomerEmail = null;
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Ok(new { url = session.Url });
        }
        catch (StripeException ex)
        {
            return StatusCode(500, new { error = ex.StripeError?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Parent")]
    [ValidateAntiForgeryToken]
    [HttpPost("/billing/cancel-subscription")]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await stripeBillingService.CancelSubscriptionAsync(userId, notifyUser: true);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error ?? "Não foi possível cancelar a assinatura." });
        }

        var user = await db.Users.FirstAsync(x => x.Id == userId);
        return Ok(new
        {
            status = user.SubscriptionStatus,
            currentPeriodEnd = user.SubscriptionCurrentPeriodEnd
        });
    }

    [Authorize(Roles = "Parent")]
    [ValidateAntiForgeryToken]
    [HttpPost("/billing/create-portal-session")]
    public async Task<IActionResult> CreatePortalSession([FromBody] CreateBillingPortalRequest? request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var returnUrl = NormalizeLocalReturnUrl(request?.ReturnUrl);
        var domain = $"{Request.Scheme}://{Request.Host}";
        var fullReturnUrl = BuildRelativeUrl(returnUrl, new Dictionary<string, string?>
        {
            ["billing"] = "return"
        });
        var sessionUrl = await stripeBillingService.CreatePortalSessionUrlAsync(userId, domain, fullReturnUrl);
        if (string.IsNullOrWhiteSpace(sessionUrl))
        {
            return BadRequest(new { error = "Nao encontramos um cliente Stripe para este usuario." });
        }

        return Ok(new { url = sessionUrl });
    }

    [AllowAnonymous]
    [HttpPost("/api/webhook/stripe")]
    public async Task<IActionResult> SnapshotWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, _stripe.WebhookSecretSnapshot);
        }
        catch
        {
            return BadRequest();
        }

        await HandleStripeEventAsync(stripeEvent);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("/api/min/webhook/stripe")]
    public async Task<IActionResult> ThinWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, _stripe.WebhookSecretMin);
        }
        catch
        {
            return BadRequest();
        }

        if (!string.IsNullOrWhiteSpace(stripeEvent.Id))
        {
            var eventService = new EventService();
            stripeEvent = await eventService.GetAsync(stripeEvent.Id);
        }

        await HandleStripeEventAsync(stripeEvent);
        return Ok();
    }

    private async Task HandleStripeEventAsync(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                if (stripeEvent.Data.Object is Session session)
                {
                    await HandleCheckoutCompletedAsync(session);
                }
                break;

            case "customer.subscription.created":
            case "customer.subscription.updated":
                if (stripeEvent.Data.Object is Subscription subscription)
                {
                    await HandleSubscriptionUpdatedAsync(subscription);
                }
                break;

            case "customer.subscription.deleted":
                if (stripeEvent.Data.Object is Subscription deletedSubscription)
                {
                    await HandleSubscriptionDeletedAsync(deletedSubscription);
                }
                break;

            case "invoice.payment_failed":
                if (stripeEvent.Data.Object is Invoice failedInvoice)
                {
                    var failedUser = await FindUserByCustomerIdAsync(failedInvoice.CustomerId);
                    if (failedUser is null)
                    {
                        return;
                    }

                    failedUser.SubscriptionStatus = "past_due";
                    await db.SaveChangesAsync();
                }
                break;

            case "invoice.paid":
                if (stripeEvent.Data.Object is Invoice paidInvoice)
                {
                    var paidUser = await FindUserByCustomerIdAsync(paidInvoice.CustomerId);
                    if (paidUser is null)
                    {
                        return;
                    }

                    var paidSubscriptionId = paidInvoice.Parent?.SubscriptionDetails?.SubscriptionId;
                    if (!string.IsNullOrWhiteSpace(paidSubscriptionId))
                    {
                        var subscriptionService = new SubscriptionService();
                        var paidSubscription = await subscriptionService.GetAsync(paidSubscriptionId);
                        evidenceStoragePlanService.ApplySubscriptionToUser(paidUser, paidSubscription);
                    }
                    else
                    {
                        paidUser.SubscriptionStatus = "active";
                    }

                    await db.SaveChangesAsync();
                }
                break;
        }
    }

    private async Task HandleCheckoutCompletedAsync(Session session)
    {
        if (string.IsNullOrWhiteSpace(session.ClientReferenceId) || !Guid.TryParse(session.ClientReferenceId, out var userId))
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
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
    }

    private async Task HandleSubscriptionUpdatedAsync(Subscription subscription)
    {
        var user = await FindUserByCustomerIdAsync(subscription.CustomerId);
        if (user is null)
        {
            return;
        }

        evidenceStoragePlanService.ApplySubscriptionToUser(user, subscription);
        await db.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeletedAsync(Subscription subscription)
    {
        var user = await FindUserByCustomerIdAsync(subscription.CustomerId);
        if (user is null)
        {
            return;
        }

        user.SubscriptionStatus = "canceled";
        user.StripeSubscriptionId = null;
        user.SubscriptionCurrentPeriodEnd = subscription.Items?.Data?.Any() == true
            ? subscription.Items.Data.Max(item => item.CurrentPeriodEnd)
            : subscription.CanceledAt;
        evidenceStoragePlanService.DowngradeToFree(user);
        await db.SaveChangesAsync();
    }

    private Task<AppUser?> FindUserByCustomerIdAsync(string? customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Task.FromResult<AppUser?>(null);
        }

        return db.Users.FirstOrDefaultAsync(user => user.StripeCustomerId == customerId);
    }

    private static bool HasManagedSubscription(AppUser user)
    {
        return !string.IsNullOrWhiteSpace(user.StripeSubscriptionId) &&
               user.SubscriptionStatus is "active" or "trialing" or "past_due" or "unpaid";
    }

    private async Task ClearStaleSubscriptionAsync(AppUser user)
    {
        user.StripeSubscriptionId = null;
        user.SubscriptionStatus = string.Empty;
        user.SubscriptionCurrentPeriodEnd = null;
        evidenceStoragePlanService.DowngradeToFree(user);
        await db.SaveChangesAsync();
    }

    private static bool IsMissingSubscription(StripeException exception)
    {
        return string.Equals(exception.StripeError?.Code, "resource_missing", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("No such subscription", StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeLocalReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl!;
        }

        return Url.Action("Index", "Parent") ?? "/Parent/Index";
    }

    private static string BuildRelativeUrl(string returnUrl, IDictionary<string, string?> query)
    {
        return QueryHelpers.AddQueryString(returnUrl, query!);
    }

    private static string BuildAbsoluteUrl(string domain, string returnUrl, IDictionary<string, string?> query)
    {
        return $"{domain}{BuildRelativeUrl(returnUrl, query)}";
    }
}
