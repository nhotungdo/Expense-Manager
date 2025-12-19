using System;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoneyTrackerApp.Configurations;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, long userId);
        Task SendEmailToUserAsync(long userId, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly ExpenseManagerContext _context;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, ExpenseManagerContext context)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Gửi email đến user dựa trên UserId, tự động kiểm tra EmailNotifications setting
        /// </summary>
        public async Task SendEmailToUserAsync(long userId, string subject, string body)
        {
            // 1. Lấy thông tin User từ Database
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning($"User {userId} not found or has no email address");
                return;
            }

            // 2. Kiểm tra xem User có bật nhận thông báo email không
            if (!user.EmailNotifications)
            {
                _logger.LogInformation($"Email notifications disabled for user {userId}");
                return; // Người dùng đã tắt thông báo
            }

            // 3. Tạo bản ghi trong bảng Emails (Trạng thái Pending)
            var emailLog = new Email
            {
                UserId = userId,
                Subject = subject,
                Body = body,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Emails.Add(emailLog);
            await _context.SaveChangesAsync();

            // 4. Thực hiện gửi Email qua SMTP
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                message.To.Add(new MailAddress(user.Email));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true; // Cho phép gửi HTML

                using (var client = new SmtpClient(_emailSettings.MailServer, _emailSettings.MailPort))
                {
                    client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;
                    
                    await client.SendMailAsync(message);
                }

                // 5. Cập nhật trạng thái thành công
                emailLog.Status = "sent";
                emailLog.SentAt = DateTime.UtcNow;
                _logger.LogInformation($"Email sent successfully to user {userId}");
            }
            catch (Exception ex)
            {
                // 6. Cập nhật trạng thái thất bại nếu có lỗi
                emailLog.Status = "failed";
                _logger.LogError(ex, $"Failed to send email to user {userId}");
            }

            // Lưu cập nhật trạng thái
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gửi email trực tiếp đến địa chỉ email (legacy method, không kiểm tra user settings)
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body, long userId)
        {
            var emailLog = new Email
            {
                UserId = userId,
                Subject = subject,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                Status = "pending"
            };

            try
            {
                using var client = new SmtpClient(_emailSettings.MailServer, _emailSettings.MailPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                
                emailLog.Status = "sent";
                emailLog.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                emailLog.Status = "failed";
            }
            finally
            {
                try 
                {
                    _context.Emails.Add(emailLog);
                    await _context.SaveChangesAsync();
                }
                catch(Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to log email to database");
                }
            }
        }
    }
}
