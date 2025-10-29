using System.ComponentModel.DataAnnotations;

namespace Quiz_Web.Models.ViewModels
{
    public class CreateFlashcardSetViewModel
    {
        [Required(ErrorMessage = "Ti�u ?? kh�ng ???c ?? tr?ng")]
        [StringLength(200, ErrorMessage = "Ti�u ?? kh�ng ???c v??t qu� 200 k� t?")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "M� t? kh�ng ???c v??t qu� 1000 k� t?")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ch? ?? hi?n th? kh�ng ???c ?? tr?ng")]
        public string Visibility { get; set; } = "Public";

        public string? CoverUrl { get; set; }

        public string? TagsText { get; set; }

        [StringLength(50, ErrorMessage = "Ng�n ng? kh�ng ???c v??t qu� 50 k� t?")]
        public string? Language { get; set; }
    }

    public class EditFlashcardSetViewModel
    {
        public int SetId { get; set; }

        [Required(ErrorMessage = "Ti�u ?? kh�ng ???c ?? tr?ng")]
        [StringLength(200, ErrorMessage = "Ti�u ?? kh�ng ???c v??t qu� 200 k� t?")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "M� t? kh�ng ???c v??t qu� 1000 k� t?")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ch? ?? hi?n th? kh�ng ???c ?? tr?ng")]
        public string Visibility { get; set; } = "Public";

        public string? CoverUrl { get; set; }

        public string? TagsText { get; set; }

        [StringLength(50, ErrorMessage = "Ng�n ng? kh�ng ???c v??t qu� 50 k� t?")]
        public string? Language { get; set; }
    }

    public class FlashcardSetDetailViewModel
    {
        public int SetId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Visibility { get; set; } = "Public";
        public string? CoverUrl { get; set; }
        public string? TagsText { get; set; }
        public string? Language { get; set; }
        public int CardCount { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateFlashcardViewModel
    {
        public int SetId { get; set; }

        [Required(ErrorMessage = "M?t tr??c kh�ng ???c ?? tr?ng")]
        [StringLength(500, ErrorMessage = "M?t tr??c kh�ng ???c v??t qu� 500 k� t?")]
        public string FrontText { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t sau kh�ng ???c ?? tr?ng")]
        [StringLength(500, ErrorMessage = "M?t sau kh�ng ???c v??t qu� 500 k� t?")]
        public string BackText { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "G?i � kh�ng ???c v??t qu� 200 k� t?")]
        public string? Hint { get; set; }

        public int OrderIndex { get; set; }
    }

    public class EditFlashcardViewModel
    {
        public int CardId { get; set; }

        public int SetId { get; set; }

        [Required(ErrorMessage = "M?t tr??c kh�ng ???c ?? tr?ng")]
        [StringLength(500, ErrorMessage = "M?t tr??c kh�ng ???c v??t qu� 500 k� t?")]
        public string FrontText { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t sau kh�ng ???c ?? tr?ng")]
        [StringLength(500, ErrorMessage = "M?t sau kh�ng ???c v??t qu� 500 k� t?")]
        public string BackText { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "G?i � kh�ng ???c v??t qu� 200 k� t?")]
        public string? Hint { get; set; }

        public int OrderIndex { get; set; }
    }
}
