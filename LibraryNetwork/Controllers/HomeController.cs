using LibraryNetwork.Data;
using LibraryNetwork.Models;
using LibraryNetwork.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static LibraryNetwork.Models.Enums;

namespace LibraryNetwork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.LibraryId != null)
                {
                    var libraryId = user.LibraryId.Value;

                    var library = await _context.Libraries.FindAsync(libraryId);
                    var copies = await _context.BookCopies
                        .Where(bc => bc.LibraryId == libraryId)
                        .ToListAsync();

                    var loans = await _context.Loans
                        .Include(l => l.BookCopy)
                            .ThenInclude(bc => bc!.Book)
                        .Include(l => l.Member)
                        .Where(l => l.BookCopy!.LibraryId == libraryId)
                        .ToListAsync();

                    var dashboard = new AdminDashboardVM
                    {
                        LibraryName = library?.Name ?? "My Library",
                        TotalCopies = copies.Count,
                        AvailableCopies = copies.Count(c => c.IsAvailable),
                        ActiveLoans = loans.Count(l => l.ReturnDate == null && l.Status == LoanStatus.Active),
                        OverdueLoans = loans.Count(l => l.ReturnDate == null && l.DueDate.HasValue && DateTime.UtcNow.Date > l.DueDate.Value.Date),
                        RecentLoans = loans
                            .OrderByDescending(l => l.BorrowDate)
                            .Take(10)
                            .Select(l => new RecentLoanVM
                            {
                                LoanId = l.Id,
                                BookTitle = l.BookCopy?.Book?.Title ?? "",
                                MemberName = l.Member != null ? $"{l.Member.FirstName} {l.Member.LastName}" : "",
                                InventoryCode = l.BookCopy?.InventoryCode ?? "",
                                BorrowDate = l.BorrowDate,
                                DueDate = l.DueDate,
                                Status = l.IsOverdue ? "Overdue" : l.Status.ToString(),
                                IsOverdue = l.IsOverdue
                            })
                            .ToList()
                    };

                    return View("AdminDashboard", dashboard);
                }
            }

            // Pass stats for the public hero section
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalLibraries = await _context.Libraries.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
