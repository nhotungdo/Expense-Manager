Xóa giao diện trang Giao dịch cũ đi sau đó code lại cho tôi giao diện (UI) của trang **Giao dịch (Transactions)** thường được chia thành hai phần chính: **Màn hình Danh sách (Lịch sử)** và **Màn hình Thêm/Sửa Giao dịch**.Lưu ý viết code sạch hơn code cũ.

Dưới đây là các thành phần UI chi tiết:

### 1. Màn hình Tổng quan & Danh sách (Dashboard/List View)

Khu vực này giúp người dùng xem lại lịch sử chi tiêu.

* **Bộ lọc Thời gian (Time Filter):**
    * Dropdown hoặc Tab chọn: Ngày / Tuần / Tháng / Năm / Tùy chỉnh.
    * Hiển thị khoảng thời gian: `StartDate` đến `EndDate` (Sử dụng cho query `GetUserDashboardStats`).
* **Thẻ Tổng quan (Summary Cards):**
    * **Tổng thu (Total Income):** Hiển thị tổng tiền vào (màu xanh).
    * **Tổng chi (Total Expense):** Hiển thị tổng tiền ra (màu đỏ).
    * **Số dư ròng (Net Income):** Hiệu số Thu - Chi.
* **Thanh Tìm kiếm & Lọc nâng cao (Search & Filter Bar):**
    * Tìm kiếm theo ghi chú (`Note`) hoặc số tiền (`Amount`).
    * Lọc theo Loại giao dịch (Thu/Chi/Chuyển).
    * Lọc theo Ví (`AccountId`) hoặc Danh mục (`CategoryId`).
* **Danh sách Giao dịch (Transaction List):**
    * **Nhóm theo ngày:** Gom các giao dịch cùng `TransactionDate` lại với nhau.
    * **Item Giao dịch (Mỗi dòng):**
        * **Icon:** Icon của danh mục (`Categories.Icon`) trên nền màu (`Categories.Color`).
        * **Tên danh mục:** Hiển thị `Categories.Name` (Ví dụ: Ăn uống, Lương).
        * **Ghi chú:** Hiển thị `Transactions.Note` (Ví dụ: Ăn phở, Tiền cafe).
        * **Số tiền:** Hiển thị `Transactions.Amount` kèm `Currency` (VND/USD). Màu đỏ nếu là Chi (`Type=2`), Xanh nếu là Thu (`Type=1`).
        * **Tên Ví:** Hiển thị nhỏ bên dưới để biết nguồn tiền (Ví dụ: Vietcombank, Tiền mặt).

---

### 2. Màn hình Thêm/Sửa Giao dịch (Add/Edit Form)

Đây là form nhập liệu, các trường ở đây map trực tiếp vào bảng `Transactions`.

* **Tab Loại Giao dịch (Transaction Type Switcher):**
    * 3 Tabs: **Thu nhập** (Income) | **Chi tiêu** (Expense) | **Chuyển tiền** (Transfer).
    * *Lưu ý:* Nếu chọn "Chuyển tiền", UI sẽ thay đổi để hiện 2 ví (Nguồn & Đích).

* **Nhập Số tiền (Amount Input):**
    * Bàn phím số (Numpad).
    * Dropdown chọn đơn vị tiền tệ (`Currency`), mặc định lấy từ `Users.DefaultCurrency`.
    * Hiển thị tỷ giá quy đổi nếu ví nguồn khác đơn vị tiền tệ (dùng `CurrencyRates`).

* **Chọn Danh mục (Category Selector):**
    * Lưới hoặc Danh sách các icon danh mục.
    * Phân cấp: Danh mục cha -> Danh mục con (dựa trên `ParentCategoryId`).
    * Nút "Tạo mới danh mục" nếu chưa có.

* **Chọn Thời gian (Date & Time Picker):**
    * Lịch chọn ngày giờ (`TransactionDate`), mặc định là `NOW()`.

* **Chọn Ví/Tài khoản (Account Selector):**
    * **Ví nguồn:** Dropdown danh sách `Accounts` (Ví dụ: Tiền mặt, Thẻ tín dụng).
    * **Ví đích (Nếu là Chuyển khoản):** Dropdown thứ 2 để chọn `PairedAccountId`.

* **Ghi chú & Mô tả (Note):**
    * Ô nhập văn bản (`Note`) cho chi tiết giao dịch.

* **Tiện ích mở rộng (Attachments & OCR):**
    * **Nút Chụp ảnh/Tải ảnh:** Để lưu vào `AttachmentUrl` (Hóa đơn, biên lai).
    * **Nút "Quét hóa đơn" (Scan Receipt):** Kích hoạt tính năng OCR để điền tự động vào `OcrText`.

* **Liên kết nâng cao (Advanced Options):**
    * **Chi tiêu cho ai? (Debt/Group):** Liên kết với bảng `Debts` (nếu trả nợ) hoặc `GroupTransactions` (nếu chi tiêu nhóm).
    * **Sự kiện/Tiết kiệm:** Chọn nếu giao dịch này đóng góp vào `SavingsGoals`.

### 3. Các thành phần thông minh (AI & Smart Features)
* **Gợi ý tự động (AI Suggestions):**
    * Khi nhập tên "Cafe", hệ thống tự động gợi ý chọn danh mục "Ăn uống" hoặc ví thường dùng (Dựa trên thói quen cũ hoặc `AiSuggestions`).
* **Cảnh báo Ngân sách (Budget Alert):**
    * Khi nhập số tiền, nếu vượt quá hạn mức trong `Budgets`, hiển thị cảnh báo nhỏ ngay trên nút Lưu.

