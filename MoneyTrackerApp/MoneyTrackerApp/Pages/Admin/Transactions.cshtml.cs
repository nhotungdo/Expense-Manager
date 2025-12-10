using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TransactionsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

