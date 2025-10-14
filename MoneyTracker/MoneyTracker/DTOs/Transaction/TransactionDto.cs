using System.ComponentModel.DataAnnotations;
using MoneyTracker.Models;

namespace MoneyTracker.DTOs.Transaction;

public class TransactionDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CategoryId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CategoryDto? Category { get; set; }
}

public class CreateTransactionRequest
{
    [Required]
    public long? CategoryId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }
}

public class UpdateTransactionRequest
{
    [Required]
    public long? CategoryId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }
}

public class TransactionFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TransactionType? Type { get; set; }
    public long? CategoryId { get; set; }
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
