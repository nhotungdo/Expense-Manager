using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

// ===================================
// PROFILE DTOs
// ===================================

public class UserProfileDto
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string DefaultCurrency { get; set; } = "VND";
    public string Language { get; set; } = "vi";
    public string Theme { get; set; } = "light";
    public DateTime? CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; } // Male, Female, Other

    [StringLength(500)]
    public string? Address { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = null!;
}

// ===================================
// SECURITY DTOs
// ===================================

public class Enable2FADto
{
    [Required]
    public string Method { get; set; } = "totp"; // totp or sms
}

public class Verify2FADto
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = null!;
}

public class Disable2FADto
{
    [Required]
    public string Password { get; set; } = null!;
}

// ===================================
// SETTINGS DTOs
// ===================================

public class UserSettingsDto
{
    public string Language { get; set; } = "vi";
    public string DefaultCurrency { get; set; } = "VND";
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string Theme { get; set; } = "light";
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public string TimeFormat { get; set; } = "24h";
    public int FirstDayOfWeek { get; set; } = 1; // 0=Sunday, 1=Monday
    public bool NotificationsEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
}

public class UpdateSettingsDto
{
    [StringLength(5)]
    public string? Language { get; set; }

    [StringLength(3)]
    public string? DefaultCurrency { get; set; }

    [StringLength(100)]
    public string? Timezone { get; set; }

    [StringLength(10)]
    public string? Theme { get; set; }

    [StringLength(20)]
    public string? DateFormat { get; set; }

    [StringLength(5)]
    public string? TimeFormat { get; set; }

    [Range(0, 6)]
    public int? FirstDayOfWeek { get; set; }

    public bool? NotificationsEnabled { get; set; }
    public bool? EmailNotifications { get; set; }
    public bool? PushNotifications { get; set; }
}

public class UpdateLanguageDto
{
    [Required]
    [StringLength(5)]
    public string Language { get; set; } = null!; // en, vi
}

public class UpdateCurrencyDto
{
    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = null!; // VND, USD, EUR, etc.
}

public class UpdateThemeDto
{
    [Required]
    [StringLength(10)]
    public string Theme { get; set; } = null!; // light, dark, auto
}

public class UpdateTimezoneDto
{
    [Required]
    [StringLength(100)]
    public string Timezone { get; set; } = null!;
}

public class UpdateNotificationPreferencesDto
{
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
}

// ===================================
// EXPORT DTOs
// ===================================

public class ExportRequestDto
{
    [Required]
    public string Format { get; set; } = "excel"; // excel, pdf, csv

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? ReportType { get; set; } // transactions, cashflow, categories, etc.

    public List<long>? AccountIds { get; set; }
    public List<long>? CategoryIds { get; set; }
}

public class ExportResponseDto
{
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public DateTime GeneratedAt { get; set; }
}
