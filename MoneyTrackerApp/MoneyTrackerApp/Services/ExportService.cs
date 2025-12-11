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
    Task<byte[]> ExportCashFlowReportToExcelAsync(long userId, DateTime startDate, DateTime endDate);
    Task<byte[]> ExportCashFlowReportToPdfAsync(long userId, DateTime startDate, DateTime endDate);
    Task<byte[]> ExportCashFlowReportToJsonAsync(long userId, DateTime startDate, DateTime endDate);
    
    Task<byte[]> ExportCategoryReportToExcelAsync(long userId, DateTime? startDate, DateTime? endDate);
    Task<byte[]> ExportCategoryReportToPdfAsync(long userId, DateTime? startDate, DateTime? endDate);
    Task<byte[]> ExportCategoryReportToJsonAsync(long userId, DateTime? startDate, DateTime? endDate);
    
    Task<byte[]> ExportMonthlyTrendsToExcelAsync(long userId, int year);
    Task<byte[]> ExportMonthlyTrendsToPdfAsync(long userId, int year);
    Task<byte[]> ExportMonthlyTrendsToJsonAsync(long userId, int year);
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
        html.Append("<h1>Báo cáo giao dịch</h1>");
        html.Append($"<p>Giai đoạn: {startDate?.ToString("dd/MM/yyyy") ?? "Tất cả"} - {endDate?.ToString("dd/MM/yyyy") ?? "Tất cả"}</p>");
        html.Append("<table>");
        html.Append("<tr><th>Ngày</th><th>Loại</th><th>Danh mục</th><th>Tài khoản</th><th>Số tiền</th><th>Tiền tệ</th></tr>");

        foreach (var transaction in transactions)
        {
            html.Append("<tr>");
            html.Append($"<td>{transaction.TransactionDate:dd/MM/yyyy}</td>");
            html.Append($"<td>{(transaction.TransactionType == 1 ? "Thu nhập" : "Chi tiêu")}</td>");
            html.Append($"<td>{transaction.Category?.Name ?? "N/A"}</td>");
            html.Append($"<td>{transaction.Account?.Name ?? "N/A"}</td>");
            html.Append($"<td>{transaction.Amount:N2}</td>");
            html.Append($"<td>{transaction.Currency}</td>");
            html.Append("</tr>");
        }

        html.Append("</table>");
        html.Append($"<h3>Tổng tiền: {transactions.Sum(t => t.Amount):N2}</h3>");
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
        csv.AppendLine("Ngày,Loại,Danh mục,Tài khoản,Số tiền,Tiền tệ,Ghi chú");

        // Data
        foreach (var transaction in transactions)
        {
            csv.AppendLine($"{transaction.TransactionDate:dd/MM/yyyy}," +
                          $"{(transaction.TransactionType == 1 ? "Thu nhập" : "Chi tiêu")}," +
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
        csv.AppendLine($"Báo cáo dòng tiền - {month}/{year}");
        csv.AppendLine("Loại,Số tiền");
        csv.AppendLine($"Tổng thu nhập,{income}");
        csv.AppendLine($"Tổng chi tiêu,{expense}");
        csv.AppendLine($"Dòng tiền ròng,{income - expense}");
        csv.AppendLine();
        csv.AppendLine("Chi tiết danh mục");
        csv.AppendLine("Danh mục,Số tiền,Loại");

        var categoryGroups = transactions
            .GroupBy(t => new { t.Category?.Name, t.TransactionType })
            .Select(g => new
            {
                Category = g.Key.Name ?? "Chưa phân loại",
                Amount = g.Sum(t => t.Amount),
                Type = g.Key.TransactionType == 1 ? "Thu nhập" : "Chi tiêu"
            })
            .OrderByDescending(g => g.Amount);

        foreach (var group in categoryGroups)
        {
            csv.AppendLine($"\"{group.Category}\",{group.Amount},{group.Type}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportCashFlowReportToExcelAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var data = await GetCashFlowDataAsync(userId, startDate, endDate);

        var csv = new StringBuilder();
        csv.AppendLine($"Báo cáo dòng tiền - {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
        csv.AppendLine("Loại,Số tiền");
        csv.AppendLine($"Tổng thu nhập,{data.Income}");
        csv.AppendLine($"Tổng chi tiêu,{data.Expense}");
        csv.AppendLine($"Dòng tiền ròng,{data.NetCashFlow}");
        csv.AppendLine();
        csv.AppendLine("Chi tiết danh mục");
        csv.AppendLine("Danh mục,Số tiền,Loại");

        foreach (var group in data.CategoryGroups)
        {
            csv.AppendLine($"\"{group.Category}\",{group.Amount},{group.Type}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportCashFlowReportToPdfAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var data = await GetCashFlowDataAsync(userId, startDate, endDate);
        
        var html = new StringBuilder();
        html.Append("<html><head><style>");
        html.Append("body { font-family: Arial, sans-serif; }");
        html.Append("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        html.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.Append("th { background-color: #f2f2f2; }");
        html.Append(".summary { margin-bottom: 30px; }");
        html.Append(".income { color: green; } .expense { color: red; }");
        html.Append("</style></head><body>");
        
        html.Append($"<h1>Báo cáo dòng tiền</h1>");
        html.Append($"<p>Giai đoạn: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}</p>");
        
        html.Append("<div class='summary'>");
        html.Append($"<h3>Tổng quan</h3>");
        html.Append($"<p>Tổng thu nhập: <span class='income'>{data.Income:N2}</span></p>");
        html.Append($"<p>Tổng chi tiêu: <span class='expense'>{data.Expense:N2}</span></p>");
        html.Append($"<p>Dòng tiền ròng: <strong>{data.NetCashFlow:N2}</strong></p>");
        html.Append("</div>");

        html.Append("<h3>Chi tiết danh mục</h3>");
        html.Append("<table>");
        html.Append("<tr><th>Danh mục</th><th>Loại</th><th>Số tiền</th></tr>");
        
        foreach (var group in data.CategoryGroups)
        {
            html.Append("<tr>");
            html.Append($"<td>{group.Category}</td>");
            html.Append($"<td>{group.Type}</td>");
            html.Append($"<td>{group.Amount:N2}</td>");
            html.Append("</tr>");
        }
        
        html.Append("</table></body></html>");
        return Encoding.UTF8.GetBytes(html.ToString());
    }

    public async Task<byte[]> ExportCashFlowReportToJsonAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var data = await GetCashFlowDataAsync(userId, startDate, endDate);
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// Export category report to Excel (Fallback to CSV)
    /// </summary>
    public async Task<byte[]> ExportCategoryReportToExcelAsync(long userId, DateTime? startDate, DateTime? endDate)
    {
        var data = await GetCategoryDataAsync(userId, startDate, endDate);

        var csv = new StringBuilder();
        csv.AppendLine("Danh mục,Loại,Số giao dịch,Tổng tiền,Trung bình");

        foreach (var stat in data)
        {
            csv.AppendLine($"\"{stat.Category}\",{stat.Type},{stat.Count},{stat.Total},{stat.Average}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportCategoryReportToPdfAsync(long userId, DateTime? startDate, DateTime? endDate)
    {
        var data = await GetCategoryDataAsync(userId, startDate, endDate);
        
        var html = new StringBuilder();
        html.Append("<html><head><style>");
        html.Append("body { font-family: Arial, sans-serif; }");
        html.Append("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        html.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.Append("th { background-color: #f2f2f2; }");
        html.Append("</style></head><body>");
        
        html.Append($"<h1>Báo cáo chi tiết danh mục</h1>");
        html.Append($"<p>Giai đoạn: {startDate?.ToString("dd/MM/yyyy") ?? "Tất cả"} - {endDate?.ToString("dd/MM/yyyy") ?? "Tất cả"}</p>");
        
        html.Append("<table>");
        html.Append("<tr><th>Danh mục</th><th>Loại</th><th>Số lượng</th><th>Tổng tiền</th><th>Trung bình</th></tr>");
        
        foreach (var stat in data)
        {
            html.Append("<tr>");
            html.Append($"<td>{stat.Category}</td>");
            html.Append($"<td>{stat.Type}</td>");
            html.Append($"<td>{stat.Count}</td>");
            html.Append($"<td>{stat.Total:N2}</td>");
            html.Append($"<td>{stat.Average:N2}</td>");
            html.Append("</tr>");
        }
        
        html.Append("</table></body></html>");
        return Encoding.UTF8.GetBytes(html.ToString());
    }

    public async Task<byte[]> ExportCategoryReportToJsonAsync(long userId, DateTime? startDate, DateTime? endDate)
    {
        var data = await GetCategoryDataAsync(userId, startDate, endDate);
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task<byte[]> ExportMonthlyTrendsToExcelAsync(long userId, int year)
    {
        var data = await GetMonthlyTrendsDataAsync(userId, year);

        var csv = new StringBuilder();
        csv.AppendLine($"Báo cáo xu hướng tháng - {year}");
        csv.AppendLine("Tháng,Thu nhập,Chi tiêu,Tiết kiệm ròng");

        foreach (var month in data)
        {
            csv.AppendLine($"{month.MonthName},{month.Income},{month.Expense},{month.Savings}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportMonthlyTrendsToPdfAsync(long userId, int year)
    {
        var data = await GetMonthlyTrendsDataAsync(userId, year);
        
        var html = new StringBuilder();
        html.Append("<html><head><style>");
        html.Append("body { font-family: Arial, sans-serif; }");
        html.Append("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        html.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.Append("th { background-color: #f2f2f2; }");
        html.Append("</style></head><body>");
        
        html.Append($"<h1>Báo cáo xu hướng tháng - {year}</h1>");
        
        html.Append("<table>");
        html.Append("<tr><th>Tháng</th><th>Thu nhập</th><th>Chi tiêu</th><th>Tiết kiệm ròng</th></tr>");
        
        foreach (var month in data)
        {
            html.Append("<tr>");
            html.Append($"<td>{month.MonthName}</td>");
            html.Append($"<td>{month.Income:N2}</td>");
            html.Append($"<td>{month.Expense:N2}</td>");
            html.Append($"<td>{month.Savings:N2}</td>");
            html.Append("</tr>");
        }
        
        html.Append("</table></body></html>");
        return Encoding.UTF8.GetBytes(html.ToString());
    }

    public async Task<byte[]> ExportMonthlyTrendsToJsonAsync(long userId, int year)
    {
        var data = await GetMonthlyTrendsDataAsync(userId, year);
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    // Helper methods to fetch data
    private async Task<CashFlowData> GetCashFlowDataAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var income = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.TransactionType == 0).Sum(t => t.Amount);

        var categoryGroups = transactions
            .GroupBy(t => new { t.Category?.Name, t.TransactionType })
            .Select(g => new CategoryGroupData
            {
                Category = g.Key.Name ?? "Chưa phân loại",
                Amount = g.Sum(t => t.Amount),
                Type = g.Key.TransactionType == 1 ? "Thu nhập" : "Chi tiêu"
            })
            .OrderByDescending(g => g.Amount)
            .ToList();

        return new CashFlowData
        {
            Income = income,
            Expense = expense,
            NetCashFlow = income - expense,
            CategoryGroups = categoryGroups
        };
    }

    private async Task<List<CategoryStatData>> GetCategoryDataAsync(long userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var transactions = await query.ToListAsync();

        return transactions
            .GroupBy(t => new { t.Category?.Name, t.TransactionType })
            .Select(g => new CategoryStatData
            {
                Category = g.Key.Name ?? "Chưa phân loại",
                Type = g.Key.TransactionType == 1 ? "Thu nhập" : "Chi tiêu",
                Count = g.Count(),
                Total = g.Sum(t => t.Amount),
                Average = g.Average(t => t.Amount)
            })
            .OrderByDescending(g => g.Total)
            .ToList();
    }

    private async Task<List<MonthlyTrendData>> GetMonthlyTrendsDataAsync(long userId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var monthlyData = transactions
            .GroupBy(t => t.TransactionDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                Expense = g.Where(t => t.TransactionType == 0).Sum(t => t.Amount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        var result = new List<MonthlyTrendData>();
        for (int i = 1; i <= 12; i++)
        {
            var monthData = monthlyData.FirstOrDefault(m => m.Month == i);
            var income = monthData?.Income ?? 0;
            var expense = monthData?.Expense ?? 0;
            
            result.Add(new MonthlyTrendData
            {
                Month = i,
                MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                Income = income,
                Expense = expense,
                Savings = income - expense
            });
        }
        return result;
    }

    // Private DTOs for internal use
    private class CashFlowData
    {
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal NetCashFlow { get; set; }
        public List<CategoryGroupData> CategoryGroups { get; set; } = new List<CategoryGroupData>();
    }

    private class CategoryGroupData
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    private class CategoryStatData
    {
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Total { get; set; }
        public decimal Average { get; set; }
    }

    private class MonthlyTrendData
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Savings { get; set; }
    }
}
