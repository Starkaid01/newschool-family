using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class EvidenceAutomationService
{
    public EvidenceAutomationViewModel BuildForDailySession(
        ChildProfile child,
        DailyTrailViewModel todayTrail,
        IReadOnlyCollection<PlanBlockViewModel> blocks,
        bool justLoggedSession,
        bool hasMediaSaved,
        string saveActionUrl)
    {
        var evidencePrompts = blocks
            .Select(block => block.SuggestedTask?.EvidencePrompt ?? block.EvidencePrompt)
            .Concat(todayTrail.EvidenceChecklist)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var primaryPrompt = evidencePrompts.FirstOrDefault()
            ?? $"Guarde uma prova curta mostrando o que {BuildShortName(child.FullName)} conseguiu fazer hoje.";
        var preferredCapture = DeterminePreferredCapture(evidencePrompts);
        var focusLabel = string.IsNullOrWhiteSpace(todayTrail.FocusAreaLabel)
            ? "a aula de hoje"
            : todayTrail.FocusAreaLabel.ToLowerInvariant();

        return new EvidenceAutomationViewModel
        {
            Headline = justLoggedSession
                ? "Prova automatica para fechar a sessao"
                : "Prova da aprendizagem pronta para hoje",
            Summary = hasMediaSaved
                ? $"A sessao de {BuildShortName(child.FullName)} ja tem midia salva. Agora vale completar com texto curto e um checklist para a prova ficar forte no dashboard."
                : $"O sistema ja decidiu a prova mais util para {BuildShortName(child.FullName)} em {focusLabel}. Se voce salvar so uma coisa agora, comece por {BuildPreferredCaptureInstruction(preferredCapture)}.",
            StatusLabel = hasMediaSaved
                ? "midia salva"
                : justLoggedSession
                    ? "falta anexar prova"
                    : "sugestao pronta",
            StatusChipClass = hasMediaSaved ? "success" : justLoggedSession ? "warning" : "neutral",
            ProofTargetLabel = BuildProofTarget(preferredCapture, hasMediaSaved),
            PreferredCaptureLabel = BuildPreferredCaptureLabel(preferredCapture),
            QuickTextPrompt = BuildDailyQuickTextPrompt(primaryPrompt, todayTrail.DailyOutcome),
            FileHint = BuildDailyFileHint(primaryPrompt, preferredCapture, hasMediaSaved),
            WinsPlaceholder = BuildWinsPlaceholder(primaryPrompt),
            ChallengesPlaceholder = BuildChallengesPlaceholder(todayTrail.FocusAreaLabel),
            NotesPlaceholder = "Ex.: guardamos foto/video, precisou de ajuda em uma parte especifica e o proximo passo ja ficou claro.",
            SaveActionLabel = "Registrar no dashboard",
            SaveActionUrl = saveActionUrl,
            CaptureIdeas = BuildCaptureIdeas(primaryPrompt, todayTrail.DailyOutcome, preferredCapture),
            ChecklistItems = BuildDailyChecklist(primaryPrompt, todayTrail.DailyOutcome, hasMediaSaved)
        };
    }

    public EvidenceAutomationViewModel BuildForExternalContent(
        ChildProfile child,
        ExternalContentCatalogItem item,
        string selectedFocusTitle,
        string selectedFocusEvidenceIdea,
        IReadOnlyCollection<string> evidenceIdeas,
        bool isCompleted,
        bool justCompleted,
        string saveActionUrl)
    {
        var promptPool = evidenceIdeas
            .Append(selectedFocusEvidenceIdea)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var primaryPrompt = promptPool.FirstOrDefault()
            ?? $"Guarde uma prova curta mostrando o que {BuildShortName(child.FullName)} conseguiu fazer com o apoio externo.";
        var preferredCapture = DeterminePreferredCapture(promptPool);
        var focusLabel = string.IsNullOrWhiteSpace(selectedFocusTitle)
            ? item.Title
            : selectedFocusTitle;

        return new EvidenceAutomationViewModel
        {
            Headline = justCompleted
                ? "Conteudo externo concluido: salve a prova agora"
                : "Qual prova guardar quando este conteudo terminar",
            Summary = isCompleted
                ? $"Este conteudo ja entrou como concluido para {BuildShortName(child.FullName)}. Falta transformar isso em memoria util no dashboard com uma prova simples e objetiva."
                : $"Antes de fechar {focusLabel}, o sistema ja diz qual prova vale mais: foto, video, texto curto e checklist em portugues para nao perder o que foi aprendido.",
            StatusLabel = isCompleted
                ? justCompleted ? "concluido agora" : "concluido"
                : "prepare a prova",
            StatusChipClass = isCompleted ? "success" : "warning",
            ProofTargetLabel = BuildProofTarget(preferredCapture, false),
            PreferredCaptureLabel = BuildPreferredCaptureLabel(preferredCapture),
            QuickTextPrompt = BuildExternalQuickTextPrompt(focusLabel),
            FileHint = $"Se for registrar so uma evidencia deste conteudo, comece por {BuildPreferredCaptureInstruction(preferredCapture)}.",
            WinsPlaceholder = $"Ex.: em {focusLabel.ToLowerInvariant()}, conseguiu sozinho(a) a parte principal da atividade.",
            ChallengesPlaceholder = "Ex.: ainda precisou de ajuda em leitura, ritmo, escrita ou organizacao.",
            NotesPlaceholder = "Ex.: qual material externo usamos, que pagina/etapa foi feita e o que entrou melhor.",
            SaveActionLabel = "Abrir painel da crianca para registrar",
            SaveActionUrl = saveActionUrl,
            CaptureIdeas = BuildCaptureIdeas(primaryPrompt, item.PortugueseGuideNote, preferredCapture),
            ChecklistItems = BuildExternalChecklist(primaryPrompt, focusLabel)
        };
    }

    private static List<EvidenceCaptureSuggestionViewModel> BuildCaptureIdeas(
        string primaryPrompt,
        string outcome,
        EvidenceCaptureKind preferredCapture)
    {
        var shortPrompt = Shorten(primaryPrompt, 150);
        var shortOutcome = Shorten(outcome, 130);

        return new List<EvidenceCaptureSuggestionViewModel>
        {
            new()
            {
                TypeLabel = "Foto",
                TypeChipClass = preferredCapture == EvidenceCaptureKind.Photo ? "success" : "neutral",
                Title = preferredCapture == EvidenceCaptureKind.Photo ? "Comece pela foto final" : "Foto do produto final",
                Prompt = ContainsAny(primaryPrompt, "mapa", "caderno", "pagina", "desenho", "atividade", "massa", "conta", "tracado")
                    ? $"Fotografe o resultado inteiro da atividade. Priorize a parte que prova isto: {shortPrompt}"
                    : $"Fotografe o produto final da aula ou o momento em que a crianca mostra o que fez.",
                WhyItWorks = "Entra rapido no dashboard e prova o que foi concluido sem exigir explicacao longa."
            },
            new()
            {
                TypeLabel = "Video",
                TypeChipClass = preferredCapture == EvidenceCaptureKind.Video ? "success" : "neutral",
                Title = preferredCapture == EvidenceCaptureKind.Video ? "Este e o melhor registro" : "Video curto explicando",
                Prompt = ContainsAny(primaryPrompt, "dizendo", "som", "narr", "leitura", "reconto", "explic", "oral")
                    ? $"Grave 20 a 40 segundos da crianca explicando ou falando em voz alta. Foque nisto: {shortPrompt}"
                    : $"Grave um video curto mostrando o resultado e a crianca dizendo o que aprendeu ou como resolveu.",
                WhyItWorks = "Capta fala, autonomia e entendimento real, o que costuma valer mais que a folha sozinha."
            },
            new()
            {
                TypeLabel = "Texto curto",
                TypeChipClass = preferredCapture == EvidenceCaptureKind.Text ? "success" : "neutral",
                Title = "Legenda pronta para o adulto",
                Prompt = $"Escreva em 2 linhas: o que conseguiu, onde precisou de ajuda e qual foi a melhor pista da aula. Resultado esperado: {shortOutcome}",
                WhyItWorks = "Transforma a midia em registro pedagogico util para releitura futura."
            },
            new()
            {
                TypeLabel = "Checklist",
                TypeChipClass = preferredCapture == EvidenceCaptureKind.Checklist ? "success" : "neutral",
                Title = "Feche a prova em poucos toques",
                Prompt = "Use o checklist abaixo para garantir que a prova mostra produto final, fala da crianca e proximo passo.",
                WhyItWorks = "Evita salvar arquivo solto sem contexto e sem valor para o historico da crianca."
            }
        };
    }

    private static List<string> BuildDailyChecklist(string primaryPrompt, string outcome, bool hasMediaSaved)
    {
        var items = new List<string>
        {
            BuildChecklistLead(primaryPrompt),
            "A crianca explicou ou mostrou o que fez com as proprias palavras.",
            "O adulto registrou se foi sozinho, com ajuda ou com modelo.",
            "O proximo passo ficou anotado em uma frase curta."
        };

        if (!hasMediaSaved)
        {
            items.Insert(0, "Salvar pelo menos uma foto ou um video curto desta aula.");
        }

        if (!string.IsNullOrWhiteSpace(outcome))
        {
            items.Add($"Confirmar se o resultado esperado apareceu: {Shorten(outcome, 110)}");
        }

        return items
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private static List<string> BuildExternalChecklist(string primaryPrompt, string focusLabel)
    {
        return new List<string>
        {
            $"Mostrar claramente a atividade concluida de {focusLabel.ToLowerInvariant()}.",
            BuildChecklistLead(primaryPrompt),
            "Registrar em texto curto o que a crianca conseguiu fazer de verdade.",
            "Anotar se o apoio externo ajudou bastante, ajudou pouco ou ainda precisa voltar.",
            "Salvar a prova no dashboard da crianca no mesmo dia."
        };
    }

    private static string BuildDailyQuickTextPrompt(string primaryPrompt, string outcome)
    {
        var outcomeCopy = string.IsNullOrWhiteSpace(outcome)
            ? "conseguiu concluir a parte principal da aula"
            : Shorten(outcome, 90);

        return $"Ex.: hoje {outcomeCopy}; a melhor prova foi {Shorten(primaryPrompt, 80).ToLowerInvariant()}.";
    }

    private static string BuildExternalQuickTextPrompt(string focusLabel)
    {
        return $"Ex.: em {focusLabel.ToLowerInvariant()}, conseguiu completar a proposta e mostrou entendimento ao explicar o que fez.";
    }

    private static string BuildDailyFileHint(string primaryPrompt, EvidenceCaptureKind preferredCapture, bool hasMediaSaved)
    {
        if (hasMediaSaved)
        {
            return "Ja existe uma midia salva hoje. Se quiser reforcar a prova, envie mais uma captura curta ou complete o texto de contexto.";
        }

        return preferredCapture switch
        {
            EvidenceCaptureKind.Video => $"Priorize um video curto: {Shorten(primaryPrompt, 110)}",
            EvidenceCaptureKind.Photo => $"Priorize uma foto clara do resultado final: {Shorten(primaryPrompt, 110)}",
            EvidenceCaptureKind.Text => "Se a midia falhar, pelo menos registre um texto curto dizendo o que a crianca fez e onde precisou de ajuda.",
            _ => "Se estiver com pouco tempo, salve uma foto ou video curto e complete com duas linhas de contexto."
        };
    }

    private static string BuildWinsPlaceholder(string primaryPrompt)
    {
        if (ContainsAny(primaryPrompt, "som", "letra", "palavra", "leitura"))
        {
            return "Ex.: reconheceu a letra, falou o som certo e encontrou palavras da casa.";
        }

        if (ContainsAny(primaryPrompt, "conta", "numero", "quantidade", "matemat"))
        {
            return "Ex.: contou sozinho, explicou a resposta e usou o material concreto sem travar.";
        }

        return "Ex.: fez a atividade principal, explicou o que aprendeu e concluiu a etapa com seguranca.";
    }

    private static string BuildChallengesPlaceholder(string focusAreaLabel)
    {
        if (string.Equals(focusAreaLabel, "Linguagem", StringComparison.OrdinalIgnoreCase))
        {
            return "Ex.: cansou na hora de ler, trocou sons ou precisou repetir junto.";
        }

        if (string.Equals(focusAreaLabel, "Matematica", StringComparison.OrdinalIgnoreCase))
        {
            return "Ex.: travou na contagem, pulou etapas ou precisou voltar para o concreto.";
        }

        return "Ex.: perdeu o foco, precisou de mais ajuda ou a etapa precisou ficar menor.";
    }

    private static string BuildProofTarget(EvidenceCaptureKind preferredCapture, bool hasMediaSaved)
    {
        if (hasMediaSaved)
        {
            return "1 midia + 1 texto curto";
        }

        return preferredCapture switch
        {
            EvidenceCaptureKind.Video => "1 video curto + 1 texto curto",
            EvidenceCaptureKind.Photo => "1 foto + 1 texto curto",
            EvidenceCaptureKind.Text => "1 texto curto + checklist",
            _ => "1 foto ou video + checklist"
        };
    }

    private static string BuildPreferredCaptureLabel(EvidenceCaptureKind preferredCapture) => preferredCapture switch
    {
        EvidenceCaptureKind.Video => "melhor prova: video curto",
        EvidenceCaptureKind.Photo => "melhor prova: foto final",
        EvidenceCaptureKind.Text => "melhor prova: texto curto",
        _ => "melhor prova: checklist guiado"
    };

    private static string BuildPreferredCaptureInstruction(EvidenceCaptureKind preferredCapture) => preferredCapture switch
    {
        EvidenceCaptureKind.Video => "um video curto da crianca explicando o que fez",
        EvidenceCaptureKind.Photo => "uma foto clara do produto final",
        EvidenceCaptureKind.Text => "um texto curto dizendo o que conseguiu",
        _ => "um checklist simples com contexto"
    };

    private static string BuildChecklistLead(string primaryPrompt)
    {
        if (ContainsAny(primaryPrompt, "video", "audio", "explic", "reconto", "narr"))
        {
            return "A prova mostra a fala da crianca e nao so o material pronto.";
        }

        if (ContainsAny(primaryPrompt, "foto", "pagina", "caderno", "desenho", "mapa"))
        {
            return "A prova mostra o produto final inteiro, sem cortar a parte principal.";
        }

        return $"A prova bate com o objetivo combinado: {Shorten(primaryPrompt, 100)}";
    }

    private static EvidenceCaptureKind DeterminePreferredCapture(IReadOnlyCollection<string> prompts)
    {
        if (prompts.Any(prompt => ContainsAny(prompt, "video", "audio", "fala", "dizendo", "narr", "reconto", "som", "ler", "leitura", "explic")))
        {
            return EvidenceCaptureKind.Video;
        }

        if (prompts.Any(prompt => ContainsAny(prompt, "foto", "imagem", "pagina", "caderno", "desenho", "mapa", "atividade", "massa", "tracado")))
        {
            return EvidenceCaptureKind.Photo;
        }

        if (prompts.Any(prompt => ContainsAny(prompt, "texto", "frase", "escreva", "escrita", "anote", "responda")))
        {
            return EvidenceCaptureKind.Text;
        }

        return EvidenceCaptureKind.Checklist;
    }

    private static bool ContainsAny(string source, params string[] keywords)
    {
        return keywords.Any(keyword => source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildShortName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "a crianca";
        }

        return fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
            ?? fullName;
    }

    private static string Shorten(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var clean = value.Trim().TrimEnd('.');
        return clean.Length <= maxLength ? clean : $"{clean[..(maxLength - 3)]}...";
    }

    private enum EvidenceCaptureKind
    {
        Photo,
        Video,
        Text,
        Checklist
    }
}
