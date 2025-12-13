using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<OcrController> _logger;

        public OcrController(IOcrService ocrService, ILogger<OcrController> logger)
        {
            _ocrService = ocrService;
            _logger = logger;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanReceipt([FromBody] OcrReceiptDto request)
        {
            if (string.IsNullOrEmpty(request.ImageBase64))
            {
                return BadRequest("Image is required");
            }

            try
            {
                _logger.LogInformation("Processing receipt OCR for user {UserId}", User.Identity?.Name);
                
                // In a real app, validation of the Base64 string/image format would go here.
                
                var result = await _ocrService.ProcessReceiptAsync(request.ImageBase64);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing receipt OCR");
                return StatusCode(500, "Internal server error during OCR processing");
            }
        }
    }
}
