using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;
    private readonly IWebHostEnvironment _env;

    public ReportsController(ExpenseManagerContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    public record CreateReportRequest(string ReportType, string ReportName, DateOnly StartDate, DateOnly EndDate, string? FileFormat);

    [HttpGet]
    public IActionResult List()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var items = _db.Reports.Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Id, r.ReportType, r.ReportName, r.StartDate, r.EndDate, r.FileFormat, r.FilePath, r.GeneratedAt })
            .ToList();
        return Ok(items);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateReportRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.ReportType)) return BadRequest(new { error = "ReportType is required" });
        if (string.IsNullOrWhiteSpace(request.ReportName)) return BadRequest(new { error = "ReportName is required" });
        if (request.EndDate < request.StartDate) return BadRequest(new { error = "EndDate must be after StartDate" });

        var now = DateTime.UtcNow;
        var report = new Report
        {
            UserId = userId,
            ReportType = request.ReportType,
            ReportName = request.ReportName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            FileFormat = string.IsNullOrWhiteSpace(request.FileFormat) ? "csv" : request.FileFormat!.ToLowerInvariant(),
            CreatedAt = now
        };
        _db.Reports.Add(report);
        _db.SaveChanges();

        // Simple inline generation for CSV demo; in production, use background service
        var relDir = Path.Combine("reports", userId.ToString());
        var absDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relDir);
        Directory.CreateDirectory(absDir);
        var fileName = $"{report.Id}_{report.ReportType}_{now:yyyyMMddHHmmss}.csv";
        var absPath = Path.Combine(absDir, fileName);
        var relPath = Path.Combine(relDir, fileName).Replace("\\", "/");

        var sb = new StringBuilder();
        sb.AppendLine("Date,Type,CategoryId,Amount,Description");
        var tx = _db.Transactions.Where(t => t.UserId == userId && t.TransactionDate >= request.StartDate.ToDateTime(TimeOnly.MinValue) && t.TransactionDate <= request.EndDate.ToDateTime(TimeOnly.MaxValue))
            .OrderBy(t => t.TransactionDate)
            .Select(t => new { t.TransactionDate, t.Type, t.CategoryId, t.Amount, t.Description })
            .ToList();
        foreach (var t in tx)
        {
            var desc = (t.Description ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Replace(',', ' ');
            sb.AppendLine($"{t.TransactionDate:yyyy-MM-dd},{t.Type},{t.CategoryId},{t.Amount},{desc}");
        }
        System.IO.File.WriteAllText(absPath, sb.ToString(), Encoding.UTF8);

        report.FilePath = "/" + relPath;
        report.GeneratedAt = DateTime.UtcNow;
        _db.SaveChanges();

        return CreatedAtAction(nameof(Download), new { id = report.Id }, new { report.Id, report.FilePath });
    }

    [HttpGet("{id:long}/download")]
    [AllowAnonymous]
    public IActionResult Download(long id)
    {
        // Allow direct file download since file is under wwwroot/reports; still check ownership if needed
        var report = _db.Reports.FirstOrDefault(r => r.Id == id);
        if (report == null || string.IsNullOrWhiteSpace(report.FilePath)) return NotFound();
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var absPath = Path.Combine(webRoot, report.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (!System.IO.File.Exists(absPath)) return NotFound();
        var contentType = "text/csv";
        var fileName = Path.GetFileName(absPath);
        var bytes = System.IO.File.ReadAllBytes(absPath);
        return File(bytes, contentType, fileName);
    }
}


