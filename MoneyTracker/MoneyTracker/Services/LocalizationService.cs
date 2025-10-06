using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using System.Text.Json;

namespace MoneyTracker.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<LocalizationService> _logger;
        private readonly Dictionary<string, Dictionary<string, string>> _translations;
        private readonly Dictionary<long, string> _userLanguages;

        public LocalizationService(ExpenseManagerContext context, ILogger<LocalizationService> logger)
        {
            _context = context;
            _logger = logger;
            _userLanguages = new Dictionary<long, string>();
            _translations = InitializeTranslations();
        }

        public string GetString(string key, string language = "vi")
        {
            if (_translations.TryGetValue(language, out var langDict) &&
                langDict.TryGetValue(key, out var translation))
            {
                return translation;
            }

            // Fallback to Vietnamese if translation not found
            if (language != "vi" && _translations.TryGetValue("vi", out var viDict) &&
                viDict.TryGetValue(key, out var viTranslation))
            {
                return viTranslation;
            }

            return key; // Return key if no translation found
        }

        public Dictionary<string, string> GetLocalizedStrings(string language = "vi")
        {
            return _translations.TryGetValue(language, out var translations)
                ? translations
                : _translations["vi"];
        }

        public List<LanguageDto> GetSupportedLanguages()
        {
            return new List<LanguageDto>
            {
                new LanguageDto { Code = "vi", Name = "Vietnamese", NativeName = "Tiếng Việt", Flag = "🇻🇳" },
                new LanguageDto { Code = "en", Name = "English", NativeName = "English", Flag = "🇺🇸" },
                new LanguageDto { Code = "ja", Name = "Japanese", NativeName = "日本語", Flag = "🇯🇵" },
                new LanguageDto { Code = "ko", Name = "Korean", NativeName = "한국어", Flag = "🇰🇷" },
                new LanguageDto { Code = "zh", Name = "Chinese", NativeName = "中文", Flag = "🇨🇳" },
                new LanguageDto { Code = "th", Name = "Thai", NativeName = "ไทย", Flag = "🇹🇭" }
            };
        }

        public async Task SetUserLanguageAsync(long userId, string language)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Language = language;
                    await _context.SaveChangesAsync();
                    _userLanguages[userId] = language;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user language for user {UserId}", userId);
            }
        }

        public string GetUserLanguage(long userId)
        {
            return _userLanguages.TryGetValue(userId, out var language) ? language : "vi";
        }

        private Dictionary<string, Dictionary<string, string>> InitializeTranslations()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                ["vi"] = new Dictionary<string, string>
                {
                    // Navigation
                    ["nav.home"] = "Trang chủ",
                    ["nav.dashboard"] = "Bảng điều khiển",
                    ["nav.expenses"] = "Chi tiêu",
                    ["nav.incomes"] = "Thu nhập",
                    ["nav.categories"] = "Danh mục",
                    ["nav.reports"] = "Báo cáo",
                    ["nav.profile"] = "Hồ sơ",
                    ["nav.settings"] = "Cài đặt",
                    ["nav.logout"] = "Đăng xuất",

                    // Common
                    ["common.add"] = "Thêm",
                    ["common.edit"] = "Sửa",
                    ["common.delete"] = "Xóa",
                    ["common.save"] = "Lưu",
                    ["common.cancel"] = "Hủy",
                    ["common.confirm"] = "Xác nhận",
                    ["common.search"] = "Tìm kiếm",
                    ["common.filter"] = "Lọc",
                    ["common.export"] = "Xuất",
                    ["common.import"] = "Nhập",
                    ["common.date"] = "Ngày",
                    ["common.amount"] = "Số tiền",
                    ["common.category"] = "Danh mục",
                    ["common.note"] = "Ghi chú",
                    ["common.total"] = "Tổng cộng",
                    ["common.balance"] = "Số dư",

                    // Dashboard
                    ["dashboard.title"] = "Bảng điều khiển",
                    ["dashboard.total_income"] = "Tổng thu nhập",
                    ["dashboard.total_expenses"] = "Tổng chi tiêu",
                    ["dashboard.net_worth"] = "Tài sản ròng",
                    ["dashboard.monthly_income"] = "Thu nhập tháng này",
                    ["dashboard.monthly_expenses"] = "Chi tiêu tháng này",
                    ["dashboard.monthly_savings"] = "Tiết kiệm tháng này",
                    ["dashboard.recent_transactions"] = "Giao dịch gần đây",
                    ["dashboard.ai_suggestions"] = "Gợi ý AI",

                    // Expenses
                    ["expenses.title"] = "Quản lý chi tiêu",
                    ["expenses.add_expense"] = "Thêm chi tiêu",
                    ["expenses.edit_expense"] = "Sửa chi tiêu",
                    ["expenses.expense_date"] = "Ngày chi tiêu",
                    ["expenses.expense_amount"] = "Số tiền chi tiêu",
                    ["expenses.expense_category"] = "Danh mục chi tiêu",
                    ["expenses.expense_note"] = "Ghi chú chi tiêu",

                    // Incomes
                    ["incomes.title"] = "Quản lý thu nhập",
                    ["incomes.add_income"] = "Thêm thu nhập",
                    ["incomes.edit_income"] = "Sửa thu nhập",
                    ["incomes.income_date"] = "Ngày thu nhập",
                    ["incomes.income_amount"] = "Số tiền thu nhập",
                    ["incomes.income_category"] = "Danh mục thu nhập",
                    ["incomes.income_note"] = "Ghi chú thu nhập",

                    // Categories
                    ["categories.title"] = "Quản lý danh mục",
                    ["categories.add_category"] = "Thêm danh mục",
                    ["categories.edit_category"] = "Sửa danh mục",
                    ["categories.category_name"] = "Tên danh mục",
                    ["categories.category_type"] = "Loại danh mục",
                    ["categories.category_description"] = "Mô tả danh mục",
                    ["categories.expense_categories"] = "Danh mục chi tiêu",
                    ["categories.income_categories"] = "Danh mục thu nhập",

                    // Reports
                    ["reports.title"] = "Báo cáo",
                    ["reports.monthly_report"] = "Báo cáo hàng tháng",
                    ["reports.export_pdf"] = "Xuất PDF",
                    ["reports.export_excel"] = "Xuất Excel",
                    ["reports.export_csv"] = "Xuất CSV",
                    ["reports.date_range"] = "Khoảng thời gian",
                    ["reports.from_date"] = "Từ ngày",
                    ["reports.to_date"] = "Đến ngày",

                    // Profile
                    ["profile.title"] = "Hồ sơ cá nhân",
                    ["profile.personal_info"] = "Thông tin cá nhân",
                    ["profile.settings"] = "Cài đặt",
                    ["profile.full_name"] = "Họ và tên",
                    ["profile.email"] = "Email",
                    ["profile.phone"] = "Số điện thoại",
                    ["profile.address"] = "Địa chỉ",
                    ["profile.language"] = "Ngôn ngữ",
                    ["profile.currency"] = "Tiền tệ",
                    ["profile.timezone"] = "Múi giờ",
                    ["profile.theme"] = "Giao diện",

                    // AI Suggestions
                    ["ai.title"] = "Gợi ý AI",
                    ["ai.suggestions"] = "Gợi ý thông minh",
                    ["ai.budget_analysis"] = "Phân tích ngân sách",
                    ["ai.spending_patterns"] = "Mẫu chi tiêu",
                    ["ai.recommendations"] = "Khuyến nghị",

                    // Messages
                    ["message.success"] = "Thành công",
                    ["message.error"] = "Lỗi",
                    ["message.warning"] = "Cảnh báo",
                    ["message.info"] = "Thông tin",
                    ["message.confirm_delete"] = "Bạn có chắc chắn muốn xóa?",
                    ["message.save_success"] = "Lưu thành công",
                    ["message.delete_success"] = "Xóa thành công",
                    ["message.export_success"] = "Xuất báo cáo thành công"
                },

                ["en"] = new Dictionary<string, string>
                {
                    // Navigation
                    ["nav.home"] = "Home",
                    ["nav.dashboard"] = "Dashboard",
                    ["nav.expenses"] = "Expenses",
                    ["nav.incomes"] = "Incomes",
                    ["nav.categories"] = "Categories",
                    ["nav.reports"] = "Reports",
                    ["nav.profile"] = "Profile",
                    ["nav.settings"] = "Settings",
                    ["nav.logout"] = "Logout",

                    // Common
                    ["common.add"] = "Add",
                    ["common.edit"] = "Edit",
                    ["common.delete"] = "Delete",
                    ["common.save"] = "Save",
                    ["common.cancel"] = "Cancel",
                    ["common.confirm"] = "Confirm",
                    ["common.search"] = "Search",
                    ["common.filter"] = "Filter",
                    ["common.export"] = "Export",
                    ["common.import"] = "Import",
                    ["common.date"] = "Date",
                    ["common.amount"] = "Amount",
                    ["common.category"] = "Category",
                    ["common.note"] = "Note",
                    ["common.total"] = "Total",
                    ["common.balance"] = "Balance",

                    // Dashboard
                    ["dashboard.title"] = "Dashboard",
                    ["dashboard.total_income"] = "Total Income",
                    ["dashboard.total_expenses"] = "Total Expenses",
                    ["dashboard.net_worth"] = "Net Worth",
                    ["dashboard.monthly_income"] = "Monthly Income",
                    ["dashboard.monthly_expenses"] = "Monthly Expenses",
                    ["dashboard.monthly_savings"] = "Monthly Savings",
                    ["dashboard.recent_transactions"] = "Recent Transactions",
                    ["dashboard.ai_suggestions"] = "AI Suggestions",

                    // Expenses
                    ["expenses.title"] = "Expense Management",
                    ["expenses.add_expense"] = "Add Expense",
                    ["expenses.edit_expense"] = "Edit Expense",
                    ["expenses.expense_date"] = "Expense Date",
                    ["expenses.expense_amount"] = "Expense Amount",
                    ["expenses.expense_category"] = "Expense Category",
                    ["expenses.expense_note"] = "Expense Note",

                    // Incomes
                    ["incomes.title"] = "Income Management",
                    ["incomes.add_income"] = "Add Income",
                    ["incomes.edit_income"] = "Edit Income",
                    ["incomes.income_date"] = "Income Date",
                    ["incomes.income_amount"] = "Income Amount",
                    ["incomes.income_category"] = "Income Category",
                    ["incomes.income_note"] = "Income Note",

                    // Categories
                    ["categories.title"] = "Category Management",
                    ["categories.add_category"] = "Add Category",
                    ["categories.edit_category"] = "Edit Category",
                    ["categories.category_name"] = "Category Name",
                    ["categories.category_type"] = "Category Type",
                    ["categories.category_description"] = "Category Description",
                    ["categories.expense_categories"] = "Expense Categories",
                    ["categories.income_categories"] = "Income Categories",

                    // Reports
                    ["reports.title"] = "Reports",
                    ["reports.monthly_report"] = "Monthly Report",
                    ["reports.export_pdf"] = "Export PDF",
                    ["reports.export_excel"] = "Export Excel",
                    ["reports.export_csv"] = "Export CSV",
                    ["reports.date_range"] = "Date Range",
                    ["reports.from_date"] = "From Date",
                    ["reports.to_date"] = "To Date",

                    // Profile
                    ["profile.title"] = "Profile",
                    ["profile.personal_info"] = "Personal Information",
                    ["profile.settings"] = "Settings",
                    ["profile.full_name"] = "Full Name",
                    ["profile.email"] = "Email",
                    ["profile.phone"] = "Phone",
                    ["profile.address"] = "Address",
                    ["profile.language"] = "Language",
                    ["profile.currency"] = "Currency",
                    ["profile.timezone"] = "Timezone",
                    ["profile.theme"] = "Theme",

                    // AI Suggestions
                    ["ai.title"] = "AI Suggestions",
                    ["ai.suggestions"] = "Smart Suggestions",
                    ["ai.budget_analysis"] = "Budget Analysis",
                    ["ai.spending_patterns"] = "Spending Patterns",
                    ["ai.recommendations"] = "Recommendations",

                    // Messages
                    ["message.success"] = "Success",
                    ["message.error"] = "Error",
                    ["message.warning"] = "Warning",
                    ["message.info"] = "Information",
                    ["message.confirm_delete"] = "Are you sure you want to delete?",
                    ["message.save_success"] = "Saved successfully",
                    ["message.delete_success"] = "Deleted successfully",
                    ["message.export_success"] = "Report exported successfully"
                },

                ["ja"] = new Dictionary<string, string>
                {
                    // Navigation
                    ["nav.home"] = "ホーム",
                    ["nav.dashboard"] = "ダッシュボード",
                    ["nav.expenses"] = "支出",
                    ["nav.incomes"] = "収入",
                    ["nav.categories"] = "カテゴリ",
                    ["nav.reports"] = "レポート",
                    ["nav.profile"] = "プロフィール",
                    ["nav.settings"] = "設定",
                    ["nav.logout"] = "ログアウト",

                    // Common
                    ["common.add"] = "追加",
                    ["common.edit"] = "編集",
                    ["common.delete"] = "削除",
                    ["common.save"] = "保存",
                    ["common.cancel"] = "キャンセル",
                    ["common.confirm"] = "確認",
                    ["common.search"] = "検索",
                    ["common.filter"] = "フィルター",
                    ["common.export"] = "エクスポート",
                    ["common.import"] = "インポート",
                    ["common.date"] = "日付",
                    ["common.amount"] = "金額",
                    ["common.category"] = "カテゴリ",
                    ["common.note"] = "メモ",
                    ["common.total"] = "合計",
                    ["common.balance"] = "残高",

                    // Dashboard
                    ["dashboard.title"] = "ダッシュボード",
                    ["dashboard.total_income"] = "総収入",
                    ["dashboard.total_expenses"] = "総支出",
                    ["dashboard.net_worth"] = "純資産",
                    ["dashboard.monthly_income"] = "月間収入",
                    ["dashboard.monthly_expenses"] = "月間支出",
                    ["dashboard.monthly_savings"] = "月間貯蓄",
                    ["dashboard.recent_transactions"] = "最近の取引",
                    ["dashboard.ai_suggestions"] = "AI提案",

                    // Messages
                    ["message.success"] = "成功",
                    ["message.error"] = "エラー",
                    ["message.warning"] = "警告",
                    ["message.info"] = "情報",
                    ["message.confirm_delete"] = "削除してもよろしいですか？",
                    ["message.save_success"] = "保存しました",
                    ["message.delete_success"] = "削除しました",
                    ["message.export_success"] = "レポートをエクスポートしました"
                }
            };
        }
    }
}
