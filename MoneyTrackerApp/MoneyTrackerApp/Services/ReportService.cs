using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for generating financial reports and analytics
/// Handles cash flow, trends, category breakdown, and dashboard data
/// </summary>
public interface IReportService
{
    Task<CashFlowReportDto> GenerateCashFlowReportAsync(long userId, DateTime startDate, DateTime endDate);
    Task<MonthlyTrendReportDto> GenerateMonthlyTrendReportAsync(long userId, int year);
    Task<CategoryBreakdownReportDto> GenerateCategoryBreakdownAsync(long userId, DateTime startDate, DateTime endDate);
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(long userId);
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(long userId, int days);
    Task<string> ExportReportAsync(long userId, GenerateReportDto dto);
    Task<object> GetPersonalWalletSummaryAsync(long userId);
    Task<List<CategoryBreakdownItem>> GetExpenseBreakdownAsync(long userId, string period);
    Task<List<CategoryBreakdownItem>> GetIncomeBreakdownAsync(long userId, string period);
}

public class CategoryBreakdownItem
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class ReportService : IReportService
{
    private readonly ExpenseManagerContext _context;
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;
    private readonly ISavingsGoalService _savingsGoalService;

    public ReportService(
        ExpenseManagerContext context, 
        ITransactionService transactionService, 
        IBudgetService budgetService,
        ISavingsGoalService savingsGoalService)
    {
        _context = context;
        _transactionService = transactionService;
        _budgetService = budgetService;
        _savingsGoalService = savingsGoalService;
    }

    public async Task<CashFlowReportDto> GenerateCashFlowReportAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var incomeTransactions = transactions.Where(t => t.TransactionType == 1).ToList();
        var expenseTransactions = transactions.Where(t => t.TransactionType == 2).ToList();

        var totalIncome = incomeTransactions.Sum(t => t.Amount);
        var totalExpense = expenseTransactions.Sum(t => t.Amount);

        var report = new CashFlowReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetCashFlow = totalIncome - totalExpense,
            IncomeItems = incomeTransactions
                .GroupBy(t => t.Category?.Name ?? "Other")
                .Select(g => new CashFlowItemDto
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = totalIncome > 0 ? (g.Sum(t => t.Amount) / totalIncome) * 100 : 0
                }).ToList(),
            ExpenseItems = expenseTransactions
                .GroupBy(t => t.Category?.Name ?? "Other")
                .Select(g => new CashFlowItemDto
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = totalExpense > 0 ? (g.Sum(t => t.Amount) / totalExpense) * 100 : 0
                }).ToList(),
            DailyBreakdown = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new DailyCashFlowDto
                {
                    Date = g.Key,
                    Income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                    Expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                    NetFlow = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount) - g.Where(t => t.TransactionType == 2).Sum(t => t.Amount)
                })
                .OrderBy(d => d.Date)
                .ToList()
        };

        return report;
    }

    public async Task<MonthlyTrendReportDto> GenerateMonthlyTrendReportAsync(long userId, int year)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate.Year == year)
            .ToListAsync();

        var monthlyData = new List<MonthlyDataDto>();
        for (int i = 1; i <= 12; i++)
        {
            var monthTransactions = transactions.Where(t => t.TransactionDate.Month == i).ToList();
            var income = monthTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var expense = monthTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
            
            monthlyData.Add(new MonthlyDataDto
            {
                Month = i,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                Income = income,
                Expense = expense,
                NetIncome = income - expense,
                SavingsRate = income > 0 ? ((income - expense) / income) * 100 : 0
            });
        }

        var avgIncome = monthlyData.Average(m => m.Income);
        var avgExpense = monthlyData.Average(m => m.Expense);
        
        // Simple trend analysis
        var firstHalf = monthlyData.Take(6).Sum(m => m.NetIncome);
        var secondHalf = monthlyData.Skip(6).Sum(m => m.NetIncome);
        string trend = "Stable";
        if (secondHalf > firstHalf * 1.1m) trend = "Increasing";
        else if (secondHalf < firstHalf * 0.9m) trend = "Decreasing";

        return new MonthlyTrendReportDto
        {
            Year = year,
            MonthlyData = monthlyData,
            AverageIncome = avgIncome,
            AverageExpense = avgExpense,
            Trend = trend
        };
    }

    public async Task<CategoryBreakdownReportDto> GenerateCategoryBreakdownAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var incomeTransactions = transactions.Where(t => t.TransactionType == 1).ToList();
        var expenseTransactions = transactions.Where(t => t.TransactionType == 2).ToList();
        var totalIncome = incomeTransactions.Sum(t => t.Amount);
        var totalExpense = expenseTransactions.Sum(t => t.Amount);

        return new CategoryBreakdownReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            IncomeCategories = incomeTransactions
                .GroupBy(t => t.Category)
                .Select(g => new CategoryBreakdownItemDto
                {
                    CategoryName = g.Key?.Name ?? "Other",
                    CategoryIcon = g.Key?.Icon,
                    CategoryColor = g.Key?.Color,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = totalIncome > 0 ? (g.Sum(t => t.Amount) / totalIncome) * 100 : 0,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(i => i.Amount)
                .ToList(),
            ExpenseCategories = expenseTransactions
                .GroupBy(t => t.Category)
                .Select(g => new CategoryBreakdownItemDto
                {
                    CategoryName = g.Key?.Name ?? "Other",
                    CategoryIcon = g.Key?.Icon,
                    CategoryColor = g.Key?.Color,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = totalExpense > 0 ? (g.Sum(t => t.Amount) / totalExpense) * 100 : 0,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(i => i.Amount)
                .ToList()
        };
    }

    /// <summary>
    /// Get dashboard overview with charts and recent data
    /// </summary>
    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(long userId)
    {
        var now = DateTime.UtcNow;
        var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        var endOfCurrentMonth = startOfCurrentMonth.AddMonths(1).AddDays(-1);
        var startOfSixMonthsAgo = startOfCurrentMonth.AddMonths(-5);

        // Get current balance
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotal)
            .ToListAsync();
        var currentBalance = accounts.Sum(a => a.CurrentBalance);

        // Get transactions for the last 6 months (for chart)
        var sixMonthTransactions = await _context.Transactions
            .Where(t => t.UserId == userId 
                && t.TransactionDate >= startOfSixMonthsAgo 
                && t.TransactionDate <= endOfCurrentMonth
                && t.TransactionType != 3)
            .ToListAsync();

        // Calculate stats for the CURRENT month
        var currentMonthTransactions = sixMonthTransactions
            .Where(t => t.TransactionDate >= startOfCurrentMonth && t.TransactionDate <= endOfCurrentMonth)
            .ToList();

        var monthlyIncome = currentMonthTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var monthlyExpense = currentMonthTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
        var monthlySavings = monthlyIncome - monthlyExpense;
        var savingsRate = monthlyIncome > 0 ? (monthlySavings / monthlyIncome) * 100 : 0;

        // Cash flow chart (last 6 months)
        var cashFlowChart = new CashFlowChartDto
        {
            Labels = new List<string>(),
            IncomeData = new List<decimal>(),
            ExpenseData = new List<decimal>()
        };

        for (int i = 0; i < 6; i++)
        {
            var monthDate = startOfSixMonthsAgo.AddMonths(i);
            cashFlowChart.Labels.Add(monthDate.ToString("MM/yyyy"));

            var monthTrans = sixMonthTransactions
                .Where(t => t.TransactionDate.Year == monthDate.Year && t.TransactionDate.Month == monthDate.Month)
                .ToList();

            cashFlowChart.IncomeData.Add(monthTrans.Where(t => t.TransactionType == 1).Sum(t => t.Amount));
            cashFlowChart.ExpenseData.Add(monthTrans.Where(t => t.TransactionType == 2).Sum(t => t.Amount));
        }

        // Expense pie chart (top 3 categories in current month)
        var expensePieChart = currentMonthTransactions
            .Where(t => t.TransactionType == 2)
            .GroupBy(t => new { CategoryName = t.Category?.Name ?? "Other", Color = t.Category?.Color ?? "#999999" })
            .Select(g => new CategoryPieChartDto
            {
                Label = g.Key.CategoryName,
                Value = g.Sum(t => t.Amount),
                Color = g.Key.Color
            })
            .OrderByDescending(c => c.Value)
            .Take(3)
            .ToList();

        // Recent transactions
        var recentTransactions = await _transactionService.GetRecentTransactionsAsync(userId, 5);

        // Budget alerts
        var budgetAlerts = await _budgetService.GetBudgetAlertsAsync(userId);

        // Savings Goals
        var savingsGoals = await _savingsGoalService.GetUserSavingsGoalsAsync(userId, activeOnly: true);

        return new DashboardOverviewDto
        {
            CurrentBalance = currentBalance,
            MonthlyIncome = monthlyIncome,
            MonthlyExpense = monthlyExpense,
            MonthlySavings = monthlySavings,
            SavingsRate = savingsRate,
            CashFlowChart = cashFlowChart,
            ExpensePieChart = expensePieChart,
            RecentTransactions = recentTransactions.Select(t => new RecentTransactionDto
            {
                Id = t.Id,
                Description = t.Note ?? "Transaction",
                Amount = t.Amount,
                Type = t.TransactionTypeDisplay,
                Date = t.TransactionDate,
                CategoryIcon = t.CategoryIcon,
                CategoryColor = t.CategoryColor,
                CategoryName = t.CategoryName,
                AccountName = t.AccountName
            }).ToList(),
            BudgetAlerts = budgetAlerts,
            SavingsGoals = savingsGoals.Take(3).ToList() // Take top 3 goals
        };
    }

    /// <summary>
    /// Export report to file (PDF, Excel, CSV)
    /// </summary>
    public async Task<string> ExportReportAsync(long userId, GenerateReportDto dto)
    {
        // Generate report data based on type
        object reportData = dto.ReportType switch
        {
            1 => await GenerateCashFlowReportAsync(userId, dto.StartDate, dto.EndDate),
            3 => await GenerateCategoryBreakdownAsync(userId, dto.StartDate, dto.EndDate),
            4 => await GenerateMonthlyTrendReportAsync(userId, dto.StartDate.Year),
            _ => throw new NotImplementedException("Report type not implemented")
        };

        // TODO: Implement actual file generation
        // For now, return a placeholder path
        var fileName = $"report_{dto.ReportType}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var extension = dto.FileFormat switch
        {
            1 => ".pdf",
            2 => ".xlsx",
            3 => ".csv",
            4 => ".json",
            _ => ".txt"
        };

        var filePath = $"/reports/{fileName}{extension}";

        // In production, you would:
        // 1. Use a library like iTextSharp for PDF
        // 2. Use EPPlus or ClosedXML for Excel
        // 3. Use CsvHelper for CSV
        // 4. Use System.Text.Json for JSON

        return filePath;
    }

    /// <summary>
    /// Get Dashboard Analytics Data for Donut Charts
    /// </summary>
    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(long userId, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var endDate = DateTime.UtcNow;

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        // Color palette
        var colors = new[] { "#7C3AED", "#3B82F6", "#14B8A6", "#FACC15", "#F472B6", "#10B981", "#F59E0B", "#EF4444" };

        // 1. Chi tiêu theo Danh mục (Expense Categories)
        var categorySpending = transactions
            .Where(t => t.TransactionType == 2) // Expense
            .GroupBy(t => t.Category?.Name ?? "Khác")
            .Select((g, index) => new CategorySpendingDto
            {
                Name = g.Key,
                Value = g.Sum(t => t.Amount),
                Count = g.Count(),
                Color = colors[index % colors.Length]
            })
            .OrderByDescending(c => c.Value)
            .Take(5)
            .ToList();

        // 2. Thu nhập theo Nguồn (Income Sources)
        var incomeSource = transactions
            .Where(t => t.TransactionType == 1) // Income
            .GroupBy(t => t.Category?.Name ?? "Thu nhập khác")
            .Select((g, index) => new IncomeSourceDto
            {
                Name = g.Key,
                Value = g.Sum(t => t.Amount),
                Count = g.Count(),
                Color = new[] { "#10B981", "#14B8A6", "#3B82F6" }[index % 3]
            })
            .OrderByDescending(i => i.Value)
            .ToList();

        // 3. Giao dịch theo Loại (Transaction Types)
        var transactionType = new List<TransactionTypeDto>
        {
            new TransactionTypeDto
            {
                Name = "Thu",
                Value = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                Count = transactions.Count(t => t.TransactionType == 1),
                Color = "#10B981"
            },
            new TransactionTypeDto
            {
                Name = "Chi",
                Value = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                Count = transactions.Count(t => t.TransactionType == 2),
                Color = "#F472B6"
            }
        };

        // 4. Giao dịch theo Ví (Wallet Distribution)
        var walletDistribution = transactions
            .GroupBy(t => t.Account?.Name ?? "Không xác định")
            .Select((g, index) => new WalletDistributionDto
            {
                Name = g.Key,
                Value = g.Sum(t => t.Amount),
                Count = g.Count(),
                Color = colors[index % colors.Length]
            })
            .OrderByDescending(w => w.Value)
            .ToList();

        // Calculate totals
        var totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
        var balance = totalIncome - totalExpense;

        return new DashboardAnalyticsDto
        {
            CategorySpending = categorySpending,
            IncomeSource = incomeSource,
            TransactionType = transactionType,
            WalletDistribution = walletDistribution,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = balance
        };
    }

    public async Task<object> GetPersonalWalletSummaryAsync(long userId)
    {
        // Get all accounts for user
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var totalBalance = accounts.Sum(a => a.CurrentBalance);

        // Get current month transactions
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var monthlyTransactions = await _context.Transactions
            .Where(t => t.UserId == userId && 
                   t.TransactionDate >= startOfMonth && 
                   t.TransactionDate <= endOfMonth)
            .ToListAsync();

        var monthlyIncome = monthlyTransactions
            .Where(t => t.TransactionType == 1)
            .Sum(t => t.Amount);

        var monthlyExpense = monthlyTransactions
            .Where(t => t.TransactionType == 2)
            .Sum(t => t.Amount);

        return new
        {
            totalBalance = totalBalance,
            monthlyIncome = monthlyIncome,
            monthlyExpense = monthlyExpense,
            accountCount = accounts.Count
        };
    }

    public async Task<List<CategoryBreakdownItem>> GetExpenseBreakdownAsync(long userId, string period)
    {
        var (startDate, endDate) = GetDateRangeFromPeriod(period);

        var expenses = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && 
                   t.TransactionType == 2 && // Expense
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .GroupBy(t => t.Category != null ? t.Category.Name : "Khác")
            .Select(g => new CategoryBreakdownItem
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToListAsync();

        return expenses;
    }

    public async Task<List<CategoryBreakdownItem>> GetIncomeBreakdownAsync(long userId, string period)
    {
        var (startDate, endDate) = GetDateRangeFromPeriod(period);

        var income = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && 
                   t.TransactionType == 1 && // Income
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .GroupBy(t => t.Category != null ? t.Category.Name : "Khác")
            .Select(g => new CategoryBreakdownItem
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToListAsync();

        return income;
    }

    private (DateTime startDate, DateTime endDate) GetDateRangeFromPeriod(string period)
    {
        var now = DateTime.Now;
        DateTime startDate, endDate;

        switch (period.ToLower())
        {
            case "week":
                // Current week (Monday to Sunday)
                var dayOfWeek = (int)now.DayOfWeek;
                var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                startDate = now.Date.AddDays(-daysToMonday);
                endDate = startDate.AddDays(6);
                break;

            case "year":
                // Current year
                startDate = new DateTime(now.Year, 1, 1);
                endDate = new DateTime(now.Year, 12, 31);
                break;

            case "month":
            default:
                // Current month
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                break;
        }

        return (startDate, endDate);
    }
}
