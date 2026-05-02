using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using Stripe;
using Stripe.Checkout;

namespace NewSchool.Web.Services;

public class EvidenceStoragePlanService(
    ApplicationDbContext db,
    IOptions<StripeSettings> stripeOptions)
{
    public const string FreePlanCode = "free";
    public const string Files200PlanCode = "files_200";
    public const string Files1000PlanCode = "files_1000";
    public const string Files5000PlanCode = "files_5000";

    private const string ExtraFilesProductMetadataKey = "newschool_storage_role";
    private const string ExtraFilesProductMetadataValue = "extra_100_files";
    private readonly StripeSettings _stripe = stripeOptions.Value;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly IReadOnlyList<EvidenceStoragePlanDefinition> Catalog =
    [
        new(FreePlanCode, "Plano gratuito", 0m, 3, "3 arquivos totais para começar e testar a rotina sem custo.", false),
        new(Files200PlanCode, "Plano 200 arquivos", 20m, 200, "200 arquivos para guardar as provas mais importantes da família.", false),
        new(Files1000PlanCode, "Plano 1.000 arquivos", 80m, 1000, "1.000 arquivos para acompanhar vários meses de fotos, vídeos e documentos.", false),
        new(Files5000PlanCode, "Plano 5.000 arquivos", 120m, 5000, "5.000 arquivos e expansão em blocos de 100 quando a família precisar de mais espaço.", true)
    ];

    public IReadOnlyList<EvidenceStoragePlanDefinition> GetCatalog() => Catalog;

    public EvidenceStoragePlanDefinition GetPlanDefinition(string? planCode)
    {
        var normalized = NormalizePlanCode(planCode);
        return Catalog.FirstOrDefault(plan => string.Equals(plan.Code, normalized, StringComparison.OrdinalIgnoreCase))
               ?? Catalog[0];
    }

    public async Task<EvidenceStorageSummaryViewModel> BuildSummaryAsync(Guid parentId)
    {
        var user = await db.Users.FirstAsync(x => x.Id == parentId);
        var usedFiles = await CountUsedFilesAsync(parentId);
        return BuildSummary(user, usedFiles);
    }

    public EvidenceStorageSummaryViewModel BuildSummary(AppUser user, int usedFiles)
    {
        var plan = GetCurrentPlan(user);
        var fileLimit = GetFileLimit(user);
        var remainingFiles = Math.Max(fileLimit - usedFiles, 0);
        var usagePercent = fileLimit <= 0
            ? 0
            : Math.Min(100, (int)Math.Round(usedFiles * 100m / fileLimit, MidpointRounding.AwayFromZero));
        var hasPaidPlan = !string.Equals(plan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase) && IsPaidStorageActive(user);
        var canUpload = usedFiles < fileLimit;
        var extraBlocks = GetNormalizedExtraBlocks(user);
        var extraFiles = extraBlocks * 100;
        var currentPlanSummary = string.Equals(plan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase)
            ? "O currículo continua livre. O upgrade serve só para guardar mais fotos, vídeos e documentos."
            : "Aulas e currículo seguem liberados para a família toda. A assinatura amplia apenas o espaço do acervo.";

        return new EvidenceStorageSummaryViewModel
        {
            CurrentPlanCode = plan.Code,
            CurrentPlanName = plan.Name,
            CurrentPlanPriceLabel = FormatPrice(plan.MonthlyPrice, extraBlocks),
            CurrentPlanStatusLabel = BuildStatusLabel(user, plan),
            CurrentPlanSummary = currentPlanSummary,
            UsedFiles = usedFiles,
            FileLimit = fileLimit,
            RemainingFiles = remainingFiles,
            UsagePercent = usagePercent,
            CanUpload = canUpload,
            HasPaidPlan = hasPaidPlan,
            SupportsExtraBlocks = plan.SupportsExtraBlocks,
            ExtraBlocks = extraBlocks,
            ExtraFiles = extraFiles,
            ExtraBlockSummary = extraBlocks > 0
                ? $"+{extraFiles.ToString("N0", PtBr)} arquivos extras por R$ {(extraBlocks * 10m).ToString("N2", PtBr)}/mês."
                : "Acima de 5.000 arquivos, a família pode somar blocos de 100 por R$ 10/mês.",
            UploadNotice = BuildUploadNotice(plan, usedFiles, fileLimit, remainingFiles),
            UpgradeHint = "O sistema é gratuito. O upgrade só aumenta o limite do acervo de evidências.",
            CanManageBilling = !string.IsNullOrWhiteSpace(user.StripeCustomerId),
            Plans = Catalog.Select(definition => new EvidenceStoragePlanCardViewModel
            {
                PlanCode = definition.Code,
                Name = definition.Name,
                PriceLabel = definition.MonthlyPrice <= 0m
                    ? "Grátis"
                    : $"R$ {definition.MonthlyPrice.ToString("N0", PtBr)}/mês",
                CapacityLabel = $"{definition.BaseFileLimit.ToString("N0", PtBr)} arquivo(s)",
                Description = definition.Description,
                IsCurrent = string.Equals(definition.Code, plan.Code, StringComparison.OrdinalIgnoreCase),
                SupportsExtraBlocks = definition.SupportsExtraBlocks,
                ButtonLabel = string.Equals(definition.Code, plan.Code, StringComparison.OrdinalIgnoreCase)
                    ? "Plano atual"
                    : definition.MonthlyPrice <= 0m
                        ? "Continuar grátis"
                        : $"Escolher {definition.Name}"
            }).ToList()
        };
    }

    public async Task<EvidenceUploadAllowanceResult> BuildAllowanceAsync(Guid parentId)
    {
        var user = await db.Users.FirstAsync(x => x.Id == parentId);
        var usedFiles = await CountUsedFilesAsync(parentId);
        var fileLimit = GetFileLimit(user);
        var remainingFiles = Math.Max(fileLimit - usedFiles, 0);
        return new EvidenceUploadAllowanceResult(
            user,
            usedFiles,
            fileLimit,
            remainingFiles,
            usedFiles < fileLimit,
            BuildQuotaMessage(GetCurrentPlan(user), usedFiles, fileLimit, remainingFiles));
    }

    public async Task<int> CountUsedFilesAsync(Guid parentId)
    {
        return await db.LearningSessions.CountAsync(session =>
            session.Child.ParentId == parentId &&
            !string.IsNullOrWhiteSpace(session.MediaUrl));
    }

    public EvidenceStoragePlanDefinition GetCurrentPlan(AppUser user)
    {
        return GetPlanDefinition(ResolveStoredPlanCode(user));
    }

    public int GetFileLimit(AppUser user)
    {
        var currentPlan = GetCurrentPlan(user);
        return currentPlan.BaseFileLimit + (GetNormalizedExtraBlocks(user) * 100);
    }

    public bool IsPaidStorageActive(AppUser user)
    {
        var currentPlan = GetCurrentPlan(user);
        if (string.Equals(currentPlan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return user.SubscriptionStatus switch
        {
            "active" or "trialing" or "past_due" or "unpaid" => true,
            "canceled" when user.SubscriptionCurrentPeriodEnd.HasValue && user.SubscriptionCurrentPeriodEnd.Value.ToUniversalTime() > DateTime.UtcNow => true,
            _ => !string.IsNullOrWhiteSpace(user.StripeSubscriptionId)
        };
    }

    public string GetRequiredPriceId(string planCode)
    {
        var normalized = NormalizePlanCode(planCode);
        var priceId = normalized switch
        {
            Files200PlanCode => string.IsNullOrWhiteSpace(_stripe.PriceId20) ? _stripe.PriceId : _stripe.PriceId20,
            Files1000PlanCode => _stripe.PriceId80,
            Files5000PlanCode => _stripe.PriceId120,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(priceId))
        {
            throw new InvalidOperationException($"Nao encontramos o Price ID configurado para o plano {normalized}.");
        }

        return priceId;
    }

    public int NormalizeExtraBlocks(string planCode, int extraBlocks)
    {
        if (!string.Equals(NormalizePlanCode(planCode), Files5000PlanCode, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        return Math.Clamp(extraBlocks, 0, 100);
    }

    public string? ResolvePlanCodeFromPriceId(string? priceId)
    {
        if (string.IsNullOrWhiteSpace(priceId))
        {
            return null;
        }

        if (string.Equals(priceId, GetSafePriceId(_stripe.PriceId20, _stripe.PriceId), StringComparison.OrdinalIgnoreCase))
        {
            return Files200PlanCode;
        }

        if (string.Equals(priceId, _stripe.PriceId80, StringComparison.OrdinalIgnoreCase))
        {
            return Files1000PlanCode;
        }

        if (string.Equals(priceId, _stripe.PriceId120, StringComparison.OrdinalIgnoreCase))
        {
            return Files5000PlanCode;
        }

        return null;
    }

    public async Task<string> EnsureExtraFilesPriceIdAsync()
    {
        if (!string.IsNullOrWhiteSpace(_stripe.PriceIdExtra100))
        {
            return _stripe.PriceIdExtra100;
        }

        var productService = new ProductService();
        var products = await productService.ListAsync(new ProductListOptions { Limit = 100 });
        var product = products.Data.FirstOrDefault(item =>
            item.Metadata.TryGetValue(ExtraFilesProductMetadataKey, out var role) &&
            string.Equals(role, ExtraFilesProductMetadataValue, StringComparison.OrdinalIgnoreCase));

        if (product is null)
        {
            product = await productService.CreateAsync(new ProductCreateOptions
            {
                Name = "NewSchool Acervo extra 100 arquivos",
                Description = "Bloco mensal extra de 100 arquivos para o acervo de evidencias.",
                Metadata = new Dictionary<string, string>
                {
                    [ExtraFilesProductMetadataKey] = ExtraFilesProductMetadataValue
                }
            });
        }

        var priceService = new PriceService();
        var prices = await priceService.ListAsync(new PriceListOptions
        {
            Product = product.Id,
            Limit = 100,
            Active = true
        });

        var extraPrice = prices.Data.FirstOrDefault(price =>
            price.UnitAmount == 1000 &&
            string.Equals(price.Currency, "brl", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(price.Type, "recurring", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(price.Recurring?.Interval, "month", StringComparison.OrdinalIgnoreCase));

        if (extraPrice is null)
        {
            extraPrice = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = product.Id,
                Currency = "brl",
                UnitAmount = 1000,
                Nickname = "NewSchool extra 100 arquivos",
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month"
                },
                Metadata = new Dictionary<string, string>
                {
                    [ExtraFilesProductMetadataKey] = ExtraFilesProductMetadataValue
                }
            });
        }

        return extraPrice.Id;
    }

    public async Task<List<SessionLineItemOptions>> BuildCheckoutLineItemsAsync(string planCode, int extraBlocks)
    {
        var normalizedPlanCode = NormalizePlanCode(planCode);
        if (string.Equals(normalizedPlanCode, FreePlanCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("O plano gratuito nao precisa de checkout.");
        }

        var lineItems = new List<SessionLineItemOptions>
        {
            new()
            {
                Price = GetRequiredPriceId(normalizedPlanCode),
                Quantity = 1
            }
        };

        var normalizedBlocks = NormalizeExtraBlocks(normalizedPlanCode, extraBlocks);
        if (normalizedBlocks > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                Price = await EnsureExtraFilesPriceIdAsync(),
                Quantity = normalizedBlocks
            });
        }

        return lineItems;
    }

    public async Task<Subscription> UpdateSubscriptionPlanAsync(AppUser user, string planCode, int extraBlocks)
    {
        if (string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
        {
            throw new InvalidOperationException("Nao existe uma assinatura Stripe ativa para atualizar.");
        }

        var normalizedPlanCode = NormalizePlanCode(planCode);
        var normalizedBlocks = NormalizeExtraBlocks(normalizedPlanCode, extraBlocks);
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(user.StripeSubscriptionId);
        var addonPriceId = normalizedBlocks > 0 ? await EnsureExtraFilesPriceIdAsync() : null;

        var currentItems = subscription.Items?.Data ?? new List<SubscriptionItem>();
        var baseItem = currentItems.FirstOrDefault(item => !IsExtraFilesItem(item.Price)) ?? currentItems.FirstOrDefault();
        var extraItem = currentItems.FirstOrDefault(item => IsExtraFilesItem(item.Price));
        var updatedItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<SubscriptionItemOptions>();

        if (baseItem is not null)
        {
            items.Add(new SubscriptionItemOptions
            {
                Id = baseItem.Id,
                Price = GetRequiredPriceId(normalizedPlanCode),
                Quantity = 1
            });
            updatedItemIds.Add(baseItem.Id);
        }
        else
        {
            items.Add(new SubscriptionItemOptions
            {
                Price = GetRequiredPriceId(normalizedPlanCode),
                Quantity = 1
            });
        }

        if (normalizedBlocks > 0 && !string.IsNullOrWhiteSpace(addonPriceId))
        {
            if (extraItem is not null)
            {
                items.Add(new SubscriptionItemOptions
                {
                    Id = extraItem.Id,
                    Price = addonPriceId,
                    Quantity = normalizedBlocks
                });
                updatedItemIds.Add(extraItem.Id);
            }
            else
            {
                items.Add(new SubscriptionItemOptions
                {
                    Price = addonPriceId,
                    Quantity = normalizedBlocks
                });
            }
        }

        foreach (var item in currentItems.Where(item => !updatedItemIds.Contains(item.Id)))
        {
            items.Add(new SubscriptionItemOptions
            {
                Id = item.Id,
                Deleted = true
            });
        }

        var updatedSubscription = await subscriptionService.UpdateAsync(subscription.Id, new SubscriptionUpdateOptions
        {
            Items = items,
            ProrationBehavior = "create_prorations"
        });

        ApplySubscriptionToUser(user, updatedSubscription);
        await db.SaveChangesAsync();
        return updatedSubscription;
    }

    public void ApplySubscriptionToUser(AppUser user, Subscription subscription)
    {
        user.StripeSubscriptionId = subscription.Id;
        user.SubscriptionStatus = subscription.Status ?? "active";
        user.SubscriptionCurrentPeriodEnd = subscription.Items?.Data?.Any() == true
            ? subscription.Items.Data.Max(item => item.CurrentPeriodEnd)
            : subscription.CancelAt;

        var planCode = subscription.Items?.Data
            .Select(item => ResolvePlanCodeFromPriceId(item.Price?.Id))
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code));

        user.StoragePlanCode = planCode ?? ResolveStoredPlanCode(user);
        user.StorageExtraFileBlocks = subscription.Items?.Data is null
            ? 0
            : subscription.Items.Data
                .Where(item => IsExtraFilesItem(item.Price))
                .Sum(item => (int)item.Quantity);

        if (IsPaidStorageActive(user))
        {
            user.FirstSubscribedAt ??= DateTime.UtcNow;
        }
    }

    public void DowngradeToFree(AppUser user)
    {
        user.StoragePlanCode = FreePlanCode;
        user.StorageExtraFileBlocks = 0;
    }

    private string ResolveStoredPlanCode(AppUser user)
    {
        var normalized = NormalizePlanCode(user.StoragePlanCode);
        if (string.Equals(normalized, FreePlanCode, StringComparison.OrdinalIgnoreCase) &&
            (!string.IsNullOrWhiteSpace(user.StripeSubscriptionId) || !string.IsNullOrWhiteSpace(user.StripeCustomerId)) &&
            user.SubscriptionStatus is "active" or "trialing" or "past_due" or "unpaid")
        {
            return Files5000PlanCode;
        }

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        return user.SubscriptionStatus switch
        {
            "active" or "trialing" or "past_due" or "unpaid" => Files5000PlanCode,
            "canceled" when user.SubscriptionCurrentPeriodEnd.HasValue && user.SubscriptionCurrentPeriodEnd.Value.ToUniversalTime() > DateTime.UtcNow => Files5000PlanCode,
            _ => FreePlanCode
        };
    }

    private int GetNormalizedExtraBlocks(AppUser user)
    {
        return NormalizeExtraBlocks(ResolveStoredPlanCode(user), user.StorageExtraFileBlocks);
    }

    private static string NormalizePlanCode(string? planCode)
    {
        if (string.IsNullOrWhiteSpace(planCode))
        {
            return string.Empty;
        }

        return planCode.Trim().ToLowerInvariant();
    }

    private static string GetSafePriceId(string preferredPriceId, string fallbackPriceId)
    {
        return string.IsNullOrWhiteSpace(preferredPriceId) ? fallbackPriceId : preferredPriceId;
    }

    private string BuildStatusLabel(AppUser user, EvidenceStoragePlanDefinition plan)
    {
        if (string.Equals(plan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase))
        {
            return "Gratuito";
        }

        return user.SubscriptionStatus switch
        {
            "active" or "trialing" => "Assinatura ativa",
            "past_due" or "unpaid" => "Pagamento pendente",
            "canceled" when user.SubscriptionCurrentPeriodEnd.HasValue && user.SubscriptionCurrentPeriodEnd.Value.Date >= DateTime.Today => $"Ativo até {user.SubscriptionCurrentPeriodEnd.Value:dd/MM}",
            _ => "Plano pago"
        };
    }

    private static string BuildUploadNotice(EvidenceStoragePlanDefinition plan, int usedFiles, int fileLimit, int remainingFiles)
    {
        if (remainingFiles <= 0)
        {
            return $"O acervo chegou ao limite de {fileLimit.ToString("N0", PtBr)} arquivo(s). Os anexos já salvos continuam disponíveis, mas os próximos uploads pedem upgrade.";
        }

        if (remainingFiles <= 10)
        {
            return $"Faltam {remainingFiles.ToString("N0", PtBr)} arquivo(s) antes do limite atual.";
        }

        if (string.Equals(plan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase))
        {
            return "No plano gratuito, a família usa o sistema inteiro e guarda até 3 arquivos no acervo.";
        }

        return $"{usedFiles.ToString("N0", PtBr)} arquivo(s) usados de {fileLimit.ToString("N0", PtBr)} no acervo da família.";
    }

    private static string BuildQuotaMessage(EvidenceStoragePlanDefinition plan, int usedFiles, int fileLimit, int remainingFiles)
    {
        if (usedFiles >= fileLimit)
        {
            return $"Seu acervo da família atingiu o limite de {fileLimit.ToString("N0", PtBr)} arquivo(s). Faça upgrade para continuar enviando fotos, vídeos e documentos.";
        }

        return string.Equals(plan.Code, FreePlanCode, StringComparison.OrdinalIgnoreCase)
            ? $"Você ainda pode guardar {remainingFiles.ToString("N0", PtBr)} arquivo(s) no plano gratuito."
            : $"Você ainda pode guardar {remainingFiles.ToString("N0", PtBr)} arquivo(s) antes do próximo limite.";
    }

    private static string FormatPrice(decimal monthlyPrice, int extraBlocks)
    {
        var total = monthlyPrice + (extraBlocks * 10m);
        return monthlyPrice <= 0m
            ? "Grátis"
            : $"R$ {total.ToString("N2", PtBr)}/mês";
    }

    private static bool IsExtraFilesItem(Price? price)
    {
        if (price is null)
        {
            return false;
        }

        if (price.Metadata.TryGetValue(ExtraFilesProductMetadataKey, out var role) &&
            string.Equals(role, ExtraFilesProductMetadataValue, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(price.Nickname, "NewSchool extra 100 arquivos", StringComparison.OrdinalIgnoreCase);
    }
}

public record EvidenceStoragePlanDefinition(
    string Code,
    string Name,
    decimal MonthlyPrice,
    int BaseFileLimit,
    string Description,
    bool SupportsExtraBlocks);

public record EvidenceUploadAllowanceResult(
    AppUser User,
    int UsedFiles,
    int FileLimit,
    int RemainingFiles,
    bool CanUpload,
    string Message);
