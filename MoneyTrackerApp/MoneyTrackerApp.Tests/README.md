# Money Tracker App - Email Testing Guide

## Tổng quan

Repository này chứa các test cases và automated tests để kiểm tra chức năng gửi email của Money Tracker App, đặc biệt tập trung vào việc gửi email đến **donhotung2004@gmail.com**.

## Cấu trúc Test

### 1. Tài liệu Test Cases
📄 **File:** `EmailSendingTestCases.md`

Tài liệu chi tiết chứa 8 test cases chính:
- **TC-EMAIL-001**: Gửi email thành công
- **TC-EMAIL-002**: Nội dung email phức tạp (HTML, ký tự đặc biệt, tiếng Việt)
- **TC-EMAIL-003**: Tiêu đề dài và ký tự đặc biệt
- **TC-EMAIL-004**: Validation email không hợp lệ
- **TC-EMAIL-005**: Hiệu năng - Gửi nhiều email
- **TC-EMAIL-006**: Xử lý lỗi SMTP
- **TC-EMAIL-007**: Lên lịch gửi email
- **TC-EMAIL-008**: Gửi đến nhiều người nhận

### 2. Automated Tests

#### Unit Tests
📄 **File:** `MoneyTrackerApp.Tests/EmailServiceTests.cs`

Tests cho `EmailService` class:
- ✅ Tạo email records trong database
- ✅ Link email với user
- ✅ Xử lý HTML content
- ✅ Xử lý ký tự đặc biệt và tiếng Việt
- ✅ Gửi nhiều email
- ✅ Performance testing

**Chạy unit tests:**
```bash
cd MoneyTrackerApp.Tests
dotnet test --filter "FullyQualifiedName~EmailServiceTests"
```

#### Integration Tests
📄 **File:** `MoneyTrackerApp.Tests/EmailSenderModelTests.cs`

Tests cho `EmailSenderModel` (Page Model):
- ✅ OnPostAsync với input hợp lệ
- ✅ Validation
- ✅ Parse nhiều email
- ✅ Scheduled emails
- ✅ Load email logs

**Chạy integration tests:**
```bash
cd MoneyTrackerApp.Tests
dotnet test --filter "FullyQualifiedName~EmailSenderModelTests"
```

#### Manual Tests
📄 **File:** `MoneyTrackerApp.Tests/EmailManualTests.cs`

⚠️ **CẢNH BÁO**: Các tests này sẽ GỬI EMAIL THỰC SỰ đến **donhotung2004@gmail.com**!

Tests bao gồm:
- 📧 Gửi email cơ bản
- 📧 Gửi email HTML phức tạp
- 📧 Gửi email với tiêu đề dài
- 📧 Gửi 10 emails liên tiếp (performance)
- 📧 Gửi đến nhiều người nhận
- 📧 Tạo email báo cáo test

## Cách chạy Manual Tests

### Bước 1: Cấu hình SMTP Settings

Mở file `MoneyTrackerApp.Tests/EmailManualTests.cs` và cập nhật:

```csharp
var emailSettings = Options.Create(new EmailSettings
{
    Host = "smtp.gmail.com",
    Port = 587,
    Username = "YOUR_EMAIL@gmail.com",        // ← Thay đổi
    Password = "YOUR_APP_PASSWORD",           // ← Thay đổi (App Password)
    FromEmail = "YOUR_EMAIL@gmail.com",       // ← Thay đổi
    FromName = "Money Tracker App - Test System",
    EnableSsl = true
});
```

### Bước 2: Bỏ Skip attribute

Tìm test case bạn muốn chạy và bỏ `Skip = "..."`:

**Trước:**
```csharp
[Fact(DisplayName = "TC-EMAIL-001: ...", Skip = "Manual test - Uncomment để chạy")]
```

**Sau:**
```csharp
[Fact(DisplayName = "TC-EMAIL-001: ...")]
```

### Bước 3: Chạy test

```bash
cd MoneyTrackerApp.Tests

# Chạy một test cụ thể
dotnet test --filter "DisplayName~TC-EMAIL-001"

# Chạy tất cả manual tests (nếu đã bỏ Skip)
dotnet test --filter "Category=ManualTest"

# Chạy với output chi tiết
dotnet test --filter "DisplayName~TC-EMAIL-001" --logger "console;verbosity=detailed"
```

### Bước 4: Kiểm tra kết quả

1. ✅ Kiểm tra console output
2. ✅ Kiểm tra hộp thư của **donhotung2004@gmail.com**
3. ✅ Ghi lại kết quả trong `EmailSendingTestCases.md`

## Chạy tất cả tests

```bash
cd MoneyTrackerApp.Tests

# Chạy tất cả unit và integration tests (không bao gồm manual tests)
dotnet test --filter "Category!=ManualTest"

# Chạy tất cả tests với coverage
dotnet test --collect:"XPlat Code Coverage"

# Chạy với output chi tiết
dotnet test --logger "console;verbosity=detailed"
```

## Cấu hình Gmail App Password

Để gửi email qua Gmail SMTP, bạn cần tạo App Password:

### Bước 1: Bật 2-Step Verification
1. Truy cập https://myaccount.google.com/security
2. Tìm "2-Step Verification"
3. Bật tính năng này

### Bước 2: Tạo App Password
1. Truy cập https://myaccount.google.com/apppasswords
2. Chọn "Mail" và "Other (Custom name)"
3. Nhập tên: "Money Tracker App Test"
4. Click "Generate"
5. Copy password 16 ký tự
6. Sử dụng password này trong SMTP settings

### Bước 3: Cập nhật appsettings.json

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Money Tracker App",
    "EnableSsl": true
  }
}
```

## Test Checklist

Trước khi chạy tests, đảm bảo:

- [ ] SMTP settings đã được cấu hình đúng
- [ ] Gmail App Password đã được tạo
- [ ] Database đã được migrate (`dotnet ef database update`)
- [ ] Application có thể build thành công (`dotnet build`)
- [ ] Có quyền truy cập email **donhotung2004@gmail.com** để kiểm tra
- [ ] Đã đọc kỹ test cases trong `EmailSendingTestCases.md`

## Ghi lại kết quả test

Sau khi chạy mỗi test case, cập nhật kết quả trong `EmailSendingTestCases.md`:

```markdown
### Kết quả thực tế
- Thời gian test: 25/12/2025 18:45:00
- Status: [X] Pass / [ ] Fail
- Thời gian nhận email: ~2 giây
- Screenshot: screenshots/tc-email-001.png
- Ghi chú: Email được gửi và nhận thành công
```

## Troubleshooting

### Lỗi: "Authentication failed"
- ✅ Kiểm tra Username và Password
- ✅ Đảm bảo đang dùng App Password, không phải password thường
- ✅ Kiểm tra 2-Step Verification đã bật

### Lỗi: "Connection timeout"
- ✅ Kiểm tra firewall
- ✅ Kiểm tra Port (587 hoặc 465)
- ✅ Kiểm tra EnableSsl = true

### Lỗi: "Mailbox unavailable"
- ✅ Kiểm tra địa chỉ email người nhận
- ✅ Kiểm tra FromEmail có hợp lệ

### Email không đến hộp thư
- ✅ Kiểm tra Spam folder
- ✅ Đợi 1-2 phút
- ✅ Kiểm tra email logs trong database

## SQL Queries hữu ích

```sql
-- Xem tất cả emails đã gửi đến donhotung2004@gmail.com
SELECT * FROM Emails 
WHERE RecipientEmail = 'donhotung2004@gmail.com'
ORDER BY CreatedAt DESC;

-- Đếm số email theo status
SELECT Status, COUNT(*) as Count
FROM Emails
WHERE RecipientEmail = 'donhotung2004@gmail.com'
GROUP BY Status;

-- Tính success rate
SELECT 
    COUNT(*) as Total,
    SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Sent,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    CAST(SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as SuccessRate
FROM Emails
WHERE RecipientEmail = 'donhotung2004@gmail.com';

-- Xóa tất cả test emails
DELETE FROM Emails 
WHERE Subject LIKE '%[TEST]%';
```

## Test Report Template

Sau khi hoàn thành tất cả tests, tạo báo cáo:

```markdown
# Email Testing Report
**Date:** 25/12/2025
**Tester:** [Your Name]
**Environment:** Development

## Summary
- Total Test Cases: 8
- Passed: X
- Failed: Y
- Pass Rate: Z%

## Test Results
[Copy từ EmailSendingTestCases.md]

## Issues Found
1. [Bug description]
2. [Bug description]

## Recommendations
1. [Improvement suggestion]
2. [Improvement suggestion]
```

## Liên hệ

Nếu có vấn đề hoặc câu hỏi về tests:
- 📧 Email: donhotung2004@gmail.com
- 📝 Tạo issue trong repository

---

**Happy Testing! 🎉**
