using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using Resend;
using Stripe;

namespace NewSchool.Web.Services;

public class EmailAutomationService(
    ApplicationDbContext db,
    IWebHostEnvironment environment,
    IOptions<EmailSettings> emailOptions,
    ChildGoalCycleService childGoalCycleService,
    IResend resend,
    ILogger<EmailAutomationService> logger)
{
    private readonly EmailSettings _settings = emailOptions.Value;

    public async Task SendOnboardingEmailIfNeededAsync(AppUser user)
    {
        if (user.OnboardingEmailSentAt.HasValue)
        {
            return;
        }

        await SendCampaignAsync(user, EmailCampaignType.Welcome, force: true);
        user.OnboardingEmailSentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SendTrialReminderIfNeededAsync(AppUser user)
    {
        if (user.TrialEndsAt is null || user.SubscriptionStatus == "active")
        {
            return;
        }

        var daysLeft = (user.TrialEndsAt.Value.Date - DateTime.UtcNow.Date).Days;
        if (daysLeft > 2 || user.TrialReminderSentAt.HasValue)
        {
            return;
        }

        await SendCampaignAsync(user, EmailCampaignType.TrialEnding, force: true);
        user.TrialReminderSentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SendReactivationEmailIfNeededAsync(AppUser user)
    {
        var inactiveForDays = user.LastActiveAt.HasValue
            ? (DateTime.UtcNow.Date - user.LastActiveAt.Value.Date).Days
            : 99;

        var shouldSendForCanceled = user.SubscriptionStatus == "canceled" && inactiveForDays >= 2;
        var shouldSendForInactiveUse = user.SubscriptionStatus == "active" && inactiveForDays >= 7;

        if ((!shouldSendForCanceled && !shouldSendForInactiveUse) || user.ReactivationEmailSentAt.HasValue)
        {
            return;
        }

        await SendCampaignAsync(user, EmailCampaignType.Reactivation, force: true);
        user.ReactivationEmailSentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SendPaymentRecoveryEmailIfNeededAsync(AppUser user)
    {
        if (user.SubscriptionStatus != "past_due" && user.SubscriptionStatus != "unpaid")
        {
            return;
        }

        if (user.PaymentRecoveryEmailSentAt.HasValue && user.PaymentRecoveryEmailSentAt.Value.Date >= DateTime.UtcNow.Date)
        {
            return;
        }

        await SendCampaignAsync(user, EmailCampaignType.PaymentFailed, force: true);
        user.PaymentRecoveryEmailSentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SendChildProgressRiskEmailIfNeededAsync(AppUser user)
    {
        var now = DateTime.UtcNow;
        var childIds = await db.Children
            .Where(x => x.ParentId == user.Id)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var childId in childIds)
        {
            await childGoalCycleService.SyncCurrentCycleAsync(childId, user.Id);
        }

        var cycles = await db.ChildMonthlyGoalCycles
            .Include(x => x.Child)
            .Include(x => x.Items)
            .Where(x => x.Child.ParentId == user.Id)
            .Where(x => x.Year == now.Year && x.Month == now.Month)
            .Where(x => x.RiskLevel == "high")
            .Where(x => !x.RetentionAlertSentAt.HasValue)
            .OrderBy(x => x.Child.FullName)
            .ToListAsync();

        if (cycles.Count == 0)
        {
            return;
        }

        await SendCampaignAsync(user, EmailCampaignType.ChildProgressRisk, force: true);

        foreach (var cycle in cycles)
        {
            cycle.RetentionAlertSentAt = now;
        }

        await db.SaveChangesAsync();
    }

    public async Task SendCampaignAsync(AppUser user, EmailCampaignType campaign, bool force = false)
    {
        if (!force && !ShouldSendCampaign(user, campaign))
        {
            return;
        }

        var model = await BuildCampaignAsync(user, campaign);
        await SendEmailAsync(user.Email, model.Subject, model.TextBody, model.HtmlBody);
    }

    public async Task<int> RunRecoveryAutomationAsync()
    {
        var parents = db.Users.Where(x => x.Role == UserRole.Parent).ToList();
        var sent = 0;

        foreach (var user in parents)
        {
            var before = GetEmailTouchCount(user);

            await SendTrialReminderIfNeededAsync(user);
            await SendReactivationEmailIfNeededAsync(user);
            await SendPaymentRecoveryEmailIfNeededAsync(user);
            await SendChildProgressRiskEmailIfNeededAsync(user);

            if (GetEmailTouchCount(user) > before)
            {
                sent++;
            }
        }

        return sent;
    }

    private bool ShouldSendCampaign(AppUser user, EmailCampaignType campaign)
    {
        return campaign switch
        {
            EmailCampaignType.Welcome => !user.OnboardingEmailSentAt.HasValue,
            EmailCampaignType.TrialEnding => !user.TrialReminderSentAt.HasValue,
            EmailCampaignType.PaymentFailed => !user.PaymentRecoveryEmailSentAt.HasValue,
            EmailCampaignType.Reactivation => !user.ReactivationEmailSentAt.HasValue,
            EmailCampaignType.ChildProgressRisk => true,
            _ => false
        };
    }

    private async Task<EmailCampaignContent> BuildCampaignAsync(AppUser user, EmailCampaignType campaign)
    {
        var dashboardUrl = BuildAbsoluteUrl("/Parent/Index");

        return campaign switch
        {
            EmailCampaignType.Welcome => BuildWelcomeCampaign(user, dashboardUrl),
            EmailCampaignType.TrialEnding => BuildTrialEndingCampaign(user, dashboardUrl),
            EmailCampaignType.PaymentFailed => BuildPaymentFailedCampaign(user, await BuildBillingPortalLinkAsync(user) ?? dashboardUrl),
            EmailCampaignType.Reactivation => BuildReactivationCampaign(user, dashboardUrl),
            EmailCampaignType.ChildProgressRisk => await BuildChildProgressRiskCampaignAsync(user, dashboardUrl),
            _ => BuildWelcomeCampaign(user, dashboardUrl)
        };
    }

    private EmailCampaignContent BuildWelcomeCampaign(AppUser user, string dashboardUrl)
    {
        var trialText = user.TrialEndsAt.HasValue
            ? $"Seu periodo de teste vai ate {user.TrialEndsAt:dd/MM/yyyy}."
            : "Seu acesso experimental ja esta liberado.";

        return BuildCampaignLayout(
            preheader: "Sua familia ja pode comecar a rotina guiada do NewSchool.",
            title: $"Bem-vindo, {FirstName(user.FullName)}.",
            subtitle: "A base da sua rotina de ensino domiciliar ja esta pronta.",
            bodyLines:
            [
                "O NewSchool foi criado para tirar peso do adulto e trazer clareza para o dia a dia de ensino em casa.",
                $"{trialText} Agora o melhor proximo passo e cadastrar seu filho, abrir a primeira rotina e registrar a primeira sessao.",
                "Quando voce comeca, o sistema organiza trilhas por idade, reforca habilidades fracas e monta o plano de amanha automaticamente."
            ],
            spotlightTitle: "O que voce libera logo no primeiro uso",
            spotlightPoints:
            [
                "Rotina diaria por idade com foco em linguagem, matematica e autonomia",
                "Guia do adulto para ensinar mesmo sem formacao pedagogica",
                "Registro rapido e relatorio semanal com leitura clara da evolucao"
            ],
            primaryLabel: "Comecar minha rotina",
            primaryUrl: dashboardUrl,
            secondaryLabel: "Falar com a StarkAid",
            secondaryUrl: $"mailto:{_settings.ResendReplyTo}",
            footer: "Voce nao precisa improvisar sozinho. O NewSchool foi desenhado para ajudar sua familia a ensinar com mais seguranca e consistencia.",
            accent: "#2c6e63",
            subject: "Sua rotina guiada no NewSchool ja pode comecar");
    }

    private EmailCampaignContent BuildTrialEndingCampaign(AppUser user, string dashboardUrl)
    {
        var daysLeft = user.TrialEndsAt.HasValue
            ? Math.Max((user.TrialEndsAt.Value.Date - DateTime.UtcNow.Date).Days, 0)
            : 0;

        return BuildCampaignLayout(
            preheader: "Seu trial esta acabando e sua rotina premium pode ser interrompida.",
            title: $"Faltam {daysLeft} dia(s) para o fim do seu trial.",
            subtitle: "Nao deixe a rotina da familia esfriar agora.",
            bodyLines:
            [
                "Quando o trial termina, o cadastro de criancas, os planos diarios adaptativos e os relatorios premium deixam de ficar disponiveis.",
                "Se a sua familia ja comecou a usar, este e o melhor momento para manter o ritmo e preservar o historico pedagogico.",
                "Assinar agora significa continuar com um sistema que organiza o que ensinar, como ensinar e o que reforcar depois."
            ],
            spotlightTitle: "Por que vale manter o acesso",
            spotlightPoints:
            [
                "A rotina nao para no meio do progresso da crianca",
                "O sistema continua ajustando reforco e avancos automaticamente",
                "Os pais ganham clareza com relatorios, trilhas e recomendacoes da proxima semana"
            ],
            primaryLabel: "Garantir minha assinatura",
            primaryUrl: dashboardUrl,
            secondaryLabel: "Voltar ao painel",
            secondaryUrl: dashboardUrl,
            footer: "A melhor venda do NewSchool e quando a familia sente que o dia ficou mais leve. Esse ganho aparece quando a rotina continua.",
            accent: "#b85c38",
            subject: "Seu trial esta acabando. Mantenha a rotina da familia ativa.");
    }

    private EmailCampaignContent BuildPaymentFailedCampaign(AppUser user, string billingUrl)
    {
        return BuildCampaignLayout(
            preheader: "Houve uma falha de cobranca e seu acesso pode ser interrompido.",
            title: "Seu pagamento precisa de atencao agora.",
            subtitle: "Corrija em poucos minutos e evite perder o ritmo da familia.",
            bodyLines:
            [
                "A Stripe sinalizou uma falha na renovacao da sua assinatura.",
                "Isso normalmente acontece por cartao vencido, limite, bloqueio do banco ou dados desatualizados.",
                "Atualize o pagamento agora para nao interromper as rotinas diarias, os relatorios e o acompanhamento pedagogico da casa."
            ],
            spotlightTitle: "O que voce protege ao regularizar hoje",
            spotlightPoints:
            [
                "Acesso continuo aos planos diarios por idade",
                "Historico de progresso e recomendacoes pedagogicas",
                "Relatorios semanais e automacoes de reforco da crianca"
            ],
            primaryLabel: "Atualizar pagamento",
            primaryUrl: billingUrl,
            secondaryLabel: "Abrir painel",
            secondaryUrl: BuildAbsoluteUrl("/Parent/Index"),
            footer: "Quanto mais rapido voce ajusta a cobranca, mais facil fica manter a rotina sem quebra de consistencia.",
            accent: "#c24f4f",
            subject: "Falha na cobranca: atualize o pagamento da sua assinatura");
    }

    private EmailCampaignContent BuildReactivationCampaign(AppUser user, string dashboardUrl)
    {
        var inactiveForDays = user.LastActiveAt.HasValue
            ? (DateTime.UtcNow.Date - user.LastActiveAt.Value.Date).Days
            : 0;

        var title = user.SubscriptionStatus == "canceled"
            ? "Sua assinatura pode voltar a qualquer momento."
            : "Sua rotina esta pedindo uma retomada.";

        var subtitle = user.SubscriptionStatus == "canceled"
            ? "Seu historico continua aqui. Voce nao precisa recomecar do zero."
            : "Retomar agora e mais facil do que recomeçar depois.";

        var bodyLines = user.SubscriptionStatus == "canceled"
            ? new List<string>
            {
                "Mesmo com a assinatura cancelada, a estrutura que sua familia construiu continua salva.",
                "Reativar agora devolve acesso aos planos, relatórios e recomendacoes sem perder o que ja foi construído.",
                "Se a intencao era pausar, tudo bem. Mas se a meta continua sendo ensinar seus filhos em casa com mais clareza, vale muito retomar."
            }
            : new List<string>
            {
                $"Percebemos que a familia esta sem registrar atividade ha {inactiveForDays} dias.",
                "Quando a rotina esfria, o maior custo nao e so perder dias. E perder continuidade, clareza e confianca do adulto na condução.",
                "Volte hoje e deixe o sistema organizar o proximo passo, reforcar o que esta fraco e reacender o ritmo de aprendizagem."
            };

        return BuildCampaignLayout(
            preheader: "A rotina da familia ainda pode ser retomada com clareza e sem improviso.",
            title: title,
            subtitle: subtitle,
            bodyLines: bodyLines,
            spotlightTitle: "O que voce encontra ao voltar",
            spotlightPoints:
            [
                "Habilidades fracas destacadas com clareza",
                "Plano do dia e do amanha ja organizados",
                "Leitura simples do que revisar, consolidar e avancar"
            ],
            primaryLabel: user.SubscriptionStatus == "canceled" ? "Reativar agora" : "Retomar rotina",
            primaryUrl: dashboardUrl,
            secondaryLabel: "Ver painel da familia",
            secondaryUrl: dashboardUrl,
            footer: "Ensino domiciliar funciona melhor quando o adulto nao precisa decidir tudo sozinho. Esse e o papel do NewSchool.",
            accent: "#6d5bd0",
            subject: user.SubscriptionStatus == "canceled"
                ? "Reative sua assinatura e retome a rotina da familia"
                : "Sua familia pode retomar a rotina hoje");
    }

    private async Task<EmailCampaignContent> BuildChildProgressRiskCampaignAsync(AppUser user, string dashboardUrl)
    {
        var now = DateTime.UtcNow;
        var cycles = await db.ChildMonthlyGoalCycles
            .Include(x => x.Child)
            .Include(x => x.Items)
            .Where(x => x.Child.ParentId == user.Id)
            .Where(x => x.Year == now.Year && x.Month == now.Month)
            .Where(x => x.RiskLevel == "high")
            .OrderBy(x => x.Child.FullName)
            .ToListAsync();

        var recoveryPlanUrl = dashboardUrl;
        var firstCycle = cycles.FirstOrDefault();
        if (firstCycle is not null)
        {
            var plan = await db.ChildRecoveryPlans
                .Where(x => x.ChildId == firstCycle.ChildId && x.Status == "active")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (plan is not null)
            {
                recoveryPlanUrl = BuildAbsoluteUrl($"/Parent/RecoveryPlan/{firstCycle.ChildId}");
            }
        }

        var spotlightPoints = cycles
            .Select(cycle =>
            {
                var urgentGoal = cycle.Items
                    .OrderBy(x => x.Status == "completed" ? 1 : 0)
                    .ThenBy(x => x.PriorityOrder)
                    .FirstOrDefault()?.SkillName ?? "retomar a rotina";
                return $"{cycle.Child.FullName}: foco urgente em {urgentGoal.ToLowerInvariant()}";
            })
            .Take(3)
            .ToList();

        if (spotlightPoints.Count == 0)
        {
            spotlightPoints.Add("Retome a rotina guiada para o sistema voltar a consolidar metas e progresso.");
        }

        return BuildCampaignLayout(
            preheader: "Detectamos uma crianca com metas do mes em risco e vale agir agora.",
            title: "Sua rotina precisa de uma retomada guiada agora.",
            subtitle: "O sistema detectou sinais de esfriamento pedagogico e metas que podem escapar neste ciclo.",
            bodyLines:
            [
                "Quando a familia perde alguns dias, o maior risco nao e so pausar a plataforma. E quebrar a consistencia que sustenta o progresso da crianca.",
                "O NewSchool ja identificou quais metas do mes pedem atencao imediata para sua familia voltar ao ritmo com clareza.",
                "Voltar hoje ajuda a recuperar seguranca, manter a rotina e evitar que o mes termine sem a consolidacao esperada."
            ],
            spotlightTitle: "Criancas ou metas que pedem acao",
            spotlightPoints: spotlightPoints,
            primaryLabel: "Abrir minha Central de Evolucao",
            primaryUrl: recoveryPlanUrl,
            secondaryLabel: "Voltar ao painel",
            secondaryUrl: dashboardUrl,
            footer: "A melhor retencao acontece quando a familia sente que ainda da tempo de corrigir a rota. Esse email existe para isso.",
            accent: "#9a5a14",
            subject: "Metas do mes em risco: vale retomar a rotina agora");
    }

    private EmailCampaignContent BuildCampaignLayout(
        string preheader,
        string title,
        string subtitle,
        IReadOnlyList<string> bodyLines,
        string spotlightTitle,
        IReadOnlyList<string> spotlightPoints,
        string primaryLabel,
        string primaryUrl,
        string? secondaryLabel,
        string? secondaryUrl,
        string footer,
        string accent,
        string subject)
    {
        var bulletItems = string.Join("", spotlightPoints.Select(point => $"<li style=\"margin-bottom:10px;\">{WebUtility.HtmlEncode(point)}</li>"));
        var paragraphs = string.Join("", bodyLines.Select(line => $"<p style=\"margin:0 0 16px;color:#334155;font-size:16px;line-height:1.7;\">{WebUtility.HtmlEncode(line)}</p>"));
        var secondaryLink = string.IsNullOrWhiteSpace(secondaryLabel) || string.IsNullOrWhiteSpace(secondaryUrl)
            ? string.Empty
            : $"<a href=\"{secondaryUrl}\" style=\"display:inline-block;margin-left:14px;color:{accent};font-weight:600;text-decoration:none;\">{WebUtility.HtmlEncode(secondaryLabel)}</a>";

        var html = $"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>{WebUtility.HtmlEncode(subject)}</title>
</head>
<body style="margin:0;padding:0;background:#f4f1ea;">
  <span style="display:none!important;visibility:hidden;opacity:0;color:transparent;height:0;width:0;overflow:hidden;">{WebUtility.HtmlEncode(preheader)}</span>
  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f1ea;padding:32px 12px;">
    <tr>
      <td align="center">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 12px 40px rgba(20,32,39,0.08);">
          <tr>
            <td style="padding:32px 36px;background:linear-gradient(135deg,{accent},#142027);color:#ffffff;">
              <div style="font-size:12px;letter-spacing:0.18em;text-transform:uppercase;opacity:0.84;margin-bottom:14px;">StarkAid School</div>
              <h1 style="margin:0 0 12px;font-size:34px;line-height:1.18;font-family:Georgia,'Times New Roman',serif;">{WebUtility.HtmlEncode(title)}</h1>
              <p style="margin:0;font-size:17px;line-height:1.6;color:rgba(255,255,255,0.9);">{WebUtility.HtmlEncode(subtitle)}</p>
            </td>
          </tr>
          <tr>
            <td style="padding:34px 36px 22px;">
              {paragraphs}
            </td>
          </tr>
          <tr>
            <td style="padding:0 36px 28px;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f8f7f3;border:1px solid #ece7dd;border-radius:18px;">
                <tr>
                  <td style="padding:24px 24px 16px;">
                    <div style="font-size:13px;letter-spacing:0.14em;text-transform:uppercase;color:{accent};font-weight:700;margin-bottom:10px;">Resumo claro</div>
                    <h2 style="margin:0 0 14px;font-size:22px;color:#142027;">{WebUtility.HtmlEncode(spotlightTitle)}</h2>
                    <ul style="margin:0;padding-left:18px;color:#334155;font-size:15px;line-height:1.7;">
                      {bulletItems}
                    </ul>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style="padding:0 36px 30px;">
              <a href="{primaryUrl}" style="display:inline-block;background:{accent};color:#ffffff;text-decoration:none;font-weight:700;padding:15px 22px;border-radius:999px;">{WebUtility.HtmlEncode(primaryLabel)}</a>{secondaryLink}
            </td>
          </tr>
          <tr>
            <td style="padding:0 36px 34px;">
              <p style="margin:0;color:#64748b;font-size:14px;line-height:1.7;">{WebUtility.HtmlEncode(footer)}</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
""";

        var text = string.Join(
            "\n\n",
            new[]
            {
                title,
                subtitle,
                string.Join("\n\n", bodyLines),
                $"{spotlightTitle}:",
                string.Join("\n", spotlightPoints.Select(x => $"- {x}")),
                $"{primaryLabel}: {primaryUrl}",
                !string.IsNullOrWhiteSpace(secondaryLabel) && !string.IsNullOrWhiteSpace(secondaryUrl)
                    ? $"{secondaryLabel}: {secondaryUrl}"
                    : string.Empty,
                footer
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return new EmailCampaignContent(subject, text, html);
    }

    private int GetEmailTouchCount(AppUser user)
    {
        return (user.TrialReminderSentAt.HasValue ? 1 : 0)
             + (user.ReactivationEmailSentAt.HasValue ? 1 : 0)
             + (user.PaymentRecoveryEmailSentAt.HasValue ? 1 : 0);
    }

    private async Task<string?> BuildBillingPortalLinkAsync(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(user.StripeCustomerId))
        {
            return null;
        }

        var baseUrl = _settings.PublicBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        try
        {
            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = user.StripeCustomerId,
                ReturnUrl = $"{baseUrl}/Parent/Index"
            });

            return session.Url;
        }
        catch (StripeException)
        {
            return null;
        }
    }

    private string BuildAbsoluteUrl(string relativePath)
    {
        var baseUrl = _settings.PublicBaseUrl?.TrimEnd('/');
        return string.IsNullOrWhiteSpace(baseUrl)
            ? relativePath
            : $"{baseUrl}{relativePath}";
    }

    private async Task SendEmailAsync(string to, string subject, string textBody, string htmlBody)
    {
        try
        {
            if (UseResendTransport())
            {
                await SendViaResendAsync(to, subject, htmlBody);
                return;
            }

            if (UseSmtpTransport())
            {
                await SendViaSmtpAsync(to, subject, textBody, htmlBody);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Email provider failed for {Recipient}. Falling back to pickup folder.", to);
        }

        try
        {
            await WriteEmailToPickupAsync(to, subject, textBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pickup folder fallback also failed for {Recipient}. Email will not be persisted.", to);
        }
    }

    private bool UseResendTransport()
    {
        return string.Equals(_settings.Transport, "Resend", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(_settings.ResendApiKey);
    }

    private bool UseSmtpTransport()
    {
        return string.Equals(_settings.Transport, "SMTP", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(_settings.SmtpHost);
    }

    private async Task SendViaResendAsync(string to, string subject, string htmlBody)
    {
        var message = new EmailMessage
        {
            From = BuildFromHeader(),
            Subject = subject,
            HtmlBody = htmlBody
        };

        message.To.Add(to);

        if (!string.IsNullOrWhiteSpace(_settings.ResendReplyTo))
        {
            message.ReplyTo = _settings.ResendReplyTo;
        }

        await resend.EmailSendAsync(message);
    }

    private async Task SendViaSmtpAsync(string to, string subject, string textBody, string htmlBody)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
        message.To.Add(to);
        message.Subject = subject;
        message.Body = textBody;
        message.IsBodyHtml = false;
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));

        using var client = new SmtpClient(_settings.SmtpHost!, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
        }

        await client.SendMailAsync(message);
    }

    private async Task WriteEmailToPickupAsync(string to, string subject, string body)
    {
        var folder = _settings.PickupFolder;
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = Path.Combine(environment.ContentRootPath, "App_Data", "emails");
        }

        Directory.CreateDirectory(folder);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{SanitizeFileName(to)}.txt";
        var path = Path.Combine(folder, fileName);

        var content = $"""
From: {_settings.FromName} <{_settings.FromEmail}>
To: {to}
Subject: {subject}
Date: {DateTime.UtcNow:O}

{body}
""";

        await System.IO.File.WriteAllTextAsync(path, content);
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }

    private string BuildFromHeader()
    {
        return string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";
    }

    private static string FirstName(string fullName)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? "familia"
            : fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? fullName;
    }

    private sealed record EmailCampaignContent(string Subject, string TextBody, string HtmlBody);
}
