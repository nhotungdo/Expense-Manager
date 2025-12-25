using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Net;
using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Enums;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly VnPayService _vnPayService;
    private readonly IServicePackageService _packageService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ExpenseManagerContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public PaymentController(
        VnPayService vnPayService,
        IServicePackageService packageService,
        ISubscriptionService subscriptionService,
        ExpenseManagerContext context,
        IEmailService emailService,
        ILogger<PaymentController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _vnPayService = vnPayService;
        _packageService = packageService;
        _subscriptionService = subscriptionService;
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("vnpay/qr")]
    public async Task<IActionResult> CreateQrPayment([FromBody] CreateQrPaymentRequest request)
    {
        if (request is null || request.PackageId <= 0)
        {
            return BadRequest(new { message = "Thiếu thông tin gói dịch vụ" });
        }

        var package = await _packageService.GetPackageByIdAsync(request.PackageId);
        if (package is null)
        {
            return NotFound(new { message = "Gói dịch vụ không tồn tại" });
        }

        var userId = ResolveUserId(request.UserId);
        
        try
        {
            var vietQr = _configuration.GetSection("VietQR");
            var bankId = vietQr["BankId"] ?? "BIDV";
            var bankName = vietQr["BankName"] ?? "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam";
            var accountNo = vietQr["AccountNo"] ?? "8827256654";
            var template = vietQr["Template"] ?? "compact";
            var accountName = vietQr["AccountName"] ?? "DO NHO TUNG";

            var amount = package.Price;
            var description = $"Thanh toan goi {package.Name}";
            
            // Construct VietQR URL
            // Format: https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-<TEMPLATE>.png?amount=<AMOUNT>&addInfo=<CONTENT>&accountName=<NAME>
            var paymentUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={WebUtility.UrlEncode(description)}&accountName={WebUtility.UrlEncode(accountName)}";

            return Ok(new
            {
                success = true,
                paymentUrl,
                package = new { package.Id, package.Name, package.Price },
                bankInfo = new 
                {
                    BankId = bankId,
                    BankName = bankName,
                    AccountNo = accountNo,
                    AccountName = accountName,
                    Amount = amount,
                    Template = template,
                    Description = description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create VietQR URL");
            return StatusCode(500, new { message = "Không thể tạo mã QR thanh toán" });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        if (request is null || request.PackageId <= 0)
        {
            return BadRequest(new { message = "Thiếu thông tin gói dịch vụ" });
        }

        var userId = ResolveUserId(request.UserId);
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập để tiếp tục" });
        }

        try
        {
            var package = await _packageService.GetPackageByIdAsync(request.PackageId);
            if (package is null)
            {
                return NotFound(new { message = "Gói dịch vụ không tồn tại" });
            }

            // Check if user already has an active subscription
            var existingSubscription = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == (int)SubscriptionStatus.Active)
                .FirstOrDefaultAsync();

            if (existingSubscription != null)
            {
                return BadRequest(new { message = "Bạn đã có gói dịch vụ đang hoạt động" });
            }

            // Create subscription using the subscription service
            var createDto = new MoneyTrackerApp.DTOs.CreateSubscriptionDto
            {
                PackageId = request.PackageId,
                AutoRenew = false,
                ReturnUrl = "/Subscription"
            };

            var paymentResponse = await _subscriptionService.CreateSubscriptionAsync(userId, createDto);

            // Activate the subscription and payment directly
            var payment = await _context.Payments
                .Include(p => p.Subscription)
                .FirstOrDefaultAsync(p => p.Id == paymentResponse.PaymentId);

            if (payment != null)
            {
                // Update payment status
                payment.Status = (int)PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = $"QR_{paymentResponse.PaymentId}_{DateTime.UtcNow.Ticks}";
                payment.PaymentData = $"QR Payment confirmed by user for package {package.Name}";
                payment.UpdatedAt = DateTime.UtcNow;

                // Activate subscription
                var subscription = payment.Subscription;
                if (subscription != null)
                {
                    subscription.Status = (int)SubscriptionStatus.Active;
                    subscription.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            // Get the now-active subscription with features
            var activeSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Send success notification and email
            if (activeSubscription != null)
            {
                var features = new List<string>();
                if (activeSubscription.HasAdvancedReports) features.Add("Báo cáo nâng cao");
                if (activeSubscription.HasAiAdvisor) features.Add("Tư vấn AI");
                if (activeSubscription.HasGroupExpense) features.Add("Quản lý chi tiêu nhóm");
                features.Add($"Tối đa {activeSubscription.MaxAccounts} tài khoản");

                await SendSubscriptionSuccessNotification(
                    userId,
                    package.Name,
                    activeSubscription.StartDate,
                    activeSubscription.EndDate,
                    features
                );
            }

            return Ok(new
            {
                success = true,
                message = "Đã kích hoạt gói dịch vụ thành công",
                subscription = new
                {
                    packageName = package.Name,
                    features = new
                    {
                        hasAdvancedReports = activeSubscription?.HasAdvancedReports ?? false,
                        hasAiAdvisor = activeSubscription?.HasAiAdvisor ?? false,
                        hasGroupExpense = activeSubscription?.HasGroupExpense ?? false,
                        maxAccounts = activeSubscription?.MaxAccounts ?? 3
                    },
                    startDate = activeSubscription?.StartDate,
                    endDate = activeSubscription?.EndDate
                },
                redirectUrl = "/Subscription"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment for user {UserId}", userId);
            
            // Send failure notification
            var packageName = "Unknown";
            try
            {
                var pkg = await _packageService.GetPackageByIdAsync(request.PackageId);
                packageName = pkg?.Name ?? packageName;
            }
            catch { }

            await SendSubscriptionFailureNotification(userId, packageName, ex.Message);
            
            return StatusCode(500, new { message = ex.Message });
        }
    }


    private long ResolveUserId(long? userId)
    {
        if (userId.HasValue && userId.Value > 0) return userId.Value;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(claim, out var parsed) ? parsed : 0;
    }

    private async Task SendSubscriptionSuccessNotification(long userId, string packageName, DateTime startDate, DateTime endDate, List<string> features)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Create in-app notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Gói dịch vụ đã được kích hoạt!",
                Message = $"Gói {packageName} của bạn đã được kích hoạt thành công. Bạn có thể sử dụng tất cả các tính năng đến {endDate:dd/MM/yyyy}.",
                Type = "success",
                IsRead = false,
                IsImportant = true,
                ActionUrl = "/Subscription",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email
            if (!string.IsNullOrEmpty(user.Email))
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "Email", "SubscriptionSuccess.html");
                var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                var featuresHtml = string.Join("", features.Select(f => $"<li>{f}</li>"));

                emailBody = emailBody
                    .Replace("{{UserName}}", user.FullName ?? user.Email)
                    .Replace("{{PackageName}}", packageName)
                    .Replace("{{PackagePrice}}", "Đã thanh toán")
                    .Replace("{{StartDate}}", startDate.ToString("dd/MM/yyyy HH:mm"))
                    .Replace("{{EndDate}}", endDate.ToString("dd/MM/yyyy HH:mm"))
                    .Replace("{{Features}}", featuresHtml)
                    .Replace("{{DashboardLink}}", $"{Request.Scheme}://{Request.Host}/Subscription");

                await _emailService.SendEmailAsync(user.Email, "Gói dịch vụ đã được kích hoạt", emailBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription success notification for user {UserId}", userId);
        }
    }

    private async Task SendSubscriptionFailureNotification(long userId, string packageName, string errorMessage)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Create in-app notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Kích hoạt gói dịch vụ thất bại",
                Message = $"Không thể kích hoạt gói {packageName}. Lý do: {errorMessage}. Vui lòng thử lại.",
                Type = "error",
                IsRead = false,
                IsImportant = true,
                ActionUrl = "/Subscription/Checkout",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email
            if (!string.IsNullOrEmpty(user.Email))
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "Email", "SubscriptionFailed.html");
                var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                emailBody = emailBody
                    .Replace("{{UserName}}", user.FullName ?? user.Email)
                    .Replace("{{PackageName}}", packageName)
                    .Replace("{{ErrorMessage}}", errorMessage)
                    .Replace("{{RetryLink}}", $"{Request.Scheme}://{Request.Host}/Subscription/Checkout");

                await _emailService.SendEmailAsync(user.Email, "Kích hoạt gói dịch vụ thất bại", emailBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription failure notification for user {UserId}", userId);
        }
    }
}

public class CreateQrPaymentRequest
{
    public long? UserId { get; set; }
    public int PackageId { get; set; }
}

public class ConfirmPaymentRequest
{
    public long? UserId { get; set; }
    public int PackageId { get; set; }
}



