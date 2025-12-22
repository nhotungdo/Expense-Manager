using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    public string CurrentUserId { get; private set; } = "";

    public void OnGet()
    {
        CurrentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}
