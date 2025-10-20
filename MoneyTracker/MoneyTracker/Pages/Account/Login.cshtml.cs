using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginModel(ExpenseManagerContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var normalized = Input.UserNameOrEmail.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            (u.NormalizedUserName == normalized || u.NormalizedEmail == normalized) && u.Enabled);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        if (user.TwoFactorEnabled)
        {
            TempData["2fa_uid"] = user.Id.ToString();
            TempData["2fa_remember"] = Input.RememberMe.ToString();
            TempData["2fa_return"] = returnUrl ?? string.Empty;
            return RedirectToPage("/Account/TwoFactor/Verify");
        }

        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Index");
    }
}


