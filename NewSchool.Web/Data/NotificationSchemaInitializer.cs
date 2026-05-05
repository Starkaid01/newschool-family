using Microsoft.EntityFrameworkCore;

namespace NewSchool.Web.Data;

public static class NotificationSchemaInitializer
{
    public static async Task EnsureSchemaAsync(ApplicationDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.NS_UserNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NS_UserNotifications (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        SentByAdminId uniqueidentifier NULL,
        Title nvarchar(180) NOT NULL CONSTRAINT DF_NS_UserNotifications_Title DEFAULT '',
        Message nvarchar(1500) NOT NULL CONSTRAINT DF_NS_UserNotifications_Message DEFAULT '',
        NotificationLevel nvarchar(20) NOT NULL CONSTRAINT DF_NS_UserNotifications_NotificationLevel DEFAULT 'info',
        ActionUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_UserNotifications_ActionUrl DEFAULT '',
        IsSystemGenerated bit NOT NULL CONSTRAINT DF_NS_UserNotifications_IsSystemGenerated DEFAULT 0,
        CreatedAt datetime2 NOT NULL,
        ReadAt datetime2 NULL,
        CONSTRAINT FK_NS_UserNotifications_NS_AppUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.NS_AppUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NS_UserNotifications_NS_AppUsers_SentByAdminId
            FOREIGN KEY (SentByAdminId) REFERENCES dbo.NS_AppUsers(Id)
    );
    CREATE INDEX IX_NS_UserNotifications_UserId_CreatedAt
        ON dbo.NS_UserNotifications (UserId, CreatedAt DESC);
    CREATE INDEX IX_NS_UserNotifications_UserId_ReadAt
        ON dbo.NS_UserNotifications (UserId, ReadAt);
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'Title') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD Title nvarchar(180) NOT NULL CONSTRAINT DF_NS_UserNotifications_Title_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'Message') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD Message nvarchar(1500) NOT NULL CONSTRAINT DF_NS_UserNotifications_Message_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'NotificationLevel') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD NotificationLevel nvarchar(20) NOT NULL CONSTRAINT DF_NS_UserNotifications_NotificationLevel_Alter DEFAULT 'info';
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'ActionUrl') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD ActionUrl nvarchar(500) NOT NULL CONSTRAINT DF_NS_UserNotifications_ActionUrl_Alter DEFAULT '';
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'IsSystemGenerated') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD IsSystemGenerated bit NOT NULL CONSTRAINT DF_NS_UserNotifications_IsSystemGenerated_Alter DEFAULT 0;
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_NS_UserNotifications_CreatedAt_Alter DEFAULT SYSUTCDATETIME();
END;

IF COL_LENGTH('dbo.NS_UserNotifications', 'ReadAt') IS NULL
BEGIN
    ALTER TABLE dbo.NS_UserNotifications ADD ReadAt datetime2 NULL;
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
