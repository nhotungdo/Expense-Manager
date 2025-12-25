namespace MoneyTrackerApp.DTOs;



/// <summary>
/// DTO for subscription
/// </summary>
public class SubscriptionDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool HasAdvancedReports { get; set; }
    public bool HasAiAdvisor { get; set; }
    public bool HasGroupExpense { get; set; }
    public int MaxAccounts { get; set; }
}

/// <summary>
/// DTO for creating subscription
/// </summary>
public class CreateSubscriptionDto
{
    public int PackageId { get; set; }
    public bool AutoRenew { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// DTO for payment
/// </summary>
public class PaymentDto
{
    public long Id { get; set; }
    public long SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// DTO for payment response
/// </summary>
public class PaymentResponseDto
{
    public long PaymentId { get; set; }
    public long SubscriptionId { get; set; }
    public string PaymentUrl { get; set; } = null!;
    public string QrCodeUrl { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public long? PaymentId { get; set; }
}
