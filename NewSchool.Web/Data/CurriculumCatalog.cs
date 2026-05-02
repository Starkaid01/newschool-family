using NewSchool.Web.Domain;

namespace NewSchool.Web.Data;

public static class CurriculumCatalog
{
    public static List<CurriculumTemplate> Build()
    {
        var items = new List<CurriculumTemplate>();
        var counters = new Dictionary<(int Age, LearningDomain Domain), int>();

        foreach (var profile in Profiles)
        {
            var teaCommonCommunication = profile.TeaCommonCommunication.Count > 0
                ? profile.TeaCommonCommunication
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Communication);
            var teaCommonRegulation = profile.TeaCommonExecutive.Count > 0
                ? profile.TeaCommonExecutive
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Regulation);
            var teaLevel1Communication = profile.TeaLevel1Communication.Count > 0
                ? profile.TeaLevel1Communication
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Communication);
            var teaLevel1Regulation = profile.TeaLevel1Executive.Count > 0
                ? profile.TeaLevel1Executive
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Regulation);
            var teaLevel2Communication = profile.TeaLevel2Communication.Count > 0
                ? profile.TeaLevel2Communication
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Communication);
            var teaLevel2Regulation = profile.TeaLevel2Executive.Count > 0
                ? profile.TeaLevel2Executive
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Regulation);
            var teaLevel3Communication = profile.TeaLevel3Communication.Count > 0
                ? profile.TeaLevel3Communication
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Communication);
            var teaLevel3Regulation = profile.TeaLevel3Executive.Count > 0
                ? profile.TeaLevel3Executive
                : BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Regulation);

            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "balanced_growth", profile.LanguageCore, 2, "base de linguagem e leitura", "escuta, reconto e escrita funcional");
            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "literacy", profile.LanguageTrack, 2, "trilha forte de alfabetizacao", "fonica, fluencia e producao escrita");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "balanced_growth", profile.MathCore, 2, "base matematica do ano", "numero, problema e representacao");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "math_foundations", profile.MathTrack, 2, "trilha forte de matematica base", "concreto, estrategia e registro");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "balanced_growth", profile.WorldCore, 3, "repertorio de mundo real", "observacao, pergunta e descoberta");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "science_discovery", profile.WorldTrack, 3, "trilha de ciencias e investigacao", "hipotese, experimento e conclusao");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "balanced_growth", profile.ExecutiveCore, 2, "base de foco e autonomia", "rotina, iniciacao e fechamento");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.General, FunctionalSupportTrack.Base, "autonomy", profile.ExecutiveTrack, 2, "trilha forte de autonomia", "planejamento, foco e autorregulacao");

            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Communication, "literacy", teaCommonCommunication, 2, "base de comunicacao funcional", "comunicacao, compreensao e resposta funcional");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Regulation, "autonomy", teaCommonRegulation, 2, "base de previsibilidade, regulacao e rotina", "transicao, autorregulacao e previsibilidade");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Sensory, "science_discovery", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Sensory), 3, "base de leitura sensorial do ambiente", "ajuste sensorial, pausa e retorno");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.DailyLiving, "autonomy", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.DailyLiving), 2, "base de vida diaria e sequencias funcionais", "cuidado, organizacao e rotina funcional");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.AcademicAdapted, "math_foundations", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.AcademicAdapted), 2, "base academica com acesso adaptado", "entrada na tarefa, resposta funcional e generalizacao");

            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Communication, "literacy", teaLevel1Communication, 2, "comunicacao pragmatica e ampliacao da linguagem", "comunicacao social, linguagem e contexto");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Regulation, "autonomy", teaLevel1Regulation, 2, "flexibilidade, autonomia e organizacao do estudo", "planejamento, flexibilidade e autonomia");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Sensory, "science_discovery", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Sensory), 3, "integracao sensorial com mais autorrelato", "percepcao do ambiente, ajuste e retorno");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.DailyLiving, "autonomy", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.DailyLiving), 2, "vida diaria com mais planejamento e flexibilidade", "organizacao funcional, sequencia e autonomia");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.AcademicAdapted, "math_foundations", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.AcademicAdapted), 2, "acesso academico adaptado com mais generalizacao", "estrategia, transferencia e explicacao guiada");

            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Communication, "literacy", teaLevel2Communication, 2, "comunicacao funcional com apoio visual", "pedido, resposta funcional e ampliacao gradual");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Regulation, "autonomy", teaLevel2Regulation, 2, "rotina guiada, passos menores e apoio visual intenso", "previsibilidade, regulacao e sequencia guiada");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Sensory, "science_discovery", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Sensory), 3, "regulacao sensorial com apoio visual intenso", "preparo do corpo, pausa e reorganizacao");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.DailyLiving, "autonomy", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.DailyLiving), 2, "vida diaria com passos menores e reforcadores frequentes", "autocuidado, rotina e transicao guiada");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.AcademicAdapted, "math_foundations", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.AcademicAdapted), 2, "acesso academico altamente apoiado", "pareamento, apoio visual e resposta guiada");

            AddSeries(items, counters, profile.Age, LearningDomain.Language, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Communication, "literacy", teaLevel3Communication, 2, "comunicacao funcional altamente estruturada", "associacao direta, resposta funcional e comunicacao alternativa");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Regulation, "autonomy", teaLevel3Regulation, 2, "seguranca, rotina muito estruturada e vida diaria", "previsibilidade, regulacao primaria e conclusao concreta");
            AddSeries(items, counters, profile.Age, LearningDomain.World, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Sensory, "science_discovery", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Sensory), 3, "seguranca sensorial e preparacao para aprender", "ambiente seguro, entrada sensorial e retorno funcional");
            AddSeries(items, counters, profile.Age, LearningDomain.ExecutiveFunction, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.DailyLiving, "autonomy", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.DailyLiving), 2, "vida diaria muito concreta e encadeada", "sequencia funcional, ajuda total e ganho gradual");
            AddSeries(items, counters, profile.Age, LearningDomain.Math, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.AcademicAdapted, "math_foundations", BuildTeaTrackTitles(profile.Age, CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.AcademicAdapted), 2, "entrada academica altamente estruturada", "pareamento concreto, resposta curta e conclusao observavel");
        }

        return items;
    }

    private static void AddSeries(
        ICollection<CurriculumTemplate> items,
        IDictionary<(int Age, LearningDomain Domain), int> counters,
        int age,
        LearningDomain domain,
        CurriculumSupportScope supportScope,
        FunctionalSupportTrack functionalTrack,
        string goalTrack,
        IReadOnlyList<string> titles,
        int reviewAfterDays,
        string goalStem,
        string evaluationStem)
    {
        string? prerequisite = null;
        foreach (var title in titles)
        {
            prerequisite = Add(items, counters, age, domain, supportScope, goalTrack, title,
                $"Fortalecer {goalStem} por meio da habilidade {title.ToLowerInvariant()}.",
                BuildMaterials(domain, goalTrack, age, title),
                BuildGuide(domain, goalTrack, age, title, supportScope, functionalTrack),
                BuildMission(domain, age, title),
                BuildEvidence(domain, evaluationStem, title),
                functionalTrack,
                prerequisite,
                reviewAfterDays);
        }
    }

    private static string Add(
        ICollection<CurriculumTemplate> items,
        IDictionary<(int Age, LearningDomain Domain), int> counters,
        int age,
        LearningDomain domain,
        CurriculumSupportScope supportScope,
        string goalTrack,
        string title,
        string goal,
        string materials,
        string guide,
        string mission,
        string evidence,
        FunctionalSupportTrack functionalTrack,
        string? prerequisiteSkillCode,
        int reviewAfterDays)
    {
        var key = (age, domain);
        counters.TryGetValue(key, out var current);
        current += 1;
        counters[key] = current;
        var skillCode = $"A{age}-{domain}-{current}";

        items.Add(new CurriculumTemplate
        {
            Id = Guid.NewGuid(),
            Age = age,
            Domain = domain,
            SupportScope = supportScope,
            FunctionalTrack = functionalTrack,
            GoalTrack = goalTrack,
            SkillCode = skillCode,
            PrerequisiteSkillCode = prerequisiteSkillCode ?? string.Empty,
            SkillName = title,
            Title = title,
            Goal = goal,
            Materials = materials,
            ParentGuide = guide,
            ChildMission = mission,
            EvidencePrompt = evidence,
            ReviewAfterDays = reviewAfterDays,
            SortOrder = current
        });

        return skillCode;
    }

    private static string BuildMaterials(LearningDomain domain, string goalTrack, int age, string title) => domain switch
    {
        LearningDomain.Language when goalTrack == "literacy" => $"Cartoes, letras moveis, caderno leve e material concreto ligados a {title.ToLowerInvariant()}.",
        LearningDomain.Language => $"Livro curto, figuras, cartoes e caderno para praticar {title.ToLowerInvariant()} aos {age} anos.",
        LearningDomain.Math when goalTrack == "math_foundations" => $"Objetos para contar, representar e desenhar a ideia de {title.ToLowerInvariant()}.",
        LearningDomain.Math => $"Material concreto, quadro simples e registro curto para trabalhar {title.ToLowerInvariant()}.",
        LearningDomain.World => $"Objeto real, imagem forte ou mini experimento para explorar {title.ToLowerInvariant()}.",
        _ => $"Checklist visual, cronometro curto e apoio concreto para treinar {title.ToLowerInvariant()}."
    };

    private static string BuildGuide(LearningDomain domain, string goalTrack, int age, string title, CurriculumSupportScope supportScope, FunctionalSupportTrack functionalTrack)
    {
        var baseGuide = domain switch
        {
            LearningDomain.Language when goalTrack == "literacy" => $"Modele {title.ToLowerInvariant()} em microetapas, faça a crianca ouvir, repetir, apontar e registrar com apoio proporcional para a faixa de {age} anos.",
            LearningDomain.Language => $"Conduza {title.ToLowerInvariant()} com leitura viva, conversa breve, reconto e registro curto, evitando explicacoes longas.",
            LearningDomain.Math when goalTrack == "math_foundations" => $"Comece no concreto, passe para desenho e so depois avance para simbolo ao trabalhar {title.ToLowerInvariant()}.",
            LearningDomain.Math => $"Use problema curto, raciocinio em voz alta e material concreto para dar sentido a {title.ToLowerInvariant()}.",
            LearningDomain.World => $"Abra com observacao, faça uma pergunta forte e feche com fala ou registro simples sobre {title.ToLowerInvariant()}.",
            _ => $"Quebre {title.ToLowerInvariant()} em passos visiveis, combine inicio claro, ajuda medida e checagem de termino."
        };

        return $"{baseGuide} {BuildFunctionalTrackGuide(functionalTrack)} {BuildSupportGuide(supportScope)}".Trim();
    }

    private static string BuildMission(LearningDomain domain, int age, string title) => domain switch
    {
        LearningDomain.Language => $"Missao de {age} anos: mostrar {title.ToLowerInvariant()} com fala, leitura ou escrita curta.",
        LearningDomain.Math => $"Missao de {age} anos: resolver {title.ToLowerInvariant()} explicando como pensou.",
        LearningDomain.World => $"Missao de {age} anos: investigar {title.ToLowerInvariant()} e contar a descoberta principal.",
        _ => $"Missao de {age} anos: praticar {title.ToLowerInvariant()} ate concluir com mais autonomia."
    };

    private static string BuildEvidence(LearningDomain domain, string evaluationStem, string title) => domain switch
    {
        LearningDomain.Language => $"Registrar audio, foto ou frase curta mostrando {title.ToLowerInvariant()} com clareza dentro de {evaluationStem}.",
        LearningDomain.Math => $"Registrar representacao concreta, desenho ou resolucao curta de {title.ToLowerInvariant()} dentro de {evaluationStem}.",
        LearningDomain.World => $"Registrar fala, foto ou observacao breve que prove {title.ToLowerInvariant()} em {evaluationStem}.",
        _ => $"Registrar video curto, checklist ou autoavaliacao simples mostrando {title.ToLowerInvariant()} em {evaluationStem}."
    };

    private static readonly IReadOnlyList<AgeCurriculumProfile> Profiles =
    [
        new(3, ["Sons do proprio nome", "Vocabulário do cotidiano", "Cantiga com repeticao", "Reconto de imagem simples"], ["Rimas do cotidiano", "Escuta de sílabas orais", "Traços e marcas iniciais", "Alfabeto sensorial inicial"], ["Contagem concreta ate 5", "Comparacao de grandezas", "Conjuntos do dia a dia", "Padroes A-B simples"], ["Numero e quantidade ate 5", "Juntar e tirar com objetos", "Formas do cotidiano", "Classificacao e seriacao"], ["Rotina e natureza", "Animais e seus ambientes"], ["Descoberta com agua e cor", "Observacao guiada do quintal"], ["Missao de dois passos", "Esperar, terminar e guardar"], ["Foco de 5 minutos", "Rotina visual de inicio e fim"]),
        new(4, ["Rimas e aliteracao", "Escuta e reconto", "Frase oral com clareza", "Traçado funcional inicial"], ["Consciência fonologica inicial", "Sílabas com palmas", "Correspondencia som-letra inicial", "Pre-escrita de palavras conhecidas"], ["Contagem ate 10", "Mais, menos e igual", "Padroes e sequencias", "Posicao e orientacao espacial"], ["Numero e quantidade ate 10", "Compor e decompor pequenas quantidades", "Problema oral de uma etapa", "Formas e classificacao"], ["Animais e habitats", "Plantas e clima"], ["Mini experimento de mistura", "Observacao de crescimento e mudanca"], ["Atenção e memoria curta", "Seguir combinados do bloco"], ["Iniciar sem enrolar", "Concluir e revisar materiais"]),
        new(5, ["Escuta e reconto com começo, meio e fim", "Frase com sentido", "Leitura de imagens", "Vocabulário por categorias"], ["Pre-alfabetizacao fonica", "Sílabas e palavras familiares", "Leitura de palavras simples", "Escrita inicial apoiada"], ["Contagem e sequencia ate 20", "Juntar e tirar ate 10", "Comparar quantidades", "Padroes e classificacoes"], ["Numero, traco e quantidade", "Problema concreto de adicao", "Problema concreto de subtracao", "Compor e decompor ate 10"], ["Conhecimento de mundo e comunidade", "Tempo, clima e rotina"], ["Experimento com previsao simples", "Registro de descoberta em desenho"], ["Planejar e terminar", "Foco com pausa curta"], ["Rotina com checklist", "Autonomia para buscar e guardar material"]),
        new(6, ["Leitura guiada de frases", "Compreensao literal", "Escrita de frases", "Reconto com detalhes"], ["Fluencia inicial de leitura", "Segmentacao e ortografia inicial", "Escrita com pontuacao apoiada", "Leitura de pequeno texto"], ["Numero ate 100", "Adicao e subtracao com sentido", "Sequencias e regularidades", "Medidas do cotidiano"], ["Dezenas e agrupamentos", "Problemas do cotidiano", "Registro matematico com desenho", "Comparacao e estimativa inicial"], ["Observacao cientifica", "Mapa do mundo proximo"], ["Experimento com hipotese", "Comparacao e conclusao simples"], ["Autonomia academica", "Revisao do proprio bloco"], ["Iniciar, manter e concluir sozinho", "Organizacao dos materiais da sessao"]),
        new(7, ["Compreensao de texto curto", "Escrita de pequeno paragrafo", "Resposta argumentada inicial", "Vocabulário em contexto"], ["Leio, penso e argumento", "Resumo oral e escrito", "Ortografia em palavras frequentes", "Produção de texto guiado"], ["Problemas de duas etapas simples", "Valor posicional", "Padroes e regularidades", "Fatos basicos com estrategia"], ["Estrategias de resolucao", "Raciocinio logico", "Representacao em barras ou esquemas", "Revisao de calculo com explicacao"], ["Ciencia e argumentacao", "Geografia do cotidiano"], ["Mini laboratorio com registro", "Pesquisa guiada por pergunta"], ["Planejamento e revisao", "Persistencia em tarefa maior"], ["Autogestao do bloco", "Checklist de planejamento e conferencia"]),
        new(8, ["Leitura com inferencia", "Paragrafo com ideia central", "Resumo de texto curto", "Resposta com evidência"], ["Producao de paragrafo", "Leitura orientada por pistas", "Revisao de frase e conectivo", "Ampliação de repertorio escrito"], ["Problemas em etapas", "Multiplicacao com sentido", "Divisao em contexto", "Representacao de dados simples"], ["Tabuada com sentido", "Modelo de barras e desenhos", "Problemas multietapas", "Frações concretas iniciais"], ["Pesquisa guiada", "Relacoes de causa e efeito"], ["Experimento com tabela simples", "Coleta e apresentacao de descoberta"], ["Projeto em etapas", "Revisao com criterio"], ["Planejamento com prazos curtos", "Autonomia com autochecagem"]),
        new(9, ["Compreensao comparativa", "Texto de opiniao", "Sintese de ideias", "Leitura com evidencias"], ["Escrita de opiniao", "Resumo e reorganizacao de texto", "Revisao de paragrafo", "Leitura crítica guiada"], ["Fracoes no cotidiano", "Problemas com registro", "Multiplicacao e divisao com estrategia", "Medidas e conversoes simples"], ["Resolucao com registro", "Problemas com mais de uma operacao", "Frações com representacao", "Raciocinio proporcional inicial"], ["Projeto de investigacao", "Mapa e sociedade"], ["Pergunta, dado e conclusao", "Pesquisa orientada com fonte curta"], ["Autonomia com revisao", "Gestao de tarefa mais longa"], ["Planejar, executar e revisar", "Prioridade, foco e entrega"]),
        new(10, ["Leitura critica e sintese", "Texto com estrutura forte", "Anotacao e resumo", "Argumentacao mais clara"], ["Revisao autoral de texto", "Sintese com palavras proprias", "Leitura informativa com criterio", "Produção escrita em varias etapas"], ["Desafios multietapas", "Padroes e proporcionalidade", "Frações e porcentagem inicial", "Representacao e justificativa"], ["Problema estrategico com checagem", "Modelo visual do raciocinio", "Razao e proporcionalidade inicial", "Algebra verbal inicial"], ["Projeto interdisciplinar", "Investigacao de mundo real"], ["Experimento com variaveis simples", "Pesquisa, sintese e apresentacao"], ["Gestao do proprio estudo", "Revisao semanal de metas"], ["Planejamento autonomo", "Autoavaliacao e ajuste de rotina"], ["Rotina visual com antecipacao", "Transicao com previsibilidade"], ["Comunicacao funcional com apoio visual", "Pedido e resposta funcional"], ["Flexibilidade com regras combinadas", "Organizacao de tarefa com escolha guiada"], ["Pragmatica em contexto", "Combinado social e resposta contextual"], ["Sequencia visual de passos", "Regulacao com pausa e retorno"], ["Comunicacao funcional guiada", "Nomeacao e pedido com suporte"], ["Seguranca, rotina e previsibilidade", "Conclusao concreta com ajuda total"], ["Comunicacao alternativa ou altamente apoiada", "Pareamento direto entre necessidade e resposta"]),
        new(11, ["Leitura informativa com anotacoes", "Tese e dois argumentos", "Sintese de duas fontes", "Revisao por criterio"], ["Artigo curto com evidencia", "Comparacao de pontos de vista", "Citacao, parafrase e registro", "Reescrita com proposito"], ["Razao e porcentagem do cotidiano", "Equacoes simples com sentido", "Problemas com tabela e grafico", "Planejamento financeiro inicial"], ["Proporcao em situacoes reais", "Equacao de uma etapa com justificativa", "Leitura critica de dados", "Percentual aplicado em compras"], ["Brasil contemporaneo e territorio", "Fontes e confiabilidade", "Ambiente, consumo e impacto", "Mapa, dado e sociedade"], ["Pesquisa com duas fontes", "Linha do tempo e consequencia historica", "Experimento com variaveis controladas", "Argumento com fato e causa"], ["Planejamento semanal guiado", "Checklist de revisao com criterio", "Estudo em blocos com pausa", "Registro do que aprendi"], ["Gestao de tarefa longa", "Prioridade e prazo curto", "Autonomia com roteiro de entrega", "Autoavaliacao da semana"]),
        new(12, ["Leitura de artigo e anotacao lateral", "Paragrafo com tese, prova e fecho", "Resumo tecnico com palavras proprias", "Comparacao entre texto e grafico"], ["Defesa de ideia com evidencia", "Contraexemplo e revisao", "Caderno de leitura com citacao util", "Reescrita para publico diferente"], ["Porcentagem em descontos e aumentos", "Equacoes com mais de uma etapa", "Razao, escala e comparacao", "Dados, media e interpretacao"], ["Tabela para justificar raciocinio", "Orcamento com escolhas limitadas", "Proporcionalidade em mapas e receitas", "Leitura critica de porcentagens"], ["Brasil, regioes e desigualdades", "Ciencia, tecnologia e sociedade", "Fontes historicas e ponto de vista", "Territorio, agua e energia"], ["Comparacao de fontes sobre o mesmo fato", "Mapa tematico e conclusao", "Investigacao de problema ambiental", "Relacao entre causa, impacto e resposta"], ["Plano semanal com metas reais", "Revisao antes de entregar", "Organizacao de materiais por materia", "Registro de duvida e proximo passo"], ["Autonomia em bloco de 30 minutos", "Gestao de projeto curto", "Fechamento com autochecagem", "Reflexao do que funcionou"]),
        new(13, ["Leitura critica com tese e vies", "Texto argumentativo em camadas", "Sintese de fontes com posicionamento", "Revisao de clareza e precisao"], ["Tese, prova e contra-argumento", "Artigo de opiniao curto", "Selecao de evidencias relevantes", "Reescrita para convencer"], ["Porcentagem, juros e orcamento", "Algebra verbal e equacoes", "Razao, proporcao e escalas reais", "Dados para tomada de decisao"], ["Planejamento financeiro com justificativa", "Modelo algebrico de situacao real", "Comparacao de propostas usando porcentagem", "Leitura critica de grafico e tabela"], ["Historia do Brasil e consequencias sociais", "Geografia economica e territorio", "Ciencia, ambiente e decisoes publicas", "Pesquisa com evidencia e posicao"], ["Causa, consequencia e tomada de posicao", "Debate com duas fontes", "Linha do tempo com interpretacao", "Estudo de caso brasileiro"], ["Gestao de estudo por prioridade", "Rotina semanal com autonomia", "Revisao de entrega por criterio", "Registro de progresso e lacuna"], ["Sprint independente de estudo", "Planejamento reverso de tarefa", "Autonomia sem lembrete constante", "Reflexao e ajuste de metodo"]),
        new(14, ["Leitura de textos complexos com anotacao", "Posicionamento com base em multiplas fontes", "Sintese escrita com estrutura forte", "Revisao autoral por impacto e precisao"], ["Argumentacao com evidencia e refutacao", "Artigo curto com voz propria", "Selecao e hierarquia de provas", "Reescrita para clareza e persuasao"], ["Orcamento, taxa e comparacao de cenarios", "Equacoes e relacoes algebricas do cotidiano", "Dados, probabilidade inicial e interpretacao", "Porcentagem aplicada a decisoes reais"], ["Comparar cenarios financeiros com justificativa", "Resolver problema com equacao e explicacao", "Ler dados e propor decisao", "Raciocinio proporcional em projeto real"], ["Brasil contemporaneo: territorio e cidadania", "Historia e memoria com analise", "Ciencia, evidencia e responsabilidade publica", "Projeto investigativo com posicao final"], ["Analisar problema publico com fontes", "Mapa, dado e argumento", "Estudo de caso com conclusao critica", "Investigacao longa com sintese final"], ["Gestao do proprio estudo semanal", "Planejamento de entregas e revisao", "Autochecagem de qualidade", "Registro reflexivo do processo"], ["Projeto autoral com cronograma", "Fechamento independente de unidade", "Ajuste de rota com autonomia", "Preparacao para estudar sem conducao continua"])
    ];

    private sealed class AgeCurriculumProfile
    {
        public AgeCurriculumProfile(
            int age,
            IReadOnlyList<string> languageCore,
            IReadOnlyList<string> languageTrack,
            IReadOnlyList<string> mathCore,
            IReadOnlyList<string> mathTrack,
            IReadOnlyList<string> worldCore,
            IReadOnlyList<string> worldTrack,
            IReadOnlyList<string> executiveCore,
            IReadOnlyList<string> executiveTrack,
            IReadOnlyList<string>? teaCommonExecutive = null,
            IReadOnlyList<string>? teaCommonCommunication = null,
            IReadOnlyList<string>? teaLevel1Executive = null,
            IReadOnlyList<string>? teaLevel1Communication = null,
            IReadOnlyList<string>? teaLevel2Executive = null,
            IReadOnlyList<string>? teaLevel2Communication = null,
            IReadOnlyList<string>? teaLevel3Executive = null,
            IReadOnlyList<string>? teaLevel3Communication = null)
        {
            Age = age;
            LanguageCore = languageCore;
            LanguageTrack = languageTrack;
            MathCore = mathCore;
            MathTrack = mathTrack;
            WorldCore = worldCore;
            WorldTrack = worldTrack;
            ExecutiveCore = executiveCore;
            ExecutiveTrack = executiveTrack;
            TeaCommonExecutive = teaCommonExecutive ?? Array.Empty<string>();
            TeaCommonCommunication = teaCommonCommunication ?? Array.Empty<string>();
            TeaLevel1Executive = teaLevel1Executive ?? Array.Empty<string>();
            TeaLevel1Communication = teaLevel1Communication ?? Array.Empty<string>();
            TeaLevel2Executive = teaLevel2Executive ?? Array.Empty<string>();
            TeaLevel2Communication = teaLevel2Communication ?? Array.Empty<string>();
            TeaLevel3Executive = teaLevel3Executive ?? Array.Empty<string>();
            TeaLevel3Communication = teaLevel3Communication ?? Array.Empty<string>();
        }

        public int Age { get; }
        public IReadOnlyList<string> LanguageCore { get; }
        public IReadOnlyList<string> LanguageTrack { get; }
        public IReadOnlyList<string> MathCore { get; }
        public IReadOnlyList<string> MathTrack { get; }
        public IReadOnlyList<string> WorldCore { get; }
        public IReadOnlyList<string> WorldTrack { get; }
        public IReadOnlyList<string> ExecutiveCore { get; }
        public IReadOnlyList<string> ExecutiveTrack { get; }
        public IReadOnlyList<string> TeaCommonExecutive { get; }
        public IReadOnlyList<string> TeaCommonCommunication { get; }
        public IReadOnlyList<string> TeaLevel1Executive { get; }
        public IReadOnlyList<string> TeaLevel1Communication { get; }
        public IReadOnlyList<string> TeaLevel2Executive { get; }
        public IReadOnlyList<string> TeaLevel2Communication { get; }
        public IReadOnlyList<string> TeaLevel3Executive { get; }
        public IReadOnlyList<string> TeaLevel3Communication { get; }
    }

    private static string BuildFunctionalTrackGuide(FunctionalSupportTrack functionalTrack) => functionalTrack switch
    {
        FunctionalSupportTrack.Communication => "Trabalhe comunicacao funcional, espera curta de resposta e reforco imediato quando a crianca consegue se expressar.",
        FunctionalSupportTrack.Regulation => "Abra com antecipacao visual, mantenha transicoes claras e feche o bloco com um sinal concreto de termino.",
        FunctionalSupportTrack.Sensory => "Prepare o ambiente, leia sinais de sobrecarga e ajuste estimulos antes de aumentar demanda academica.",
        FunctionalSupportTrack.DailyLiving => "Ensine em sequencia visivel, com inicio e fim claros, e mantenha o adulto modelando apenas o necessario.",
        FunctionalSupportTrack.AcademicAdapted => "Reduza a carga abstrata, ofereca modelo visual e aceite resposta funcional antes de exigir mais complexidade.",
        _ => string.Empty
    };

    private static IReadOnlyList<string> BuildTeaTrackTitles(int age, CurriculumSupportScope supportScope, FunctionalSupportTrack functionalTrack)
    {
        var ageBand = age <= 4 ? "3_4" : age <= 7 ? "5_7" : "8_10";

        return (supportScope, functionalTrack, ageBand) switch
        {
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Communication, "3_4") => ["Pedido funcional com gesto, imagem ou palavra", "Escolher entre duas opcoes e comunicar", "Responder ao nome e ao combinado visual"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Communication, "5_7") => ["Pedido funcional em frase curta", "Responder pergunta concreta sem fuga", "Compreender instrucao de um passo com apoio visual"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Communication, _) => ["Explicar necessidade com apoio visual", "Responder por etapas em conversa guiada", "Ampliar comunicacao funcional em contexto academico"],

            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Communication, "3_4") => ["Brincadeira guiada com troca comunicativa", "Nomear preferencia e justificar em frase curta", "Conversar em turnos curtos com apoio"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Communication, "5_7") => ["Pragmatica em contexto de estudo", "Explicar o que precisa com mais precisao", "Ampliar repertorio de resposta contextual"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Communication, _) => ["Argumentar em contexto social guiado", "Ajustar linguagem ao contexto", "Organizar fala para pedir ajuda ou negociar"],

            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Communication, "3_4") => ["Pedido funcional com quadro visual", "Responder sim, nao ou escolha com apoio", "Parear figura, gesto e necessidade"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Communication, "5_7") => ["Comunicacao funcional com roteiro visual", "Nomear necessidade e esperar retorno", "Responder com frase curta apoiada"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Communication, _) => ["Comunicacao guiada em passos curtos", "Explicar necessidade com forte apoio visual", "Usar script funcional em atividade academica"],

            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Communication, "3_4") => ["Parear necessidade e comunicacao alternativa", "Escolher com apontar ou gesto funcional", "Responder a rotina com sinal concreto"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Communication, "5_7") => ["Comunicar pedido com sistema visual concreto", "Associar simbolo, objeto e necessidade", "Responder com apoio total e fade gradual"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Communication, _) => ["Usar comunicacao alternativa para participar", "Responder com suporte maximo e previsivel", "Generalizar pedido funcional em ambiente seguro"],

            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Regulation, "3_4") => ["Aceitar transicao curta com apoio visual", "Pedir pausa e retornar", "Organizar corpo para sentar e iniciar"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Regulation, "5_7") => ["Antecipar mudanca sem perder o ritmo", "Pedir ajuda antes da escalada", "Voltar para a tarefa depois de pausa curta"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Regulation, _) => ["Regular frustracao com roteiro curto", "Usar combinados para retomar foco", "Antecipar e concluir transicoes com menos desgaste"],

            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Regulation, "3_4") => ["Tolerar pequena mudanca de combinados", "Esperar a vez com sinal claro", "Iniciar com menos rigidez"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Regulation, "5_7") => ["Flexibilizar caminho para a mesma meta", "Sustentar regra nova com aviso previo", "Planejar e ajustar sem romper a tarefa"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Regulation, _) => ["Lidar com erro sem abandonar a atividade", "Negociar pequena mudanca de plano", "Reorganizar foco apos frustracao leve"],

            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Regulation, "3_4") => ["Seguir rotina visual de dois passos", "Completar microbloco com reforco rapido", "Retomar com ajuda apos pausa sensorial"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Regulation, "5_7") => ["Manter previsibilidade em blocos curtos", "Aceitar comando unico e concreto", "Fechar uma tarefa antes da proxima"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Regulation, _) => ["Regular entrada e saida da tarefa", "Voltar ao eixo com apoio visual intenso", "Sustentar atividade em passos muito claros"],

            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Regulation, "3_4") => ["Aceitar inicio e fim muito concretos", "Permanecer com seguranca por poucos minutos", "Usar pausa previsivel para retornar"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Regulation, "5_7") => ["Organizar corpo para atividade curta", "Retornar apos apoio regulatorio estruturado", "Encerrar bloco com ritual concreto"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Regulation, _) => ["Manter seguranca e previsibilidade maxima", "Sair e voltar com ajuda total estruturada", "Concluir microbloco com sinal funcional claro"],

            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Sensory, "3_4") => ["Escolher apoio sensorial para comecar", "Notar excesso de som, luz ou toque", "Usar pausa sensorial curta e voltar"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Sensory, "5_7") => ["Preparar o corpo antes da atividade", "Regular estimulos do ambiente com ajuda", "Voltar ao foco depois de ajuste sensorial"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.Sensory, _) => ["Ler sinais de sobrecarga e agir cedo", "Organizar ambiente de estudo com menos ruido", "Recuperar foco apos apoio sensorial funcional"],

            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Sensory, "3_4") => ["Nomear desconforto sensorial simples", "Escolher estrategia de autorregulacao", "Ajustar ambiente antes da escalada"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Sensory, "5_7") => ["Pedir ajuste de som, luz ou toque", "Usar kit sensorial e voltar para a meta", "Reconhecer gatilho sensorial do dia"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.Sensory, _) => ["Antecipar sobrecarga e ajustar rotina", "Selecionar apoio sensorial funcional", "Retomar a tarefa com menos atrito sensorial"],

            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Sensory, "3_4") => ["Usar pista visual para pausa sensorial", "Associar corpo agitado a ajuda concreta", "Voltar com suporte depois do alivio sensorial"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Sensory, "5_7") => ["Organizar entrada sensorial antes do estudo", "Fazer pausa sensorial com roteiro visual", "Ajustar o ambiente com mediacao forte"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.Sensory, _) => ["Manter regulacao sensorial em rotina guiada", "Usar suporte concreto para reduzir sobrecarga", "Retornar ao academico depois do reequilibrio"],

            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Sensory, "3_4") => ["Parear conforto sensorial e inicio da rotina", "Tolerar microexposicao com seguranca", "Receber apoio sensorial antes da demanda"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Sensory, "5_7") => ["Organizar corpo com entrada sensorial segura", "Aceitar ambiente adaptado e previsivel", "Retornar so depois da estabilizacao sensorial"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.Sensory, _) => ["Seguranca sensorial como pre-requisito academico", "Usar regulacao concreta antes de qualquer demanda", "Manter previsibilidade maxima do ambiente"],

            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.DailyLiving, "3_4") => ["Guardar material no lugar certo", "Lavar maos em sequencia curta", "Buscar e devolver item da rotina"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.DailyLiving, "5_7") => ["Organizar mochila ou bandeja da atividade", "Seguir sequencia visual de autocuidado", "Preparar e encerrar o bloco com autonomia"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.DailyLiving, _) => ["Executar rotina funcional em ordem", "Cuidar do proprio material de estudo", "Fechar atividade e ambiente com menos ajuda"],

            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.DailyLiving, "3_4") => ["Fazer transicao de rotina com mais autonomia", "Organizar material sem perder o foco", "Cumprir sequencia curta de cuidado pessoal"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.DailyLiving, "5_7") => ["Planejar inicio, meio e fim de uma rotina", "Lidar com pequeno imprevisto no autocuidado", "Assumir mais passos da organizacao diaria"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.DailyLiving, _) => ["Autogerir rotina funcional com pouca ajuda", "Ajustar o plano do cotidiano com flexibilidade", "Manter organizacao diaria com mais independencia"],

            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.DailyLiving, "3_4") => ["Vestir, guardar ou limpar com roteiro visual", "Seguir dois ou tres passos de vida diaria", "Concluir pequena rotina com reforco rapido"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.DailyLiving, "5_7") => ["Executar autocuidado com passos menores", "Organizar o bloco com modelagem visual", "Generalizar sequencia funcional em mais contextos"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.DailyLiving, _) => ["Vida diaria com apoio visual intenso", "Completar rotina funcional com mediacao forte", "Aumentar autonomia em microetapas repetidas"],

            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.DailyLiving, "3_4") => ["Aceitar ajuda total em sequencia funcional", "Participar do autocuidado com gesto ou escolha", "Concluir microetapa concreta da rotina"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.DailyLiving, "5_7") => ["Vida diaria com ajuda total e fade gradual", "Responder a rotina funcional muito concreta", "Engajar em sequencia curta de cuidado e organizacao"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.DailyLiving, _) => ["Participar de rotina funcional altamente estruturada", "Manter previsibilidade maxima no autocuidado", "Ganhar pequena autonomia em gesto funcional essencial"],

            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.AcademicAdapted, "3_4") => ["Parear figura, objeto e resposta funcional", "Completar tarefa curta com modelo visual", "Entrar no academico por um unico passo"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.AcademicAdapted, "5_7") => ["Responder instrucao academica com apoio visual", "Parear numero e quantidade com suporte concreto", "Completar bloco academico curto e previsivel"],
            (CurriculumSupportScope.TeaCommon, FunctionalSupportTrack.AcademicAdapted, _) => ["Resolver tarefa com modelo e checagem curta", "Responder academicamente sem sobrecarga de linguagem", "Generalizar habilidade academica em contexto seguro"],

            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.AcademicAdapted, "3_4") => ["Aplicar habilidade em novo contexto com ajuda", "Explicar o proprio raciocinio em frase curta", "Manter flexibilidade diante de pequena variacao academica"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.AcademicAdapted, "5_7") => ["Transferir a estrategia para outro material", "Resolver com script curto de pensamento", "Ampliar autonomia na mesma meta academica"],
            (CurriculumSupportScope.TeaLevel1, FunctionalSupportTrack.AcademicAdapted, _) => ["Justificar resposta com organizacao visual", "Generalizar conteudo academico com menos rigidez", "Manter desempenho mesmo com pequena mudanca de formato"],

            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.AcademicAdapted, "3_4") => ["Responder usando apontar, parear ou marcar", "Resolver com forte modelo visual", "Concluir tarefa academica em microetapas"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.AcademicAdapted, "5_7") => ["Executar conteudo em passos muito visiveis", "Usar apoio concreto antes da abstracao", "Finalizar bloco academico com reforco previsivel"],
            (CurriculumSupportScope.TeaLevel2, FunctionalSupportTrack.AcademicAdapted, _) => ["Resolver atividade com apoio visual intenso", "Generalizar apenas depois da consolidacao guiada", "Responder academicamente com menos linguagem oral"],

            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.AcademicAdapted, "3_4") => ["Parear simbolo e funcao com ajuda total", "Responder a um unico pedido academico concreto", "Concluir bloco altamente estruturado com fim observavel"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.AcademicAdapted, "5_7") => ["Associar objeto, simbolo e resposta funcional", "Responder academicamente com estrutura maxima", "Manter microtarefa concreta ate o encerramento"],
            (CurriculumSupportScope.TeaLevel3, FunctionalSupportTrack.AcademicAdapted, _) => ["Entrar no academico por resposta concreta e previsivel", "Generalizar apenas em ambiente muito seguro", "Concluir atividade funcional antes de expandir complexidade"],

            _ => ["Missao adaptada de apoio funcional", "Rotina guiada com objetivo claro", "Concluir com vitoria observavel"]
        };
    }

    private static string BuildSupportGuide(CurriculumSupportScope supportScope) => supportScope switch
    {
        CurriculumSupportScope.TeaCommon => "Use previsibilidade, apoio visual simples, linguagem concreta e encerre com uma vitoria observavel.",
        CurriculumSupportScope.TeaLevel1 => "Mantenha a explicacao objetiva, trabalhe flexibilidade com combinados claros e peça verbalizacao curta do que vai acontecer.",
        CurriculumSupportScope.TeaLevel2 => "Quebre a tarefa em passos menores, aumente os suportes visuais e valide cada microconquista antes de seguir.",
        CurriculumSupportScope.TeaLevel3 => "Priorize seguranca, comunicacao funcional, apoio visual intenso e atividades curtas com inicio e fim muito concretos.",
        _ => string.Empty
    };
}
