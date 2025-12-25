namespace MoneyTrackerApp.DTOs;

public class LeaderboardEntryDto
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string AvatarUrl { get; set; }
    public double Score { get; set; } // Can be Health Score or Savings %
    public int Rank { get; set; }
    public string Badge { get; set; } // "Saver", "Spender", etc.
}
