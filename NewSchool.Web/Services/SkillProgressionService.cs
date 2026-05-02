using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class SkillProgressionService
{
    public void InitializeProgress(ChildSkillProgress progress)
    {
        progress.SkillStage = "starting";
        progress.NextMilestone = "Comecar a praticar com apoio";
        progress.Recommendation = "Nova habilidade";
        progress.RemediationPlan = string.Empty;
        progress.NeedsReadinessCheck = false;
        progress.ReadinessApproved = false;
        progress.StageChangedAt = DateTime.UtcNow;
    }

    public void ApplyFeedback(ChildSkillProgress progress, DailyPlanBlock block, SkillFeedbackLevel rating)
    {
        progress.TimesPracticed += 1;
        progress.LastPracticedAt = DateTime.UtcNow;
        progress.MasteryScore = Math.Clamp(progress.MasteryScore + (rating switch
        {
            SkillFeedbackLevel.NeedsSupport => -12,
            SkillFeedbackLevel.Developing => 6,
            _ => 12
        }), 0, 100);

        if (rating == SkillFeedbackLevel.Secure)
        {
            progress.TimesSuccessful += 1;
            progress.SecureStreak += 1;
            progress.StruggleStreak = 0;
        }
        else if (rating == SkillFeedbackLevel.NeedsSupport)
        {
            progress.StruggleStreak += 1;
            progress.SecureStreak = 0;
        }
        else
        {
            progress.SecureStreak = 0;
            progress.StruggleStreak = 0;
        }

        var previousStage = progress.SkillStage;
        progress.SkillStage = ResolveStage(progress);
        if (!string.Equals(previousStage, progress.SkillStage, StringComparison.OrdinalIgnoreCase))
        {
            progress.StageChangedAt = DateTime.UtcNow;
        }

        progress.ReadyToAdvance = progress.SkillStage is "consolidating" or "ready_to_advance"
            && progress.SecureStreak >= 2
            && progress.MasteryScore >= 72;
        if (progress.ReadyToAdvance)
        {
            progress.NeedsReadinessCheck = true;
            progress.ReadinessApproved = false;
        }
        else if (rating == SkillFeedbackLevel.NeedsSupport || progress.MasteryScore < 72)
        {
            progress.NeedsReadinessCheck = false;
            progress.ReadinessApproved = false;
        }
        progress.NeedsRemediation = progress.StruggleStreak >= 2 || progress.MasteryScore < 38;
        progress.NextMilestone = BuildNextMilestone(progress);
        progress.RemediationPlan = progress.NeedsRemediation
            ? BuildRemediationPlan(progress, block)
            : string.Empty;
        progress.NextReviewAt = BuildNextReviewAt(progress, rating);
        progress.Recommendation = BuildRecommendation(progress, block);
    }

    public void ApplyDiagnosticCheckup(ChildSkillProgress progress, ChildSkillCheckup checkup, SkillFeedbackLevel rating)
    {
        progress.LastPracticedAt = DateTime.UtcNow;
        progress.MasteryScore = Math.Clamp(progress.MasteryScore + (rating switch
        {
            SkillFeedbackLevel.NeedsSupport => -10,
            SkillFeedbackLevel.Developing => 4,
            _ => 10
        }), 0, 100);

        if (rating == SkillFeedbackLevel.Secure)
        {
            progress.TimesSuccessful += 1;
            progress.SecureStreak += 1;
            progress.StruggleStreak = 0;
        }
        else if (rating == SkillFeedbackLevel.NeedsSupport)
        {
            progress.StruggleStreak += 1;
            progress.SecureStreak = 0;
        }
        else
        {
            progress.SecureStreak = 0;
            progress.StruggleStreak = 0;
        }

        var previousStage = progress.SkillStage;
        progress.SkillStage = ResolveStage(progress);
        if (!string.Equals(previousStage, progress.SkillStage, StringComparison.OrdinalIgnoreCase))
        {
            progress.StageChangedAt = DateTime.UtcNow;
        }

        progress.ReadyToAdvance = progress.SkillStage is "consolidating" or "ready_to_advance"
            && progress.SecureStreak >= 2
            && progress.MasteryScore >= 72;
        if (progress.ReadyToAdvance)
        {
            progress.NeedsReadinessCheck = true;
            progress.ReadinessApproved = false;
        }
        else if (rating == SkillFeedbackLevel.NeedsSupport || progress.MasteryScore < 72)
        {
            progress.NeedsReadinessCheck = false;
            progress.ReadinessApproved = false;
        }
        progress.NeedsRemediation = progress.StruggleStreak >= 2 || progress.MasteryScore < 38;
        progress.NextMilestone = BuildNextMilestone(progress);
        progress.RemediationPlan = progress.NeedsRemediation
            ? $"A checagem quinzenal mostrou que {checkup.SkillName} ainda precisa de apoio guiado. Retome com bloco curto, exemplo concreto e uma unica variacao por vez."
            : string.Empty;
        progress.NextReviewAt = BuildNextReviewAt(progress, rating);
        progress.Recommendation = rating switch
        {
            SkillFeedbackLevel.NeedsSupport => $"Checagem quinzenal: volte para o prerequisito imediato de {checkup.SkillName.ToLowerInvariant()} antes de avancar.",
            SkillFeedbackLevel.Developing => $"Checagem quinzenal: mantenha {checkup.SkillName.ToLowerInvariant()} em pratica guiada curta nesta semana.",
            _ => $"Checagem quinzenal: {checkup.SkillName.ToLowerInvariant()} respondeu bem e pode seguir para consolidacao."
        };
    }

    public void ApplyReadinessCheck(ChildSkillProgress progress, ChildSkillReadinessCheck check, SkillFeedbackLevel rating)
    {
        progress.LastReadinessCheckAt = DateTime.UtcNow;

        if (rating == SkillFeedbackLevel.Secure)
        {
            progress.ReadinessApproved = true;
            progress.NeedsReadinessCheck = false;
            progress.ReadyToAdvance = true;
            progress.NextMilestone = $"Base consolidada. A proxima habilidade pode entrar: {check.UnlocksSkillName}.";
            progress.Recommendation = $"Prontidao aprovada: {check.UnlocksSkillName.ToLowerInvariant()} ja pode entrar como proximo degrau.";
            progress.NextReviewAt = DateTime.UtcNow.Date.AddDays(5);
            return;
        }

        progress.ReadinessApproved = false;
        progress.NeedsReadinessCheck = rating != SkillFeedbackLevel.NeedsSupport;
        progress.ReadyToAdvance = false;
        progress.StruggleStreak = Math.Max(progress.StruggleStreak, 1);
        progress.MasteryScore = Math.Clamp(progress.MasteryScore - (rating == SkillFeedbackLevel.NeedsSupport ? 8 : 3), 0, 100);
        progress.NextMilestone = rating == SkillFeedbackLevel.Developing
            ? $"Ganhar mais estabilidade em {progress.SkillName} antes de liberar {check.UnlocksSkillName}."
            : $"Voltar ao prerequisito de {progress.SkillName} com mais apoio antes de tentar subir.";
        progress.RemediationPlan = rating == SkillFeedbackLevel.NeedsSupport
            ? $"A pre-avaliacao mostrou que {progress.SkillName} ainda nao esta firme. Repita em contexto mais simples, com modelagem curta e uma unica variacao por vez."
            : progress.RemediationPlan;
        progress.Recommendation = rating == SkillFeedbackLevel.Developing
            ? $"Prontidao parcial: mantenha {progress.SkillName.ToLowerInvariant()} em consolidacao e tente a pre-avaliacao novamente em poucos dias."
            : $"Prontidao nao aprovada: segure o avanço e reforce {progress.SkillName.ToLowerInvariant()} antes de liberar a proxima habilidade.";
        progress.NextReviewAt = DateTime.UtcNow.Date.AddDays(rating == SkillFeedbackLevel.Developing ? 2 : 1);
    }

    public string GetStageLabel(string skillStage) => skillStage switch
    {
        "starting" => "Comecando",
        "guided_practice" => "Pratica guiada",
        "developing" => "Em desenvolvimento",
        "consolidating" => "Consolidando",
        "ready_to_advance" => "Pronta para avancar",
        _ => "Em desenvolvimento"
    };

    public string GetStageChip(string skillStage, bool needsRemediation, bool readyToAdvance)
    {
        if (needsRemediation)
        {
            return "warning";
        }

        if (readyToAdvance || skillStage == "ready_to_advance")
        {
            return "success";
        }

        return skillStage switch
        {
            "starting" or "guided_practice" => "warning",
            "developing" => "neutral",
            _ => "success"
        };
    }

    public string GetReadinessStatusLabel(ChildSkillProgress progress)
    {
        if (progress.ReadinessApproved)
        {
            return "Prontidao aprovada";
        }

        if (progress.NeedsReadinessCheck)
        {
            return "Aguardando pre-avaliacao";
        }

        return progress.ReadyToAdvance
            ? "Pode se aproximar do proximo degrau"
            : "Ainda consolidando base";
    }

    private static string ResolveStage(ChildSkillProgress progress)
    {
        return progress.MasteryScore switch
        {
            < 35 => "starting",
            < 50 => "guided_practice",
            < 72 => "developing",
            < 86 => "consolidating",
            _ => "ready_to_advance"
        };
    }

    private static string BuildNextMilestone(ChildSkillProgress progress) => progress.SkillStage switch
    {
        "starting" => "Reconhecer a habilidade com ajuda do adulto",
        "guided_practice" => "Acertar com apoio leve e repeticao",
        "developing" => "Conseguir fazer com menos ajuda e mais consistencia",
        "consolidating" => "Mostrar seguranca em mais de um contexto",
        _ => "Avancar para a proxima habilidade relacionada"
    };

    private static string BuildRemediationPlan(ChildSkillProgress progress, DailyPlanBlock block)
    {
        var domainTip = block.Domain switch
        {
            LearningDomain.Language => "volte para exemplos orais, repita em voz alta e use menos texto de uma vez",
            LearningDomain.Math => "retome com material concreto, conte devagar e faca uma operacao por vez",
            LearningDomain.World => "traga observacao concreta, objetos reais e perguntas bem curtas",
            _ => "quebre a tarefa em microetapas, com um comando por vez e pausa curta"
        };

        return $"A habilidade {block.SkillName} entrou em remediacao. Nas proximas 2 praticas, {domainTip}. Busque uma vitoria pequena antes de aumentar a dificuldade.";
    }

    private static string BuildRecommendation(ChildSkillProgress progress, DailyPlanBlock block)
    {
        if (progress.NeedsRemediation)
        {
            return $"Remediacao ativa: simplifique {block.SkillName.ToLowerInvariant()}, reduza a carga e repita com apoio concreto.";
        }

        if (progress.ReadyToAdvance)
        {
            return $"A habilidade {block.SkillName.ToLowerInvariant()} ja pode entrar em avancos e variacoes mais desafiadoras.";
        }

        return progress.SkillStage switch
        {
            "starting" => "Comece com apoio forte, modelagem e repeticao curta.",
            "guided_practice" => "Mantenha pratica guiada e feedback imediato.",
            "developing" => "Repita em contextos parecidos para ganhar consistencia.",
            "consolidating" => "Varie o contexto para consolidar sem perder seguranca.",
            _ => "Pronta para avancar e consolidar em novos desafios."
        };
    }

    private static DateTime BuildNextReviewAt(ChildSkillProgress progress, SkillFeedbackLevel rating)
    {
        var days = rating switch
        {
            SkillFeedbackLevel.NeedsSupport => 1,
            SkillFeedbackLevel.Developing when progress.SkillStage is "starting" or "guided_practice" => 2,
            SkillFeedbackLevel.Developing => 3,
            SkillFeedbackLevel.Secure when progress.ReadyToAdvance => 7,
            _ => 4
        };

        return DateTime.UtcNow.Date.AddDays(days);
    }
}
