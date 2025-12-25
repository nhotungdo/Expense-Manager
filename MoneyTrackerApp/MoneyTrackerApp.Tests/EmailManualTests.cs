using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoneyTrackerApp.Configuration;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using Xunit;
using Xunit.Abstractions;

namespace MoneyTrackerApp.Tests;

/// <summary>
/// Manual/Integration tests - Gửi email thực tế đến donhotung2004@gmail.com
/// CẢNH BÁO: Các tests này sẽ GỬI EMAIL THỰC SỰ!
/// Chỉ chạy khi đã cấu hình SMTP settings đúng trong appsettings.json
/// 
/// Để chạy: dotnet test --filter "Category=ManualTest"
/// </summary>
[Collection("ManualTests")]
[Trait("Category", "ManualTest")]
public class EmailManualTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ExpenseManagerContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<EmailService> _logger;

    // QUAN TRỌNG: Cập nhật các thông tin này từ appsettings.json của bạn
    private const string TEST_RECIPIENT = "donhotung2004@gmail.com";
    
    public EmailManualTests(ITestOutputHelper output)
    {
        _output = output;

        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<ExpenseManagerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ExpenseManagerContext(options);

        // Setup Email Settings - CẬP NHẬT THÔNG TIN NÀY!
        var emailSettings = Options.Create(new EmailSettings
        {
            Host = "smtp.gmail.com",
            Port = 587,
            Username = "YOUR_EMAIL@gmail.com", // Thay bằng email của bạn
            Password = "YOUR_APP_PASSWORD", // Thay bằng App Password của bạn
            FromEmail = "YOUR_EMAIL@gmail.com",
            FromName = "Money Tracker App - Test System",
            EnableSsl = true
        });

        // Setup Logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        _logger = loggerFactory.CreateLogger<EmailService>();

        _emailService = new EmailService(emailSettings, _context, _logger);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region TC-EMAIL-001: Gửi email thành công

    [Fact(DisplayName = "TC-EMAIL-001: Gửi email cơ bản đến donhotung2004@gmail.com", Skip = "Manual test - Uncomment để chạy")]
    // Bỏ Skip và cập nhật SMTP settings để chạy test này
    public async Task TC_EMAIL_001_SendBasicEmail()
    {
        // Arrange
        var subject = $"[TEST] TC-EMAIL-001 - Email Test Cơ Bản - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var body = @"
            <h2>Test Case: TC-EMAIL-001</h2>
            <p>Đây là email test cơ bản để kiểm tra chức năng gửi email.</p>
            <p><strong>Thời gian gửi:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</p>
            <p><strong>Mục đích:</strong> Kiểm tra email được gửi thành công và hiển thị đúng trong hộp thư.</p>
            <hr/>
            <p><em>Email này được gửi tự động từ hệ thống test.</em></p>
        ";

        _output.WriteLine($"Sending email to: {TEST_RECIPIENT}");
        _output.WriteLine($"Subject: {subject}");

        // Act
        var startTime = DateTime.UtcNow;
        await _emailService.SendEmailAsync(TEST_RECIPIENT, subject, body);
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalSeconds;

        _output.WriteLine($"Email sent in {duration} seconds");

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == TEST_RECIPIENT && e.Subject == subject);

        Assert.NotNull(emailLog);
        Assert.Equal("Sent", emailLog.Status);
        Assert.NotNull(emailLog.SentAt);
        
        _output.WriteLine("✅ Test PASSED - Kiểm tra hộp thư của donhotung2004@gmail.com");
    }

    #endregion

    #region TC-EMAIL-002: Nội dung phức tạp

    [Fact(DisplayName = "TC-EMAIL-002: Gửi email với nội dung HTML phức tạp", Skip = "Manual test - Uncomment để chạy")]
    public async Task TC_EMAIL_002_SendHtmlEmail()
    {
        // Arrange
        var subject = $"[TEST] TC-EMAIL-002 - Nội dung HTML Phức Tạp !@#$%^&*() - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var body = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; }
                    .header { background-color: #4CAF50; color: white; padding: 20px; }
                    .content { padding: 20px; }
                    .special-chars { background-color: #f0f0f0; padding: 10px; margin: 10px 0; }
                    .vietnamese { color: #2196F3; }
                    .emoji { font-size: 24px; }
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Test Case: TC-EMAIL-002</h1>
                </div>
                <div class='content'>
                    <h2>Kiểm tra nội dung HTML phức tạp</h2>
                    
                    <h3>1. HTML Formatting</h3>
                    <p>Đây là đoạn văn bản với <strong>chữ đậm</strong>, <em>chữ nghiêng</em>, và <u>gạch chân</u>.</p>
                    
                    <h3>2. Lists</h3>
                    <ul>
                        <li>Mục 1</li>
                        <li>Mục 2</li>
                        <li>Mục 3</li>
                    </ul>
                    
                    <ol>
                        <li>Bước 1</li>
                        <li>Bước 2</li>
                        <li>Bước 3</li>
                    </ol>
                    
                    <h3>3. Ký tự đặc biệt</h3>
                    <div class='special-chars'>
                        <p>Ký tự đặc biệt: !@#$%^&*()_+-=[]{}|;':"",./<>?</p>
                        <p>Dấu ngoặc: () [] {} &lt;&gt;</p>
                        <p>Toán học: + - × ÷ = ≠ ≤ ≥</p>
                    </div>
                    
                    <h3>4. Tiếng Việt có dấu</h3>
                    <div class='vietnamese'>
                        <p>Nguyên âm: Àáảãạâầấẩẫậăằắẳẵặ</p>
                        <p>Phụ âm: ĐÊỒỘỜỚỢ</p>
                        <p>Câu hoàn chỉnh: Việt Nam là một đất nước xinh đẹp với văn hóa phong phú.</p>
                    </div>
                    
                    <h3>5. Emoji</h3>
                    <div class='emoji'>
                        <p>😀 🎉 ✅ ❌ 📧 💼 🚀 ⭐ 💯 🔥</p>
                    </div>
                    
                    <h3>6. Table</h3>
                    <table border='1' cellpadding='10'>
                        <tr>
                            <th>Cột 1</th>
                            <th>Cột 2</th>
                            <th>Cột 3</th>
                        </tr>
                        <tr>
                            <td>Dữ liệu 1</td>
                            <td>Dữ liệu 2</td>
                            <td>Dữ liệu 3</td>
                        </tr>
                    </table>
                    
                    <hr/>
                    <p><em>Thời gian gửi: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</em></p>
                </div>
            </body>
            </html>
        ";

        _output.WriteLine($"Sending HTML email to: {TEST_RECIPIENT}");

        // Act
        await _emailService.SendEmailAsync(TEST_RECIPIENT, subject, body);

        // Assert
        var emailLog = await _context.Emails.FirstOrDefaultAsync(e => e.Subject == subject);
        Assert.NotNull(emailLog);
        Assert.Equal("Sent", emailLog.Status);
        
        _output.WriteLine("✅ Test PASSED - Kiểm tra HTML rendering trong email");
    }

    #endregion

    #region TC-EMAIL-003: Tiêu đề dài

    [Fact(DisplayName = "TC-EMAIL-003: Gửi email với tiêu đề dài và emoji", Skip = "Manual test - Uncomment để chạy")]
    public async Task TC_EMAIL_003_SendLongSubjectEmail()
    {
        // Arrange
        var subject = "📧 [TEST] TC-EMAIL-003 - Đây là tiêu đề email rất dài để kiểm tra khả năng xử lý của hệ thống khi tiêu đề vượt quá 100 ký tự và có thể lên đến 150 ký tự với emoji 🎉 ✅ 😀";
        var body = @"
            <h2>Test Case: TC-EMAIL-003</h2>
            <p>Email này kiểm tra:</p>
            <ul>
                <li>Tiêu đề dài (trên 100 ký tự)</li>
                <li>Emoji trong tiêu đề</li>
                <li>Ký tự đặc biệt trong tiêu đề</li>
            </ul>
            <p><strong>Độ dài tiêu đề:</strong> " + subject.Length + @" ký tự</p>
        ";

        _output.WriteLine($"Subject length: {subject.Length} characters");

        // Act
        await _emailService.SendEmailAsync(TEST_RECIPIENT, subject, body);

        // Assert
        var emailLog = await _context.Emails.FirstOrDefaultAsync(e => e.RecipientEmail == TEST_RECIPIENT);
        Assert.NotNull(emailLog);
        Assert.True(emailLog.Subject.Length >= 100);
        
        _output.WriteLine("✅ Test PASSED - Kiểm tra tiêu đề trong email client");
    }

    #endregion

    #region TC-EMAIL-005: Hiệu năng

    [Fact(DisplayName = "TC-EMAIL-005: Gửi 10 email liên tiếp", Skip = "Manual test - Uncomment để chạy")]
    public async Task TC_EMAIL_005_Send10Emails()
    {
        // Arrange
        var emailCount = 10;
        var startTime = DateTime.UtcNow;

        _output.WriteLine($"Sending {emailCount} emails to {TEST_RECIPIENT}...");

        // Act
        for (int i = 1; i <= emailCount; i++)
        {
            var subject = $"[TEST] TC-EMAIL-005 - Email #{i}/{emailCount} - {DateTime.Now:HH:mm:ss}";
            var body = $@"
                <h2>Performance Test Email #{i}</h2>
                <p>Đây là email test hiệu năng số {i} trong tổng số {emailCount} emails.</p>
                <p><strong>Thời gian gửi:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            ";

            await _emailService.SendEmailAsync(TEST_RECIPIENT, subject, body);
            _output.WriteLine($"  ✓ Sent email {i}/{emailCount}");
        }

        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalSeconds;

        _output.WriteLine($"Total time: {duration} seconds");
        _output.WriteLine($"Average: {duration / emailCount:F2} seconds per email");

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject.Contains("TC-EMAIL-005"))
            .ToListAsync();

        Assert.Equal(emailCount, emailLogs.Count);
        Assert.True(duration < 60, $"Should complete in less than 60 seconds, took {duration}");
        
        _output.WriteLine($"✅ Test PASSED - Sent {emailCount} emails in {duration:F2} seconds");
    }

    #endregion

    #region TC-EMAIL-008: Nhiều người nhận

    [Fact(DisplayName = "TC-EMAIL-008: Gửi email đến nhiều người nhận", Skip = "Manual test - Uncomment để chạy")]
    public async Task TC_EMAIL_008_SendToMultipleRecipients()
    {
        // Arrange
        var recipients = new List<string>
        {
            TEST_RECIPIENT,
            TEST_RECIPIENT, // Gửi 2 lần đến cùng email để test
            TEST_RECIPIENT
        };
        
        var subject = $"[TEST] TC-EMAIL-008 - Multiple Recipients - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var body = @"
            <h2>Test Case: TC-EMAIL-008</h2>
            <p>Email này được gửi đến nhiều người nhận cùng lúc.</p>
            <p>Kiểm tra xem tất cả người nhận đều nhận được email.</p>
        ";

        _output.WriteLine($"Sending to {recipients.Count} recipients");

        // Act
        await _emailService.SendEmailAsync(recipients, subject, body);

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject == subject)
            .ToListAsync();

        Assert.Equal(recipients.Count, emailLogs.Count);
        
        _output.WriteLine($"✅ Test PASSED - Sent to {recipients.Count} recipients");
        _output.WriteLine($"   Check inbox for {TEST_RECIPIENT} (should have {recipients.Count} emails)");
    }

    #endregion

    #region Test Report Generator

    [Fact(DisplayName = "GENERATE: Tạo email báo cáo test", Skip = "Manual test - Uncomment để chạy")]
    public async Task GenerateTestReport()
    {
        // Tạo một email báo cáo tổng hợp kết quả test
        var subject = $"📊 [TEST REPORT] Email Testing Summary - {DateTime.Now:yyyy-MM-dd}";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .test-case {{ background-color: #f9f9f9; border-left: 4px solid #4CAF50; padding: 15px; margin: 15px 0; }}
                    .test-case h3 {{ margin-top: 0; color: #333; }}
                    .status {{ display: inline-block; padding: 5px 10px; border-radius: 3px; font-weight: bold; }}
                    .status.pending {{ background-color: #FFC107; color: #000; }}
                    .status.pass {{ background-color: #4CAF50; color: white; }}
                    .status.fail {{ background-color: #f44336; color: white; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                    th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                    th {{ background-color: #667eea; color: white; }}
                    .footer {{ background-color: #f0f0f0; padding: 20px; text-align: center; margin-top: 30px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>📧 Email Testing Report</h1>
                    <p>Money Tracker App - Email Notification System</p>
                    <p>Test Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                </div>
                
                <div class='content'>
                    <h2>Test Summary</h2>
                    <table>
                        <tr>
                            <th>Metric</th>
                            <th>Value</th>
                        </tr>
                        <tr>
                            <td>Total Test Cases</td>
                            <td>8 main test cases</td>
                        </tr>
                        <tr>
                            <td>Test Recipient</td>
                            <td>{TEST_RECIPIENT}</td>
                        </tr>
                        <tr>
                            <td>Test Environment</td>
                            <td>Development</td>
                        </tr>
                        <tr>
                            <td>SMTP Server</td>
                            <td>smtp.gmail.com:587</td>
                        </tr>
                    </table>

                    <h2>Test Cases</h2>
                    
                    <div class='test-case'>
                        <h3>TC-EMAIL-001: Gửi email thành công</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra chức năng gửi email cơ bản</p>
                        <p><strong>Expected:</strong> Email được gửi thành công, hiển thị đúng trong hộp thư</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-002: Nội dung email phức tạp</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra HTML, ký tự đặc biệt, tiếng Việt, emoji</p>
                        <p><strong>Expected:</strong> Tất cả nội dung hiển thị chính xác</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-003: Tiêu đề dài và ký tự đặc biệt</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra tiêu đề > 100 ký tự với emoji</p>
                        <p><strong>Expected:</strong> Tiêu đề hiển thị đầy đủ</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-004: Validation email không hợp lệ</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra xử lý lỗi với email invalid</p>
                        <p><strong>Expected:</strong> Hệ thống báo lỗi, không gửi email</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-005: Hiệu năng - Gửi nhiều email</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Gửi 10-100 email liên tiếp</p>
                        <p><strong>Expected:</strong> Hoàn thành trong thời gian hợp lý</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-006: Xử lý lỗi SMTP</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra khi SMTP settings sai</p>
                        <p><strong>Expected:</strong> Error handling đúng, không crash</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-007: Lên lịch gửi email</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Kiểm tra scheduled email</p>
                        <p><strong>Expected:</strong> Email được gửi đúng thời gian</p>
                    </div>

                    <div class='test-case'>
                        <h3>TC-EMAIL-008: Nhiều người nhận</h3>
                        <p><span class='status pending'>PENDING</span></p>
                        <p><strong>Mô tả:</strong> Gửi email đến nhiều địa chỉ</p>
                        <p><strong>Expected:</strong> Tất cả người nhận đều nhận được</p>
                    </div>

                    <h2>Next Steps</h2>
                    <ol>
                        <li>Chạy từng test case manual</li>
                        <li>Ghi lại kết quả trong EmailSendingTestCases.md</li>
                        <li>Chụp screenshot minh chứng</li>
                        <li>Tổng hợp báo cáo cuối cùng</li>
                    </ol>
                </div>

                <div class='footer'>
                    <p><strong>Money Tracker App - QA Team</strong></p>
                    <p>Generated automatically by test system</p>
                    <p>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                </div>
            </body>
            </html>
        ";

        // Act
        await _emailService.SendEmailAsync(TEST_RECIPIENT, subject, body);

        // Assert
        _output.WriteLine("✅ Test report email sent!");
        _output.WriteLine($"   Check {TEST_RECIPIENT} for the report");
    }

    #endregion
}
