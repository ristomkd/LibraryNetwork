using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LibraryNetwork.Data;
using LibraryNetwork.Models;

namespace LibraryNetwork.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LibrariansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibrariansController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        private async Task<int?> GetUserLibraryIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.LibraryId;
        }

        // GET: Librarians
        public async Task<IActionResult> Index()
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var librarians = await _context.Librarians
                .Include(l => l.Library)
                .Where(l => l.LibraryId == libraryId.Value)
                .ToListAsync();

            return View(librarians);
        }

        // GET: Librarians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var librarian = await _context.Librarians
                .Include(l => l.Library)
                .FirstOrDefaultAsync(m => m.Id == id && m.LibraryId == libraryId.Value);

            if (librarian == null) return NotFound();
            return View(librarian);
        }

        // GET: Librarians/Create
        public async Task<IActionResult> Create()
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            return View();
        }

        // POST: Librarians/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Librarian librarian, IFormFile? imageFile)
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            // Force LibraryId to the admin's own library
            librarian.LibraryId = libraryId.Value;

            if (ModelState.IsValid)
            {
                librarian.ImageUrl = await SaveImageAsync(imageFile, "librarians");
                _context.Add(librarian);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Librarian added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(librarian);
        }

        // GET: Librarians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var librarian = await _context.Librarians
                .FirstOrDefaultAsync(l => l.Id == id && l.LibraryId == libraryId.Value);

            if (librarian == null) return NotFound();

            return View(librarian);
        }

        // POST: Librarians/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Librarian librarian, IFormFile? imageFile)
        {
            if (id != librarian.Id) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var existing = await _context.Librarians
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && l.LibraryId == libraryId.Value);

            if (existing == null) return NotFound();

            // Force LibraryId to admin's library
            librarian.LibraryId = libraryId.Value;

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    DeleteImageIfExists(existing.ImageUrl, "librarians");
                    librarian.ImageUrl = await SaveImageAsync(imageFile, "librarians");
                }
                else
                {
                    librarian.ImageUrl = existing.ImageUrl;
                }

                _context.Update(librarian);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Librarian updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(librarian);
        }

        // GET: Librarians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var librarian = await _context.Librarians
                .Include(l => l.Library)
                .FirstOrDefaultAsync(m => m.Id == id && m.LibraryId == libraryId.Value);

            if (librarian == null) return NotFound();
            return View(librarian);
        }

        // POST: Librarians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libraryId = await GetUserLibraryIdAsync();
            if (libraryId == null) return Forbid();

            var librarian = await _context.Librarians
                .FirstOrDefaultAsync(l => l.Id == id && l.LibraryId == libraryId.Value);

            if (librarian != null)
            {
                DeleteImageIfExists(librarian.ImageUrl, "librarians");
                _context.Librarians.Remove(librarian);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Librarian removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile, string folder)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext)) return null;

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }

        private void DeleteImageIfExists(string? imageUrl, string folder)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            if (!imageUrl.StartsWith($"/images/{folder}/")) return;

            var relative = imageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_env.WebRootPath, relative);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
