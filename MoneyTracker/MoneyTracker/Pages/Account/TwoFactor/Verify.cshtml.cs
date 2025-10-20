using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account.TwoFactor;

public class VerifyModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public VerifyModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Authenticator code")]
        public string Code { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (TempData["2fa_uid"] == null) return RedirectToPage("/Account/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var uidStr = TempData["2fa_uid"] as string;
        var rememberStr = TempData["2fa_remember"] as string;
        var returnUrl = TempData["2fa_return"] as string;
        if (!long.TryParse(uidStr, out var userId)) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Enabled);
        if (user == null || string.IsNullOrEmpty(user.SecurityStamp)) return RedirectToPage("/Account/Login");

        if (!TotpHelper.ValidateTotp(user.SecurityStamp, Input.Code))
        {
            ModelState.AddModelError(string.Empty, "Invalid code.");
            // restore tempdata for another try
            TempData["2fa_uid"] = uidStr;
            TempData["2fa_remember"] = rememberStr;
            TempData["2fa_return"] = returnUrl ?? string.Empty;
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProps = new AuthenticationProperties { IsPersistent = bool.TryParse(rememberStr, out var r) && r };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToPage("/Index");
    }
}


