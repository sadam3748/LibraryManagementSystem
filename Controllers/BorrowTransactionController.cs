using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BorrowTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowTransactionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Include(t => t.ApplicationUser)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            return View(transactions);
        }

        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> MyBorrowings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transactions = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Where(t => t.ApplicationUserId == user.Id)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            return View(transactions);
        }

        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return NotFound();
            }

            if (!book.IsAvailable)
            {
                TempData["ErrorMessage"] = "This book is currently not available.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var setting = await GetBorrowingSettingAsync();

            ViewBag.LoanDurationDays = setting.LoanDurationDays;
            ViewBag.DueDate = DateTime.Now.AddDays(setting.LoanDurationDays);

            return View(book);
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirmed(int bookId)
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

            if (!book.IsAvailable)
            {
                TempData["ErrorMessage"] = "This book is currently not available.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var setting = await GetBorrowingSettingAsync();

            var activeBorrowCount = await _context.BorrowTransactions
                .CountAsync(t =>
                    t.ApplicationUserId == user.Id &&
                    t.Status == "Borrowed");

            if (activeBorrowCount >= setting.MaxBooksPerMember)
            {
                TempData["ErrorMessage"] = $"You can only borrow maximum {setting.MaxBooksPerMember} book(s) at one time.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var transaction = new BorrowTransaction
            {
                BookId = book.Id,
                ApplicationUserId = user.Id,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(setting.LoanDurationDays),
                Status = "Borrowed",
                FineAmount = 0,
                RenewCount = 0
            };

            book.IsAvailable = false;

            _context.BorrowTransactions.Add(transaction);
            _context.Books.Update(book);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book borrowed successfully.";
            return RedirectToAction(nameof(MyBorrowings));
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewBook(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.BorrowTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.ApplicationUserId == user.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != "Borrowed")
            {
                TempData["ErrorMessage"] = "Only active borrowed books can be renewed.";
                return RedirectToAction(nameof(MyBorrowings));
            }

            if (transaction.DueDate.Date < DateTime.Now.Date)
            {
                TempData["ErrorMessage"] = "Overdue books cannot be renewed. Please return the book first.";
                return RedirectToAction(nameof(MyBorrowings));
            }

            var setting = await GetBorrowingSettingAsync();

            if (transaction.RenewCount >= setting.RenewalLimit)
            {
                TempData["ErrorMessage"] = $"Renewal limit reached. You can renew this book only {setting.RenewalLimit} time(s).";
                return RedirectToAction(nameof(MyBorrowings));
            }

            transaction.DueDate = transaction.DueDate.AddDays(setting.LoanDurationDays);
            transaction.RenewCount += 1;

            _context.BorrowTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book renewed successfully.";
            return RedirectToAction(nameof(MyBorrowings));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var transaction = await _context.BorrowTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status == "Returned")
            {
                TempData["ErrorMessage"] = "This book has already been returned.";
                return RedirectToAction(nameof(Index));
            }

            var setting = await GetBorrowingSettingAsync();

            transaction.ReturnDate = DateTime.Now;
            transaction.Status = "Returned";

            if (transaction.ReturnDate.Value.Date > transaction.DueDate.Date)
            {
                int lateDays = (transaction.ReturnDate.Value.Date - transaction.DueDate.Date).Days;
                transaction.FineAmount = lateDays * setting.FinePerDay;
            }
            else
            {
                transaction.FineAmount = 0;
            }

            if (transaction.Book != null)
            {
                transaction.Book.IsAvailable = true;
                _context.Books.Update(transaction.Book);
            }

            _context.BorrowTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book returned successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<BorrowingSetting> GetBorrowingSettingAsync()
        {
            var setting = await _context.BorrowingSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                setting = new BorrowingSetting
                {
                    LoanDurationDays = 14,
                    MaxBooksPerMember = 3,
                    FinePerDay = 1.00m,
                    RenewalLimit = 1,
                    UpdatedDate = DateTime.Now
                };

                _context.BorrowingSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return setting;
        }
    }
}