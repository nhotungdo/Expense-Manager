using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ExpenseManagerContext context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var targetMonth = month ?? DateTime.Now.Month;
                var targetYear = year ?? DateTime.Now.Year;

                var monthStart = new DateOnly(targetYear, targetMonth, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Get income and expense data
                var monthlyIncome = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= monthStart &&
                               i.IncomeDate <= monthEnd)
                    .SumAsync(i => i.Amount);

                var monthlyExpense = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= monthStart &&
                               e.ExpenseDate <= monthEnd)
                    .SumAsync(e => e.Amount);

                // Get income by category
                var incomeByCategory = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= monthStart &&
                               i.IncomeDate <= monthEnd)
                    .Include(i => i.Category)
                    .GroupBy(i => i.Category != null ? i.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(i => i.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Amount)
                    .ToListAsync();

                // Get expense by category
                var expenseByCategory = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= monthStart &&
                               e.ExpenseDate <= monthEnd)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category != null ? e.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Amount)
                    .ToListAsync();

                // Get daily spending trend
                var dailySpending = new List<object>();
                for (int day = 1; day <= monthEnd.Day; day++)
                {
                    var currentDate = new DateOnly(targetYear, targetMonth, day);
                    var dayIncome = await _context.Incomes
                        .Where(i => i.UserId == userId && i.IncomeDate == currentDate)
                        .SumAsync(i => i.Amount);
                    var dayExpense = await _context.Expenses
                        .Where(e => e.UserId == userId && e.ExpenseDate == currentDate)
                        .SumAsync(e => e.Amount);

                    dailySpending.Add(new
                    {
                        Date = currentDate.ToString("dd/MM"),
                        Income = dayIncome,
                        Expense = dayExpense,
                        Net = dayIncome - dayExpense
                    });
                }

                var report = new
                {
                    Month = targetMonth,
                    Year = targetYear,
                    TotalIncome = monthlyIncome,
                    TotalExpense = monthlyExpense,
                    NetAmount = monthlyIncome - monthlyExpense,
                    IncomeByCategory = incomeByCategory,
                    ExpenseByCategory = expenseByCategory,
                    DailySpending = dailySpending,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int? year)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var targetYear = year ?? DateTime.Now.Year;
                var yearStart = new DateOnly(targetYear, 1, 1);
                var yearEnd = new DateOnly(targetYear, 12, 31);

                // Get monthly trends
                var monthlyTrends = new List<object>();
                for (int month = 1; month <= 12; month++)
                {
                    var monthStart = new DateOnly(targetYear, month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var monthIncome = await _context.Incomes
                        .Where(i => i.UserId == userId &&
                                   i.IncomeDate >= monthStart &&
                                   i.IncomeDate <= monthEnd)
                        .SumAsync(i => i.Amount);

                    var monthExpense = await _context.Expenses
                        .Where(e => e.UserId == userId &&
                                   e.ExpenseDate >= monthStart &&
                                   e.ExpenseDate <= monthEnd)
                        .SumAsync(e => e.Amount);

                    monthlyTrends.Add(new
                    {
                        Month = month,
                        MonthName = new DateTime(targetYear, month, 1).ToString("MMM"),
                        Income = monthIncome,
                        Expense = monthExpense,
                        Net = monthIncome - monthExpense
                    });
                }

                // Get yearly totals
                var yearlyIncome = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= yearStart &&
                               i.IncomeDate <= yearEnd)
                    .SumAsync(i => i.Amount);

                var yearlyExpense = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= yearStart &&
                               e.ExpenseDate <= yearEnd)
                    .SumAsync(e => e.Amount);

                // Get top categories
                var topIncomeCategories = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= yearStart &&
                               i.IncomeDate <= yearEnd)
                    .Include(i => i.Category)
                    .GroupBy(i => i.Category != null ? i.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(i => i.Amount),
                        Percentage = yearlyIncome > 0 ? (g.Sum(i => i.Amount) / yearlyIncome) * 100 : 0m
                    })
                    .OrderByDescending(g => g.Amount)
                    .Take(5)
                    .ToListAsync();

                var topExpenseCategories = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= yearStart &&
                               e.ExpenseDate <= yearEnd)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category != null ? e.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Percentage = yearlyExpense > 0 ? (g.Sum(e => e.Amount) / yearlyExpense) * 100 : 0m
                    })
                    .OrderByDescending(g => g.Amount)
                    .Take(5)
                    .ToListAsync();

                // Percentages are now calculated in the Select statement above

                var report = new
                {
                    Year = targetYear,
                    TotalIncome = yearlyIncome,
                    TotalExpense = yearlyExpense,
                    NetAmount = yearlyIncome - yearlyExpense,
                    MonthlyTrends = monthlyTrends,
                    TopIncomeCategories = topIncomeCategories,
                    TopExpenseCategories = topExpenseCategories,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating yearly report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("custom")]
        public async Task<IActionResult> GetCustomReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var start = DateOnly.FromDateTime(startDate);
                var end = DateOnly.FromDateTime(endDate);

                // Get income and expense data
                var totalIncome = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= start &&
                               i.IncomeDate <= end)
                    .SumAsync(i => i.Amount);

                var totalExpense = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= start &&
                               e.ExpenseDate <= end)
                    .SumAsync(e => e.Amount);

                // Get category breakdown
                var incomeByCategory = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= start &&
                               i.IncomeDate <= end)
                    .Include(i => i.Category)
                    .GroupBy(i => i.Category != null ? i.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(i => i.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Amount)
                    .ToListAsync();

                var expenseByCategory = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= start &&
                               e.ExpenseDate <= end)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category != null ? e.Category.Name : "Khác")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Amount)
                    .ToListAsync();

                // Get transaction count
                var incomeCount = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= start &&
                               i.IncomeDate <= end)
                    .CountAsync();

                var expenseCount = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= start &&
                               e.ExpenseDate <= end)
                    .CountAsync();

                var report = new
                {
                    StartDate = start,
                    EndDate = end,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    NetAmount = totalIncome - totalExpense,
                    IncomeByCategory = incomeByCategory,
                    ExpenseByCategory = expenseByCategory,
                    TransactionCount = new
                    {
                        Income = incomeCount,
                        Expense = expenseCount,
                        Total = incomeCount + expenseCount
                    },
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("export/{format}")]
        public async Task<IActionResult> ExportReport([FromRoute] string format, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string reportType = "monthly")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                // This would integrate with the existing ReportExportService
                // For now, return a placeholder response
                return Ok(new { message = $"Export functionality for {format} format will be implemented" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            return null;
        }
    }
}