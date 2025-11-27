using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToPage("/Index");
        }

        public IActionResult OnPost()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToPage("/Index");
        }
    }
}
