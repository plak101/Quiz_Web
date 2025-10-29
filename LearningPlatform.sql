/* =========================================================
   Learning Platform — SQL Server DDL (INT IDENTITY version)
   ========================================================= */

-- 0) CREATE DATABASE
IF DB_ID(N'LearningPlatform') IS NULL
BEGIN
    CREATE DATABASE [LearningPlatform];
END
GO
USE [LearningPlatform];
GO

-- Tắt tất cả khóa ngoại
--EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT ALL";

-- Xóa tất cả bảng
--EXEC sp_MSforeachtable "DROP TABLE ?";


SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================
   1) USERS / ROLES
   ========================= */
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,   
    Name NVARCHAR(50) NOT NULL UNIQUE      
);
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,     -- PK trực tiếp
    Username VARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    AvatarUrl NVARCHAR(500) NULL,
    Phone VARCHAR(20) NULL,
    RoleId INT ,      
    Status INT NOT NULL DEFAULT (1),
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2(7) NULL,
	PasswordResetToken NVARCHAR(255) NULL,
    PasswordResetTokenExpiry DATETIME2 NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);



CREATE TABLE dbo.UserProfiles (
    UserId       INT            NOT NULL,
    DoB          DATE           NULL,
    Gender       NVARCHAR(20)   NULL,
    Bio          NVARCHAR(500)  NULL,
    SchoolName   NVARCHAR(200)  NULL,
    GradeLevel   NVARCHAR(50)   NULL,
    Locale       NVARCHAR(10)   NULL,
    TimeZone     NVARCHAR(64)   NULL,
    CONSTRAINT PK_UserProfiles PRIMARY KEY (UserId)
);
GO

/* =========================
   2) FILES & TAGGING
   ========================= */
CREATE TABLE dbo.Files (
    FileId         INT             NOT NULL IDENTITY(1,1),
    OwnerId        INT             NOT NULL,
    FileName       NVARCHAR(255)   NOT NULL,
    MimeType       NVARCHAR(100)   NOT NULL,
    SizeBytes      BIGINT          NOT NULL,
    StoragePath    NVARCHAR(500)   NOT NULL,
    Width          INT             NULL,
    Height         INT             NULL,
    DurationSec    INT             NULL,
    CreatedAt      DATETIME2(7)    NOT NULL CONSTRAINT DF_Files_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Files PRIMARY KEY (FileId)
);
GO

CREATE TABLE dbo.Tags (
    TagId   INT            NOT NULL IDENTITY(1,1),
    Name    NVARCHAR(80)   NOT NULL,
    Slug    NVARCHAR(100)  NOT NULL,
    CONSTRAINT PK_Tags PRIMARY KEY (TagId),
    CONSTRAINT UQ_Tags_Name UNIQUE (Name),
    CONSTRAINT UQ_Tags_Slug UNIQUE (Slug)
);
GO

CREATE TABLE dbo.ContentTags (
    ContentTagId INT           NOT NULL IDENTITY(1,1),
    ContentType  VARCHAR(20)   NOT NULL, -- FlashcardSet/Test/Course
    ContentId    INT           NOT NULL,
    TagId        INT           NOT NULL,
    CONSTRAINT PK_ContentTags PRIMARY KEY (ContentTagId),
    CONSTRAINT CK_ContentTags_Type CHECK (ContentType IN ('FlashcardSet','Test','Course'))
);
GO

/* =========================
   3) FLASHCARDS
   ========================= */
CREATE TABLE dbo.FlashcardSets (
    SetId        INT             NOT NULL IDENTITY(1,1),
    OwnerId      INT             NOT NULL,
    Title        NVARCHAR(200)   NOT NULL,
    Description  NVARCHAR(MAX)   NULL,
    Visibility   VARCHAR(20)     NOT NULL, -- Private/Class/Public/Course
    CoverUrl     NVARCHAR(500)   NULL,
    TagsText     NVARCHAR(500)   NULL,
    Language     NVARCHAR(20)    NULL,
    CreatedAt    DATETIME2(7)    NOT NULL CONSTRAINT DF_FlashcardSets_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2(7)    NULL,
    IsDeleted    BIT             NOT NULL CONSTRAINT DF_FlashcardSets_IsDeleted DEFAULT (0),
    CONSTRAINT PK_FlashcardSets PRIMARY KEY (SetId),
    CONSTRAINT CK_FlashcardSets_Visibility CHECK (Visibility IN ('Private','Class','Public','Course'))
);
GO

CREATE TABLE dbo.Flashcards (
    CardId        INT             NOT NULL IDENTITY(1,1),
    SetId         INT             NOT NULL,
    FrontText     NVARCHAR(MAX)   NOT NULL,
    BackText      NVARCHAR(MAX)   NOT NULL,
    FrontMediaId  INT             NULL,
    BackMediaId   INT             NULL,
    Hint          NVARCHAR(500)   NULL,
    OrderIndex    INT             NOT NULL CONSTRAINT DF_Flashcards_OrderIndex DEFAULT (0),
    CreatedAt     DATETIME2(7)    NOT NULL CONSTRAINT DF_Flashcards_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2(7)    NULL,
    CONSTRAINT PK_Flashcards PRIMARY KEY (CardId)
);
GO

CREATE TABLE dbo.FlashcardPracticeLogs (
    LogId            INT            NOT NULL IDENTITY(1,1),
    UserId           INT            NOT NULL,
    CardId           INT            NOT NULL,
    SetId            INT            NOT NULL,
    ScheduledAt      DATETIME2(7)   NULL,
    ReviewedAt       DATETIME2(7)   NULL,
    QualityScore     INT            NULL,          -- 0..5
    NextIntervalDays INT            NULL,
    EaseFactor       DECIMAL(4,2)   NULL,
    CONSTRAINT PK_FlashcardPracticeLogs PRIMARY KEY (LogId)
);
GO

/* =========================
   4) TESTS & QUESTIONS
   ========================= */
CREATE TABLE dbo.Tests (
    TestId           INT            NOT NULL IDENTITY(1,1),
    OwnerId          INT            NOT NULL,
    Title            NVARCHAR(200)  NOT NULL,
    Description      NVARCHAR(MAX)  NULL,
    Visibility       VARCHAR(20)    NOT NULL, -- Private/Class/Public/Course
    TimeLimitSec     INT            NULL,
    MaxAttempts      INT            NULL,
    ShuffleQuestions BIT            NOT NULL CONSTRAINT DF_Tests_ShuffleQ DEFAULT (0),
    ShuffleOptions   BIT            NOT NULL CONSTRAINT DF_Tests_ShuffleO DEFAULT (0),
    GradingMode      VARCHAR(10)    NOT NULL, -- Auto/Manual/Mixed
    CreatedAt        DATETIME2(7)   NOT NULL CONSTRAINT DF_Tests_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2(7)   NULL,
    IsDeleted        BIT            NOT NULL CONSTRAINT DF_Tests_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Tests PRIMARY KEY (TestId),
    CONSTRAINT CK_Tests_Visibility CHECK (Visibility IN ('Private','Class','Public','Course')),
    CONSTRAINT CK_Tests_GradingMode CHECK (GradingMode IN ('Auto','Manual','Mixed'))
);
GO

CREATE TABLE dbo.Questions (
    QuestionId   INT            NOT NULL IDENTITY(1,1),
    TestId       INT            NOT NULL,
    Type         VARCHAR(20)    NOT NULL, -- MCQ_Single/MCQ_Multi/TrueFalse/Cloze/Range/ShortText
    StemText     NVARCHAR(MAX)  NOT NULL,
    StemMediaId  INT            NULL,
    Points       DECIMAL(5,2)   NOT NULL CONSTRAINT DF_Questions_Points DEFAULT (1),
    OrderIndex   INT            NOT NULL CONSTRAINT DF_Questions_Order DEFAULT (0),
    Metadata     NVARCHAR(MAX)  NULL,   -- JSON
    CONSTRAINT PK_Questions PRIMARY KEY (QuestionId),
    CONSTRAINT CK_Questions_Type CHECK (Type IN ('MCQ_Single','MCQ_Multi','TrueFalse','Cloze','Range','ShortText'))
);
GO

CREATE TABLE dbo.QuestionOptions (
    OptionId      INT            NOT NULL IDENTITY(1,1),
    QuestionId    INT            NOT NULL,
    OptionText    NVARCHAR(MAX)  NOT NULL,
    OptionMediaId INT            NULL,
    IsCorrect     BIT            NOT NULL CONSTRAINT DF_QuestionOptions_IsCorrect DEFAULT (0),
    OrderIndex    INT            NOT NULL CONSTRAINT DF_QuestionOptions_Order DEFAULT (0),
    CONSTRAINT PK_QuestionOptions PRIMARY KEY (OptionId)
);
GO

CREATE TABLE dbo.QuestionClozeBlanks (
    BlankId       INT            NOT NULL IDENTITY(1,1),
    QuestionId    INT            NOT NULL,
    BlankIndex    INT            NOT NULL,
    CorrectText   NVARCHAR(400)  NOT NULL,
    AcceptRegex   NVARCHAR(400)  NULL,
    CaseSensitive BIT            NOT NULL CONSTRAINT DF_QuestionClozeBlanks_Case DEFAULT (0),
    CONSTRAINT PK_QuestionClozeBlanks PRIMARY KEY (BlankId)
);
GO

CREATE TABLE dbo.QuestionRangeAnswers (
    RangeId     INT            NOT NULL IDENTITY(1,1),
    QuestionId  INT            NOT NULL,
    MinValue    DECIMAL(12,4)  NOT NULL,
    MaxValue    DECIMAL(12,4)  NOT NULL,
    Tolerance   DECIMAL(12,4)  NULL,
    CONSTRAINT PK_QuestionRangeAnswers PRIMARY KEY (RangeId)
);
GO

CREATE TABLE dbo.TestAttempts (
    AttemptId     INT            NOT NULL IDENTITY(1,1),
    TestId        INT            NOT NULL,
    UserId        INT            NOT NULL,
    StartedAt     DATETIME2(7)   NOT NULL CONSTRAINT DF_TestAttempts_StartedAt DEFAULT SYSUTCDATETIME(),
    SubmittedAt   DATETIME2(7)   NULL,
    Status        VARCHAR(12)    NOT NULL,   -- InProgress/Submitted/Graded/Expired
    TimeSpentSec  INT            NULL,
    Score         DECIMAL(6,2)   NULL,
    MaxScore      DECIMAL(6,2)   NULL,
    CONSTRAINT PK_TestAttempts PRIMARY KEY (AttemptId),
    CONSTRAINT CK_TestAttempts_Status CHECK (Status IN ('InProgress','Submitted','Graded','Expired'))
);
GO

CREATE TABLE dbo.AttemptAnswers (
    AttemptAnswerId INT            NOT NULL IDENTITY(1,1),
    AttemptId       INT            NOT NULL,
    QuestionId      INT            NOT NULL,
    AnswerPayload   NVARCHAR(MAX)  NULL, -- JSON
    IsCorrect       BIT            NULL,
    Score           DECIMAL(5,2)   NULL,
    AutoGraded      BIT            NOT NULL CONSTRAINT DF_AttemptAnswers_Auto DEFAULT (0),
    GradedAt        DATETIME2(7)   NULL,
    GraderId        INT            NULL,
    CONSTRAINT PK_AttemptAnswers PRIMARY KEY (AttemptAnswerId)
);
GO

/* Optional: gán Test qua ClassAssignments */
CREATE TABLE dbo.TestAssignments (
    TestAssignId         INT            NOT NULL IDENTITY(1,1),
    AssignmentId         INT            NOT NULL,
    TestId               INT            NOT NULL,
    StartAt              DATETIME2(7)   NULL,
    DueAt                DATETIME2(7)   NULL,
    AttemptsAllowed      INT            NULL,
    OverrideTimeLimitSec INT            NULL,
    CONSTRAINT PK_TestAssignments PRIMARY KEY (TestAssignId)
);
GO

/* =========================
   5) CLASSES
   ========================= */
CREATE TABLE dbo.Classes (
    ClassId     INT             NOT NULL IDENTITY(1,1),
    TeacherId   INT             NOT NULL,
    Name        NVARCHAR(200)   NOT NULL,
    Code        NVARCHAR(50)    NOT NULL,
    Description NVARCHAR(MAX)   NULL,
    Term        NVARCHAR(50)    NULL,
    IsArchived  BIT             NOT NULL CONSTRAINT DF_Classes_IsArchived DEFAULT (0),
    CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Classes_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Classes PRIMARY KEY (ClassId),
    CONSTRAINT UQ_Classes_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.ClassStudents (
    ClassId   INT            NOT NULL,
    StudentId INT            NOT NULL,
    JoinedAt  DATETIME2(7)   NOT NULL CONSTRAINT DF_ClassStudents_JoinedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ClassStudents PRIMARY KEY (ClassId, StudentId)
);
GO

CREATE TABLE dbo.ClassAnnouncements (
    AnnouncementId INT            NOT NULL IDENTITY(1,1),
    ClassId        INT            NOT NULL,
    AuthorId       INT            NOT NULL,
    Title          NVARCHAR(200)  NOT NULL,
    Content        NVARCHAR(MAX)  NOT NULL,
    CreatedAt      DATETIME2(7)   NOT NULL CONSTRAINT DF_ClassAnnouncements_CreatedAt DEFAULT SYSUTCDATETIME(),
    PinUntil       DATETIME2(7)   NULL,
    CONSTRAINT PK_ClassAnnouncements PRIMARY KEY (AnnouncementId)
);
GO

CREATE TABLE dbo.ClassAssignments (
    AssignmentId    INT            NOT NULL IDENTITY(1,1),
    ClassId         INT            NOT NULL,
    Title           NVARCHAR(200)  NOT NULL,
    Type            VARCHAR(20)    NOT NULL,  -- FlashcardSet/Test
    ContentId       INT            NOT NULL,  -- Polymorphic link
    StartAt         DATETIME2(7)   NULL,
    DueAt           DATETIME2(7)   NULL,
    AttemptsAllowed INT            NULL,
    GradingPolicy   NVARCHAR(50)   NULL,
    CONSTRAINT PK_ClassAssignments PRIMARY KEY (AssignmentId),
    CONSTRAINT CK_ClassAssignments_Type CHECK (Type IN ('FlashcardSet','Test'))
);
GO

/* =========================
   6) COURSES & PAYMENTS
   ========================= */
CREATE TABLE dbo.Courses (
    CourseId    INT             NOT NULL IDENTITY(1,1),
    OwnerId     INT             NOT NULL,
    Title       NVARCHAR(200)   NOT NULL,
    Slug        NVARCHAR(200)   NOT NULL,
    Summary     NVARCHAR(MAX)   NULL,
    CoverUrl    NVARCHAR(500)   NULL,
    Price       DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Courses_Price DEFAULT (0),
    Currency    NVARCHAR(10)    NOT NULL CONSTRAINT DF_Courses_Currency DEFAULT (N'VND'),
    IsPublished BIT             NOT NULL CONSTRAINT DF_Courses_IsPublished DEFAULT (0),
    CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Courses_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2(7)    NULL,
    CONSTRAINT PK_Courses PRIMARY KEY (CourseId),
    CONSTRAINT UQ_Courses_Slug UNIQUE (Slug)
);
GO

CREATE TABLE dbo.CourseSections (
    SectionId   INT            NOT NULL IDENTITY(1,1),
    CourseId    INT            NOT NULL,
    Title       NVARCHAR(200)  NOT NULL,
    OrderIndex  INT            NOT NULL CONSTRAINT DF_CourseSections_Order DEFAULT (0),
    CONSTRAINT PK_CourseSections PRIMARY KEY (SectionId)
);
GO

CREATE TABLE dbo.CourseContents (
    CourseContentId INT            NOT NULL IDENTITY(1,1),
    CourseId        INT            NOT NULL,
    SectionId       INT            NULL,
    ContentType     VARCHAR(20)    NOT NULL, -- FlashcardSet/Test
    ContentId       INT            NOT NULL,
    TitleOverride   NVARCHAR(200)  NULL,
    OrderIndex      INT            NOT NULL CONSTRAINT DF_CourseContents_Order DEFAULT (0),
    IsPreview       BIT            NOT NULL CONSTRAINT DF_CourseContents_Preview DEFAULT (0),
    CONSTRAINT PK_CourseContents PRIMARY KEY (CourseContentId),
    CONSTRAINT CK_CourseContents_Type CHECK (ContentType IN ('FlashcardSet','Test'))
);
GO

CREATE TABLE dbo.CoursePurchases (
    PurchaseId  INT             NOT NULL IDENTITY(1,1),
    CourseId    INT             NOT NULL,
    BuyerId     INT             NOT NULL,
    PricePaid   DECIMAL(12,2)   NOT NULL,
    Currency    NVARCHAR(10)    NOT NULL,
    Status      VARCHAR(20)     NOT NULL, -- Pending/Paid/Refunded/Failed
    PurchasedAt DATETIME2(7)    NOT NULL CONSTRAINT DF_CoursePurchases_At DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CoursePurchases PRIMARY KEY (PurchaseId),
    CONSTRAINT CK_CoursePurchases_Status CHECK (Status IN ('Pending','Paid','Refunded','Failed'))
);
GO

CREATE TABLE dbo.Payments (
    PaymentId   INT             NOT NULL IDENTITY(1,1),
    PurchaseId  INT             NOT NULL,
    Provider    VARCHAR(40)     NOT NULL, -- Stripe/VNPay/...
    ProviderRef NVARCHAR(120)   NULL,
    Amount      DECIMAL(12,2)   NOT NULL,
    Currency    NVARCHAR(10)    NOT NULL,
    Status      VARCHAR(20)     NOT NULL,
    PaidAt      DATETIME2(7)    NULL,
    RawPayload  NVARCHAR(MAX)   NULL,
    CONSTRAINT PK_Payments PRIMARY KEY (PaymentId)
);
GO

/* =========================
   7) LIBRARIES
   ========================= */
CREATE TABLE dbo.Libraries (
    LibraryId   INT             NOT NULL IDENTITY(1,1),
    OwnerId     INT             NOT NULL,
    Name        NVARCHAR(200)   NOT NULL,
    Description NVARCHAR(MAX)   NULL,
    CreatedAt   DATETIME2(7)    NOT NULL CONSTRAINT DF_Libraries_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Libraries PRIMARY KEY (LibraryId)
);
GO

CREATE TABLE dbo.Folders (
    FolderId        INT            NOT NULL IDENTITY(1,1),
    LibraryId       INT            NOT NULL,
    ParentFolderId  INT            NULL,
    Name            NVARCHAR(200)  NOT NULL,
    OrderIndex      INT            NOT NULL CONSTRAINT DF_Folders_Order DEFAULT (0),
    CreatedAt       DATETIME2(7)   NOT NULL CONSTRAINT DF_Folders_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Folders PRIMARY KEY (FolderId)
);
GO

CREATE TABLE dbo.SavedItems (
    SavedItemId INT            NOT NULL IDENTITY(1,1),
    LibraryId   INT            NOT NULL,
    FolderId    INT            NULL,
    ContentType VARCHAR(20)    NOT NULL, -- FlashcardSet/Test/Course
    ContentId   INT            NOT NULL,
    AddedAt     DATETIME2(7)   NOT NULL CONSTRAINT DF_SavedItems_AddedAt DEFAULT SYSUTCDATETIME(),
    Note        NVARCHAR(500)  NULL,
    CONSTRAINT PK_SavedItems PRIMARY KEY (SavedItemId),
    CONSTRAINT CK_SavedItems_Type CHECK (ContentType IN ('FlashcardSet','Test','Course'))
);
GO

/* =========================
   8) SHARING (polymorphic)
   ========================= */
CREATE TABLE dbo.ContentShares (
    ShareId     INT            NOT NULL IDENTITY(1,1),
    ContentType VARCHAR(20)    NOT NULL, -- FlashcardSet/Test
    ContentId   INT            NOT NULL,
    TargetType  VARCHAR(10)    NOT NULL, -- User/Class/Public/Course
    TargetId    INT            NULL,     -- NULL if Public
    CanView     BIT            NOT NULL CONSTRAINT DF_ContentShares_View DEFAULT (1),
    CanEdit     BIT            NOT NULL CONSTRAINT DF_ContentShares_Edit DEFAULT (0),
    CanAssign   BIT            NOT NULL CONSTRAINT DF_ContentShares_Assign DEFAULT (0),
    ExpiresAt   DATETIME2(7)   NULL,
    CreatedBy   INT            NOT NULL,
    CreatedAt   DATETIME2(7)   NOT NULL CONSTRAINT DF_ContentShares_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ContentShares PRIMARY KEY (ShareId),
    CONSTRAINT CK_ContentShares_ContentType CHECK (ContentType IN ('FlashcardSet','Test')),
    CONSTRAINT CK_ContentShares_TargetType  CHECK (TargetType  IN ('User','Class','Public','Course'))
);
GO

/* =========================
   9) SUBMISSIONS (optional aggregator)
   ========================= */
CREATE TABLE dbo.Submissions (
    SubmissionId INT            NOT NULL IDENTITY(1,1),
    AssignmentId INT            NOT NULL,
    UserId       INT            NOT NULL,
    SubmittedAt  DATETIME2(7)   NOT NULL CONSTRAINT DF_Submissions_SubAt DEFAULT SYSUTCDATETIME(),
    Status       VARCHAR(12)    NOT NULL, -- Submitted/Graded/Returned
    TotalScore   DECIMAL(6,2)   NULL,
    MaxScore     DECIMAL(6,2)   NULL,
    Feedback     NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_Submissions PRIMARY KEY (SubmissionId),
    CONSTRAINT CK_Submissions_Status CHECK (Status IN ('Submitted','Graded','Returned'))
);
GO

CREATE TABLE dbo.SubmissionItems (
    SubmissionItemId INT            NOT NULL IDENTITY(1,1),
    SubmissionId     INT            NOT NULL,
    RefType          VARCHAR(20)    NOT NULL, -- FlashcardSet/Test
    RefId            INT            NOT NULL,
    Score            DECIMAL(6,2)   NULL,
    MaxScore         DECIMAL(6,2)   NULL,
    Feedback         NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_SubmissionItems PRIMARY KEY (SubmissionItemId),
    CONSTRAINT CK_SubmissionItems_RefType CHECK (RefType IN ('FlashcardSet','Test'))
);
GO

/* =========================
   10) NOTIFICATIONS & REMINDERS
   ========================= */
CREATE TABLE dbo.Notifications (
    NotificationId INT             NOT NULL IDENTITY(1,1),
    UserId         INT             NOT NULL,
    Type           VARCHAR(40)     NOT NULL,
    Title          NVARCHAR(200)   NOT NULL,
    Body           NVARCHAR(MAX)   NULL,
    Data           NVARCHAR(MAX)   NULL,
    IsRead         BIT             NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
    CreatedAt      DATETIME2(7)    NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSUTCDATETIME(),
    ReadAt         DATETIME2(7)    NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId)
);
GO

CREATE TABLE dbo.NotificationChannels (
    ChannelId      INT            NOT NULL IDENTITY(1,1),
    UserId         INT            NOT NULL,
    Channel        VARCHAR(10)    NOT NULL, -- InApp/Email/Push
    Enabled        BIT            NOT NULL CONSTRAINT DF_NotificationChannels_Enabled DEFAULT (1),
    AddressOrToken NVARCHAR(500)  NULL,
    CreatedAt      DATETIME2(7)   NOT NULL CONSTRAINT DF_NotificationChannels_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_NotificationChannels PRIMARY KEY (ChannelId),
    CONSTRAINT CK_NotificationChannels_Channel CHECK (Channel IN ('InApp','Email','Push'))
);
GO

CREATE TABLE dbo.Reminders (
    ReminderId  INT            NOT NULL IDENTITY(1,1),
    UserId      INT            NOT NULL,
    RelatedType VARCHAR(20)    NOT NULL, -- Assignment/TestAttempt/Course
    RelatedId   INT            NOT NULL,
    TriggerAt   DATETIME2(7)   NOT NULL,
    SentAt      DATETIME2(7)   NULL,
    Status      VARCHAR(10)    NOT NULL, -- Pending/Sent/Cancelled
    CONSTRAINT PK_Reminders PRIMARY KEY (ReminderId),
    CONSTRAINT CK_Reminders_RelatedType CHECK (RelatedType IN ('Assignment','TestAttempt','Course')),
    CONSTRAINT CK_Reminders_Status CHECK (Status IN ('Pending','Sent','Cancelled'))
);
GO

/* =========================
   11) AUDIT & ERRORS
   ========================= */
CREATE TABLE dbo.AuditLogs (
    AuditId    INT             NOT NULL IDENTITY(1,1),
    UserId     INT             NULL,
    Action     NVARCHAR(100)   NOT NULL,
    EntityType NVARCHAR(50)    NOT NULL,
    EntityId   INT             NULL,
    Before     NVARCHAR(MAX)   NULL, -- JSON
    After      NVARCHAR(MAX)   NULL, -- JSON
    CreatedAt  DATETIME2(7)    NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
    IpAddress  VARCHAR(45)     NULL,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditId)
);
GO

CREATE TABLE dbo.ErrorLogs (
    ErrorId   INT             NOT NULL IDENTITY(1,1),
    Severity  VARCHAR(10)     NOT NULL, -- Info/Warn/Error
    Message   NVARCHAR(4000)  NOT NULL,
    Stack     NVARCHAR(MAX)   NULL,
    Context   NVARCHAR(MAX)   NULL,
    CreatedAt DATETIME2(7)    NOT NULL CONSTRAINT DF_ErrorLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ErrorLogs PRIMARY KEY (ErrorId),
    CONSTRAINT CK_ErrorLogs_Severity CHECK (Severity IN ('Info','Warn','Error'))
);
GO

/* =========================
   12) INVITES / SETTINGS / CERTIFICATES
   ========================= */
CREATE TABLE dbo.Invitations (
    InviteId     INT            NOT NULL IDENTITY(1,1),
    InviterId    INT            NOT NULL,
    ClassId      INT            NULL,
    Email        NVARCHAR(255)  NOT NULL,
    RoleSuggested VARCHAR(10)   NULL, -- Admin/Teacher/Student
    Token        NVARCHAR(100)  NOT NULL,
    ExpiresAt    DATETIME2(7)   NOT NULL,
    AcceptedAt   DATETIME2(7)   NULL,
    CONSTRAINT PK_Invitations PRIMARY KEY (InviteId),
    CONSTRAINT UQ_Invitations_Token UNIQUE (Token)
);
GO

CREATE TABLE dbo.UserSettings (
    UserId        INT            NOT NULL,
    UiTheme       VARCHAR(20)    NULL, -- light/dark/system
    Language      NVARCHAR(10)   NULL,
    TimeZone      NVARCHAR(64)   NULL,
    EmailOptIn    BIT            NOT NULL CONSTRAINT DF_UserSettings_EmailOptIn DEFAULT (1),
    PushOptIn     BIT            NOT NULL CONSTRAINT DF_UserSettings_PushOptIn DEFAULT (1),
    CONSTRAINT PK_UserSettings PRIMARY KEY (UserId)
);
GO

CREATE TABLE dbo.Certificates (
    CertId     INT            NOT NULL IDENTITY(1,1),
    CourseId   INT            NOT NULL,
    UserId     INT            NOT NULL,
    IssuedAt   DATETIME2(7)   NOT NULL CONSTRAINT DF_Certificates_IssuedAt DEFAULT SYSUTCDATETIME(),
    Serial     NVARCHAR(50)   NULL,
    VerifyCode NVARCHAR(50)   NOT NULL,
    CONSTRAINT PK_Certificates PRIMARY KEY (CertId),
    CONSTRAINT UQ_Certificates_Verify UNIQUE (VerifyCode)
);
GO

/* =========================
   13) LESSONS (New)
   ========================= */

/* Bảng chính chứa thông tin chung của bài học */
CREATE TABLE dbo.Lessons (
    LessonId INT IDENTITY(1,1) PRIMARY KEY,
    OwnerId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Visibility VARCHAR(20) NOT NULL, -- Private/Class/Public/Course
    CoverUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Lessons_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Lessons_IsDeleted DEFAULT (0),

    CONSTRAINT FK_Lessons_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Lessons_Visibility CHECK (Visibility IN ('Private','Class','Public','Course'))
);
GO

/* Bảng này chứa TỪNG slide trong một bài học (Lesson) */
/* Đây là bảng "xương sống" định nghĩa cấu trúc của bài học */
CREATE TABLE dbo.LessonSlides (
    SlideId INT IDENTITY(1,1) PRIMARY KEY,
    LessonId INT NOT NULL,
    OrderIndex INT NOT NULL CONSTRAINT DF_LessonSlides_Order DEFAULT (0),
   
    /* Cột quan trọng: 'Flashcard', 'MCQ_Single', 'MCQ_Multi', 'TrueFalse', 'ShortText' */
    SlideType VARCHAR(20) NOT NULL, 

    /* Dùng chung cho (Câu hỏi / Mặt trước Flashcard) */
    StemText NVARCHAR(MAX) NOT NULL, 
    StemMediaId INT NULL, 

    /* Điểm cho slide này (mặc định là 0 cho flashcard, 1 cho câu hỏi) */
    Points DECIMAL(5,2) NOT NULL CONSTRAINT DF_LessonSlides_Points DEFAULT (0), 

    CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_LessonSlides_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(7) NULL,

    CONSTRAINT FK_LessonSlides_Lesson FOREIGN KEY (LessonId) REFERENCES dbo.Lessons(LessonId),
    CONSTRAINT FK_LessonSlides_StemMedia FOREIGN KEY (StemMediaId) REFERENCES dbo.Files(FileId),
    CONSTRAINT CK_LessonSlides_Type CHECK (SlideType IN ('Flashcard','MCQ_Single','MCQ_Multi','TrueFalse','ShortText'))
);
GO

/* Chứa dữ liệu riêng cho loại slide 'Flashcard' */
/* Quan hệ 1-1 với LessonSlides */
CREATE TABLE dbo.LessonSlide_Flashcard (
    SlideId INT NOT NULL, /* Đây là PK và FK */
    BackText NVARCHAR(MAX) NOT NULL,
    BackMediaId INT NULL,

    CONSTRAINT PK_LessonSlide_Flashcard PRIMARY KEY (SlideId),
    CONSTRAINT FK_LSFlashcard_Slide FOREIGN KEY (SlideId) REFERENCES dbo.LessonSlides(SlideId),
    CONSTRAINT FK_LSFlashcard_BackMedia FOREIGN KEY (BackMediaId) REFERENCES dbo.Files(FileId)
);
GO

/* Chứa các lựa chọn (A, B, C) cho 'MCQ_Single', 'MCQ_Multi', 'TrueFalse' */
/* Quan hệ 1-Nhiều với LessonSlides */
CREATE TABLE dbo.LessonSlide_Options (
    OptionId INT IDENTITY(1,1) PRIMARY KEY,
    SlideId INT NOT NULL,
    OptionText NVARCHAR(MAX) NOT NULL,
    OptionMediaId INT NULL,
    IsCorrect BIT NOT NULL CONSTRAINT DF_LSOptions_IsCorrect DEFAULT (0),
    OrderIndex INT NOT NULL CONSTRAINT DF_LSOptions_Order DEFAULT (0),

    CONSTRAINT FK_LSOptions_Slide FOREIGN KEY (SlideId) REFERENCES dbo.LessonSlides(SlideId),
    CONSTRAINT FK_LSOptions_Media FOREIGN KEY (OptionMediaId) REFERENCES dbo.Files(FileId)
);
GO

/* Chứa câu trả lời đúng cho 'ShortText' (Điền vào chỗ trống) */
/* Quan hệ 1-Nhiều với LessonSlides (cho phép nhiều đáp án đúng) */
CREATE TABLE dbo.LessonSlide_ShortText (
    AnswerId INT IDENTITY(1,1) PRIMARY KEY,
    SlideId INT NOT NULL,
    CorrectText NVARCHAR(400) NOT NULL, /* Đáp án chấp nhận */
    CaseSensitive BIT NOT NULL CONSTRAINT DF_LSShortText_Case DEFAULT (0),

    CONSTRAINT FK_LSShortText_Slide FOREIGN KEY (SlideId) REFERENCES dbo.LessonSlides(SlideId)
);
GO



/* =========================================================
   FOREIGN KEYS
   ========================================================= */

   /* 1. Xóa ràng buộc cũ */

/* 2. Thêm ràng buộc mới, bao gồm 'Lesson' */
ALTER TABLE dbo.CourseContents
ADD CONSTRAINT CK_CourseContents_Type
CHECK (ContentType IN ('FlashcardSet', 'Test', 'Lesson'));
GO

ALTER TABLE dbo.UserProfiles
ADD CONSTRAINT FK_UserProfiles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Files
ADD CONSTRAINT FK_Files_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.ContentTags
ADD CONSTRAINT FK_ContentTags_Tag FOREIGN KEY (TagId) REFERENCES dbo.Tags(TagId);

ALTER TABLE dbo.FlashcardSets
ADD CONSTRAINT FK_FlashcardSets_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Flashcards
ADD CONSTRAINT FK_Flashcards_Set  FOREIGN KEY (SetId) REFERENCES dbo.FlashcardSets(SetId),
    CONSTRAINT FK_Flashcards_FrontMedia FOREIGN KEY (FrontMediaId) REFERENCES dbo.Files(FileId),
    CONSTRAINT FK_Flashcards_BackMedia  FOREIGN KEY (BackMediaId)  REFERENCES dbo.Files(FileId);

ALTER TABLE dbo.FlashcardPracticeLogs
ADD CONSTRAINT FK_FPL_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_FPL_Card FOREIGN KEY (CardId) REFERENCES dbo.Flashcards(CardId),
    CONSTRAINT FK_FPL_Set  FOREIGN KEY (SetId)  REFERENCES dbo.FlashcardSets(SetId);

ALTER TABLE dbo.Tests
ADD CONSTRAINT FK_Tests_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Questions
ADD CONSTRAINT FK_Questions_Test FOREIGN KEY (TestId) REFERENCES dbo.Tests(TestId),
    CONSTRAINT FK_Questions_StemMedia FOREIGN KEY (StemMediaId) REFERENCES dbo.Files(FileId);

ALTER TABLE dbo.QuestionOptions
ADD CONSTRAINT FK_QOptions_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Questions(QuestionId),
    CONSTRAINT FK_QOptions_Media    FOREIGN KEY (OptionMediaId) REFERENCES dbo.Files(FileId);

ALTER TABLE dbo.QuestionClozeBlanks
ADD CONSTRAINT FK_QCloze_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Questions(QuestionId);

ALTER TABLE dbo.QuestionRangeAnswers
ADD CONSTRAINT FK_QRange_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Questions(QuestionId);

ALTER TABLE dbo.TestAttempts
ADD CONSTRAINT FK_Attempts_Test FOREIGN KEY (TestId) REFERENCES dbo.Tests(TestId),
    CONSTRAINT FK_Attempts_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.AttemptAnswers
ADD CONSTRAINT FK_AAnswers_Attempt  FOREIGN KEY (AttemptId)  REFERENCES dbo.TestAttempts(AttemptId),
    CONSTRAINT FK_AAnswers_Question FOREIGN KEY (QuestionId)  REFERENCES dbo.Questions(QuestionId),
    CONSTRAINT FK_AAnswers_Grader   FOREIGN KEY (GraderId)    REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.TestAssignments
ADD CONSTRAINT FK_TAssign_Assignment FOREIGN KEY (AssignmentId) REFERENCES dbo.ClassAssignments(AssignmentId),
    CONSTRAINT FK_TAssign_Test       FOREIGN KEY (TestId)       REFERENCES dbo.Tests(TestId);

ALTER TABLE dbo.Classes
ADD CONSTRAINT FK_Classes_Teacher FOREIGN KEY (TeacherId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.ClassStudents
ADD CONSTRAINT FK_ClassStudents_Class   FOREIGN KEY (ClassId)   REFERENCES dbo.Classes(ClassId),
    CONSTRAINT FK_ClassStudents_Student FOREIGN KEY (StudentId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.ClassAnnouncements
ADD CONSTRAINT FK_CAnnouncements_Class  FOREIGN KEY (ClassId)  REFERENCES dbo.Classes(ClassId),
    CONSTRAINT FK_CAnnouncements_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.ClassAssignments
ADD CONSTRAINT FK_CAssignments_Class FOREIGN KEY (ClassId) REFERENCES dbo.Classes(ClassId);

ALTER TABLE dbo.Courses
ADD CONSTRAINT FK_Courses_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.CourseSections
ADD CONSTRAINT FK_CSections_Course FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId);

ALTER TABLE dbo.CourseContents
ADD CONSTRAINT FK_CContents_Course  FOREIGN KEY (CourseId)  REFERENCES dbo.Courses(CourseId),
    CONSTRAINT FK_CContents_Section FOREIGN KEY (SectionId) REFERENCES dbo.CourseSections(SectionId);

ALTER TABLE dbo.CoursePurchases
ADD CONSTRAINT FK_CPurchases_Course FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    CONSTRAINT FK_CPurchases_Buyer  FOREIGN KEY (BuyerId)  REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Payments
ADD CONSTRAINT FK_Payments_Purchase FOREIGN KEY (PurchaseId) REFERENCES dbo.CoursePurchases(PurchaseId);

ALTER TABLE dbo.Libraries
ADD CONSTRAINT FK_Libraries_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Folders
ADD CONSTRAINT FK_Folders_Library FOREIGN KEY (LibraryId)      REFERENCES dbo.Libraries(LibraryId),
    CONSTRAINT FK_Folders_Parent  FOREIGN KEY (ParentFolderId) REFERENCES dbo.Folders(FolderId);

ALTER TABLE dbo.SavedItems
ADD CONSTRAINT FK_SavedItems_Library FOREIGN KEY (LibraryId) REFERENCES dbo.Libraries(LibraryId),
    CONSTRAINT FK_SavedItems_Folder  FOREIGN KEY (FolderId)  REFERENCES dbo.Folders(FolderId);

ALTER TABLE dbo.Notifications
ADD CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.NotificationChannels
ADD CONSTRAINT FK_NChannels_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Reminders
ADD CONSTRAINT FK_Reminders_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.AuditLogs
ADD CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Invitations
ADD CONSTRAINT FK_Invitations_Inviter FOREIGN KEY (InviterId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Invitations_Class   FOREIGN KEY (ClassId)   REFERENCES dbo.Classes(ClassId);

ALTER TABLE dbo.UserSettings
ADD CONSTRAINT FK_UserSettings_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

ALTER TABLE dbo.Certificates
ADD CONSTRAINT FK_Certificates_Course FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    CONSTRAINT FK_Certificates_User   FOREIGN KEY (UserId)   REFERENCES dbo.Users(UserId);

	USE [LearningPlatform];
GO

/* =========================
   1) USERS / ROLES
   ========================= */
INSERT INTO dbo.Roles (Name)
VALUES (N'Admin'), (N'Giáo viên'), (N'Học sinh');
GO

INSERT INTO dbo.Users (Email,Username, PasswordHash, FullName, AvatarUrl, Phone, RoleId)
VALUES 
(N'admin@learn.vn','admin', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Quản trị viên', NULL, '0909000001', 1),
(N'teacher1@learn.vn','teacher', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Trần Minh Khoa', NULL, '0909000002', 2),
(N'student1@learn.vn','student1', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Nguyễn Lan Anh', NULL, '0909000003', 2),
(N'student2@learn.vn','student2', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Lê Hoàng Nam', NULL, '0909000004', 3);
GO


INSERT INTO dbo.UserProfiles (UserId, DoB, Gender, Bio, SchoolName, GradeLevel, Locale)
VALUES
(1, '1990-01-01', N'Nam', N'Quản lý hệ thống', N'Đại học CNTT', N'Cử nhân', N'vi-VN'),
(2, '1995-07-15', N'Nam', N'Giảng viên CNTT', N'Đại học GTVT TP.HCM', N'Giảng viên', N'vi-VN'),
(3, '2004-04-10', N'Nữ', N'Học sinh lớp 12 chuyên Tin', N'THPT Nguyễn Thị Minh Khai', N'Lớp 12', N'vi-VN'),
(4, '2003-11-22', N'Nam', N'Sinh viên năm nhất', N'ĐH Bách Khoa', N'Năm 1', N'vi-VN');
GO

/* =========================
   2) TAGS
   ========================= */
INSERT INTO dbo.Tags (Name, Slug)
VALUES 
(N'Tin học', 'tin-hoc'),
(N'Lập trình C#', 'lap-trinh-csharp'),
(N'Cơ sở dữ liệu', 'co-so-du-lieu'),
(N'Toán học', 'toan-hoc');
GO

/* =========================
   3) FLASHCARDS
   ========================= */
INSERT INTO dbo.FlashcardSets (OwnerId, Title, Description, Visibility, Language)
VALUES
(2, N'Flashcards SQL Cơ bản', N'Học các lệnh SQL cơ bản: SELECT, INSERT, UPDATE...', 'Public', 'vi'),
(2, N'Từ vựng CNTT', N'Tổng hợp các thuật ngữ chuyên ngành CNTT thông dụng', 'Public', 'vi');
GO

INSERT INTO dbo.Flashcards (SetId, FrontText, BackText, Hint, OrderIndex)
VALUES
(1, N'Câu lệnh SELECT dùng để làm gì?', N'Dùng để truy vấn dữ liệu từ bảng', N'Lấy dữ liệu', 1),
(1, N'Câu lệnh INSERT dùng để làm gì?', N'Dùng để thêm dữ liệu mới vào bảng', N'Thêm bản ghi', 2),
(2, N'Hardware nghĩa là gì?', N'Phần cứng', N'Thiết bị vật lý của máy tính', 1),
(2, N'Software nghĩa là gì?', N'Phần mềm', N'Chương trình điều khiển phần cứng', 2);
GO

/* =========================
   4) TESTS & QUESTIONS
   ========================= */
INSERT INTO dbo.Tests (OwnerId, Title, Description, Visibility, GradingMode)
VALUES
(2, N'Bài kiểm tra SQL cơ bản', N'Gồm các câu hỏi trắc nghiệm về SQL cơ bản', 'Public', 'Auto'),
(2, N'Bài kiểm tra CNTT cơ bản', N'Kiến thức tổng hợp về phần cứng và phần mềm', 'Public', 'Auto');
GO

INSERT INTO dbo.Questions (TestId, Type, StemText)
VALUES
(1, 'MCQ_Single', N'Lệnh nào dùng để chọn dữ liệu trong SQL?'),
(1, 'MCQ_Single', N'Lệnh nào dùng để xóa dữ liệu trong bảng?'),
(2, 'TrueFalse', N'RAM là bộ nhớ lưu trữ tạm thời.'),
(2, 'TrueFalse', N'Hệ điều hành là phần cứng của máy tính.');
GO

INSERT INTO dbo.QuestionOptions (QuestionId, OptionText, IsCorrect)
VALUES
(1, N'SELECT', 1),
(1, N'INSERT', 0),
(2, N'DELETE', 1),
(2, N'UPDATE', 0),
(3, N'Đúng', 1),
(3, N'Sai', 0),
(4, N'Đúng', 0),
(4, N'Sai', 1);
GO

/* =========================
   5) CLASSES
   ========================= */
INSERT INTO dbo.Classes (TeacherId, Name, Code, Term)
VALUES
(2, N'Lớp SQL 101', N'SQL101', N'Học kỳ I - 2025'),
(2, N'Lớp Nhập môn CNTT', N'CNTT100', N'Học kỳ I - 2025');
GO

INSERT INTO dbo.ClassStudents (ClassId, StudentId)
VALUES
(1, 3),
(1, 4),
(2, 3);
GO

INSERT INTO dbo.ClassAnnouncements (ClassId, AuthorId, Title, Content)
VALUES
(1, 2, N'Chào mừng lớp SQL 101', N'Buổi học đầu tiên sẽ bắt đầu vào thứ Hai, 8h sáng.'),
(2, 2, N'Giới thiệu môn CNTT', N'Hãy đọc trước tài liệu chương 1.');
GO

/* =========================
   6) COURSES
   ========================= */
INSERT INTO dbo.Courses (OwnerId, Title, Slug, Summary, Price, IsPublished)
VALUES
(2, N'Khóa học SQL Cơ bản', 'sql-co-ban', N'Học từ cơ bản đến nâng cao về SQL.', 199000, 1),
(2, N'Khóa học Lập trình C#', 'lap-trinh-csharp', N'Học lập trình hướng đối tượng với C#.', 299000, 1);
GO

INSERT INTO dbo.CourseSections (CourseId, Title, OrderIndex)
VALUES
(1, N'Giới thiệu về SQL', 1),
(1, N'Câu lệnh SELECT', 2),
(2, N'Giới thiệu C#', 1);
GO

/* =========================
   7) PURCHASES & PAYMENTS
   ========================= */
INSERT INTO dbo.CoursePurchases (CourseId, BuyerId, PricePaid, Currency, Status)
VALUES
(1, 3, 199000, N'VND', 'Paid'),
(2, 4, 299000, N'VND', 'Pending');
GO

INSERT INTO dbo.Payments (PurchaseId, Provider, Amount, Currency, Status)
VALUES
(1, 'VNPay', 199000, N'VND', 'Paid'),
(2, 'VNPay', 299000, N'VND', 'Pending');
GO

/* =========================
   8) LIBRARIES
   ========================= */
INSERT INTO dbo.Libraries (OwnerId, Name, Description)
VALUES
(3, N'Thư viện học tập Lan Anh', N'Lưu trữ bộ flashcard và khóa học đã học'),
(4, N'Thư viện Nam', N'Tổng hợp bài kiểm tra yêu thích');
GO

INSERT INTO dbo.Folders (LibraryId, Name, OrderIndex)
VALUES
(1, N'SQL', 1),
(1, N'Lập trình', 2),
(2, N'Bài kiểm tra', 1);
GO

INSERT INTO dbo.SavedItems (LibraryId, FolderId, ContentType, ContentId, Note)
VALUES
(1, 1, 'FlashcardSet', 1, N'Ôn tập mỗi ngày'),
(1, 2, 'Course', 2, N'Khóa học C# đang học'),
(2, 1, 'Test', 1, N'Làm lại để cải thiện điểm');
GO

/* =========================
   9) NOTIFICATIONS
   ========================= */
INSERT INTO dbo.Notifications (UserId, Type, Title, Body)
VALUES
(3, 'Class', N'Bạn đã được thêm vào lớp SQL 101', N'Giáo viên Trần Minh Khoa đã thêm bạn vào lớp học.'),
(4, 'Payment', N'Thanh toán đang chờ xử lý', N'Khóa học Lập trình C# đang chờ thanh toán.');
GO

/* =========================
   10) INVITATIONS & CERTIFICATES
   ========================= */
INSERT INTO dbo.Invitations (InviterId, ClassId, Email, RoleSuggested, Token, ExpiresAt)
VALUES
(2, 1, N'hocsinhmoi@learn.vn', N'Học sinh', N'TOKEN123', DATEADD(DAY,7,GETUTCDATE()));
GO

INSERT INTO dbo.Certificates (CourseId, UserId, Serial, VerifyCode)
VALUES
(1, 3, N'SQL2025-001', N'VER123ABC');
GO

-- Thêm khóa học mẫu
INSERT INTO Courses (OwnerId, Title, Slug, Summary, CoverUrl, Price, Currency, IsPublished, CreatedAt)
VALUES 
(1, 'Complete Blender Creator: Learn 3D Modelling for Beginners', 'blender-3d-modelling', 
 'Học 3D modeling từ cơ bản đến nâng cao với Blender', 
 'https://img-c.udemycdn.com/course/240x135/851712_fc61_6.jpg', 
 0, 'VND', 1, GETUTCDATE()),

(1, 'Complete C# Unity 2D Game Development', 'unity-2d-game-dev', 
 'Phát triển game 2D với Unity và C#', 
 'https://img-c.udemycdn.com/course/240x135/258316_3a48_11.jpg', 
 599000, 'VND', 1, GETUTCDATE()),

(1, 'Complete C# Masterclass', 'csharp-masterclass', 
 'Làm chủ ngôn ngữ lập trình C#', 
 'https://img-c.udemycdn.com/course/240x135/1242834_58e4_4.jpg', 
 799000, 'VND', 1, GETUTCDATE());