using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return Redirect("/home");
            }
            return Page();
        }
    }
}