using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class ExportExcelModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExportExcelModel(ExpenseManagerContext db) { _db = db; }

    public async Task<IActionResult> OnGetAsync(DateTime? from, DateTime? to, string? type)
    {
        // For simplicity, return CSV with .xlsx filename so user can open in Excel
        var csv = await new ExportCsvModel(_db).OnGetAsync(from, to, type) as FileContentResult;
        if (csv == null) return BadRequest();
        return File(csv.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
    }
}


