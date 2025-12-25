using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services;

public interface IChallengeService
{
    Task<List<Challenge>> GetAvailableChallengesAsync(long userId);
    Task<List<UserChallenge>> GetUserChallengesAsync(long userId);
    Task JoinChallengeAsync(long userId, long challengeId);
    Task UpdateChallengeProgressAsync(long userId, Transaction transaction);
    Task CheckChallengeCompletionAsync(long userId); // For time-based expiry
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(long userId);
    Task<UserChallenge?> GetUserChallengeAsync(long userId, long id);
}

public class ChallengeService : IChallengeService
{
    private readonly ExpenseManagerContext _context;
    private readonly INotificationService _notificationService;

    public ChallengeService(ExpenseManagerContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<Challenge>> GetAvailableChallengesAsync(long userId)
    {
        // Get challenges user has NOT joined or joined but failed/completed long ago (re-joinable?)
        // simplified: Just all challenges not currently active
        var activeChallengeIds = await _context.UserChallenges
            .Where(uc => uc.UserId == userId && uc.Status == "Active")
            .Select(uc => uc.ChallengeId)
            .ToListAsync();

        return await _context.Challenges
            .Where(c => !activeChallengeIds.Contains(c.Id))
            .ToListAsync();
    }

    public async Task<List<UserChallenge>> GetUserChallengesAsync(long userId)
    {
        return await _context.UserChallenges
            .Include(uc => uc.Challenge)
            .Where(uc => uc.UserId == userId && uc.Status == "Active")
            .ToListAsync();
    }

    public async Task<UserChallenge?> GetUserChallengeAsync(long userId, long id)
    {
        return await _context.UserChallenges
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);
    }

    public async Task JoinChallengeAsync(long userId, long challengeId)
    {
        var challenge = await _context.Challenges.FindAsync(challengeId);
        if (challenge == null) throw new Exception("Challenge not found");

        var existing = await _context.UserChallenges
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId && uc.Status == "Active");
        
        if (existing != null) throw new Exception("Already joined this challenge");

        var userChallenge = new UserChallenge
        {
            UserId = userId,
            ChallengeId = challengeId,
            Status = "Active",
            Progress = 0,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Tham gia thử thách thành công!",
            Message = $"Bạn đã tham gia thử thách: {challenge.Title}. Hãy cố gắng nhé!",
            Type = "Challenge",
            ActionUrl = "/Games/Challenges"
        });
    }

    public async Task UpdateChallengeProgressAsync(long userId, Transaction transaction)
    {
        var activeChallenges = await _context.UserChallenges
            .Include(uc => uc.Challenge)
            .Where(uc => uc.UserId == userId && uc.Status == "Active")
            .ToListAsync();

        foreach (var uc in activeChallenges)
        {
            if (uc.Challenge.Type == "NoSpend")
            {
                // Logic: Limit spending in specific category
                if (transaction.TransactionType == 2 && // Expense
                    uc.Challenge.TargetCategoryId.HasValue && 
                    transaction.CategoryId == uc.Challenge.TargetCategoryId.Value)
                {
                    uc.Progress += transaction.Amount;
                    
                    if (uc.Challenge.TargetAmount.HasValue && uc.Progress > uc.Challenge.TargetAmount.Value)
                    {
                        uc.Status = "Failed";
                        await NotifyChallengeUpdate(userId, uc.Challenge.Title, "Thất bại! Bạn đã chi tiêu vượt hạn mức.");
                    }
                }
            }
            else if (uc.Challenge.Type == "SavingsTarget")
            {
                // Logic: Accumulate savings (Transfer to Savings Account or specific Income?)
                // Assuming Savings Goal Transfers (TransType 3 to Savings Account)
                // Need to identify Savings Account. Ideally we check AccountType == 4 (Savings)
                
                // Let's assume TransactionType 3 (Transfer) AND Target Account is Savings
                var targetAccount = await _context.Accounts.FindAsync(transaction.PairedAccountId);
                if (transaction.TransactionType == 3 && targetAccount != null && targetAccount.AccountType == 4)
                {
                    uc.Progress += transaction.Amount;

                    if (uc.Challenge.TargetAmount.HasValue && uc.Progress >= uc.Challenge.TargetAmount.Value)
                    {
                        uc.Status = "Completed";
                        uc.CompletedAt = DateTime.UtcNow;
                        await NotifyChallengeUpdate(userId, uc.Challenge.Title, "Chúc mừng! Bạn đã hoàn thành thử thách.");
                        
                        // Award Points (Future: Add User.Points field)
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task CheckChallengeCompletionAsync(long userId)
    {
        // Run periodically to check if challenge expired (DurationDays)
        // If Type == NoSpend and Time Expired and Status == Active -> Success!
        
        var activeChallenges = await _context.UserChallenges
            .Include(uc => uc.Challenge)
            .Where(uc => uc.UserId == userId && uc.Status == "Active")
            .ToListAsync();

        foreach (var uc in activeChallenges)
        {
            var expiryDate = uc.JoinedAt.AddDays(uc.Challenge.DurationDays);
            if (DateTime.UtcNow > expiryDate)
            {
                if (uc.Challenge.Type == "NoSpend")
                {
                    // Survived the duration without exceeding limit
                    uc.Status = "Completed";
                    uc.CompletedAt = DateTime.UtcNow;
                    await NotifyChallengeUpdate(userId, uc.Challenge.Title, "Chúc mừng! Bạn đã hoàn thành thử thách không chi tiêu.");
                }
                else if (uc.Challenge.Type == "SavingsTarget")
                {
                    // Time up and not reached target
                    uc.Status = "Failed"; 
                    await NotifyChallengeUpdate(userId, uc.Challenge.Title, "Hết thời gian! Bạn chưa đạt mục tiêu tiết kiệm.");
                }
            }
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task NotifyChallengeUpdate(long userId, string challengeTitle, string message)
    {
        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = $"Thử thách: {challengeTitle}",
            Message = message,
            Type = "Challenge",
            ActionUrl = "/Games/Challenges"
        });
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(long userId)
    {
        // 1. Get Friends
        var friendships = await _context.Friendships
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == 1)
            .ToListAsync();

        var friendIds = friendships.Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId).ToList();
        friendIds.Add(userId); // Include self

        // 2. Calculate Score for each user (Last 30 days Savings Rate)
        var monthAgo = DateTime.UtcNow.AddDays(-30);
        
        var stats = await _context.Transactions
            .Where(t => friendIds.Contains(t.UserId) && t.TransactionDate >= monthAgo)
            .GroupBy(t => t.UserId)
            .Select(g => new 
            {
                UserId = g.Key,
                Income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                Expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount)
            })
            .ToListAsync();

        var users = await _context.Users
            .Where(u => friendIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        var leaderboard = new List<LeaderboardEntryDto>();

        foreach (var stat in stats)
        {
            if (!users.ContainsKey(stat.UserId)) continue;
            var user = users[stat.UserId];

            double savingsRate = 0;
            if (stat.Income > 0)
            {
                savingsRate = (double)((stat.Income - stat.Expense) / stat.Income * 100);
            }
            
            // Score = Savings Rate (Max 100). If negative, 0.
            // Or maybe combine with Points? For now, Savings % is the key metric requested.
            
            var badge = "Newbie";
            if (savingsRate >= 50) badge = "Master Saver 🏆";
            else if (savingsRate >= 30) badge = "Pro Saver 🥇";
            else if (savingsRate >= 10) badge = "Good Saver 🥈";
            else if (savingsRate < 0) badge = "Spender 💸";

            leaderboard.Add(new LeaderboardEntryDto
            {
                UserId = user.Id,
                UserName = user.FullName ?? user.UserName ?? "Unknown",
                AvatarUrl = user.ProfilePictureUrl,
                Score = Math.Round(savingsRate, 1),
                Badge = badge
            });
        }

        // Add users with no transactions (0 score)
        foreach (var id in friendIds)
        {
            if (!leaderboard.Any(l => l.UserId == id) && users.ContainsKey(id))
            {
                var user = users[id];
                leaderboard.Add(new LeaderboardEntryDto
                {
                    UserId = user.Id,
                    UserName = user.FullName ?? user.UserName ?? "Unknown",
                    AvatarUrl = user.ProfilePictureUrl,
                    Score = 0,
                    Rank = 0,
                    Badge = "Sleepy 😴"
                });
            }
        }

        return leaderboard.OrderByDescending(x => x.Score).Select((x, i) => { x.Rank = i + 1; return x; }).ToList();
    }
}
