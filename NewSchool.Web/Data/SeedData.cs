using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Domain;
using NewSchool.Web.Services;

namespace NewSchool.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        var familyLibrarySyncService = scope.ServiceProvider.GetRequiredService<FamilyLibrarySyncService>();

        Console.WriteLine("[SeedData] Iniciando EnsureSchemaAsync...");
        await DatabaseInitializer.EnsureSchemaAsync(db);
        await FamilyLibrarySchemaInitializer.EnsureSchemaAsync(db);
        Console.WriteLine("[SeedData] Schema garantido.");

        Console.WriteLine("[SeedData] Validando catalogo curricular...");
        if (await db.CurriculumTemplates.CountAsync() < 192 ||
            await db.CurriculumTemplates.MaxAsync(x => (int?)x.Age) < 10 ||
            await db.CurriculumTemplates.AllAsync(x => x.PrerequisiteSkillCode == "") ||
            await db.CurriculumTemplates.AnyAsync(x => x.SkillCode == null || x.SkillCode == "") ||
            !await db.CurriculumTemplates.AnyAsync(x => x.GoalTrack == "balanced_growth") ||
            !await db.CurriculumTemplates.AnyAsync(x => x.SupportScope != CurriculumSupportScope.General) ||
            !await db.CurriculumTemplates.AnyAsync(x => x.FunctionalTrack != FunctionalSupportTrack.Base) ||
            !await db.CurriculumTemplates.AnyAsync(x => x.FunctionalTrack == FunctionalSupportTrack.Sensory) ||
            !await db.CurriculumTemplates.AnyAsync(x => x.FunctionalTrack == FunctionalSupportTrack.DailyLiving) ||
            !await db.CurriculumTemplates.AnyAsync(x => x.FunctionalTrack == FunctionalSupportTrack.AcademicAdapted))
        {
            db.CurriculumTemplates.RemoveRange(db.CurriculumTemplates);
            await db.SaveChangesAsync();
            db.CurriculumTemplates.AddRange(CurriculumCatalog.Build());
        }

        Console.WriteLine("[SeedData] Validando playbook de intervencao...");
        if (await db.InterventionPlaybookEntries.CountAsync() < 10 ||
            await db.InterventionPlaybookEntries.AnyAsync(x => x.TriggerCode == null || x.TriggerCode == ""))
        {
            db.InterventionPlaybookEntries.RemoveRange(db.InterventionPlaybookEntries);
            await db.SaveChangesAsync();
            db.InterventionPlaybookEntries.AddRange(BuildInterventionPlaybook());
        }

        Console.WriteLine("[SeedData] Validando biblioteca curada...");
        var resourcesNeedReset = await db.CuratedLearningResources.CountAsync() < 4 ||
                                 !await db.CuratedLearningResources.AnyAsync(x => x.IsHostedLocally) ||
                                 await db.CuratedLearningResources.AnyAsync(x => x.Slug == null || x.Slug == "") ||
                                 !await db.CuratedLearningResources.AnyAsync(x => x.Slug == "aprendendo-o-alfabeto-public-domain") ||
                                 !await db.CuratedLearningResources.AnyAsync(x => x.Slug == "i-can-dress-myself") ||
                                 !await db.CuratedLearningResources.AnyAsync(x => x.Slug == "zanele-sees-numbers") ||
                                 !await db.CuratedLearningResources.AnyAsync(x => x.Slug == "why-the-owl-never-sleeps");

        Console.WriteLine("[SeedData] Validando tarefas autorais...");
        var tasksNeedReset = await db.CuratedTaskTemplates.CountAsync() < 72 ||
                             await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == null || x.Slug == "") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.PrimaryResourceId.HasValue) ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-math-zanele-counting") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "language-evidence-paragraph") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-language-rhyme-basket") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "world-brazil-map-and-region") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "language-copywork-verse") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "executive-week-review-and-next-step") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-language-name-trace-and-sound") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "math-fraction-kitchen-halves") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "world-brazil-biome-observation") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-bible-creation-colors") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "math-market-money-play") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "language-proverbs-copy-and-meaning") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-language-story-picture-talk") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "pre-math-shapes-house-hunt") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "literacy-sound-to-word-path") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "language-character-problem-solution") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "language-compare-two-texts") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "world-brazil-waterways-impact") ||
                             !await db.CuratedTaskTemplates.AnyAsync(x => x.Slug == "executive-self-check-before-submit");

        if (resourcesNeedReset || tasksNeedReset)
        {
            db.CuratedTaskTemplates.RemoveRange(db.CuratedTaskTemplates);
            await db.SaveChangesAsync();

            if (resourcesNeedReset)
            {
                db.CuratedLearningResources.RemoveRange(db.CuratedLearningResources);
                await db.SaveChangesAsync();
                db.CuratedLearningResources.AddRange(CuratedLibraryCatalog.BuildResources());
                await db.SaveChangesAsync();
            }

            db.CuratedTaskTemplates.AddRange(CuratedLibraryCatalog.BuildTasks());
        }

        Console.WriteLine("[SeedData] Validando usuarios demo...");
        if (environment.IsDevelopment() && !await db.Users.AnyAsync())
        {
            var admin = new AppUser
            {
                FullName = "Admin NewSchool",
                Email = "admin@newschool.local",
                ReferralCode = "ADMIN-DEMO",
                Role = UserRole.Admin,
                SubscriptionStatus = "active",
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

            var parent = new AppUser
            {
                FullName = "Juliana Demo",
                Email = "parent@newschool.local",
                ReferralCode = "PARENT-DEMO",
                Role = UserRole.Parent,
                SubscriptionStatus = "inactive",
                TrialStartedAt = DateTime.UtcNow.Date,
                TrialEndsAt = DateTime.UtcNow.Date.AddDays(7),
                LastActiveAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            parent.PasswordHash = passwordHasher.HashPassword(parent, "Parent123!");

            var child = new ChildProfile
            {
                Parent = parent,
                FullName = "Mateus Demo",
                BirthDate = DateTime.UtcNow.Date.AddYears(-5).AddMonths(-3),
                DailyStudyMinutes = 45,
                Notes = "Curioso, gosta de dinossauros e desenhar.",
                SupportProfile = SupportProfile.General,
                TeachingMethodology = "eclectic",
                LearningProfile = "hands_on",
                GuidanceStyle = "guided",
                CreatedAt = DateTime.UtcNow
            };

            db.Users.AddRange(admin, parent);
            db.Children.Add(child);
        }

        Console.WriteLine("[SeedData] Validando perfis pedagogicos...");
        var childrenWithoutProfile = await db.Children
            .Where(x => !db.ChildDevelopmentProfiles.Any(profile => profile.ChildId == x.Id))
            .ToListAsync();

        foreach (var child in childrenWithoutProfile)
        {
            db.ChildDevelopmentProfiles.Add(new ChildDevelopmentProfile
            {
                ChildId = child.Id,
                LanguageLevel = 3,
                MathLevel = 3,
                WorldLevel = 3,
                ExecutiveFunctionLevel = 3,
                StrengthsSummary = string.IsNullOrWhiteSpace(child.Notes) ? "Rotina em construcao com a familia." : child.Notes,
                SupportSummary = "Complete o diagnostico inicial para personalizar melhor as metas mensais.",
                AssessedAt = DateTime.UtcNow
            });
        }

        Console.WriteLine("[SeedData] Validando perfis TEA...");
        var childrenWithoutTeaProfile = await db.Children
            .Where(x => !db.ChildTeaProfiles.Any(profile => profile.ChildId == x.Id))
            .ToListAsync();

        foreach (var child in childrenWithoutTeaProfile)
        {
            db.ChildTeaProfiles.Add(new ChildTeaProfile
            {
                ChildId = child.Id,
                CommunicationProfile = child.SupportProfile == SupportProfile.General
                    ? "Descreva como a crianca responde melhor em interacoes, instrucoes e pedidos de ajuda."
                    : "Descreva como a crianca entende pedidos, inicia comunicacao e responde melhor durante atividades.",
                CommunicationNotes = string.Empty,
                AnxietyLevel = child.SupportProfile == SupportProfile.General ? 2 : 3,
                CognitiveRigidityLevel = child.SupportProfile == SupportProfile.General ? 2 : 3,
                SensorySensitivityLevel = child.SupportProfile == SupportProfile.General ? 2 : 3,
                TransitionDifficultyLevel = child.SupportProfile == SupportProfile.General ? 2 : 3,
                SupportIntensityLevel = child.SupportProfile == SupportProfile.General ? 2 : 3,
                NeedsVisualRoutine = child.SupportProfile != SupportProfile.General,
                NeedsFirstThen = child.SupportProfile != SupportProfile.General,
                NeedsTimer = child.GuidanceStyle == "focus_support",
                NeedsPlanB = child.SupportProfile != SupportProfile.General,
                SpecialInterests = string.IsNullOrWhiteSpace(child.Notes) ? string.Empty : child.Notes,
                EffectiveReinforcers = "Escolha aqui o que realmente ajuda esta crianca a aceitar a rotina.",
                CommonTriggers = "Mudanca inesperada, excesso de demanda, transicao sem aviso.",
                OverloadSignals = "Rigidez maior, evitacao, choro, fuga, silencio, aceleracao.",
                CalmingStrategies = "Reduzir bloco, avisar o proximo passo, oferecer pausa curta.",
                TransitionSupports = "Aviso antecipado, contagem regressiva, rotina visual, first-then.",
                DailyLivingPriorities = "Defina prioridades de autonomia e regulacao para esta fase.",
                ParentPrimaryGoal = string.IsNullOrWhiteSpace(child.Notes)
                    ? "Definir uma rotina segura e sustentavel."
                    : "Transformar interesses e observacoes da familia em rotina aplicavel.",
                SchoolBarrierSummary = "Registre aqui o que esta tornando o contexto escolar inviavel ou muito desgastante.",
                DocumentationNotes = "Use este campo para registrar fatos que precisam entrar no dossie pedagogico.",
                UpdatedAt = DateTime.UtcNow
            });
        }

        Console.WriteLine("[SeedData] Persistindo seed...");
        await db.SaveChangesAsync();
        Console.WriteLine("[SeedData] Sincronizando biblioteca da família...");
        await familyLibrarySyncService.SyncAsync();
        db.ChangeTracker.Clear();
        Console.WriteLine("[SeedData] Garantindo coleção autoral NewSchool...");
        await EnsureProprietaryFamilyLibraryAsync(db);
        Console.WriteLine("[SeedData] Seed concluido.");
    }

    private static async Task EnsureProprietaryFamilyLibraryAsync(ApplicationDbContext db)
    {
        var seeds = ProprietaryFamilyLibraryCatalog.Build();
        var seedIds = seeds.Select(item => item.Id).ToHashSet();

        var existingAuthoredMaterials = await db.FamilyLibraryMaterials
            .Where(item => seedIds.Contains(item.Id) || item.SourceSyncToken.StartsWith("AUTHOR:"))
            .ToListAsync();

        var authoredToRemove = existingAuthoredMaterials
            .Where(item => ProprietaryFamilyLibraryCatalog.IsAuthoredMaterial(item) && !seedIds.Contains(item.Id))
            .ToList();

        if (authoredToRemove.Count > 0)
        {
            db.FamilyLibraryMaterials.RemoveRange(authoredToRemove);
            await db.SaveChangesAsync();
            existingAuthoredMaterials = existingAuthoredMaterials
                .Where(item => !authoredToRemove.Any(removed => removed.Id == item.Id))
                .ToList();
        }

        foreach (var seed in seeds)
        {
            var material = existingAuthoredMaterials.FirstOrDefault(item => item.Id == seed.Id);
            if (material is null)
            {
                material = new FamilyLibraryMaterial
                {
                    Id = seed.Id
                };

                db.FamilyLibraryMaterials.Add(material);
                existingAuthoredMaterials.Add(material);
            }

            material.Title = seed.Title;
            material.Category = seed.Category;
            material.EducationStage = seed.EducationStage;
            material.RecommendedMinAge = seed.RecommendedMinAge;
            material.RecommendedMaxAge = seed.RecommendedMaxAge;
            material.SkillFocus = seed.SkillFocus;
            material.Description = seed.Description;
            material.CollectionLabel = seed.CollectionLabel;
            material.IsPrintable = seed.IsPrintable;
            material.PageCount = seed.Pages.Count;
            material.HasIllustrations = seed.HasIllustrations;
            material.CoverImageRelativePath = seed.CoverImageRelativePath;
            material.SourceRelativePath = seed.SourceRelativePath;
            material.SourceSyncToken = seed.SourceSyncToken;
            material.SourceUpdatedAtUtc = seed.SourceUpdatedAtUtc;
            material.SyncedAtUtc = DateTime.UtcNow;

            await db.FamilyLibraryPages
                .Where(page => page.MaterialId == material.Id)
                .ExecuteDeleteAsync();

            foreach (var page in seed.Pages.OrderBy(page => page.PageNumber))
            {
                db.FamilyLibraryPages.Add(new FamilyLibraryPage
                {
                    MaterialId = material.Id,
                    PageNumber = page.PageNumber,
                    TextContent = page.TextContent,
                    ImageRelativePath = page.ImageRelativePath
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static List<InterventionPlaybookEntry> BuildInterventionPlaybook()
    {
        var items = new List<InterventionPlaybookEntry>();

        void Add(
            LearningDomain domain,
            string goalTrack,
            string triggerCode,
            string triggerLabel,
            string keywords,
            string stageScope,
            string headline,
            string howToSpot,
            string likelyCause,
            string whatToSay,
            string whatToAvoid,
            string quickActivity,
            string materials,
            string successSignal,
            string repeatPlan,
            string fallbackAction)
        {
            items.Add(new InterventionPlaybookEntry
            {
                Domain = domain,
                GoalTrack = goalTrack,
                TriggerCode = triggerCode,
                TriggerLabel = triggerLabel,
                MatchKeywords = keywords,
                StageScope = stageScope,
                Headline = headline,
                HowToSpot = howToSpot,
                LikelyCause = likelyCause,
                WhatToSay = whatToSay,
                WhatToAvoid = whatToAvoid,
                QuickActivity = quickActivity,
                Materials = materials,
                SuccessSignal = successSignal,
                RepeatPlan = repeatPlan,
                FallbackAction = fallbackAction
            });
        }

        Add(LearningDomain.Language, "literacy", "phonemic_confusion", "Confunde sons e letras parecidas", "som,letra,rima,fonema,silaba", "starting,guided_practice,developing", "Volte do nome da letra para o som dela", "A crianca olha para a letra, mas responde com hesitacao, troca letras parecidas ou adivinha pela memoria.", "Ela ainda nao consolidou a ponte entre som, gesto oral e simbolo visual.", "Escuta comigo: esta letra faz este som. Vamos achar duas palavras que comecem assim.", "Nao corrija com muitas letras de uma vez, nem peça para decorar o alfabeto inteiro.", "Separar 3 objetos da casa pelo som inicial e repetir em voz alta bem devagar.", "Objetos da casa e dois cartoes de letra.", "Quando ela acerta o som inicial em pelo menos 2 exemplos seguidos sem chute.", "por 3 dias curtos de pratica", "volte para o som do proprio nome e de palavras muito familiares antes de reintroduzir novas letras.");
        Add(LearningDomain.Language, "literacy", "syllable_segmentation_block", "Nao consegue quebrar palavras em silabas", "silaba,palmas,pedacos", "starting,guided_practice,developing", "Transforme a palavra em batidas do corpo", "A crianca fala a palavra inteira, mas nao consegue dividir em partes com palmas ou toques.", "Ela ainda precisa sentir a estrutura sonora com o corpo, nao apenas ouvir a explicacao.", "Vamos bater juntas cada pedacinho da palavra. Eu mostro e voce copia.", "Nao soletrar e nao insistir em explicacoes abstratas sobre silaba.", "Bater palmas ou passos para 3 palavras conhecidas, começando por nomes da familia.", "Mãos, passos no chão ou tampinhas.", "Quando ela acompanha a divisao em 3 palavras sem pular partes.", "em blocos de 5 minutos por 3 a 4 dias", "volte para palavras bem curtas, com duas silabas, antes de aumentar o tamanho.");
        Add(LearningDomain.Language, "literacy", "reading_comprehension_break", "Le a frase mas nao entende o que leu", "leitura,texto,compreensao,entender", "developing,consolidating,ready_to_advance", "Troque pressa de leitura por leitura com parada", "A crianca consegue decodificar, mas nao sabe explicar o que acabou de ler.", "Toda a energia foi para ler as palavras, sobrando pouca atencao para o sentido.", "Vamos ler uma parte so e me contar o que aconteceu com suas palavras.", "Nao fazer interrogatorio longo nem corrigir cada palavra antes de perguntar sentido.", "Ler uma frase ou paragrafo curto e responder apenas uma pergunta de sentido concreto.", "Texto curto e lapis marca-texto.", "Quando ela explica a ideia principal sem repetir mecanicamente o texto.", "em duas releituras curtas no mesmo dia", "reduza o texto e volte para frases mais curtas com imagem de apoio.");
        Add(LearningDomain.Language, "", "writing_transcription_friction", "A escrita trava no traco ou espelha letras", "escrita,traco,espelho,copia,grafia", "starting,guided_practice,developing", "Diminua a carga motora para liberar a linguagem", "A crianca sabe o que quer escrever, mas trava ao registrar, inverte letras ou perde o espacamento.", "A demanda motora e visual ainda esta alta demais para o nivel atual.", "Primeiro vamos formar no ar, depois no dedo, depois no papel. Sem pressa.", "Nao apagar tudo o que ela fez nem exigir pagina cheia para corrigir um traco.", "Traçar letras grandes no ar, no dedo ou em superficie com farinha antes do caderno.", "Farinha, bandeja, dedo ou pincel e caderno.", "Quando o traço sai mais fluido e a letra fica reconhecivel em 2 ou 3 tentativas.", "em micropraticas de 4 a 6 minutos", "reduza o volume escrito e volte para copiar uma unica palavra com modelo claro.");
        Add(LearningDomain.Math, "math_foundations", "quantity_without_meaning", "Conta decorando, mas sem entender quantidade", "conta,contagem,quantidade,numero", "starting,guided_practice,developing", "Traga a matematica de volta para objetos reais", "A crianca recita os numeros, mas aponta errado, pula itens ou nao percebe o total.", "Ela memorizou a sequencia oral sem consolidar correspondencia um a um.", "Cada numero combina com um objeto. Vamos tocar e contar juntos bem devagar.", "Nao corrigir só falando mais alto a sequencia numérica.", "Montar pequenos grupos de 3 a 8 objetos, tocando cada item ao contar.", "Tampinhas, blocos, frutas ou brinquedos pequenos.", "Quando ela toca e conta na mesma velocidade sem sobrar nem faltar item.", "por 3 praticas curtas seguidas", "volte para grupos menores e contraste visual entre poucos e muitos.");
        Add(LearningDomain.Math, "math_foundations", "operation_sequence_break", "Se perde no meio da soma ou subtracao", "soma,subtracao,operacao,problema,juntar,tirar", "guided_practice,developing,consolidating", "Reduza o problema para uma historia visivel", "A crianca ate começa a conta, mas se perde na ordem dos passos ou chuta o resultado.", "Ainda falta clareza de representacao e previsibilidade na sequencia da operacao.", "Vamos montar a historia com pecas: primeiro o que tinha, depois o que entrou ou saiu.", "Nao pular direto para conta armada sem contexto.", "Representar uma unica operacao com material concreto antes de registrar no papel.", "Tampinhas, palitos ou desenhos simples.", "Quando ela explica o que juntou ou tirou antes de dar a resposta.", "em 2 ou 3 exemplos por dia", "volte para problemas de uma etapa com numeros pequenos.");
        Add(LearningDomain.Math, "math_foundations", "multiplication_without_groups", "Multiplica sem entender grupos iguais", "multiplicacao,tabuada,grupos", "developing,consolidating,ready_to_advance", "Construa grupos iguais antes da tabuada", "A crianca tenta decorar resultados, mas nao sabe representar o que a multiplicacao significa.", "Faltou a ideia concreta de grupos iguais e repeticao estruturada.", "Antes da tabuada, vamos montar grupos iguais e descobrir quantos temos no total.", "Nao cobrar memorizacao seca antes de construir sentido.", "Formar grupos iguais com objetos e dizer a frase matematica em voz alta.", "Tampinhas, palitos ou blocos.", "Quando ela cria e explica um grupo igual sem ajuda forte.", "em 3 encontros breves na semana", "retorne a adicao repetida antes de reintroduzir a multiplicacao.");
        Add(LearningDomain.ExecutiveFunction, "autonomy", "short_attention_span", "Perde o foco muito rapido", "foco,atencao,distra", "all", "Corte o bloco e marque uma linha de chegada visivel", "A crianca começa, mas desliga em poucos minutos ou troca de tarefa sem terminar.", "O bloco esta longo demais ou a meta nao esta clara para o nivel atual.", "Vamos fazer so esta parte agora. Quando terminar, voce me mostra e fecha a missao.", "Nao dar tres comandos juntos nem prolongar o bloco esperando que ela volte sozinha.", "Usar cronometro curto e uma unica microtarefa com final visivel.", "Cronometro visual, checklist ou cartao de missao.", "Quando ela sustenta atencao ate o fim de um bloco curto sem fugir da tarefa.", "em blocos de 5 a 8 minutos com pausa curta", "reduza ainda mais a tarefa e volte para comandos de um passo.");
        Add(LearningDomain.ExecutiveFunction, "autonomy", "task_initiation_block", "Sabe o que fazer, mas nao consegue comecar", "comecar,iniciar,sozinho", "all", "Ensine a largada da tarefa, nao so o conteudo", "A crianca entende a proposta, mas enrola, espera demais ou pede ajuda antes de tentar.", "Iniciar sozinho ainda nao virou rotina interna.", "Eu vou fazer o primeiro pedaco com voce e o segundo e seu.", "Nao repetir apenas 'vai la' sem mostrar como começar.", "Criar um ritual de largada com respira, pega material, faz o primeiro item e chama.", "Checklist visual e material já separado.", "Quando ela inicia o primeiro passo em até um minuto com apoio leve.", "todos os dias na primeira atividade", "volte para rotinas de dois passos muito previsiveis antes de aumentar autonomia.");
        Add(LearningDomain.ExecutiveFunction, "", "error_resistance", "Entra em conflito quando erra", "erro,frustra,chora,resiste,nao quer", "all", "Troque correcao por treino seguro", "A crianca trava, reclama ou abandona a tarefa logo depois de um erro.", "O erro ainda esta sendo vivido como ameaça, nao como parte do treino.", "Aqui a gente treina, nao precisa acertar de primeira. Vamos encontrar uma vitoria pequena.", "Nao corrigir em tom de bronca nem pedir para refazer tudo.", "Separar uma versao bem facil da mesma habilidade para recuperar confiança antes de voltar.", "Versao simplificada da atividade e elogio especifico.", "Quando ela aceita tentar de novo sem escalar o conflito.", "na mesma sessao, em no maximo duas novas tentativas", "retome um prerequisito mais facil e encerre com uma pequena vitoria.");
        Add(LearningDomain.World, "science_discovery", "observation_to_language_gap", "Observa bem, mas nao consegue explicar", "observa,comparar,explicar,registrar", "guided_practice,developing,consolidating", "Ajude a transformar observacao em linguagem", "A crianca olha, percebe e até aponta, mas não consegue organizar a explicação.", "Falta moldura verbal para transformar descoberta em frase.", "Eu vou começar a frase e voce termina: eu notei que..., depois..., por isso...", "Nao exigir relatorio longo logo depois da observacao.", "Fazer uma observacao concreta e completar frases-guia em voz alta.", "Objeto real, experimento simples ou figura observada.", "Quando ela consegue dizer uma observacao e uma conclusao curta.", "em duas falas guiadas por atividade", "volte para contraste simples entre dois objetos antes de pedir explicacao completa.");
        Add(LearningDomain.World, "science_discovery", "research_without_focus", "Pesquisa sem conseguir selecionar o essencial", "pesquisa,informacao,tema", "developing,consolidating,ready_to_advance", "Diminua o universo da pesquisa", "A crianca traz muita informação solta ou copia tudo sem filtrar.", "Ainda precisa de uma pergunta central e limite claro de coleta.", "Hoje nossa pesquisa vai responder so uma pergunta e guardar tres descobertas.", "Nao abrir muitas fontes nem aceitar copia sem reorganizar com a propria linguagem.", "Escolher uma pergunta, achar 3 respostas e transformar em mini apresentacao oral.", "Folha com uma pergunta central e espaço para 3 achados.", "Quando ela consegue dizer o que era importante e o que ficou de fora.", "em um unico ciclo curto de pesquisa", "reduza para uma fonte e uma pergunta mais concreta.");

        return items;
    }

    private static List<CurriculumTemplate> BuildCurriculum() => CurriculumCatalog.Build();
}
