# Money Tracker - Ứng dụng Quản lý Tài chính Cá nhân

Ứng dụng Money Tracker là một hệ thống quản lý tài chính cá nhân hoàn chỉnh với backend ASP.NET Core và frontend React TypeScript.

## 🚀 Tính năng chính

### Backend (ASP.NET Core Web API)
- ✅ **Clean Architecture** với Repository Pattern và Unit of Work
- ✅ **JWT Authentication** kết hợp Google OAuth2
- ✅ **Entity Framework Core** với SQL Server
- ✅ **Swagger/OpenAPI** documentation
- ✅ **Serilog** logging
- ✅ **Comprehensive API endpoints** cho tất cả tính năng

### Frontend (React TypeScript)
- ✅ **Modern React 18** với TypeScript
- ✅ **Material-UI (MUI)** cho giao diện đẹp
- ✅ **React Router** cho navigation
- ✅ **React Query** cho data fetching
- ✅ **Zustand** cho state management
- ✅ **Recharts** cho biểu đồ
- ✅ **Responsive design** cho mobile và desktop

### Tính năng nghiệp vụ
- 🔐 **Đăng nhập bằng Google OAuth2**
- 📊 **Dashboard** với biểu đồ thu chi
- 💰 **Quản lý giao dịch** (thu nhập/chi tiêu)
- 📂 **Quản lý danh mục** (hệ thống + cá nhân)
- 💳 **Quản lý ngân sách** với cảnh báo
- 📈 **Báo cáo và thống kê** chi tiết
- 🤖 **AI Suggestions** thông minh
- 📤 **Xuất báo cáo** Excel/PDF

## 🛠️ Công nghệ sử dụng

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Google OAuth2
- Swagger/OpenAPI
- Serilog
- AutoMapper
- ClosedXML (Excel export)

### Frontend
- React 18
- TypeScript
- Material-UI (MUI)
- React Router DOM
- React Query
- Zustand
- Recharts
- Axios
- React Hook Form

## 📁 Cấu trúc dự án

```
Expense-Manager/
├── MoneyTracker/                    # Backend ASP.NET Core
│   ├── MoneyTracker/
│   │   ├── Controllers/            # API Controllers
│   │   ├── Services/               # Business Logic Services
│   │   ├── Core/                   # Domain Interfaces
│   │   ├── Infrastructure/         # Repository Implementation
│   │   ├── Data/                   # DbContext
│   │   ├── Models/                 # Entity Models
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   └── Program.cs              # Application Entry Point
│   └── MoneyTracker.sln
├── money-tracker-frontend/          # Frontend React
│   ├── src/
│   │   ├── components/             # React Components
│   │   ├── services/               # API Services
│   │   ├── store/                  # State Management
│   │   ├── types/                  # TypeScript Types
│   │   └── utils/                  # Utility Functions
│   └── package.json
└── README.md
```

## 🚀 Hướng dẫn chạy ứng dụng

### 1. Chuẩn bị môi trường

**Yêu cầu:**
- .NET 8 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Node.js 16+ và npm
- Visual Studio 2022 hoặc VS Code

### 2. Chạy Backend

```bash
# Di chuyển vào thư mục backend
cd MoneyTracker/MoneyTracker

# Restore packages
dotnet restore

# Cập nhật database (nếu cần)
dotnet ef database update

# Chạy ứng dụng
dotnet run
```

Backend sẽ chạy tại: `https://localhost:7000`
Swagger UI: `https://localhost:7000`

### 3. Chạy Frontend

```bash
# Di chuyển vào thư mục frontend
cd money-tracker-frontend

# Cài đặt dependencies
npm install

# Tạo file environment
echo "REACT_APP_API_URL=https://localhost:7000/api" > .env.local
echo "REACT_APP_GOOGLE_CLIENT_ID=294978301369-6b2q7e4pdo503srrn6vuvv7dppqntuuv.apps.googleusercontent.com" >> .env.local

# Chạy ứng dụng
npm start
```

Frontend sẽ chạy tại: `http://localhost:3000`

### 4. Cấu hình Database

Database sẽ được tạo tự động khi chạy lần đầu. Nếu cần tạo thủ công:

```sql
-- Chạy script Database_Schema.sql trong SQL Server Management Studio
-- Hoặc sử dụng Entity Framework migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🔧 Cấu hình

### Backend Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NHOTUNG\\SQLEXPRESS;Initial Catalog=ExpenseManager;..."
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "MoneyTracker",
    "Audience": "MoneyTrackerUsers",
    "ExpiryMinutes": 60
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

### Frontend Configuration (.env.local)
```
REACT_APP_API_URL=https://localhost:7000/api
REACT_APP_GOOGLE_CLIENT_ID=your-google-client-id
```

## 📚 API Documentation

Sau khi chạy backend, truy cập Swagger UI tại `https://localhost:7000` để xem tài liệu API đầy đủ.

### Các endpoint chính:
- `POST /api/auth/google-login` - Đăng nhập Google
- `GET /api/users/me` - Lấy thông tin user
- `GET /api/transactions` - Lấy danh sách giao dịch
- `POST /api/transactions` - Tạo giao dịch mới
- `GET /api/categories` - Lấy danh mục
- `GET /api/budgets` - Lấy ngân sách
- `GET /api/reports/summary` - Báo cáo tổng quan
- `GET /api/ai/suggestions` - Gợi ý AI

## 🎯 Sử dụng ứng dụng

1. **Đăng nhập**: Sử dụng Google OAuth2
2. **Dashboard**: Xem tổng quan tài chính với biểu đồ
3. **Giao dịch**: Thêm, sửa, xóa thu nhập/chi tiêu
4. **Danh mục**: Quản lý danh mục cá nhân
5. **Ngân sách**: Tạo và theo dõi ngân sách
6. **Báo cáo**: Xem báo cáo chi tiết và xuất file
7. **AI Suggestions**: Nhận gợi ý thông minh

## 🔒 Bảo mật

- JWT token authentication
- Google OAuth2 integration
- CORS configuration
- Input validation
- SQL injection protection
- XSS protection

## 📱 Responsive Design

Ứng dụng được thiết kế responsive, hoạt động tốt trên:
- Desktop (1200px+)
- Tablet (768px - 1199px)
- Mobile (< 768px)

## 🚀 Deployment

### Backend
- Deploy lên Azure App Service hoặc IIS
- Cấu hình connection string cho production database
- Cập nhật Google OAuth2 redirect URLs

### Frontend
- Build production: `npm run build`
- Deploy lên Netlify, Vercel hoặc Azure Static Web Apps
- Cấu hình environment variables

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

## 📄 License

MIT License

## 📞 Hỗ trợ

Nếu có vấn đề, vui lòng tạo issue trên GitHub hoặc liên hệ qua email.

---

**Chúc bạn sử dụng ứng dụng Money Tracker hiệu quả! 💰📊**