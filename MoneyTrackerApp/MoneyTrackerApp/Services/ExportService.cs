using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for exporting data to various formats
/// </summary>
public interface IExportService
{
    Task<byte[]> ExportTransactionsToExcelAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null);
    Task<byte[]> ExportTransactionsToPdfAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null);
    Task<byte[]> ExportTransactionsToCsvAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null);
    Task<byte[]> ExportCashFlowReportToExcelAsync(long userId, int year, int month);
    Task<byte[]> ExportCategoryReportToExcelAsync(long userId, DateTime? startDate, DateTime? endDate);
}

public class ExportService : IExportService
{
    private readonly ExpenseManagerContext _context;

    public ExportService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Export transactions to Excel (Fallback to CSV for now as ClosedXML is not installed)
    /// </summary>
    public async Task<byte[]> ExportTransactionsToExcelAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null)
    {
        // Note: In a real environment with NuGet access, we would use ClosedXML here.
        // For now, we'll return CSV content which Excel can open.
        return await ExportTransactionsToCsvAsync(userId, startDate, endDate, accountIds);
    }

    /// <summary>
    /// Export transactions to PDF (Fallback to HTML for now as iText is not installed)
    /// </summary>
    public async Task<byte[]> ExportTransactionsToPdfAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (accountIds != null && accountIds.Any())
            query = query.Where(t => accountIds.Contains(t.AccountId));

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        var html = new StringBuilder();
        html.Append("<html><head><style>table { border-collapse: collapse; width: 100%; } th, td { border: 1px solid black; padding: 8px; text-align: left; } th { background-color: #f2f2f2; }</style></head><body>");
        html.Append("<h1>Transaction Report</h1>");
        html.Append($"<p>Period: {startDate?.ToString("dd/MM/yyyy") ?? "All"} - {endDate?.ToString("dd/MM/yyyy") ?? "All"}</p>");
        html.Append("<table>");
        html.Append("<tr><th>Date</th><th>Type</th><th>Category</th><th>Account</th><th>Amount</th><th>Currency</th></tr>");

        foreach (var transaction in transactions)
        {
            html.Append("<tr>");
            html.Append($"<td>{transaction.TransactionDate:dd/MM/yyyy}</td>");
            html.Append($"<td>{(transaction.TransactionType == 1 ? "Income" : "Expense")}</td>");
            html.Append($"<td>{transaction.Category?.Name ?? "N/A"}</td>");
            html.Append($"<td>{transaction.Account?.Name ?? "N/A"}</td>");
            html.Append($"<td>{transaction.Amount:N2}</td>");
            html.Append($"<td>{transaction.Currency}</td>");
            html.Append("</tr>");
        }

        html.Append("</table>");
        html.Append($"<h3>Total Amount: {transactions.Sum(t => t.Amount):N2}</h3>");
        html.Append("</body></html>");

        return Encoding.UTF8.GetBytes(html.ToString());
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    public async Task<byte[]> ExportTransactionsToCsvAsync(long userId, DateTime? startDate, DateTime? endDate, List<long>? accountIds = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (accountIds != null && accountIds.Any())
            query = query.Where(t => accountIds.Contains(t.AccountId));

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        var csv = new StringBuilder();
        
        // Headers
        csv.AppendLine("Date,Type,Category,Account,Amount,Currency,Note");

        // Data
        foreach (var transaction in transactions)
        {
            csv.AppendLine($"{transaction.TransactionDate:dd/MM/yyyy}," +
                          $"{(transaction.TransactionType == 1 ? "Income" : "Expense")}," +
                          $"\"{transaction.Category?.Name ?? "N/A"}\"," +
                          $"\"{transaction.Account?.Name ?? "N/A"}\"," +
                          $"{transaction.Amount}," +
                          $"{transaction.Currency}," +
                          $"\"{transaction.Note?.Replace("\"", "\"\"") ?? ""}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Export cash flow report to Excel (Fallback to CSV)
    /// </summary>
    public async Task<byte[]> ExportCashFlowReportToExcelAsync(long userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var income = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.TransactionType == 0).Sum(t => t.Amount);

        var csv = new StringBuilder();
        csv.AppendLine($"Cash Flow Report - {month}/{year}");
        csv.AppendLine("Type,Amount");
        csv.AppendLine($"Total Income,{income}");
        csv.AppendLine($"Total Expense,{expense}");
        csv.AppendLine($"Net Cash Flow,{income - expense}");
        csv.AppendLine();
        csv.AppendLine("Category Breakdown");
        csv.AppendLine("Category,Amount,Type");

        var categoryGroups = transactions
            .GroupBy(t => new { t.Category?.Name, t.TransactionType })
            .Select(g => new
            {
                Category = g.Key.Name ?? "Uncategorized",
                Amount = g.Sum(t => t.Amount),
                Type = g.Key.TransactionType == 1 ? "Income" : "Expense"
            })
            .OrderByDescending(g => g.Amount);

        foreach (var group in categoryGroups)
        {
            csv.AppendLine($"\"{group.Category}\",{group.Amount},{group.Type}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Export category report to Excel (Fallback to CSV)
    /// </summary>
    public async Task<byte[]> ExportCategoryReportToExcelAsync(long userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var transactions = await query.ToListAsync();

        var categoryStats = transactions
            .GroupBy(t => new { t.Category?.Name, t.TransactionType })
            .Select(g => new
            {
                Category = g.Key.Name ?? "Uncategorized",
                Type = g.Key.TransactionType == 1 ? "Income" : "Expense",
                Count = g.Count(),
                Total = g.Sum(t => t.Amount),
                Average = g.Average(t => t.Amount)
            })
            .OrderByDescending(g => g.Total);

        var csv = new StringBuilder();
        csv.AppendLine("Category,Type,Transaction Count,Total Amount,Average Amount");

        foreach (var stat in categoryStats)
        {
            csv.AppendLine($"\"{stat.Category}\",{stat.Type},{stat.Count},{stat.Total},{stat.Average}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
