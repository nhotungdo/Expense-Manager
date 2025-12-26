using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MoneyTrackerApp.DTOs;


namespace MoneyTrackerApp.Pages.Groups
{
    using MoneyTrackerApp.Services;

    public class DetailsModel : PageModel
    {
        private readonly ExpenseManagerContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly IFriendshipService _friendshipService;
        private readonly IGroupExpenseService _groupExpenseService;

        public DetailsModel(
            ExpenseManagerContext context, 
            IEmailService emailService, 
            IWebHostEnvironment environment, 
            IFriendshipService friendshipService,
            IGroupExpenseService groupExpenseService)
        {
            _context = context;
            _emailService = emailService;
            _environment = environment;
            _friendshipService = friendshipService;
            _groupExpenseService = groupExpenseService;
        }


        public GroupExpense Group { get; set; } = default!;
        public List<GroupTransaction> Transactions { get; set; } = new List<GroupTransaction>();
        public List<GroupInvitation> ExternalInvitations { get; set; } = new List<GroupInvitation>();
        public List<User> GroupParticipants { get; set; } = new List<User>();
        public List<FriendshipDto> FriendsList { get; set; } = new List<FriendshipDto>();


        // Financial Summaries
        public decimal TotalGroupSpending { get; set; }
        public decimal CurrentUserPaid { get; set; }
        public decimal CurrentUserShare { get; set; }
        public decimal CurrentUserBalance { get; set; }
        public bool IsPendingMember { get; set; }
        public string CategorySpendingJson { get; set; }

        [BindProperty]
        public AddExpenseInput ExpenseInput { get; set; }

        public class AddExpenseInput
        {
            [Required(ErrorMessage = "Vui lòng nhập mô tả")]
            public string Description { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số tiền")]
            [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu là 1,000")]
            public decimal Amount { get; set; }

            [Required]
            public DateTime Date { get; set; } = DateTime.Now;

            public string Category { get; set; }

            [Required]
            public long PaidByUserId { get; set; }

            public List<long> SplitUserIds { get; set; } = new List<long>();

            [Display(Name = "Ảnh minh chứng")]
            public IFormFile? ProofImage { get; set; }
        }

        [BindProperty]
        public InviteMemberInput InviteInput { get; set; }

        public class InviteMemberInput
        {
            [Display(Name = "Địa chỉ Email")]
            public string? Emails { get; set; }
            
            public List<long> UserIds { get; set; } = new List<long>();

        }

        [BindProperty]
        public UpdateGroupInput GroupInput { get; set; }

        public class UpdateGroupInput
        {
            [Required(ErrorMessage = "Tên nhóm không được để trống")]
            [StringLength(100, ErrorMessage = "Tên nhóm tối đa 100 ký tự")]
            public string Name { get; set; }

            [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Load Group with Members and Creator
            Group = await _context.GroupExpenses
                .Include(g => g.GroupMembers).ThenInclude(gm => gm.User)
                .Include(g => g.CreatedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Group == null)
            {
                return NotFound();
            }

            // Security Check: Ensure user is member or creator
            var isMember = Group.GroupMembers.Any(m => m.UserId == userId);
            var isCreator = Group.CreatedByUserId == userId;

            if (!isMember && !isCreator)
            {
                return Forbid();
            }

            // Check if current user is pending
            var membership = Group.GroupMembers.FirstOrDefault(m => m.UserId == userId);
            if (membership != null && membership.JoinedAt == null)
            {
                IsPendingMember = true;
            }

            // Load Transactions with Splits
            Transactions = await _context.GroupTransactions
                .Include(t => t.PaidByUser)
                .Include(t => t.GroupTransactionSplits).ThenInclude(s => s.User)
                .Where(t => t.GroupId == id)
                .OrderByDescending(t => t.TransactionDate)
                .AsNoTracking()
                .ToListAsync();

            // Load External Invitations
            ExternalInvitations = await _context.GroupInvitations
                .Where(i => i.GroupId == id && i.Status == "Pending")
                .AsNoTracking()
                .ToListAsync();

            // Prepare Group Participants (Members + Creator)
            GroupParticipants = Group.GroupMembers.Select(m => m.User).ToList();
            if (Group.CreatedByUser != null && !GroupParticipants.Any(u => u.Id == Group.CreatedByUserId))
            {
                GroupParticipants.Add(Group.CreatedByUser);
                GroupParticipants.Add(Group.CreatedByUser);
            }

            // Get Friends but exclude those already in the group
            var allFriends = await _friendshipService.GetFriendsAsync(userId);
            var groupMemberIds = GroupParticipants.Select(u => u.Id).ToHashSet();
            FriendsList = allFriends.Where(f => !groupMemberIds.Contains(f.FriendId)).ToList();


            CalculateFinancials(userId);
            
            // Prepare Category Spending JSON for Chart
            var categoryStats = Transactions
                .GroupBy(t => t.Category ?? "Other")
                .Select(g => new { Label = g.Key, Value = g.Sum(t => t.Amount) })
                .ToList();
            CategorySpendingJson = System.Text.Json.JsonSerializer.Serialize(categoryStats);

            // Set default values for forms
            if (ExpenseInput == null)
            {
                ExpenseInput = new AddExpenseInput
                {
                    PaidByUserId = userId,
                    Date = DateTime.Now
                };
            }

            if (GroupInput == null)
            {
                GroupInput = new UpdateGroupInput
                {
                    Name = Group.Name,
                    Description = Group.Description
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnGetSearchPeopleAsync(string query)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return Unauthorized();

            var users = await _friendshipService.SearchUsersAsync(query, userId);
            
            // Filter out users already in group? 
            // The frontend might need all results, but better to filter if we have the group ID in context.
            // Since this is a simple search, we will return list and let frontend handle visual disabling if needed, 
            // or just rely on the backend validation on Post.
            
            return new JsonResult(users.Select(u => new { 
                id = u.Id, 
                name = u.FullName, 
                avatar = u.ProfilePictureUrl, 
                email = u.UserName 
            })); 
        }


        public async Task<IActionResult> OnPostAddExpenseAsync(long id)
        {
            // Aggressively clear all validation errors that are NOT related to the current form
            var keysToRemove = ModelState.Keys
                .Where(k => !k.StartsWith("ExpenseInput"))
                .ToList();
                
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                ViewData["ShowAddExpenseModal"] = true;
                return await OnGetAsync(id);
            }

            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId))
            {
                 return RedirectToPage("/Auth/Login");
            }

            // Verify group exists and user is member
            var group = await _context.GroupExpenses
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (group == null) return NotFound();
            if (!group.GroupMembers.Any(m => m.UserId == userId) && group.CreatedByUserId != userId) return Forbid();

            // Handle Image Upload
            string? attachmentUrl = null;
            if (ExpenseInput.ProofImage != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ExpenseInput.ProofImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ExpenseInput.ProofImage.CopyToAsync(fileStream);
                }
                attachmentUrl = "/uploads/receipts/" + uniqueFileName;
            }

            // Create Transaction
            var transaction = new GroupTransaction
            {
                GroupId = id,
                PaidByUserId = ExpenseInput.PaidByUserId,
                Amount = ExpenseInput.Amount,
                Description = ExpenseInput.Description,
                TransactionDate = ExpenseInput.Date,
                Category = ExpenseInput.Category,
                AttachmentUrl = attachmentUrl,
                Currency = "VND", // Default currency
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.GroupTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Handle Splits
            // If SplitUserIds is empty or null, assume split equally among ALL group participants (Members + Creator)
            var allParticipantIds = group.GroupMembers.Select(m => m.UserId).ToList();
            if (!allParticipantIds.Contains(group.CreatedByUserId)) allParticipantIds.Add(group.CreatedByUserId);

            var membersToSplit = ExpenseInput.SplitUserIds != null && ExpenseInput.SplitUserIds.Any()
                ? ExpenseInput.SplitUserIds
                : allParticipantIds;

            // Safety check: Avoid DivisionByZero. 
            // If somehow list is still empty, default to the Payer.
            if (!membersToSplit.Any()) {
                 membersToSplit.Add(ExpenseInput.PaidByUserId);
            }
            
            // Ensure unique IDs
            membersToSplit = membersToSplit.Distinct().ToList();
            
            decimal splitAmount = ExpenseInput.Amount / membersToSplit.Count;

            foreach (var memberId in membersToSplit)
            {
                var split = new GroupTransactionSplit
                {
                    GroupTransactionId = transaction.Id,
                    UserId = memberId,
                    Amount = splitAmount, // Need robust rounding handling in production
                    IsPaid = memberId == ExpenseInput.PaidByUserId
                };
                
                if (memberId == ExpenseInput.PaidByUserId)
                {
                    split.PaidAt = DateTime.Now;
                }

                _context.GroupTransactionSplits.Add(split);
            }
            
            group.UpdatedAt = DateTime.Now;
            _context.Entry(group).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Thêm chi tiêu mới thành công.";
            return RedirectToPage("./Details", new { id = id });
        }

        public async Task<IActionResult> OnPostInviteMemberAsync(long id)
        {
            if (string.IsNullOrEmpty(InviteInput?.Emails) && (InviteInput?.UserIds == null || !InviteInput.UserIds.Any()))
            {
                ModelState.AddModelError("InviteInput.Emails", "Vui lòng chọn bạn bè hoặc nhập email.");
                ViewData["ShowInviteMemberModal"] = true;
                return await OnGetAsync(id);
            }

            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // 1. Process Direct User Adds (From Friends/Search)
            if (InviteInput.UserIds != null && InviteInput.UserIds.Any())
            {
                foreach (var targetUserId in InviteInput.UserIds)
                {
                     try 
                     {
                        await _groupExpenseService.AddMemberAsync(userId, new AddGroupMemberDto 
                        { 
                            GroupId = id, 
                            UserId = targetUserId,
                            Role = "Member"
                        });
                        // Auto-link friend request
                        await _friendshipService.SendFriendRequestAsync(userId, targetUserId);
                     }
                     catch (Exception)
                     {
                         // Ignore if already member or error
                     }
                }
            }

            // 2. Process Email Invites via Service
            if (!string.IsNullOrEmpty(InviteInput.Emails))
            {
                var inputEmails = InviteInput.Emails.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(e => e.Trim())
                                           .Distinct()
                                           .ToList();
                
                if (inputEmails.Any())
                {
                    try
                    {
                        var dto = new InviteGroupMemberDto 
                        { 
                            GroupId = id, 
                            Emails = inputEmails 
                        };
                        var sentEmails = await _groupExpenseService.InviteMembersAsync(userId, dto);
                        
                        if (sentEmails.Count > 0)
                        {
                            TempData["SuccessMessage"] = $"Đã xử lý {sentEmails.Count} lời mời.";
                        }
                        else
                        {
                             // If no emails sent, maybe they were duplicates. Better than "Error".
                             TempData["SuccessMessage"] = "Đã cập nhật lời mời (không có lời mời mới nào được gửi).";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = ex.Message;
                        ViewData["ShowInviteMemberModal"] = true;
                        return await OnGetAsync(id);
                    }
                }
            }
            else if (InviteInput.UserIds != null && InviteInput.UserIds.Any())
            {
                 TempData["SuccessMessage"] = "Đã thêm thành viên thành công.";
            }

            return RedirectToPage("./Details", new { id = id });
        }

        public async Task<IActionResult> OnPostAcceptInviteAsync(long id)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return RedirectToPage("/Auth/Login");

            var member = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
            if (member == null) return NotFound();

            member.JoinedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã tham gia nhóm thành công!";
            return RedirectToPage("./Details", new { id = id });
        }

        public async Task<IActionResult> OnPostDeclineInviteAsync(long id)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return RedirectToPage("/Auth/Login");

            var member = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
            if (member != null)
            {
                _context.GroupMembers.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Groups/Index"); // Redirect to list as they are no longer member
        }

        public async Task<IActionResult> OnPostUpdateGroupAsync(long id)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return RedirectToPage("/Auth/Login");

            var group = await _context.GroupExpenses.FindAsync(id);
            if (group == null) return NotFound();

            if (group.CreatedByUserId != userId) return Forbid(); // Only creator can edit

            // Clear validation errors from other forms
            var errorKeysToRemove = ModelState.Keys
                .Where(k => k.StartsWith("ExpenseInput") || k.StartsWith("InviteInput"))
                .ToList();
            foreach (var key in errorKeysToRemove)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid) 
            {
                ViewData["ShowSettingsModal"] = true;
                return await OnGetAsync(id);
            }

            group.Name = GroupInput.Name;
            group.Description = GroupInput.Description;
            group.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật thông tin nhóm thành công.";
            return RedirectToPage("./Details", new { id = id });
        }

        public async Task<IActionResult> OnPostLeaveGroupAsync(long id)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return RedirectToPage("/Auth/Login");

            var group = await _context.GroupExpenses
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (group == null) return NotFound();

            var member = group.GroupMembers.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return BadRequest("Bạn không phải thành viên nhóm này.");

            if (group.CreatedByUserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn là Admim, không thể rời nhóm. Hãy xóa nhóm hoặc chuyển quyền admin.";
                return RedirectToPage("./Details", new { id = id });
            }

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Groups/Index");
        }

        public async Task<IActionResult> OnPostExportAsync(long id)
        {
            var userIdS = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdS) || !long.TryParse(userIdS, out long userId)) return RedirectToPage("/Auth/Login");

            var group = await _context.GroupExpenses.FindAsync(id);
            if (group == null) return NotFound();

            var transactions = await _context.GroupTransactions
                .Include(t=>t.PaidByUser)
                .Where(t => t.GroupId == id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Date,Description,Category,Amount,PaidBy");
            foreach (var t in transactions)
            {
                builder.AppendLine($"{t.TransactionDate:yyyy-MM-dd},\"{t.Description}\",{t.Category},{t.Amount},{t.PaidByUser.FullName}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Statement-{group.Name}-{DateTime.Now:yyyyMMdd}.csv");
        }

        private void CalculateFinancials(long userId)
        {
            // 1. Total Spending of the Group
            TotalGroupSpending = Transactions.Sum(t => t.Amount);

            // 2. Amount the current user has physically paid
            CurrentUserPaid = Transactions
                .Where(t => t.PaidByUserId == userId)
                .Sum(t => t.Amount);

            // 3. The user's fair share of the expenses (what they consumed)
            CurrentUserShare = Transactions
                .SelectMany(t => t.GroupTransactionSplits)
                .Where(s => s.UserId == userId)
                .Sum(s => s.Amount);

            // 4. Net Balance
            // Positive = Owed to user
            // Negative = User owes
            CurrentUserBalance = CurrentUserPaid - CurrentUserShare;
        }

        /// <summary>
        /// Helper to get User Initials for UI
        /// </summary>
        public string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";
            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();
            if (parts.Length > 1) return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            return "U";
        }
    }
}
