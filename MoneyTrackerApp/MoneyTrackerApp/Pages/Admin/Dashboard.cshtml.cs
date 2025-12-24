using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin
{
    // [Authorize(Roles = "Admin")] // Uncomment when Authentication is ready
    public class DashboardModel : PageModel
    {
        private readonly IAdminDashboardService _dashboardService;

        public DashboardModel(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public AdminDashboardDto Data { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Data = await _dashboardService.GetSummaryAsync(cancellationToken);
        }
    }
}
