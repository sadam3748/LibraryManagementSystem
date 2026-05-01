using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");

            var totalBooks = await _context.Books.CountAsync();

            var activeBorrowings = await _context.BorrowTransactions
                .CountAsync(t => t.Status == "Borrowed");

            var overdueCount = await _context.BorrowTransactions
                .CountAsync(t =>
                    t.Status == "Borrowed" &&
                    t.DueDate.Date < DateTime.Now.Date);

            var pendingReservations = await _context.Reservations
                .CountAsync(r => r.Status == "Pending");

            ViewBag.PendingReservations = pendingReservations;

            var recentTransactions = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Include(t => t.ApplicationUser)
                .OrderByDescending(t => t.BorrowDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalMembers = members.Count;
            ViewBag.ActiveBorrowings = activeBorrowings;
            ViewBag.OverdueCount = overdueCount;
            ViewBag.RecentTransactions = recentTransactions;

            return View();
        }

        [HttpGet]
        public IActionResult CreateMember()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMember(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync("Member"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Member"));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");

                TempData["SuccessMessage"] = "Member account created successfully.";
                return RedirectToAction(nameof(Members));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Members()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");

            var model = members.Select(user => new AdminMemberViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                UserName = user.UserName ?? ""
            }).ToList();

            return View(model);
        }
    }
}