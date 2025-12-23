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
    public class DebtController : ControllerBase
    {
        private readonly IDebtService _debtService;

        public DebtController(IDebtService debtService)
        {
            _debtService = debtService;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<DebtResponseDto>>> GetDebts([FromQuery] int? type)
        {
            try
            {
                var debts = await _debtService.GetUserDebtsAsync(GetUserId(), type);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving debts", details = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DebtSummaryDto>> GetSummary()
        {
            try
            {
                var summary = await _debtService.GetDebtSummaryAsync(GetUserId());
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving debt summary", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DebtResponseDto>> GetDebt(long id)
        {
            try
            {
                var debt = await _debtService.GetDebtByIdAsync(id, GetUserId());
                if (debt == null) return NotFound();
                return Ok(debt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving debt", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DebtResponseDto>> CreateDebt([FromBody] CreateDebtDto dto)
        {
            try
            {
                var debt = await _debtService.CreateDebtAsync(GetUserId(), dto);
                return CreatedAtAction(nameof(GetDebt), new { id = debt.Id }, debt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating debt", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DebtResponseDto>> UpdateDebt(long id, [FromBody] UpdateDebtDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            
            try
            {
                var debt = await _debtService.UpdateDebtAsync(GetUserId(), dto);
                return Ok(debt);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating debt", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(long id)
        {
            try
            {
                var result = await _debtService.DeleteDebtAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting debt", details = ex.Message });
            }
        }

        [HttpPost("payment")]
        public async Task<ActionResult<DebtResponseDto>> RecordPayment([FromBody] RecordDebtPaymentDto dto)
        {
            try
            {
                var debt = await _debtService.RecordPaymentAsync(GetUserId(), dto);
                return Ok(debt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording payment", details = ex.Message });
            }
        }
    }
}
