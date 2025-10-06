using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class AdvancedAnalyticsService : IAdvancedAnalyticsService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<AdvancedAnalyticsService> _logger;

        public AdvancedAnalyticsService(ExpenseManagerContext context, ILogger<AdvancedAnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SpendingAnalysisDto> GetSpendingAnalysisAsync(long userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = DateOnly.FromDateTime(startDate);
                var endDateOnly = DateOnly.FromDateTime(endDate);

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= startDateOnly &&
                               e.ExpenseDate <= endDateOnly)
                    .Include(e => e.Category)
                    .ToListAsync();

                var totalSpent = expenses.Sum(e => e.Amount);
                var days = (endDate - startDate).Days + 1;

                var analysis = new SpendingAnalysisDto
                {
                    TotalSpent = totalSpent,
                    AverageDailySpending = days > 0 ? totalSpent / days : 0,
                    AverageMonthlySpending = totalSpent * 30 / days,
                    HighestSpendingDay = expenses.GroupBy(e => e.ExpenseDate)
                        .Max(g => g.Sum(e => e.Amount)),
                    HighestSpendingDate = expenses.GroupBy(e => e.ExpenseDate)
                        .OrderByDescending(g => g.Sum(e => e.Amount))
                        .FirstOrDefault()?.Key.ToDateTime(TimeOnly.MinValue)
                };

                // Top categories
                analysis.TopCategories = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .Select(g => new CategorySpendingDto
                    {
                        CategoryName = g.Key,
                        Amount = g.Sum(e => e.Amount),
                        Percentage = totalSpent > 0 ? (g.Sum(e => e.Amount) / totalSpent) * 100 : 0,
                        TransactionCount = g.Count(),
                        AverageAmount = g.Average(e => e.Amount)
                    })
                    .OrderByDescending(c => c.Amount)
                    .Take(10)
                    .ToList();

                // Daily spending
                analysis.DailySpending = expenses
                    .GroupBy(e => e.ExpenseDate)
                    .Select(g => new DailySpendingDto
                    {
                        Date = g.Key.ToDateTime(TimeOnly.MinValue),
                        Amount = g.Sum(e => e.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Weekly spending
                analysis.WeeklySpending = expenses
                    .GroupBy(e => new
                    {
                        Week = GetWeekOfYear(e.ExpenseDate.ToDateTime(TimeOnly.MinValue)),
                        Year = e.ExpenseDate.Year
                    })
                    .Select(g => new WeeklySpendingDto
                    {
                        Week = g.Key.Week,
                        Year = g.Key.Year,
                        Amount = g.Sum(e => e.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(w => w.Year).ThenBy(w => w.Week)
                    .ToList();

                // Spending patterns
                analysis.SpendingPattern = AnalyzeSpendingPatterns(expenses);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing spending for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IncomeAnalysisDto> GetIncomeAnalysisAsync(long userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = DateOnly.FromDateTime(startDate);
                var endDateOnly = DateOnly.FromDateTime(endDate);

                var incomes = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate >= startDateOnly &&
                               i.IncomeDate <= endDateOnly)
                    .Include(i => i.Category)
                    .ToListAsync();

                var totalIncome = incomes.Sum(i => i.Amount);
                var days = (endDate - startDate).Days + 1;

                var analysis = new IncomeAnalysisDto
                {
                    TotalIncome = totalIncome,
                    AverageDailyIncome = days > 0 ? totalIncome / days : 0,
                    AverageMonthlyIncome = totalIncome * 30 / days,
                    HighestIncomeDay = incomes.GroupBy(i => i.IncomeDate)
                        .Max(g => g.Sum(i => i.Amount)),
                    HighestIncomeDate = incomes.GroupBy(i => i.IncomeDate)
                        .OrderByDescending(g => g.Sum(i => i.Amount))
                        .FirstOrDefault()?.Key.ToDateTime(TimeOnly.MinValue)
                };

                // Top categories
                analysis.TopCategories = incomes
                    .GroupBy(i => i.Category?.Name ?? "Uncategorized")
                    .Select(g => new CategoryIncomeDto
                    {
                        CategoryName = g.Key,
                        Amount = g.Sum(i => i.Amount),
                        Percentage = totalIncome > 0 ? (g.Sum(i => i.Amount) / totalIncome) * 100 : 0,
                        TransactionCount = g.Count(),
                        AverageAmount = g.Average(i => i.Amount)
                    })
                    .OrderByDescending(c => c.Amount)
                    .Take(10)
                    .ToList();

                // Daily income
                analysis.DailyIncome = incomes
                    .GroupBy(i => i.IncomeDate)
                    .Select(g => new DailyIncomeDto
                    {
                        Date = g.Key.ToDateTime(TimeOnly.MinValue),
                        Amount = g.Sum(i => i.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Weekly income
                analysis.WeeklyIncome = incomes
                    .GroupBy(i => new
                    {
                        Week = GetWeekOfYear(i.IncomeDate.ToDateTime(TimeOnly.MinValue)),
                        Year = i.IncomeDate.Year
                    })
                    .Select(g => new WeeklyIncomeDto
                    {
                        Week = g.Key.Week,
                        Year = g.Key.Year,
                        Amount = g.Sum(i => i.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(w => w.Year).ThenBy(w => w.Week)
                    .ToList();

                // Income patterns
                analysis.IncomePattern = AnalyzeIncomePatterns(incomes);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing income for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BudgetAnalysisDto> GetBudgetAnalysisAsync(long userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = DateOnly.FromDateTime(startDate);
                var endDateOnly = DateOnly.FromDateTime(endDate);

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= startDateOnly &&
                               e.ExpenseDate <= endDateOnly)
                    .Include(e => e.Category)
                    .ToListAsync();

                var totalSpent = expenses.Sum(e => e.Amount);

                // For demo purposes, we'll create a simple budget based on historical data
                var historicalMonthlyAverage = await GetHistoricalMonthlyAverage(userId);
                var totalBudget = historicalMonthlyAverage * 1.1m; // 10% buffer

                var analysis = new BudgetAnalysisDto
                {
                    TotalBudget = totalBudget,
                    TotalSpent = totalSpent,
                    RemainingBudget = totalBudget - totalSpent,
                    BudgetUtilization = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0,
                    BudgetStatus = GetBudgetStatus(totalSpent, totalBudget)
                };

                // Category budgets
                analysis.CategoryBudgets = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .Select(g => new CategoryBudgetDto
                    {
                        CategoryName = g.Key,
                        BudgetAmount = g.Sum(e => e.Amount) * 1.1m, // Simple budget calculation
                        SpentAmount = g.Sum(e => e.Amount),
                        RemainingAmount = g.Sum(e => e.Amount) * 0.1m,
                        UtilizationPercentage = 90, // Simplified
                        Status = "Good"
                    })
                    .ToList();

                // Budget alerts
                analysis.Alerts = GenerateBudgetAlerts(analysis);

                // Recommendations
                analysis.Recommendations = GenerateBudgetRecommendations(analysis);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing budget for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FinancialHealthDto> GetFinancialHealthAsync(long userId)
        {
            try
            {
                var totalIncome = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .SumAsync(i => i.Amount);

                var totalExpenses = await _context.Expenses
                    .Where(i => i.UserId == userId)
                    .SumAsync(i => i.Amount);

                var netWorth = totalIncome - totalExpenses;
                var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0;

                var health = new FinancialHealthDto
                {
                    NetWorth = netWorth,
                    SavingsRate = savingsRate,
                    DebtToIncomeRatio = 0, // Simplified - would need debt data
                    HealthScore = CalculateHealthScore(netWorth, savingsRate),
                    HealthStatus = GetHealthStatus(savingsRate)
                };

                // Health metrics
                health.Metrics = new List<HealthMetricDto>
                {
                    new() { Name = "Savings Rate", Value = savingsRate, Unit = "%", Status = GetMetricStatus(savingsRate, 20), Description = "Percentage of income saved" },
                    new() { Name = "Net Worth", Value = netWorth, Unit = "VND", Status = netWorth > 0 ? "Good" : "Poor", Description = "Total assets minus liabilities" },
                    new() { Name = "Monthly Income", Value = totalIncome, Unit = "VND", Status = "Good", Description = "Total monthly income" },
                    new() { Name = "Monthly Expenses", Value = totalExpenses, Unit = "VND", Status = GetMetricStatus(totalExpenses, totalIncome * 0.8m), Description = "Total monthly expenses" }
                };

                // Recommendations
                health.Recommendations = GenerateHealthRecommendations(health);

                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing financial health for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TrendAnalysisDto> GetTrendAnalysisAsync(long userId, int months = 12)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                var monthlyTrends = new List<MonthlyTrendDto>();

                for (int i = 0; i < months; i++)
                {
                    var month = endDate.AddMonths(-i);
                    var monthStart = new DateTime(month.Year, month.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var monthIncome = await _context.Incomes
                        .Where(inc => inc.UserId == userId &&
                                     inc.IncomeDate >= DateOnly.FromDateTime(monthStart) &&
                                     inc.IncomeDate <= DateOnly.FromDateTime(monthEnd))
                        .SumAsync(inc => inc.Amount);

                    var monthExpenses = await _context.Expenses
                        .Where(exp => exp.UserId == userId &&
                                     exp.ExpenseDate >= DateOnly.FromDateTime(monthStart) &&
                                     exp.ExpenseDate <= DateOnly.FromDateTime(monthEnd))
                        .SumAsync(exp => exp.Amount);

                    monthlyTrends.Add(new MonthlyTrendDto
                    {
                        Year = month.Year,
                        Month = month.Month,
                        Income = monthIncome,
                        Expenses = monthExpenses,
                        Savings = monthIncome - monthExpenses,
                        NetWorth = monthIncome - monthExpenses // Simplified
                    });
                }

                monthlyTrends = monthlyTrends.OrderBy(t => t.Year).ThenBy(t => t.Month).ToList();

                var analysis = new TrendAnalysisDto
                {
                    MonthlyTrends = monthlyTrends,
                    IncomeTrend = CalculateTrend(monthlyTrends.Select(t => t.Income).ToList()),
                    ExpenseTrend = CalculateTrend(monthlyTrends.Select(t => t.Expenses).ToList()),
                    SavingsTrend = CalculateTrend(monthlyTrends.Select(t => t.Savings).ToList())
                };

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing trends for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CategoryInsightsDto> GetCategoryInsightsAsync(long userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = DateOnly.FromDateTime(startDate);
                var endDateOnly = DateOnly.FromDateTime(endDate);

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate >= startDateOnly &&
                               e.ExpenseDate <= endDateOnly)
                    .Include(e => e.Category)
                    .ToListAsync();

                var insights = new CategoryInsightsDto();

                // Generate insights for each category
                var categoryGroups = expenses.GroupBy(e => e.Category?.Name ?? "Uncategorized");

                foreach (var group in categoryGroups)
                {
                    var categoryName = group.Key;
                    var amounts = group.Select(e => e.Amount).ToList();
                    var average = amounts.Average();
                    var max = amounts.Max();
                    var min = amounts.Min();

                    // Spending pattern insight
                    if (max > average * 2)
                    {
                        insights.Insights.Add(new CategoryInsightDto
                        {
                            CategoryName = categoryName,
                            Insight = $"High variance in spending - highest transaction was {max:C0}",
                            Value = max,
                            Type = "Variance"
                        });
                    }

                    // Frequency insight
                    var frequency = amounts.Count;
                    if (frequency > 10)
                    {
                        insights.Insights.Add(new CategoryInsightDto
                        {
                            CategoryName = categoryName,
                            Insight = $"Frequent spending - {frequency} transactions this period",
                            Value = frequency,
                            Type = "Frequency"
                        });
                    }
                }

                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing category insights for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ForecastDto> GetFinancialForecastAsync(long userId, int months = 6)
        {
            try
            {
                var historicalData = await GetHistoricalMonthlyData(userId, 12);

                var forecast = new ForecastDto();
                var currentDate = DateTime.UtcNow;

                for (int i = 1; i <= months; i++)
                {
                    var forecastDate = currentDate.AddMonths(i);
                    var avgIncome = historicalData.Average(d => d.Income);
                    var avgExpenses = historicalData.Average(d => d.Expenses);

                    forecast.MonthlyForecasts.Add(new MonthlyForecastDto
                    {
                        Year = forecastDate.Year,
                        Month = forecastDate.Month,
                        ForecastedIncome = avgIncome,
                        ForecastedExpenses = avgExpenses,
                        ForecastedSavings = avgIncome - avgExpenses,
                        ForecastedNetWorth = avgIncome - avgExpenses, // Simplified
                        Confidence = CalculateForecastConfidence(historicalData)
                    });
                }

                forecast.ProjectedIncome = forecast.MonthlyForecasts.Sum(f => f.ForecastedIncome);
                forecast.ProjectedExpenses = forecast.MonthlyForecasts.Sum(f => f.ForecastedExpenses);
                forecast.ProjectedSavings = forecast.ProjectedIncome - forecast.ProjectedExpenses;
                forecast.ProjectedNetWorth = forecast.ProjectedSavings;

                return forecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating forecast for user {UserId}", userId);
                throw;
            }
        }

        // Helper methods
        private SpendingPatternDto AnalyzeSpendingPatterns(List<Expense> expenses)
        {
            var dailyGroups = expenses.GroupBy(e => e.ExpenseDate.DayOfWeek);
            var mostSpentDay = dailyGroups.OrderByDescending(g => g.Sum(e => e.Amount)).First().Key.ToString();

            return new SpendingPatternDto
            {
                MostSpentDay = mostSpentDay,
                MostSpentTime = "Evening", // Simplified
                WeekendSpending = expenses.Where(e => e.ExpenseDate.DayOfWeek == DayOfWeek.Saturday ||
                                                     e.ExpenseDate.DayOfWeek == DayOfWeek.Sunday)
                                         .Sum(e => e.Amount),
                WeekdaySpending = expenses.Where(e => e.ExpenseDate.DayOfWeek != DayOfWeek.Saturday &&
                                                     e.ExpenseDate.DayOfWeek != DayOfWeek.Sunday)
                                         .Sum(e => e.Amount)
            };
        }

        private IncomePatternDto AnalyzeIncomePatterns(List<Income> incomes)
        {
            var dailyGroups = incomes.GroupBy(i => i.IncomeDate.DayOfWeek);
            var mostIncomeDay = dailyGroups.OrderByDescending(g => g.Sum(i => i.Amount)).First().Key.ToString();

            return new IncomePatternDto
            {
                MostIncomeDay = mostIncomeDay,
                MostIncomeTime = "Morning", // Simplified
                WeekendIncome = incomes.Where(i => i.IncomeDate.DayOfWeek == DayOfWeek.Saturday ||
                                                  i.IncomeDate.DayOfWeek == DayOfWeek.Sunday)
                                      .Sum(i => i.Amount),
                WeekdayIncome = incomes.Where(i => i.IncomeDate.DayOfWeek != DayOfWeek.Saturday &&
                                                  i.IncomeDate.DayOfWeek != DayOfWeek.Sunday)
                                      .Sum(i => i.Amount)
            };
        }

        private async Task<decimal> GetHistoricalMonthlyAverage(long userId)
        {
            var last6Months = DateTime.UtcNow.AddMonths(-6);
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate >= DateOnly.FromDateTime(last6Months))
                .SumAsync(e => e.Amount);

            return expenses / 6;
        }

        private string GetBudgetStatus(decimal spent, decimal budget)
        {
            var utilization = budget > 0 ? (spent / budget) * 100 : 0;
            return utilization switch
            {
                > 100 => "Over Budget",
                > 90 => "Critical",
                > 80 => "Warning",
                _ => "Good"
            };
        }

        private List<BudgetAlertDto> GenerateBudgetAlerts(BudgetAnalysisDto analysis)
        {
            var alerts = new List<BudgetAlertDto>();

            if (analysis.BudgetUtilization > 100)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Type = "Over Budget",
                    Message = "You have exceeded your budget",
                    Severity = "Critical",
                    CategoryName = "Overall"
                });
            }
            else if (analysis.BudgetUtilization > 90)
            {
                alerts.Add(new BudgetAlertDto
                {
                    Type = "Near Budget",
                    Message = "You are approaching your budget limit",
                    Severity = "Warning",
                    CategoryName = "Overall"
                });
            }

            return alerts;
        }

        private BudgetRecommendationDto GenerateBudgetRecommendations(BudgetAnalysisDto analysis)
        {
            var recommendations = new BudgetRecommendationDto();

            if (analysis.BudgetUtilization > 100)
            {
                recommendations.Recommendations.Add("Consider reducing discretionary spending");
                recommendations.Recommendations.Add("Review and adjust your budget categories");
                recommendations.Priority = "High";
            }
            else if (analysis.BudgetUtilization > 80)
            {
                recommendations.Recommendations.Add("Monitor your spending closely");
                recommendations.Recommendations.Add("Look for areas to cut back");
                recommendations.Priority = "Medium";
            }
            else
            {
                recommendations.Recommendations.Add("Great job staying within budget!");
                recommendations.Recommendations.Add("Consider increasing savings goals");
                recommendations.Priority = "Low";
            }

            return recommendations;
        }

        private string CalculateHealthScore(decimal netWorth, decimal savingsRate)
        {
            var score = 0;
            if (netWorth > 0) score += 30;
            if (savingsRate > 20) score += 40;
            else if (savingsRate > 10) score += 20;
            else if (savingsRate > 0) score += 10;

            return score.ToString();
        }

        private string GetHealthStatus(decimal savingsRate)
        {
            return savingsRate switch
            {
                > 20 => "Excellent",
                > 10 => "Good",
                > 0 => "Fair",
                _ => "Poor"
            };
        }

        private string GetMetricStatus(decimal value, decimal threshold)
        {
            return value >= threshold ? "Good" : "Needs Improvement";
        }

        private List<HealthRecommendationDto> GenerateHealthRecommendations(FinancialHealthDto health)
        {
            var recommendations = new List<HealthRecommendationDto>();

            if (health.SavingsRate < 10)
            {
                recommendations.Add(new HealthRecommendationDto
                {
                    Title = "Increase Savings Rate",
                    Description = "Aim to save at least 10-20% of your income",
                    Priority = "High",
                    Category = "Savings"
                });
            }

            if (health.NetWorth < 0)
            {
                recommendations.Add(new HealthRecommendationDto
                {
                    Title = "Build Emergency Fund",
                    Description = "Focus on building 3-6 months of expenses in savings",
                    Priority = "High",
                    Category = "Emergency Fund"
                });
            }

            return recommendations;
        }

        private TrendDirectionDto CalculateTrend(List<decimal> values)
        {
            if (values.Count < 2)
            {
                return new TrendDirectionDto { Direction = "stable", Percentage = 0, Description = "Insufficient data" };
            }

            var firstHalf = values.Take(values.Count / 2).Average();
            var secondHalf = values.Skip(values.Count / 2).Average();
            var change = ((secondHalf - firstHalf) / firstHalf) * 100;

            return new TrendDirectionDto
            {
                Direction = change > 5 ? "up" : change < -5 ? "down" : "stable",
                Percentage = Math.Abs(change),
                Description = change > 5 ? "Increasing trend" : change < -5 ? "Decreasing trend" : "Stable trend"
            };
        }

        private async Task<List<(decimal Income, decimal Expenses)>> GetHistoricalMonthlyData(long userId, int months)
        {
            var data = new List<(decimal Income, decimal Expenses)>();
            var endDate = DateTime.UtcNow;

            for (int i = 0; i < months; i++)
            {
                var month = endDate.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var income = await _context.Incomes
                    .Where(inc => inc.UserId == userId &&
                                 inc.IncomeDate >= DateOnly.FromDateTime(monthStart) &&
                                 inc.IncomeDate <= DateOnly.FromDateTime(monthEnd))
                    .SumAsync(inc => inc.Amount);

                var expenses = await _context.Expenses
                    .Where(exp => exp.UserId == userId &&
                                 exp.ExpenseDate >= DateOnly.FromDateTime(monthStart) &&
                                 exp.ExpenseDate <= DateOnly.FromDateTime(monthEnd))
                    .SumAsync(exp => exp.Amount);

                data.Add((income, expenses));
            }

            return data;
        }

        private decimal CalculateForecastConfidence(List<(decimal Income, decimal Expenses)> historicalData)
        {
            // Simplified confidence calculation based on data consistency
            var incomeVariance = CalculateVariance(historicalData.Select(d => d.Income).ToList());
            var expenseVariance = CalculateVariance(historicalData.Select(d => d.Expenses).ToList());

            var avgVariance = (incomeVariance + expenseVariance) / 2;
            return Math.Max(0, 100 - avgVariance);
        }

        private decimal CalculateVariance(List<decimal> values)
        {
            if (values.Count < 2) return 0;

            var average = values.Average();
            var variance = values.Sum(v => (decimal)Math.Pow((double)(v - average), 2)) / values.Count;
            return variance;
        }

        private int GetWeekOfYear(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
    }
}
