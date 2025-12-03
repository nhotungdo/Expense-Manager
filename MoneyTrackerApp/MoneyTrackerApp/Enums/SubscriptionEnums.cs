namespace MoneyTrackerApp.Enums
{
    /// <summary>
    /// Service package types
    /// </summary>
    public enum PackageType
    {
        Free = 0,
        Pro = 1,
        Team = 2
    }

    /// <summary>
    /// Subscription status
    /// </summary>
    public enum SubscriptionStatus
    {
        Pending = 0,      // Waiting for payment
        Active = 1,       // Active subscription
        Expired = 2,      // Expired
        Cancelled = 3,    // Cancelled by user
        Suspended = 4     // Suspended by admin
    }

    /// <summary>
    /// Payment status
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 0,      // Waiting for payment
        Processing = 1,   // Processing payment
        Completed = 2,    // Payment successful
        Failed = 3,       // Payment failed
        Refunded = 4      // Refunded
    }

    /// <summary>
    /// Billing cycle
    /// </summary>
    public enum BillingCycle
    {
        Monthly = 1,
        Quarterly = 3,
        Yearly = 12
    }
}
