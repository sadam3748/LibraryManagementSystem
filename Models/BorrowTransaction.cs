using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public bool IsReturned { get; set; } = false;
    }
}