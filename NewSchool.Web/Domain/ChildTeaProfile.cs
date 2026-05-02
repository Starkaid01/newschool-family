namespace NewSchool.Web.Domain;

public class ChildTeaProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public string CommunicationProfile { get; set; } = string.Empty;
    public string CommunicationNotes { get; set; } = string.Empty;
    public int AnxietyLevel { get; set; } = 3;
    public int CognitiveRigidityLevel { get; set; } = 3;
    public int SensorySensitivityLevel { get; set; } = 3;
    public int TransitionDifficultyLevel { get; set; } = 3;
    public int SupportIntensityLevel { get; set; } = 3;
    public bool NeedsVisualRoutine { get; set; }
    public bool NeedsFirstThen { get; set; }
    public bool NeedsTimer { get; set; }
    public bool NeedsPlanB { get; set; }
    public string SpecialInterests { get; set; } = string.Empty;
    public string EffectiveReinforcers { get; set; } = string.Empty;
    public string CommonTriggers { get; set; } = string.Empty;
    public string OverloadSignals { get; set; } = string.Empty;
    public string CalmingStrategies { get; set; } = string.Empty;
    public string TransitionSupports { get; set; } = string.Empty;
    public string DailyLivingPriorities { get; set; } = string.Empty;
    public string ParentPrimaryGoal { get; set; } = string.Empty;
    public string SchoolBarrierSummary { get; set; } = string.Empty;
    public string DocumentationNotes { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
