using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available service packages
    /// </summary>
    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages()
    {
        try
        {
            var packages = await _subscriptionService.GetAllPackagesAsync();
            return Ok(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting packages: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi lấy danh sách gói dịch vụ" });
        }
    }

    /// <summary>
    /// Get package details by ID
    /// </summary>
    [HttpGet("packages/{packageId}")]
    public async Task<IActionResult> GetPackageById(int packageId)
    {
        try
        {
            var package = await _subscriptionService.GetPackageByIdAsync(packageId);
            if (package == null)
                return NotFound(new { message = "Không tìm thấy gói dịch vụ" });

            return Ok(package);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting package {packageId}: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi lấy thông tin gói dịch vụ" });
        }
    }

    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    [HttpGet("my-subscription")]
    [Authorize]
    public async Task<IActionResult> GetMySubscription()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
            }

            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            if (subscription == null)
                return NotFound(new { message = "Không có gói dịch vụ đang hoạt động" });

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting subscription: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi lấy thông tin gói dịch vụ" });
        }
    }

    /// <summary>
    /// Create a new subscription (requires authentication)
    /// </summary>
    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
            }

            var paymentResponse = await _subscriptionService.CreateSubscriptionAsync(userId, dto);
            return Ok(paymentResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating subscription: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel current subscription
    /// </summary>
    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
            }

            var result = await _subscriptionService.CancelSubscriptionAsync(userId, dto.Reason);
            if (!result)
                return NotFound(new { message = "Không tìm thấy gói dịch vụ để hủy" });

            return Ok(new { message = "Đã hủy gói dịch vụ thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cancelling subscription: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi hủy gói dịch vụ" });
        }
    }

    /// <summary>
    /// Get payment history
    /// </summary>
    [HttpGet("payments")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
            }

            var payments = await _subscriptionService.GetPaymentHistoryAsync(userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting payment history: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi lấy lịch sử thanh toán" });
        }
    }

    /// <summary>
    /// VNPay payment callback
    /// </summary>
    /// <summary>
    /// VNPay payment callback
    /// </summary>
    [HttpGet("payment-callback")]
    public async Task<IActionResult> PaymentCallback()
    {
        try
        {
            var result = await _subscriptionService.ProcessVnPayPaymentReturn(Request.Query);

            if (result.Success)
            {
                return Redirect("/Subscription/Success");
            }
            else
            {
                return Redirect($"/Subscription/Failed?message={Uri.EscapeDataString(result.Message)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing payment callback: {ex.Message}");
            return Redirect("/Subscription/Failed");
        }
    }

    /// <summary>
    /// Check payment status
    /// </summary>
    [HttpGet("check-status/{paymentId}")]
    public async Task<IActionResult> CheckStatus(long paymentId)
    {
        try
        {
            var payment = await _subscriptionService.GetPaymentStatusAsync(paymentId);
            if (payment == null)
                return NotFound(new { message = "Không tìm thấy giao dịch" });

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking payment status: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi kiểm tra trạng thái thanh toán" });
        }
    }

    /// <summary>
    /// Webhook for payment notification (e.g. from Casso/Sepay)
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> PaymentWebhook([FromBody] object webhookData)
    {
        // TODO: Implement specific webhook logic based on provider (Casso, Sepay, etc.)
        // This is a placeholder for Step 7 & 8
        _logger.LogInformation($"Received webhook: {webhookData}");
        return Ok(new { success = true });
    }
}

public class CancelSubscriptionDto
{
    public string? Reason { get; set; }
}
