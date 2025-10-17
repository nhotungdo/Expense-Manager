using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ExpenseManagerContext _db;

    public DashboardController(ExpenseManagerContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out long userId)
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(sub, out userId);
    }

    private async Task<List<Dictionary<string, object>>> ExecProcAsync(string procName, params SqlParameter[] parameters)
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = procName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (var p in parameters) cmd.Parameters.Add(p);
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                row[name] = value!;
            }
            result.Add(row);
        }
        return result;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var list = await ExecProcAsync(
            "GetUserDashboardStats",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@FromDate", (object?)from ?? DBNull.Value) { IsNullable = true },
            new SqlParameter("@ToDate", (object?)to ?? DBNull.Value) { IsNullable = true }
        );
        return Ok(list);
    }

    [HttpGet("category-summary")]
    public async Task<IActionResult> CategorySummary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var list = await ExecProcAsync(
            "GetCategorySpendingSummary",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@FromDate", (object?)from ?? DBNull.Value) { IsNullable = true },
            new SqlParameter("@ToDate", (object?)to ?? DBNull.Value) { IsNullable = true }
        );
        return Ok(list);
    }

    [HttpGet("monthly-trends")]
    public async Task<IActionResult> MonthlyTrends([FromQuery] int? year = null)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var theYear = year ?? DateTime.UtcNow.Year;
        var list = await ExecProcAsync(
            "GetMonthlyTrends",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Year", theYear)
        );
        return Ok(list);
    }
}


