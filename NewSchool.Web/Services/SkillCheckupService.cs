using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class SkillCheckupService(
    ApplicationDbContext db,
    SkillProgressionService skillProgressionService)
{
    public async Task SyncBiweeklyCheckupsAsync(Guid childId, Guid parentId)
    {
        var today = DateTime.Today;
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.SkillCheckups)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var windowStart = today.AddDays(-13);
        var hasOpenWindow = child.SkillCheckups.Any(x =>
            !x.CompletedAt.HasValue &&
            x.ScheduledFor.Date >= windowStart &&
            x.ScheduledFor.Date <= today.Date);

        if (hasOpenWindow)
        {
            return;
        }

        var dueCandidates = child.SkillProgressEntries
            .Where(x => x.TimesPracticed > 0)
            .OrderByDescending(x => x.NeedsRemediation)
            .ThenBy(x => x.NextReviewAt ?? DateTime.MaxValue)
            .ThenBy(x => x.MasteryScore)
            .ThenBy(x => x.TimesPracticed)
            .Take(3)
            .ToList();

        if (dueCandidates.Count == 0)
        {
            return;
        }

        foreach (var progress in dueCandidates)
        {
            var lastCheckup = child.SkillCheckups
                .Where(x => x.SkillCode == progress.SkillCode)
                .OrderByDescending(x => x.ScheduledFor)
                .FirstOrDefault();

            if (lastCheckup is not null && (today.Date - lastCheckup.ScheduledFor.Date).Days < 14)
            {
                continue;
            }

            var probe = BuildProbe(progress);
            db.ChildSkillCheckups.Add(new ChildSkillCheckup
            {
                ChildId = child.Id,
                Domain = progress.Domain,
                SkillCode = progress.SkillCode,
                SkillName = progress.SkillName,
                PromptTitle = probe.Title,
                ParentPrompt = probe.ParentPrompt,
                SuccessCriteria = probe.SuccessCriteria,
                ScheduledFor = today,
                RecalibratedScore = progress.MasteryScore
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<ChildSkillCheckup>> GetPendingCheckupsAsync(Guid childId, Guid parentId)
    {
        await SyncBiweeklyCheckupsAsync(childId, parentId);

        return await db.ChildSkillCheckups
            .Where(x => x.ChildId == childId && x.Child.ParentId == parentId && !x.CompletedAt.HasValue)
            .OrderBy(x => x.ScheduledFor)
            .ThenBy(x => x.SkillName)
            .ToListAsync();
    }

    public async Task ApplyCheckupsAsync(Guid childId, Guid parentId, IEnumerable<SkillCheckupSubmissionItem> items)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var submitted = items.ToList();
        if (submitted.Count == 0)
        {
            return;
        }

        var checkupIds = submitted.Select(x => x.CheckupId).ToList();
        var checkups = await db.ChildSkillCheckups
            .Where(x => x.ChildId == childId && checkupIds.Contains(x.Id))
            .ToListAsync();

        foreach (var item in submitted)
        {
            var checkup = checkups.FirstOrDefault(x => x.Id == item.CheckupId);
            if (checkup is null || checkup.CompletedAt.HasValue)
            {
                continue;
            }

            var rating = Enum.IsDefined(typeof(SkillFeedbackLevel), item.Rating)
                ? (SkillFeedbackLevel)item.Rating
                : SkillFeedbackLevel.Developing;

            var progress = child.SkillProgressEntries.FirstOrDefault(x => x.SkillCode == checkup.SkillCode);
            if (progress is null)
            {
                progress = new ChildSkillProgress
                {
                    ChildId = childId,
                    Age = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14),
                    Domain = checkup.Domain,
                    SkillCode = checkup.SkillCode,
                    SkillName = checkup.SkillName,
                    MasteryScore = 45
                };
                skillProgressionService.InitializeProgress(progress);
                db.ChildSkillProgressEntries.Add(progress);
                child.SkillProgressEntries.Add(progress);
            }

            skillProgressionService.ApplyDiagnosticCheckup(progress, checkup, rating);
            checkup.Rating = rating;
            checkup.Notes = item.Notes ?? string.Empty;
            checkup.CompletedAt = DateTime.UtcNow;
            checkup.RecalibratedScore = progress.MasteryScore;
        }

        await db.SaveChangesAsync();
    }

    private static (string Title, string ParentPrompt, string SuccessCriteria) BuildProbe(ChildSkillProgress progress)
    {
        return progress.Domain switch
        {
            LearningDomain.Language => (
                $"Checagem quinzenal de {progress.SkillName}",
                $"Peça para a crianca mostrar {progress.SkillName.ToLowerInvariant()} em 2 tentativas curtas, sem transformar em prova longa. Pare depois de 3 minutos.",
                "A crianca consegue responder com menos ajuda, sem chute excessivo e com alguma estabilidade entre as tentativas."),
            LearningDomain.Math => (
                $"Checagem quinzenal de {progress.SkillName}",
                $"Monte um desafio concreto e curto sobre {progress.SkillName.ToLowerInvariant()}. Observe se ela explica o que fez, nao apenas a resposta final.",
                "A crianca representa a ideia com objetos, desenho ou fala e mantém o raciocinio ate o fim."),
            LearningDomain.World => (
                $"Checagem quinzenal de {progress.SkillName}",
                $"Convide a crianca a observar, comparar e explicar algo sobre {progress.SkillName.ToLowerInvariant()} em uma fala curta.",
                "A crianca observa com atencao e consegue registrar ou explicar uma descoberta simples."),
            _ => (
                $"Checagem quinzenal de {progress.SkillName}",
                $"Proponha uma missao curta ligada a {progress.SkillName.ToLowerInvariant()} e veja se ela inicia, sustenta foco e conclui com menos ajuda.",
                "A crianca comeca mais rapido, precisa de menos lembretes e termina a microtarefa.")
        };
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

public class SkillCheckupSubmissionItem
{
    public Guid CheckupId { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
}
