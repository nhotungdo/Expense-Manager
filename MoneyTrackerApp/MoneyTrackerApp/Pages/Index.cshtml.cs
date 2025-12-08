using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public DashboardOverviewDto DashboardData { get; set; }

        public async Task OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (long.TryParse(userIdStr, out var userId))
                {
                    DashboardData = await _reportService.GetDashboardOverviewAsync(userId);
                }
            }
        }
    }
}