using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Get total income and expenses
            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.Amount);

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.Amount);

            // Get monthly income and expenses
            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == currentMonth &&
                           i.IncomeDate.Year == currentYear)
                .SumAsync(i => i.Amount);

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == currentMonth &&
                           e.ExpenseDate.Year == currentYear)
                .SumAsync(e => e.Amount);

            // Get expenses by category
            var expensesByCategory = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Amount);

            // Get income by category
            var incomeByCategory = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .GroupBy(i => i.Category != null ? i.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Amount = g.Sum(i => i.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Amount);

            // Get monthly trends (last 6 months)
            var monthlyTrends = new List<MonthlyTrend>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var monthIncome = await _context.Incomes
                    .Where(inc => inc.UserId == userId &&
                                 inc.IncomeDate.Month == date.Month &&
                                 inc.IncomeDate.Year == date.Year)
                    .SumAsync(inc => inc.Amount);

                var monthExpenses = await _context.Expenses
                    .Where(exp => exp.UserId == userId &&
                                 exp.ExpenseDate.Month == date.Month &&
                                 exp.ExpenseDate.Year == date.Year)
                    .SumAsync(exp => exp.Amount);

                monthlyTrends.Add(new MonthlyTrend
                {
                    Year = date.Year,
                    Month = date.Month,
                    Income = monthIncome,
                    Expenses = monthExpenses,
                    Savings = monthIncome - monthExpenses
                });
            }

            // Get recent transactions
            var recentExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new RecentTransaction
                {
                    Id = e.Id,
                    Type = "Expense",
                    Amount = e.Amount,
                    Category = e.Category != null ? e.Category.Name : "Uncategorized",
                    Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    Note = e.Note
                })
                .ToListAsync();

            var recentIncomes = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new RecentTransaction
                {
                    Id = i.Id,
                    Type = "Income",
                    Amount = i.Amount,
                    Category = i.Category != null ? i.Category.Name : "Uncategorized",
                    Date = i.IncomeDate.ToDateTime(TimeOnly.MinValue),
                    Note = i.Note
                })
                .ToListAsync();

            var recentTransactions = recentExpenses
                .Concat(recentIncomes)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            // Get AI suggestions
            var aiSuggestions = await _context.AiSuggestions
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var dashboardDto = new DashboardDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetWorth = totalIncome - totalExpenses,
                MonthlyIncome = monthlyIncome,
                MonthlyExpenses = monthlyExpenses,
                MonthlySavings = monthlyIncome - monthlyExpenses,
                ExpensesByCategory = expensesByCategory,
                IncomeByCategory = incomeByCategory,
                MonthlyTrends = monthlyTrends,
                RecentTransactions = recentTransactions,
                AiSuggestions = aiSuggestions
            };

            return Ok(dashboardDto);
        }

        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var targetMonth = month ?? DateTime.UtcNow.Month;
            var targetYear = year ?? DateTime.UtcNow.Year;

            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == targetMonth &&
                           i.IncomeDate.Year == targetYear)
                .Include(i => i.Category)
                .ToListAsync();

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == targetMonth &&
                           e.ExpenseDate.Year == targetYear)
                .Include(e => e.Category)
                .ToListAsync();

            var report = new
            {
                Month = targetMonth,
                Year = targetYear,
                TotalIncome = monthlyIncome.Sum(i => i.Amount),
                TotalExpenses = monthlyExpenses.Sum(e => e.Amount),
                NetSavings = monthlyIncome.Sum(i => i.Amount) - monthlyExpenses.Sum(e => e.Amount),
                IncomeByCategory = monthlyIncome
                    .GroupBy(i => i.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount)),
                ExpensesByCategory = monthlyExpenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                IncomeTransactions = monthlyIncome.Count,
                ExpenseTransactions = monthlyExpenses.Count
            };

            return Ok(report);
        }

        [HttpGet("budget-analysis")]
        public async Task<IActionResult> GetBudgetAnalysis()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == currentMonth &&
                           i.IncomeDate.Year == currentYear)
                .SumAsync(i => i.Amount);

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == currentMonth &&
                           e.ExpenseDate.Year == currentYear)
                .SumAsync(e => e.Amount);

            var expenseRatio = monthlyIncome > 0 ? (monthlyExpenses / monthlyIncome) * 100 : 0;
            var savingsRate = monthlyIncome > 0 ? ((monthlyIncome - monthlyExpenses) / monthlyIncome) * 100 : 0;

            var analysis = new
            {
                MonthlyIncome = monthlyIncome,
                MonthlyExpenses = monthlyExpenses,
                ExpenseRatio = Math.Round(expenseRatio, 2),
                SavingsRate = Math.Round(savingsRate, 2),
                BudgetStatus = expenseRatio switch
                {
                    > 90 => "Critical",
                    > 80 => "Warning",
                    > 70 => "Caution",
                    _ => "Good"
                },
                Recommendations = GetBudgetRecommendations(expenseRatio, savingsRate)
            };

            return Ok(analysis);
        }

        [HttpGet("spending-trends")]
        public async Task<IActionResult> GetSpendingTrends([FromQuery] int months = 6)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var startDate = DateTime.UtcNow.AddMonths(-months);
            var endDate = DateTime.UtcNow;

            var trends = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= DateOnly.FromDateTime(startDate) &&
                           e.ExpenseDate <= DateOnly.FromDateTime(endDate))
                .Include(e => e.Category)
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalAmount = g.Sum(e => e.Amount),
                    TransactionCount = g.Count(),
                    Categories = g.GroupBy(e => e.Category!.Name)
                        .Select(cg => new
                        {
                            Category = cg.Key,
                            Amount = cg.Sum(e => e.Amount),
                            Percentage = 0.0 // Will be calculated on client side
                        })
                        .OrderByDescending(c => c.Amount)
                        .Take(5)
                        .ToList()
                })
                .OrderBy(t => t.Month)
                .ToListAsync();

            return Ok(trends);
        }

        [HttpPost("generate-ai-suggestion")]
        public async Task<IActionResult> GenerateAiSuggestion()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Simple AI suggestion logic based on spending patterns
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == currentMonth &&
                           e.ExpenseDate.Year == currentYear)
                .Include(e => e.Category)
                .ToListAsync();

            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == currentMonth &&
                           i.IncomeDate.Year == currentYear)
                .SumAsync(i => i.Amount);

            var totalMonthlyExpenses = monthlyExpenses.Sum(e => e.Amount);
            var suggestions = new List<string>();

            // Generate suggestions based on spending patterns
            if (totalMonthlyExpenses > monthlyIncome * 0.9m)
            {
                suggestions.Add("Cảnh báo: Chi tiêu tháng này đã vượt quá 90% thu nhập. Hãy cân nhắc cắt giảm chi tiêu không cần thiết.");
            }

            var expensesByCategory = monthlyExpenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            foreach (var category in expensesByCategory)
            {
                var percentage = (category.Value / totalMonthlyExpenses) * 100;
                if (percentage > 30)
                {
                    suggestions.Add($"Chi tiêu cho '{category.Key}' chiếm {percentage:F1}% tổng chi tiêu. Hãy xem xét có thể tiết kiệm ở danh mục này.");
                }
            }

            if (monthlyIncome > totalMonthlyExpenses)
            {
                var savings = monthlyIncome - totalMonthlyExpenses;
                var savingsRate = (savings / monthlyIncome) * 100;
                if (savingsRate > 20)
                {
                    suggestions.Add($"Tuyệt vời! Bạn đã tiết kiệm được {savingsRate:F1}% thu nhập tháng này. Hãy xem xét đầu tư số tiền này.");
                }
                else
                {
                    suggestions.Add($"Bạn đã tiết kiệm được {savingsRate:F1}% thu nhập. Cố gắng tăng tỷ lệ tiết kiệm lên 20% để có tương lai tài chính tốt hơn.");
                }
            }

            if (suggestions.Count == 0)
            {
                suggestions.Add("Hãy tiếp tục theo dõi chi tiêu và thu nhập để có cái nhìn tổng quan về tình hình tài chính của bạn.");
            }

            // Save AI suggestion to database
            var aiSuggestion = new AiSuggestion
            {
                UserId = userId.Value,
                Suggestion = string.Join(" ", suggestions),
                CreatedAt = DateTime.UtcNow
            };

            _context.AiSuggestions.Add(aiSuggestion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated AI suggestion for user {UserId}", userId);

            return Ok(new { suggestion = aiSuggestion.Suggestion });
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private List<string> GetBudgetRecommendations(decimal expenseRatio, decimal savingsRate)
        {
            var recommendations = new List<string>();

            if (expenseRatio > 90)
            {
                recommendations.Add("Cảnh báo: Chi tiêu đã vượt quá 90% thu nhập. Cần cắt giảm chi tiêu ngay lập tức.");
                recommendations.Add("Hãy xem xét các khoản chi tiêu không cần thiết và tạm dừng các mua sắm lớn.");
            }
            else if (expenseRatio > 80)
            {
                recommendations.Add("Cảnh báo: Chi tiêu đã vượt quá 80% thu nhập. Cần kiểm soát chi tiêu tốt hơn.");
                recommendations.Add("Hãy lập danh sách ưu tiên cho các khoản chi tiêu và cắt giảm những thứ không quan trọng.");
            }
            else if (expenseRatio > 70)
            {
                recommendations.Add("Chú ý: Chi tiêu đã vượt quá 70% thu nhập. Cần theo dõi chi tiêu cẩn thận hơn.");
                recommendations.Add("Hãy đặt mục tiêu tiết kiệm ít nhất 20% thu nhập mỗi tháng.");
            }
            else
            {
                recommendations.Add("Tuyệt vời! Bạn đang quản lý chi tiêu tốt.");
                if (savingsRate > 20)
                {
                    recommendations.Add("Tỷ lệ tiết kiệm của bạn rất tốt. Hãy xem xét đầu tư số tiền tiết kiệm này.");
                }
                else if (savingsRate > 10)
                {
                    recommendations.Add("Tỷ lệ tiết kiệm khá tốt. Hãy cố gắng tăng lên 20% để có tương lai tài chính vững chắc.");
                }
                else
                {
                    recommendations.Add("Hãy cố gắng tăng tỷ lệ tiết kiệm lên ít nhất 10-20% thu nhập.");
                }
            }

            return recommendations;
        }
    }
}
