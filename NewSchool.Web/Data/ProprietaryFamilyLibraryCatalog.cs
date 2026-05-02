using NewSchool.Web.Domain;

namespace NewSchool.Web.Data;

public static class ProprietaryFamilyLibraryCatalog
{
    public const string CollectionLabel = "Coleção NewSchool";
    private const string SyncPrefix = "AUTHOR";

    public static IReadOnlyList<ProprietaryFamilyLibraryMaterialSeed> Build()
    {
        return
        [
            Book(
                "5E2D8D95-7E17-4D49-9F71-516D7B553C01",
                "as-cores-da-criacao",
                "As Cores da Criação",
                "Histórias Bíblicas",
                "criação, cores, observação, gratidão, fala guiada",
                "Livro curto para educação infantil que liga criação, cores do dia e fala guiada com começo, meio e fim.",
                3,
                4,
                "Educação Infantil",
                [
                    "Página 1\nNo começo da semana, a mamãe de Ana levou uma cesta com lápis de cor para o quintal.\nEla disse: \"Hoje vamos olhar o mundo que Deus fez e escolher uma cor de cada vez.\"",
                    "Página 2\nAna viu o céu azul, a folha verde e a flor amarela.\nCada vez que encontrava uma cor, ela apontava e dizia: \"Eu achei!\"",
                    "Página 3\nDepois, Ana separou três objetos: uma folha, uma pedrinha e uma flor.\nA mamãe perguntou: \"Qual cor você quer contar primeiro?\"",
                    "Página 4\nAna respondeu com calma: \"Primeiro o verde. Depois o amarelo. No final, o azul.\"\nEla percebeu que conseguia organizar o que via em ordem.",
                    "Página 5\nAntes de guardar os lápis, Ana completou a frase: \"Hoje eu agradeço a Deus pelo céu azul e pela folha verde.\"\nA mamãe sorriu e disse: \"Essa é a nossa memória da leitura de hoje.\""
                ]),

            Book(
                "E8483A62-BD89-4B0B-8D0A-7D9E0D6EBF31",
                "lila-e-as-letras-do-quintal",
                "Lila e as Letras do Quintal",
                "Literatura",
                "som inicial, letra, palavra, quintal, pré-alfabetização",
                "Livro autoral para 3 a 4 anos que apresenta letra, som inicial e nomeação de objetos reais da casa.",
                3,
                4,
                "Educação Infantil",
                [
                    "Página 1\nLila saiu para o quintal com uma caixa pequena.\nDentro dela havia cartões com letras grandes.",
                    "Página 2\nA primeira letra era L.\nLila falou devagar: \"L faz lllll.\"",
                    "Página 3\nEla olhou ao redor e encontrou limão, lápis e livro.\nA cada objeto, repetia o som antes de falar a palavra inteira.",
                    "Página 4\nDepois, a vovó mostrou a letra M.\nLila encontrou mão, mesa e mel na cozinha.",
                    "Página 5\nNo final, Lila escolheu sua letra favorita e desenhou um objeto que começava com aquele som.\nEla percebeu que os sons ajudam a abrir as palavras."
                ]),

            Book(
                "2B1BF7C1-8B1E-4204-923D-C1E94F0D43F9",
                "a-casa-das-silabas",
                "A Casa das Sílabas",
                "Literatura",
                "sílabas, palmas, palavras conhecidas, leitura inicial",
                "Livro autoral para 5 a 6 anos com palavras familiares, palmas e pedaços sonoros que cabem na rotina diária.",
                5,
                6,
                "Educação Infantil",
                [
                    "Página 1\nNa rua de Beto havia uma casa engraçada.\nCada porta só abria quando uma palavra era falada em pedaços.",
                    "Página 2\nBeto chegou à porta do sapato.\nEle bateu palmas: sa-pa-to.\nA porta abriu devagar.",
                    "Página 3\nDepois veio a porta da panela.\nBeto falou: pa-ne-la.\nA irmã dele contou os pedaços com bolinhas no papel.",
                    "Página 4\nNa última porta havia a palavra boneca.\nBeto respirou, falou bo-ne-ca e abriu mais uma vez.",
                    "Página 5\nNo fim, Beto percebeu que palavras grandes podem ser ouvidas em partes.\nQuando ele escutava os pedaços, ficava mais fácil ler e escrever."
                ]),

            Book(
                "0F1C532C-3C9A-4B59-A48A-0EDC86839D53",
                "pedro-conta-a-feira",
                "Pedro Conta a Feira",
                "Literatura",
                "contagem, quantidade, grupos, feira, matemática concreta",
                "Livro autoral para 5 a 6 anos que trabalha contagem, agrupamento e registro simples com objetos reais.",
                5,
                6,
                "Educação Infantil",
                [
                    "Página 1\nPedro foi à feira com o pai para comprar frutas.\nO combinado era contar antes de colocar qualquer coisa na sacola.",
                    "Página 2\nNa banca da banana, Pedro contou 2, depois mais 2.\nO pai perguntou: \"Quanto deu no total?\"",
                    "Página 3\nPedro organizou as bananas em dois grupos iguais.\nAssim ficou mais fácil perceber a quantidade.",
                    "Página 4\nDepois ele desenhou as frutas no caderno da feira.\nAo lado, escreveu o número que combinava com cada grupo.",
                    "Página 5\nNo caminho de volta, Pedro disse: \"Quando eu monto os grupos, o número para de parecer só um desenho.\""
                ]),

            Book(
                "3C2C8D6B-19E5-4FE5-BD71-82A40A76AC32",
                "o-bairro-cabe-num-mapa",
                "O Bairro Cabe Num Mapa",
                "Literatura",
                "mapa, bairro, referência, geografia, mundo real",
                "Livro autoral para 7 a 8 anos que apresenta mapa, pontos de referência e orientação espacial com linguagem simples.",
                7,
                8,
                "Ensino Fundamental",
                [
                    "Página 1\nClara queria mostrar para a avó como chegar até a padaria da esquina.\nO pai disse: \"Hoje você vai descobrir que um bairro cabe num mapa.\"",
                    "Página 2\nClara desenhou a sua casa no canto da folha.\nDepois marcou a praça, a igreja e a padaria como pontos de referência.",
                    "Página 3\nEla percebeu que não bastava dizer os nomes.\nPrecisava mostrar o caminho em ordem: sair, virar, seguir e chegar.",
                    "Página 4\nAo comparar o desenho com a caminhada real, Clara ajustou as ruas e o lugar da praça.\nO mapa ficou mais claro.",
                    "Página 5\nNo final, a avó conseguiu ler o mapa e repetir o caminho.\nClara entendeu que um mapa ajuda a organizar o mundo em volta."
                ]),

            Book(
                "9DA85A03-089C-4C50-A806-77D4A6A4C8C8",
                "samuel-organiza-a-missao",
                "Samuel Organiza a Missão",
                "Literatura",
                "autonomia, checklist, começar, concluir, rotina de estudo",
                "Livro autoral para 7 a 8 anos que ensina a abrir, fazer e fechar uma missão de estudo sem travar.",
                7,
                8,
                "Ensino Fundamental",
                [
                    "Página 1\nSamuel sabia fazer a lição, mas perdia muito tempo antes de começar.\nA mãe então escreveu três passos num cartão.",
                    "Página 2\nO cartão dizia: separar, fazer, revisar.\nSamuel leu em voz alta e colocou o material certo na mesa.",
                    "Página 3\nQuando terminou a primeira parte, ele teve vontade de levantar.\nA mãe apontou para o segundo passo e perguntou: \"O que ainda falta nesta missão?\"",
                    "Página 4\nSamuel voltou ao caderno, terminou e revisou só o que era necessário.\nNo final, marcou um X ao lado dos três passos.",
                    "Página 5\nEle percebeu que não precisava adivinhar o que vinha depois.\nQuando a missão tinha começo, meio e fim, estudar ficava mais leve."
                ]),

            Book(
                "7B6ED882-749C-4A34-B340-0F2EBBE8096B",
                "rita-aprende-a-defender-sua-ideia",
                "Rita Aprende a Defender sua Ideia",
                "Literatura",
                "opinião, prova do texto, ideia central, detalhes",
                "Livro autoral para 9 a 10 anos que ensina a responder com opinião clara e duas provas, sem virar redação longa.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nNa roda de leitura, Rita disse que o personagem principal foi corajoso.\nA professora perguntou: \"Como você prova isso com o texto?\"",
                    "Página 2\nRita voltou ao trecho e encontrou duas pistas.\nPrimeiro, o personagem continuou mesmo com medo.\nDepois, ele ajudou outra pessoa sem esperar recompensa.",
                    "Página 3\nEla entendeu que opinião forte não nasce do nada.\nPrecisa vir junto de detalhes que sustentam a resposta.",
                    "Página 4\nNo caderno, Rita escreveu uma frase principal e dois detalhes de apoio.\nO texto ficou curto, mas muito mais claro.",
                    "Página 5\nQuando leu em voz alta, ela percebeu que sua resposta estava organizada.\nAgora sua ideia tinha base."
                ]),

            Book(
                "2D1BAA50-0E20-4B0D-9E08-7E56D59371CF",
                "viagem-pelos-rios-do-brasil",
                "Viagem pelos Rios do Brasil",
                "Literatura",
                "rios do brasil, mapa, impacto, geografia, sociedade",
                "Livro autoral para 9 a 10 anos que conecta rios, mapa, pessoas e impacto real na vida do país.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nCaio abriu o mapa do Brasil e percebeu que os rios pareciam caminhos.\nA professora disse: \"Hoje vamos entender por que esses caminhos importam tanto.\"",
                    "Página 2\nEle viu que muitos rios ajudam cidades, plantações e barcos.\nAlguns também levam água para lugares muito diferentes entre si.",
                    "Página 3\nAo comparar duas regiões, Caio notou que o mesmo rio pode servir para transporte, pesca e vida das pessoas.\nO mapa começou a fazer sentido.",
                    "Página 4\nDepois ele registrou uma pergunta: \"O que muda quando um rio é poluído ou desviado?\"\nA conversa saiu do desenho e entrou na vida real.",
                    "Página 5\nNo final, Caio concluiu: \"Rios não são só linhas no mapa. Eles sustentam escolhas, trabalho e comunidades inteiras.\""
                ]),

            Book(
                "6E0F91D4-10CB-4EB8-BFD1-22333D46061A",
                "jornal-da-turma-e-a-tese-principal",
                "Jornal da Turma e a Tese Principal",
                "Literatura",
                "tese, argumento, prova textual, leitura critica",
                "Livro autoral para 11 a 12 anos que mostra como transformar leitura em tese e prova, sem resposta vaga.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nA turma de Lara queria montar um jornal da escola.\nMas a professora avisou: \"Nao basta ter opiniao. Cada texto precisa de uma tese e de provas fortes.\"",
                    "Pagina 2\nLara escolheu escrever sobre o patio muito quente no recreio.\nPrimeiro ela anotou a tese: a escola precisa de mais sombra nos espacos de convivencia.",
                    "Pagina 3\nDepois ela procurou duas provas.\nA primeira veio da observacao do calor e do pouco uso do patio em certos horarios.\nA segunda veio de um trecho sobre outra escola que plantou arvores e viu o espaco ganhar vida.",
                    "Pagina 4\nQuando releu o texto, Lara percebeu que prova nao e enfeite.\nE o que segura a tese de pe quando alguem pergunta: \"Como voce sabe disso?\"",
                    "Pagina 5\nNo fechamento, ela escreveu: \"Minha ideia ficou mais forte porque eu mostrei onde ela se apoia.\"\nO jornal da turma virou tambem uma aula de pensamento claro."
                ]),

            Book(
                "7A844D6B-8038-4204-A9DA-3A2A5FA7909A",
                "marina-compara-as-receitas",
                "Marina Compara as Receitas",
                "Literatura",
                "razao, tabela, proporcao, justificativa matematica",
                "Livro autoral para 11 a 12 anos que ensina a organizar dados em tabela antes de calcular e justificar.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nMarina precisava dobrar uma receita para a reuniao da familia.\nA avo disse: \"Nao comece na conta. Primeiro organize os dados.\"",
                    "Pagina 2\nEla montou uma tabela com colunas para leite, farinha e ovos.\nNa primeira linha entrou a receita original.\nNa segunda, a versao maior.",
                    "Pagina 3\nQuando viu tudo na tabela, Marina percebeu que a proporcao ficava mais clara.\nDobrar um item sem dobrar os outros mudaria o resultado.",
                    "Pagina 4\nNo caderno, ela escreveu a justificativa: \"A receita continua equivalente porque aumentei as duas partes na mesma relacao.\"",
                    "Pagina 5\nMarina descobriu que a tabela nao serve so para organizar.\nEla ajuda a explicar por que a resposta final faz sentido."
                ]),

            Book(
                "3EF65D4C-71E1-4F6E-9BE6-33CA86D86F26",
                "duas-fontes-uma-pergunta",
                "Duas Fontes, Uma Pergunta",
                "Literatura",
                "fontes, comparacao, conclusao, brasil, agua",
                "Livro autoral para 11 a 12 anos que mostra como comparar duas fontes e fechar uma conclusao sem copiar tudo.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nNo estudo da semana, Joana leu uma noticia sobre reservatorios e um texto informativo sobre desperdicio de agua.\nEla achou que as duas leituras pareciam diferentes demais.",
                    "Pagina 2\nO pai explicou: \"As duas estao respondendo a mesma pergunta, so que por caminhos diferentes.\"",
                    "Pagina 3\nJoana anotou um dado da noticia e uma explicacao do texto informativo.\nEntao percebeu que uma fonte mostrava o fato e a outra ajudava a entender a causa.",
                    "Pagina 4\nNo final, ela escreveu: \"O problema da agua nas cidades nao depende de uma coisa so. Estrutura e uso das pessoas entram juntos.\"",
                    "Pagina 5\nComparar fontes fez Joana pensar melhor.\nEm vez de repetir duas leituras separadas, ela conseguiu responder a pergunta central."
                ]),

            Book(
                "E3F4A307-9B75-4BCF-A0E7-6A69B532B189",
                "elisa-escreve-para-convencer",
                "Elisa Escreve para Convencer",
                "Literatura",
                "tese, contra-argumento, refutacao, artigo de opiniao",
                "Livro autoral para 13 a 14 anos que ensina a sustentar tese, considerar objecao e fechar texto com maturidade.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nElisa queria escrever sobre uso de celular no estudo.\nNo primeiro rascunho, ela apenas disse que o aparelho atrapalha.",
                    "Pagina 2\nA professora devolveu a folha com uma pergunta: \"E o que voce faz com o argumento do outro lado?\"",
                    "Pagina 3\nElisa voltou ao texto e reconheceu que o celular tambem pode servir para pesquisa e organizacao.\nMas explicou que, sem regras claras, o custo em foco costuma ser alto.",
                    "Pagina 4\nEla entendeu que texto maduro nao ignora a objecao.\nEle encara a objecao e mostra por que a tese continua forte.",
                    "Pagina 5\nNo final, Elisa releu o artigo e percebeu que sua voz ficou mais convincente.\nAgora havia tese, prova, contra-argumento e resposta."
                ]),

            Book(
                "65A7E12C-A31B-44B6-95F8-50CA92CB3350",
                "cem-reais-muitos-caminhos",
                "Cem Reais, Muitos Caminhos",
                "Literatura",
                "orcamento, porcentagem, comparacao de cenarios, decisao",
                "Livro autoral para 13 a 14 anos que leva a crianca a comparar cenarios financeiros e justificar a melhor escolha.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nDavi recebeu a missao de organizar a compra de materiais para um projeto com limite de cem reais.\nParecia simples, mas havia mais de uma possibilidade.",
                    "Pagina 2\nUma opcao custava menos no inicio, mas exigia reposicao logo.\nA outra parecia cara, porem durava mais e evitava nova compra na semana seguinte.",
                    "Pagina 3\nDavi montou uma tabela com custo inicial, gasto posterior e total previsto.\nFoi assim que a matematica saiu do papel e entrou em uma decisao real.",
                    "Pagina 4\nAo comparar os valores, ele percebeu que barato e caro dependem do periodo analisado.\nO melhor cenario nem sempre e o menor numero da primeira linha.",
                    "Pagina 5\nNo fechamento, Davi escreveu: \"Escolher bem tambem e saber comparar cenarios, nao apenas somar valores isolados.\""
                ]),

            Book(
                "E721D813-7A1D-4B37-8E1A-930A5C25FDFB",
                "quando-o-rio-mudou-a-cidade",
                "Quando o Rio Mudou a Cidade",
                "Literatura",
                "causa, consequencia, territorio, decisao publica",
                "Livro autoral para 13 a 14 anos que conecta problema publico, causa, impacto e tomada de posicao.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nNo estudo do mes, Sofia leu sobre uma cidade afetada por enchentes repetidas.\nA pergunta nao era so o que aconteceu, mas por que aquilo continuava acontecendo.",
                    "Pagina 2\nEla descobriu que chuva forte era apenas uma parte do quadro.\nFaltavam drenagem, planejamento urbano e cuidado com o territorio.",
                    "Pagina 3\nAo montar um quadro de causas e consequencias, Sofia percebeu que o problema tinha camadas.\nTambem viu que as respostas publicas precisam olhar para todas elas.",
                    "Pagina 4\nNo final da conversa, ela tomou uma posicao: prevenir custa trabalho, mas reparar depois custa mais para a cidade e para as familias.",
                    "Pagina 5\nSofia concluiu que estudar Brasil e mundo nao e decorar fatos.\nE aprender a enxergar impacto, responsabilidade e escolha."
                ]),

            Book(
                "B7D77E88-73AF-4B27-BC9A-614B6C567015",
                "projeto-prazo-e-entrega",
                "Projeto, Prazo e Entrega",
                "Literatura",
                "sprint, autonomia, prazo, revisao, entrega",
                "Livro autoral para 13 a 14 anos que ensina a estudar em sprint, revisar e ajustar o proprio metodo.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nRafa queria terminar o trabalho da semana, mas vivia trocando de tarefa no meio.\nO pai propos um novo jeito: estudar em sprint.",
                    "Pagina 2\nCada sprint tinha uma meta clara, um tempo curto e uma prova final.\nNada de abrir tres frentes ao mesmo tempo.",
                    "Pagina 3\nNo primeiro bloco, Rafa concluiu a parte principal, mas percebeu que o texto saiu apressado.\nNa revisao, anotou o ajuste para o proximo sprint: menos pressa no fechamento.",
                    "Pagina 4\nAo repetir o ciclo, ele viu que estudar nao era apenas produzir.\nTambem era revisar o processo e ajustar o metodo.",
                    "Pagina 5\nNo fim da semana, Rafa percebeu que autonomia nao significa estudar sozinho sem pensar.\nSignifica planejar, executar, revisar e melhorar."
                ]),

            Printable(
                "86BEF9DF-E047-4B42-9A58-16C9DB9227DB",
                "meu-nome-e-minha-letra-inicial",
                "Meu Nome e Minha Letra Inicial",
                "Atividade de linguagem",
                "nome, letra inicial, traçado, som inicial",
                "Folha autoral para 3 a 4 anos com nome próprio, letra inicial e objetos da casa.",
                3,
                4,
                "Educação Infantil",
                [
                    "Página 1\nATIVIDADE: MEU NOME E MINHA LETRA INICIAL\n\n1. Diga o nome da criança devagar.\n2. Circule a primeira letra.\n3. Repita o som inicial três vezes.\n4. Desenhe ou cole dois objetos que começam com o mesmo som.\n\nQuadro de registro:\nMeu nome é: __________\nMinha primeira letra é: __________\nPalavra 1: __________\nPalavra 2: __________",
                    "Página 2\nTRILHA DE TRAÇADO GRANDE\n\nPasse o dedo no ar.\nDepois trace a letra grande três vezes.\nNo final, pinte a letra favorita da página.\n\nLetra 1: __________\nLetra 2: __________\nLetra 3: __________"
                ]),

            Printable(
                "C8EAC8AF-7965-46B3-B3BB-59A9B9D8A4C8",
                "contar-ligar-e-colorir-ate-5",
                "Contar, Ligar e Colorir Até 5",
                "Atividade de matemática",
                "contagem, quantidade, número, ligar, colorir",
                "Folha autoral para 3 a 4 anos com correspondência um a um e grupos pequenos.",
                3,
                4,
                "Educação Infantil",
                [
                    "Página 1\nATIVIDADE: CONTAR ATÉ 5\n\nConte os grupos abaixo tocando um por um.\nDepois ligue cada grupo ao número certo.\n\nGrupo A: ○ ○\nGrupo B: ○ ○ ○\nGrupo C: ○ ○ ○ ○\nGrupo D: ○ ○ ○ ○ ○\n\nNúmeros para ligar: 2  3  4  5",
                    "Página 2\nCOLORIR E COMPARAR\n\nPinte o grupo com MAIS objetos.\nCircule o grupo com MENOS objetos.\nNo final, diga em voz alta:\n\"Este grupo tem ___ e este grupo tem ___.\""
                ]),

            Printable(
                "DFA11747-57A5-4D5D-BB89-44E8E173D75A",
                "palmas-das-silabas-da-semana",
                "Palmas das Sílabas da Semana",
                "Atividade de linguagem",
                "sílabas, palmas, pedaços, palavras conhecidas",
                "Folha autoral para 5 a 6 anos que transforma sílabas em palmas, bolinhas e registro rápido.",
                5,
                6,
                "Educação Infantil",
                [
                    "Página 1\nATIVIDADE: PALMAS DAS SÍLABAS\n\nFale cada palavra devagar.\nBata palmas para cada pedaço.\nDepois registre com bolinhas.\n\nSAPATO  ________\nPANELA  ________\nBONECA  ________\nTOMATE  ________\n\nNo final, responda:\nQual palavra teve menos pedaços?\nQual palavra teve mais pedaços?",
                    "Página 2\nORDEM DOS PEDAÇOS\n\nEscolha duas palavras da página anterior.\nEscreva ou dite os pedaços na ordem certa.\n\nPalavra 1: __________________\nPedaços: __________________\n\nPalavra 2: __________________\nPedaços: __________________"
                ]),

            Printable(
                "F8137B37-03D5-498A-A4BD-AB1C64679018",
                "dez-objetos-dois-grupos",
                "Dez Objetos, Dois Grupos",
                "Atividade de matemática",
                "dezena, grupos, soma, decomposição",
                "Folha autoral para 5 a 6 anos com decomposição de quantidade e desenho de grupos.",
                5,
                6,
                "Educação Infantil",
                [
                    "Página 1\nATIVIDADE: DEZ OBJETOS, DOIS GRUPOS\n\nMonte 10 objetos reais.\nDepois desenhe dois grupos que também formem 10.\n\nGrupo 1: __________\nGrupo 2: __________\nTotal: __________\n\nTente duas combinações diferentes.\nCombinação A: ____ + ____ = 10\nCombinação B: ____ + ____ = 10",
                    "Página 2\nEXPLIQUE SUA ESCOLHA\n\nComplete com ajuda leve:\n\"Eu formei o número 10 com ____ e ____ porque...\"\n\nEspaço para desenho dos grupos:\n________________________________________"
                ]),

            Printable(
                "AAB90DCA-C43F-436B-B970-EB7A45D6D21E",
                "personagem-problema-e-solucao",
                "Personagem, Problema e Solução",
                "Atividade de linguagem",
                "personagem, problema, solução, leitura, reconto",
                "Folha autoral para 7 a 8 anos com reconto organizado e resposta curta.",
                7,
                8,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: PERSONAGEM, PROBLEMA E SOLUÇÃO\n\nDepois da leitura, preencha só o essencial.\n\nQuem é o personagem principal?\n________________________________\n\nQual problema apareceu?\n________________________________\n\nComo esse problema começou?\n________________________________",
                    "Página 2\nCOMO A HISTÓRIA SE RESOLVEU?\n\nEscreva em 3 a 5 linhas a solução do texto.\n________________________________\n________________________________\n________________________________\n\nNo final, circule a resposta:\nA solução foi rápida / pensada / corajosa / inesperada"
                ]),

            Printable(
                "81C95D30-6D47-448C-926F-2EE5E380F0BF",
                "mapa-do-meu-bairro",
                "Mapa do Meu Bairro",
                "Atividade de mundo",
                "mapa, bairro, referência, geografia",
                "Folha autoral para 7 a 8 anos com casa, rua, praça e caminho em ordem.",
                7,
                8,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: MAPA DO MEU BAIRRO\n\nDesenhe estes pontos no mapa:\n1. Minha casa\n2. Um lugar importante da família\n3. Uma praça, igreja, mercado ou escola\n\nDepois ligue os pontos com setas mostrando o caminho.",
                    "Página 2\nEXPLICANDO O CAMINHO\n\nComplete com clareza:\nPrimeiro eu saio de __________________.\nDepois passo por __________________.\nNo final eu chego em __________________.\n\nO ponto de referência mais importante é __________________ porque __________________."
                ]),

            Printable(
                "428794FC-D802-4FB8-A46C-EFF95541D37D",
                "ideia-central-e-dois-detalhes",
                "Ideia Central e Dois Detalhes",
                "Atividade de linguagem",
                "ideia central, detalhes, prova do texto, síntese",
                "Folha autoral para 9 a 10 anos que organiza ideia principal e dois detalhes de apoio sem rodeio.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: IDEIA CENTRAL E DOIS DETALHES\n\nDepois da leitura, responda sem copiar o texto inteiro.\n\nIdeia central do trecho:\n________________________________\n\nDetalhe 1 que ajuda a provar isso:\n________________________________\n\nDetalhe 2 que ajuda a provar isso:\n________________________________",
                    "Página 2\nFECHAMENTO CURTO\n\nEscreva um resumo oral ou escrito em até 4 linhas.\n________________________________\n________________________________\n________________________________\n________________________________\n\nChecklist:\n[ ] Minha resposta fala do essencial.\n[ ] Usei dois detalhes do texto.\n[ ] Não me perdi em informação secundária."
                ]),

            Printable(
                "EEB06B34-8FD6-498E-908D-6866870EF386",
                "perimetro-da-sala-e-justificativa",
                "Perímetro da Sala e Justificativa",
                "Atividade de matemática",
                "perímetro, medida, sala, justificativa, estratégia",
                "Folha autoral para 9 a 10 anos com caminhada de perímetro, registro e justificativa da estratégia usada.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: PERÍMETRO DA SALA\n\nMeça a volta de um espaço da casa ou da sala.\nUse passos, barbante ou régua.\n\nLado 1: ______\nLado 2: ______\nLado 3: ______\nLado 4: ______\n\nPerímetro total: ______",
                    "Página 2\nJUSTIFIQUE SUA ESTRATÉGIA\n\nExplique em 4 linhas como você mediu e por que sua estratégia faz sentido.\n________________________________\n________________________________\n________________________________\n________________________________\n\nNo final, responda:\nO resultado ficaria diferente com outro instrumento? Por quê?"
                ]),

            Printable(
                "BEE05F31-DA43-4EC2-B62A-614238813B7E",
                "rios-do-brasil-e-impacto",
                "Rios do Brasil e Impacto",
                "Atividade de mundo",
                "rios do brasil, mapa, impacto, sociedade",
                "Folha autoral para 9 a 10 anos com comparação entre rio, uso e impacto para pessoas e regiões.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: RIOS DO BRASIL E IMPACTO\n\nEscolha um rio estudado hoje.\n\nNome do rio: __________________\nRegião do Brasil: __________________\nO que esse rio ajuda a mover ou sustentar?\n________________________________\n________________________________",
                    "Página 2\nSE O RIO MUDA, O QUE MUDA TAMBÉM?\n\nEscreva um efeito possível para a cidade, para o trabalho ou para as pessoas.\n________________________________\n________________________________\n________________________________\n\nFeche com uma frase:\n\"Esse rio importa porque __________________.\""
                ]),

            Printable(
                "387EC978-47E0-45F5-B70F-8C5A5A253B8A",
                "tese-e-duas-provas",
                "Tese e Duas Provas",
                "Atividade de linguagem",
                "tese, prova textual, argumento, leitura",
                "Folha autoral para 11 a 12 anos com estrutura curta de tese, prova 1, prova 2 e conclusao.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: TESE E DUAS PROVAS\n\nTema do texto:\n______________________________\n\nMinha tese em uma frase:\n______________________________\n______________________________\n\nProva 1 do texto:\n______________________________\n______________________________",
                    "Pagina 2\nPROVA 2 E FECHAMENTO\n\nProva 2 do texto:\n______________________________\n______________________________\n\nConclusao em palavras proprias:\n______________________________\n______________________________\n\nChecklist:\n[ ] Minha tese esta clara.\n[ ] Usei duas provas reais.\n[ ] Fechei sem copiar o texto inteiro."
                ]),

            Printable(
                "4A50A455-03A8-44B8-B559-E851CF91A8DF",
                "razao-tabela-e-justificativa",
                "Razao, Tabela e Justificativa",
                "Atividade de matematica",
                "razao, tabela, proporcao, justificativa",
                "Folha autoral para 11 a 12 anos com organizacao de dados, comparacao e justificativa da estrategia.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: RAZAO E TABELA\n\nSituacao:\n________________________________\n\nTabela de dados:\nItem 1: ______\nItem 2: ______\nItem 3: ______\n\nRazao ou proporcao encontrada:\n______________________________",
                    "Pagina 2\nJUSTIFICATIVA\n\nExplique por que sua tabela ajuda a entender a resposta:\n______________________________\n______________________________\n______________________________\n\nConclusao final:\n______________________________"
                ]),

            Printable(
                "EE6CC06C-E174-4C2D-AFA6-9BA08BFC7D39",
                "duas-fontes-e-uma-conclusao",
                "Duas Fontes e Uma Conclusao",
                "Atividade de mundo",
                "fontes, comparacao, dado, conclusao",
                "Folha autoral para 11 a 12 anos que guia registro de um dado por fonte e uma conclusao final.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: DUAS FONTES\n\nTema comum:\n______________________________\n\nFonte 1: dado principal\n______________________________\n______________________________\n\nFonte 2: explicacao principal\n______________________________\n______________________________",
                    "Pagina 2\nCOMPARACAO E CONCLUSAO\n\nO que se repete nas duas fontes?\n______________________________\n\nO que muda de uma para a outra?\n______________________________\n\nMinha conclusao final:\n______________________________"
                ]),

            Printable(
                "35C2E9A8-69C0-44CD-9A63-A1932AB165C6",
                "tese-contra-argumento-e-fecho",
                "Tese, Contra-argumento e Fecho",
                "Atividade de linguagem",
                "tese, contra-argumento, fecho, refutacao",
                "Folha autoral para 13 a 14 anos com estrutura curta para tese, objecao, resposta e conclusao final.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: TESE E OBJECao\n\nMinha tese:\n______________________________\n______________________________\n\nProva principal:\n______________________________\n______________________________\n\nContra-argumento que precisa ser considerado:\n______________________________",
                    "Pagina 2\nRESPOSTA E FECHO\n\nComo respondo a objecao:\n______________________________\n______________________________\n\nConclusao final:\n______________________________\n______________________________\n\nChecklist:\n[ ] Minha tese esta clara.\n[ ] Reconheci uma objecao real.\n[ ] Fechei sem perder a posicao."
                ]),

            Printable(
                "44B1A920-3A64-4B4E-B1E8-5B1FCE2B6E35",
                "orcamento-e-cenarios",
                "Orcamento e Cenarios",
                "Atividade de matematica",
                "orcamento, porcentagem, cenarios, decisao",
                "Folha autoral para 13 a 14 anos com comparacao de custos, porcentagens e escolha final justificada.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: ORCAMENTO E CENARIOS\n\nOpcao A: __________________\nCusto inicial: ______\nCusto posterior: ______\n\nOpcao B: __________________\nCusto inicial: ______\nCusto posterior: ______\n\nTotal previsto de cada opcao:\nA: ______\nB: ______",
                    "Pagina 2\nDECISAO JUSTIFICADA\n\nQual opcao faz mais sentido?\n______________________________\n\nQue numero ou porcentagem prova isso?\n______________________________\n\nMinha justificativa final:\n______________________________"
                ]),

            Printable(
                "F8FE9E76-C5FC-4E76-8A8F-2DAAF315BE55",
                "causa-consequencia-e-posicao",
                "Causa, Consequencia e Posicao",
                "Atividade de mundo",
                "causa, consequencia, posicao, estudo de caso",
                "Folha autoral para 13 a 14 anos com quadro de causas, impactos e posicao final sobre um caso real.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: CAUSA E CONSEQUENCIA\n\nCaso estudado:\n______________________________\n\nCausa principal:\n______________________________\n\nConsequencia 1:\n______________________________\n\nConsequencia 2:\n______________________________",
                    "Pagina 2\nPOSICAO FINAL\n\nDepois da analise, que resposta faz mais sentido?\n______________________________\n______________________________\n\nQue evidencias do caso sustentam essa posicao?\n______________________________"
                ]),

            Printable(
                "1DA355B9-0C2B-4A26-9C32-FD7DA3DF8A01",
                "sprint-de-estudo-e-revisao-final",
                "Sprint de Estudo e Revisao Final",
                "Atividade de autonomia",
                "sprint, meta, revisao, ajuste",
                "Folha autoral para 13 a 14 anos com meta do sprint, prova da entrega e ajuste final do metodo.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nATIVIDADE: SPRINT DE ESTUDO\n\nMeta unica do sprint:\n______________________________\n\nTempo previsto:\n______________________________\n\nQual prova mostrara que terminou?\n______________________________",
                    "Pagina 2\nREVISAO E AJUSTE\n\nO que foi concluido de verdade?\n______________________________\n\nO que funcionou bem no metodo?\n______________________________\n\nQue ajuste entra no proximo sprint?\n______________________________"
                ]),

            Printable(
                "2F8E4181-9B9B-4E43-9375-777EAF3A4B09",
                "checklist-de-estudo-e-autochecagem",
                "Checklist de Estudo e Autochecagem",
                "Atividade de autonomia",
                "autonomia, checklist, revisar, concluir, rotina",
                "Folha autoral para 7 a 10 anos com abertura, execução e revisão de uma missão de estudo.",
                7,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nATIVIDADE: CHECKLIST DE ESTUDO\n\nAntes de começar:\n[ ] Separei o material certo.\n[ ] Li a missão do dia.\n[ ] Sei qual é o primeiro passo.\n\nDurante a tarefa:\n[ ] Fiquei na mesma missão até terminar.\n[ ] Pedi ajuda só depois de tentar.\n[ ] Revisei o que escrevi.",
                    "Página 2\nAUTOCHECAGEM FINAL\n\nO que fiz bem hoje?\n________________________________\n\nO que preciso melhorar amanhã?\n________________________________\n\nQual será meu primeiro passo na próxima lição?\n________________________________"
                ])
        ];
    }

    public static bool IsAuthoredMaterial(FamilyLibraryMaterial material)
    {
        return IsAuthoredSyncToken(material.SourceSyncToken);
    }

    public static bool IsAuthoredSyncToken(string? sourceSyncToken)
    {
        return !string.IsNullOrWhiteSpace(sourceSyncToken)
               && sourceSyncToken.StartsWith($"{SyncPrefix}:", StringComparison.OrdinalIgnoreCase);
    }

    private static ProprietaryFamilyLibraryMaterialSeed Book(
        string id,
        string slug,
        string title,
        string category,
        string skillFocus,
        string description,
        int minAge,
        int maxAge,
        string educationStage,
        IReadOnlyList<string> pages)
    {
        return BuildSeed(id, slug, title, category, skillFocus, description, minAge, maxAge, educationStage, false, pages);
    }

    private static ProprietaryFamilyLibraryMaterialSeed Printable(
        string id,
        string slug,
        string title,
        string category,
        string skillFocus,
        string description,
        int minAge,
        int maxAge,
        string educationStage,
        IReadOnlyList<string> pages)
    {
        return BuildSeed(id, slug, title, category, skillFocus, description, minAge, maxAge, educationStage, true, pages);
    }

    private static ProprietaryFamilyLibraryMaterialSeed BuildSeed(
        string id,
        string slug,
        string title,
        string category,
        string skillFocus,
        string description,
        int minAge,
        int maxAge,
        string educationStage,
        bool isPrintable,
        IReadOnlyList<string> pages)
    {
        return new ProprietaryFamilyLibraryMaterialSeed
        {
            Id = Guid.Parse(id),
            Slug = slug,
            Title = title,
            Category = category,
            EducationStage = educationStage,
            RecommendedMinAge = minAge,
            RecommendedMaxAge = maxAge,
            SkillFocus = skillFocus,
            Description = description,
            CollectionLabel = CollectionLabel,
            IsPrintable = isPrintable,
            HasIllustrations = false,
            CoverImageRelativePath = string.Empty,
            SourceRelativePath = $"author/{slug}",
            SourceSyncToken = $"{SyncPrefix}:{slug}:v1",
            SourceUpdatedAtUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Pages = pages
                .Select((text, index) => new ProprietaryFamilyLibraryPageSeed
                {
                    PageNumber = index + 1,
                    TextContent = text
                })
                .ToList()
        };
    }
}

public class ProprietaryFamilyLibraryMaterialSeed
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public int RecommendedMinAge { get; set; }
    public int RecommendedMaxAge { get; set; }
    public string SkillFocus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public bool IsPrintable { get; set; }
    public bool HasIllustrations { get; set; }
    public string CoverImageRelativePath { get; set; } = string.Empty;
    public string SourceRelativePath { get; set; } = string.Empty;
    public string SourceSyncToken { get; set; } = string.Empty;
    public DateTime SourceUpdatedAtUtc { get; set; }
    public List<ProprietaryFamilyLibraryPageSeed> Pages { get; set; } = new();
}

public class ProprietaryFamilyLibraryPageSeed
{
    public int PageNumber { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string ImageRelativePath { get; set; } = string.Empty;
}
