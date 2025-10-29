using System;
using System.Collections.Generic;

namespace Quiz_Web.Models.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Visibility { get; set; } = null!;

    public string? CoverUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<LessonSlide> LessonSlides { get; set; } = new List<LessonSlide>();

    public virtual User Owner { get; set; } = null!;
}
