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

        public OtpService(ExpenseManagerContext context)
        {
            _context = context;
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

            // 3. Send Email (Removed)
            /*
            var subject = "Mã xác thực OTP - Money Tracker App";
            var body = $@" ... ";
            if (!string.IsNullOrEmpty(user.Email)) { ... }
            */

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
