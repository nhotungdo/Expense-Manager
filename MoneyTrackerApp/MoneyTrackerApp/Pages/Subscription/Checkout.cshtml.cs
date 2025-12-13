using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Subscription;

public class CheckoutModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int PackageId { get; set; } = 2;

    public long UserId { get; private set; }

    public void OnGet()
    {
        if (PackageId <= 0) PackageId = 2;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(claim, out var id))
        {
            UserId = id;
        }
        else
        {
            UserId = 0; // guest/placeholder
        }
    }
}



