using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class ParentAcademyService
{
    public ParentAcademyViewModel BuildViewModel(
        string parentName,
        IReadOnlyList<ParentAcademyChildOptionViewModel> children,
        Guid? selectedChildId,
        string selectedChildName,
        SupportProfile? supportProfile,
        IReadOnlyList<SystemCurriculumTrackViewModel> systemCurriculumTracks,
        IReadOnlyList<ParentAcademyResourceViewModel> hostedLibrary,
        IReadOnlyList<ParentAcademyCategoryViewModel>? categories = null)
    {
        var note = supportProfile switch
        {
            SupportProfile.TeaLevel1 => $"A biblioteca foi organizada para {selectedChildName}, com rotina previsível, linguagem simples e começo rápido.",
            SupportProfile.TeaLevel2 => $"A biblioteca foi organizada para {selectedChildName}, com mais apoio visual, transições curtas e etapas bem claras.",
            SupportProfile.TeaLevel3 => $"A biblioteca foi organizada para {selectedChildName}, com previsibilidade máxima, tarefas concretas e autonomia passo a passo.",
            _ => "Aqui ficam os materiais em português, os guias adaptados e as referências certas para a família estudar sem sair caçando conteúdo."
        };
        var resolvedCategories = categories?.ToList() ?? BuildCategories(supportProfile);
        var hostedLibraryNote = hostedLibrary.Count == 0
            ? "Ainda nao ha apostila hospedada em portugues para esta faixa etaria."
            : "Esses materiais ficam dentro da plataforma porque a origem permite uso e redistribuicao com credito claro.";

        return new ParentAcademyViewModel
        {
            ParentName = parentName,
            SelectedChildId = selectedChildId,
            SelectedChildName = selectedChildName,
            PersonalizationNote = note,
            Children = children.ToList(),
            QuickStartCards = BuildQuickStartCards(systemCurriculumTracks, hostedLibrary, resolvedCategories),
            SystemCurriculumTracks = systemCurriculumTracks.ToList(),
            HostedLibraryNote = hostedLibraryNote,
            HostedLibrary = hostedLibrary.ToList(),
            Categories = resolvedCategories
        };
    }

    private static List<ParentAcademyQuickStartCardViewModel> BuildQuickStartCards(
        IReadOnlyList<SystemCurriculumTrackViewModel> systemCurriculumTracks,
        IReadOnlyList<ParentAcademyResourceViewModel> hostedLibrary,
        IReadOnlyList<ParentAcademyCategoryViewModel> categories)
    {
        var cards = new List<ParentAcademyQuickStartCardViewModel>();
        var primaryTrack = systemCurriculumTracks.FirstOrDefault();
        if (primaryTrack is not null)
        {
            cards.Add(new ParentAcademyQuickStartCardViewModel
            {
                Eyebrow = "Use primeiro",
                Title = primaryTrack.Title,
                Summary = $"Comece por {primaryTrack.CurrentFocus.ToLowerInvariant()} e siga a etapa atual do ano sem improvisar.",
                PrimaryActionLabel = "Abrir currículo próprio",
                PrimaryActionUrl = "#system-curriculum-tracks",
                SecondaryActionLabel = "Ver sequência do ano",
                SecondaryActionUrl = "#system-curriculum-tracks"
            });
        }

        var hosted = hostedLibrary.FirstOrDefault();
        if (hosted is not null)
        {
            cards.Add(new ParentAcademyQuickStartCardViewModel
            {
                Eyebrow = "Apostila em português",
                Title = hosted.Title,
                Summary = hosted.PortugueseGuideNote,
                PrimaryActionLabel = hosted.AccessLabel,
                PrimaryActionUrl = hosted.Url,
                SecondaryActionLabel = "Ver materiais hospedados",
                SecondaryActionUrl = "#hosted-library"
            });
        }

        var guidedExternal = categories
            .SelectMany(category => category.Resources)
            .OrderByDescending(GetQuickStartPriority)
            .FirstOrDefault(resource => resource.HasGuide)
            ?? categories
                .SelectMany(category => category.Resources)
                .OrderByDescending(GetQuickStartPriority)
                .FirstOrDefault(resource => resource.HasUrl);
        if (guidedExternal is not null)
        {
            cards.Add(new ParentAcademyQuickStartCardViewModel
            {
                Eyebrow = "Reforço da semana",
                Title = guidedExternal.Title,
                Summary = guidedExternal.WhyItMatters,
                PrimaryActionLabel = guidedExternal.HasGuide ? guidedExternal.GuideLabel : guidedExternal.AccessLabel,
                PrimaryActionUrl = guidedExternal.HasGuide ? guidedExternal.GuideUrl : guidedExternal.Url,
                SecondaryActionLabel = "Ver coleções organizadas",
                SecondaryActionUrl = "#guided-collections"
            });
        }

        return cards;
    }

    private static int GetQuickStartPriority(ParentAcademyResourceViewModel resource)
    {
        var score = 0;

        if (resource.SourceLabel.Contains("Educalar", StringComparison.OrdinalIgnoreCase) ||
            resource.SourceLabel.Contains("Baixe Livros", StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        if (resource.FormatLabel.Contains("portugues", StringComparison.OrdinalIgnoreCase) ||
            resource.Summary.Contains("portugues", StringComparison.OrdinalIgnoreCase) ||
            resource.PortugueseGuideNote.Contains("portugues", StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        if (resource.HasGuide)
        {
            score += 12;
        }

        return score;
    }

    private static List<ParentAcademyCategoryViewModel> BuildCategories(SupportProfile? supportProfile)
    {
        var regulationAudience = supportProfile switch
        {
            SupportProfile.TeaLevel2 or SupportProfile.TeaLevel3 => "Muito prioritário",
            _ => "Prioritário"
        };

        return
        [
            new()
            {
                Title = "Comece hoje sem se perder",
                Description = "O mínimo que um pai precisa para abrir o sistema, dar a aula do dia e sair com prova registrada sem transformar o processo numa bagunça.",
                Resources =
                [
                    Internal(
                        "Checklist da aula do dia em 4 passos",
                        "Escolha a criança, leia o objetivo, aplique um bloco por vez e salve uma evidência no fim.",
                        "Esse é o fluxo principal do produto e precisa funcionar mesmo para quem não tem experiência pedagógica.",
                        "Playbook interno",
                        "3 min",
                        "Rotina base",
                        "success",
                        "O sistema pode mostrar essa lógica em português simples, como guia fixo para toda aula."
                    ),
                    Internal(
                        "Como registrar prova com foto, vídeo e nota curta",
                        "Use uma imagem do trabalho, um vídeo curto da criança explicando o que fez e duas frases objetivas sobre avanço e dificuldade.",
                        "Transforma atividade feita em histórico pedagógico utilizável.",
                        "Guia prático",
                        "5 min",
                        "Prova e registro",
                        "track-academic",
                        "Esse método deve aparecer como orientação nativa do NewSchool, e não como material externo."
                    ),
                    Internal(
                        "Como montar uma semana que a família consegue cumprir",
                        "Defina um foco por criança, mantenha blocos curtos, reserve um dia leve e feche toda semana com um resumo simples.",
                        "Sem uma semana enxuta, o sistema vira mais uma tela aberta e não uma rotina usada de verdade.",
                        "Guia interno",
                        "8 min",
                        "Pais iniciantes",
                        "neutral",
                        "O texto pode ser nosso em PT-BR, transformando método em passo a passo de uso."
                    )
                ]
            },
            new()
            {
                Title = "3 a 6 anos: pré-alfabetização e começo da rotina",
                Description = "Fontes para letras, sons, coordenação, contagem e atividades de entrada no ensino domiciliar, sempre com adaptação para uso guiado pelos pais.",
                Resources =
                [
                    ThirdParty(
                        "Easy Peasy Getting Ready 1",
                        "Curso introdutório com foco forte em pré-leitura, reconhecimento de letras, histórias fonéticas e pequenas atividades impressas. Conteúdo original em inglês.",
                        "Ajuda a estruturar os primeiros meses de rotina para crianças pequenas sem partir do zero.",
                        "Currículo externo",
                        "Uso contínuo",
                        "Easy Peasy",
                        "3 a 5 anos",
                        "success",
                        "Dentro do sistema, o ideal é mostrar um resumo em português do método e mandar o pai para a fonte original quando precisar da sequência completa.",
                        "Abrir currículo original",
                        "https://allinonehomeschool.com/getting-ready-1/"
                    ),
                    ThirdParty(
                        "Easy Peasy Preschool and Kindergarten Overview",
                        "Visão geral de como os cursos Getting Ready 1 e 2 trabalham alfabeto, revisão por semanas e início da autonomia. Conteúdo original em inglês.",
                        "Serve como mapa macro para adaptar o currículo automático do sistema por faixa etária.",
                        "Visão curricular",
                        "Leitura rápida",
                        "Easy Peasy",
                        "3 a 6 anos",
                        "neutral",
                        "O NewSchool pode resumir a estrutura em português e usar isso para sugerir rotinas e tarefas próprias.",
                        "Abrir visão geral original",
                        "https://allinonehomeschool.com/getting-ready-1-and-getting-ready-2/"
                    ),
                    ThirdParty(
                        "Getting Ready 1 Printables PDF",
                        "Packet oficial de printáveis com apoio para letras, histórias e atividades iniciais do curso.",
                        "Acelera a criação de tarefas prontas para as primeiras semanas de alfabetização.",
                        "PDF oficial",
                        "Baixar e usar",
                        "Easy Peasy",
                        "Pré-alfabetização",
                        "track-communication",
                        "O sistema pode apontar o download direto e explicar em português quais páginas usar por objetivo da semana.",
                        "Abrir PDF original",
                        "https://allinonehomeschool.com/wp-content/uploads/2012/01/gr1-boys-printables.pdf"
                    ),
                    ThirdParty(
                        "Aprendendo o Alfabeto",
                        "PDF público para alfabetização inicial com apoio impresso rápido em português.",
                        "É um material direto para imprimir e transformar em tarefa pronta dentro do sistema.",
                        "PDF público",
                        "Baixar e usar",
                        "Archive Public Domain",
                        "Alfabetização",
                        "track-communication",
                        "Esse pode ser integrado com muito mais liberdade por ser público, mas ainda assim convém manter a origem clara.",
                        "Abrir PDF público",
                        "https://archivepublicdomain.com/files/2025/09/aprendendo-o-alfabeto.pdf?t=ead6113d4b3b96f8f57f98a45259e9259cc5c19d7630e86fe2ab7038006d532e"
                    ),
                    ThirdParty(
                        "Educalar: materiais gratuitos",
                        "Acervo em português com atividades e recursos pensados para educação domiciliar e apoio ao desenvolvimento infantil.",
                        "É uma fonte brasileira prática para abastecer tarefas prontas e reforços semanais.",
                        "Acervo gratuito",
                        "Navegação guiada",
                        "Educalar",
                        "Pais brasileiros",
                        "track-dailyliving",
                        "A academia pode explicar o uso por idade e deixar a família abrir o material no site de origem.",
                        "Abrir fonte original",
                        "https://www.educalar.com.br/materiais-gratuitos"
                    )
                ]
            },
            new()
            {
                Title = "6 a 10 anos: currículo organizado por matéria",
                Description = "Fontes mais estruturadas para leitura, escrita, matemática, ciências, história e formação cultural, com foco em organização por idade e sequência.",
                Resources =
                [
                    ThirdParty(
                        "AmblesideOnline Curriculum Overview",
                        "Panorama de anos, áreas e estrutura de um currículo Charlotte Mason com muitos livros e recursos gratuitos. Conteúdo original em inglês.",
                        "É referência útil para desenhar trilhas mais ricas de leitura, história, ciência e artes dentro do sistema.",
                        "Currículo externo",
                        "Uso como base",
                        "AmblesideOnline",
                        "6 a 10 anos",
                        "track-academic",
                        "O sistema pode trazer um resumo orientado em português e direcionar para a página original para a estrutura completa.",
                        "Abrir currículo original",
                        "https://www.amblesideonline.org/curriculum"
                    ),
                    ThirdParty(
                        "AmblesideOnline Year 1 36-week schedule",
                        "Quadro semanal oficial do Year 1 com divisão por termos e semanas.",
                        "Ajuda a enxergar como quebrar um currículo anual em porções pequenas e utilizáveis.",
                        "Plano anual",
                        "Consultar por semanas",
                        "AmblesideOnline",
                        "Planejamento",
                        "neutral",
                        "O NewSchool pode converter essa lógica em tarefas nossas, sem precisar duplicar o quadro inteiro dentro da plataforma.",
                        "Abrir plano original",
                        "https://www.amblesideonline.org/ao-y1-sch"
                    ),
                    ThirdParty(
                        "AmblesideOnline By Year",
                        "Página de navegação por anos, do Year 0 até o Year 12.",
                        "É um bom índice para famílias que queiram enxergar continuidade do currículo ao longo do tempo.",
                        "Índice por ano",
                        "Consulta rápida",
                        "AmblesideOnline",
                        "Escala anual",
                        "success",
                        "Pode entrar no sistema como biblioteca de referência por faixa etária, sempre apontando para a origem.",
                        "Abrir navegação original",
                        "https://amblesideonline.org/years"
                    ),
                    ThirdParty(
                        "Baixe Livros Didáticos",
                        "Biblioteca com materiais separados por educação infantil e fundamental, útil para reforço, impressão e apoio curricular em português.",
                        "Centraliza opções brasileiras por etapa escolar sem depender apenas de um único fornecedor.",
                        "Biblioteca externa",
                        "Navegação guiada",
                        "Baixe Livros",
                        "Infantil ao 9º ano",
                        "success",
                        "No sistema, isso deve aparecer como catálogo de apoio externo com orientação do que baixar primeiro.",
                        "Abrir biblioteca original",
                        "https://www.baixelivros.com.br/didaticos#1"
                    ),
                    Internal(
                        "Trilha automática de linguagem, matemática, mundo real e função executiva",
                        "O sistema deve quebrar cada semana em quatro frentes: leitura e escrita, raciocínio lógico, conteúdo de mundo e autonomia para estudar.",
                        "Essa divisão deixa o currículo automático claro e simples para o adulto acompanhar.",
                        "Diretriz interna",
                        "Sempre ativa",
                        "Currículo automático",
                        "track-academic",
                        "Esse é o pedaço que precisa ser autoral do produto, porque organiza tudo o que vem de fora em um fluxo só."
                    )
                ]
            },
            new()
            {
                Title = "Printáveis, PDFs e atividades prontas",
                Description = "Fontes de impressão para abastecer tarefas, trabalhos e aulas do dia sem a família sair procurando arquivo por arquivo na internet.",
                Resources =
                [
                    ThirdParty(
                        "Easy Peasy Print Page",
                        "Página oficial com pacotes de printáveis por disciplina e nível, incluindo pré-escola, linguagem, matemática, ciências e saúde.",
                        "É uma das melhores portas de entrada para converter currículo externo em tarefas prontas dentro do sistema.",
                        "Printáveis oficiais",
                        "Uso recorrente",
                        "Easy Peasy",
                        "Impressão",
                        "success",
                        "O sistema pode classificar essa página por matéria e idade, mantendo o download sempre no endereço original.",
                        "Abrir página oficial",
                        "https://allinonehomeschool.com/the-print-page/"
                    ),
                    ThirdParty(
                        "Language Arts 1 Copywork",
                        "PDF oficial de copywork de Language Arts 1, pensado para facilitar a prática escrita da criança.",
                        "É um exemplo claro de material terceiro que o sistema pode recomendar sem precisar hospedar o arquivo.",
                        "PDF oficial",
                        "Baixar e usar",
                        "Easy Peasy",
                        "Escrita inicial",
                        "track-communication",
                        "O NewSchool pode traduzir as instruções de uso e deixar o PDF ser aberto na fonte deles.",
                        "Abrir PDF original",
                        "https://allinonehomeschool.com/wp-content/uploads/2023/01/Language-Arts-1-Copywork.pdf"
                    ),
                    ThirdParty(
                        "Math 1 Printables",
                        "Pacote oficial de printáveis de matemática inicial para contagem, fatos básicos e noção de número.",
                        "Ajuda a transformar a aula do dia em folha concreta, especialmente para pais que preferem atividade impressa.",
                        "PDF oficial",
                        "Baixar e usar",
                        "Easy Peasy",
                        "Matemática inicial",
                        "track-academic",
                        "O sistema pode sugerir páginas específicas por objetivo, sem baixar nem republicar o pacote.",
                        "Abrir PDF original",
                        "https://allinonehomeschool.com/wp-content/uploads/2017/12/third-edition-math-1-printables2.pdf"
                    ),
                    ThirdParty(
                        "Easy Peasy Getting Ready 2 Course of Study",
                        "Documento oficial com a sequência do curso seguinte, útil para prolongar a trilha depois do nível introdutório.",
                        "Dá continuidade organizada para a fase em que a criança já começou a reconhecer letras e seguir rotina.",
                        "PDF de curso",
                        "Uso como base",
                        "Easy Peasy",
                        "Sequência do ciclo",
                        "neutral",
                        "Isso pode alimentar o currículo automático como referência de progressão, mantendo a consulta no PDF deles.",
                        "Abrir PDF original",
                        "https://allinonehomeschool.com/wp-content/uploads/2018/11/ep-getting-ready-2-course-of-study.pdf"
                    ),
                    Internal(
                        "Baixe Livros e Educalar como reserva tática",
                        "Quando faltar folha, atividade de reforço ou material de transição, essas duas bibliotecas viram o apoio rápido da semana.",
                        "Evita que o pai pare a rotina por falta de recurso impresso.",
                        "Estratégia de uso",
                        "Consulta contínua",
                        "Plano B",
                        "warning",
                        "O papel do sistema aqui é dizer o que buscar, em que ordem e para qual objetivo da criança."
                    )
                ]
            },
            new()
            {
                Title = "Provas, evolução e histórico pedagógico",
                Description = "Tudo que ajuda o sistema a deixar de ser só um lugar para estudar e virar também um arquivo sério de progresso da criança.",
                Resources =
                [
                    Internal(
                        "O que salvar em toda atividade concluída",
                        "Tema da aula, minutos, acerto principal, dificuldade principal, uma foto ou vídeo e, quando fizer sentido, uma fala da criança.",
                        "Esse pacote mínimo já constrói currículo visível e evolução rastreável.",
                        "Checklist interno",
                        "2 min",
                        "Registro essencial",
                        "success",
                        "Essa parte precisa ser padrão do sistema e aparecer embutida no fluxo de registro."
                    ),
                    Internal(
                        "Como transformar conteúdo externo em tarefa do sistema",
                        "Pegue um PDF ou página externa, defina objetivo, blocos, materiais, evidência esperada e próximo passo.",
                        "É a ponte entre 'conteúdo na internet' e 'aula mastigada para os pais'.",
                        "Método interno",
                        "7 min",
                        "Organização automática",
                        "track-academic",
                        "Esse é exatamente o papel do NewSchool: transformar fontes dispersas em trabalho organizado."
                    ),
                    Internal(
                        "Rotina de regulação e retomada",
                        "Nos dias em que a criança não sustenta a aula inteira, reduza a carga, preserve a evidência e marque o que funcionou.",
                        "A evolução real aparece quando o sistema também sabe registrar dias difíceis sem virar fracasso.",
                        "Protocolo interno",
                        "5 min",
                        regulationAudience,
                        "warning",
                        "O método pode ser nosso, mesmo usando fontes externas só como apoio pedagógico."
                    )
                ]
            }
        ];
    }

    private static ParentAcademyResourceViewModel Internal(
        string title,
        string summary,
        string whyItMatters,
        string formatLabel,
        string durationLabel,
        string audienceLabel,
        string audienceChipClass,
        string portugueseGuideNote)
    {
        return new ParentAcademyResourceViewModel
        {
            Title = title,
            Summary = summary,
            WhyItMatters = whyItMatters,
            PortugueseGuideNote = portugueseGuideNote,
            FormatLabel = formatLabel,
            DurationLabel = durationLabel,
            SourceLabel = "NewSchool",
            OwnershipLabel = "Conteúdo do sistema",
            IsThirdParty = false,
            AccessLabel = "Usar no fluxo do sistema",
            AudienceLabel = audienceLabel,
            AudienceChipClass = audienceChipClass
        };
    }

    private static ParentAcademyResourceViewModel ThirdParty(
        string title,
        string summary,
        string whyItMatters,
        string formatLabel,
        string durationLabel,
        string sourceLabel,
        string audienceLabel,
        string audienceChipClass,
        string portugueseGuideNote,
        string accessLabel,
        string url)
    {
        return new ParentAcademyResourceViewModel
        {
            Title = title,
            Summary = summary,
            WhyItMatters = whyItMatters,
            PortugueseGuideNote = portugueseGuideNote,
            FormatLabel = formatLabel,
            DurationLabel = durationLabel,
            SourceLabel = sourceLabel,
            OwnershipLabel = "Produto de terceiro",
            IsThirdParty = true,
            AccessLabel = accessLabel,
            Url = url,
            AudienceLabel = audienceLabel,
            AudienceChipClass = audienceChipClass
        };
    }
}
