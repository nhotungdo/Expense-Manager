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
}
