using System;
using System.Collections.Generic;

namespace Quiz_Web.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Phone { get; set; }

    public int? RoleId { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ClassAnnouncement> ClassAnnouncements { get; set; } = new List<ClassAnnouncement>();

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<CoursePurchase> CoursePurchases { get; set; } = new List<CoursePurchase>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<FlashcardPracticeLog> FlashcardPracticeLogs { get; set; } = new List<FlashcardPracticeLog>();

    public virtual ICollection<FlashcardSet> FlashcardSets { get; set; } = new List<FlashcardSet>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Library> Libraries { get; set; } = new List<Library>();

    public virtual ICollection<NotificationChannel> NotificationChannels { get; set; } = new List<NotificationChannel>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual UserSetting? UserSetting { get; set; }
}
