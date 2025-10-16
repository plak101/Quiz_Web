using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.Entities;

namespace Quiz_Web.Models.EF;

public partial class LearningPlatformContext : DbContext
{
    public LearningPlatformContext()
    {
    }

    public LearningPlatformContext(DbContextOptions<LearningPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttemptAnswer> AttemptAnswers { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassAnnouncement> ClassAnnouncements { get; set; }

    public virtual DbSet<ClassAssignment> ClassAssignments { get; set; }

    public virtual DbSet<ClassStudent> ClassStudents { get; set; }

    public virtual DbSet<ContentShare> ContentShares { get; set; }

    public virtual DbSet<ContentTag> ContentTags { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseContent> CourseContents { get; set; }

    public virtual DbSet<CoursePurchase> CoursePurchases { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<Entities.File> Files { get; set; }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<FlashcardPracticeLog> FlashcardPracticeLogs { get; set; }

    public virtual DbSet<FlashcardSet> FlashcardSets { get; set; }

    public virtual DbSet<Folder> Folders { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<Library> Libraries { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationChannel> NotificationChannels { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionClozeBlank> QuestionClozeBlanks { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<QuestionRangeAnswer> QuestionRangeAnswers { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SavedItem> SavedItems { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SubmissionItem> SubmissionItems { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestAssignment> TestAssignments { get; set; }

    public virtual DbSet<TestAttempt> TestAttempts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttemptAnswer>(entity =>
        {
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Attempt).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AAnswers_Attempt");

            entity.HasOne(d => d.Grader).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.GraderId)
                .HasConstraintName("FK_AAnswers_Grader");

            entity.HasOne(d => d.Question).WithMany(p => p.AttemptAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AAnswers_Question");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertId);

            entity.HasIndex(e => e.VerifyCode, "UQ_Certificates_Verify").IsUnique();

            entity.Property(e => e.IssuedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Serial).HasMaxLength(50);
            entity.Property(e => e.VerifyCode).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_Course");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_User");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_Classes_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Term).HasMaxLength(50);

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Teacher");
        });

        modelBuilder.Entity<ClassAnnouncement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.ClassAnnouncements)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CAnnouncements_Author");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassAnnouncements)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CAnnouncements_Class");
        });

        modelBuilder.Entity<ClassAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);

            entity.Property(e => e.GradingPolicy).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassAssignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CAssignments_Class");
        });

        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.HasKey(e => new { e.ClassId, e.StudentId });

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassStudents_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassStudents_Student");
        });

        modelBuilder.Entity<ContentShare>(entity =>
        {
            entity.HasKey(e => e.ShareId);

            entity.Property(e => e.CanView).HasDefaultValue(true);
            entity.Property(e => e.ContentType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TargetType)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ContentTag>(entity =>
        {
            entity.Property(e => e.ContentType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Tag).WithMany(p => p.ContentTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContentTags_Tag");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.Slug, "UQ_Courses_Slug").IsUnique();

            entity.Property(e => e.CoverUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND");
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Owner).WithMany(p => p.Courses)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Owner");
        });

        modelBuilder.Entity<CourseContent>(entity =>
        {
            entity.Property(e => e.ContentType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TitleOverride).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseContents)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CContents_Course");

            entity.HasOne(d => d.Section).WithMany(p => p.CourseContents)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_CContents_Section");
        });

        modelBuilder.Entity<CoursePurchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId);

            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.PricePaid).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PurchasedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Buyer).WithMany(p => p.CoursePurchases)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CPurchases_Buyer");

            entity.HasOne(d => d.Course).WithMany(p => p.CoursePurchases)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CPurchases_Course");
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasKey(e => e.SectionId);

            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CSections_Course");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorId);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(4000);
            entity.Property(e => e.Severity)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Entities.File>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.StoragePath).HasMaxLength(500);

            entity.HasOne(d => d.Owner).WithMany(p => p.Files)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Files_Owner");
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Hint).HasMaxLength(500);

            entity.HasOne(d => d.BackMedia).WithMany(p => p.FlashcardBackMedia)
                .HasForeignKey(d => d.BackMediaId)
                .HasConstraintName("FK_Flashcards_BackMedia");

            entity.HasOne(d => d.FrontMedia).WithMany(p => p.FlashcardFrontMedia)
                .HasForeignKey(d => d.FrontMediaId)
                .HasConstraintName("FK_Flashcards_FrontMedia");

            entity.HasOne(d => d.Set).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.SetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flashcards_Set");
        });

        modelBuilder.Entity<FlashcardPracticeLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.Property(e => e.EaseFactor).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.Card).WithMany(p => p.FlashcardPracticeLogs)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FPL_Card");

            entity.HasOne(d => d.Set).WithMany(p => p.FlashcardPracticeLogs)
                .HasForeignKey(d => d.SetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FPL_Set");

            entity.HasOne(d => d.User).WithMany(p => p.FlashcardPracticeLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FPL_User");
        });

        modelBuilder.Entity<FlashcardSet>(entity =>
        {
            entity.HasKey(e => e.SetId);

            entity.Property(e => e.CoverUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Language).HasMaxLength(20);
            entity.Property(e => e.TagsText).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Visibility)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.FlashcardSets)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FlashcardSets_Owner");
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Library).WithMany(p => p.Folders)
                .HasForeignKey(d => d.LibraryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Folders_Library");

            entity.HasOne(d => d.ParentFolder).WithMany(p => p.InverseParentFolder)
                .HasForeignKey(d => d.ParentFolderId)
                .HasConstraintName("FK_Folders_Parent");
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.InviteId);

            entity.HasIndex(e => e.Token, "UQ_Invitations_Token").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.RoleSuggested)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Token).HasMaxLength(100);

            entity.HasOne(d => d.Class).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_Invitations_Class");

            entity.HasOne(d => d.Inviter).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.InviterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitations_Inviter");
        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Owner).WithMany(p => p.Libraries)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Libraries_Owner");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(40)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<NotificationChannel>(entity =>
        {
            entity.HasKey(e => e.ChannelId);

            entity.Property(e => e.AddressOrToken).HasMaxLength(500);
            entity.Property(e => e.Channel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.NotificationChannels)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NChannels_User");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Provider)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ProviderRef).HasMaxLength(120);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Purchase).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PurchaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Purchase");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(e => e.Points)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.StemMedia).WithMany(p => p.Questions)
                .HasForeignKey(d => d.StemMediaId)
                .HasConstraintName("FK_Questions_StemMedia");

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Questions_Test");
        });

        modelBuilder.Entity<QuestionClozeBlank>(entity =>
        {
            entity.HasKey(e => e.BlankId);

            entity.Property(e => e.AcceptRegex).HasMaxLength(400);
            entity.Property(e => e.CorrectText).HasMaxLength(400);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionClozeBlanks)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QCloze_Question");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId);

            entity.HasOne(d => d.OptionMedia).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.OptionMediaId)
                .HasConstraintName("FK_QOptions_Media");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QOptions_Question");
        });

        modelBuilder.Entity<QuestionRangeAnswer>(entity =>
        {
            entity.HasKey(e => e.RangeId);

            entity.Property(e => e.MaxValue).HasColumnType("decimal(12, 4)");
            entity.Property(e => e.MinValue).HasColumnType("decimal(12, 4)");
            entity.Property(e => e.Tolerance).HasColumnType("decimal(12, 4)");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionRangeAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QRange_Question");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.Property(e => e.RelatedType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reminders_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SavedItem>(entity =>
        {
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ContentType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.Folder).WithMany(p => p.SavedItems)
                .HasForeignKey(d => d.FolderId)
                .HasConstraintName("FK_SavedItems_Folder");

            entity.HasOne(d => d.Library).WithMany(p => p.SavedItems)
                .HasForeignKey(d => d.LibraryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SavedItems_Library");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(e => e.MaxScore).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TotalScore).HasColumnType("decimal(6, 2)");
        });

        modelBuilder.Entity<SubmissionItem>(entity =>
        {
            entity.Property(e => e.MaxScore).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.RefType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Score).HasColumnType("decimal(6, 2)");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Tags_Name").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ_Tags_Slug").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.Slug).HasMaxLength(100);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GradingMode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Visibility)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.Tests)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tests_Owner");
        });

        modelBuilder.Entity<TestAssignment>(entity =>
        {
            entity.HasKey(e => e.TestAssignId);

            entity.HasOne(d => d.Assignment).WithMany(p => p.TestAssignments)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TAssign_Assignment");

            entity.HasOne(d => d.Test).WithMany(p => p.TestAssignments)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TAssign_Test");
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId);

            entity.Property(e => e.MaxScore).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Score).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .IsUnicode(false);

            entity.HasOne(d => d.Test).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attempts_Test");

            entity.HasOne(d => d.User).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attempts_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Role"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_User"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.GradeLevel).HasMaxLength(50);
            entity.Property(e => e.Locale).HasMaxLength(10);
            entity.Property(e => e.SchoolName).HasMaxLength(200);
            entity.Property(e => e.TimeZone).HasMaxLength(64);

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProfiles_User");
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.EmailOptIn).HasDefaultValue(true);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.PushOptIn).HasDefaultValue(true);
            entity.Property(e => e.TimeZone).HasMaxLength(64);
            entity.Property(e => e.UiTheme)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.UserSetting)
                .HasForeignKey<UserSetting>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSettings_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
