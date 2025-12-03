using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Profile
{
    [Authorize]
    public class MyWalletModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<MyWalletModel> _logger;

        public MyWalletModel(
            IAccountService accountService, 
            ITransactionService transactionService,
            ILogger<MyWalletModel> logger)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _logger = logger;
        }

        public List<AccountResponseDto> Wallets { get; set; } = new();
        public List<TransactionResponseDto> RecentTransactions { get; set; } = new();
        public decimal TotalBalance { get; set; }
        public string DefaultCurrency { get; set; } = "VND";
        public long SelectedWalletId { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                Wallets = await _accountService.GetUserAccountsAsync(userId);
                
                if (Wallets.Any())
                {
                    DefaultCurrency = Wallets.First().Currency;
                    TotalBalance = Wallets.Where(w => w.IncludeInTotal).Sum(w => w.CurrentBalance);
                    
                    // Select first wallet by default
                    SelectedWalletId = Wallets.First().Id;
                    
                    // Fetch transactions for the first wallet
                    RecentTransactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
                    {
                        AccountId = SelectedWalletId,
                        PageSize = 20,
                        PageNumber = 1
                    });
                }
            }
        }

        public async Task<IActionResult> OnGetWalletTransactionsAsync(long walletId)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var transactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
            {
                AccountId = walletId,
                PageSize = 20,
                PageNumber = 1
            });

            return Partial("_WalletTransactions", transactions);
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
