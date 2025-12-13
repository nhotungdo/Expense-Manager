Kiểm tra các chức năng dưới đây đã hoàn thiện bao nhiêu phần trăm rồi của dự án.Các chức năng được chia từ cơ bản (Core) đến nâng cao (Advanced) dựa trên cấu trúc bảng và các trường dữ liệu.

### 1. Nhóm Chức năng Cơ bản (Core Features)

Đây là các chức năng nền tảng bắt buộc phải có để ứng dụng hoạt động.

* **Quản lý Tài khoản & Xác thực người dùng (Auth):**
    * Đăng ký/Đăng nhập (Hỗ trợ Google Login qua `GoogleId`).
    * Quản lý hồ sơ cá nhân: Avatar, Ngày sinh, Địa chỉ, Giới tính.
    * Cài đặt cá nhân hóa: Đổi ngôn ngữ, Đơn vị tiền tệ mặc định, Múi giờ, Giao diện Sáng/Tối (Light/Dark theme).
    * Bảo mật: Xác thực 2 lớp (2FA), Khóa tài khoản khi đăng nhập sai nhiều lần.
    * Quy trình Onboarding: Hướng dẫn người dùng mới thiết lập hồ sơ ban đầu (Thu nhập, chi tiêu, mục tiêu).

* **Quản lý Ví/Tài khoản tiền (Wallet Management):**
    * Tạo nhiều loại ví: Tiền mặt, Ngân hàng, Ví điện tử, Thẻ tín dụng, Tiết kiệm.
    * Theo dõi số dư: Số dư ban đầu và Số dư hiện tại.
    * Tùy chỉnh giao diện ví: Chọn Icon và Màu sắc để dễ nhận diện.

* **Quản lý Danh mục (Categories):**
    * Phân loại Thu/Chi.
    * Hỗ trợ danh mục đa cấp (Danh mục cha - con).
    * Danh mục mặc định của hệ thống & Danh mục riêng do người dùng tạo.

* **Quản lý Giao dịch (Transactions):**
    * Thêm mới giao dịch: Thu, Chi, Chuyển tiền giữa các ví (Transfer).
    * Ghi chú, đính kèm ảnh hóa đơn (`AttachmentUrl`).
    * Tự động cập nhật số dư ví khi có giao dịch phát sinh (Trigger `tr_Transactions_UpdateAccountBalance`).

### 2. Nhóm Chức năng Trung cấp (Intermediate Features)

Các chức năng giúp người dùng kiểm soát tài chính tốt hơn.

* **Quản lý Ngân sách (Budgets):**
    * Thiết lập hạn mức chi tiêu theo Danh mục hoặc theo Ví.
    * Chu kỳ ngân sách linh hoạt: Tuần, Tháng, Năm hoặc Tùy chỉnh ngày.

* **Sổ Tiết kiệm & Mục tiêu (Savings Goals):**
    * Tạo mục tiêu tiết kiệm (Mua nhà, Mua xe...) với số tiền và ngày đích.
    * Theo dõi tiến độ hoàn thành (Thanh trạng thái, màu sắc).
    * Ghi nhận các giao dịch nạp tiền vào mục tiêu.

* **Giao dịch Định kỳ (Recurring/Scheduled):**
    * Lên lịch tự động cho các khoản thu chi cố định (Tiền nhà, Tiền lương, Netflix...).
    * Tần suất đa dạng: Hàng ngày, tuần, tháng, năm.

* **Quản lý Nợ vay (Debts):**
    * Ghi chép sổ nợ: "Tôi nợ ai" và "Ai nợ tôi".
    * Tính lãi suất nợ (Interest Rate).
    * Theo dõi lịch sử trả nợ từng phần.

* **Báo cáo & Thống kê (Reports):**
    * Dashboard tổng quan: Tổng thu, tổng chi, số dư.
    * Biểu đồ xu hướng dòng tiền theo tháng.
    * Phân tích chi tiêu theo danh mục.
    * Xuất báo cáo ra file (PDF/Excel).

### 3. Nhóm Chức năng Nâng cao (Advanced Features)

Các chức năng tạo nên sự khác biệt, thông minh và mang tính cộng đồng.

* **Công nghệ OCR (Quét hóa đơn):**
    * Trích xuất thông tin tự động từ hình ảnh hóa đơn (trường `OcrText` trong bảng Transactions) giúp nhập liệu nhanh.

* **Quản lý Đầu tư (Investments):**
    * Theo dõi danh mục đầu tư đa dạng: Cổ phiếu, Crypto, Vàng, Quỹ.
    * Ghi nhận giá mua và giá trị hiện tại (lời/lỗ).

* **Chi tiêu Nhóm (Group Expenses - Splitwise style):**
    * Tạo nhóm chi tiêu (Du lịch, Ăn uống, Tiền nhà trọ).
    * Thêm thành viên vào nhóm.
    * Chia tiền hóa đơn: Ai trả tiền, chia cho những ai (Splits).

* **Ví Chia sẻ (Shared Wallets):**
    * Khác với chi tiêu nhóm, đây là chia sẻ quyền truy cập trực tiếp vào một ví cụ thể (Ví dụ: Ví gia đình).
    * Phân quyền chi tiết: Chỉ xem, Được thêm giao dịch, hoặc Toàn quyền.

* **Kết nối Ngân hàng (Bank Sync):**
    * Liên kết tài khoản ngân hàng thực tế để đồng bộ giao dịch tự động (thông qua Provider, AccessToken).

* **Đa tiền tệ (Multi-currency):**
    * Hỗ trợ giao dịch với nhiều loại tiền tệ khác nhau.
    * Cập nhật tỷ giá hối đoái tự động.

* **Trợ lý ảo AI (AI Advisor):**
    * Hệ thống gợi ý tài chính thông minh dựa trên dữ liệu chi tiêu của người dùng.

* **Hệ thống Hội viên & Thanh toán (Subscription & Monetization):**
    * Cung cấp các gói dịch vụ (Gói Miễn phí, Cơ bản, Chuyên nghiệp, Doanh nghiệp).
    * Quản lý đăng ký định kỳ, tự động gia hạn.
    * Tích hợp cổng thanh toán để mua gói nâng cấp.

* **Hệ thống Thông báo (Notifications):**
    * Cảnh báo tài chính (Vượt ngân sách, nhắc nợ).
    * Thông báo đẩy (Push) và Email.
    * Audit Log: Ghi lại lịch sử hoạt động để bảo mật và tra soát.

