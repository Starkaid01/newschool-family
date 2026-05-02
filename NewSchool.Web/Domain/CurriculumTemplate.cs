namespace NewSchool.Web.Domain;

public class CurriculumTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Age { get; set; }
    public LearningDomain Domain { get; set; }
    public CurriculumSupportScope SupportScope { get; set; } = CurriculumSupportScope.General;
    public FunctionalSupportTrack FunctionalTrack { get; set; } = FunctionalSupportTrack.Base;
    public string GoalTrack { get; set; } = "balanced_growth";
    public string SkillCode { get; set; } = string.Empty;
    public string PrerequisiteSkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Materials { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string ChildMission { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public int ReviewAfterDays { get; set; } = 3;
    public int SortOrder { get; set; }
    public ICollection<ChildFavoriteActivity> FavoritedByChildren { get; set; } = new List<ChildFavoriteActivity>();
    public ICollection<ChildPlanDirective> PlanDirectives { get; set; } = new List<ChildPlanDirective>();
}
