using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TradingController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;

        public TradingController(
            ITransactionService transactionService,
            ICategoryService categoryService)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        [HttpGet("analysis")]
        public async Task<ActionResult> GetAnalysis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserId();
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;

                var filter = new TransactionFilterDto
                {
                    StartDate = start,
                    EndDate = end
                };

                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

                var income = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
                var expense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
                var balance = income - expense;

                // Calculate trends (mock for now)
                var incomeTrend = 12.5;
                var expenseTrend = -8.3;
                var balanceTrend = 15.2;

                return Ok(new
                {
                    income,
                    expense,
                    balance,
                    trends = new
                    {
                        income = incomeTrend,
                        expense = expenseTrend,
                        balance = balanceTrend
                    },
                    transactionCount = transactions.Count,
                    period = new { start, end }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving analysis", details = ex.Message });
            }
        }

        [HttpGet("category-breakdown")]
        public async Task<ActionResult> GetCategoryBreakdown([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserId();
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;

                var filter = new TransactionFilterDto
                {
                    StartDate = start,
                    EndDate = end,
                    TransactionType = 2 // Expense only
                };

                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

                var categoryBreakdown = transactions
                    .GroupBy(t => new { t.CategoryId, t.CategoryName })
                    .Select(g => new
                    {
                        categoryId = g.Key.CategoryId,
                        categoryName = g.Key.CategoryName ?? "Khác",
                        amount = g.Sum(t => t.Amount),
                        count = g.Count(),
                        percentage = 0.0 // Will calculate below
                    })
                    .OrderByDescending(c => c.amount)
                    .ToList();

                var totalExpense = categoryBreakdown.Sum(c => c.amount);
                
                var result = categoryBreakdown.Select(c => new
                {
                    c.categoryId,
                    c.categoryName,
                    c.amount,
                    c.count,
                    percentage = totalExpense > 0 ? (c.amount / totalExpense * 100) : 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving category breakdown", details = ex.Message });
            }
        }

        [HttpGet("trends")]
        public async Task<ActionResult> GetTrends([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string period = "daily")
        {
            try
            {
                var userId = GetUserId();
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;

                var filter = new TransactionFilterDto
                {
                    StartDate = start,
                    EndDate = end
                };

                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

                // Group transactions based on period
                var trendData = new List<object>();
                
                if (period.ToLower() == "daily")
                {
                    var grouped = transactions.GroupBy(t => t.TransactionDate.Date);
                    trendData = grouped.Select(g => new
                    {
                        period = FormatPeriod(g.Key, period),
                        income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                        expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                        count = g.Count()
                    }).OrderBy(t => t.period).Cast<object>().ToList();
                }
                else if (period.ToLower() == "weekly")
                {
                    var grouped = transactions.GroupBy(t => GetWeekNumber(t.TransactionDate));
                    trendData = grouped.Select(g => new
                    {
                        period = FormatPeriod(g.Key, period),
                        income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                        expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                        count = g.Count()
                    }).OrderBy(t => t.period).Cast<object>().ToList();
                }
                else if (period.ToLower() == "monthly")
                {
                    var grouped = transactions.GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month });
                    trendData = grouped.Select(g => new
                    {
                        period = FormatPeriod(g.Key, period),
                        income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                        expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                        count = g.Count()
                    }).OrderBy(t => t.period).Cast<object>().ToList();
                }
                else
                {
                    var grouped = transactions.GroupBy(t => t.TransactionDate.Date);
                    trendData = grouped.Select(g => new
                    {
                        period = FormatPeriod(g.Key, period),
                        income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                        expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                        count = g.Count()
                    }).OrderBy(t => t.period).Cast<object>().ToList();
                }

                return Ok(trendData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving trends", details = ex.Message });
            }
        }

        [HttpGet("ai-insights")]
        public async Task<ActionResult> GetAIInsights([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserId();
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;

                var filter = new TransactionFilterDto
                {
                    StartDate = start,
                    EndDate = end
                };

                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

                var income = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
                var expense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
                var balance = income - expense;

                var insights = new List<object>();

                // High spending warning
                if (income > 0 && expense > income * 0.8m)
                {
                    insights.Add(new
                    {
                        type = "warning",
                        title = "⚠️ Chi tiêu cao",
                        message = $"Chi tiêu của bạn đang chiếm {(expense / income * 100):F1}% thu nhập. Hãy xem xét cắt giảm một số khoản chi không cần thiết.",
                        priority = "high"
                    });
                }

                // Good savings
                if (income > 0 && balance / income > 0.3m)
                {
                    insights.Add(new
                    {
                        type = "success",
                        title = "✨ Tiết kiệm tốt",
                        message = $"Bạn đang tiết kiệm được {(balance / income * 100):F1}% thu nhập. Đây là một thói quen tài chính tuyệt vời!",
                        priority = "medium"
                    });
                }

                // Top spending category
                var topCategory = transactions
                    .Where(t => t.TransactionType == 2)
                    .GroupBy(t => t.CategoryName)
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .FirstOrDefault();

                if (topCategory != null)
                {
                    var categoryAmount = topCategory.Sum(t => t.Amount);
                    var categoryPercentage = expense > 0 ? (categoryAmount / expense * 100) : 0;
                    
                    insights.Add(new
                    {
                        type = "info",
                        title = "📊 Chi tiêu nhiều nhất",
                        message = $"Bạn chi tiêu nhiều nhất cho '{topCategory.Key}' với {categoryPercentage:F1}% tổng chi tiêu.",
                        priority = "medium"
                    });
                }

                // Budget suggestion
                insights.Add(new
                {
                    type = "tip",
                    title = "💡 Gợi ý",
                    message = "Hãy thiết lập ngân sách cho từng danh mục để kiểm soát chi tiêu tốt hơn.",
                    priority = "low"
                });

                // Savings goal suggestion
                if (balance > 0 && income > 0)
                {
                    var suggestedSavings = income * 0.2m;
                    insights.Add(new
                    {
                        type = "tip",
                        title = "🎯 Mục tiêu tiết kiệm",
                        message = $"Với thu nhập hiện tại, bạn nên cố gắng tiết kiệm ít nhất {suggestedSavings:N0} VND mỗi tháng (20% thu nhập).",
                        priority = "low"
                    });
                }

                return Ok(insights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating insights", details = ex.Message });
            }
        }

        [HttpGet("comparison")]
        public async Task<ActionResult> GetComparison([FromQuery] string type = "previous")
        {
            try
            {
                var userId = GetUserId();
                var now = DateTime.Now;

                // Current period
                var currentStart = new DateTime(now.Year, now.Month, 1);
                var currentEnd = currentStart.AddMonths(1).AddDays(-1);

                // Comparison period
                DateTime comparisonStart, comparisonEnd;
                
                switch (type.ToLower())
                {
                    case "lastyear":
                        comparisonStart = currentStart.AddYears(-1);
                        comparisonEnd = currentEnd.AddYears(-1);
                        break;
                    case "average":
                        comparisonStart = currentStart.AddMonths(-3);
                        comparisonEnd = currentStart.AddDays(-1);
                        break;
                    default: // previous
                        comparisonStart = currentStart.AddMonths(-1);
                        comparisonEnd = currentStart.AddDays(-1);
                        break;
                }

                var currentTransactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
                {
                    StartDate = currentStart,
                    EndDate = currentEnd
                });

                var comparisonTransactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
                {
                    StartDate = comparisonStart,
                    EndDate = comparisonEnd
                });

                var currentIncome = currentTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
                var currentExpense = currentTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
                
                var comparisonIncome = comparisonTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
                var comparisonExpense = comparisonTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);

                // For average, divide by 3
                if (type.ToLower() == "average")
                {
                    comparisonIncome /= 3;
                    comparisonExpense /= 3;
                }

                return Ok(new
                {
                    current = new
                    {
                        income = currentIncome,
                        expense = currentExpense,
                        balance = currentIncome - currentExpense,
                        period = new { start = currentStart, end = currentEnd }
                    },
                    comparison = new
                    {
                        income = comparisonIncome,
                        expense = comparisonExpense,
                        balance = comparisonIncome - comparisonExpense,
                        period = new { start = comparisonStart, end = comparisonEnd }
                    },
                    changes = new
                    {
                        income = comparisonIncome > 0 ? ((currentIncome - comparisonIncome) / comparisonIncome * 100) : 0,
                        expense = comparisonExpense > 0 ? ((currentExpense - comparisonExpense) / comparisonExpense * 100) : 0,
                        balance = (currentIncome - currentExpense) - (comparisonIncome - comparisonExpense)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error performing comparison", details = ex.Message });
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            var firstDayOfYear = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)firstDayOfYear.DayOfWeek;
            var firstWeekDay = firstDayOfYear.AddDays(-daysOffset);
            var weekNumber = ((date - firstWeekDay).Days / 7) + 1;
            return weekNumber;
        }

        private string FormatPeriod(object key, string period)
        {
            return period.ToLower() switch
            {
                "daily" => ((DateTime)key).ToString("dd/MM"),
                "weekly" => $"Tuần {key}",
                "monthly" => $"{((dynamic)key).Month}/{((dynamic)key).Year}",
                _ => key.ToString()
            };
        }
    }
}
