using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<TransactionResponseDto>>> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            try
            {
                var transactions = await _transactionService.GetUserTransactionsAsync(GetUserId(), filter);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving transactions", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetTransaction(long id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id, GetUserId());
                if (transaction == null) return NotFound();
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving transaction", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var transaction = await _transactionService.CreateTransactionAsync(userId, dto);
                
                // Email notification removed per request
                
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // In development/debugging, it's helpful to return the actual error
                return StatusCode(500, new { message = $"Error creating transaction: {ex.Message}", details = ex.ToString() });
            }
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<TransactionResponseDto>> TransferMoney([FromBody] TransferMoneyDto dto)
        {
            try
            {
                var transaction = await _transactionService.TransferMoneyAsync(GetUserId(), dto);
                return Ok(transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing transfer", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(long id, [FromBody] UpdateTransactionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            try
            {
                var transaction = await _transactionService.UpdateTransactionAsync(GetUserId(), dto);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating transaction", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(long id)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting transaction", details = ex.Message });
            }
        }

        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentTransactions([FromQuery] int limit = 5)
        {
            try
            {
                var userId = GetUserId();
                var filter = new TransactionFilterDto
                {
                    PageSize = limit,
                    PageNumber = 1
                };
                
                var transactions = await _transactionService.GetUserTransactionsAsync(userId, filter);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent transactions", details = ex.Message });
            }
        }
    }
}
