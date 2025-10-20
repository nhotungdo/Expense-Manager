using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Debts;

[Authorize]
public class EditModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public EditModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? PersonName { get; set; }
        [Range(0,1)] public int DebtType { get; set; } = 0;
        [Range(typeof(decimal), "0.00", "9999999999")] public decimal InitialAmount { get; set; }
        [Range(typeof(decimal), "0.00", "100")] public decimal InterestRate { get; set; }
        [DataType(DataType.Date)] public DateTime StartDate { get; set; } = DateTime.Today;
        [DataType(DataType.Date)] public DateTime? DueDate { get; set; }
        public int Status { get; set; } = 1;
    }

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        if (id.HasValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long.TryParse(userIdStr, out var userId);
            var d = await _db.Debts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (d == null) return RedirectToPage("Index");
            Input = new InputModel
            {
                Id = d.Id,
                Name = d.Name,
                PersonName = d.PersonName,
                DebtType = d.DebtType,
                InitialAmount = d.InitialAmount,
                InterestRate = d.InterestRate,
                StartDate = d.StartDate.ToDateTime(TimeOnly.MinValue),
                DueDate = d.DueDate?.ToDateTime(TimeOnly.MinValue),
                Status = d.Status
            };
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        if (Input.Id == 0)
        {
            var d = new Debt
            {
                UserId = userId,
                Name = Input.Name.Trim(),
                PersonName = Input.PersonName,
                DebtType = Input.DebtType,
                InitialAmount = Input.InitialAmount,
                AmountPaid = 0,
                InterestRate = Input.InterestRate,
                StartDate = DateOnly.FromDateTime(Input.StartDate),
                DueDate = Input.DueDate.HasValue ? DateOnly.FromDateTime(Input.DueDate.Value) : null,
                Status = Input.Status,
                CreatedAt = DateTime.UtcNow
            };
            _db.Debts.Add(d);
        }
        else
        {
            var d = await _db.Debts.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (d == null) return RedirectToPage("Index");
            d.Name = Input.Name.Trim();
            d.PersonName = Input.PersonName;
            d.DebtType = Input.DebtType;
            d.InitialAmount = Input.InitialAmount;
            d.InterestRate = Input.InterestRate;
            d.StartDate = DateOnly.FromDateTime(Input.StartDate);
            d.DueDate = Input.DueDate.HasValue ? DateOnly.FromDateTime(Input.DueDate.Value) : null;
            d.Status = Input.Status;
            d.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


