using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class TeachingGuideService(
    ApplicationDbContext db,
    LearningPlanService learningPlanService,
    AdaptiveRoutineService adaptiveRoutineService,
    PortuguesePlanningService portuguesePlanningService)
{
    public async Task<TeachingGuideViewModel> BuildAsync(Guid childId, Guid parentId)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.TeaProfile)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var age = CalculateAge(child.BirthDate, today);
        var plan = await learningPlanService.EnsurePlanAsync(child, today);
        var reloadedPlan = await db.DailyPlans
            .Include(x => x.Blocks.OrderBy(b => b.SortOrder))
            .Include(x => x.Sessions.OrderByDescending(s => s.LoggedAt))
            .FirstAsync(x => x.Id == plan.Id);

        var adaptiveSnapshot = await adaptiveRoutineService.BuildSnapshotAsync(child.Id, today);
        var portugueseGuidance = portuguesePlanningService.GetDailyGuidance(age, today);
        var portugueseStage = portuguesePlanningService.GetStageForAge(age);
        var firstName = GetFirstName(child.FullName);
        var languageBlocks = reloadedPlan.Blocks
            .Where(x => x.Domain == LearningDomain.Language)
            .ToList();
        var mathBlocks = reloadedPlan.Blocks
            .Where(x => x.Domain == LearningDomain.Math)
            .ToList();

        var lessons = reloadedPlan.Blocks
            .Select(block => BuildLessonCard(block, age, snapshot: adaptiveSnapshot))
            .ToList();
        var materialAlternatives = BuildMaterialAlternatives(lessons);

        return new TeachingGuideViewModel
        {
            ChildId = child.Id,
            FullName = child.FullName,
            Age = age,
            TodayLabel = $"{today:dd/MM/yyyy}",
            SupportProfileLabel = GetSupportProfileLabel(child.SupportProfile),
            FamilyGoalTrackLabel = GetFamilyGoalTrackLabel(child.FamilyGoalTrack),
            Theme = reloadedPlan.Theme,
            ParentSummary = BuildSimpleOpeningSummary(reloadedPlan, age),
            DailyRecommendation = BuildDailyRecommendation(reloadedPlan, adaptiveSnapshot),
            TomorrowAdjustment = BuildTomorrowAdjustment(reloadedPlan, adaptiveSnapshot),
            EvidenceReminder = BuildEvidenceReminder(portugueseStage),
            CompletedToday = reloadedPlan.Completed || reloadedPlan.Sessions.Count > 0,
            TotalMinutesPlanned = reloadedPlan.Blocks.Sum(x => x.DurationMinutes),
            BeforeStartChecklist = BuildBeforeStartChecklist(reloadedPlan, adaptiveSnapshot),
            MaterialChecklist = BuildMaterialChecklist(lessons),
            MaterialAlternatives = materialAlternatives,
            Lessons = lessons,
            PrintableActivities = [],
            Curriculum = BuildCurriculumSnapshot(portugueseStage, portugueseGuidance),
            QuickTips = BuildQuickTips(child, adaptiveSnapshot)
        };
    }

    private static TeachingCurriculumSnapshotViewModel BuildCurriculumSnapshot(
        PortuguesePlanningStageViewModel? stage,
        PortuguesePlanningGuidance? guidance)
    {
        return new TeachingCurriculumSnapshotViewModel
        {
            StageLabel = stage?.SchoolPlacement ?? "Rotina pedagogica em casa",
            CurrentFocus = guidance is null
                ? "Linguagem oral, leitura compartilhada e registro do que a crianca conseguiu fazer hoje."
                : $"{guidance.TermLabel}: {guidance.MainFocus}",
            AnnualObjective = stage?.AnnualObjective
                ?? "Construir rotina de ensino, consolidar habilidades e registrar evidencias de progresso em casa.",
            EvidenceTargets = stage?.SuggestedEvidence.Take(3).ToList()
                ?? ["Foto da atividade final", "Video curto da crianca explicando", "Registro simples do que funcionou"]
        };
    }

    private static List<string> BuildBeforeStartChecklist(DailyPlan plan, AdaptiveRoutineSnapshot snapshot)
    {
        var firstBlock = plan.Blocks.OrderBy(x => x.SortOrder).FirstOrDefault();
        var checklist = new List<string>
        {
            $"Separe tudo antes de chamar a crianca. Hoje o plano tem {plan.Blocks.Count} bloco(s) e cerca de {plan.Blocks.Sum(x => x.DurationMinutes)} minutos.",
            $"Use bloco curto de {snapshot.WorkBlockMinutes} min e pausa de {snapshot.BreakMinutes} min. Pare antes do desgaste.",
            "Mostre o que vai acontecer com uma frase curta ou um papel simples: agora atividade, depois pausa.",
            $"Comece por {firstBlock?.Title ?? "uma atividade simples"} para garantir uma primeira vitoria."
        };

        checklist.Add("Se travar, nao force. Pare 2 minutos, reduza a tarefa pela metade e volte com uma escolha simples.");

        return checklist;
    }

    private static List<string> BuildMaterialChecklist(IEnumerable<TeachingLessonCardViewModel> lessons)
    {
        return lessons
            .SelectMany(lesson => lesson.MaterialItems.Select(item => $"{item} ({lesson.SubjectLabel})"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
    }

    private static List<TeachingQuickTipViewModel> BuildMaterialAlternatives(IEnumerable<TeachingLessonCardViewModel> lessons)
    {
        var subjects = lessons
            .Select(x => x.SubjectLabel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ideas = new List<TeachingQuickTipViewModel>
        {
            new()
            {
                Title = "Se faltar material de Portugues",
                Body = "Use o que existe em casa: objetos reais, embalagens, figuras de revista, fotos do celular, nomes da familia e desenhos feitos na hora."
            },
            new()
            {
                Title = "Se faltar material de Matematica",
                Body = "Use tampinhas, feijoes, pregadores, colheres, brinquedos pequenos ou dedos. Para comparar, monte grupos na mesa ou no chao."
            },
            new()
            {
                Title = "Se faltar apoio visual",
                Body = "Pegue um papel e desenhe 2 quadrados: agora e depois. Tambem vale post-it, caderno ou quadro simples."
            },
            new()
            {
                Title = "Se a crianca cansar do mesmo formato",
                Body = "Troque mesa por chao, folha por quadro, objeto por desenho, resposta falada por apontar ou resposta desenhada."
            }
        };

        if (subjects.Contains("Funcao executiva"))
        {
            ideas.Add(new TeachingQuickTipViewModel
            {
                Title = "Para treinar combinados e transicao",
                Body = "Use timer do celular, contagem regressiva com os dedos e um combinado simples: primeiro fazemos isto, depois vem a pausa."
            });
        }

        if (subjects.Contains("Mundo e ciencias"))
        {
            ideas.Add(new TeachingQuickTipViewModel
            {
                Title = "Para ciencias sem comprar nada",
                Body = "Use agua, folhas, frutas, luz da janela, sombras, roupas da casa, plantas, sementes e objetos quentes ou frios para observar."
            });
        }

        return ideas.Take(5).ToList();
    }

    private static TeachingLessonCardViewModel BuildLessonCard(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        var guide = BuildLaypersonGuide(block, age, snapshot);

        return new TeachingLessonCardViewModel
        {
            SubjectLabel = GetSubjectLabel(block.Domain),
            BlockTitle = block.Title,
            SupportSourceLabel = GetSupportScopeLabel(block.SupportScope),
            SupportSourceChipClass = GetSupportScopeChip(block.SupportScope),
            FunctionalTrackLabel = GetFunctionalTrackLabel(block.FunctionalTrack),
            FunctionalTrackChipClass = GetFunctionalTrackChip(block.FunctionalTrack),
            FocusLabel = block.IsRecoveryFocus
                ? "Reforco"
                : block.IsSpacedReview
                    ? "Revisao"
                    : "Avanco",
            FocusChipClass = block.IsRecoveryFocus
                ? "warning"
                : block.IsSpacedReview
                    ? "neutral"
                    : "success",
            DurationMinutes = block.DurationMinutes,
            Goal = guide.Goal,
            AdultInstruction = guide.AdultInstruction,
            ExampleScript = guide.ExampleScript,
            ChildInstruction = guide.ChildInstruction,
            IfStuck = guide.IfStuck,
            SuccessSignal = guide.SuccessSignal,
            EvidenceToSave = guide.EvidenceToSave,
            Materials = string.Join(", ", guide.MaterialItems),
            MaterialItems = guide.MaterialItems,
            StepByStep = guide.StepByStep,
            IsPrintableFocus = block.Domain == LearningDomain.Language
        };
    }

    private static LaypersonGuide BuildLaypersonGuide(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        return block.Domain switch
        {
            LearningDomain.Language => BuildLanguageGuide(block, age, snapshot),
            LearningDomain.Math => BuildMathGuide(block, age, snapshot),
            LearningDomain.ExecutiveFunction => BuildExecutiveGuide(block, age, snapshot),
            _ => BuildWorldGuide(block, age, snapshot)
        };
    }

    private static LaypersonGuide BuildLanguageGuide(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        var key = NormalizeForSearch($"{block.Title} {block.SkillName} {block.Goal}");

        if (age <= 5 && (key.Contains("fonolog") || key.Contains("som") || key.Contains("rima")))
        {
            return new LaypersonGuide
            {
                Goal = "Brincar com sons iguais no comeco das palavras.",
                AdultInstruction = "Voce vai usar objetos ou figuras da casa para a crianca escutar o som inicial das palavras e comparar quais comecam igual.",
                ExampleScript = "Eu vou dizer duas palavras: bola e boneca. Elas comecam igual. Agora escuta: mesa e macaco. Comecam igual ou diferente?",
                ChildInstruction = "Escutar, repetir as palavras e apontar quais comecam com o mesmo som.",
                MaterialItems =
                [
                    "2 ou 3 objetos pequenos da casa ou figuras simples",
                    "1 folha em branco",
                    "lapis de cor"
                ],
                StepByStep =
                [
                    "Escolha 2 palavras bem conhecidas da crianca.",
                    "Fale devagar, uma por vez, e destaque o comeco da palavra.",
                    "Peca para a crianca repetir e apontar os dois itens que comecam igual.",
                    "No fim, desenhe ou cole os itens que ela conseguiu perceber."
                ],
                IfStuck = "Diminua para apenas 2 palavras, use objetos reais e faca voce primeiro antes de pedir que ela responda.",
                SuccessSignal = "Deu certo quando ela percebe que duas palavras comecam igual, mesmo com sua ajuda.",
                EvidenceToSave = "Foto dos objetos ou do desenho final, ou video curto dela repetindo as palavras."
            };
        }

        if (age <= 5 && (key.Contains("comunic") || key.Contains("troca") || key.Contains("brincadeira guiada")))
        {
            return new LaypersonGuide
            {
                Goal = "Treinar conversa curta, espera da vez e resposta simples.",
                AdultInstruction = "Voce vai brincar junto, fazer uma pergunta curta e esperar uma resposta simples da crianca, com fala, gesto ou apontar.",
                ExampleScript = "Agora e minha vez de perguntar: qual voce quer, a bola ou o livro? Muito bem. Agora e sua vez de me mostrar.",
                ChildInstruction = "Escolher, responder com uma palavra, gesto ou apontando, e esperar a vez de novo.",
                MaterialItems =
                [
                    "2 brinquedos ou 2 figuras que a crianca goste",
                    "1 cadeira ou tapete para sentar junto",
                    "timer do celular se a espera estiver dificil"
                ],
                StepByStep =
                [
                    "Mostre 2 opcoes bem claras para a crianca.",
                    "Faca uma pergunta curta: qual voce quer, qual voce escolhe ou qual voce gostou.",
                    "Espere alguns segundos pela resposta sem repetir muitas vezes.",
                    "Responda a escolha dela e troque a vez mais uma vez."
                ],
                IfStuck = "Reduza para uma escolha por vez e aceite apontar ou olhar como resposta valida.",
                SuccessSignal = "Deu certo quando ela entra em pelo menos duas trocas curtas com voce sem se desorganizar.",
                EvidenceToSave = "Video curto da brincadeira ou foto dos dois itens usados."
            };
        }

        if (key.Contains("reconto") || key.Contains("historia"))
        {
            return new LaypersonGuide
            {
                Goal = age <= 5
                    ? "Contar de novo uma historia curta com apoio de imagem."
                    : "Ler ou ouvir um texto curto e contar com as proprias palavras o que aconteceu.",
                AdultInstruction = "Leia a historia primeiro. Depois volte nas imagens ou nas partes principais e puxe a crianca para contar de novo o que viu.",
                ExampleScript = "Primeiro apareceu quem? E depois, o que aconteceu? Me conta com suas palavras.",
                ChildInstruction = "Olhar as imagens, lembrar da ordem e contar o que aconteceu.",
                MaterialItems =
                [
                    "1 livro curto ou sequencia de 3 imagens",
                    "1 folha em branco",
                    "lapis de cor"
                ],
                StepByStep =
                [
                    "Leia a historia inteira uma vez sem cobrar resposta.",
                    "Na segunda vez, pare em 2 ou 3 momentos importantes.",
                    "Pergunte o que aconteceu primeiro, depois e no final.",
                    "Feche com um desenho da parte mais importante."
                ],
                IfStuck = "Mostre a imagem e ofereca 2 opcoes simples: foi o cachorro ou a menina? aconteceu antes ou depois?",
                SuccessSignal = "Deu certo quando a crianca consegue lembrar pelo menos uma parte importante da historia.",
                EvidenceToSave = "Audio do reconto, foto do desenho ou anotacao curta do que ela conseguiu contar."
            };
        }

        if (age <= 5)
        {
            return new LaypersonGuide
            {
                Goal = "Ampliar fala, escuta e uso de palavras do dia a dia.",
                AdultInstruction = "Voce vai nomear, repetir e conversar sobre objetos e acoes conhecidas da crianca em blocos bem curtos.",
                ExampleScript = "Isto e uma bola. Agora fala comigo: bola. O que da para fazer com a bola?",
                ChildInstruction = "Ouvir, repetir a palavra e responder com gesto, palavra ou frase curta.",
                MaterialItems =
                [
                    "3 objetos conhecidos da casa",
                    "1 folha",
                    "lapis de cor"
                ],
                StepByStep =
                [
                    "Escolha 3 objetos que a crianca goste ou use no dia a dia.",
                    "Nomeie um objeto por vez e peca para ela repetir.",
                    "Faca uma pergunta simples sobre cada objeto.",
                    "No fim, desenhem juntos o objeto preferido."
                ],
                IfStuck = "Use menos objetos, aceite gesto ou apontar e transforme a resposta em fala para ela ouvir.",
                SuccessSignal = "Deu certo quando ela participa, mesmo que com apoio, e usa pelo menos uma palavra ou gesto com sentido.",
                EvidenceToSave = "Foto do desenho final ou video curto da nomeacao."
            };
        }

        return new LaypersonGuide
        {
            Goal = "Ler, conversar e registrar uma ideia curta de forma clara.",
            AdultInstruction = "Voce vai ler junto, explicar uma parte de cada vez e pedir uma resposta curta antes de seguir.",
            ExampleScript = "Vamos ler esta parte juntos. Agora me diz: o que aconteceu aqui?",
            ChildInstruction = "Ler ou ouvir, responder uma pergunta curta e registrar uma frase, palavra ou desenho.",
            MaterialItems =
            [
                "texto curto ou livro",
                "1 folha",
                "lapis"
            ],
            StepByStep =
            [
                "Leia uma parte curta do texto.",
                "Faca uma pergunta simples sobre o que acabou de ler.",
                "Registre a resposta com palavra, frase ou desenho.",
                "Repita so mais uma vez e encerre."
            ],
            IfStuck = "Leia voce primeiro, reduza a quantidade de texto e transforme a resposta longa em uma pergunta de escolha simples.",
            SuccessSignal = "Deu certo quando a crianca entende a parte principal e consegue responder algo com sentido.",
            EvidenceToSave = "Foto da resposta no papel ou audio da resposta oral."
        };
    }

    private static LaypersonGuide BuildMathGuide(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        var key = NormalizeForSearch($"{block.Title} {block.SkillName} {block.Goal}");

        if (age <= 5 && (key.Contains("cont") || key.Contains("numero") || key.Contains("quantidade")))
        {
            return new LaypersonGuide
            {
                Goal = "Contar objetos reais e ligar numero com quantidade.",
                AdultInstruction = "Voce vai usar objetos pequenos, contar junto apontando um por um e so depois passar para o papel.",
                ExampleScript = "Vamos contar juntos. Um, dois, tres. Agora pega tres tampinhas para mim.",
                ChildInstruction = "Apontar, contar junto e separar a quantidade pedida.",
                MaterialItems =
                [
                    "10 tampinhas, blocos ou brinquedos pequenos",
                    "1 folha",
                    "giz de cera ou lapis de cor"
                ],
                StepByStep =
                [
                    "Coloque poucos objetos na mesa, de 1 a 5.",
                    "Conte apontando um por um com a crianca.",
                    "Peca: me da 3, me mostra 4, coloca 2 aqui.",
                    "No fim, desenhem a mesma quantidade na folha."
                ],
                IfStuck = "Volte para quantidades menores e ajude a apontar um por um sem pressa.",
                SuccessSignal = "Deu certo quando ela consegue separar a quantidade pedida com sua ajuda.",
                EvidenceToSave = "Foto dos grupos montados ou da folha com os desenhos."
            };
        }

        if (age <= 5 && (key.Contains("compar") || key.Contains("mais") || key.Contains("menos")))
        {
            return new LaypersonGuide
            {
                Goal = "Perceber onde tem mais, menos ou igual.",
                AdultInstruction = "Monte dois grupos pequenos de objetos e compare junto com a crianca.",
                ExampleScript = "Aqui tem mais ou aqui tem menos? Vamos colocar lado a lado para conferir.",
                ChildInstruction = "Olhar os grupos e dizer ou apontar onde tem mais, menos ou igual.",
                MaterialItems =
                [
                    "8 tampinhas ou blocos",
                    "2 pratinhos ou 2 folhas para separar grupos"
                ],
                StepByStep =
                [
                    "Monte dois grupos pequenos na mesa.",
                    "Pergunte onde tem mais e onde tem menos.",
                    "Aproxime os grupos para comparar um a um.",
                    "Troque a quantidade e repita."
                ],
                IfStuck = "Use grupos bem diferentes no comeco, como 2 e 5, antes de comparar quantidades parecidas.",
                SuccessSignal = "Deu certo quando ela percebe diferenca entre os grupos e responde apontando ou falando.",
                EvidenceToSave = "Foto dos grupos comparados."
            };
        }

        if (key.Contains("dezena") || key.Contains("unidade") || key.Contains("valor posicional"))
        {
            return new LaypersonGuide
            {
                Goal = "Entender que um numero grande e feito de grupos menores.",
                AdultInstruction = "Voce vai montar dezenas e unidades com objetos ou risquinhos antes de escrever o numero.",
                ExampleScript = "Aqui temos um grupo de 10 e mais 4 soltos. Isso vira 14.",
                ChildInstruction = "Separar grupos de 10 e os que sobraram, e depois dizer ou escrever o numero.",
                MaterialItems =
                [
                    "palitos, tampinhas ou bloquinhos",
                    "1 folha",
                    "lapis"
                ],
                StepByStep =
                [
                    "Monte um grupo de 10 objetos.",
                    "Acrescente alguns objetos soltos.",
                    "Mostre o numero completo e diga de onde ele veio.",
                    "Peca para a crianca montar outro numero parecido."
                ],
                IfStuck = "Comece com um unico grupo de 10 e poucos soltos, sem passar de 20.",
                SuccessSignal = "Deu certo quando ela enxerga o grupo de 10 e os que sobraram.",
                EvidenceToSave = "Foto da montagem ou da folha com a decomposicao."
            };
        }

        if (key.Contains("som") || key.Contains("adicao") || key.Contains("juntar") || key.Contains("problema"))
        {
            return new LaypersonGuide
            {
                Goal = "Juntar quantidades pequenas e descobrir quanto ficou ao todo.",
                AdultInstruction = "Voce vai representar o problema com objetos primeiro e so depois mostrar a conta.",
                ExampleScript = "Aqui tinham 3 tampinhas. Chegaram mais 2. Vamos juntar e ver quantas ficaram ao todo.",
                ChildInstruction = "Montar a situacao com objetos, juntar e contar o total.",
                MaterialItems =
                [
                    "10 tampinhas, blocos ou botões",
                    "1 folha",
                    "lapis"
                ],
                StepByStep =
                [
                    "Monte a primeira quantidade na mesa.",
                    "Acrescente a segunda quantidade devagar.",
                    "Conte tudo junto com a crianca.",
                    "Mostre a conta so depois da contagem concreta."
                ],
                IfStuck = "Diminua os numeros e deixe a crianca mexer nos objetos com as maos.",
                SuccessSignal = "Deu certo quando ela entende que juntar aumenta a quantidade e consegue contar o total.",
                EvidenceToSave = "Foto da conta concreta ou da conta registrada na folha."
            };
        }

        if (key.Contains("subtr") || key.Contains("tirar"))
        {
            return new LaypersonGuide
            {
                Goal = "Perceber o que acontece quando tiramos uma parte.",
                AdultInstruction = "Voce vai montar uma quantidade, tirar uma parte na frente da crianca e contar quanto sobrou.",
                ExampleScript = "Aqui temos 7. Se eu tirar 2, quantos ficam na mesa?",
                ChildInstruction = "Observar o que foi tirado e contar quanto sobrou.",
                MaterialItems =
                [
                    "10 tampinhas ou blocos",
                    "1 folha",
                    "lapis"
                ],
                StepByStep =
                [
                    "Monte a quantidade inicial na mesa.",
                    "Tire alguns objetos devagar, na frente da crianca.",
                    "Conte o que sobrou junto com ela.",
                    "Repita com um numero parecido."
                ],
                IfStuck = "Use numeros menores e deixe a crianca tirar os objetos com a propria mao.",
                SuccessSignal = "Deu certo quando ela percebe que tirar diminui a quantidade e consegue contar o que sobrou.",
                EvidenceToSave = "Foto da atividade concreta ou da resposta na folha."
            };
        }

        return new LaypersonGuide
        {
            Goal = "Resolver uma ideia matematica usando objetos antes do papel.",
            AdultInstruction = "Voce vai mostrar a situacao com objetos, falar em voz alta e so depois passar para o registro.",
            ExampleScript = "Primeiro a gente monta com objetos. Depois a gente descobre o numero e registra.",
            ChildInstruction = "Mexer nos objetos, contar, comparar ou juntar e depois registrar.",
            MaterialItems =
            [
                "objetos pequenos para contar",
                "1 folha",
                "lapis"
            ],
            StepByStep =
            [
                "Monte a situacao com objetos reais.",
                "Fale em voz alta o que esta acontecendo.",
                "Peca para a crianca agir com os objetos.",
                "Registre so o final da ideia na folha."
            ],
            IfStuck = "Volte para menos objetos e uma unica acao por vez.",
            SuccessSignal = "Deu certo quando ela entende a ideia concreta, mesmo com sua ajuda.",
            EvidenceToSave = "Foto da atividade concreta ou da folha final."
        };
    }

    private static LaypersonGuide BuildExecutiveGuide(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        return new LaypersonGuide
        {
            Goal = "Treinar seguir um combinado curto e mudar de uma atividade para outra sem susto.",
            AdultInstruction = "Voce vai avisar o que vem agora e o que vem depois, usar um tempo curto e fechar antes do desgaste.",
            ExampleScript = "Agora vamos fazer isto por pouco tempo. Depois vem a pausa. Quando o timer tocar, a gente troca junto.",
            ChildInstruction = "Comecar a tarefa, fazer por pouco tempo e aceitar a troca com ajuda.",
            MaterialItems =
            [
                "timer do celular",
                "cartao simples de agora e depois",
                "1 atividade curta ja separada"
            ],
            StepByStep =
            [
                "Mostre para a crianca o que sera feito agora e o que vem depois.",
                "Ajuste o timer para um tempo curto.",
                "Comece a atividade e lembre no meio que ela vai acabar logo.",
                "Quando terminar, feche a tarefa e entregue a pausa prometida."
            ],
            IfStuck = "Reduza o tempo, simplifique a tarefa e mantenha a pausa combinada como previsibilidade.",
            SuccessSignal = "Deu certo quando a crianca entra na tarefa e consegue sair dela com menos atrito.",
            EvidenceToSave = "Foto da rotina visual ou anotacao curta dizendo como foi a troca."
        };
    }

    private static LaypersonGuide BuildWorldGuide(DailyPlanBlock block, int age, AdaptiveRoutineSnapshot snapshot)
    {
        return new LaypersonGuide
        {
            Goal = "Observar uma coisa do mundo real e conversar sobre o que viu.",
            AdultInstruction = "Voce vai mostrar um objeto, cena, imagem ou experiencia simples e puxar uma conversa curta e concreta.",
            ExampleScript = "Olha bem. O que voce esta vendo? Qual parte chama mais atencao? O que mudou?",
            ChildInstruction = "Observar, apontar, comparar e responder com fala, gesto ou desenho.",
            MaterialItems =
            [
                "1 objeto real, imagem ou elemento da natureza",
                "1 folha",
                "lapis de cor"
            ],
            StepByStep =
            [
                "Mostre uma coisa real para observar.",
                "Faca 2 ou 3 perguntas bem simples.",
                "Peça para a crianca mostrar, apontar ou desenhar o que percebeu.",
                "Feche registrando uma descoberta do dia."
            ],
            IfStuck = "Diminua a quantidade de perguntas e aceite apontar ou desenhar como resposta.",
            SuccessSignal = "Deu certo quando a crianca observa algo real e consegue responder de algum jeito com sentido.",
            EvidenceToSave = "Foto do objeto observado, desenho final ou audio da resposta."
        };
    }

    private static List<TeachingPrintableActivityViewModel> BuildPrintableLibrary(
        int age,
        string firstName,
        string schoolStageLabel,
        PortuguesePlanningGuidance? guidance,
        IReadOnlyList<string> languageBlockTitles,
        IReadOnlyList<string> mathBlockTitles)
    {
        var focus = guidance?.MainFocus ?? "leitura, oralidade e escrita";
        var blockTitle = languageBlockTitles.FirstOrDefault() ?? "atividade de linguagem";
        var mathBlockTitle = mathBlockTitles.FirstOrDefault() ?? "atividade de matematica";
        var activities = new List<TeachingPrintableActivityViewModel>();

        activities.AddRange(age switch
        {
            <= 5 => BuildEarlyLanguagePrintablePack(firstName, schoolStageLabel, focus, blockTitle),
            6 => BuildAgeSixPrintablePack(firstName, schoolStageLabel, focus, blockTitle),
            7 => BuildAgeSevenPrintablePack(firstName, schoolStageLabel, focus, blockTitle),
            _ => BuildAgeEightPrintablePack(firstName, schoolStageLabel, focus, blockTitle)
        });

        activities.AddRange(age switch
        {
            <= 5 => BuildEarlyMathPrintablePack(firstName, schoolStageLabel, mathBlockTitle),
            6 => BuildAgeSixMathPrintablePack(firstName, schoolStageLabel, mathBlockTitle),
            7 => BuildAgeSevenMathPrintablePack(firstName, schoolStageLabel, mathBlockTitle),
            _ => BuildAgeEightMathPrintablePack(firstName, schoolStageLabel, mathBlockTitle)
        });

        return activities;
    }

    private static List<TeachingPrintableActivityViewModel> BuildEarlyLanguagePrintablePack(
        string firstName,
        string schoolStageLabel,
        string focus,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Historia + desenho + reconto",
                Subtitle = $"Foco do periodo: {focus}",
                Instructions = $"Leia uma historia curta e depois puxe o bloco \"{blockTitle}\". A crianca desenha a parte principal e conta oralmente com sua ajuda.",
                Prompt = "Desenhe a parte mais importante da historia. Depois conte oralmente quem apareceu e o que aconteceu.",
                SupportTip = "Se travar, volte nas imagens e aceite apontar, nomear ou falar so uma palavra de cada vez.",
                Questions =
                [
                    "Leia a historia em voz alta uma vez inteira.",
                    "Volte nas figuras e pergunte: quem apareceu e o que aconteceu primeiro.",
                    "Peca para a crianca desenhar a parte favorita e contar do jeito dela."
                ],
                WritingLines = 0,
                ShowDrawingBox = true
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Som do dia",
                Subtitle = "Brincadeira de fala e desenho",
                Instructions = "Escolha um som facil, como B ou M. Fale exemplos em voz alta e peca para a crianca desenhar ou apontar coisas que comecem igual.",
                Prompt = "Escolha um som do dia e desenhe 2 ou 3 coisas que comecem com esse som.",
                SupportTip = "Nao cobre letra certa nem escrita. O foco e escutar e brincar com o comeco das palavras.",
                Questions =
                [
                    "Escolha um som simples, como B.",
                    "Diga duas palavras com esse som, por exemplo bola e boneca, e peca para a crianca repetir.",
                    "Convide a desenhar uma coisa que comeca com esse som."
                ],
                WritingLines = 0,
                ShowDrawingBox = true
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeSixPrintablePack(
        string firstName,
        string schoolStageLabel,
        string focus,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Palavras e frase curta",
                Subtitle = $"Foco do periodo: {focus}",
                Instructions = $"Use como extensao do bloco \"{blockTitle}\". Leia as palavras junto com a crianca e depois monte uma frase curta.",
                Prompt = "Circule as palavras que voce conseguiu ler e escreva uma frase com duas delas.",
                SupportTip = "Se a leitura travar, leia voce primeiro e peca para a crianca apontar, repetir e montar a frase oralmente antes de escrever.",
                WordBank =
                [
                    "casa",
                    "bola",
                    "mesa",
                    "lupa",
                    "pato",
                    "fita"
                ],
                WritingLines = 5
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Reconto curto",
                Subtitle = "Leitura e compreensao literal",
                Instructions = "Leia o texto curto em voz alta duas vezes. Na segunda leitura, pare e confirme se a crianca entendeu o que aconteceu.",
                SampleText = "Lia viu um gato no quintal. O gato correu atras da bola. Depois de brincar, ele dormiu na sombra.",
                Prompt = "Responda oralmente ou por escrito: quem apareceu, o que aconteceu primeiro e como a historia terminou.",
                SupportTip = "Se travar, transforme cada pergunta em apontamento: \"mostra onde aparece o gato\", \"o que ele fez depois\".",
                Questions =
                [
                    "Quem apareceu no texto?",
                    "O que aconteceu primeiro?",
                    "Como a historia terminou?"
                ],
                WritingLines = 4
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeSevenPrintablePack(
        string firstName,
        string schoolStageLabel,
        string focus,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Leia o bilhete",
                Subtitle = $"Foco do periodo: {focus}",
                Instructions = $"Use depois do bloco \"{blockTitle}\". Leia o bilhete uma vez com a crianca e depois deixe que ela encontre as respostas no texto.",
                SampleText = "Bilhete: " + firstName + ", hoje vamos ler uma historia e depois guardar os materiais na caixa azul. Nao esqueca de levar o caderno. Mamae.",
                Prompt = "Responda: quem escreveu, o que precisa ser feito depois da historia e onde os materiais vao ficar.",
                SupportTip = "Se travar, sublinhe no texto a palavra-chave de cada pergunta antes de responder.",
                Questions =
                [
                    "Quem escreveu o bilhete?",
                    "O que precisa ser feito depois da historia?",
                    "Onde os materiais vao ficar?"
                ],
                WritingLines = 4
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Escreva um bilhete curto",
                Subtitle = "Texto funcional",
                Instructions = "Convide a crianca a escrever um bilhete real para alguem da casa.",
                Prompt = "Escreva um bilhete curto contando o que voce estudou hoje e o que precisa fazer depois.",
                SupportTip = "Se precisar, dite a estrutura: para quem e o bilhete, o recado e a assinatura.",
                WordBank =
                [
                    "hoje",
                    "depois",
                    "guardar",
                    "ler",
                    "caderno"
                ],
                WritingLines = 7
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeEightPrintablePack(
        string firstName,
        string schoolStageLabel,
        string focus,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Leia e encontre a ideia principal",
                Subtitle = $"Foco do periodo: {focus}",
                Instructions = $"Aplique depois do bloco \"{blockTitle}\". Leia o texto uma vez com apoio e outra vez pedindo para a crianca localizar a ideia central.",
                SampleText = "No quintal de casa, " + firstName + " plantou duas sementes em vasos diferentes. Um vaso ficou perto da janela e recebeu sol. O outro ficou no canto sem luz. Depois de alguns dias, a planta que recebeu sol cresceu mais rapido.",
                Prompt = "Escreva a ideia principal do texto e dois detalhes que ajudam a provar essa ideia.",
                SupportTip = "Se travar, pergunte: \"sobre o que esse texto quer ensinar?\" e \"quais frases ajudam a mostrar isso?\".",
                Questions =
                [
                    "Qual e a ideia principal do texto?",
                    "Quais sao dois detalhes importantes?",
                    "O que podemos concluir sobre a planta?"
                ],
                WritingLines = 6
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Lingua Portuguesa",
                StageLabel = schoolStageLabel,
                Title = "Paragrafo curto com organizacao",
                Subtitle = "Producao escrita",
                Instructions = "Planeje oralmente antes de escrever. Peca uma frase de inicio, uma de desenvolvimento e uma de fechamento.",
                Prompt = "Escreva um paragrafo curto contando algo que voce aprendeu hoje e por que isso foi importante.",
                SupportTip = "Se travar, deixe a crianca falar primeiro e transforme a fala em tres frases-base.",
                WordBank =
                [
                    "primeiro",
                    "depois",
                    "porque",
                    "tambem",
                    "aprendi"
                ],
                WritingLines = 8
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildEarlyMathPrintablePack(
        string firstName,
        string schoolStageLabel,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Conte e desenhe",
                Subtitle = "Quantidade e contagem inicial",
                Instructions = $"Use depois do bloco \"{blockTitle}\". A crianca conta objetos reais primeiro e depois registra no papel.",
                Prompt = "Desenhe 1 bolinha, depois 2, depois 3, depois 4. Conte em voz alta junto com o adulto.",
                SupportTip = "Se travar, conte junto apontando um por um. O foco e quantidade, nao numero escrito.",
                Questions =
                [
                    "Coloque objetos reais na mesa antes da folha.",
                    "Conte junto apontando um por um: 1, 2, 3 e 4.",
                    "Depois passe para o desenho na folha."
                ],
                WritingLines = 0,
                ShowDrawingBox = true
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Mais, menos e igual",
                Subtitle = "Comparacao de quantidades",
                Instructions = "Monte dois grupos com tampinhas ou brinquedos antes de marcar no papel.",
                Prompt = "Olhe os dois grupos e marque onde tem mais, menos ou a mesma quantidade.",
                SupportTip = "Use objetos concretos antes do papel. Depois peça para a crianca explicar com as palavras dela.",
                Questions =
                [
                    "Monte 2 grupos pequenos com objetos da casa.",
                    "Pergunte onde tem mais, onde tem menos ou se esta igual.",
                    "Se quiser, desenhe os grupos no espaco grande abaixo."
                ],
                WritingLines = 0,
                ShowDrawingBox = true
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeSixMathPrintablePack(
        string firstName,
        string schoolStageLabel,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Numero, quantidade e vizinhos",
                Subtitle = "1o ano: sequencia numerica",
                Instructions = $"Use como extensao do bloco \"{blockTitle}\". Mostre a linha numerica ate 20 e depois passe para a folha.",
                Prompt = "Complete o numero que vem antes e o que vem depois.",
                SupportTip = "Se travar, volte para a linha numerica oral. Fale e aponte junto antes de escrever.",
                WordBank = ["7", "8", "9", "10", "11", "12", "13", "14"],
                WritingLines = 5
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Junte e descubra",
                Subtitle = "Adicao inicial com apoio concreto",
                Instructions = "Use tampinhas, blocos ou dedos antes da conta no papel.",
                Prompt = "Resolva as somas desenhando ou usando objetos: 3 + 2, 4 + 1, 5 + 3.",
                SupportTip = "Se travar, represente cada parcela com objetos e junte devagar. Depois mostre o numero total.",
                Questions =
                [
                    "Quantos tinham no começo?",
                    "Quantos chegaram depois?",
                    "Quantos ficaram ao todo?"
                ],
                WritingLines = 4
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeSevenMathPrintablePack(
        string firstName,
        string schoolStageLabel,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Problemas do cotidiano",
                Subtitle = "2o ano: adicao e subtracao em contexto",
                Instructions = $"Use depois do bloco \"{blockTitle}\". Leia o enunciado em voz alta e destaque o que a pergunta quer saber.",
                Prompt = "Resolva: Ana tinha 8 lapis e ganhou mais 4. Quantos lapis ela tem agora? Depois: Pedro tinha 12 figurinhas e deu 3. Com quantas ficou?",
                SupportTip = "Se travar, desenhe a situacao ou represente com objetos antes de ir para os numeros.",
                Questions =
                [
                    "O que o problema esta contando?",
                    "Entrou mais ou tirou?",
                    "Qual conta ajuda a descobrir?"
                ],
                WritingLines = 5
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Dezena e unidade",
                Subtitle = "Valor posicional inicial",
                Instructions = "Monte os numeros com palitos ou blocos antes de registrar no papel.",
                Prompt = "Separe em dezenas e unidades: 14, 18, 23 e 27.",
                SupportTip = "Se travar, forme grupos de 10 primeiro. A crianca precisa ver a dezena fisicamente antes de abstrair.",
                WordBank = ["14", "18", "23", "27"],
                WritingLines = 4
            }
        ];
    }

    private static List<TeachingPrintableActivityViewModel> BuildAgeEightMathPrintablePack(
        string firstName,
        string schoolStageLabel,
        string blockTitle)
    {
        return
        [
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Valor posicional e composicao",
                Subtitle = "3o ano: centenas, dezenas e unidades",
                Instructions = $"Use como fechamento do bloco \"{blockTitle}\". Peca para a crianca decompor o numero antes de registrar.",
                Prompt = "Decomponha: 134, 208 e 356. Depois escreva cada numero por extenso do seu jeito.",
                SupportTip = "Se travar, use quadro de valor posicional ou material dourado improvisado com grupos desenhados.",
                WordBank = ["134", "208", "356"],
                WritingLines = 6
            },
            new TeachingPrintableActivityViewModel
            {
                SubjectLabel = "Matematica",
                StageLabel = schoolStageLabel,
                Title = "Problema com estrategia",
                Subtitle = "Leitura matematica e justificativa",
                Instructions = "Leia o problema, sublinhe os dados importantes e peça para a crianca explicar qual operacao faz sentido antes de resolver.",
                Prompt = "Numa caixa havia 24 livros. Chegaram mais 13. Depois, 8 foram guardados em outra prateleira. Quantos livros ficaram na caixa?",
                SupportTip = "Se travar, resolva em duas etapas e registre cada etapa separadamente.",
                Questions =
                [
                    "Qual e a primeira conta?",
                    "O que acontece depois?",
                    "Qual resposta final faz sentido?"
                ],
                WritingLines = 6
            }
        ];
    }

    private static List<TeachingQuickTipViewModel> BuildQuickTips(
        ChildProfile child,
        AdaptiveRoutineSnapshot snapshot)
    {
        var tips = new List<TeachingQuickTipViewModel>
        {
            new()
            {
                Title = "Uma instrucao por vez",
                Body = "Evite explicar tudo de uma vez. Fale uma acao, espere a resposta e so depois avance para a proxima."
            },
            new()
            {
                Title = "Mostre agora e depois",
                Body = "Use uma fala curta ou um papel simples: agora atividade, depois pausa. Isso ajuda a crianca a entrar e sair da tarefa."
            },
            new()
            {
                Title = "Feche antes do desgaste",
                Body = $"Use o tempo como aliado: {snapshot.WorkBlockMinutes} min de trabalho e {snapshot.BreakMinutes} min de pausa costumam funcionar melhor hoje."
            },
            new()
            {
                Title = "Nao force no travamento",
                Body = "Se a crianca travar, pare, simplifique e volte com uma escolha pequena. O objetivo e terminar com alguma vitoria, nao insistir ate piorar."
            }
        };

        if (child.SupportProfile != SupportProfile.General)
        {
            tips.Add(new TeachingQuickTipViewModel
            {
                Title = "TEA: aceite resposta simples",
                Body = "Voce nao precisa cobrar frase longa. Apontar, olhar, escolher ou dizer uma palavra ja conta como resposta valida em muitos momentos."
            });
        }

        return tips.Take(5).ToList();
    }

    private static string BuildDailyRecommendation(DailyPlan plan, AdaptiveRoutineSnapshot snapshot)
    {
        var firstBlock = plan.Blocks.OrderBy(x => x.SortOrder).FirstOrDefault()?.Title ?? "uma atividade simples";
        return $"Hoje siga so esta ordem: comece por {firstBlock}, mantenha blocos curtos de {snapshot.WorkBlockMinutes} min, use pausa de {snapshot.BreakMinutes} min e termine com uma evidencia curta do que a crianca conseguiu fazer.";
    }

    private static string BuildSimpleOpeningSummary(DailyPlan plan, int age)
    {
        var titles = plan.Blocks
            .OrderBy(x => x.SortOrder)
            .Take(3)
            .Select(x => x.Title)
            .ToList();

        var opening = titles.Count switch
        {
            0 => "Hoje o foco e fazer um bloco curto, fechar com tranquilidade e guardar uma evidencia simples.",
            1 => $"Hoje voce vai conduzir 1 bloco curto: {titles[0]}.",
            2 => $"Hoje voce vai conduzir 2 blocos curtos: {titles[0]} e {titles[1]}.",
            _ => $"Hoje voce vai conduzir 3 blocos curtos: {titles[0]}, {titles[1]} e {titles[2]}."
        };

        return age <= 5
            ? $"{opening} Para esta idade, o certo e ensinar falando, mostrando, apontando e desenhando junto, sem cobrar leitura ou escrita."
            : $"{opening} O objetivo e explicar pouco por vez, fazer junto e registrar no final o que a crianca conseguiu.";
    }

    private static string BuildTomorrowAdjustment(DailyPlan plan, AdaptiveRoutineSnapshot snapshot)
    {
        var recoveryBlock = plan.Blocks
            .OrderBy(x => x.SortOrder)
            .FirstOrDefault(x => x.IsRecoveryFocus)?.Title;
        var openingBlock = recoveryBlock
            ?? plan.Blocks.OrderBy(x => x.SortOrder).FirstOrDefault()?.Title
            ?? "uma atividade curta";
        var reviewMinutes = Math.Clamp(snapshot.WorkBlockMinutes, 8, 15);

        return $"Amanha comece revisando {openingBlock} por cerca de {reviewMinutes} min, com menos fala, exemplo concreto e resposta curta por gesto, fala ou desenho.";
    }

    private static string BuildEvidenceReminder(PortuguesePlanningStageViewModel? stage)
    {
        var suggestion = stage?.SuggestedEvidence.FirstOrDefault();
        return string.IsNullOrWhiteSpace(suggestion)
            ? "No fim, salve uma foto, um video curto ou a producao final da atividade."
            : $"No fim, tente guardar isto: {suggestion}";
    }

    private static int CalculateAge(DateTime birthDate, DateTime today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static string GetFirstName(string fullName)
    {
        return fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?? "Crianca";
    }

    private static string GetSupportProfileLabel(SupportProfile profile) => profile switch
    {
        SupportProfile.TeaLevel1 => "TEA nivel 1",
        SupportProfile.TeaLevel2 => "TEA nivel 2",
        SupportProfile.TeaLevel3 => "TEA nivel 3",
        _ => "Perfil geral"
    };

    private static string GetFamilyGoalTrackLabel(string familyGoalTrack) => familyGoalTrack switch
    {
        "literacy" => "Alfabetizacao",
        "math_foundations" => "Matematica base",
        "autonomy" => "Autonomia e foco",
        "science_discovery" => "Ciencias em casa",
        _ => "Crescimento equilibrado"
    };

    private static string GetSubjectLabel(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Lingua Portuguesa",
        LearningDomain.Math => "Matematica",
        LearningDomain.World => "Mundo e ciencias",
        LearningDomain.ExecutiveFunction => "Funcao executiva",
        _ => "Materia do dia"
    };

    private static string GetSchoolStageLabel(int age) => age switch
    {
        4 => "Educacao Infantil (4 anos)",
        5 => "Educacao Infantil (5 anos)",
        6 => "1o ano do Ensino Fundamental",
        7 => "2o ano do Ensino Fundamental",
        8 => "3o ano do Ensino Fundamental",
        _ => "Rotina pedagogica em casa"
    };

    private static string GetSupportScopeLabel(CurriculumSupportScope supportScope) => supportScope switch
    {
        CurriculumSupportScope.TeaCommon => "TEA comum",
        CurriculumSupportScope.TeaLevel1 => "TEA nivel 1",
        CurriculumSupportScope.TeaLevel2 => "TEA nivel 2",
        CurriculumSupportScope.TeaLevel3 => "TEA nivel 3",
        _ => "Base comum"
    };

    private static string GetSupportScopeChip(CurriculumSupportScope supportScope) => supportScope switch
    {
        CurriculumSupportScope.TeaCommon => "support-common",
        CurriculumSupportScope.TeaLevel1 => "support-level1",
        CurriculumSupportScope.TeaLevel2 => "support-level2",
        CurriculumSupportScope.TeaLevel3 => "support-level3",
        _ => "support-base"
    };

    private static string GetFunctionalTrackLabel(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => "Comunicacao",
        FunctionalSupportTrack.Regulation => "Regulacao",
        FunctionalSupportTrack.Sensory => "Sensorial",
        FunctionalSupportTrack.DailyLiving => "Vida diaria",
        FunctionalSupportTrack.AcademicAdapted => "Academico adaptado",
        _ => "Base academica"
    };

    private static string GetFunctionalTrackChip(FunctionalSupportTrack track) => track switch
    {
        FunctionalSupportTrack.Communication => "track-communication",
        FunctionalSupportTrack.Regulation => "track-regulation",
        FunctionalSupportTrack.Sensory => "track-sensory",
        FunctionalSupportTrack.DailyLiving => "track-dailyliving",
        FunctionalSupportTrack.AcademicAdapted => "track-academic",
        _ => "track-base"
    };

    private static string TakeSentences(string? text, int sentenceCount)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sentences = text
            .Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(sentenceCount)
            .ToList();

        if (sentences.Count == 0)
        {
            return text.Trim();
        }

        return string.Join(". ", sentences).Trim().TrimEnd('.') + ".";
    }

    private static string ShortenText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength].TrimEnd() + "...";
    }

    private static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }

    private sealed class LaypersonGuide
    {
        public string Goal { get; init; } = string.Empty;
        public string AdultInstruction { get; init; } = string.Empty;
        public string ExampleScript { get; init; } = string.Empty;
        public string ChildInstruction { get; init; } = string.Empty;
        public List<string> MaterialItems { get; init; } = new();
        public List<string> StepByStep { get; init; } = new();
        public string IfStuck { get; init; } = string.Empty;
        public string SuccessSignal { get; init; } = string.Empty;
        public string EvidenceToSave { get; init; } = string.Empty;
    }
}
