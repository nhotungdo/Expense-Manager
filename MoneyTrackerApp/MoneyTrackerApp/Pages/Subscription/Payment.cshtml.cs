using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Subscription
{
    [Authorize]
    public class PaymentModel : PageModel
    {
        public int PackageId { get; set; }

        public void OnGet(int packageId)
        {
            PackageId = packageId;
        }
    }
}
