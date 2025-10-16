using System;
using System.Collections.Generic;

namespace Quiz_Web.Models.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string? CoverUrl { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CourseContent> CourseContents { get; set; } = new List<CourseContent>();

    public virtual ICollection<CoursePurchase> CoursePurchases { get; set; } = new List<CoursePurchase>();

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual User Owner { get; set; } = null!;
}
