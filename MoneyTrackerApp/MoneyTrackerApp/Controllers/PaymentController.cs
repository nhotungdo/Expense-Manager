using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly VnPayService _vnPayService;
    private readonly IServicePackageService _packageService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        VnPayService vnPayService,
        IServicePackageService packageService,
        ILogger<PaymentController> logger)
    {
        _vnPayService = vnPayService;
        _packageService = packageService;
        _logger = logger;
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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        try
        {
            var paymentUrl = _vnPayService.CreatePaymentUrl(
                userId,
                package.Id,
                package.Price,
                package.Name,
                ipAddress);

            return Ok(new
            {
                success = true,
                paymentUrl,
                package = new { package.Id, package.Name, package.Price }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create VNPay QR URL");
            return StatusCode(500, new { message = "Không thể tạo liên kết thanh toán" });
        }
    }

    private long ResolveUserId(long? userId)
    {
        if (userId.HasValue && userId.Value > 0) return userId.Value;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(claim, out var parsed) ? parsed : 0;
    }
}

public class CreateQrPaymentRequest
{
    public long? UserId { get; set; }
    public int PackageId { get; set; }
}





