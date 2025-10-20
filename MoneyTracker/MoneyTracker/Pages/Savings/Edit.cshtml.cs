using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Savings;

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
        [Range(typeof(decimal), "0.00", "9999999999")] public decimal TargetAmount { get; set; }
        [Range(typeof(decimal), "0.00", "9999999999")] public decimal CurrentAmount { get; set; }
        public int Status { get; set; } = 1;
    }

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        if (id.HasValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long.TryParse(userIdStr, out var userId);
            var g = await _db.SavingsGoals.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (g == null) return RedirectToPage("Index");
            Input = new InputModel { Id = g.Id, Name = g.Name, TargetAmount = g.TargetAmount, CurrentAmount = g.CurrentAmount, Status = g.Status };
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
            var g = new SavingsGoal { UserId = userId, Name = Input.Name.Trim(), TargetAmount = Input.TargetAmount, CurrentAmount = Input.CurrentAmount, Status = Input.Status, CreatedAt = DateTime.UtcNow };
            _db.SavingsGoals.Add(g);
        }
        else
        {
            var g = await _db.SavingsGoals.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (g == null) return RedirectToPage("Index");
            g.Name = Input.Name.Trim();
            g.TargetAmount = Input.TargetAmount;
            g.CurrentAmount = Input.CurrentAmount;
            g.Status = Input.Status;
            g.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


