using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
