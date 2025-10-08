using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<DashboardService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IExpenseService _expenseService;
        private readonly IIncomeService _incomeService;
        private readonly IAISuggestionService _aiSuggestionService;

        public DashboardService(
            ExpenseManagerContext context,
            ILogger<DashboardService> logger,
            IMemoryCache cache,
            IExpenseService expenseService,
            IIncomeService incomeService,
            IAISuggestionService aiSuggestionService)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _expenseService = expenseService;
            _incomeService = incomeService;
            _aiSuggestionService = aiSuggestionService;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(long userId)
        {
            var cacheKey = $"dashboard_{userId}_{DateTime.UtcNow:yyyyMMddHH}";

            if (_cache.TryGetValue(cacheKey, out DashboardDto? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Get total income and expenses
            var totalIncome = await _incomeService.GetTotalIncomeAsync(userId);
            var totalExpenses = await _expenseService.GetTotalExpensesAsync(userId);

            // Get monthly income and expenses
            var monthStart = new DateTime(currentYear, currentMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyIncome = await _incomeService.GetTotalIncomeAsync(userId, monthStart, monthEnd);
            var monthlyExpenses = await _expenseService.GetTotalExpensesAsync(userId, monthStart, monthEnd);

            // Get expenses by category
            var expensesByCategory = await _expenseService.GetExpensesByCategorySummaryAsync(userId);

            // Get income by category
            var incomeByCategory = await _incomeService.GetIncomeByCategorySummaryAsync(userId);

            // Get monthly trends (last 6 months)
            var monthlyTrends = new List<MonthlyTrendDto>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var trendStart = new DateTime(date.Year, date.Month, 1);
                var trendEnd = trendStart.AddMonths(1).AddDays(-1);

                var monthIncome = await _incomeService.GetTotalIncomeAsync(userId, trendStart, trendEnd);
                var monthExpenses = await _expenseService.GetTotalExpensesAsync(userId, trendStart, trendEnd);

                monthlyTrends.Add(new MonthlyTrendDto
                {
                    Year = date.Year,
                    Month = date.Month,
                    Income = monthIncome,
                    Expenses = monthExpenses,
                    Savings = monthIncome - monthExpenses,
                    NetWorth = monthIncome - monthExpenses
                });
            }

            // Get recent transactions
            var recentExpenses = await _expenseService.GetRecentExpensesAsync(userId, 5);
            var recentIncomes = await _incomeService.GetRecentIncomesAsync(userId, 5);

            var recentTransactions = recentExpenses
                .Select(e => new Models.DTOs.RecentTransaction
                {
                    Id = e.Id,
                    Type = "Expense",
                    Amount = e.Amount,
                    Category = e.Category?.Name ?? "Uncategorized",
                    Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    Note = e.Note
                })
                .Concat(recentIncomes.Select(i => new Models.DTOs.RecentTransaction
                {
                    Id = i.Id,
                    Type = "Income",
                    Amount = i.Amount,
                    Category = i.Category?.Name ?? "Uncategorized",
                    Date = i.IncomeDate.ToDateTime(TimeOnly.MinValue),
                    Note = i.Note
                }))
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            // Get AI suggestions
            var aiSuggestions = await _aiSuggestionService.GetSuggestionsAsync(userId, 0, 5);

            var dashboardDto = new DashboardDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetWorth = totalIncome - totalExpenses,
                MonthlyIncome = monthlyIncome,
                MonthlyExpenses = monthlyExpenses,
                MonthlySavings = monthlyIncome - monthlyExpenses,
                ExpensesByCategory = expensesByCategory.Select(kvp => new CategorySpendingDto
                {
                    CategoryName = kvp.Key,
                    Amount = kvp.Value,
                    Percentage = 0, // Will be calculated on client side
                    TransactionCount = 0, // Will be calculated separately if needed
                    AverageAmount = 0 // Will be calculated separately if needed
                }).ToList(),
                IncomeByCategory = incomeByCategory.Select(kvp => new CategorySpendingDto
                {
                    CategoryName = kvp.Key,
                    Amount = kvp.Value,
                    Percentage = 0, // Will be calculated on client side
                    TransactionCount = 0, // Will be calculated separately if needed
                    AverageAmount = 0 // Will be calculated separately if needed
                }).ToList(),
                MonthlyTrends = monthlyTrends,
                RecentTransactions = recentTransactions,
                AiSuggestions = aiSuggestions.ToList()
            };

            // Cache for 1 hour
            _cache.Set(cacheKey, dashboardDto, TimeSpan.FromHours(1));

            return dashboardDto;
        }

        public async Task<object> GetMonthlyReportAsync(long userId, int? month = null, int? year = null)
        {
            var targetMonth = month ?? DateTime.UtcNow.Month;
            var targetYear = year ?? DateTime.UtcNow.Year;

            var monthStart = new DateTime(targetYear, targetMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyIncome = await _incomeService.GetTotalIncomeAsync(userId, monthStart, monthEnd);
            var monthlyExpenses = await _expenseService.GetTotalExpensesAsync(userId, monthStart, monthEnd);

            var incomeByCategory = await _incomeService.GetIncomeByCategorySummaryAsync(userId, monthStart, monthEnd);
            var expensesByCategory = await _expenseService.GetExpensesByCategorySummaryAsync(userId, monthStart, monthEnd);

            var incomeTransactions = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == targetMonth &&
                           i.IncomeDate.Year == targetYear)
                .CountAsync();

            var expenseTransactions = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == targetMonth &&
                           e.ExpenseDate.Year == targetYear)
                .CountAsync();

            return new
            {
                Month = targetMonth,
                Year = targetYear,
                TotalIncome = monthlyIncome,
                TotalExpenses = monthlyExpenses,
                NetSavings = monthlyIncome - monthlyExpenses,
                IncomeByCategory = incomeByCategory,
                ExpensesByCategory = expensesByCategory,
                IncomeTransactions = incomeTransactions,
                ExpenseTransactions = expenseTransactions
            };
        }

        public async Task<object> GetBudgetAnalysisAsync(long userId)
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthStart = new DateTime(currentYear, currentMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyIncome = await _incomeService.GetTotalIncomeAsync(userId, monthStart, monthEnd);
            var monthlyExpenses = await _expenseService.GetTotalExpensesAsync(userId, monthStart, monthEnd);

            var expenseRatio = monthlyIncome > 0 ? (monthlyExpenses / monthlyIncome) * 100 : 0;
            var savingsRate = monthlyIncome > 0 ? ((monthlyIncome - monthlyExpenses) / monthlyIncome) * 100 : 0;

            var recommendations = await _aiSuggestionService.GenerateBudgetRecommendationsAsync(userId);

            return new
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
                Recommendations = recommendations
            };
        }

        public async Task<object> GetSpendingTrendsAsync(long userId, int months = 6)
        {
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

            return trends;
        }

        public async Task<object> GetCategoryBreakdownAsync(long userId, string type = "expense")
        {
            if (type.ToLower() == "expense")
            {
                return await _expenseService.GetExpensesByCategorySummaryAsync(userId);
            }
            else
            {
                return await _incomeService.GetIncomeByCategorySummaryAsync(userId);
            }
        }

        public async Task<object> GetRecentActivityAsync(long userId, int count = 10)
        {
            var recentExpenses = await _expenseService.GetRecentExpensesAsync(userId, count);
            var recentIncomes = await _incomeService.GetRecentIncomesAsync(userId, count);

            var activities = recentExpenses
                .Select(e => new
                {
                    Id = e.Id,
                    Type = "Expense",
                    Amount = e.Amount,
                    Category = e.Category?.Name ?? "Uncategorized",
                    Date = e.CreatedAt,
                    Note = e.Note
                })
                .Concat(recentIncomes.Select(i => new
                {
                    Id = i.Id,
                    Type = "Income",
                    Amount = i.Amount,
                    Category = i.Category?.Name ?? "Uncategorized",
                    Date = i.CreatedAt,
                    Note = i.Note
                }))
                .OrderByDescending(a => a.Date)
                .Take(count)
                .ToList();

            return activities;
        }

        public async Task<AiSuggestion> GenerateAiSuggestionAsync(long userId)
        {
            return await _aiSuggestionService.GenerateSuggestionAsync(userId);
        }
    }
}
