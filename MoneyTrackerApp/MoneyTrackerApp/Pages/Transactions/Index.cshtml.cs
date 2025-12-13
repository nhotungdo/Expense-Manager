using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Transactions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly IReportService _reportService;
    private readonly IAccountService _accountService;
    private readonly ICategoryService _categoryService;

    public IndexModel(
        ITransactionService transactionService,
        IReportService reportService,
        IAccountService accountService,
        ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _reportService = reportService;
        _accountService = accountService;
        _categoryService = categoryService;
    }

    [BindProperty(SupportsGet = true)]
    public TransactionFilterDto Filter { get; set; } = new();

    public TransactionListViewModel ViewModel { get; set; } = new();

    public SelectList? AccountList { get; set; }
    public SelectList? CategoryList { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        // Initialize defaults if not set
        if (!Filter.StartDate.HasValue)
        {
            // Default to current month
            var now = DateTime.Today;
            Filter.StartDate = new DateTime(now.Year, now.Month, 1);
            Filter.EndDate = Filter.StartDate.Value.AddMonths(1).AddDays(-1);
        }

        // 1. Get Summary (Total Income, Expense, Net)
        // Ensure EndDate is included fully if it's just a date (add 23:59:59 mainly handled by DB or service logic but let's be safe if service expects date)
        // ReportService uses <= EndDate, so date part is fine if they are dates.
        var cashFlow = await _reportService.GenerateCashFlowReportAsync(userId, Filter.StartDate.Value, Filter.EndDate ?? DateTime.Today);
        
        ViewModel.TotalIncome = cashFlow.TotalIncome;
        ViewModel.TotalExpense = cashFlow.TotalExpense;
        ViewModel.NetIncome = cashFlow.NetCashFlow;

        // 2. Get Transactions
        ViewModel.Transactions = await _transactionService.GetUserTransactionsAsync(userId, Filter);

        // 3. Load Filter Dropdowns
        await LoadDropdownsAsync(userId);

        return Page();
    }

    private async Task LoadDropdownsAsync(long userId)
    {
        var accounts = await _accountService.GetAccountSummariesAsync(userId);
        AccountList = new SelectList(accounts ?? Enumerable.Empty<AccountSummaryDto>(), "Id", "Name", Filter.AccountId);

        var categories = await _categoryService.GetCategorySummariesAsync(userId);
        // Flatten or use as is. Since SelectList doesn't support groups easily without work, simple list for now.
        // Assuming CategorySummaryDto has Name.
        CategoryList = new SelectList(categories ?? Enumerable.Empty<CategorySummaryDto>(), "Id", "Name", Filter.CategoryId);
    }
}

public class TransactionListViewModel
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetIncome { get; set; }
    public List<TransactionResponseDto> Transactions { get; set; } = new();
}
