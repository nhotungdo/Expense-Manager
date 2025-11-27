using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for manually adjusting account balance
/// </summary>
public class AdjustAccountBalanceDto
{
    [Required(ErrorMessage = "Account ID is required")]
    public long AccountId { get; set; }

    [Required(ErrorMessage = "Adjustment amount is required")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Adjustment reason is required")]
    [StringLength(500, MinimumLength = 5,
        ErrorMessage = "Reason must be between 5 and 500 characters")]
    public string Reason { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Notes must be less than 1000 characters")]
    public string? Notes { get; set; }
}
