using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;
using NewSchool.Web.Models;

namespace NewSchool.Web.Services;

public class FundamentalAssessmentService(
    ApplicationDbContext db,
    CuratedLearningLibraryService curatedLearningLibraryService,
    SystemCurriculumLibraryService systemCurriculumLibraryService)
{
    private sealed record AssessmentContext(
        ChildProfile Child,
        DailyPlanBlock Block,
        CuratedTaskSuggestionViewModel? Suggestion,
        SystemCurriculumUnitViewModel? CurrentUnit,
        int SchoolYearNumber,
        string SchoolPlacementLabel,
        string SubjectLabel,
        int PhaseNumber,
        string PhaseLabel,
        int QuestionCount,
        string LessonTitle,
        string UnitTitle,
        string Goal,
        string ChildPrompt,
        string Materials,
        string SourceHeadline,
        string SourceText);

    public async Task<Dictionary<Guid, FundamentalAssessmentSummaryViewModel>> BuildLessonSummariesAsync(
        Guid parentId,
        ChildProfile child,
        IReadOnlyCollection<DailyPlanBlock> blocks,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, FundamentalAssessmentSummaryViewModel>();
        if (!IsFundamental(child))
        {
            return result;
        }

        var blockIds = blocks.Select(block => block.Id).ToList();
        var assessments = await db.ChildLessonAssessments
            .AsNoTracking()
            .Where(item => item.ParentUserId == parentId && item.ChildId == child.Id && blockIds.Contains(item.DailyPlanBlockId))
            .ToDictionaryAsync(item => item.DailyPlanBlockId, cancellationToken);

        foreach (var block in blocks)
        {
            var schoolYearNumber = GetSchoolYearNumber(CalculateAge(child.BirthDate, DateTime.Today));
            var questionCount = GetQuestionCount(schoolYearNumber);
            var subjectLabel = CurriculumStructure.FormatDomainLabel(block.Domain);
            assessments.TryGetValue(block.Id, out var assessment);

            result[block.Id] = new FundamentalAssessmentSummaryViewModel
            {
                IsAvailable = true,
                IsPrinted = assessment?.PrintedAtUtc is not null,
                IsCorrected = assessment?.IsCompleted == true,
                Headline = $"Teste de {subjectLabel.ToLowerInvariant()}",
                Summary = schoolYearNumber <= 4
                    ? "Depois da explicação da matéria, aplique um teste curto de até 10 perguntas no papel e corrija aqui."
                    : "Depois da explicação da matéria, aplique um teste de até 20 perguntas no papel e lance a nota aqui.",
                StatusLabel = assessment?.IsCompleted == true
                    ? $"Corrigido • {FormatScoreLabel(assessment.ScoreValue, assessment.CorrectCount, assessment.QuestionCount)}"
                    : assessment?.PrintedAtUtc is not null
                        ? "Pronto para corrigir"
                        : "Pronto para imprimir",
                QuestionCountLabel = $"{questionCount} pergunta(s)",
                PrintableUrl = url.Action("LessonAssessmentPrintable", "Parent", new { childId = child.Id, blockId = block.Id }) ?? string.Empty,
                CorrectionUrl = url.Action("LessonAssessmentCorrection", "Parent", new { childId = child.Id, blockId = block.Id }) ?? string.Empty,
                ReinforcementUrl = url.Action("Reinforcement", "Parent", new { id = child.Id }) ?? string.Empty,
                ScoreLabel = assessment?.IsCompleted == true
                    ? FormatScoreLabel(assessment.ScoreValue, assessment.CorrectCount, assessment.QuestionCount)
                    : string.Empty
            };
        }

        return result;
    }

    public async Task<FundamentalAssessmentPrintableViewModel?> BuildPrintableAsync(
        Guid parentId,
        Guid childId,
        Guid blockId,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var child = await db.Children
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == childId && item.ParentId == parentId, cancellationToken);
        if (child is null || !IsFundamental(child))
        {
            return null;
        }

        var block = await db.DailyPlanBlocks
            .Include(item => item.DailyPlan)
            .FirstOrDefaultAsync(item => item.Id == blockId && item.DailyPlan.ChildId == childId, cancellationToken);
        if (block is null)
        {
            return null;
        }

        var context = await BuildContextAsync(child, block, cancellationToken);
        var assessment = await EnsureAssessmentAsync(parentId, context, markPrinted: true, cancellationToken);

        return new FundamentalAssessmentPrintableViewModel
        {
            ChildId = child.Id,
            DailyPlanBlockId = block.Id,
            AssessmentId = assessment.Id,
            ChildName = child.FullName,
            SchoolPlacementLabel = context.SchoolPlacementLabel,
            SubjectLabel = context.SubjectLabel,
            LessonTitle = context.LessonTitle,
            UnitTitle = context.UnitTitle,
            AssessmentTitle = assessment.AssessmentTitle,
            PrintableHeadline = assessment.PrintableHeadline,
            PrintableSummary = assessment.PrintableSummary,
            QuestionCount = assessment.QuestionCount,
            PrintUrl = url.Action("LessonAssessmentPrintable", "Parent", new { childId = child.Id, blockId = block.Id }) ?? string.Empty,
            CorrectionUrl = url.Action("LessonAssessmentCorrection", "Parent", new { childId = child.Id, blockId = block.Id }) ?? string.Empty,
            ChildUrl = url.Action("Child", "Parent", new { id = child.Id }) ?? string.Empty,
            Questions = assessment.Items
                .OrderBy(item => item.SortOrder)
                .Select(item => new FundamentalAssessmentQuestionViewModel
                {
                    ItemId = item.Id,
                    SortOrder = item.SortOrder,
                    Prompt = item.Prompt,
                    ExpectedAnswer = item.ExpectedAnswer,
                    TeacherNote = item.TeacherNote,
                    IsCorrect = item.IsCorrect
                })
                .ToList()
        };
    }

    public async Task<FundamentalAssessmentCorrectionViewModel?> BuildCorrectionAsync(
        Guid parentId,
        Guid childId,
        Guid blockId,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var child = await db.Children
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == childId && item.ParentId == parentId, cancellationToken);
        if (child is null || !IsFundamental(child))
        {
            return null;
        }

        var block = await db.DailyPlanBlocks
            .Include(item => item.DailyPlan)
            .FirstOrDefaultAsync(item => item.Id == blockId && item.DailyPlan.ChildId == childId, cancellationToken);
        if (block is null)
        {
            return null;
        }

        var context = await BuildContextAsync(child, block, cancellationToken);
        var assessment = await EnsureAssessmentAsync(parentId, context, markPrinted: false, cancellationToken);

        return new FundamentalAssessmentCorrectionViewModel
        {
            AssessmentId = assessment.Id,
            ChildId = child.Id,
            DailyPlanBlockId = block.Id,
            ChildName = child.FullName,
            SchoolPlacementLabel = context.SchoolPlacementLabel,
            SubjectLabel = context.SubjectLabel,
            LessonTitle = context.LessonTitle,
            AssessmentTitle = assessment.AssessmentTitle,
            Summary = assessment.IsCompleted
                ? $"Prova corrigida. {FormatScoreLabel(assessment.ScoreValue, assessment.CorrectCount, assessment.QuestionCount)}."
                : "Marque cada item como acertou ou errou e salve a nota no final.",
            SaveUrl = url.Action("SaveLessonAssessmentCorrection", "Parent") ?? string.Empty,
            PrintableUrl = url.Action("LessonAssessmentPrintable", "Parent", new { childId = child.Id, blockId = block.Id }) ?? string.Empty,
            ChildUrl = url.Action("Child", "Parent", new { id = child.Id }) ?? string.Empty,
            IsCompleted = assessment.IsCompleted,
            ScoreLabel = assessment.IsCompleted
                ? FormatScoreLabel(assessment.ScoreValue, assessment.CorrectCount, assessment.QuestionCount)
                : string.Empty,
            Questions = assessment.Items
                .OrderBy(item => item.SortOrder)
                .Select(item => new FundamentalAssessmentQuestionViewModel
                {
                    ItemId = item.Id,
                    SortOrder = item.SortOrder,
                    Prompt = item.Prompt,
                    ExpectedAnswer = item.ExpectedAnswer,
                    TeacherNote = item.TeacherNote,
                    IsCorrect = item.IsCorrect
                })
                .ToList()
        };
    }

    public async Task SaveCorrectionAsync(
        Guid parentId,
        SaveFundamentalAssessmentCorrectionViewModel model,
        CancellationToken cancellationToken = default)
    {
        var assessment = await db.ChildLessonAssessments
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == model.AssessmentId && item.ParentUserId == parentId && item.ChildId == model.ChildId, cancellationToken);
        if (assessment is null)
        {
            throw new InvalidOperationException("Não foi possível encontrar esse teste para salvar a correção.");
        }

        var answersByItem = model.Items.ToDictionary(item => item.ItemId, item => item.IsCorrect);
        foreach (var item in assessment.Items)
        {
            if (!answersByItem.TryGetValue(item.Id, out var isCorrect) || !isCorrect.HasValue)
            {
                throw new InvalidOperationException("Marque todas as perguntas como acertou ou errou antes de salvar.");
            }

            item.IsCorrect = isCorrect.Value;
        }

        assessment.CorrectCount = assessment.Items.Count(item => item.IsCorrect == true);
        assessment.ScorePercent = assessment.QuestionCount == 0
            ? 0
            : (int)Math.Round(assessment.CorrectCount * 100m / assessment.QuestionCount, MidpointRounding.AwayFromZero);
        assessment.ScoreValue = assessment.QuestionCount == 0
            ? 0m
            : Math.Round(assessment.CorrectCount * 10m / assessment.QuestionCount, 1, MidpointRounding.AwayFromZero);
        assessment.IsCompleted = true;
        assessment.CorrectedAtUtc = DateTime.UtcNow;
        assessment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChildReinforcementViewModel?> BuildReinforcementAsync(
        Guid parentId,
        Guid childId,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var child = await db.Children
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == childId && item.ParentId == parentId, cancellationToken);
        if (child is null || !IsFundamental(child))
        {
            return null;
        }

        var subjectAverages = await BuildSubjectAveragesAsync(childId, parentId, cancellationToken);
        var lowSubjects = subjectAverages
            .Where(item => item.AverageScoreValue < 7m)
            .OrderBy(item => item.AverageScoreValue)
            .ToList();

        return new ChildReinforcementViewModel
        {
            ChildId = child.Id,
            ChildName = child.FullName,
            SchoolPlacementLabel = CurriculumStructure.GetSchoolPlacementLabel(CalculateAge(child.BirthDate, DateTime.Today)),
            Headline = "Reforço por matéria",
            Summary = lowSubjects.Count == 0
                ? "Nenhuma matéria está pedindo reforço agora. Continue acompanhando as provas já corrigidas."
                : "As matérias abaixo estão com média mais baixa e pedem retomada curta antes de seguir normalmente.",
            ChildUrl = url.Action("Child", "Parent", new { id = child.Id }) ?? string.Empty,
            SchoolReportUrl = url.Action("SchoolReport", "Parent", new { id = child.Id }) ?? string.Empty,
            Subjects = lowSubjects
                .Select(item => new ReinforcementSubjectViewModel
                {
                    SubjectLabel = item.SubjectLabel,
                    SubjectChipClass = CurriculumStructure.GetDomainChipClass(item.Domain),
                    AverageScoreValue = item.AverageScoreValue,
                    AverageScoreLabel = item.AverageScoreLabel,
                    AssessmentCount = item.AssessmentCount,
                    ReinforcementSummary = BuildReinforcementSummary(item.Domain, item.SubjectLabel),
                    NextAction = BuildReinforcementAction(item.Domain)
                })
                .ToList()
        };
    }

    public async Task<List<SubjectAssessmentAverageViewModel>> BuildSubjectAveragesAsync(
        Guid childId,
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var completed = await db.ChildLessonAssessments
            .AsNoTracking()
            .Where(item => item.ParentUserId == parentId && item.ChildId == childId && item.IsCompleted)
            .ToListAsync(cancellationToken);

        return completed
            .GroupBy(item => item.Domain)
            .Select(group =>
            {
                var average = group.Average(item => item.ScoreValue);
                return new SubjectAssessmentAverageViewModel
                {
                    Domain = group.Key,
                    SubjectLabel = CurriculumStructure.FormatDomainLabel(group.Key),
                    AverageScoreValue = Math.Round((decimal)average, 1, MidpointRounding.AwayFromZero),
                    AverageScoreLabel = $"{Math.Round((decimal)average, 1, MidpointRounding.AwayFromZero):0.0}",
                    AssessmentCount = group.Count(),
                    NeedsReinforcement = average < 7m
                };
            })
            .OrderBy(item => item.SubjectLabel)
            .ToList();
    }

    private async Task<AssessmentContext> BuildContextAsync(
        ChildProfile child,
        DailyPlanBlock block,
        CancellationToken cancellationToken)
    {
        var schoolYearNumber = GetSchoolYearNumber(CalculateAge(child.BirthDate, DateTime.Today));
        var questionCount = GetQuestionCount(schoolYearNumber);
        var subjectLabel = CurriculumStructure.FormatDomainLabel(block.Domain);
        var systemTracks = await systemCurriculumLibraryService.BuildAsync(child);
        var suggestions = await curatedLearningLibraryService.BuildBlockSuggestionsAsync(child, [block]);
        var suggestion = suggestions.TryGetValue(block.Id, out var task) ? task : null;
        var currentTrack = systemTracks.FirstOrDefault(track => string.Equals(track.DomainLabel, subjectLabel, StringComparison.OrdinalIgnoreCase));
        var currentUnit = currentTrack?.CurrentUnit;
        var lessonTitle = suggestion?.CurriculumLessonTitle
                          ?? suggestion?.LessonPacket?.UnitTitle
                          ?? suggestion?.Title
                          ?? currentUnit?.TaskTitle
                          ?? block.Title;
        var unitTitle = suggestion?.LessonPacket?.UnitTitle
                        ?? currentUnit?.Title
                        ?? block.SkillName;
        var goal = suggestion?.LessonPacket?.PracticeTask
                   ?? currentUnit?.TaskPrompt
                   ?? suggestion?.Goal
                   ?? block.Goal;
        var childPrompt = suggestion?.LessonPacket?.AnchorQuestion
                          ?? suggestion?.ChildPrompt
                          ?? block.ChildPrompt;
        var materials = suggestion?.MaterialsSummary
                        ?? block.Materials;
        var sourceHeadline = suggestion?.LessonPacket?.CoreMaterialTitle
                             ?? currentUnit?.TaskTitle
                             ?? block.Title;
        var sourceText = suggestion?.LessonPacket?.CoreMaterialParagraphs.Count > 0 == true
            ? string.Join(" ", suggestion.LessonPacket.CoreMaterialParagraphs.Take(2))
            : currentUnit?.Summary
              ?? suggestion?.Goal
              ?? block.Goal;

        return new AssessmentContext(
            child,
            block,
            suggestion,
            currentUnit,
            schoolYearNumber,
            CurriculumStructure.GetSchoolPlacementLabel(CalculateAge(child.BirthDate, DateTime.Today)),
            subjectLabel,
            ResolvePhaseNumber(block.DailyPlan.PlannedDate),
            $"{ResolvePhaseNumber(block.DailyPlan.PlannedDate)}ª etapa",
            questionCount,
            lessonTitle,
            unitTitle,
            goal,
            childPrompt,
            materials,
            sourceHeadline,
            sourceText);
    }

    private async Task<ChildLessonAssessment> EnsureAssessmentAsync(
        Guid parentId,
        AssessmentContext context,
        bool markPrinted,
        CancellationToken cancellationToken)
    {
        var assessment = await db.ChildLessonAssessments
            .Include(item => item.Items.OrderBy(question => question.SortOrder))
            .FirstOrDefaultAsync(
                item => item.ParentUserId == parentId &&
                        item.ChildId == context.Child.Id &&
                        item.DailyPlanBlockId == context.Block.Id,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (assessment is null)
        {
            assessment = new ChildLessonAssessment
            {
                ChildId = context.Child.Id,
                ParentUserId = parentId,
                DailyPlanId = context.Block.DailyPlanId,
                DailyPlanBlockId = context.Block.Id,
                Domain = context.Block.Domain,
                SchoolYearNumber = context.SchoolYearNumber,
                PhaseNumber = context.PhaseNumber,
                PhaseLabel = context.PhaseLabel,
                SubjectLabel = context.SubjectLabel,
                LessonTitle = context.LessonTitle,
                UnitTitle = context.UnitTitle,
                AssessmentTitle = BuildAssessmentTitle(context),
                PrintableHeadline = BuildPrintableHeadline(context),
                PrintableSummary = BuildPrintableSummary(context),
                QuestionCount = context.QuestionCount,
                PrintedAtUtc = markPrinted ? now : null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                Items = GenerateQuestions(context)
                    .Select((question, index) => new ChildLessonAssessmentItem
                    {
                        SortOrder = index + 1,
                        Prompt = question.Prompt,
                        ExpectedAnswer = question.ExpectedAnswer,
                        TeacherNote = question.TeacherNote
                    })
                    .ToList()
            };

            db.ChildLessonAssessments.Add(assessment);
            await db.SaveChangesAsync(cancellationToken);
            return assessment;
        }

        assessment.Domain = context.Block.Domain;
        assessment.SchoolYearNumber = context.SchoolYearNumber;
        assessment.PhaseNumber = context.PhaseNumber;
        assessment.PhaseLabel = context.PhaseLabel;
        assessment.SubjectLabel = context.SubjectLabel;
        assessment.LessonTitle = context.LessonTitle;
        assessment.UnitTitle = context.UnitTitle;
        assessment.AssessmentTitle = BuildAssessmentTitle(context);
        assessment.PrintableHeadline = BuildPrintableHeadline(context);
        assessment.PrintableSummary = BuildPrintableSummary(context);
        assessment.QuestionCount = context.QuestionCount;
        if (markPrinted && assessment.PrintedAtUtc is null)
        {
            assessment.PrintedAtUtc = now;
        }

        if (assessment.Items.Count != context.QuestionCount)
        {
            db.ChildLessonAssessmentItems.RemoveRange(assessment.Items);
            assessment.Items = GenerateQuestions(context)
                .Select((question, index) => new ChildLessonAssessmentItem
                {
                    SortOrder = index + 1,
                    Prompt = question.Prompt,
                    ExpectedAnswer = question.ExpectedAnswer,
                    TeacherNote = question.TeacherNote
                })
                .ToList();
        }

        assessment.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return assessment;
    }

    private static List<(string Prompt, string ExpectedAnswer, string TeacherNote)> GenerateQuestions(AssessmentContext context)
    {
        var isUpper = context.SchoolYearNumber >= 5;
        var questions = new List<(string Prompt, string ExpectedAnswer, string TeacherNote)>();
        for (var index = 1; index <= context.QuestionCount; index++)
        {
            questions.Add(BuildQuestion(context, index, isUpper));
        }

        return questions;
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildQuestion(AssessmentContext context, int index, bool isUpper)
    {
        return context.Block.Domain switch
        {
            LearningDomain.Language => BuildLanguageQuestion(context, index, isUpper),
            LearningDomain.Math => BuildMathQuestion(context, index, isUpper),
            LearningDomain.Science => BuildScienceQuestion(context, index, isUpper),
            LearningDomain.History => BuildHistoryQuestion(context, index, isUpper),
            LearningDomain.Geography => BuildGeographyQuestion(context, index, isUpper),
            _ => BuildAutonomyQuestion(context, index, isUpper)
        };
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildLanguageQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? new[]
            {
                $"Resuma a ideia central de \"{context.SourceHeadline}\" em duas ou três frases.",
                $"Explique como a lição \"{context.LessonTitle}\" organiza as informações principais.",
                $"Aponte uma evidência do texto-base que sustenta a resposta sobre {context.UnitTitle.ToLowerInvariant()}.",
                $"Aponte uma segunda evidência diferente da anterior para sustentar a mesma resposta.",
                $"Explique o sentido de uma palavra-chave desta aula e por que ela importa para {context.UnitTitle.ToLowerInvariant()}.",
                $"Compare duas partes da leitura e diga o que mudou entre elas.",
                $"Escreva uma resposta completa para a pergunta: {context.ChildPrompt}",
                $"Explique qual seria um erro de leitura comum nesta atividade e como evitá-lo.",
                $"Mostre como a aula de hoje se liga à unidade \"{context.UnitTitle}\".",
                $"Reescreva a ideia principal usando palavras próprias sem copiar o texto-base.",
                $"Separe fato principal e detalhe de apoio em \"{context.SourceHeadline}\".",
                $"Explique como o autor ou narrador construiu a informação mais importante.",
                $"Indique um trecho que mereceria releitura e explique por quê.",
                $"Produza um parágrafo curto com começo, meio e fim sobre a leitura de hoje.",
                $"Mostre uma inferência possível a partir do texto-base da aula.",
                $"Explique qual foi o ponto de vista ou intenção mais forte do texto trabalhado.",
                $"Construa uma pergunta nova que faria sentido depois desta leitura.",
                $"Responda à pergunta criada usando uma informação da própria aula.",
                $"Faça uma síntese final de \"{context.LessonTitle}\" com linguagem clara e objetiva.",
                $"Revise sua síntese e mostre uma melhoria feita antes de entregar."
            }
            : new[]
            {
                $"Diga com suas palavras qual foi o assunto principal de \"{context.SourceHeadline}\".",
                $"Conte o que precisava ser feito primeiro em \"{context.LessonTitle}\".",
                $"Mostre uma palavra ou frase importante trabalhada hoje.",
                $"Diga uma informação que ajuda a entender {context.UnitTitle.ToLowerInvariant()}.",
                $"Responda com frase curta à pergunta: {context.ChildPrompt}",
                $"Organize dois momentos da atividade na ordem certa.",
                $"Explique como você descobriu a resposta mais importante da aula.",
                $"Releia sua resposta e corrija uma parte para deixá-la mais clara.",
                $"Conte para outra pessoa o que aprendeu hoje em linguagem.",
                $"Faça uma resposta final curta mostrando {context.Goal.ToLowerInvariant()}."
            };

        var expectedAnswers = isUpper
            ? new[]
            {
                $"Síntese fiel à leitura-base, sem perder a ideia central de {context.UnitTitle.ToLowerInvariant()}.",
                $"Resposta que mostre organização do texto, da fala ou da escrita pedida na lição.",
                $"Primeira evidência coerente com o texto-base da aula.",
                $"Segunda evidência coerente e diferente da primeira.",
                $"Explicação correta da palavra-chave dentro do contexto da unidade.",
                $"Comparação que faça sentido com o texto ou com a atividade aplicada.",
                $"Resposta completa, ligada à pergunta-âncora da lição.",
                $"Identificação de erro plausível e estratégia concreta de revisão.",
                $"Ligação clara entre a aula de hoje e a unidade atual.",
                $"Reescrita com palavras próprias e preservação do sentido principal.",
                $"Separação correta entre fato principal e detalhe de apoio.",
                $"Explicação de construção de sentido com base na leitura ou narrativa.",
                $"Trecho escolhido com justificativa coerente.",
                $"Parágrafo organizado e compreensível.",
                $"Inferência possível a partir das pistas do texto.",
                $"Leitura do ponto de vista ou intenção do texto com boa justificativa.",
                $"Pergunta nova coerente com a leitura feita.",
                $"Resposta coerente para a pergunta criada.",
                $"Síntese final objetiva e conectada à lição.",
                $"Revisão real da própria resposta antes da entrega."
            }
            : new[]
            {
                $"Resposta curta que mostre o assunto principal da leitura ou atividade.",
                $"Registro do primeiro passo da tarefa em ordem correta.",
                $"Identificação de palavra, frase ou pista central da aula.",
                $"Uma informação que ajude a mostrar {context.UnitTitle.ToLowerInvariant()}.",
                $"Frase curta ou resposta oral coerente com a pergunta da aula.",
                $"Dois momentos da atividade em sequência lógica.",
                $"Explicação simples de como a resposta foi encontrada.",
                $"Correção ou melhoria curta na própria resposta.",
                $"Reconto simples do que foi aprendido hoje.",
                $"Entrega final curta, clara e ligada ao objetivo da lição."
            };

        return (prompts[index - 1], expectedAnswers[index - 1], "Aceite resposta oral, escrita curta ou registro guiado, desde que mostre compreensão real.");
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildMathQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? new[]
            {
                $"Explique qual problema matemático a aula \"{context.LessonTitle}\" estava resolvendo.",
                $"Mostre a estratégia principal usada hoje em {context.UnitTitle.ToLowerInvariant()}.",
                $"Resolva um exemplo semelhante ao da aula e registre as etapas.",
                $"Mostre uma estimativa antes de calcular o resultado final.",
                $"Explique por que a estratégia usada faz sentido para este tipo de problema.",
                $"Escreva uma conta, tabela ou expressão ligada à aula de hoje.",
                $"Compare duas formas possíveis de resolver a mesma situação.",
                $"Aponte um erro comum nesta matéria e corrija-o.",
                $"Explique o resultado com palavras, não só com números.",
                $"Mostre como conferir a resposta final.",
                $"Crie um novo exemplo do mesmo tipo trabalhado hoje.",
                $"Resolva o exemplo criado mantendo a mesma lógica da unidade.",
                $"Mostre onde aparece medida, quantidade ou relação matemática nesta lição.",
                $"Explique o papel de cada dado usado na resolução.",
                $"Escolha a informação principal do problema e justifique sua escolha.",
                $"Mostre o que muda se um dos dados do problema for alterado.",
                $"Explique quando a estratégia de hoje não seria a melhor escolha.",
                $"Reescreva a solução de forma mais clara e organizada.",
                $"Faça uma síntese final do procedimento matemático aprendido.",
                $"Revise a resposta e mostre a última conferência antes de entregar."
            }
            : new[]
            {
                $"Explique com suas palavras qual conta ou desafio apareceu hoje.",
                $"Mostre a estratégia usada para resolver a atividade.",
                $"Resolva um exemplo parecido com o da aula.",
                $"Escreva ou desenhe o resultado final da atividade.",
                $"Explique como você sabe que a resposta faz sentido.",
                $"Mostre uma conta, agrupamento ou desenho que ajude na solução.",
                $"Encontre um erro possível e mostre a correção.",
                $"Conte a ordem dos passos usados para chegar à resposta.",
                $"Explique a resposta para outra pessoa de forma simples.",
                $"Faça uma resposta final organizada e legível."
            };

        var answers = isUpper
            ? new[]
            {
                $"Leitura correta do problema matemático central da lição.",
                $"Estratégia principal registrada com clareza.",
                $"Exemplo resolvido com procedimento coerente.",
                $"Estimativa plausível antes do cálculo.",
                $"Justificativa de por que a estratégia funciona.",
                $"Registro matemático compatível com a atividade.",
                $"Comparação entre duas soluções possíveis.",
                $"Erro plausível identificado e corrigido.",
                $"Explicação verbal do resultado final.",
                $"Conferência correta da resposta.",
                $"Novo exemplo coerente com o conteúdo da unidade.",
                $"Resolução correta do exemplo criado.",
                $"Reconhecimento da ideia matemática presente na lição.",
                $"Uso correto dos dados do problema.",
                $"Escolha da informação principal com justificativa.",
                $"Explicação do impacto de alterar um dado.",
                $"Identificação de limite ou escolha inadequada de estratégia.",
                $"Resolução reescrita com organização melhor.",
                $"Síntese do procedimento aprendido.",
                $"Revisão final real antes de entregar."
            }
            : new[]
            {
                $"Descrição simples do desafio matemático da aula.",
                $"Estratégia ou material usado de forma coerente.",
                $"Exemplo parecido resolvido corretamente.",
                $"Resultado final claro.",
                $"Justificativa simples de que a resposta faz sentido.",
                $"Conta, desenho ou agrupamento que apoie a solução.",
                $"Erro e correção mostrados com clareza.",
                $"Passos em ordem lógica.",
                $"Explicação simples da resposta.",
                $"Entrega final organizada."
            };

        return (prompts[index - 1], answers[index - 1], "Aceite cálculo, desenho, conta armada ou explicação oral, desde que revelem a estratégia correta.");
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildScienceQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? BuildKnowledgeUpperPrompts(context, "ciências", "fenômeno, conceito ou observação")
            : BuildKnowledgeLowerPrompts(context, "ciências", "observação");
        var answers = isUpper
            ? BuildKnowledgeUpperAnswers("ciências")
            : BuildKnowledgeLowerAnswers("ciências");
        return (prompts[index - 1], answers[index - 1], "Aceite resposta curta, esquema, desenho ou registro científico simples.");
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildHistoryQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? BuildKnowledgeUpperPrompts(context, "história", "fato, personagem ou processo histórico")
            : BuildKnowledgeLowerPrompts(context, "história", "fato ou personagem");
        var answers = isUpper
            ? BuildKnowledgeUpperAnswers("história")
            : BuildKnowledgeLowerAnswers("história");
        return (prompts[index - 1], answers[index - 1], "Aceite linha do tempo simples, frase curta, oralidade ou comparação histórica básica.");
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildGeographyQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? BuildKnowledgeUpperPrompts(context, "geografia", "lugar, mapa, território ou paisagem")
            : BuildKnowledgeLowerPrompts(context, "geografia", "lugar ou paisagem");
        var answers = isUpper
            ? BuildKnowledgeUpperAnswers("geografia")
            : BuildKnowledgeLowerAnswers("geografia");
        return (prompts[index - 1], answers[index - 1], "Aceite mapa simples, localização verbal, desenho ou descrição curta.");
    }

    private static (string Prompt, string ExpectedAnswer, string TeacherNote) BuildAutonomyQuestion(AssessmentContext context, int index, bool isUpper)
    {
        var prompts = isUpper
            ? BuildKnowledgeUpperPrompts(context, "autonomia", "rotina, organização ou revisão")
            : BuildKnowledgeLowerPrompts(context, "autonomia", "rotina ou organização");
        var answers = isUpper
            ? BuildKnowledgeUpperAnswers("autonomia")
            : BuildKnowledgeLowerAnswers("autonomia");
        return (prompts[index - 1], answers[index - 1], "Aceite checklist, relato curto, organização no papel ou explicação oral com passos claros.");
    }

    private static string[] BuildKnowledgeLowerPrompts(AssessmentContext context, string subject, string focusWord)
    {
        return
        [
            $"Conte com suas palavras qual foi o assunto principal da aula de {subject}.",
            $"Mostre um {focusWord} importante trabalhado hoje.",
            $"Explique o que precisou ser observado primeiro em \"{context.LessonTitle}\".",
            $"Responda à pergunta da aula com frase curta: {context.ChildPrompt}",
            $"Diga uma informação que ajuda a entender {context.UnitTitle.ToLowerInvariant()}.",
            $"Organize dois passos da atividade na ordem certa.",
            $"Mostre um exemplo simples ligado ao conteúdo de hoje.",
            $"Explique o que você aprendeu e por que isso importa.",
            $"Revise a resposta e melhore uma parte dela.",
            $"Faça um fechamento curto mostrando o essencial da aula."
        ];
    }

    private static string[] BuildKnowledgeUpperPrompts(AssessmentContext context, string subject, string focusWord)
    {
        return
        [
            $"Resuma o foco principal da aula de {subject} em duas ou três frases.",
            $"Explique o conceito central de {context.UnitTitle.ToLowerInvariant()}.",
            $"Mostre uma evidência da aula que ajude a entender esse {focusWord}.",
            $"Mostre uma segunda evidência diferente da anterior.",
            $"Explique uma palavra, dado ou ideia-chave usada hoje.",
            $"Compare duas informações trabalhadas na lição.",
            $"Explique causa e consequência dentro do conteúdo de hoje.",
            $"Aplique a ideia principal a uma situação nova.",
            $"Aponte um erro comum que alguém poderia cometer nessa matéria.",
            $"Mostre como revisar esse erro.",
            $"Construa uma resposta completa para a pergunta da aula: {context.ChildPrompt}",
            $"Relacione a atividade de hoje com a unidade \"{context.UnitTitle}\".",
            $"Separe informação principal e informação secundária.",
            $"Reescreva a explicação da aula de forma mais clara.",
            $"Crie uma pergunta nova sobre o tema e responda em seguida.",
            $"Explique o que mudaria se uma condição do tema fosse alterada.",
            $"Mostre uma justificativa para a resposta que você deu.",
            $"Faça uma síntese final do que foi aprendido hoje.",
            $"Revise essa síntese e registre uma melhoria feita.",
            $"Feche a prova mostrando o que ficou mais importante neste conteúdo."
        ];
    }

    private static string[] BuildKnowledgeLowerAnswers(string subject)
    {
        return
        [
            $"Resumo simples do assunto principal da aula de {subject}.",
            $"Identificação de um elemento importante do conteúdo trabalhado.",
            "Primeira observação ou passo registrado corretamente.",
            "Resposta curta coerente com a pergunta da aula.",
            "Uma informação correta que ajude a entender o conteúdo.",
            "Dois passos em sequência lógica.",
            "Exemplo simples ligado ao tema do dia.",
            "Explicação curta do aprendizado e da sua importância.",
            "Revisão ou melhoria curta da própria resposta.",
            "Fechamento objetivo mostrando o essencial."
        ];
    }

    private static string[] BuildKnowledgeUpperAnswers(string subject)
    {
        return
        [
            $"Síntese coerente do foco principal da aula de {subject}.",
            "Explicação correta do conceito central.",
            "Primeira evidência pertinente ao conteúdo.",
            "Segunda evidência pertinente e diferente da primeira.",
            "Explicação adequada de palavra, dado ou ideia-chave.",
            "Comparação coerente entre duas informações.",
            "Relação correta de causa e consequência.",
            "Aplicação plausível da ideia a situação nova.",
            "Erro comum identificado de modo realista.",
            "Revisão adequada desse erro.",
            "Resposta completa para a pergunta principal da aula.",
            "Ligação clara entre a lição e a unidade do ano.",
            "Separação correta entre ideia principal e secundária.",
            "Explicação reescrita com mais clareza.",
            "Pergunta nova coerente com resposta correspondente.",
            "Percepção correta do que mudaria com nova condição.",
            "Justificativa adequada para a resposta final.",
            "Síntese final bem organizada.",
            "Melhoria real após revisão.",
            "Fechamento que destaque o mais importante do conteúdo."
        ];
    }

    private static string BuildAssessmentTitle(AssessmentContext context)
    {
        return $"Teste de {context.SubjectLabel} • {context.LessonTitle}";
    }

    private static string BuildPrintableHeadline(AssessmentContext context)
    {
        return context.SchoolYearNumber <= 4
            ? $"Teste curto de {context.SubjectLabel.ToLowerInvariant()} para aplicar depois da aula"
            : $"Teste completo de {context.SubjectLabel.ToLowerInvariant()} para registrar a nota da aula";
    }

    private static string BuildPrintableSummary(AssessmentContext context)
    {
        return $"A prova usa a matéria \"{context.LessonTitle}\" da unidade \"{context.UnitTitle}\". Aplique no papel, corrija aqui e guarde a nota no histórico escolar da criança.";
    }

    private static string BuildReinforcementSummary(LearningDomain domain, string subjectLabel)
    {
        return domain switch
        {
            LearningDomain.Language => "Retome leitura guiada, resposta curta e revisão de ideias principais antes de avançar.",
            LearningDomain.Math => "Retome estratégia, cálculo e explicação do procedimento com um exemplo menor antes de seguir.",
            LearningDomain.Science => "Volte à observação central, ao conceito-chave e a uma explicação curta com evidência.",
            LearningDomain.History => "Retome fatos, ordem dos acontecimentos e explicação de causa e consequência.",
            LearningDomain.Geography => "Retome localização, paisagem, mapa ou relação entre lugar e fenômeno trabalhado.",
            _ => $"Retome a base de {subjectLabel.ToLowerInvariant()} com prática curta e revisão guiada."
        };
    }

    private static string BuildReinforcementAction(LearningDomain domain)
    {
        return domain switch
        {
            LearningDomain.Language => "Abra a próxima aula de linguagem e refaça com leitura mais curta e resposta guiada.",
            LearningDomain.Math => "Abra a próxima aula de matemática e trabalhe um exemplo concreto antes do teste seguinte.",
            LearningDomain.Science => "Abra a próxima aula de ciências e peça observação + explicação curta com prova.",
            LearningDomain.History => "Abra a próxima aula de história e reforce a ordem dos fatos e o sentido do tema.",
            LearningDomain.Geography => "Abra a próxima aula de geografia e reforce mapa, paisagem ou localização com apoio visual.",
            _ => "Abra a próxima aula da matéria e retome o ponto principal com apoio menor."
        };
    }

    private static string FormatScoreLabel(decimal scoreValue, int correctCount, int questionCount)
    {
        return $"Nota {scoreValue:0.0} • {correctCount}/{questionCount} acertos";
    }

    private static bool IsFundamental(ChildProfile child)
    {
        var age = CalculateAge(child.BirthDate, DateTime.Today);
        return age >= 6 && age <= 14;
    }

    private static int GetSchoolYearNumber(int age)
    {
        return Math.Clamp(age - 5, 1, 9);
    }

    private static int GetQuestionCount(int schoolYearNumber)
    {
        return schoolYearNumber <= 4 ? 10 : 20;
    }

    private static int ResolvePhaseNumber(DateTime planDate)
    {
        return Math.Clamp(((planDate.Month - 1) / 3) + 1, 1, 4);
    }

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}

public class SubjectAssessmentAverageViewModel
{
    public LearningDomain Domain { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;
    public decimal AverageScoreValue { get; set; }
    public string AverageScoreLabel { get; set; } = string.Empty;
    public int AssessmentCount { get; set; }
    public bool NeedsReinforcement { get; set; }
}
