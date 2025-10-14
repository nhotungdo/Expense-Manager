using ClosedXML.Excel;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, ITransactionService transactionService, ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<dynamic> GetSummaryReportAsync(long userId, string period = "month")
    {
        var (startDate, endDate) = GetPeriodDates(period);
        var (totalIncome, totalExpense, netIncome) = await _transactionService.GetUserSummaryAsync(userId, startDate, endDate);

        return new
        {
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetIncome = netIncome,
            SavingsRate = totalIncome > 0 ? (netIncome / totalIncome) * 100 : 0
        };
    }

    public async Task<IEnumerable<dynamic>> GetCategoryBreakdownReportAsync(long userId, string period = "month")
    {
        var (startDate, endDate) = GetPeriodDates(period);
        return await _transactionService.GetCategoryBreakdownAsync(userId, startDate, endDate);
    }

    public async Task<IEnumerable<dynamic>> GetIncomeExpenseTrendReportAsync(long userId, string period = "year")
    {
        var months = period switch
        {
            "year" => 12,
            "month" => 1,
            "quarter" => 3,
            _ => 12
        };

        return await _transactionService.GetIncomeExpenseTrendAsync(userId, months);
    }

    public async Task<byte[]> ExportTransactionsAsync(long userId, string format, DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t =>
            t.UserId == userId &&
            t.TransactionDate >= startDate &&
            t.TransactionDate <= endDate);

        return format.ToLower() switch
        {
            "excel" => await ExportToExcelAsync(transactions),
            "csv" => await ExportToCsvAsync(transactions),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    public async Task<Report> GenerateReportAsync(long userId, string reportType, DateTime startDate, DateTime endDate, string? parameters = null)
    {
        var report = new Report
        {
            UserId = userId,
            ReportType = reportType,
            ReportName = $"{reportType} Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            StartDate = DateOnly.FromDateTime(startDate),
            EndDate = DateOnly.FromDateTime(endDate),
            Parameters = parameters,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Generated report {ReportId} for user {UserId}", report.Id, userId);
        return report;
    }

    private (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
    {
        var now = DateTime.UtcNow;

        return period.ToLower() switch
        {
            "week" => (now.AddDays(-7), now),
            "month" => (now.AddMonths(-1), now),
            "quarter" => (now.AddMonths(-3), now),
            "year" => (now.AddYears(-1), now),
            _ => (now.AddMonths(-1), now)
        };
    }

    private Task<byte[]> ExportToExcelAsync(IEnumerable<Transaction> transactions)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        // Headers
        worksheet.Cell(1, 1).Value = "Date";
        worksheet.Cell(1, 2).Value = "Type";
        worksheet.Cell(1, 3).Value = "Amount";
        worksheet.Cell(1, 4).Value = "Category";
        worksheet.Cell(1, 5).Value = "Note";

        // Data
        var row = 2;
        foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
        {
            worksheet.Cell(row, 1).Value = transaction.TransactionDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 2).Value = transaction.Type.ToString();
            worksheet.Cell(row, 3).Value = transaction.Amount;
            worksheet.Cell(row, 4).Value = transaction.Category?.Name ?? "Uncategorized";
            worksheet.Cell(row, 5).Value = transaction.Description ?? "";
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    private Task<byte[]> ExportToCsvAsync(IEnumerable<Transaction> transactions)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Date,Type,Amount,Category,Note");

        foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
        {
            csv.AppendLine($"{transaction.TransactionDate:yyyy-MM-dd},{transaction.Type},{transaction.Amount},{transaction.Category?.Name ?? "Uncategorized"},\"{transaction.Description ?? ""}\"");
        }

        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv.ToString()));
    }
}
