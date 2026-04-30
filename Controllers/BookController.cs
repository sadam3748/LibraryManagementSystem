using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Book
        // Admin and members can browse books
        [HttpGet]
        public async Task<IActionResult> Index(string? searchString, string? genre, string? availability)
        {
            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.ISBN.Contains(searchString) ||
                    b.Genre.Contains(searchString) ||
                    b.Publisher.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                booksQuery = booksQuery.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrWhiteSpace(availability))
            {
                if (availability == "Available")
                {
                    booksQuery = booksQuery.Where(b => b.IsAvailable);
                }
                else if (availability == "Unavailable")
                {
                    booksQuery = booksQuery.Where(b => !b.IsAvailable);
                }
            }

            var books = await booksQuery
                .OrderBy(b => b.Title)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.SelectedGenre = genre;
            ViewBag.SelectedAvailability = availability;

            ViewBag.Genres = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Genre))
                .Select(b => b.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            return View(books);
        }

        // GET: /Book/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: /Book/Create
        // Only admin can add books
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Book/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            book.CreatedDate = DateTime.Now;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Book/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            try
            {
                var existingBook = await _context.Books
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (existingBook == null)
                {
                    return NotFound();
                }

                book.CreatedDate = existingBook.CreatedDate;

                _context.Update(book);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Book updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookExists(book.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Book/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BookExists(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }
    }
}