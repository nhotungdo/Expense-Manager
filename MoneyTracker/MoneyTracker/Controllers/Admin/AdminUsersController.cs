using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Core.Models;
using MoneyTracker.Models;
using MoneyTracker.DTOs.Auth;

namespace MoneyTracker.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<AdminUsersController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        try
        {
            var users = await _unitOfWork.Users.GetPagedAsync(page, pageSize, search);

            var userDtos = users.Items.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                FullName = u.FullName ?? "",
                ProfilePictureUrl = u.ProfilePictureUrl ?? "",
                OnboardingCompleted = u.OnboardingCompleted,
                Role = u.Role,
                Enabled = u.Enabled,
                LastLogin = u.LastLogin,
                CreatedAt = u.CreatedAt,
                Language = u.Language,
                DefaultCurrency = u.DefaultCurrency,
                Timezone = u.Timezone,
                Theme = u.Theme
            }).ToList();

            var result = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = users.TotalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)users.TotalCount / pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult> UpdateUserRole(long id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Role = request.Role;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User {UserId} role updated to {Role}", id, request.Role);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/toggle-lock")]
    public async Task<ActionResult> ToggleUserLock(long id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Enabled = !user.Enabled;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User {UserId} lock status changed to {Enabled}", id, user.Enabled);
            return Ok(new { Enabled = user.Enabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user lock");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}
