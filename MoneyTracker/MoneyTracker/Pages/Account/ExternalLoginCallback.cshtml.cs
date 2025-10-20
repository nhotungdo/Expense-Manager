using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class ExternalLoginCallbackModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExternalLoginCallbackModel(ExpenseManagerContext db) { _db = db; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        var authResult = await HttpContext.AuthenticateAsync("External");
        var principal = authResult?.Principal ?? User;
        var loginProvider = principal.FindFirst(ClaimTypes.AuthenticationMethod)?.Value
            ?? principal.Identity?.AuthenticationType
            ?? (authResult?.Ticket?.AuthenticationScheme ?? "");

        var externalUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal.Identity?.Name ?? email ?? externalUserId;

        User? user = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.ToUpperInvariant();
            user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        }
        if (user == null)
        {
            // Try user login mapping
            user = await _db.Users
                .Include(u => u.AspNetUserLogins)
                .FirstOrDefaultAsync(u => u.AspNetUserLogins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == externalUserId));
        }

        if (user == null)
        {
            user = new User
            {
                UserName = name,
                NormalizedUserName = name.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email?.ToUpperInvariant(),
                EmailConfirmed = true,
                Enabled = true,
                Language = "vi",
                DefaultCurrency = "VND",
                Timezone = "Asia/Ho_Chi_Minh",
                Theme = "light",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // Add/ensure external login link
        var existingLogin = await _db.AspNetUserLogins.FirstOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == externalUserId && l.UserId == user.Id);
        if (existingLogin == null)
        {
            _db.AspNetUserLogins.Add(new AspNetUserLogin
            {
                LoginProvider = loginProvider,
                ProviderKey = externalUserId,
                ProviderDisplayName = loginProvider,
                UserId = user.Id
            });
            await _db.SaveChangesAsync();
        }

        // Sign into app cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToPage("/Index");
    }
}


