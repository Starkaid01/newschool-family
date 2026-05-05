using Microsoft.EntityFrameworkCore;
using NewSchool.Web.Domain;

namespace NewSchool.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ChildProfile> Children => Set<ChildProfile>();
    public DbSet<CurriculumTemplate> CurriculumTemplates => Set<CurriculumTemplate>();
    public DbSet<DailyPlan> DailyPlans => Set<DailyPlan>();
    public DbSet<DailyPlanBlock> DailyPlanBlocks => Set<DailyPlanBlock>();
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();
    public DbSet<ChildSkillProgress> ChildSkillProgressEntries => Set<ChildSkillProgress>();
    public DbSet<LearningBlockFeedback> LearningBlockFeedbackEntries => Set<LearningBlockFeedback>();
    public DbSet<ChildDevelopmentProfile> ChildDevelopmentProfiles => Set<ChildDevelopmentProfile>();
    public DbSet<ChildTeaProfile> ChildTeaProfiles => Set<ChildTeaProfile>();
    public DbSet<ChildMonthlySnapshot> ChildMonthlySnapshots => Set<ChildMonthlySnapshot>();
    public DbSet<ChildMonthlyGoalCycle> ChildMonthlyGoalCycles => Set<ChildMonthlyGoalCycle>();
    public DbSet<ChildMonthlyGoalItem> ChildMonthlyGoalItems => Set<ChildMonthlyGoalItem>();
    public DbSet<ChildRecoveryPlan> ChildRecoveryPlans => Set<ChildRecoveryPlan>();
    public DbSet<ChildRecoveryPlanDay> ChildRecoveryPlanDays => Set<ChildRecoveryPlanDay>();
    public DbSet<ChildAchievement> ChildAchievements => Set<ChildAchievement>();
    public DbSet<TrackAcquisitionSnapshot> TrackAcquisitionSnapshots => Set<TrackAcquisitionSnapshot>();
    public DbSet<InterventionPlaybookEntry> InterventionPlaybookEntries => Set<InterventionPlaybookEntry>();
    public DbSet<ChildSkillCheckup> ChildSkillCheckups => Set<ChildSkillCheckup>();
    public DbSet<ChildSkillReadinessCheck> ChildSkillReadinessChecks => Set<ChildSkillReadinessCheck>();
    public DbSet<ChildFavoriteActivity> ChildFavoriteActivities => Set<ChildFavoriteActivity>();
    public DbSet<ChildPlanDirective> ChildPlanDirectives => Set<ChildPlanDirective>();
    public DbSet<ChildRoutineObservation> ChildRoutineObservations => Set<ChildRoutineObservation>();
    public DbSet<ChildExternalContentProgress> ChildExternalContentProgressEntries => Set<ChildExternalContentProgress>();
    public DbSet<DailyPlanBlockCompletion> DailyPlanBlockCompletions => Set<DailyPlanBlockCompletion>();
    public DbSet<CuratedLearningResource> CuratedLearningResources => Set<CuratedLearningResource>();
    public DbSet<CuratedTaskTemplate> CuratedTaskTemplates => Set<CuratedTaskTemplate>();
    public DbSet<FamilyLibraryMaterial> FamilyLibraryMaterials => Set<FamilyLibraryMaterial>();
    public DbSet<FamilyLibraryPage> FamilyLibraryPages => Set<FamilyLibraryPage>();
    public DbSet<FamilyLibraryUserState> FamilyLibraryUserStates => Set<FamilyLibraryUserState>();
    public DbSet<ChildLibraryReadingProgress> ChildLibraryReadingProgressEntries => Set<ChildLibraryReadingProgress>();
    public DbSet<ChildLessonAssessment> ChildLessonAssessments => Set<ChildLessonAssessment>();
    public DbSet<ChildLessonAssessmentItem> ChildLessonAssessmentItems => Set<ChildLessonAssessmentItem>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>().ToTable("NS_AppUsers");
        modelBuilder.Entity<ChildProfile>().ToTable("NS_ChildProfiles");
        modelBuilder.Entity<CurriculumTemplate>().ToTable("NS_CurriculumTemplates");
        modelBuilder.Entity<DailyPlan>().ToTable("NS_DailyPlans");
        modelBuilder.Entity<DailyPlanBlock>().ToTable("NS_DailyPlanBlocks");
        modelBuilder.Entity<LearningSession>().ToTable("NS_LearningSessions");
        modelBuilder.Entity<ChildSkillProgress>().ToTable("NS_ChildSkillProgress");
        modelBuilder.Entity<LearningBlockFeedback>().ToTable("NS_LearningBlockFeedback");
        modelBuilder.Entity<ChildDevelopmentProfile>().ToTable("NS_ChildDevelopmentProfiles");
        modelBuilder.Entity<ChildTeaProfile>().ToTable("NS_ChildTeaProfiles");
        modelBuilder.Entity<ChildMonthlySnapshot>().ToTable("NS_ChildMonthlySnapshots");
        modelBuilder.Entity<ChildMonthlyGoalCycle>().ToTable("NS_ChildMonthlyGoalCycles");
        modelBuilder.Entity<ChildMonthlyGoalItem>().ToTable("NS_ChildMonthlyGoalItems");
        modelBuilder.Entity<ChildRecoveryPlan>().ToTable("NS_ChildRecoveryPlans");
        modelBuilder.Entity<ChildRecoveryPlanDay>().ToTable("NS_ChildRecoveryPlanDays");
        modelBuilder.Entity<ChildAchievement>().ToTable("NS_ChildAchievements");
        modelBuilder.Entity<TrackAcquisitionSnapshot>().ToTable("NS_TrackAcquisitionSnapshots");
        modelBuilder.Entity<InterventionPlaybookEntry>().ToTable("NS_InterventionPlaybookEntries");
        modelBuilder.Entity<ChildSkillCheckup>().ToTable("NS_ChildSkillCheckups");
        modelBuilder.Entity<ChildSkillReadinessCheck>().ToTable("NS_ChildSkillReadinessChecks");
        modelBuilder.Entity<ChildFavoriteActivity>().ToTable("NS_ChildFavoriteActivities");
        modelBuilder.Entity<ChildPlanDirective>().ToTable("NS_ChildPlanDirectives");
        modelBuilder.Entity<ChildRoutineObservation>().ToTable("NS_ChildRoutineObservations");
        modelBuilder.Entity<ChildExternalContentProgress>().ToTable("NS_ChildExternalContentProgress");
        modelBuilder.Entity<DailyPlanBlockCompletion>().ToTable("NS_DailyPlanBlockCompletions");
        modelBuilder.Entity<CuratedLearningResource>().ToTable("NS_CuratedLearningResources");
        modelBuilder.Entity<CuratedTaskTemplate>().ToTable("NS_CuratedTaskTemplates");
        modelBuilder.Entity<FamilyLibraryMaterial>().ToTable("NS_FamilyLibraryMaterials");
        modelBuilder.Entity<FamilyLibraryPage>().ToTable("NS_FamilyLibraryPages");
        modelBuilder.Entity<FamilyLibraryUserState>().ToTable("NS_FamilyLibraryUserStates");
        modelBuilder.Entity<ChildLibraryReadingProgress>().ToTable("NS_ChildLibraryReadingProgress");
        modelBuilder.Entity<ChildLessonAssessment>().ToTable("NS_ChildLessonAssessments");
        modelBuilder.Entity<ChildLessonAssessmentItem>().ToTable("NS_ChildLessonAssessmentItems");
        modelBuilder.Entity<UserNotification>().ToTable("NS_UserNotifications");

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Email)
            .IsUnique();
        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.ReferralCode)
            .IsUnique();

        modelBuilder.Entity<AppUser>().Property(x => x.FullName).HasMaxLength(160);
        modelBuilder.Entity<AppUser>().Property(x => x.Email).HasMaxLength(190);
        modelBuilder.Entity<AppUser>().Property(x => x.PhoneNumber).HasMaxLength(40);
        modelBuilder.Entity<AppUser>().Property(x => x.PasswordHash).HasMaxLength(400);
        modelBuilder.Entity<AppUser>().Property(x => x.ReferralCode).HasMaxLength(40);
        modelBuilder.Entity<AppUser>().Property(x => x.AcquisitionTrack).HasMaxLength(40);
        modelBuilder.Entity<AppUser>().Property(x => x.PreferredReminderChannel).HasMaxLength(20);
        modelBuilder.Entity<AppUser>().Property(x => x.SubscriptionStatus).HasMaxLength(40);
        modelBuilder.Entity<AppUser>().Property(x => x.StripeCustomerId).HasMaxLength(100);
        modelBuilder.Entity<AppUser>().Property(x => x.StripeSubscriptionId).HasMaxLength(100);
        modelBuilder.Entity<AppUser>().Property(x => x.StoragePlanCode).HasMaxLength(40);
        modelBuilder.Entity<AppUser>().Property(x => x.PasswordResetTokenHash).HasMaxLength(200);

        modelBuilder.Entity<AppUser>()
            .Property(x => x.Role)
            .HasConversion<string>();
        modelBuilder.Entity<AppUser>()
            .HasOne(x => x.ReferredByUser)
            .WithMany(x => x.Referrals)
            .HasForeignKey(x => x.ReferredByUserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<AppUser>()
            .HasMany(x => x.Notifications)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AppUser>()
            .HasMany(x => x.SentNotifications)
            .WithOne(x => x.SentByAdmin)
            .HasForeignKey(x => x.SentByAdminId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ChildProfile>().Property(x => x.FullName).HasMaxLength(160);
        modelBuilder.Entity<ChildProfile>().Property(x => x.Notes).HasMaxLength(1000);
        modelBuilder.Entity<ChildProfile>().Property(x => x.FamilyGoalTrack).HasMaxLength(40);
        modelBuilder.Entity<ChildProfile>().Property(x => x.TeachingMethodology).HasMaxLength(40);
        modelBuilder.Entity<ChildProfile>().Property(x => x.LearningProfile).HasMaxLength(40);
        modelBuilder.Entity<ChildProfile>().Property(x => x.GuidanceStyle).HasMaxLength(40);
        modelBuilder.Entity<ChildProfile>().Property(x => x.SupportProfile).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<ChildProfile>()
            .HasIndex(x => x.ParentId);
        modelBuilder.Entity<ChildDevelopmentProfile>().Property(x => x.StrengthsSummary).HasMaxLength(800);
        modelBuilder.Entity<ChildDevelopmentProfile>().Property(x => x.SupportSummary).HasMaxLength(800);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.CommunicationProfile).HasMaxLength(220);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.CommunicationNotes).HasMaxLength(1000);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.SpecialInterests).HasMaxLength(1000);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.EffectiveReinforcers).HasMaxLength(1000);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.CommonTriggers).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.OverloadSignals).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.CalmingStrategies).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.TransitionSupports).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.DailyLivingPriorities).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.ParentPrimaryGoal).HasMaxLength(800);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.SchoolBarrierSummary).HasMaxLength(1200);
        modelBuilder.Entity<ChildTeaProfile>().Property(x => x.DocumentationNotes).HasMaxLength(1200);
        modelBuilder.Entity<ChildMonthlySnapshot>().Property(x => x.StrongestArea).HasMaxLength(120);
        modelBuilder.Entity<ChildMonthlySnapshot>().Property(x => x.AttentionArea).HasMaxLength(120);
        modelBuilder.Entity<ChildMonthlySnapshot>().Property(x => x.Summary).HasMaxLength(800);
        modelBuilder.Entity<ChildMonthlyGoalCycle>().Property(x => x.GoalHeadline).HasMaxLength(240);
        modelBuilder.Entity<ChildMonthlyGoalCycle>().Property(x => x.Summary).HasMaxLength(800);
        modelBuilder.Entity<ChildMonthlyGoalCycle>().Property(x => x.RiskLevel).HasMaxLength(30);
        modelBuilder.Entity<ChildMonthlyGoalItem>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<ChildMonthlyGoalItem>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<ChildMonthlyGoalItem>().Property(x => x.Status).HasMaxLength(40);
        modelBuilder.Entity<ChildRecoveryPlan>().Property(x => x.Title).HasMaxLength(220);
        modelBuilder.Entity<ChildRecoveryPlan>().Property(x => x.Summary).HasMaxLength(800);
        modelBuilder.Entity<ChildRecoveryPlan>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<ChildRecoveryPlanDay>().Property(x => x.FocusSkill).HasMaxLength(180);
        modelBuilder.Entity<ChildRecoveryPlanDay>().Property(x => x.GoalText).HasMaxLength(400);
        modelBuilder.Entity<ChildRecoveryPlanDay>().Property(x => x.ParentTip).HasMaxLength(600);
        modelBuilder.Entity<ChildAchievement>().Property(x => x.Code).HasMaxLength(80);
        modelBuilder.Entity<ChildAchievement>().Property(x => x.Title).HasMaxLength(180);
        modelBuilder.Entity<ChildAchievement>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<TrackAcquisitionSnapshot>().Property(x => x.TrackCode).HasMaxLength(40);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.GoalTrack).HasMaxLength(40);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.TriggerCode).HasMaxLength(80);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.TriggerLabel).HasMaxLength(180);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.MatchKeywords).HasMaxLength(500);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.StageScope).HasMaxLength(120);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.Headline).HasMaxLength(220);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.HowToSpot).HasMaxLength(600);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.LikelyCause).HasMaxLength(500);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.WhatToSay).HasMaxLength(600);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.WhatToAvoid).HasMaxLength(500);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.QuickActivity).HasMaxLength(700);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.Materials).HasMaxLength(500);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.SuccessSignal).HasMaxLength(400);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.RepeatPlan).HasMaxLength(300);
        modelBuilder.Entity<InterventionPlaybookEntry>().Property(x => x.FallbackAction).HasMaxLength(500);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.PromptTitle).HasMaxLength(220);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.ParentPrompt).HasMaxLength(900);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.SuccessCriteria).HasMaxLength(500);
        modelBuilder.Entity<ChildSkillCheckup>().Property(x => x.Notes).HasMaxLength(600);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.Headline).HasMaxLength(220);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.ParentPrompt).HasMaxLength(900);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.SuccessCriteria).HasMaxLength(500);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.UnlocksSkillCode).HasMaxLength(80);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.UnlocksSkillName).HasMaxLength(180);
        modelBuilder.Entity<ChildSkillReadinessCheck>().Property(x => x.Notes).HasMaxLength(600);
        modelBuilder.Entity<ChildPlanDirective>().Property(x => x.Note).HasMaxLength(300);
        modelBuilder.Entity<ChildPlanDirective>()
            .Property(x => x.DirectiveType)
            .HasConversion<string>()
            .HasMaxLength(30);
        modelBuilder.Entity<ChildPlanDirective>()
            .Property(x => x.FunctionalTrack)
            .HasConversion<string>()
            .HasMaxLength(30);
        modelBuilder.Entity<ChildExternalContentProgress>().Property(x => x.ContentSlug).HasMaxLength(120);
        modelBuilder.Entity<ChildExternalContentProgress>().Property(x => x.ContentTitle).HasMaxLength(220);
        modelBuilder.Entity<ChildExternalContentProgress>().Property(x => x.Provider).HasMaxLength(120);
        modelBuilder.Entity<ChildExternalContentProgress>().Property(x => x.AreaLabel).HasMaxLength(120);
        modelBuilder.Entity<ChildExternalContentProgress>().Property(x => x.Notes).HasMaxLength(800);
        modelBuilder.Entity<ChildExternalContentProgress>()
            .HasIndex(x => new { x.ChildId, x.ContentSlug })
            .IsUnique();
        modelBuilder.Entity<ChildExternalContentProgress>()
            .HasOne(x => x.Child)
            .WithMany(x => x.ExternalContentProgressEntries)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DailyPlanBlockCompletion>().Property(x => x.Notes).HasMaxLength(400);
        modelBuilder.Entity<DailyPlanBlockCompletion>().Property(x => x.CompletionSource).HasMaxLength(40);
        modelBuilder.Entity<DailyPlanBlockCompletion>()
            .HasIndex(x => new { x.ChildId, x.DailyPlanBlockId })
            .IsUnique();
        modelBuilder.Entity<DailyPlanBlockCompletion>()
            .HasIndex(x => new { x.ChildId, x.DailyPlanId });
        modelBuilder.Entity<DailyPlanBlockCompletion>()
            .HasOne(x => x.Child)
            .WithMany(x => x.TaskCompletions)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<DailyPlanBlockCompletion>()
            .HasOne(x => x.DailyPlan)
            .WithMany(x => x.TaskCompletions)
            .HasForeignKey(x => x.DailyPlanId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<DailyPlanBlockCompletion>()
            .HasOne(x => x.DailyPlanBlock)
            .WithMany(x => x.Completions)
            .HasForeignKey(x => x.DailyPlanBlockId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.Slug).HasMaxLength(120);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.Title).HasMaxLength(220);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.Summary).HasMaxLength(900);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.UseNote).HasMaxLength(900);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.FormatLabel).HasMaxLength(80);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.ResourceKind).HasMaxLength(80);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.SourceName).HasMaxLength(120);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.SourceUrl).HasMaxLength(500);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.AccessUrl).HasMaxLength(500);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.LicenseLabel).HasMaxLength(80);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.Attribution).HasMaxLength(400);
        modelBuilder.Entity<CuratedLearningResource>().Property(x => x.LanguageCode).HasMaxLength(20);
        modelBuilder.Entity<CuratedLearningResource>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<CuratedLearningResource>()
            .HasIndex(x => x.Slug)
            .IsUnique();
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.Slug).HasMaxLength(120);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.Title).HasMaxLength(220);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.GoalTrack).HasMaxLength(40);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.MatchKeywords).HasMaxLength(600);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.Goal).HasMaxLength(900);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.ParentGuide).HasMaxLength(1500);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.ChildPrompt).HasMaxLength(800);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.TaskSteps).HasMaxLength(2000);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.MaterialsSummary).HasMaxLength(900);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.EvidencePrompt).HasMaxLength(900);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.ExpectedOutcome).HasMaxLength(700);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.SupportLinkLabel).HasMaxLength(120);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.SupportLinkUrl).HasMaxLength(500);
        modelBuilder.Entity<CuratedTaskTemplate>().Property(x => x.SupportLinkSource).HasMaxLength(120);
        modelBuilder.Entity<CuratedTaskTemplate>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<CuratedTaskTemplate>()
            .Property(x => x.FunctionalTrack)
            .HasConversion<string>()
            .HasMaxLength(30);
        modelBuilder.Entity<CuratedTaskTemplate>()
            .HasIndex(x => x.Slug)
            .IsUnique();
        modelBuilder.Entity<CuratedTaskTemplate>()
            .HasOne(x => x.PrimaryResource)
            .WithMany(x => x.PrimaryTasks)
            .HasForeignKey(x => x.PrimaryResourceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.Title).HasMaxLength(260);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.Category).HasMaxLength(160);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.EducationStage).HasMaxLength(120);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.SkillFocus).HasMaxLength(240);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.Description).HasMaxLength(900);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.CollectionLabel).HasMaxLength(120);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.CoverImageRelativePath).HasMaxLength(400);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.SourceRelativePath).HasMaxLength(400);
        modelBuilder.Entity<FamilyLibraryMaterial>().Property(x => x.SourceSyncToken).HasMaxLength(200);
        modelBuilder.Entity<FamilyLibraryMaterial>()
            .HasIndex(x => new { x.IsPrintable, x.EducationStage, x.Category });

        modelBuilder.Entity<FamilyLibraryPage>().Property(x => x.ImageRelativePath).HasMaxLength(400);
        modelBuilder.Entity<FamilyLibraryPage>()
            .HasIndex(x => new { x.MaterialId, x.PageNumber })
            .IsUnique();
        modelBuilder.Entity<FamilyLibraryPage>()
            .HasOne(x => x.Material)
            .WithMany(x => x.Pages)
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FamilyLibraryUserState>()
            .HasIndex(x => new { x.UserId, x.MaterialId })
            .IsUnique();
        modelBuilder.Entity<FamilyLibraryUserState>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FamilyLibraryUserState>()
            .HasOne(x => x.Material)
            .WithMany(x => x.UserStates)
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildLibraryReadingProgress>().Property(x => x.PhaseLabel).HasMaxLength(80);
        modelBuilder.Entity<ChildLibraryReadingProgress>().Property(x => x.PeriodKey).HasMaxLength(40);
        modelBuilder.Entity<ChildLibraryReadingProgress>().Property(x => x.CompletionKind).HasMaxLength(40);
        modelBuilder.Entity<ChildLibraryReadingProgress>().Property(x => x.GoalLabel).HasMaxLength(180);
        modelBuilder.Entity<ChildLibraryReadingProgress>().Property(x => x.Notes).HasMaxLength(600);
        modelBuilder.Entity<ChildLibraryReadingProgress>()
            .HasIndex(x => new { x.ChildId, x.MaterialId, x.PhaseNumber, x.CompletionKind, x.PeriodKey })
            .IsUnique();
        modelBuilder.Entity<ChildLibraryReadingProgress>()
            .HasOne(x => x.Child)
            .WithMany(x => x.LibraryReadingProgressEntries)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildLibraryReadingProgress>()
            .HasOne(x => x.ParentUser)
            .WithMany()
            .HasForeignKey(x => x.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildLibraryReadingProgress>()
            .HasOne(x => x.Material)
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.PhaseLabel).HasMaxLength(80);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.SubjectLabel).HasMaxLength(80);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.LessonTitle).HasMaxLength(260);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.UnitTitle).HasMaxLength(260);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.AssessmentTitle).HasMaxLength(260);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.PrintableHeadline).HasMaxLength(260);
        modelBuilder.Entity<ChildLessonAssessment>().Property(x => x.PrintableSummary).HasMaxLength(1200);
        modelBuilder.Entity<ChildLessonAssessment>()
            .Property(x => x.Domain)
            .HasConversion<string>()
            .HasMaxLength(40);
        modelBuilder.Entity<ChildLessonAssessment>()
            .HasIndex(x => new { x.ChildId, x.DailyPlanBlockId })
            .IsUnique();
        modelBuilder.Entity<ChildLessonAssessment>()
            .HasOne(x => x.Child)
            .WithMany()
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildLessonAssessment>()
            .HasOne(x => x.ParentUser)
            .WithMany()
            .HasForeignKey(x => x.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildLessonAssessment>()
            .HasOne(x => x.DailyPlan)
            .WithMany()
            .HasForeignKey(x => x.DailyPlanId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildLessonAssessment>()
            .HasOne(x => x.DailyPlanBlock)
            .WithMany()
            .HasForeignKey(x => x.DailyPlanBlockId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildLessonAssessmentItem>().Property(x => x.Prompt).HasMaxLength(1200);
        modelBuilder.Entity<ChildLessonAssessmentItem>().Property(x => x.ExpectedAnswer).HasMaxLength(1200);
        modelBuilder.Entity<ChildLessonAssessmentItem>().Property(x => x.TeacherNote).HasMaxLength(800);
        modelBuilder.Entity<ChildLessonAssessmentItem>()
            .HasIndex(x => new { x.AssessmentId, x.SortOrder })
            .IsUnique();
        modelBuilder.Entity<ChildLessonAssessmentItem>()
            .HasOne(x => x.Assessment)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserNotification>().Property(x => x.Title).HasMaxLength(180);
        modelBuilder.Entity<UserNotification>().Property(x => x.Message).HasMaxLength(1500);
        modelBuilder.Entity<UserNotification>().Property(x => x.NotificationLevel).HasMaxLength(20);
        modelBuilder.Entity<UserNotification>().Property(x => x.ActionUrl).HasMaxLength(500);
        modelBuilder.Entity<UserNotification>()
            .HasIndex(x => new { x.UserId, x.CreatedAt });
        modelBuilder.Entity<UserNotification>()
            .HasIndex(x => new { x.UserId, x.ReadAt });

        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.Title).HasMaxLength(180);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.GoalTrack).HasMaxLength(40);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.SupportScope).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.FunctionalTrack).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.PrerequisiteSkillCode).HasMaxLength(80);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.Goal).HasMaxLength(600);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.Materials).HasMaxLength(600);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.ParentGuide).HasMaxLength(1200);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.ChildMission).HasMaxLength(600);
        modelBuilder.Entity<CurriculumTemplate>().Property(x => x.EvidencePrompt).HasMaxLength(600);

        modelBuilder.Entity<ChildProfile>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildProfile>()
            .HasOne(x => x.DevelopmentProfile)
            .WithOne(x => x.Child)
            .HasForeignKey<ChildDevelopmentProfile>(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildDevelopmentProfile>()
            .HasIndex(x => x.ChildId)
            .IsUnique();
        modelBuilder.Entity<ChildTeaProfile>()
            .HasIndex(x => x.ChildId)
            .IsUnique();
        modelBuilder.Entity<ChildProfile>()
            .HasOne(x => x.TeaProfile)
            .WithOne(x => x.Child)
            .HasForeignKey<ChildTeaProfile>(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildMonthlySnapshot>()
            .HasIndex(x => new { x.ChildId, x.Year, x.Month })
            .IsUnique();
        modelBuilder.Entity<ChildMonthlySnapshot>()
            .HasOne(x => x.Child)
            .WithMany(x => x.MonthlySnapshots)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildMonthlyGoalCycle>()
            .HasIndex(x => new { x.ChildId, x.Year, x.Month })
            .IsUnique();
        modelBuilder.Entity<ChildMonthlyGoalCycle>()
            .HasOne(x => x.Child)
            .WithMany(x => x.MonthlyGoalCycles)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildMonthlyGoalCycle>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Cycle)
            .HasForeignKey(x => x.CycleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildMonthlyGoalItem>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<ChildRecoveryPlan>()
            .HasOne(x => x.Child)
            .WithMany(x => x.RecoveryPlans)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildRecoveryPlan>()
            .HasOne(x => x.GoalCycle)
            .WithMany()
            .HasForeignKey(x => x.GoalCycleId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildRecoveryPlan>()
            .HasMany(x => x.Days)
            .WithOne(x => x.RecoveryPlan)
            .HasForeignKey(x => x.RecoveryPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildAchievement>()
            .HasIndex(x => new { x.ChildId, x.Code })
            .IsUnique();
        modelBuilder.Entity<ChildAchievement>()
            .HasOne(x => x.Child)
            .WithMany(x => x.Achievements)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackAcquisitionSnapshot>()
            .HasIndex(x => new { x.TrackCode, x.UserId })
            .IsUnique();
        modelBuilder.Entity<TrackAcquisitionSnapshot>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackAcquisitionSnapshot>()
            .HasOne(x => x.Child)
            .WithMany()
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<InterventionPlaybookEntry>()
            .HasIndex(x => new { x.Domain, x.TriggerCode, x.GoalTrack })
            .IsUnique();
        modelBuilder.Entity<InterventionPlaybookEntry>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<ChildSkillCheckup>()
            .HasIndex(x => new { x.ChildId, x.SkillCode, x.ScheduledFor });
        modelBuilder.Entity<ChildSkillCheckup>()
            .HasOne(x => x.Child)
            .WithMany(x => x.SkillCheckups)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildSkillCheckup>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<ChildSkillCheckup>()
            .Property(x => x.Rating)
            .HasConversion<string>();
        modelBuilder.Entity<ChildSkillReadinessCheck>()
            .HasIndex(x => new { x.ChildId, x.SkillCode, x.ScheduledFor });
        modelBuilder.Entity<ChildSkillReadinessCheck>()
            .HasOne(x => x.Child)
            .WithMany(x => x.SkillReadinessChecks)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildSkillReadinessCheck>()
            .Property(x => x.Domain)
            .HasConversion<string>();
        modelBuilder.Entity<ChildSkillReadinessCheck>()
            .Property(x => x.Rating)
            .HasConversion<string>();
        modelBuilder.Entity<ChildFavoriteActivity>()
            .HasIndex(x => new { x.ChildId, x.TemplateId })
            .IsUnique();
        modelBuilder.Entity<ChildFavoriteActivity>()
            .HasOne(x => x.Child)
            .WithMany(x => x.FavoriteActivities)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildFavoriteActivity>()
            .HasOne(x => x.Template)
            .WithMany(x => x.FavoritedByChildren)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildPlanDirective>()
            .HasIndex(x => new { x.ChildId, x.PlannedDate, x.DirectiveType });
        modelBuilder.Entity<ChildPlanDirective>()
            .HasOne(x => x.Child)
            .WithMany(x => x.PlanDirectives)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildPlanDirective>()
            .HasOne(x => x.Template)
            .WithMany(x => x.PlanDirectives)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CurriculumTemplate>()
            .Property(x => x.Domain)
            .HasConversion<string>();

        modelBuilder.Entity<DailyPlan>()
            .HasOne(x => x.Child)
            .WithMany(x => x.DailyPlans)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyPlan>()
            .HasIndex(x => new { x.ChildId, x.PlannedDate })
            .IsUnique();

        modelBuilder.Entity<DailyPlan>().Property(x => x.Theme).HasMaxLength(180);
        modelBuilder.Entity<DailyPlan>().Property(x => x.ParentSummary).HasMaxLength(1500);
        modelBuilder.Entity<DailyPlan>().Property(x => x.ChildNarrative).HasMaxLength(1500);
        modelBuilder.Entity<DailyPlan>().Property(x => x.RecoveryHeadline).HasMaxLength(220);

        modelBuilder.Entity<DailyPlan>()
            .HasMany(x => x.Blocks)
            .WithOne(x => x.DailyPlan)
            .HasForeignKey(x => x.DailyPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DailyPlanBlock>()
            .HasOne(x => x.SourceTemplate)
            .WithMany()
            .HasForeignKey(x => x.SourceTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.Title).HasMaxLength(180);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.SupportScope).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.FunctionalTrack).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.SourceTemplateId);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.Goal).HasMaxLength(700);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.ParentGuide).HasMaxLength(1500);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.ChildPrompt).HasMaxLength(700);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.Materials).HasMaxLength(700);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.EvidencePrompt).HasMaxLength(700);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.RecoveryNote).HasMaxLength(500);
        modelBuilder.Entity<DailyPlanBlock>().Property(x => x.ReviewNote).HasMaxLength(500);

        modelBuilder.Entity<LearningSession>()
            .HasOne(x => x.Child)
            .WithMany(x => x.LearningSessions)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<LearningSession>()
            .HasOne(x => x.DailyPlan)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.DailyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningSession>().Property(x => x.Wins).HasMaxLength(1200);
        modelBuilder.Entity<LearningSession>().Property(x => x.Challenges).HasMaxLength(1200);
        modelBuilder.Entity<LearningSession>().Property(x => x.Notes).HasMaxLength(1200);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaUrl).HasMaxLength(500);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaContentType).HasMaxLength(120);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaFileName).HasMaxLength(260);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaStorageProvider).HasMaxLength(40);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaStorageKey).HasMaxLength(600);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaThumbnailUrl).HasMaxLength(500);
        modelBuilder.Entity<LearningSession>().Property(x => x.MediaThumbnailStorageKey).HasMaxLength(600);
        modelBuilder.Entity<LearningSession>()
            .HasIndex(x => new { x.ChildId, x.LoggedAt });
        modelBuilder.Entity<LearningSession>()
            .HasIndex(x => x.ChildId)
            .HasFilter("[MediaUrl] <> ''");
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.ContextPeriod).HasMaxLength(120);
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.Antecedent).HasMaxLength(1200);
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.ChildReaction).HasMaxLength(1200);
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.WhatHelped).HasMaxLength(1200);
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.SupportUsed).HasMaxLength(600);
        modelBuilder.Entity<ChildRoutineObservation>().Property(x => x.Notes).HasMaxLength(1200);
        modelBuilder.Entity<ChildRoutineObservation>()
            .HasIndex(x => new { x.ChildId, x.ObservedAt });
        modelBuilder.Entity<ChildRoutineObservation>()
            .HasOne(x => x.Child)
            .WithMany(x => x.RoutineObservations)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildRoutineObservation>()
            .HasOne(x => x.Session)
            .WithMany(x => x.RoutineObservations)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ChildRoutineObservation>()
            .HasOne(x => x.DailyPlan)
            .WithMany()
            .HasForeignKey(x => x.DailyPlanId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.SkillName).HasMaxLength(180);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.SupportScope).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.FunctionalTrack).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.SkillStage).HasMaxLength(40);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.NextMilestone).HasMaxLength(220);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.RemediationPlan).HasMaxLength(500);
        modelBuilder.Entity<ChildSkillProgress>().Property(x => x.Recommendation).HasMaxLength(180);
        modelBuilder.Entity<ChildSkillProgress>()
            .HasIndex(x => new { x.ChildId, x.SkillCode })
            .IsUnique();
        modelBuilder.Entity<ChildSkillProgress>()
            .HasOne(x => x.Child)
            .WithMany(x => x.SkillProgressEntries)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChildSkillProgress>()
            .Property(x => x.Domain)
            .HasConversion<string>();

        modelBuilder.Entity<LearningBlockFeedback>().Property(x => x.SkillCode).HasMaxLength(80);
        modelBuilder.Entity<LearningBlockFeedback>().Property(x => x.Notes).HasMaxLength(500);
        modelBuilder.Entity<LearningBlockFeedback>()
            .Property(x => x.Rating)
            .HasConversion<string>();
        modelBuilder.Entity<LearningBlockFeedback>()
            .HasOne(x => x.Session)
            .WithMany(x => x.BlockFeedbacks)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LearningBlockFeedback>()
            .HasOne(x => x.DailyPlanBlock)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.DailyPlanBlockId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DailyPlanBlock>()
            .Property(x => x.Domain)
            .HasConversion<string>();
    }
}
