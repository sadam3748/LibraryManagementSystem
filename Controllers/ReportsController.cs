using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;

            var totalBooks = await _context.Books
                .AsNoTracking()
                .CountAsync();

            var availableBooks = await _context.Books
                .AsNoTracking()
                .CountAsync(b => b.IsAvailable);

            var unavailableBooks = await _context.Books
                .AsNoTracking()
                .CountAsync(b => !b.IsAvailable);

            var totalMembers = await _context.Users
                .AsNoTracking()
                .CountAsync();

            var totalTransactions = await _context.BorrowTransactions
                .AsNoTracking()
                .CountAsync();

            var activeLoans = await _context.BorrowTransactions
                .AsNoTracking()
                .CountAsync(t => t.Status == "Borrowed");

            var returnedBooks = await _context.BorrowTransactions
                .AsNoTracking()
                .CountAsync(t => t.Status == "Returned");

            var overdueBooks = await _context.BorrowTransactions
                .AsNoTracking()
                .CountAsync(t => t.Status == "Borrowed" && t.DueDate.Date < today);

            var totalFineAmount = await _context.BorrowTransactions
                .AsNoTracking()
                .SumAsync(t => t.FineAmount);

            var totalReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync();

            var pendingReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.Status == "Pending");

            var approvedReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.Status == "Approved");

            var cancelledReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.Status == "Cancelled");

            var recentTransactions = await _context.BorrowTransactions
                .AsNoTracking()
                .Include(t => t.Book)
                .Include(t => t.ApplicationUser)
                .AsSplitQuery()
                .OrderByDescending(t => t.BorrowDate)
                .Take(10)
                .ToListAsync();

            var recentReservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Book)
                .Include(r => r.ApplicationUser)
                .AsSplitQuery()
                .OrderByDescending(r => r.ReservationDate)
                .Take(10)
                .ToListAsync();

            var mostBorrowedBooks = await _context.BorrowTransactions
                .AsNoTracking()
                .Include(t => t.Book)
                .Where(t => t.Book != null)
                .GroupBy(t => new
                {
                    t.BookId,
                    BookTitle = t.Book!.Title,
                    BookAuthor = t.Book.Author
                })
                .Select(g => new MostBorrowedBookReportViewModel
                {
                    BookTitle = g.Key.BookTitle,
                    BookAuthor = g.Key.BookAuthor,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(x => x.BorrowCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalBooks = totalBooks;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.UnavailableBooks = unavailableBooks;
            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalTransactions = totalTransactions;
            ViewBag.ActiveLoans = activeLoans;
            ViewBag.ReturnedBooks = returnedBooks;
            ViewBag.OverdueBooks = overdueBooks;
            ViewBag.TotalFineAmount = totalFineAmount;

            ViewBag.TotalReservations = totalReservations;
            ViewBag.PendingReservations = pendingReservations;
            ViewBag.ApprovedReservations = approvedReservations;
            ViewBag.CancelledReservations = cancelledReservations;

            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.RecentReservations = recentReservations;
            ViewBag.MostBorrowedBooks = mostBorrowedBooks;

            return View();
        }
    }

    public class MostBorrowedBookReportViewModel
    {
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }
}