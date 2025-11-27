using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating an investment
/// </summary>
public class CreateInvestmentDto
{
    public long? AccountId { get; set; }

    [Required(ErrorMessage = "Investment name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Investment name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Asset type is required")]
    [StringLength(50, ErrorMessage = "Asset type must be less than 50 characters")]
    public string AssetType { get; set; } = null!;

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Purchase price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Purchase price must be greater than 0")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "Purchase date is required")]
    public DateOnly PurchaseDate { get; set; }

    public decimal? CurrentValue { get; set; }
}

/// <summary>
/// DTO for updating an investment
/// </summary>
public class UpdateInvestmentDto
{
    [Required(ErrorMessage = "Investment ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Investment name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Current value must be greater than 0")]
    public decimal? CurrentValue { get; set; }
}

/// <summary>
/// DTO for updating investment market price
/// </summary>
public class UpdateInvestmentPriceDto
{
    [Required(ErrorMessage = "Investment ID is required")]
    public long Id { get; set; }

    [Required(ErrorMessage = "Current value is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Current value must be greater than 0")]
    public decimal CurrentValue { get; set; }
}

/// <summary>
/// DTO for investment response
/// </summary>
public class InvestmentResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string Name { get; set; } = null!;
    public string AssetType { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal TotalInvested { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? CurrentMarketPrice { get; set; }
    public decimal? TotalCurrentValue { get; set; }
    public decimal? ProfitLoss { get; set; }
    public decimal? ProfitLossPercentage { get; set; }
    public bool IsProfit { get; set; }
    public DateTime? LastUpdated { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for investment portfolio summary
/// </summary>
public class InvestmentPortfolioDto
{
    public int TotalInvestments { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal TotalProfitLossPercentage { get; set; }
    public bool IsOverallProfit { get; set; }
    public List<InvestmentByAssetTypeDto> ByAssetType { get; set; } = new();
    public List<InvestmentResponseDto> Investments { get; set; } = new();
}

/// <summary>
/// DTO for investment breakdown by asset type
/// </summary>
public class InvestmentByAssetTypeDto
{
    public string AssetType { get; set; } = null!;
    public int Count { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercentage { get; set; }
    public decimal PortfolioPercentage { get; set; }
}
