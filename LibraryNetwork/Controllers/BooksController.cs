using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LibraryNetwork.Data;
using LibraryNetwork.Models;

namespace LibraryNetwork.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: Books (public — everyone can browse)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchTitle, string searchAuthor, string category, int? libraryId)
        {
            var books = _context.Books
                .Include(b => b.BookCopies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
                books = books.Where(b => b.Title!.Contains(searchTitle));

            if (!string.IsNullOrEmpty(searchAuthor))
                books = books.Where(b => b.Author!.Contains(searchAuthor));

            if (!string.IsNullOrEmpty(category))
                books = books.Where(b => b.Category == category);

            if (libraryId.HasValue)
                books = books.Where(b => b.BookCopies.Any(bc => bc.LibraryId == libraryId.Value));

            ViewData["Categories"] = new SelectList(
                await _context.Books.Select(b => b.Category).Distinct().ToListAsync());

            ViewData["Libraries"] = new SelectList(_context.Libraries, "Id", "Name");

            return View(await books.ToListAsync());
        }

        // GET: Books/Details/5 (public)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BookCopies)
                    .ThenInclude(bc => bc.Library)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create (Admin only)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                book.ImageUrl = await SaveImageAsync(imageFile);
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? imageFile)
        {
            if (id != book.Id) return NotFound();

            var existing = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    DeleteImageIfExists(existing.ImageUrl);
                    book.ImageUrl = await SaveImageAsync(imageFile);
                }
                else
                {
                    book.ImageUrl = existing.ImageUrl;
                }

                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                DeleteImageIfExists(book.ImageUrl);
                _context.Books.Remove(book);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "books");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext)) return null;

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            return $"/images/books/{fileName}";
        }

        private void DeleteImageIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            if (!imageUrl.StartsWith("/images/books/")) return;

            var relative = imageUrl.TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_env.WebRootPath, relative);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
