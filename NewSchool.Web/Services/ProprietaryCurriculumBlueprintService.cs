using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ProprietaryCurriculumBlueprintService
{
    public ProprietaryCurriculumSubjectBlueprintViewModel BuildSubject(int age, LearningDomain domain)
    {
        var normalizedAge = Math.Clamp(age, 3, 14);
        var units = BuildUnits(normalizedAge, domain);

        return new ProprietaryCurriculumSubjectBlueprintViewModel
        {
            Age = normalizedAge,
            Domain = domain,
            SchoolPlacementLabel = CurriculumStructure.GetSchoolPlacementLabel(normalizedAge),
            SubjectLabel = CurriculumStructure.FormatDomainLabel(domain),
            TrackTitle = BuildTrackTitle(domain),
            YearGoal = BuildYearGoal(normalizedAge, domain),
            ParentMethod = BuildParentMethod(normalizedAge, domain),
            Phases = units
                .Select(MapPhase)
                .ToList()
        };
    }

    public ProprietaryCurriculumUnitBlueprintViewModel? GetCurrentUnit(int age, LearningDomain domain, DateTime referenceDate)
    {
        var subject = BuildSubject(age, domain);
        var currentPhaseIndex = referenceDate.Month switch
        {
            <= 3 => 0,
            <= 6 => 1,
            <= 9 => 2,
            _ => 3
        };

        return subject.Phases
            .ElementAtOrDefault(currentPhaseIndex)?
            .Units
            .FirstOrDefault();
    }

    private static ProprietaryCurriculumPhaseBlueprintViewModel MapPhase(UnitSeed unit)
    {
        return new ProprietaryCurriculumPhaseBlueprintViewModel
        {
            PhaseNumber = unit.PhaseNumber,
            PhaseLabel = unit.PhaseLabel,
            Summary = unit.Summary,
            Units =
            [
                new ProprietaryCurriculumUnitBlueprintViewModel
                {
                    SchoolPlacementLabel = unit.SchoolPlacementLabel,
                    SubjectLabel = unit.SubjectLabel,
                    PhaseNumber = unit.PhaseNumber,
                    PhaseLabel = unit.PhaseLabel,
                    UnitNumber = unit.UnitNumber,
                    UnitLabel = unit.UnitLabel,
                    Title = unit.Title,
                    Summary = unit.Summary,
                    Objective = unit.Objective,
                    ParentGuide = unit.ParentGuide,
                    TaskPrompt = unit.TaskPrompt,
                    CompletionSignal = unit.CompletionSignal,
                    OptionalEvidenceNote = unit.OptionalEvidenceNote,
                    CompanionBookTitle = unit.CompanionBookTitle,
                    PrintableTitle = unit.PrintableTitle,
                    AssessmentTitle = unit.AssessmentTitle,
                    LessonTitles = unit.Lessons.Select(lesson => lesson.Title).ToList(),
                    Lessons = unit.Lessons.Select(MapLesson).ToList(),
                    Materials = unit.Materials.ToList()
                }
            ]
        };
    }

    private static ProprietaryCurriculumLessonBlueprintViewModel MapLesson(LessonSeed lesson)
    {
        return new ProprietaryCurriculumLessonBlueprintViewModel
        {
            TaskSlug = lesson.TaskSlug,
            LessonNumber = lesson.LessonNumber,
            Title = lesson.Title,
            Goal = lesson.Goal,
            OpeningForAdult = lesson.OpeningForAdult,
            AnchorQuestion = lesson.AnchorQuestion,
            CoreMaterialLabel = lesson.CoreMaterialLabel,
            CoreMaterialTitle = lesson.CoreMaterialTitle,
            CoreMaterialParagraphs = lesson.CoreMaterialParagraphs.ToList(),
            AdultSteps = lesson.AdultSteps.ToList(),
            AdultQuestions = lesson.AdultQuestions.ToList(),
            AcceptableAnswers = lesson.AcceptableAnswers.ToList(),
            PracticeTask = lesson.PracticeTask,
            CompletionDefinition = lesson.CompletionDefinition,
            EvidencePrompt = lesson.EvidencePrompt,
            ExpectedOutcome = lesson.ExpectedOutcome,
            SuggestedMinutes = lesson.SuggestedMinutes,
            MatchKeywords = lesson.MatchKeywords,
            PrimaryMaterialTitle = lesson.PrimaryMaterialTitle,
            SupportMaterialTitle = lesson.SupportMaterialTitle
        };
    }

    private static string BuildTrackTitle(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem em casa",
        LearningDomain.Math => "Matemática no cotidiano",
        LearningDomain.Science => "Ciências em casa",
        LearningDomain.History => "História em casa",
        LearningDomain.Geography => "Geografia em casa",
        LearningDomain.ExecutiveFunction => "Autonomia para estudar",
        _ => "Currículo autoral"
    };

    private static string BuildYearGoal(int age, LearningDomain domain) => domain switch
    {
        LearningDomain.Language when age <= 5 => "Construir fala organizada, consciência fonológica, letras e leitura inicial com base concreta.",
        LearningDomain.Language => "Fortalecer leitura, escrita, síntese, argumentação e produção autoral de acordo com a série.",
        LearningDomain.Math when age <= 5 => "Consolidar número, quantidade, comparação e resolução concreta com registro simples.",
        LearningDomain.Math => "Avançar em cálculo, problemas, medidas, representação e pensamento matemático formal.",
        LearningDomain.Science when age <= 5 => "Explorar corpo, natureza, animais e fenômenos do cotidiano com observação guiada.",
        LearningDomain.Science => "Construir investigação, experimento, registro e explicação científica progressiva.",
        LearningDomain.History when age <= 5 => "Perceber sequência, memória, família, comunidade e mudanças no tempo.",
        LearningDomain.History => "Ler fatos, processos, fontes e relações de causa e consequência em escala escolar.",
        LearningDomain.Geography when age <= 5 => "Ler casa, bairro, clima, trajetos e referências do espaço próximo.",
        LearningDomain.Geography => "Ler mapas, território, regiões, ambiente e organização do espaço social.",
        LearningDomain.ExecutiveFunction when age <= 5 => "Criar rotina, foco e pequenas responsabilidades durante a aula.",
        LearningDomain.ExecutiveFunction => "Ganhar autonomia para planejar, iniciar, revisar e concluir o estudo.",
        _ => "Avançar de forma constante durante o ano."
    };

    private static string BuildParentMethod(int age, LearningDomain domain) => domain switch
    {
        LearningDomain.Language when age <= 5 => "Leia em voz alta, modele a resposta e aceite fala curta antes de exigir escrita.",
        LearningDomain.Language => "Trabalhe leitura, pergunta clara, produção curta e revisão visível.",
        LearningDomain.Math => "Comece no concreto, passe pelo desenho e só depois consolide no símbolo.",
        LearningDomain.Science => "Abra com observação, puxe uma pergunta forte e feche com descoberta registrável.",
        LearningDomain.History => "Organize o tempo, compare fatos e use fonte curta ou memória familiar como apoio.",
        LearningDomain.Geography => "Use mapa, trajeto, referência visual e comparação entre lugares reais.",
        LearningDomain.ExecutiveFunction => "Repita a mesma estrutura de início, execução, revisão e fechamento até virar rotina.",
        _ => "Conduza em passos curtos e observáveis."
    };

    private static UnitSeed[] BuildUnits(int age, LearningDomain domain) => domain switch
    {
        LearningDomain.Language => BuildLanguageUnits(age),
        LearningDomain.Math => BuildMathUnits(age),
        LearningDomain.Science => BuildScienceUnits(age),
        LearningDomain.History => BuildHistoryUnits(age),
        LearningDomain.Geography => BuildGeographyUnits(age),
        LearningDomain.ExecutiveFunction => BuildExecutiveUnits(age),
        _ => BuildExecutiveUnits(age)
    };

    private static UnitSeed[] BuildLanguageUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.Language,
            "Escuta, nome e repertório oral",
            "Rimas e sons que se repetem",
            "Histórias em três momentos",
            "Marcas, traços e letras iniciais",
            "oralidade, livro ilustrado, espelho, cartão com nome"),
        4 => BuildUnits(age, LearningDomain.Language,
            "Rimas, aliteração e escuta",
            "Sílabas, nome e letras conhecidas",
            "Frases orais com sentido",
            "Pré-escrita de palavras familiares",
            "cartões, nome próprio, figuras, apostila curta"),
        5 => BuildUnits(age, LearningDomain.Language,
            "Som, sílaba e palavra conhecida",
            "Leitura de palavras e frases curtas",
            "Reconto, descrição e frase com sentido",
            "Início da escrita guiada",
            "caderno, letras móveis, apostila, figuras"),
        6 => BuildUnits(age, LearningDomain.Language,
            "Leitura inicial, som-grafia e sentido",
            "Reconto com apoio visual e sequência clara",
            "Ortografia frequente e frase completa",
            "Bilhete, legenda e pequeno relato",
            "livro curto, caderno, cartões de sílabas, marca-texto"),
        7 => BuildUnits(age, LearningDomain.Language,
            "Fluência, autocorreção e frase bem formada",
            "Parágrafo com ideia completa e resposta guiada",
            "Pontuação básica e revisão visível",
            "Texto curto com começo, problema e fechamento",
            "texto curto, caderno, cartões de palavras, quadro de perguntas"),
        8 => BuildUnits(age, LearningDomain.Language,
            "Personagem, cenário e conflito",
            "Sequência de fatos, causa e consequência",
            "Parágrafo com ideia central e detalhe de apoio",
            "Comparação curta entre dois textos",
            "conto curto, caderno, grifos, quadro de leitura"),
        9 => BuildUnits(age, LearningDomain.Language,
            "Ideia central, prova e detalhe relevante",
            "Personagem, problema e solução com justificativa",
            "Parágrafo com evidência retirada do texto",
            "Opinião com argumento simples e prova",
            "texto informativo, caderno, quadro de perguntas, marca-texto"),
        10 => BuildUnits(age, LearningDomain.Language,
            "Resumo com palavras próprias e foco no essencial",
            "Inferência, ponto de vista e leitura crítica",
            "Texto de opinião com repertório e prova",
            "Revisão de coesão, clareza e escolha lexical",
            "texto informativo, caderno, rubrica curta, dicionário"),
        11 => BuildUnits(age, LearningDomain.Language,
            "Tese, argumento e fonte confiável",
            "Comparação de fontes e síntese guiada",
            "Resumo técnico e reescrita objetiva",
            "Posicionamento com evidência e explicação",
            "artigo curto, caderno de leitura, duas fontes, grifos"),
        12 => BuildUnits(age, LearningDomain.Language,
            "Leitura de artigo e estrutura argumentativa",
            "Síntese de múltiplas vozes e contraste de fontes",
            "Citação, paráfrase e nota de fonte",
            "Texto expositivo com defesa organizada",
            "artigo, duas fontes, caderno de leitura, checklist"),
        13 => BuildUnits(age, LearningDomain.Language,
            "Leitura crítica, ironia e escolha lexical",
            "Tese, contra-argumento e refutação",
            "Ensaio breve com repertório e conclusão",
            "Revisão autoral por critérios de banca",
            "duas fontes, artigo, caderno, rubrica de revisão"),
        _ => BuildUnits(age, LearningDomain.Language,
            "Artigo de opinião e debate público",
            "Síntese de múltiplas fontes com posicionamento",
            "Repertório histórico e social na argumentação",
            "Posicionamento crítico com revisão final",
            "fontes múltiplas, artigo, caderno, checklist de revisão")
    };

    private static UnitSeed[] BuildMathUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.Math,
            "Contagem até 5",
            "Mais, menos e igual",
            "Formas e classificações",
            "Padrões e sequências simples",
            "tampinhas, blocos, cartões, caderno"),
        4 => BuildUnits(age, LearningDomain.Math,
            "Contagem até 10",
            "Comparação e agrupamento",
            "Posição, espaço e formas",
            "Pequenos problemas do cotidiano",
            "objetos da casa, dezena improvisada, calendário, folhas"),
        5 => BuildUnits(age, LearningDomain.Math,
            "Número e quantidade até 20",
            "Juntar e tirar até 10",
            "Compor e decompor quantidades",
            "Registro matemático inicial",
            "palitos, numerais, caderno, jogos simples"),
        6 => BuildUnits(age, LearningDomain.Math,
            "Contagem, dezenas, unidades e agrupamentos",
            "Adição e subtração com material e desenho",
            "Problemas do cotidiano em uma etapa",
            "Calendário, relógio e rotina do dia",
            "material dourado caseiro, reta numérica, relógio, caderno"),
        7 => BuildUnits(age, LearningDomain.Math,
            "Valor posicional até centenas",
            "Adição e subtração com reagrupamento",
            "Multiplicação inicial com arranjos",
            "Medidas, dinheiro e situações de compra",
            "material dourado, cédulas de brincadeira, tabela, caderno"),
        8 => BuildUnits(age, LearningDomain.Math,
            "Multiplicação com estratégias variadas",
            "Divisão e repartição com sentido",
            "Frações iniciais e representação concreta",
            "Tabelas, gráficos simples e leitura de dados",
            "barras desenhadas, reta numérica, tabela, caderno"),
        9 => BuildUnits(age, LearningDomain.Math,
            "Tabuada com sentido e justificativa",
            "Multiplicação, divisão e problemas multietapas",
            "Frações, equivalência e partes do todo",
            "Tempo, medidas e gráficos do cotidiano",
            "barras desenhadas, fração concreta, tabela, caderno"),
        10 => BuildUnits(age, LearningDomain.Math,
            "Problemas estratégicos com várias etapas",
            "Perímetro, área e medida em situações reais",
            "Frações, decimais e porcentagem inicial",
            "Orçamento, troco e decisão financeira simples",
            "tabela, fita métrica, folheto, caderno"),
        11 => BuildUnits(age, LearningDomain.Math,
            "Razão, proporção e escala",
            "Equações com sentido real",
            "Tabelas, gráficos e média",
            "Porcentagem em orçamento e comparação",
            "planilha simples, problema contextual, caderno, calculadora opcional"),
        12 => BuildUnits(age, LearningDomain.Math,
            "Proporção, porcentagem e juros simples",
            "Equação, regularidade e regra geral",
            "Estatística e leitura crítica de dados",
            "Orçamento e decisão com restrição",
            "planilha, tabela, gráfico, caderno"),
        13 => BuildUnits(age, LearningDomain.Math,
            "Álgebra inicial e modelagem",
            "Desconto, acréscimo e comparação de cenários",
            "Probabilidade e gráficos comparativos",
            "Finanças do cotidiano e escolha justificada",
            "planilha, tabela, gráfico, caderno"),
        _ => BuildUnits(age, LearningDomain.Math,
            "Equações e relações algébricas",
            "Função, variação e leitura de gráfico",
            "Probabilidade e análise de dados",
            "Matemática financeira e decisão em projeto",
            "planilha, tabela, gráfico, caderno")
    };

    private static UnitSeed[] BuildScienceUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.Science,
            "Meu corpo e meus sentidos",
            "Animais e seus jeitos",
            "Água, cor e mistura",
            "Natureza bem perto",
            "espelho, água, folhas, animais de brinquedo"),
        4 => BuildUnits(age, LearningDomain.Science,
            "Plantas, clima e observação",
            "Som, luz e movimento",
            "Misturas e mudanças",
            "Cuidado com o ambiente",
            "copo, sementes, lanterna, folha de registro"),
        5 => BuildUnits(age, LearningDomain.Science,
            "Corpo, saúde e rotina",
            "Animais, plantas e habitat",
            "Experimentos com previsão",
            "Tempo, clima e natureza do dia",
            "sementes, copos, imagens, caderno"),
        6 => BuildUnits(age, LearningDomain.Science,
            "Seres vivos, ambiente e observação",
            "Água, ar e clima do cotidiano",
            "Corpo, saúde e hábitos",
            "Experimento simples com previsão e conclusão",
            "tabela simples, experimento caseiro, lupa, caderno"),
        7 => BuildUnits(age, LearningDomain.Science,
            "Plantas, animais e ciclo de vida",
            "Matéria, mistura e transformação",
            "Energia, luz e som",
            "Registro de pergunta, teste e conclusão",
            "sementes, lanterna, experimento caseiro, caderno"),
        8 => BuildUnits(age, LearningDomain.Science,
            "Ecossistemas, cadeia alimentar e equilíbrio",
            "Forças, movimento e observação",
            "Corpo humano e sistemas básicos",
            "Experimento com comparação de resultados",
            "imã, água, tabela, caderno de ciências"),
        9 => BuildUnits(age, LearningDomain.Science,
            "Matéria, energia e transformação",
            "Água, solo e ambiente brasileiro",
            "Corpo humano, saúde e prevenção",
            "Método científico no cotidiano",
            "roteiro de observação, tabela, caderno, imagens"),
        10 => BuildUnits(age, LearningDomain.Science,
            "Ecossistemas brasileiros e ciclos naturais",
            "Digestão, respiração e circulação",
            "Eletricidade, calor e mudanças físicas",
            "Experimento guiado com variável simples",
            "roteiro de experimento, caderno, imagens, tabela"),
        11 => BuildUnits(age, LearningDomain.Science,
            "Método científico e qualidade da evidência",
            "Células, sistemas e corpo humano",
            "Ecossistemas, ambiente e impacto",
            "Matéria, energia e fenômenos observáveis",
            "duas fontes, experimento, tabela, caderno"),
        12 => BuildUnits(age, LearningDomain.Science,
            "Modelos científicos e explicação",
            "Genética inicial e regulação do corpo",
            "Ambiente, clima e tecnologia",
            "Movimento, energia e interpretação de fenômenos",
            "duas fontes, experimento, esquema, caderno"),
        13 => BuildUnits(age, LearningDomain.Science,
            "Energia, clima e sustentabilidade",
            "Química do cotidiano e reações",
            "Saúde coletiva, prevenção e corpo",
            "Pesquisa orientada com dados e conclusão",
            "fonte curta, tabela, experimento, caderno"),
        _ => BuildUnits(age, LearningDomain.Science,
            "Ciência, tecnologia e sociedade",
            "Ambiente, energia e responsabilidade pública",
            "Fenômenos físicos com variáveis e modelo",
            "Projeto científico com defesa de conclusão",
            "fonte curta, tabela, experimento, caderno")
    };

    private static UnitSeed[] BuildHistoryUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.History,
            "Minha família e minha rotina",
            "Antes e depois no dia",
            "Memórias com imagens",
            "História curta em sequência",
            "fotos da família, cartões, história ilustrada, caderno"),
        4 => BuildUnits(age, LearningDomain.History,
            "Sequência do dia e da semana",
            "Família, comunidade e memória",
            "Histórias de ontem e hoje",
            "Calendário, festa e mudança",
            "calendário, fotos, figuras, linha do tempo curta"),
        5 => BuildUnits(age, LearningDomain.History,
            "Ontem, hoje e amanhã",
            "História da família e da comunidade",
            "Acontecimentos em ordem",
            "Mudanças no tempo e nos costumes",
            "linha do tempo simples, fotos, relatos, caderno"),
        6 => BuildUnits(age, LearningDomain.History,
            "Família, memória e comunidade",
            "Ontem, hoje e amanhã em sequência",
            "Festas, costumes e mudanças no tempo",
            "Linha do tempo da vida cotidiana",
            "linha do tempo, fotos, biografia curta, caderno"),
        7 => BuildUnits(age, LearningDomain.History,
            "Tempo histórico e personagens do cotidiano",
            "Memória local, trabalho e comunidade",
            "Símbolos do Brasil e marcos de convivência",
            "Fato, mudança e permanência",
            "linha do tempo, biografia curta, mapa histórico, caderno"),
        8 => BuildUnits(age, LearningDomain.History,
            "Linha do tempo e leitura de fontes",
            "Povos originários, colônia e encontros",
            "Independência, cidadania e formação do país",
            "Mudanças sociais e vida cotidiana",
            "fonte curta, linha do tempo, imagem histórica, caderno"),
        9 => BuildUnits(age, LearningDomain.History,
            "Brasil colônia e organização da sociedade",
            "Independência, império e transformações",
            "Trabalho, escravidão e resistência",
            "Fato histórico, causa e consequência",
            "fonte curta, linha do tempo, documento, caderno"),
        10 => BuildUnits(age, LearningDomain.History,
            "República, cidadania e mudanças do Brasil",
            "Migrações, trabalho e vida social",
            "Memória, documento e interpretação histórica",
            "Fato histórico com causa, consequência e debate",
            "documento curto, linha do tempo, caderno, mapa"),
        11 => BuildUnits(age, LearningDomain.History,
            "Fontes históricas e ponto de vista",
            "Brasil colônia, império e república",
            "Tempo histórico e relações de causa",
            "Debate histórico com evidência",
            "duas fontes, documento, linha do tempo, caderno"),
        12 => BuildUnits(age, LearningDomain.History,
            "Brasil e mundo em conexão histórica",
            "Revoluções, direitos e cidadania",
            "República, modernização e conflitos sociais",
            "Documento histórico, interpretação e debate",
            "duas fontes, cronologia, caderno, quadro comparativo"),
        13 => BuildUnits(age, LearningDomain.History,
            "Brasil moderno e conflitos do século XX",
            "Regimes políticos, memória e democracia",
            "Industrialização, urbanização e trabalho",
            "Argumento histórico com fontes múltiplas",
            "fonte, cronologia, caderno, quadro de comparação"),
        _ => BuildUnits(age, LearningDomain.History,
            "Estado, democracia e cidadania",
            "Ditadura, redemocratização e memória pública",
            "Globalização, desigualdade e conflitos sociais",
            "Estudo de caso histórico com posição final",
            "fonte, cronologia, caderno, quadro de comparação")
    };

    private static UnitSeed[] BuildGeographyUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.Geography,
            "Minha casa e os lugares do dia",
            "Dentro, fora, perto e longe",
            "Clima e roupa do dia",
            "Caminhos e referências simples",
            "objetos da casa, fotos, setas, cartões"),
        4 => BuildUnits(age, LearningDomain.Geography,
            "Casa, escola e bairro",
            "Mapa simples do cotidiano",
            "Tempo, clima e paisagem",
            "Lugares que usamos para viver",
            "desenho de trajeto, mapa simples, janela, caderno"),
        5 => BuildUnits(age, LearningDomain.Geography,
            "Meu trajeto e meus pontos de referência",
            "Bairro, cidade e convívio",
            "Clima, natureza e paisagem",
            "Mapa simples e localização",
            "mapa desenhado, fotos, seta, caderno"),
        6 => BuildUnits(age, LearningDomain.Geography,
            "Casa, escola, bairro e orientação",
            "Serviços da cidade e vida cotidiana",
            "Mapa simples, legenda e trajeto",
            "Clima, natureza e uso do espaço",
            "mapa simples, legenda, imagens, caderno"),
        7 => BuildUnits(age, LearningDomain.Geography,
            "Bairro, cidade e convivência urbana",
            "Paisagem, tempo e recursos do lugar",
            "Mapa, legenda e pontos de referência",
            "Campo, cidade e formas de viver",
            "mapa simples, legenda, imagens, caderno"),
        8 => BuildUnits(age, LearningDomain.Geography,
            "Mapas, regiões e território brasileiro",
            "Paisagens do Brasil e recursos naturais",
            "População, trabalho e redes de circulação",
            "Ambiente, deslocamentos e uso do espaço",
            "mapa do Brasil, legenda, dados simples, caderno"),
        9 => BuildUnits(age, LearningDomain.Geography,
            "Regiões do Brasil e identidades territoriais",
            "Biomas, recursos e conservação",
            "Campo, cidade, mobilidade e redes",
            "Território, ambiente e impactos humanos",
            "mapa do Brasil, atlas, dados simples, caderno"),
        10 => BuildUnits(age, LearningDomain.Geography,
            "Território, fronteiras e ocupação do Brasil",
            "População, migração e urbanização",
            "Clima, relevo, água e ambiente",
            "Mapa temático e leitura do espaço econômico",
            "atlas simples, mapa, dados, caderno"),
        11 => BuildUnits(age, LearningDomain.Geography,
            "Cartografia, escala e representação",
            "Território, população e redes",
            "Clima, vegetação, água e sociedade",
            "Mapa temático com conclusão escrita",
            "mapa temático, tabela, atlas, caderno"),
        12 => BuildUnits(age, LearningDomain.Geography,
            "Cartografia, geotecnologia e leitura espacial",
            "Migração, desigualdade e população",
            "Espaço agrário, urbano e industrial",
            "Geopolítica, ambiente e redes globais",
            "atlas, mapa temático, tabela, caderno"),
        13 => BuildUnits(age, LearningDomain.Geography,
            "Território, globalização e logística",
            "Urbanização, indústria e trabalho",
            "Clima, biomas e crise ambiental",
            "Leitura cartográfica com dados comparativos",
            "mapa, gráfico, atlas, caderno"),
        _ => BuildUnits(age, LearningDomain.Geography,
            "Geopolítica, território e poder",
            "Energia, ambiente e uso do espaço",
            "Cidade, desigualdade e mobilidade",
            "Problema territorial com proposta de intervenção",
            "mapa, gráfico, atlas, caderno")
    };

    private static UnitSeed[] BuildExecutiveUnits(int age) => age switch
    {
        3 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Começar sem travar",
            "Pegar, fazer e guardar",
            "Esperar a vez e terminar",
            "Fechar a rotina com ajuda leve",
            "checklist visual, cesta, timer curto, cartão first-then"),
        4 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Rotina visual com dois passos",
            "Início rápido da atividade",
            "Concluir e revisar materiais",
            "Autonomia básica no fechamento",
            "quadro visual, timer, cesta de materiais, checklist"),
        5 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Abrir a aula com previsibilidade",
            "Seguir combinados do bloco",
            "Planejar e terminar",
            "Guardar e contar o que fez",
            "checklist, quadro de missões, timer, cesta"),
        6 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Abrir material e começar no tempo certo",
            "Seguir instruções em dois passos",
            "Revisar antes de marcar como feito",
            "Guardar material e fechar a rotina",
            "cronômetro, checklist, caderno, quadro da aula"),
        7 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Preparar a mesa e iniciar sem demora",
            "Sustentar foco em bloco curto",
            "Conferir resposta e corrigir com apoio",
            "Fechar a aula contando o que fez",
            "cronômetro, checklist, caderno, quadro da aula"),
        8 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Planejar a tarefa e estimar tempo",
            "Dividir missão grande em partes",
            "Revisar por critérios simples",
            "Organizar próximo passo do estudo",
            "plano do dia, checklist, cronômetro, caderno"),
        9 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Escolher estratégia e começar sem travar",
            "Manter foco, pausa e retomada",
            "Autochecagem antes de concluir",
            "Registrar progresso e próximo passo",
            "plano do dia, checklist, cronômetro, caderno"),
        10 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Planejamento semanal e prioridades",
            "Gestão de tarefa longa com checkpoints",
            "Revisão por checklist e ajuste",
            "Entrega com responsabilidade e registro",
            "planner, checklist, calendário, caderno"),
        11 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Metas reais por bloco de estudo",
            "Organização de materiais por matéria",
            "Revisão antes de entregar",
            "Fechamento semanal com reflexão objetiva",
            "planner, checklist, calendário, rubrica curta"),
        12 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Plano semanal com ajuste de rota",
            "Gestão de materiais e prazos",
            "Autochecagem com evidência do que foi feito",
            "Reflexão e retomada do que ficou pendente",
            "planner, checklist, calendário, rubrica curta"),
        13 => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Sprint de estudo e autogestão",
            "Checkpoint de projeto e revisão parcial",
            "Rubrica, revisão e correção autônoma",
            "Responsabilidade com prazo e comunicação",
            "planner, quadro de projeto, checklist, calendário"),
        _ => BuildUnits(age, LearningDomain.ExecutiveFunction,
            "Planejamento por objetivo e prioridade",
            "Gestão de projeto e fontes de trabalho",
            "Revisão autoral e checkpoint crítico",
            "Fechamento de etapa com próxima ação definida",
            "planner, quadro de projeto, checklist, calendário")
    };

    private static UnitSeed[] BuildUnits(
        int age,
        LearningDomain domain,
        string unit1,
        string unit2,
        string unit3,
        string unit4,
        string materials)
    {
        return
        [
            CreateUnit(age, domain, 1, unit1, materials),
            CreateUnit(age, domain, 2, unit2, materials),
            CreateUnit(age, domain, 3, unit3, materials),
            CreateUnit(age, domain, 4, unit4, materials)
        ];
    }

    private static UnitSeed CreateUnit(int age, LearningDomain domain, int phaseNumber, string title, string materials)
    {
        var subjectLabel = CurriculumStructure.FormatDomainLabel(domain);
        var schoolPlacementLabel = CurriculumStructure.GetSchoolPlacementLabel(age);
        var placementShortLabel = BuildPlacementShortLabel(age);
        var phaseLabel = $"{phaseNumber}ª etapa";
        var unitLabel = $"Unidade {phaseNumber}";
        var companionBookTitle = $"Leituras de {subjectLabel} • {placementShortLabel}";
        var printableTitle = $"Apostila de {subjectLabel} • {placementShortLabel}";
        var assessmentTitle = $"Caderno de avaliação de {subjectLabel} • {placementShortLabel}";
        var materialList = materials
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => item.Length > 1)
            .ToArray();

        var summary = phaseNumber switch
        {
            1 => $"Abrir a base anual em {title.ToLowerInvariant()} com passos curtos, material concreto e resposta modelada.",
            2 => $"Ampliar {title.ToLowerInvariant()} ligando prática, registro e explicação com menos improviso.",
            3 => $"Consolidar {title.ToLowerInvariant()} com comparação, revisão e mais autonomia da criança.",
            _ => $"Fechar {title.ToLowerInvariant()} com síntese, prova da aprendizagem e entrega observável."
        };

        var objective = phaseNumber switch
        {
            1 => $"Levar a criança a entrar em {title.ToLowerInvariant()} com segurança e linguagem clara.",
            2 => $"Fazer a criança praticar {title.ToLowerInvariant()} com constância e registro melhor organizado.",
            3 => $"Transformar {title.ToLowerInvariant()} em resposta cada vez mais consistente e justificável.",
            _ => $"Fechar a unidade mostrando que {title.ToLowerInvariant()} já virou habilidade observável."
        };

        var parentGuide = domain switch
        {
            LearningDomain.Language when age <= 5 => $"Leia, modele a fala e aceite resposta curta antes de exigir escrita em {title.ToLowerInvariant()}.",
            LearningDomain.Language => $"Abra com leitura curta, peça resposta oral e só depois registre por escrito o trabalho de {title.ToLowerInvariant()}.",
            LearningDomain.Math => $"Comece no concreto, passe pelo desenho e feche com símbolo ou explicação curta em {title.ToLowerInvariant()}.",
            LearningDomain.Science => $"Abra com observação real, registre hipótese e feche com descoberta clara sobre {title.ToLowerInvariant()}.",
            LearningDomain.History => $"Use memória, linha do tempo, fato ou fonte curta para organizar {title.ToLowerInvariant()} sem decorar solto.",
            LearningDomain.Geography => $"Mostre mapa, trajeto, imagem ou lugar real antes de pedir que a criança explique {title.ToLowerInvariant()}.",
            LearningDomain.ExecutiveFunction => $"Repita a mesma rotina de abrir, fazer, revisar e fechar até {title.ToLowerInvariant()} ficar previsível.",
            _ => $"Conduza {title.ToLowerInvariant()} em etapas visíveis, uma por vez."
        };

        var taskPrompt = domain switch
        {
            LearningDomain.Language => $"Hoje a missão é ler, responder e registrar algo essencial de {title.ToLowerInvariant()} sem copiar tudo.",
            LearningDomain.Math => $"Hoje a missão é resolver {title.ToLowerInvariant()} mostrando conta, desenho ou justificativa.",
            LearningDomain.Science => $"Hoje a missão é observar, perguntar e concluir algo real sobre {title.ToLowerInvariant()}.",
            LearningDomain.History => $"Hoje a missão é organizar {title.ToLowerInvariant()} em ordem, causa e consequência.",
            LearningDomain.Geography => $"Hoje a missão é localizar, comparar ou explicar {title.ToLowerInvariant()} usando referência concreta.",
            LearningDomain.ExecutiveFunction => $"Hoje a missão é executar {title.ToLowerInvariant()} com começo, meio e fim visíveis.",
            _ => $"Hoje a missão é praticar {title.ToLowerInvariant()} com uma entrega clara."
        };

        var completionSignal = domain switch
        {
            LearningDomain.Language => $"A unidade avança quando a criança responde com clareza, usa prova do texto quando preciso e registra o essencial.",
            LearningDomain.Math => $"A unidade avança quando a criança mostra o raciocínio de {title.ToLowerInvariant()} e consegue explicar a escolha feita.",
            LearningDomain.Science => $"A unidade avança quando a criança observa, registra e fecha uma conclusão coerente sobre {title.ToLowerInvariant()}.",
            LearningDomain.History => $"A unidade avança quando a criança organiza fatos, ordem e causa com linguagem própria.",
            LearningDomain.Geography => $"A unidade avança quando a criança lê o espaço, usa referência e explica a relação entre lugar e ação humana.",
            LearningDomain.ExecutiveFunction => $"A unidade avança quando a criança inicia, conclui e revisa a missão com menos lembretes.",
            _ => $"A unidade avança quando a habilidade aparece de forma observável."
        };

        var optionalEvidenceNote = $"Se a família quiser guardar memória desta unidade, pode salvar foto, vídeo curto ou folha final de {title.ToLowerInvariant()} em Evidências.";
        var lessonBlueprints = BuildLessons(
            age,
            domain,
            schoolPlacementLabel,
            placementShortLabel,
            phaseNumber,
            unitLabel,
            title,
            objective,
            companionBookTitle,
            printableTitle,
            assessmentTitle,
            materialList);

        return new UnitSeed(
            subjectLabel,
            schoolPlacementLabel,
            phaseNumber,
            phaseLabel,
            phaseNumber,
            unitLabel,
            title,
            summary,
            objective,
            parentGuide,
            taskPrompt,
            completionSignal,
            optionalEvidenceNote,
            companionBookTitle,
            printableTitle,
            assessmentTitle,
            lessonBlueprints,
            materialList);
    }

    private static LessonSeed[] BuildLessons(
        int age,
        LearningDomain domain,
        string schoolPlacementLabel,
        string placementShortLabel,
        int phaseNumber,
        string unitLabel,
        string unitTitle,
        string objective,
        string companionBookTitle,
        string printableTitle,
        string assessmentTitle,
        IReadOnlyList<string> materials)
    {
        var subjectLabel = CurriculumStructure.FormatDomainLabel(domain);
        var narrative = BuildCoreNarrative(age, domain, unitTitle, phaseNumber);
        var lesson1Title = BuildLessonTitle(domain, age, unitTitle, 1);
        var lesson2Title = BuildLessonTitle(domain, age, unitTitle, 2);
        var lesson3Title = BuildLessonTitle(domain, age, unitTitle, 3);
        var lesson4Title = BuildLessonTitle(domain, age, unitTitle, 4);
        var lesson5Title = BuildLessonTitle(domain, age, unitTitle, 5);
        var suggestedMinutes = domain == LearningDomain.ExecutiveFunction ? 12 : age <= 5 ? 18 : 24;

        return
        [
            CreateLesson(
                age,
                domain,
                phaseNumber,
                1,
                subjectLabel,
                schoolPlacementLabel,
                placementShortLabel,
                unitLabel,
                unitTitle,
                objective,
                lesson1Title,
                narrative,
                companionBookTitle,
                printableTitle,
                materials,
                suggestedMinutes),
            CreateLesson(
                age,
                domain,
                phaseNumber,
                2,
                subjectLabel,
                schoolPlacementLabel,
                placementShortLabel,
                unitLabel,
                unitTitle,
                objective,
                lesson2Title,
                narrative,
                printableTitle,
                assessmentTitle,
                materials,
                suggestedMinutes + 2),
            CreateLesson(
                age,
                domain,
                phaseNumber,
                3,
                subjectLabel,
                schoolPlacementLabel,
                placementShortLabel,
                unitLabel,
                unitTitle,
                objective,
                lesson3Title,
                narrative,
                assessmentTitle,
                companionBookTitle,
                materials,
                suggestedMinutes + 4),
            CreateLesson(
                age,
                domain,
                phaseNumber,
                4,
                subjectLabel,
                schoolPlacementLabel,
                placementShortLabel,
                unitLabel,
                unitTitle,
                objective,
                lesson4Title,
                narrative,
                printableTitle,
                assessmentTitle,
                materials,
                suggestedMinutes + 2),
            CreateLesson(
                age,
                domain,
                phaseNumber,
                5,
                subjectLabel,
                schoolPlacementLabel,
                placementShortLabel,
                unitLabel,
                unitTitle,
                objective,
                lesson5Title,
                narrative,
                assessmentTitle,
                companionBookTitle,
                materials,
                suggestedMinutes + 4)
        ];
    }

    private static LessonSeed CreateLesson(
        int age,
        LearningDomain domain,
        int unitNumber,
        int lessonNumber,
        string subjectLabel,
        string schoolPlacementLabel,
        string placementShortLabel,
        string unitLabel,
        string unitTitle,
        string objective,
        string lessonTitle,
        NarrativeSeed narrative,
        string primaryMaterialTitle,
        string supportMaterialTitle,
        IReadOnlyList<string> materials,
        int suggestedMinutes)
    {
        var taskSlug = $"curriculo-{BuildPlacementCode(age)}-{BuildDomainCode(domain)}-u{unitNumber}-l{lessonNumber}";
        var openingForAdult = BuildOpeningForAdult(domain, age, subjectLabel, unitLabel, unitTitle, lessonNumber);
        var goal = BuildLessonGoal(domain, age, unitTitle, lessonNumber, primaryMaterialTitle);

        var anchorQuestion = BuildAnchorQuestion(domain, age, unitTitle, lessonNumber);
        var adultSteps = BuildAdultSteps(domain, age, unitTitle, lessonNumber, narrative, materials);
        var adultQuestions = BuildAdultQuestions(domain, age, unitTitle, lessonNumber);
        var acceptableAnswers = BuildAcceptableAnswers(domain, age, unitTitle, lessonNumber);
        var practiceTask = BuildPracticeTask(domain, age, unitTitle, lessonNumber, primaryMaterialTitle);
        var completionDefinition = BuildCompletionDefinition(domain, unitTitle, lessonNumber);
        var evidencePrompt = BuildEvidencePrompt(domain, unitTitle, lessonNumber);
        var expectedOutcome = BuildExpectedOutcome(domain, unitTitle, lessonNumber, objective);
        var matchKeywords = BuildMatchKeywords(domain, unitTitle, lessonTitle, primaryMaterialTitle, supportMaterialTitle);

        return new LessonSeed(
            taskSlug,
            lessonNumber,
            lessonTitle,
            goal,
            openingForAdult,
            anchorQuestion,
            lessonNumber switch
            {
                1 => "Leia ou apresente agora",
                2 => "Prática guiada",
                3 => "Consolidação",
                4 => "Revisão da unidade",
                _ => "Prova da unidade"
            },
            narrative.Title,
            narrative.Paragraphs,
            adultSteps,
            adultQuestions,
            acceptableAnswers,
            practiceTask,
            completionDefinition,
            evidencePrompt,
            expectedOutcome,
            suggestedMinutes,
            matchKeywords,
            primaryMaterialTitle,
            supportMaterialTitle);
    }

    private static NarrativeSeed BuildCoreNarrative(int age, LearningDomain domain, string unitTitle, int phaseNumber)
    {
        return domain switch
        {
            LearningDomain.Language when age <= 5 => new NarrativeSeed(
                "História do dia",
                [
                    $"Na roda de leitura, Sara ouviu um trecho curto sobre {unitTitle.ToLowerInvariant()} e acompanhou cada palavra com o dedo, sem pressa. Quando a voz do adulto fazia pausa, ela olhava de novo para o cartaz e tentava descobrir onde a frase começava e onde terminava.",
                    "Na segunda vez, Sara percebeu uma palavra que voltava no texto e apontou para a parte mais importante antes de falar qualquer resposta. Em vez de repetir por imitação, ela começou a ligar som, imagem e sentido no mesmo momento.",
                    "No fim, Sara contou com palavras simples o que entendeu e desenhou a parte que mais chamou sua atenção. A leitura ficou curta, mas clara, e o texto deixou de ser só som para virar compreensão."
                ]),
            LearningDomain.Language when age <= 7 => new NarrativeSeed(
                "Texto do dia",
                [
                    $"No mural da classe havia um texto curto sobre {unitTitle.ToLowerInvariant()}. Daniel leu primeiro devagar, marcando com o dedo as palavras que já reconhecia, e depois voltou ao começo para perceber qual frase realmente carregava a ideia principal.",
                    "Enquanto relia, Daniel separou o que era detalhe do que era essencial. Ele descobriu que uma boa resposta não depende de copiar tudo, mas de escolher a informação certa e falar com começo, meio e fim.",
                    "Quando terminou, Daniel explicou o texto com uma frase completa e mostrou no caderno a palavra ou o trecho que sustentava sua resposta. Assim, a leitura virou pensamento organizado."
                ]),
            LearningDomain.Language when age <= 10 => new NarrativeSeed(
                "Texto do dia",
                [
                    $"O texto do dia apresentava uma situação ligada a {unitTitle.ToLowerInvariant()} e pedia atenção aos detalhes que realmente sustentavam a ideia central. Elisa leu uma vez para entender o assunto geral e depois releu procurando pistas mais fortes.",
                    "Na releitura, ela percebeu que algumas informações pareciam importantes, mas só duas realmente explicavam o ponto principal. Foi nesse momento que a leitura deixou de ser apenas passagem de olhos e começou a virar análise.",
                    "Ao fechar a página, Elisa conseguiu resumir o texto com clareza, citando a ideia central e os detalhes que a comprovavam. O foco não ficou em escrever muito, e sim em responder bem."
                ]),
            LearningDomain.Language => new NarrativeSeed(
                "Texto de leitura",
                [
                    $"O material desta lição discute {unitTitle.ToLowerInvariant()} em linguagem compatível com {BuildPlacementShortLabel(age).ToLowerInvariant()}. Em vez de apresentar frases soltas, o texto foi escrito para exigir leitura atenta, seleção de evidências e resposta bem sustentada.",
                    "Ao longo do texto, aparecem pistas que podem confirmar, ampliar ou até tensionar a interpretação inicial do leitor. Por isso, a compreensão não termina na primeira leitura: ela amadurece quando a criança compara trechos, pesa informações e organiza o raciocínio.",
                    "No fechamento, o que vale é construir uma resposta clara, mostrando qual ponto central foi encontrado e que passagens servem como prova. Ler bem, aqui, significa pensar antes de concluir."
                ]),
            LearningDomain.Math => new NarrativeSeed(
                "Problema do dia",
                [
                    $"Na banca da feira da igreja, Miguel recebeu a tarefa de organizar produtos e conferir quantidades. O problema parecia simples no começo, mas logo ficou claro que {unitTitle.ToLowerInvariant()} exigia olhar atento para os dados antes de qualquer conta.",
                    "Ele desenhou a situação, agrupou o que era parecido e testou uma estratégia de cada vez. Quando uma resposta parecia rápida demais, Miguel voltava ao enunciado para conferir se o resultado combinava com o que realmente estava sobre a mesa.",
                    "No fim, ele percebeu que resolver não era apenas chegar a um número. Era mostrar por que aquele número fazia sentido dentro do problema. Essa é a parte mais importante da matemática desta lição."
                ]),
            LearningDomain.Science => new NarrativeSeed(
                "Observação do dia",
                [
                    $"A investigação de hoje começa com uma observação concreta sobre {unitTitle.ToLowerInvariant()}. Sofia notou que pequenas mudanças em um objeto, em um ser vivo ou em um material quase sempre revelam uma lógica escondida para quem aprende a observar sem pressa.",
                    "Por isso, ela primeiro descreveu o que viu, depois levantou uma hipótese e só então comparou a ideia inicial com o que o experimento ou a imagem mostravam. Nem toda expectativa se confirmou, e isso fez parte do aprendizado.",
                    "Ao final, Sofia conseguiu explicar o fenômeno com linguagem simples, mas precisa. A ciência se fortaleceu quando observação, hipótese e evidência começaram a caminhar juntas."
                ]),
            LearningDomain.History => new NarrativeSeed(
                "Fonte do dia",
                [
                    $"Dona Celina abriu uma pasta com cartas, fotografias e anotações antigas para conversar sobre {unitTitle.ToLowerInvariant()}. Cada documento parecia pequeno sozinho, mas juntos eles começavam a mostrar como as pessoas viveram, decidiram e mudaram o lugar onde estavam.",
                    "Ao observar as pistas, a criança percebeu que história não é só decorar datas. É entender a ordem dos fatos, reconhecer causas e perceber que uma mudança quase sempre deixa vestígios.",
                    "Quando a leitura termina, fica mais fácil explicar o que veio primeiro, o que mudou depois e por que aquilo importa. É assim que o passado começa a fazer sentido no presente."
                ]),
            LearningDomain.Geography => new NarrativeSeed(
                "Leitura do lugar",
                [
                    $"O estudo de hoje começa com um lugar real ligado a {unitTitle.ToLowerInvariant()}. Pode ser uma rua, um bairro, um mapa simples ou uma paisagem conhecida. O importante é perceber que o espaço nunca é neutro: ele mostra escolhas, usos e relações.",
                    "Ao comparar dois pontos do território, a criança nota diferença de circulação, de moradia, de vegetação ou de atividade humana. O mapa deixa de ser desenho parado e vira forma de entender a vida concreta.",
                    "Quando a leitura termina, a criança já consegue dizer o que vê, o que muda e por que aquele espaço funciona daquele jeito. É nesse momento que a geografia ganha sentido."
                ]),
            LearningDomain.ExecutiveFunction => new NarrativeSeed(
                "Cena do dia",
                [
                    $"João queria resolver tudo rápido, mas a rotina de {unitTitle.ToLowerInvariant()} pedia outra atitude: preparar, executar e revisar. Quando ele separou o material certo antes de começar, percebeu que a tarefa ficava menor e mais possível.",
                    "Durante o processo, João travou uma vez, respirou e voltou ao combinado inicial. Em vez de abandonar a missão, ele marcou o que já tinha feito e escolheu o passo seguinte com mais clareza.",
                    "Ao final, revisou a entrega, guardou o que usou e identificou o ponto em que quase se perdeu. Essa revisão curta fez a autonomia crescer de verdade."
                ]),
            _ => new NarrativeSeed(
                "Leitura do dia",
                [
                    $"Esta unidade trabalha {unitTitle.ToLowerInvariant()} com uma situação curta e observável.",
                    "A leitura foi escrita para abrir conversa boa, observação e resposta clara.",
                    "No fim, a criança deve conseguir dizer o que entendeu e mostrar isso de forma simples."
                ])
        };
    }

    private static string BuildOpeningForAdult(
        LearningDomain domain,
        int age,
        string subjectLabel,
        string unitLabel,
        string unitTitle,
        int lessonNumber) => lessonNumber switch
    {
        1 => domain switch
        {
            LearningDomain.Language when age <= 5 => $"Diga: \"Vamos ouvir com calma e descobrir juntos o que esta leitura está mostrando sobre {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.Language => $"Diga: \"Leia primeiro até o fim e depois me conte qual parte melhor explica {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.Math => $"Diga: \"Antes de fazer conta, vamos entender a situação e organizar o que o problema está pedindo em {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.Science => $"Diga: \"Observe primeiro, sem responder correndo. Quero ver o que você percebe em {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.History => $"Diga: \"Vamos ler esta fonte com calma e descobrir o que ela mostra sobre {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.Geography => $"Diga: \"Olhe bem para este lugar e me diga o que ele revela sobre {unitTitle.ToLowerInvariant()}.\"",
            LearningDomain.ExecutiveFunction => $"Diga: \"Hoje vamos começar esta missão passo a passo, sem fazer tudo de uma vez.\"",
            _ => $"Diga: \"Vamos abrir a lição de {subjectLabel.ToLowerInvariant()} com calma e entender primeiro o que está sendo estudado.\""
        },
        2 => $"Diga: \"Agora faça a atividade principal de {unitTitle.ToLowerInvariant()} com mais autonomia, mas sem pular a organização.\"",
        3 => $"Diga: \"Quero ver você fechar {unitTitle.ToLowerInvariant()} com uma resposta mais clara e menos ajuda.\"",
        4 => $"Diga: \"Vamos revisar o que já entrou bem em {unitLabel.ToLowerInvariant()} e corrigir só o que ainda está inseguro.\"",
        _ => $"Diga: \"Agora é hora de mostrar sozinho o que já aprendeu em {unitLabel.ToLowerInvariant()}.\""
    };

    private static string BuildLessonGoal(
        LearningDomain domain,
        int age,
        string unitTitle,
        int lessonNumber,
        string primaryMaterialTitle) => lessonNumber switch
    {
        1 => domain switch
        {
            LearningDomain.Language when age <= 5 => $"Ouvir a leitura, reconhecer a parte mais importante e responder sobre {unitTitle.ToLowerInvariant()} com fala, apontar ou desenho.",
            LearningDomain.Language => $"Ler o texto do dia, encontrar a ideia central e mostrar a pista mais forte sobre {unitTitle.ToLowerInvariant()}.",
            LearningDomain.Math => $"Entender a situação de {unitTitle.ToLowerInvariant()}, organizar os dados e escolher uma estratégia antes do cálculo.",
            LearningDomain.Science => $"Observar o fenômeno de {unitTitle.ToLowerInvariant()}, levantar uma hipótese e registrar a primeira descoberta.",
            LearningDomain.History => $"Ler a fonte de {unitTitle.ToLowerInvariant()}, colocar os fatos em ordem e perceber o que mudou.",
            LearningDomain.Geography => $"Observar o espaço estudado em {unitTitle.ToLowerInvariant()} e localizar as referências mais importantes.",
            LearningDomain.ExecutiveFunction => $"Começar a missão de {unitTitle.ToLowerInvariant()} com material preparado e primeiro passo visível.",
            _ => $"Abrir {unitTitle.ToLowerInvariant()} com compreensão clara do ponto principal."
        },
        2 => domain switch
        {
            LearningDomain.Language when age <= 5 => $"Responder sobre {unitTitle.ToLowerInvariant()} com fala, desenho ou registro curto ligado ao texto ouvido.",
            LearningDomain.Language => $"Responder à pergunta principal de {unitTitle.ToLowerInvariant()} com frase clara e prova curta.",
            LearningDomain.Math => $"Resolver a tarefa de {unitTitle.ToLowerInvariant()} mostrando conta, desenho, agrupamento ou tabela.",
            LearningDomain.Science => $"Registrar a investigação de {unitTitle.ToLowerInvariant()} com observação e conclusão curta.",
            LearningDomain.History => $"Explicar uma causa, mudança ou sequência de {unitTitle.ToLowerInvariant()} com apoio da fonte lida.",
            LearningDomain.Geography => $"Comparar lugares, pontos ou usos do território em {unitTitle.ToLowerInvariant()} e registrar a diferença principal.",
            LearningDomain.ExecutiveFunction => $"Executar a missão de {unitTitle.ToLowerInvariant()} sustentando a sequência combinada até o fim.",
            _ => $"Praticar {unitTitle.ToLowerInvariant()} com resposta guiada e registro claro."
        },
        3 => domain switch
        {
            LearningDomain.Language => $"Fechar {unitTitle.ToLowerInvariant()} com resposta organizada, menos ajuda e melhor justificativa.",
            LearningDomain.Math => $"Conferir a solução de {unitTitle.ToLowerInvariant()} e justificar por que o resultado faz sentido.",
            LearningDomain.Science => $"Explicar o que foi descoberto em {unitTitle.ToLowerInvariant()} usando evidência observável.",
            LearningDomain.History => $"Sintetizar {unitTitle.ToLowerInvariant()} em ordem, com fato principal e consequência clara.",
            LearningDomain.Geography => $"Explicar o território de {unitTitle.ToLowerInvariant()} com leitura mais completa do espaço.",
            LearningDomain.ExecutiveFunction => $"Revisar e fechar {unitTitle.ToLowerInvariant()} com menos lembretes e mais autocorreção.",
            _ => $"Consolidar {unitTitle.ToLowerInvariant()} com resposta mais autônoma."
        },
        4 => $"Revisar {unitTitle.ToLowerInvariant()} e corrigir o que ainda não está firme antes da avaliação.",
        _ => $"Aplicar o teste de {unitTitle.ToLowerInvariant()} e registrar o resultado final com clareza."
    };

    private static string BuildLessonTitle(LearningDomain domain, int age, string unitTitle, int lessonNumber) => domain switch
    {
        LearningDomain.Language when lessonNumber == 1 => "Leitura do dia",
        LearningDomain.Language when lessonNumber == 2 => "Resposta sobre a leitura",
        LearningDomain.Language when lessonNumber == 3 => "Leitura com resposta completa",
        LearningDomain.Language when lessonNumber == 4 => "Revisão de leitura",
        LearningDomain.Language => "Teste de leitura",

        LearningDomain.Math when lessonNumber == 1 => "Problema do dia",
        LearningDomain.Math when lessonNumber == 2 => "Registro da solução",
        LearningDomain.Math when lessonNumber == 3 => "Conferência da resposta",
        LearningDomain.Math when lessonNumber == 4 => "Revisão de matemática",
        LearningDomain.Math => "Teste de matemática",

        LearningDomain.Science when lessonNumber == 1 => "Observação do dia",
        LearningDomain.Science when lessonNumber == 2 => "Registro da investigação",
        LearningDomain.Science when lessonNumber == 3 => "Conclusão da investigação",
        LearningDomain.Science when lessonNumber == 4 => "Revisão de ciências",
        LearningDomain.Science => "Teste de ciências",

        LearningDomain.History when lessonNumber == 1 => "Fonte do dia",
        LearningDomain.History when lessonNumber == 2 => "Causa e consequência",
        LearningDomain.History when lessonNumber == 3 => "Síntese histórica",
        LearningDomain.History when lessonNumber == 4 => "Revisão de história",
        LearningDomain.History => "Teste de história",

        LearningDomain.Geography when lessonNumber == 1 => "Leitura do lugar",
        LearningDomain.Geography when lessonNumber == 2 => "Comparação de espaços",
        LearningDomain.Geography when lessonNumber == 3 => "Explicação do território",
        LearningDomain.Geography when lessonNumber == 4 => "Revisão de geografia",
        LearningDomain.Geography => "Teste de geografia",

        LearningDomain.ExecutiveFunction when lessonNumber == 1 => "Missão do dia",
        LearningDomain.ExecutiveFunction when lessonNumber == 2 => "Execução da missão",
        LearningDomain.ExecutiveFunction when lessonNumber == 3 => "Revisão da missão",
        LearningDomain.ExecutiveFunction when lessonNumber == 4 => "Checklist da rotina",
        LearningDomain.ExecutiveFunction => "Teste de autonomia",

        _ => $"{unitTitle} • lição {lessonNumber}"
    };

    private static string BuildAnchorQuestion(LearningDomain domain, int age, string unitTitle, int lessonNumber) => domain switch
    {
        LearningDomain.Language when age <= 5 => $"O que você consegue contar, apontar ou desenhar para mostrar {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Language when lessonNumber == 1 => $"Qual é a ideia mais importante deste material sobre {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Language when lessonNumber == 2 => $"Que frase ou resposta curta mostra {unitTitle.ToLowerInvariant()} com clareza?",
        LearningDomain.Language => $"Como você prova sua resposta sobre {unitTitle.ToLowerInvariant()} sem copiar tudo?",

        LearningDomain.Math when lessonNumber == 1 => $"Como você monta {unitTitle.ToLowerInvariant()} com objetos, desenho ou tabela?",
        LearningDomain.Math when lessonNumber == 2 => $"Que conta, desenho ou esquema prova sua escolha em {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Math => $"Como você confere se a resposta de {unitTitle.ToLowerInvariant()} faz sentido?",

        LearningDomain.Science when lessonNumber == 1 => $"O que você observou primeiro em {unitTitle.ToLowerInvariant()} e o que acha que vai acontecer?",
        LearningDomain.Science when lessonNumber == 2 => $"Que registro mostra melhor o que aconteceu em {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Science => $"O que você descobriu em {unitTitle.ToLowerInvariant()} e como consegue explicar isso?",

        LearningDomain.History when lessonNumber == 1 => $"O que veio primeiro em {unitTitle.ToLowerInvariant()} e o que mudou depois?",
        LearningDomain.History when lessonNumber == 2 => $"Qual causa ajuda a explicar melhor {unitTitle.ToLowerInvariant()}?",
        LearningDomain.History => $"Como você resumiria {unitTitle.ToLowerInvariant()} em ordem e com sentido?",

        LearningDomain.Geography when lessonNumber == 1 => $"Que lugar, mapa ou referência ajuda a abrir {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Geography when lessonNumber == 2 => $"O que muda no espaço quando você compara {unitTitle.ToLowerInvariant()}?",
        LearningDomain.Geography => $"Que leitura do território resume melhor {unitTitle.ToLowerInvariant()}?",

        LearningDomain.ExecutiveFunction when lessonNumber == 1 => $"Qual é o primeiro passo visível de {unitTitle.ToLowerInvariant()}?",
        LearningDomain.ExecutiveFunction when lessonNumber == 2 => $"Como você sabe que está no meio certo de {unitTitle.ToLowerInvariant()}?",
        LearningDomain.ExecutiveFunction => $"Que sinal mostra que {unitTitle.ToLowerInvariant()} terminou de verdade?",

        _ => $"Como você pratica {unitTitle.ToLowerInvariant()} nesta lição?"
    };

    private static List<string> BuildAdultSteps(
        LearningDomain domain,
        int age,
        string unitTitle,
        int lessonNumber,
        NarrativeSeed narrative,
        IReadOnlyList<string> materials)
    {
        var materialText = materials.Count == 0
            ? "o material da lição"
            : string.Join(", ", materials.Take(4));

        return domain switch
        {
            LearningDomain.Language when age <= 5 =>
            [
                "Leia o texto uma vez em voz alta, apontando as partes importantes sem pressa.",
                $"Peça que a criança fale, aponte ou desenhe algo que mostre {unitTitle.ToLowerInvariant()}.",
                "Repita a pergunta com apoio leve, mas sem entregar a resposta pronta.",
                $"Feche no desenho, no caderno ou na ficha usando {materialText}."
            ],
            LearningDomain.Language =>
            [
                "Leia o texto inteiro uma vez antes de pedir resposta.",
                $"Na releitura, peça que a criança marque o essencial em {unitTitle.ToLowerInvariant()}.",
                "Converse a resposta oral primeiro e só depois passe para o registro.",
                $"Feche no caderno usando {materialText} e revise a frase principal."
            ],
            LearningDomain.Math =>
            [
                $"Monte a situação de {unitTitle.ToLowerInvariant()} usando {materialText}.",
                "Peça que a criança resolva primeiro no concreto ou no desenho.",
                "Só depois leve a resposta para o símbolo, tabela ou cálculo.",
                "Feche pedindo que a criança explique por que a resposta faz sentido."
            ],
            LearningDomain.Science =>
            [
                $"Abra {unitTitle.ToLowerInvariant()} com observação real, usando {materialText}.",
                "Puxe uma hipótese simples antes do teste ou da comparação.",
                "Registre o que mudou, apareceu ou se repetiu durante a investigação.",
                "Feche com uma conclusão curta em linguagem própria."
            ],
            LearningDomain.History =>
            [
                $"Apresente {narrative.Title.ToLowerInvariant()} e deixe a criança localizar tempo, fato ou personagem.",
                "Monte a ordem do acontecimento antes de pedir interpretação.",
                "Pergunte pela causa principal e por um efeito observável.",
                "Feche com linha do tempo, fala curta ou síntese escrita."
            ],
            LearningDomain.Geography =>
            [
                $"Abra a lição mostrando mapa, trajeto, imagem ou cenário ligado a {unitTitle.ToLowerInvariant()}.",
                "Peça que a criança localize o lugar e use pelo menos uma referência concreta.",
                "Compare o espaço com outro lugar ou com o uso humano desse território.",
                "Feche com legenda, explicação ou registro do que aquele espaço revela."
            ],
            LearningDomain.ExecutiveFunction =>
            [
                $"Mostre a missão de {unitTitle.ToLowerInvariant()} já dividida em início, execução e fechamento.",
                "Peça que a criança diga qual passo vem primeiro antes de começar.",
                "Durante a tarefa, aponte só o próximo passo, sem abrir três instruções de uma vez.",
                "No final, revise o que foi entregue e combine a abertura da próxima missão."
            ],
            _ =>
            [
                $"Abra {unitTitle.ToLowerInvariant()} em passos curtos.",
                "Modele a primeira resposta.",
                "Peça a produção da criança.",
                "Feche com revisão do que foi entregue."
            ]
        };
    }

    private static List<string> BuildAdultQuestions(LearningDomain domain, int age, string unitTitle, int lessonNumber) => domain switch
    {
        LearningDomain.Language when age <= 5 =>
        [
            "O que você ouviu primeiro?",
            "Que parte você consegue contar com suas palavras?",
            $"Que desenho ou palavra ajuda a mostrar {unitTitle.ToLowerInvariant()}?"
        ],
        LearningDomain.Language =>
        [
            "Qual é a ideia principal deste trecho?",
            "Que detalhe realmente sustenta essa resposta?",
            "Como você diria isso sem copiar a leitura?"
        ],
        LearningDomain.Math =>
        [
            "Que dados o problema já entregou?",
            "Como você organizou a conta ou o desenho?",
            "Que conferência mostra que a resposta faz sentido?"
        ],
        LearningDomain.Science =>
        [
            "O que você observou primeiro?",
            "O que mudou durante o teste ou a comparação?",
            "Qual explicação final cabe no que você viu?"
        ],
        LearningDomain.History =>
        [
            "O que aconteceu primeiro?",
            "Qual causa ajuda a explicar o que veio depois?",
            "Que síntese mostra o fato sem embaralhar a ordem?"
        ],
        LearningDomain.Geography =>
        [
            "Que referência ajuda a localizar este lugar?",
            "O que muda quando você compara dois espaços?",
            "Como este território interfere na vida das pessoas?"
        ],
        LearningDomain.ExecutiveFunction =>
        [
            "Qual é o primeiro passo desta missão?",
            "O que ainda falta concluir?",
            "Que sinal mostra que você pode marcar como feito?"
        ],
        _ =>
        [
            $"O que você percebeu em {unitTitle.ToLowerInvariant()}?",
            "Como você organizou a resposta?",
            "O que mostra que a atividade terminou?"
        ]
    };

    private static List<string> BuildAcceptableAnswers(LearningDomain domain, int age, string unitTitle, int lessonNumber) => domain switch
    {
        LearningDomain.Language when age <= 5 =>
        [
            "Uma resposta oral curta com começo claro.",
            "Um desenho ou legenda que mostre a ideia da lição.",
            $"Uma fala em ordem que prove {unitTitle.ToLowerInvariant()}."
        ],
        LearningDomain.Language =>
        [
            "Uma frase principal escrita com sentido.",
            "Dois detalhes ou provas curtas que sustentem a resposta.",
            "Uma síntese em palavras próprias, sem copiar tudo."
        ],
        LearningDomain.Math =>
        [
            "Conta, desenho, tabela ou esquema coerente com o problema.",
            "Explicação curta de como os dados foram organizados.",
            "Conferência simples mostrando por que a resposta fecha."
        ],
        LearningDomain.Science =>
        [
            "Observação concreta e não vaga.",
            "Registro do que mudou, apareceu ou se confirmou.",
            "Conclusão que conversa com a hipótese ou com o resultado visto."
        ],
        LearningDomain.History =>
        [
            "Sequência correta do acontecimento.",
            "Uma causa ou consequência conectada ao fato estudado.",
            "Síntese que respeite tempo, contexto e linguagem própria."
        ],
        LearningDomain.Geography =>
        [
            "Uso correto de referência espacial.",
            "Comparação simples entre lugares, trajetos ou paisagens.",
            "Explicação de como o espaço interfere nas pessoas ou na rotina."
        ],
        LearningDomain.ExecutiveFunction =>
        [
            "Início da missão sem travar por muito tempo.",
            "Execução com foco em uma tarefa por vez.",
            "Fechamento com revisão rápida do que foi entregue."
        ],
        _ =>
        [
            "Resposta coerente com a proposta.",
            "Registro curto do que foi feito.",
            "Explicação final do aprendizado."
        ]
    };

    private static string BuildPracticeTask(LearningDomain domain, int age, string unitTitle, int lessonNumber, string primaryMaterialTitle) => domain switch
    {
        _ when lessonNumber == 4 => $"Revisar os passos principais de {unitTitle.ToLowerInvariant()}, corrigir o que ainda falha e marcar o que já entrou com segurança.",
        _ when lessonNumber == 5 => $"Aplicar uma prova curta de {unitTitle.ToLowerInvariant()} e registrar a resposta final sem ajuda excessiva.",
        LearningDomain.Language when age <= 5 => $"Ouvir a leitura, apontar a parte importante e responder sobre {unitTitle.ToLowerInvariant()} com fala, desenho ou registro curto.",
        LearningDomain.Language => $"Responder no caderno à pergunta central de {unitTitle.ToLowerInvariant()} e sustentar a resposta com uma ou duas provas.",
        LearningDomain.Math => $"Resolver a missão de {unitTitle.ToLowerInvariant()} no caderno, mostrando organização dos dados e a justificativa da resposta.",
        LearningDomain.Science => $"Registrar a observação ou o experimento de {unitTitle.ToLowerInvariant()} em frase, desenho, tabela ou conclusão curta.",
        LearningDomain.History => $"Montar uma linha de tempo, síntese ou quadro de causa e consequência que feche {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Geography => $"Produzir mapa, legenda, comparação de lugares ou explicação curta que mostre {unitTitle.ToLowerInvariant()}.",
        LearningDomain.ExecutiveFunction => $"Executar a missão de {unitTitle.ToLowerInvariant()} com checklist curto, revisão final e marcação do que foi concluído.",
        _ => $"Entregar uma produção curta e clara sobre {unitTitle.ToLowerInvariant()}."
    };

    private static string BuildCompletionDefinition(LearningDomain domain, string unitTitle, int lessonNumber) => domain switch
    {
        _ when lessonNumber == 4 => $"Marque como concluída quando a criança revisar os pontos principais de {unitTitle.ToLowerInvariant()} e mostrar o que ainda precisa ajustar antes da prova.",
        _ when lessonNumber == 5 => $"Marque como concluída quando a prova curta de {unitTitle.ToLowerInvariant()} for entregue com resposta clara e registro final guardável.",
        LearningDomain.Language => $"Marque como concluída quando a criança responder com clareza e registrar o essencial de {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Math => $"Marque como concluída quando a criança mostrar conta, desenho ou tabela coerente e explicar a solução de {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Science => $"Marque como concluída quando a criança registrar observação, hipótese ou conclusão observável em {unitTitle.ToLowerInvariant()}.",
        LearningDomain.History => $"Marque como concluída quando a criança organizar ordem, fato e causa de {unitTitle.ToLowerInvariant()} sem embaralhar a sequência.",
        LearningDomain.Geography => $"Marque como concluída quando a criança localizar, comparar e explicar o espaço estudado em {unitTitle.ToLowerInvariant()}.",
        LearningDomain.ExecutiveFunction => $"Marque como concluída quando a criança iniciar, executar e revisar {unitTitle.ToLowerInvariant()} com menos lembretes.",
        _ => $"Marque como concluída quando a entrega final de {unitTitle.ToLowerInvariant()} estiver clara."
    };

    private static string BuildEvidencePrompt(LearningDomain domain, string unitTitle, int lessonNumber) => domain switch
    {
        _ when lessonNumber == 4 => $"Guarde, se fizer sentido, uma foto do material revisado de {unitTitle.ToLowerInvariant()} com correções já marcadas.",
        _ when lessonNumber == 5 => $"Guarde a prova curta, a folha final ou um vídeo curto da criança fechando {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Language => $"Guarde foto do caderno, áudio de leitura ou vídeo curto da criança mostrando {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Math => $"Guarde foto da conta, da tabela ou do material concreto usado em {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Science => $"Guarde foto do experimento, do desenho científico ou do registro de conclusão de {unitTitle.ToLowerInvariant()}.",
        LearningDomain.History => $"Guarde foto da linha do tempo, do quadro de fatos ou do resumo histórico de {unitTitle.ToLowerInvariant()}.",
        LearningDomain.Geography => $"Guarde foto do mapa, da legenda ou do registro territorial feito em {unitTitle.ToLowerInvariant()}.",
        LearningDomain.ExecutiveFunction => $"Guarde foto do checklist, da mesa pronta ou do fechamento da missão de {unitTitle.ToLowerInvariant()}.",
        _ => $"Guarde uma evidência curta do que foi produzido em {unitTitle.ToLowerInvariant()}."
    };

    private static string BuildExpectedOutcome(LearningDomain domain, string unitTitle, int lessonNumber, string objective) => domain switch
    {
        LearningDomain.Language => $"{objective} A criança termina a lição com leitura, resposta e registro mais claros.",
        LearningDomain.Math => $"{objective} A criança fecha a lição mostrando raciocínio e não só o resultado.",
        LearningDomain.Science => $"{objective} A criança termina com observação, registro e conclusão visíveis.",
        LearningDomain.History => $"{objective} A criança fecha a lição organizando fato, tempo e consequência.",
        LearningDomain.Geography => $"{objective} A criança termina a lição localizando, comparando e explicando o espaço estudado.",
        LearningDomain.ExecutiveFunction => $"{objective} A criança fecha a lição com mais previsibilidade e autonomia.",
        _ => objective
    };

    private static string BuildMatchKeywords(LearningDomain domain, string unitTitle, string lessonTitle, string primaryMaterialTitle, string supportMaterialTitle)
    {
        var domainKeywords = domain switch
        {
            LearningDomain.Language => "linguagem, leitura, escrita, texto, frase, reconto, opinião",
            LearningDomain.Math => "matemática, número, cálculo, problema, medida, tabela",
            LearningDomain.Science => "ciências, observação, experimento, hipótese, natureza, fenômeno",
            LearningDomain.History => "história, tempo, fonte, fato, linha do tempo, memória",
            LearningDomain.Geography => "geografia, mapa, lugar, território, região, paisagem",
            LearningDomain.ExecutiveFunction => "autonomia, rotina, checklist, revisar, concluir, foco",
            _ => "currículo"
        };

        return string.Join(", ",
        [
            domainKeywords,
            unitTitle,
            lessonTitle,
            primaryMaterialTitle,
            supportMaterialTitle
        ]);
    }

    private static string BuildPlacementShortLabel(int age) => age switch
    {
        3 => "Infantil 3",
        4 => "Infantil 4",
        5 => "Infantil 5",
        6 => "1º ano",
        7 => "2º ano",
        8 => "3º ano",
        9 => "4º ano",
        10 => "5º ano",
        11 => "6º ano",
        12 => "7º ano",
        13 => "8º ano",
        14 => "9º ano",
        _ when age <= 5 => "Infantil",
        _ => "Fundamental"
    };

    private static string BuildPlacementCode(int age) => age switch
    {
        3 => "infantil-3",
        4 => "infantil-4",
        5 => "infantil-5",
        6 => "1-ano",
        7 => "2-ano",
        8 => "3-ano",
        9 => "4-ano",
        10 => "5-ano",
        11 => "6-ano",
        12 => "7-ano",
        13 => "8-ano",
        14 => "9-ano",
        _ => "fundamental"
    };

    private static string BuildDomainCode(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "linguagem",
        LearningDomain.Math => "matematica",
        LearningDomain.Science => "ciencias",
        LearningDomain.History => "historia",
        LearningDomain.Geography => "geografia",
        LearningDomain.ExecutiveFunction => "autonomia",
        _ => "geral"
    };

    private sealed record NarrativeSeed(string Title, IReadOnlyList<string> Paragraphs);

    private sealed record LessonSeed(
        string TaskSlug,
        int LessonNumber,
        string Title,
        string Goal,
        string OpeningForAdult,
        string AnchorQuestion,
        string CoreMaterialLabel,
        string CoreMaterialTitle,
        IReadOnlyList<string> CoreMaterialParagraphs,
        IReadOnlyList<string> AdultSteps,
        IReadOnlyList<string> AdultQuestions,
        IReadOnlyList<string> AcceptableAnswers,
        string PracticeTask,
        string CompletionDefinition,
        string EvidencePrompt,
        string ExpectedOutcome,
        int SuggestedMinutes,
        string MatchKeywords,
        string PrimaryMaterialTitle,
        string SupportMaterialTitle);

    private sealed record UnitSeed(
        string SubjectLabel,
        string SchoolPlacementLabel,
        int PhaseNumber,
        string PhaseLabel,
        int UnitNumber,
        string UnitLabel,
        string Title,
        string Summary,
        string Objective,
        string ParentGuide,
        string TaskPrompt,
        string CompletionSignal,
        string OptionalEvidenceNote,
        string CompanionBookTitle,
        string PrintableTitle,
        string AssessmentTitle,
        IReadOnlyList<LessonSeed> Lessons,
        IReadOnlyList<string> Materials);
}
