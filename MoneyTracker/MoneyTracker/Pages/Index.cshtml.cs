using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Pages
{
    public class IndexModel : PageModel
    {
        public List<DemoExpense> DemoExpenses { get; set; } = new();
        public List<DemoIncome> DemoIncomes { get; set; } = new();
        public List<DemoCategory> DemoCategories { get; set; } = new();
        public DashboardStats DashboardStats { get; set; } = new();

        public void OnGet()
        {
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            // Demo Dashboard Stats
            DashboardStats = new DashboardStats
            {
                TotalIncome = 25500000,
                TotalExpenses = 18700000,
                NetWorth = 6800000,
                SavingsRate = 27
            };

            // Demo Expenses
            DemoExpenses = new List<DemoExpense>
            {
                new() { Date = "25/12/2024", Category = "Ăn uống", Amount = -450000, Note = "Tiệc Giáng sinh", CategoryColor = "bg-warning" },
                new() { Date = "24/12/2024", Category = "Di chuyển", Amount = -85000, Note = "Xăng xe", CategoryColor = "bg-info" },
                new() { Date = "23/12/2024", Category = "Mua sắm", Amount = -1250000, Note = "Quần áo mùa đông", CategoryColor = "bg-success" },
                new() { Date = "22/12/2024", Category = "Giải trí", Amount = -200000, Note = "Xem phim", CategoryColor = "bg-danger" }
            };

            // Demo Incomes
            DemoIncomes = new List<DemoIncome>
            {
                new() { Date = "01/12/2024", Category = "Lương", Amount = 15000000, Note = "Lương tháng 12", CategoryColor = "bg-primary" },
                new() { Date = "15/12/2024", Category = "Freelance", Amount = 5000000, Note = "Dự án web", CategoryColor = "bg-info" },
                new() { Date = "20/12/2024", Category = "Đầu tư", Amount = 2500000, Note = "Cổ tức chứng khoán", CategoryColor = "bg-warning" }
            };

            // Demo Categories
            DemoCategories = new List<DemoCategory>
            {
                new() { Name = "Ăn uống", Type = "Expense", Icon = "fas fa-utensils", Color = "text-warning", CreatedDate = "01/01/2024" },
                new() { Name = "Di chuyển", Type = "Expense", Icon = "fas fa-car", Color = "text-info", CreatedDate = "01/01/2024" },
                new() { Name = "Mua sắm", Type = "Expense", Icon = "fas fa-shopping-bag", Color = "text-success", CreatedDate = "01/01/2024" },
                new() { Name = "Lương", Type = "Income", Icon = "fas fa-briefcase", Color = "text-primary", CreatedDate = "01/01/2024" },
                new() { Name = "Freelance", Type = "Income", Icon = "fas fa-laptop-code", Color = "text-info", CreatedDate = "01/01/2024" }
            };
        }
    }

    public class DemoExpense
    {
        public string Date { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
    }

    public class DemoIncome
    {
        public string Date { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
    }

    public class DemoCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
    }

    public class DashboardStats
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetWorth { get; set; }
        public decimal SavingsRate { get; set; }
    }
}
