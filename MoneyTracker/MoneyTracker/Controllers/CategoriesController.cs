using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public CategoriesController(ExpenseManagerContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    public record CreateCategoryRequest(string Name, int Type, string? Description, string? Icon, string? Color);
    public record UpdateCategoryRequest(string Name, int Type, string? Description, string? Icon, string? Color, bool? IsActive);

    [HttpGet]
    public IActionResult List()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var items = _db.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Type, c.Description, c.Icon, c.Color, c.IsDefault, c.IsActive, c.UserId })
            .ToList();
        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public IActionResult Get(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var c = _db.Categories.FirstOrDefault(x => x.Id == id && (x.UserId == null || x.UserId == userId));
        if (c == null) return NotFound();
        return Ok(new { c.Id, c.Name, c.Type, c.Description, c.Icon, c.Color, c.IsDefault, c.IsActive, c.UserId });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateCategoryRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var error = Validate(request.Name, request.Type);
        if (error != null) return BadRequest(new { error });

        var entity = new Category
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Description = request.Description,
            Icon = request.Icon,
            Color = request.Color,
            UserId = userId,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id });
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] UpdateCategoryRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Categories.FirstOrDefault(x => x.Id == id && (x.UserId == userId || x.UserId == null));
        if (entity == null) return NotFound();
        if (entity.IsDefault && entity.UserId == null)
        {
            return Forbid();
        }
        var error = Validate(request.Name, request.Type);
        if (error != null) return BadRequest(new { error });

        entity.Name = request.Name.Trim();
        entity.Type = request.Type;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.Color = request.Color;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var entity = _db.Categories.FirstOrDefault(x => x.Id == id && (x.UserId == userId || x.UserId == null));
        if (entity == null) return NotFound();
        if (entity.IsDefault && entity.UserId == null)
        {
            return Forbid();
        }

        // Use stored procedure DeleteCategorySafely to enforce business rules
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DeleteCategorySafely";
        cmd.CommandType = CommandType.StoredProcedure;
        var pId = new SqlParameter("@CategoryId", id);
        cmd.Parameters.Add(pId);
        var affected = cmd.ExecuteNonQuery();
        if (affected <= 0)
        {
            return BadRequest(new { error = "Delete failed" });
        }
        return NoContent();
    }

    private string? Validate(string name, int type)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required";
        if (name.Trim().Length > 100) return "Name too long";
        // Type mapping per model (e.g., 0=Expense, 1=Income) — keep within expected range
        if (type != 0 && type != 1) return "Type must be 0 (Expense) or 1 (Income)";
        return null;
    }
}


