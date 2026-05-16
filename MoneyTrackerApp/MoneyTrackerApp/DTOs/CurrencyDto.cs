using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

public class CurrencyResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public string? Country { get; set; }
    public string? FlagUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string TimeAgo { get; set; } = null!;
}

public class CreateCurrencyDto
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Code { get; set; } = null!;
    [Required]
    public string Symbol { get; set; } = null!;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public string? Country { get; set; }
    public string? FlagUrl { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCurrencyDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? Country { get; set; }
    public string? FlagUrl { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

public class CurrencyConversionRequestDto
{
    public string FromCode { get; set; } = null!;
    public string ToCode { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class CurrencyConversionResponseDto
{
    public string FromCode { get; set; } = null!;
    public string ToCode { get; set; } = null!;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
}
