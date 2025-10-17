using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public BudgetsController(ExpenseManagerContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    public record CreateBudgetRequest(long? CategoryId, decimal Amount, int Period, DateTime StartDate, DateTime EndDate);
    public record UpdateBudgetRequest(long? CategoryId, decimal Amount, int Period, DateTime StartDate, DateTime EndDate);

    [HttpGet]
    public IActionResult List()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var items = _db.Budgets.Where(b => b.UserId == userId)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Select(b => new { b.Id, b.CategoryId, b.Amount, b.Period, b.StartDate, b.EndDate })
            .ToList();
        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public IActionResult Get(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var b = _db.Budgets.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (b == null) return NotFound();
        return Ok(new { b.Id, b.CategoryId, b.Amount, b.Period, b.StartDate, b.EndDate });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateBudgetRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var validation = Validate(userId, request.CategoryId, request.Amount, request.Period, request.StartDate, request.EndDate);
        if (validation != null) return BadRequest(new { error = validation });

        var entity = new Budget
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Period = request.Period,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Budgets.Add(entity);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id });
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] UpdateBudgetRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Budgets.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity == null) return NotFound();
        var validation = Validate(userId, request.CategoryId, request.Amount, request.Period, request.StartDate, request.EndDate);
        if (validation != null) return BadRequest(new { error = validation });

        entity.CategoryId = request.CategoryId;
        entity.Amount = request.Amount;
        entity.Period = request.Period;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Budgets.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (entity == null) return NotFound();
        _db.Budgets.Remove(entity);
        _db.SaveChanges();
        return NoContent();
    }

    [HttpGet("{id:long}/progress")]
    public IActionResult Progress(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var b = _db.Budgets.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (b == null) return NotFound();

        // Compute total spent for category in range (expenses/transactions of Type Expense)
        var spent = _db.Transactions
            .Where(t => t.UserId == userId && t.Type == 0 && t.TransactionDate >= b.StartDate && t.TransactionDate <= b.EndDate)
            .Where(t => b.CategoryId == null || t.CategoryId == b.CategoryId)
            .Select(t => t.Amount)
            .DefaultIfEmpty(0)
            .Sum();

        var percent = b.Amount <= 0 ? 0 : (double)(spent / b.Amount) * 100.0;
        var status = percent >= 100 ? "Exceeded" : percent >= 80 ? "Warning" : "OK";
        return Ok(new { spent, budget = b.Amount, percent, status });
    }

    private string? Validate(long userId, long? categoryId, decimal amount, int period, DateTime start, DateTime end)
    {
        if (amount <= 0) return "Amount must be positive";
        if (end < start) return "EndDate must be after StartDate";
        if (period != 7 && period != 30 && period != 365) return "Period must be 7, 30, or 365";
        if (categoryId.HasValue)
        {
            var category = _db.Categories.FirstOrDefault(c => c.Id == categoryId.Value && (c.UserId == null || c.UserId == userId));
            if (category == null) return "Invalid CategoryId";
        }
        return null;
    }
}


