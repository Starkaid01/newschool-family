using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class AdaptiveRoutineService(ApplicationDbContext db)
{
    public async Task<AdaptiveRoutineSnapshot> BuildSnapshotAsync(Guid childId, DateTime forDate)
    {
        var child = await db.Children
            .Include(x => x.DevelopmentProfile)
            .FirstAsync(x => x.Id == childId);
        var profile = await db.ChildTeaProfiles.FirstOrDefaultAsync(x => x.ChildId == childId);
        var observations = await db.ChildRoutineObservations
            .Where(x => x.ChildId == childId)
            .OrderByDescending(x => x.ObservedAt)
            .Take(12)
            .ToListAsync();

        var averageDistress = observations.Count == 0 ? 0 : observations.Average(x => x.DistressLevel);
        var supportPressure = new[]
        {
            (double)(profile?.AnxietyLevel ?? 3),
            (double)(profile?.CognitiveRigidityLevel ?? 3),
            (double)(profile?.SensorySensitivityLevel ?? 3),
            (double)(profile?.TransitionDifficultyLevel ?? 3),
            (double)(profile?.SupportIntensityLevel ?? 3),
            averageDistress == 0 ? (double)(profile?.AnxietyLevel ?? 3) : averageDistress
        }.Average();

        var workBlockMinutes = supportPressure switch
        {
            >= 4.2 => 8,
            >= 3.6 => 10,
            >= 3.0 => 12,
            _ => 15
        };

        var breakMinutes = supportPressure switch
        {
            >= 4.2 => 5,
            >= 3.6 => 4,
            >= 3.0 => 3,
            _ => 2
        };

        var helpfulSupports = BuildTopItems(
            observations.SelectMany(x => SplitCsvLike($"{x.WhatHelped},{x.SupportUsed}"))
                .Concat(profile is null ? [] : SplitCsvLike($"{profile.CalmingStrategies},{profile.TransitionSupports},{profile.EffectiveReinforcers}")),
            4);

        var commonTriggers = BuildTopItems(
            observations.SelectMany(x => SplitCsvLike(x.Antecedent))
                .Concat(profile is null ? [] : SplitCsvLike($"{profile.CommonTriggers},{profile.OverloadSignals}")),
            4);

        var supportIntensity = supportPressure switch
        {
            >= 4.2 => AdaptiveSupportIntensity.High,
            >= 3.2 => AdaptiveSupportIntensity.Moderate,
            _ => AdaptiveSupportIntensity.Light
        };

        var topTrigger = commonTriggers.FirstOrDefault() ?? "mudancas inesperadas";
        var topSupport = helpfulSupports.FirstOrDefault() ?? "rotina visual com passos curtos";
        var visualRecommendation = profile?.NeedsVisualRoutine == true || profile?.NeedsFirstThen == true || supportIntensity != AdaptiveSupportIntensity.Light
            ? "Abrir o bloco com rotina visual, passo atual destacado e first-then visivel."
            : "Manter a proposta visual simples, mas sem sobrecarregar a tela ou a mesa.";
        var transitionRecommendation = profile?.TransitionDifficultyLevel >= 4 || profile?.NeedsTimer == true
            ? "Avisar a mudanca com 5 e 2 minutos, mostrando o proximo passo antes da troca."
            : "Anunciar a proxima troca com antecedencia curta e uma frase previsivel.";
        var planBRecommendation = profile?.NeedsPlanB == true || observations.Any(x => x.NeededPlanB)
            ? "Se houver rigidez ou ansiedade alta, troque para uma versao menor da mesma habilidade, preserve a previsibilidade e feche com uma vitoria curta."
            : "Se o bloco travar, reduza a exigencia, mantenha o objetivo e evite virar confronto.";

        var summary = profile is null
            ? "Ainda nao ha perfil funcional TEA preenchido. O sistema esta sugerindo ajustes iniciais a partir do historico de sessoes."
            : $"O perfil atual pede apoio {GetSupportIntensityLabel(supportIntensity).ToLowerInvariant()}, com foco em ansiedade {profile.AnxietyLevel}/5, rigidez {profile.CognitiveRigidityLevel}/5 e transicoes {profile.TransitionDifficultyLevel}/5.";

        var tomorrowAdjustment = $"Amanha vale começar com blocos de {workBlockMinutes} min, pausa de {breakMinutes} min e apoio forte em {topSupport.ToLowerInvariant()} antes de exigir mudancas depois de {topTrigger.ToLowerInvariant()}.";

        return new AdaptiveRoutineSnapshot
        {
            WorkBlockMinutes = workBlockMinutes,
            BreakMinutes = breakMinutes,
            Intensity = supportIntensity,
            Summary = summary,
            VisualSupportRecommendation = visualRecommendation,
            TransitionRecommendation = transitionRecommendation,
            PlanBRecommendation = planBRecommendation,
            TomorrowAdjustment = tomorrowAdjustment,
            HelpfulSupports = helpfulSupports,
            CommonTriggers = commonTriggers
        };
    }

    public async Task<List<ChildRoutineObservation>> GetRecentObservationsAsync(Guid childId, int take = 12)
    {
        return await db.ChildRoutineObservations
            .Where(x => x.ChildId == childId)
            .OrderByDescending(x => x.ObservedAt)
            .Take(take)
            .ToListAsync();
    }

    public static string GetSupportIntensityLabel(AdaptiveSupportIntensity intensity) => intensity switch
    {
        AdaptiveSupportIntensity.High => "Apoio alto",
        AdaptiveSupportIntensity.Moderate => "Apoio moderado",
        _ => "Apoio leve"
    };

    private static List<string> BuildTopItems(IEnumerable<string> items, int take)
    {
        return items
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Take(take)
            .Select(x => x.First())
            .ToList();
    }

    private static IEnumerable<string> SplitCsvLike(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x));
    }
}

public class AdaptiveRoutineSnapshot
{
    public int WorkBlockMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public AdaptiveSupportIntensity Intensity { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string VisualSupportRecommendation { get; set; } = string.Empty;
    public string TransitionRecommendation { get; set; } = string.Empty;
    public string TomorrowAdjustment { get; set; } = string.Empty;
    public string PlanBRecommendation { get; set; } = string.Empty;
    public List<string> HelpfulSupports { get; set; } = new();
    public List<string> CommonTriggers { get; set; } = new();
}

public enum AdaptiveSupportIntensity
{
    Light = 1,
    Moderate = 2,
    High = 3
}
