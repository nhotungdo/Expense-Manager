using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages;

[Authorize]
public class ReportsModel : PageModel
{
    public void OnGet()
    {
    }
}
