using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public NotificationsController(ExpenseManagerContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    [HttpGet]
    public IActionResult List([FromQuery] bool onlyUnread = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var q = _db.Notifications.Where(n => n.UserId == userId);
        if (onlyUnread) q = q.Where(n => !n.IsRead);
        var total = q.Count();
        var items = q.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new { n.Id, n.Title, n.Message, n.Type, n.IsRead, n.IsImportant, n.ActionUrl, n.CreatedAt })
            .ToList();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpPut("{id:long}/read")]
    public IActionResult MarkRead(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var n = _db.Notifications.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (n == null) return NotFound();
        n.IsRead = true;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpPut("read-all")]
    public IActionResult MarkAllRead()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var items = _db.Notifications.Where(x => x.UserId == userId && !x.IsRead).ToList();
        foreach (var n in items) n.IsRead = true;
        _db.SaveChanges();
        return NoContent();
    }
}


