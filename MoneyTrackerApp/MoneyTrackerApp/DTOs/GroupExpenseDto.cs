using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a group expense
/// </summary>
public class CreateGroupExpenseDto
{
    [Required(ErrorMessage = "Group name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Group name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }

    public List<long>? MemberUserIds { get; set; }
}

/// <summary>
/// DTO for updating a group expense
/// </summary>
public class UpdateGroupExpenseDto
{
    [Required(ErrorMessage = "Group ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Group name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }
}

/// <summary>
/// DTO for group expense response
/// </summary>
public class GroupExpenseResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = null!;
    public bool IsPublic { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int MemberCount { get; set; }
    public decimal TotalExpenses { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}

/// <summary>
/// DTO for group member
/// </summary>
public class GroupMemberDto
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}

/// <summary>
/// DTO for adding member to group
/// </summary>
public class AddGroupMemberDto
{
    [Required(ErrorMessage = "Group ID is required")]
    public long GroupId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public long UserId { get; set; }

    [StringLength(20, ErrorMessage = "Role must be less than 20 characters")]
    public string Role { get; set; } = "Member";
}

/// <summary>
/// DTO for creating group transaction
/// </summary>
public class CreateGroupTransactionDto
{
    [Required(ErrorMessage = "Group ID is required")]
    public long GroupId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string Currency { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string Description { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    [StringLength(100, ErrorMessage = "Category must be less than 100 characters")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Split method is required")]
    [Range(1, 3, ErrorMessage = "Invalid split method (1=Equal, 2=ByAmount, 3=ByPercentage)")]
    public int SplitMethod { get; set; }

    public List<GroupTransactionSplitDto>? CustomSplits { get; set; }
}

/// <summary>
/// DTO for group transaction split
/// </summary>
public class GroupTransactionSplitDto
{
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}

/// <summary>
/// DTO for group transaction response
/// </summary>
public class GroupTransactionResponseDto
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public long PaidByUserId { get; set; }
    public string PaidByUserName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string? Category { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<GroupTransactionSplitDto> Splits { get; set; } = new();
}

/// <summary>
/// DTO for group balance summary
/// </summary>
public class GroupBalanceSummaryDto
{
    public long GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public List<GroupMemberBalanceDto> MemberBalances { get; set; } = new();
    public List<GroupDebtDto> Settlements { get; set; } = new();
}

/// <summary>
/// DTO for member balance in group
/// </summary>
public class GroupMemberBalanceDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public decimal TotalPaid { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// DTO for group debt settlement
/// </summary>
public class GroupDebtDto
{
    public long FromUserId { get; set; }
    public string FromUserName { get; set; } = null!;
    public long ToUserId { get; set; }
    public string ToUserName { get; set; } = null!;
    public decimal Amount { get; set; }
}
