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
    private readonly ISavingsGoalService _savingsGoalService;

    public CreateModel(
        ITransactionService transactionService,
        IAccountService accountService,
        ICategoryService categoryService,
        ISavingsGoalService savingsGoalService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
        _savingsGoalService = savingsGoalService;
    }

    [BindProperty]
    public CreateTransactionDto Transaction { get; set; } = new();

    [BindProperty]
    public long? Id { get; set; }

    [BindProperty]
    public long? SavingsGoalId { get; set; }

    public bool IsEditMode => Id.HasValue;

    public SelectList? AccountList { get; set; }
    public SelectList? SavingsGoalList { get; set; }
    public List<CategorySummaryDto> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(long? id, long? goalId)
    {
        await LoadDropdownsAsync();

        if (goalId.HasValue)
        {
            SavingsGoalId = goalId.Value;
        }
        
        if (id.HasValue)
        {
            var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(idValue, out var userId)) return Unauthorized();

            var existingTransaction = await _transactionService.GetTransactionByIdAsync(id.Value, userId);
            if (existingTransaction == null) return NotFound();

            Id = id;
            Transaction = new CreateTransactionDto
            {
                AccountId = existingTransaction.AccountId,
                CategoryId = existingTransaction.CategoryId,
                TransactionType = existingTransaction.TransactionType,
                Amount = existingTransaction.Amount,
                Currency = existingTransaction.Currency,
                Note = existingTransaction.Note,
                TransactionDate = existingTransaction.TransactionDate,
                PairedAccountId = existingTransaction.PairedAccountId,
                AttachmentUrl = existingTransaction.AttachmentUrl,
                OcrText = existingTransaction.OcrText
            };
        }
        else
        {
            Transaction.TransactionDate = DateTime.Today;
            Transaction.Currency = "VND"; // Default currency
        }
        
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
            if (Id.HasValue)
            {
                var updateDto = new UpdateTransactionDto
                {
                    Id = Id.Value,
                    CategoryId = Transaction.CategoryId,
                    Amount = Transaction.Amount,
                    Note = Transaction.Note,
                    TransactionDate = Transaction.TransactionDate,
                    AttachmentUrl = Transaction.AttachmentUrl
                };
                await _transactionService.UpdateTransactionAsync(userId, updateDto);
            }
            else
            {
                var result = await _transactionService.CreateTransactionAsync(userId, Transaction);
                
                // Add to savings goal if selected
                if (SavingsGoalId.HasValue && result != null)
                {
                    await _savingsGoalService.AddToSavingsAsync(userId, new AddToSavingsDto
                    {
                        SavingsGoalId = SavingsGoalId.Value,
                        TransactionId = result.Id,
                        Amount = Transaction.Amount, // Assume full amount contributes? Or prompts user? Requirement says "Select if this transaction contributes". Usually full amount.
                        Note = "Contribution from transaction"
                    });
                }
            }
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

        var savingsGoals = await _savingsGoalService.GetUserSavingsGoalsAsync(userId, activeOnly: true);
        SavingsGoalList = new SelectList(savingsGoals ?? Enumerable.Empty<SavingsGoalResponseDto>(), "Id", "Name");

        Categories = await _categoryService.GetCategorySummariesAsync(userId) ?? new List<CategorySummaryDto>();
    }
}
