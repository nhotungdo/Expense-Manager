using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public interface IReportExportService
    {
        Task<byte[]> ExportToPdfAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly");
        Task<byte[]> ExportToExcelAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly");
        Task<byte[]> ExportToCsvAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly");
    }
}
