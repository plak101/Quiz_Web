using System.ComponentModel.DataAnnotations;

namespace Quiz_Web.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form t?o kh�a h?c nhi?u b??c
    /// </summary>
    public class CourseBuilderViewModel
    {
        // Step 1: Course Info
        [Required(ErrorMessage = "Ti�u ?? kh�a h?c l� b?t bu?c")]
        [StringLength(200, ErrorMessage = "Ti�u ?? kh�ng ???c v??t qu� 200 k� t?")]
        [Display(Name = "Ti�u ?? kh�a h?c")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Slug l� b?t bu?c")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug ch? ???c ch?a ch? th??ng, s? v� d?u g?ch ngang")]
        [StringLength(200, ErrorMessage = "Slug kh�ng ???c v??t qu� 200 k� t?")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = null!;

        [StringLength(5000, ErrorMessage = "M� t? kh�ng ???c v??t qu� 5000 k� t?")]
        [Display(Name = "M� t? ng?n")]
        public string? Summary { get; set; }

        [Display(Name = "Danh m?c")]
        public int? CategoryId { get; set; }

        [StringLength(2048, ErrorMessage = "Link ?nh qu� d�i")]
        [Display(Name = "Link ?nh b�a")]
        public string? CoverUrl { get; set; }

        [Range(0, 999999999, ErrorMessage = "Gi� ph?i t? 0 ??n 999,999,999")]
        [Display(Name = "Gi� kh�a h?c (VN?)")]
        public decimal? Price { get; set; }

        [Display(Name = "Xu?t b?n ngay")]
        public bool IsPublished { get; set; }

        // Step 2: Chapters and Lessons
        public List<ChapterBuilderViewModel> Chapters { get; set; } = new();
    }

    public class ChapterBuilderViewModel
    {
        public int? ChapterId { get; set; }

        [Required(ErrorMessage = "T�n ch??ng l� b?t bu?c")]
        [StringLength(200, ErrorMessage = "T�n ch??ng kh�ng ???c v??t qu� 200 k� t?")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "M� t? kh�ng ???c v??t qu� 2000 k� t?")]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        public List<LessonBuilderViewModel> Lessons { get; set; } = new();
    }

    public class LessonBuilderViewModel
    {
        public int? LessonId { get; set; }

        [Required(ErrorMessage = "T�n b�i h?c l� b?t bu?c")]
        [StringLength(200, ErrorMessage = "T�n b�i h?c kh�ng ???c v??t qu� 200 k� t?")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "M� t? kh�ng ???c v??t qu� 1000 k� t?")]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        [Required]
        public string Visibility { get; set; } = "Course"; // Public/Private/Course

        public List<LessonContentBuilderViewModel> Contents { get; set; } = new();
    }

    public class LessonContentBuilderViewModel
    {
        public int? ContentId { get; set; }

        [Required(ErrorMessage = "Lo?i n?i dung l� b?t bu?c")]
        public string ContentType { get; set; } = null!; // Video/Theory/FlashcardSet/Test

        public int? RefId { get; set; }

        [StringLength(200, ErrorMessage = "Ti�u ?? kh�ng ???c v??t qu� 200 k� t?")]
        public string? Title { get; set; }

        public string? Body { get; set; } // For Theory content type

        public int OrderIndex { get; set; }
    }

    /// <summary>
    /// ViewModel cho vi?c l?u nh�p (autosave)
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
    /// Response cho c�c API calls
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
