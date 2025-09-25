using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Incomes
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public IList<Income> Items { get; set; } = new List<Income>();
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
                Items = new List<Income>();
                return;
            }
            var userId = long.Parse(userIdStr);

            From = from;
            To = to;

            var query = _db.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(i => i.IncomeDate >= from.Value);
            if (to.HasValue)
                query = query.Where(i => i.IncomeDate <= to.Value);

            Items = await query
                .OrderByDescending(i => i.IncomeDate)
                .ThenByDescending(i => i.Id)
                .ToListAsync();

            Currency = Items.FirstOrDefault()?.Currency ?? "VND";
            TotalAmount = Items.Sum(i => i.Amount);
        }
    }
}


