using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.InteropServices;

namespace MoneyTrackerApp.Pages.Admin.Monitoring.Logs
{
    public class IndexModel : PageModel
    {
        public SystemInfo Info { get; set; } = new();
        public List<LogEntry> SystemLogs { get; set; } = new();

        public void OnGet()
        {
            Info = new SystemInfo
            {
                OSDescription = RuntimeInformation.OSDescription,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                MachineName = Environment.MachineName,
                Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss")
            };

            // Mock System Logs
            SystemLogs = new List<LogEntry>
            {
                new LogEntry { Timestamp = DateTime.Now.AddMinutes(-2), Level = "INFO", Message = "System health check completed. All services operational." },
                new LogEntry { Timestamp = DateTime.Now.AddMinutes(-15), Level = "INFO", Message = "Database connection pool refreshed." },
                new LogEntry { Timestamp = DateTime.Now.AddHours(-1), Level = "WARN", Message = "High memory usage detected (78%). Garbage collection triggered." },
                new LogEntry { Timestamp = DateTime.Now.AddHours(-2), Level = "INFO", Message = "Scheduled backup job 'DailyBackup' completed successfully." },
                new LogEntry { Timestamp = DateTime.Now.AddHours(-5), Level = "ERROR", Message = "Failed to send email to user@example.com. Retry in 5 minutes." },
                new LogEntry { Timestamp = DateTime.Now.AddHours(-5).AddMinutes(-1), Level = "INFO", Message = "Email service started." },
                new LogEntry { Timestamp = DateTime.Now.AddHours(-12), Level = "INFO", Message = "Application startup sequence initiated." },
            };
        }

        public class SystemInfo
        {
            public string OSDescription { get; set; } = string.Empty;
            public string FrameworkDescription { get; set; } = string.Empty;
            public string ProcessArchitecture { get; set; } = string.Empty;
            public int ProcessorCount { get; set; }
            public string MachineName { get; set; } = string.Empty;
            public string Uptime { get; set; } = string.Empty;
        }

        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}
