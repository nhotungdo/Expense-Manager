using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages.Subscription
{
    public class ProcessingModel : PageModel
    {
        public string TransactionId { get; set; }

        public void OnGet(string transaction)
        {
            TransactionId = transaction;
        }
    }
}
