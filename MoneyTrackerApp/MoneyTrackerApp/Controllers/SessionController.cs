using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSessions()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

            var sessions = await _sessionService.GetActiveSessionsAsync(userId);
            
            var currentSid = User.FindFirst("sid")?.Value;
            
            var result = sessions.Select(s => new 
            {
                s.Id,
                s.DeviceName,
                s.Browser,
                s.OperatingSystem,
                s.IpAddress,
                s.Location,
                s.LastActiveAt,
                s.CreatedAt,
                IsCurrent = s.Id.ToString().Equals(currentSid, StringComparison.OrdinalIgnoreCase)
            });

            return Ok(result);
        }

        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> RevokeSession(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

            // Verify ownership
            var session = await _sessionService.GetSessionByIdAsync(id);
            if (session == null || session.UserId != userId)
            {
                return NotFound();
            }

            await _sessionService.TerminateSessionAsync(id);
            return Ok();
        }

        [HttpPost("revoke-others")]
        public async Task<IActionResult> RevokeAllExceptCurrent()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

            var currentSidStr = User.FindFirst("sid")?.Value;
            if (!Guid.TryParse(currentSidStr, out var currentSid))
            {
                return BadRequest("Current session ID not found.");
            }

            await _sessionService.TerminateAllSessionsExceptAsync(userId, currentSid);
            return Ok();
        }
    }
}
