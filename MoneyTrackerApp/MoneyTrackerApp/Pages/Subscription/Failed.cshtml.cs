using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Subscription;

public class FailedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Message { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PackageId { get; set; }

    public void OnGet()
    {
    }
}
