using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorrowingSettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BorrowingSetting model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var setting = await _context.BorrowingSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                model.UpdatedDate = DateTime.Now;
                _context.BorrowingSettings.Add(model);
            }
            else
            {
                setting.LoanDurationDays = model.LoanDurationDays;
                setting.MaxBooksPerMember = model.MaxBooksPerMember;
                setting.FinePerDay = model.FinePerDay;
                setting.RenewalLimit = model.RenewalLimit;
                setting.UpdatedDate = DateTime.Now;

                _context.BorrowingSettings.Update(setting);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Borrowing settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}