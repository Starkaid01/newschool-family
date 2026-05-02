using Microsoft.AspNetCore.Authentication.Cookies;
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

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

var connectionString =
    builder.Configuration.GetConnectionString("StarkaidSchoolConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:StarkaidSchoolConnection para o banco online unico do NewSchool.");

var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>() ?? new StripeSettings();
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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "NewSchool.Auth";
    });

builder.Services.AddAuthorization();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
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

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

await SeedData.InitializeAsync(app.Services, app.Environment);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
