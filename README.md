# Expense Manager

> Ứng dụng theo dõi thu chi cá nhân/doanh nghiệp, đăng nhập Google, phân quyền Admin/User.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET-Razor%20Pages-5C2D91" alt="Razor Pages" />
  <img src="https://img.shields.io/badge/EF%20Core-SqlServer-2C3E50" alt="EF Core SQL Server" />
</p>

---

## Tính năng
- Đăng nhập bằng Google (OAuth2) và cookie session
- Quản lý hồ sơ cá nhân: tên, email, avatar
- Phân quyền: `ADMIN` / `USER`
- Trang Admin quản lý người dùng: bật/tắt, đổi role
- Quản lý danh mục, thu nhập và chi tiêu (mô hình sẵn trong DB)

## Kiến trúc & Công nghệ
- **Backend**: ASP.NET Core 8, Razor Pages, Minimal APIs
- **Auth**: `Microsoft.AspNetCore.Authentication.Google` + Cookie
- **Database**: SQL Server, Entity Framework Core
- **UI**: Bootstrap 5

## Cấu trúc thư mục
```
Expense-Manager/
  ├─ db/
  │  └─ demo1_Expense.sql               # Script demo DB (tuỳ chọn)
  └─ MyExpense/
     └─ MyExpense/
        ├─ Models/                      # EF Core models & DbContext
        ├─ Pages/                       # Razor Pages (Account, Admin, ...)
        ├─ wwwroot/                     # Static assets (css/js/lib)
        ├─ Program.cs                   # DI, AuthN/AuthZ, routes
        ├─ appsettings.json             # ConnectionString & Google OAuth
        └─ MyExpense.csproj
```

## Yêu cầu hệ thống
- .NET SDK 8.0+
- SQL Server (Express cũng được)

## Cài đặt & chạy
1) Sao chép mã nguồn
```bash
git clone <repo-url>
cd Expense-Manager/MyExpense/MyExpense
```

2) Cấu hình Connection String & Google OAuth trong `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DBDefault": "Data Source=YOUR_SQL;Initial Catalog=ExpenseManager;User ID=sa;Password=***;Trusted_Connection=True;Trust Server Certificate=True"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```
- Tạo OAuth Client trên Google Cloud → OAuth consent screen → Credentials → Web application
- Authorized redirect URI (mặc định): `https://localhost:5001/signin-google` và `http://localhost:5000/signin-google` (tự động bởi middleware Google)

3) Áp dụng database (nếu chưa có)
- Tạo database `ExpenseManager` và chạy script trong `db/demo1_Expense.sql` hoặc để EF làm việc với bảng đã scaffold sẵn.

4) Chạy ứng dụng
```bash
# Windows PowerShell (từ thư mục MyExpense/MyExpense)
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet restore
dotnet run
```
Ứng dụng chạy tại: `https://localhost:5001`

## Tài khoản & phân quyền
- Lần đầu đăng nhập Google → người dùng được tạo với role mặc định `USER`.
- Nâng quyền Admin: cập nhật cột `role` = `ADMIN` cho user trong bảng `users` (SQL) rồi đăng nhập lại.
- Trang quản trị: `/Admin/Users` (chỉ Admin truy cập).

## Ảnh màn hình
> Thay ảnh thật khi sẵn sàng.

| Đăng nhập | Admin Users |
|---|---|
| ![Login](https://via.placeholder.com/420x300?text=Login) | ![Admin](https://via.placeholder.com/420x300?text=Admin+Users) |

## Lệnh nhanh hữu ích
```bash
# Build
dotnet build

# Chạy
dotnet run

# Format C# (nếu cần)
dotnet format
```

## Bảo mật & cấu hình
- Không commit ClientSecret thật lên repo. Dùng `dotnet user-secrets` hoặc biến môi trường trong môi trường thật.
- Sản xuất: bật HTTPS, HSTS, và cập nhật `AllowedHosts` nếu cần.

## Giấy phép
MIT — sử dụng thoải mái cho học tập và sản phẩm.

---

Nếu bạn cần CI/CD (GitHub Actions) hoặc seeding admin mặc định, mở issue/trao đổi để mình thêm vào.
