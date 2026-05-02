namespace NewSchool.Web.Domain;

public class FamilyLibraryUserState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid MaterialId { get; set; }
    public FamilyLibraryMaterial Material { get; set; } = null!;
    public int CurrentPageNumber { get; set; } = 1;
    public bool IsFavorite { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? LastReadAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
