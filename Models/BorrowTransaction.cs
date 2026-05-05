using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BorrowTransaction
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Borrowed";

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fine Amount")]
        public decimal FineAmount { get; set; } = 0;

        [Display(Name = "Renew Count")]
        public int RenewCount { get; set; } = 0;
    }
}