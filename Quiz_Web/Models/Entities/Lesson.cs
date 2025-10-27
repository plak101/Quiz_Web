using System;
using System.Collections.Generic;

namespace Quiz_Web.Models.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Visibility { get; set; } = null!;

    public string? CoverUrl { get; set; }

    public int? EstimatedTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User Owner { get; set; } = null!;
}
