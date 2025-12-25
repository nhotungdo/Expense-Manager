using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class FinancialHealthLog
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int Score { get; set; } // 0-1000

    public decimal SavingsIncomeRatio { get; set; }

    public decimal DebtAssetRatio { get; set; }

    public decimal BudgetCompliance { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
