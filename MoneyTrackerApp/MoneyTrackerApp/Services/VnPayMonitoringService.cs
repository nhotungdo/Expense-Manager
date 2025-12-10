using System.Collections.Concurrent;
using System.Text.Json;

namespace MoneyTrackerApp.Services
{
    /// <summary>
    /// Monitoring and alerting service for VNPay transactions
    /// </summary>
    public class VnPayMonitoringService
    {
        private readonly ILogger<VnPayMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, ErrorMetrics> _errorMetrics = new();
        private static readonly ConcurrentQueue<TransactionLog> _recentTransactions = new();
        private const int MAX_RECENT_TRANSACTIONS = 100;

        public VnPayMonitoringService(
            ILogger<VnPayMonitoringService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public class ErrorMetrics
        {
            public int Count { get; set; }
            public DateTime FirstOccurrence { get; set; }
            public DateTime LastOccurrence { get; set; }
            public List<string> RecentMessages { get; set; } = new();
        }

        public class TransactionLog
        {
            public DateTime Timestamp { get; set; }
            public long UserId { get; set; }
            public int PackageId { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? ErrorMessage { get; set; }
            public long? TransactionId { get; set; }
            public string? TxnRef { get; set; }
        }

        /// <summary>
        /// Log transaction attempt
        /// </summary>
        public void LogTransaction(TransactionLog log)
        {
            _recentTransactions.Enqueue(log);
            
            // Keep only recent transactions
            while (_recentTransactions.Count > MAX_RECENT_TRANSACTIONS)
            {
                _recentTransactions.TryDequeue(out _);
            }

            var logJson = JsonSerializer.Serialize(log, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            if (log.Status == "Success")
            {
                _logger.LogInformation("VNPay Transaction Success: {Log}", logJson);
            }
            else
            {
                _logger.LogWarning("VNPay Transaction Failed: {Log}", logJson);
            }
        }

        /// <summary>
        /// Track error occurrence
        /// </summary>
        public void TrackError(string errorType, string errorMessage)
        {
            var metrics = _errorMetrics.GetOrAdd(errorType, _ => new ErrorMetrics
            {
                FirstOccurrence = DateTime.UtcNow
            });

            metrics.Count++;
            metrics.LastOccurrence = DateTime.UtcNow;
            metrics.RecentMessages.Add($"[{DateTime.UtcNow:HH:mm:ss}] {errorMessage}");

            // Keep only last 10 messages
            if (metrics.RecentMessages.Count > 10)
            {
                metrics.RecentMessages.RemoveAt(0);
            }

            // Check if we need to send alert
            CheckAndSendAlert(errorType, metrics);
        }

        /// <summary>
        /// Check if alert threshold is reached and send alert
        /// </summary>
        private void CheckAndSendAlert(string errorType, ErrorMetrics metrics)
        {
            var alertThreshold = _configuration.GetValue<int>("VnPayMonitoring:AlertThreshold", 10);
            var alertTimeWindowMinutes = _configuration.GetValue<int>("VnPayMonitoring:AlertTimeWindowMinutes", 5);

            var recentErrorCount = metrics.RecentMessages.Count;
            var timeSpan = DateTime.UtcNow - metrics.FirstOccurrence;

            if (recentErrorCount >= alertThreshold && timeSpan.TotalMinutes <= alertTimeWindowMinutes)
            {
                SendAlert(errorType, metrics);
            }
        }

        /// <summary>
        /// Send alert (can be extended to send email, SMS, etc.)
        /// </summary>
        private void SendAlert(string errorType, ErrorMetrics metrics)
        {
            var alert = new
            {
                AlertType = "VNPay Error Threshold Exceeded",
                ErrorType = errorType,
                ErrorCount = metrics.Count,
                FirstOccurrence = metrics.FirstOccurrence,
                LastOccurrence = metrics.LastOccurrence,
                RecentMessages = metrics.RecentMessages,
                Timestamp = DateTime.UtcNow
            };

            var alertJson = JsonSerializer.Serialize(alert, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogCritical("🚨 VNPay ALERT: {Alert}", alertJson);

            // TODO: Implement actual alerting mechanism
            // - Send email to admin
            // - Send SMS
            // - Post to Slack/Teams
            // - Create incident ticket
        }

        /// <summary>
        /// Get error statistics
        /// </summary>
        public Dictionary<string, ErrorMetrics> GetErrorStatistics()
        {
            return new Dictionary<string, ErrorMetrics>(_errorMetrics);
        }

        /// <summary>
        /// Get recent transactions
        /// </summary>
        public List<TransactionLog> GetRecentTransactions(int count = 20)
        {
            return _recentTransactions.TakeLast(count).ToList();
        }

        /// <summary>
        /// Get success rate
        /// </summary>
        public (int total, int success, double successRate) GetSuccessRate()
        {
            var transactions = _recentTransactions.ToList();
            var total = transactions.Count;
            var success = transactions.Count(t => t.Status == "Success");
            var successRate = total > 0 ? (double)success / total * 100 : 0;

            return (total, success, successRate);
        }

        /// <summary>
        /// Clear old metrics (should be called periodically)
        /// </summary>
        public void ClearOldMetrics(TimeSpan olderThan)
        {
            var cutoffTime = DateTime.UtcNow - olderThan;
            
            var keysToRemove = _errorMetrics
                .Where(kvp => kvp.Value.LastOccurrence < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _errorMetrics.TryRemove(key, out _);
            }

            _logger.LogInformation("Cleared {Count} old error metrics", keysToRemove.Count);
        }

        /// <summary>
        /// Generate health report
        /// </summary>
        public object GenerateHealthReport()
        {
            var (total, success, successRate) = GetSuccessRate();
            var errorStats = GetErrorStatistics();

            return new
            {
                Timestamp = DateTime.UtcNow,
                Transactions = new
                {
                    Total = total,
                    Success = success,
                    Failed = total - success,
                    SuccessRate = $"{successRate:F2}%"
                },
                Errors = errorStats.Select(kvp => new
                {
                    Type = kvp.Key,
                    Count = kvp.Value.Count,
                    FirstOccurrence = kvp.Value.FirstOccurrence,
                    LastOccurrence = kvp.Value.LastOccurrence,
                    RecentMessages = kvp.Value.RecentMessages.TakeLast(3)
                }),
                Status = successRate >= 95 ? "Healthy" : successRate >= 80 ? "Degraded" : "Critical"
            };
        }
    }
}
