using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class UsersModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public UsersModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public IList<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            Users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(long id, string action)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return RedirectToPage();

            switch (action)
            {
                case "toggle":
                    user.Enabled = !user.Enabled;
                    break;
                case "makeAdmin":
                    user.Role = "ADMIN";
                    break;
                case "makeUser":
                    user.Role = "USER";
                    break;
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}


