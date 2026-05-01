using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowings = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Where(t => t.ApplicationUserId == user.Id)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            var pendingReservations = await _context.Reservations
    .AsNoTracking()
    .CountAsync(r =>
        r.ApplicationUserId == user.Id &&
        r.Status == "Pending");

            ViewBag.PendingReservations = pendingReservations;

            ViewBag.TotalBorrowed = borrowings.Count;
            ViewBag.CurrentlyBorrowed = borrowings.Count(t => t.Status == "Borrowed");
            ViewBag.Overdue = borrowings.Count(t =>
                t.Status == "Borrowed" &&
                t.DueDate.Date < DateTime.Now.Date);

            ViewBag.RecentBorrowings = borrowings.Take(5).ToList();

            return View();
        }
    }
}