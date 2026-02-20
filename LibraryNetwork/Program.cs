using LibraryNetwork.Data;
using LibraryNetwork.Models;
using LibraryNetwork.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

await SeedRolesAndUsersAsync(app);

app.Run();

static async Task SeedRolesAndUsersAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


    string[] roles = new[] { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Ensure the library exists for the admin
    var library = db.Libraries.FirstOrDefault(l => l.Name == "Brakja Miladinovci");

    if (library == null)
    {
        library = new Library { Name = "Brakja Miladinovci" };
        db.Libraries.Add(library);
        await db.SaveChangesAsync();
    }

    // Create admin user linked to Library 1
    string adminEmail = "admin@library.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            LibraryId = library.Id,
            DisplayName = "Admin"
        };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    else
    {
        bool changed = false;
        if (admin.LibraryId == null) { admin.LibraryId = library.Id; changed = true; }
        if (string.IsNullOrEmpty(admin.DisplayName)) { admin.DisplayName = "Admin"; changed = true; }
        if (changed) await userManager.UpdateAsync(admin);
    }

    // Create member user linked to Member 1
    string memberEmail = "member@library.com";
    var member = await userManager.FindByEmailAsync(memberEmail);

    if (member == null)
    {
        member = new ApplicationUser
        {
            UserName = memberEmail,
            Email = memberEmail,
            EmailConfirmed = true,
            MemberId = 1,
            DisplayName = "Risto Kizov"
        };
        await userManager.CreateAsync(member, "Member123!");
        await userManager.AddToRoleAsync(member, "Member");
    }
    else
    {
        bool changed = false;
        if (member.MemberId == null) { member.MemberId = 1; changed = true; }
        if (string.IsNullOrEmpty(member.DisplayName)) { member.DisplayName = "Risto Kizov"; changed = true; }
        if (changed) await userManager.UpdateAsync(member);
    }
}