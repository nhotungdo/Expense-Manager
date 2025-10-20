using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Debts;

[Authorize]
public class PayModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public PayModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string DebtName { get; set; } = string.Empty;
    public List<SelectListItem> AccountOptions { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }
        [Range(typeof(decimal), "0.01", "9999999999")] public decimal Amount { get; set; }
        [Required] public long AccountId { get; set; }
        [DataType(DataType.Date)] public DateTime PaymentDate { get; set; } = DateTime.Today;
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadAccountsAsync();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var debt = await _db.Debts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (debt == null) return RedirectToPage("Index");
        DebtName = debt.Name;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAccountsAsync();
            return Page();
        }
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        var debt = await _db.Debts.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
        if (debt == null) return RedirectToPage("Index");

        var tx = new Transaction
        {
            UserId = userId,
            AccountId = Input.AccountId,
            TransactionDate = Input.PaymentDate,
            TransactionType = debt.DebtType == 0 ? 0 : 1,
            Amount = Input.Amount,
            Currency = "VND",
            Note = (Input.Note ?? "") + " (Debt payment)"
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();

        var payment = new DebtPayment
        {
            DebtId = debt.Id,
            TransactionId = tx.Id,
            Amount = Input.Amount,
            PaymentDate = Input.PaymentDate,
            Note = Input.Note
        };
        _db.DebtPayments.Add(payment);
        debt.AmountPaid += Input.Amount;
        debt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task LoadAccountsAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        AccountOptions = await _db.Accounts.Where(a => a.UserId == userId && a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            .ToListAsync();
    }
}


