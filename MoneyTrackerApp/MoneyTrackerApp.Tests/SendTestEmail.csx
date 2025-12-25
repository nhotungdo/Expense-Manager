// Script C# đơn giản để test gửi email
// Chạy với: dotnet script SendTestEmail.csx

#r "nuget: System.Net.Mail, 8.0.0"

using System;
using System.Net;
using System.Net.Mail;

Console.WriteLine("============================================================");
Console.WriteLine("  📧 TEST GỬI EMAIL - Money Tracker App");
Console.WriteLine("============================================================");
Console.WriteLine("");

// Thông tin SMTP từ appsettings.json
var smtpHost = "smtp.gmail.com";
var smtpPort = 587;
var username = "nhotungdo89@gmail.com";
var password = "mpegnuzdgxuoqbfq";
var fromEmail = "nhotungdo89@gmail.com";
var fromName = "Money Tracker App - Test System";

// Thông tin email test
var toEmail = "donhotung2004@gmail.com";
var subject = $"[TEST] Test được chưa - {DateTime.Now:HH:mm:ss}";
var body = @$"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 10px;
            padding: 30px;
            color: white;
            text-align: center;
        }}
        .content {{
            background-color: white;
            color: #333;
            border-radius: 8px;
            padding: 30px;
            margin-top: 20px;
        }}
        .message {{
            font-size: 24px;
            font-weight: bold;
            color: #667eea;
            margin: 20px 0;
        }}
        .emoji {{
            font-size: 48px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>📧 Email Test System</h1>
        <p>Money Tracker App</p>
    </div>
    
    <div class='content'>
        <div class='emoji'>✅</div>
        <div class='message'>Test được chưa</div>
        
        <p>Đây là email test từ hệ thống Money Tracker App.</p>
        
        <p><strong>Thông tin test:</strong></p>
        <ul style='text-align: left;'>
            <li>Người nhận: {toEmail}</li>
            <li>Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
            <li>Test case: Gửi email đơn giản</li>
        </ul>
        
        <p style='margin-top: 30px; padding: 15px; background-color: #f0f8ff; border-left: 4px solid #667eea;'>
            <strong>💡 Lưu ý:</strong> Nếu bạn nhận được email này, 
            có nghĩa là chức năng gửi email đã hoạt động thành công! 🎉
        </p>
    </div>
</body>
</html>
";

Console.WriteLine($"📝 Thông tin email:");
Console.WriteLine($"   Từ: {fromEmail}");
Console.WriteLine($"   Đến: {toEmail}");
Console.WriteLine($"   Tiêu đề: {subject}");
Console.WriteLine("");

try
{
    Console.WriteLine("📤 Đang gửi email...");
    var startTime = DateTime.Now;

    using (var message = new MailMessage())
    {
        message.From = new MailAddress(fromEmail, fromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using (var client = new SmtpClient(smtpHost, smtpPort))
        {
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;
            
            await client.SendMailAsync(message);
        }
    }

    var endTime = DateTime.Now;
    var duration = (endTime - startTime).TotalSeconds;

    Console.WriteLine("");
    Console.WriteLine("============================================================");
    Console.WriteLine("  ✅ EMAIL ĐÃ GỬI THÀNH CÔNG!");
    Console.WriteLine("============================================================");
    Console.WriteLine($"⏱️  Thời gian gửi: {duration:F2} giây");
    Console.WriteLine("");
    Console.WriteLine("📧 Kiểm tra email tại:");
    Console.WriteLine($"   https://mail.google.com");
    Console.WriteLine($"   Tài khoản: {toEmail}");
    Console.WriteLine($"   Tìm email với tiêu đề: {subject}");
    Console.WriteLine("============================================================");
}
catch (Exception ex)
{
    Console.WriteLine("");
    Console.WriteLine("============================================================");
    Console.WriteLine("  ❌ LỖI KHI GỬI EMAIL");
    Console.WriteLine("============================================================");
    Console.WriteLine($"Lỗi: {ex.Message}");
    Console.WriteLine($"Chi tiết: {ex.StackTrace}");
    Console.WriteLine("============================================================");
}
