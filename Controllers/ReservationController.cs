using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin: view all reservations
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Book)
                .Include(r => r.ApplicationUser)
                .AsSplitQuery()
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        // Member: view own reservations
        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reservations = await _context.Reservations
     .AsNoTracking()
     .Include(r => r.Book)
     .Include(r => r.ApplicationUser)
     .Where(r => r.ApplicationUserId == user.Id)
     .AsSplitQuery()
     .OrderByDescending(r => r.ReservationDate)
     .ToListAsync();

            return View(reservations);
        }

        // Member: reserve unavailable book
        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveBook(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return NotFound();
            }

            if (book.IsAvailable)
            {
                TempData["ErrorMessage"] = "This book is available. You can borrow it directly.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var alreadyBorrowedSameBook = await _context.BorrowTransactions
                .AnyAsync(t =>
                    t.BookId == bookId &&
                    t.ApplicationUserId == user.Id &&
                    t.Status == "Borrowed");

            if (alreadyBorrowedSameBook)
            {
                TempData["ErrorMessage"] = "You already borrowed this book.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var existingReservation = await _context.Reservations
                .AnyAsync(r =>
                    r.BookId == bookId &&
                    r.ApplicationUserId == user.Id &&
                    r.Status == "Pending");

            if (existingReservation)
            {
                TempData["ErrorMessage"] = "You already have a pending reservation for this book.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var reservation = new Reservation
            {
                BookId = bookId,
                ApplicationUserId = user.Id,
                ReservationDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book reserved successfully.";
            return RedirectToAction(nameof(MyReservations));
        }

        // Member: cancel own pending reservation
        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.ApplicationUserId == user.Id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending reservations can be cancelled.";
                return RedirectToAction(nameof(MyReservations));
            }

            reservation.Status = "Cancelled";
            reservation.CancelledDate = DateTime.Now;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(MyReservations));
        }

        // Admin: approve reservation and automatically issue book to reserved member
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending reservations can be approved.";
                return RedirectToAction(nameof(Index));
            }

            if (reservation.Book == null)
            {
                TempData["ErrorMessage"] = "Book record was not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!reservation.Book.IsAvailable)
            {
                TempData["ErrorMessage"] = "Book is not available yet. Return the book first before approving this reservation.";
                return RedirectToAction(nameof(Index));
            }

            var memberActiveLoans = await _context.BorrowTransactions
                .CountAsync(t =>
                    t.ApplicationUserId == reservation.ApplicationUserId &&
                    t.Status == "Borrowed");

            int loanDurationDays = 14;
            int maxBooksPerMember = 5;

            var borrowingSetting = await _context.BorrowingSettings
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (borrowingSetting != null)
            {
                loanDurationDays = borrowingSetting.LoanDurationDays;
                maxBooksPerMember = borrowingSetting.MaxBooksPerMember;
            }

            if (memberActiveLoans >= maxBooksPerMember)
            {
                TempData["ErrorMessage"] = "This member has reached the maximum borrowing limit.";
                return RedirectToAction(nameof(Index));
            }

            var transaction = new BorrowTransaction
            {
                BookId = reservation.BookId,
                ApplicationUserId = reservation.ApplicationUserId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(loanDurationDays),
                Status = "Borrowed",
                FineAmount = 0
            };

            reservation.Status = "Approved";
            reservation.ApprovedDate = DateTime.Now;

            reservation.Book.IsAvailable = false;

            _context.BorrowTransactions.Add(transaction);
            _context.Reservations.Update(reservation);
            _context.Books.Update(reservation.Book);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation approved and book issued to the member successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Admin: cancel reservation
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelByAdmin(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending reservations can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Status = "Cancelled";
            reservation.CancelledDate = DateTime.Now;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}