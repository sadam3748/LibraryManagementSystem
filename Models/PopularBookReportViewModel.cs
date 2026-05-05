namespace LibraryManagementSystem.Models
{
    public class PopularBookReportViewModel
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int BorrowCount { get; set; }
    }
}