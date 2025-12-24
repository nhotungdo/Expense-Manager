using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for updating group transaction
/// </summary>
public class UpdateGroupTransactionDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    public long GroupId { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public long PaidByUserId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? AttachmentUrl { get; set; }
    
    public string? Category { get; set; }

    public List<SplitDetailDto> Splits { get; set; } = new();
}
