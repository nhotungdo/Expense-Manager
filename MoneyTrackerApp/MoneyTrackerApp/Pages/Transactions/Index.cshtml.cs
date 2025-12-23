using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Transactions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        public IndexModel(ITransactionService transactionService, IAccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
        }

        public List<TransactionResponseDto> Transactions { get; set; } = new();
        public decimal TotalBalance { get; set; }
        public string Currency { get; set; } = "VND";

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                // Fetch Total Balance
                var wallets = await _accountService.GetUserAccountsAsync(userId);
                TotalBalance = wallets.Where(w => w.IncludeInTotal).Sum(w => w.CurrentBalance);
                if (wallets.Any()) Currency = wallets.First().Currency;

                // Fetch Recent Transactions (Global)
                Transactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
                {
                    PageNumber = 1,
                    PageSize = 50 // Show reasonable amount
                });
            }
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
