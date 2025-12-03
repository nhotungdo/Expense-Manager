using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

/// <summary>
/// Service package model
/// </summary>
public partial class ServicePackage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int PackageType { get; set; }

    public decimal Price { get; set; }

    public int BillingCycle { get; set; } // 1 = Monthly, 3 = Quarterly, 12 = Yearly

    public string? Features { get; set; } // JSON array of features

    public int MaxTransactions { get; set; }

    public int MaxAccounts { get; set; }

    public int MaxBudgets { get; set; }

    public bool HasAdvancedReports { get; set; }

    public bool HasAiAdvisor { get; set; }

    public bool HasGroupExpense { get; set; }

    public bool HasPrioritySupport { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
