using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ExpenseManagerContext _context;

    public SettingsController(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all user settings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserSettingsDto>> GetSettings()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserSettingsDto
        {
            Language = user.Language,
            DefaultCurrency = user.DefaultCurrency,
            Timezone = user.Timezone,
            Theme = user.Theme,
            PrimaryColor = GetClaimValue(user, "PrimaryColor") ?? "#10b981",
            DateFormat = GetClaimValue(user, "DateFormat") ?? "DD/MM/YYYY",
            TimeFormat = GetClaimValue(user, "TimeFormat") ?? "24h",
            FirstDayOfWeek = int.TryParse(GetClaimValue(user, "FirstDayOfWeek"), out var fd) ? fd : 1,
            NotificationsEnabled = bool.TryParse(GetClaimValue(user, "NotificationsEnabled"), out var ne) ? ne : true,
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications
        });
    }

    /// <summary>
    /// Update all settings at once
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<UserSettingsDto>> UpdateSettings([FromBody] UpdateSettingsDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Update fields
        if (!string.IsNullOrEmpty(dto.Language))
            user.Language = dto.Language;

        if (!string.IsNullOrEmpty(dto.DefaultCurrency))
            user.DefaultCurrency = dto.DefaultCurrency;

        if (!string.IsNullOrEmpty(dto.Timezone))
            user.Timezone = dto.Timezone;

        if (!string.IsNullOrEmpty(dto.Theme))
            user.Theme = dto.Theme;

        if (!string.IsNullOrEmpty(dto.PrimaryColor))
            UpdateOrAddClaim(user, "PrimaryColor", dto.PrimaryColor);

        if (!string.IsNullOrEmpty(dto.DateFormat))
            UpdateOrAddClaim(user, "DateFormat", dto.DateFormat);

        if (!string.IsNullOrEmpty(dto.TimeFormat))
            UpdateOrAddClaim(user, "TimeFormat", dto.TimeFormat);

        if (dto.FirstDayOfWeek.HasValue)
            UpdateOrAddClaim(user, "FirstDayOfWeek", dto.FirstDayOfWeek.Value.ToString());

        if (dto.NotificationsEnabled.HasValue)
            UpdateOrAddClaim(user, "NotificationsEnabled", dto.NotificationsEnabled.Value.ToString());

        if (dto.EmailNotifications.HasValue)
            user.EmailNotifications = dto.EmailNotifications.Value;

        if (dto.PushNotifications.HasValue)
            user.PushNotifications = dto.PushNotifications.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new UserSettingsDto
        {
            Language = user.Language,
            DefaultCurrency = user.DefaultCurrency,
            Timezone = user.Timezone,
            Theme = user.Theme,
            PrimaryColor = GetClaimValue(user, "PrimaryColor") ?? "#10b981",
            DateFormat = GetClaimValue(user, "DateFormat") ?? "DD/MM/YYYY",
            TimeFormat = GetClaimValue(user, "TimeFormat") ?? "24h",
            FirstDayOfWeek = int.TryParse(GetClaimValue(user, "FirstDayOfWeek"), out var fd) ? fd : 1,
            NotificationsEnabled = bool.TryParse(GetClaimValue(user, "NotificationsEnabled"), out var ne) ? ne : true,
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications
        });
    }

    /// <summary>
    /// Update language
    /// </summary>
    [HttpPut("language")]
    public async Task<ActionResult> UpdateLanguage([FromBody] UpdateLanguageDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var supportedLanguages = new[] { "en", "vi" };
        if (!supportedLanguages.Contains(dto.Language.ToLower()))
            return BadRequest(new { message = "Unsupported language. Supported: en, vi" });

        user.Language = dto.Language.ToLower();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { language = user.Language, message = "Language updated successfully" });
    }

    /// <summary>
    /// Update default currency
    /// </summary>
    [HttpPut("currency")]
    public async Task<ActionResult> UpdateCurrency([FromBody] UpdateCurrencyDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var supportedCurrencies = new[] { "VND", "USD", "EUR", "GBP", "JPY", "CNY", "KRW" };
        if (!supportedCurrencies.Contains(dto.Currency.ToUpper()))
            return BadRequest(new { message = $"Unsupported currency. Supported: {string.Join(", ", supportedCurrencies)}" });

        user.DefaultCurrency = dto.Currency.ToUpper();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { currency = user.DefaultCurrency, message = "Currency updated successfully" });
    }

    /// <summary>
    /// Update theme
    /// </summary>
    [HttpPut("theme")]
    public async Task<ActionResult> UpdateTheme([FromBody] UpdateThemeDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var supportedThemes = new[] { "light", "dark", "auto" };
        if (!supportedThemes.Contains(dto.Theme.ToLower()))
            return BadRequest(new { message = "Unsupported theme. Supported: light, dark, auto" });

        user.Theme = dto.Theme.ToLower();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { theme = user.Theme, message = "Theme updated successfully" });
    }

    /// <summary>
    /// Update primary color
    /// </summary>
    [HttpPut("primary-color")]
    public async Task<ActionResult> UpdatePrimaryColor([FromBody] UpdatePrimaryColorDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Store primary color in user claims
        UpdateOrAddClaim(user, "PrimaryColor", dto.Color);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { color = dto.Color, message = "Primary color updated successfully" });
    }

    /// <summary>
    /// Update timezone
    /// </summary>
    [HttpPut("timezone")]
    public async Task<ActionResult> UpdateTimezone([FromBody] UpdateTimezoneDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(dto.Timezone);
        }
        catch
        {
            return BadRequest(new { message = "Invalid timezone" });
        }

        user.Timezone = dto.Timezone;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { timezone = user.Timezone, message = "Timezone updated successfully" });
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    [HttpPut("notifications")]
    public async Task<ActionResult> UpdateNotificationPreferences([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        UpdateOrAddClaim(user, "NotificationsEnabled", dto.NotificationsEnabled.ToString());
        user.EmailNotifications = dto.EmailNotifications;
        user.PushNotifications = dto.PushNotifications;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            notificationsEnabled = dto.NotificationsEnabled,
            emailNotifications = user.EmailNotifications,
            pushNotifications = user.PushNotifications,
            message = "Notification preferences updated successfully"
        });
    }

    /// <summary>
    /// Get available options for settings
    /// </summary>
    [HttpGet("options")]
    [AllowAnonymous]
    public ActionResult GetSettingsOptions()
    {
        return Ok(new
        {
            languages = new[]
            {
                new { code = "en", name = "English" },
                new { code = "vi", name = "Tiếng Việt" }
            },
            currencies = new[]
            {
                new { code = "VND", symbol = "₫", name = "Vietnamese Dong" },
                new { code = "USD", symbol = "$", name = "US Dollar" },
                new { code = "EUR", symbol = "€", name = "Euro" },
                new { code = "GBP", symbol = "£", name = "British Pound" },
                new { code = "JPY", symbol = "¥", name = "Japanese Yen" },
                new { code = "CNY", symbol = "¥", name = "Chinese Yuan" },
                new { code = "KRW", symbol = "₩", name = "Korean Won" }
            },
            themes = new[]
            {
                new { code = "light", name = "Light Mode" },
                new { code = "dark", name = "Dark Mode" },
                new { code = "auto", name = "Auto (System)" }
            },
            dateFormats = new[]
            {
                "DD/MM/YYYY",
                "MM/DD/YYYY",
                "YYYY-MM-DD"
            },
            timeFormats = new[]
            {
                "12h",
                "24h"
            },
            timezones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new { id = tz.Id, displayName = tz.DisplayName })
                .ToList()
        });
    }

    /// <summary>
    /// Reset settings to default
    /// </summary>
    [HttpPost("reset")]
    public async Task<ActionResult> ResetToDefaults()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Reset to defaults
        user.Language = "vi";
        user.DefaultCurrency = "VND";
        user.Timezone = "Asia/Ho_Chi_Minh";
        user.Theme = "light";
        user.EmailNotifications = true;
        user.PushNotifications = true;
        
        UpdateOrAddClaim(user, "DateFormat", "DD/MM/YYYY");
        UpdateOrAddClaim(user, "TimeFormat", "24h");
        UpdateOrAddClaim(user, "FirstDayOfWeek", "1");
        UpdateOrAddClaim(user, "NotificationsEnabled", "True");

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Settings reset to defaults successfully" });
    }

    private void UpdateOrAddClaim(User user, string type, string value)
    {
        var claim = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == type);
        if (claim != null)
        {
            claim.ClaimValue = value;
        }
        else
        {
            user.AspNetUserClaims.Add(new AspNetUserClaim
            {
                UserId = user.Id,
                ClaimType = type,
                ClaimValue = value
            });
        }
    }

    private string? GetClaimValue(User user, string type)
    {
        return user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == type)?.ClaimValue;
    }
}
