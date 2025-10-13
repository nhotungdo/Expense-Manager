using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

                // Get current month date range
                var currentDate = DateTime.Now;
                var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Get last month date range
                var lastMonth = startOfMonth.AddMonths(-1);
                var endOfLastMonth = startOfMonth.AddDays(-1);

                // Use stored procedure for current month stats
                var currentMonthStats = await _context.Database
                    .SqlQueryRaw<DashboardStatsDto>(
                        "EXEC GetUserDashboardStats @UserId, @StartDate, @EndDate",
                        new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                        new Microsoft.Data.SqlClient.SqlParameter("@StartDate", startOfMonth.Date),
                        new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endOfMonth.Date))
                    .FirstOrDefaultAsync();

                // Use stored procedure for last month stats
                var lastMonthStats = await _context.Database
                    .SqlQueryRaw<DashboardStatsDto>(
                        "EXEC GetUserDashboardStats @UserId, @StartDate, @EndDate",
                        new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                        new Microsoft.Data.SqlClient.SqlParameter("@StartDate", lastMonth.Date),
                        new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endOfLastMonth.Date))
                    .FirstOrDefaultAsync();

                // Calculate percentage changes
                var incomeChange = lastMonthStats?.TotalIncome > 0
                    ? Math.Round(((currentMonthStats?.TotalIncome ?? 0) - lastMonthStats.TotalIncome) / lastMonthStats.TotalIncome * 100, 1)
                    : 0;

                var expenseChange = lastMonthStats?.TotalExpense > 0
                    ? Math.Round(((currentMonthStats?.TotalExpense ?? 0) - lastMonthStats.TotalExpense) / lastMonthStats.TotalExpense * 100, 1)
                    : 0;

                var transactionChange = lastMonthStats?.TransactionCount > 0
                    ? Math.Round(((currentMonthStats?.TransactionCount ?? 0) - lastMonthStats.TransactionCount) / (double)lastMonthStats.TransactionCount * 100, 1)
                    : 0;

                var stats = new
                {
                    totalIncome = currentMonthStats?.TotalIncome ?? 0,
                    totalExpense = currentMonthStats?.TotalExpense ?? 0,
                    currentBalance = currentMonthStats?.NetIncome ?? 0,
                    monthlyTransactions = currentMonthStats?.TransactionCount ?? 0,
                    incomeChange = incomeChange,
                    expenseChange = expenseChange,
                    transactionChange = transactionChange
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

                // Get recent transactions from unified Transactions table
                var recentTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new
                    {
                        id = t.Id,
                        type = t.Type,
                        amount = t.Amount,
                        description = t.Note,
                        category = t.Category != null ? t.Category.Name : "Khác",
                        date = t.TransactionDate
                    })
                    .ToListAsync();

                _logger.LogInformation("Recent transactions retrieved for user {UserId}", userId);
                return Ok(recentTransactions);
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

                            var dayStats = await _context.Database
                                .SqlQueryRaw<DashboardStatsDto>(
                                    "EXEC GetUserDashboardStats @UserId, @StartDate, @EndDate",
                                    new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                                    new Microsoft.Data.SqlClient.SqlParameter("@StartDate", date.Date),
                                    new Microsoft.Data.SqlClient.SqlParameter("@EndDate", date.Date))
                                .FirstOrDefaultAsync();

                            incomeData.Add(dayStats?.TotalIncome ?? 0);
                            expenseData.Add(dayStats?.TotalExpense ?? 0);
                        }
                        break;

                    case "month":
                        startDate = DateTime.Now.AddDays(-30);
                        endDate = DateTime.Now;
                        for (int i = 29; i >= 0; i--)
                        {
                            var date = DateTime.Now.AddDays(-i);
                            labels.Add(date.ToString("dd/MM"));

                            var dayStats = await _context.Database
                                .SqlQueryRaw<DashboardStatsDto>(
                                    "EXEC GetUserDashboardStats @UserId, @StartDate, @EndDate",
                                    new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                                    new Microsoft.Data.SqlClient.SqlParameter("@StartDate", date.Date),
                                    new Microsoft.Data.SqlClient.SqlParameter("@EndDate", date.Date))
                                .FirstOrDefaultAsync();

                            incomeData.Add(dayStats?.TotalIncome ?? 0);
                            expenseData.Add(dayStats?.TotalExpense ?? 0);
                        }
                        break;

                    case "year":
                        startDate = DateTime.Now.AddYears(-1);
                        endDate = DateTime.Now;

                        // Use stored procedure for monthly trends
                        var monthlyTrends = await _context.Database
                            .SqlQueryRaw<MonthlyTrendDto>(
                                "EXEC GetMonthlyTrends @UserId, @Months",
                                new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                                new Microsoft.Data.SqlClient.SqlParameter("@Months", 12))
                            .ToListAsync();

                        labels = monthlyTrends.Select(t => $"{t.Month:00}/{t.Year}").ToList();
                        incomeData = monthlyTrends.Select(t => t.Income).ToList();
                        expenseData = monthlyTrends.Select(t => t.Expenses).ToList();
                        break;

                    default:
                        return BadRequest(new { message = "Invalid period" });
                }

                // Get category spending data using stored procedure
                var categoryData = await _context.Database
                    .SqlQueryRaw<CategorySpendingDto>(
                        "EXEC GetCategorySpendingSummary @UserId, @StartDate, @EndDate",
                        new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId),
                        new Microsoft.Data.SqlClient.SqlParameter("@StartDate", startDate.Date),
                        new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endDate.Date))
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
                        labels = categoryData.Select(c => c.CategoryName).ToList(),
                        data = categoryData.Select(c => (double)c.Amount).ToList()
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
                    .Where(e => e.UserId == userId && e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                    .SumAsync(e => e.Amount);

                var lastMonthExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseDate.Month == lastMonth && e.ExpenseDate.Year == lastMonthYear)
                    .SumAsync(e => e.Amount);

                var currentMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.IncomeDate.Month == currentMonth && i.IncomeDate.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var lastMonthIncomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.IncomeDate.Month == lastMonth && i.IncomeDate.Year == lastMonthYear)
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