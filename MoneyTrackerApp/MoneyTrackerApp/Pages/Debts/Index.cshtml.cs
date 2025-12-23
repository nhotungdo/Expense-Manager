using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Debts;

[Authorize]
public class IndexModel : PageModel
{
    public string CurrentUserId { get; private set; } = string.Empty;

    public void OnGet()
    {
        CurrentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}
