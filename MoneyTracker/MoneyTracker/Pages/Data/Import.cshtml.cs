using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Data;

[Authorize]
public class ImportModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ImportModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<MoneyTracker.Models.Account> Accounts { get; set; } = new();
    public string WebhookUrl { get; set; } = "";
    public string? ImportResult { get; set; }

    public class InputModel
    {
        public IFormFile? File { get; set; }
        public long? AccountId { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        Accounts = await _db.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();

        WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/transactions/webhook?userId={userId}";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        if (!ModelState.IsValid || Input.File == null)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            using var reader = new StreamReader(Input.File.OpenReadStream());
            var csvContent = await reader.ReadToEndAsync();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var imported = 0;
            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                var fields = lines[i].Split(',');
                if (fields.Length >= 4)
                {
                    var typeStr = fields[0].Trim();
                    var txType = typeStr == "Income" ? 1 : typeStr == "Expense" ? 0 : 2; // 2 = other/transfer
                    var transaction = new Transaction
                    {
                        UserId = userId,
                        AccountId = Input.AccountId.GetValueOrDefault(),
                        TransactionType = txType,
                        Amount = decimal.Parse(fields[1].Trim()),
                        TransactionDate = DateTime.Parse(fields[3].Trim()),
                        // combine optional fields into Note
                        Note = (fields.Length > 4 ? fields[4].Trim() + " - " : "") + (fields.Length > 2 ? fields[2].Trim() : "Imported from CSV")
                    };

                    _db.Transactions.Add(transaction);
                    imported++;
                }
            }

            await _db.SaveChangesAsync();
            ImportResult = $"Successfully imported {imported} transactions";
        }
        catch (Exception ex)
        {
            ImportResult = $"Import failed: {ex.Message}";
        }

        await OnGetAsync();
        return Page();
    }
}
