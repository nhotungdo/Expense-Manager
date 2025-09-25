using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.AI
{
    public class SuggestionsModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public SuggestionsModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string? Note { get; set; }
        public string Classification { get; set; } = string.Empty;
        public IList<string> SavingTips { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            SavingTips = await BuildSavingTipsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "classify")
            {
                Classification = ClassifyCategoryFromNote(Note);
            }
            SavingTips = await BuildSavingTipsAsync();
            return Page();
        }

        private string ClassifyCategoryFromNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note)) return "KHÁC";
            var n = note.ToLowerInvariant();
            if (n.Contains("cà phê") || n.Contains("coffee") || n.Contains("ăn") || n.Contains("food")) return "ĂN UỐNG";
            if (n.Contains("xăng") || n.Contains("grab") || n.Contains("taxi") || n.Contains("bus")) return "DI CHUYỂN";
            if (n.Contains("điện") || n.Contains("nước") || n.Contains("internet") || n.Contains("wifi")) return "HÓA ĐƠN";
            return "KHÁC";
        }

        private async Task<IList<string>> BuildSavingTipsAsync()
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return new List<string>();
            var userId = long.Parse(userIdStr);

            var last30 = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var expenses = await _db.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.ExpenseDate >= last30)
                .ToListAsync();

            var total = expenses.Sum(e => e.Amount);
            if (total <= 0) return new List<string> { "Chưa có dữ liệu chi tiêu để phân tích." };

            var top = expenses
                .GroupBy(e => e.Category?.Name ?? "Khác")
                .Select(g => new { Name = g.Key, Sum = g.Sum(x => x.Amount), Ratio = g.Sum(x => x.Amount) / total })
                .OrderByDescending(x => x.Sum)
                .Take(3)
                .ToList();

            var tips = new List<string>();
            foreach (var t in top)
            {
                var percent = Math.Round(t.Ratio * 100, 1);
                tips.Add($"Hạng mục '{t.Name}' chiếm {percent}% tổng chi tiêu 30 ngày qua. Cân nhắc cắt giảm 10-15%.");
            }
            tips.Add("Đặt hạn mức chi tiêu theo tuần cho hạng mục lớn nhất.");
            tips.Add("Ưu tiên dùng danh mục mặc định để dễ theo dõi thống kê.");
            return tips;
        }
    }
}


