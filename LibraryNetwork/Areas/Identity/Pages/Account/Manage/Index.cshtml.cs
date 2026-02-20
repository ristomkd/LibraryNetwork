#nullable disable

using System.ComponentModel.DataAnnotations;
using LibraryNetwork.Data;
using LibraryNetwork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryNetwork.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>True when the user has no Member record yet (old accounts).</summary>
        public bool NeedsSetup { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            [StringLength(100, MinimumLength = 2)]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            [StringLength(100, MinimumLength = 2)]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            NeedsSetup = user.MemberId == null && !User.IsInRole("Admin");

            // Try to load linked member
            Member member = null;
            if (user.MemberId != null)
                member = await _db.Members.FindAsync(user.MemberId);

            Input = new InputModel
            {
                FirstName = member?.FirstName ?? "",
                LastName = member?.LastName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? member?.phoneNumber ?? ""
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                NeedsSetup = user.MemberId == null && !User.IsInRole("Admin");
                return Page();
            }

            // --- Update email if changed ---
            if (Input.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    NeedsSetup = user.MemberId == null && !User.IsInRole("Admin");
                    return Page();
                }
                await _userManager.SetUserNameAsync(user, Input.Email);
            }

            // --- Update phone ---
            await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

            // --- Update DisplayName ---
            user.DisplayName = $"{Input.FirstName} {Input.LastName}";

            // --- Link or create Member (skip for Admin-only accounts) ---
            if (!User.IsInRole("Admin"))
            {
                Member member = null;

                if (user.MemberId != null)
                {
                    // Already linked — just update fields
                    member = await _db.Members.FindAsync(user.MemberId);
                    if (member != null)
                    {
                        member.FirstName = Input.FirstName;
                        member.LastName = Input.LastName;
                        member.Email = Input.Email;
                        member.phoneNumber = Input.PhoneNumber;
                    }
                }
                else
                {
                    // Not linked yet — find existing unlinked member or create new
                    member = await _db.Members.FirstOrDefaultAsync(m =>
                        m.FirstName == Input.FirstName &&
                        m.LastName == Input.LastName &&
                        m.UserId == null);

                    if (member != null)
                    {
                        member.UserId = user.Id;
                        if (string.IsNullOrEmpty(member.Email))
                            member.Email = Input.Email;
                    }
                    else
                    {
                        member = new Member
                        {
                            FirstName = Input.FirstName,
                            LastName = Input.LastName,
                            Email = Input.Email,
                            phoneNumber = Input.PhoneNumber,
                            MembershipNumber = $"MEM-{DateTime.UtcNow:yyyyMMddHHmmss}",
                            UserId = user.Id
                        };
                        _db.Members.Add(member);
                    }

                    await _db.SaveChangesAsync();
                    user.MemberId = member.Id;

                    // Assign Member role if missing
                    if (!await _userManager.IsInRoleAsync(user, "Member"))
                        await _userManager.AddToRoleAsync(user, "Member");
                }
            }

            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();

            // Refresh the sign-in cookie so role claims update immediately
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated.";
            return RedirectToPage();
        }
    }
}