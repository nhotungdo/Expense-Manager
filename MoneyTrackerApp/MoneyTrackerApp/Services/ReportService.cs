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
    Task<string> ExportReportAsync(long userId, GenerateReportDto dto);
}

public class ReportService : IReportService
{
    private readonly ExpenseManagerContext _context;
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;

    public ReportService(ExpenseManagerContext context, ITransactionService transactionService, IBudgetService budgetService)
    {
        _context = context;
        _transactionService = transactionService;
        _budgetService = budgetService;
    }

    /// <summary>
    /// Generate cash flow report
    /// </summary>
    public async Task<CashFlowReportDto> GenerateCashFlowReportAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId 
                && t.TransactionDate >= startDate 
                && t.TransactionDate <= endDate
                && t.TransactionType != 3) // Exclude transfers
            .ToListAsync();

        var incomeTransactions = transactions.Where(t => t.TransactionType == 1).ToList();
        var expenseTransactions = transactions.Where(t => t.TransactionType == 2).ToList();

        var totalIncome = incomeTransactions.Sum(t => t.Amount);
        var totalExpense = expenseTransactions.Sum(t => t.Amount);

        // Group by category
        var incomeItems = incomeTransactions
            .GroupBy(t => t.Category?.Name ?? "Uncategorized")
            .Select(g => new CashFlowItemDto
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalIncome > 0 ? (g.Sum(t => t.Amount) / totalIncome) * 100 : 0
            })
            .OrderByDescending(i => i.Amount)
            .ToList();

        var expenseItems = expenseTransactions
            .GroupBy(t => t.Category?.Name ?? "Uncategorized")
            .Select(g => new CashFlowItemDto
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpense > 0 ? (g.Sum(t => t.Amount) / totalExpense) * 100 : 0
            })
            .OrderByDescending(i => i.Amount)
            .ToList();

        // Daily breakdown
        var dailyBreakdown = transactions
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new DailyCashFlowDto
            {
                Date = g.Key,
                Income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                Expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount),
                NetFlow = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount) - 
                         g.Where(t => t.TransactionType == 2).Sum(t => t.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new CashFlowReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetCashFlow = totalIncome - totalExpense,
            IncomeItems = incomeItems,
            ExpenseItems = expenseItems,
            DailyBreakdown = dailyBreakdown
        };
    }

    /// <summary>
    /// Generate monthly trend report
    /// </summary>
    public async Task<MonthlyTrendReportDto> GenerateMonthlyTrendReportAsync(long userId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId 
                && t.TransactionDate >= startDate 
                && t.TransactionDate <= endDate
                && t.TransactionType != 3) // Exclude transfers
            .ToListAsync();

        var monthlyData = new List<MonthlyDataDto>();

        for (int month = 1; month <= 12; month++)
        {
            var monthTransactions = transactions
                .Where(t => t.TransactionDate.Month == month)
                .ToList();

            var income = monthTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var expense = monthTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
            var netIncome = income - expense;
            var savingsRate = income > 0 ? (netIncome / income) * 100 : 0;

            monthlyData.Add(new MonthlyDataDto
            {
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Income = income,
                Expense = expense,
                NetIncome = netIncome,
                SavingsRate = savingsRate
            });
        }

        var averageIncome = monthlyData.Average(m => m.Income);
        var averageExpense = monthlyData.Average(m => m.Expense);

        // Determine trend
        var firstHalfIncome = monthlyData.Take(6).Average(m => m.Income);
        var secondHalfIncome = monthlyData.Skip(6).Average(m => m.Income);
        var trend = secondHalfIncome > firstHalfIncome * 1.1m ? "Increasing" :
                   secondHalfIncome < firstHalfIncome * 0.9m ? "Decreasing" : "Stable";

        return new MonthlyTrendReportDto
        {
            Year = year,
            MonthlyData = monthlyData,
            AverageIncome = averageIncome,
            AverageExpense = averageExpense,
            Trend = trend
        };
    }

    /// <summary>
    /// Generate category breakdown report
    /// </summary>
    public async Task<CategoryBreakdownReportDto> GenerateCategoryBreakdownAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId 
                && t.TransactionDate >= startDate 
                && t.TransactionDate <= endDate
                && t.TransactionType != 3)
            .ToListAsync();

        var totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);

        var incomeCategories = transactions
            .Where(t => t.TransactionType == 1)
            .GroupBy(t => new { t.Category?.Name, t.Category?.Icon, t.Category?.Color })
            .Select(g => new CategoryBreakdownItemDto
            {
                CategoryName = g.Key.Name ?? "Uncategorized",
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalIncome > 0 ? (g.Sum(t => t.Amount) / totalIncome) * 100 : 0,
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var expenseCategories = transactions
            .Where(t => t.TransactionType == 2)
            .GroupBy(t => new { t.Category?.Name, t.Category?.Icon, t.Category?.Color })
            .Select(g => new CategoryBreakdownItemDto
            {
                CategoryName = g.Key.Name ?? "Uncategorized",
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpense > 0 ? (g.Sum(t => t.Amount) / totalExpense) * 100 : 0,
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        return new CategoryBreakdownReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            IncomeCategories = incomeCategories,
            ExpenseCategories = expenseCategories,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense
        };
    }

    /// <summary>
    /// Get dashboard overview with charts and recent data
    /// </summary>
    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(long userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get current balance
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotal)
            .ToListAsync();
        var currentBalance = accounts.Sum(a => a.CurrentBalance);

        // Get monthly income and expense
        var monthlyTransactions = await _context.Transactions
            .Where(t => t.UserId == userId 
                && t.TransactionDate >= startOfMonth 
                && t.TransactionDate <= endOfMonth
                && t.TransactionType != 3)
            .ToListAsync();

        var monthlyIncome = monthlyTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var monthlyExpense = monthlyTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
        var monthlySavings = monthlyIncome - monthlyExpense;
        var savingsRate = monthlyIncome > 0 ? (monthlySavings / monthlyIncome) * 100 : 0;

        // Cash flow chart (last 7 days)
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => now.AddDays(-6 + i).Date)
            .ToList();

        var cashFlowChart = new CashFlowChartDto
        {
            Labels = last7Days.Select(d => d.ToString("MM/dd")).ToList(),
            IncomeData = new List<decimal>(),
            ExpenseData = new List<decimal>()
        };

        foreach (var day in last7Days)
        {
            var dayTransactions = monthlyTransactions.Where(t => t.TransactionDate.Date == day).ToList();
            cashFlowChart.IncomeData.Add(dayTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount));
            cashFlowChart.ExpenseData.Add(dayTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount));
        }

        // Expense pie chart (top 5 categories)
        var expensePieChart = monthlyTransactions
            .Where(t => t.TransactionType == 2)
            .GroupBy(t => new { CategoryName = t.Category?.Name ?? "Other", Color = t.Category?.Color ?? "#999999" })
            .Select(g => new CategoryPieChartDto
            {
                Label = g.Key.CategoryName,
                Value = g.Sum(t => t.Amount),
                Color = g.Key.Color
            })
            .OrderByDescending(c => c.Value)
            .Take(5)
            .ToList();

        // Recent transactions
        var recentTransactions = await _transactionService.GetRecentTransactionsAsync(userId, 5);

        // Budget alerts
        var budgetAlerts = await _budgetService.GetBudgetAlertsAsync(userId);

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
                CategoryIcon = t.CategoryIcon
            }).ToList(),
            BudgetAlerts = budgetAlerts
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
}
