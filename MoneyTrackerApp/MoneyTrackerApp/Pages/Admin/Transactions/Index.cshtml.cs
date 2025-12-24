using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin.Transactions
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ITransactionService _transactionService;

        public IndexModel(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [BindProperty(SupportsGet = true)]
        public TransactionFilterDto Filter { get; set; } = new TransactionFilterDto { PageNumber = 1, PageSize = 20 };

        public List<TransactionResponseDto> Transactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            Transactions = await _transactionService.GetAllTransactionsForAdminAsync(Filter);
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id, long userId)
        {
            // Note: Admin deletion might need a special Admin method if standard delete involves strict permission checks against userId
            // But TransactionService.DeleteTransactionAsync takes userId. 
            // In Admin context, we usually want to delete ON BEHALF of the user or as SYSTEM.
            // If the service checks "Does User X own Transaction Y", we must pass the transaction's owner ID.
            
            // For now, assuming we get the userId from the form (which we should).
            await _transactionService.DeleteTransactionAsync(id, userId);
            
            return RedirectToPage();
        }
    }
}
