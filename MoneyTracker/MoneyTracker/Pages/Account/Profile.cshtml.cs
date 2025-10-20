using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileModel(ExpenseManagerContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string CurrentAvatarUrl { get; set; } = "/favicon.ico";

    public class InputModel
    {
        [Display(Name = "First name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last name")]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToPage("/Account/Login");

        Input = new InputModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        };

        CurrentAvatarUrl = string.IsNullOrWhiteSpace(user.ProfilePictureUrl) ? "/favicon.ico" : user.ProfilePictureUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToPage("/Account/Login");

        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        user.PhoneNumber = Input.PhoneNumber;
        user.Address = Input.Address;

        // handle avatar upload
        var file = Request.Form.Files["avatar"];
        if (file != null && file.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploads);
            var fileName = $"u{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            user.ProfilePictureUrl = $"/uploads/avatars/{fileName}";
        }

        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}


