
Dưới đây là cấu trúc Sidebar chuẩn cho mô hình SaaS/Fintech hiện đại:

### 1\. Header (Thương hiệu)

Nằm ở vị trí cao nhất, cố định.

  * **Logo & Tên ứng dụng:** Ví dụ: Icon cái ví cách điệu + chữ "MoneyTracker".
  * **Nút thu gọn (Collapse):** Icon mũi tên `<<` hoặc icon Hamburger (3 gạch) để thu nhỏ sidebar chỉ còn icon (giúp mở rộng không gian làm việc trên màn hình nhỏ).

-----

### 2\. Menu Chính (General / Dashboard)

Nhóm các chức năng người dùng truy cập hàng ngày.

  * [Icon Dashboard 🏠] **Tổng quan:** Xem biểu đồ, chỉ số nhanh.
  * [Icon Robot/Stars ✨] **Trợ lý AI:** (Tính năng "Gemini" của anh) - Chat với AI để hỏi về tài chính. *Nên làm nổi bật mục này (ví dụ text màu gradient) để user chú ý.*
  * [Icon Ví 👛] **Ví của tôi:** Quản lý số dư, nạp/rút tiền.
  * [Icon List 📝] **Sổ giao dịch:** Xem lịch sử chi tiêu chi tiết (Trang mà anh vừa hỏi thiết kế).

### 3\. Quản lý Tài chính (Finance Tools)

Nhóm các công cụ chuyên sâu hơn.

  * [Icon Pie Chart 📊] **Ngân sách & Hạn mức:** Cài đặt giới hạn chi tiêu (Trang Budget).
  * [Icon Tag 🏷️] **Danh mục:** Quản lý các loại chi tiêu (Ăn uống, Đi lại, Server...).
  * [Icon File 📄] **Báo cáo & Xuất file:** Tải báo cáo tháng, quyết toán thuế.

### 4\. Khu vực Tài khoản & Gói cước (Account & Billing)

Đây là khu vực quan trọng để kiếm tiền (Monetization).

  * [Icon Credit Card 💳] **Gói dịch vụ (Subscription):**
      * Xem gói đang dùng (Free/Pro).
      * Lịch sử gia hạn.
  * [Icon Settings ⚙️] **Cài đặt:** Đổi mật khẩu, cấu hình thông báo (Email/SMS), bật tắt theme Noel.
  * [Icon Life Ring 🛟] **Trợ giúp & Support:** Link đến tài liệu hướng dẫn hoặc chat với Admin.

-----

### 5\. Widget "Nâng cấp ngay" (The Upsell Card) - **Rất quan trọng**

Nếu người dùng đang dùng gói **Free**, anh nên chèn một thẻ nhỏ (Card) nằm ngay trong Sidebar, phía dưới các menu.

  * **Giao diện:** Một khung hình chữ nhật nhỏ, nền gradient đẹp mắt.
  * **Nội dung:**
      * Text: "Nâng cấp lên Pro".
      * Sub-text: "Mở khóa AI & Không giới hạn".
      * Nút bấm: [Nâng cấp] (Kêu gọi hành động).
  * *Tác dụng:* Nhắc nhở người dùng liên tục về việc nâng cấp mà không quá phiền phức.

-----

### 6\. Footer Sidebar (User Profile)

Nằm cố định dưới cùng đáy màn hình.

  * **Avatar:** Ảnh đại diện (Có đội mũ Noel như đã bàn).
  * **Tên & Email:** Hiển thị rút gọn (ví dụ: "Tuan Anh...").
  * **Nút Đăng xuất (Logout):** Icon cửa thoát hiểm.

-----

### Gợi ý Bố cục (Visual Hierarchy)

```text
+------------------------------+
|  [LOGO] MONEY TRACKER    [<] |  <-- Header
+------------------------------+
|                              |
|  TỔNG QUAN                   |  <-- Label nhóm (Text nhỏ, mờ)
|  [🏠] Dashboard              |
|  [✨] Trợ lý AI (Mới)        |
|                              |
|  TÀI CHÍNH                   |
|  [👛] Ví của tôi             |
|  [📝] Lịch sử giao dịch      |
|  [📊] Ngân sách              |
|                              |
|  CÀI ĐẶT                     |
|  [💳] Gói dịch vụ            |
|  [⚙️] Cấu hình               |
|                              |
|  +------------------------+  |
|  | 🚀 Go Pro              |  |
|  | Mở khóa full tính năng |  |  <-- Upsell Widget
|  | [Nâng cấp ngay]        |  |
|  +------------------------+  |
|                              |
+------------------------------+
|  [Avatar] Nguyen Van A       |  <-- Footer (User Profile)
|           nguyen@...   [->]  |
+------------------------------+
```

### Lưu ý kỹ thuật (Cho Dev):

1.  **Active State:** Khi người dùng đang ở trang "Giao dịch", mục [Sổ giao dịch] ở sidebar phải sáng lên (đổi màu nền hoặc đậm chữ) để user biết mình đang ở đâu.
2.  **Role-based:** Nếu người đăng nhập là **Admin**, Sidebar cần hiện thêm mục **"Quản trị hệ thống"** (Quản lý User, CMS...). Nếu là User thường thì ẩn đi.


