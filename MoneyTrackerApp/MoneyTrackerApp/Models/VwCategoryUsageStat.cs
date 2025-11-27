using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class VwCategoryUsageStat
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int CategoryType { get; set; }

    public string? CategoryIcon { get; set; }

    public string? CategoryColor { get; set; }

    public int? UsageCount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AverageAmount { get; set; }

    public DateTime? LastUsedDate { get; set; }
}
