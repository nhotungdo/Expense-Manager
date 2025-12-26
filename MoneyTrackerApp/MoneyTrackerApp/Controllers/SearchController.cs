using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ExpenseManagerContext context, ILogger<SearchController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Global search for transactions, categories, and accounts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string? type = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Ok(new
                {
                    transactions = new List<object>(),
                    categories = new List<object>(),
                    accounts = new List<object>()
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var searchTerm = query.ToLower();
            var results = new
            {
                transactions = type == null || type == "transaction" ? await SearchTransactions(userId, searchTerm) : new List<object>(),
                categories = type == null || type == "category" ? await SearchCategories(userId, searchTerm) : new List<object>(),
                accounts = type == null || type == "account" ? await SearchAccounts(userId, searchTerm) : new List<object>()
            };

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching with query: {Query}", query);
            return StatusCode(500, new { error = "Lỗi khi tìm kiếm" });
        }
    }

    private async Task<List<object>> SearchTransactions(long userId, string searchTerm)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId &&
                       (t.Note.ToLower().Contains(searchTerm) ||
                        t.Category.Name.ToLower().Contains(searchTerm) ||
                        t.Account.Name.ToLower().Contains(searchTerm)))
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .Select(t => new
            {
                id = t.Id,
                note = t.Note,
                amount = t.Amount,
                date = t.TransactionDate,
                categoryName = t.Category.Name,
                categoryIcon = t.Category.Icon,
                categoryColor = t.Category.Color,
                accountName = t.Account.Name,
                type = t.Category.Type == 1 ? "income" : t.Category.Type == 2 ? "expense" : "transfer"
            })
            .ToListAsync<object>();

        return transactions;
    }

    private async Task<List<object>> SearchCategories(long userId, string searchTerm)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == userId && c.Name.ToLower().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .Take(10)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                icon = c.Icon,
                color = c.Color,
                type = c.Type == 1 ? "income" : c.Type == 2 ? "expense" : "transfer",
                transactionCount = _context.Transactions.Count(t => t.CategoryId == c.Id)
            })
            .ToListAsync<object>();

        return categories;
    }

    private async Task<List<object>> SearchAccounts(long userId, string searchTerm)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.Name.ToLower().Contains(searchTerm))
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new
            {
                id = a.Id,
                name = a.Name,
                balance = a.CurrentBalance,
                icon = a.Icon,
                color = a.Color,
                currency = a.Currency
            })
            .ToListAsync<object>();

        return accounts;
    }

    /// <summary>
    /// Quick search suggestions (autocomplete)
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Ok(new List<object>());
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var searchTerm = query.ToLower();
            
            // Get recent transaction notes
            var suggestions = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId && 
                           !string.IsNullOrEmpty(t.Note) &&
                           t.Note.ToLower().Contains(searchTerm))
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => t.Note)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions");
            return Ok(new List<object>());
        }
    }
}
