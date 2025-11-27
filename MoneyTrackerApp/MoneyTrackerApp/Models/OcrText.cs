using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class OcrText
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string RawText { get; set; } = null!;

    public string? MerchantName { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
