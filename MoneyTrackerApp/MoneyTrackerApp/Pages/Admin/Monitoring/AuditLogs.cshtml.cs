using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Admin.Monitoring
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}


