using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using System.Security.Claims;
using Microsoft.Extensions.Configuration; // Added
using System.Net; // Added

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly VnPayService _vnPayService;
    private readonly IServicePackageService _packageService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;

    public PaymentController(
        VnPayService vnPayService,
        IServicePackageService packageService,
        ILogger<PaymentController> logger,
        IConfiguration configuration)
    {
        _vnPayService = vnPayService;
        _packageService = packageService;
        _logger = logger;
        _configuration = configuration;
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
            var accountNo = vietQr["AccountNo"] ?? "8827256654";
            var template = vietQr["Template"] ?? "qr_only";
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
                package = new { package.Id, package.Name, package.Price }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create VietQR URL");
            return StatusCode(500, new { message = "Không thể tạo mã QR thanh toán" });
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





