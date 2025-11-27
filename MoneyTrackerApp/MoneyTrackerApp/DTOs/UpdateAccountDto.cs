using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for updating an existing wallet/account
/// </summary>
public class UpdateAccountDto
{
    [Required(ErrorMessage = "Account ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, 
        ErrorMessage = "Wallet name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
    public decimal? CurrentBalance { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code")]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }

    public bool? IncludeInTotal { get; set; }

    [StringLength(500, ErrorMessage = "Notes must be less than 500 characters")]
    public string? AdjustmentNotes { get; set; }
}
