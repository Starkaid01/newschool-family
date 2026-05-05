using Microsoft.EntityFrameworkCore;

namespace NewSchool.Web.Data;

public static class AssessmentSchemaInitializer
{
    public static async Task EnsureSchemaAsync(ApplicationDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.NS_ChildLessonAssessments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildLessonAssessments (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        ParentUserId uniqueidentifier NOT NULL,
        DailyPlanId uniqueidentifier NOT NULL,
        DailyPlanBlockId uniqueidentifier NOT NULL,
        Domain nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_Domain DEFAULT '',
        SchoolYearNumber int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_SchoolYearNumber DEFAULT 1,
        PhaseNumber int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_PhaseNumber DEFAULT 1,
        PhaseLabel nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_PhaseLabel DEFAULT '',
        SubjectLabel nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_SubjectLabel DEFAULT '',
        LessonTitle nvarchar(260) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_LessonTitle DEFAULT '',
        UnitTitle nvarchar(260) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_UnitTitle DEFAULT '',
        AssessmentTitle nvarchar(260) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_AssessmentTitle DEFAULT '',
        PrintableHeadline nvarchar(260) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_PrintableHeadline DEFAULT '',
        PrintableSummary nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_PrintableSummary DEFAULT '',
        QuestionCount int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_QuestionCount DEFAULT 0,
        CorrectCount int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_CorrectCount DEFAULT 0,
        ScorePercent int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_ScorePercent DEFAULT 0,
        ScoreValue decimal(5,2) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_ScoreValue DEFAULT 0,
        IsCompleted bit NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_IsCompleted DEFAULT 0,
        PrintedAtUtc datetime2 NULL,
        CorrectedAtUtc datetime2 NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_ChildLessonAssessments_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_NS_ChildLessonAssessments_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildLessonAssessments_NS_AppUsers_ParentUserId
            FOREIGN KEY (ParentUserId) REFERENCES dbo.NS_AppUsers(Id),
        CONSTRAINT FK_NS_ChildLessonAssessments_NS_DailyPlans_DailyPlanId
            FOREIGN KEY (DailyPlanId) REFERENCES dbo.NS_DailyPlans(Id),
        CONSTRAINT FK_NS_ChildLessonAssessments_NS_DailyPlanBlocks_DailyPlanBlockId
            FOREIGN KEY (DailyPlanBlockId) REFERENCES dbo.NS_DailyPlanBlocks(Id)
    );
    CREATE UNIQUE INDEX IX_NS_ChildLessonAssessments_ChildId_DailyPlanBlockId
        ON dbo.NS_ChildLessonAssessments (ChildId, DailyPlanBlockId);
    CREATE INDEX IX_NS_ChildLessonAssessments_ChildId_Domain
        ON dbo.NS_ChildLessonAssessments (ChildId, Domain);
END;

IF OBJECT_ID(N'dbo.NS_ChildLessonAssessmentItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildLessonAssessmentItems (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        AssessmentId uniqueidentifier NOT NULL,
        SortOrder int NOT NULL CONSTRAINT DF_NS_ChildLessonAssessmentItems_SortOrder DEFAULT 1,
        Prompt nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessmentItems_Prompt DEFAULT '',
        ExpectedAnswer nvarchar(1200) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessmentItems_ExpectedAnswer DEFAULT '',
        TeacherNote nvarchar(800) NOT NULL CONSTRAINT DF_NS_ChildLessonAssessmentItems_TeacherNote DEFAULT '',
        IsCorrect bit NULL,
        CONSTRAINT FK_NS_ChildLessonAssessmentItems_NS_ChildLessonAssessments_AssessmentId
            FOREIGN KEY (AssessmentId) REFERENCES dbo.NS_ChildLessonAssessments(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildLessonAssessmentItems_AssessmentId_SortOrder
        ON dbo.NS_ChildLessonAssessmentItems (AssessmentId, SortOrder);
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
