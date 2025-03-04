-- Create Documents table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Documents] (
        [Id] NVARCHAR(50) PRIMARY KEY,
        [Name] NVARCHAR(255) NOT NULL,
        [StoragePath] NVARCHAR(1000) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [Size] BIGINT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
    )
END
GO

-- Create Document stored procedures
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CreateDocument]') AND type in (N'P'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_CreateDocument]
        @Name NVARCHAR(255),
        @StoragePath NVARCHAR(1000),
        @ContentType NVARCHAR(100),
        @Size BIGINT,
        @CreatedAt DATETIME2,
        @DocumentId NVARCHAR(50) OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;
        SET @DocumentId = NEWID();
        
        INSERT INTO [dbo].[Documents] ([Id], [Name], [StoragePath], [ContentType], [Size], [CreatedAt])
        VALUES (@DocumentId, @Name, @StoragePath, @ContentType, @Size, @CreatedAt);
        
        RETURN 0;
    END')
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetDocument]') AND type in (N'P'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_GetDocument]
        @DocumentId NVARCHAR(50)
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT [Id], [Name], [StoragePath], [ContentType], [Size], [CreatedAt]
        FROM [dbo].[Documents]
        WHERE [Id] = @DocumentId;
        
        RETURN 0;
    END')
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ListDocuments]') AND type in (N'P'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_ListDocuments]
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT [Id], [Name], [StoragePath], [ContentType], [Size], [CreatedAt]
        FROM [dbo].[Documents];
        
        RETURN 0;
    END')
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DeleteDocument]') AND type in (N'P'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_DeleteDocument]
        @DocumentId NVARCHAR(50)
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DELETE FROM [dbo].[Documents]
        WHERE [Id] = @DocumentId;
        
        RETURN 0;
    END')
END
GO