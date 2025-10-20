using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account.TwoFactor;

[Authorize]
public class EnableModel : PageModel
{
    private readonly ExpenseManagerContext _db;

    public EnableModel(ExpenseManagerContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string QrCodeDataUrl { get; set; } = string.Empty;
    public string ManualKey { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [Display(Name = "Authenticator code")]
        public string Code { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToPage("/Account/Login");

        if (string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            user.SecurityStamp = Guid.NewGuid().ToString("N");
            await _db.SaveChangesAsync();
        }

        var issuer = "MoneyTracker";
        var account = user.Email ?? user.UserName ?? ($"user-{user.Id}");
        ManualKey = user.SecurityStamp;
        var otpauth = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={ManualKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30&algorithm=SHA1";

        // Minimal QR using Google Chart API (for demo). Replace later with local QR generator.
        var chartUrl = $"https://chart.googleapis.com/chart?cht=qr&chs=220x220&chl={Uri.EscapeDataString(otpauth)}";
        QrCodeDataUrl = chartUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToPage("/Account/Login");

        if (string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            ModelState.AddModelError(string.Empty, "Invalid secret. Reload page.");
            return Page();
        }

        if (!TotpHelper.ValidateTotp(user.SecurityStamp, Input.Code))
        {
            ModelState.AddModelError(string.Empty, "Invalid code.");
            return Page();
        }

        user.TwoFactorEnabled = true;
        await _db.SaveChangesAsync();
        return RedirectToPage("/Account/Profile");
    }
}

internal static class TotpHelper
{
    public static bool ValidateTotp(string secret, string code)
    {
        // Minimal TOTP validator (30s window, SHA1). For production, use a library like Otp.NET.
        if (string.IsNullOrWhiteSpace(code)) return false;
        code = code.Trim().Replace(" ", "");
        var expected = GenerateTotp(secret, DateTimeOffset.UtcNow);
        var previous = GenerateTotp(secret, DateTimeOffset.UtcNow.AddSeconds(-30));
        var next = GenerateTotp(secret, DateTimeOffset.UtcNow.AddSeconds(30));
        return code == expected || code == previous || code == next;
    }

    private static string GenerateTotp(string secret, DateTimeOffset timestamp)
    {
        long timestep = (long)Math.Floor(timestamp.ToUnixTimeSeconds() / 30.0);
        var secretBytes = System.Text.Encoding.ASCII.GetBytes(secret);
        var timeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(timestep));
        using var hmac = new System.Security.Cryptography.HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);
        int offset = hash[^1] & 0x0F;
        int binary = ((hash[offset] & 0x7f) << 24)
                   | ((hash[offset + 1] & 0xff) << 16)
                   | ((hash[offset + 2] & 0xff) << 8)
                   | (hash[offset + 3] & 0xff);
        int otp = binary % 1000000;
        return otp.ToString("D6");
    }
}


