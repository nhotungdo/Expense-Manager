using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class VwUserTransactionSummary
{
    public long UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public int? TotalTransactions { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetIncome { get; set; }

    public DateTime? LastTransactionDate { get; set; }
}
