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
public class EditModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public EditModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ParentOptions { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }
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

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadParentOptionsAsync();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && (x.UserId == userId || x.IsDefault));
        if (c == null) return RedirectToPage("Index");
        Input = new InputModel
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type,
            ParentCategoryId = c.ParentCategoryId,
            Icon = c.Icon,
            Color = c.Color,
            Description = c.Description
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadParentOptionsAsync();
            return Page();
        }
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == Input.Id && (x.UserId == userId || x.IsDefault));
        if (c == null) return RedirectToPage("Index");
        c.Name = Input.Name.Trim();
        c.Type = Input.Type;
        c.ParentCategoryId = Input.ParentCategoryId;
        c.Icon = Input.Icon;
        c.Color = Input.Color;
        c.Description = Input.Description;
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


