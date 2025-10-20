using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly ExpenseManagerContext _db;

    public ForgotPasswordModel(ExpenseManagerContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Sent { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
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

        var normalizedEmail = Input.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.Enabled);

        if (user != null)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            var userToken = new AspNetUserToken
            {
                UserId = user.Id,
                LoginProvider = "PasswordReset",
                Name = "ResetToken",
                Value = token
            };
            _db.AspNetUserTokens.Add(userToken);

            var resetUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { email = user.Email, token = token },
                protocol: Request.Scheme);

            _db.Emails.Add(new Email
            {
                UserId = user.Id,
                Subject = "Reset your password",
                Body = $"Click the link to reset your password: {resetUrl}",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        Sent = true;
        return Page();
    }
}


