Đề xuất phong cách thiết kế **"Clean Bento Fintech"** kết hợp với một chút **Glassmorphism** (hiệu ứng kính mờ) để tạo điểm nhấn hiện đại.

Dưới đây là gợi ý chi tiết cho hệ thống UI:

### 1. Bảng màu (Color Palette)

Cần phân biệt rõ ràng giữa các trạng thái tài chính và tính năng AI.

* **Màu chủ đạo (Primary):** `Deep Royal Blue` (#2563EB) hoặc `Violet` (#7C3AED). Màu này dùng cho các nút chính, thanh điều hướng và nhận diện thương hiệu.
* **Màu dòng tiền (Functional):**
* **Thu (Income):** `Emerald Green` (#10B981) - Tạo cảm giác tích cực, an toàn.
* **Chi (Expense):** `Rose Red` (#F43F5E) - Màu đỏ dịu, không quá gắt để tránh gây stress cho người dùng, nhưng đủ để cảnh báo.


* **Màu AI/Smart Features:** `Gradient Purple to Blue` (ví dụ: từ #6366f1 đến #a855f7). Dùng cho các nút "Phân tích AI", "Gợi ý thông minh" hoặc các widget liên quan đến bảng `AiSuggestions`.
* **Nền (Background):**
* Light Mode: `#F3F4F6` (Xám rất nhạt) thay vì trắng tinh để đỡ mỏi mắt.
* Dark Mode: `#111827` (Xám than chì) thay vì đen tuyền.



### 2. Typography & Iconography

* **Font chữ:** Sử dụng các font Sans-serif hiện đại, tròn trịa và dễ đọc số liệu như **Inter**, **Nunito Sans**, hoặc **Be Vietnam Pro** (hỗ trợ tiếng Việt rất tốt).
* **Số liệu:** Khi hiển thị số tiền (`Amount`), hãy dùng font **Monospace** hoặc thiết lập `font-feature-settings: 'tnum'` để các con số thẳng hàng nhau, dễ so sánh.
* **Icon:** Dùng bộ icon **Rounded** hoặc **Duotone** (2 màu) để giao diện trông mềm mại hơn (ví dụ: thư viện *Phosphor Icons* hoặc *Heroicons*).

### 3. Cấu trúc Layout (Bento Grid)

Dữ liệu của bạn rất nhiều (Ví, Ngân sách, Nợ, Đầu tư...), hãy sử dụng bố cục dạng lưới **Bento Grid** (giống giao diện widget của iOS hoặc Windows 11).

* **Các thẻ (Card):** Mỗi chức năng là một khối bo tròn (`border-radius: 16px` hoặc `20px`).
* **Hiệu ứng:** Sử dụng đổ bóng nhẹ (`box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1)`) để các khối nổi lên khỏi nền.

### 4. Chi tiết từng trang chức năng

#### A. Dashboard (Trang chủ)

Dựa trên `GetUserDashboardStats` và các View thống kê:

* **Header:** Lời chào + Avatar + Nút thông báo (có chấm đỏ nếu bảng `Notifications` có `IsRead=0`).
* **Total Balance Card:** Hiển thị tổng tài sản to, rõ ràng. Nền dùng Gradient chủ đạo. Có nút ẩn/hiện số dư (hình con mắt).
* **AI Insight Widget (Quan trọng):** Một thẻ đặc biệt có viền Gradient chuyển màu. Hiển thị text từ bảng `AiSuggestions`.
* *Ví dụ:* "💡 Gemini nhận thấy bạn đã chi quá 20% cho 'Ăn uống' so với tháng trước."


* **Quick Actions:** Các nút tròn to để thao tác nhanh: "Thêm giao dịch", "Quét hóa đơn (OCR)", "Chuyển tiền".

#### B. Trang Giao dịch (Transactions List)

* **Danh sách:** Group giao dịch theo Ngày (`TransactionDate`).
* **Row Item:**
* Bên trái: Icon danh mục (lấy `Icon` và `Color` từ bảng `Categories`) đặt trong hình tròn nền nhạt.
* Giữa: Tên danh mục + Ghi chú (`Note`).
* Bên phải: Số tiền. Màu xanh (+), màu đỏ (-).


* **Filter:** Thanh ngang trên cùng để lọc theo Ví (`AccountId`) hoặc Thời gian.

#### C. Trang Thêm Giao dịch (Add Transaction)

* **Input Số tiền:** Phải thật to và rõ ràng, đặt ngay trên cùng.
* **Bàn phím ảo (Mobile):** Nếu làm app mobile, hãy tự thiết kế bàn phím số riêng có nút "Lưu" tiện lợi.
* **Category Selector:** Dạng lưới icon grid thay vì dropdown list để chọn nhanh hơn.
* **OCR Button:** Nút camera nổi bật bên cạnh số tiền. Khi chụp xong, animation "quét" chạy qua ảnh và tự điền số liệu vào form.

#### D. Trang Ví & Ngân sách (Wallets & Budgets)

* **Ví:** Thiết kế mô phỏng thẻ ngân hàng thật (Bank Card UI). Hiển thị Logo ngân hàng/Ví ở góc. Nền thẻ lấy theo màu `Color` trong bảng `Accounts`.
* **Thanh Ngân sách:** Dùng **Progress Bar**.
* Màu xanh khi < 50%.
* Màu vàng khi 50-80%.
* Màu đỏ khi > 80% (Cảnh báo).
* Hiển thị text: "Còn lại 500k / 2tr".



#### E. Trang Báo cáo (Reports)

* **Biểu đồ:**
* Dùng **Donut Chart** (Biểu đồ tròn rỗng giữa) cho cơ cấu chi tiêu.
* Dùng **Spline Area Chart** (Biểu đồ đường cong có tô màu nền dưới) cho biến động số dư theo thời gian (dữ liệu từ `GetMonthlyTrends`).


* **Tương tác:** Khi rê chuột hoặc chạm vào biểu đồ, hiển thị Tooltip chi tiết số tiền.

### 5. Yếu tố UX "Thông minh" (Smart Features)

Để làm nổi bật yếu tố AI Gemini:

* **Skeleton Loading:** Khi đang chờ API Gemini phân tích, đừng dùng vòng xoay loading nhàm chán. Hãy dùng hiệu ứng **Shimmer** (ánh sáng chạy qua khung xám) tạo cảm giác dữ liệu đang được xử lý.
* **Chat Interface:** Nếu có tính năng hỏi đáp với AI, giao diện nên giống iMessage hoặc ChatGPT nhưng nền tin nhắn của AI có màu gradient nhẹ để phân biệt với người dùng.

### 6. Animation (Hiệu ứng chuyển động)

* **Micro-interactions:**
* Khi bấm "Lưu giao dịch": Nút chuyển thành dấu tích xanh ✅.
* Khi xóa danh mục: Hiệu ứng trượt sang trái (Swipe).


* **Confetti:** Khi người dùng hoàn thành một `SavingsGoal` (Mục tiêu tiết kiệm), hãy bắn pháo giấy chúc mừng.

