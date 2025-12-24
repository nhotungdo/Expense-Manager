using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages.Groups;

[Authorize]
public class DetailsModel : PageModel
{
    public int GroupId { get; private set; }
    public string CurrentUserId { get; private set; } = "";

    public IActionResult OnGet(int id)
    {
        GroupId = id;
        CurrentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            return RedirectToPage("/Auth/Login");
        }
        
        return Page();
    }
}
