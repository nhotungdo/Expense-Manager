using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Asset
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal InitialValue { get; set; }

    public decimal CurrentValue { get; set; }

    public DateTime PurchaseDate { get; set; }

    public int UsefulLifeMonths { get; set; } // For depreciation

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
