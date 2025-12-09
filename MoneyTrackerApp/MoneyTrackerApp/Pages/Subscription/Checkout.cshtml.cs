using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Subscription;

[Authorize]
public class CheckoutModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int PackageId { get; set; }

    public IActionResult OnGet()
    {
        if (PackageId <= 0)
        {
            return RedirectToPage("/ServicePackages");
        }

        return Page();
    }
}
