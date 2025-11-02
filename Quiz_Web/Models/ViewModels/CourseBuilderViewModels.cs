using System.ComponentModel.DataAnnotations;

namespace Quiz_Web.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form t?o khóa h?c nhi?u b??c
    /// </summary>
    public class CourseBuilderViewModel
    {
        // Step 1: Course Info
        [Required(ErrorMessage = "Tiêu ?? khóa h?c là b?t bu?c")]
        [StringLength(200, ErrorMessage = "Tiêu ?? không ???c v??t quá 200 ký t?")]
        [Display(Name = "Tiêu ?? khóa h?c")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Slug là b?t bu?c")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug ch? ???c ch?a ch? th??ng, s? và d?u g?ch ngang")]
        [StringLength(200, ErrorMessage = "Slug không ???c v??t quá 200 ký t?")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = null!;

        [StringLength(5000, ErrorMessage = "Mô t? không ???c v??t quá 5000 ký t?")]
        [Display(Name = "Mô t? ng?n")]
        public string? Summary { get; set; }

        [Display(Name = "Danh m?c")]
        public int? CategoryId { get; set; }

        [StringLength(2048, ErrorMessage = "Link ?nh quá dài")]
        [Display(Name = "Link ?nh bìa")]
        public string? CoverUrl { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá ph?i t? 0 ??n 999,999,999")]
        [Display(Name = "Giá khóa h?c (VN?)")]
        public decimal? Price { get; set; }

        [Display(Name = "Xu?t b?n ngay")]
        public bool IsPublished { get; set; }

        // Step 2: Chapters and Lessons
        public List<ChapterBuilderViewModel> Chapters { get; set; } = new();
    }

    public class ChapterBuilderViewModel
    {
        public int? ChapterId { get; set; }

        [Required(ErrorMessage = "Tên ch??ng là b?t bu?c")]
        [StringLength(200, ErrorMessage = "Tên ch??ng không ???c v??t quá 200 ký t?")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô t? không ???c v??t quá 2000 ký t?")]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        public List<LessonBuilderViewModel> Lessons { get; set; } = new();
    }

    public class LessonBuilderViewModel
    {
        public int? LessonId { get; set; }

        [Required(ErrorMessage = "Tên bài h?c là b?t bu?c")]
        [StringLength(200, ErrorMessage = "Tên bài h?c không ???c v??t quá 200 ký t?")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        [Required]
        public string Visibility { get; set; } = "Course"; // Public/Private/Course

        public List<LessonContentBuilderViewModel> Contents { get; set; } = new();
    }

    public class LessonContentBuilderViewModel
    {
        public int? ContentId { get; set; }

        [Required(ErrorMessage = "Lo?i n?i dung là b?t bu?c")]
        public string ContentType { get; set; } = null!; // Video/Theory/FlashcardSet/Test

        public int? RefId { get; set; }

        [StringLength(200, ErrorMessage = "Tiêu ?? không ???c v??t quá 200 ký t?")]
        public string? Title { get; set; }

        public string? Body { get; set; } // For Theory content type

        [StringLength(2048, ErrorMessage = "URL video quá dài")]
        public string? VideoUrl { get; set; } // For Video content type

        public int OrderIndex { get; set; }
    }

    /// <summary>
    /// ViewModel cho vi?c l?u nháp (autosave)
    /// </summary>
    public class CourseAutosaveViewModel
    {
        public int? CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        public int? CategoryId { get; set; }
        public string? CoverUrl { get; set; }
        public decimal? Price { get; set; }
        public bool IsPublished { get; set; }
    }

    /// <summary>
    /// Response cho các API calls
    /// </summary>
    public class CourseBuilderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? CourseId { get; set; }
        public string? Slug { get; set; }
        public object? Data { get; set; }
    }
}
