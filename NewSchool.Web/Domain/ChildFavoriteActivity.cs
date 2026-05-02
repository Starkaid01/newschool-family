namespace NewSchool.Web.Domain;

public class ChildFavoriteActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid TemplateId { get; set; }
    public CurriculumTemplate Template { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
