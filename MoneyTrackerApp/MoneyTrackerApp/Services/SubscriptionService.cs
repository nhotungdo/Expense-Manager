using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Enums;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Helpers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MoneyTrackerApp.Services;

public interface ISubscriptionService
{
    Task<List<ServicePackageDto>> GetAllPackagesAsync();
    Task<ServicePackageDto?> GetPackageByIdAsync(int packageId);
    Task<SubscriptionDto?> GetActiveSubscriptionAsync(long userId);
    Task<PaymentResponseDto> CreateSubscriptionAsync(long userId, CreateSubscriptionDto dto);
    Task<bool> ProcessPaymentCallbackAsync(string transactionId, bool success, string? paymentData);
    Task<PaymentResultDto> ProcessVnPayPaymentReturn(IQueryCollection collections);
    Task<bool> CancelSubscriptionAsync(long userId, string? reason);
    Task<List<PaymentDto>> GetPaymentHistoryAsync(long userId);
    Task<PaymentDto?> GetPaymentStatusAsync(long paymentId);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubscriptionService(
        ExpenseManagerContext context,
        ILogger<SubscriptionService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
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

        // Generate VNPay payment URL
        var paymentUrl = GenerateVNPayUrl(payment.Id, package.Price, dto.ReturnUrl);
        var qrCodeUrl = GenerateQRCode(payment.Id, package.Price);

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
        // This method might be deprecated or used for other payment methods
        // For VNPay, use ProcessVnPayPaymentReturn
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

    public async Task<PaymentResultDto> ProcessVnPayPaymentReturn(IQueryCollection collections)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        var vnp_ResponseCode = collections.FirstOrDefault(p => p.Key == "vnp_ResponseCode").Value;
        var vnp_OrderInfo = collections.FirstOrDefault(p => p.Key == "vnp_OrderInfo").Value;
        long vnp_TxnRef = Convert.ToInt64(collections.FirstOrDefault(p => p.Key == "vnp_TxnRef").Value);
        string vnp_TransactionNo = collections.FirstOrDefault(p => p.Key == "vnp_TransactionNo").Value;
        string vnp_HashSecret = _configuration["Vnpay:HashSecret"];

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

        if (!checkSignature)
        {
            return new PaymentResultDto
            {
                Success = false,
                Message = "Invalid signature"
            };
        }

        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == vnp_TxnRef);

        if (payment == null)
        {
             return new PaymentResultDto
            {
                Success = false,
                Message = "Payment not found"
            };
        }
        
        if (payment.Status == (int)PaymentStatus.Completed)
        {
             return new PaymentResultDto
            {
                Success = true,
                Message = "Payment already completed",
                PaymentId = payment.Id,
                TransactionId = vnp_TransactionNo
            };
        }

        if (vnp_ResponseCode == "00")
        {
            payment.Status = (int)PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.TransactionId = vnp_TransactionNo;
            payment.PaymentData = JsonSerializer.Serialize(collections.ToDictionary(k => k.Key, v => v.Value.ToString()));

            // Activate subscription
            var subscription = payment.Subscription;
            subscription.Status = (int)SubscriptionStatus.Active;
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return new PaymentResultDto
            {
                Success = true,
                Message = "Payment successful",
                PaymentId = payment.Id,
                TransactionId = vnp_TransactionNo
            };
        }
        else
        {
            payment.Status = (int)PaymentStatus.Failed;
            payment.FailureReason = $"VNPay Error: {vnp_ResponseCode}";
            payment.TransactionId = vnp_TransactionNo;
             payment.PaymentData = JsonSerializer.Serialize(collections.ToDictionary(k => k.Key, v => v.Value.ToString()));

            // Cancel subscription
            var subscription = payment.Subscription;
            subscription.Status = (int)SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.CancellationReason = "Payment failed";
            
            await _context.SaveChangesAsync();

            return new PaymentResultDto
            {
                Success = false,
                Message = $"Payment failed with code {vnp_ResponseCode}",
                PaymentId = payment.Id,
                TransactionId = vnp_TransactionNo
            };
        }
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

    public async Task<PaymentDto?> GetPaymentStatusAsync(long paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            StatusName = ((PaymentStatus)payment.Status).ToString(),
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
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
        string vnp_Returnurl = returnUrl ?? _configuration["Vnpay:PaymentBackReturnUrl"];
        string vnp_Url = _configuration["Vnpay:BaseUrl"];
        string vnp_TmnCode = _configuration["Vnpay:TmnCode"];
        string vnp_HashSecret = _configuration["Vnpay:HashSecret"];
        
        if (string.IsNullOrEmpty(vnp_Returnurl) || string.IsNullOrEmpty(vnp_Url) || string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
        {
             // Fallback to defaults if config is missing (for safety)
             vnp_Returnurl = vnp_Returnurl ?? "http://localhost:5000/Subscription/PaymentCallback";
             vnp_Url = vnp_Url ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        }

        VnPayLibrary vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); 
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + paymentId);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
        vnpay.AddRequestData("vnp_TxnRef", paymentId.ToString()); 

        string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        return paymentUrl;
    }

    private string GenerateQRCode(long paymentId, decimal amount)
    {
        // Get Bank Transfer config
        var bankBin = _configuration["BankTransfer:Bin"];
        var accountNumber = _configuration["BankTransfer:AccountNumber"];

        if (!string.IsNullOrEmpty(bankBin) && !string.IsNullOrEmpty(accountNumber))
        {
            // Generate VietQR (EMVCo) string
            var content = $"MTA {paymentId}";
            var emvString = EmvQrLibrary.GenerateVietQr(bankBin, accountNumber, ((long)amount).ToString(), content);
            
            // Return QR code image URL encoding the EMV string
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(emvString)}";
        }

        // Fallback to VNPay URL if Bank config is missing
        var paymentUrl = GenerateVNPayUrl(paymentId, amount, null);
        return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(paymentUrl)}";
    }
}
