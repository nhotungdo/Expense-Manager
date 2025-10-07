using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<PerformanceService> _logger;

        public PerformanceService(ExpenseManagerContext context, ILogger<PerformanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OptimizeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Starting database optimization");

                // Update statistics
                await _context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS");

                // Rebuild indexes
                await RebuildIndexesAsync();

                // Clean up old data
                await CleanupOldDataAsync();

                _logger.LogInformation("Database optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database optimization");
                throw;
            }
        }

        public async Task CleanupOldDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data cleanup");

                var cutoffDate = DateTime.UtcNow.AddYears(-2);

                // Clean up old audit logs (keep only 2 years)
                var oldAuditLogs = await _context.AuditLogs
                    .Where(a => a.CreatedAt < cutoffDate)
                    .CountAsync();

                if (oldAuditLogs > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM audit_logs WHERE created_at < {0}", cutoffDate);
                    _logger.LogInformation("Cleaned up {Count} old audit logs", oldAuditLogs);
                }

                // Clean up old AI suggestions (keep only 1 year)
                var oldSuggestions = await _context.AiSuggestions
                    .Where(a => a.CreatedAt < DateTime.UtcNow.AddYears(-1))
                    .CountAsync();

                if (oldSuggestions > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM ai_suggestions WHERE created_at < {0}", DateTime.UtcNow.AddYears(-1));
                    _logger.LogInformation("Cleaned up {Count} old AI suggestions", oldSuggestions);
                }

                // Clean up old email records (keep only 6 months)
                var oldEmails = await _context.Emails
                    .Where(e => e.CreatedAt < DateTime.UtcNow.AddMonths(-6))
                    .CountAsync();

                if (oldEmails > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM emails WHERE created_at < {0}", DateTime.UtcNow.AddMonths(-6));
                    _logger.LogInformation("Cleaned up {Count} old email records", oldEmails);
                }

                _logger.LogInformation("Data cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data cleanup");
                throw;
            }
        }

        public async Task RebuildIndexesAsync()
        {
            try
            {
                _logger.LogInformation("Starting index rebuild");

                // Rebuild indexes for frequently queried tables
                var indexCommands = new[]
                {
                    "ALTER INDEX ALL ON users REBUILD",
                    "ALTER INDEX ALL ON expenses REBUILD",
                    "ALTER INDEX ALL ON incomes REBUILD",
                    "ALTER INDEX ALL ON categories REBUILD",
                    "ALTER INDEX ALL ON audit_logs REBUILD"
                };

                foreach (var command in indexCommands)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(command);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to rebuild index with command: {Command}", command);
                    }
                }

                _logger.LogInformation("Index rebuild completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during index rebuild");
                throw;
            }
        }

        public async Task AnalyzeQueryPerformanceAsync()
        {
            try
            {
                _logger.LogInformation("Starting query performance analysis");

                // Analyze query performance using SQL Server DMVs
                var slowQueries = await _context.Database.SqlQueryRaw<string>(
                    @"SELECT TOP 10 
                        SUBSTRING(qt.text, (qs.statement_start_offset/2) + 1,
                        ((CASE WHEN qs.statement_end_offset = -1 
                        THEN LEN(CONVERT(nvarchar(max), qt.text)) * 2 
                        ELSE qs.statement_end_offset 
                        END - qs.statement_start_offset)/2) + 1) AS query_text,
                        qs.execution_count,
                        qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time
                      FROM sys.dm_exec_query_stats qs
                      CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
                      WHERE qs.total_elapsed_time / qs.execution_count > 1000000 -- > 1 second
                      ORDER BY avg_elapsed_time DESC").ToListAsync();

                if (slowQueries.Any())
                {
                    _logger.LogWarning("Found {Count} slow queries", slowQueries.Count);
                    foreach (var query in slowQueries)
                    {
                        _logger.LogWarning("Slow query: {Query}", query);
                    }
                }

                _logger.LogInformation("Query performance analysis completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during query performance analysis");
                // Don't throw - this is not critical
            }
        }

        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            try
            {
                var stats = new DatabaseStats();

                // Get table counts
                stats.TotalUsers = await _context.Users.CountAsync();
                stats.TotalExpenses = await _context.Expenses.CountAsync();
                stats.TotalIncomes = await _context.Incomes.CountAsync();
                stats.TotalCategories = await _context.Categories.CountAsync();
                stats.TotalAuditLogs = await _context.AuditLogs.CountAsync();

                // Get database size
                try
                {
                    var dbSize = await _context.Database.SqlQueryRaw<long>(
                        "SELECT SUM(size * 8 * 1024) FROM sys.database_files").FirstOrDefaultAsync();
                    stats.DatabaseSizeBytes = dbSize;
                }
                catch
                {
                    stats.DatabaseSizeBytes = 0;
                }

                // Get slow queries
                try
                {
                    stats.SlowQueries = await _context.Database.SqlQueryRaw<string>(
                        @"SELECT TOP 5 
                            SUBSTRING(qt.text, 1, 200) AS query_text
                          FROM sys.dm_exec_query_stats qs
                          CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
                          WHERE qs.total_elapsed_time / qs.execution_count > 1000000
                          ORDER BY qs.total_elapsed_time / qs.execution_count DESC").ToListAsync();
                }
                catch
                {
                    // Ignore if DMVs are not accessible
                }

                // Generate recommendations
                GenerateRecommendations(stats);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database stats");
                throw;
            }
        }

        private void GenerateRecommendations(DatabaseStats stats)
        {
            var recommendations = new List<string>();

            if (stats.TotalAuditLogs > 100000)
            {
                recommendations.Add("Consider archiving old audit logs to improve performance");
            }

            if (stats.DatabaseSizeBytes > 1024 * 1024 * 1024) // 1GB
            {
                recommendations.Add("Database size is large. Consider data archiving strategy");
            }

            if (stats.SlowQueries.Count > 0)
            {
                recommendations.Add("Found slow queries. Consider adding indexes or optimizing queries");
            }

            if (stats.TotalExpenses > 1000000)
            {
                recommendations.Add("Large number of expenses. Consider partitioning by date");
            }

            stats.Recommendations = recommendations;
        }
    }
}
