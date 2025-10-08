# Money Tracker - Ứng Dụng Quản Lý Tài Chính Thông Minh

## 📋 Tổng Quan

Money Tracker là một ứng dụng web quản lý tài chính cá nhân được xây dựng bằng ASP.NET Core 8.0, cung cấp các tính năng quản lý thu chi, ngân sách, báo cáo thống kê và gợi ý thông minh từ AI.

## ✨ Tính Năng Chính

### 👤 Cho Người Dùng
- **🔐 Xác thực & Tài khoản**
  - Đăng nhập bằng Google OAuth2
  - Quản lý hồ sơ cá nhân
  - Onboarding cho người dùng mới

- **💸 Quản lý Giao dịch**
  - Thêm, sửa, xóa giao dịch thu/chi
  - Tìm kiếm và lọc giao dịch
  - Xem danh sách giao dịch theo thời gian

- **🧾 Quản lý Danh mục**
  - Tạo, sửa, xóa danh mục cá nhân
  - Danh mục mặc định cho thu nhập và chi tiêu
  - Hỗ trợ icon và màu sắc cho danh mục

- **💰 Ngân sách cá nhân**
  - Đặt ngân sách theo tháng/tuần/năm
  - Theo dõi chi tiêu so với ngân sách
  - Cảnh báo khi vượt hạn mức

- **🤖 AI Gợi ý**
  - Gợi ý chi tiêu thông minh
  - Khuyến nghị ngân sách tự động
  - Phân tích xu hướng chi tiêu

- **📊 Báo cáo & Thống kê**
  - Báo cáo tháng/tuần/năm
  - Biểu đồ thu/chi trực quan
  - So sánh kỳ trước
  - Xuất file Excel/PDF

### 🧑‍💼 Cho Quản trị viên
- **👥 Quản lý người dùng**
  - Xem danh sách người dùng
  - Phân quyền User/Admin
  - Khóa/mở khóa tài khoản

- **🗂️ Quản lý danh mục hệ thống**
  - Tạo danh mục mặc định
  - Chỉnh sửa danh mục toàn cục

- **📈 Giám sát hệ thống**
  - Theo dõi hoạt động người dùng
  - Thống kê tổng chi tiêu toàn hệ thống
  - Kiểm tra gợi ý AI

## 🛠️ Công Nghệ Sử Dụng

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Authentication** - Xác thực
- **Google OAuth2** - Đăng nhập Google
- **Serilog** - Logging
- **AutoMapper** - Object mapping

### Frontend
- **Razor Pages** - Server-side rendering
- **Bootstrap 5** - CSS Framework
- **Chart.js** - Biểu đồ
- **Font Awesome** - Icons
- **JavaScript ES6+** - Client-side logic

### Services & Libraries
- **MailKit** - Email service
- **iTextSharp** - PDF generation
- **ClosedXML** - Excel export
- **EPPlus** - Excel processing

## 🚀 Cài Đặt và Chạy

### Yêu Cầu Hệ Thống
- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone Repository
```bash
git clone <repository-url>
cd Expense-Manager/MoneyTracker/MoneyTracker
```

### Bước 2: Cấu Hình Database
1. Mở file `appsettings.json`
2. Cập nhật connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=ExpenseManager;User ID=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=True;Trust Server Certificate=True"
  }
}
```

### Bước 3: Cấu Hình Google OAuth
1. Tạo project trên [Google Cloud Console](https://console.cloud.google.com/)
2. Bật Google+ API
3. Tạo OAuth 2.0 credentials
4. Cập nhật trong `appsettings.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  }
}
```

### Bước 4: Chạy Migration
```bash
dotnet ef database update
```

### Bước 5: Chạy Ứng Dụng
```bash
dotnet run
```

Truy cập: `https://localhost:5001`

## 📁 Cấu Trúc Dự Án

```
MoneyTracker/
├── Controllers/          # API Controllers
│   ├── AuthController.cs
│   ├── DashboardController.cs
│   ├── TransactionController.cs
│   ├── BudgetController.cs
│   ├── ReportController.cs
│   └── AdminController.cs
├── Models/              # Data Models
│   ├── User.cs
│   ├── Expense.cs
│   ├── Income.cs
│   ├── Budget.cs
│   ├── Category.cs
│   └── DTOs/           # Data Transfer Objects
├── Services/            # Business Logic
│   ├── ITransactionService.cs
│   ├── TransactionService.cs
│   ├── IBudgetService.cs
│   ├── BudgetService.cs
│   └── ...
├── Pages/              # Razor Pages
│   ├── Login.cshtml
│   ├── Dashboard.cshtml
│   ├── Transactions.cshtml
│   ├── Budgets.cshtml
│   ├── Reports.cshtml
│   ├── AI.cshtml
│   └── Admin.cshtml
├── Migrations/         # Database Migrations
├── wwwroot/           # Static Files
│   ├── css/
│   ├── js/
│   └── lib/
└── Program.cs         # Application Entry Point
```

## 🔧 API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/google` - Đăng nhập Google
- `POST /api/auth/logout` - Đăng xuất

### Transactions
- `GET /api/transactions` - Lấy danh sách giao dịch
- `POST /api/transactions` - Tạo giao dịch mới
- `PUT /api/transactions/{id}` - Cập nhật giao dịch
- `DELETE /api/transactions/{id}` - Xóa giao dịch

### Budgets
- `GET /api/budgets` - Lấy danh sách ngân sách
- `POST /api/budgets` - Tạo ngân sách mới
- `PUT /api/budgets/{id}` - Cập nhật ngân sách
- `DELETE /api/budgets/{id}` - Xóa ngân sách

### Reports
- `GET /api/reports/monthly` - Báo cáo tháng
- `GET /api/reports/yearly` - Báo cáo năm
- `GET /api/reports/custom` - Báo cáo tùy chỉnh
- `GET /api/reports/export/{format}` - Xuất báo cáo

### Admin
- `GET /api/admin/users` - Quản lý người dùng
- `GET /api/admin/stats` - Thống kê hệ thống
- `GET /api/admin/logs` - Nhật ký hoạt động

## 🎨 Giao Diện

Ứng dụng sử dụng thiết kế hiện đại với:
- **Glassmorphism** - Hiệu ứng kính mờ
- **Gradient Backgrounds** - Nền gradient đẹp mắt
- **Responsive Design** - Tương thích mọi thiết bị
- **Dark Theme** - Giao diện tối hiện đại
- **Smooth Animations** - Hiệu ứng mượt mà

## 🔒 Bảo Mật

- **JWT Authentication** - Xác thực token
- **Google OAuth2** - Đăng nhập an toàn
- **HTTPS** - Mã hóa dữ liệu
- **Input Validation** - Kiểm tra đầu vào
- **SQL Injection Protection** - Bảo vệ khỏi SQL injection
- **CORS Configuration** - Cấu hình CORS

## 📊 Database Schema

### Bảng chính:
- **Users** - Thông tin người dùng
- **Expenses** - Giao dịch chi tiêu
- **Incomes** - Giao dịch thu nhập
- **Categories** - Danh mục
- **Budgets** - Ngân sách
- **Transactions** - Giao dịch tổng hợp
- **Reports** - Báo cáo
- **AuditLogs** - Nhật ký hoạt động

## 🚀 Triển Khai

### Docker
```bash
docker build -t money-tracker .
docker run -p 5000:80 money-tracker
```

### Azure
1. Tạo App Service trên Azure
2. Cấu hình connection string
3. Deploy từ GitHub Actions

### IIS
1. Publish ứng dụng
2. Cấu hình IIS
3. Thiết lập SSL certificate

## 🤝 Đóng Góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📝 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📞 Liên Hệ

- **Email**: support@moneytracker.com
- **Website**: https://moneytracker.com
- **GitHub**: https://github.com/yourusername/money-tracker

## 🙏 Acknowledgments

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) - Web framework
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Chart.js](https://www.chartjs.org/) - Chart library
- [Font Awesome](https://fontawesome.com/) - Icons
- [Google OAuth](https://developers.google.com/identity) - Authentication

---

**Money Tracker** - Quản lý tài chính thông minh, đơn giản và hiệu quả! 💰✨