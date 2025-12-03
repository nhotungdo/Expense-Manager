using System;

namespace MoneyTrackerApp.Models;

/// <summary>
/// Payment transaction model
/// </summary>
public partial class Payment
{
    public long Id { get; set; }

    public long SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public int Status { get; set; } // 0 = Pending, 1 = Processing, 2 = Completed, 3 = Failed, 4 = Refunded

    public string PaymentMethod { get; set; } = null!; // VNPay, Momo, etc.

    public string? TransactionId { get; set; } // Payment gateway transaction ID

    public string? PaymentData { get; set; } // JSON data from payment gateway

    public DateTime? PaidAt { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;
}
