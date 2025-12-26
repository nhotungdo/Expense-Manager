using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyTrackerApp.Services;

public interface IAnalysisService
{
    Task<AnalysisResultDto> AnalyzeAsync(long userId);
}

public class AnalysisService : IAnalysisService
{
    private readonly ExpenseManagerContext _context;

    public AnalysisService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<AnalysisResultDto> AnalyzeAsync(long userId)
    {
        var now = DateTime.UtcNow; 
        // In a real app, should handle User's Timezone. Assuming UTC or Local match for simplicity as provided snippet does.
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        
        // Fetch expenses for last 2 months
        var expenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == (int)TransactionType.Expense && t.TransactionDate >= startOfLastMonth)
            .Include(t => t.Category)
            .ToListAsync();

        var thisMonthExpenses = expenses.Where(t => t.TransactionDate >= startOfMonth).ToList();
        var lastMonthExpenses = expenses.Where(t => t.TransactionDate < startOfMonth && t.TransactionDate >= startOfLastMonth).ToList();

        decimal thisMonthTotal = thisMonthExpenses.Sum(t => t.Amount);
        decimal lastMonthTotal = lastMonthExpenses.Sum(t => t.Amount);

        // Simple Prediction: Pro-rate the rest of the month
        int daysPassed = (now - startOfMonth).Days + 1;
        if (daysPassed < 1) daysPassed = 1;

        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        decimal predictedTotal = (thisMonthTotal / daysPassed) * daysInMonth;

        // Trend
        string trend = "Stable";
        decimal trendPercent = 0;
        
        // Compare Projected This Month vs Actual Last Month
        if (lastMonthTotal > 0)
        {
            var diff = predictedTotal - lastMonthTotal;
            var pct = (diff / lastMonthTotal) * 100;
            trendPercent = Math.Abs(pct);
            
            if (pct > 5) trend = "Increasing";
            else if (pct < -5) trend = "Decreasing";
        }
        else if (predictedTotal > 0)
        {
             trend = "Increasing";
             trendPercent = 100;
        }

        // Anomalies 
        // Logic: Transaction > 2x Average Transaction Size AND > 500k VND (to avoid small noise)
        var averageTx = thisMonthExpenses.Any() ? thisMonthExpenses.Average(t => t.Amount) : 0;
        var threshold = Math.Max(averageTx * 2, 500000); // Minimum threshold 500k

        var anomalies = thisMonthExpenses
            .Where(t => t.Amount > threshold)
            .OrderByDescending(t => t.Amount)
            .Take(3)
            .Select(t => new AnomalyDto
            {
                TransactionId = t.Id,
                Date = t.TransactionDate,
                Amount = t.Amount,
                CategoryName = t.Category?.Name ?? "Khác",
                Note = t.Note,
                Reason = "Giao dịch lớn bất thường"
            })
            .ToList();

        // Generate Insights
        var insights = new List<string>();

        // Trend Insight
        if (trend == "Increasing") 
            insights.Add($"Xu hướng chi tiêu tăng {trendPercent:N1}% so với tháng trước. Hãy chú ý ngân sách.");
        else if (trend == "Decreasing") 
            insights.Add($"Tuyệt vời! Chi tiêu dự kiến giảm {trendPercent:N1}% so với tháng trước.");
        else 
            insights.Add("Chi tiêu của bạn đang duy trì ổn định so với tháng trước.");

        // Category Insight
        var topCategory = thisMonthExpenses
            .GroupBy(t => t.Category?.Name ?? "Khác")
            .Select(g => new { Name = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Amount)
            .FirstOrDefault();
            
        if (topCategory != null && thisMonthTotal > 0)
        {
             var catPct = (topCategory.Amount / thisMonthTotal) * 100;
             insights.Add($"'{topCategory.Name}' chiếm {catPct:N0}% tổng chi tiêu thàng này.");
        }

        // Anomaly Insight
        if (anomalies.Count > 0)
        {
            insights.Add($"Phát hiện {anomalies.Count} giao dịch bất thường cần xem lại.");
        }

        return new AnalysisResultDto
        {
            Trend = trend,
            TrendPercentage = trendPercent,
            TotalSpendingThisMonth = thisMonthTotal,
            PredictedSpendingThisMonth = predictedTotal,
            Anomalies = anomalies,
            Insights = insights
        };
    }
}
