# Money Tracker - Ứng dụng Quản lý Tài chính Cá nhân

## 📋 Tổng quan

Money Tracker là một ứng dụng web quản lý tài chính cá nhân được xây dựng bằng ASP.NET Core 8.0, cung cấp các tính năng quản lý thu chi, báo cáo tài chính và gợi ý AI thông minh.

## ✨ Tính năng chính

### 🔐 Xác thực & Phân quyền
- ✅ Đăng nhập với Google OAuth2
- ✅ Đăng xuất
- ✅ Phân quyền User / Admin

### 👤 Tính năng cho Người dùng

#### 📊 Dashboard
- ✅ Trang tổng quan hiển thị tình hình thu/chi, số dư, biểu đồ nhanh

#### 💰 Quản lý Chi tiêu (Expense Management)
- ✅ Thêm chi tiêu
- ✅ Sửa chi tiêu
- ✅ Xóa chi tiêu
- ✅ Xem danh sách chi tiêu (lọc theo ngày, danh mục, số tiền, tìm kiếm)

#### 💵 Quản lý Thu nhập (Income Management)
- ✅ Thêm thu nhập
- ✅ Sửa thu nhập
- ✅ Xóa thu nhập
- ✅ Xem danh sách thu nhập (lọc, tìm kiếm)

#### 🏷️ Quản lý Danh mục (Category Management)
- ✅ Tạo danh mục (ăn uống, mua sắm, lương, đầu tư,…)
- ✅ Sửa danh mục
- ✅ Xóa danh mục

#### 🤖 AI Suggestions (Nâng cao)
- ✅ Xem gợi ý tài chính từ AI dựa trên lịch sử chi tiêu
- ✅ Gợi ý ngân sách cá nhân hóa (ví dụ: hạn chế chi cho ăn uống > 30% tổng thu nhập)

#### 📈 Báo cáo & Thống kê
- ✅ Báo cáo tổng hợp hàng tháng
- ✅ Biểu đồ (chi tiêu theo danh mục, thu nhập theo nguồn, xu hướng theo tháng)
- ✅ Xuất báo cáo (PDF/Excel – optional)

### 👨‍💼 Tính năng cho Admin

#### 👥 Quản lý Người dùng
- ✅ Xem danh sách người dùng
- ✅ Enable/Disable user (khóa/mở tài khoản)
- ✅ Xóa user

#### 🌐 Quản lý Danh mục toàn cục
- ✅ Quản lý các danh mục mặc định dùng chung

#### 🤖 Giám sát AI Suggestions
- ✅ Xem gợi ý AI được hệ thống tạo ra
- ✅ Điều chỉnh hoặc chặn gợi ý nếu cần

### ⚙️ Quản trị Hệ thống
- ✅ Logging & Audit (ghi log hoạt động: login, CRUD thu chi, thay đổi danh mục…)
- ✅ Email Notifications (tùy chọn, gửi báo cáo hoặc nhắc nhở chi tiêu hàng tháng)

## 🛠️ Công nghệ sử dụng

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Authentication** - Xác thực
- **Google OAuth2** - Đăng nhập Google
- **Serilog** - Logging
- **MailKit** - Email service
- **AutoMapper** - Object mapping

### Frontend
- **Razor Pages** - Server-side rendering
- **Bootstrap 5** - UI framework
- **Chart.js** - Biểu đồ
- **Font Awesome** - Icons
- **jQuery** - JavaScript library

## 📁 Cấu trúc dự án

```
MoneyTracker/
├── Controllers/           # API Controllers
│   ├── AuthController.cs
│   ├── ExpenseController.cs
│   ├── IncomeController.cs
│   ├── CategoryController.cs
│   ├── DashboardController.cs
│   └── AdminController.cs
├── Models/               # Data Models
│   ├── DTOs/            # Data Transfer Objects
│   ├── User.cs
│   ├── Expense.cs
│   ├── Income.cs
│   ├── Category.cs
│   ├── AiSuggestion.cs
│   └── ExpenseManagerContext.cs
├── Services/            # Business Logic
│   ├── Interfaces/     # Service Interfaces
│   └── Implementations/ # Service Implementations
├── Pages/              # Razor Pages
│   ├── Dashboard.cshtml
│   ├── Expenses.cshtml
│   ├── Login.cshtml
│   └── Shared/
└── wwwroot/           # Static files
```

## 🚀 Cài đặt và Chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019 hoặc mới hơn
- Visual Studio 2022 hoặc VS Code

### Cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd MoneyTracker
```

2. **Cấu hình Database**
- Cập nhật connection string trong `appsettings.json`
- Chạy migration để tạo database

3. **Cấu hình Google OAuth**
- Tạo Google OAuth credentials tại [Google Cloud Console](https://console.cloud.google.com/)
- Cập nhật `ClientId` và `ClientSecret` trong `appsettings.json`

4. **Cấu hình Email (Optional)**
- Cập nhật email settings trong `appsettings.json`

5. **Chạy ứng dụng**
```bash
dotnet run
```

### Cấu hình Database

Connection string mặc định:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NHOTUNG\\SQLEXPRESS;Initial Catalog=ExpenseManager;User ID=sa;Password=123;Trusted_Connection=True;Trust Server Certificate=True"
  }
}
```

## 🔧 Cấu hình

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "MoneyTracker",
    "Audience": "MoneyTrackerUsers",
    "ExpiryMinutes": 60
  }
}
```

### Google OAuth
```json
{
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

### Email Settings
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@moneytracker.com",
    "UseSsl": true
  }
}
```

## 📚 API Documentation

### Authentication Endpoints
- `GET /api/auth/google-login` - Đăng nhập Google
- `GET /api/auth/google-callback` - Callback từ Google
- `POST /api/auth/logout` - Đăng xuất
- `GET /api/auth/me` - Lấy thông tin user hiện tại

### Expense Endpoints
- `GET /api/expense` - Lấy danh sách chi tiêu
- `GET /api/expense/{id}` - Lấy chi tiêu theo ID
- `POST /api/expense` - Tạo chi tiêu mới
- `PUT /api/expense/{id}` - Cập nhật chi tiêu
- `DELETE /api/expense/{id}` - Xóa chi tiêu

### Income Endpoints
- `GET /api/income` - Lấy danh sách thu nhập
- `GET /api/income/{id}` - Lấy thu nhập theo ID
- `POST /api/income` - Tạo thu nhập mới
- `PUT /api/income/{id}` - Cập nhật thu nhập
- `DELETE /api/income/{id}` - Xóa thu nhập

### Category Endpoints
- `GET /api/category` - Lấy danh sách danh mục
- `GET /api/category/global` - Lấy danh mục toàn cục
- `POST /api/category` - Tạo danh mục mới
- `PUT /api/category/{id}` - Cập nhật danh mục
- `DELETE /api/category/{id}` - Xóa danh mục

### Dashboard Endpoints
- `GET /api/dashboard` - Lấy dữ liệu dashboard
- `GET /api/dashboard/monthly-report` - Báo cáo hàng tháng
- `GET /api/dashboard/yearly-report` - Báo cáo hàng năm
- `POST /api/dashboard/generate-ai-suggestion` - Tạo gợi ý AI

### Admin Endpoints (Yêu cầu quyền Admin)
- `GET /api/admin/users` - Quản lý người dùng
- `GET /api/admin/global-categories` - Quản lý danh mục toàn cục
- `GET /api/admin/ai-suggestions` - Quản lý gợi ý AI
- `GET /api/admin/audit-logs` - Xem log hệ thống

## 🔒 Bảo mật

- JWT Token authentication
- Google OAuth2 integration
- Role-based authorization (User/Admin)
- Input validation và sanitization
- SQL injection protection
- XSS protection
- CORS configuration

## 📝 Logging

Ứng dụng sử dụng Serilog để ghi log:
- Console logging
- File logging (logs/moneytracker-{date}.txt)
- Database logging (audit trail)
- Structured logging với context

## 🧪 Testing

Để test ứng dụng:
1. Chạy ứng dụng
2. Truy cập `/Login` để đăng nhập
3. Sử dụng Google OAuth để xác thực
4. Truy cập `/Dashboard` để xem giao diện chính

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

## 📄 License

Dự án này được phát hành dưới MIT License.

## 👥 Tác giả

- **MoneyTracker Team** - *Phát triển ban đầu*

## 📞 Liên hệ

- Email: support@moneytracker.com
- Website: https://moneytracker.com

---

**Lưu ý**: Đây là phiên bản demo. Để sử dụng trong production, vui lòng cấu hình lại các settings bảo mật và database connection string.
