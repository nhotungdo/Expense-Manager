# Script chạy Quick Email Test
# Gửi email "Test được chưa" đến donhotung2004@gmail.com

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  📧 QUICK EMAIL TEST - Money Tracker App" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra xem đã cập nhật App Password chưa
Write-Host "⚠️  QUAN TRỌNG: Trước khi chạy test, hãy đảm bảo:" -ForegroundColor Yellow
Write-Host "   1. Đã tạo App Password từ Google Account" -ForegroundColor Yellow
Write-Host "   2. Đã cập nhật Password trong QuickEmailTest.cs (dòng 52)" -ForegroundColor Yellow
Write-Host ""

$continue = Read-Host "Bạn đã cập nhật App Password chưa? (y/n)"

if ($continue -ne "y") {
    Write-Host ""
    Write-Host "❌ Vui lòng cập nhật App Password trước khi chạy test!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Hướng dẫn:" -ForegroundColor Cyan
    Write-Host "1. Mở file: QuickEmailTest.cs" -ForegroundColor White
    Write-Host "2. Tìm dòng 52: Password = 'YOUR_APP_PASSWORD_HERE'" -ForegroundColor White
    Write-Host "3. Thay YOUR_APP_PASSWORD_HERE bằng App Password của bạn" -ForegroundColor White
    Write-Host "4. Lưu file và chạy lại script này" -ForegroundColor White
    Write-Host ""
    Write-Host "Xem chi tiết trong file: QUICK_EMAIL_TEST_GUIDE.md" -ForegroundColor Cyan
    Write-Host ""
    pause
    exit
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  🚀 Bắt đầu chạy test..." -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""

# Chạy test
Write-Host "📝 Test Case: Gửi email 'Test được chưa'" -ForegroundColor White
Write-Host "📧 Người nhận: donhotung2004@gmail.com" -ForegroundColor White
Write-Host ""

# Chạy test với output chi tiết
dotnet test --filter "FullyQualifiedName~QuickEmailTest.SendSimpleTestEmail" --logger "console;verbosity=detailed"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  ✅ Test hoàn tất!" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📧 Kiểm tra email tại: https://mail.google.com" -ForegroundColor Yellow
Write-Host "   Tài khoản: donhotung2004@gmail.com" -ForegroundColor Yellow
Write-Host "   Tìm email với tiêu đề: [TEST] Test được chưa" -ForegroundColor Yellow
Write-Host ""

pause
