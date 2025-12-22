namespace MoneyTrackerApp.DTOs;

public class FriendDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? UserName { get; set; }
}

public class FriendshipDto
{
    public long Id { get; set; } // Friendship Id
    public long FriendId { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public string? FriendAvatar { get; set; }
    public int Status { get; set; } // 0: Pending, 1: Accepted
    public bool IsRequester { get; set; } // True if current user sent the request
    public DateTime? CreatedAt { get; set; }
}
