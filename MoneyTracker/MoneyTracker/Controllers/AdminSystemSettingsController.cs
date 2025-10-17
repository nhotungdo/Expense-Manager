using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/admin/system-settings")]
[Authorize(Policy = "AdminOnly")]
public class AdminSystemSettingsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public AdminSystemSettingsController(ExpenseManagerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult List()
    {
        var items = _db.SystemSettings
            .OrderBy(s => s.SettingKey)
            .Select(s => new { s.Id, s.SettingKey, s.SettingValue, s.Description, s.SettingType, s.IsActive, s.UpdatedAt })
            .ToList();
        return Ok(items);
    }

    public record UpsertSettingRequest(string SettingKey, string SettingValue, string SettingType, string? Description, bool? IsActive);

    [HttpPost]
    public IActionResult Create([FromBody] UpsertSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SettingKey)) return BadRequest(new { error = "SettingKey required" });
        var exists = _db.SystemSettings.FirstOrDefault(s => s.SettingKey == request.SettingKey);
        if (exists != null) return Conflict(new { error = "SettingKey already exists" });
        var now = DateTime.UtcNow;
        var s = new SystemSetting
        {
            SettingKey = request.SettingKey,
            SettingValue = request.SettingValue,
            SettingType = request.SettingType,
            Description = request.Description,
            IsActive = request.IsActive ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.SystemSettings.Add(s);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = s.Id }, new { s.Id });
    }

    [HttpGet("{id:long}")]
    public IActionResult Get(long id)
    {
        var s = _db.SystemSettings.FirstOrDefault(x => x.Id == id);
        if (s == null) return NotFound();
        return Ok(new { s.Id, s.SettingKey, s.SettingValue, s.Description, s.SettingType, s.IsActive, s.UpdatedAt });
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] UpsertSettingRequest request)
    {
        var s = _db.SystemSettings.FirstOrDefault(x => x.Id == id);
        if (s == null) return NotFound();
        if (!string.Equals(s.SettingKey, request.SettingKey, StringComparison.Ordinal)) return BadRequest(new { error = "SettingKey cannot be changed" });
        s.SettingValue = request.SettingValue;
        s.SettingType = request.SettingType;
        s.Description = request.Description;
        if (request.IsActive.HasValue) s.IsActive = request.IsActive.Value;
        s.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        var s = _db.SystemSettings.FirstOrDefault(x => x.Id == id);
        if (s == null) return NotFound();
        _db.SystemSettings.Remove(s);
        _db.SaveChanges();
        return NoContent();
    }
}


