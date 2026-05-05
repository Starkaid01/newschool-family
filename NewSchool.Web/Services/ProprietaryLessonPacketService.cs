using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ProprietaryLessonPacketService
{
    public ProprietaryLessonPacketViewModel BuildPacket(
        CuratedTaskTemplate task,
        ChildProfile child,
        DailyPlanBlock block,
        int age,
        ProprietaryCurriculumUnitBlueprintViewModel? currentUnit,
        ProprietaryCurriculumLessonBlueprintViewModel? currentLesson)
    {
        var firstName = GetFirstName(child.FullName);

        return task.Slug switch
        {
            "pre-language-dress-sequence" => BuildPreschoolLetterRoutinePacket(firstName),
            "pre-math-zanele-counting" => BuildPreschoolCountingPacket(firstName),
            "pre-world-body-care" => BuildPreschoolBodyCarePacket(firstName),
            "pre-executive-first-then-dress" => BuildPreschoolFirstThenPacket(firstName),
            "literacy-owl-retell" => BuildRetellPacket(firstName),
            "math-number-hunt-home" => BuildNumberHuntPacket(firstName),
            "world-night-animals-questions" => BuildNightAnimalsPacket(),
            "executive-checklist-three-steps" => BuildThreeStepChecklistPacket(firstName),
            "language-quote-and-summary" => BuildQuoteAndSummaryPacket(),
            "math-bar-model-home" => BuildBarModelPacket(firstName),
            "world-mini-research-three-facts" => BuildMiniResearchPacket(),
            "executive-plan-do-register" => BuildPlanDoRegisterPacket(firstName),
            "language-reading-journal-question" => BuildReadingJournalPacket(),
            "math-fraction-kitchen-halves" => BuildFractionsKitchenPacket(firstName),
            "world-nature-notebook-psalm" => BuildNatureNotebookPacket(),
            "executive-independent-material-check" => BuildIndependentMaterialPacket(firstName),
            "math-strategy-and-justification" => BuildMathStrategyJustificationPacket(firstName),
            "world-guided-study-notes" => BuildGuidedStudyNotesPacket(),
            "executive-independence-closeout" => BuildIndependenceCloseoutPacket(firstName),
            "pre-language-rhyme-basket" => BuildRhymeBasketPacket(),
            "pre-math-pattern-path" => BuildPatternPathPacket(firstName),
            "pre-world-bible-creation-colors" => BuildBibleCreationColorsPacket(),
            "pre-executive-pray-breathe-begin" => BuildPrayBreatheBeginPacket(firstName),
            "literacy-syllable-basket" => BuildSyllableBasketPacket(),
            "math-decompose-ten-frame" => BuildDecomposeTenPacket(firstName),
            "executive-opening-board" => BuildOpeningBoardPacket(firstName),
            "language-copywork-verse" => BuildCopyworkVersePacket(),
            "math-measure-home-race" => BuildMeasureHomeRacePacket(firstName),
            "world-biography-timeline" => BuildBiographyTimelinePacket(),
            "executive-study-board-three-missions" => BuildThreeMissionsPacket(firstName),
            "pre-language-name-trace-and-sound" => BuildNameTracePacket(firstName),
            "pre-language-story-picture-talk" => BuildStoryPictureTalkPacket(),
            "pre-math-more-less-baskets" => BuildMoreLessPacket(firstName),
            "pre-math-shapes-house-hunt" => BuildShapesHouseHuntPacket(firstName),
            "pre-world-bible-animal-pairs" => BuildBibleAnimalPairsPacket(),
            "pre-world-weather-window" => BuildWeatherWindowPacket(),
            "pre-executive-cleanup-basket" => BuildCleanupBasketPacket(firstName),
            "pre-executive-choose-and-finish" => BuildChooseAndFinishPacket(firstName),
            "literacy-picture-sequence-sentence" => BuildPictureSequenceSentencePacket(),
            "literacy-sound-to-word-path" => BuildSoundToWordPacket(firstName),
            "math-calendar-and-days" => BuildCalendarDaysPacket(firstName),
            "math-addition-with-objects" => BuildAdditionObjectsPacket(firstName),
            "world-neighborhood-map" => BuildNeighborhoodMapPacket(firstName),
            "world-community-helpers-around-us" => BuildCommunityHelpersPacket(),
            "executive-prepare-table-start" => BuildPrepareTablePacket(firstName),
            "executive-finish-and-tell" => BuildFinishAndTellPacket(firstName),
            "language-main-idea-and-details" => BuildMainIdeaPacket(),
            "language-character-problem-solution" => BuildCharacterProblemPacket(),
            "language-evidence-paragraph" => BuildEvidenceParagraphPacket(),
            "language-argument-with-proof" => BuildArgumentWithProofPacket(),
            "language-proverbs-copy-and-meaning" => BuildProverbMeaningPacket(),
            "language-compare-two-texts" => BuildCompareTextsPacket(),
            "math-multi-step-problem-table" => BuildMultiStepMathPacket(firstName),
            "math-money-plan" => BuildMoneyPlanPacket(firstName),
            "math-market-money-play" => BuildMarketMoneyPacket(firstName),
            "math-time-and-clock-routine" => BuildClockRoutinePacket(firstName),
            "math-perimeter-room-walk" => BuildPerimeterRoomPacket(firstName),
            "world-brazil-map-and-region" => BuildBrazilMapPacket(),
            "world-brazil-biome-observation" => BuildBrazilBiomePacket(),
            "world-brazil-history-connection" => BuildBrazilHistoryPacket(),
            "world-plant-growth-notes" => BuildPlantGrowthPacket(),
            "world-brazil-waterways-impact" => BuildBrazilWaterwaysPacket(),
            "executive-study-review-calendar" => BuildStudyCalendarPacket(firstName),
            "pre-bible-creation-colors" => BuildBibleCreationColorsPacket(),
            "pre-language-psalm-echo-and-draw" => BuildPsalmEchoPacket(),
            "pre-math-pairs-ark-animals" => BuildPairsArkAnimalsPacket(firstName),
            "literacy-bible-sequence-card-story" => BuildBibleSequenceStoryPacket(),
            "world-brazil-symbols-and-anthem" => BuildBrazilSymbolsPacket(),
            "executive-family-study-helper-role" => BuildFamilyStudyHelperPacket(firstName),
            "executive-two-step-independent-work" => BuildTwoStepIndependentPacket(firstName),
            "executive-self-check-before-submit" => BuildSelfCheckPacket(firstName),
            "executive-week-review-and-next-step" => BuildWeekReviewPacket(firstName),
            "language-thesis-and-two-proofs" => BuildThesisAndProofsPacket(),
            "math-ratio-table-justification" => BuildRatioTablePacket(firstName),
            "world-two-sources-one-conclusion" => BuildTwoSourcesPacket(),
            "executive-week-plan-and-proof" => BuildWeekPlanPacket(firstName),
            "language-claim-counterargument-closing" => BuildCounterArgumentPacket(),
            "math-budget-percent-choice" => BuildBudgetChoicePacket(firstName),
            "world-cause-consequence-and-position" => BuildCauseConsequencePacket(),
            "executive-independent-sprint-and-reflection" => BuildIndependentSprintPacket(firstName),
            "language-paraphrase-and-source-note" => BuildParaphrasePacket(),
            "math-discount-comparison" => BuildDiscountComparisonPacket(firstName),
            "world-timeline-and-change" => BuildTimelineChangePacket(),
            "executive-materials-and-deadline" => BuildMaterialsDeadlinePacket(firstName),
            "language-source-synthesis-and-position" => BuildSourceSynthesisPacket(),
            "math-equation-scenario-decision" => BuildEquationScenarioPacket(firstName),
            "world-public-problem-and-proposal" => BuildPublicProblemPacket(),
            "executive-project-checkpoint-adjustment" => BuildProjectCheckpointPacket(firstName),
            _ => BuildFallbackPacket(task, block, age, currentUnit, currentLesson)
        };
    }

    private static ProprietaryLessonPacketViewModel BuildRetellPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem em casa • inicio da alfabetizacao • reconto com comeco, meio e fim",
            UnitTitle = "Historia curta com tres momentos",
            UnitSummary = "A crianca ouve uma historia curta, reorganiza os acontecimentos e aprende a contar com ordem.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje eu vou ler uma historia curtinha e voce vai me contar o que aconteceu primeiro, depois e no final.\"",
            AnchorQuestion = "O que aconteceu primeiro, depois e no final?",
            CoreMaterialTitle = "Historia da licao: O filhote e a sombra",
            CoreMaterialParagraphs =
            [
                "Um filhote de gato saiu para brincar no quintal logo cedo.",
                "Quando o sol apareceu, ele viu uma sombra andando com ele e ficou curioso.",
                "Depois de observar por um tempo, o filhote percebeu que a sombra mudava quando ele corria, pulava e sentava."
            ],
            AdultSteps =
            [
                "Leia a historia uma vez inteira sem interromper.",
                "Leia de novo, parando depois de cada frase para a crianca repetir o que entendeu.",
                "Peça que a crianca conte a historia em tres partes usando as palavras primeiro, depois e no final.",
                "Se travar, ofereca tres desenhos rapidos para ajudar a lembrar da ordem."
            ],
            AdultQuestions =
            [
                "Quem saiu para brincar?",
                "O que chamou a atencao do filhote?",
                "O que ele descobriu no final?"
            ],
            AcceptableAnswers =
            [
                "Primeiro o filhote saiu para brincar.",
                "Depois ele viu a sombra e ficou curioso.",
                "No final ele entendeu que a sombra mudava com os movimentos dele."
            ],
            PracticeTask = "No papel, desenhe tres quadrinhos simples e escreva uma palavra para cada parte: saiu, viu, descobriu.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir recontar as tres partes na ordem certa, mesmo com apoio leve."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildNumberHuntPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica no cotidiano • 5 a 6 anos • numero, quantidade e registro",
            UnitTitle = "Numero do dia com objetos da casa",
            UnitSummary = "A crianca encontra um numero real, monta a quantidade e registra no caderno.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje o numero do dia e 8. Vamos achar esse numero, montar oito objetos e depois desenhar o que fizemos.\"",
            AnchorQuestion = "Como voce mostra o numero 8 com objetos e com desenho?",
            CoreMaterialLabel = "Missao de hoje",
            CoreMaterialTitle = "Numero do dia: 8",
            CoreMaterialParagraphs =
            [
                "Procure o numero 8 em um calendario, livro ou embalagem.",
                "Separe oito tampinhas, blocos ou botões.",
                "Depois desenhe oito bolinhas no caderno e circule grupos de 4 + 4."
            ],
            AdultSteps =
            [
                "Mostre o numero 8 escrito grande.",
                "Conte junto enquanto a crianca toca um objeto por vez.",
                "Depois de montar a quantidade, peca o desenho no caderno.",
                "Feche perguntando se oito pode aparecer em dois grupos menores."
            ],
            AdultQuestions =
            [
                "Quantos objetos temos agora?",
                "Se eu tirar um, fica quanto?",
                "Voce consegue mostrar o 8 em dois grupos?"
            ],
            AcceptableAnswers =
            [
                "Temos oito objetos.",
                "Se tirar um, ficam sete.",
                "Pode fazer 4 + 4 ou 5 + 3."
            ],
            PracticeTask = "Registrar no caderno: numero 8, desenho de oito objetos e uma divisao em dois grupos.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir montar a quantidade certa e representar o numero no papel."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildNightAnimalsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real • ciencias em casa • observacao e comparacao",
            UnitTitle = "Dia, noite e comportamento animal",
            UnitSummary = "A crianca compara o que muda no ambiente e descobre por que alguns animais saem mais a noite.",
            OpeningForAdult = "Diga: \"Hoje vamos comparar o dia e a noite para entender por que alguns animais ficam mais ativos quando escurece.\"",
            AnchorQuestion = "O que muda quando anoitece e por que um animal pode gostar disso?",
            CoreMaterialTitle = "Texto curto: O mocho acorda quando escurece",
            CoreMaterialParagraphs =
            [
                "Durante o dia, muitos passaros procuram alimento com bastante luz.",
                "Quando anoitece, alguns animais descansam, mas outros ficam mais atentos, como o mocho.",
                "A noite traz menos barulho e menos calor, o que ajuda certos animais a procurar comida com mais seguranca."
            ],
            AdultSteps =
            [
                "Leia o texto ou conte a ideia com suas palavras.",
                "Compare um animal diurno com um animal noturno.",
                "Peça uma observacao: luz, som, temperatura ou movimento.",
                "Feche com um desenho do animal escolhido e uma frase do tipo: ele prefere a noite porque..."
            ],
            AdultQuestions =
            [
                "O que muda no ambiente quando escurece?",
                "Qual animal do texto aparece como exemplo?",
                "Por que um animal pode preferir a noite?"
            ],
            AcceptableAnswers =
            [
                "Fica mais escuro e geralmente mais silencioso.",
                "O mocho.",
                "Porque encontra menos barulho, menos calor ou consegue procurar alimento melhor."
            ],
            PracticeTask = "Desenhar um animal noturno e completar a frase: Ele sai a noite porque...",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir comparar dia e noite e dar uma explicacao simples para o habito do animal."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildThreeStepChecklistPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 5 a 6 anos • inicio previsivel da rotina",
            UnitTitle = "Ritual curto antes da primeira licao",
            UnitSummary = "A crianca aprende um mesmo ritual de abertura para depender menos de lembretes.",
            OpeningForAdult = $"Diga: \"{firstName}, antes de estudar vamos fazer sempre os mesmos tres passos: pegar, ouvir, comecar.\"",
            AnchorQuestion = "Qual passo voce esta fazendo agora: pegar, ouvir ou comecar?",
            CoreMaterialLabel = "Checklist da licao",
            CoreMaterialTitle = "Os 3 passos de hoje",
            CoreMaterialParagraphs =
            [
                "1. Pegar o caderno e o lapis.",
                "2. Ouvir a missao do dia sem interromper.",
                "3. Comecar a primeira atividade em ate um minuto."
            ],
            AdultSteps =
            [
                "Leia os tres passos apontando para cada item.",
                "Peça que a crianca repita os passos em voz alta.",
                "Deixe a crianca executar e marque cada item concluido.",
                "Use exatamente a mesma ordem nos proximos dias."
            ],
            AdultQuestions =
            [
                "O que voce pega primeiro?",
                "Qual e o segundo passo?",
                "O que acontece depois que voce ouve a missao?"
            ],
            AcceptableAnswers =
            [
                "Primeiro eu pego o material.",
                "Depois eu escuto a missao.",
                "Depois eu comeco a primeira parte."
            ],
            PracticeTask = "Marcar os tres passos do ritual em um cartao ou folha curta.",
            CompletionDefinition = "Marque como concluida quando a crianca fizer os tres passos em ordem com no maximo um lembrete."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildQuoteAndSummaryPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 2o ano • leitura guiada • resumo e frase-chave",
            UnitTitle = "Ler, resumir e escolher a frase mais importante",
            UnitSummary = "A crianca pratica leitura curta, organiza o essencial e registra a frase que melhor representa o texto.",
            OpeningForAdult = "Diga: \"Hoje vamos ler um trecho curto, contar o que aconteceu e guardar a frase mais importante da leitura.\"",
            AnchorQuestion = "O que aconteceu e por que essa parte e a mais importante?",
            CoreMaterialTitle = "Trecho da licao: O menino e a muda de feijao",
            CoreMaterialParagraphs =
            [
                "Joao colocou um feijao no algodao e deixou o copo perto da janela.",
                "Dois dias depois, ele percebeu uma pontinha verde saindo da semente.",
                "Joao entendeu que a planta precisava de agua, luz e paciencia para crescer."
            ],
            AdultSteps =
            [
                "Peça leitura em voz alta, alternando com sua ajuda quando necessario.",
                "Pergunte o que aconteceu no texto em uma frase.",
                "Pergunte por que a ultima frase e importante para entender a historia.",
                "Registre no caderno um resumo curto e destaque a frase-chave."
            ],
            AdultQuestions =
            [
                "O que Joao fez primeiro?",
                "O que aconteceu depois de dois dias?",
                "Qual frase ensina a coisa mais importante do texto?"
            ],
            AcceptableAnswers =
            [
                "Ele colocou o feijao no algodao.",
                "A semente comecou a brotar.",
                "A frase importante e a que diz que a planta precisa de agua, luz e paciencia."
            ],
            PracticeTask = "Escrever duas frases: uma dizendo o que aconteceu e outra dizendo o que a leitura ensinou.",
            CompletionDefinition = "Marque como concluida quando a crianca resumir o texto em duas frases com sentido e escolher a frase-chave."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildReadingJournalPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 2o ano • diario de leitura • resposta com uma pergunta forte",
            UnitTitle = "Uma pergunta que faz a leitura render",
            UnitSummary = "Em vez de muitas perguntas rasas, a familia trabalha uma pergunta central e registra a melhor resposta.",
            OpeningForAdult = "Diga: \"Hoje nao vamos responder muitas perguntas. Vamos escolher uma pergunta forte e pensar nela com calma.\"",
            AnchorQuestion = "Qual pergunta ajuda a entender esse texto de verdade?",
            CoreMaterialTitle = "Trecho da licao: A visita ao mercado",
            CoreMaterialParagraphs =
            [
                "Lia foi ao mercado com a avo para comprar frutas, arroz e sabao.",
                "No caminho, ela percebeu que a avo sempre comparava os precos antes de escolher.",
                "No fim, Lia disse que economizar tambem e uma forma de cuidar da casa."
            ],
            AdultSteps =
            [
                "Leia o texto junto com a crianca.",
                "Escolha a pergunta central: o que Lia aprendeu na ida ao mercado?",
                "Converse oralmente primeiro, sem escrever ainda.",
                "Registre no diario duas frases com a melhor resposta encontrada."
            ],
            AdultQuestions =
            [
                "O que a avo fazia antes de escolher os produtos?",
                "O que Lia percebeu nessa visita?",
                "Qual foi o aprendizado principal do texto?"
            ],
            AcceptableAnswers =
            [
                "A avo comparava os precos.",
                "Lia percebeu que comprar bem ajuda a cuidar da casa.",
                "O aprendizado principal e que economizar tambem e cuidar da familia."
            ],
            PracticeTask = "No diario, escrever a pergunta central e responder em duas frases completas.",
            CompletionDefinition = "Marque como concluida quando a crianca registrar uma resposta coerente para a pergunta central."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildFractionsKitchenPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 2o ano • nocao de fracao • metade e quarto",
            UnitTitle = "Dividir de forma justa",
            UnitSummary = "A crianca percebe metade e quarto com material concreto antes de decorar palavras.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos dividir algo de forma justa. Primeiro em duas partes, depois em quatro.\"",
            AnchorQuestion = "Como voce sabe que a divisao ficou justa?",
            CoreMaterialLabel = "Situacao da licao",
            CoreMaterialTitle = "Papel dobrado ou fatia imaginaria",
            CoreMaterialParagraphs =
            [
                "Pegue um papel quadrado, uma massinha ou um sanduiche de brinquedo.",
                "Primeiro, divida em duas partes iguais.",
                "Depois, pegue uma dessas partes e veja como ela pode virar duas de novo."
            ],
            AdultSteps =
            [
                "Mostre a divisao em duas partes iguais e nomeie metade.",
                "Abra a divisao em quatro partes e nomeie quarto.",
                "Compare os tamanhos sem correr para simbolos.",
                "Peça que a crianca explique com as maos e com a fala."
            ],
            AdultQuestions =
            [
                "Quantas partes temos quando dividimos no meio?",
                "O que acontece se dividirmos de novo?",
                "Qual parte e maior: metade ou quarto?"
            ],
            AcceptableAnswers =
            [
                "Temos duas partes iguais.",
                "Passamos a ter quatro partes.",
                "Metade e maior do que quarto."
            ],
            PracticeTask = "Desenhar um circulo ou quadrado no caderno e marcar metade e quarto com cores diferentes.",
            CompletionDefinition = "Marque como concluida quando a crianca mostrar metade e quarto com material concreto e explicar qual parte e maior."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildNatureNotebookPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Ciencias e mundo real • 2o ano • observacao da natureza com registro",
            UnitTitle = "Observar, notar e agradecer",
            UnitSummary = "A crianca treina olhar atento, nomeia detalhes e registra uma observacao com linguagem clara.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar uma coisa da natureza com calma e anotar o que quase todo mundo deixa passar.\"",
            AnchorQuestion = "O que voce percebeu que antes passaria despercebido?",
            CoreMaterialLabel = "Objeto de observacao",
            CoreMaterialTitle = "Observe uma folha, flor, nuvem ou passarinho",
            CoreMaterialParagraphs =
            [
                "Pare por um minuto em silencio diante do objeto escolhido.",
                "Note cor, forma, textura, tamanho ou movimento.",
                "Depois transforme a observacao em tres detalhes escritos ou desenhados."
            ],
            AdultSteps =
            [
                "Escolha um unico elemento da natureza.",
                "Peça um minuto de observacao silenciosa.",
                "Anote tres detalhes exatos, nao genericos.",
                "Feche com uma frase curta de admiracao ou gratidao."
            ],
            AdultQuestions =
            [
                "Que cor voce ve primeiro?",
                "Que detalhe pequeno quase ninguem nota?",
                "O que isso mostra sobre o mundo a nossa volta?"
            ],
            AcceptableAnswers =
            [
                "A folha e verde escura com nervuras claras.",
                "A borda e serrilhada ou a nuvem mudou de forma.",
                "Mostra variedade, beleza ou organizacao na natureza."
            ],
            PracticeTask = "Registrar tres detalhes no caderno da natureza e fechar com a frase: Hoje eu reparei que...",
            CompletionDefinition = "Marque como concluida quando a crianca registrar tres observacoes concretas sobre o que viu."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildIndependentMaterialPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 2o ano • preparar o material sem depender do adulto",
            UnitTitle = "Conferencia independente",
            UnitSummary = "A crianca aprende a checar o material da propria licao seguindo uma ordem fixa.",
            OpeningForAdult = $"Diga: \"{firstName}, antes de comecar a licao voce vai conferir sozinho o que precisa. Eu so corrijo no final.\"",
            AnchorQuestion = "O que ainda falta na sua mesa para a licao comecar?",
            CoreMaterialLabel = "Lista da licao",
            CoreMaterialTitle = "Conferencia em 4 itens",
            CoreMaterialParagraphs =
            [
                "1. Caderno aberto na pagina certa.",
                "2. Lapis e borracha na mesa.",
                "3. Material especial da licao separado.",
                "4. Espaco livre para escrever."
            ],
            AdultSteps =
            [
                "Leia os quatro itens so na primeira vez.",
                "Depois, deixe a crianca fazer a conferencia sozinha.",
                "Espere ate o final para corrigir o que faltou.",
                "Repita a mesma ordem por varios dias seguidos."
            ],
            AdultQuestions =
            [
                "Qual item voce conferiu primeiro?",
                "Tem algo faltando na mesa?",
                "Como voce sabe que esta pronto para comecar?"
            ],
            AcceptableAnswers =
            [
                "Eu comecei pelo caderno.",
                "Falta a borracha ou o material especial.",
                "Porque todos os quatro itens estao prontos."
            ],
            PracticeTask = "Usar a mesma lista em duas licões do dia e marcar cada item concluido.",
            CompletionDefinition = "Marque como concluida quando a crianca fizer a conferencia completa com no maximo um ajuste final do adulto."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMainIdeaPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 4o ano • leitura informativa • ideia principal e detalhes de apoio",
            UnitTitle = "Rios do Brasil e sua importancia",
            UnitSummary = "A crianca aprende a descobrir o assunto central do texto e separar os detalhes que realmente provam essa ideia.",
            OpeningForAdult = "Diga: \"Hoje vamos ler um texto curto sobre um rio brasileiro e descobrir a mensagem principal dele. Depois vamos provar essa ideia com dois detalhes do proprio texto.\"",
            AnchorQuestion = "Qual e a ideia principal deste texto e quais dois detalhes realmente provam isso?",
            CoreMaterialTitle = "Texto da licao: O rio que ajuda muita gente",
            CoreMaterialParagraphs =
            [
                "O Rio Sao Francisco passa por varios estados brasileiros e ajuda comunidades de diferentes regioes do pais.",
                "Em muitos lugares, suas aguas servem para pesca, transporte, plantio e abastecimento das casas.",
                "Por isso, quando um rio importante e mal cuidado, muita gente sente os problemas no dia a dia."
            ],
            AdultSteps =
            [
                "Leia o texto em voz alta uma vez inteira.",
                "Na segunda leitura, peca que a crianca marque as palavras mais importantes.",
                "Pergunte: se eu tivesse de contar esse texto em uma frase, o que nao poderia faltar?",
                "Depois peca dois detalhes do texto que provam a frase escolhida."
            ],
            AdultQuestions =
            [
                "Sobre o que o texto esta falando de verdade?",
                "Qual frase resume melhor o texto inteiro?",
                "Que duas informacoes do texto ajudam a provar essa frase?"
            ],
            AcceptableAnswers =
            [
                "Ideia principal possivel: O Rio Sao Francisco e importante para a vida de muitas pessoas.",
                "Detalhe 1: Ele passa por varios estados brasileiros.",
                "Detalhe 2: Suas aguas servem para pesca, transporte, plantio e abastecimento."
            ],
            PracticeTask = "No caderno, escrever 1 frase de ideia principal e listar 2 detalhes de apoio retirados do texto.",
            CompletionDefinition = "Marque como concluida quando a crianca registrar uma ideia principal coerente e dois detalhes do texto que realmente a sustentem."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildEvidenceParagraphPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 4o ano • resposta com evidencia • paragrafo curto",
            UnitTitle = "Responder com prova, nao so com opiniao",
            UnitSummary = "A crianca sai do comentario solto e aprende a sustentar uma ideia com uma prova do texto.",
            OpeningForAdult = "Diga: \"Hoje voce vai responder em um paragrafo curto. Primeiro vem a ideia principal, depois uma prova do texto e por fim sua conclusao.\"",
            AnchorQuestion = "O que o texto ensina, qual prova aparece nele e como voce fecha essa resposta?",
            CoreMaterialTitle = "Texto da licao: A pracinha da comunidade",
            CoreMaterialParagraphs =
            [
                "Os moradores do bairro passaram meses pedindo reforma para a pracinha.",
                "Quando bancos, lixeiras e brinquedos foram arrumados, mais familias voltaram a usar o espaco.",
                "O lugar ficou mais limpo porque as pessoas passaram a cuidar melhor daquilo que agora estava funcionando."
            ],
            AdultSteps =
            [
                "Leia o texto junto com a crianca.",
                "Peça uma frase principal respondendo: o que esse texto mostra?",
                "Peça uma prova especifica retirada do texto.",
                "Feche com uma conclusao curta em palavras proprias."
            ],
            AdultQuestions =
            [
                "Qual e a ideia principal do texto?",
                "Que parte do texto serve como prova?",
                "Como voce conclui essa resposta com suas palavras?"
            ],
            AcceptableAnswers =
            [
                "Ideia principal: Quando um espaco publico e cuidado, as pessoas voltam a usa-lo melhor.",
                "Prova: Mais familias voltaram a usar a pracinha depois que ela foi arrumada.",
                "Conclusao: Cuidar dos lugares da comunidade melhora a vida de todo mundo."
            ],
            PracticeTask = "Escrever um paragrafo de 3 frases: ideia principal, prova do texto e conclusao.",
            CompletionDefinition = "Marque como concluida quando o paragrafo tiver uma ideia central clara, uma prova real do texto e um fechamento coerente."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildArgumentWithProofPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 4o ano • opiniao com base no texto • argumento inicial",
            UnitTitle = "Dizer o que pensa e provar",
            UnitSummary = "A crianca pratica opiniao responsavel: nao basta falar o que pensa, e preciso sustentar com o texto.",
            OpeningForAdult = "Diga: \"Hoje voce vai dar uma opiniao, mas ela nao vale so porque voce disse. Vamos procurar uma prova no texto para sustentar essa resposta.\"",
            AnchorQuestion = "Qual e sua opiniao, que prova no texto mostra isso e como voce fecha a ideia?",
            CoreMaterialTitle = "Texto da licao: Bicicleta para ir a escola",
            CoreMaterialParagraphs =
            [
                "Em uma cidade pequena, varias criancas passaram a ir para a escola de bicicleta.",
                "Com isso, algumas familias economizaram no transporte e os alunos chegaram mais despertos para estudar.",
                "A mudanca so funcionou porque houve mais cuidado com o trajeto e mais atencao dos adultos."
            ],
            AdultSteps =
            [
                "Leia o texto e pergunte se a mudanca pareceu boa ou ruim.",
                "Peça uma opiniao clara em uma frase.",
                "Procure junto uma prova no proprio texto.",
                "Peça uma conclusao curta usando as palavras da crianca."
            ],
            AdultQuestions =
            [
                "Voce acha que ir de bicicleta foi uma boa mudanca? Por que?",
                "Que parte do texto ajuda a provar isso?",
                "Qual seria sua frase final?"
            ],
            AcceptableAnswers =
            [
                "Opiniao possivel: Foi uma boa mudanca.",
                "Prova: As familias economizaram e os alunos chegaram mais despertos.",
                "Fechamento: Deu certo porque houve beneficio e organizacao."
            ],
            PracticeTask = "Escrever 3 frases: minha opiniao, a prova do texto e minha conclusao.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir ligar opiniao e prova do texto sem fugir do assunto."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildProverbMeaningPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Portugues do 4o ano • copia com sentido • leitura, interpretacao e aplicacao",
            UnitTitle = "Proverbio curto com aplicacao pratica",
            UnitSummary = "A crianca copia menos e entende mais: primeiro interpreta, depois escreve e aplica.",
            OpeningForAdult = "Diga: \"Hoje voce nao vai so copiar. Primeiro vamos entender o que esta frase ensina para a vida real.\"",
            AnchorQuestion = "O que esse proverbio quer ensinar para a vida real?",
            CoreMaterialTitle = "Frase da licao",
            CoreMaterialParagraphs =
            [
                "\"A resposta branda desvia o furor.\"",
                "Explique em linguagem simples: falar com calma ajuda a diminuir uma briga.",
                "Pense em uma situacao da vida em que responder com calma mudaria o resultado."
            ],
            AdultSteps =
            [
                "Leia a frase duas vezes em voz alta.",
                "Explique as palavras que podem travar a compreensao.",
                "Puxe um exemplo real de casa, irmaos ou amizade.",
                "So depois peca a copia e uma frase de aplicacao."
            ],
            AdultQuestions =
            [
                "O que e uma resposta branda?",
                "O que esse proverbio quer evitar?",
                "Em que situacao da vida real isso pode ajudar?"
            ],
            AcceptableAnswers =
            [
                "Resposta branda e uma resposta calma.",
                "Quer evitar aumento de raiva ou briga.",
                "Pode ajudar quando duas pessoas estao discutindo."
            ],
            PracticeTask = "Copiar o proverbio no caderno e escrever uma frase de aplicacao do tipo: Isso me ajuda quando...",
            CompletionDefinition = "Marque como concluida quando a crianca copiar a frase com cuidado e explicar o sentido dela com um exemplo real."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMultiStepMathPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 4o ano • problema de duas etapas • organizar antes de calcular",
            UnitTitle = "Tabela de raciocinio antes da conta",
            UnitSummary = "A crianca aprende a separar dados e decidir a ordem das operacoes antes de sair calculando no impulso.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos resolver um problema em duas etapas, mas primeiro vamos organizar as informacoes numa tabela.\"",
            AnchorQuestion = "O que ja sabemos, o que falta descobrir e qual conta vem primeiro?",
            CoreMaterialLabel = "Problema da licao",
            CoreMaterialTitle = "Cadernos para a oficina",
            CoreMaterialParagraphs =
            [
                "Uma oficina de desenho recebeu 4 caixas com 6 cadernos em cada uma.",
                "Antes da aula comecar, 5 cadernos foram separados para os professores.",
                "Quantos cadernos sobraram para distribuir entre os alunos?"
            ],
            AdultSteps =
            [
                "Desenhe uma tabela com tres linhas: o que sabemos, o que vamos descobrir primeiro, o que falta no final.",
                "Ajude a crianca a perceber que primeiro precisa descobrir o total de cadernos.",
                "Depois peca a segunda etapa: tirar os cadernos separados para os professores.",
                "Feche pedindo que ela explique o caminho inteiro em voz alta."
            ],
            AdultQuestions =
            [
                "Quantas caixas existem?",
                "Quantos cadernos ha em cada caixa?",
                "Qual conta precisa vir primeiro?",
                "O que fazemos com os 5 cadernos separados?"
            ],
            AcceptableAnswers =
            [
                "Primeira conta: 4 x 6 = 24.",
                "Depois: 24 - 5 = 19.",
                "Resposta final: sobraram 19 cadernos para os alunos."
            ],
            PracticeTask = "Preencher a tabela e resolver as duas etapas no caderno, deixando as operacoes visiveis.",
            CompletionDefinition = "Marque como concluida quando a crianca organizar os dados, resolver as duas etapas e explicar a ordem das contas."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMoneyPlanPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 4o ano • dinheiro e comparacao • decidir e justificar",
            UnitTitle = "Qual compra faz mais sentido?",
            UnitSummary = "A crianca compara valores e aprende a defender sua escolha com conta e justificativa.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce vai decidir qual compra faz mais sentido e vai me provar isso com os numeros.\"",
            AnchorQuestion = "Qual opcao faz mais sentido e como voce pode provar?",
            CoreMaterialLabel = "Situacao-problema",
            CoreMaterialTitle = "Lanche para um passeio",
            CoreMaterialParagraphs =
            [
                "Uma garrafa de suco custa 7 reais.",
                "Um pacote com 4 caixinhas custa 24 reais.",
                "A familia precisa de 4 porcoes de bebida para um passeio. O que compensa mais?"
            ],
            AdultSteps =
            [
                "Peça que a crianca diga o que cada opcao entrega.",
                "Pergunte quanto custaria comprar 4 garrafas separadas.",
                "Compare com o pacote fechado.",
                "Peça uma justificativa curta, nao so a resposta."
            ],
            AdultQuestions =
            [
                "Quanto custam 4 garrafas separadas?",
                "Quanto custa o pacote?",
                "Qual opcao tem menor valor total?",
                "Como voce explicaria essa escolha para outra pessoa?"
            ],
            AcceptableAnswers =
            [
                "4 garrafas separadas custam 28 reais.",
                "O pacote custa 24 reais.",
                "O pacote compensa mais porque sai 4 reais mais barato."
            ],
            PracticeTask = "Escrever a conta de cada opcao e uma frase final justificando a melhor escolha.",
            CompletionDefinition = "Marque como concluida quando a crianca comparar as duas opcoes e justificar a resposta com os valores."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMarketMoneyPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 2o ao 3o ano • mercadinho em casa • numero, compra e troco",
            UnitTitle = "Comprando com valor certo",
            UnitSummary = "A crianca usa matematica funcional em um mini mercado montado na mesa.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce e o cliente do nosso mercadinho. Vai escolher produtos, pagar e conferir se sobrou alguma moeda.\"",
            AnchorQuestion = "Quanto custa, quanto falta e sobrou alguma moeda?",
            CoreMaterialLabel = "Mercadinho da licao",
            CoreMaterialTitle = "3 produtos com preco simples",
            CoreMaterialParagraphs =
            [
                "Banana: 3 reais.",
                "Pao: 5 reais.",
                "Suco: 4 reais."
            ],
            AdultSteps =
            [
                "Escolha dois produtos para a compra.",
                "Peça que a crianca some os valores.",
                "Entregue um valor de pagamento, como 10 reais.",
                "Pergunte se falta dinheiro ou se existe troco."
            ],
            AdultQuestions =
            [
                "Quanto custa a compra escolhida?",
                "Se pagar com 10 reais, sobra quanto?",
                "Qual conta voce fez para descobrir?"
            ],
            AcceptableAnswers =
            [
                "Banana + pao = 8 reais.",
                "Se pagar com 10, sobram 2 reais.",
                "Usei adicao para o total e subtracao para o troco."
            ],
            PracticeTask = "Montar duas compras diferentes e registrar total e troco das duas.",
            CompletionDefinition = "Marque como concluida quando a crianca somar o valor da compra e explicar se houve troco ou falta de dinheiro."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBrazilMapPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Geografia inicial do Brasil • 5 a 6 anos • mapa, regiao e curiosidade",
            UnitTitle = "Onde fica e o que chama atencao",
            UnitSummary = "A crianca ve o mapa do Brasil, localiza uma regiao e liga o lugar a uma curiosidade concreta.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar o mapa do Brasil e descobrir uma parte do pais com uma curiosidade facil de lembrar.\"",
            AnchorQuestion = "Que parte do Brasil estamos vendo e o que chama sua atencao nela?",
            CoreMaterialTitle = "Regiao do dia: Nordeste",
            CoreMaterialParagraphs =
            [
                "O Nordeste fica na parte leste e norte do mapa do Brasil.",
                "Muitas praias famosas ficam nessa regiao.",
                "Tambem e uma regiao com comidas, festas e paisagens bem marcantes."
            ],
            AdultSteps =
            [
                "Mostre o contorno do Brasil e aponte a regiao do dia.",
                "Pinte a regiao com uma cor forte.",
                "Conte uma curiosidade simples ligada a comida, clima ou paisagem.",
                "Peça que a crianca repita o nome da regiao e desenhe um simbolo dela."
            ],
            AdultQuestions =
            [
                "Qual regiao estamos olhando hoje?",
                "O que existe nela que chama atencao?",
                "Voce lembra em que parte do mapa ela fica?"
            ],
            AcceptableAnswers =
            [
                "E o Nordeste.",
                "Tem praias, comidas e festas conhecidas.",
                "Fica do lado direito de cima do mapa."
            ],
            PracticeTask = "Colorir a regiao no mapa e desenhar uma curiosidade ao lado.",
            CompletionDefinition = "Marque como concluida quando a crianca reconhecer o nome da regiao e ligar o lugar a pelo menos uma curiosidade concreta."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBrazilBiomePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Geografia do Brasil • 4o ano • comparacao entre biomas",
            UnitTitle = "Amazonia e Cerrado lado a lado",
            UnitSummary = "A crianca compara dois biomas brasileiros com clima, vegetacao, animais e importancia ambiental.",
            OpeningForAdult = "Diga: \"Hoje vamos comparar dois biomas do Brasil. Nao e para decorar nomes soltos; e para perceber o que muda de um ambiente para o outro.\"",
            AnchorQuestion = "O que muda de um bioma para o outro e por que isso importa?",
            CoreMaterialTitle = "Texto da licao: Dois biomas, dois jeitos de viver",
            CoreMaterialParagraphs =
            [
                "A Amazonia tem muita umidade, rios grandes e vegetacao fechada. E um ambiente com grande diversidade de plantas e animais.",
                "O Cerrado tem ar mais seco em boa parte do ano, vegetacao mais aberta e arvores com troncos retorcidos.",
                "Cada bioma abriga formas diferentes de vida. Quando um deles e destruido, animais, plantas e comunidades locais sofrem as consequencias."
            ],
            AdultSteps =
            [
                "Leia o texto inteiro com a crianca.",
                "Monte uma tabela com duas colunas: Amazonia e Cerrado.",
                "Preencha juntos clima, vegetacao e exemplos de vida animal.",
                "Feche com uma conclusao sobre por que comparar biomas ajuda a pensar em preservacao."
            ],
            AdultQuestions =
            [
                "Como e o clima da Amazonia?",
                "Como e a vegetacao do Cerrado?",
                "Por que nao faz sentido tratar os dois biomas como se fossem iguais?"
            ],
            AcceptableAnswers =
            [
                "A Amazonia e mais umida e cheia de rios.",
                "O Cerrado tem vegetacao mais aberta e ar mais seco em boa parte do ano.",
                "Porque cada bioma tem clima, plantas e animais diferentes."
            ],
            PracticeTask = "Preencher uma tabela comparativa e escrever uma frase final sobre por que os biomas precisam ser preservados.",
            CompletionDefinition = "Marque como concluida quando a crianca comparar os dois biomas com pelo menos dois contrastes corretos e uma conclusao final."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBrazilHistoryPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Historia e geografia do Brasil • 4o ano • passado ligado ao presente",
            UnitTitle = "Cafe, estrada e cidade",
            UnitSummary = "A crianca percebe que um fato historico so faz sentido quando consegue enxergar reflexo dele na vida de hoje.",
            OpeningForAdult = "Diga: \"Hoje vamos ver um tema do Brasil e descobrir onde ele ainda aparece na vida de hoje.\"",
            AnchorQuestion = "O que aprendemos sobre o Brasil e onde isso aparece hoje?",
            CoreMaterialTitle = "Texto da licao: O cafe mudou caminhos e cidades",
            CoreMaterialParagraphs =
            [
                "Em varios momentos da historia do Brasil, o cultivo do cafe movimentou estradas, trabalho e comercio.",
                "Cidades cresceram perto de areas onde havia producao e transporte desse produto.",
                "Ainda hoje, quando falamos de trabalho no campo, exportacao e desenvolvimento de certas regioes, parte dessa historia continua aparecendo."
            ],
            AdultSteps =
            [
                "Leia o texto destacando o antes e o agora.",
                "Anote tres fatos do passado citados no texto.",
                "Peça uma conexao com o presente para cada um deles.",
                "Feche com uma frase: isso aparece hoje quando..."
            ],
            AdultQuestions =
            [
                "Que produto aparece no texto?",
                "O que ele ajudou a movimentar no passado?",
                "Onde voce percebe reflexo disso hoje?"
            ],
            AcceptableAnswers =
            [
                "O cafe.",
                "Ajudou a movimentar trabalho, estradas e cidades.",
                "Hoje isso pode aparecer na economia, no campo ou em regioes produtoras."
            ],
            PracticeTask = "Fazer um quadro com duas colunas: passado e hoje.",
            CompletionDefinition = "Marque como concluida quando a crianca ligar pelo menos dois fatos do passado a um reflexo do presente."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildStudyCalendarPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 4o ano • fechar o dia sabendo o proximo passo",
            UnitTitle = "Semana visivel e sem excesso",
            UnitSummary = "A crianca aprende a olhar o que ja foi feito e enxergar so o proximo passo, sem se perder em um monte de metas.",
            OpeningForAdult = $"Diga: \"{firstName}, agora vamos olhar a semana de um jeito simples: o que foi feito, o que falta e qual e o proximo passo real.\"",
            AnchorQuestion = "O que foi feito, o que falta e qual e o proximo passo real?",
            CoreMaterialLabel = "Calendario da semana",
            CoreMaterialTitle = "Quadro curto de 5 dias",
            CoreMaterialParagraphs =
            [
                "Segunda: leitura concluida.",
                "Terca: matematica concluida.",
                "Quarta: geografia em andamento.",
                "Quinta e sexta: ainda vazias."
            ],
            AdultSteps =
            [
                "Abra o quadro da semana e mostre so cinco espacos.",
                "Peça que a crianca marque o que ja concluiu.",
                "Pergunte qual e o item que sobe para o dia seguinte.",
                "Feche com uma combinacao curta para amanha."
            ],
            AdultQuestions =
            [
                "O que ja foi concluido?",
                "Qual item ainda esta em andamento?",
                "Qual e o proximo passo de amanha?"
            ],
            AcceptableAnswers =
            [
                "Leitura e matematica ja foram feitas.",
                "Geografia esta em andamento.",
                "Amanha o proximo passo e terminar geografia ou abrir a proxima licao definida."
            ],
            PracticeTask = "Marcar a semana com tres cores: feito, em andamento e proximo.",
            CompletionDefinition = "Marque como concluida quando a crianca identificar o que ja fez e disser com clareza qual e o proximo passo."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildWeekReviewPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 4o ano • revisao curta e proximo passo",
            UnitTitle = "Olhar para tras e decidir o seguinte",
            UnitSummary = "A crianca fecha a rotina com leitura de progresso, sem drama e sem metas soltas demais.",
            OpeningForAdult = $"Diga: \"{firstName}, vamos fechar a semana olhando uma coisa que funcionou, uma que travou e o que voce vai fazer no proximo passo.\"",
            AnchorQuestion = "O que funcionou, o que travou e qual e o proximo passo concreto?",
            CoreMaterialLabel = "Fechamento da semana",
            CoreMaterialTitle = "Roteiro de 3 respostas",
            CoreMaterialParagraphs =
            [
                "1. O que funcionou bem nesta semana?",
                "2. O que ainda travou ou cansou?",
                "3. Qual e o proximo passo concreto para o proximo dia?"
            ],
            AdultSteps =
            [
                "Leia uma pergunta de cada vez.",
                "Peça respostas curtas e reais, nao perfeitas.",
                "Anote so uma frase por pergunta.",
                "Feche repetindo o proximo passo em voz alta."
            ],
            AdultQuestions =
            [
                "Qual licao saiu melhor nesta semana?",
                "Onde voce ainda sentiu dificuldade?",
                "O que voce vai fazer primeiro no proximo dia?"
            ],
            AcceptableAnswers =
            [
                "Funcionou melhor a leitura ou a matematica.",
                "Travou em copiar, organizar ou manter foco.",
                "O proximo passo pode ser abrir o caderno certo, terminar a tabela ou ler o novo texto."
            ],
            PracticeTask = "Escrever tres frases curtas, uma para cada pergunta do roteiro.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir nomear um acerto, uma dificuldade e um proximo passo real."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPreschoolLetterRoutinePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Pre-alfabetizacao • 3 a 4 anos • letra, som e palavra concreta",
            UnitTitle = "Letra da semana com objetos da casa",
            UnitSummary = "A crianca associa uma letra ao seu som e encontra palavras reais que comecam com esse som.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos brincar com uma letra e achar coisas da casa que combinam com esse som.\"",
            AnchorQuestion = "Que som esta letra faz e que palavra da casa comeca com ela?",
            CoreMaterialTitle = "Letra do dia: M",
            CoreMaterialParagraphs =
            [
                "Mostre a letra M bem grande.",
                "Repita devagar o som mmmmm.",
                "Procure palavras simples como mesa, mao ou meia."
            ],
            AdultSteps =
            [
                "Mostre a letra com o dedo e diga o som duas vezes.",
                "Peça que a crianca repita o som.",
                "Ache duas palavras reais da casa com o mesmo inicio.",
                "Feche com tracado grande no ar ou na folha."
            ],
            AdultQuestions =
            [
                "Que som voce ouviu?",
                "Que objeto da casa comeca com esse som?",
                "Voce consegue fazer a letra bem grande?"
            ],
            AcceptableAnswers =
            [
                "Som de mmmmm.",
                "Mesa, meia, mamae ou mao.",
                "A crianca faz o tracado grande mesmo sem perfeicao."
            ],
            PracticeTask = "Escolher uma palavra com M, apontar o objeto e fazer o tracado grande da letra.",
            CompletionDefinition = "Marque como concluida quando a crianca repetir o som da letra e ligar esse som a uma palavra concreta."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPreschoolCountingPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica inicial • 3 a 4 anos • contagem com toque e quantidade pequena",
            UnitTitle = "Contar sem pular objetos",
            UnitSummary = "A crianca aprende a tocar cada objeto enquanto conta e compara grupos pequenos.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos contar devagar para nenhum objeto ficar sem ser tocado.\"",
            AnchorQuestion = "Quantos objetos temos aqui e qual grupo tem mais?",
            CoreMaterialTitle = "Contagem do dia: 5 tampinhas",
            CoreMaterialParagraphs =
            [
                "Separe 5 tampinhas em linha.",
                "Conte tocando uma por uma.",
                "Depois compare com um grupo menor."
            ],
            AdultSteps =
            [
                "Monte um grupo de 5 objetos.",
                "Conte junto, tocando um por vez.",
                "Monte um segundo grupo com 3 objetos.",
                "Pergunte qual grupo tem mais e qual tem menos."
            ],
            AdultQuestions =
            [
                "Quantos objetos tem no primeiro grupo?",
                "Quantos tem no segundo grupo?",
                "Qual grupo tem mais?"
            ],
            AcceptableAnswers =
            [
                "Primeiro grupo com 5.",
                "Segundo grupo com 3.",
                "O grupo com 5 tem mais."
            ],
            PracticeTask = "Montar dois grupos pequenos e dizer qual tem mais, menos ou igual.",
            CompletionDefinition = "Marque como concluida quando a crianca contar sem pular objetos e comparar dois grupos pequenos."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPreschoolBodyCarePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real • 3 a 4 anos • corpo, roupa e autocuidado",
            UnitTitle = "Roupas para o dia de hoje",
            UnitSummary = "A crianca observa o proprio corpo, pensa no clima e escolhe uma roupa adequada.",
            OpeningForAdult = $"Diga: \"{firstName}, vamos pensar no seu dia. Que roupa combina com o clima e com o que voce vai fazer?\"",
            AnchorQuestion = "Que roupa combina com o seu dia de hoje e por que?",
            CoreMaterialTitle = "Situacao da licao",
            CoreMaterialParagraphs =
            [
                "Hoje esta frio ou calor?",
                "Voce vai brincar dentro de casa ou sair?",
                "Que peca ajuda mais neste momento?"
            ],
            AdultSteps =
            [
                "Mostre duas pecas de roupa reais.",
                "Converse sobre clima e atividade do dia.",
                "Peça que a crianca escolha uma peca e explique.",
                "Feche nomeando parte do corpo e funcao da roupa."
            ],
            AdultQuestions =
            [
                "Esta frio ou calor?",
                "Que roupa ajuda hoje?",
                "Para que essa roupa serve?"
            ],
            AcceptableAnswers =
            [
                "Esta frio ou esta calor.",
                "Casaco, camiseta, short ou meia conforme o contexto.",
                "Serve para aquecer, cobrir ou proteger."
            ],
            PracticeTask = "Escolher uma roupa real e dizer em uma frase por que ela combina com o dia.",
            CompletionDefinition = "Marque como concluida quando a crianca fizer uma escolha coerente de roupa e explicar com suas palavras."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPreschoolFirstThenPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia inicial • 3 a 4 anos • first-then para iniciar sem travar",
            UnitTitle = "Primeiro isso, depois aquilo",
            UnitSummary = "A crianca aprende a comecar uma tarefa curta sabendo exatamente o que vem depois.",
            OpeningForAdult = $"Diga: \"{firstName}, primeiro a gente faz esta parte. Depois vem a parte gostosa de terminar e guardar.\"",
            AnchorQuestion = "O que vem primeiro e o que vem depois?",
            CoreMaterialTitle = "Cartao de hoje",
            CoreMaterialParagraphs =
            [
                "Primeiro: colar 3 figuras.",
                "Depois: guardar os lapis no pote."
            ],
            AdultSteps =
            [
                "Mostre o cartao first-then.",
                "Leia os dois passos em voz alta.",
                "Acompanhe a primeira etapa com ajuda leve.",
                "Feche mostrando que o segundo passo tambem tem fim claro."
            ],
            AdultQuestions =
            [
                "Qual parte vem primeiro?",
                "O que acontece depois?",
                "Ja podemos marcar a primeira parte?"
            ],
            AcceptableAnswers =
            [
                "Primeiro colar as figuras.",
                "Depois guardar os lapis.",
                "A crianca reconhece a ordem e conclui a microsequencia."
            ],
            PracticeTask = "Executar uma rotina curta em dois passos e apontar a ordem antes de comecar.",
            CompletionDefinition = "Marque como concluida quando a crianca seguir a ordem first-then com no maximo um lembrete."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBarModelPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 2o ano • problema com desenho de barras • pensar antes da conta",
            UnitTitle = "Barra para enxergar o problema",
            UnitSummary = "A crianca usa desenho simples para entender parte, total e diferenca antes de calcular.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje nao vamos correr para a conta. Primeiro vamos desenhar barras para enxergar o problema.\"",
            AnchorQuestion = "O que e o total, o que e uma parte e o que ainda falta descobrir?",
            CoreMaterialTitle = "Problema da licao",
            CoreMaterialParagraphs =
            [
                "Ana tem 12 figurinhas.",
                "4 figurinhas sao de animais.",
                "Quantas figurinhas nao sao de animais?"
            ],
            AdultSteps =
            [
                "Desenhe uma barra inteira representando 12.",
                "Marque uma parte da barra como 4.",
                "Mostre que a parte que sobra e a resposta procurada.",
                "So depois resolva a conta com a crianca."
            ],
            AdultQuestions =
            [
                "Qual numero representa o total?",
                "Qual parte ja conhecemos?",
                "Que conta ajuda a descobrir a parte que falta?"
            ],
            AcceptableAnswers =
            [
                "O total e 12.",
                "A parte conhecida e 4.",
                "Podemos fazer 12 - 4 = 8."
            ],
            PracticeTask = "Desenhar a barra e resolver a subtracao depois de identificar total e parte.",
            CompletionDefinition = "Marque como concluida quando a crianca usar a barra para explicar o problema e chegar a resposta."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMiniResearchPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 2o ano • pequena pesquisa guiada",
            UnitTitle = "Descobrir 3 fatos sem se perder",
            UnitSummary = "A crianca aprende a pesquisar um tema pequeno e guardar so tres fatos realmente importantes.",
            OpeningForAdult = "Diga: \"Hoje vamos pesquisar um tema pequeno. Nosso objetivo nao e juntar muita coisa, e sair com tres fatos bons.\"",
            AnchorQuestion = "Quais 3 fatos voce descobriu sobre esse tema?",
            CoreMaterialTitle = "Tema da licao: Beija-flor",
            CoreMaterialParagraphs =
            [
                "O beija-flor e um passaro pequeno.",
                "Ele se alimenta do nectar das flores.",
                "Suas asas batem muito rapido."
            ],
            AdultSteps =
            [
                "Leia um texto curto ou mostre tres informacoes prontas.",
                "Peça que a crianca escolha os tres fatos que mais importam.",
                "Anote um fato por linha.",
                "Feche com uma apresentacao oral de 20 segundos."
            ],
            AdultQuestions =
            [
                "Sobre o que estamos pesquisando?",
                "Qual fato voce achou mais interessante?",
                "Voce consegue repetir os tres fatos em ordem?"
            ],
            AcceptableAnswers =
            [
                "Estamos pesquisando o beija-flor.",
                "Ele come nectar, e pequeno e bate as asas rapido.",
                "A crianca consegue repetir tres fatos essenciais."
            ],
            PracticeTask = "Escrever ou desenhar tres fatos em uma ficha curta de pesquisa.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir selecionar e apresentar tres fatos sobre o tema."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPlanDoRegisterPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 2o ano • planejar, fazer e registrar",
            UnitTitle = "Tres momentos da tarefa",
            UnitSummary = "A crianca aprende a olhar a tarefa em tres momentos claros: antes, durante e depois.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos pensar em tres partes: o que vou fazer, como vou fazer e o que vou registrar no final.\"",
            AnchorQuestion = "Qual e o plano, o que voce vai fazer agora e o que fica registrado no final?",
            CoreMaterialTitle = "Quadro da licao",
            CoreMaterialParagraphs =
            [
                "Plano: ler a missao.",
                "Fazer: executar a atividade principal.",
                "Registrar: guardar uma frase, conta ou desenho do que foi feito."
            ],
            AdultSteps =
            [
                "Leia o quadro antes da atividade.",
                "Peça que a crianca diga o plano com a propria boca.",
                "Execute a missao principal.",
                "Feche com um registro pequeno no papel."
            ],
            AdultQuestions =
            [
                "Qual e o plano de hoje?",
                "O que voce esta fazendo agora?",
                "O que vai ficar registrado no final?"
            ],
            AcceptableAnswers =
            [
                "Meu plano e ler a missao e fazer a atividade.",
                "Agora eu estou executando a tarefa.",
                "No final fica uma frase, um desenho ou uma conta."
            ],
            PracticeTask = "Usar o quadro plan-do-register em uma licao do dia e fazer um registro pequeno no final.",
            CompletionDefinition = "Marque como concluida quando a crianca conseguir dizer o plano, executar a tarefa e deixar um registro final."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMathStrategyJustificationPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 3o ano • escolher estrategia e justificar",
            UnitTitle = "Nao basta acertar: precisa explicar",
            UnitSummary = "A crianca resolve um problema e aprende a contar qual estrategia usou para chegar na resposta.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce vai resolver a conta e depois me explicar o caminho usado.\"",
            AnchorQuestion = "Que estrategia voce usou e por que ela funcionou?",
            CoreMaterialTitle = "Problema da licao",
            CoreMaterialParagraphs =
            [
                "Uma caixa tem 24 lapis.",
                "Eles vao ser colocados em 6 potes com a mesma quantidade.",
                "Quantos lapis ficam em cada pote?"
            ],
            AdultSteps =
            [
                "Leia o problema inteiro antes de qualquer conta.",
                "Pergunte se a situacao parece divisao, soma repetida ou desenho.",
                "Deixe a crianca escolher uma estrategia.",
                "Peça explicacao oral depois da resposta."
            ],
            AdultQuestions =
            [
                "Que operacao combina com esse problema?",
                "Como voce pensou antes de responder?",
                "Como provar que a resposta esta certa?"
            ],
            AcceptableAnswers =
            [
                "A operacao e divisao ou reparticao igual.",
                "24 dividido por 6 da 4.",
                "Da para provar montando 6 grupos com 4."
            ],
            PracticeTask = "Resolver o problema e escrever uma frase: Eu usei esta estrategia porque...",
            CompletionDefinition = "Marque como concluida quando a crianca acertar a resposta e explicar a estrategia usada."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildGuidedStudyNotesPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 3o ano • anotar sem copiar tudo",
            UnitTitle = "Notas guiadas em 3 linhas",
            UnitSummary = "A crianca aprende a registrar o essencial de um estudo sem encher a folha com copia sem sentido.",
            OpeningForAdult = "Diga: \"Hoje vamos estudar um tema e anotar so o que realmente precisa ficar.\"",
            AnchorQuestion = "Se eu pudesse guardar so 3 linhas deste estudo, o que entraria nelas?",
            CoreMaterialTitle = "Tema da licao: Ciclo da agua",
            CoreMaterialParagraphs =
            [
                "A agua evapora com o calor.",
                "Depois vira gotinhas nas nuvens.",
                "Por fim, cai em forma de chuva."
            ],
            AdultSteps =
            [
                "Leia o texto curto inteiro.",
                "Peça que a crianca diga as tres ideias principais.",
                "Anote uma ideia por linha.",
                "Feche com um desenho simples do processo."
            ],
            AdultQuestions =
            [
                "Qual e a primeira parte do processo?",
                "O que acontece nas nuvens?",
                "Como a agua volta para o solo?"
            ],
            AcceptableAnswers =
            [
                "Primeiro a agua evapora.",
                "Depois se junta em gotinhas nas nuvens.",
                "No final cai como chuva."
            ],
            PracticeTask = "Fazer tres linhas de anotacao e um desenho simples do tema estudado.",
            CompletionDefinition = "Marque como concluida quando a crianca resumir o estudo em tres ideias claras, sem copiar o texto inteiro."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildIndependenceCloseoutPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 3o ano • fechamento da rotina com revisao leve",
            UnitTitle = "Antes de levantar, conferir",
            UnitSummary = "A crianca aprende a encerrar a propria rotina olhando se terminou, guardou e registrou o essencial.",
            OpeningForAdult = $"Diga: \"{firstName}, antes de sair da mesa vamos fazer a revisao final da tarefa.\"",
            AnchorQuestion = "O que falta terminar, guardar ou registrar antes de encerrar?",
            CoreMaterialTitle = "Checklist de saida",
            CoreMaterialParagraphs =
            [
                "1. Terminei a atividade?",
                "2. Guardei o material?",
                "3. Ficou um registro do que aprendi?"
            ],
            AdultSteps =
            [
                "Leia o checklist no fim da aula.",
                "Peça que a crianca responda item por item.",
                "Corrija so o item que realmente faltou.",
                "Use o mesmo checklist em outros dias."
            ],
            AdultQuestions =
            [
                "A atividade terminou mesmo?",
                "Tudo foi guardado?",
                "O que ficou registrado desta licao?"
            ],
            AcceptableAnswers =
            [
                "Sim, a atividade foi terminada.",
                "O material ja esta guardado.",
                "Ficou uma conta, frase, desenho ou ficha curta."
            ],
            PracticeTask = "Responder ao checklist no fim da licao e ajustar o que faltar.",
            CompletionDefinition = "Marque como concluida quando a crianca encerrar a rotina conferindo os tres itens de saida."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildRhymeBasketPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Pre-alfabetizacao • 3 a 4 anos • rima e escuta sonora",
            UnitTitle = "Cesta das palavras que combinam",
            UnitSummary = "A crianca brinca com sons parecidos e percebe rimas sem precisar ler.",
            OpeningForAdult = "Diga: \"Hoje vamos ouvir palavras que combinam no final.\"",
            AnchorQuestion = "Qual palavra combina com outra no som do final?",
            CoreMaterialTitle = "Pares da licao",
            CoreMaterialParagraphs =
            [
                "Pato combina com gato.",
                "Bola nao combina com sapato.",
                "Mao pode combinar com pao."
            ],
            AdultSteps =
            [
                "Fale duas palavras por vez.",
                "Peça que a crianca diga se combinam ou nao.",
                "Repita as palavras alongando o final.",
                "Feche com um par criado pela propria crianca."
            ],
            AdultQuestions =
            [
                "Pato combina com gato?",
                "Mao combina com pao?",
                "Voce lembra de mais um par parecido?"
            ],
            AcceptableAnswers =
            [
                "Sim, pato combina com gato.",
                "Sim, mao combina com pao.",
                "A crianca produz ou reconhece um novo par."
            ],
            PracticeTask = "Separar ou repetir tres pares de palavras que rimam.",
            CompletionDefinition = "Marque como concluida quando a crianca reconhecer ao menos dois pares de rima."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPatternPathPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica inicial • 3 a 4 anos • padrao e repeticao",
            UnitTitle = "Caminho de cores e formas",
            UnitSummary = "A crianca percebe o que se repete e consegue continuar um padrao simples.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos descobrir o segredo da sequencia para continuar o caminho sem errar.\"",
            AnchorQuestion = "O que esta se repetindo nesta sequencia?",
            CoreMaterialTitle = "Padrao da licao",
            CoreMaterialParagraphs =
            [
                "Vermelho, azul, vermelho, azul...",
                "Circulo, quadrado, circulo, quadrado..."
            ],
            AdultSteps =
            [
                "Monte uma sequencia curta com duas cores ou duas formas.",
                "Peça que a crianca observe antes de tocar.",
                "Pergunte o que se repete.",
                "Deixe que ela continue a sequencia sozinha."
            ],
            AdultQuestions =
            [
                "Qual cor vem depois?",
                "O que esta se repetindo?",
                "Voce consegue montar mais duas partes?"
            ],
            AcceptableAnswers =
            [
                "Depois do vermelho vem azul.",
                "As cores ou formas repetem na mesma ordem.",
                "A crianca consegue continuar a sequencia."
            ],
            PracticeTask = "Continuar uma sequencia de cor ou forma com pelo menos quatro repeticoes.",
            CompletionDefinition = "Marque como concluida quando a crianca identificar o padrao e continuar a sequencia corretamente."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBibleCreationColorsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real e repertorio biblico • 3 a 5 anos • cores da criacao",
            UnitTitle = "Cores que vemos no mundo",
            UnitSummary = "A crianca observa cores da natureza e liga essa observacao a uma conversa simples sobre criacao e cuidado.",
            OpeningForAdult = "Diga: \"Hoje vamos procurar cores no mundo e lembrar que ele e cheio de beleza e variedade.\"",
            AnchorQuestion = "Que cor voce encontrou e onde ela apareceu?",
            CoreMaterialTitle = "Busca de cores",
            CoreMaterialParagraphs =
            [
                "Procure algo verde, azul e amarelo.",
                "Pode ser planta, ceu, flor, fruta ou desenho.",
                "Depois conte o que mais chamou sua atencao."
            ],
            AdultSteps =
            [
                "Escolha duas ou tres cores para procurar.",
                "Peça que a crianca aponte exemplos reais ou em figuras.",
                "Converse sobre onde aquela cor apareceu.",
                "Feche com desenho ou pintura."
            ],
            AdultQuestions =
            [
                "Que cor voce encontrou primeiro?",
                "Onde voce viu essa cor?",
                "Qual cor foi a sua preferida hoje?"
            ],
            AcceptableAnswers =
            [
                "Verde na folha ou planta.",
                "Azul no ceu ou em um desenho.",
                "A crianca nomeia pelo menos duas cores com um exemplo."
            ],
            PracticeTask = "Desenhar ou pintar tres coisas vistas nas cores da licao.",
            CompletionDefinition = "Marque como concluida quando a crianca localizar cores e relaciona-las a coisas reais."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPrayBreatheBeginPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia e regulacao • 3 a 5 anos • acalmar antes de iniciar",
            UnitTitle = "Parar, respirar e comecar",
            UnitSummary = "A crianca aprende um micro-ritual de entrada para reduzir agitacao antes da licao.",
            OpeningForAdult = $"Diga: \"{firstName}, antes de comecar vamos parar, respirar duas vezes e abrir a primeira parte juntos.\"",
            AnchorQuestion = "O que fazemos antes de comecar a licao?",
            CoreMaterialTitle = "Ritual da licao",
            CoreMaterialParagraphs =
            [
                "1. Mãos quietas.",
                "2. Duas respiracoes lentas.",
                "3. Abrir a primeira atividade."
            ],
            AdultSteps =
            [
                "Modele as duas respiracoes com o corpo.",
                "Peça que a crianca repita sem pressa.",
                "So depois abra a atividade.",
                "Repita o mesmo ritual nos proximos dias."
            ],
            AdultQuestions =
            [
                "Qual e o primeiro passo?",
                "Quantas respiracoes vamos fazer?",
                "O que acontece depois disso?"
            ],
            AcceptableAnswers =
            [
                "Primeiro as maos ficam quietas.",
                "Fazemos duas respiracoes.",
                "Depois abrimos a atividade."
            ],
            PracticeTask = "Executar o micro-ritual completo antes da primeira licao do dia.",
            CompletionDefinition = "Marque como concluida quando a crianca fizer o ritual de entrada e iniciar a licao com mais calma."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildSyllableBasketPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Alfabetizacao inicial • 5 a 6 anos • silabas e palavras conhecidas",
            UnitTitle = "Quebrar palavras em pedacos",
            UnitSummary = "A crianca ouve palavras do cotidiano, bate palmas para as silabas e começa a perceber sua estrutura.",
            OpeningForAdult = "Diga: \"Hoje vamos ouvir palavras e sentir seus pedacos com palmas.\"",
            AnchorQuestion = "Quantos pedacos voce escuta nesta palavra?",
            CoreMaterialTitle = "Palavras da licao",
            CoreMaterialParagraphs =
            [
                "Bo-la",
                "Ca-sa",
                "Ma-ca-co"
            ],
            AdultSteps =
            [
                "Fale a palavra devagar.",
                "Bata palmas junto com a crianca para cada pedaco sonoro.",
                "Repita com tres palavras conhecidas.",
                "Feche escolhendo a palavra com mais pedacos."
            ],
            AdultQuestions =
            [
                "Quantos pedacos ha em bola?",
                "Casa tem mais ou menos pedacos que macaco?",
                "Qual palavra teve mais palmas?"
            ],
            AcceptableAnswers =
            [
                "Bola tem 2 pedacos.",
                "Casa tem 2 e macaco tem 3.",
                "Macaco teve mais palmas."
            ],
            PracticeTask = "Bater palmas para tres palavras e registrar o numero de pedacos com bolinhas ou tracos.",
            CompletionDefinition = "Marque como concluida quando a crianca separar oralmente palavras simples em silabas com apoio."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildDecomposeTenPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 1o ano • decomposicao do 10 • parte e todo",
            UnitTitle = "Formas de fazer 10",
            UnitSummary = "A crianca percebe que o numero 10 pode ser montado de jeitos diferentes com apoio visual.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos descobrir quantos jeitos existem de formar 10.\"",
            AnchorQuestion = "Que duas partes juntam 10?",
            CoreMaterialTitle = "Quadro de 10",
            CoreMaterialParagraphs =
            [
                "5 e 5 formam 10.",
                "6 e 4 tambem formam 10.",
                "7 e 3 e outro jeito."
            ],
            AdultSteps =
            [
                "Use quadro de 10 ou desenho com bolinhas.",
                "Monte uma combinacao e leia em voz alta.",
                "Peça outra combinacao diferente.",
                "Registre duas ou tres decomposicoes no caderno."
            ],
            AdultQuestions =
            [
                "Que numero falta para completar 10 com 6?",
                "E se eu tiver 7, quanto falta?",
                "Voce consegue achar outro jeito?"
            ],
            AcceptableAnswers =
            [
                "Faltam 4.",
                "Faltam 3.",
                "Pode usar 5 + 5, 8 + 2 ou 9 + 1."
            ],
            PracticeTask = "Montar e registrar tres combinacoes diferentes que somam 10.",
            CompletionDefinition = "Marque como concluida quando a crianca descobrir e registrar mais de uma decomposicao do 10."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildOpeningBoardPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 1o ano • abrir o estudo com clareza",
            UnitTitle = "Quadro de abertura da aula",
            UnitSummary = "A crianca olha o quadro, entende a ordem do dia e começa a primeira parte sem adivinhacao.",
            OpeningForAdult = $"Diga: \"{firstName}, antes de comecar vamos olhar o quadro do dia para saber a ordem certa.\"",
            AnchorQuestion = "Qual e a primeira missao do quadro de hoje?",
            CoreMaterialTitle = "Quadro da aula",
            CoreMaterialParagraphs =
            [
                "1. Leitura curta.",
                "2. Atividade principal.",
                "3. Fechamento e guardar."
            ],
            AdultSteps =
            [
                "Leia os tres blocos do quadro.",
                "Peça que a crianca repita o primeiro bloco.",
                "Comece so a primeira missao.",
                "Volte ao quadro na hora de trocar de etapa."
            ],
            AdultQuestions =
            [
                "O que vem primeiro?",
                "Qual e a atividade principal?",
                "O que fazemos no final?"
            ],
            AcceptableAnswers =
            [
                "Primeiro a leitura curta.",
                "Depois a atividade principal.",
                "No final vem o fechamento e guardar."
            ],
            PracticeTask = "Usar o quadro do dia para conduzir pelo menos duas licoes em ordem.",
            CompletionDefinition = "Marque como concluida quando a crianca reconhecer a ordem do quadro e iniciar a primeira missao com menos ajuda."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCopyworkVersePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem do 1o ano • copia guiada com significado",
            UnitTitle = "Copiar com sentido, nao por cansaco",
            UnitSummary = "A crianca le uma frase curta, entende o que ela diz e so depois copia com atencao.",
            OpeningForAdult = "Diga: \"Hoje voce vai copiar uma frase curtinha, mas primeiro vamos entender o que ela esta dizendo.\"",
            AnchorQuestion = "O que esta frase quer dizer antes de voce copiar?",
            CoreMaterialTitle = "Frase da licao",
            CoreMaterialParagraphs =
            [
                "\"A luz alegra o dia.\"",
                "Explique com palavras simples: a luz deixa o dia claro e bonito."
            ],
            AdultSteps =
            [
                "Leia a frase em voz alta.",
                "Peça que a crianca repita com voce.",
                "Explique o sentido em linguagem simples.",
                "So depois peça a copia com letra caprichada."
            ],
            AdultQuestions =
            [
                "Que palavra aparece primeiro?",
                "O que a frase quer dizer?",
                "Voce consegue copiar prestando atencao nos espacos?"
            ],
            AcceptableAnswers =
            [
                "A palavra primeiro e luz.",
                "Quer dizer que a luz deixa o dia alegre ou claro.",
                "A copia pode ter apoio, mas com sentido."
            ],
            PracticeTask = "Copiar a frase e desenhar uma pequena imagem ligada ao sentido dela.",
            CompletionDefinition = "Marque como concluida quando a crianca entender a frase e fizer a copia com cuidado."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMeasureHomeRacePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 1o ano • medida e comparacao",
            UnitTitle = "Maior, menor e mais comprido",
            UnitSummary = "A crianca compara comprimentos usando objetos da casa antes de depender de regua.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos comparar tamanhos e descobrir qual objeto e maior, menor ou mais comprido.\"",
            AnchorQuestion = "Qual objeto e mais comprido e como voce percebeu isso?",
            CoreMaterialTitle = "Corrida de medidas",
            CoreMaterialParagraphs =
            [
                "Escolha um lapis, uma colher e um livro fino.",
                "Coloque os objetos lado a lado pela mesma ponta.",
                "Observe qual passa mais longe."
            ],
            AdultSteps =
            [
                "Alinhe os objetos pela mesma borda.",
                "Peça que a crianca observe antes de responder.",
                "Pergunte qual e maior e qual e menor.",
                "Se quiser, registre com desenho e setas."
            ],
            AdultQuestions =
            [
                "Qual objeto vai mais longe?",
                "Qual e o menor?",
                "Como voce comparou?"
            ],
            AcceptableAnswers =
            [
                "O objeto que passa mais longe e o mais comprido.",
                "O menor e o que termina antes.",
                "Comparei colocando lado a lado."
            ],
            PracticeTask = "Comparar tres objetos e registrar maior e menor em desenho ou frase curta.",
            CompletionDefinition = "Marque como concluida quando a crianca comparar comprimentos com criterio correto."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBiographyTimelinePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Historia e linguagem • 1o ano • ordem dos acontecimentos",
            UnitTitle = "Vida em tres momentos",
            UnitSummary = "A crianca aprende que uma biografia simples pode ser organizada em inicio, meio e etapa seguinte.",
            OpeningForAdult = "Diga: \"Hoje vamos conhecer uma pessoa em tres momentos da vida e colocar tudo na ordem certa.\"",
            AnchorQuestion = "O que veio primeiro, depois e por ultimo nessa historia de vida?",
            CoreMaterialTitle = "Biografia curta: Tarsila em tres momentos",
            CoreMaterialParagraphs =
            [
                "Quando pequena, Tarsila gostava de desenhar.",
                "Mais tarde, estudou arte com muito interesse.",
                "Depois ficou conhecida por suas pinturas brasileiras."
            ],
            AdultSteps =
            [
                "Leia os tres fatos da biografia.",
                "Peça que a crianca coloque em ordem.",
                "Monte uma linha do tempo com tres caixas.",
                "Feche com um reconto oral breve."
            ],
            AdultQuestions =
            [
                "O que ela gostava de fazer quando pequena?",
                "O que aconteceu depois?",
                "Como a historia termina?"
            ],
            AcceptableAnswers =
            [
                "Ela gostava de desenhar.",
                "Depois estudou arte.",
                "Mais tarde ficou conhecida por suas pinturas."
            ],
            PracticeTask = "Montar uma linha do tempo com tres quadrinhos e uma palavra em cada um.",
            CompletionDefinition = "Marque como concluida quando a crianca ordenar os tres fatos e recontar a sequencia."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildThreeMissionsPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 1o ao 2o ano • quadro de progresso simples",
            UnitTitle = "Tres missoes do dia",
            UnitSummary = "A crianca enxerga o dia em tres partes pequenas e acompanha o proprio progresso.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje a aula tem tres missoes. Vamos fechar uma de cada vez.\"",
            AnchorQuestion = "Qual missao voce fechou e qual ainda falta?",
            CoreMaterialTitle = "Quadro de missoes",
            CoreMaterialParagraphs =
            [
                "Missao 1: leitura.",
                "Missao 2: atividade principal.",
                "Missao 3: registro e guardar."
            ],
            AdultSteps =
            [
                "Mostre as tres missoes antes de comecar.",
                "Marque a primeira quando terminar.",
                "Volte ao quadro antes de subir para a proxima.",
                "Feche com uma fala curta sobre o que foi concluido."
            ],
            AdultQuestions =
            [
                "Qual missao terminou?",
                "Qual missao estamos fazendo agora?",
                "Qual vai ficar para o final?"
            ],
            AcceptableAnswers =
            [
                "Terminei a leitura ou a atividade principal.",
                "Agora estamos na missao atual.",
                "No fim fica o registro e guardar."
            ],
            PracticeTask = "Usar o quadro de 3 missoes durante a aula e marcar cada parte concluida.",
            CompletionDefinition = "Marque como concluida quando a crianca acompanhar pelo menos duas trocas de missao olhando para o quadro."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildNameTracePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Pre-alfabetizacao • 3 a 4 anos • nome proprio e letra inicial",
            UnitTitle = "Meu nome com letra grande",
            UnitSummary = "A crianca reconhece a inicial do proprio nome e participa do tracado grande com significado afetivo.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje a nossa palavra mais importante e o seu nome.\"",
            AnchorQuestion = "Qual e a primeira letra do seu nome?",
            CoreMaterialTitle = "Nome da licao",
            CoreMaterialParagraphs =
            [
                $"Escreva {firstName} bem grande.",
                "Circule a primeira letra.",
                "Mostre como essa letra aparece em outras palavras."
            ],
            AdultSteps =
            [
                "Leia o nome da crianca em voz alta.",
                "Aponte a primeira letra e diga o som.",
                "Peça que a crianca passe o dedo por cima.",
                "Feche com tracado grande da inicial."
            ],
            AdultQuestions =
            [
                "Qual e a primeira letra?",
                "Que som ela faz?",
                "Voce consegue desenhar essa letra grande?"
            ],
            AcceptableAnswers =
            [
                "A crianca identifica a inicial do proprio nome.",
                "Repete o som com ajuda do adulto.",
                "Participa do tracado grande."
            ],
            PracticeTask = "Passar o dedo pelo nome e fazer a inicial em folha grande ou no ar.",
            CompletionDefinition = "Marque como concluida quando a crianca reconhecer a inicial do proprio nome e tracar essa letra com apoio."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMoreLessPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica inicial • 3 a 4 anos • mais, menos e igual",
            UnitTitle = "Dois cestos para comparar",
            UnitSummary = "A crianca compara quantidade pequena em dois grupos e aprende a equilibrar os conjuntos.",
            OpeningForAdult = $"Diga: \"{firstName}, vamos olhar dois cestos e descobrir qual tem mais, qual tem menos e como podemos deixar igual.\"",
            AnchorQuestion = "Qual cesto tem mais e como deixamos os dois iguais?",
            CoreMaterialTitle = "Cestos da licao",
            CoreMaterialParagraphs =
            [
                "No primeiro cesto, coloque 4 objetos.",
                "No segundo, coloque 2 objetos.",
                "Depois mova itens para deixar igual."
            ],
            AdultSteps =
            [
                "Monte os dois cestos com quantidades diferentes.",
                "Conte cada grupo com a crianca.",
                "Pergunte qual tem mais e qual tem menos.",
                "Mova um item de cada vez ate equilibrar."
            ],
            AdultQuestions =
            [
                "Qual tem mais?",
                "Qual tem menos?",
                "O que podemos fazer para ficar igual?"
            ],
            AcceptableAnswers =
            [
                "O cesto com 4 tem mais.",
                "O cesto com 2 tem menos.",
                "Pode colocar mais itens no menor ou tirar do maior."
            ],
            PracticeTask = "Comparar dois grupos e fazer uma mudanca para deixalos iguais.",
            CompletionDefinition = "Marque como concluida quando a crianca identificar mais e menos e participar de uma igualacao simples."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBibleAnimalPairsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real e repertorio biblico • 3 a 4 anos • pares de animais",
            UnitTitle = "Animais em pares",
            UnitSummary = "A crianca nomeia animais, percebe pares e observa pequenas diferencas visuais.",
            OpeningForAdult = "Diga: \"Hoje vamos juntar animais em pares e reparar no que e igual e diferente neles.\"",
            AnchorQuestion = "Quais animais formam um par e o que voce percebe neles?",
            CoreMaterialTitle = "Pares da licao",
            CoreMaterialParagraphs =
            [
                "Separe duas figuras de passaro, duas de leao e duas de peixe.",
                "Misture tudo e monte os pares."
            ],
            AdultSteps =
            [
                "Mostre as figuras misturadas.",
                "Peça que a crianca monte os pares.",
                "Converse sobre cor, tamanho ou som do animal.",
                "Feche nomeando o animal favorito."
            ],
            AdultQuestions =
            [
                "Quais dois combinam?",
                "Que animal voce encontrou primeiro?",
                "O que chama sua atencao nesse animal?"
            ],
            AcceptableAnswers =
            [
                "A crianca junta as figuras correspondentes.",
                "Nomeia ao menos um animal corretamente.",
                "Aponta uma caracteristica simples, como cor ou som."
            ],
            PracticeTask = "Montar tres pares de animais e nomear pelo menos dois deles.",
            CompletionDefinition = "Marque como concluida quando a crianca formar os pares e nomear animais com apoio."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCleanupBasketPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia inicial • 3 a 4 anos • pegar, usar e guardar",
            UnitTitle = "Tudo volta para o cesto",
            UnitSummary = "A crianca aprende que a atividade tem fim e que guardar faz parte da mesma rotina.",
            OpeningForAdult = $"Diga: \"{firstName}, quando terminamos a atividade, tudo volta para o cesto.\"",
            AnchorQuestion = "O que falta guardar antes de terminar de verdade?",
            CoreMaterialTitle = "Rotina da licao",
            CoreMaterialParagraphs =
            [
                "1. Pegar o material do cesto.",
                "2. Usar na atividade.",
                "3. Guardar tudo no mesmo lugar."
            ],
            AdultSteps =
            [
                "Mostre o cesto no inicio.",
                "Abra a atividade e nomeie o material usado.",
                "Avise com antecedencia que ja vai terminar.",
                "Guarde os itens junto com a crianca."
            ],
            AdultQuestions =
            [
                "De onde tiramos o material?",
                "Onde ele volta quando termina?",
                "Ja podemos encerrar?"
            ],
            AcceptableAnswers =
            [
                "O material veio do cesto.",
                "Ele volta para o mesmo cesto.",
                "So termina quando estiver guardado."
            ],
            PracticeTask = "Fazer uma microatividade e devolver todos os itens ao cesto no final.",
            CompletionDefinition = "Marque como concluida quando a crianca participar do guardar com previsibilidade e menos resistencia."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPictureSequenceSentencePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem do 1o ano • sequencia de imagens • oralidade e frase final",
            UnitTitle = "Olhar, contar e escrever uma frase",
            UnitSummary = "A crianca organiza imagens em ordem, reconstrui a pequena historia e registra uma frase final.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar tres imagens, contar a historia e guardar uma frase final no caderno.\"",
            AnchorQuestion = "O que aconteceu primeiro, depois e no final?",
            CoreMaterialTitle = "Sequencia da licao",
            CoreMaterialParagraphs =
            [
                "Imagem 1: menino planta uma semente.",
                "Imagem 2: a planta cresce.",
                "Imagem 3: ele rega e observa a flor."
            ],
            AdultSteps =
            [
                "Mostre as imagens fora de ordem.",
                "Peça que a crianca organize a sequencia.",
                "Puxe o reconto oral em tres partes.",
                "Escolha uma frase final para registrar."
            ],
            AdultQuestions =
            [
                "Qual imagem vem primeiro?",
                "O que mudou na segunda imagem?",
                "Que frase final resume essa pequena historia?"
            ],
            AcceptableAnswers =
            [
                "Primeiro o menino planta.",
                "Depois a planta cresce.",
                "No final ele cuida da flor."
            ],
            PracticeTask = "Organizar as imagens e escrever ou ditar uma frase final sobre a historia.",
            CompletionDefinition = "Marque como concluida quando a crianca recontar a sequencia corretamente e fechar com uma frase."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCalendarDaysPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica do 1o ano • tempo e calendario",
            UnitTitle = "Ontem, hoje e amanha",
            UnitSummary = "A crianca usa o calendario para ligar contagem e passagem do tempo a acontecimentos reais.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos olhar o calendario para descobrir ontem, hoje, amanha e quantos dias faltam para um combinado.\"",
            AnchorQuestion = "Que dia e hoje e quantos dias faltam para o proximo evento?",
            CoreMaterialTitle = "Calendario da licao",
            CoreMaterialParagraphs =
            [
                "Marque o dia de hoje.",
                "Circule ontem e amanha.",
                "Escolha um evento simples para contar quantos dias faltam."
            ],
            AdultSteps =
            [
                "Mostre o calendario da semana ou do mes.",
                "Aponte hoje, ontem e amanha.",
                "Conte os dias ate um evento real.",
                "Feche com o numero do dia no caderno."
            ],
            AdultQuestions =
            [
                "Que dia e hoje?",
                "Qual foi ontem?",
                "Quantos dias faltam para o evento combinado?"
            ],
            AcceptableAnswers =
            [
                "A crianca localiza o dia de hoje.",
                "Reconhece ontem e amanha com apoio visual.",
                "Conta os dias faltantes com ajuda leve."
            ],
            PracticeTask = "Marcar ontem, hoje e amanha no calendario e contar ate um evento simples.",
            CompletionDefinition = "Marque como concluida quando a crianca localizar dias no calendario e usar a contagem para falar do tempo."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildNeighborhoodMapPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real • 1o ano • localizacao e trajeto conhecido",
            UnitTitle = "Meu caminho no papel",
            UnitSummary = "A crianca transforma um trajeto conhecido em desenho simples, com inicio, caminho e destino.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos desenhar um caminho que voce realmente conhece.\"",
            AnchorQuestion = "Como mostramos no papel o caminho que voce ja conhece?",
            CoreMaterialTitle = "Trajeto da licao",
            CoreMaterialParagraphs =
            [
                "Escolha um caminho conhecido: quarto ate cozinha, portao ate quintal ou casa ate padaria.",
                "Separe dois ou tres pontos de referencia.",
                "Desenhe em linha simples."
            ],
            AdultSteps =
            [
                "Peça que a crianca conte o caminho com a boca.",
                "Escolha tres pontos importantes.",
                "Desenhe o trajeto com setas ou linha.",
                "Peça que a crianca explique o mapa pronto."
            ],
            AdultQuestions =
            [
                "Onde o caminho comeca?",
                "Que lugar aparece no meio?",
                "Onde ele termina?"
            ],
            AcceptableAnswers =
            [
                "A crianca nomeia inicio, meio e destino.",
                "Usa pontos de referencia conhecidos.",
                "Explica o trajeto com certa ordem."
            ],
            PracticeTask = "Desenhar um trajeto conhecido com pelo menos tres pontos de referencia.",
            CompletionDefinition = "Marque como concluida quando a crianca representar um caminho conhecido e explicar o percurso no mapa."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPrepareTablePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 1o ano • preparar o ambiente de estudo",
            UnitTitle = "Mesa pronta antes da licao",
            UnitSummary = "A crianca cria previsibilidade no comeco da aula preparando a mesa do mesmo jeito.",
            OpeningForAdult = $"Diga: \"{firstName}, antes da licao a mesa precisa ficar pronta sempre do mesmo jeito.\"",
            AnchorQuestion = "O que precisa ficar pronto antes da primeira licao?",
            CoreMaterialTitle = "Itens fixos da mesa",
            CoreMaterialParagraphs =
            [
                "1. Caderno.",
                "2. Estojo ou lapis.",
                "3. Material especial do dia."
            ],
            AdultSteps =
            [
                "Mostre os tres itens fixos.",
                "Peça que a crianca coloque tudo na mesa.",
                "Confira uma unica vez.",
                "Comece a licao logo depois."
            ],
            AdultQuestions =
            [
                "Qual item entra primeiro na mesa?",
                "O que falta para ficar pronta?",
                "Agora ja podemos começar?"
            ],
            AcceptableAnswers =
            [
                "A crianca separa os itens essenciais.",
                "Identifica algo que ainda falta.",
                "Relaciona mesa pronta ao inicio da licao."
            ],
            PracticeTask = "Preparar a mesa do mesmo jeito antes de duas tarefas do dia.",
            CompletionDefinition = "Marque como concluida quando a crianca preparar a mesa com mais independencia e menos lembrete."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPsalmEchoPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem e memoria auditiva • 3 a 5 anos • eco oral com desenho",
            UnitTitle = "Escutar, repetir e transformar em imagem",
            UnitSummary = "A crianca ouve uma frase curta, repete e desenha uma imagem ligada ao que ouviu.",
            OpeningForAdult = "Diga: \"Hoje eu vou falar uma frase curtinha, voce vai repetir comigo e depois me mostrar em desenho o que ela lembrou.\"",
            AnchorQuestion = "Que imagem vem a sua mente quando voce escuta essa frase?",
            CoreMaterialTitle = "Frase da licao",
            CoreMaterialParagraphs =
            [
                "\"O Senhor e bom.\"",
                "Repita a frase duas vezes, devagar e com calma."
            ],
            AdultSteps =
            [
                "Leia a frase duas vezes devagar.",
                "Peça o eco da crianca.",
                "Converse brevemente sobre a ideia da frase.",
                "Feche com um desenho ligado ao que ela ouviu."
            ],
            AdultQuestions =
            [
                "Voce consegue repetir comigo?",
                "O que essa frase faz voce lembrar?",
                "Como voce desenharia isso?"
            ],
            AcceptableAnswers =
            [
                "A crianca repete a frase com apoio.",
                "Traz uma imagem simples ligada a bondade, cuidado ou alegria.",
                "Representa a ideia em desenho."
            ],
            PracticeTask = "Repetir a frase e desenhar uma cena ou simbolo ligado a ela.",
            CompletionDefinition = "Marque como concluida quando a crianca repetir a frase e produzir um desenho coerente com a ideia principal."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPairsArkAnimalsPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica inicial • 3 a 5 anos • pares e contagem concreta",
            UnitTitle = "Animais em duplas",
            UnitSummary = "A crianca monta pares, conta duplas e percebe quando um animal fica sem companheiro.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos montar pares de animais e contar quantas duplas conseguimos formar.\"",
            AnchorQuestion = "Quantos pares montamos e sobrou algum animal sem par?",
            CoreMaterialTitle = "Pares da licao",
            CoreMaterialParagraphs =
            [
                "Separe figuras ou brinquedos de animais.",
                "Monte duplas iguais.",
                "Conte os pares devagar."
            ],
            AdultSteps =
            [
                "Misture as figuras de animais.",
                "Peça que a crianca monte os pares.",
                "Conte as duplas formadas.",
                "Se sobrar um animal, converse sobre o que falta para completar."
            ],
            AdultQuestions =
            [
                "Quantos pares montamos?",
                "Sobrou algum animal sozinho?",
                "O que falta para completar esse par?"
            ],
            AcceptableAnswers =
            [
                "A crianca conta os pares montados.",
                "Reconhece quando algo ficou sem par.",
                "Aponta o animal que falta para completar a dupla."
            ],
            PracticeTask = "Montar pares de animais e dizer quantas duplas existem.",
            CompletionDefinition = "Marque como concluida quando a crianca formar pares e usar contagem para falar das duplas."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBibleSequenceStoryPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem do 1o ano • reconto visual • historia em tres cenas",
            UnitTitle = "Tres cenas para contar melhor",
            UnitSummary = "A crianca organiza uma historia conhecida em tres partes e fortalece oralidade e sequencia.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar tres cenas de uma historia conhecida e colocar tudo na ordem certa.\"",
            AnchorQuestion = "O que aconteceu primeiro, depois e no final nessa historia?",
            CoreMaterialTitle = "Historia da licao: o menino e os paes",
            CoreMaterialParagraphs =
            [
                "Cena 1: o menino entrega os paes e peixes.",
                "Cena 2: as pessoas se organizam para comer.",
                "Cena 3: todos se alimentam."
            ],
            AdultSteps =
            [
                "Mostre as tres cenas fora de ordem.",
                "Peça que a crianca reorganize.",
                "Converse sobre cada cena em uma frase.",
                "Feche com uma frase simples sobre o que aconteceu."
            ],
            AdultQuestions =
            [
                "Qual cena vem primeiro?",
                "O que acontece no meio?",
                "Como a historia termina?"
            ],
            AcceptableAnswers =
            [
                "Primeiro o menino entrega os alimentos.",
                "Depois as pessoas se organizam.",
                "No final todos comem."
            ],
            PracticeTask = "Ordenar as cenas e registrar uma frase final da historia.",
            CompletionDefinition = "Marque como concluida quando a crianca organizar as tres cenas e recontar a historia em ordem."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBrazilSymbolsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 2o ano • simbolos nacionais e pertencimento",
            UnitTitle = "Bandeira, simbolos e identidade",
            UnitSummary = "A crianca conhece simbolos do Brasil e relaciona esse estudo ao lugar onde vive.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar simbolos do Brasil e pensar no que eles representam para a nossa vida.\"",
            AnchorQuestion = "O que representa o Brasil para voce quando olha esses simbolos?",
            CoreMaterialTitle = "Simbolos da licao",
            CoreMaterialParagraphs =
            [
                "A bandeira e um dos simbolos mais conhecidos do Brasil.",
                "As cores e formas ajudam a identificar o pais.",
                "Outros simbolos podem aparecer em hino, brasao e datas importantes."
            ],
            AdultSteps =
            [
                "Mostre a bandeira e mais dois simbolos simples.",
                "Converse sobre o que a crianca reconhece primeiro.",
                "Peça um registro em desenho ou frase curta.",
                "Feche ligando o simbolo ao lugar onde a crianca vive."
            ],
            AdultQuestions =
            [
                "Que simbolo voce reconhece primeiro?",
                "Que cores voce ve na bandeira?",
                "Por que esses simbolos ajudam a falar do Brasil?"
            ],
            AcceptableAnswers =
            [
                "A bandeira costuma ser reconhecida primeiro.",
                "A crianca nomeia as cores mais visiveis.",
                "Os simbolos representam o pais e ajudam a identifica-lo."
            ],
            PracticeTask = "Desenhar a bandeira ou outro simbolo e escrever uma frase simples sobre o Brasil.",
            CompletionDefinition = "Marque como concluida quando a crianca reconhecer simbolos basicos do Brasil e registrar ao menos uma ideia sobre eles."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildFamilyStudyHelperPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia do 2o ao 4o ano • responsabilidade dentro da rotina",
            UnitTitle = "Minha funcao no estudo de hoje",
            UnitSummary = "A crianca recebe uma funcao simples e aprende a participar do funcionamento da rotina, nao so do conteudo.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce tem uma funcao importante na rotina de estudo.\"",
            AnchorQuestion = "Qual e sua funcao hoje e como voce vai cumpri-la?",
            CoreMaterialTitle = "Funcao da licao",
            CoreMaterialParagraphs =
            [
                "Opcao 1: separar materiais.",
                "Opcao 2: conferir o calendario.",
                "Opcao 3: marcar a licao concluida no quadro."
            ],
            AdultSteps =
            [
                "Escolha uma unica funcao do dia.",
                "Explique claramente comeco e fim dessa responsabilidade.",
                "Deixe a crianca executar antes de intervir.",
                "Feche reconhecendo o que foi feito."
            ],
            AdultQuestions =
            [
                "Qual e a sua funcao hoje?",
                "O que voce precisa fazer para cumpri-la?",
                "Como saberemos que terminou?"
            ],
            AcceptableAnswers =
            [
                "A crianca nomeia a funcao do dia.",
                "Explica os passos basicos para executa-la.",
                "Reconhece quando ela foi concluida."
            ],
            PracticeTask = "Executar uma funcao da rotina e explicar em uma frase como ela ajudou a aula a acontecer.",
            CompletionDefinition = "Marque como concluida quando a crianca assumir uma pequena responsabilidade e leva-la ate o fim."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildStoryPictureTalkPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem em casa • 3 a 4 anos • imagem, escuta e fala guiada",
            UnitTitle = "Olhar, ouvir e contar com as próprias palavras",
            UnitSummary = "A criança observa uma imagem simples, ouve uma mini-história e responde com frases curtas sobre o que entendeu.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar uma imagem, ouvir uma história curtinha e depois você vai me contar quem apareceu e o que aconteceu.\"",
            AnchorQuestion = "Quem apareceu na história e o que aconteceu com ele?",
            CoreMaterialTitle = "Imagem da lição: A menina e o passarinho",
            CoreMaterialParagraphs =
            [
                "Uma menina abriu a janela logo cedo.",
                "Ela viu um passarinho pousado perto do vaso de flores.",
                "A menina ficou quietinha para observar o passarinho por mais tempo."
            ],
            AdultSteps =
            [
                "Mostre a imagem antes de contar a história.",
                "Conte as três frases com voz calma.",
                "Peça que a criança aponte quem apareceu e diga o que aconteceu.",
                "Feche com um desenho do detalhe de que ela mais gostou."
            ],
            AdultQuestions =
            [
                "Quem apareceu na história?",
                "Onde estava o passarinho?",
                "O que a menina fez no final?"
            ],
            AcceptableAnswers =
            [
                "A menina e o passarinho.",
                "Perto do vaso de flores.",
                "Ela ficou observando quietinha."
            ],
            PracticeTask = "Desenhar a janela, a menina ou o passarinho e contar em uma frase o que aconteceu.",
            CompletionDefinition = "Marque como concluída quando a criança conseguir responder às perguntas principais e relacionar a fala com a imagem."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildShapesHouseHuntPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matemática inicial • 3 a 4 anos • formas no cotidiano",
            UnitTitle = "Formas que já moram dentro da casa",
            UnitSummary = "A criança procura círculo, quadrado e triângulo em objetos reais, em vez de ver forma só no papel.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos achar formas escondidas pela casa. Primeiro um círculo, depois um quadrado e por último um triângulo.\"",
            AnchorQuestion = "Onde encontramos um círculo, um quadrado e um triângulo por aqui?",
            CoreMaterialTitle = "Missão da lição",
            CoreMaterialParagraphs =
            [
                "Procure um objeto redondo, um objeto quadrado e um objeto com ponta de triângulo.",
                "Nomeie cada forma antes de passar para a próxima.",
                "No final, escolha a forma favorita para desenhar."
            ],
            AdultSteps =
            [
                "Mostre as três formas em desenho rápido.",
                "Caminhe pela casa com a criança procurando exemplos.",
                "Pare em cada objeto para repetir o nome da forma.",
                "Feche com um contorno ou desenho simples."
            ],
            AdultQuestions =
            [
                "Qual objeto ficou redondo como um círculo?",
                "Qual ficou parecido com um quadrado?",
                "Onde apareceu o triângulo?"
            ],
            AcceptableAnswers =
            [
                "Prato, tampa ou relógio.",
                "Caixa, azulejo ou almofada.",
                "Telhado de brinquedo, desenho ou objeto com ponta triangular."
            ],
            PracticeTask = "Escolher uma forma e desenhar um objeto da casa que combine com ela.",
            CompletionDefinition = "Marque como concluída quando a criança reconhecer ao menos duas formas em objetos reais e nomeá-las."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildWeatherWindowPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real • 3 a 4 anos • clima, rotina e observação",
            UnitTitle = "Olhar o dia antes de começar",
            UnitSummary = "A criança observa o clima, fala sobre o que vê e liga isso à roupa ou à rotina do dia.",
            OpeningForAdult = "Diga: \"Hoje vamos olhar pela janela para descobrir como está o dia e o que isso muda na nossa rotina.\"",
            AnchorQuestion = "Como está o tempo hoje e o que muda no seu dia por causa disso?",
            CoreMaterialTitle = "Observação da lição",
            CoreMaterialParagraphs =
            [
                "Olhe para o céu, para a luz e para a rua.",
                "Diga se o dia está ensolarado, nublado ou chuvoso.",
                "Depois pense em uma roupa ou ação que combina com esse tempo."
            ],
            AdultSteps =
            [
                "Abra a janela ou mostre o quintal para a criança.",
                "Nomeie juntos o clima em palavras simples.",
                "Pergunte o que muda no dia por causa desse clima.",
                "Feche com desenho do tempo de hoje."
            ],
            AdultQuestions =
            [
                "Hoje está claro, escuro, ensolarado ou chuvoso?",
                "Que roupa combina com esse dia?",
                "O que você faria nesse tempo?"
            ],
            AcceptableAnswers =
            [
                "A criança descreve o tempo com uma palavra principal.",
                "Escolhe roupa coerente com o clima.",
                "Relaciona o dia a uma ação simples, como usar casaco ou brincar no quintal."
            ],
            PracticeTask = "Desenhar o céu de hoje e dizer uma frase sobre o clima.",
            CompletionDefinition = "Marque como concluída quando a criança observar o clima e ligá-lo a alguma escolha do próprio dia."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildChooseAndFinishPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia inicial • 3 a 4 anos • escolher, concluir e guardar",
            UnitTitle = "Fazer uma escolha e levar até o fim",
            UnitSummary = "A criança aprende que escolher uma atividade também inclui terminar e guardar o material depois.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje você vai escolher uma missão curta, terminar e depois guardar tudo no lugar certo.\"",
            AnchorQuestion = "Qual missão você escolheu e onde vai guardar o material quando terminar?",
            CoreMaterialTitle = "As duas opções da lição",
            CoreMaterialParagraphs =
            [
                "Opção 1: desenhar e pintar uma figura.",
                "Opção 2: montar uma sequência curta com blocos.",
                "No final, o material volta para a cesta."
            ],
            AdultSteps =
            [
                "Mostre apenas duas opções bem curtas.",
                "Deixe a criança escolher uma sem pressa.",
                "Acompanhe até o final sem trocar de atividade no meio.",
                "Feche com o gesto de guardar tudo na cesta."
            ],
            AdultQuestions =
            [
                "Qual missão você escolheu?",
                "Como sabemos que ela terminou?",
                "Onde vamos guardar isso agora?"
            ],
            AcceptableAnswers =
            [
                "A criança nomeia a atividade escolhida.",
                "Reconhece que terminou quando fez tudo combinado.",
                "Guarda o material no local certo."
            ],
            PracticeTask = "Escolher uma das duas opções, concluir e guardar o material sozinha ou com apoio leve.",
            CompletionDefinition = "Marque como concluída quando a criança fizer a sequência escolher, concluir e guardar sem trocar de tarefa."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildSoundToWordPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Alfabetização • 5 a 6 anos • som, sílaba e palavra",
            UnitTitle = "Sair do som e chegar na palavra",
            UnitSummary = "A criança parte de um som ou sílaba conhecida e chega a uma palavra concreta que consegue montar e ler.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos pegar um som pequeno e transformá-lo em palavra de verdade.\"",
            AnchorQuestion = "Que palavra conseguimos montar com esse som ou essa sílaba?",
            CoreMaterialTitle = "Som da lição: PA",
            CoreMaterialParagraphs =
            [
                "Hoje o som forte da lição é PA.",
                "Vamos pensar em palavras que começam com PA, como pato e panela.",
                "No final vamos montar uma palavra e ler juntos."
            ],
            AdultSteps =
            [
                "Leia o som da lição em voz clara.",
                "Peça duas palavras conhecidas com esse começo.",
                "Monte uma palavra com letras móveis ou no caderno.",
                "Leia junto e feche com repetição oral."
            ],
            AdultQuestions =
            [
                "Que palavra você conhece com PA?",
                "Qual palavra vamos montar hoje?",
                "Você consegue ler a palavra pronta?"
            ],
            AcceptableAnswers =
            [
                "Pato, panela, papel ou outra palavra familiar.",
                "A palavra montada precisa começar com PA.",
                "A criança lê a palavra com apoio leve."
            ],
            PracticeTask = "Montar uma palavra com a sílaba PA e registrar com desenho ou escrita curta.",
            CompletionDefinition = "Marque como concluída quando a criança ligar o som a uma palavra real e conseguir lê-la junto com o adulto."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildAdditionObjectsPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matemática no cotidiano • 5 a 6 anos • juntar quantidades",
            UnitTitle = "Somar com as mãos e com os olhos",
            UnitSummary = "A criança vê a adição acontecer com objetos antes de registrá-la no caderno.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos juntar dois grupos de objetos e descobrir quantos ficaram no final.\"",
            AnchorQuestion = "Quantos tínhamos antes, quantos entraram e quanto ficou no final?",
            CoreMaterialTitle = "Situação da lição",
            CoreMaterialParagraphs =
            [
                "Comece com 4 tampinhas.",
                "Depois coloque mais 3 tampinhas.",
                "Agora conte tudo para descobrir o total."
            ],
            AdultSteps =
            [
                "Monte o primeiro grupo de objetos.",
                "Adicione o segundo grupo na frente da criança.",
                "Conte tudo devagar tocando cada item.",
                "Registre a conta 4 + 3 = 7 no caderno."
            ],
            AdultQuestions =
            [
                "Quantas tampinhas tínhamos primeiro?",
                "Quantas entraram depois?",
                "Com quantas ficamos ao final?"
            ],
            AcceptableAnswers =
            [
                "Primeiro havia 4.",
                "Depois entraram 3.",
                "No final ficaram 7."
            ],
            PracticeTask = "Montar mais uma adição simples com objetos e registrar a conta correspondente.",
            CompletionDefinition = "Marque como concluída quando a criança conseguir juntar os grupos, contar o total e registrar a operação."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCommunityHelpersPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Mundo real • 5 a 6 anos • comunidade e pertencimento",
            UnitTitle = "Quem ajuda a vida da cidade a funcionar",
            UnitSummary = "A criança conhece pessoas e funções da comunidade e percebe como elas afetam a vida da família.",
            OpeningForAdult = "Diga: \"Hoje vamos pensar nas pessoas que cuidam da nossa comunidade e fazem o dia das famílias funcionar melhor.\"",
            AnchorQuestion = "Quem ajuda a nossa comunidade e como essa pessoa cuida de nós?",
            CoreMaterialTitle = "Pessoas da lição",
            CoreMaterialParagraphs =
            [
                "Profissional 1: quem cuida da saúde.",
                "Profissional 2: quem ajuda a manter a segurança.",
                "Profissional 3: quem trabalha para levar serviços e produtos até as famílias."
            ],
            AdultSteps =
            [
                "Escolha três funções que a criança reconheça.",
                "Explique cada uma com exemplo real do bairro ou da rotina.",
                "Peça que a criança escolha a que considera mais importante hoje.",
                "Feche com desenho ou frase curta."
            ],
            AdultQuestions =
            [
                "Quem cuida da nossa saúde quando precisamos?",
                "Que pessoa ajuda a organizar a cidade ou o bairro?",
                "Qual dessas funções parece mais importante para você e por quê?"
            ],
            AcceptableAnswers =
            [
                "A criança reconhece pelo menos duas funções.",
                "Consegue dizer como uma delas ajuda a comunidade.",
                "Escolhe uma e dá uma justificativa simples."
            ],
            PracticeTask = "Escolher uma pessoa da comunidade e registrar como ela ajuda a família ou o bairro.",
            CompletionDefinition = "Marque como concluída quando a criança conseguir relacionar uma função da comunidade à vida real."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildFinishAndTellPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia guiada • 5 a 6 anos • concluir e explicar",
            UnitTitle = "Fechar a tarefa sabendo dizer como fez",
            UnitSummary = "A criança aprende que terminar não é só parar: é conseguir contar a sequência do que foi feito.",
            OpeningForAdult = $"Diga: \"{firstName}, depois de terminar a missão de hoje, você vai me contar como conseguiu fazer.\"",
            AnchorQuestion = "O que você fez primeiro, depois e no final para terminar essa tarefa?",
            CoreMaterialTitle = "Fechamento da lição",
            CoreMaterialParagraphs =
            [
                "Primeiro eu comecei a tarefa.",
                "Depois eu fiz a parte principal.",
                "No final eu terminei e conferi."
            ],
            AdultSteps =
            [
                "Conclua uma tarefa curta antes de fazer perguntas.",
                "Puxe a fala em ordem: primeiro, depois e no final.",
                "Modele a resposta se a criança travar.",
                "Feche com elogio pela conclusão e pela explicação."
            ],
            AdultQuestions =
            [
                "Qual foi o primeiro passo?",
                "O que você fez no meio da tarefa?",
                "Como soube que tinha terminado?"
            ],
            AcceptableAnswers =
            [
                "A criança relata o começo da tarefa.",
                "Menciona a parte principal do que executou.",
                "Consegue dizer quando terminou."
            ],
            PracticeTask = "Concluir uma tarefa curta e depois explicar a sequência em três passos.",
            CompletionDefinition = "Marque como concluída quando a criança terminar a atividade e explicar o processo em ordem simples."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCharacterProblemPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Leitura e compreensão • 7 a 8 anos • estrutura da narrativa",
            UnitTitle = "Ler pensando em personagem, problema e solução",
            UnitSummary = "A criança aprende a enxergar a estrutura da história para compreender melhor o texto.",
            OpeningForAdult = "Diga: \"Hoje não vamos só contar o que aconteceu. Vamos descobrir quem é o personagem, qual problema apareceu e como tudo se resolveu.\"",
            AnchorQuestion = "Quem era o personagem, qual problema apareceu e como tudo foi resolvido?",
            CoreMaterialTitle = "Texto da lição: O menino e a ponte de madeira",
            CoreMaterialParagraphs =
            [
                "Pedro precisava atravessar um riacho para levar frutas para a avó.",
                "Quando chegou perto da ponte, percebeu que uma tábua estava solta.",
                "Ele chamou um vizinho, esperou ajuda e os dois arrumaram a passagem antes de continuar."
            ],
            AdultSteps =
            [
                "Leia o texto completo uma vez.",
                "Separe em três colunas: personagem, problema e solução.",
                "Peça que a criança preencha com palavras ou frases curtas.",
                "Feche com uma frase-resumo da história."
            ],
            AdultQuestions =
            [
                "Quem é o personagem principal?",
                "Qual problema aparece no caminho?",
                "Como o problema é resolvido?"
            ],
            AcceptableAnswers =
            [
                "Pedro.",
                "A ponte estava com uma tábua solta.",
                "Ele pediu ajuda e a passagem foi arrumada."
            ],
            PracticeTask = "Montar o quadro personagem, problema e solução no caderno.",
            CompletionDefinition = "Marque como concluída quando a criança identificar os três elementos principais da narrativa."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildClockRoutinePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matemática prática • 7 a 8 anos • horas e rotina",
            UnitTitle = "Ler horas para entender o próprio dia",
            UnitSummary = "A criança usa relógio e horários reais da rotina para dar sentido ao estudo de tempo.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos olhar o relógio para organizar momentos reais do seu dia.\"",
            AnchorQuestion = "Que horas costumamos começar, pausar e terminar a rotina de estudo?",
            CoreMaterialTitle = "Horários da lição",
            CoreMaterialParagraphs =
            [
                "Hora de começar os estudos.",
                "Hora da pausa curta.",
                "Hora de fechar a rotina."
            ],
            AdultSteps =
            [
                "Escolha três horários reais da rotina.",
                "Marque cada um no relógio desenhado ou analógico.",
                "Peça que a criança associe cada horário ao momento certo do dia.",
                "Feche com uma linha do tempo simples."
            ],
            AdultQuestions =
            [
                "Qual horário marca o começo do estudo?",
                "Que horas fazemos a pausa?",
                "Como sabemos a hora de terminar?"
            ],
            AcceptableAnswers =
            [
                "A criança identifica o início da rotina.",
                "Relaciona o horário da pausa ao meio da sessão.",
                "Reconhece o horário final combinado."
            ],
            PracticeTask = "Marcar três horários da rotina e escrever o que acontece em cada um.",
            CompletionDefinition = "Marque como concluída quando a criança usar as horas para explicar a própria rotina."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPlantGrowthPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Ciência em casa • 7 a 8 anos • observação ao longo do tempo",
            UnitTitle = "Registrar o crescimento de uma planta de verdade",
            UnitSummary = "A criança observa, compara e registra mudanças reais, aprendendo a construir um pequeno diário científico.",
            OpeningForAdult = "Diga: \"Hoje vamos observar uma planta de verdade e registrar o que mudou desde a última vez.\"",
            AnchorQuestion = "O que mudou hoje nessa planta e o que isso mostra sobre o crescimento dela?",
            CoreMaterialTitle = "Observação da lição",
            CoreMaterialParagraphs =
            [
                "Olhe caule, folhas, cor e tamanho.",
                "Compare com o último registro.",
                "Anote ou desenhe o que realmente mudou."
            ],
            AdultSteps =
            [
                "Leve a criança até a planta observada.",
                "Peça que ela diga primeiro o que percebeu.",
                "Compare com o registro anterior.",
                "Feche com uma frase de conclusão."
            ],
            AdultQuestions =
            [
                "O que está diferente hoje?",
                "A planta parece maior, igual ou menor?",
                "O que isso ensina sobre crescimento?"
            ],
            AcceptableAnswers =
            [
                "A criança identifica ao menos uma mudança real.",
                "Compara com o registro anterior.",
                "Tira uma conclusão curta sobre crescimento."
            ],
            PracticeTask = "Registrar em caderno ou desenho o que mudou na planta entre um dia e outro.",
            CompletionDefinition = "Marque como concluída quando a criança observar, comparar e registrar uma mudança concreta."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildTwoStepIndependentPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 7 a 8 anos • sequência independente curta",
            UnitTitle = "Cumprir duas missões na ordem certa",
            UnitSummary = "A criança organiza duas pequenas missões e aprende a concluí-las com menos interferência do adulto.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje você vai cumprir duas missões seguidas. Vamos combinar a ordem e depois você mesmo vai me mostrar quando terminar.\"",
            AnchorQuestion = "Quais são suas duas missões e como você vai saber que terminou cada uma?",
            CoreMaterialTitle = "As duas missões da lição",
            CoreMaterialParagraphs =
            [
                "Missão 1: concluir a parte principal do conteúdo.",
                "Missão 2: registrar ou organizar o fechamento.",
                "As duas precisam acontecer na ordem combinada."
            ],
            AdultSteps =
            [
                "Apresente as duas missões no cartão ou em voz alta.",
                "Peça que a criança repita a ordem.",
                "Deixe executar a primeira e depois a segunda com intervenção mínima.",
                "Feche conferindo o que foi concluído."
            ],
            AdultQuestions =
            [
                "Qual missão vem primeiro?",
                "O que acontece depois que ela termina?",
                "Como você vai me mostrar que as duas acabaram?"
            ],
            AcceptableAnswers =
            [
                "A criança repete a ordem correta.",
                "Reconhece a segunda missão sem precisar perguntar de novo.",
                "Sabe apontar ou dizer quando tudo foi concluído."
            ],
            PracticeTask = "Cumprir duas missões seguidas e marcar o cartão de conclusão.",
            CompletionDefinition = "Marque como concluída quando a criança sustentar a sequência de duas missões com menos lembretes."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCompareTextsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem avançando • 9 a 10 anos • comparação de textos",
            UnitTitle = "Comparar, não só repetir",
            UnitSummary = "A criança lê dois textos do mesmo tema e aprende a destacar semelhanças e diferenças importantes.",
            OpeningForAdult = "Diga: \"Hoje você vai ler dois textos parecidos e descobrir o que eles têm em comum e o que muda de um para o outro.\"",
            AnchorQuestion = "O que esses dois textos têm em comum e o que muda de um para o outro?",
            CoreMaterialTitle = "Textos da lição: dois relatos sobre chuva forte",
            CoreMaterialParagraphs =
            [
                "Texto 1: uma família relata a dificuldade de sair de casa durante uma chuva forte.",
                "Texto 2: um agricultor explica por que a mesma chuva foi importante para a plantação.",
                "Os dois falam da chuva, mas mostram efeitos diferentes."
            ],
            AdultSteps =
            [
                "Leia os dois textos sem pressa.",
                "Anote o tema comum no topo da página.",
                "Peça uma semelhança forte e uma diferença importante.",
                "Feche com uma conclusão comparativa."
            ],
            AdultQuestions =
            [
                "Qual é o assunto comum dos dois textos?",
                "O que aparece de parecido nas duas leituras?",
                "Qual é a diferença principal entre elas?"
            ],
            AcceptableAnswers =
            [
                "Os dois falam de chuva forte.",
                "Nos dois a chuva afeta a vida das pessoas.",
                "Em um caso a chuva atrapalha e no outro ajuda a plantação."
            ],
            PracticeTask = "Registrar tema comum, uma semelhança e uma diferença, depois escrever uma conclusão curta.",
            CompletionDefinition = "Marque como concluída quando a criança conseguir comparar os dois textos sem ficar apenas recontando cada um separado."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPerimeterRoomPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matemática em casa • 9 a 10 anos • medida e perímetro",
            UnitTitle = "Dar a volta inteira e somar o contorno",
            UnitSummary = "A criança entende perímetro medindo um espaço ou objeto real e somando seus lados.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos medir um espaço de verdade para descobrir quanto é a volta completa dele.\"",
            AnchorQuestion = "Quanto precisamos percorrer para dar a volta completa nesse espaço?",
            CoreMaterialTitle = "Espaço da lição: tapete ou mesa da casa",
            CoreMaterialParagraphs =
            [
                "Escolha um espaço retangular simples.",
                "Meça cada lado com régua, fita ou passos iguais.",
                "Some tudo para descobrir o perímetro."
            ],
            AdultSteps =
            [
                "Escolha um objeto ou espaço com contorno claro.",
                "Meça cada lado separadamente.",
                "Registre os valores e faça a soma final.",
                "Feche perguntando o que exatamente foi calculado."
            ],
            AdultQuestions =
            [
                "Que medida encontramos em cada lado?",
                "O que acontece quando somamos todos eles?",
                "Isso mostra área ou contorno?"
            ],
            AcceptableAnswers =
            [
                "A criança registra os lados medidos.",
                "Sabe que somar todos mostra a volta completa.",
                "Reconhece que perímetro é contorno, não área."
            ],
            PracticeTask = "Medir outro objeto simples e calcular o perímetro no caderno.",
            CompletionDefinition = "Marque como concluída quando a criança medir os lados e explicar que perímetro é a soma do contorno."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBrazilWaterwaysPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 9 a 10 anos • rios, território e vida real",
            UnitTitle = "Rios do Brasil e impacto na vida das pessoas",
            UnitSummary = "A criança estuda um rio brasileiro olhando para geografia, utilidade e preservação.",
            OpeningForAdult = "Diga: \"Hoje vamos localizar um rio importante do Brasil e entender por que ele muda a vida das pessoas e da natureza ao redor.\"",
            AnchorQuestion = "Por que um rio importante muda a vida das pessoas e do lugar onde ele passa?",
            CoreMaterialTitle = "Rio da lição: São Francisco",
            CoreMaterialParagraphs =
            [
                "O Rio São Francisco atravessa diferentes partes do Brasil.",
                "Ele ajuda no abastecimento, na agricultura, em atividades econômicas e na vida de muitas cidades.",
                "Quando a água é mal cuidada, pessoas, animais e plantações também sofrem."
            ],
            AdultSteps =
            [
                "Mostre o rio no mapa do Brasil.",
                "Anote duas utilidades e um cuidado necessário.",
                "Converse sobre quem depende dessa água.",
                "Feche com uma conclusão sobre preservação."
            ],
            AdultQuestions =
            [
                "Onde esse rio passa?",
                "Como ele ajuda a vida das pessoas?",
                "Por que cuidar da água desse rio é importante?"
            ],
            AcceptableAnswers =
            [
                "A criança localiza o rio no mapa ou reconhece a região.",
                "Cita uso para abastecimento, agricultura, transporte ou energia.",
                "Relaciona preservação da água à vida das pessoas e da natureza."
            ],
            PracticeTask = "Registrar no caderno: localização, duas utilidades e um cuidado necessário com o rio estudado.",
            CompletionDefinition = "Marque como concluída quando a criança ligar o rio à vida real e ao cuidado com o território."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildSelfCheckPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia acadêmica • 9 a 10 anos • revisão antes de concluir",
            UnitTitle = "Não entregar no impulso",
            UnitSummary = "A criança aprende uma rotina curta de revisão para não marcar a lição como concluída sem conferir o essencial.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje você só vai concluir a lição depois de passar por três perguntas de revisão.\"",
            AnchorQuestion = "Está completo, legível e responde exatamente ao que foi pedido?",
            CoreMaterialTitle = "Cartão de autochecagem",
            CoreMaterialParagraphs =
            [
                "1. Está completo?",
                "2. Está legível?",
                "3. Responde mesmo ao que foi pedido?"
            ],
            AdultSteps =
            [
                "Termine primeiro a atividade principal.",
                "Leia as três perguntas com a criança.",
                "Corrija apenas o que estiver realmente faltando.",
                "Só depois marque como concluído."
            ],
            AdultQuestions =
            [
                "Você respondeu tudo o que a tarefa pediu?",
                "Alguém conseguiria ler o que você escreveu?",
                "Falta revisar algum detalhe antes de concluir?"
            ],
            AcceptableAnswers =
            [
                "A criança verifica se a resposta está completa.",
                "Revisa a clareza do registro.",
                "Corrige algum detalhe antes de concluir, se necessário."
            ],
            PracticeTask = "Passar pelo cartão de autochecagem antes de marcar a lição como concluída.",
            CompletionDefinition = "Marque como concluída quando a criança revisar o próprio trabalho usando as três perguntas combinadas."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildThesisAndProofsPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem em aprofundamento • 11 a 12 anos • tese e prova textual",
            UnitTitle = "Uma tese que nao fica solta",
            UnitSummary = "A criança lê um texto curto, formula uma tese em uma frase e aprende a sustentá-la com duas provas retiradas da leitura.",
            OpeningForAdult = "Diga: \"Hoje voce nao vai responder por impulso. Primeiro vai decidir a sua tese. Depois vai mostrar duas provas do texto que sustentam essa resposta.\"",
            AnchorQuestion = "Qual e a sua tese sobre o texto e quais duas provas realmente sustentam essa resposta?",
            CoreMaterialTitle = "Texto da licao: A praça do bairro precisa ou nao de mais arvores?",
            CoreMaterialParagraphs =
            [
                "No texto, moradores reclamam do calor forte na praça e da falta de sombra em horários de maior movimento.",
                "Outro trecho mostra que, depois do plantio em uma rua próxima, o espaço ficou mais usado por famílias e idosos.",
                "A leitura também lembra que plantar exige cuidado contínuo, e não só o dia da ação comunitária."
            ],
            AdultSteps =
            [
                "Leia o texto uma vez inteira.",
                "Pergunte qual posição faz mais sentido depois da leitura.",
                "Peça que a criança formule a tese em uma frase.",
                "Volte ao texto e marque duas provas específicas antes de escrever a conclusão."
            ],
            AdultQuestions =
            [
                "Qual e a sua tese em uma frase curta?",
                "Que parte do texto sustenta essa tese?",
                "Qual segunda prova mostra que a tese nao e chute?"
            ],
            AcceptableAnswers =
            [
                "A criança formula uma tese clara, como: a praça precisa de mais árvores.",
                "Usa a falta de sombra e o calor como primeira prova.",
                "Usa o exemplo da rua mais frequentada após o plantio como segunda prova."
            ],
            PracticeTask = "Escrever uma tese, registrar duas provas do texto e fechar com uma conclusão curta em palavras próprias.",
            CompletionDefinition = "Marque como concluida quando a criança apresentar tese clara e duas provas concretas, sem copiar o texto inteiro."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildRatioTablePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica aplicada • 11 a 12 anos • razao, tabela e justificativa",
            UnitTitle = "Organizar antes de calcular",
            UnitSummary = "A criança monta tabela, compara razões e explica por que a estratégia usada faz sentido.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce nao vai começar pela conta. Vai organizar os dados primeiro, porque a tabela ajuda a enxergar a proporcao com clareza.\"",
            AnchorQuestion = "Como a tabela ajuda voce a mostrar a proporcao e a explicar a resposta final?",
            CoreMaterialTitle = "Situacao da licao: suco concentrado para a turma",
            CoreMaterialParagraphs =
            [
                "A receita base usa 2 copos de concentrado para 6 copos de agua.",
                "A turma quer preparar o dobro dessa receita sem perder o sabor.",
                "No final, a criança precisa mostrar a nova razao e justificar por que ela continua equivalente."
            ],
            AdultSteps =
            [
                "Monte duas colunas: concentrado e agua.",
                "Registre a razao original antes de dobrar.",
                "Peça que a criança complete a nova tabela com a versão ampliada.",
                "Feche com uma justificativa curta sobre equivalência."
            ],
            AdultQuestions =
            [
                "Qual e a razao original da receita?",
                "Se dobrarmos tudo, o sabor muda ou permanece proporcional?",
                "Como a tabela prova a sua resposta?"
            ],
            AcceptableAnswers =
            [
                "A criança registra 2 para 6 como razao inicial.",
                "Mostra 4 para 12 na nova linha.",
                "Explica que dobrou as duas partes, por isso a proporcao foi mantida."
            ],
            PracticeTask = "Montar a tabela da receita, ampliar a proporcao e escrever uma justificativa curta.",
            CompletionDefinition = "Marque como concluida quando a criança organizar os dados em tabela e explicar a proporcao sem depender apenas do resultado."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildTwoSourcesPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 11 a 12 anos • comparacao de fontes e conclusao",
            UnitTitle = "Duas fontes, uma pergunta central",
            UnitSummary = "A criança compara duas fontes sobre o mesmo tema, seleciona o essencial e constrói uma conclusão mais segura.",
            OpeningForAdult = "Diga: \"Hoje voce vai ler duas fontes curtas sobre o mesmo assunto. O foco nao e copiar tudo, e descobrir o que cada uma ajuda a responder.\"",
            AnchorQuestion = "O que essas duas fontes ajudam voce a responder sobre o mesmo tema?",
            CoreMaterialTitle = "Tema da licao: agua nas cidades brasileiras",
            CoreMaterialParagraphs =
            [
                "Fonte 1: noticia curta sobre uma cidade que ampliou reservatorios por causa da seca.",
                "Fonte 2: texto informativo explicando como desperdicio e crescimento urbano pressionam o abastecimento.",
                "As duas falam de agua nas cidades, mas cada uma mostra um angulo diferente do problema."
            ],
            AdultSteps =
            [
                "Leia a primeira fonte e anote o dado principal.",
                "Leia a segunda e anote a explicação principal.",
                "Compare o que se repete e o que muda entre elas.",
                "Peça uma conclusão final em duas ou três frases."
            ],
            AdultQuestions =
            [
                "Que dado aparece na primeira fonte?",
                "O que a segunda fonte explica sobre a causa do problema?",
                "Depois de comparar as duas, qual conclusao faz mais sentido?"
            ],
            AcceptableAnswers =
            [
                "A criança identifica o dado principal da noticia.",
                "Reconhece a explicacao sobre desperdicio ou pressão urbana.",
                "Conclui que o problema da agua depende tanto de estrutura quanto de uso responsável."
            ],
            PracticeTask = "Registrar um dado da primeira fonte, uma explicacao da segunda e uma conclusao final em linguagem propria.",
            CompletionDefinition = "Marque como concluida quando a criança conseguir comparar as duas fontes e fechar com uma conclusao coerente."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildWeekPlanPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 11 a 12 anos • plano semanal com prova de entrega",
            UnitTitle = "Planejar a semana para nao estudar no escuro",
            UnitSummary = "A criança aprende a escolher metas reais, definir ordem e mostrar prova concreta do que foi concluído.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje vamos abrir a semana com tres metas reais. Nao basta listar; voce tambem vai dizer qual vai primeiro e como vai provar que concluiu.\"",
            AnchorQuestion = "Quais sao as tres metas da semana, qual vem primeiro e como voce vai provar que fez?",
            CoreMaterialTitle = "Quadro da licao: plano de semana com tres metas",
            CoreMaterialParagraphs =
            [
                "Meta 1: a mais urgente ou a que destrava as outras.",
                "Meta 2: a que pode ser feita com o material ja separado.",
                "Meta 3: a que fecha a semana com revisao ou registro."
            ],
            AdultSteps =
            [
                "Peça tres metas claras e concretas.",
                "Numere a ordem das metas com a criança.",
                "Defina uma prova simples para cada uma: foto, folha preenchida ou checklist.",
                "Feche anotando o primeiro passo da meta 1."
            ],
            AdultQuestions =
            [
                "Qual meta precisa vir primeiro e por quê?",
                "Como vamos saber que ela foi concluida de verdade?",
                "Que primeiro passo voce consegue fazer hoje?"
            ],
            AcceptableAnswers =
            [
                "A criança escolhe uma meta prioritária e justifica.",
                "Define uma prova simples e observavel para cada meta.",
                "Nomeia o primeiro passo imediato sem depender de adivinhacao."
            ],
            PracticeTask = "Montar um plano com tres metas, ordem de prioridade e prova de entrega para cada uma.",
            CompletionDefinition = "Marque como concluida quando a criança sair da licao com a semana priorizada e a primeira entrega bem definida."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCounterArgumentPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem autoral • 13 a 14 anos • tese, contra-argumento e fechamento",
            UnitTitle = "Responder objeção sem perder a propria tese",
            UnitSummary = "A criança aprende a sustentar uma posição, reconhecer uma objeção possível e fechar o texto com mais maturidade.",
            OpeningForAdult = "Diga: \"Hoje sua resposta nao vai ser apenas opiniao. Voce vai defender uma tese, considerar um contra-argumento e mostrar por que sua posição continua de pé.\"",
            AnchorQuestion = "Qual e a sua tese, que contra-argumento precisa ser reconhecido e como voce responde a ele?",
            CoreMaterialTitle = "Tema da licao: uso de celular em horarios de estudo",
            CoreMaterialParagraphs =
            [
                "Um texto defende que o celular atrapalha foco e profundidade de estudo.",
                "Outro lembra que o aparelho tambem pode ser ferramenta de pesquisa e organizacao.",
                "A tarefa da criança e responder sem simplificar demais um tema que tem dois lados."
            ],
            AdultSteps =
            [
                "Defina a tese principal com a criança.",
                "Peça uma prova forte em favor dessa tese.",
                "Levante um contra-argumento honesto.",
                "Feche com uma refutação curta e uma conclusão final."
            ],
            AdultQuestions =
            [
                "Qual e a sua posicao principal?",
                "Que contra-argumento um colega poderia apresentar?",
                "Como voce responde a essa objeção sem abandonar sua tese?"
            ],
            AcceptableAnswers =
            [
                "A criança formula tese clara e defensavel.",
                "Reconhece um contra-argumento real, sem caricatura.",
                "Consegue responder a objeção e fechar a posição com coerência."
            ],
            PracticeTask = "Escrever uma resposta com tese, uma prova, um contra-argumento e um fechamento final convincente.",
            CompletionDefinition = "Marque como concluida quando a criança conseguir considerar a objeção e ainda assim sustentar a propria tese com clareza."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildBudgetChoicePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica em decisao real • 13 a 14 anos • orcamento, porcentagem e escolha",
            UnitTitle = "Comparar cenarios para tomar uma decisao",
            UnitSummary = "A criança usa porcentagem, tabela e limite de gasto para defender qual opção cabe melhor em um orçamento.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje a conta precisa terminar em decisao. Voce vai comparar duas opcoes, usar porcentagem e mostrar qual delas faz mais sentido dentro do orcamento.\"",
            AnchorQuestion = "Qual opcao cabe melhor no orcamento e que numero prova isso?",
            CoreMaterialTitle = "Situacao da licao: compra de materiais para um projeto",
            CoreMaterialParagraphs =
            [
                "Opcao A: custo menor na compra inicial, mas com reposicao mais frequente.",
                "Opcao B: custo inicial maior, mas dura mais e reduz novas despesas.",
                "A criança precisa comparar as duas com limite de 100 reais e justificar a melhor escolha."
            ],
            AdultSteps =
            [
                "Monte uma tabela com custo inicial, reposicao e total previsto.",
                "Peça calculo percentual ou comparacao proporcional quando fizer sentido.",
                "Compare os cenarios olhando curto e medio prazo.",
                "Feche com a decisão escolhida e a justificativa numerica."
            ],
            AdultQuestions =
            [
                "Quanto cada opcao custa no começo?",
                "Qual delas pesa mais no total previsto?",
                "Que numero mostra com mais clareza a melhor escolha?"
            ],
            AcceptableAnswers =
            [
                "A criança organiza custos em tabela comparativa.",
                "Usa porcentagem ou diferenca relativa para comparar cenarios.",
                "Defende a escolha final com base numerica, e nao por preferencia solta."
            ],
            PracticeTask = "Montar a tabela dos cenarios, calcular a comparacao principal e escrever a decisao com justificativa.",
            CompletionDefinition = "Marque como concluida quando a criança usar dados numericos para defender a melhor escolha dentro do orcamento."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildCauseConsequencePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 13 a 14 anos • causa, consequencia e tomada de posicao",
            UnitTitle = "Nao parar no fato: explicar o que ele provoca",
            UnitSummary = "A criança analisa um caso do mundo real separando causa, consequência e posição final com base em evidência.",
            OpeningForAdult = "Diga: \"Hoje voce nao vai ficar apenas no fato. Vai explicar o que causou o problema, o que aconteceu depois e qual posição faz sentido assumir.\"",
            AnchorQuestion = "O que causou esse problema, que consequencias ele trouxe e qual posicao faz mais sentido depois da analise?",
            CoreMaterialTitle = "Caso da licao: enchentes urbanas e ocupacao desordenada",
            CoreMaterialParagraphs =
            [
                "O texto mostra como o crescimento urbano sem drenagem adequada aumenta o risco de enchentes.",
                "Tambem mostra que o problema nao depende de uma unica causa, mas da soma entre chuva forte, impermeabilizacao e falta de estrutura.",
                "A resposta final precisa ligar causa, consequência e decisão pública ou comunitária."
            ],
            AdultSteps =
            [
                "Leia o caso e destaque a causa principal.",
                "Liste duas consequencias reais para pessoas ou territorio.",
                "Pergunte qual resposta ou posicao faz mais sentido.",
                "Feche com uma conclusão curta e justificada."
            ],
            AdultQuestions =
            [
                "Qual causa principal aparece no texto?",
                "Quais consequencias reais esse problema traz?",
                "Depois da analise, que posicao ou resposta parece mais coerente?"
            ],
            AcceptableAnswers =
            [
                "A criança identifica causa relevante e não genérica.",
                "Relaciona o problema a duas consequencias concretas.",
                "Fecha com uma posição conectada ao que leu e não apenas a opinião pessoal."
            ],
            PracticeTask = "Registrar causa principal, duas consequencias e uma posicao final justificada.",
            CompletionDefinition = "Marque como concluida quando a criança ligar fato, impacto e posição final com clareza."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildIndependentSprintPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia academica • 13 a 14 anos • sprint independente e reflexao final",
            UnitTitle = "Estudar com meta curta e ajuste de metodo",
            UnitSummary = "A criança trabalha um bloco mais independente, revisa a entrega e identifica o ajuste que precisa fazer no próximo sprint.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce vai estudar em sprint: meta clara, foco unico e revisao curta no final. O mais importante e terminar sabendo o que manter e o que ajustar.\"",
            AnchorQuestion = "Qual e a meta deste sprint, o que prova que voce concluiu e que ajuste voce faria para estudar melhor no proximo bloco?",
            CoreMaterialTitle = "Roteiro da licao: sprint de estudo",
            CoreMaterialParagraphs =
            [
                "Meta do sprint: uma entrega clara, nao varias ao mesmo tempo.",
                "Foco do sprint: manter a mesma missao ate o fim.",
                "Fechamento do sprint: revisar e escolher um ajuste para a proxima vez."
            ],
            AdultSteps =
            [
                "Defina com a criança uma meta concluivel em 20 a 30 minutos.",
                "Combine qual prova vai mostrar que o bloco terminou.",
                "Deixe a execução mais independente, interrompendo o minimo possível.",
                "Feche com revisão da entrega e uma frase de ajuste para o próximo sprint."
            ],
            AdultQuestions =
            [
                "Qual e a meta unica deste sprint?",
                "Que prova vai mostrar que voce terminou?",
                "No final, o que precisa mudar para estudar ainda melhor?"
            ],
            AcceptableAnswers =
            [
                "A criança define uma meta objetiva e observavel.",
                "Conclui o bloco sem trocar de tarefa no meio.",
                "Fecha com um ajuste honesto de metodo, e não só com elogio vazio."
            ],
            PracticeTask = "Executar um sprint com meta unica, revisar a entrega e registrar um ajuste para o proximo bloco.",
            CompletionDefinition = "Marque como concluida quando a criança terminar um bloco independente e sair dele com revisão e ajuste definidos."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildParaphrasePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem em aprofundamento • 11 a 12 anos • parafrase e anotacao",
            UnitTitle = "Registrar sem copiar",
            UnitSummary = "A criança identifica a ideia central de um trecho e a registra com palavras proprias, preservando o sentido original.",
            OpeningForAdult = "Diga: \"Hoje a meta nao e copiar bonito. E mostrar que voce entendeu o trecho a ponto de reescrever a ideia com suas palavras.\"",
            AnchorQuestion = "Como voce reescreveria essa ideia sem copiar a frase pronta e sem perder o sentido?",
            CoreMaterialTitle = "Trecho da licao: por que os rios urbanos precisam de cuidado continuo",
            CoreMaterialParagraphs =
            [
                "O texto explica que rios urbanos sofrem com lixo, ocupacao das margens e falta de cuidado continuo.",
                "Tambem afirma que recuperar um rio exige constancia, e nao apenas acoes de um dia.",
                "A tarefa e registrar a ideia principal de forma fiel, mas com linguagem propria."
            ],
            AdultSteps =
            [
                "Leia o trecho em voz alta.",
                "Circule a frase mais importante.",
                "Peça uma parafrase em duas linhas.",
                "Feche com uma anotacao de estudo do que vale lembrar depois."
            ],
            AdultQuestions =
            [
                "O que o texto quer explicar acima de tudo?",
                "Como voce diria isso com palavras suas?",
                "O que nao pode se perder quando voce reescreve?"
            ],
            AcceptableAnswers =
            [
                "A criança identifica a ideia principal do trecho.",
                "Reescreve a ideia sem copiar a frase inteira.",
                "Mantem o sentido de cuidado continuo com o rio."
            ],
            PracticeTask = "Escrever uma parafrase fiel do trecho e uma anotacao curta do que vale lembrar.",
            CompletionDefinition = "Marque como concluida quando a criança registrar a ideia central com palavras proprias e sentido preservado."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildDiscountComparisonPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica aplicada • 11 a 12 anos • desconto, aumento e comparacao",
            UnitTitle = "Porcentagem com efeito real",
            UnitSummary = "A criança compara descontos e aumentos olhando o impacto real sobre o valor final, e nao apenas o numero escrito.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje voce vai comparar duas mudancas de preco e descobrir qual mexe mais no valor final de verdade.\"",
            AnchorQuestion = "Qual desconto ou aumento mexe mais no valor final e como voce prova isso?",
            CoreMaterialTitle = "Situacao da licao: duas promocoes de material escolar",
            CoreMaterialParagraphs =
            [
                "Promocao A: 20% de desconto em uma compra de 80 reais.",
                "Promocao B: 10% de desconto mais 5 reais de frete em uma compra de 70 reais.",
                "A tarefa e descobrir qual cenarios realmente pesa menos no total."
            ],
            AdultSteps =
            [
                "Calcule o valor final de cada opcao.",
                "Compare o efeito real do desconto e do frete.",
                "Peça que a criança explique qual cenário ficou melhor.",
                "Feche com uma frase de justificativa."
            ],
            AdultQuestions =
            [
                "Quanto fica cada opcao no final?",
                "O desconto por si so resolveu tudo?",
                "Qual cenário vale mais a pena e por quê?"
            ],
            AcceptableAnswers =
            [
                "A criança calcula os dois valores finais.",
                "Percebe que taxa extra pode mudar a leitura do desconto.",
                "Justifica a melhor escolha com base no total final."
            ],
            PracticeTask = "Comparar os dois cenarios, registrar os valores finais e escrever a melhor escolha.",
            CompletionDefinition = "Marque como concluida quando a criança comparar os dois cenarios usando o efeito real da porcentagem."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildTimelineChangePacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 11 a 12 anos • linha do tempo e mudanca historica",
            UnitTitle = "Historia como processo",
            UnitSummary = "A criança organiza marcos em ordem e explica o que mudou entre um ponto e outro, sem ficar presa a datas soltas.",
            OpeningForAdult = "Diga: \"Hoje voce vai montar uma linha do tempo curta para mostrar como uma historia muda com o passar dos marcos, e nao so para decorar datas.\"",
            AnchorQuestion = "O que mudou nessa historia de um marco para o outro?",
            CoreMaterialTitle = "Recorte da licao: ampliacao da escola publica no bairro",
            CoreMaterialParagraphs =
            [
                "Marco 1: havia poucas vagas e muitas crianças ficavam longe da escola.",
                "Marco 2: a comunidade pressionou por mais estrutura.",
                "Marco 3: com a ampliacao, novas familias passaram a ter acesso mais proximo."
            ],
            AdultSteps =
            [
                "Coloque os tres marcos em ordem.",
                "Explique a passagem de um para o outro.",
                "Peça que a criança diga qual marco mostra a maior mudança.",
                "Feche com uma frase de síntese histórica."
            ],
            AdultQuestions =
            [
                "O que acontece primeiro?",
                "Que mudança aparece no marco seguinte?",
                "Qual marco ajuda mais a entender a história toda?"
            ],
            AcceptableAnswers =
            [
                "A criança ordena os marcos corretamente.",
                "Explica a mudança entre falta de vagas, pressão e ampliação.",
                "Fecha com uma leitura de processo, e nao de data isolada."
            ],
            PracticeTask = "Montar a linha do tempo em tres marcos e escrever a principal mudança percebida.",
            CompletionDefinition = "Marque como concluida quando a criança mostrar processo historico e nao apenas fatos soltos."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildMaterialsDeadlinePacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia para estudar • 11 a 12 anos • material certo e prazo visivel",
            UnitTitle = "Preparar antes de travar",
            UnitSummary = "A criança aprende a começar o bloco já com material certo, tempo previsto e entrega definida.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje o treino começa antes da licao. Voce vai separar o material, definir o tempo e dizer o que vai me mostrar no final.\"",
            AnchorQuestion = "Que material voce precisa, quanto tempo vai usar e qual entrega vai provar que a tarefa terminou?",
            CoreMaterialTitle = "Checklist da licao",
            CoreMaterialParagraphs =
            [
                "1. Material certo na mesa.",
                "2. Tempo do bloco definido.",
                "3. Entrega observavel combinada antes de começar."
            ],
            AdultSteps =
            [
                "Peça que a criança liste o material sem levantar no meio.",
                "Defina o tempo do bloco com ela.",
                "Combine a entrega final antes de começar.",
                "Feche conferindo se tudo foi cumprido."
            ],
            AdultQuestions =
            [
                "Que material voce realmente vai usar agora?",
                "Quanto tempo este bloco precisa?",
                "Como vamos saber que a tarefa terminou?"
            ],
            AcceptableAnswers =
            [
                "A criança separa o material sem improviso.",
                "Define um tempo plausivel para o bloco.",
                "Nomeia uma entrega observavel e coerente."
            ],
            PracticeTask = "Executar um bloco com material certo, prazo definido e entrega combinada antes do inicio.",
            CompletionDefinition = "Marque como concluida quando a criança abrir a tarefa preparada e sem depender de adivinhacao do adulto."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildSourceSynthesisPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Linguagem autoral • 13 a 14 anos • sintese de fontes com posicionamento",
            UnitTitle = "Sintetizar e ainda assumir posicao",
            UnitSummary = "A criança junta o que duas fontes acrescentam e termina com posicionamento claro, e nao com um resumo neutro demais.",
            OpeningForAdult = "Diga: \"Hoje a tarefa nao termina no resumo. Depois de comparar as fontes, voce vai sintetizar e dizer qual posicao faz mais sentido para voce.\"",
            AnchorQuestion = "Depois de comparar essas fontes, que sintese voce faz e que posicao ela sustenta?",
            CoreMaterialTitle = "Tema da licao: transporte publico e tempo de deslocamento",
            CoreMaterialParagraphs =
            [
                "Uma fonte apresenta dados de tempo perdido no deslocamento urbano.",
                "Outra discute propostas de melhoria e seus limites.",
                "A criança precisa juntar os dois lados e fechar com posição sustentada."
            ],
            AdultSteps =
            [
                "Anote o que cada fonte acrescenta.",
                "Peça uma sintese em poucas linhas.",
                "Pergunte que posição a criança assume depois da leitura.",
                "Feche com uma conclusão autoral."
            ],
            AdultQuestions =
            [
                "O que a primeira fonte traz de mais forte?",
                "O que a segunda muda ou amplia?",
                "Depois das duas, qual posicao voce sustenta?"
            ],
            AcceptableAnswers =
            [
                "A criança diferencia o papel de cada fonte.",
                "Sintetiza sem apagar as diferenças.",
                "Fecha com uma posição baseada no conjunto lido."
            ],
            PracticeTask = "Produzir uma sintese curta das fontes e uma conclusão com posicionamento final.",
            CompletionDefinition = "Marque como concluida quando a criança sintetizar e assumir posição com apoio nas fontes."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildEquationScenarioPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Matematica em decisao real • 13 a 14 anos • equacao, cenario e decisao",
            UnitTitle = "A equacao precisa contar uma historia",
            UnitSummary = "A criança modela uma situação real com equação simples e interpreta o resultado para tomar decisão.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje a equacao nao fica so no simbolo. Ela precisa representar um problema real e ajudar voce a decidir entre cenarios.\"",
            AnchorQuestion = "Que situacao essa equacao representa e como o resultado ajuda a decidir?",
            CoreMaterialTitle = "Situacao da licao: custo fixo e custo por unidade em duas propostas",
            CoreMaterialParagraphs =
            [
                "Uma proposta tem taxa inicial menor e custo por unidade maior.",
                "Outra tem taxa inicial maior e custo por unidade menor.",
                "A equacao serve para descobrir em que ponto uma se torna mais vantajosa."
            ],
            AdultSteps =
            [
                "Defina o que cada letra da equacao representa.",
                "Monte as duas expressões do problema.",
                "Compare os resultados ou o ponto de encontro.",
                "Feche com a decisão apoiada nessa leitura."
            ],
            AdultQuestions =
            [
                "O que a variavel representa nesta situacao?",
                "Como cada proposta aparece na equacao?",
                "Em que momento uma opcao passa a fazer mais sentido?"
            ],
            AcceptableAnswers =
            [
                "A criança liga a letra a uma quantidade real.",
                "Modela as propostas com coerencia.",
                "Interpreta o resultado em linguagem de decisão, e nao só em conta."
            ],
            PracticeTask = "Montar a equacao do caso, interpretar o resultado e defender qual cenário faz mais sentido.",
            CompletionDefinition = "Marque como concluida quando a criança usar a equacao para explicar uma escolha real."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildPublicProblemPacket()
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Brasil e mundo • 13 a 14 anos • problema publico e proposta",
            UnitTitle = "Sair da reclamacao e entrar na proposta",
            UnitSummary = "A criança analisa um problema publico, organiza causas e impactos e termina em proposta de resposta.",
            OpeningForAdult = "Diga: \"Hoje voce vai estudar um problema publico, mas a tarefa so fica completa quando chegar a uma proposta coerente com o que leu.\"",
            AnchorQuestion = "Que proposta faz mais sentido depois de entender causas e impactos desse problema?",
            CoreMaterialTitle = "Caso da licao: descarte irregular de lixo em area urbana",
            CoreMaterialParagraphs =
            [
                "O caso mostra lixo acumulado, impacto em chuva e risco para a comunidade.",
                "Tambem mostra que o problema depende de infraestrutura, hábito e fiscalização.",
                "A conclusão precisa ligar analise e proposta, e nao apenas indignacao."
            ],
            AdultSteps =
            [
                "Liste as causas do problema.",
                "Liste dois impactos concretos para pessoas e territorio.",
                "Peça uma proposta viavel de resposta.",
                "Feche com justificativa curta da proposta."
            ],
            AdultQuestions =
            [
                "Que causa aparece com mais força no caso?",
                "Quem e o que sofre impacto direto?",
                "Que proposta dialoga melhor com essas causas e impactos?"
            ],
            AcceptableAnswers =
            [
                "A criança analisa causas e não apenas o sintoma.",
                "Nomeia impactos concretos do problema.",
                "Fecha com proposta coerente e justificada."
            ],
            PracticeTask = "Registrar causas, impactos e uma proposta final ligada ao caso estudado.",
            CompletionDefinition = "Marque como concluida quando a criança terminar a análise com proposta coerente e não só com crítica solta."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildProjectCheckpointPacket(string firstName)
    {
        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = "Autonomia academica • 13 a 14 anos • checkpoint e ajuste de projeto",
            UnitTitle = "Parar no meio para melhorar o final",
            UnitSummary = "A criança revisa o que já andou, identifica o que falta e ajusta rota antes da entrega final.",
            OpeningForAdult = $"Diga: \"{firstName}, hoje o treino nao e acabar tudo. E fazer um checkpoint no meio do projeto para melhorar a rota antes da entrega final.\"",
            AnchorQuestion = "O que ja avançou, o que ainda falta e que ajuste entra agora para a entrega sair melhor?",
            CoreMaterialTitle = "Quadro da licao: checkpoint de projeto",
            CoreMaterialParagraphs =
            [
                "Parte 1: o que ja foi concluido.",
                "Parte 2: o que ainda falta.",
                "Parte 3: ajuste de prazo, foco ou qualidade."
            ],
            AdultSteps =
            [
                "Peça que a criança descreva o avanço real do projeto.",
                "Liste o que ainda falta com honestidade.",
                "Escolha um ajuste concreto de rota.",
                "Feche definindo o proximo passo com prazo curto."
            ],
            AdultQuestions =
            [
                "O que ja saiu do papel de verdade?",
                "Que parte ainda esta travando ou faltando?",
                "Que ajuste ajuda mais agora: prazo, foco ou qualidade?"
            ],
            AcceptableAnswers =
            [
                "A criança reconhece avanço real, e não impressão vaga.",
                "Consegue nomear o que ainda falta.",
                "Define um ajuste útil para melhorar a entrega."
            ],
            PracticeTask = "Fazer checkpoint do projeto, registrar o ajuste e sair com o proximo passo definido.",
            CompletionDefinition = "Marque como concluida quando a criança revisar o projeto e ajustar a rota antes do fim."
        };
    }

    private static ProprietaryLessonPacketViewModel BuildFallbackPacket(
        CuratedTaskTemplate task,
        DailyPlanBlock block,
        int age,
        ProprietaryCurriculumUnitBlueprintViewModel? currentUnit,
        ProprietaryCurriculumLessonBlueprintViewModel? currentLesson)
    {
        var subjectLabel = currentUnit?.SubjectLabel ?? CurriculumStructure.FormatDomainLabel(block.Domain);

        var coreMaterialLabel = CurriculumStructure.GetAnalyticsDomain(block.Domain) switch
        {
            LearningDomain.Language => "Leia agora",
            LearningDomain.Math => "Resolva agora",
            LearningDomain.Science => "Investigue agora",
            LearningDomain.History => "Entenda agora",
            LearningDomain.Geography => "Localize agora",
            LearningDomain.ExecutiveFunction => "Organize agora",
            _ => "Licao de hoje"
        };

        var resolvedLessonTitle = string.IsNullOrWhiteSpace(currentLesson?.Title)
            ? task.Title
            : currentLesson.Title;
        var resolvedPlacement = currentUnit is null
            ? $"{subjectLabel} • trilha proprietaria • faixa de {age} anos"
            : $"{currentUnit.SubjectLabel} • {currentUnit.SchoolPlacementLabel} • {currentUnit.UnitLabel.ToLowerInvariant()}";
        var resolvedUnitTitle = currentUnit?.Title ?? task.Title;
        var resolvedSummary = currentUnit?.Summary ?? currentLesson?.Goal ?? task.Goal;
        var resolvedObjective = currentLesson?.Goal ?? currentUnit?.Objective ?? task.ExpectedOutcome;
        var resolvedPrompt = currentLesson?.AnchorQuestion ?? currentUnit?.TaskPrompt ?? task.ChildPrompt;
        var resolvedCompletion = currentLesson?.CompletionDefinition ?? currentUnit?.CompletionSignal ?? $"Marque como concluida quando a crianca atingir isto: {task.ExpectedOutcome}";
        var materialBase = currentUnit?.Materials.Count > 0 == true
            ? string.Join(", ", currentUnit.Materials)
            : task.MaterialsSummary;

        return new ProprietaryLessonPacketViewModel
        {
            CurriculumPlacement = resolvedPlacement,
            UnitTitle = resolvedUnitTitle,
            UnitSummary = resolvedSummary,
            OpeningForAdult = !string.IsNullOrWhiteSpace(currentLesson?.OpeningForAdult)
                ? currentLesson.OpeningForAdult
                : currentUnit is null
                ? $"Use a licao de hoje como uma missao curta e concreta. {task.ParentGuide}"
                : $"Diga: \"Hoje vamos trabalhar {resolvedLessonTitle.ToLowerInvariant()} dentro de {currentUnit.UnitLabel.ToLowerInvariant()} de {currentUnit.SubjectLabel.ToLowerInvariant()}.\" {currentUnit.ParentGuide}",
            AnchorQuestion = !string.IsNullOrWhiteSpace(currentLesson?.AnchorQuestion)
                ? currentLesson.AnchorQuestion
                : string.IsNullOrWhiteSpace(task.ChildPrompt)
                    ? $"Qual e a resposta mais clara que voce consegue dar para {resolvedLessonTitle.ToLowerInvariant()}?"
                    : resolvedPrompt,
            CoreMaterialLabel = currentLesson?.CoreMaterialLabel ?? coreMaterialLabel,
            CoreMaterialTitle = currentLesson?.CoreMaterialTitle ?? "Base da licao",
            CoreMaterialParagraphs = currentLesson?.CoreMaterialParagraphs.Count > 0
                ? currentLesson.CoreMaterialParagraphs
                :
                [
                    resolvedObjective,
                    $"Material base: {materialBase}",
                    $"Ao terminar, verifique isto: {resolvedCompletion}"
                ],
            AdultSteps = currentLesson?.AdultSteps.Count > 0
                ? currentLesson.AdultSteps
                : SplitLines(task.TaskSteps).Count > 0
                ? SplitLines(task.TaskSteps)
                : BuildFallbackSteps(currentUnit, resolvedLessonTitle),
            AdultQuestions = currentLesson?.AdultQuestions.Count > 0
                ? currentLesson.AdultQuestions
                :
                [
                    string.IsNullOrWhiteSpace(task.ChildPrompt)
                        ? "O que voce entendeu que precisa fazer agora?"
                        : resolvedPrompt
                ],
            AcceptableAnswers = currentLesson?.AcceptableAnswers.Count > 0
                ? currentLesson.AcceptableAnswers
                :
                [
                    resolvedObjective
                ],
            PracticeTask = !string.IsNullOrWhiteSpace(currentLesson?.PracticeTask)
                ? currentLesson.PracticeTask
                : currentUnit is null
                ? $"Execute a licao usando: {task.MaterialsSummary}"
                : $"Execute {resolvedLessonTitle.ToLowerInvariant()} usando: {materialBase}.",
            CompletionDefinition = resolvedCompletion
        };
    }

    private static List<string> BuildFallbackSteps(
        ProprietaryCurriculumUnitBlueprintViewModel? currentUnit,
        string lessonTitle)
    {
        if (currentUnit is null)
        {
            return [];
        }

        return
        [
            $"Explique para a crianca o objetivo de {lessonTitle.ToLowerInvariant()} em uma frase curta.",
            $"Execute a parte principal de {lessonTitle.ToLowerInvariant()} com apoio proporcional e sem adiantar a proxima licao.",
            $"Feche {lessonTitle.ToLowerInvariant()} com registro curto, resposta oral ou folha final, conforme a materia.",
            $"Confira se a crianca mostrou o sinal de avance combinado em {currentUnit.UnitLabel.ToLowerInvariant()}."
        ];
    }

    private static List<string> SplitLines(string value)
    {
        return value
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static string GetFirstName(string fullName)
    {
        var firstName = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstName) ? "Filho" : firstName;
    }
}
