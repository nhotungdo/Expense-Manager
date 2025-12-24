using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// API Controller for Group Expense and Split Bill features
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupExpenseController : ControllerBase
{
    private readonly IGroupExpenseService _groupExpenseService;
    private readonly ILogger<GroupExpenseController> _logger;

    public GroupExpenseController(IGroupExpenseService groupExpenseService, ILogger<GroupExpenseController> logger)
    {
        _groupExpenseService = groupExpenseService;
        _logger = logger;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get all groups for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GroupExpenseResponseDto>>> GetUserGroups()
    {
        try
        {
            var userId = GetUserId();
            var groups = await _groupExpenseService.GetUserGroupsAsync(userId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups");
            return StatusCode(500, new { message = "An error occurred while retrieving groups" });
        }
    }

    /// <summary>
    /// Get a specific group by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GroupExpenseResponseDto>> GetGroupById(long id)
    {
        try
        {
            var userId = GetUserId();
            var group = await _groupExpenseService.GetGroupByIdAsync(id, userId);
            
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the group" });
        }
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GroupExpenseResponseDto>> CreateGroup([FromBody] CreateGroupExpenseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var group = await _groupExpenseService.CreateGroupAsync(userId, dto);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return StatusCode(500, new { message = "An error occurred while creating the group" });
        }
    }

    /// <summary>
    /// Update a group
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<GroupExpenseResponseDto>> UpdateGroup(long id, [FromBody] UpdateGroupExpenseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            var userId = GetUserId();
            var group = await _groupExpenseService.UpdateGroupAsync(userId, dto);
            return Ok(group);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the group" });
        }
    }

    /// <summary>
    /// Delete a group
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGroup(long id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupExpenseService.DeleteGroupAsync(id, userId);
            
            if (!result)
                return NotFound(new { message = "Group not found or you don't have permission" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the group" });
        }
    }

    /// <summary>
    /// Add a member to a group
    /// </summary>
    [HttpPost("members")]
    public async Task<ActionResult<GroupMemberDto>> AddMember([FromBody] AddGroupMemberDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var member = await _groupExpenseService.AddMemberAsync(userId, dto);
            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to group");
            return StatusCode(500, new { message = "An error occurred while adding the member" });
        }
    }

    /// <summary>
    /// Remove a member from a group
    /// </summary>
    [HttpDelete("groups/{groupId}/members/{memberId}")]
    public async Task<ActionResult> RemoveMember(long groupId, long memberId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupExpenseService.RemoveMemberAsync(groupId, memberId, userId);
            
            if (!result)
                return NotFound(new { message = "Member not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group");
            return StatusCode(500, new { message = "An error occurred while removing the member" });
        }
    }

    /// <summary>
    /// Create a group transaction with automatic splitting
    /// </summary>
    [HttpPost("transactions")]
    public async Task<ActionResult<GroupTransactionResponseDto>> CreateTransaction([FromBody] CreateGroupTransactionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var transaction = await _groupExpenseService.CreateGroupTransactionAsync(userId, dto);
            return Ok(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group transaction");
            return StatusCode(500, new { message = "An error occurred while creating the transaction" });
        }
    }

    /// <summary>
    /// Update a group transaction
    /// </summary>
    [HttpPut("transactions/{id}")]
    public async Task<ActionResult<GroupTransactionResponseDto>> UpdateTransaction(long id, [FromBody] UpdateGroupTransactionDto dto)
    {
        try
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            var transaction = await _groupExpenseService.UpdateGroupTransactionAsync(userId, dto);
            return Ok(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction");
            return StatusCode(500, new { message = "An error occurred while updating the transaction" });
        }
    }

    /// <summary>
    /// Delete a group transaction
    /// </summary>
    [HttpDelete("transactions/{id}")]
    public async Task<ActionResult> DeleteTransaction(long id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupExpenseService.DeleteGroupTransactionAsync(id, userId);
            
            if (!result) return NotFound(new { message = "Transaction not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
             return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction");
            return StatusCode(500, new { message = "An error occurred while deleting the transaction" });
        }
    }

    /// <summary>
    /// Get all transactions for a group
    /// </summary>
    [HttpGet("{groupId}/transactions")]
    public async Task<ActionResult<List<GroupTransactionResponseDto>>> GetGroupTransactions(long groupId)
    {
        try
        {
            var userId = GetUserId();
            var transactions = await _groupExpenseService.GetGroupTransactionsAsync(groupId, userId);
            return Ok(transactions);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group transactions");
            return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
        }
    }

    /// <summary>
    /// Get group balances (who owes whom)
    /// </summary>
    [HttpGet("{groupId}/balances")]
    public async Task<ActionResult<GroupBalanceSummaryDto>> GetGroupBalances(long groupId)
    {
        try
        {
            var userId = GetUserId();
            var balances = await _groupExpenseService.GetGroupBalancesAsync(groupId, userId);
            return Ok(balances);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group balances");
            return StatusCode(500, new { message = "An error occurred while calculating balances" });
        }
    }

    /// <summary>
    /// Calculate optimal debt settlements for a group
    /// </summary>
    [HttpGet("{groupId}/settlements")]
    public async Task<ActionResult<List<GroupDebtDto>>> CalculateSettlements(long groupId)
    {
        try
        {
            var userId = GetUserId();
            var settlements = await _groupExpenseService.CalculateSettlementsAsync(groupId, userId);
            return Ok(settlements);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating settlements");
            return StatusCode(500, new { message = "An error occurred while calculating settlements" });
        }
    }

    /// <summary>
    /// Get members of a group with their roles and statistics
    /// </summary>
    [HttpGet("{groupId}/members")]
    public async Task<ActionResult<List<GroupMemberDetailDto>>> GetGroupMembers(long groupId)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify user has access to this group
            var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            // Get members with statistics
            var members = await _groupExpenseService.GetGroupMembersWithStatsAsync(groupId);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group members");
            return StatusCode(500, new { message = "An error occurred while retrieving members" });
        }
    }

    /// <summary>
    /// Get categories for a group
    /// </summary>
    [HttpGet("{groupId}/categories")]
    public async Task<ActionResult<List<GroupCategoryDto>>> GetGroupCategories(long groupId)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify user has access to this group
            var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            // For now, return default categories
            // TODO: Implement custom categories per group
            var categories = GetDefaultCategories();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group categories");
            return StatusCode(500, new { message = "An error occurred while retrieving categories" });
        }
    }

    /// <summary>
    /// Get statistics for a group
    /// </summary>
    [HttpGet("{groupId}/statistics")]
    public async Task<ActionResult<GroupStatisticsDto>> GetGroupStatistics(long groupId)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify user has access to this group
            var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            var transactions = await _groupExpenseService.GetGroupTransactionsAsync(groupId, userId);
            
            // Calculate statistics
            var totalExpenses = transactions.Sum(t => t.Amount);
            var averageExpense = transactions.Any() ? totalExpenses / transactions.Count : 0;
            
            // Calculate trend (compare last 30 days vs previous 30 days)
            var now = DateTime.UtcNow;
            var last30Days = transactions.Where(t => t.TransactionDate >= now.AddDays(-30)).Sum(t => t.Amount);
            var previous30Days = transactions.Where(t => t.TransactionDate >= now.AddDays(-60) && t.TransactionDate < now.AddDays(-30)).Sum(t => t.Amount);
            var trend = previous30Days > 0 ? ((last30Days - previous30Days) / previous30Days) * 100 : 0;

            var statistics = new GroupStatisticsDto
            {
                TotalExpenses = totalExpenses,
                AverageExpense = averageExpense,
                ExpenseTrend = (double)trend,
                TransactionCount = transactions.Count,
                LastTransactionDate = transactions.Any() ? transactions.Max(t => t.TransactionDate) : null
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group statistics");
            return StatusCode(500, new { message = "An error occurred while calculating statistics" });
        }
    }

    /// <summary>
    /// Get budget information for a group
    /// </summary>
    [HttpGet("{groupId}/budget")]
    public async Task<ActionResult<GroupBudgetDto>> GetGroupBudget(long groupId)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify user has access to this group
            var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            var transactions = await _groupExpenseService.GetGroupTransactionsAsync(groupId, userId);
            var spent = transactions.Sum(t => t.Amount);

            // For now, use a default budget limit
            // TODO: Implement custom budget limits per group
            var budget = new GroupBudgetDto
            {
                Limit = 10000000, // 10 million VND default
                Spent = spent,
                Remaining = 10000000 - spent
            };

            return Ok(budget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group budget");
            return StatusCode(500, new { message = "An error occurred while retrieving budget" });
        }
    }

    /// <summary>
    /// Get budget alerts for a group
    /// </summary>
    [HttpGet("{groupId}/alerts")]
    public async Task<ActionResult<List<GroupBudgetAlertDto>>> GetBudgetAlerts(long groupId)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify user has access to this group
            var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
            if (group == null)
                return NotFound(new { message = "Group not found or you don't have access" });

            var budget = await GetGroupBudget(groupId);
            var budgetData = (budget.Result as OkObjectResult)?.Value as GroupBudgetDto;
            
            var alerts = new List<GroupBudgetAlertDto>();

            if (budgetData != null)
            {
                var percentage = (budgetData.Spent / budgetData.Limit) * 100;

                if (percentage >= 100)
                {
                    alerts.Add(new GroupBudgetAlertDto
                    {
                        Id = 1,
                        Severity = "danger",
                        Icon = "fas fa-exclamation-circle",
                        Title = "Vượt ngân sách",
                        Message = "Nhóm đã vượt quá ngân sách đề ra"
                    });
                }
                else if (percentage >= 80)
                {
                    alerts.Add(new GroupBudgetAlertDto
                    {
                        Id = 2,
                        Severity = "warning",
                        Icon = "fas fa-exclamation-triangle",
                        Title = "Gần đạt ngân sách",
                        Message = $"Đã sử dụng {Math.Round(percentage)}% ngân sách"
                    });
                }
            }

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget alerts");
            return StatusCode(500, new { message = "An error occurred while retrieving alerts" });
        }
    }

    // Helper method for default categories
    private List<GroupCategoryDto> GetDefaultCategories()
    {
        return new List<GroupCategoryDto>
        {
            new GroupCategoryDto { Id = 1, Name = "Ăn uống", Icon = "fas fa-utensils", Color = "#ef4444", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 },
            new GroupCategoryDto { Id = 2, Name = "Di chuyển", Icon = "fas fa-car", Color = "#f59e0b", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 },
            new GroupCategoryDto { Id = 3, Name = "Mua sắm", Icon = "fas fa-shopping-bag", Color = "#8b5cf6", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 },
            new GroupCategoryDto { Id = 4, Name = "Giải trí", Icon = "fas fa-film", Color = "#ec4899", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 },
            new GroupCategoryDto { Id = 5, Name = "Nhà ở", Icon = "fas fa-home", Color = "#10b981", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 },
            new GroupCategoryDto { Id = 6, Name = "Khác", Icon = "fas fa-tag", Color = "#94a3b8", TransactionCount = 0, TotalAmount = 0, AverageAmount = 0 }
        };
    }
}
