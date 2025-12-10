using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MoneyTrackerApp.Configurations;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly AdminOptions _adminOptions;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IAdminDashboardService dashboardService,
            IOptions<AdminOptions> adminOptions,
            ILogger<DashboardModel> logger)
        {
            _dashboardService = dashboardService;
            _adminOptions = adminOptions.Value;
            _logger = logger;
        }

        public AdminDashboardDto DashboardData { get; private set; } = new();
        public string? AdminEmail => _adminOptions.DashboardEmail;

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            // Primary gate: role-based admin
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            // Backward-compatible gate: configured admin email
            var isConfiguredAdmin = !string.IsNullOrWhiteSpace(_adminOptions.DashboardEmail) &&
                                    string.Equals(email, _adminOptions.DashboardEmail, StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isConfiguredAdmin)
            {
                TempData["Message"] = "Bạn không có quyền truy cập bảng điều khiển quản trị.";
                return RedirectToPage("/Dashboard");
            }

            DashboardData = await _dashboardService.GetSummaryAsync(cancellationToken);
            return Page();
        }
    }
}
