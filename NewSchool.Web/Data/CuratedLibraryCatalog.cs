using System.Security.Cryptography;
using System.Text;
using NewSchool.Web.Domain;
using NewSchool.Web.Services;

namespace NewSchool.Web.Data;

public static class CuratedLibraryCatalog
{
    private static readonly Guid AprendendoOAlfabetoId = Guid.Parse("94C3076D-C55F-4F74-BDC9-0CA0D0BC06AC");

    public static IReadOnlyList<CuratedLearningResource> BuildResources()
    {
        var resources = new List<CuratedLearningResource>
        {
            new()
            {
                Id = AprendendoOAlfabetoId,
                Slug = "aprendendo-o-alfabeto-public-domain",
                Title = "Aprendendo o Alfabeto",
                Summary = "Apostila em português com letras, traços simples e ilustrações para apoiar a alfabetização inicial sem sair do NewSchool.",
                UseNote = "Use poucas páginas por semana, sempre com objetivo curto, prática orientada e prova simples salva em Evidências.",
                Domain = LearningDomain.Language,
                AgeMin = 3,
                AgeMax = 6,
                FormatLabel = "PDF hospedado em português",
                ResourceKind = "Apostila em domínio público",
                SourceName = "Archive Public Domain",
                SourceUrl = "https://archivepublicdomain.com/files/2025/09/aprendendo-o-alfabeto.pdf?t=ead6113d4b3b96f8f57f98a45259e9259cc5c19d7630e86fe2ab7038006d532e",
                AccessUrl = "/library/public-books/aprendendo-o-alfabeto_pt-br.pdf",
                IsHostedLocally = true,
                LicenseLabel = "Domínio público",
                Attribution = "Arquivo público indicado pela fonte original.",
                LanguageCode = "pt-BR",
                SortOrder = 1
            }
        };

        var sortOrder = 20;
        foreach (var material in ProprietaryFamilyLibraryCatalog.Build()
                     .OrderBy(item => item.RecommendedMinAge)
                     .ThenBy(item => item.IsPrintable)
                     .ThenBy(item => item.Title))
        {
            resources.Add(new CuratedLearningResource
            {
                Id = material.Id,
                Slug = material.Slug,
                Title = material.Title,
                Summary = material.Description,
                UseNote = material.IsPrintable
                    ? "Material autoral do NewSchool pronto para abrir, visualizar e imprimir dentro da rotina da unidade."
                    : "Material autoral do NewSchool para leitura guiada dentro da unidade, sem obrigar a família a procurar conteúdo fora.",
                Domain = InferDomain(material),
                AgeMin = material.RecommendedMinAge,
                AgeMax = material.RecommendedMaxAge,
                FormatLabel = material.IsPrintable ? "Imprimível autoral" : "Livro digital autoral",
                ResourceKind = material.IsPrintable ? "Apostila / prova autoral" : "Livro digital autoral",
                SourceName = "NewSchool",
                SourceUrl = string.Empty,
                AccessUrl = material.IsPrintable
                    ? $"/Library/Printable/{material.Id}"
                    : $"/Library/Book/{material.Id}",
                IsHostedLocally = true,
                LicenseLabel = "Uso interno NewSchool",
                Attribution = "Conteúdo proprietário do NewSchool.",
                LanguageCode = "pt-BR",
                SortOrder = sortOrder++
            });
        }

        return resources;
    }

    public static IReadOnlyList<CuratedTaskTemplate> BuildTasks()
    {
        var resources = BuildResources();
        var resourceByTitle = resources.ToDictionary(item => item.Title, StringComparer.OrdinalIgnoreCase);
        var blueprintService = new ProprietaryCurriculumBlueprintService();
        var tasks = new List<CuratedTaskTemplate>();
        var sortOrder = 1;

        foreach (var age in Enumerable.Range(3, 12))
        {
            foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
            {
                var subject = blueprintService.BuildSubject(age, domain);
                foreach (var phase in subject.Phases.OrderBy(item => item.PhaseNumber))
                {
                    foreach (var unit in phase.Units.OrderBy(item => item.UnitNumber))
                    {
                        foreach (var lesson in unit.Lessons.OrderBy(item => item.LessonNumber))
                        {
                            var primaryResourceId = ResolvePrimaryResourceId(
                                resourceByTitle,
                                lesson.PrimaryMaterialTitle,
                                unit.PrintableTitle,
                                unit.CompanionBookTitle,
                                unit.AssessmentTitle);
                            var supportLink = ResolveSupportLink(
                                resourceByTitle,
                                lesson.SupportMaterialTitle,
                                unit.PrintableTitle,
                                unit.CompanionBookTitle,
                                unit.AssessmentTitle,
                                primaryResourceId);

                            tasks.Add(new CuratedTaskTemplate
                            {
                                Id = CreateDeterministicGuid($"task:{lesson.TaskSlug}"),
                                Slug = lesson.TaskSlug,
                                Title = lesson.Title,
                                Domain = domain,
                                FunctionalTrack = MapFunctionalTrack(domain),
                                AgeMin = age,
                                AgeMax = age,
                                GoalTrack = MapGoalTrack(domain),
                                MatchKeywords = lesson.MatchKeywords,
                                Goal = lesson.Goal,
                                ParentGuide = lesson.OpeningForAdult,
                                ChildPrompt = lesson.AnchorQuestion,
                                TaskSteps = string.Join('\n', lesson.AdultSteps),
                                MaterialsSummary = string.Join(", ", unit.Materials),
                                EvidencePrompt = lesson.EvidencePrompt,
                                ExpectedOutcome = lesson.ExpectedOutcome,
                                SuggestedMinutes = lesson.SuggestedMinutes,
                                PrimaryResourceId = primaryResourceId,
                                SupportLinkLabel = supportLink.Label,
                                SupportLinkUrl = supportLink.Url,
                                SupportLinkSource = supportLink.Source,
                                SortOrder = sortOrder++
                            });
                        }
                    }
                }
            }
        }

        return tasks;
    }

    private static Guid? ResolvePrimaryResourceId(
        IReadOnlyDictionary<string, CuratedLearningResource> resourceByTitle,
        params string[] candidates)
    {
        foreach (var candidate in candidates.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (resourceByTitle.TryGetValue(candidate, out var resource))
            {
                return resource.Id;
            }
        }

        return null;
    }

    private static (string Label, string Url, string Source) ResolveSupportLink(
        IReadOnlyDictionary<string, CuratedLearningResource> resourceByTitle,
        string preferredTitle,
        string printableTitle,
        string companionBookTitle,
        string assessmentTitle,
        Guid? primaryResourceId)
    {
        var candidates = new[]
        {
            preferredTitle,
            printableTitle,
            companionBookTitle,
            assessmentTitle
        };

        foreach (var candidate in candidates.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (!resourceByTitle.TryGetValue(candidate, out var resource) || resource.Id == primaryResourceId)
            {
                continue;
            }

            return (
                resource.FormatLabel.StartsWith("Imprimível", StringComparison.OrdinalIgnoreCase)
                    ? "Abrir apoio de papel da unidade"
                    : "Abrir leitura da unidade",
                resource.AccessUrl,
                "Coleção NewSchool");
        }

        return (string.Empty, string.Empty, string.Empty);
    }

    private static LearningDomain InferDomain(ProprietaryFamilyLibraryMaterialSeed material)
    {
        var haystack = $"{material.Category} {material.SkillFocus} {material.Title}".ToLowerInvariant();
        if (haystack.Contains("matem") || haystack.Contains("numero") || haystack.Contains("porcent") || haystack.Contains("orçamento") || haystack.Contains("orcamento"))
        {
            return LearningDomain.Math;
        }

        if (haystack.Contains("ciências") || haystack.Contains("ciencias") || haystack.Contains("experimento") || haystack.Contains("natureza") || haystack.Contains("fenomen"))
        {
            return LearningDomain.Science;
        }

        if (haystack.Contains("história") || haystack.Contains("historia") || haystack.Contains("biografia") || haystack.Contains("tempo"))
        {
            return LearningDomain.History;
        }

        if (haystack.Contains("geografia") || haystack.Contains("mapa") || haystack.Contains("territ") || haystack.Contains("bairro") || haystack.Contains("regi"))
        {
            return LearningDomain.Geography;
        }

        if (haystack.Contains("autonomia") || haystack.Contains("checklist") || haystack.Contains("rotina") || haystack.Contains("sprint"))
        {
            return LearningDomain.ExecutiveFunction;
        }

        return LearningDomain.Language;
    }

    private static FunctionalSupportTrack MapFunctionalTrack(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => FunctionalSupportTrack.Communication,
        LearningDomain.ExecutiveFunction => FunctionalSupportTrack.Regulation,
        _ => FunctionalSupportTrack.Base
    };

    private static string MapGoalTrack(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "literacy",
        LearningDomain.Math => "math_foundations",
        LearningDomain.Science => "science_discovery",
        LearningDomain.ExecutiveFunction => "autonomy",
        _ => "balanced_growth"
    };

    private static Guid CreateDeterministicGuid(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }
}
