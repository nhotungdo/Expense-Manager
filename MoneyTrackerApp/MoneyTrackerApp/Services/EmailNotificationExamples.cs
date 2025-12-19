using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    /// <summary>
    /// Các ví dụ sử dụng EmailService trong các tình huống thực tế
    /// File này chỉ để tham khảo, không được sử dụng trực tiếp
    /// </summary>
    public class EmailNotificationExamples
    {
        private readonly IEmailService _emailService;

        public EmailNotificationExamples(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Ví dụ 1: Gửi email khi tạo giao dịch mới
        /// </summary>
        public async Task SendTransactionCreatedEmail(long userId, CreateTransactionDto transaction)
        {
            var transactionType = transaction.TransactionType == 1 ? "thu nhập" : "chi tiêu";
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Giao dịch mới</h2>
                    <p>Bạn vừa thêm một giao dịch {transactionType}:</p>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'><strong>Số tiền:</strong></td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{transaction.Amount:N0} {transaction.Currency ?? "VND"}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'><strong>Ghi chú:</strong></td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{transaction.Note ?? "Không có"}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'><strong>Ngày:</strong></td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{transaction.TransactionDate:dd/MM/yyyy HH:mm}</td>
                        </tr>
                    </table>
                    <p style='color: #7f8c8d; font-size: 12px; margin-top: 20px;'>
                        Cảm ơn bạn đã sử dụng Expense Manager!
                    </p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, "Thông báo giao dịch mới", emailContent);
        }

        /// <summary>
        /// Ví dụ 2: Gửi email cảnh báo vượt ngân sách
        /// </summary>
        public async Task SendBudgetExceededEmail(long userId, string categoryName, decimal budgetAmount, decimal spentAmount)
        {
            var percentOver = ((spentAmount - budgetAmount) / budgetAmount) * 100;
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #e74c3c;'>⚠️ Cảnh báo vượt ngân sách</h2>
                    <p>Bạn đã vượt ngân sách cho danh mục <strong>{categoryName}</strong>!</p>
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Ngân sách:</strong> {budgetAmount:N0} VND</p>
                        <p style='margin: 5px 0;'><strong>Đã chi:</strong> {spentAmount:N0} VND</p>
                        <p style='margin: 5px 0;'><strong>Vượt:</strong> {(spentAmount - budgetAmount):N0} VND ({percentOver:F1}%)</p>
                    </div>
                    <p>Hãy xem xét điều chỉnh chi tiêu hoặc tăng ngân sách cho danh mục này.</p>
                    <p style='color: #7f8c8d; font-size: 12px; margin-top: 20px;'>
                        Bạn có thể tắt thông báo email trong phần Cài đặt.
                    </p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, $"⚠️ Vượt ngân sách: {categoryName}", emailContent);
        }

        /// <summary>
        /// Ví dụ 3: Gửi email cảnh báo sắp đến hạn thanh toán nợ
        /// </summary>
        public async Task SendDebtDueReminderEmail(long userId, string debtName, decimal remainingAmount, DateTime dueDate)
        {
            var daysUntilDue = (dueDate - DateTime.Now).Days;
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #f39c12;'>🔔 Nhắc nhở thanh toán nợ</h2>
                    <p>Khoản nợ <strong>{debtName}</strong> sắp đến hạn thanh toán!</p>
                    <div style='background-color: #fef5e7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Số tiền còn lại:</strong> {remainingAmount:N0} VND</p>
                        <p style='margin: 5px 0;'><strong>Ngày đến hạn:</strong> {dueDate:dd/MM/yyyy}</p>
                        <p style='margin: 5px 0;'><strong>Còn lại:</strong> {daysUntilDue} ngày</p>
                    </div>
                    <p>Hãy chuẩn bị thanh toán để tránh phát sinh lãi suất hoặc phí phạt.</p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, $"🔔 Nhắc nhở: {debtName} sắp đến hạn", emailContent);
        }

        /// <summary>
        /// Ví dụ 4: Gửi email báo cáo tháng
        /// </summary>
        public async Task SendMonthlyReportEmail(long userId, decimal totalIncome, decimal totalExpense, decimal netIncome, int transactionCount)
        {
            var currentMonth = DateTime.Now.ToString("MM/yyyy");
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #3498db;'>📊 Báo cáo tài chính tháng {currentMonth}</h2>
                    <p>Dưới đây là tổng quan tài chính của bạn trong tháng vừa qua:</p>
                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                        <tr style='background-color: #ecf0f1;'>
                            <td style='padding: 12px; border: 1px solid #bdc3c7;'><strong>Tổng thu nhập</strong></td>
                            <td style='padding: 12px; border: 1px solid #bdc3c7; text-align: right; color: #27ae60;'>{totalIncome:N0} VND</td>
                        </tr>
                        <tr>
                            <td style='padding: 12px; border: 1px solid #bdc3c7;'><strong>Tổng chi tiêu</strong></td>
                            <td style='padding: 12px; border: 1px solid #bdc3c7; text-align: right; color: #e74c3c;'>{totalExpense:N0} VND</td>
                        </tr>
                        <tr style='background-color: #ecf0f1;'>
                            <td style='padding: 12px; border: 1px solid #bdc3c7;'><strong>Thu nhập ròng</strong></td>
                            <td style='padding: 12px; border: 1px solid #bdc3c7; text-align: right; font-weight: bold;'>{netIncome:N0} VND</td>
                        </tr>
                        <tr>
                            <td style='padding: 12px; border: 1px solid #bdc3c7;'><strong>Số giao dịch</strong></td>
                            <td style='padding: 12px; border: 1px solid #bdc3c7; text-align: right;'>{transactionCount}</td>
                        </tr>
                    </table>
                    <p>Hãy tiếp tục theo dõi chi tiêu để đạt được mục tiêu tài chính của bạn!</p>
                    <a href='https://yourapp.com/reports' style='display: inline-block; padding: 10px 20px; background-color: #3498db; color: white; text-decoration: none; border-radius: 5px; margin-top: 10px;'>
                        Xem báo cáo chi tiết
                    </a>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, $"📊 Báo cáo tài chính tháng {currentMonth}", emailContent);
        }

        /// <summary>
        /// Ví dụ 5: Gửi email chào mừng user mới
        /// </summary>
        public async Task SendWelcomeEmail(long userId, string userName)
        {
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Chào mừng đến với Expense Manager! 🎉</h2>
                    <p>Xin chào <strong>{userName}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký sử dụng Expense Manager. Chúng tôi rất vui được đồng hành cùng bạn trong hành trình quản lý tài chính cá nhân.</p>
                    
                    <h3 style='color: #3498db;'>Bắt đầu nhanh:</h3>
                    <ol>
                        <li>Thêm ví đầu tiên của bạn</li>
                        <li>Tạo các danh mục thu chi</li>
                        <li>Bắt đầu ghi chép giao dịch</li>
                        <li>Thiết lập ngân sách hàng tháng</li>
                    </ol>
                    
                    <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>💡 Mẹo:</strong> Hãy ghi chép giao dịch ngay sau khi chi tiêu để không bỏ sót!</p>
                    </div>
                    
                    <p>Nếu bạn cần hỗ trợ, đừng ngần ngại liên hệ với chúng tôi.</p>
                    <p>Chúc bạn quản lý tài chính hiệu quả!</p>
                    
                    <p style='color: #7f8c8d; font-size: 12px; margin-top: 30px;'>
                        Expense Manager Team<br>
                        Email: support@expensemanager.com
                    </p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, "Chào mừng đến với Expense Manager! 🎉", emailContent);
        }

        /// <summary>
        /// Ví dụ 6: Gửi email xác nhận đổi mật khẩu
        /// </summary>
        public async Task SendPasswordChangedEmail(long userId)
        {
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>🔒 Mật khẩu đã được thay đổi</h2>
                    <p>Mật khẩu tài khoản của bạn vừa được thay đổi thành công.</p>
                    
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>⚠️ Lưu ý:</strong> Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ ngay với chúng tôi để bảo vệ tài khoản.</p>
                    </div>
                    
                    <p><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    
                    <p style='color: #7f8c8d; font-size: 12px; margin-top: 30px;'>
                        Đây là email tự động, vui lòng không trả lời.
                    </p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, "🔒 Xác nhận thay đổi mật khẩu", emailContent);
        }

        /// <summary>
        /// Ví dụ 7: Gửi email khi đạt mục tiêu tiết kiệm
        /// </summary>
        public async Task SendSavingsGoalAchievedEmail(long userId, string goalName, decimal targetAmount)
        {
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>🎯 Chúc mừng! Bạn đã đạt mục tiêu tiết kiệm!</h2>
                    <p>Tuyệt vời! Bạn đã hoàn thành mục tiêu tiết kiệm <strong>{goalName}</strong>!</p>
                    
                    <div style='background-color: #d4edda; padding: 20px; border-radius: 5px; margin: 20px 0; text-align: center;'>
                        <p style='font-size: 24px; margin: 0; color: #27ae60;'>✨ {targetAmount:N0} VND ✨</p>
                        <p style='margin: 10px 0 0 0;'>Mục tiêu đã đạt được!</p>
                    </div>
                    
                    <p>Sự kiên trì và kỷ luật của bạn đã được đền đáp. Hãy tiếp tục duy trì thói quen tốt này!</p>
                    <p>Bạn có thể thiết lập mục tiêu tiết kiệm mới để tiếp tục hành trình tài chính của mình.</p>
                    
                    <p style='color: #7f8c8d; font-size: 12px; margin-top: 30px;'>
                        Chúc mừng một lần nữa!<br>
                        Expense Manager Team
                    </p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, $"🎯 Chúc mừng! Đã đạt mục tiêu: {goalName}", emailContent);
        }

        /// <summary>
        /// Ví dụ 8: Gửi email nhắc nhở giao dịch định kỳ
        /// </summary>
        public async Task SendRecurringTransactionReminderEmail(long userId, string transactionName, decimal amount, DateTime dueDate)
        {
            var emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #9b59b6;'>🔄 Nhắc nhở giao dịch định kỳ</h2>
                    <p>Giao dịch định kỳ <strong>{transactionName}</strong> sắp đến hạn!</p>
                    
                    <div style='background-color: #f4ecf7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>Tên giao dịch:</strong> {transactionName}</p>
                        <p style='margin: 5px 0;'><strong>Số tiền:</strong> {amount:N0} VND</p>
                        <p style='margin: 5px 0;'><strong>Ngày:</strong> {dueDate:dd/MM/yyyy}</p>
                    </div>
                    
                    <p>Hệ thống sẽ tự động tạo giao dịch này vào ngày đến hạn.</p>
                    <p>Bạn có thể chỉnh sửa hoặc tạm dừng giao dịch định kỳ trong phần Cài đặt.</p>
                </div>
            ";

            await _emailService.SendEmailToUserAsync(userId, $"🔄 Nhắc nhở: {transactionName}", emailContent);
        }
    }
}
