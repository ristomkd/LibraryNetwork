using LibraryNetwork.Data;
using LibraryNetwork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryNetwork.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookCopiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserLibraryIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.LibraryId;
        }

        // GET: BookCopies
        public async Task<IActionResult> Index()
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var copies = await _context.BookCopies
                .Where(bc => bc.LibraryId == libraryId.Value)
                .Include(bc => bc.Book)
                .Include(bc => bc.Library)
                .OrderBy(bc => bc.Book!.Title)
                .ToListAsync();

            return View(copies);
        }

        // GET: BookCopies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .Include(bc => bc.Library)
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.LibraryId == libraryId.Value);

            if (bookCopy == null) return NotFound();

            return View(bookCopy);
        }

        // GET: BookCopies/Create
        public async Task<IActionResult> Create()
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            ViewData["BookId"] = new SelectList(
                await _context.Books.OrderBy(b => b.Title).ToListAsync(),
                "Id", "Title");

            return View();
        }

        // POST: BookCopies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCopy bookCopy, int quantity = 1)
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            if (quantity < 1) quantity = 1;
            if (quantity > 500) quantity = 500;

            for (int i = 0; i < quantity; i++)
            {
                var copy = new BookCopy
                {
                    BookId = bookCopy.BookId,
                    LibraryId = libraryId.Value,
                    IsAvailable = bookCopy.IsAvailable,
                    PricePerDay = bookCopy.PricePerDay,
                    InventoryCode = quantity == 1
                        ? bookCopy.InventoryCode
                        : $"{bookCopy.InventoryCode}-{i + 1:D3}"
                };
                _context.Add(copy);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{quantity} cop{(quantity == 1 ? "y" : "ies")} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: BookCopies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.LibraryId == libraryId.Value);

            if (bookCopy == null) return NotFound();

            ViewData["BookId"] = new SelectList(
                await _context.Books.OrderBy(b => b.Title).ToListAsync(),
                "Id", "Title", bookCopy.BookId);

            return View(bookCopy);
        }

        // POST: BookCopies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCopy bookCopy)
        {
            if (id != bookCopy.Id) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            // Verify the copy belongs to admin's library
            var existing = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.LibraryId == libraryId.Value);

            if (existing == null) return NotFound();

            // Update only allowed fields; force LibraryId
            existing.InventoryCode = bookCopy.InventoryCode;
            existing.IsAvailable = bookCopy.IsAvailable;
            existing.PricePerDay = bookCopy.PricePerDay;
            existing.BookId = bookCopy.BookId;
            // LibraryId stays as existing.LibraryId (admin's library)

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: BookCopies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .Include(bc => bc.Library)
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.LibraryId == libraryId.Value);

            if (bookCopy == null) return NotFound();

            return View(bookCopy);
        }

        // POST: BookCopies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.LibraryId == libraryId.Value);

            if (bookCopy == null) return NotFound();

            _context.BookCopies.Remove(bookCopy);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
