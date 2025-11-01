-- Verify UserInterests table exists
-- This table should already exist in your database from Entity Framework migrations
-- If not, run this script to create it:

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserInterests')
BEGIN
    CREATE TABLE dbo.UserInterests (
        UserInterestId INT IDENTITY(1,1) PRIMARY KEY,
        UserId         INT NOT NULL,
        CategoryId     INT NOT NULL,
        CreatedAt      DATETIME2(7) NOT NULL CONSTRAINT DF_UserInterests_CreatedAt DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT FK_UserInterests_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_UserInterests_Category FOREIGN KEY (CategoryId) REFERENCES dbo.CourseCategories(CategoryId),
        CONSTRAINT UQ_UserInterests_User_Category UNIQUE (UserId, CategoryId)
    );
    
    PRINT 'UserInterests table created successfully';
END
ELSE
BEGIN
    PRINT 'UserInterests table already exists';
END
GO
