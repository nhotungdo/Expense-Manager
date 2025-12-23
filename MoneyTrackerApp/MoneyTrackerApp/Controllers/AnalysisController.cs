using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;
using System.Linq;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IGeminiAnalysisService _geminiService;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IBudgetService _budgetService;

        public AnalysisController(
            IGeminiAnalysisService geminiService, 
            ITransactionService transactionService, 
            ICategoryService categoryService,
            IBudgetService budgetService)
        {
            _geminiService = geminiService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _budgetService = budgetService;
        }

        private long GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) throw new UnauthorizedAccessException();
            return userId;
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetAIInsights([FromQuery] int days = 30)
        {
            try
            {
                var userId = GetUserId();
                 // Fetch recent transactions for context
                var filter = new TransactionFilterDto { 
                    PageSize = 50, 
                    PageNumber = 1, 
                    StartDate = DateTime.Now.AddDays(-days),
                    EndDate = DateTime.Now
                };
                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

                decimal totalIncome = 0;
                decimal totalExpense = 0;
                
                if (transactions != null)
                {
                    totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
                    totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
                }

                if (transactions == null || transactions.Count == 0)
                {
                     return Ok(new { 
                        content = "Chưa có đủ dữ liệu giao dịch để phân tích.",
                        totalIncome = 0,
                        totalExpense = 0,
                        netBalance = 0,
                        savingsRate = 0,
                        topCategory = "",
                        expenseTrend = 0
                     });
                }

                string insightContent = "Hệ thống đang phân tích...";
                try 
                {
                    var analysisDtos = transactions.Select(t => new TransactionAnalysisDto
                    {
                        Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                        Amount = t.Amount,
                        Category = t.CategoryName ?? "Uncategorized",
                        Type = t.TransactionType == 1 ? "Income" : t.TransactionType == 2 ? "Expense" : "Transfer",
                        Note = t.Note
                    }).ToList();

                    // Non-blocking AI call or allow failure
                    insightContent = await _geminiService.AnalyzeTransactionsAsync(analysisDtos);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail request
                    insightContent = "Dịch vụ AI hiện không khả dụng. Vui lòng thử lại sau.";
                    Console.WriteLine($"AI Error: {ex.Message}");
                }

                var netBalance = totalIncome - totalExpense;
                var savingsRate = totalIncome > 0 ? ((netBalance) / totalIncome) * 100 : 0;
                
                var topCategory = transactions
                    .Where(t => t.TransactionType == 2)
                    .GroupBy(t => t.CategoryName)
                    .OrderByDescending(g => g.Sum(t => t.Amount))
                    .Select(g => g.Key)
                    .FirstOrDefault();

                return Ok(new { 
                    content = insightContent,
                    totalIncome,
                    totalExpense,
                    netBalance,
                    savingsRate,
                    topCategory,
                    expenseTrend = 0 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("predictions")]
        public async Task<IActionResult> GetPredictions([FromQuery] int days = 7)
        {
            try 
            {
                var userId = GetUserId();
                // Get last 60 days of data for better prediction
                var filter = new TransactionFilterDto { 
                     PageSize = 1000, 
                     StartDate = DateTime.Now.AddDays(-60),
                     EndDate = DateTime.Now
                };
                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);
                
                // Filter only expenses
                var expenses = transactions.Where(t => t.TransactionType == 2).ToList();

                if (!expenses.Any()) 
                {
                     return Ok(new { labels = new string[0], values = new decimal[0] });
                }

                // Group by date
                var dailyExpenses = expenses
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderBy(x => x.Date)
                    .ToList();

                // Simple Moving Average (SMA) Prediction
                var labels = new List<string>();
                var values = new List<decimal>();
                
                var avgDaily = dailyExpenses.Any() ? dailyExpenses.Average(x => x.Total) : 0;
                
                // Create last 7 days + next 7 days projection
                // Only return next 'days' days for the chart to append
                var today = DateTime.Now;
                
                for(int i = 1; i <= days; i++)
                {
                     var futureDate = today.AddDays(i);
                     labels.Add(futureDate.ToString("dd/MM"));
                     
                     // Add some simple variance based on day of week (e.g. weekends higher)
                     var factor = (futureDate.DayOfWeek == DayOfWeek.Saturday || futureDate.DayOfWeek == DayOfWeek.Sunday) ? 1.2m : 0.9m;
                     values.Add(avgDaily * factor);
                }

                return Ok(new { labels, values });
            } 
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpGet("anomalies")]
        public async Task<IActionResult> GetAnomalies()
        {
             try
             {
                var userId = GetUserId();
                var filter = new TransactionFilterDto { 
                     PageSize = 20, 
                     StartDate = DateTime.Now.AddDays(-30)
                };
                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);
                
                // Heuristic: Expense > 2x Average of last 30 days
                
                var expenses = transactions.Where(t => t.TransactionType == 2).ToList();
                if (!expenses.Any()) return Ok(new List<object>()); // Empty

                var avg = expenses.Average(t => t.Amount);
                var threshold = avg * 2.5m; // 2.5x average
                
                // Safeguard min threshold
                if (threshold < 500000) threshold = 500000; 

                var anomalies = expenses.Where(t => t.Amount > threshold).Select(t => new {
                     t.Id,
                     t.Amount,
                     t.CategoryName,
                     t.TransactionDate,
                     IsWarning = true,
                     Message = "Chi tiêu cao bất thường so với trung bình"
                }).ToList();

                return Ok(anomalies);
             }
             catch(Exception ex)
             {
                 return StatusCode(500, new { message = ex.Message });
             }
        }

        [HttpGet("budget-status")]
        public async Task<IActionResult> GetBudgetStatus()
        {
            try
            {
                var userId = GetUserId();
                var summary = await _budgetService.GetBudgetSummaryAsync(userId);
                
                // Determine overall status
                string status = "Good";
                string message = "Bạn đang kiểm soát ngân sách tốt.";
                
                if (summary.OverBudgetCount > 0)
                {
                    status = "Danger";
                    message = $"Bạn đã vượt quá {summary.OverBudgetCount} ngân sách!";
                }
                else if (summary.NearLimitCount > 0)
                {
                    status = "Warning";
                    message = $"Có {summary.NearLimitCount} ngân sách sắp hết hạn mức.";
                }

                var percentage = summary.TotalBudgeted > 0 
                    ? (summary.TotalSpent / summary.TotalBudgeted) * 100 
                    : 0;

                return Ok(new 
                {
                    summary.TotalBudgeted,
                    summary.TotalSpent,
                    summary.TotalRemaining,
                    Percentage = percentage,
                    Status = status,
                    Message = message,
                    OverBudgetCount = summary.OverBudgetCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
