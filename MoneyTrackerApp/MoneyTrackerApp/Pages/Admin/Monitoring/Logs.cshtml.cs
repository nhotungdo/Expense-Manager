using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Admin.Monitoring
{
    [Authorize(Roles = "Admin")]
    public class LogsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

