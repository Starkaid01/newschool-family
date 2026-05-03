using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using NewSchool.Web.Services;
using Resend;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
const long evidenceUploadMaxBytes = 95L * 1024 * 1024;

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

var dedicatedConnectionString =
    builder.Configuration["NewSchool:SqlConnectionString"]
    ?? Environment.GetEnvironmentVariable("NEWSCHOOL_SQLSERVER_CONNECTION_STRING");

var connectionString =
    dedicatedConnectionString
    ?? builder.Configuration.GetConnectionString("StarkaidSchoolConnection")
    ?? throw new InvalidOperationException(
        "Configure NewSchool:SqlConnectionString (ou a variavel NEWSCHOOL_SQLSERVER_CONNECTION_STRING) para o banco online do NewSchool.");

var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

var stripeSettings = ResolveStripeSettings(builder.Configuration);
if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(stripeSettings.PublishableKey) || stripeSettings.PublishableKey.StartsWith("pk_test_", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Configure Stripe:PublishableKey de producao antes do deploy.");
    }

    if (string.IsNullOrWhiteSpace(stripeSettings.SecretKey) || stripeSettings.SecretKey.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Configure Stripe:SecretKey de producao antes do deploy.");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = evidenceUploadMaxBytes;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = evidenceUploadMaxBytes;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "NewSchool.Auth";
    });

builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = null;
});
builder.Services.Configure<StripeSettings>(options =>
{
    options.PublishableKey = stripeSettings.PublishableKey;
    options.SecretKey = stripeSettings.SecretKey;
    options.PriceId = stripeSettings.PriceId;
    options.PriceId20 = stripeSettings.PriceId20;
    options.PriceId30 = stripeSettings.PriceId30;
    options.PriceId80 = stripeSettings.PriceId80;
    options.PriceId120 = stripeSettings.PriceId120;
    options.PriceIdExtra100 = stripeSettings.PriceIdExtra100;
    options.WebhookSecretSnapshot = stripeSettings.WebhookSecretSnapshot;
    options.WebhookSecretMin = stripeSettings.WebhookSecretMin;
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<OpenRouterSettings>(builder.Configuration.GetSection("OpenRouter"));
builder.Services.Configure<AdvertisingOptions>(builder.Configuration.GetSection(AdvertisingOptions.SectionName));
builder.Services.Configure<FamilyLibraryOptions>(builder.Configuration.GetSection(FamilyLibraryOptions.SectionName));
builder.Services.AddOptions();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["Email:ResendApiKey"] ?? string.Empty;
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LearningPlanService>();
builder.Services.AddScoped<AdaptiveRoutineService>();
builder.Services.AddScoped<SkillProgressionService>();
builder.Services.AddScoped<OpenRouterPedagogyService>();
builder.Services.AddScoped<AdultInterventionService>();
builder.Services.AddScoped<SkillCheckupService>();
builder.Services.AddScoped<SkillReadinessService>();
builder.Services.AddScoped<ChildGoalCycleService>();
builder.Services.AddScoped<ChildEvolutionService>();
builder.Services.AddScoped<ChildRecoveryPlanService>();
builder.Services.AddScoped<ChildAchievementService>();
builder.Services.AddScoped<ConsistencyService>();
builder.Services.AddScoped<ReferralService>();
builder.Services.AddScoped<TrackAnalyticsService>();
builder.Services.AddScoped<EmailAutomationService>();
builder.Services.AddScoped<ProprietaryLessonPacketService>();
builder.Services.AddScoped<CuratedLearningLibraryService>();
builder.Services.AddScoped<DailyTrailComposerService>();
builder.Services.AddScoped<GuidedLessonExperienceService>();
builder.Services.AddScoped<EvidenceAutomationService>();
builder.Services.AddScoped<EvidenceStoragePlanService>();
builder.Services.AddScoped<WeeklyRoadmapService>();
builder.Services.AddScoped<AnnualCurriculumPlannerService>();
builder.Services.AddScoped<SystemCurriculumLibraryService>();
builder.Services.AddScoped<ExternalContentHubService>();
builder.Services.AddScoped<ParentAcademyService>();
builder.Services.AddScoped<PortuguesePlanningService>();
builder.Services.AddScoped<TeachingGuideService>();
builder.Services.AddScoped<FamilyLibrarySyncService>();
builder.Services.AddScoped<FamilyLibraryService>();
builder.Services.AddHostedService<RevenueRecoveryHostedService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.Logger.LogInformation(
    "Banco online do NewSchool: {DataSource} / {Database}",
    sqlConnectionStringBuilder.DataSource,
    sqlConnectionStringBuilder.InitialCatalog);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

StripeConfiguration.ApiKey = stripeSettings.SecretKey;

await SeedData.InitializeAsync(app.Services, app.Environment);

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static StripeSettings ResolveStripeSettings(IConfiguration configuration)
{
    var stripeSettings = configuration.GetSection("Stripe").Get<StripeSettings>() ?? new StripeSettings();

    stripeSettings.PublishableKey = FirstNonEmpty(
        stripeSettings.PublishableKey,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PUBLISHABLE_KEY"));
    stripeSettings.SecretKey = FirstNonEmpty(
        stripeSettings.SecretKey,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_SECRET_KEY"));
    stripeSettings.PriceId = FirstNonEmpty(
        stripeSettings.PriceId,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID"));
    stripeSettings.PriceId20 = FirstNonEmpty(
        stripeSettings.PriceId20,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID_20"),
        stripeSettings.PriceId);
    stripeSettings.PriceId30 = FirstNonEmpty(
        stripeSettings.PriceId30,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID_30"));
    stripeSettings.PriceId80 = FirstNonEmpty(
        stripeSettings.PriceId80,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID_80"));
    stripeSettings.PriceId120 = FirstNonEmpty(
        stripeSettings.PriceId120,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID_120"));
    stripeSettings.PriceIdExtra100 = FirstNonEmpty(
        stripeSettings.PriceIdExtra100,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_PRICE_ID_EXTRA_100"));
    stripeSettings.WebhookSecretSnapshot = FirstNonEmpty(
        stripeSettings.WebhookSecretSnapshot,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_WEBHOOK_SECRET_SNAPSHOT"));
    stripeSettings.WebhookSecretMin = FirstNonEmpty(
        stripeSettings.WebhookSecretMin,
        Environment.GetEnvironmentVariable("NEWSCHOOL_STRIPE_WEBHOOK_SECRET_MIN"));

    return stripeSettings;
}

static string FirstNonEmpty(params string?[] values)
{
    return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}
