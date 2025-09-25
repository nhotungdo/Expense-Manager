using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Expenses
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public IList<Expense> Items { get; set; } = new List<Expense>();
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public string? FromStr => From?.ToString("yyyy-MM-dd");
        public string? ToStr => To?.ToString("yyyy-MM-dd");

        public async Task OnGetAsync(DateOnly? from, DateOnly? to)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                Items = new List<Expense>();
                return;
            }
            var userId = long.Parse(userIdStr);

            From = from;
            To = to;

            var query = _db.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value);

            Items = await query
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.Id)
                .ToListAsync();

            Currency = Items.FirstOrDefault()?.Currency ?? "VND";
            TotalAmount = Items.Sum(e => e.Amount);
        }
    }
}


