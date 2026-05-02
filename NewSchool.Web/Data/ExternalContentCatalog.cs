using NewSchool.Web.Domain;

namespace NewSchool.Web.Data;

public static class ExternalContentCatalog
{
    public static IReadOnlyList<ExternalContentCatalogItem> Build()
    {
        return
        [
            new ExternalContentCatalogItem
            {
                Slug = "easy-peasy-getting-ready-1",
                CategoryTitle = "Curriculos internacionais adaptados para o Brasil",
                CategoryDescription = "Guias internos do NewSchool que traduzem metodo, ritmo e aplicacao para familias brasileiras, sem depender de PDF em ingles.",
                SortOrder = 1,
                AgeMin = 3,
                AgeMax = 5,
                Domain = LearningDomain.Language,
                Title = "Easy Peasy Getting Ready 1",
                Summary = "Curso introdutorio focado em pre-leitura, reconhecimento de letras, sons e rotina curta de alfabetizacao inicial.",
                WhyItMatters = "Ajuda a estruturar os primeiros meses de rotina para criancas pequenas sem partir do zero.",
                PortugueseGuideNote = "No NewSchool o pai acessa um guia traduzido e adaptado em portugues, com sequencia semanal, objetivo por idade e evidencias para registrar.",
                FormatLabel = "Curriculo adaptado",
                SourceLabel = "Easy Peasy",
                GoalTracks = ["literacy", "autonomy", "balanced_growth"],
                MatchKeywords = "letra,som,palavra,alfabeto,pre-alfabetizacao,rima,historia,rotina",
                OfficialUrl = "https://allinonehomeschool.com/getting-ready-1/",
                OfficialActionLabel = "Abrir curriculo original",
                AudienceLabel = "3 a 5 anos",
                AudienceChipClass = "success",
                HasGuide = true,
                GuideLabel = "Acessar curriculo traduzido e adaptado para o Brasil",
                AdaptedHeadline = "Como usar o metodo sem jogar ingles e PDF em cima da familia brasileira",
                Intro = "O foco nao e seguir pagina por pagina. O foco e pegar a ideia central: blocos curtos, som antes do nome da letra, repeticao viva, historias, movimento e registro simples da evolucao.",
                ParentSteps =
                [
                    "Trabalhe uma letra ou familia de sons por vez em blocos de 10 a 15 minutos.",
                    "Misture canto, historia, fala guiada, tracado grande e uma atividade concreta.",
                    "Nao use o workbook em ingles dentro do NewSchool; use tarefas originais do sistema em portugues.",
                    "Feche com uma prova curta: foto da atividade, audio da fala ou video de reconto."
                ],
                WeeklyRhythm =
                [
                    "Segunda: som e historia da letra.",
                    "Terca: reconhecimento visual e busca de palavras.",
                    "Quarta: tracado grande, pintura ou recorte.",
                    "Quinta: revisao oral e mini evidencia.",
                    "Sexta: atividade leve e celebracao do que foi consolidado."
                ],
                EvidenceIdeas =
                [
                    "Video da crianca dizendo o som inicial de 3 palavras.",
                    "Foto de letra feita com massinha, tinta ou objetos.",
                    "Audio de reconto rapido da historia da semana."
                ],
                StarterMaterials =
                [
                    "cartoes de letras",
                    "objetos da casa para som inicial",
                    "massa ou giz",
                    "uma historia curta em portugues"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "letras-e-som-inicial",
                        Title = "Letras e som inicial",
                        Summary = "Use este caminho quando a meta for apresentar uma letra, repetir som inicial e fechar com uma evidencia curta.",
                        WhenToUse = "Ideal para semanas de pre-alfabetizacao com criancas de 3 a 5 anos.",
                        HowItEnters = "No NewSchool isso vira bloco curto de som, objeto da casa, tracado grande e video curto da fala da crianca.",
                        EvidenceIdea = "Grave a crianca dizendo o som inicial de 3 palavras.",
                        OfficialUrl = "https://allinonehomeschool.com/getting-ready-1/",
                        ActionLabel = "Abrir curriculo original"
                    }
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "ambleside-primeiros-anos",
                CategoryTitle = "Curriculos internacionais adaptados para o Brasil",
                CategoryDescription = "Guias internos do NewSchool que traduzem metodo, ritmo e aplicacao para familias brasileiras, sem depender de PDF em ingles.",
                SortOrder = 2,
                AgeMin = 6,
                AgeMax = 10,
                Domain = LearningDomain.World,
                Title = "AmblesideOnline primeiros anos",
                Summary = "Curriculo gratuito de inspiracao Charlotte Mason com leitura viva, historia, geografia, natureza, artes e fe crista.",
                WhyItMatters = "Da profundidade cultural ao ensino em casa e ajuda a fugir de atividades soltas sem fio condutor.",
                PortugueseGuideNote = "No NewSchool isso vira um mapa simples em portugues: leitura viva, reconto, caderno de descobertas e cronograma semanal realista para o Brasil.",
                FormatLabel = "Curriculo adaptado",
                SourceLabel = "AmblesideOnline",
                GoalTracks = ["literacy", "science_discovery", "balanced_growth"],
                MatchKeywords = "leitura,narracao,historia,geografia,ciencia,natureza,biografia,timeline",
                OfficialUrl = "https://www.amblesideonline.org/curriculum",
                OfficialActionLabel = "Abrir curriculo original",
                AudienceLabel = "6 a 10 anos",
                AudienceChipClass = "track-academic",
                HasGuide = true,
                GuideLabel = "Acessar curriculo traduzido e adaptado para o Brasil",
                AdaptedHeadline = "Como traduzir Charlotte Mason para uma rotina brasileira simples e usavel",
                Intro = "A ideia central e trocar excesso de ficha por leitura viva, narracao, observacao da natureza, historia contada e registro curto, sempre com metas pequenas e visiveis.",
                ParentSteps =
                [
                    "Abra a semana com uma leitura viva curta e pergunte o que a crianca entendeu.",
                    "Alterne linguagem, matematica, historia e mundo real ao longo da semana.",
                    "Guarde uma evidencia por area: reconto, mapa, desenho cientifico ou texto curto.",
                    "Mantenha a densidade baixa; o valor esta na qualidade da atencao, nao no volume."
                ],
                WeeklyRhythm =
                [
                    "Segunda: leitura viva e reconto.",
                    "Terca: matematica com representacao e explicacao.",
                    "Quarta: historia ou geografia com mapa ou linha do tempo.",
                    "Quinta: ciencia, natureza ou observacao guiada.",
                    "Sexta: fechamento com memoria da semana e portifolio."
                ],
                EvidenceIdeas =
                [
                    "Video da crianca narrando a leitura.",
                    "Foto do mapa, linha do tempo ou caderno de natureza.",
                    "Texto curto respondendo o que aprendeu."
                ],
                StarterMaterials =
                [
                    "livro vivo ou texto narrativo",
                    "caderno de descobertas",
                    "mapa simples",
                    "materiais concretos de matematica"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "leitura-viva-e-narracao",
                        Title = "Leitura viva e narracao",
                        Summary = "Use este caminho quando quiser abrir a semana com leitura curta, reconto e memoria do que foi lido.",
                        WhenToUse = "Ideal para semanas de linguagem, historia e formacao cultural.",
                        HowItEnters = "No NewSchool isso vira leitura guiada, duas perguntas, reconto em video e anotacao curta no dashboard.",
                        EvidenceIdea = "Salve um video do reconto e uma foto do caderno de descobertas.",
                        OfficialUrl = "https://www.amblesideonline.org/curriculum",
                        ActionLabel = "Abrir curriculo original"
                    }
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "educalar-infantil-organizado",
                CategoryTitle = "Colecoes externas em portugues ja organizadas",
                CategoryDescription = "Entradas organizadas pelo NewSchool para portais brasileiros que ja possuem material em portugues e podem entrar no dashboard da crianca.",
                SortOrder = 3,
                AgeMin = 0,
                AgeMax = 5,
                Domain = LearningDomain.Language,
                Title = "materiais-gratuitos da educalar.com.br - Ensino Infantil",
                Summary = "Colecao brasileira para primeira infancia com trilhas de linguagem, brincadeira, motor, ciencias e leitura em voz alta.",
                WhyItMatters = "Evita que a familia abra o portal sem saber por onde comecar. O sistema ja mostra as pastas mais relevantes por objetivo.",
                PortugueseGuideNote = "O NewSchool organiza a entrada por idade e por materia, para o pai nao precisar vasculhar o portal inteiro.",
                FormatLabel = "Colecao brasileira",
                SourceLabel = "Educalar",
                GoalTracks = ["literacy", "math_foundations", "autonomy", "science_discovery", "balanced_growth"],
                MatchKeywords = "alfabeto,brincadeiras,ciencias,motor,leitura,livrinhos,matematica,pre-escolar",
                OfficialUrl = "https://www.educalar.com.br/materiais-gratuitos",
                OfficialActionLabel = "Abrir fonte original",
                AudienceLabel = "0 a 5 anos",
                AudienceChipClass = "success",
                HasGuide = true,
                GuideLabel = "Ver colecao organizada no NewSchool",
                AdaptedHeadline = "Por onde entrar primeiro no portal da Educalar para criancas pequenas",
                Intro = "A entrada nao deve ser pelo volume de PDF. Deve ser por objetivo da semana. O NewSchool separa as pastas para linguagem, brincadeira, motor e matematica inicial.",
                FolderHighlights =
                [
                    "Alfabeto",
                    "Brincadeiras",
                    "Ciencias",
                    "Desenvolvimento Motor",
                    "Fabulas",
                    "Historias Musicalizadas",
                    "Leitura em Voz Alta",
                    "Livrinhos",
                    "Matematica"
                ],
                ParentSteps =
                [
                    "Se a meta for fala e pre-leitura, comece por Alfabeto, Leitura em Voz Alta e Livrinhos.",
                    "Se a crianca estiver inquieta, puxe Brincadeiras e Desenvolvimento Motor antes da ficha.",
                    "Se a semana pedir curiosidade e observacao, use Ciencias e Fabulas.",
                    "Use Matematica em dias curtos, sempre com objeto e contagem concreta."
                ],
                WeeklyRhythm =
                [
                    "2 entradas de linguagem",
                    "1 entrada de motor ou brincadeira",
                    "1 entrada de matematica concreta",
                    "1 entrada leve de historia ou ciencia"
                ],
                EvidenceIdeas =
                [
                    "foto do trabalho impresso",
                    "video da leitura em voz alta",
                    "registro da brincadeira dirigida"
                ],
                StarterMaterials =
                [
                    "lapis de cor",
                    "tesoura sem ponta",
                    "cartoes visuais",
                    "caixa de tampinhas ou blocos"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "alfabeto",
                        Title = "Alfabeto",
                        Summary = "Entrada para semanas de letra, som inicial, reconhecimento visual e palavras da casa.",
                        WhenToUse = "Quando a crianca estiver em pre-alfabetizacao ou iniciando relacao entre som e letra.",
                        HowItEnters = "O sistema converte isso em aula curta com letra da semana, objetos concretos e prova simples.",
                        EvidenceIdea = "Foto da atividade impressa ou video da crianca dizendo o som da letra."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "brincadeiras",
                        Title = "Brincadeiras",
                        Summary = "Entrada para semanas em que a crianca precisa aprender em movimento antes de sentar para ficha.",
                        WhenToUse = "Quando houver dispersao, resistencia de inicio ou excesso de energia.",
                        HowItEnters = "O sistema puxa uma brincadeira dirigida antes do bloco academico e fecha com registro do que a crianca conseguiu fazer.",
                        EvidenceIdea = "Video curto da brincadeira guiada ou foto do material usado."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "ciencias",
                        Title = "Ciencias",
                        Summary = "Entrada para observacao, perguntas do mundo real e descoberta guiada.",
                        WhenToUse = "Quando a semana pedir curiosidade, natureza, corpo humano ou fenomenos simples.",
                        HowItEnters = "No NewSchool isso vira conversa guiada, atividade concreta e evidencia visual do que foi observado.",
                        EvidenceIdea = "Foto do experimento, desenho do que viu ou audio explicando a descoberta."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "desenvolvimento-motor",
                        Title = "Desenvolvimento Motor",
                        Summary = "Entrada para coordenação fina, traçado, recorte e preparação para escrever.",
                        WhenToUse = "Quando a crianca ainda nao sustenta bem lapis, tesoura ou permanencia curta em mesa.",
                        HowItEnters = "O sistema usa isso antes da escrita formal para evitar sobrecarga e acelerar autonomia motora.",
                        EvidenceIdea = "Foto do recorte, do tracado ou da atividade de motricidade pronta."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "leitura-em-voz-alta",
                        Title = "Leitura em Voz Alta",
                        Summary = "Entrada para escuta, vocabulario, imaginacao e reconto simples.",
                        WhenToUse = "Quando a meta da semana for linguagem oral, concentracao ou repertorio de historias.",
                        HowItEnters = "O NewSchool transforma isso em leitura curta, duas perguntas e uma pequena prova oral.",
                        EvidenceIdea = "Audio de reconto ou video da crianca comentando a historia."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "matematica",
                        Title = "Matematica",
                        Summary = "Entrada para contagem, numeros, comparacao e primeiras relacoes matematicas.",
                        WhenToUse = "Quando a semana pedir quantidade, contar objetos, ordenar ou fazer pequenas comparacoes.",
                        HowItEnters = "No sistema isso vira bloco concreto com tampinhas, dedos, cartas e registro rapido.",
                        EvidenceIdea = "Foto da quantidade montada ou video da contagem da crianca."
                    }
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "educalar-fundamental-organizado",
                CategoryTitle = "Colecoes externas em portugues ja organizadas",
                CategoryDescription = "Entradas organizadas pelo NewSchool para portais brasileiros que ja possuem material em portugues e podem entrar no dashboard da crianca.",
                SortOrder = 4,
                AgeMin = 6,
                AgeMax = 14,
                Domain = LearningDomain.World,
                Title = "materiais-gratuitos da educalar.com.br - Ensino Fundamental",
                Summary = "Colecao brasileira para ensino fundamental com entradas para biografia, flashcards, literatura, matematica, teologia e timeline.",
                WhyItMatters = "Da rota de entrada para estudos mais ricos e evita que o adulto tenha que adivinhar qual pasta combina com a idade e a materia da semana.",
                PortugueseGuideNote = "O NewSchool transforma essas pastas em trilhas de uso real: leitura, memoria, pesquisa guiada e revisao por tema.",
                FormatLabel = "Colecao brasileira",
                SourceLabel = "Educalar",
                GoalTracks = ["literacy", "math_foundations", "autonomy", "science_discovery", "balanced_growth"],
                MatchKeywords = "biografia,flashcards,literatura,matematica,teologia,timeline,leitura,pesquisa",
                OfficialUrl = "https://www.educalar.com.br/materiais-gratuitos",
                OfficialActionLabel = "Abrir fonte original",
                AudienceLabel = "6 a 14 anos",
                AudienceChipClass = "track-academic",
                HasGuide = true,
                GuideLabel = "Ver colecao organizada no NewSchool",
                AdaptedHeadline = "Como usar a Educalar para montar semanas mais completas no fundamental",
                Intro = "A colecao funciona melhor quando cada pasta entra com funcao clara: literatura para leitura viva, timeline para historia, flashcards para revisao, matematica para treino guiado.",
                FolderHighlights =
                [
                    "Biografia",
                    "Flashcards",
                    "Literatura",
                    "Matematica",
                    "Teologia",
                    "Timeline"
                ],
                ParentSteps =
                [
                    "Use Literatura para abrir leitura e narracao da semana.",
                    "Puxe Timeline quando a crianca estiver estudando historia ou ordem cronologica.",
                    "Use Flashcards como revisao curta, nunca como aula inteira.",
                    "Entre em Teologia quando a familia quiser integrar formacao biblica ao curriculo."
                ],
                WeeklyRhythm =
                [
                    "1 leitura principal",
                    "1 bloco de matematica",
                    "1 revisao com flashcards",
                    "1 producao ou pesquisa guiada"
                ],
                EvidenceIdeas =
                [
                    "foto do flashcard usado",
                    "linha do tempo pronta",
                    "reconto gravado da leitura"
                ],
                StarterMaterials =
                [
                    "caderno",
                    "cartoes",
                    "mapa ou linha do tempo",
                    "lapis coloridos"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "biografia",
                        Title = "Biografia",
                        Summary = "Entrada para estudar pessoas importantes, eventos e linhas de vida com narrativa simples.",
                        WhenToUse = "Quando a semana estiver puxando historia, valores ou repertorio cultural.",
                        HowItEnters = "O sistema usa isso para montar leitura curta, reconto e uma linha do tempo simples.",
                        EvidenceIdea = "Foto da linha do tempo ou video da crianca contando quem estudou."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "flashcards",
                        Title = "Flashcards",
                        Summary = "Entrada para revisao curta, memorizacao leve e retomada sem transformar a aula num treino cansativo.",
                        WhenToUse = "Quando a crianca precisa revisar conceitos, vocabulario, datas ou operacoes.",
                        HowItEnters = "O NewSchool encaixa flashcards em 5 minutos de abertura ou fechamento de aula.",
                        EvidenceIdea = "Foto dos cards usados ou video da revisao oral."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "literatura",
                        Title = "Literatura",
                        Summary = "Entrada para leitura viva, imaginacao, oralidade e escrita de resposta.",
                        WhenToUse = "Quando a semana estiver focada em leitura, reconto e interpretacao.",
                        HowItEnters = "O sistema transforma isso em leitura guiada, pergunta-chave e registro curto.",
                        EvidenceIdea = "Audio de leitura ou reconto, com foto do caderno."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "matematica",
                        Title = "Matematica",
                        Summary = "Entrada para treino orientado, revisao de operacoes e pratica de problemas simples.",
                        WhenToUse = "Quando a meta da semana pedir exercicio guiado, conta ou reforco de conceitos.",
                        HowItEnters = "No NewSchool isso entra como complemento de uma tarefa concreta ja explicada ao pai.",
                        EvidenceIdea = "Foto da folha resolvida com uma nota curta do que a crianca conseguiu fazer sozinha."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "teologia",
                        Title = "Teologia",
                        Summary = "Entrada para integrar fe cristã, Biblia e formacao de virtudes na rotina da crianca.",
                        WhenToUse = "Quando a familia quiser unir conteudo academico com base biblica e reflexao simples.",
                        HowItEnters = "O sistema usa isso em versiculo da semana, pergunta devocional e atividade curta.",
                        EvidenceIdea = "Video da memorizacao do versiculo ou foto da atividade ligada a virtudes."
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "timeline",
                        Title = "Timeline",
                        Summary = "Entrada para organizar historia, antes e depois, sequencia de eventos e cronologia.",
                        WhenToUse = "Quando a semana puxar historia do Brasil, biografias ou ordem temporal.",
                        HowItEnters = "No NewSchool isso vira linha do tempo simples, mapa mental ou cartoes de eventos.",
                        EvidenceIdea = "Foto da timeline pronta ou audio da crianca explicando a ordem dos fatos."
                    }
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-aprendendo-alfabeto",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 5,
                AgeMin = 3,
                AgeMax = 5,
                Domain = LearningDomain.Language,
                Title = "Aprendendo o Alfabeto - Caligrafia e Escrita",
                Summary = "Apostila em portugues com tracos simples, letras em destaque e ilustracoes para colorir.",
                WhyItMatters = "Serve bem como apoio de letra da semana e treino motor leve para a fase de pre-alfabetizacao.",
                PortugueseGuideNote = "No NewSchool isso entra como apoio opcional para a tarefa da semana, nunca como curso solto sem contexto.",
                FormatLabel = "PDF em portugues",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["literacy", "balanced_growth"],
                MatchKeywords = "alfabeto,caligrafia,letra,traco,escrita,colorir",
                OfficialUrl = "https://www.baixelivros.com.br/didatico/aprendendo-o-alfabeto",
                OfficialActionLabel = "Abrir material especifico",
                AudienceLabel = "3 a 5 anos",
                AudienceChipClass = "success",
                Highlights =
                [
                    "Caligrafia",
                    "Crianças de 3 a 5 anos",
                    "Educacao Infantil",
                    "Imprimir e Colorir"
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-consciencia-fonologica",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 6,
                AgeMin = 3,
                AgeMax = 5,
                Domain = LearningDomain.Language,
                Title = "Consciencia Fonologica - Maynara Abreu",
                Summary = "Material ilustrado em portugues para rimas, sons iniciais, repeticao de frases e percepcao auditiva.",
                WhyItMatters = "Ajuda a fortalecer base pre-leitora antes da alfabetizacao ganhar carga maior.",
                PortugueseGuideNote = "O sistema pode sugerir esse caderno quando o objetivo da semana for rima, som inicial e percepcao de palavras.",
                FormatLabel = "PDF em portugues",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["literacy", "balanced_growth"],
                MatchKeywords = "fonologica,rima,som,palavra,silaba,pre-leitura",
                OfficialUrl = "https://www.baixelivros.com.br/didatico/consciencia-fonologica",
                OfficialActionLabel = "Abrir material especifico",
                AudienceLabel = "3 a 5 anos",
                AudienceChipClass = "success",
                Highlights =
                [
                    "Alfabetizacao",
                    "Cadernos de Atividades",
                    "Crianças de 3 a 5 anos",
                    "Imprimir e Colorir"
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-subtracao-inicial",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 7,
                AgeMin = 6,
                AgeMax = 8,
                Domain = LearningDomain.Math,
                Title = "Subtracao Inicial - Matematica Ludica",
                Summary = "Apostila em portugues para iniciar subtracao com exercicios simples e ilustrados.",
                WhyItMatters = "Funciona como reforco quando a aula do dia pede tirar, comparar ou resolver pequenos problemas.",
                PortugueseGuideNote = "O NewSchool usa esse tipo de material como complemento de uma tarefa guiada, nao como aula automatica por si so.",
                FormatLabel = "PDF em portugues",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["math_foundations", "balanced_growth"],
                MatchKeywords = "subtracao,matematica,problema,quantidade,comparar,conta",
                OfficialUrl = "https://www.baixelivros.com.br/didatico/subtracao-inicial",
                OfficialActionLabel = "Abrir material especifico",
                AudienceLabel = "6 a 8 anos",
                AudienceChipClass = "track-academic",
                Highlights =
                [
                    "Matematica ludica",
                    "Crianças de 6 a 8 anos",
                    "Ensino Fundamental"
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-caligrafia-versiculos",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 8,
                AgeMin = 7,
                AgeMax = 12,
                Domain = LearningDomain.Language,
                Title = "Caligrafia com Versiculos Biblicos - Alyne Leite",
                Summary = "Livro em portugues que mistura pratica de escrita com versiculos biblicos.",
                WhyItMatters = "Abre uma linha clara para familias que querem unir caligrafia, devocional e memorizacao curta.",
                PortugueseGuideNote = "O sistema pode recomendar este material em semanas de copia curta, virtudes e fechamento com base biblica.",
                FormatLabel = "PDF em portugues",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["literacy", "balanced_growth", "autonomy"],
                MatchKeywords = "caligrafia,versiculos,biblia,copia,virtudes,escrita",
                OfficialUrl = "https://www.baixelivros.com.br/didatico/caligrafia-virtudes-versiculos",
                OfficialActionLabel = "Abrir material especifico",
                AudienceLabel = "7 a 12 anos",
                AudienceChipClass = "track-communication",
                Highlights =
                [
                    "Caligrafia",
                    "Versiculos biblicos",
                    "Ensino Fundamental"
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-educacao-infantil-acervo",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 9,
                AgeMin = 3,
                AgeMax = 6,
                Domain = LearningDomain.World,
                Title = "Baixe Livros - Acervo de Educacao Infantil",
                Summary = "Pagina de acervo focada em educacao infantil com varias entradas gratuitas em portugues.",
                WhyItMatters = "Serve como reserva boa para imprimir quando a familia quiser mais material, sem sair do filtro infantil.",
                PortugueseGuideNote = "A rota correta e entrar ja no acervo infantil, e nao na biblioteca inteira do site.",
                FormatLabel = "Acervo filtrado",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["literacy", "math_foundations", "science_discovery", "balanced_growth"],
                MatchKeywords = "educacao infantil,alfabeto,fonologica,subtracao,letras,palavras",
                OfficialUrl = "https://www.baixelivros.com.br/acervo/educacao-infantil",
                OfficialActionLabel = "Abrir acervo infantil",
                AudienceLabel = "3 a 6 anos",
                AudienceChipClass = "success",
                Highlights =
                [
                    "Consciencia Fonologica",
                    "Aprendendo o Alfabeto",
                    "Subtracao Inicial",
                    "Letras e Palavras"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "aprendendo-o-alfabeto",
                        Title = "Aprendendo o Alfabeto",
                        Summary = "Acesso rapido ao material em portugues para letra da semana e tracado inicial.",
                        WhenToUse = "Quando a meta da semana for letra, som inicial e traco simples.",
                        HowItEnters = "O NewSchool usa poucas paginas por vez, sempre com objetivo claro e evidencia salva.",
                        EvidenceIdea = "Foto da pagina trabalhada ou video da crianca dizendo palavras da letra.",
                        OfficialUrl = "https://www.baixelivros.com.br/didatico/aprendendo-o-alfabeto",
                        ActionLabel = "Abrir material especifico"
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "consciencia-fonologica",
                        Title = "Consciencia Fonologica",
                        Summary = "Acesso rapido ao material em portugues para rimas, som inicial e percepcao auditiva.",
                        WhenToUse = "Quando a semana estiver focada em pre-leitura e oralidade.",
                        HowItEnters = "O sistema encaixa esse caderno como apoio externo para tarefas de rima e sons.",
                        EvidenceIdea = "Audio da crianca repetindo rimas ou foto da atividade pronta.",
                        OfficialUrl = "https://www.baixelivros.com.br/didatico/consciencia-fonologica",
                        ActionLabel = "Abrir material especifico"
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "subtracao-inicial",
                        Title = "Subtracao Inicial",
                        Summary = "Acesso rapido ao material em portugues para primeiras subtracoes com apoio visual.",
                        WhenToUse = "Quando a semana pedir comparar, tirar, resolver pequenas contas ou quantidades.",
                        HowItEnters = "O sistema combina concreto primeiro e folha depois, sem jogar conta seca de cara.",
                        EvidenceIdea = "Foto da conta resolvida ou video explicando o que sobrou.",
                        OfficialUrl = "https://www.baixelivros.com.br/didatico/subtracao-inicial",
                        ActionLabel = "Abrir material especifico"
                    }
                ]
            },
            new ExternalContentCatalogItem
            {
                Slug = "baixe-livros-caligrafia-acervo",
                CategoryTitle = "Apostilas externas em portugues com link direto",
                CategoryDescription = "Materiais especificos em portugues com link direto para a pagina certa, sem jogar a familia em listas gigantes.",
                SortOrder = 10,
                AgeMin = 6,
                AgeMax = 12,
                Domain = LearningDomain.Language,
                Title = "Baixe Livros - Acervo de Caligrafia",
                Summary = "Acervo direto de caligrafia em portugues com alfabetos, numeros, cursiva e versiculos biblicos.",
                WhyItMatters = "Economiza tempo quando a familia quer montar uma trilha de escrita semanal em vez de um PDF solto.",
                PortugueseGuideNote = "O NewSchool pode usar esse acervo como banco de reforco para semanas de escrita, copia e capricho grafico.",
                FormatLabel = "Acervo filtrado",
                SourceLabel = "Baixe Livros",
                GoalTracks = ["literacy", "balanced_growth", "autonomy"],
                MatchKeywords = "caligrafia,cursiva,numeros,versiculos,escrita,copia",
                OfficialUrl = "https://www.baixelivros.com.br/acervo/caligrafia",
                OfficialActionLabel = "Abrir acervo de caligrafia",
                AudienceLabel = "6 a 12 anos",
                AudienceChipClass = "track-communication",
                Highlights =
                [
                    "Caligrafia: Numeros",
                    "Caligrafia Divertida",
                    "Versiculos Biblicos",
                    "Escrita cursiva"
                ],
                FocusOptions =
                [
                    new ExternalContentFocusOption
                    {
                        Slug = "versiculos-biblicos",
                        Title = "Versiculos Biblicos",
                        Summary = "Acesso rapido ao material de caligrafia com base biblica em portugues.",
                        WhenToUse = "Quando a familia quiser unir escrita, memoria curta e formacao de virtudes.",
                        HowItEnters = "O NewSchool usa uma linha curta por dia, sem sobrecarga, e pede uma prova simples no fim.",
                        EvidenceIdea = "Foto da copia ou video da leitura do versiculo.",
                        OfficialUrl = "https://www.baixelivros.com.br/didatico/caligrafia-virtudes-versiculos",
                        ActionLabel = "Abrir material especifico"
                    },
                    new ExternalContentFocusOption
                    {
                        Slug = "alfabeto-e-cursiva",
                        Title = "Alfabeto e Escrita Cursiva",
                        Summary = "Acesso rapido a materiais de escrita gradual para capricho grafico e rotina de copia.",
                        WhenToUse = "Quando a semana pedir reforco de letra, copia curta e melhora do tracado.",
                        HowItEnters = "No sistema isso entra como complemento leve, nao como carga principal da aula.",
                        EvidenceIdea = "Foto da atividade final com observacao curta do adulto.",
                        OfficialUrl = "https://www.baixelivros.com.br/acervo/caligrafia",
                        ActionLabel = "Abrir acervo de caligrafia"
                    }
                ]
            }
        ];
    }

    public static ExternalContentCatalogItem GetRequired(string slug)
    {
        var item = Build().FirstOrDefault(x => string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return item ?? throw new InvalidOperationException($"Conteudo externo nao encontrado: {slug}");
    }
}

public sealed class ExternalContentCatalogItem
{
    public string Slug { get; init; } = string.Empty;
    public string CategoryTitle { get; init; } = string.Empty;
    public string CategoryDescription { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public int AgeMin { get; init; }
    public int AgeMax { get; init; }
    public LearningDomain Domain { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string WhyItMatters { get; init; } = string.Empty;
    public string PortugueseGuideNote { get; init; } = string.Empty;
    public string FormatLabel { get; init; } = string.Empty;
    public string SourceLabel { get; init; } = string.Empty;
    public List<string> GoalTracks { get; init; } = new();
    public string MatchKeywords { get; init; } = string.Empty;
    public string OfficialUrl { get; init; } = string.Empty;
    public string OfficialActionLabel { get; init; } = string.Empty;
    public string AudienceLabel { get; init; } = string.Empty;
    public string AudienceChipClass { get; init; } = "neutral";
    public bool HasGuide { get; init; }
    public string GuideLabel { get; init; } = string.Empty;
    public string AdaptedHeadline { get; init; } = string.Empty;
    public string Intro { get; init; } = string.Empty;
    public List<string> ParentSteps { get; init; } = new();
    public List<string> WeeklyRhythm { get; init; } = new();
    public List<string> EvidenceIdeas { get; init; } = new();
    public List<string> FolderHighlights { get; init; } = new();
    public List<string> StarterMaterials { get; init; } = new();
    public List<string> Highlights { get; init; } = new();
    public List<ExternalContentFocusOption> FocusOptions { get; init; } = new();
}

public sealed class ExternalContentFocusOption
{
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string WhenToUse { get; init; } = string.Empty;
    public string HowItEnters { get; init; } = string.Empty;
    public string EvidenceIdea { get; init; } = string.Empty;
    public string OfficialUrl { get; init; } = string.Empty;
    public string ActionLabel { get; init; } = "Abrir fonte oficial";
}
