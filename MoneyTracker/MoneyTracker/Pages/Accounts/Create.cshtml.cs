using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Accounts;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public CreateModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Range(typeof(decimal), "0.00", "9999999999")] public decimal InitialBalance { get; set; }
        [StringLength(3)] public string Currency { get; set; } = "VND";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var acc = new MoneyTracker.Models.Account
        {
            UserId = userId,
            Name = Input.Name.Trim(),
            Currency = Input.Currency,
            InitialBalance = Input.InitialBalance,
            CurrentBalance = Input.InitialBalance,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Accounts.Add(acc);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


