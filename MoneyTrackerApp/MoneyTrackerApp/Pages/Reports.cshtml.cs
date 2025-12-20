using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MoneyTrackerApp.Pages
{
    /// <summary>
    /// Reports page model with proper authorization and error handling
    /// </summary>
    [Authorize]
    public class ReportsModel : PageModel
    {
        private readonly ILogger<ReportsModel> _logger;

        public ReportsModel(ILogger<ReportsModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle GET request for Reports page
        /// </summary>
        public void OnGet()
        {
            try
            {
                // Log page access
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("User {UserId} accessed Reports page at {Time}", 
                    userId, DateTime.UtcNow);

                // Page will be rendered with client-side data loading
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Reports page");
                // Page will still render, errors will be handled client-side
            }
        }
    }
}
