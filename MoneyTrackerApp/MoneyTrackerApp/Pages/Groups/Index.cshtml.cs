using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.Pages.Groups
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public IList<GroupExpense> Groups { get; set; } = new List<GroupExpense>();
        public Dictionary<long, decimal> UserBalances { get; set; } = new Dictionary<long, decimal>();

        [BindProperty]
        public CreateGroupInput Input { get; set; }

        public class CreateGroupInput
        {
            [Required(ErrorMessage = "Vui lòng nhập tên nhóm")]
            public string Name { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; } = "fas fa-users";
            public string Color { get; set; } = "#3B82F6";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Fetch groups where the user is either the creator or a member
            Groups = await _context.GroupExpenses
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
                // Include Transactions and Splits to calculate balance on the dashboard
                .Include(g => g.GroupTransactions)
                .ThenInclude(gt => gt.GroupTransactionSplits)
                .Where(g => g.CreatedByUserId == userId || g.GroupMembers.Any(m => m.UserId == userId))
                .OrderByDescending(g => g.UpdatedAt ?? g.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            // Calculate Balances
            foreach (var group in Groups)
            {
                var paid = group.GroupTransactions.Where(t => t.PaidByUserId == userId).Sum(t => t.Amount);
                var share = group.GroupTransactions
                    .SelectMany(t => t.GroupTransactionSplits)
                    .Where(s => s.UserId == userId)
                    .Sum(s => s.Amount);
                
                UserBalances[group.Id] = paid - share;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateGroupAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var group = new GroupExpense
            {
                Name = Input.Name,
                Description = Input.Description,
                CreatedByUserId = userId,
                Icon = Input.Icon,
                Color = Input.Color,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsPublic = false
            };

            _context.GroupExpenses.Add(group);
            await _context.SaveChangesAsync();

            // Add creator as Admin member
            var member = new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                Role = "Admin", // Or an enum if you used one
                JoinedAt = DateTime.Now
            };
            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = group.Id });
        }
    }
}
