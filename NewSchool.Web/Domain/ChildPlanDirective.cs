namespace NewSchool.Web.Domain;

public class ChildPlanDirective
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public DateTime PlannedDate { get; set; }
    public PlanDirectiveType DirectiveType { get; set; }
    public Guid? TemplateId { get; set; }
    public CurriculumTemplate? Template { get; set; }
    public FunctionalSupportTrack? FunctionalTrack { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
}
