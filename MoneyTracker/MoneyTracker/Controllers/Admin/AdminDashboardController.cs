using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;

namespace MoneyTracker.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IUnitOfWork unitOfWork,
        ILogger<AdminDashboardController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        try
        {
            var totalUsers = await _unitOfWork.Users.GetTotalCountAsync();
            var totalTransactions = await _unitOfWork.Transactions.GetTotalCountAsync();
            var totalCategories = await _unitOfWork.Categories.GetTotalCountAsync();
            var totalBudgets = await _unitOfWork.Budgets.GetTotalCountAsync();

            // Get recent activity
            var recentUsers = await _unitOfWork.Users.GetRecentUsersAsync(7); // Last 7 days
            var recentTransactions = await _unitOfWork.Transactions.GetRecentTransactionsAsync(7); // Last 7 days

            // Get monthly stats
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var monthlyUsers = await _unitOfWork.Users.GetMonthlyCountAsync(currentYear, currentMonth);
            var monthlyTransactions = await _unitOfWork.Transactions.GetMonthlyCountAsync(currentYear, currentMonth);

            var stats = new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalTransactions = totalTransactions,
                TotalCategories = totalCategories,
                TotalBudgets = totalBudgets,
                RecentUsers = recentUsers,
                RecentTransactions = recentTransactions,
                MonthlyUsers = monthlyUsers,
                MonthlyTransactions = monthlyTransactions,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalCategories { get; set; }
    public int TotalBudgets { get; set; }
    public int RecentUsers { get; set; }
    public int RecentTransactions { get; set; }
    public int MonthlyUsers { get; set; }
    public int MonthlyTransactions { get; set; }
    public DateTime GeneratedAt { get; set; }
}
