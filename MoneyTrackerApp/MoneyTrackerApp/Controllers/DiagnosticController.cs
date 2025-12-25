using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.Enums;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly ExpenseManagerContext _context;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(
        ExpenseManagerContext context,
        ISubscriptionService subscriptionService,
        ILogger<DiagnosticController> logger)
    {
        _context = context;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpGet("check-subscription")]
    public async Task<IActionResult> CheckSubscription()
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Please login to check subscription" });
            }

            // Get user info
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Get active subscription using the service
            var activeSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Get all subscriptions for this user
            var allSubscriptions = await _context.Subscriptions
                .Include(s => s.Package)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.PackageId,
                    PackageName = s.Package.Name,
                    s.Status,
                    StatusName = ((SubscriptionStatus)s.Status).ToString(),
                    s.StartDate,
                    s.EndDate,
                    s.AutoRenew,
                    s.CreatedAt,
                    PackageFeatures = new
                    {
                        s.Package.HasAdvancedReports,
                        s.Package.HasAiAdvisor,
                        s.Package.HasGroupExpense,
                        s.Package.HasPrioritySupport,
                        s.Package.MaxAccounts,
                        s.Package.MaxBudgets,
                        s.Package.MaxTransactions
                    }
                })
                .ToListAsync();

            // Get all service packages
            var allPackages = await _context.ServicePackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.PackageType,
                    PackageTypeName = ((PackageType)p.PackageType).ToString(),
                    p.Price,
                    p.DurationDays,
                    Features = new
                    {
                        p.HasAdvancedReports,
                        p.HasAiAdvisor,
                        p.HasGroupExpense,
                        p.HasPrioritySupport,
                        p.MaxAccounts,
                        p.MaxBudgets,
                        p.MaxTransactions
                    },
                    p.IsActive,
                    p.IsPopular
                })
                .ToListAsync();

            // Get payment history
            var payments = await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Subscription.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.SubscriptionId,
                    p.Amount,
                    p.Status,
                    StatusName = ((PaymentStatus)p.Status).ToString(),
                    p.PaymentMethod,
                    p.TransactionId,
                    p.PaidAt,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName
                },
                activeSubscription = activeSubscription != null ? new
                {
                    activeSubscription.Id,
                    activeSubscription.PackageId,
                    activeSubscription.PackageName,
                    activeSubscription.Status,
                    activeSubscription.StatusName,
                    activeSubscription.StartDate,
                    activeSubscription.EndDate,
                    activeSubscription.DaysRemaining,
                    activeSubscription.AutoRenew,
                    features = new
                    {
                        activeSubscription.HasAdvancedReports,
                        activeSubscription.HasAiAdvisor,
                        activeSubscription.HasGroupExpense,
                        activeSubscription.MaxAccounts
                    }
                } : null,
                allSubscriptions,
                allPackages,
                payments,
                diagnosis = new
                {
                    hasActiveSubscription = activeSubscription != null,
                    activeSubscriptionStatus = activeSubscription?.StatusName ?? "None",
                    packageName = activeSubscription?.PackageName ?? "None",
                    featuresEnabled = activeSubscription != null ? new
                    {
                        advancedReports = activeSubscription.HasAdvancedReports,
                        aiAdvisor = activeSubscription.HasAiAdvisor,
                        groupExpense = activeSubscription.HasGroupExpense,
                        maxAccounts = activeSubscription.MaxAccounts
                    } : null,
                    possibleIssues = GetPossibleIssues(activeSubscription, allSubscriptions)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subscription");
            return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("fix-subscription/{subscriptionId}")]
    public async Task<IActionResult> FixSubscription(long subscriptionId)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Package)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null)
            {
                return NotFound(new { message = "Subscription not found" });
            }

            // Check if package has features enabled
            var package = subscription.Package;
            var issues = new List<string>();

            if (!package.HasAdvancedReports)
                issues.Add("Package does not have HasAdvancedReports enabled");
            if (!package.HasAiAdvisor)
                issues.Add("Package does not have HasAiAdvisor enabled");
            if (!package.HasGroupExpense)
                issues.Add("Package does not have HasGroupExpense enabled");
            if (package.MaxAccounts <= 3)
                issues.Add($"Package MaxAccounts is only {package.MaxAccounts} (should be higher for Pro)");

            if (issues.Any())
            {
                return Ok(new
                {
                    message = "Package configuration issues found",
                    issues,
                    recommendation = "Run the SQL script to update package features",
                    sqlScript = GenerateFixScript(package.Id, package.Name)
                });
            }

            return Ok(new
            {
                message = "No issues found with package configuration",
                package = new
                {
                    package.Id,
                    package.Name,
                    package.HasAdvancedReports,
                    package.HasAiAdvisor,
                    package.HasGroupExpense,
                    package.MaxAccounts
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing subscription");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private List<string> GetPossibleIssues(DTOs.SubscriptionDto? activeSubscription, dynamic allSubscriptions)
    {
        var issues = new List<string>();

        if (activeSubscription == null)
        {
            issues.Add("No active subscription found");
            if (allSubscriptions.Count > 0)
            {
                issues.Add("User has subscriptions but none are active - check subscription status");
            }
            return issues;
        }

        if (!activeSubscription.HasAdvancedReports)
            issues.Add("Advanced Reports feature is disabled in package configuration");
        
        if (!activeSubscription.HasAiAdvisor)
            issues.Add("AI Advisor feature is disabled in package configuration");
        
        if (!activeSubscription.HasGroupExpense)
            issues.Add("Group Expense feature is disabled in package configuration");
        
        if (activeSubscription.MaxAccounts <= 3)
            issues.Add($"MaxAccounts is {activeSubscription.MaxAccounts} (Free tier level) - should be higher for Pro package");

        if (activeSubscription.EndDate < DateTime.UtcNow)
            issues.Add("Subscription has expired");

        return issues;
    }

    private string GenerateFixScript(int packageId, string packageName)
    {
        return $@"
-- Fix package features for {packageName} (ID: {packageId})
UPDATE ServicePackages
SET 
    HasAdvancedReports = 1,
    HasAiAdvisor = 1,
    HasGroupExpense = 1,
    HasPrioritySupport = 1,
    MaxAccounts = 10,
    MaxBudgets = 50,
    MaxTransactions = -1,
    UpdatedAt = GETUTCDATE()
WHERE Id = {packageId};

-- Verify the update
SELECT 
    Id, Name, PackageType,
    HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport,
    MaxAccounts, MaxBudgets, MaxTransactions
FROM ServicePackages
WHERE Id = {packageId};
";
    }
}
