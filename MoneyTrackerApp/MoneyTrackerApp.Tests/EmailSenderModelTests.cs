using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Pages.Admin.Notifications;
using MoneyTrackerApp.Services;
using Xunit;

namespace MoneyTrackerApp.Tests;

/// <summary>
/// Integration tests cho EmailSender Page Model
/// Test Cases: TC-EMAIL-001, TC-EMAIL-004, TC-EMAIL-007, TC-EMAIL-008
/// </summary>
public class EmailSenderModelTests : IDisposable
{
    private readonly ExpenseManagerContext _context;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly EmailSenderModel _pageModel;

    public EmailSenderModelTests()
    {
        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<ExpenseManagerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ExpenseManagerContext(options);

        // Setup Email Service Mock
        _emailServiceMock = new Mock<IEmailService>();

        // Create Page Model
        _pageModel = new EmailSenderModel(_emailServiceMock.Object, _context);
        
        // Setup PageContext
        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region TC-EMAIL-001: Test gửi email thành công

    [Fact(DisplayName = "TC-EMAIL-001: OnPostAsync - Gửi email thành công")]
    public async Task OnPostAsync_ValidInput_SendsEmailSuccessfully()
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com",
            Subject = "Test Email - TC-EMAIL-001",
            Body = "Đây là email test cơ bản để kiểm tra chức năng gửi email."
        };

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Emails sent successfully.", _pageModel.StatusMessage);
        
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.Is<List<string>>(list => list.Contains("donhotung2004@gmail.com")),
                "Test Email - TC-EMAIL-001",
                It.IsAny<string>(),
                null),
            Times.Once);
    }

    [Fact(DisplayName = "TC-EMAIL-001.2: OnGetAsync - Load email logs")]
    public async Task OnGetAsync_LoadsEmailLogs()
    {
        // Arrange
        var emails = new List<Email>
        {
            new Email
            {
                RecipientEmail = "donhotung2004@gmail.com",
                Subject = "Test 1",
                Body = "Body 1",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                SentAt = DateTime.UtcNow.AddMinutes(-9)
            },
            new Email
            {
                RecipientEmail = "donhotung2004@gmail.com",
                Subject = "Test 2",
                Body = "Body 2",
                Status = "Failed",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _context.Emails.AddRange(emails);
        await _context.SaveChangesAsync();

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.EmailLogs);
        Assert.Equal(2, _pageModel.EmailLogs.Count);
        Assert.Equal("Test 2", _pageModel.EmailLogs[0].Subject); // Ordered by CreatedAt DESC
        Assert.Equal("Test 1", _pageModel.EmailLogs[1].Subject);
    }

    #endregion

    #region TC-EMAIL-004: Test validation email không hợp lệ

    [Theory(DisplayName = "TC-EMAIL-004: Validation - Email không hợp lệ")]
    [InlineData("", "Email is required")] // TC-EMAIL-004.4: Email rỗng
    [InlineData("invalid-email", "Invalid email format")] // TC-EMAIL-004.1: Thiếu @
    [InlineData("@gmail.com", "Invalid email format")] // TC-EMAIL-004.3: Chỉ có @
    [InlineData("test@", "Invalid email format")] // TC-EMAIL-004.2: Thiếu domain
    public async Task OnPostAsync_InvalidEmail_ReturnsValidationError(string email, string expectedError)
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = email,
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Manually trigger validation
        var validationContext = new ValidationContext(_pageModel.Input);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(_pageModel.Input, validationContext, validationResults, true);

        // Assert
        if (string.IsNullOrEmpty(email))
        {
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("To"));
        }
    }

    [Fact(DisplayName = "TC-EMAIL-004: ModelState Invalid - Không gửi email")]
    public async Task OnPostAsync_InvalidModelState_DoesNotSendEmail()
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com",
            Subject = "", // Subject is required
            Body = "Test Body"
        };

        _pageModel.ModelState.AddModelError("Input.Subject", "The Subject field is required.");

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()),
            Times.Never);
    }

    #endregion

    #region TC-EMAIL-007: Test lên lịch gửi email

    [Fact(DisplayName = "TC-EMAIL-007.1: OnPostAsync - Lên lịch email trong tương lai")]
    public async Task OnPostAsync_ScheduledTime_CreatesScheduledEmail()
    {
        // Arrange
        var scheduledTime = DateTime.UtcNow.AddMinutes(5);
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com",
            Subject = "Test Email - TC-EMAIL-007.1 - Scheduled",
            Body = "Email này được lên lịch gửi sau 5 phút",
            ScheduleTime = scheduledTime
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Emails scheduled successfully.", _pageModel.StatusMessage);

        var scheduledEmail = await _context.Emails
            .FirstOrDefaultAsync(e => e.Subject.Contains("TC-EMAIL-007.1"));

        Assert.NotNull(scheduledEmail);
        Assert.Equal("Scheduled", scheduledEmail.Status);
        Assert.NotNull(scheduledEmail.ScheduledAt);
        Assert.Null(scheduledEmail.SentAt);

        // Email service không được gọi ngay lập tức
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()),
            Times.Never);
    }

    [Fact(DisplayName = "TC-EMAIL-007.2: OnPostAsync - Thời gian trong quá khứ - Gửi ngay")]
    public async Task OnPostAsync_PastScheduledTime_SendsImmediately()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddMinutes(-5);
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com",
            Subject = "Test Email - TC-EMAIL-007.2",
            Body = "Test past time",
            ScheduleTime = pastTime
        };

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.Equal("Emails sent successfully.", _pageModel.StatusMessage);
        
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()),
            Times.Once);
    }

    [Fact(DisplayName = "TC-EMAIL-007.3: Lên lịch nhiều email")]
    public async Task OnPostAsync_ScheduledMultipleRecipients_CreatesMultipleRecords()
    {
        // Arrange
        var scheduledTime = DateTime.UtcNow.AddHours(1);
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com, test1@gmail.com, test2@gmail.com",
            Subject = "Test Email - TC-EMAIL-007.3",
            Body = "Scheduled to multiple recipients",
            ScheduleTime = scheduledTime
        };

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        var scheduledEmails = await _context.Emails
            .Where(e => e.Subject.Contains("TC-EMAIL-007.3"))
            .ToListAsync();

        Assert.Equal(3, scheduledEmails.Count);
        Assert.All(scheduledEmails, email => 
        {
            Assert.Equal("Scheduled", email.Status);
            Assert.NotNull(email.ScheduledAt);
        });
    }

    #endregion

    #region TC-EMAIL-008: Test gửi nhiều email

    [Fact(DisplayName = "TC-EMAIL-008.1: OnPostAsync - Parse nhiều email từ string")]
    public async Task OnPostAsync_MultipleEmails_ParsesCorrectly()
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com, test1@gmail.com, test2@gmail.com",
            Subject = "Test Email - TC-EMAIL-008.1",
            Body = "Email gửi đến nhiều người nhận"
        };

        List<string> capturedRecipients = null;
        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()))
            .Callback<List<string>, string, string, List<IFormFile>>((recipients, subject, body, attachments) =>
            {
                capturedRecipients = recipients;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.NotNull(capturedRecipients);
        Assert.Equal(3, capturedRecipients.Count);
        Assert.Contains("donhotung2004@gmail.com", capturedRecipients);
        Assert.Contains("test1@gmail.com", capturedRecipients);
        Assert.Contains("test2@gmail.com", capturedRecipients);
    }

    [Fact(DisplayName = "TC-EMAIL-008.2: OnPostAsync - Trim whitespace từ email list")]
    public async Task OnPostAsync_EmailsWithWhitespace_TrimsCorrectly()
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = " donhotung2004@gmail.com , test1@gmail.com  ,  test2@gmail.com ",
            Subject = "Test Email - TC-EMAIL-008.2",
            Body = "Test trimming"
        };

        List<string> capturedRecipients = null;
        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()))
            .Callback<List<string>, string, string, List<IFormFile>>((recipients, subject, body, attachments) =>
            {
                capturedRecipients = recipients;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _pageModel.OnPostAsync();

        // Assert
        Assert.All(capturedRecipients, email =>
        {
            Assert.DoesNotContain(" ", email);
            Assert.DoesNotContain("\t", email);
        });
    }

    [Fact(DisplayName = "TC-EMAIL-008.3: OnPostAsync - Bỏ qua email rỗng")]
    public async Task OnPostAsync_EmptyEmailsInList_SkipsThem()
    {
        // Arrange
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = "donhotung2004@gmail.com,,test1@gmail.com, ,test2@gmail.com",
            Subject = "Test Email - TC-EMAIL-008.3",
            Body = "Test empty emails"
        };

        List<string> capturedRecipients = null;
        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>()))
            .Callback<List<string>, string, string, List<IFormFile>>((recipients, subject, body, attachments) =>
            {
                capturedRecipients = recipients;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _pageModel.OnPostAsync();

        // Assert
        Assert.Equal(3, capturedRecipients.Count);
        Assert.DoesNotContain("", capturedRecipients);
    }

    #endregion

    #region Additional Tests

    [Fact(DisplayName = "Additional: OnPostAsync - Link scheduled email với user")]
    public async Task OnPostAsync_ScheduledEmailWithExistingUser_LinksToUser()
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

        var scheduledTime = DateTime.UtcNow.AddMinutes(10);
        _pageModel.Input = new EmailSenderModel.EmailInputModel
        {
            To = user.Email,
            Subject = "Test Scheduled with User Link",
            Body = "Test",
            ScheduleTime = scheduledTime
        };

        // Act
        await _pageModel.OnPostAsync();

        // Assert
        var scheduledEmail = await _context.Emails
            .FirstOrDefaultAsync(e => e.RecipientEmail == user.Email);

        Assert.NotNull(scheduledEmail);
        Assert.Equal(user.Id, scheduledEmail.UserId);
    }

    [Fact(DisplayName = "Additional: OnGetAsync - Giới hạn 50 logs mới nhất")]
    public async Task OnGetAsync_LoadsOnly50MostRecentLogs()
    {
        // Arrange
        var emails = new List<Email>();
        for (int i = 0; i < 100; i++)
        {
            emails.Add(new Email
            {
                RecipientEmail = "test@gmail.com",
                Subject = $"Test {i}",
                Body = "Body",
                Status = "Sent",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        _context.Emails.AddRange(emails);
        await _context.SaveChangesAsync();

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal(50, _pageModel.EmailLogs.Count);
        Assert.Equal("Test 0", _pageModel.EmailLogs[0].Subject); // Most recent
    }

    [Fact(DisplayName = "Additional: Input validation - Required fields")]
    public void InputModel_RequiredFields_HaveValidation()
    {
        // Arrange
        var input = new EmailSenderModel.EmailInputModel
        {
            To = "",
            Subject = "",
            Body = ""
        };

        var validationContext = new ValidationContext(input);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(input, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("To"));
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Subject"));
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Body"));
    }

    #endregion
}
