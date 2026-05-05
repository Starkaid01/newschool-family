using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using Stripe;

namespace NewSchool.Web.Services;

public class StripeBillingService(
    ApplicationDbContext db,
    IOptions<StripeSettings> stripeOptions,
    EvidenceStoragePlanService evidenceStoragePlanService,
    UserNotificationService userNotificationService,
    IMemoryCache memoryCache)
{
    private readonly StripeSettings _stripe = stripeOptions.Value;

    public async Task<(bool Success, string? Error)> CancelSubscriptionAsync(Guid userId, bool notifyUser, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return (false, "Usuário não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
        {
            return (false, "Nenhuma assinatura ativa foi encontrada para cancelar.");
        }

        try
        {
            var service = new SubscriptionService();
            var subscription = await service.CancelAsync(user.StripeSubscriptionId, null, cancellationToken: cancellationToken);

            user.SubscriptionStatus = subscription.Status ?? "canceled";
            user.SubscriptionCurrentPeriodEnd = subscription.Items?.Data?.Any() == true
                ? subscription.Items.Data.Max(item => item.CurrentPeriodEnd)
                : subscription.CanceledAt;
            user.StripeSubscriptionId = null;
            evidenceStoragePlanService.DowngradeToFree(user);
            await db.SaveChangesAsync(cancellationToken);

            if (notifyUser)
            {
                await userNotificationService.CreateAsync(
                    user.Id,
                    "Plano cancelado",
                    "Seu plano pago foi cancelado. O sistema continua livre e o acervo volta ao limite do plano gratuito.",
                    "warning",
                    "/Parent/Plans",
                    null,
                    true,
                    cancellationToken);
            }

            return (true, null);
        }
        catch (StripeException ex)
        {
            return (false, ex.StripeError?.Message ?? ex.Message);
        }
    }

    public async Task<string?> CreatePortalSessionUrlAsync(Guid userId, string domain, string returnUrl, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.StripeCustomerId))
        {
            return null;
        }

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = user.StripeCustomerId,
            ReturnUrl = $"{domain}{returnUrl}"
        }, cancellationToken: cancellationToken);

        return session.Url;
    }

    public async Task<decimal> GetTotalSubscriptionRevenueAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "stripe-total-subscription-revenue";
        if (memoryCache.TryGetValue(cacheKey, out decimal cached))
        {
            return cached;
        }

        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            return 0m;
        }

        var service = new InvoiceService();
        string? startingAfter = null;
        decimal total = 0m;

        while (true)
        {
            var invoices = await service.ListAsync(new InvoiceListOptions
            {
                Limit = 100,
                StartingAfter = startingAfter,
                Status = "paid"
            }, cancellationToken: cancellationToken);

            foreach (var invoice in invoices.Data)
            {
                total += (invoice.AmountPaid / 100m);
            }

            if (!invoices.HasMore || invoices.Data.Count == 0)
            {
                break;
            }

            startingAfter = invoices.Data.Last().Id;
        }

        memoryCache.Set(cacheKey, total, TimeSpan.FromMinutes(15));
        return total;
    }
}
