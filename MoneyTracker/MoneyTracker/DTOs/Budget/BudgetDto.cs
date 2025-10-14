using System.ComponentModel.DataAnnotations;
using MoneyTracker.Models;

namespace MoneyTracker.DTOs.Budget;

public class BudgetDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CategoryDto? Category { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => Amount - SpentAmount;
    public decimal UtilizationRate => Amount > 0 ? (SpentAmount / Amount) * 100 : 0;
}

public class CreateBudgetRequest
{
    [Required]
    public long? CategoryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public BudgetPeriod Period { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class UpdateBudgetRequest
{
    [Required]
    public long? CategoryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public BudgetPeriod Period { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsDefault { get; set; }
}
