using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Models;
using NewSchool.Web.Services;

namespace NewSchool.Web.Controllers;

public class HomeController(ApplicationDbContext db, ReferralService referralService) : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index()
    {
        ApplySeo(
            "Ensino domiciliar simples no celular",
            "Curriculo, aula do dia, leituras, materiais e evidencias em um fluxo mobile para familias brasileiras que ensinam em casa.");

        var vm = new HomeIndexViewModel
        {
            TotalFamilies = await db.Users.CountAsync(x => x.Role == Domain.UserRole.Parent),
            TotalChildren = await db.Children.CountAsync(),
            CurriculumItems = await db.CurriculumTemplates.CountAsync(),
            ActiveSubscribers = await db.Users.CountAsync(x => x.Role == Domain.UserRole.Parent && x.SubscriptionStatus == "active"),
            SessionsDelivered = await db.LearningSessions.CountAsync(),
            EvidenceCaptured = await db.LearningSessions.CountAsync(x => x.MediaUrl != ""),
            MonthlySnapshotsRecorded = await db.ChildMonthlySnapshots.CountAsync(),
            FamiliesTrackingEvolution = await db.Children.CountAsync(x => x.MonthlySnapshots.Any()),
            TrackOffers = BuildTrackOffers(),
            ResourceSources = BuildResourceSources()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("convite/{code}")]
    public async Task<IActionResult> Invite(string code)
    {
        var vm = await referralService.BuildReferralLandingAsync(code, $"{Request.Scheme}://{Request.Host}");
        if (vm is null)
        {
            return RedirectToAction(nameof(Index));
        }

        ApplySeo(
            "Convite para ensino domiciliar organizado",
            "Convite de uma familia para conhecer uma rotina mais organizada no NewSchool Family.",
            noIndex: true);
        ViewData["HideFooterAd"] = true;

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("trilhas/{track}")]
    public IActionResult TrackOffer(string track)
    {
        var vm = BuildTrackOffer(track, $"{Request.Scheme}://{Request.Host}");
        if (vm is null)
        {
            return RedirectToAction(nameof(Index));
        }

        ApplySeo(
            $"{vm.TrackLabel} para ensino domiciliar em casa",
            vm.Lead);

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("sobre")]
    public IActionResult About()
    {
        ApplySeo(
            "Sobre o NewSchool Family",
            "Conheca a proposta do NewSchool Family para organizar curriculo, rotina e evidencias no ensino domiciliar.");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("contato")]
    public IActionResult Contact()
    {
        ApplySeo(
            "Contato do NewSchool Family",
            "Email publico de contato da plataforma NewSchool Family.");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("politica-de-privacidade")]
    public IActionResult Privacy()
    {
        ApplySeo(
            "Politica de Privacidade do NewSchool Family",
            "Resumo simples sobre dados de conta, criancas, evidencias e pagamentos dentro da plataforma.");
        return View();
    }

    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [HttpGet("robots.txt")]
    public IActionResult RobotsTxt()
    {
        var baseUrl = GetBaseUrl();
        var lines = new[]
        {
            "User-agent: *",
            "Allow: /",
            "Disallow: /Account/",
            "Disallow: /Admin/",
            "Disallow: /Parent/",
            "Disallow: /Library/",
            "Disallow: /Billing/",
            "Disallow: /api/",
            $"Sitemap: {baseUrl}/sitemap.xml"
        };

        return Content(string.Join('\n', lines), "text/plain; charset=utf-8");
    }

    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [HttpGet("sitemap.xml")]
    public IActionResult SitemapXml()
    {
        var baseUrl = GetBaseUrl();
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(
                ns + "urlset",
                BuildSiteMapEntries(baseUrl).Select(entry =>
                    new XElement(
                        ns + "url",
                        new XElement(ns + "loc", entry.Location),
                        new XElement(ns + "lastmod", now),
                        new XElement(ns + "changefreq", entry.ChangeFrequency),
                        new XElement(ns + "priority", entry.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture))))));

        return Content(document.ToString(), "application/xml; charset=utf-8");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static List<TrackOfferLinkViewModel> BuildTrackOffers()
    {
        return
        [
            new() { TrackCode = "literacy", TrackLabel = "Alfabetizacao", Url = "/trilhas/literacy", Description = "Para familias que querem destravar leitura, escrita e linguagem com clareza." },
            new() { TrackCode = "math_foundations", TrackLabel = "Matematica base", Url = "/trilhas/math_foundations", Description = "Para criar seguranca em numeros, operacoes e raciocinio do cotidiano." },
            new() { TrackCode = "autonomy", TrackLabel = "Autonomia e foco", Url = "/trilhas/autonomy", Description = "Para reduzir dispersao, criar ritmo e fortalecer independencia." },
            new() { TrackCode = "science_discovery", TrackLabel = "Ciencias em casa", Url = "/trilhas/science_discovery", Description = "Para familias que querem curiosidade, investigacao e projetos mais vivos." }
        ];
    }

    private static List<HomeResourceSourceViewModel> BuildResourceSources()
    {
        return
        [
            new()
            {
                Title = "Educalar",
                Description = "Materiais gratuitos e conteudo para educacao domiciliar pensado para a realidade brasileira.",
                Url = "https://www.educalar.com.br/materiais-gratuitos",
                ActionLabel = "Abrir fonte"
            },
            new()
            {
                Title = "Baixe Livros",
                Description = "Catalogos por etapa escolar para educacao infantil, alfabetizacao, matematica, ciencias e apoio curricular.",
                Url = "https://www.baixelivros.com.br/didaticos#1",
                ActionLabel = "Abrir fonte"
            },
            new()
            {
                Title = "Archive Public Domain",
                Description = "PDFs realmente publicos para impressao rapida, como apoio direto na alfabetizacao inicial.",
                Url = "https://archivepublicdomain.com/files/2025/09/aprendendo-o-alfabeto.pdf?t=ead6113d4b3b96f8f57f98a45259e9259cc5c19d7630e86fe2ab7038006d532e",
                ActionLabel = "Abrir PDF"
            }
        ];
    }

    private static TrackOfferViewModel? BuildTrackOffer(string? track, string baseUrl)
    {
        var normalized = (track ?? string.Empty).Trim().ToLowerInvariant();
        var registerUrl = $"{baseUrl.TrimEnd('/')}/Account/Register?track={normalized}";
        var secondaryUrl = $"{baseUrl.TrimEnd('/')}/";

        return normalized switch
        {
            "literacy" => new TrackOfferViewModel
            {
                TrackCode = normalized,
                TrackLabel = "Alfabetizacao",
                Eyebrow = "Oferta por trilha",
                Headline = "A trilha de alfabetizacao do NewSchool transforma rotina em leitura, escrita e progresso visivel.",
                Lead = "Feita para familias que querem parar de improvisar e comecar a ver a crianca ganhar linguagem, consciencia fonologica, leitura inicial e producao escrita com mais seguranca.",
                Promise = "Voce nao compra so atividades. Voce compra um caminho guiado para a alfabetizacao com prova de evolucao.",
                RegisterUrl = registerUrl,
                SecondaryUrl = secondaryUrl,
                SocialProof = "Ideal para familias que sentem que a crianca esta pronta para dar um salto em letras, sons, leitura e expressao.",
                ConversionHook = "Entre agora e comece com a trilha de alfabetizacao ja preselecionada no onboarding.",
                Outcomes = ["consciencia fonologica", "leitura inicial", "frases com sentido", "mais confianca para o adulto conduzir"],
                ProofBlocks = ["plano diario focado em linguagem", "metas mensais coerentes com alfabetizacao", "portfolio e relatorios para mostrar a evolucao"],
                ParentWins = ["menos duvida sobre o que ensinar", "mais clareza sobre o que revisar", "sensacao de acompanhamento premium em casa"]
            },
            "math_foundations" => new TrackOfferViewModel
            {
                TrackCode = normalized,
                TrackLabel = "Matematica base",
                Eyebrow = "Oferta por trilha",
                Headline = "A trilha de matematica base do NewSchool organiza o raciocinio e tira o medo dos numeros do dia a dia.",
                Lead = "Feita para familias que querem fortalecer contagem, operacoes, resolucao de problemas e estrategia com uma progressao clara e concreta.",
                Promise = "Voce ganha um sistema que mostra o que praticar hoje e prova quando a crianca esta consolidando a base matematica.",
                RegisterUrl = registerUrl,
                SecondaryUrl = secondaryUrl,
                SocialProof = "Ideal para quem quer sair do 'faz continha avulsa' e construir raciocinio que realmente sustenta o aprendizado.",
                ConversionHook = "Comece agora com a trilha de matematica base ja pronta para entrar no primeiro cadastro da crianca.",
                Outcomes = ["contagem e quantidade", "operacoes com sentido", "problemas do cotidiano", "mais confianca na logica"],
                ProofBlocks = ["blocos diarios focados em matematica", "metas mensais por habilidade", "historico para provar ganho de dominio"],
                ParentWins = ["menos improviso nas atividades", "mais visibilidade do progresso", "seguranca para reforcar o que ainda esta fraco"]
            },
            "autonomy" => new TrackOfferViewModel
            {
                TrackCode = normalized,
                TrackLabel = "Autonomia e foco",
                Eyebrow = "Oferta por trilha",
                Headline = "A trilha de autonomia e foco do NewSchool ajuda a crianca a sustentar rotina, seguir etapas e ganhar independencia.",
                Lead = "Feita para familias que sentem que o maior gargalo nao e conteudo, e conseguir que a crianca engaje, mantenha atencao e termine o que comecou.",
                Promise = "Voce ganha um caminho guiado para foco, organizacao e constancia, com sinais claros de evolucao no comportamento de estudo.",
                RegisterUrl = registerUrl,
                SecondaryUrl = secondaryUrl,
                SocialProof = "Ideal para quem precisa de mais paz no dia a dia e de uma rotina que funcione de verdade dentro de casa.",
                ConversionHook = "Comece agora com a trilha de autonomia e foco ja preselecionada no onboarding.",
                Outcomes = ["mais foco nas atividades", "mais autonomia", "menos desgaste para o adulto", "rotina mais sustentavel"],
                ProofBlocks = ["missões em etapas", "metas mensais de autonomia", "intervencao automatica quando o ritmo cai"],
                ParentWins = ["menos briga para comecar", "mais clareza no que cobrar", "mais sensacao de progresso comportamental"]
            },
            "science_discovery" => new TrackOfferViewModel
            {
                TrackCode = normalized,
                TrackLabel = "Ciencias em casa",
                Eyebrow = "Oferta por trilha",
                Headline = "A trilha de ciencias em casa do NewSchool transforma curiosidade em investigacao, repertorio e descobertas guardaveis.",
                Lead = "Feita para familias que querem um ensino domiciliar mais vivo, com observacao, experimentos, perguntas e projetos que a crianca tem orgulho de mostrar.",
                Promise = "Voce ganha uma jornada guiada de descoberta, com atividades concretas e evidencias que fazem a evolucao parecer premium.",
                RegisterUrl = registerUrl,
                SecondaryUrl = secondaryUrl,
                SocialProof = "Ideal para quem quer despertar curiosidade real e sair da sensacao de ensino repetitivo.",
                ConversionHook = "Entre agora com a trilha de ciencias em casa ja pronta para o primeiro cadastro.",
                Outcomes = ["mais curiosidade", "observacao e investigacao", "projetos com sentido", "portfolio mais encantador"],
                ProofBlocks = ["plano diario com experiencias concretas", "metas mensais ligadas a descoberta", "evidencias e portfolio que a familia pode guardar"],
                ParentWins = ["mais entusiasmo no ensino em casa", "mais material bonito para compartilhar", "mais sensacao de jornada viva"]
            },
            _ => null
        };
    }

    private void ApplySeo(string title, string description, bool noIndex = false)
    {
        ViewData["Title"] = title;
        ViewData["MetaDescription"] = description;
        ViewData["MetaRobots"] = noIndex ? "noindex,nofollow" : "index,follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        ViewData["OpenGraphType"] = "website";
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}".TrimEnd('/');
    }

    private static List<SiteMapEntry> BuildSiteMapEntries(string baseUrl)
    {
        var entries = new List<SiteMapEntry>
        {
            new($"{baseUrl}/", "weekly", 1.0m),
            new($"{baseUrl}/sobre", "monthly", 0.6m),
            new($"{baseUrl}/contato", "monthly", 0.5m),
            new($"{baseUrl}/politica-de-privacidade", "monthly", 0.5m)
        };

        entries.AddRange(
            BuildTrackOffers().Select(track => new SiteMapEntry($"{baseUrl}{track.Url}", "weekly", 0.8m)));

        return entries;
    }

    private sealed record SiteMapEntry(string Location, string ChangeFrequency, decimal Priority);
}
