using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentGatewayService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PaymentGatewayService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // Get client IP address
        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        // Get user agent
        private string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        // Get current user ID
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Create payment transaction and get payment URL
        /// POST /api/payments/create
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                // Validate request
                if (request.UserId <= 0 || request.PackageId <= 0)
                {
                    return BadRequest(new { error = "Invalid UserId or PackageId" });
                }

                // Get IP and User Agent for security logging
                var ipAddress = GetClientIpAddress();
                var userAgent = GetUserAgent();

                // Create payment transaction
                var result = await _paymentService.CreatePaymentTransactionAsync(
                    request.UserId,
                    request.PackageId,
                    ipAddress,
                    userAgent);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                // Return payment URL and transaction details
                return Ok(new
                {
                    success = true,
                    paymentTransactionId = result.PaymentTransactionId,
                    paymentUrl = result.PaymentUrl,
                    sessionToken = result.SessionToken,
                    amount = result.Amount,
                    currency = result.Currency,
                    message = "Payment transaction created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePayment");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Handle return callback from link.com gateway
        /// GET /api/payments/return?session={token}&transaction_id={id}&status={status}&signature={sig}
        /// </summary>
        [HttpGet("return")]
        public async Task<IActionResult> ReturnCallback(
            [FromQuery] string session,
            [FromQuery] string transaction_id,
            [FromQuery] string status,
            [FromQuery] string signature)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(session) || string.IsNullOrEmpty(transaction_id) || string.IsNullOrEmpty(status))
                {
                    return Redirect("/subscription/failed?error=missing_parameters");
                }

                // Get IP and User Agent for security
                var ipAddress = GetClientIpAddress();
                var userAgent = GetUserAgent();

                // Process callback
                var result = await _paymentService.ProcessReturnCallbackAsync(
                    session,
                    transaction_id,
                    status,
                    signature,
                    ipAddress,
                    userAgent);

                if (!result.Success)
                {
                    _logger.LogWarning($"Payment callback failed: {result.ErrorMessage}");
                    return Redirect($"/subscription/failed?error={Uri.EscapeDataString(result.ErrorMessage)}");
                }

                // Redirect based on status
                if (result.Status == 2) // Success
                {
                    return Redirect($"/subscription/success?transaction={result.PaymentTransactionId}");
                }
                else if (result.Status == 3) // Failed
                {
                    return Redirect($"/subscription/failed?transaction={result.PaymentTransactionId}");
                }
                else if (result.Status == 4) // Cancelled
                {
                    return Redirect($"/subscription/failed?transaction={result.PaymentTransactionId}&cancelled=true");
                }
                else
                {
                    return Redirect($"/subscription/processing?transaction={result.PaymentTransactionId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReturnCallback");
                return Redirect("/subscription/failed?error=system_error");
            }
        }

        /// <summary>
        /// Handle cancel callback from link.com gateway
        /// GET /api/payments/cancel?session={token}
        /// </summary>
        [HttpGet("cancel")]
        public async Task<IActionResult> CancelCallback([FromQuery] string session)
        {
            try
            {
                if (string.IsNullOrEmpty(session))
                {
                    return Redirect("/subscription/failed?error=missing_session");
                }

                // Process cancel
                await _paymentService.ProcessCancelCallbackAsync(session);

                return Redirect("/subscription/failed?cancelled=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CancelCallback");
                return Redirect("/subscription/failed?error=system_error");
            }
        }

        /// <summary>
        /// Handle webhook from link.com gateway
        /// POST /api/payments/webhook
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] WebhookPayload payload, [FromHeader(Name = "X-Signature")] string signature)
        {
            try
            {
                // Validate signature
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Webhook received without signature");
                    return Unauthorized(new { error = "Missing signature" });
                }

                // Process webhook
                var success = await _paymentService.ProcessWebhookAsync(payload, signature);

                if (!success)
                {
                    return BadRequest(new { error = "Webhook processing failed" });
                }

                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get payment transaction status
        /// GET /api/payments/status/{transactionId}
        /// </summary>
        [HttpGet("status/{transactionId}")]
        public async Task<IActionResult> GetPaymentStatus(long transactionId)
        {
            try
            {
                // This would query the database for transaction status
                // Implementation left as exercise
                return Ok(new { transactionId, status = "pending" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentStatus");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request DTOs
    public class CreatePaymentRequest
    {
        public long UserId { get; set; }
        public int PackageId { get; set; }
    }
}
