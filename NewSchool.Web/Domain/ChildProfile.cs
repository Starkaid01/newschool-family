namespace NewSchool.Web.Domain;

public class ChildProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParentId { get; set; }
    public AppUser Parent { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int DailyStudyMinutes { get; set; } = 40;
    public string Notes { get; set; } = string.Empty;
    public SupportProfile SupportProfile { get; set; } = SupportProfile.General;
    public string FamilyGoalTrack { get; set; } = "balanced_growth";
    public string TeachingMethodology { get; set; } = "eclectic";
    public string LearningProfile { get; set; } = "balanced";
    public string GuidanceStyle { get; set; } = "guided";
    public DateTime CreatedAt { get; set; }
    public ChildDevelopmentProfile? DevelopmentProfile { get; set; }
    public ChildTeaProfile? TeaProfile { get; set; }
    public ICollection<DailyPlan> DailyPlans { get; set; } = new List<DailyPlan>();
    public ICollection<LearningSession> LearningSessions { get; set; } = new List<LearningSession>();
    public ICollection<ChildRoutineObservation> RoutineObservations { get; set; } = new List<ChildRoutineObservation>();
    public ICollection<ChildSkillProgress> SkillProgressEntries { get; set; } = new List<ChildSkillProgress>();
    public ICollection<ChildMonthlySnapshot> MonthlySnapshots { get; set; } = new List<ChildMonthlySnapshot>();
    public ICollection<ChildMonthlyGoalCycle> MonthlyGoalCycles { get; set; } = new List<ChildMonthlyGoalCycle>();
    public ICollection<ChildRecoveryPlan> RecoveryPlans { get; set; } = new List<ChildRecoveryPlan>();
    public ICollection<ChildAchievement> Achievements { get; set; } = new List<ChildAchievement>();
    public ICollection<ChildSkillCheckup> SkillCheckups { get; set; } = new List<ChildSkillCheckup>();
    public ICollection<ChildSkillReadinessCheck> SkillReadinessChecks { get; set; } = new List<ChildSkillReadinessCheck>();
    public ICollection<ChildFavoriteActivity> FavoriteActivities { get; set; } = new List<ChildFavoriteActivity>();
    public ICollection<ChildPlanDirective> PlanDirectives { get; set; } = new List<ChildPlanDirective>();
    public ICollection<ChildExternalContentProgress> ExternalContentProgressEntries { get; set; } = new List<ChildExternalContentProgress>();
    public ICollection<ChildLibraryReadingProgress> LibraryReadingProgressEntries { get; set; } = new List<ChildLibraryReadingProgress>();
    public ICollection<DailyPlanBlockCompletion> TaskCompletions { get; set; } = new List<DailyPlanBlockCompletion>();
}
