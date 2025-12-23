using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;
using System.Text;
using ClosedXML.Excel;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportExportController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;

        public ReportExportController(ExpenseManagerContext context)
        {
            _context = context;
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] long? accountId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            // Default to this month
            var now = DateTime.Now;
            startDate ??= new DateTime(now.Year, now.Month, 1);
            endDate ??= now.Date;

            // Query transactions
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }

            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

            // Calculate summaries
            var totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
            var netIncome = totalIncome - totalExpense;

            using var workbook = new XLWorkbook();
            
            // Summary Sheet
            var summarySheet = workbook.Worksheets.Add("Tổng quan");
            summarySheet.Cell("A1").Value = "BÁO CÁO TÀI CHÍNH";
            summarySheet.Cell("A1").Style.Font.FontSize = 16;
            summarySheet.Cell("A1").Style.Font.Bold = true;
            
            summarySheet.Cell("A3").Value = "Khoảng thời gian:";
            summarySheet.Cell("B3").Value = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
            
            summarySheet.Cell("A5").Value = "Tổng thu nhập:";
            summarySheet.Cell("B5").Value = totalIncome;
            summarySheet.Cell("B5").Style.NumberFormat.Format = "#,##0 ₫";
            summarySheet.Cell("B5").Style.Font.FontColor = XLColor.Green;
            
            summarySheet.Cell("A6").Value = "Tổng chi tiêu:";
            summarySheet.Cell("B6").Value = totalExpense;
            summarySheet.Cell("B6").Style.NumberFormat.Format = "#,##0 ₫";
            summarySheet.Cell("B6").Style.Font.FontColor = XLColor.Red;
            
            summarySheet.Cell("A7").Value = "Số dư ròng:";
            summarySheet.Cell("B7").Value = netIncome;
            summarySheet.Cell("B7").Style.NumberFormat.Format = "#,##0 ₫";
            summarySheet.Cell("B7").Style.Font.Bold = true;
            
            // Transactions Sheet
            var transSheet = workbook.Worksheets.Add("Chi tiết giao dịch");
            transSheet.Cell("A1").Value = "Ngày";
            transSheet.Cell("B1").Value = "Loại";
            transSheet.Cell("C1").Value = "Danh mục";
            transSheet.Cell("D1").Value = "Tài khoản";
            transSheet.Cell("E1").Value = "Ghi chú";
            transSheet.Cell("F1").Value = "Số tiền";
            
            var headerRange = transSheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            
            int row = 2;
            foreach (var t in transactions)
            {
                transSheet.Cell(row, 1).Value = t.TransactionDate.ToString("dd/MM/yyyy");
                transSheet.Cell(row, 2).Value = t.TransactionType == 1 ? "Thu" : t.TransactionType == 2 ? "Chi" : "Chuyển";
                transSheet.Cell(row, 3).Value = t.Category?.Name ?? "Khác";
                transSheet.Cell(row, 4).Value = t.Account?.Name ?? "";
                transSheet.Cell(row, 5).Value = t.Note ?? "";
                transSheet.Cell(row, 6).Value = t.Amount;
                transSheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0 ₫";
                
                if (t.TransactionType == 1)
                {
                    transSheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
                }
                else if (t.TransactionType == 2)
                {
                    transSheet.Cell(row, 6).Style.Font.FontColor = XLColor.Red;
                }
                
                row++;
            }
            
            transSheet.Columns().AdjustToContents();
            summarySheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            
            var fileName = $"BaoCaoTaiChinh_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("csv")]
        public async Task<IActionResult> ExportToCsv(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] long? accountId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var now = DateTime.Now;
            startDate ??= new DateTime(now.Year, now.Month, 1);
            endDate ??= now.Date;

            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }

            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Ngày,Loại,Danh mục,Tài khoản,Ghi chú,Số tiền");
            
            foreach (var t in transactions)
            {
                var type = t.TransactionType == 1 ? "Thu" : t.TransactionType == 2 ? "Chi" : "Chuyển";
                csv.AppendLine($"{t.TransactionDate:dd/MM/yyyy},{type},{t.Category?.Name ?? "Khác"},{t.Account?.Name ?? ""},{t.Note ?? ""},{t.Amount}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"BaoCaoTaiChinh_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}
