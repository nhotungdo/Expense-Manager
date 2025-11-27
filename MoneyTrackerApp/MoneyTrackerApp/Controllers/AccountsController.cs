using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// Controller for managing user wallets/accounts
/// Handles wallet creation, updates, balance management, and net worth calculations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ISharedAccountService _sharedAccountService;
    private readonly IBankConnectionService _bankConnectionService;
    private readonly INetWorthService _netWorthService;

    public AccountsController(
        IAccountService accountService,
        ISharedAccountService sharedAccountService,
        IBankConnectionService bankConnectionService,
        INetWorthService netWorthService)
    {
        _accountService = accountService;
        _sharedAccountService = sharedAccountService;
        _bankConnectionService = bankConnectionService;
        _netWorthService = netWorthService;
    }

    // Helper to get current user ID from JWT claims
    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID in token");
        return userId;
    }

    #region Account Management Endpoints

    /// <summary>
    /// Get all accounts for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AccountResponseDto>>> GetAllAccounts([FromQuery] bool includeInactive = false)
    {
        try
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId, includeInactive);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve accounts", details = ex.Message });
        }
    }

    /// <summary>
    /// Get account summaries (minimal info for dashboard)
    /// </summary>
    [HttpGet("summaries")]
    [ProducesResponseType(typeof(List<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AccountSummaryDto>>> GetAccountSummaries()
    {
        try
        {
            var userId = GetUserId();
            var summaries = await _accountService.GetAccountSummariesAsync(userId);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve account summaries", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific account by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponseDto>> GetAccount(long id)
    {
        try
        {
            var userId = GetUserId();
            var account = await _accountService.GetAccountByIdAsync(id, userId);

            if (account == null)
                return NotFound(new { error = "Account not found" });

            return Ok(account);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve account", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new wallet/account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponseDto>> CreateAccount([FromBody] CreateAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var account = await _accountService.CreateAccountAsync(userId, dto);

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create account", details = ex.Message });
        }
    }

    /// <summary>
    /// Update account details (name, icon, color, visibility)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponseDto>> UpdateAccount(long id, [FromBody] UpdateAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Id != id)
                return BadRequest(new { error = "ID mismatch" });

            var userId = GetUserId();
            var account = await _accountService.UpdateAccountAsync(userId, dto);

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update account", details = ex.Message });
        }
    }

    /// <summary>
    /// Adjust account balance manually
    /// </summary>
    [HttpPost("{id}/adjust-balance")]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponseDto>> AdjustBalance(long id, [FromBody] AdjustAccountBalanceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.AccountId != id)
                return BadRequest(new { error = "Account ID mismatch" });

            var userId = GetUserId();
            var account = await _accountService.AdjustBalanceAsync(userId, dto);

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to adjust balance", details = ex.Message });
        }
    }

    /// <summary>
    /// Toggle account visibility (activate/deactivate)
    /// </summary>
    [HttpPatch("{id}/visibility")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleVisibility(long id, [FromBody] bool isActive)
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.ToggleAccountVisibilityAsync(id, userId, isActive);

            if (!result)
                return NotFound(new { error = "Account not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to toggle visibility", details = ex.Message });
        }
    }

    /// <summary>
    /// Get hidden/inactive accounts
    /// </summary>
    [HttpGet("hidden/list")]
    [ProducesResponseType(typeof(List<AccountResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AccountResponseDto>>> GetHiddenAccounts()
    {
        try
        {
            var userId = GetUserId();
            var accounts = await _accountService.GetHiddenAccountsAsync(userId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve hidden accounts", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete an account permanently
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount(long id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.DeleteAccountAsync(id, userId);

            if (!result)
                return NotFound(new { error = "Account not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete account", details = ex.Message });
        }
    }

    #endregion

    #region Shared Account Endpoints

    /// <summary>
    /// Get accounts shared with the current user
    /// </summary>
    [HttpGet("shared/received")]
    [ProducesResponseType(typeof(List<SharedAccountListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SharedAccountListDto>>> GetSharedAccounts()
    {
        try
        {
            var userId = GetUserId();
            var sharedAccounts = await _sharedAccountService.GetSharedAccountsForUserAsync(userId);
            return Ok(sharedAccounts);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve shared accounts", details = ex.Message });
        }
    }

    /// <summary>
    /// Get users this account is shared with
    /// </summary>
    [HttpGet("{id}/sharing")]
    [ProducesResponseType(typeof(List<SharedAccountResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SharedAccountResponseDto>>> GetAccountSharing(long id)
    {
        try
        {
            var userId = GetUserId();
            var sharing = await _sharedAccountService.GetAccountSharingAsync(id, userId);
            return Ok(sharing);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve sharing information", details = ex.Message });
        }
    }

    /// <summary>
    /// Share an account with another user
    /// </summary>
    [HttpPost("{id}/share")]
    [ProducesResponseType(typeof(SharedAccountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SharedAccountResponseDto>> ShareAccount(long id, [FromBody] ShareAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.AccountId != id)
                return BadRequest(new { error = "Account ID mismatch" });

            var userId = GetUserId();
            var shared = await _sharedAccountService.ShareAccountAsync(userId, dto);

            return CreatedAtAction(nameof(GetAccountSharing), new { id = id }, shared);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to share account", details = ex.Message });
        }
    }

    /// <summary>
    /// Update permission for a shared account
    /// </summary>
    [HttpPut("shared/{sharedId}/permission")]
    [ProducesResponseType(typeof(SharedAccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SharedAccountResponseDto>> UpdatePermission(long sharedId, [FromBody] int permission)
    {
        try
        {
            var userId = GetUserId();
            var shared = await _sharedAccountService.UpdatePermissionAsync(userId, sharedId, permission);
            return Ok(shared);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update permission", details = ex.Message });
        }
    }

    /// <summary>
    /// Revoke access to a shared account
    /// </summary>
    [HttpDelete("shared/{sharedId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAccess(long sharedId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _sharedAccountService.RevokeAccessAsync(userId, sharedId);

            if (!result)
                return NotFound(new { error = "Shared account not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to revoke access", details = ex.Message });
        }
    }

    #endregion

    #region Bank Connection Endpoints

    /// <summary>
    /// Get all bank connections for the current user
    /// </summary>
    [HttpGet("bank-connections")]
    [ProducesResponseType(typeof(List<BankConnectionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BankConnectionResponseDto>>> GetBankConnections()
    {
        try
        {
            var userId = GetUserId();
            var connections = await _bankConnectionService.GetUserBankConnectionsAsync(userId);
            return Ok(connections);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve bank connections", details = ex.Message });
        }
    }

    /// <summary>
    /// Link a bank account
    /// </summary>
    [HttpPost("bank-connections")]
    [ProducesResponseType(typeof(BankConnectionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BankConnectionResponseDto>> LinkBankAccount([FromBody] LinkBankAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var connection = await _bankConnectionService.LinkBankAccountAsync(userId, dto);

            return CreatedAtAction(nameof(GetBankConnections), connection);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to link bank account", details = ex.Message });
        }
    }

    /// <summary>
    /// Unlink a bank account
    /// </summary>
    [HttpDelete("bank-connections/{connectionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlinkBankAccount(long connectionId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _bankConnectionService.UnlinkBankAccountAsync(connectionId, userId);

            if (!result)
                return NotFound(new { error = "Bank connection not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to unlink bank account", details = ex.Message });
        }
    }

    #endregion

    #region Net Worth Endpoints

    /// <summary>
    /// Calculate and get complete net worth summary
    /// </summary>
    [HttpGet("net-worth")]
    [ProducesResponseType(typeof(NetWorthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NetWorthDto>> GetNetWorth([FromQuery] bool includeHidden = false)
    {
        try
        {
            var userId = GetUserId();
            var netWorth = await _netWorthService.CalculateNetWorthAsync(userId, includeHidden);
            return Ok(netWorth);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to calculate net worth", details = ex.Message });
        }
    }

    /// <summary>
    /// Get total assets
    /// </summary>
    [HttpGet("net-worth/assets")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetTotalAssets()
    {
        try
        {
            var userId = GetUserId();
            var assets = await _netWorthService.GetTotalAssetsAsync(userId);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve total assets", details = ex.Message });
        }
    }

    /// <summary>
    /// Get total debt
    /// </summary>
    [HttpGet("net-worth/debt")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetTotalDebt()
    {
        try
        {
            var userId = GetUserId();
            var debt = await _netWorthService.GetTotalDebtAsync(userId);
            return Ok(debt);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve total debt", details = ex.Message });
        }
    }

    #endregion
}
