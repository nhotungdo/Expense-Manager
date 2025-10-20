using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    private readonly IPasswordHasher<User> _hasher;
    public ResetPasswordModel(ExpenseManagerContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Done { get; set; }

    public class InputModel
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Token { get; set; } = string.Empty;
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string email, string token)
    {
        Input.Email = email;
        Input.Token = token;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var normalizedEmail = Input.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.Enabled);
        if (user == null)
        {
            Done = true; // don't reveal user existence
            return Page();
        }

        var token = await _db.AspNetUserTokens.FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "PasswordReset" && t.Name == "ResetToken" && t.Value == Input.Token);
        if (token == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired token.");
            return Page();
        }

        user.PasswordHash = _hasher.HashPassword(user, Input.Password);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        _db.AspNetUserTokens.Remove(token);
        await _db.SaveChangesAsync();
        Done = true;
        return Page();
    }
}


