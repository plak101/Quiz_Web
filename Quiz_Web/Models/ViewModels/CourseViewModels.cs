namespace Quiz_Web.Models.ViewModels
{
    public class CreateCourseViewModel
    {
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "VND";
        public bool IsPublished { get; set; }
        public string? CoverUrl { get; set; }
    }

    public class EditCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "VND";
        public bool IsPublished { get; set; }
        public string? CoverUrl { get; set; }
    }
}