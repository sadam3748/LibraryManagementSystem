using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BorrowingSetting
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Loan Duration Days")]
        [Range(1, 60, ErrorMessage = "Loan duration must be between 1 and 60 days")]
        public int LoanDurationDays { get; set; } = 14;

        [Required]
        [Display(Name = "Maximum Books Per Member")]
        [Range(1, 20, ErrorMessage = "Maximum books must be between 1 and 20")]
        public int MaxBooksPerMember { get; set; } = 3;

        [Required]
        [Display(Name = "Fine Per Day")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100, ErrorMessage = "Fine per day must be between 0 and 100")]
        public decimal FinePerDay { get; set; } = 1.00m;

        [Required]
        [Display(Name = "Renewal Limit")]
        [Range(0, 10, ErrorMessage = "Renewal limit must be between 0 and 10")]
        public int RenewalLimit { get; set; } = 1;

        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}