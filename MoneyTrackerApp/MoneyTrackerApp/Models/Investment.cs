using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Investment
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string AssetType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateOnly PurchaseDate { get; set; }

    public decimal? CurrentValue { get; set; }

    public DateTime? LastUpdated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? Account { get; set; }

    public virtual User User { get; set; } = null!;
}
