using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Budgets;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
