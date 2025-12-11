Trang **"Quản lý Ngân sách" (Budget Management)** khác với trang "Ví của tôi". Nếu "Ví" là nơi chứa tiền và lịch sử nạp/rút, thì "Ngân sách" là nơi **kiểm soát chi tiêu** và **cài đặt giới hạn** để tránh việc người dùng bị "sốc" khi nhận hóa đơn cuối tháng (Bill Shock).

Dưới đây là mô tả chi tiết các thành phần UI/UX cho trang này:

-----

### 1\. Khu vực "Sức khỏe Ngân sách" (Budget Health Monitor)

Đây là phần hiển thị trực quan nhất, giúp người dùng biết ngay lập tức họ có đang tiêu lạm hay không.

  * **Thanh Tiến độ Ngân sách (Budget Progress Bar):**
      * Một thanh ngang lớn hiển thị mức tiêu dùng hiện tại so với giới hạn đã đặt.
      * **Màu sắc động:**
          * Xanh lá: \< 50% (An toàn).
          * Vàng: 51% - 80% (Cần chú ý).
          * Đỏ: \> 80% (Cảnh báo nguy hiểm).
      * *Ví dụ text:* "Bạn đã dùng **1.500.000đ** trên tổng ngân sách **2.000.000đ** (75%)".
  * **Dự báo chi tiêu (Spending Forecast):**
      * Hiển thị một vạch mờ trên thanh tiến độ, dự đoán mức tiêu dùng đến cuối tháng dựa trên tốc độ hiện tại.
      * *Text:* "Dự kiến cuối tháng sẽ dùng hết 2.100.000đ (Vượt ngân sách 5%)".

### 2\. Khu vực Cài đặt Hạn mức (Budget Configuration)

Nơi người dùng thiết lập "luật chơi" cho tài khoản của mình.

  * **Input nhập số tiền:**
      * Ô nhập liệu lớn: "Đặt ngân sách hàng tháng".
      * Gợi ý nhanh (Chips): [1 Triệu] [2 Triệu] [5 Triệu].
  * **Loại giới hạn (Threshold Action):** (Rất quan trọng cho dịch vụ SaaS/API)
      * *Radio Button 1:* **Giới hạn Mềm (Soft Cap):** Chỉ gửi cảnh báo, dịch vụ vẫn chạy tiếp khi vượt ngưỡng (Phù hợp khách hàng doanh nghiệp không muốn gián đoạn).
      * *Radio Button 2:* **Giới hạn Cứng (Hard Cap):** Tự động ngắt dịch vụ/API khi chạm ngưỡng (Phù hợp người dùng cá nhân sợ tốn tiền).
  * **Nút Lưu:** "Cập nhật ngân sách".

### 3\. Khu vực Cảnh báo (Alert Rules)

Người dùng muốn được thông báo như thế nào và khi nào.

  * **Các mốc kích hoạt (Threshold Triggers):**
      * Danh sách checkbox hoặc thanh trượt (slider) đa điểm.
      * ☑ Gửi cảnh báo khi đạt 50%.
      * ☑ Gửi cảnh báo khi đạt 80%.
      * ☑ Gửi cảnh báo khi đạt 100%.
  * **Kênh thông báo (Notification Channels):**
      * ☑ Email.
      * ☑ SMS (có thể tính phí).
      * ☑ Notification trên App/Web.
      * ☑ Webhook (Dành cho Dev muốn tích hợp vào Slack/Telegram riêng).

### 4\. Khu vực Phân tích chi tiêu (Cost Breakdown)

Trả lời câu hỏi: "Tại sao tôi lại tốn nhiều tiền thế?"

  * **Biểu đồ tròn (Donut Chart):** Chia tỉ lệ các khoản chi.
      * *Ví dụ:* 60% cho GPT-4 API, 30% cho phí duy trì Gói Pro, 10% cho phí lưu trữ (Storage).
  * **Top tiêu hao (Top Consumers):**
      * Danh sách 5 mục tốn tiền nhất.
      * *Ví dụ:* "Project A: 500k", "Project B: 200k".

### 5\. Góc AI Tư vấn (AI Budget Advisor)

Tận dụng tính năng AI mà chúng ta đã bàn trước đó.

  * **Thẻ Insight (Card):**
      * "Tháng này bạn tiêu nhiều hơn tháng trước 20% do việc sử dụng API tăng đột biến vào ngày thứ Ba."
      * "Gợi ý: Nếu bạn nâng cấp lên gói Enterprise, với mức dùng hiện tại, bạn sẽ tiết kiệm được 15% so với trả theo lượt (Pay-as-you-go)."

-----

### Gợi ý Bố cục (Layout Skeleton)

```text
+---------------------------------------------------------------+
|  TITLE: Quản lý & Kiểm soát Ngân sách                         |
+---------------------------------------------------------------+
|                                                               |
|  [ KHU VỰC 1: HEALTH MONITOR - Chiếm chiều ngang ]            |
|  +---------------------------------------------------------+  |
|  |  Đã dùng: 1.5tr / 2.0tr                                 |  |
|  |  [||||||||||||||||||||||||-------] 75% (Màu Vàng)       |  |
|  |  (Dự báo sẽ vượt ngưỡng vào ngày 28/12)                 |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  [ Cột Trái - Cài đặt ]             [ Cột Phải - Phân tích ]  |
|                                                               |
|  +-----------------------+          +----------------------+  |
|  | CÀI ĐẶT HẠN MỨC       |          | PHÂN BỔ CHI TIÊU     |  |
|  | [ Input: 2.000.000 ]  |          |      (Biểu đồ)       |  |
|  |                       |          |      O  Sub: 30%     |  |
|  | (o) Soft Cap (Báo)    |          |         API: 70%     |  |
|  | ( ) Hard Cap (Ngắt)   |          |                      |  |
|  +-----------------------+          +----------------------+  |
|                                                               |
|  +-----------------------+          +----------------------+  |
|  | CẤU HÌNH CẢNH BÁO     |          | AI INSIGHTS          |  |
|  | [x] Báo khi đạt 50%   |          | "Bạn nên tắt server  |  |
|  | [x] Báo khi đạt 90%   |          | test vào cuối tuần   |  |
|  | Gửi qua: [Email]      |          | để tiết kiệm..."     |  |
|  +-----------------------+          +----------------------+  |
|                                                               |
+---------------------------------------------------------------+
```

### Ý tưởng kết hợp Giáng sinh (Christmas Theme)

Vì anh đang làm theme Noel, đây là vài chi tiết nhỏ có thể thêm vào trang Ngân sách cho vui mắt:

  * **Thanh tiến độ (Progress Bar):** Thay vì màu xanh/đỏ trơn, có thể làm hiệu ứng **"Thanh kẹo gậy" (Candy Cane)** sọc trắng đỏ chéo nhau.
  * **Khi vượt ngân sách:** Thay vì icon cảnh báo tam giác vàng bình thường, có thể hiện hình **"Ông già Noel mặt buồn"** hoặc **"Cục than đen"** (biểu tượng quà tặng cho trẻ hư).

