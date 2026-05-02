using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class PortuguesePlanningService(ApplicationDbContext db)
{
    public async Task<PortuguesePlanningViewModel> BuildAsync(Guid parentId, Guid? childId)
    {
        var children = await db.Children
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.FullName)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.BirthDate
            })
            .ToListAsync();

        var today = DateTime.Today;
        var childOptions = children
            .Select(x =>
            {
                var age = CalculateAge(x.BirthDate, today);
                return new PlanningChildOptionViewModel
                {
                    Id = x.Id,
                    Name = x.FullName,
                    Age = age,
                    StageLabel = GetSchoolPlacement(age)
                };
            })
            .ToList();

        var stages = BuildStages();
        var selectedChild = childOptions.FirstOrDefault(x => x.Id == childId);
        var selectedStage = selectedChild is null
            ? null
            : stages.FirstOrDefault(x => x.Age == selectedChild.Age);

        return new PortuguesePlanningViewModel
        {
            SubjectName = "Lingua Portuguesa",
            ScopeLabel = "Planejamento pedagogico BNCC",
            ScopeSummary = "Planejamento de Lingua Portuguesa para criancas de 4 a 8 anos, alinhado a etapa/ano escolar de referencia, com foco anual, organizacao bimestral, evidencias e orientacoes para uso em casa.",
            SelectedChildId = selectedChild?.Id,
            SelectedChildName = selectedChild?.Name,
            SelectedChildAge = selectedChild?.Age,
            SelectedStageLabel = selectedStage?.SchoolPlacement,
            SelectedStageSummary = selectedStage?.AnnualObjective,
            Children = childOptions,
            Stages = stages
        };
    }

    public PortuguesePlanningGuidance? GetDailyGuidance(int age, DateTime targetDate)
    {
        if (age < 4 || age > 8)
        {
            return null;
        }

        var stages = BuildStages();
        var stage = stages.First(x => x.Age == age);
        var termIndex = ResolveTermIndex(targetDate);
        var term = stage.Terms[termIndex];

        return new PortuguesePlanningGuidance
        {
            Age = age,
            StageLabel = stage.StageLabel,
            SchoolPlacement = stage.SchoolPlacement,
            TermLabel = term.TermLabel,
            MainFocus = term.MainFocus,
            Summary = $"No {term.TermLabel.ToLowerInvariant()}, o foco de Lingua Portuguesa para {stage.SchoolPlacement.ToLowerInvariant()} e {term.MainFocus.ToLowerInvariant()}.",
            DailyTheme = $"Portugues BNCC - {term.TermLabel}: {term.MainFocus}",
            PriorityKeywords = BuildPriorityKeywords(age, termIndex),
            PreferredTemplateTitles = BuildPreferredTemplateTitles(age, termIndex),
            RecommendedDailyLanguageBlocks = 2
        };
    }

    public PortuguesePlanningStageViewModel? GetStageForAge(int age)
    {
        if (age < 4 || age > 8)
        {
            return null;
        }

        return BuildStages().FirstOrDefault(x => x.Age == age);
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

    private static string GetSchoolPlacement(int age) => age switch
    {
        4 => "Educacao Infantil - crianca pequena (4 anos)",
        5 => "Educacao Infantil - crianca pequena (5 anos)",
        6 => "1o ano do Ensino Fundamental",
        7 => "2o ano do Ensino Fundamental",
        8 => "3o ano do Ensino Fundamental",
        _ => "Faixa fora do escopo atual"
    };

    private static int ResolveTermIndex(DateTime targetDate) => targetDate.Month switch
    {
        >= 1 and <= 3 => 0,
        >= 4 and <= 6 => 1,
        >= 7 and <= 9 => 2,
        _ => 3
    };

    private static List<string> BuildPriorityKeywords(int age, int termIndex) => (age, termIndex) switch
    {
        (4, 0) => ["escuta", "historia", "reconto", "vocabulario", "imagem", "roda"],
        (4, 1) => ["rima", "sons", "parlenda", "palmas", "aliteracao"],
        (4, 2) => ["reconto", "sequencia", "descricao", "historia", "personagem"],
        (4, 3) => ["nome", "palavras", "legenda", "lista", "escrita espontanea"],

        (5, 0) => ["reconto", "imagem", "historia", "vocabulario", "descricao"],
        (5, 1) => ["rima", "sons", "nome", "palavras", "silabas"],
        (5, 2) => ["lista", "bilhete", "legenda", "escrita", "palavras"],
        (5, 3) => ["reconto", "palavras", "escrita", "poemas", "rotina"],

        (6, 0) => ["nome", "letras", "palavras", "alfabetizacao", "som", "rima"],
        (6, 1) => ["silabas", "palavras", "frases", "som", "grafia", "leitura"],
        (6, 2) => ["frases", "texto", "compreensao", "leitura", "escrita"],
        (6, 3) => ["fluencia", "bilhete", "lista", "reconto", "convite"],

        (7, 0) => ["fluencia", "historias", "frases", "leitura", "reconto"],
        (7, 1) => ["bilhetes", "convites", "regras", "avisos", "texto funcional"],
        (7, 2) => ["ortografia", "pontuacao", "relato", "reconto", "frases"],
        (7, 3) => ["texto", "leitura", "poema", "relato", "conto"],

        (8, 0) => ["fluencia", "texto", "personagens", "informacoes", "poemas"],
        (8, 1) => ["inferencia", "ideia principal", "paragrafo", "resumo", "texto"],
        (8, 2) => ["genero", "planejamento", "revisao", "narrativa", "instrucao"],
        (8, 3) => ["literaria", "narrativa", "opiniao", "autoria", "texto informativo"],

        _ => ["leitura", "escrita", "compreensao"]
    };

    private static List<string> BuildPreferredTemplateTitles(int age, int termIndex) => (age, termIndex) switch
    {
        (4, 0) => ["Escuta e reconto", "Frase oral com clareza"],
        (4, 1) => ["Rimas e aliteracao", "Consciencia fonologica inicial", "Silabas com palmas"],
        (4, 2) => ["Escuta e reconto", "Frase oral com clareza"],
        (4, 3) => ["Tracado funcional inicial", "Pre-escrita de palavras conhecidas", "Correspondencia som-letra inicial"],

        (5, 0) => ["Escuta e reconto com comeco, meio e fim", "Leitura de imagens", "Frase com sentido"],
        (5, 1) => ["Pre-alfabetizacao fonica", "Silabas e palavras familiares", "Vocabulario por categorias"],
        (5, 2) => ["Escrita inicial apoiada", "Leitura de palavras simples", "Frase com sentido"],
        (5, 3) => ["Escuta e reconto com comeco, meio e fim", "Escrita inicial apoiada", "Pre-alfabetizacao fonica"],

        (6, 0) => ["Leitura guiada de frases", "Segmentacao e ortografia inicial", "Fluencia inicial de leitura"],
        (6, 1) => ["Fluencia inicial de leitura", "Segmentacao e ortografia inicial", "Leitura guiada de frases"],
        (6, 2) => ["Leitura de pequeno texto", "Compreensao literal", "Escrita de frases"],
        (6, 3) => ["Leitura de pequeno texto", "Escrita com pontuacao apoiada", "Reconto com detalhes"],

        (7, 0) => ["Compreensao de texto curto", "Resumo oral e escrito", "Ortografia em palavras frequentes"],
        (7, 1) => ["Compreensao de texto curto", "Producao de texto guiado", "Resumo oral e escrito"],
        (7, 2) => ["Ortografia em palavras frequentes", "Escrita de pequeno paragrafo", "Producao de texto guiado"],
        (7, 3) => ["Leio, penso e argumento", "Resumo oral e escrito", "Producao de texto guiado"],

        (8, 0) => ["Leitura orientada por pistas", "Resumo de texto curto", "Resposta com evidencia"],
        (8, 1) => ["Leitura com inferencia", "Paragrafo com ideia central", "Resumo de texto curto"],
        (8, 2) => ["Producao de paragrafo", "Revisao de frase e conectivo", "Ampliacao de repertorio escrito"],
        (8, 3) => ["Leitura com inferencia", "Producao de paragrafo", "Resposta com evidencia"],

        _ => []
    };

    private static List<PortuguesePlanningStageViewModel> BuildStages() =>
    [
        new PortuguesePlanningStageViewModel
        {
            Age = 4,
            StageLabel = "4 anos",
            SchoolPlacement = "Educacao Infantil - crianca pequena (4 anos)",
            BnccAnchor = "Na Educacao Infantil, a BNCC organiza a linguagem em direitos de aprendizagem e no campo de experiencias 'Escuta, fala, pensamento e imaginacao', preservando interacoes e brincadeira como eixo.",
            AnnualObjective = "Ampliar repertorio oral, escuta atenta, reconto, vocabulario e consciencia sonora inicial, com escrita espontanea e marcas graficas com intencao comunicativa, sem antecipar formalmente a alfabetizacao.",
            DiagnosticFocus = "Observar se a crianca participa de rodas, compreende consignas simples, reconta trechos de historias, brinca com rimas, reconhece o proprio nome e atribui sentido a desenhos e marcas.",
            PedagogicalApproach = "Trabalhar linguagem de forma ludica, com leitura diaria em voz alta, conversa dirigida, musica, parlendas, reconto com imagens, jogos sonoros e propostas curtas de escrita espontanea.",
            BnccReferences =
            [
                "BNCC Educacao Infantil: direitos de aprendizagem, campos de experiencias e objetivos organizados por faixa etaria (pp. 26-27).",
                "Campo de experiencias 'Escuta, fala, pensamento e imaginacao' para criancas pequenas (4 a 5 anos e 11 meses).",
                "Referencias operacionais: EI03EF01, EI03EF03, EI03EF05, EI03EF07, EI03EF08 e EI03EF09."
            ],
            ExpectedOutcomes =
            [
                "Participa de rodas de conversa, responde e faz pequenos relatos.",
                "Reconta historias com apoio de imagens e sequencia de fatos.",
                "Brinca com rimas, aliteracoes e repeticoes orais.",
                "Reconhece o proprio nome e algumas palavras estaveis do cotidiano.",
                "Produz desenhos, legendas orais e escritas espontaneas com intencao."
            ],
            SuggestedEvidence =
            [
                "Audio de reconto oral ou participacao em roda.",
                "Foto de desenho com legenda ditada ou escrita espontanea.",
                "Registro de brincadeira com rimas, parlendas ou cantigas.",
                "Checklist de participacao, escuta e reconto."
            ],
            FamilyRoutineActions =
            [
                "Ler em voz alta todos os dias por 10 a 15 minutos.",
                "Usar nome proprio, lista da casa e bilhetes simples como contexto real de linguagem.",
                "Brincar com rimas, sons iniciais e reconto de historias sem cobrar acerto formal.",
                "Registrar falas interessantes da crianca para voltar a elas em novos contextos."
            ],
            Terms =
            [
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "1o bimestre",
                    MainFocus = "Escuta, participacao oral e repertorio de historias",
                    LearningGoals =
                    [
                        "Escutar historias curtas ate o fim e comentar personagens, objetos e acoes.",
                        "Participar de rodas com fala breve e turnos simples.",
                        "Ampliar vocabulario de rotina, corpo, familia e brincadeiras."
                    ],
                    SuggestedGenres = ["Contos acumulativos", "Cantigas", "Parlendas", "Rodas de conversa"],
                    AssessmentSignals =
                    [
                        "Mantem atencao por mais tempo em leitura mediada.",
                        "Responde a perguntas literais sobre a historia.",
                        "Usa palavras novas em contexto."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "2o bimestre",
                    MainFocus = "Consciencia sonora inicial e jogos com a fala",
                    LearningGoals =
                    [
                        "Perceber repeticoes, rimas e sons iniciais em palavras familiares.",
                        "Bater palmas para pedacos orais de palavras conhecidas.",
                        "Relacionar oralidade, musica e memoria verbal."
                    ],
                    SuggestedGenres = ["Parlendas", "Trava-linguas", "Cantigas", "Jogos sonoros"],
                    AssessmentSignals =
                    [
                        "Reconhece rimas em pares simples.",
                        "Repete sequencias orais com apoio ritmico.",
                        "Participa de jogos de escuta e imitacao verbal."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "3o bimestre",
                    MainFocus = "Reconto, descricao e sequencia narrativa",
                    LearningGoals =
                    [
                        "Recontar partes de historias com comeco, durante e final apoiados por imagens.",
                        "Descrever personagens, cenarios e acoes.",
                        "Organizar oralmente pequenas sequencias do cotidiano."
                    ],
                    SuggestedGenres = ["Contos", "Sequencias de imagens", "Relato de experiencia", "Livro sem palavras"],
                    AssessmentSignals =
                    [
                        "Reconstrui ao menos dois eventos da historia em ordem.",
                        "Nomeia personagens e acoes principais.",
                        "Usa conectores simples como 'depois' e 'ai'."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "4o bimestre",
                    MainFocus = "Nome proprio, palavras estaveis e escrita espontanea",
                    LearningGoals =
                    [
                        "Reconhecer o proprio nome e alguns nomes/palavras de alta relevancia.",
                        "Produzir marcas graficas, listas e legendas com intencao comunicativa.",
                        "Relacionar leitura de mundo, imagem e oralidade."
                    ],
                    SuggestedGenres = ["Listas", "Convites ilustrados", "Legendas", "Cartazes de rotina"],
                    AssessmentSignals =
                    [
                        "Reconhece o proprio nome em diferentes suportes.",
                        "Produz escrita espontanea e explica o que escreveu.",
                        "Usa desenho e fala para sustentar sentido."
                    ]
                }
            ]
        },
        new PortuguesePlanningStageViewModel
        {
            Age = 5,
            StageLabel = "5 anos",
            SchoolPlacement = "Educacao Infantil - crianca pequena (5 anos)",
            BnccAnchor = "A BNCC mantem a Educacao Infantil como etapa de experiencias e nao de escolarizacao precoce, mas amplia intencionalmente reconto, vocabulario, consciencia fonologica e escrita espontanea com mais organizacao.",
            AnnualObjective = "Consolidar linguagem oral, reconto com sequencia, ampliacao de vocabulario, consciencia fonologica mais consistente, reconhecimento de palavras estaveis e escrita espontanea em contextos reais de comunicacao.",
            DiagnosticFocus = "Verificar se a crianca relata fatos com mais detalhe, segmenta oralmente palavras, identifica rimas e sons iniciais, reconhece nome proprio e tenta escrever palavras de referencia com hipoteses pessoais.",
            PedagogicalApproach = "Planejar experiencias com livros, parlendas, reconto oral, sequencias de imagens, nome proprio, jogos de palavras e producoes espontaneas em listas, bilhetes, legendas e convites.",
            BnccReferences =
            [
                "BNCC Educacao Infantil: direitos de aprendizagem, campos de experiencias e transicao para o Ensino Fundamental.",
                "Campo 'Escuta, fala, pensamento e imaginacao' para criancas pequenas.",
                "Referencias operacionais: EI03EF01, EI03EF04, EI03EF05, EI03EF06, EI03EF07 e EI03EF09."
            ],
            ExpectedOutcomes =
            [
                "Reconta historias com mais clareza e encadeamento.",
                "Percebe rimas, aliteracoes e unidades sonoras em jogos orais.",
                "Reconhece nome proprio e palavras estaveis de uso frequente.",
                "Produz escritas espontaneas com mais relacao entre fala e registro.",
                "Participa de leitura compartilhada com comentario, antecipacao e inferencia simples."
            ],
            SuggestedEvidence =
            [
                "Reconto oral gravado com apoio de imagens.",
                "Foto de lista, convite, legenda ou bilhete espontaneo.",
                "Registro de jogo fonologico com observacoes do adulto.",
                "Portifolio com nome proprio, palavras estaveis e reconto ditado."
            ],
            FamilyRoutineActions =
            [
                "Manter leitura diaria e conversa sobre historias.",
                "Usar lista de compras, calendario, bilhete e nomeacoes da rotina como suportes reais.",
                "Brincar com pedacos sonoros de palavras, rimas e nomes da familia.",
                "Valorizar a escrita espontanea sem corrigir em excesso."
            ],
            Terms =
            [
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "1o bimestre",
                    MainFocus = "Oralidade, reconto e leitura de imagens",
                    LearningGoals =
                    [
                        "Recontar historias com apoio visual e estrutura basica.",
                        "Ampliar vocabulario para descrever cenas, personagens e acontecimentos.",
                        "Participar de combinados de escuta e fala em grupo."
                    ],
                    SuggestedGenres = ["Contos", "Album ilustrado", "Sequencia de imagens", "Relato de experiencia"],
                    AssessmentSignals =
                    [
                        "Retoma partes centrais da historia.",
                        "Usa mais detalhes nas descricoes.",
                        "Respeita turnos de fala com mediacao."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "2o bimestre",
                    MainFocus = "Consciencia fonologica e palavras estaveis",
                    LearningGoals =
                    [
                        "Perceber rimas, aliteracoes e semelhancas sonoras.",
                        "Comparar nomes e palavras conhecidas pelo som inicial e tamanho.",
                        "Reconhecer e usar nome proprio e palavras do cotidiano."
                    ],
                    SuggestedGenres = ["Parlendas", "Cantigas", "Listas", "Jogos com nomes"],
                    AssessmentSignals =
                    [
                        "Localiza palavras conhecidas em cartazes e listas.",
                        "Identifica sons iniciais em alguns vocabulos familiares.",
                        "Bate palmas para silabas de palavras simples."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "3o bimestre",
                    MainFocus = "Relacao fala-escrita e producao espontanea",
                    LearningGoals =
                    [
                        "Produzir listas, legendas e bilhetes com hipoteses de escrita.",
                        "Explicar oralmente o que escreveu.",
                        "Perceber que a escrita representa a fala com regularidades."
                    ],
                    SuggestedGenres = ["Listas", "Bilhetes", "Convites", "Legendas"],
                    AssessmentSignals =
                    [
                        "Produz escritas com intencao comunicativa clara.",
                        "Relaciona oralmente o que quis escrever.",
                        "Reutiliza palavras estaveis como referencia."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "4o bimestre",
                    MainFocus = "Preparacao para a alfabetizacao formal",
                    LearningGoals =
                    [
                        "Consolidar reconto, jogos sonoros e reconhecimento de palavras importantes.",
                        "Ampliar autonomia em atividades de leitura compartilhada e escrita espontanea.",
                        "Encerrar o ano com rotina de linguagem forte e prazerosa."
                    ],
                    SuggestedGenres = ["Livro repetitivo", "Cartaz de rotina", "Historias sequenciadas", "Poemas curtos"],
                    AssessmentSignals =
                    [
                        "Reconta com maior autonomia.",
                        "Reconhece repertorio estavel de nomes e palavras de uso frequente.",
                        "Mostra prontidao para entrar no 1o ano sem pressa de escolarizacao precoce."
                    ]
                }
            ]
        },
        new PortuguesePlanningStageViewModel
        {
            Age = 6,
            StageLabel = "6 anos",
            SchoolPlacement = "1o ano do Ensino Fundamental",
            BnccAnchor = "Nos anos iniciais, a BNCC organiza Lingua Portuguesa em oralidade, leitura/escuta, producao de textos e analise linguistica/semiotica, com alfabetizacao sistematizada especialmente nos dois primeiros anos.",
            AnnualObjective = "Iniciar a alfabetizacao com foco no sistema de escrita alfabetica, leitura de palavras e frases, escuta e compreensao de textos breves, producao de pequenas escritas e uso de linguagem em situacoes reais.",
            DiagnosticFocus = "Identificar hipóteses de escrita, consciencia fonemica e silabica, reconhecimento de letras, relacao som-grafia, interesse por livros, compreensao oral e repertorio de palavras do cotidiano.",
            PedagogicalApproach = "Combinar instrução explicita e ludica: rotina de leitura, consciencia fonologica, nome proprio, leitura compartilhada, escrita com apoio, generos de uso social e pratica curta diaria.",
            BnccReferences =
            [
                "BNCC Lingua Portuguesa - anos iniciais, p. 90: oralidade, leitura/escuta, producao de textos e analise linguistica/semiotica; alfabetizacao sistematizada nos dois primeiros anos.",
                "Referencias operacionais do 1o ano: familias EF01LP e EF15LP.",
                "Habilidades de articulacao do bloco alfabetico: familias EF12LP."
            ],
            ExpectedOutcomes =
            [
                "Relaciona letras e sons com repertorio crescente.",
                "Le palavras, frases e textos muito curtos com apoio.",
                "Escreve palavras e frases simples em contexto significativo.",
                "Compreende textos lidos pelo adulto e comenta informacoes explicitas.",
                "Produz pequenos registros como listas, legendas e bilhetes."
            ],
            SuggestedEvidence =
            [
                "Leitura gravada de lista, frase ou pequeno texto.",
                "Foto de escrita espontanea e escrita apoiada.",
                "Checklist de correspondencias letra-som e segmentacao.",
                "Registro de reconto e compreensao oral."
            ],
            FamilyRoutineActions =
            [
                "Manter rotina diaria curta de leitura compartilhada e de brincadeira com letras e sons.",
                "Usar nome proprio, lista de materiais, nomes da familia e palavras estaveis como ancora.",
                "Fazer microblocos de 10 a 15 minutos, sem excesso de copia.",
                "Priorizar qualidade e constancia, nao volume."
            ],
            Terms =
            [
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "1o bimestre",
                    MainFocus = "Entrada na alfabetizacao e principio alfabetico",
                    LearningGoals =
                    [
                        "Reconhecer letras, nome proprio e palavras estaveis.",
                        "Perceber relacao entre fala, palavra e escrita.",
                        "Avancar em jogos de rima, segmentacao oral e som inicial/final."
                    ],
                    SuggestedGenres = ["Listas", "Cantigas", "Parlendas", "Cartazes de rotina"],
                    AssessmentSignals =
                    [
                        "Reconhece nome proprio e algumas palavras-chave.",
                        "Distingue desenho, letra, numero e palavra.",
                        "Mostra hipoteses de escrita e maior consciencia sonora."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "2o bimestre",
                    MainFocus = "Correspondencias som-grafia e leitura de palavras",
                    LearningGoals =
                    [
                        "Ler e escrever silabas, palavras e frases curtas com apoio.",
                        "Compreender instrucoes e pequenos textos de rotina.",
                        "Produzir listas, legendas e frases curtas."
                    ],
                    SuggestedGenres = ["Listas", "Bilhetes", "Legenda", "Quadrinhas"],
                    AssessmentSignals =
                    [
                        "Relaciona som e grafia com mais seguranca.",
                        "Le palavras frequentes sem soletrar tudo.",
                        "Escreve palavras com hipoteses mais proximas do convencional."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "3o bimestre",
                    MainFocus = "Leitura compartilhada e escrita de frases",
                    LearningGoals =
                    [
                        "Ler frases e pequenos textos com forte mediacao.",
                        "Identificar informacoes explicitas em textos breves.",
                        "Escrever frases com espacos e sentido global."
                    ],
                    SuggestedGenres = ["Bilhetes", "Avisos", "Historias curtas", "Regras de brincadeira"],
                    AssessmentSignals =
                    [
                        "Le e compreende frases completas.",
                        "Responde oralmente ao que leu ou ouviu.",
                        "Escreve frases curtas com mais autonomia."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "4o bimestre",
                    MainFocus = "Consolidacao inicial da leitura e da escrita funcional",
                    LearningGoals =
                    [
                        "Ampliar fluencia inicial em repertorio conhecido.",
                        "Produzir pequenos textos de apoio do adulto para revisao.",
                        "Fechar o ano com rotina solida de leitura, escrita e compreensao."
                    ],
                    SuggestedGenres = ["Bilhete", "Lista", "Pequeno reconto", "Convite"],
                    AssessmentSignals =
                    [
                        "Le pequenos textos familiares com maior estabilidade.",
                        "Escreve pequenos registros com sentido.",
                        "Compreende a finalidade de generos do cotidiano."
                    ]
                }
            ]
        },
        new PortuguesePlanningStageViewModel
        {
            Age = 7,
            StageLabel = "7 anos",
            SchoolPlacement = "2o ano do Ensino Fundamental",
            BnccAnchor = "O 2o ano segue no bloco de alfabetizacao da BNCC, consolidando sistema de escrita, fluencia inicial, compreensao, producao de textos e analise de regularidades ortograficas em uso.",
            AnnualObjective = "Consolidar a alfabetizacao, ampliando fluencia de leitura, compreensao de textos curtos, producao de pequenos generos, pontuacao basica e ortografias mais frequentes em contexto significativo.",
            DiagnosticFocus = "Verificar estabilidade na leitura de palavras e frases, fluencia inicial, compreensao literal, segmentacao convencional, repertorio ortografico basico e capacidade de produzir pequenos textos com apoio.",
            PedagogicalApproach = "Equilibrar leitura diaria, escrita guiada, ortografia em contexto, reconto, genero textual e conversa sobre o texto. O 2o ano precisa consolidar base, nao apenas acelerar volume.",
            BnccReferences =
            [
                "BNCC Lingua Portuguesa - anos iniciais, com alfabetizacao sistematizada nos dois primeiros anos (p. 90).",
                "Referencias operacionais do 2o ano: familias EF02LP e EF15LP.",
                "Habilidades do bloco alfabetico compartilhado: familias EF12LP."
            ],
            ExpectedOutcomes =
            [
                "Le textos curtos com mais fluidez e menor dependencia de silabacao.",
                "Compreende informacoes explicitas e faz inferencias simples.",
                "Produz bilhetes, relatos e pequenos textos com apoio decrescente.",
                "Usa pontuacao basica, segmentacao e ortografias frequentes com maior regularidade.",
                "Participa de leitura literaria e conversa sobre personagens, cenarios e acontecimentos."
            ],
            SuggestedEvidence =
            [
                "Leitura em voz alta de texto curto com marcacao de fluencia.",
                "Texto curto produzido pela crianca com revisao guiada.",
                "Fichas de ortografia em contexto, nao isoladas.",
                "Registros de compreensao oral e escrita."
            ],
            FamilyRoutineActions =
            [
                "Garantir leitura diaria curta e previsivel.",
                "Fazer ditado reflexivo de palavras e frases em microblocos, nao listas extensas.",
                "Explorar bilhetes, listas, regras e pequenos relatos do cotidiano.",
                "Voltar sempre a textos conhecidos para consolidar fluencia."
            ],
            Terms =
            [
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "1o bimestre",
                    MainFocus = "Revisao da base alfabetica e fluencia inicial",
                    LearningGoals =
                    [
                        "Reforcar leitura de palavras, frases e pequenos textos.",
                        "Consolidar segmentacao entre palavras e correspondencias frequentes.",
                        "Retomar reconto e compreensao oral."
                    ],
                    SuggestedGenres = ["Parlendas", "Listas", "Bilhetes", "Historias curtas"],
                    AssessmentSignals =
                    [
                        "Le com menos interrupcoes em repertorio conhecido.",
                        "Escreve com segmentacao mais estavel.",
                        "Compreende o essencial do que le."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "2o bimestre",
                    MainFocus = "Compreensao leitora e generos do cotidiano",
                    LearningGoals =
                    [
                        "Identificar finalidade e informacoes principais em textos do cotidiano.",
                        "Localizar dados explicitos em avisos, bilhetes, listas, convites e regras.",
                        "Produzir pequenos textos funcionais com ajuda."
                    ],
                    SuggestedGenres = ["Avisos", "Bilhetes", "Convites", "Regras"],
                    AssessmentSignals =
                    [
                        "Explica para que serve o texto.",
                        "Encontra informacoes pedidas no texto.",
                        "Produz pequeno texto funcional com estrutura basica."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "3o bimestre",
                    MainFocus = "Ortografia frequente, pontuacao e pequenos relatos",
                    LearningGoals =
                    [
                        "Estabilizar ortografias de alta frequencia e regularidades mais comuns.",
                        "Usar maiuscula inicial e pontuacao basica em frases e pequenos textos.",
                        "Produzir relatos e recontos com sequencia."
                    ],
                    SuggestedGenres = ["Relato", "Reconto", "Diario curto", "Legenda ampliada"],
                    AssessmentSignals =
                    [
                        "Melhora o controle ortografico em palavras recorrentes.",
                        "Pontua frases com maior intencao.",
                        "Organiza melhor a sequencia do texto."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "4o bimestre",
                    MainFocus = "Fechamento do ciclo alfabetico",
                    LearningGoals =
                    [
                        "Ler textos curtos com mais autonomia e compreensao.",
                        "Produzir pequenos textos com planejamento e revisao guiada.",
                        "Encerrar o ano com base robusta para o 3o ano."
                    ],
                    SuggestedGenres = ["Conto curto", "Bilhete ampliado", "Relato pessoal", "Poema curto"],
                    AssessmentSignals =
                    [
                        "Le com maior independencia.",
                        "Produz texto curto com comeco, desenvolvimento e fechamento simples.",
                        "Mostra menor dependencia do adulto para tarefas basicas de leitura e escrita."
                    ]
                }
            ]
        },
        new PortuguesePlanningStageViewModel
        {
            Age = 8,
            StageLabel = "8 anos",
            SchoolPlacement = "3o ano do Ensino Fundamental",
            BnccAnchor = "No 3o ano, a BNCC amplia letramento e autonomia: a crianca passa a ler textos mais variados, produzir com maior organizacao e aprofundar regularidades da lingua em diferentes campos de atuacao.",
            AnnualObjective = "Consolidar a passagem da alfabetizacao para o letramento mais autonomo, com fluencia crescente, compreensao literal e inferencial, producao de pequenos paragrafos, leitura literaria e estudo sistematico de regularidades ortograficas.",
            DiagnosticFocus = "Observar fluencia, compreensao de ideia central e detalhes, organizacao de paragrafos, uso de conectores simples, ortografia frequente, interesse leitor e autonomia diante de textos narrativos e informativos.",
            PedagogicalApproach = "Trabalhar leitura diaria, conversa interpretativa, estudo de genero, producao com planejamento e revisao, ortografizacao em contexto e progressiva autonomia leitora e escritora.",
            BnccReferences =
            [
                "BNCC Lingua Portuguesa - anos iniciais, p. 90: ampliacao do letramento, leitura em textos de complexidade crescente e producao de diferentes generos.",
                "Referencias operacionais do 3o ano: familia EF03LP.",
                "Habilidades compartilhadas do 3o ao 5o ano: familias EF35LP, incluindo leitura literaria autonoma e criacao de narrativas com mais autonomia (p. 134)."
            ],
            ExpectedOutcomes =
            [
                "Le textos curtos e medios com maior fluencia e compreensao.",
                "Identifica ideia principal, personagens, tempo, espaco e algumas inferencias simples.",
                "Produz pequenos paragrafos e narrativas curtas com mais autonomia.",
                "Usa recursos basicos de coesao, pontuacao e ortografia frequente.",
                "Amplia repertorio literario e informativo."
            ],
            SuggestedEvidence =
            [
                "Leitura gravada de narrativa ou texto informativo curto.",
                "Producao escrita revisada em duas versoes.",
                "Ficha de compreensao com ideia principal e inferencia simples.",
                "Portifolio de genero com texto final e observacoes do adulto."
            ],
            FamilyRoutineActions =
            [
                "Ler diariamente com alternancia entre leitura do adulto e leitura da crianca.",
                "Conversar sobre o sentido do texto, nao apenas sobre decodificacao.",
                "Usar planejamento e revisao em qualquer producao curta.",
                "Incluir textos literarios, informativos e de rotina para ampliar repertorio."
            ],
            Terms =
            [
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "1o bimestre",
                    MainFocus = "Fluencia e compreensao literal",
                    LearningGoals =
                    [
                        "Consolidar leitura oral mais fluida em textos curtos.",
                        "Identificar personagens, acontecimentos, assunto e informacoes explicitas.",
                        "Retomar ortografia frequente e pontuacao basica em uso."
                    ],
                    SuggestedGenres = ["Contos curtos", "Bilhetes", "Noticias infantis", "Poemas curtos"],
                    AssessmentSignals =
                    [
                        "Le com ritmo mais estavel e menor dependencia.",
                        "Encontra informacoes explicitas com seguranca.",
                        "Melhora o controle ortografico em producoes curtas."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "2o bimestre",
                    MainFocus = "Inferencia, ideia central e organizacao do paragrafo",
                    LearningGoals =
                    [
                        "Localizar ideia principal e detalhes relevantes.",
                        "Fazer inferencias simples a partir do texto e da ilustracao.",
                        "Produzir pequenos paragrafos com unidade tematica."
                    ],
                    SuggestedGenres = ["Narrativas", "Verbete simples", "Pequena reportagem", "Resumo oral/escrito"],
                    AssessmentSignals =
                    [
                        "Explica o texto com suas palavras.",
                        "Aponta pistas para uma inferencia simples.",
                        "Organiza texto curto em frase inicial, desenvolvimento e fechamento."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "3o bimestre",
                    MainFocus = "Genero, planejamento e revisao de texto",
                    LearningGoals =
                    [
                        "Planejar e produzir textos curtos adequados ao genero.",
                        "Revisar ortografia, pontuacao e clareza com roteiro simples.",
                        "Ampliar repertorio de conectores e marcas de organizacao textual."
                    ],
                    SuggestedGenres = ["Relato", "Instrucao", "Carta/bilhete", "Narrativa curta"],
                    AssessmentSignals =
                    [
                        "Planeja antes de escrever.",
                        "Aceita revisar o texto e melhora a segunda versao.",
                        "Usa conectores simples como 'depois', 'porque', 'entao'."
                    ]
                },
                new PortuguesePlanningTermViewModel
                {
                    TermLabel = "4o bimestre",
                    MainFocus = "Leitura literaria mais autonoma e autoria inicial",
                    LearningGoals =
                    [
                        "Ler e compreender textos literarios com mais autonomia.",
                        "Produzir narrativa curta ou texto de opiniao inicial com apoio.",
                        "Fechar o ano pronto para aprofundar estudo de lingua e texto no 4o ano."
                    ],
                    SuggestedGenres = ["Narrativa ficcional", "Poema", "Texto de opiniao inicial", "Pequeno texto informativo"],
                    AssessmentSignals =
                    [
                        "Demonstra preferencia leitora e comenta o que le.",
                        "Produz texto mais articulado e menos dependente do adulto.",
                        "Mostra base consistente de leitura, compreensao e escrita para avancar."
                    ]
                }
            ]
        }
    ];
}

public sealed class PortuguesePlanningGuidance
{
    public int Age { get; init; }
    public string StageLabel { get; init; } = string.Empty;
    public string SchoolPlacement { get; init; } = string.Empty;
    public string TermLabel { get; init; } = string.Empty;
    public string MainFocus { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string DailyTheme { get; init; } = string.Empty;
    public IReadOnlyList<string> PriorityKeywords { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PreferredTemplateTitles { get; init; } = Array.Empty<string>();
    public int RecommendedDailyLanguageBlocks { get; init; } = 1;
}
