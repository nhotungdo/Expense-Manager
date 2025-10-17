using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public TransactionsController(ExpenseManagerContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    public record CreateTransactionRequest(decimal Amount, long CategoryId, int Type, DateTime TransactionDate, string? Description);
    public record UpdateTransactionRequest(decimal Amount, long CategoryId, int Type, DateTime TransactionDate, string? Description);

    [HttpGet]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] string? type = null, [FromQuery] long? categoryId = null)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var query = _db.Transactions.Where(t => t.UserId == userId);

        if (from.HasValue) query = query.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue) query = query.Where(t => t.TransactionDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (string.Equals(type, "Income", StringComparison.OrdinalIgnoreCase)) query = query.Where(t => t.Type == 1);
            else if (string.Equals(type, "Expense", StringComparison.OrdinalIgnoreCase)) query = query.Where(t => t.Type == 0);
        }
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);

        var total = query.Count();
        var items = query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Amount,
                t.CategoryId,
                t.Type,
                t.TransactionDate,
                t.Description
            })
            .ToList();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:long}")]
    public IActionResult Get(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var t = _db.Transactions.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (t == null) return NotFound();
        return Ok(new
        {
            t.Id,
            t.Amount,
            t.CategoryId,
            t.Type,
            t.TransactionDate,
            t.Description
        });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateTransactionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        if (request.Amount <= 0) return BadRequest(new { error = "Amount must be positive" });
        var validType = request.Type == 0 || request.Type == 1;
        if (!validType) return BadRequest(new { error = "Type must be Income or Expense" });
        var category = _db.Categories.FirstOrDefault(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId));
        if (category == null) return BadRequest(new { error = "Invalid CategoryId" });

        var entity = new Transaction
        {
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            Type = request.Type,
            TransactionDate = request.TransactionDate,
            Description = request.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(entity);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id });
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] UpdateTransactionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Transactions.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity == null) return NotFound();

        if (request.Amount <= 0) return BadRequest(new { error = "Amount must be positive" });
        var validType = request.Type == 0 || request.Type == 1;
        if (!validType) return BadRequest(new { error = "Type must be Income or Expense" });
        var category = _db.Categories.FirstOrDefault(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId));
        if (category == null) return BadRequest(new { error = "Invalid CategoryId" });

        entity.Amount = request.Amount;
        entity.CategoryId = request.CategoryId;
        entity.Type = request.Type;
        entity.TransactionDate = request.TransactionDate;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Transactions.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity == null) return NotFound();
        _db.Transactions.Remove(entity);
        _db.SaveChanges();
        return NoContent();
    }
}


