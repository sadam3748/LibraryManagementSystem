using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(80)]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(30)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Display(Name = "Published Year")]
        [Range(1000, 9999, ErrorMessage = "Enter a valid year")]
        public int? PublishedYear { get; set; }

        [StringLength(50)]
        public string Edition { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Cover Image")]
        public string CoverImage { get; set; } = string.Empty;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}