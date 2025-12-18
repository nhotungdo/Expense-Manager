using System;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(long userId);
        Task<bool> ValidateOtpAsync(long userId, string otpCode);
    }

    public class OtpService : IOtpService
    {
        private readonly ExpenseManagerContext _context;
        private readonly IEmailService _emailService;

        public OtpService(ExpenseManagerContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GenerateAndSendOtpAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            // 1. Generate OTP
            var random = new Random();
            var otpCode = random.Next(0, 999999).ToString("D6"); // 6 digits

            // 2. Save to DB
            var userOtp = new UserOtp
            {
                UserId = userId,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.UserOtps.Add(userOtp);
            await _context.SaveChangesAsync();

            // 3. Send Email
            var subject = "Mã xác thực OTP - Money Tracker App";
            var body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; max-width: 500px;'>
                    <h2 style='color: #2563eb;'>Mã Xác Thực Giao Dịch</h2>
                    <p>Xin chào <strong>{user.FullName}</strong>,</p>
                    <p>Bạn vừa yêu cầu mã xác thực OTP cho giao dịch trên Money Tracker App.</p>
                    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #1e40af;'>{otpCode}</span>
                    </div>
                    <p>Mã này có hiệu lực trong vòng <strong>5 phút</strong>. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr style='border: 0; border-top: 1px solid #e0e0e0; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #6b7280;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này hoặc liên hệ hỗ trợ.</p>
                </div>
            ";

            // Fire and forget email sending to not block response? 
            // Better to await it to ensure it's sent or logged.
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(user.Email, subject, body, userId);
            }

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(long userId, string otpCode)
        {
            var otpRecord = await _context.UserOtps
                .Where(o => o.UserId == userId && o.OtpCode == otpCode && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null) return false;

            if (otpRecord.ExpiresAt < DateTime.UtcNow) return false;

            // Mark as used
            otpRecord.IsUsed = true;
            _context.UserOtps.Update(otpRecord);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
