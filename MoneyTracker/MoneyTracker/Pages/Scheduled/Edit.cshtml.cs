using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Scheduled;

[Authorize]
public class EditModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public EditModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public List<SelectListItem> AccountOptions { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }
        [Required] public long AccountId { get; set; }
        [Range(0,1)] public int TransactionType { get; set; } = 0;
        [Range(typeof(decimal), "0.01", "9999999999")] public decimal Amount { get; set; }
        [Required] public string Frequency { get; set; } = "Monthly";
        [Range(1,365)] public int Interval { get; set; } = 1;
        [DataType(DataType.Date)] public DateTime StartDate { get; set; } = DateTime.Today;
        public bool IsActive { get; set; } = true;
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        await LoadAccountsAsync();
        if (id.HasValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long.TryParse(userIdStr, out var userId);
            var s = await _db.ScheduledTransactions.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (s == null) return RedirectToPage("Index");
            Input = new InputModel
            {
                Id = s.Id,
                AccountId = s.AccountId,
                TransactionType = s.TransactionType,
                Amount = s.Amount,
                Frequency = s.Frequency,
                Interval = s.Interval,
                StartDate = s.StartDate.ToDateTime(TimeOnly.MinValue),
                IsActive = s.IsActive,
                Note = s.Note
            };
        }
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
        if (Input.Id == 0)
        {
            var s = new ScheduledTransaction
            {
                UserId = userId,
                AccountId = Input.AccountId,
                TransactionType = Input.TransactionType,
                Amount = Input.Amount,
                Frequency = Input.Frequency,
                Interval = Input.Interval,
                StartDate = DateOnly.FromDateTime(Input.StartDate),
                NextRunDate = DateOnly.FromDateTime(Input.StartDate),
                IsActive = Input.IsActive,
                Note = Input.Note,
                CreatedAt = DateTime.UtcNow
            };
            _db.ScheduledTransactions.Add(s);
        }
        else
        {
            var s = await _db.ScheduledTransactions.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (s == null) return RedirectToPage("Index");
            s.AccountId = Input.AccountId;
            s.TransactionType = Input.TransactionType;
            s.Amount = Input.Amount;
            s.Frequency = Input.Frequency;
            s.Interval = Input.Interval;
            s.StartDate = DateOnly.FromDateTime(Input.StartDate);
            if (s.NextRunDate < s.StartDate) s.NextRunDate = s.StartDate;
            s.IsActive = Input.IsActive;
            s.Note = Input.Note;
            s.UpdatedAt = DateTime.UtcNow;
        }
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


