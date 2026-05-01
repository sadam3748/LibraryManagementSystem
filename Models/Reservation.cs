using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        public DateTime? ApprovedDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";
    }
}