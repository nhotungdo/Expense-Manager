using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages;

[Authorize]
public class GroupsModel : PageModel
{
    public void OnGet()
    {
    }
}
