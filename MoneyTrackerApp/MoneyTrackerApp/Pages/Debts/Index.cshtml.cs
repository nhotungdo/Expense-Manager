using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Debts;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
