using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class CurrencyRate
{
    public int Id { get; set; }

    public string FromCurrency { get; set; } = null!;

    public string ToCurrency { get; set; } = null!;

    public decimal Rate { get; set; }

    public DateTime LastUpdated { get; set; }
}
