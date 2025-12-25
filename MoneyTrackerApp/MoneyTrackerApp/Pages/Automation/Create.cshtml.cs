using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;
using System.Text.Json;

namespace MoneyTrackerApp.Pages.Automation
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ExpenseManagerContext _context;
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;

        public CreateModel(ExpenseManagerContext context, ICategoryService categoryService, IAccountService accountService)
        {
            _context = context;
            _categoryService = categoryService;
            _accountService = accountService;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string TriggerType { get; set; } = "TransactionCreated";

        [BindProperty]
        public int? ConditionTransactionType { get; set; }

        [BindProperty]
        public long? ConditionCategoryId { get; set; }

        [BindProperty]
        public string ConditionCheckType { get; set; } = "Transaction"; // Transaction, SpendingLimit, Balance

        [BindProperty]
        public long? ConditionAccountId { get; set; } // Source Account

        [BindProperty]
        public string ConditionPeriod { get; set; } = "Monthly";

        [BindProperty]
        public decimal? ConditionAmount { get; set; }

        [BindProperty]
        public string ConditionOperator { get; set; } = ">";

        [BindProperty]
        public string ActionType { get; set; } // Notify, Transfer

        [BindProperty]
        public string? NoteMessage { get; set; } // For Notify

        [BindProperty]
        public long? ActionDestinationAccountId { get; set; }

        [BindProperty]
        public decimal ActionAmount { get; set; }

        [BindProperty]
        public bool ActionIsPercentage { get; set; }

        public SelectList TriggerTypes { get; set; }
        public SelectList ActionTypes { get; set; }
        public SelectList Categories { get; set; }
        public SelectList Accounts { get; set; }

        public async Task OnGetAsync()
        {
            await LoadOptions();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Custom Validation Logic
            if (ActionType == "Notify")
            {
                // Remove errors for Transfer-specific fields
                ModelState.Remove("ActionDestinationAccountId");
                ModelState.Remove("ActionAmount");
            }
            else if (ActionType == "Transfer")
            {
                if (!ActionDestinationAccountId.HasValue)
                {
                    ModelState.AddModelError("ActionDestinationAccountId", "Vui lòng chọn tài khoản đích.");
                }
                if (ActionAmount <= 0)
                {
                    ModelState.AddModelError("ActionAmount", "Số tiền chuyển phải lớn hơn 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Ensure errors are visible
                if (ModelState.ErrorCount > 0 && !ModelState.ContainsKey(string.Empty))
                {
                   // Optional: Aggregate errors if needed, but summary handles it
                }
                await LoadOptions();
                return Page();
            }

            var condition = new AutomationConditionDto
            {
                CheckType = ConditionCheckType,
                TransactionType = ConditionTransactionType,
                CategoryId = ConditionCategoryId,
                AccountId = ConditionAccountId,
                AmountThreshold = ConditionAmount,
                Operator = ConditionOperator,
                Period = ConditionPeriod
            };

            var action = new AutomationActionDto
            {
                Type = ActionType,
                Message = NoteMessage ?? (ActionType == "Transfer" ? $"Auto-transfer: {Name}" : Name),
                TargetAccountId = ActionDestinationAccountId,
                Amount = ActionAmount,
                IsPercentage = ActionIsPercentage
            };

            var rule = new AutomationRule
            {
                UserId = userId,
                Name = Name,
                TriggerType = "TransactionCreated", // Currently only support this
                ConditionJson = JsonSerializer.Serialize(condition),
                ActionType = ActionType,
                ActionJson = JsonSerializer.Serialize(action),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.AutomationRules.Add(rule);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo quy tắc: " + ex.Message);
                await LoadOptions();
                return Page();
            }
        }

        private async Task LoadOptions()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            TriggerTypes = new SelectList(new[] { new { Value = "TransactionCreated", Text = "Khi có giao dịch mới" } }, "Value", "Text");
            
            ActionTypes = new SelectList(new[] { 
                new { Value = "Notify", Text = "Gửi thông báo" },
                new { Value = "Transfer", Text = "Tự động chuyển tiền" }
            }, "Value", "Text");

            var cats = await _categoryService.GetUserCategoriesAsync(userId);
            Categories = new SelectList(cats, "Id", "Name");

            var accs = await _accountService.GetUserAccountsAsync(userId);
            Accounts = new SelectList(accs, "Id", "Name");
        }
    }
}
