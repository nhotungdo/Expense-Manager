using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Enums;
using MoneyTrackerApp.Models;
using System.Text.Json;

namespace MoneyTrackerApp.Services;

public interface ISubscriptionService
{
    Task<List<ServicePackageDto>> GetAllPackagesAsync();
    Task<ServicePackageDto?> GetPackageByIdAsync(int packageId);
    Task<SubscriptionDto?> GetActiveSubscriptionAsync(long userId);
    Task<PaymentResponseDto> CreateSubscriptionAsync(long userId, CreateSubscriptionDto dto);
    Task<bool> ProcessPaymentCallbackAsync(string transactionId, bool success, string? paymentData);
    Task<bool> CancelSubscriptionAsync(long userId, string? reason);
    Task<List<PaymentDto>> GetPaymentHistoryAsync(long userId);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IConfiguration _configuration;

    public SubscriptionService(
        ExpenseManagerContext context,
        ILogger<SubscriptionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<ServicePackageDto>> GetAllPackagesAsync()
    {
        var packages = await _context.ServicePackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        return packages.Select(p => new ServicePackageDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            PackageType = p.PackageType,
            PackageTypeName = ((PackageType)p.PackageType).ToString(),
            Price = p.Price,
            BillingCycle = p.BillingCycle,
            BillingCycleName = GetBillingCycleName(p.BillingCycle),
            Features = string.IsNullOrEmpty(p.Features) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(p.Features) ?? new List<string>(),
            MaxTransactions = p.MaxTransactions,
            MaxAccounts = p.MaxAccounts,
            MaxBudgets = p.MaxBudgets,
            HasAdvancedReports = p.HasAdvancedReports,
            HasAiAdvisor = p.HasAiAdvisor,
            HasGroupExpense = p.HasGroupExpense,
            HasPrioritySupport = p.HasPrioritySupport,
            IsPopular = p.PackageType == (int)PackageType.Pro
        }).ToList();
    }

    public async Task<ServicePackageDto?> GetPackageByIdAsync(int packageId)
    {
        var package = await _context.ServicePackages
            .FirstOrDefaultAsync(p => p.Id == packageId && p.IsActive);

        if (package == null) return null;

        return new ServicePackageDto
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            PackageType = package.PackageType,
            PackageTypeName = ((PackageType)package.PackageType).ToString(),
            Price = package.Price,
            BillingCycle = package.BillingCycle,
            BillingCycleName = GetBillingCycleName(package.BillingCycle),
            Features = string.IsNullOrEmpty(package.Features)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(package.Features) ?? new List<string>(),
            MaxTransactions = package.MaxTransactions,
            MaxAccounts = package.MaxAccounts,
            MaxBudgets = package.MaxBudgets,
            HasAdvancedReports = package.HasAdvancedReports,
            HasAiAdvisor = package.HasAiAdvisor,
            HasGroupExpense = package.HasGroupExpense,
            HasPrioritySupport = package.HasPrioritySupport,
            IsPopular = package.PackageType == (int)PackageType.Pro
        };
    }

    public async Task<SubscriptionDto?> GetActiveSubscriptionAsync(long userId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Package)
            .Where(s => s.UserId == userId && s.Status == (int)SubscriptionStatus.Active)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (subscription == null) return null;

        var daysRemaining = (subscription.EndDate - DateTime.UtcNow).Days;

        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            PackageId = subscription.PackageId,
            PackageName = subscription.Package.Name,
            Status = subscription.Status,
            StatusName = ((SubscriptionStatus)subscription.Status).ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            DaysRemaining = daysRemaining > 0 ? daysRemaining : 0,
            AutoRenew = subscription.AutoRenew,
            CreatedAt = subscription.CreatedAt
        };
    }

    public async Task<PaymentResponseDto> CreateSubscriptionAsync(long userId, CreateSubscriptionDto dto)
    {
        var package = await _context.ServicePackages
            .FirstOrDefaultAsync(p => p.Id == dto.PackageId && p.IsActive);

        if (package == null)
            throw new Exception("Gói dịch vụ không tồn tại");

        // Check if user already has an active subscription
        var existingSubscription = await _context.Subscriptions
            .Where(s => s.UserId == userId && s.Status == (int)SubscriptionStatus.Active)
            .FirstOrDefaultAsync();

        if (existingSubscription != null)
            throw new Exception("Bạn đã có gói dịch vụ đang hoạt động");

        // Create subscription
        var subscription = new Subscription
        {
            UserId = userId,
            PackageId = package.Id,
            Status = (int)SubscriptionStatus.Pending,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(package.BillingCycle),
            AutoRenew = dto.AutoRenew,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Create payment
        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = package.Price,
            Currency = "VND",
            Status = (int)PaymentStatus.Pending,
            PaymentMethod = "VNPay",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Generate VNPay payment URL (simplified - you'll need to implement actual VNPay integration)
        var paymentUrl = GenerateVNPayUrl(payment.Id, package.Price, dto.ReturnUrl);
        var qrCodeUrl = GenerateQRCode(paymentUrl);

        return new PaymentResponseDto
        {
            PaymentId = payment.Id,
            SubscriptionId = subscription.Id,
            PaymentUrl = paymentUrl,
            QrCodeUrl = qrCodeUrl,
            Amount = package.Price,
            Currency = "VND"
        };
    }

    public async Task<bool> ProcessPaymentCallbackAsync(string transactionId, bool success, string? paymentData)
    {
        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

        if (payment == null) return false;

        if (success)
        {
            payment.Status = (int)PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PaymentData = paymentData;

            // Activate subscription
            var subscription = payment.Subscription;
            subscription.Status = (int)SubscriptionStatus.Active;
            subscription.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.FailureReason = paymentData;

            // Cancel subscription
            var subscription = payment.Subscription;
            subscription.Status = (int)SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.CancellationReason = "Payment failed";
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(long userId, string? reason)
    {
        var subscription = await _context.Subscriptions
            .Where(s => s.UserId == userId && s.Status == (int)SubscriptionStatus.Active)
            .FirstOrDefaultAsync();

        if (subscription == null) return false;

        subscription.Status = (int)SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PaymentDto>> GetPaymentHistoryAsync(long userId)
    {
        var payments = await _context.Payments
            .Include(p => p.Subscription)
            .Where(p => p.Subscription.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            SubscriptionId = p.SubscriptionId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            StatusName = ((PaymentStatus)p.Status).ToString(),
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    private string GetBillingCycleName(int billingCycle)
    {
        return billingCycle switch
        {
            1 => "Tháng",
            3 => "Quý",
            12 => "Năm",
            _ => "Tháng"
        };
    }

    private string GenerateVNPayUrl(long paymentId, decimal amount, string? returnUrl)
    {
        // This is a simplified version. You'll need to implement actual VNPay integration
        // with proper hashing, signing, and parameter encoding
        var vnpUrl = _configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var vnpTmnCode = _configuration["VNPay:TmnCode"] ?? "DEMO";
        var vnpHashSecret = _configuration["VNPay:HashSecret"] ?? "SECRET";
        
        returnUrl = returnUrl ?? "/Subscription/PaymentCallback";
        
        var queryString = $"?vnp_TmnCode={vnpTmnCode}" +
            $"&vnp_Amount={amount * 100}" + // VNPay uses smallest currency unit
            $"&vnp_TxnRef={paymentId}" +
            $"&vnp_OrderInfo=Thanh toan goi dich vu {paymentId}" +
            $"&vnp_ReturnUrl={returnUrl}";

        return vnpUrl + queryString;
    }

    private string GenerateQRCode(string paymentUrl)
    {
        // Generate QR code URL using a QR code API service
        return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(paymentUrl)}";
    }
}
