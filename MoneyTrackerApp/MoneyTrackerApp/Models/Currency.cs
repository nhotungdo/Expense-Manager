using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Currency
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!; // e.g. USD, VND

    public string Symbol { get; set; } = null!; // e.g. $, ₫

    public decimal ExchangeRate { get; set; } // Rate relative to USD (base)

    public string? Country { get; set; }

    public string? FlagUrl { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastUpdated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
