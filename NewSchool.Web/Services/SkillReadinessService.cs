using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Data;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public class SkillReadinessService(
    ApplicationDbContext db,
    SkillProgressionService skillProgressionService)
{
    public async Task SyncReadinessChecksAsync(Guid childId, Guid parentId)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .Include(x => x.SkillReadinessChecks)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var age = Math.Clamp(CalculateAge(child.BirthDate, DateTime.Today), 3, 14);
        var templates = await db.CurriculumTemplates
            .Where(x => x.Age == age && !string.IsNullOrWhiteSpace(x.PrerequisiteSkillCode))
            .ToListAsync();

        var unlockMap = templates
            .GroupBy(x => x.PrerequisiteSkillCode)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.SortOrder).First());

        foreach (var progress in child.SkillProgressEntries
                     .Where(x => x.ReadyToAdvance && x.NeedsReadinessCheck))
        {
            if (!unlockMap.TryGetValue(progress.SkillCode, out var nextTemplate))
            {
                continue;
            }

            var openCheck = child.SkillReadinessChecks.Any(x =>
                x.SkillCode == progress.SkillCode &&
                !x.CompletedAt.HasValue);

            if (openCheck)
            {
                continue;
            }

            var recentCheck = child.SkillReadinessChecks
                .Where(x => x.SkillCode == progress.SkillCode)
                .OrderByDescending(x => x.ScheduledFor)
                .FirstOrDefault();

            if (recentCheck is not null && (DateTime.Today - recentCheck.ScheduledFor.Date).Days < 7)
            {
                continue;
            }

            var probe = BuildProbe(progress, nextTemplate);
            db.ChildSkillReadinessChecks.Add(new ChildSkillReadinessCheck
            {
                ChildId = child.Id,
                Domain = progress.Domain,
                SkillCode = progress.SkillCode,
                SkillName = progress.SkillName,
                Headline = probe.Headline,
                ParentPrompt = probe.ParentPrompt,
                SuccessCriteria = probe.SuccessCriteria,
                UnlocksSkillCode = nextTemplate.SkillCode,
                UnlocksSkillName = nextTemplate.SkillName,
                ScheduledFor = DateTime.Today
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<ChildSkillReadinessCheck>> GetPendingReadinessChecksAsync(Guid childId, Guid parentId)
    {
        await SyncReadinessChecksAsync(childId, parentId);

        return await db.ChildSkillReadinessChecks
            .Where(x => x.ChildId == childId && x.Child.ParentId == parentId && !x.CompletedAt.HasValue)
            .OrderBy(x => x.ScheduledFor)
            .ThenBy(x => x.SkillName)
            .ToListAsync();
    }

    public async Task ApplyReadinessChecksAsync(Guid childId, Guid parentId, IEnumerable<SkillReadinessSubmissionItem> items)
    {
        var child = await db.Children
            .Include(x => x.SkillProgressEntries)
            .FirstAsync(x => x.Id == childId && x.ParentId == parentId);

        var submitted = items.ToList();
        if (submitted.Count == 0)
        {
            return;
        }

        var ids = submitted.Select(x => x.ReadinessCheckId).ToList();
        var checks = await db.ChildSkillReadinessChecks
            .Where(x => x.ChildId == childId && ids.Contains(x.Id))
            .ToListAsync();

        foreach (var item in submitted)
        {
            var check = checks.FirstOrDefault(x => x.Id == item.ReadinessCheckId);
            if (check is null || check.CompletedAt.HasValue)
            {
                continue;
            }

            var rating = Enum.IsDefined(typeof(SkillFeedbackLevel), item.Rating)
                ? (SkillFeedbackLevel)item.Rating
                : SkillFeedbackLevel.Developing;
            var progress = child.SkillProgressEntries.FirstOrDefault(x => x.SkillCode == check.SkillCode);
            if (progress is null)
            {
                continue;
            }

            skillProgressionService.ApplyReadinessCheck(progress, check, rating);
            check.Rating = rating;
            check.Passed = rating == SkillFeedbackLevel.Secure;
            check.Notes = item.Notes ?? string.Empty;
            check.CompletedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static (string Headline, string ParentPrompt, string SuccessCriteria) BuildProbe(ChildSkillProgress progress, CurriculumTemplate nextTemplate)
    {
        return (
            $"Pre-avaliacao de prontidao: {progress.SkillName}",
            $"Antes de liberar {nextTemplate.SkillName.ToLowerInvariant()}, proponha uma tarefa curta em {progress.SkillName.ToLowerInvariant()} sem muita ajuda. Observe se a crianca sustenta a habilidade em 2 tentativas e consegue explicar o que fez.",
            $"Passe se a crianca mantiver seguranca, pouca ajuda e estabilidade suficiente para entrar em {nextTemplate.SkillName.ToLowerInvariant()} sem perder a base."
        );
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

public class SkillReadinessSubmissionItem
{
    public Guid ReadinessCheckId { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
}
