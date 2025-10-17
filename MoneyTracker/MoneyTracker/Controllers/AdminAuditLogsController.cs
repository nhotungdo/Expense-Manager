using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Policy = "AdminOnly")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public AdminAuditLogsController(ExpenseManagerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult List([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? action = null, [FromQuery] string? entityType = null, [FromQuery] long? userId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var q = _db.AuditLogs.AsQueryable();
        if (from.HasValue) q = q.Where(a => a.CreatedAt >= from);
        if (to.HasValue) q = q.Where(a => a.CreatedAt <= to);
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(a => a.EntityType == entityType);
        if (userId.HasValue) q = q.Where(a => a.UserId == userId.Value);

        var total = q.Count();
        var items = q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new { a.Id, a.UserId, a.Action, a.EntityType, a.EntityId, a.IpAddress, a.UserAgent, a.CreatedAt })
            .ToList();
        return Ok(new { total, page, pageSize, items });
    }
}


