using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages;

[Authorize]
public class NotificationsModel : PageModel
{
    public void OnGet()
    {
    }
}
