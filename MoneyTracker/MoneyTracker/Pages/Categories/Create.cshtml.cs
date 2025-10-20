using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Categories;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public CreateModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ParentOptions { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Range(0, 1)]
        public int Type { get; set; } = 0;
        public long? ParentCategoryId { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string? Description { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadParentOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadParentOptionsAsync();
            return Page();
        }
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");

        var cat = new Category
        {
            Name = Input.Name.Trim(),
            Type = Input.Type,
            ParentCategoryId = Input.ParentCategoryId,
            Icon = Input.Icon,
            Color = Input.Color,
            Description = Input.Description,
            UserId = userId,
            IsActive = true,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task LoadParentOptionsAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        ParentOptions = await _db.Categories
            .Where(c => (c.UserId == userId || c.IsDefault) && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }
}


