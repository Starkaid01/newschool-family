using Microsoft.EntityFrameworkCore;

namespace NewSchool.Web.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureSchemaAsync(ApplicationDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.NS_AppUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_AppUsers (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        FullName nvarchar(160) NOT NULL,
        Email nvarchar(190) NOT NULL,
        PhoneNumber nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_PhoneNumber DEFAULT '',
        PasswordHash nvarchar(400) NOT NULL,
        ReferralCode nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_ReferralCode DEFAULT '',
        ReferredByUserId uniqueidentifier NULL,
        Role nvarchar(30) NOT NULL,
        CreatedAt datetime2 NOT NULL,
        AcquisitionTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_AcquisitionTrack DEFAULT '',
        FirstSubscribedAt datetime2 NULL,
        PreferredReminderChannel nvarchar(20) NOT NULL CONSTRAINT DF_NS_AppUsers_PreferredReminderChannel DEFAULT 'email',
        SubscriptionStatus nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_SubscriptionStatus DEFAULT 'inactive',
        StripeCustomerId nvarchar(100) NULL,
        StripeSubscriptionId nvarchar(100) NULL,
        StoragePlanCode nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_StoragePlanCode DEFAULT 'free',
        StorageExtraFileBlocks int NOT NULL CONSTRAINT DF_NS_AppUsers_StorageExtraFileBlocks DEFAULT 0,
        SubscriptionCurrentPeriodEnd datetime2 NULL,
        TrialStartedAt datetime2 NULL,
        TrialEndsAt datetime2 NULL,
        LastActiveAt datetime2 NULL,
        OnboardingEmailSentAt datetime2 NULL,
        TrialReminderSentAt datetime2 NULL,
        ReactivationEmailSentAt datetime2 NULL,
        PaymentRecoveryEmailSentAt datetime2 NULL,
        DailyReminderMessageSentAt datetime2 NULL,
        ProgressRiskMessageSentAt datetime2 NULL,
        PasswordResetTokenHash nvarchar(200) NOT NULL CONSTRAINT DF_NS_AppUsers_PasswordResetTokenHash DEFAULT '',
        PasswordResetRequestedAt datetime2 NULL,
        PasswordResetExpiresAt datetime2 NULL
    );
    CREATE UNIQUE INDEX IX_NS_AppUsers_Email ON dbo.NS_AppUsers (Email);
    CREATE UNIQUE INDEX IX_NS_AppUsers_ReferralCode ON dbo.NS_AppUsers (ReferralCode);
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PhoneNumber') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PhoneNumber nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_PhoneNumber_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'ReferralCode') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD ReferralCode nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_ReferralCode_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'ReferredByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD ReferredByUserId uniqueidentifier NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PreferredReminderChannel') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PreferredReminderChannel nvarchar(20) NOT NULL CONSTRAINT DF_NS_AppUsers_PreferredReminderChannel_Alter DEFAULT 'email';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'AcquisitionTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD AcquisitionTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_AcquisitionTrack_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'FirstSubscribedAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD FirstSubscribedAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'SubscriptionStatus') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD SubscriptionStatus nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_SubscriptionStatus_Alter DEFAULT 'inactive';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'StripeCustomerId') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD StripeCustomerId nvarchar(100) NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'StripeSubscriptionId') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD StripeSubscriptionId nvarchar(100) NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'StoragePlanCode') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD StoragePlanCode nvarchar(40) NOT NULL CONSTRAINT DF_NS_AppUsers_StoragePlanCode_Alter DEFAULT 'free';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'StorageExtraFileBlocks') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD StorageExtraFileBlocks int NOT NULL CONSTRAINT DF_NS_AppUsers_StorageExtraFileBlocks_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'SubscriptionCurrentPeriodEnd') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD SubscriptionCurrentPeriodEnd datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'TrialStartedAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD TrialStartedAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'TrialEndsAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD TrialEndsAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'LastActiveAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD LastActiveAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'OnboardingEmailSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD OnboardingEmailSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'TrialReminderSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD TrialReminderSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'ReactivationEmailSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD ReactivationEmailSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PaymentRecoveryEmailSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PaymentRecoveryEmailSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'DailyReminderMessageSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD DailyReminderMessageSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'ProgressRiskMessageSentAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD ProgressRiskMessageSentAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PasswordResetTokenHash') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PasswordResetTokenHash nvarchar(200) NOT NULL CONSTRAINT DF_NS_AppUsers_PasswordResetTokenHash_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PasswordResetRequestedAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PasswordResetRequestedAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_AppUsers', 'PasswordResetExpiresAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_AppUsers ADD PasswordResetExpiresAt datetime2 NULL;
END;

IF OBJECT_ID(N'dbo.NS_ChildProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildProfiles (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ParentId uniqueidentifier NOT NULL,
        FullName nvarchar(160) NOT NULL,
        BirthDate datetime2 NOT NULL,
        DailyStudyMinutes int NOT NULL,
        Notes nvarchar(1000) NOT NULL,
        SupportProfile nvarchar(20) NOT NULL CONSTRAINT DF_NS_ChildProfiles_SupportProfile DEFAULT 'General',
        FamilyGoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_FamilyGoalTrack DEFAULT 'balanced_growth',
        TeachingMethodology nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_TeachingMethodology DEFAULT 'eclectic',
        LearningProfile nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_LearningProfile DEFAULT 'balanced',
        GuidanceStyle nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_GuidanceStyle DEFAULT 'guided',
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildProfiles_NS_AppUsers_ParentId
            FOREIGN KEY (ParentId) REFERENCES dbo.NS_AppUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_ChildProfiles_ParentId ON dbo.NS_ChildProfiles (ParentId);
END;

IF OBJECT_ID(N'dbo.NS_ChildDevelopmentProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildDevelopmentProfiles (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        LanguageLevel int NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_LanguageLevel DEFAULT 3,
        MathLevel int NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_MathLevel DEFAULT 3,
        WorldLevel int NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_WorldLevel DEFAULT 3,
        ExecutiveFunctionLevel int NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_ExecutiveFunctionLevel DEFAULT 3,
        StrengthsSummary nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_StrengthsSummary DEFAULT '',
        SupportSummary nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildDevelopmentProfiles_SupportSummary DEFAULT '',
        AssessedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildDevelopmentProfiles_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildDevelopmentProfiles_ChildId ON dbo.NS_ChildDevelopmentProfiles (ChildId);
END;

IF OBJECT_ID(N'dbo.NS_ChildTeaProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildTeaProfiles (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        CommunicationProfile nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_CommunicationProfile DEFAULT '',
        CommunicationNotes nvarchar(1000) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_CommunicationNotes DEFAULT '',
        AnxietyLevel int NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_AnxietyLevel DEFAULT 3,
        CognitiveRigidityLevel int NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_CognitiveRigidityLevel DEFAULT 3,
        SensorySensitivityLevel int NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_SensorySensitivityLevel DEFAULT 3,
        TransitionDifficultyLevel int NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_TransitionDifficultyLevel DEFAULT 3,
        SupportIntensityLevel int NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_SupportIntensityLevel DEFAULT 3,
        NeedsVisualRoutine bit NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_NeedsVisualRoutine DEFAULT 0,
        NeedsFirstThen bit NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_NeedsFirstThen DEFAULT 0,
        NeedsTimer bit NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_NeedsTimer DEFAULT 0,
        NeedsPlanB bit NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_NeedsPlanB DEFAULT 0,
        SpecialInterests nvarchar(1000) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_SpecialInterests DEFAULT '',
        EffectiveReinforcers nvarchar(1000) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_EffectiveReinforcers DEFAULT '',
        CommonTriggers nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_CommonTriggers DEFAULT '',
        OverloadSignals nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_OverloadSignals DEFAULT '',
        CalmingStrategies nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_CalmingStrategies DEFAULT '',
        TransitionSupports nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_TransitionSupports DEFAULT '',
        DailyLivingPriorities nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_DailyLivingPriorities DEFAULT '',
        ParentPrimaryGoal nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_ParentPrimaryGoal DEFAULT '',
        SchoolBarrierSummary nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_SchoolBarrierSummary DEFAULT '',
        DocumentationNotes nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildTeaProfiles_DocumentationNotes DEFAULT '',
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildTeaProfiles_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildTeaProfiles_ChildId ON dbo.NS_ChildTeaProfiles (ChildId);
END;

IF COL_LENGTH('dbo.NS_ChildProfiles', 'LearningProfile') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildProfiles ADD LearningProfile nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_LearningProfile_Alter DEFAULT 'balanced';
END;

IF COL_LENGTH('dbo.NS_ChildProfiles', 'SupportProfile') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildProfiles ADD SupportProfile nvarchar(20) NOT NULL CONSTRAINT DF_NS_ChildProfiles_SupportProfile_Alter DEFAULT 'General';
END;

IF COL_LENGTH('dbo.NS_ChildProfiles', 'FamilyGoalTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildProfiles ADD FamilyGoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_FamilyGoalTrack_Alter DEFAULT 'balanced_growth';
END;

IF COL_LENGTH('dbo.NS_ChildProfiles', 'TeachingMethodology') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildProfiles ADD TeachingMethodology nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_TeachingMethodology_Alter DEFAULT 'eclectic';
END;

IF COL_LENGTH('dbo.NS_ChildProfiles', 'GuidanceStyle') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildProfiles ADD GuidanceStyle nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildProfiles_GuidanceStyle_Alter DEFAULT 'guided';
END;

IF OBJECT_ID(N'dbo.NS_ChildMonthlySnapshots', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildMonthlySnapshots (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        SessionsCount int NOT NULL,
        MinutesCount int NOT NULL,
        EvidenceCount int NOT NULL,
        LanguageScore int NOT NULL,
        MathScore int NOT NULL,
        WorldScore int NOT NULL,
        ExecutiveFunctionScore int NOT NULL,
        OverallScore int NOT NULL,
        StrongestArea nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildMonthlySnapshots_StrongestArea DEFAULT '',
        AttentionArea nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildMonthlySnapshots_AttentionArea DEFAULT '',
        Summary nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildMonthlySnapshots_Summary DEFAULT '',
        SnapshotMonth datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildMonthlySnapshots_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildMonthlySnapshots_ChildId_Year_Month ON dbo.NS_ChildMonthlySnapshots (ChildId, [Year], [Month]);
END;

IF OBJECT_ID(N'dbo.NS_ChildMonthlyGoalCycles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildMonthlyGoalCycles (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        GoalHeadline nvarchar(240) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalCycles_GoalHeadline DEFAULT '',
        Summary nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalCycles_Summary DEFAULT '',
        RiskLevel nvarchar(30) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalCycles_RiskLevel DEFAULT 'low',
        ProgressPercent int NOT NULL,
        GoalsOnTrack int NOT NULL,
        TotalGoals int NOT NULL,
        LastSessionAt datetime2 NULL,
        RetentionAlertSentAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildMonthlyGoalCycles_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildMonthlyGoalCycles_ChildId_Year_Month ON dbo.NS_ChildMonthlyGoalCycles (ChildId, [Year], [Month]);
END;

IF OBJECT_ID(N'dbo.NS_ChildMonthlyGoalItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildMonthlyGoalItems (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        CycleId uniqueidentifier NOT NULL,
        Domain nvarchar(30) NOT NULL,
        SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalItems_SkillCode DEFAULT '',
        SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalItems_SkillName DEFAULT '',
        StartScore int NOT NULL,
        CurrentScore int NOT NULL,
        TargetScore int NOT NULL,
        PriorityOrder int NOT NULL,
        Status nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildMonthlyGoalItems_Status DEFAULT 'at_risk',
        CONSTRAINT FK_NS_ChildMonthlyGoalItems_NS_ChildMonthlyGoalCycles_CycleId
            FOREIGN KEY (CycleId) REFERENCES dbo.NS_ChildMonthlyGoalCycles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_ChildMonthlyGoalItems_CycleId ON dbo.NS_ChildMonthlyGoalItems (CycleId);
END;

IF OBJECT_ID(N'dbo.NS_ChildRecoveryPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildRecoveryPlans (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        GoalCycleId uniqueidentifier NULL,
        Title nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlans_Title DEFAULT '',
        Summary nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlans_Summary DEFAULT '',
        Status nvarchar(30) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlans_Status DEFAULT 'active',
        StartDate datetime2 NOT NULL,
        EndDate datetime2 NOT NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildRecoveryPlans_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildRecoveryPlans_NS_ChildMonthlyGoalCycles_GoalCycleId
            FOREIGN KEY (GoalCycleId) REFERENCES dbo.NS_ChildMonthlyGoalCycles(Id)
    );
    CREATE INDEX IX_NS_ChildRecoveryPlans_ChildId ON dbo.NS_ChildRecoveryPlans (ChildId);
END;

IF OBJECT_ID(N'dbo.NS_ChildRecoveryPlanDays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildRecoveryPlanDays (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        RecoveryPlanId uniqueidentifier NOT NULL,
        DayNumber int NOT NULL,
        SuggestedDate datetime2 NOT NULL,
        FocusSkill nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlanDays_FocusSkill DEFAULT '',
        GoalText nvarchar(400) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlanDays_GoalText DEFAULT '',
        ParentTip nvarchar(600) NOT NULL CONSTRAINT DF_NS_ChildRecoveryPlanDays_ParentTip DEFAULT '',
        CompletedAt datetime2 NULL,
        CONSTRAINT FK_NS_ChildRecoveryPlanDays_NS_ChildRecoveryPlans_RecoveryPlanId
            FOREIGN KEY (RecoveryPlanId) REFERENCES dbo.NS_ChildRecoveryPlans(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_ChildRecoveryPlanDays_RecoveryPlanId ON dbo.NS_ChildRecoveryPlanDays (RecoveryPlanId);
END;

IF OBJECT_ID(N'dbo.NS_ChildAchievements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildAchievements (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        Code nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildAchievements_Code DEFAULT '',
        Title nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildAchievements_Title DEFAULT '',
        Description nvarchar(500) NOT NULL CONSTRAINT DF_NS_ChildAchievements_Description DEFAULT '',
        EarnedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildAchievements_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildAchievements_ChildId_Code ON dbo.NS_ChildAchievements (ChildId, Code);
END;

IF OBJECT_ID(N'dbo.NS_InterventionPlaybookEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_InterventionPlaybookEntries (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Domain nvarchar(30) NOT NULL,
        GoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_GoalTrack DEFAULT '',
        TriggerCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_TriggerCode DEFAULT '',
        TriggerLabel nvarchar(180) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_TriggerLabel DEFAULT '',
        MatchKeywords nvarchar(500) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_MatchKeywords DEFAULT '',
        StageScope nvarchar(120) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_StageScope DEFAULT 'all',
        Headline nvarchar(220) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_Headline DEFAULT '',
        HowToSpot nvarchar(600) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_HowToSpot DEFAULT '',
        LikelyCause nvarchar(500) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_LikelyCause DEFAULT '',
        WhatToSay nvarchar(600) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_WhatToSay DEFAULT '',
        WhatToAvoid nvarchar(500) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_WhatToAvoid DEFAULT '',
        QuickActivity nvarchar(700) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_QuickActivity DEFAULT '',
        Materials nvarchar(500) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_Materials DEFAULT '',
        SuccessSignal nvarchar(400) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_SuccessSignal DEFAULT '',
        RepeatPlan nvarchar(300) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_RepeatPlan DEFAULT '',
        FallbackAction nvarchar(500) NOT NULL CONSTRAINT DF_NS_InterventionPlaybookEntries_FallbackAction DEFAULT ''
    );
    CREATE UNIQUE INDEX IX_NS_InterventionPlaybookEntries_Domain_TriggerCode_GoalTrack
        ON dbo.NS_InterventionPlaybookEntries (Domain, TriggerCode, GoalTrack);
END;

IF OBJECT_ID(N'dbo.NS_ChildSkillCheckups', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildSkillCheckups (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        Domain nvarchar(30) NOT NULL,
        SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_SkillCode DEFAULT '',
        SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_SkillName DEFAULT '',
        PromptTitle nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_PromptTitle DEFAULT '',
        ParentPrompt nvarchar(900) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_ParentPrompt DEFAULT '',
        SuccessCriteria nvarchar(500) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_SuccessCriteria DEFAULT '',
        ScheduledFor datetime2 NOT NULL,
        CompletedAt datetime2 NULL,
        Rating nvarchar(30) NULL,
        Notes nvarchar(600) NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_Notes DEFAULT '',
        RecalibratedScore int NOT NULL CONSTRAINT DF_NS_ChildSkillCheckups_RecalibratedScore DEFAULT 0,
        CONSTRAINT FK_NS_ChildSkillCheckups_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_ChildSkillCheckups_ChildId_SkillCode_ScheduledFor
        ON dbo.NS_ChildSkillCheckups (ChildId, SkillCode, ScheduledFor);
END;

IF OBJECT_ID(N'dbo.NS_ChildSkillReadinessChecks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildSkillReadinessChecks (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        Domain nvarchar(30) NOT NULL,
        SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_SkillCode DEFAULT '',
        SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_SkillName DEFAULT '',
        Headline nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_Headline DEFAULT '',
        ParentPrompt nvarchar(900) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_ParentPrompt DEFAULT '',
        SuccessCriteria nvarchar(500) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_SuccessCriteria DEFAULT '',
        UnlocksSkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_UnlocksSkillCode DEFAULT '',
        UnlocksSkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_UnlocksSkillName DEFAULT '',
        ScheduledFor datetime2 NOT NULL,
        CompletedAt datetime2 NULL,
        Rating nvarchar(30) NULL,
        Passed bit NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_Passed DEFAULT 0,
        Notes nvarchar(600) NOT NULL CONSTRAINT DF_NS_ChildSkillReadinessChecks_Notes DEFAULT '',
        CONSTRAINT FK_NS_ChildSkillReadinessChecks_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_ChildSkillReadinessChecks_ChildId_SkillCode_ScheduledFor
        ON dbo.NS_ChildSkillReadinessChecks (ChildId, SkillCode, ScheduledFor);
END;

IF OBJECT_ID(N'dbo.NS_CurriculumTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_CurriculumTemplates (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Age int NOT NULL,
        Domain nvarchar(30) NOT NULL,
        SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SupportScope DEFAULT 'General',
        FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_FunctionalTrack DEFAULT 'Base',
        GoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_GoalTrack DEFAULT 'balanced_growth',
        SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SkillCode DEFAULT '',
        PrerequisiteSkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_PrerequisiteSkillCode DEFAULT '',
        SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SkillName DEFAULT '',
        Title nvarchar(180) NOT NULL,
        Goal nvarchar(600) NOT NULL,
        Materials nvarchar(600) NOT NULL,
        ParentGuide nvarchar(1200) NOT NULL,
        ChildMission nvarchar(600) NOT NULL,
        EvidencePrompt nvarchar(600) NOT NULL,
        ReviewAfterDays int NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_ReviewAfterDays DEFAULT 3,
        SortOrder int NOT NULL
    );
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'SkillCode') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SkillCode_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'GoalTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD GoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_GoalTrack_Alter DEFAULT 'balanced_growth';
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'SupportScope') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SupportScope_Alter DEFAULT 'General';
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'FunctionalTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_FunctionalTrack_Alter DEFAULT 'Base';
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'PrerequisiteSkillCode') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD PrerequisiteSkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_PrerequisiteSkillCode_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'ReviewAfterDays') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD ReviewAfterDays int NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_ReviewAfterDays_Alter DEFAULT 3;
END;

IF COL_LENGTH('dbo.NS_CurriculumTemplates', 'SkillName') IS NULL
BEGIN
    ALTER TABLE dbo.NS_CurriculumTemplates ADD SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_CurriculumTemplates_SkillName_Alter DEFAULT '';
END;

IF OBJECT_ID(N'dbo.NS_ChildFavoriteActivities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildFavoriteActivities (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        TemplateId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_NS_ChildFavoriteActivities_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildFavoriteActivities_NS_CurriculumTemplates_TemplateId
            FOREIGN KEY (TemplateId) REFERENCES dbo.NS_CurriculumTemplates(Id)
    );
    CREATE UNIQUE INDEX IX_NS_ChildFavoriteActivities_ChildId_TemplateId
        ON dbo.NS_ChildFavoriteActivities (ChildId, TemplateId);
END;

IF OBJECT_ID(N'dbo.NS_ChildPlanDirectives', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildPlanDirectives (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        PlannedDate datetime2 NOT NULL,
        DirectiveType nvarchar(30) NOT NULL,
        TemplateId uniqueidentifier NULL,
        FunctionalTrack nvarchar(30) NULL,
        Note nvarchar(300) NOT NULL CONSTRAINT DF_NS_ChildPlanDirectives_Note DEFAULT '',
        CreatedAt datetime2 NOT NULL,
        AppliedAt datetime2 NULL,
        CONSTRAINT FK_NS_ChildPlanDirectives_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildPlanDirectives_NS_CurriculumTemplates_TemplateId
            FOREIGN KEY (TemplateId) REFERENCES dbo.NS_CurriculumTemplates(Id)
    );
    CREATE INDEX IX_NS_ChildPlanDirectives_ChildId_PlannedDate_DirectiveType
        ON dbo.NS_ChildPlanDirectives (ChildId, PlannedDate, DirectiveType);
END;

IF OBJECT_ID(N'dbo.NS_DailyPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_DailyPlans (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        PlannedDate datetime2 NOT NULL,
        AgeAtGeneration int NOT NULL,
        Theme nvarchar(180) NOT NULL,
        ParentSummary nvarchar(1500) NOT NULL,
        ChildNarrative nvarchar(1500) NOT NULL,
        IsRecoveryPlan bit NOT NULL CONSTRAINT DF_NS_DailyPlans_IsRecoveryPlan DEFAULT 0,
        RecoveryHeadline nvarchar(220) NOT NULL CONSTRAINT DF_NS_DailyPlans_RecoveryHeadline DEFAULT '',
        Completed bit NOT NULL,
        CONSTRAINT FK_NS_DailyPlans_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_DailyPlans_ChildId_PlannedDate ON dbo.NS_DailyPlans (ChildId, PlannedDate);
END;

IF COL_LENGTH('dbo.NS_DailyPlans', 'IsRecoveryPlan') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlans ADD IsRecoveryPlan bit NOT NULL CONSTRAINT DF_NS_DailyPlans_IsRecoveryPlan_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_DailyPlans', 'RecoveryHeadline') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlans ADD RecoveryHeadline nvarchar(220) NOT NULL CONSTRAINT DF_NS_DailyPlans_RecoveryHeadline_Alter DEFAULT '';
END;

IF OBJECT_ID(N'dbo.NS_DailyPlanBlocks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_DailyPlanBlocks (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        DailyPlanId uniqueidentifier NOT NULL,
        Domain nvarchar(30) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_Domain DEFAULT 'Language',
        SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SupportScope DEFAULT 'General',
        FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_FunctionalTrack DEFAULT 'Base',
        SourceTemplateId uniqueidentifier NULL,
        SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SkillCode DEFAULT '',
        SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SkillName DEFAULT '',
        Title nvarchar(180) NOT NULL,
        Goal nvarchar(700) NOT NULL,
        ParentGuide nvarchar(1500) NOT NULL,
        ChildPrompt nvarchar(700) NOT NULL,
        Materials nvarchar(700) NOT NULL,
        EvidencePrompt nvarchar(700) NOT NULL,
        IsRecoveryFocus bit NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_IsRecoveryFocus DEFAULT 0,
        RecoveryNote nvarchar(500) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_RecoveryNote DEFAULT '',
        IsSpacedReview bit NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_IsSpacedReview DEFAULT 0,
        ReviewNote nvarchar(500) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_ReviewNote DEFAULT '',
        DurationMinutes int NOT NULL,
        SortOrder int NOT NULL,
        CONSTRAINT FK_NS_DailyPlanBlocks_NS_DailyPlans_DailyPlanId
            FOREIGN KEY (DailyPlanId) REFERENCES dbo.NS_DailyPlans(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_DailyPlanBlocks_NS_CurriculumTemplates_SourceTemplateId
            FOREIGN KEY (SourceTemplateId) REFERENCES dbo.NS_CurriculumTemplates(Id)
    );
    CREATE INDEX IX_NS_DailyPlanBlocks_DailyPlanId ON dbo.NS_DailyPlanBlocks (DailyPlanId);
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'IsRecoveryFocus') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD IsRecoveryFocus bit NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_IsRecoveryFocus_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'RecoveryNote') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD RecoveryNote nvarchar(500) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_RecoveryNote_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'IsSpacedReview') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD IsSpacedReview bit NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_IsSpacedReview_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'ReviewNote') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD ReviewNote nvarchar(500) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_ReviewNote_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'Domain') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD Domain nvarchar(30) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_Domain_Alter DEFAULT 'Language';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'SkillCode') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD SkillCode nvarchar(80) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SkillCode_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'SupportScope') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SupportScope_Alter DEFAULT 'General';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'FunctionalTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_FunctionalTrack_Alter DEFAULT 'Base';
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'SourceTemplateId') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD SourceTemplateId uniqueidentifier NULL;
    ALTER TABLE dbo.NS_DailyPlanBlocks
        ADD CONSTRAINT FK_NS_DailyPlanBlocks_NS_CurriculumTemplates_SourceTemplateId_Alter
        FOREIGN KEY (SourceTemplateId) REFERENCES dbo.NS_CurriculumTemplates(Id);
END;

IF COL_LENGTH('dbo.NS_DailyPlanBlocks', 'SkillName') IS NULL
BEGIN
    ALTER TABLE dbo.NS_DailyPlanBlocks ADD SkillName nvarchar(180) NOT NULL CONSTRAINT DF_NS_DailyPlanBlocks_SkillName_Alter DEFAULT '';
END;

IF OBJECT_ID(N'dbo.NS_DailyPlanBlockCompletions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_DailyPlanBlockCompletions (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        DailyPlanId uniqueidentifier NOT NULL,
        DailyPlanBlockId uniqueidentifier NOT NULL,
        CompletedAt datetime2 NOT NULL,
        Notes nvarchar(400) NOT NULL CONSTRAINT DF_NS_DailyPlanBlockCompletions_Notes DEFAULT '',
        CompletionSource nvarchar(40) NOT NULL CONSTRAINT DF_NS_DailyPlanBlockCompletions_CompletionSource DEFAULT 'guided_flow',
        CONSTRAINT FK_NS_DailyPlanBlockCompletions_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_NS_DailyPlanBlockCompletions_NS_DailyPlans_DailyPlanId
            FOREIGN KEY (DailyPlanId) REFERENCES dbo.NS_DailyPlans(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_NS_DailyPlanBlockCompletions_NS_DailyPlanBlocks_DailyPlanBlockId
            FOREIGN KEY (DailyPlanBlockId) REFERENCES dbo.NS_DailyPlanBlocks(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_DailyPlanBlockCompletions_ChildId_DailyPlanBlockId
        ON dbo.NS_DailyPlanBlockCompletions (ChildId, DailyPlanBlockId);
    CREATE INDEX IX_NS_DailyPlanBlockCompletions_DailyPlanId
        ON dbo.NS_DailyPlanBlockCompletions (DailyPlanId);
END;

IF OBJECT_ID(N'dbo.NS_LearningSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_LearningSessions (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        DailyPlanId uniqueidentifier NOT NULL,
        LoggedAt datetime2 NOT NULL,
        MinutesCompleted int NOT NULL,
        Wins nvarchar(1200) NOT NULL,
        Challenges nvarchar(1200) NOT NULL,
        Notes nvarchar(1200) NOT NULL,
        MediaUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaUrl DEFAULT '',
        MediaContentType nvarchar(120) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaContentType DEFAULT '',
        MediaFileName nvarchar(260) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaFileName DEFAULT '',
        MediaStorageProvider nvarchar(40) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaStorageProvider DEFAULT '',
        MediaStorageKey nvarchar(600) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaStorageKey DEFAULT '',
        MediaThumbnailUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaThumbnailUrl DEFAULT '',
        MediaThumbnailStorageKey nvarchar(600) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaThumbnailStorageKey DEFAULT '',
        CONSTRAINT FK_NS_LearningSessions_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_NS_LearningSessions_NS_DailyPlans_DailyPlanId
            FOREIGN KEY (DailyPlanId) REFERENCES dbo.NS_DailyPlans(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_NS_LearningSessions_ChildId ON dbo.NS_LearningSessions (ChildId);
    CREATE INDEX IX_NS_LearningSessions_DailyPlanId ON dbo.NS_LearningSessions (DailyPlanId);
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaUrl') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaUrl_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaContentType') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaContentType nvarchar(120) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaContentType_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaFileName') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaFileName nvarchar(260) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaFileName_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaStorageProvider') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaStorageProvider nvarchar(40) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaStorageProvider_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaStorageKey') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaStorageKey nvarchar(600) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaStorageKey_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaThumbnailUrl') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaThumbnailUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaThumbnailUrl_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_LearningSessions', 'MediaThumbnailStorageKey') IS NULL
BEGIN
    ALTER TABLE dbo.NS_LearningSessions ADD MediaThumbnailStorageKey nvarchar(600) NOT NULL CONSTRAINT DF_NS_LearningSessions_MediaThumbnailStorageKey_Alter DEFAULT '';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_NS_LearningSessions_ChildId_LoggedAt'
      AND object_id = OBJECT_ID(N'dbo.NS_LearningSessions')
)
BEGIN
    CREATE INDEX IX_NS_LearningSessions_ChildId_LoggedAt
        ON dbo.NS_LearningSessions (ChildId, LoggedAt);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_NS_LearningSessions_ChildId_WithMedia'
      AND object_id = OBJECT_ID(N'dbo.NS_LearningSessions')
)
BEGIN
    CREATE INDEX IX_NS_LearningSessions_ChildId_WithMedia
        ON dbo.NS_LearningSessions (ChildId)
        WHERE MediaUrl <> '';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_NS_DailyPlanBlockCompletions_ChildId_DailyPlanId'
      AND object_id = OBJECT_ID(N'dbo.NS_DailyPlanBlockCompletions')
)
BEGIN
    CREATE INDEX IX_NS_DailyPlanBlockCompletions_ChildId_DailyPlanId
        ON dbo.NS_DailyPlanBlockCompletions (ChildId, DailyPlanId);
END;

IF OBJECT_ID(N'dbo.NS_ChildRoutineObservations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildRoutineObservations (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        SessionId uniqueidentifier NULL,
        DailyPlanId uniqueidentifier NULL,
        ObservedAt datetime2 NOT NULL,
        ContextPeriod nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_ContextPeriod DEFAULT '',
        Antecedent nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_Antecedent DEFAULT '',
        ChildReaction nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_ChildReaction DEFAULT '',
        WhatHelped nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_WhatHelped DEFAULT '',
        SupportUsed nvarchar(600) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_SupportUsed DEFAULT '',
        DistressLevel int NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_DistressLevel DEFAULT 2,
        TaskToleranceMinutes int NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_TaskToleranceMinutes DEFAULT 10,
        NeededPlanB bit NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_NeededPlanB DEFAULT 0,
        VisualSupportHelped bit NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_VisualSupportHelped DEFAULT 0,
        BreakHelped bit NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_BreakHelped DEFAULT 0,
        CoRegulationHelped bit NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_CoRegulationHelped DEFAULT 0,
        Notes nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildRoutineObservations_Notes DEFAULT '',
        CONSTRAINT FK_NS_ChildRoutineObservations_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildRoutineObservations_NS_LearningSessions_SessionId
            FOREIGN KEY (SessionId) REFERENCES dbo.NS_LearningSessions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_NS_ChildRoutineObservations_NS_DailyPlans_DailyPlanId
            FOREIGN KEY (DailyPlanId) REFERENCES dbo.NS_DailyPlans(Id)
    );
    CREATE INDEX IX_NS_ChildRoutineObservations_ChildId_ObservedAt
        ON dbo.NS_ChildRoutineObservations (ChildId, ObservedAt);
END;

IF OBJECT_ID(N'dbo.NS_ChildSkillProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildSkillProgress (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        Age int NOT NULL,
        Domain nvarchar(30) NOT NULL,
        SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SupportScope DEFAULT 'General',
        FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_FunctionalTrack DEFAULT 'Base',
        SkillCode nvarchar(80) NOT NULL,
        SkillName nvarchar(180) NOT NULL,
        SkillStage nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SkillStage DEFAULT 'starting',
        MasteryScore int NOT NULL,
        TimesPracticed int NOT NULL,
        TimesSuccessful int NOT NULL,
        SecureStreak int NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SecureStreak DEFAULT 0,
        StruggleStreak int NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_StruggleStreak DEFAULT 0,
        ReadyToAdvance bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_ReadyToAdvance DEFAULT 0,
        NeedsReadinessCheck bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NeedsReadinessCheck DEFAULT 0,
        ReadinessApproved bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_ReadinessApproved DEFAULT 0,
        NeedsRemediation bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NeedsRemediation DEFAULT 0,
        StageChangedAt datetime2 NULL,
        LastPracticedAt datetime2 NULL,
        LastReadinessCheckAt datetime2 NULL,
        NextReviewAt datetime2 NULL,
        NextMilestone nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NextMilestone DEFAULT 'Comecar a praticar com apoio',
        RemediationPlan nvarchar(500) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_RemediationPlan DEFAULT '',
        Recommendation nvarchar(180) NOT NULL,
        CONSTRAINT FK_NS_ChildSkillProgress_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildSkillProgress_ChildId_SkillCode ON dbo.NS_ChildSkillProgress (ChildId, SkillCode);
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'SkillStage') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD SkillStage nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SkillStage_Alter DEFAULT 'starting';
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'SupportScope') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD SupportScope nvarchar(20) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SupportScope_Alter DEFAULT 'General';
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'FunctionalTrack') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_FunctionalTrack_Alter DEFAULT 'Base';
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'SecureStreak') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD SecureStreak int NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_SecureStreak_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'StruggleStreak') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD StruggleStreak int NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_StruggleStreak_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'ReadyToAdvance') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD ReadyToAdvance bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_ReadyToAdvance_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'NeedsReadinessCheck') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD NeedsReadinessCheck bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NeedsReadinessCheck_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'ReadinessApproved') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD ReadinessApproved bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_ReadinessApproved_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'NeedsRemediation') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD NeedsRemediation bit NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NeedsRemediation_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'StageChangedAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD StageChangedAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'LastReadinessCheckAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD LastReadinessCheckAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'NextReviewAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD NextReviewAt datetime2 NULL;
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'NextMilestone') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD NextMilestone nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_NextMilestone_Alter DEFAULT 'Comecar a praticar com apoio';
END;

IF COL_LENGTH('dbo.NS_ChildSkillProgress', 'RemediationPlan') IS NULL
BEGIN
    ALTER TABLE dbo.NS_ChildSkillProgress ADD RemediationPlan nvarchar(500) NOT NULL CONSTRAINT DF_NS_ChildSkillProgress_RemediationPlan_Alter DEFAULT '';
END;

IF OBJECT_ID(N'dbo.NS_LearningBlockFeedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_LearningBlockFeedback (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        SessionId uniqueidentifier NOT NULL,
        DailyPlanBlockId uniqueidentifier NOT NULL,
        SkillCode nvarchar(80) NOT NULL,
        Rating nvarchar(30) NOT NULL,
        Notes nvarchar(500) NOT NULL,
        CONSTRAINT FK_NS_LearningBlockFeedback_NS_LearningSessions_SessionId
            FOREIGN KEY (SessionId) REFERENCES dbo.NS_LearningSessions(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_LearningBlockFeedback_NS_DailyPlanBlocks_DailyPlanBlockId
            FOREIGN KEY (DailyPlanBlockId) REFERENCES dbo.NS_DailyPlanBlocks(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_NS_LearningBlockFeedback_SessionId ON dbo.NS_LearningBlockFeedback (SessionId);
    CREATE INDEX IX_NS_LearningBlockFeedback_DailyPlanBlockId ON dbo.NS_LearningBlockFeedback (DailyPlanBlockId);
END;

IF OBJECT_ID(N'dbo.NS_TrackAcquisitionSnapshots', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_TrackAcquisitionSnapshots (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TrackCode nvarchar(40) NOT NULL CONSTRAINT DF_NS_TrackAcquisitionSnapshots_TrackCode DEFAULT '',
        UserId uniqueidentifier NOT NULL,
        ChildId uniqueidentifier NULL,
        CapturedAt datetime2 NOT NULL,
        HasSubscription bit NOT NULL CONSTRAINT DF_NS_TrackAcquisitionSnapshots_HasSubscription DEFAULT 0,
        SubscriptionCapturedAt datetime2 NULL,
        IsRetained bit NOT NULL CONSTRAINT DF_NS_TrackAcquisitionSnapshots_IsRetained DEFAULT 0,
        LastRetentionCheckAt datetime2 NULL,
        CONSTRAINT FK_NS_TrackAcquisitionSnapshots_NS_AppUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.NS_AppUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_TrackAcquisitionSnapshots_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id)
    );
    CREATE UNIQUE INDEX IX_NS_TrackAcquisitionSnapshots_TrackCode_UserId ON dbo.NS_TrackAcquisitionSnapshots (TrackCode, UserId);
END;

IF OBJECT_ID(N'dbo.NS_CuratedLearningResources', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_CuratedLearningResources (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Slug nvarchar(120) NOT NULL,
        Title nvarchar(220) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_Title DEFAULT '',
        Summary nvarchar(900) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_Summary DEFAULT '',
        UseNote nvarchar(900) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_UseNote DEFAULT '',
        Domain nvarchar(30) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_Domain DEFAULT 'Language',
        AgeMin int NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_AgeMin DEFAULT 3,
        AgeMax int NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_AgeMax DEFAULT 10,
        FormatLabel nvarchar(80) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_FormatLabel DEFAULT '',
        ResourceKind nvarchar(80) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_ResourceKind DEFAULT '',
        SourceName nvarchar(120) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_SourceName DEFAULT '',
        SourceUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_SourceUrl DEFAULT '',
        AccessUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_AccessUrl DEFAULT '',
        IsHostedLocally bit NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_IsHostedLocally DEFAULT 0,
        LicenseLabel nvarchar(80) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_LicenseLabel DEFAULT '',
        Attribution nvarchar(400) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_Attribution DEFAULT '',
        LanguageCode nvarchar(20) NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_LanguageCode DEFAULT 'pt-BR',
        SortOrder int NOT NULL CONSTRAINT DF_NS_CuratedLearningResources_SortOrder DEFAULT 0
    );
    CREATE UNIQUE INDEX IX_NS_CuratedLearningResources_Slug ON dbo.NS_CuratedLearningResources (Slug);
END;

IF OBJECT_ID(N'dbo.NS_CuratedTaskTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_CuratedTaskTemplates (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Slug nvarchar(120) NOT NULL,
        Title nvarchar(220) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_Title DEFAULT '',
        Domain nvarchar(30) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_Domain DEFAULT 'Language',
        FunctionalTrack nvarchar(30) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_FunctionalTrack DEFAULT 'Base',
        AgeMin int NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_AgeMin DEFAULT 3,
        AgeMax int NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_AgeMax DEFAULT 10,
        GoalTrack nvarchar(40) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_GoalTrack DEFAULT 'balanced_growth',
        MatchKeywords nvarchar(600) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_MatchKeywords DEFAULT '',
        Goal nvarchar(900) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_Goal DEFAULT '',
        ParentGuide nvarchar(1500) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_ParentGuide DEFAULT '',
        ChildPrompt nvarchar(800) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_ChildPrompt DEFAULT '',
        TaskSteps nvarchar(2000) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_TaskSteps DEFAULT '',
        MaterialsSummary nvarchar(900) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_MaterialsSummary DEFAULT '',
        EvidencePrompt nvarchar(900) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_EvidencePrompt DEFAULT '',
        ExpectedOutcome nvarchar(700) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_ExpectedOutcome DEFAULT '',
        SuggestedMinutes int NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_SuggestedMinutes DEFAULT 15,
        PrimaryResourceId uniqueidentifier NULL,
        SupportLinkLabel nvarchar(120) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_SupportLinkLabel DEFAULT '',
        SupportLinkUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_SupportLinkUrl DEFAULT '',
        SupportLinkSource nvarchar(120) NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_SupportLinkSource DEFAULT '',
        SortOrder int NOT NULL CONSTRAINT DF_NS_CuratedTaskTemplates_SortOrder DEFAULT 0,
        CONSTRAINT FK_NS_CuratedTaskTemplates_NS_CuratedLearningResources_PrimaryResourceId
            FOREIGN KEY (PrimaryResourceId) REFERENCES dbo.NS_CuratedLearningResources(Id)
    );
    CREATE UNIQUE INDEX IX_NS_CuratedTaskTemplates_Slug ON dbo.NS_CuratedTaskTemplates (Slug);
    CREATE INDEX IX_NS_CuratedTaskTemplates_Domain_AgeMin_AgeMax
        ON dbo.NS_CuratedTaskTemplates (Domain, AgeMin, AgeMax);
END;

IF OBJECT_ID(N'dbo.NS_ChildExternalContentProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildExternalContentProgress (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        ContentSlug nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildExternalContentProgress_ContentSlug DEFAULT '',
        ContentTitle nvarchar(220) NOT NULL CONSTRAINT DF_NS_ChildExternalContentProgress_ContentTitle DEFAULT '',
        Provider nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildExternalContentProgress_Provider DEFAULT '',
        AreaLabel nvarchar(120) NOT NULL CONSTRAINT DF_NS_ChildExternalContentProgress_AreaLabel DEFAULT '',
        CompletedAt datetime2 NOT NULL,
        Notes nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildExternalContentProgress_Notes DEFAULT '',
        CONSTRAINT FK_NS_ChildExternalContentProgress_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildExternalContentProgress_ChildId_ContentSlug
        ON dbo.NS_ChildExternalContentProgress (ChildId, ContentSlug);
    CREATE INDEX IX_NS_ChildExternalContentProgress_ChildId_CompletedAt
        ON dbo.NS_ChildExternalContentProgress (ChildId, CompletedAt);
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
