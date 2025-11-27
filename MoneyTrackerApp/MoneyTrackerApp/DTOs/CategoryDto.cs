using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a category
/// </summary>
public class CreateCategoryDto
{
    public long? ParentCategoryId { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Category type is required")]
    [Range(1, 2, ErrorMessage = "Invalid category type (1=Income, 2=Expense)")]
    public int Type { get; set; }

    [StringLength(512, ErrorMessage = "Description must be less than 512 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }
}

/// <summary>
/// DTO for updating a category
/// </summary>
public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [StringLength(512, ErrorMessage = "Description must be less than 512 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for category response
/// </summary>
public class CategoryResponseDto
{
    public long Id { get; set; }
    public long? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string Name { get; set; } = null!;
    public int Type { get; set; }
    public string TypeDisplay { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public long? UserId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CategoryResponseDto>? SubCategories { get; set; }
}

/// <summary>
/// DTO for category summary
/// </summary>
public class CategorySummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int Type { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsDefault { get; set; }
}
