using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IAdminDashboardService _dashboardService;

        public DashboardModel(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public AdminDashboardDto DashboardData { get; set; } = new();

        public async Task OnGetAsync()
        {
            DashboardData = await _dashboardService.GetDashboardDataAsync();
        }

        public async Task<IActionResult> OnPostToggleMaintenanceAsync(bool enable)
        {
            await _dashboardService.ToggleMaintenanceModeAsync(enable);
            return RedirectToPage();
        }
    }
}
