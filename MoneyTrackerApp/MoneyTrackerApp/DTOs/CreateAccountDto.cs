using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a new wallet/account
/// </summary>
public class CreateAccountDto
{
    [Required(ErrorMessage = "Wallet name is required")]
    [StringLength(100, MinimumLength = 2, 
        ErrorMessage = "Wallet name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Account type is required")]
    [Range(0, 5, ErrorMessage = "Invalid account type")]
    public int AccountType { get; set; }

    [Required(ErrorMessage = "Initial balance is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative")]
    public decimal InitialBalance { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string Currency { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }

    [Required(ErrorMessage = "Include in total is required")]
    public bool IncludeInTotal { get; set; } = true;
}
