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

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return Page();
            }

            var condition = new AutomationConditionDto
            {
                TransactionType = ConditionTransactionType,
                CategoryId = ConditionCategoryId,
                AmountThreshold = ConditionAmount,
                Operator = ConditionOperator
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

            _context.AutomationRules.Add(rule);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
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
