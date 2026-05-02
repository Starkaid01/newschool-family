namespace NewSchool.Web.Domain;

public class ChildSkillProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public int Age { get; set; }
    public LearningDomain Domain { get; set; }
    public CurriculumSupportScope SupportScope { get; set; } = CurriculumSupportScope.General;
    public FunctionalSupportTrack FunctionalTrack { get; set; } = FunctionalSupportTrack.Base;
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string SkillStage { get; set; } = "starting";
    public int MasteryScore { get; set; } = 45;
    public int TimesPracticed { get; set; }
    public int TimesSuccessful { get; set; }
    public int SecureStreak { get; set; }
    public int StruggleStreak { get; set; }
    public bool ReadyToAdvance { get; set; }
    public bool NeedsReadinessCheck { get; set; }
    public bool ReadinessApproved { get; set; }
    public bool NeedsRemediation { get; set; }
    public DateTime? StageChangedAt { get; set; }
    public DateTime? LastPracticedAt { get; set; }
    public DateTime? LastReadinessCheckAt { get; set; }
    public DateTime? NextReviewAt { get; set; }
    public string NextMilestone { get; set; } = "Comecar a praticar com apoio";
    public string RemediationPlan { get; set; } = string.Empty;
    public string Recommendation { get; set; } = "Nova habilidade";
}
