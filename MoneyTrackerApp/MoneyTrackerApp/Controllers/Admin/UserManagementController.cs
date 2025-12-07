using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userService;

        public UserManagementController(IUserManagementService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            var users = await _userService.GetAllUsersAsync(filter);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUser(long id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockUser(long id, [FromBody] LockUserDto dto)
        {
            var result = await _userService.LockUserAsync(id, dto.DurationMinutes);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockUser(long id)
        {
            var result = await _userService.UnlockUserAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(long id)
        {
            var result = await _userService.ResetPasswordAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/assign-role")]
        public async Task<IActionResult> AssignRole(long id, [FromBody] AssignRoleDto dto)
        {
            var result = await _userService.AssignRoleAsync(id, dto.Role);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
