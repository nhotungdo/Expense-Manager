using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.MoneyTracker.Transactions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public List<TransactionGroupView> Groups { get; private set; } = new();

        public async Task OnGet()
        {
            var email = User?.FindFirst("email")?.Value ?? User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                Groups = new();
                return;
            }

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Groups = new();
                return;
            }

            var items = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == user.Id)
                .Include(t => t.Category)
                .OrderByDescending(t => t.TransactionDate)
                .Take(100)
                .Select(t => new TransactionItemView
                {
                    Date = t.TransactionDate,
                    CategoryIcon = t.Category != null ? (t.Category.Icon ?? "circle") : "circle",
                    CategoryName = t.Category != null ? (t.Category.Name ?? "Khác") : "Khác",
                    Note = t.Description ?? string.Empty,
                    Amount = t.Amount,
                    Type = t.Type
                })
                .ToListAsync();

            Groups = items
                .GroupBy(i => i.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroupView
                {
                    Date = g.Key,
                    Label = GetDateLabel(g.Key),
                    Items = g.ToList()
                })
                .ToList();
        }

        private static string GetDateLabel(DateTime date)
        {
            var today = DateTime.Today;
            if (date == today) return "Hôm nay";
            if (date == today.AddDays(-1)) return "Hôm qua";
            return date.ToString("dd/MM/yyyy", new CultureInfo("vi-VN"));
        }
    }

    public sealed class TransactionGroupView
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<TransactionItemView> Items { get; set; } = new();
    }

    public sealed class TransactionItemView
    {
        public DateTime Date { get; set; }
        public string CategoryIcon { get; set; } = "circle";
        public string CategoryName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Type { get; set; } // 0/1 -> define by your schema: income/expense
    }
}
