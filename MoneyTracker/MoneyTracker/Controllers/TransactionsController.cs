using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Transaction;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions([FromQuery] TransactionFilterRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var transactions = await _transactionService.GetUserTransactionsAsync(
                userId.Value,
                request.Page,
                request.PageSize,
                request.StartDate,
                request.EndDate,
                request.Type?.ToString(),
                request.CategoryId
            );

            var transactionDtos = transactions.Select(MapToDto);
            return Ok(transactionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId.Value);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var transaction = new Transaction
            {
                UserId = userId.Value,
                CategoryId = request.CategoryId,
                Type = request.Type,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = request.TransactionDate
            };

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.Id }, MapToDto(createdTransaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(long id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId.Value);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            transaction.CategoryId = request.CategoryId;
            transaction.Type = request.Type;
            transaction.Amount = request.Amount;
            transaction.Description = request.Description;
            transaction.TransactionDate = request.TransactionDate;

            var updatedTransaction = await _transactionService.UpdateTransactionAsync(transaction);
            return Ok(MapToDto(updatedTransaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTransaction(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _transactionService.DeleteTransactionAsync(id, userId.Value);
            if (!success)
            {
                return NotFound("Transaction not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            CategoryId = transaction.CategoryId,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            Category = transaction.Category != null ? new CategoryDto
            {
                Id = transaction.Category.Id,
                Name = transaction.Category.Name,
                Type = transaction.Category.Type,
                Description = transaction.Category.Description,
                Icon = transaction.Category.Icon,
                Color = transaction.Category.Color,
                IsDefault = transaction.Category.IsDefault
            } : null
        };
    }
}
