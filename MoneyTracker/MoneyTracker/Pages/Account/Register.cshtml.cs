using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterModel(ExpenseManagerContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(256, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var normalizedUserName = Input.UserName.Trim().ToUpperInvariant();
        var normalizedEmail = Input.Email.Trim().ToUpperInvariant();

        var exists = await _db.Users.AnyAsync(u => u.NormalizedUserName == normalizedUserName || u.NormalizedEmail == normalizedEmail);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "Username or Email already exists.");
            return Page();
        }

        var user = new User
        {
            UserName = Input.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            Email = Input.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            EmailConfirmed = false,
            Enabled = true,
            Language = "vi",
            DefaultCurrency = "VND",
            Timezone = "Asia/Ho_Chi_Minh",
            Theme = "light",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, Input.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Account/Login");
    }
}


