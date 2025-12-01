using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Transactions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ICategoryService _categoryService;

    public CreateModel(
        ITransactionService transactionService,
        IAccountService accountService,
        ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
    }

    [BindProperty]
    public CreateTransactionDto Transaction { get; set; } = new();

    public SelectList? AccountList { get; set; }
    public List<CategorySummaryDto> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDropdownsAsync();
        Transaction.TransactionDate = DateTime.Today;
        Transaction.Currency = "VND"; // Default currency
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(idValue, out var userId))
            {
                return Unauthorized();
            }
            await _transactionService.CreateTransactionAsync(userId, Transaction);
            return RedirectToPage("/Transactions/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(idValue, out var userId))
        {
            userId = 0;
        }
        
        var accounts = await _accountService.GetAccountSummariesAsync(userId);
        AccountList = new SelectList(accounts ?? Enumerable.Empty<AccountSummaryDto>(), "Id", "Name");

        Categories = await _categoryService.GetCategorySummariesAsync(userId) ?? new List<CategorySummaryDto>();
    }
}
