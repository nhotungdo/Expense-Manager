# Danh Sách Các Chức Năng Dự Án MoneyTracker

Dưới đây là danh sách các chức năng chính đang được triển khai và hoàn thiện trong dự án **MoneyTracker - Ứng dụng Quản lý Tài chính Cá nhân**.

## 1. Hệ Thống & Bảo Mật (System & Security)
*   **Xác thực người dùng (Auth):**
    *   Đăng ký, Đăng nhập (Email/Password).
    *   Tích hợp Google OAuth 2.0.
    *   Xác thực 2 yếu tố (2FA/OTP).
    *   Quản lý phiên đăng nhập (Session management) và Refresh Token.
*   **Onboarding:** Quy trình thiết lập ban đầu cho người dùng mới.
*   **Quản lý Profile:** Cập nhật thông tin cá nhân, thay đổi mật khẩu, cài đặt bảo mật.
*   **Thông báo (Notifications):** Hệ thống thông báo thời gian thực (Real-time) qua SignalR và Email.

## 2. Quản Lý Tài Chính Cơ Bản (Core Finance)
*   **Quản lý Tài khoản/Ví (Accounts/Wallets):**
    *   Hỗ trợ nhiều loại tài khoản: Tiền mặt, Ngân hàng, Thẻ tín dụng, Ví điện tử.
    *   Theo dõi số dư thời gian thực.
*   **Quản lý Giao dịch (Transactions):**
    *   Ghi chép Thu nhập (Income), Chi tiêu (Expense), Chuyển khoản (Transfer).
    *   Phân loại theo Danh mục (Categories).
    *   Giao dịch định kỳ (Scheduled Transactions).
*   **Quản lý Ngân sách (Budgets):** Thiết lập hạn mức chi tiêu theo tháng/tuần và cảnh báo khi sắp vượt mức.
*   **Quản lý Nợ (Debts):** Theo dõi các khoản vay/nợ và lịch sử trả nợ.
*   **Mục tiêu tiết kiệm (Savings Goals):** Thiết lập và theo dõi tiến độ tiết kiệm cho các mục tiêu cụ thể.

## 3. Tính Năng Nâng Cao & AI
*   **Phân tích AI (Gemini AI):**
    *   Phân tích xu hướng chi tiêu.
    *   Dự báo tài chính tương lai.
    *   Tư vấn tài chính cá nhân hóa qua Chatbot AI.
*   **Nhận diện hóa đơn (OCR):** Tự động trích xuất thông tin từ ảnh chụp hóa đơn để tạo giao dịch.
*   **Tự động hóa (Automation):** Thiết lập các quy tắc tự động phân loại hoặc xử lý giao dịch.
*   **Quản lý Đầu tư (Investments):** Theo dõi danh mục đầu tư (Chứng khoán, Crypto, Vàng...).
*   **Theo dõi Giá trị tài sản ròng (Net Worth):** Tổng hợp tài sản và nợ để tính toán giá trị tài sản ròng.

## 4. Tính Năng Nhóm & Xã Hội (Social & Group)
*   **Chi tiêu nhóm (Group Expense):**
    *   Tạo nhóm chi tiêu chung (gia đình, bạn bè).
    *   Chia sẻ hóa đơn (Split bill).
    *   Tính toán công nợ tự động giữa các thành viên.
*   **Bạn bè (Friendship):** Kết nối với người dùng khác.
*   **Chat Real-time:** Trò chuyện trực tiếp trong nhóm chi tiêu (SignalR).
*   **Chia sẻ tài khoản:** Cho phép người khác xem hoặc quản lý chung một tài khoản/ví.

## 5. Báo Cáo & Xuất Dữ Liệu
*   **Báo cáo trực quan (Reporting):**
    *   Biểu đồ chi tiêu theo danh mục, thời gian (Chart.js).
    *   Phân tích dòng tiền (Cash flow).
*   **Xuất báo cáo (Export):** Hỗ trợ xuất dữ liệu ra các định dạng Excel, PDF, CSV.

## 6. Thanh Toán & Gói Dịch Vụ
*   **Thanh toán QR (EMV QR):** Thư viện hỗ trợ tạo mã QR thanh toán chuẩn EMV.
*   **Cổng thanh toán VNPay:** Tích hợp thanh toán trực tuyến cho các gói dịch vụ.
*   **Gói dịch vụ (Subscription):** Quản lý các gói Free, Premium, Professional với các tính năng giới hạn khác nhau.

## 7. Công Nghệ Sử Dụng (Tech Stack)
*   **Backend:** .NET 8, ASP.NET Core Razor Pages.
*   **Database:** SQL Server, Entity Framework Core.
*   **Real-time:** SignalR.
*   **AI:** Google Gemini API.
*   **Thư viện hỗ trợ:** Tesseract (OCR), Chart.js, MailKit, VNPay SDK.

---
*Cập nhật lần cuối: 16/05/2026*
