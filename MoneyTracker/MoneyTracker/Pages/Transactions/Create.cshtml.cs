using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Transactions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ExpenseManagerContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> AccountOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [Required]
        public string TransactionType { get; set; } = "Expense";

        [Range(typeof(decimal), "0.00", "9999999999")]
        public decimal Amount { get; set; }

        [Required]
        public long AccountId { get; set; }

        public long? CategoryId { get; set; }

        public string? Note { get; set; }

        public long? PairedAccountId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var tx = new Transaction
        {
            UserId = userId,
            AccountId = Input.AccountId,
            CategoryId = Input.CategoryId,
            TransactionDate = Input.TransactionDate,
            TransactionType = Input.TransactionType == "Income" ? 1 : Input.TransactionType == "Expense" ? 0 : 2,
            Amount = Input.Amount,
            Currency = "VND",
            Note = Input.Note
        };

        var file = Request.Form.Files["attachment"];
        if (file != null && file.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "attachments");
            Directory.CreateDirectory(uploads);
            var fileName = $"t{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);
            tx.AttachmentUrl = $"/uploads/attachments/{fileName}";
        }

        if (Input.TransactionType == "Transfer" && Input.PairedAccountId.HasValue)
        {
            var pairedOut = tx;
            pairedOut.PairedAccountId = Input.PairedAccountId;

            var pairedIn = new Transaction
            {
                UserId = userId,
                AccountId = Input.PairedAccountId.Value,
                PairedAccountId = Input.AccountId,
                TransactionDate = Input.TransactionDate,
                TransactionType = 1,
                Amount = Input.Amount,
                Currency = "VND",
                Note = (Input.Note ?? "") + " (Transfer in)"
            };
            _db.Transactions.Add(pairedOut);
            _db.Transactions.Add(pairedIn);
        }
        else
        {
            _db.Transactions.Add(tx);
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task LoadOptionsAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        AccountOptions = await _db.Accounts.Where(a => a.UserId == userId && a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            .ToListAsync();
        CategoryOptions = await _db.Categories.Where(c => c.UserId == userId && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }
}


