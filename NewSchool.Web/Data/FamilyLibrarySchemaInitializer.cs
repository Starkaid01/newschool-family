using Microsoft.EntityFrameworkCore;

namespace NewSchool.Web.Data;

public static class FamilyLibrarySchemaInitializer
{
    public static async Task EnsureSchemaAsync(ApplicationDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.NS_FamilyLibraryMaterials', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_FamilyLibraryMaterials (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Title nvarchar(260) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_Title DEFAULT '',
        Category nvarchar(160) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_Category DEFAULT '',
        EducationStage nvarchar(120) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_EducationStage DEFAULT '',
        RecommendedMinAge int NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_RecommendedMinAge DEFAULT 0,
        RecommendedMaxAge int NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_RecommendedMaxAge DEFAULT 0,
        SkillFocus nvarchar(240) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_SkillFocus DEFAULT '',
        Description nvarchar(900) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_Description DEFAULT '',
        CollectionLabel nvarchar(120) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_CollectionLabel DEFAULT '',
        IsPrintable bit NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_IsPrintable DEFAULT 0,
        PageCount int NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_PageCount DEFAULT 0,
        HasIllustrations bit NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_HasIllustrations DEFAULT 0,
        CoverImageRelativePath nvarchar(400) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_CoverImageRelativePath DEFAULT '',
        SourceRelativePath nvarchar(400) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_SourceRelativePath DEFAULT '',
        SourceSyncToken nvarchar(200) NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_SourceSyncToken DEFAULT '',
        SourceUpdatedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_SourceUpdatedAtUtc DEFAULT '2000-01-01',
        SyncedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_FamilyLibraryMaterials_SyncedAtUtc DEFAULT '2000-01-01'
    );
    CREATE INDEX IX_NS_FamilyLibraryMaterials_IsPrintable_EducationStage_Category
        ON dbo.NS_FamilyLibraryMaterials (IsPrintable, EducationStage, Category);
END;

IF OBJECT_ID(N'dbo.NS_FamilyLibraryPages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_FamilyLibraryPages (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        MaterialId uniqueidentifier NOT NULL,
        PageNumber int NOT NULL,
        TextContent nvarchar(max) NOT NULL CONSTRAINT DF_NS_FamilyLibraryPages_TextContent DEFAULT '',
        ImageRelativePath nvarchar(400) NOT NULL CONSTRAINT DF_NS_FamilyLibraryPages_ImageRelativePath DEFAULT '',
        CONSTRAINT FK_NS_FamilyLibraryPages_NS_FamilyLibraryMaterials_MaterialId
            FOREIGN KEY (MaterialId) REFERENCES dbo.NS_FamilyLibraryMaterials(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_FamilyLibraryPages_MaterialId_PageNumber
        ON dbo.NS_FamilyLibraryPages (MaterialId, PageNumber);
END;

IF OBJECT_ID(N'dbo.NS_FamilyLibraryUserStates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_FamilyLibraryUserStates (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        MaterialId uniqueidentifier NOT NULL,
        CurrentPageNumber int NOT NULL CONSTRAINT DF_NS_FamilyLibraryUserStates_CurrentPageNumber DEFAULT 1,
        IsFavorite bit NOT NULL CONSTRAINT DF_NS_FamilyLibraryUserStates_IsFavorite DEFAULT 0,
        IsCompleted bit NOT NULL CONSTRAINT DF_NS_FamilyLibraryUserStates_IsCompleted DEFAULT 0,
        StartedAtUtc datetime2 NULL,
        LastReadAtUtc datetime2 NULL,
        CompletedAtUtc datetime2 NULL,
        UpdatedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_FamilyLibraryUserStates_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_NS_FamilyLibraryUserStates_NS_AppUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.NS_AppUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_FamilyLibraryUserStates_NS_FamilyLibraryMaterials_MaterialId
            FOREIGN KEY (MaterialId) REFERENCES dbo.NS_FamilyLibraryMaterials(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_FamilyLibraryUserStates_UserId_MaterialId
        ON dbo.NS_FamilyLibraryUserStates (UserId, MaterialId);
END;

IF OBJECT_ID(N'dbo.NS_ChildLibraryReadingProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_ChildLibraryReadingProgress (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ChildId uniqueidentifier NOT NULL,
        ParentUserId uniqueidentifier NOT NULL,
        MaterialId uniqueidentifier NOT NULL,
        PhaseNumber int NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_PhaseNumber DEFAULT 1,
        PhaseLabel nvarchar(80) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_PhaseLabel DEFAULT '',
        PeriodKey nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_PeriodKey DEFAULT '',
        WeekNumber int NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_WeekNumber DEFAULT 1,
        CompletionKind nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_CompletionKind DEFAULT 'weekly_reading',
        GoalLabel nvarchar(180) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_GoalLabel DEFAULT '',
        Notes nvarchar(600) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_Notes DEFAULT '',
        CompletedAtUtc datetime2 NOT NULL,
        UpdatedAtUtc datetime2 NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_NS_ChildLibraryReadingProgress_NS_ChildProfiles_ChildId
            FOREIGN KEY (ChildId) REFERENCES dbo.NS_ChildProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_ChildLibraryReadingProgress_NS_AppUsers_ParentUserId
            FOREIGN KEY (ParentUserId) REFERENCES dbo.NS_AppUsers(Id),
        CONSTRAINT FK_NS_ChildLibraryReadingProgress_NS_FamilyLibraryMaterials_MaterialId
            FOREIGN KEY (MaterialId) REFERENCES dbo.NS_FamilyLibraryMaterials(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_NS_ChildLibraryReadingProgress_ChildId_MaterialId_PhaseNumber_CompletionKind_PeriodKey
        ON dbo.NS_ChildLibraryReadingProgress (ChildId, MaterialId, PhaseNumber, CompletionKind, PeriodKey);
    CREATE INDEX IX_NS_ChildLibraryReadingProgress_ChildId_CompletedAtUtc
        ON dbo.NS_ChildLibraryReadingProgress (ChildId, CompletedAtUtc DESC);
END;

IF OBJECT_ID(N'dbo.NS_ChildLibraryReadingProgress', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.NS_ChildLibraryReadingProgress', N'PeriodKey') IS NULL
    BEGIN
        ALTER TABLE dbo.NS_ChildLibraryReadingProgress
            ADD PeriodKey nvarchar(40) NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_PeriodKey_Legacy DEFAULT '';
    END;

    IF COL_LENGTH(N'dbo.NS_ChildLibraryReadingProgress', N'WeekNumber') IS NULL
    BEGIN
        ALTER TABLE dbo.NS_ChildLibraryReadingProgress
            ADD WeekNumber int NOT NULL CONSTRAINT DF_NS_ChildLibraryReadingProgress_WeekNumber_Legacy DEFAULT 1;
    END;

    EXEC(N'
        UPDATE dbo.NS_ChildLibraryReadingProgress
        SET PeriodKey = CONCAT(
                DATEPART(year, CompletedAtUtc),
                ''-'',
                RIGHT(''0'' + CAST(DATEPART(month, CompletedAtUtc) AS varchar(2)), 2),
                ''-W'',
                CAST(CASE
                    WHEN DATEPART(day, CompletedAtUtc) <= 7 THEN 1
                    WHEN DATEPART(day, CompletedAtUtc) <= 14 THEN 2
                    WHEN DATEPART(day, CompletedAtUtc) <= 21 THEN 3
                    ELSE 4
                END AS varchar(1))
            )
        WHERE ISNULL(PeriodKey, N'''') = N'''';
    ');

    EXEC(N'
        UPDATE dbo.NS_ChildLibraryReadingProgress
        SET WeekNumber = CASE
                WHEN DATEPART(day, CompletedAtUtc) <= 7 THEN 1
                WHEN DATEPART(day, CompletedAtUtc) <= 14 THEN 2
                WHEN DATEPART(day, CompletedAtUtc) <= 21 THEN 3
                ELSE 4
            END
        WHERE WeekNumber IS NULL OR WeekNumber <= 0;
    ');

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_NS_ChildLibraryReadingProgress_ChildId_MaterialId_PhaseNumber_CompletionKind'
          AND object_id = OBJECT_ID(N'dbo.NS_ChildLibraryReadingProgress'))
    BEGIN
        DROP INDEX IX_NS_ChildLibraryReadingProgress_ChildId_MaterialId_PhaseNumber_CompletionKind
            ON dbo.NS_ChildLibraryReadingProgress;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_NS_ChildLibraryReadingProgress_ChildId_MaterialId_PhaseNumber_CompletionKind_PeriodKey'
          AND object_id = OBJECT_ID(N'dbo.NS_ChildLibraryReadingProgress'))
    BEGIN
        CREATE UNIQUE INDEX IX_NS_ChildLibraryReadingProgress_ChildId_MaterialId_PhaseNumber_CompletionKind_PeriodKey
            ON dbo.NS_ChildLibraryReadingProgress (ChildId, MaterialId, PhaseNumber, CompletionKind, PeriodKey);
    END;
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
