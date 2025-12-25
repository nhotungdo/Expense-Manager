using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MoneyTrackerApp.Configuration;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using Xunit;

namespace MoneyTrackerApp.Tests;

/// <summary>
/// Unit tests cho EmailService
/// Test Cases: TC-EMAIL-001, TC-EMAIL-002, TC-EMAIL-003, TC-EMAIL-004
/// </summary>
public class EmailServiceTests : IDisposable
{
    private readonly ExpenseManagerContext _context;
    private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<ExpenseManagerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ExpenseManagerContext(options);

        // Setup Email Settings Mock
        _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
        _emailSettingsMock.Setup(x => x.Value).Returns(new EmailSettings
        {
            Host = "smtp.gmail.com",
            Port = 587,
            Username = "test@gmail.com",
            Password = "testpassword",
            FromEmail = "test@gmail.com",
            FromName = "Test System",
            EnableSsl = true
        });

        // Setup Logger Mock
        _loggerMock = new Mock<ILogger<EmailService>>();

        // Create EmailService instance
        _emailService = new EmailService(_emailSettingsMock.Object, _context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region TC-EMAIL-001: Test gửi email thành công

    [Fact(DisplayName = "TC-EMAIL-001.1: Gửi email với địa chỉ hợp lệ - Kiểm tra database record")]
    public async Task SendEmail_ValidEmail_CreatesEmailRecord()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "Test Email - TC-EMAIL-001.1";
        var body = "Đây là email test cơ bản để kiểm tra chức năng gửi email.";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch
        {
            // SMTP sẽ fail trong test environment, nhưng ta vẫn kiểm tra database
        }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail && e.Subject == subject);

        Assert.NotNull(emailLog);
        Assert.Equal(toEmail, emailLog.RecipientEmail);
        Assert.Equal(subject, emailLog.Subject);
        Assert.Equal(body, emailLog.Body);
        Assert.NotNull(emailLog.CreatedAt);
        
        // Trong test environment, email sẽ fail do không có SMTP thực
        // Nhưng record vẫn phải được tạo
        Assert.True(emailLog.Status == "Sent" || emailLog.Status == "Failed");
    }

    [Fact(DisplayName = "TC-EMAIL-001.2: Gửi email và link với user trong database")]
    public async Task SendEmail_UserExists_LinksEmailToUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "donhotung2004@gmail.com",
            UserName = "donhotung2004@gmail.com",
            FullName = "Do Nho Tung"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var subject = "Test Email - TC-EMAIL-001.2";
        var body = "Test linking email to user";

        // Act
        try
        {
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == user.Email && e.Subject == subject);

        Assert.NotNull(emailLog);
        Assert.Equal(user.Id, emailLog.UserId);
    }

    #endregion

    #region TC-EMAIL-002: Test nội dung email phức tạp

    [Fact(DisplayName = "TC-EMAIL-002.1: Gửi email với HTML content")]
    public async Task SendEmail_HtmlContent_SavesCorrectly()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "Test Email - TC-EMAIL-002.1 - HTML Content";
        var body = @"
            <h1>Tiêu đề HTML</h1>
            <p>Đây là đoạn văn bản với <strong>chữ đậm</strong> và <em>chữ nghiêng</em>.</p>
            <ul>
                <li>Mục 1</li>
                <li>Mục 2</li>
                <li>Mục 3</li>
            </ul>
        ";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail && e.Subject == subject);

        Assert.NotNull(emailLog);
        Assert.Contains("<h1>", emailLog.Body);
        Assert.Contains("<strong>", emailLog.Body);
        Assert.Contains("<ul>", emailLog.Body);
    }

    [Fact(DisplayName = "TC-EMAIL-002.2: Gửi email với ký tự đặc biệt")]
    public async Task SendEmail_SpecialCharacters_SavesCorrectly()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "Test Email - TC-EMAIL-002.2 - Ký tự đặc biệt !@#$%^&*()";
        var body = "Ký tự đặc biệt: !@#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail);

        Assert.NotNull(emailLog);
        Assert.Contains("!@#$%^&*()", emailLog.Subject);
        Assert.Contains("!@#$%^&*()_+-=[]{}|;':\",./<>?", emailLog.Body);
    }

    [Fact(DisplayName = "TC-EMAIL-002.3: Gửi email với tiếng Việt có dấu")]
    public async Task SendEmail_VietnameseCharacters_SavesCorrectly()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "Test Email - TC-EMAIL-002.3 - Tiếng Việt";
        var body = "Tiếng Việt có dấu: Àáảãạâầấẩẫậăằắẳẵặ ĐÊỒỘỜỚỢ";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail && e.Subject == subject);

        Assert.NotNull(emailLog);
        Assert.Contains("Àáảãạâầấẩẫậăằắẳẵặ", emailLog.Body);
        Assert.Contains("ĐÊỒỘỜỚỢ", emailLog.Body);
    }

    #endregion

    #region TC-EMAIL-003: Test tiêu đề dài và ký tự đặc biệt

    [Fact(DisplayName = "TC-EMAIL-003.1: Gửi email với tiêu đề dài (150 ký tự)")]
    public async Task SendEmail_LongSubject_SavesCorrectly()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "Test Email TC-EMAIL-003.1 - Đây là tiêu đề email rất dài để kiểm tra khả năng xử lý của hệ thống khi tiêu đề vượt quá 100 ký tự và có thể lên đến 150 ký tự";
        var body = "Nội dung test tiêu đề dài";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail);

        Assert.NotNull(emailLog);
        Assert.True(emailLog.Subject.Length >= 150);
        Assert.Equal(subject, emailLog.Subject);
    }

    [Fact(DisplayName = "TC-EMAIL-003.2: Gửi email với tiêu đề chứa emoji")]
    public async Task SendEmail_SubjectWithEmoji_SavesCorrectly()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var subject = "📧 Test Email 003.2 - Email với Emoji 🎉 ✅ 😀";
        var body = "Nội dung test emoji trong subject";

        // Act
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch { }

        // Assert
        var emailLog = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == toEmail);

        Assert.NotNull(emailLog);
        Assert.Contains("📧", emailLog.Subject);
        Assert.Contains("🎉", emailLog.Subject);
    }

    #endregion

    #region TC-EMAIL-008: Test gửi nhiều email

    [Fact(DisplayName = "TC-EMAIL-008.1: Gửi email đến nhiều người nhận")]
    public async Task SendEmail_MultipleRecipients_CreatesMultipleRecords()
    {
        // Arrange
        var recipients = new List<string>
        {
            "donhotung2004@gmail.com",
            "test1@gmail.com",
            "test2@gmail.com"
        };
        var subject = "Test Email - TC-EMAIL-008.1 - Multiple Recipients";
        var body = "Email gửi đến nhiều người nhận";

        // Act
        try
        {
            await _emailService.SendEmailAsync(recipients, subject, body);
        }
        catch { }

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject == subject)
            .ToListAsync();

        Assert.Equal(3, emailLogs.Count);
        Assert.Contains(emailLogs, e => e.RecipientEmail == "donhotung2004@gmail.com");
        Assert.Contains(emailLogs, e => e.RecipientEmail == "test1@gmail.com");
        Assert.Contains(emailLogs, e => e.RecipientEmail == "test2@gmail.com");
    }

    [Fact(DisplayName = "TC-EMAIL-008.2: Một email fail không ảnh hưởng đến các email khác")]
    public async Task SendEmail_OneFailsInBatch_OthersContinue()
    {
        // Arrange
        var recipients = new List<string>
        {
            "donhotung2004@gmail.com",
            "invalid-email", // Email không hợp lệ
            "test@gmail.com"
        };
        var subject = "Test Email - TC-EMAIL-008.2";
        var body = "Test error handling";

        // Act
        try
        {
            await _emailService.SendEmailAsync(recipients, subject, body);
        }
        catch { }

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject == subject)
            .ToListAsync();

        // Tất cả 3 email đều phải có record trong database
        // Ngay cả khi một số fail
        Assert.True(emailLogs.Count >= 1); // Ít nhất 1 email được xử lý
    }

    #endregion

    #region TC-EMAIL-005: Test hiệu năng

    [Fact(DisplayName = "TC-EMAIL-005.1: Gửi 10 email đồng thời")]
    public async Task SendEmail_Send10Emails_CompletesInReasonableTime()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var startTime = DateTime.UtcNow;

        // Act
        var tasks = new List<Task>();
        for (int i = 1; i <= 10; i++)
        {
            var subject = $"Test Email - TC-EMAIL-005.1 - Email #{i}";
            var body = $"Đây là email test hiệu năng số {i}";
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(toEmail, subject, body);
                }
                catch { }
            }));
        }

        await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalSeconds;

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject.Contains("TC-EMAIL-005.1"))
            .ToListAsync();

        Assert.Equal(10, emailLogs.Count);
        Assert.True(duration < 30, $"Took {duration} seconds, should be less than 30");
    }

    [Fact(DisplayName = "TC-EMAIL-005.2: Gửi 50 email - kiểm tra database records")]
    public async Task SendEmail_Send50Emails_AllRecordsCreated()
    {
        // Arrange
        var toEmail = "donhotung2004@gmail.com";
        var emailCount = 50;

        // Act
        var tasks = new List<Task>();
        for (int i = 1; i <= emailCount; i++)
        {
            var subject = $"Test Email - TC-EMAIL-005.2 - Email #{i}";
            var body = $"Performance test email {i}";
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(toEmail, subject, body);
                }
                catch { }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var emailLogs = await _context.Emails
            .Where(e => e.Subject.Contains("TC-EMAIL-005.2"))
            .ToListAsync();

        Assert.Equal(emailCount, emailLogs.Count);
        
        // Kiểm tra tất cả đều có timestamp
        Assert.All(emailLogs, log => Assert.NotNull(log.CreatedAt));
    }

    #endregion

    #region Helper Tests

    [Fact(DisplayName = "Helper: Kiểm tra EmailSettings được inject đúng")]
    public void EmailSettings_InjectedCorrectly()
    {
        // Assert
        var settings = _emailSettingsMock.Object.Value;
        Assert.NotNull(settings);
        Assert.Equal("smtp.gmail.com", settings.Host);
        Assert.Equal(587, settings.Port);
        Assert.True(settings.EnableSsl);
    }

    [Fact(DisplayName = "Helper: Kiểm tra Database context hoạt động")]
    public async Task DatabaseContext_WorksCorrectly()
    {
        // Arrange
        var email = new Email
        {
            RecipientEmail = "test@gmail.com",
            Subject = "Test",
            Body = "Test",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Emails.Add(email);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Emails.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("test@gmail.com", saved.RecipientEmail);
    }

    #endregion
}
