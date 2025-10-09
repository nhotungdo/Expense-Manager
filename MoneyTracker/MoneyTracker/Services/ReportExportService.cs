using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;
using System.Text;

namespace MoneyTracker.Services
{
    public class ReportExportService : IReportExportService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ReportExportService> _logger;

        public ReportExportService(ExpenseManagerContext context, ILogger<ReportExportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> ExportToPdfAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly")
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException("User not found");

                var reportData = await GetReportDataAsync(userId, startDate, endDate);

                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                var title = new Paragraph($"Báo cáo tài chính - {user.FullName ?? user.UserName}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Add date range
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.GRAY);
                var dateRange = new Paragraph($"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}", dateFont);
                dateRange.Alignment = Element.ALIGN_CENTER;
                document.Add(dateRange);

                document.Add(new Paragraph(" "));

                // Add summary table
                var summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 100;
                summaryTable.SetWidths(new float[] { 1, 1 });

                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                // Summary data
                var summaryData = new[]
                {
                    new[] { "Tổng thu nhập", reportData.TotalIncome.ToString("N0") + " VND" },
                    new[] { "Tổng chi tiêu", reportData.TotalExpenses.ToString("N0") + " VND" },
                    new[] { "Số dư", reportData.NetWorth.ToString("N0") + " VND" },
                    new[] { "Tỷ lệ tiết kiệm", reportData.SavingsRate.ToString("F1") + "%" }
                };

                foreach (var row in summaryData)
                {
                    var headerCell = new PdfPCell(new Phrase(row[0], headerFont));
                    headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    headerCell.Padding = 8;
                    summaryTable.AddCell(headerCell);

                    var dataCell = new PdfPCell(new Phrase(row[1], cellFont));
                    dataCell.Padding = 8;
                    summaryTable.AddCell(dataCell);
                }

                document.Add(summaryTable);
                document.Add(new Paragraph(" "));

                // Add expenses by category
                if (reportData.ExpensesByCategory.Any())
                {
                    var expensesTable = new PdfPTable(2);
                    expensesTable.WidthPercentage = 100;
                    expensesTable.SetWidths(new float[] { 2, 1 });

                    var expensesHeader = new PdfPCell(new Phrase("Chi tiêu theo danh mục", headerFont));
                    expensesHeader.Colspan = 2;
                    expensesHeader.BackgroundColor = BaseColor.LIGHT_GRAY;
                    expensesHeader.Padding = 8;
                    expensesTable.AddCell(expensesHeader);

                    foreach (var category in reportData.ExpensesByCategory)
                    {
                        var categoryCell = new PdfPCell(new Phrase(category.Key, cellFont));
                        categoryCell.Padding = 5;
                        expensesTable.AddCell(categoryCell);

                        var amountCell = new PdfPCell(new Phrase(category.Value.ToString("N0") + " VND", cellFont));
                        amountCell.Padding = 5;
                        amountCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        expensesTable.AddCell(amountCell);
                    }

                    document.Add(expensesTable);
                    document.Add(new Paragraph(" "));
                }

                // Add recent transactions
                if (reportData.RecentTransactions.Any())
                {
                    var transactionsTable = new PdfPTable(4);
                    transactionsTable.WidthPercentage = 100;
                    transactionsTable.SetWidths(new float[] { 1, 2, 1, 1 });

                    var transHeaders = new[] { "Loại", "Ghi chú", "Số tiền", "Ngày" };
                    foreach (var header in transHeaders)
                    {
                        var headerCell = new PdfPCell(new Phrase(header, headerFont));
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.Padding = 5;
                        transactionsTable.AddCell(headerCell);
                    }

                    foreach (var transaction in reportData.RecentTransactions.Take(20))
                    {
                        transactionsTable.AddCell(new PdfPCell(new Phrase(transaction.Type, cellFont)) { Padding = 5 });
                        transactionsTable.AddCell(new PdfPCell(new Phrase(transaction.Note ?? "", cellFont)) { Padding = 5 });
                        transactionsTable.AddCell(new PdfPCell(new Phrase(transaction.Amount.ToString("N0") + " VND", cellFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                        transactionsTable.AddCell(new PdfPCell(new Phrase(transaction.Date.ToString("dd/MM/yyyy"), cellFont)) { Padding = 5 });
                    }

                    document.Add(transactionsTable);
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> ExportToExcelAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly")
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException("User not found");

                var reportData = await GetReportDataAsync(userId, startDate, endDate);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Báo cáo tài chính");

                // Add title
                worksheet.Cell("A1").Value = $"Báo cáo tài chính - {user.FullName ?? user.UserName}";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Range("A1:D1").Merge();

                // Add date range
                worksheet.Cell("A2").Value = $"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}";
                worksheet.Cell("A2").Style.Font.FontSize = 12;
                worksheet.Range("A2:D2").Merge();

                // Add summary
                var summaryRow = 4;
                worksheet.Cell($"A{summaryRow}").Value = "Tổng quan";
                worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
                worksheet.Cell($"A{summaryRow}").Style.Fill.BackgroundColor = XLColor.LightGray;

                var summaryData = new[]
                {
                    new[] { "Tổng thu nhập", reportData.TotalIncome.ToString("N0") + " VND" },
                    new[] { "Tổng chi tiêu", reportData.TotalExpenses.ToString("N0") + " VND" },
                    new[] { "Số dư", reportData.NetWorth.ToString("N0") + " VND" },
                    new[] { "Tỷ lệ tiết kiệm", reportData.SavingsRate.ToString("F1") + "%" }
                };

                for (int i = 0; i < summaryData.Length; i++)
                {
                    worksheet.Cell($"A{summaryRow + 1 + i}").Value = summaryData[i][0];
                    worksheet.Cell($"B{summaryRow + 1 + i}").Value = summaryData[i][1];
                }

                // Add expenses by category
                if (reportData.ExpensesByCategory.Any())
                {
                    var categoryRow = summaryRow + 6;
                    worksheet.Cell($"A{categoryRow}").Value = "Chi tiêu theo danh mục";
                    worksheet.Cell($"A{categoryRow}").Style.Font.Bold = true;
                    worksheet.Cell($"A{categoryRow}").Style.Fill.BackgroundColor = XLColor.LightGray;

                    var currentRow = categoryRow + 1;
                    foreach (var category in reportData.ExpensesByCategory)
                    {
                        worksheet.Cell($"A{currentRow}").Value = category.Key;
                        worksheet.Cell($"B{currentRow}").Value = category.Value.ToString("N0") + " VND";
                        currentRow++;
                    }
                }

                // Add recent transactions
                if (reportData.RecentTransactions.Any())
                {
                    var transRow = summaryRow + 6 + reportData.ExpensesByCategory.Count + 2;
                    worksheet.Cell($"A{transRow}").Value = "Giao dịch gần đây";
                    worksheet.Cell($"A{transRow}").Style.Font.Bold = true;
                    worksheet.Cell($"A{transRow}").Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Headers
                    worksheet.Cell($"A{transRow + 1}").Value = "Loại";
                    worksheet.Cell($"B{transRow + 1}").Value = "Ghi chú";
                    worksheet.Cell($"C{transRow + 1}").Value = "Số tiền";
                    worksheet.Cell($"D{transRow + 1}").Value = "Ngày";
                    worksheet.Range($"A{transRow + 1}:D{transRow + 1}").Style.Font.Bold = true;

                    var currentRow = transRow + 2;
                    foreach (var transaction in reportData.RecentTransactions.Take(50))
                    {
                        worksheet.Cell($"A{currentRow}").Value = transaction.Type;
                        worksheet.Cell($"B{currentRow}").Value = transaction.Note ?? "";
                        worksheet.Cell($"C{currentRow}").Value = transaction.Amount.ToString("N0") + " VND";
                        worksheet.Cell($"D{currentRow}").Value = transaction.Date.ToString("dd/MM/yyyy");
                        currentRow++;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> ExportToCsvAsync(long userId, DateTime startDate, DateTime endDate, string reportType = "monthly")
        {
            try
            {
                var reportData = await GetReportDataAsync(userId, startDate, endDate);
                var csv = new StringBuilder();

                // Add header
                csv.AppendLine("Báo cáo tài chính");
                csv.AppendLine($"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
                csv.AppendLine();

                // Add summary
                csv.AppendLine("Tổng quan");
                csv.AppendLine("Tổng thu nhập," + reportData.TotalIncome.ToString("N0") + " VND");
                csv.AppendLine("Tổng chi tiêu," + reportData.TotalExpenses.ToString("N0") + " VND");
                csv.AppendLine("Số dư," + reportData.NetWorth.ToString("N0") + " VND");
                csv.AppendLine("Tỷ lệ tiết kiệm," + reportData.SavingsRate.ToString("F1") + "%");
                csv.AppendLine();

                // Add expenses by category
                if (reportData.ExpensesByCategory.Any())
                {
                    csv.AppendLine("Chi tiêu theo danh mục");
                    csv.AppendLine("Danh mục,Số tiền");
                    foreach (var category in reportData.ExpensesByCategory)
                    {
                        csv.AppendLine($"{category.Key},{category.Value.ToString("N0")} VND");
                    }
                    csv.AppendLine();
                }

                // Add recent transactions
                if (reportData.RecentTransactions.Any())
                {
                    csv.AppendLine("Giao dịch gần đây");
                    csv.AppendLine("Loại,Ghi chú,Số tiền,Ngày");
                    foreach (var transaction in reportData.RecentTransactions.Take(100))
                    {
                        csv.AppendLine($"{transaction.Type},{transaction.Note ?? ""},{transaction.Amount.ToString("N0")} VND,{transaction.Date:dd/MM/yyyy}");
                    }
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSV report for user {UserId}", userId);
                throw;
            }
        }

        private async Task<ReportData> GetReportDataAsync(long userId, DateTime startDate, DateTime endDate)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.Amount);

            var totalExpenses = await _context.Expenses
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.Amount);

            var periodIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate >= startDateOnly &&
                           i.IncomeDate <= endDateOnly)
                .SumAsync(i => i.Amount);

            var periodExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= startDateOnly &&
                           e.ExpenseDate <= endDateOnly)
                .SumAsync(e => e.Amount);

            var expensesByCategory = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= startDateOnly &&
                           e.ExpenseDate <= endDateOnly)
                .Include(e => e.Category)
                .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));

            var recentTransactions = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= startDateOnly &&
                           e.ExpenseDate <= endDateOnly)
                .Include(e => e.Category)
                .OrderByDescending(e => e.CreatedAt)
                .Take(50)
                .Select(e => new RecentTransaction
                {
                    Id = e.Id,
                    Type = "Chi tiêu",
                    Amount = e.Amount,
                    Category = e.Category != null ? e.Category.Name : "Uncategorized",
                    Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    Note = e.Note
                })
                .ToListAsync();

            var recentIncomes = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate >= startDateOnly &&
                           i.IncomeDate <= endDateOnly)
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Take(50)
                .Select(i => new RecentTransaction
                {
                    Id = i.Id,
                    Type = "Thu nhập",
                    Amount = i.Amount,
                    Category = i.Category != null ? i.Category.Name : "Uncategorized",
                    Date = i.IncomeDate.ToDateTime(TimeOnly.MinValue),
                    Note = i.Note
                })
                .ToListAsync();

            var allTransactions = recentTransactions
                .Concat(recentIncomes)
                .OrderByDescending(t => t.Date)
                .ToList();

            var savingsRate = periodIncome > 0 ? ((periodIncome - periodExpenses) / periodIncome) * 100 : 0;

            return new ReportData
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetWorth = totalIncome - totalExpenses,
                PeriodIncome = periodIncome,
                PeriodExpenses = periodExpenses,
                SavingsRate = savingsRate,
                ExpensesByCategory = expensesByCategory,
                RecentTransactions = allTransactions
            };
        }
    }

    public class ReportData
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetWorth { get; set; }
        public decimal PeriodIncome { get; set; }
        public decimal PeriodExpenses { get; set; }
        public decimal SavingsRate { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
    }

    public class RecentTransaction
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
