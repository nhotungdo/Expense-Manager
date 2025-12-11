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
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<MyWalletModel> _logger;

        public MyWalletModel(
            IAccountService accountService, 
            ITransactionService transactionService,
            ISubscriptionService subscriptionService,
            ILogger<MyWalletModel> logger)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public List<AccountResponseDto> Wallets { get; set; } = new();
        public List<TransactionResponseDto> RecentTransactions { get; set; } = new();
        public SubscriptionDto? CurrentSubscription { get; set; }
        public decimal TotalBalance { get; set; }
        public string DefaultCurrency { get; set; } = "VND";
        public long SelectedWalletId { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                Wallets = await _accountService.GetUserAccountsAsync(userId);
                CurrentSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
                
                if (Wallets.Any())
                {
                    DefaultCurrency = Wallets.First().Currency;
                    TotalBalance = Wallets.Where(w => w.IncludeInTotal).Sum(w => w.CurrentBalance);
                    
                    // Select first wallet by default
                    SelectedWalletId = Wallets.First().Id;
                    
                    // Fetch transactions
                    RecentTransactions = await _transactionService.GetUserTransactionsAsync(userId, new TransactionFilterDto
                    {
                        PageSize = 20,
                        PageNumber = 1
                    });
                }
            }
        }

        public async Task<IActionResult> OnGetWalletTransactionsAsync(long? walletId, string? filterType, string? dateRange)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var filter = new TransactionFilterDto
            {
                AccountId = walletId ?? 0, // 0 means all if filtered by "All"
                PageSize = 20,
                PageNumber = 1
                // Todo: Implement date range and type fitlering in service if needed
            };
            
            // Basic filtering mapping for demo
            if (!string.IsNullOrEmpty(filterType) && filterType != "All")
            {
                // Map filterType to TransactionType if possible, currently simple fetch
            }

            var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);

            return Partial("_WalletTransactions", transactions);
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
