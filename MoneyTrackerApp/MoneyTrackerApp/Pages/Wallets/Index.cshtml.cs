using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Wallets
{
    public class WalletIndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISessionService _sessionService;
        private readonly IAccountService _accountService;

        public WalletIndexModel(
            ExpenseManagerContext context,
            ISubscriptionService subscriptionService,
            ISessionService sessionService,
            IAccountService accountService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _sessionService = sessionService;
            _accountService = accountService;
        }

        public IList<Account> Wallets { get; set; } = new List<Account>();
        public int MaxWallets { get; set; } = 3; // Default for Free
        public int CurrentWalletCount { get; set; }
        public bool CanCreateMore { get; set; }
        public bool IsPro { get; set; }

        [BindProperty]
        public CreateAccountDto NewWallet { get; set; } = new CreateAccountDto { Currency = "VND", Color = "#667eea", Icon="fas fa-wallet", IncludeInTotal=true };

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            await LoadWalletData(user.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid)
            {
                await LoadWalletData(user.Id);
                return Page();
            }

            try
            {
                await _accountService.CreateAccountAsync(user.Id, NewWallet);
                TempData["SuccessMessage"] = "Tạo ví thành công!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadWalletData(user.Id);
                return Page();
            }
        }

        private async Task LoadWalletData(long userId)
        {
            // Get Subscription
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            
            // Determine if user has Pro account
            // PackageId 1 is Free, anything else (2=Pro, 3=Team, etc.) is considered Pro
            if (subscription != null && subscription.PackageId != 1)
            {
                // Pro account: unlimited wallets
                IsPro = true;
                MaxWallets = 9999; // Represent unlimited with a very high number
            }
            else
            {
                // Free account: maximum 3 wallets
                IsPro = false;
                MaxWallets = 3;
            }

            // Get Wallets
            Wallets = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .Include(a => a.User)
                .OrderByDescending(a => a.IsActive)
                .ThenBy(a => a.Name)
                .ToListAsync();

            CurrentWalletCount = Wallets.Count;
            CanCreateMore = IsPro || CurrentWalletCount < MaxWallets;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    var token = HttpContext.Request.Cookies["accessToken"];
                    if (string.IsNullOrEmpty(token)) return null;
                    authHeader = $"Bearer {token}";
                }

                var tokenString = authHeader.Substring("Bearer ".Length);
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(tokenString))
                {
                    var token = handler.ReadJwtToken(tokenString);
                    var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                    {
                        return await _context.Users.FindAsync(userId);
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
