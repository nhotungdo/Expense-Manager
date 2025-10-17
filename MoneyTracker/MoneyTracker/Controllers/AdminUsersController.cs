using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class AdminUsersController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public AdminUsersController(ExpenseManagerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var total = _db.Users.Count();
        var items = _db.Users
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.Email, u.UserName, u.FullName, u.Enabled, u.LockoutEnd, u.Role, u.CreatedAt, u.LastLogin })
            .ToList();
        return Ok(new { total, page, pageSize, items });
    }

    public record UpdateUserStatusRequest(bool? Enabled, bool? Lock);

    [HttpPut("{id:long}")]
    public IActionResult UpdateStatus(long id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();
        if (request.Enabled.HasValue) user.Enabled = request.Enabled.Value;
        if (request.Lock.HasValue)
        {
            user.LockoutEnd = request.Lock.Value ? DateTimeOffset.UtcNow.AddYears(100) : null;
        }
        _db.SaveChanges();
        return NoContent();
    }
}


