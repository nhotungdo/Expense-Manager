using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ExpenseManagerContext _db;
    public TransactionsController(ExpenseManagerContext db) { _db = db; }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromQuery] long userId, [FromBody] WebhookTransaction data)
    {
        try
        {
            var txType = data.Type == "Income" ? 1 : data.Type == "Expense" ? 0 : 2;
            var transaction = new Transaction
            {
                UserId = userId,
                TransactionType = txType,
                Amount = data.Amount,
                TransactionDate = data.Date ?? DateTime.Today,
                Note = $"Webhook: {data.Description ?? string.Empty}"
            };

            // Find account by name if provided
            if (!string.IsNullOrEmpty(data.Account))
            {
                var account = await _db.Accounts
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Name.Contains(data.Account));
                if (account != null) transaction.AccountId = account.Id;
            }

            // Find category by name if provided
            if (!string.IsNullOrEmpty(data.Category))
            {
                var category = await _db.Categories
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.Contains(data.Category));
                if (category != null) transaction.CategoryId = category.Id;
            }

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, id = transaction.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    public class WebhookTransaction
    {
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
        public string? Account { get; set; }
        public string? Category { get; set; }
    }
}
