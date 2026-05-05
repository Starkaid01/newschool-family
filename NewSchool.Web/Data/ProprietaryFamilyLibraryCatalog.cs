using NewSchool.Web.Domain;
using NewSchool.Web.Models;
using NewSchool.Web.Services;

namespace NewSchool.Web.Data;

public static class ProprietaryFamilyLibraryCatalog
{
    public const string CollectionLabel = "Coleção NewSchool";
    private const string SyncPrefix = "AUTHOR";

    public static IReadOnlyList<ProprietaryFamilyLibraryMaterialSeed> Build()
    {
        var materials = new List<ProprietaryFamilyLibraryMaterialSeed>
        {
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
                ]),

            Printable(
                "0F7A4E8F-CC7C-4F3E-B6D2-BD81918FD7A4",
                "sondagem-de-letras-e-tracos",
                "Sondagem de Letras e Traços",
                "Avaliação diagnóstica",
                "letras, som inicial, traçado, observação",
                "Sondagem autoral para 3 a 4 anos com observação de letra inicial, traçado grande e nomeação simples.",
                3,
                4,
                "Educação Infantil",
                [
                    "Página 1\nSONDAGEM: LETRAS E TRAÇOS\n\n1. Diga o nome da criança e peça que ela encontre a primeira letra.\n2. Mostre três letras grandes e observe qual ela reconhece.\n3. Peça um traçado grande no ar e outro no papel.\n\nRegistro do adulto:\nReconheceu a letra do nome?  sim / com ajuda / ainda não\nSeguiu o traçado?  sim / com ajuda / ainda não\nNomeou um objeto com o mesmo som?  sim / com ajuda / ainda não",
                    "Página 2\nOBSERVAÇÃO FINAL\n\nO que ficou fácil hoje?\n________________________________\n\nO que precisa voltar na próxima semana?\n________________________________\n\nMeta curta para a próxima aula:\n________________________________"
                ]),

            Printable(
                "903A1D78-0B11-40E1-95D8-6E4C4FB6A5D0",
                "sondagem-de-contagem-e-grupos",
                "Sondagem de Contagem e Grupos",
                "Avaliação diagnóstica",
                "contagem, quantidade, grupos, comparação",
                "Sondagem autoral para 5 a 6 anos que mede contagem real, comparação de grupos e explicação curta.",
                5,
                6,
                "Educação Infantil",
                [
                    "Página 1\nSONDAGEM: CONTAGEM E GRUPOS\n\nMonte 3 grupos reais com objetos.\nGrupo A: 4 itens\nGrupo B: 6 itens\nGrupo C: 8 itens\n\nPeça que a criança:\n1. Conte tocando um por um.\n2. Diga qual grupo tem mais.\n3. Diga qual grupo tem menos.\n\nRegistro do adulto:\nContou sem pular itens?  sim / com ajuda / ainda não\nComparou grupos?  sim / com ajuda / ainda não",
                    "Página 2\nFECHAMENTO\n\nA criança explicou o raciocínio?\n________________________________\n\nErro mais comum observado:\n________________________________\n\nPróxima meta de matemática:\n________________________________"
                ]),

            Printable(
                "E7F4A0C2-6D0D-485C-9B60-9DAAABF53C4A",
                "avaliacao-de-reconto-e-sequencia",
                "Avaliação de Reconto e Sequência",
                "Avaliação diagnóstica",
                "reconto, sequência, personagem, problema, solução",
                "Avaliação autoral para 7 a 8 anos que verifica reconto em ordem, personagem, problema e solução.",
                7,
                8,
                "Ensino Fundamental",
                [
                    "Página 1\nAVALIAÇÃO: RECONTO E SEQUÊNCIA\n\nDepois da leitura, peça que a criança complete:\nQuem é o personagem principal?\n________________________________\n\nQual problema apareceu?\n________________________________\n\nO que aconteceu primeiro?\n________________________________",
                    "Página 2\nMEIO E FIM\n\nO que aconteceu depois?\n________________________________\n\nComo a história terminou?\n________________________________\n\nRegistro do adulto:\nManteve a ordem?  sim / com ajuda / ainda não\nSeparou essencial de detalhe?  sim / com ajuda / ainda não"
                ]),

            Printable(
                "2EDC8FC7-29BE-4E98-A3DB-C8A58940F96E",
                "avaliacao-de-ideia-central-e-provas",
                "Avaliação de Ideia Central e Provas",
                "Avaliação diagnóstica",
                "ideia central, detalhe, prova textual, síntese",
                "Avaliação autoral para 9 a 10 anos que mede síntese, ideia central e dois detalhes que provam a resposta.",
                9,
                10,
                "Ensino Fundamental",
                [
                    "Página 1\nAVALIAÇÃO: IDEIA CENTRAL\n\nDepois do texto, peça:\n1. Escreva a ideia central em uma frase.\n2. Escolha dois detalhes que realmente sustentam essa ideia.\n\nIdeia central:\n________________________________\n\nDetalhe 1:\n________________________________\n\nDetalhe 2:\n________________________________",
                    "Página 2\nRUBRICA RÁPIDA\n\nA resposta foi clara?  sim / com ajuda / ainda não\nOs detalhes realmente provam a ideia?  sim / com ajuda / ainda não\nA criança evitou copiar tudo?  sim / com ajuda / ainda não\n\nPróximo foco de linguagem:\n________________________________"
                ]),

            Printable(
                "C24E1FA5-9918-4C1E-B55D-510AB49079D9",
                "avaliacao-de-tese-e-duas-fontes",
                "Avaliação de Tese e Duas Fontes",
                "Avaliação diagnóstica",
                "tese, prova, fonte, comparação, conclusão",
                "Avaliação autoral para 11 a 12 anos que verifica tese curta, uso de duas fontes e conclusão própria.",
                11,
                12,
                "Ensino Fundamental",
                [
                    "Pagina 1\nAVALIACAO: TESE E FONTES\n\nTema comum das leituras:\n______________________________\n\nMinha tese:\n______________________________\n______________________________\n\nFonte 1: dado ou prova principal\n______________________________\n______________________________",
                    "Pagina 2\nFONTE 2 E CONCLUSAO\n\nFonte 2: dado ou explicacao principal\n______________________________\n______________________________\n\nMinha conclusao final em palavras proprias:\n______________________________\n______________________________\n\nRegistro do adulto:\nUsou duas fontes?  sim / com ajuda / ainda nao"
                ]),

            Printable(
                "A7C83EC6-1E4E-47BF-A88D-3F0C8A96E130",
                "avaliacao-de-argumento-e-projeto-integrador",
                "Avaliação de Argumento e Projeto Integrador",
                "Avaliação diagnóstica",
                "argumento, objecao, orçamento, projeto, decisão",
                "Avaliação autoral para 13 a 14 anos que junta tese, contra-argumento e decisão prática em um mini projeto.",
                13,
                14,
                "Ensino Fundamental",
                [
                    "Pagina 1\nAVALIACAO: ARGUMENTO E DECISAO\n\nProblema estudado:\n______________________________\n\nMinha tese:\n______________________________\n______________________________\n\nUma objecao real a esta tese:\n______________________________\n______________________________",
                    "Pagina 2\nPROJETO INTEGRADOR\n\nEscolha um pequeno cenario real e tome uma decisao justificada.\nCenario:\n______________________________\n\nMelhor escolha:\n______________________________\n\nNumero, fato ou evidência que sustenta a escolha:\n______________________________\n\nRegistro do adulto:\nSustentou a posicao com clareza?  sim / com ajuda / ainda nao"
                ])
        };

        materials.AddRange(BuildGeneratedCurriculumCollection());
        return materials;
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

    private static IReadOnlyList<ProprietaryFamilyLibraryMaterialSeed> BuildGeneratedCurriculumCollection()
    {
        var blueprintService = new ProprietaryCurriculumBlueprintService();
        var generated = new List<ProprietaryFamilyLibraryMaterialSeed>();

        foreach (var age in Enumerable.Range(3, 12))
        {
            foreach (var domain in CurriculumStructure.AnnualSubjectOrder)
            {
                var subject = blueprintService.BuildSubject(age, domain);
                var placementShortLabel = GetPlacementShortLabel(age);
                var subjectLabel = subject.SubjectLabel;
                var bookSlug = Slugify($"leituras {subjectLabel} {placementShortLabel}");
                var workbookSlug = Slugify($"apostila {subjectLabel} {placementShortLabel}");
                var assessmentSlug = Slugify($"avaliacao {subjectLabel} {placementShortLabel}");

                generated.Add(new ProprietaryFamilyLibraryMaterialSeed
                {
                    Id = CreateDeterministicGuid($"author-book:{bookSlug}"),
                    Slug = bookSlug,
                    Title = $"Leituras de {subjectLabel} • {placementShortLabel}",
                    Category = "Leituras-base do currículo",
                    EducationStage = CurriculumStructure.GetEducationStageLabel(age),
                    RecommendedMinAge = age,
                    RecommendedMaxAge = age,
                    SkillFocus = $"{subjectLabel}, leitura-base, unidade anual, ensino domiciliar",
                    Description = $"Livro-base autoral do NewSchool para {subjectLabel.ToLowerInvariant()} em {placementShortLabel.ToLowerInvariant()}, com uma leitura por unidade do ano.",
                    CollectionLabel = CollectionLabel,
                    IsPrintable = false,
                    HasIllustrations = false,
                    CoverImageRelativePath = string.Empty,
                    SourceRelativePath = $"author/{bookSlug}",
                    SourceSyncToken = $"{SyncPrefix}:{bookSlug}:v4",
                    SourceUpdatedAtUtc = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                    Pages = BuildReadingPages(subject)
                });

                generated.Add(new ProprietaryFamilyLibraryMaterialSeed
                {
                    Id = CreateDeterministicGuid($"author-printable:{workbookSlug}"),
                    Slug = workbookSlug,
                    Title = $"Apostila de {subjectLabel} • {placementShortLabel}",
                    Category = "Apostila do currículo",
                    EducationStage = CurriculumStructure.GetEducationStageLabel(age),
                    RecommendedMinAge = age,
                    RecommendedMaxAge = age,
                    SkillFocus = $"{subjectLabel}, prática guiada, registro, rotina do currículo",
                    Description = $"Apostila autoral do NewSchool com uma folha de trabalho por unidade para {subjectLabel.ToLowerInvariant()} em {placementShortLabel.ToLowerInvariant()}.",
                    CollectionLabel = CollectionLabel,
                    IsPrintable = true,
                    HasIllustrations = false,
                    CoverImageRelativePath = string.Empty,
                    SourceRelativePath = $"author/{workbookSlug}",
                    SourceSyncToken = $"{SyncPrefix}:{workbookSlug}:v4",
                    SourceUpdatedAtUtc = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                    Pages = BuildWorkbookPages(subject)
                });

                generated.Add(new ProprietaryFamilyLibraryMaterialSeed
                {
                    Id = CreateDeterministicGuid($"author-assessment:{assessmentSlug}"),
                    Slug = assessmentSlug,
                    Title = $"Caderno de avaliação de {subjectLabel} • {placementShortLabel}",
                    Category = "Avaliação por unidade",
                    EducationStage = CurriculumStructure.GetEducationStageLabel(age),
                    RecommendedMinAge = age,
                    RecommendedMaxAge = age,
                    SkillFocus = $"{subjectLabel}, prova, fechamento, rubrica de etapa",
                    Description = $"Caderno autoral do NewSchool com uma prova curta por unidade de {subjectLabel.ToLowerInvariant()} em {placementShortLabel.ToLowerInvariant()}.",
                    CollectionLabel = CollectionLabel,
                    IsPrintable = true,
                    HasIllustrations = false,
                    CoverImageRelativePath = string.Empty,
                    SourceRelativePath = $"author/{assessmentSlug}",
                    SourceSyncToken = $"{SyncPrefix}:{assessmentSlug}:v4",
                    SourceUpdatedAtUtc = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                    Pages = BuildAssessmentPages(subject)
                });
            }
        }

        return generated;
    }

    private static List<ProprietaryFamilyLibraryPageSeed> BuildReadingPages(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        var pages = new List<ProprietaryFamilyLibraryPageSeed>();
        var pageNumber = 1;

        pages.Add(new ProprietaryFamilyLibraryPageSeed
        {
            PageNumber = pageNumber++,
            TextContent = BuildReadingCoverPage(subject)
        });

        foreach (var unit in subject.Phases.SelectMany(phase => phase.Units))
        {
            foreach (var lesson in unit.Lessons.OrderBy(item => item.LessonNumber))
            {
                pages.Add(new ProprietaryFamilyLibraryPageSeed
                {
                    PageNumber = pageNumber++,
                    TextContent = BuildReadingLessonPage(subject, unit, lesson)
                });
            }
        }

        return pages;
    }

    private static List<ProprietaryFamilyLibraryPageSeed> BuildWorkbookPages(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        var pages = new List<ProprietaryFamilyLibraryPageSeed>();
        var pageNumber = 1;

        pages.Add(new ProprietaryFamilyLibraryPageSeed
        {
            PageNumber = pageNumber++,
            TextContent = BuildWorkbookCoverPage(subject)
        });

        foreach (var unit in subject.Phases.SelectMany(phase => phase.Units))
        {
            pages.Add(new ProprietaryFamilyLibraryPageSeed
            {
                PageNumber = pageNumber++,
                TextContent = BuildWorkbookUnitOverviewPage(subject, unit)
            });

            foreach (var lesson in unit.Lessons.OrderBy(item => item.LessonNumber))
            {
                pages.Add(new ProprietaryFamilyLibraryPageSeed
                {
                    PageNumber = pageNumber++,
                    TextContent = BuildWorkbookLessonPage(subject, unit, lesson)
                });
            }
        }

        return pages;
    }

    private static List<ProprietaryFamilyLibraryPageSeed> BuildAssessmentPages(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        var pages = new List<ProprietaryFamilyLibraryPageSeed>();
        var pageNumber = 1;

        pages.Add(new ProprietaryFamilyLibraryPageSeed
        {
            PageNumber = pageNumber++,
            TextContent = BuildAssessmentCoverPage(subject)
        });

        foreach (var unit in subject.Phases.SelectMany(phase => phase.Units))
        {
            pages.Add(new ProprietaryFamilyLibraryPageSeed
            {
                PageNumber = pageNumber++,
                TextContent = BuildAssessmentUnitOverviewPage(subject, unit)
            });

            foreach (var lesson in unit.Lessons.OrderBy(item => item.LessonNumber))
            {
                pages.Add(new ProprietaryFamilyLibraryPageSeed
                {
                    PageNumber = pageNumber++,
                    TextContent = BuildAssessmentLessonPage(subject, unit, lesson)
                });
            }
        }

        return pages;
    }

    private static string BuildReadingCoverPage(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        return
            $"LEITURAS DE {subject.SubjectLabel.ToUpperInvariant()} • {GetPlacementShortLabel(subject.Age).ToUpperInvariant()}\n\n" +
            $"Este volume reúne leituras curtas de {subject.SubjectLabel.ToLowerInvariant()} para {GetPlacementShortLabel(subject.Age).ToLowerInvariant()}, com linguagem adequada à série e progressão real ao longo do ano.\n\n" +
            $"{BuildReadingCollectionOpening(subject)}\n\n" +
            $"Use uma página por vez. Quando a leitura render bem, avance. Quando já bastar por hoje, feche sem alongar.";
    }

    private static string BuildReadingLessonPage(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        var paragraphs = BuildReadingLessonParagraphs(subject, unit, lesson);

        return
            $"{lesson.Title.ToUpperInvariant()}\n\n" +
            $"{string.Join("\n\n", paragraphs)}";
    }

    private static string BuildReadingCollectionOpening(ProprietaryCurriculumSubjectBlueprintViewModel subject) => subject.Domain switch
    {
        LearningDomain.Language => "As leituras deste caderno foram escritas para formar leitor, ampliar repertório e puxar conversa boa, sem transformar cada página em folha de exercício.",
        LearningDomain.Math => "As leituras deste caderno colocam a matemática dentro de situações claras do cotidiano, para a criança pensar antes de calcular e entender por que a resposta faz sentido.",
        LearningDomain.Science => "As leituras deste caderno apresentam observações, experiências e descobertas em linguagem acessível, para a criança ligar curiosidade, registro e explicação.",
        LearningDomain.History => "As leituras deste caderno trazem memória, tempo, mudança e personagens em textos curtos, para a criança aprender a enxergar passado com ordem e significado.",
        LearningDomain.Geography => "As leituras deste caderno aproximam mapa, lugar, paisagem e território da vida concreta, para a criança perceber o espaço como parte da rotina e da sociedade.",
        LearningDomain.ExecutiveFunction => "As leituras deste caderno mostram rotina, organização, responsabilidade e revisão em cenas simples, para a criança aprender a agir com mais autonomia.",
        _ => "Este volume reúne leituras curtas com progressão real ao longo do ano."
    };

    private static IReadOnlyList<string> BuildReadingLessonParagraphs(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        var unitText = unit.Title.ToLowerInvariant();
        var placement = GetPlacementShortLabel(subject.Age);

        return subject.Domain switch
        {
            LearningDomain.Language when subject.Age <= 5 =>
            [
                $"Na roda de leitura, Sara ouviu um trecho curto sobre {unitText} e acompanhou cada palavra com o dedo, sem pressa. Quando a voz do adulto fazia pausa, ela olhava de novo para o cartaz e tentava descobrir onde a frase começava e onde terminava.",
                $"Na segunda vez, Sara percebeu uma palavra que voltava no texto e apontou para a parte mais importante antes de falar qualquer resposta. Em vez de repetir por imitação, ela começou a ligar som, imagem e sentido no mesmo momento.",
                $"No fim, Sara contou com palavras simples o que entendeu e desenhou a parte que mais chamou sua atenção. A leitura ficou curta, mas clara, e o texto deixou de ser só som para virar compreensão."
            ],
            LearningDomain.Language when subject.Age <= 7 =>
            [
                $"No mural da classe havia um texto curto sobre {unitText}. Daniel leu primeiro devagar, marcando com o dedo as palavras que já reconhecia, e depois voltou ao começo para perceber qual frase realmente carregava a ideia principal.",
                $"Enquanto relia, Daniel separou o que era detalhe do que era essencial. Ele descobriu que uma boa resposta não depende de copiar tudo, mas de escolher a informação certa e falar com começo, meio e fim.",
                $"Quando terminou, Daniel explicou o texto com uma frase completa e mostrou no caderno a palavra ou o trecho que sustentava sua resposta. Assim, a leitura virou pensamento organizado."
            ],
            LearningDomain.Language when subject.Age <= 10 =>
            [
                $"O texto do dia apresentava uma situação ligada a {unitText} e pedia atenção aos detalhes que realmente sustentavam a ideia central. Elisa leu uma vez para entender o assunto geral e depois releu procurando pistas mais fortes.",
                $"Na releitura, ela percebeu que algumas informações pareciam importantes, mas só duas realmente explicavam o ponto principal. Foi nesse momento que a leitura deixou de ser apenas passagem de olhos e começou a virar análise.",
                $"Ao fechar a página, Elisa conseguiu resumir o texto com clareza, citando a ideia central e os detalhes que a comprovavam. O foco não ficou em escrever muito, e sim em responder bem."
            ],
            LearningDomain.Language =>
            [
                $"O material desta página discute {unitText} em linguagem compatível com {placement.ToLowerInvariant()}. Em vez de apresentar frases soltas, o texto foi escrito para exigir leitura atenta, seleção de evidências e resposta bem sustentada.",
                $"Ao longo do texto, aparecem pistas que podem confirmar, ampliar ou até tensionar a interpretação inicial do leitor. Por isso, a compreensão não termina na primeira leitura: ela amadurece quando a criança compara trechos, pesa informações e organiza o raciocínio.",
                $"No fechamento, o que vale é construir uma resposta clara, mostrando qual ponto central foi encontrado e que passagens servem como prova. Ler bem, aqui, significa pensar antes de concluir."
            ],
            LearningDomain.Math when subject.Age <= 5 =>
            [
                $"No jardim da escola, Joana ajudava a organizar flores de papel para a festa. Enquanto separava as peças por cor e quantidade, ela percebeu que {unitText} aparecia bem diante dos seus olhos, sem precisar começar por conta escrita.",
                $"Joana olhou de novo para cada grupo, comparou tamanhos, contou devagar e mudou algumas peças de lugar para ver se tudo combinava com o que estava observando. O raciocínio nasceu do concreto antes de virar número.",
                $"Quando terminou, ela conseguiu explicar o que fez, apontando para as flores e mostrando por que a resposta estava certa. Foi assim que a matemática apareceu como pensamento visível, não como chute."
            ],
            LearningDomain.Math when subject.Age <= 10 =>
            [
                $"Na banca da feira da igreja, Miguel recebeu a tarefa de organizar produtos e conferir quantidades. O problema parecia simples no começo, mas logo ficou claro que {unitText} exigia olhar atento para os dados antes de qualquer conta.",
                $"Ele desenhou a situação, agrupou o que era parecido e testou uma estratégia de cada vez. Quando uma resposta parecia rápida demais, Miguel voltava ao enunciado para conferir se o resultado combinava com o que realmente estava sobre a mesa.",
                $"No fim, ele percebeu que resolver não era apenas chegar a um número. Era mostrar por que aquele número fazia sentido dentro do problema. Essa é a parte mais importante da matemática desta página."
            ],
            LearningDomain.Math =>
            [
                $"Durante a preparação de um mutirão solidário, Rebeca precisou analisar uma situação ligada a {unitText}. Havia dados suficientes para resolver o problema, mas a resposta só apareceria se ela escolhesse uma estratégia coerente, em vez de pular direto para uma operação automática.",
                $"Rebeca organizou informações, comparou possibilidades e testou o efeito de pequenas mudanças nos números. Em alguns momentos, a conta estava certa, mas o contexto mostrava que a conclusão ainda não fazia sentido.",
                $"Foi então que ela percebeu o ponto central desta leitura: a matemática mais forte não é a que termina primeiro, e sim a que explica o caminho, verifica o resultado e sustenta a decisão com clareza."
            ],
            LearningDomain.Science when subject.Age <= 5 =>
            [
                $"Perto da janela, Tiago observou uma pequena planta enquanto a luz da manhã mudava de lugar. O adulto pediu silêncio por alguns segundos, e a criança percebeu detalhes que normalmente passariam despercebidos. Era assim que {unitText} começava a fazer sentido.",
                $"Depois da primeira observação, Tiago tocou de leve a folha, comparou cor, tamanho e posição e tentou adivinhar o que poderia acontecer mais tarde. A curiosidade veio antes da resposta pronta.",
                $"No fim, ele registrou com desenho e fala o que tinha visto. A ciência desta página nasce desse gesto simples: olhar com cuidado, comparar e explicar com as próprias palavras."
            ],
            LearningDomain.Science when subject.Age <= 10 =>
            [
                $"A investigação de hoje começa com uma observação concreta sobre {unitText}. Sofia notou que pequenas mudanças em um objeto, em um ser vivo ou em um material quase sempre revelam uma lógica escondida para quem aprende a observar sem pressa.",
                $"Por isso, ela primeiro descreveu o que viu, depois levantou uma hipótese e só então comparou a ideia inicial com o que o experimento ou a imagem mostravam. Nem toda expectativa se confirmou, e isso fez parte do aprendizado.",
                $"Ao final, Sofia conseguiu explicar o fenômeno com linguagem simples, mas precisa. A ciência se fortaleceu quando observação, hipótese e evidência começaram a caminhar juntas."
            ],
            LearningDomain.Science =>
            [
                $"O texto desta página apresenta uma situação de {unitText} como investigação, não como curiosidade isolada. A leitura convida o estudante a observar, levantar hipótese, reconhecer variável relevante e confrontar a explicação com uma evidência visível.",
                $"Ao longo do processo, algumas pistas parecem secundárias, mas ajudam a delimitar o que realmente interfere no fenômeno. É por isso que a ciência exige mais do que opinião: ela pede registro, comparação e coerência entre causa e efeito.",
                $"Quando a leitura termina, o estudante é levado a explicar o que viu usando linguagem clara e fundamento observável. O conhecimento se consolida quando a conclusão consegue dialogar com a evidência."
            ],
            LearningDomain.History when subject.Age <= 5 =>
            [
                $"No armário da sala havia uma caixa com fotos antigas da família. Lara olhou primeiro para as roupas, depois para os rostos e por fim para os objetos ao redor. Sem perceber, ela já estava entrando em {unitText} pela porta da memória.",
                $"Cada foto mostrava que o tempo muda as pessoas e também muda a casa, a rua e o jeito de viver. Lara começou a notar o que veio antes, o que ficou parecido e o que já não era mais igual.",
                $"No fim, ela contou a história de uma das fotos com começo, meio e fim. Assim, a história deixou de ser coisa distante e virou leitura do tempo vivido."
            ],
            LearningDomain.History when subject.Age <= 10 =>
            [
                $"Dona Celina abriu uma pasta com cartas, fotografias e anotações antigas para conversar sobre {unitText}. Cada documento parecia pequeno sozinho, mas juntos eles começavam a mostrar como as pessoas viveram, decidiram e mudaram o lugar onde estavam.",
                $"Ao observar as pistas, a criança percebeu que história não é só decorar datas. É entender a ordem dos fatos, reconhecer causas e perceber que uma mudança quase sempre deixa vestígios.",
                $"Quando a leitura termina, fica mais fácil explicar o que veio primeiro, o que mudou depois e por que aquilo importa. É assim que o passado começa a fazer sentido no presente."
            ],
            LearningDomain.History =>
            [
                $"A leitura desta página apresenta {unitText} por meio de uma fonte, de uma memória registrada ou de um episódio histórico organizado em sequência clara. O estudante não recebe apenas um fato: recebe um contexto para interpretar.",
                $"Ao comparar informações, percebe que processos históricos envolvem disputas, permanências e rupturas. Certas mudanças parecem rápidas; outras levam tempo e só se tornam visíveis quando diferentes pistas são lidas em conjunto.",
                $"Por isso, o fechamento histórico desta página não busca repetição. Busca compreensão: o que estava em jogo, o que mudou e que evidência permite sustentar essa conclusão."
            ],
            LearningDomain.Geography when subject.Age <= 5 =>
            [
                $"Na volta para casa, Ana percebeu que o caminho tinha subidas, esquinas, árvores e lugares onde o vento batia mais forte. Sem usar mapa formal, ela já estava lendo o espaço e entrando em {unitText}.",
                $"Quando o adulto perguntou onde ficava a padaria, de onde vinha o sol da tarde e qual era o trecho mais barulhento, Ana começou a comparar referências e a perceber que cada lugar conta alguma coisa sobre quem vive nele.",
                $"No fim, ela conseguiu apontar o trajeto, nomear pontos importantes e explicar o que mudava de um lugar para outro. A geografia apareceu como leitura do mundo próximo."
            ],
            LearningDomain.Geography when subject.Age <= 10 =>
            [
                $"O estudo de hoje começa com um lugar real ligado a {unitText}. Pode ser uma rua, um bairro, um mapa simples ou uma paisagem conhecida. O importante é perceber que o espaço nunca é neutro: ele mostra escolhas, usos e relações.",
                $"Ao comparar dois pontos do território, a criança nota diferença de circulação, de moradia, de vegetação ou de atividade humana. O mapa deixa de ser desenho parado e vira forma de entender a vida concreta.",
                $"Quando a leitura termina, a criança já consegue dizer o que vê, o que muda e por que aquele espaço funciona daquele jeito. É nesse momento que a geografia ganha sentido."
            ],
            LearningDomain.Geography =>
            [
                $"Esta leitura trata de {unitText} como relação entre espaço, sociedade e organização do território. O estudante é convidado a observar um lugar, um mapa, uma rede ou uma paisagem percebendo não só nomes, mas relações e consequências.",
                $"Ao longo da página, surgem pistas sobre circulação, ocupação, recursos, contrastes e impactos. Ler geografia bem significa juntar essas pistas e entender por que o espaço assume determinada forma.",
                $"No fechamento, o estudante é levado a produzir uma interpretação: como esse território funciona, quem o usa, o que muda nele e que leitura crítica pode ser feita a partir dessas evidências."
            ],
            LearningDomain.ExecutiveFunction when subject.Age <= 5 =>
            [
                $"Antes de começar a brincadeira, Noa precisou separar poucos materiais, ouvir o primeiro combinado e lembrar que não precisava fazer tudo de uma vez. Foi assim que {unitText} entrou na rotina sem peso e sem correria.",
                $"Quando percebeu vontade de pular etapas, ela voltou ao primeiro passo, conferiu o que já estava pronto e seguiu mais segura. A missão ficou pequena o bastante para caber nas mãos da criança.",
                $"No final, Noa guardou o material, mostrou o que tinha terminado e conseguiu dizer qual seria o próximo passo em outra hora. Autonomia começou a aparecer como hábito, não como discurso."
            ],
            LearningDomain.ExecutiveFunction when subject.Age <= 10 =>
            [
                $"João queria resolver tudo rápido, mas a rotina de {unitText} pedia outra atitude: preparar, executar e revisar. Quando ele separou o material certo antes de começar, percebeu que a tarefa ficava menor e mais possível.",
                $"Durante o processo, João travou uma vez, respirou e voltou ao combinado inicial. Em vez de abandonar a missão, ele marcou o que já tinha feito e escolheu o passo seguinte com mais clareza.",
                $"Ao final, revisou a entrega, guardou o que usou e identificou o ponto em que quase se perdeu. Essa revisão curta fez a autonomia crescer de verdade."
            ],
            LearningDomain.ExecutiveFunction =>
            [
                $"A cena desta página mostra {unitText} como prática de gestão pessoal. A tarefa não se resolve apenas com boa vontade: ela exige preparo, priorização, execução por etapas e revisão do que foi entregue.",
                $"Quando o estudante aprende a nomear onde trava, o que precisa antecipar e como checar o próprio trabalho, ele deixa de depender tanto do lembrete externo. A autonomia passa a ser construída em procedimentos visíveis.",
                $"Por isso, o valor desta leitura está menos em “fazer tudo” e mais em fazer com critério: começar bem, sustentar o foco e encerrar com revisão honesta."
            ],
            _ =>
            [
                $"{lesson.Title} aparece aqui em forma de leitura curta para {placement.ToLowerInvariant()}.",
                $"Ao longo do texto, a criança encontra pistas concretas sobre {unitText}.",
                "A proposta desta página é ler com calma e sair dela com uma ideia principal clara."
            ]
        };
    }

    private static string BuildWorkbookCoverPage(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        return
            $"APOSTILA DE TRABALHO • {subject.SubjectLabel.ToUpperInvariant()} • {GetPlacementShortLabel(subject.Age).ToUpperInvariant()}\n\n" +
            $"Esta apostila acompanha o percurso anual de {subject.SubjectLabel.ToLowerInvariant()} com atividades curtas, dirigidas e ligadas às lições reais do sistema.\n\n" +
            $"Meta do ano:\n{subject.YearGoal}\n\n" +
            $"Como aplicar sem sobrecarregar:\n{subject.ParentMethod}\n\n" +
            $"Regra de uso:\nFaça uma página por vez, feche a lição e só então avance para a próxima folha.";
    }

    private static string BuildWorkbookUnitOverviewPage(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit)
    {
        return
            $"ROTEIRO DA UNIDADE • {unit.UnitLabel.ToUpperInvariant()} • {unit.Title.ToUpperInvariant()}\n\n" +
            $"O que a criança precisa consolidar:\n{unit.Objective}\n\n" +
            $"Sequência da unidade:\n- {string.Join("\n- ", unit.LessonTitles)}\n\n" +
            $"Materiais básicos:\n- {string.Join("\n- ", unit.Materials)}\n\n" +
            $"Como esta unidade sobe de dificuldade nesta série:\n{BuildDomainSeriesSignal(subject.Domain, subject.Age)}\n\n" +
            $"Ao final, considere concluído quando:\n{unit.CompletionSignal}";
    }

    private static string BuildWorkbookLessonPage(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        var lines = BuildWorkbookTasksForDomain(subject.Domain, subject.Age, unit, lesson);

        return
            $"ATIVIDADE • {unit.UnitLabel.ToUpperInvariant()} • LIÇÃO {lesson.LessonNumber}\n\n" +
            $"Título da atividade:\n{lesson.Title}\n\n" +
            $"Antes de começar:\n{lesson.OpeningForAdult}\n\n" +
            $"O que fazer agora:\n{string.Join("\n", lines)}\n\n" +
            $"Registro do aluno:\n________________________________________\n________________________________________\n________________________________________\n\n" +
            $"Entrega esperada:\n{lesson.PracticeTask}\n\n" +
            $"O adulto observa se:\n{lesson.ExpectedOutcome}";
    }

    private static string BuildAssessmentCoverPage(ProprietaryCurriculumSubjectBlueprintViewModel subject)
    {
        return
            $"CADERNO DE AVALIAÇÃO • {subject.SubjectLabel.ToUpperInvariant()} • {GetPlacementShortLabel(subject.Age).ToUpperInvariant()}\n\n" +
            $"Este caderno fecha cada unidade com prova curta, registro claro e critério de avanço alinhado à série.\n\n" +
            $"Meta do ano:\n{subject.YearGoal}\n\n" +
            $"Como avaliar nesta matéria:\n{BuildAssessmentMethod(subject.Domain, subject.Age)}\n\n" +
            $"Importante:\nA avaliação serve para mostrar progresso real, não para alongar a rotina além do necessário.";
    }

    private static string BuildAssessmentUnitOverviewPage(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit)
    {
        return
            $"PROVA DE UNIDADE • {unit.UnitLabel.ToUpperInvariant()} • {unit.Title.ToUpperInvariant()}\n\n" +
            $"Foco do fechamento:\n{unit.Objective}\n\n" +
            $"Competências observadas nesta série:\n{BuildDomainSeriesSignal(subject.Domain, subject.Age)}\n\n" +
            $"O adulto usa esta prova para ver se a criança:\n- responde à pergunta principal\n- usa a estratégia adequada\n- fecha a atividade com clareza\n\n" +
            $"Critério de avanço da unidade:\n{unit.CompletionSignal}";
    }

    private static string BuildAssessmentLessonPage(
        ProprietaryCurriculumSubjectBlueprintViewModel subject,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        var prompts = BuildAssessmentPromptsForDomain(subject.Domain, subject.Age, unit, lesson);

        return
            $"AVALIAÇÃO CURTA • {unit.UnitLabel.ToUpperInvariant()} • LIÇÃO {lesson.LessonNumber}\n\n" +
            $"Tarefa avaliada:\n{lesson.Title}\n\n" +
            $"Pergunta-base:\n{lesson.AnchorQuestion}\n\n" +
            $"Aplicação:\n{string.Join("\n", prompts)}\n\n" +
            $"Rubrica rápida do adulto:\n" +
            $"[ ] Entendeu a consigna sem ajuda pesada.\n" +
            $"[ ] Entregou algo compatível com a série.\n" +
            $"[ ] Mostrou {lesson.ExpectedOutcome}.\n" +
            $"[ ] Pode seguir para a próxima lição.\n\n" +
            $"Registro do adulto:\n________________________________________\n________________________________________\n\n" +
            $"Evidência opcional:\n{lesson.EvidencePrompt}";
    }

    private static string BuildSeriesProgressionNote(ProprietaryCurriculumSubjectBlueprintViewModel subject) => subject.Domain switch
    {
        LearningDomain.Math when subject.Age <= 5 => "Sai de contagem e comparação concreta para agrupamento, composição e registro inicial sem pular o concreto.",
        LearningDomain.Math when subject.Age <= 10 => "Cada ano adiciona estratégia, linguagem matemática e complexidade de problema, não só números maiores.",
        LearningDomain.Math => "A série sobe em modelagem, comparação de cenários, álgebra, estatística e decisão com justificativa formal.",
        LearningDomain.Science when subject.Age <= 5 => "Começa em observação sensorial e nomeação do mundo próximo.",
        LearningDomain.Science when subject.Age <= 10 => "Sobe para experimento, registro, causa e consequência, com linguagem científica progressiva.",
        LearningDomain.Science => "Avança para sistema, método, hipótese, variável, evidência e explicação mais formal.",
        LearningDomain.History when subject.Age <= 5 => "Parte de rotina, família e passagem do tempo.",
        LearningDomain.History when subject.Age <= 10 => "A série cresce em leitura de fonte, sequência temporal e relação entre fato, contexto e mudança.",
        LearningDomain.History => "Aprofunda análise de processo histórico, conflito, permanência, ruptura e interpretação.",
        LearningDomain.Geography when subject.Age <= 5 => "Começa no corpo, casa, trajeto e clima próximo.",
        LearningDomain.Geography when subject.Age <= 10 => "A série sobe em mapa, território, paisagem e relação sociedade-ambiente.",
        LearningDomain.Geography => "Avança para região, rede, uso do território, urbanização e leitura crítica do espaço.",
        LearningDomain.ExecutiveFunction when subject.Age <= 5 => "Parte de iniciar, guardar e concluir pequenas missões com ajuda.",
        LearningDomain.ExecutiveFunction when subject.Age <= 10 => "Sobe para checklist, autocorreção, revisão e fechamento com cada vez menos suporte.",
        LearningDomain.ExecutiveFunction => "Avança para planejamento, priorização, autogestão de tempo e revisão por critério.",
        _ => "Cada série amplia a complexidade da linguagem, do raciocínio e da independência durante a aula."
    };

    private static string BuildReadingFrameForDomain(LearningDomain domain, int age, ProprietaryCurriculumLessonBlueprintViewModel lesson) => domain switch
    {
        LearningDomain.Math => $"Leitura matemática desta série:\nUse a situação como problema real. Antes de calcular, peça desenho, agrupamento ou tabela. Em {GetPlacementShortLabel(age)}, a meta não é só chegar no número, mas justificar a estratégia.",
        LearningDomain.Science => $"Leitura científica desta série:\nAbra a lição com observação, puxe uma hipótese simples e feche com explicação registrável. Em {GetPlacementShortLabel(age)}, a criança precisa ligar o que viu ao porquê do fenômeno.",
        LearningDomain.History => $"Leitura histórica desta série:\nApresente contexto, personagem, tempo ou fonte do dia. Em {GetPlacementShortLabel(age)}, a resposta precisa mostrar o que mudou, por que mudou ou que vestígio sustenta a conclusão.",
        LearningDomain.Geography => $"Leitura geográfica desta série:\nLocalize o lugar, compare referências e ligue espaço a vida concreta. Em {GetPlacementShortLabel(age)}, a criança precisa sair do nome solto e explicar relações no território.",
        LearningDomain.ExecutiveFunction => $"Leitura de autonomia desta série:\nTrate a missão como treino de rotina. Em {GetPlacementShortLabel(age)}, a criança precisa entender o passo atual, executar e revisar sem ficar adivinhando o que vem depois.",
        _ => $"Leitura guiada desta série:\n{lesson.OpeningForAdult}"
    };

    private static IEnumerable<string> BuildWorkbookTasksForDomain(
        LearningDomain domain,
        int age,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        return domain switch
        {
            LearningDomain.Math =>
            [
                $"1. Leia ou escute a situação e sublinhe a pergunta principal: {lesson.AnchorQuestion}",
                "2. Monte a estratégia antes do resultado: desenho, agrupamento, reta, tabela ou conta.",
                "3. Registre a resposta final e escreva por que essa estratégia faz sentido nesta questão."
            ],
            LearningDomain.Science =>
            [
                $"1. Observe o material da lição e registre uma descoberta inicial ligada a {lesson.Goal.ToLowerInvariant()}.",
                "2. Escreva ou desenhe o que mudou, apareceu, desapareceu ou se repetiu.",
                "3. Feche com uma explicação curta: o que você conclui e qual pista sustenta essa conclusão."
            ],
            LearningDomain.History =>
            [
                "1. Localize o fato no tempo, na família, na comunidade ou no processo histórico estudado.",
                $"2. Responda à pergunta central: {lesson.AnchorQuestion}",
                "3. Mostre uma pista, vestígio ou informação que prova a sua resposta."
            ],
            LearningDomain.Geography =>
            [
                "1. Identifique o lugar, mapa, trajeto, paisagem ou território desta lição.",
                "2. Compare dois pontos, usos ou referências do espaço observado.",
                $"3. Escreva uma conclusão curta que responda: {lesson.AnchorQuestion}"
            ],
            LearningDomain.ExecutiveFunction =>
            [
                "1. Marque o primeiro passo da missão e prepare só o material necessário.",
                "2. Faça a tarefa principal e registre onde quase travou ou se distraiu.",
                "3. Revise o que ficou pronto e marque o que ainda precisa voltar depois."
            ],
            _ =>
            [
                "1. Leia a instrução.",
                "2. Responda com base no material.",
                "3. Feche a atividade com uma prova curta."
            ]
        };
    }

    private static IEnumerable<string> BuildAssessmentPromptsForDomain(
        LearningDomain domain,
        int age,
        ProprietaryCurriculumUnitBlueprintViewModel unit,
        ProprietaryCurriculumLessonBlueprintViewModel lesson)
    {
        return domain switch
        {
            LearningDomain.Math =>
            [
                $"1. Resolva a situação principal da lição: {lesson.AnchorQuestion}",
                "2. Mostre no caderno a estratégia usada antes da resposta final.",
                "3. Explique em uma frase por que o resultado faz sentido."
            ],
            LearningDomain.Science =>
            [
                "1. Faça uma observação importante sobre o fenômeno ou material estudado.",
                "2. Registre uma hipótese, causa ou explicação ligada à observação.",
                "3. Aponte qual evidência do experimento, do texto ou da imagem sustenta sua conclusão."
            ],
            LearningDomain.History =>
            [
                "1. Diga o que aconteceu ou o que estava em jogo no contexto estudado.",
                "2. Explique uma causa, consequência, mudança ou permanência.",
                "3. Cite a pista, informação ou fonte que ajudou a fechar a resposta."
            ],
            LearningDomain.Geography =>
            [
                "1. Identifique o lugar, o mapa ou a situação espacial trabalhada.",
                "2. Compare dois elementos do território ou da paisagem.",
                "3. Feche com uma explicação sobre como esse espaço afeta a vida das pessoas."
            ],
            LearningDomain.ExecutiveFunction =>
            [
                "1. Mostre como a criança iniciou a missão sem ficar parada no começo.",
                "2. Observe se ela sustentou a sequência combinada até o final.",
                "3. Registre se revisou, checou ou guardou o material com mais autonomia."
            ],
            _ =>
            [
                "1. Responda à pergunta central.",
                "2. Mostre uma prova curta.",
                "3. Feche com clareza."
            ]
        };
    }

    private static string BuildAssessmentMethod(LearningDomain domain, int age) => domain switch
    {
        LearningDomain.Math => $"Em {GetPlacementShortLabel(age)}, avalie se a criança escolhe estratégia, representa o raciocínio e chega a um fechamento coerente.",
        LearningDomain.Science => $"Em {GetPlacementShortLabel(age)}, avalie observação, hipótese, explicação e vínculo entre pista e conclusão.",
        LearningDomain.History => $"Em {GetPlacementShortLabel(age)}, avalie leitura de contexto, causa e consequência, fonte e organização temporal.",
        LearningDomain.Geography => $"Em {GetPlacementShortLabel(age)}, avalie leitura do espaço, comparação, mapa, território e conclusão sobre relações humanas.",
        LearningDomain.ExecutiveFunction => $"Em {GetPlacementShortLabel(age)}, avalie início da tarefa, manutenção da sequência, revisão e fechamento autônomo.",
        _ => $"Em {GetPlacementShortLabel(age)}, avalie clareza, resposta e avanço real."
    };

    private static string BuildDomainSeriesSignal(LearningDomain domain, int age) => domain switch
    {
        LearningDomain.Math when age == 6 => "Neste 1º ano, a criança ainda precisa representar antes de simbolizar: contar, agrupar, desenhar e só depois registrar.",
        LearningDomain.Math when age == 7 => "Neste 2º ano, já precisa sustentar valor posicional e reagrupamento com mais estabilidade.",
        LearningDomain.Math when age == 8 => "Neste 3º ano, a série pede multiplicação com sentido, divisão inicial e leitura de dados simples.",
        LearningDomain.Math when age == 9 => "Neste 4º ano, a diferença aparece em problemas multietapas, tabuada com justificativa e fração como parte do todo.",
        LearningDomain.Math when age == 10 => "Neste 5º ano, a série sobe para perímetro, área, decimal, porcentagem inicial e decisão em contexto real.",
        LearningDomain.Math when age == 11 => "Neste 6º ano, a criança já precisa organizar razão, proporção, tabela e equação com sentido.",
        LearningDomain.Math when age == 12 => "Neste 7º ano, entram porcentagem com mais formalidade, regularidade algébrica e leitura crítica de dados.",
        LearningDomain.Math when age == 13 => "Neste 8º ano, a série pede modelagem, comparação de cenários e finanças com justificativa mais madura.",
        LearningDomain.Math when age == 14 => "Neste 9º ano, a série cobra função, variação, probabilidade e decisão financeira mais formal.",
        LearningDomain.Science when age <= 5 => "Na educação infantil, a diferença entre séries está em observar melhor, nomear melhor e registrar com mais intenção.",
        LearningDomain.Science when age <= 8 => "Nos anos iniciais, cada série sobe de observação simples para explicação, experimento e registro mais claro.",
        LearningDomain.Science when age <= 11 => "A série já diferencia hipótese, causa, sistema do corpo e relação entre fenômenos.",
        LearningDomain.Science => "Nos anos finais, a criança precisa usar linguagem científica mais controlada, comparar explicações e defender conclusão com evidência.",
        LearningDomain.History when age <= 5 => "Nesta fase, a criança percebe rotina, memória, família e antes/depois com mais clareza a cada ano.",
        LearningDomain.History when age <= 8 => "Nos primeiros anos, a diferença da série aparece em linha do tempo, fonte simples e leitura de comunidade e território.",
        LearningDomain.History when age <= 11 => "A série já exige contexto, processo histórico, conflito, permanência e mudança com mais consistência.",
        LearningDomain.History => "Nos anos finais, a resposta precisa comparar processos, ler fontes e sustentar interpretação histórica.",
        LearningDomain.Geography when age <= 5 => "Na educação infantil, a série sobe de corpo e casa para trajeto, clima e leitura do entorno.",
        LearningDomain.Geography when age <= 8 => "Nos anos iniciais, a criança sai do espaço próximo e começa a usar mapa, referência, paisagem e região com mais clareza.",
        LearningDomain.Geography when age <= 11 => "A série pede território, ambiente, trabalho humano, cidades e leitura de organização do espaço.",
        LearningDomain.Geography => "Nos anos finais, a resposta precisa relacionar rede, uso do território, urbanização e desigualdade espacial.",
        LearningDomain.ExecutiveFunction when age <= 5 => "Nesta fase, a criança aprende a abrir, fazer e guardar pequenas missões com começo e fim visíveis.",
        LearningDomain.ExecutiveFunction when age <= 8 => "Nos primeiros anos, a série sobe em checklist, espera curta, revisão simples e mais independência para iniciar.",
        LearningDomain.ExecutiveFunction when age <= 11 => "A série pede já algum planejamento, autocorreção e fechamento menos dependente do adulto.",
        LearningDomain.ExecutiveFunction => "Nos anos finais, o foco sobe para priorização, autogestão do tempo, revisão por critério e responsabilidade sobre a própria rotina.",
        _ => "A série cresce em profundidade de linguagem, complexidade de tarefa e independência para concluir."
    };

    private static Guid CreateDeterministicGuid(string value)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }

    private static string Slugify(string value)
    {
        var normalized = value.ToLowerInvariant()
            .Replace("ã", "a")
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("â", "a")
            .Replace("é", "e")
            .Replace("ê", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ô", "o")
            .Replace("õ", "o")
            .Replace("ú", "u")
            .Replace("ç", "c")
            .Replace("º", "");

        var builder = new System.Text.StringBuilder();
        var lastWasDash = false;
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string GetPlacementShortLabel(int age) => age switch
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
            Category = FamilyLibraryTaxonomyNormalizer.NormalizeCategory(category),
            EducationStage = FamilyLibraryTaxonomyNormalizer.NormalizeEducationStage(educationStage),
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
