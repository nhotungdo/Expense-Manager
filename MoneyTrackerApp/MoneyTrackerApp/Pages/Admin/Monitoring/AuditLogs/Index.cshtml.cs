using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Pages.Admin.Monitoring.AuditLogs
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
         private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public List<AuditLogDto> Logs { get; set; } = new();
        public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync(int page = 1)
        {
            PageNumber = page;
            int pageSize = 50;

            var logs = await _context.AuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Logs = logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserEmail = l.User?.Email ?? "System",
                Action = l.Action,
                EntityName = l.EntityType ?? "N/A",
                EntityId = l.EntityId?.ToString() ?? "N/A",
                Details = l.Details,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            }).ToList();
        }

        public class AuditLogDto
        {
            public long Id { get; set; }
            public string UserEmail { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string EntityName { get; set; } = string.Empty;
            public string EntityId { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public string UserAgent { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
        }
    }
}
