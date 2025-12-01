using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;
        private readonly IWebHostEnvironment _env;

        public OcrController(IOcrService ocrService, IWebHostEnvironment env)
        {
            _ocrService = ocrService;
            _env = env;
        }

        // POST: api/ocr/process
        // Accepts base64 image in body and returns OcrResultDto
        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] OcrReceiptDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageBase64))
                return BadRequest(new { message = "ImageBase64 is required" });

            try
            {
                var result = await _ocrService.ProcessReceiptAsync(dto.ImageBase64);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log exception in real app
                return StatusCode(500, new { message = "OCR processing failed", detail = ex.Message });
            }
        }

        // POST: api/ocr/upload
        // Accepts multipart/form-data with file field 'file' and saves under wwwroot/uploads
        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)] // 10MB
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required" });

            try
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativeUrl = $"/uploads/{fileName}";
                return Ok(new { url = relativeUrl, attachmentUrl = relativeUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload failed", detail = ex.Message });
            }
        }
    }
}
