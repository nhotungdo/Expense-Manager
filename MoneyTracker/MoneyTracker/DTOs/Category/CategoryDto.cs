using System.ComponentModel.DataAnnotations;
using MoneyTracker.Models;

namespace MoneyTracker.DTOs.Category;

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public long? UserId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public CategoryType Type { get; set; }

    [StringLength(512, ErrorMessage = "Description cannot exceed 512 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "Color cannot exceed 20 characters")]
    public string? Color { get; set; }
}

public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public CategoryType Type { get; set; }

    [StringLength(512, ErrorMessage = "Description cannot exceed 512 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "Color cannot exceed 20 characters")]
    public string? Color { get; set; }
}
