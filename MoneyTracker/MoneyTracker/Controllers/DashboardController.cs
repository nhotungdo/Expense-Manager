using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ExpenseManagerContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                // Get current month data
                var currentMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var currentMonthExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                // Get last month data for comparison
                var lastMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == lastMonth && i.Date.Year == lastMonthYear)
                    .SumAsync(i => i.Amount);

                var lastMonthExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == lastMonth && e.Date.Year == lastMonthYear)
                    .SumAsync(e => e.Amount);

                // Calculate transaction count
                var monthlyTransactions = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .CountAsync() +
                    await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                    .CountAsync();

                // Calculate percentage changes
                var incomeChange = lastMonthIncomes > 0
                    ? Math.Round(((currentMonthIncomes - lastMonthIncomes) / lastMonthIncomes) * 100, 1)
                    : 0;

                var expenseChange = lastMonthExpenses > 0
                    ? Math.Round(((currentMonthExpenses - lastMonthExpenses) / lastMonthExpenses) * 100, 1)
                    : 0;

                var currentBalance = currentMonthIncomes - currentMonthExpenses;

                var stats = new
                {
                    totalIncome = currentMonthIncomes,
                    totalExpense = currentMonthExpenses,
                    currentBalance = currentBalance,
                    monthlyTransactions = monthlyTransactions,
                    incomeChange = incomeChange,
                    expenseChange = expenseChange,
                    transactionChange = 15.0 // Mock data for now
                };

                _logger.LogInformation("Dashboard stats retrieved for user {UserId}", userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                // Get recent expenses
                var recentExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .OrderByDescending(e => e.Date)
                    .Take(10)
                    .Select(e => new
                    {
                        id = e.Id,
                        type = "expense",
                        amount = e.Amount,
                        description = e.Description,
                        category = e.Category != null ? e.Category.Name : "Khác",
                        date = e.Date
                    })
                    .ToListAsync();

                // Get recent incomes
                var recentIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .Include(i => i.Category)
                    .OrderByDescending(i => i.Date)
                    .Take(10)
                    .Select(i => new
                    {
                        id = i.Id,
                        type = "income",
                        amount = i.Amount,
                        description = i.Description,
                        category = i.Category != null ? i.Category.Name : "Khác",
                        date = i.Date
                    })
                    .ToListAsync();

                // Combine and sort by date
                var allTransactions = recentExpenses.Concat(recentIncomes)
                    .OrderByDescending(t => t.date)
                    .Take(10)
                    .ToList();

                _logger.LogInformation("Recent transactions retrieved for user {UserId}", userId);
                return Ok(allTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("charts")]
        public async Task<IActionResult> GetChartsData([FromQuery] string period = "week")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                DateTime startDate, endDate;
                var labels = new List<string>();
                var incomeData = new List<decimal>();
                var expenseData = new List<decimal>();

                // Set date range based on period
                switch (period.ToLower())
                {
                    case "week":
                        startDate = DateTime.Now.AddDays(-7);
                        endDate = DateTime.Now;
                        for (int i = 6; i >= 0; i--)
                        {
                            var date = DateTime.Now.AddDays(-i);
                            labels.Add(date.ToString("dd/MM"));

                            var dayIncome = await _context.Incomes
                                .Where(i => i.UserId == userId && i.Date.Date == date.Date)
                                .SumAsync(i => i.Amount);
                            incomeData.Add(dayIncome);

                            var dayExpense = await _context.Expenses
                                .Where(e => e.UserId == userId && e.Date.Date == date.Date)
                                .SumAsync(e => e.Amount);
                            expenseData.Add(dayExpense);
                        }
                        break;

                    case "month":
                        startDate = DateTime.Now.AddDays(-30);
                        endDate = DateTime.Now;
                        for (int i = 29; i >= 0; i--)
                        {
                            var date = DateTime.Now.AddDays(-i);
                            labels.Add(date.ToString("dd/MM"));

                            var dayIncome = await _context.Incomes
                                .Where(i => i.UserId == userId && i.Date.Date == date.Date)
                                .SumAsync(i => i.Amount);
                            incomeData.Add(dayIncome);

                            var dayExpense = await _context.Expenses
                                .Where(e => e.UserId == userId && e.Date.Date == date.Date)
                                .SumAsync(e => e.Amount);
                            expenseData.Add(dayExpense);
                        }
                        break;

                    case "year":
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now;
                        for (int i = 11; i >= 0; i--)
                        {
                            var date = DateTime.Now.AddMonths(-i);
                            labels.Add(date.ToString("MM/yyyy"));

                            var monthIncome = await _context.Incomes
                                .Where(i => i.UserId == userId && i.Date.Month == date.Month && i.Date.Year == date.Year)
                                .SumAsync(i => i.Amount);
                            incomeData.Add(monthIncome);

                            var monthExpense = await _context.Expenses
                                .Where(e => e.UserId == userId && e.Date.Month == date.Month && e.Date.Year == date.Year)
                                .SumAsync(e => e.Amount);
                            expenseData.Add(monthExpense);
                        }
                        break;

                    default:
                        return BadRequest(new { message = "Invalid period" });
                }

                // Get category spending data
                var categoryData = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category != null ? e.Category.Name : "Khác")
                    .Select(g => new
                    {
                        category = g.Key,
                        amount = g.Sum(e => e.Amount)
                    })
                    .OrderByDescending(g => g.amount)
                    .Take(6)
                    .ToListAsync();

                var chartData = new
                {
                    trends = new
                    {
                        labels = labels,
                        income = incomeData,
                        expense = expenseData
                    },
                    categories = new
                    {
                        labels = categoryData.Select(c => c.category).ToList(),
                        data = categoryData.Select(c => (double)c.amount).ToList()
                    }
                };

                _logger.LogInformation("Charts data retrieved for user {UserId} with period {Period}", userId, period);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving charts data");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetAIInsights()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                // Get current month data
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                var currentMonthExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                var lastMonthExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == lastMonth && e.Date.Year == lastMonthYear)
                    .SumAsync(e => e.Amount);

                var currentMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var lastMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == lastMonth && i.Date.Year == lastMonthYear)
                    .SumAsync(i => i.Amount);

                var insights = new List<object>();

                // Expense comparison insight
                if (lastMonthExpenses > 0)
                {
                    var expenseChange = ((currentMonthExpenses - lastMonthExpenses) / lastMonthExpenses) * 100;
                    if (expenseChange > 20)
                    {
                        insights.Add(new
                        {
                            type = "warning",
                            title = "Chi tiêu tăng cao",
                            message = $"Bạn đã chi tiêu nhiều hơn {expenseChange:F1}% so với tháng trước. Hãy xem xét cắt giảm chi phí không cần thiết.",
                            icon = "fas fa-exclamation-triangle"
                        });
                    }
                }

                // Income comparison insight
                if (lastMonthIncomes > 0)
                {
                    var incomeChange = ((currentMonthIncomes - lastMonthIncomes) / lastMonthIncomes) * 100;
                    if (incomeChange > 10)
                    {
                        insights.Add(new
                        {
                            type = "success",
                            title = "Thu nhập tăng trưởng",
                            message = $"Thu nhập của bạn đã tăng {incomeChange:F1}% so với tháng trước!",
                            icon = "fas fa-chart-line"
                        });
                    }
                }

                // Balance insight
                var currentBalance = currentMonthIncomes - currentMonthExpenses;
                if (currentBalance < 0)
                {
                    insights.Add(new
                    {
                        type = "danger",
                        title = "Số dư âm",
                        message = "Bạn đang chi tiêu nhiều hơn thu nhập. Hãy cân nhắc điều chỉnh ngân sách.",
                        icon = "fas fa-exclamation-circle"
                    });
                }

                _logger.LogInformation("AI insights retrieved for user {UserId}", userId);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AI insights");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}