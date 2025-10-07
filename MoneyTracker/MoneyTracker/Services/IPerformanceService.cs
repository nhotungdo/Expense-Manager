using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Services
{
    public interface IPerformanceService
    {
        Task OptimizeDatabaseAsync();
        Task CleanupOldDataAsync();
        Task RebuildIndexesAsync();
        Task AnalyzeQueryPerformanceAsync();
        Task<DatabaseStats> GetDatabaseStatsAsync();
    }

    public class DatabaseStats
    {
        public int TotalUsers { get; set; }
        public int TotalExpenses { get; set; }
        public int TotalIncomes { get; set; }
        public int TotalCategories { get; set; }
        public int TotalAuditLogs { get; set; }
        public long DatabaseSizeBytes { get; set; }
        public List<string> SlowQueries { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}
