using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IReportService
{
    Task<dynamic> GetSummaryReportAsync(long userId, string period = "month");
    Task<IEnumerable<dynamic>> GetCategoryBreakdownReportAsync(long userId, string period = "month");
    Task<IEnumerable<dynamic>> GetIncomeExpenseTrendReportAsync(long userId, string period = "year");
    Task<byte[]> ExportTransactionsAsync(long userId, string format, DateTime startDate, DateTime endDate);
    Task<Report> GenerateReportAsync(long userId, string reportType, DateTime startDate, DateTime endDate, string? parameters = null);
}
