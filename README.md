# Money Tracker - Ứng dụng Quản lý Tài chính Cá nhân

> Ứng dụng theo dõi thu chi cá nhân/doanh nghiệp, đăng nhập Google, phân quyền Admin/User.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET-Razor%20Pages-5C2D91" alt="Razor Pages" />
  <img src="https://img.shields.io/badge/EF%20Core-SqlServer-2C3E50" alt="EF Core SQL Server" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white" alt="Bootstrap 5" />
  <img src="https://img.shields.io/badge/Chart.js-4.0-FF6384?logo=chart.js&logoColor=white" alt="Chart.js" />
</p>

---

## 📋 Tổng quan

Money Tracker là một ứng dụng web quản lý tài chính cá nhân được xây dựng bằng ASP.NET Core 8.0, cung cấp các tính năng quản lý thu chi, báo cáo tài chính và gợi ý AI thông minh với giao diện hiện đại và trải nghiệm người dùng tuyệt vời.

## ✨ Tính năng chính

### 🔐 Xác thực & Phân quyền
- ✅ Đăng nhập với Google OAuth2
- ✅ Đăng xuất an toàn
- ✅ Phân quyền User / Admin
- ✅ Onboarding cho người dùng mới
- ✅ Quản lý hồ sơ cá nhân: tên, email, avatar

### 👤 Tính năng cho Người dùng

#### 🏠 HomePage - Dashboard Hiện đại
- ✅ Giao diện ultra modern với glassmorphism effects
- ✅ Real-time clock và weather info
- ✅ Financial overview cards với 3D animations
- ✅ Quick stats bar (hôm nay, tuần này, trung bình)
- ✅ Smart notifications panel với AI alerts
- ✅ Quick actions với enhanced UI/UX

#### 📊 Dashboard & Báo cáo
- ✅ Trang tổng quan hiển thị tình hình thu/chi, số dư, biểu đồ nhanh
- ✅ Báo cáo tổng hợp hàng tháng
- ✅ Biểu đồ (chi tiêu theo danh mục, thu nhập theo nguồn, xu hướng theo tháng)
- ✅ Xuất báo cáo (PDF/Excel – optional)

#### 💰 Quản lý Chi tiêu (Expense Management)
- ✅ Thêm chi tiêu với giao diện hiện đại
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
- ✅ Danh mục mặc định cho người dùng mới

#### 🤖 AI Suggestions (Nâng cao)
- ✅ Xem gợi ý tài chính từ AI dựa trên lịch sử chi tiêu
- ✅ Gợi ý ngân sách cá nhân hóa (ví dụ: hạn chế chi cho ăn uống > 30% tổng thu nhập)
- ✅ Smart notifications với budget warnings

### 👨‍💼 Tính năng cho Admin

#### 👥 Quản lý Người dùng
- ✅ Xem danh sách người dùng
- ✅ Enable/Disable user (khóa/mở tài khoản)
- ✅ Xóa user
- ✅ Dashboard admin với thống kê tổng quan

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
- **Modern CSS** - Glassmorphism, 3D effects, animations

## 📁 Cấu trúc dự án

```
Expense-Manager/
├── db/
│   └── demo1_Expense.sql               # Script demo DB (tuỳ chọn)
└── MoneyTracker/
   └── MoneyTracker/
      ├── Controllers/                  # API Controllers
      │   ├── AuthController.cs
      │   ├── ExpenseController.cs
      │   ├── IncomeController.cs
      │   ├── CategoryController.cs
      │   ├── DashboardController.cs
      │   ├── OnboardingController.cs
      │   ├── ProfileController.cs
      │   └── AdminController.cs
      ├── Models/                       # Data Models
      │   ├── DTOs/                    # Data Transfer Objects
      │   ├── User.cs
      │   ├── Expense.cs
      │   ├── Income.cs
      │   ├── Category.cs
      │   ├── AiSuggestion.cs
      │   ├── Email.cs
      │   └── ExpenseManagerContext.cs
      ├── Services/                    # Business Logic
      │   ├── AuditService.cs
      │   ├── EmailService.cs
      │   ├── DefaultCategoryService.cs
      │   └── Interfaces/
      ├── Pages/                       # Razor Pages
      │   ├── HomePage.cshtml          # Modern dashboard
      │   ├── Login.cshtml             # Enhanced login
      │   ├── Onboarding.cshtml        # User onboarding
      │   ├── Dashboard.cshtml
      │   ├── Expenses.cshtml
      │   ├── Incomes.cshtml
      │   ├── Categories.cshtml
      │   ├── Profile.cshtml
      │   ├── AiSuggestions.cshtml
      │   ├── Admin/
      │   │   ├── Dashboard.cshtml
      │   │   └── Users.cshtml
      │   └── Shared/
      │       └── _Layout.cshtml
      ├── wwwroot/                     # Static files
      │   ├── css/
      │   │   └── site.css            # Core styles (inline CSS used)
      │   ├── js/
      │   └── lib/
      ├── Program.cs                   # DI, AuthN/AuthZ, routes
      ├── appsettings.json            # ConnectionString & Google OAuth
      └── MoneyTracker.csproj
```

## 🚀 Cài đặt và Chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019 hoặc mới hơn (Express cũng được)
- Visual Studio 2022 hoặc VS Code

### Cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd Expense-Manager/MoneyTracker/MoneyTracker
```

2. **Cấu hình Database**
- Cập nhật connection string trong `appsettings.json`
- Chạy script `db/demo1_Expense.sql` để tạo database và sample data

3. **Cấu hình Google OAuth**
- Tạo Google OAuth credentials tại [Google Cloud Console](https://console.cloud.google.com/)
- Cập nhật `ClientId` và `ClientSecret` trong `appsettings.json`
- Authorized redirect URI: `https://localhost:7249/signin-google`

4. **Cấu hình Email (Optional)**
- Cập nhật email settings trong `appsettings.json`

5. **Chạy ứng dụng**
```bash
# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet restore
dotnet run
```

Ứng dụng chạy tại: `https://localhost:7249`

## 🔧 Cấu hình

### Connection String & Google OAuth
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SQL;Initial Catalog=ExpenseManager;User ID=sa;Password=***;Trusted_Connection=True;Trust Server Certificate=True"
  },
  "GoogleAuth": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
}
```

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
- `POST /api/auth/google-login` - Đăng nhập Google
- `POST /api/auth/logout` - Đăng xuất
- `GET /api/auth/verify-token` - Verify JWT token

### Onboarding Endpoints
- `POST /api/onboarding/setup-categories` - Thiết lập danh mục mặc định

### Dashboard Endpoints
- `GET /api/dashboard/overview` - Lấy dữ liệu tổng quan
- `GET /api/dashboard/recent-transactions` - Giao dịch gần đây

### Expense Endpoints
- `GET /api/expense` - Lấy danh sách chi tiêu
- `GET /api/expense/{id}` - Lấy chi tiêu theo ID
- `POST /api/expense` - Tạo chi tiêu mới
- `PUT /api/expense/{id}` - Cập nhật chi tiêu
- `DELETE /api/expense/{id}` - Xóa chi tiêu

### Income Endpoints
- `GET /api/income` - Lấy danh sách thu nhập
- `POST /api/income` - Tạo thu nhập mới
- `PUT /api/income/{id}` - Cập nhật thu nhập
- `DELETE /api/income/{id}` - Xóa thu nhập

### Category Endpoints
- `GET /api/category` - Lấy danh sách danh mục
- `POST /api/category` - Tạo danh mục mới
- `PUT /api/category/{id}` - Cập nhật danh mục
- `DELETE /api/category/{id}` - Xóa danh mục

### Profile Endpoints
- `GET /api/profile` - Lấy thông tin profile
- `PUT /api/profile` - Cập nhật profile

### Admin Endpoints (Yêu cầu quyền Admin)
- `GET /api/admin/users` - Quản lý người dùng
- `GET /api/admin/statistics` - Thống kê hệ thống
- `PUT /api/admin/users/{id}/toggle-status` - Bật/tắt user
- `DELETE /api/admin/users/{id}` - Xóa user

## 🗓️ LỘ TRÌNH PHÁT TRIỂN (3–4 tuần – có thể điều chỉnh)

### 🥇 Tuần 1: Cấu hình dự án + Xác thực người dùng
🎯 **Mục tiêu**: Khởi tạo project, kết nối database, đăng nhập Google, quản lý người dùng

| Ngày | Công việc | Chi tiết |
|------|-----------|----------|
| **Ngày 1** | ✅ Khởi tạo dự án | - Tạo project ASP.NET Core Web App hoặc API<br>- Cài Entity Framework Core<br>- Cấu hình kết nối SQL Server |
| **Ngày 2** | 🧩 Tạo database + mô hình dữ liệu | - Bảng Users, Admins, Categories, Transactions, Budgets, Feedback, AiSuggestions |
| **Ngày 3** | 🔐 Google OAuth2 login | - Tích hợp đăng nhập bằng Google<br>- Lưu thông tin user lần đầu đăng nhập |
| **Ngày 4** | 👤 Quản lý thông tin cá nhân | - Trang xem/chỉnh sửa tên, avatar<br>- Trang "My Profile" |
| **Ngày 5** | 🛡️ Phân quyền | - Phân quyền User/Admin<br>- Admin truy cập dashboard, quản lý user |
| **Ngày 6-7** | 🧪 Kiểm thử & tối ưu | - Test toàn bộ đăng nhập & phân quyền<br>- Giao diện đăng nhập đẹp, đơn giản |

### 🥈 Tuần 2: Danh mục & Giao dịch (Core Feature)
🎯 **Mục tiêu**: Người dùng có thể thêm thu/chi, phân loại và xem lịch sử

| Ngày | Công việc | Chi tiết |
|------|-----------|----------|
| **Ngày 8** | 📁 Quản lý danh mục | - CRUD danh mục chi tiêu và thu nhập |
| **Ngày 9-10** | 💸 Giao dịch | - CRUD giao dịch (chi tiêu / thu nhập)<br>- Liên kết danh mục<br>- Lọc theo ngày, tháng |
| **Ngày 11** | 📅 Lịch sử giao dịch | - Trang "Transaction History"<br>- Tìm kiếm, sắp xếp, phân trang |
| **Ngày 12** | 📊 Tổng quan thu/chi | - Biểu đồ tổng thu – chi theo thời gian |
| **Ngày 13-14** | 🧪 Kiểm thử | - Viết unit test<br>- Tối ưu UX cho form nhập giao dịch |

### 🥉 Tuần 3: Hạn mức, báo cáo & AI
🎯 **Mục tiêu**: Giúp người dùng kiểm soát chi tiêu & nhận gợi ý thông minh

| Ngày | Công việc | Chi tiết |
|------|-----------|----------|
| **Ngày 15** | 🎯 Hạn mức chi tiêu | - CRUD hạn mức theo tháng / danh mục |
| **Ngày 16** | ⚠️ Cảnh báo vượt hạn mức | - Gửi cảnh báo khi chi vượt giới hạn |
| **Ngày 17-18** | 🤖 Gợi ý AI | - Tích hợp OpenAI API (hoặc mô hình ML nhẹ)<br>- Gợi ý giảm chi, phân tích thói quen |
| **Ngày 19** | 📈 Dashboard thống kê | - Biểu đồ tổng quan: chi tiêu theo tháng, danh mục |
| **Ngày 20-21** | 🧪 Kiểm thử + Demo nội bộ | - Test toàn hệ thống<br>- Giao diện dashboard đẹp mắt |

### 🏁 Tuần 4: Hoàn thiện & Triển khai
🎯 **Mục tiêu**: Làm sản phẩm hoàn chỉnh để báo cáo thực tập / demo

| Ngày | Công việc | Chi tiết |
|------|-----------|----------|
| **Ngày 22** | ⭐ Feedback | - Trang gửi phản hồi hệ thống |
| **Ngày 23** | 🛠️ Admin Panel | - Xem danh sách người dùng<br>- Xóa / khoá tài khoản |
| **Ngày 24** | 🧼 Bảo mật & xử lý lỗi | - Thêm middleware xử lý lỗi<br>- CORS, XSS, CSRF |
| **Ngày 25** | 📦 Deploy thử nội bộ | - Deploy lên IIS / Azure / Vercel |
| **Ngày 26-28** | 🧪 Test toàn hệ thống | - Viết tài liệu<br>- Tối ưu performance |
| **Ngày 29-30** | 📊 Hoàn thiện báo cáo | - Viết tài liệu hướng dẫn, README<br>- Chuẩn bị slide báo cáo thực tập |

## 🎨 Giao diện & UX

### Modern Design Features
- **Glassmorphism Effects**: Backdrop blur và semi-transparent elements
- **3D Animations**: Perspective transforms và depth effects  
- **Micro-interactions**: Smooth hover animations và transitions
- **Real-time Updates**: Live clock, weather info, notifications
- **Smart Notifications**: AI-powered alerts và warnings
- **Responsive Design**: Mobile-first approach với touch-friendly interface

### Color Palette
- **Primary**: Purple gradient (#667eea → #764ba2)
- **Success**: Emerald gradient (#10b981 → #34d399)
- **Danger**: Rose gradient (#f43f5e → #fb7185)
- **Warning**: Amber gradient (#f59e0b → #fbbf24)
- **Info**: Violet gradient (#8b5cf6 → #a78bfa)

## 🔒 Bảo mật

- JWT Token authentication
- Google OAuth2 integration
- Role-based authorization (User/Admin)
- Input validation và sanitization
- SQL injection protection với Entity Framework
- XSS protection
- CORS configuration
- Secure cookie settings

## 📝 Logging

Ứng dụng sử dụng Serilog để ghi log:
- Console logging cho development
- File logging (logs/moneytracker-{date}.txt)
- Database logging (audit trail)
- Structured logging với context
- Error tracking và performance monitoring

## 🧪 Testing

### Để test ứng dụng:
1. Chạy ứng dụng: `dotnet run`
2. Truy cập `/Login` để đăng nhập
3. Sử dụng Google OAuth để xác thực
4. Truy cập `/HomePage` để xem giao diện chính hiện đại
5. Test các tính năng: Expenses, Incomes, Categories, Profile

### Test Accounts
- Lần đầu đăng nhập Google → tự động tạo user với role `USER`
- Nâng quyền Admin: cập nhật cột `role` = `ADMIN` trong bảng `users`
- Trang quản trị: `/Admin/Dashboard` (chỉ Admin truy cập)

## 📊 Ảnh màn hình

> Thay ảnh thật khi sẵn sàng.

| Đăng nhập | HomePage Modern | Admin Dashboard |
|-----------|-----------------|-----------------|
| ![Login](https://via.placeholder.com/420x300?text=Login+Page) | ![HomePage](https://via.placeholder.com/420x300?text=Modern+HomePage) | ![Admin](https://via.placeholder.com/420x300?text=Admin+Dashboard) |

## 🛠️ Lệnh nhanh hữu ích

```bash
# Build project
dotnet build

# Run application
dotnet run

# Run with specific environment
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Restore packages
dotnet restore

# Format C# code
dotnet format

# Create migration (if needed)
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 Giấy phép

Dự án này được phát hành dưới MIT License — sử dụng thoải mái cho học tập và sản phẩm.

## 👥 Tác giả

- **MoneyTracker Development Team** - *Phát triển và thiết kế*
- **OJT Intern Team** - *Implementation và testing*

## 📞 Liên hệ & Hỗ trợ

- **Email**: support@moneytracker.com
- **Website**: https://moneytracker.com
- **GitHub Issues**: Để báo lỗi hoặc đề xuất tính năng mới

---

## 🚀 Tính năng nổi bật

### ✨ **Ultra Modern HomePage**
- **3D Effects**: Perspective transforms với hover animations
- **Glassmorphism**: Backdrop blur effects cho modern look
- **Real-time Info**: Live clock, weather, và notifications
- **Smart Stats**: Quick overview với animated counters
- **AI Notifications**: Intelligent budget warnings và suggestions

### 🎯 **Enhanced User Experience**
- **Onboarding Flow**: Guided setup cho người dùng mới
- **Default Categories**: 18 danh mục mặc định được thiết lập tự động
- **Responsive Design**: Perfect trên mọi device
- **Loading States**: Smooth animations và feedback
- **Error Handling**: Graceful error messages và recovery

### 🔧 **Technical Excellence**
- **Clean Architecture**: Separation of concerns với Services layer
- **Entity Framework**: Code-first approach với migrations
- **JWT Security**: Secure token-based authentication
- **Audit Logging**: Complete activity tracking
- **Performance**: Optimized queries và caching

---

**Lưu ý**: Đây là phiên bản demo với giao diện hiện đại và tính năng đầy đủ. Để sử dụng trong production, vui lòng cấu hình lại các settings bảo mật, database connection string và Google OAuth credentials.

**Nếu bạn cần CI/CD (GitHub Actions), Docker deployment, hoặc seeding admin mặc định, mở issue để thảo luận thêm.**