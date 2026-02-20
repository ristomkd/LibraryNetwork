using LibraryNetwork.Data;
using LibraryNetwork.Models;
using LibraryNetwork.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static LibraryNetwork.Models.Enums;

namespace LibraryNetwork.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const float FinePerDay = 20f;
        private const int DefaultLoanDays = 14;

        public LoansController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            IQueryable<Loan> query = _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Library)
                .Include(l => l.Member);

            if (User.IsInRole("Admin"))
            {
                if (user.LibraryId == null) return Forbid();
                // Admin sees only loans for copies in their library
                query = query.Where(l => l.BookCopy!.LibraryId == user.LibraryId.Value);
            }
            else
            {
                // Member sees only their own loans
                if (user.MemberId == null) return Forbid();
                query = query.Where(l => l.MemberId == user.MemberId.Value);
            }

            var loans = await query.OrderByDescending(l => l.BorrowDate).ToListAsync();

            foreach (var loan in loans)
            {
                ApplyFineIfOverdue(loan);
            }
            await _context.SaveChangesAsync();

            return View(loans);
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Library)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            // IDOR protection
            if (User.IsInRole("Admin"))
            {
                if (loan.BookCopy?.LibraryId != user.LibraryId) return NotFound();
            }
            else
            {
                if (loan.MemberId != user.MemberId) return NotFound();
            }

            ApplyFineIfOverdue(loan);
            await _context.SaveChangesAsync();

            return View(loan);
        }

        // GET: Loans/Create (Admin only — create loan for a member)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            ViewData["BookCopyId"] = new SelectList(
                await _context.BookCopies
                    .Include(bc => bc.Book)
                    .Where(bc => bc.IsAvailable && bc.LibraryId == user.LibraryId.Value)
                    .ToListAsync(),
                "Id", "InventoryCode");

            ViewData["MemberId"] = new SelectList(
                await _context.Members.ToListAsync(),
                "Id", "FullName");

            return View();
        }

        // POST: Loans/Create (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Loan loan)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            // Verify copy belongs to admin's library and is available
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.Id == loan.BookCopyId
                    && bc.LibraryId == user.LibraryId.Value);

            if (bookCopy == null) return NotFound();

            if (!bookCopy.IsAvailable)
            {
                ModelState.AddModelError("BookCopyId", "This copy is not available.");
                await PopulateCreateDropdowns(user.LibraryId.Value, loan);
                return View(loan);
            }

            // Set loan defaults
            loan.BorrowDate = DateTime.UtcNow;
            loan.DueDate = DateTime.UtcNow.AddDays(DefaultLoanDays);
            loan.Status = LoanStatus.Active;
            loan.ReturnDate = null;
            loan.FineAmount = null;

            bookCopy.IsAvailable = false;

            _context.Add(loan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Borrow/5 (Member — pick from available copies for BookId)
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Borrow(int? id) // id = BookId
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.MemberId == null) return Forbid();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var availableCopies = await _context.BookCopies
                .Include(bc => bc.Library)
                .Where(bc => bc.BookId == id && bc.IsAvailable)
                .Select(bc => new AvailableCopyVM
                {
                    BookCopyId = bc.Id,
                    InventoryCode = bc.InventoryCode ?? "",
                    LibraryName = bc.Library!.Name ?? "",
                    PricePerDay = bc.PricePerDay ?? 0
                })
                .ToListAsync();

            if (!availableCopies.Any())
            {
                TempData["Error"] = "No copies are currently available for this book.";
                return RedirectToAction("Details", "Books", new { id });
            }

            var vm = new BorrowVM
            {
                BookId = book.Id,
                BookTitle = book.Title ?? "",
                Author = book.Author ?? "",
                ImageUrl = book.ImageUrl,
                AvailableCopies = availableCopies
            };

            return View(vm);
        }

        // POST: Loans/Borrow
        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowConfirm(int bookCopyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.MemberId == null) return Forbid();

            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Id == bookCopyId && bc.IsAvailable);

            if (bookCopy == null)
            {
                TempData["Error"] = "This copy is no longer available.";
                return RedirectToAction("Index", "Books");
            }

            var loan = new Loan
            {
                MemberId = user.MemberId.Value,
                BookCopyId = bookCopy.Id,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(DefaultLoanDays),
                Status = LoanStatus.Active
            };

            bookCopy.IsAvailable = false;

            _context.Add(loan);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"You borrowed \"{bookCopy.Book?.Title}\". Due back by {loan.DueDate:MMM dd, yyyy}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Loans/Return/5 (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.BookCopy!.LibraryId == user.LibraryId.Value);

            if (loan == null) return NotFound();

            loan.ReturnDate = DateTime.UtcNow;
            loan.Status = LoanStatus.Returned;

            ApplyFineIfOverdue(loan);

            if (loan.BookCopy != null)
                loan.BookCopy.IsAvailable = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Loans/PayFine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFine(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            // IDOR check
            if (User.IsInRole("Admin"))
            {
                if (loan.BookCopy?.LibraryId != user.LibraryId) return NotFound();
            }
            else
            {
                if (loan.MemberId != user.MemberId) return NotFound();
            }

            ApplyFineIfOverdue(loan);

            if (loan.FineAmount.HasValue && loan.FineAmount > 0)
            {
                loan.IsFinePaid = true;
                loan.FinePaidDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Loans/Edit/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.BookCopy!.LibraryId == user.LibraryId.Value);

            if (loan == null) return NotFound();

            ViewData["BookCopyId"] = new SelectList(
                await _context.BookCopies
                    .Include(bc => bc.Book)
                    .Where(bc => bc.LibraryId == user.LibraryId.Value)
                    .ToListAsync(),
                "Id", "InventoryCode", loan.BookCopyId);

            ViewData["MemberId"] = new SelectList(
                await _context.Members.ToListAsync(),
                "Id", "FullName", loan.MemberId);

            return View(loan);
        }

        // POST: Loans/Edit/5 (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Loan loan)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            var existing = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.BookCopy!.LibraryId == user.LibraryId.Value);

            if (existing == null) return NotFound();

            existing.MemberId = loan.MemberId;
            existing.BorrowDate = loan.BorrowDate;
            existing.DueDate = loan.DueDate;
            existing.ReturnDate = loan.ReturnDate;
            existing.Semester = loan.Semester;
            existing.Status = loan.Status;
            existing.FineAmount = loan.FineAmount;
            existing.IsFinePaid = loan.IsFinePaid;
            existing.FinePaidDate = loan.FinePaidDate;

            if (existing.ReturnDate != null)
            {
                existing.Status = LoanStatus.Returned;
                if (existing.BookCopy != null)
                    existing.BookCopy.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Delete/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc!.Library)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.BookCopy!.LibraryId == user.LibraryId.Value);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Loans/Delete/5 (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.LibraryId == null) return Forbid();

            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.BookCopy!.LibraryId == user.LibraryId.Value);

            if (loan == null) return NotFound();

            // Restore availability if active loan is deleted
            if (loan.ReturnDate == null && loan.BookCopy != null)
                loan.BookCopy.IsAvailable = true;

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void ApplyFineIfOverdue(Loan loan)
        {
            if (loan.ReturnDate == null &&
                loan.DueDate.HasValue &&
                DateTime.UtcNow.Date > loan.DueDate.Value.Date)
            {
                loan.Status = LoanStatus.Overdue;
                int daysLate = (DateTime.UtcNow.Date - loan.DueDate.Value.Date).Days;
                loan.FineAmount = daysLate * FinePerDay;
            }
        }

        private async Task PopulateCreateDropdowns(int libraryId, Loan? loan = null)
        {
            ViewData["BookCopyId"] = new SelectList(
                await _context.BookCopies
                    .Include(bc => bc.Book)
                    .Where(bc => bc.IsAvailable && bc.LibraryId == libraryId)
                    .ToListAsync(),
                "Id", "InventoryCode", loan?.BookCopyId);

            ViewData["MemberId"] = new SelectList(
                await _context.Members.ToListAsync(),
                "Id", "FullName", loan?.MemberId);
        }
    }
}
