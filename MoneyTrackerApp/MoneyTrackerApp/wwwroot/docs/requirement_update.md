Dưới đây là **PROMPT CODE CHUẨN** dùng cho **AI sinh code tạo **trang UI Dashboard quản lý chi tiêu** với **biểu đồ tròn (donut chart)**
(Phù hợp React + Chart.js / Recharts / Ant Design / Tailwind)

---

## 🎯 PROMPT TỔNG (KHUYẾN NGHỊ DÙNG NGUYÊN VĂN)

```text
Bạn là Senior Frontend Developer.

Hãy tạo code giao diện Dashboard cho dự án Quản lý Chi tiêu cá nhân.

Yêu cầu UI:
- Phong cách hiện đại, dashboard analytics
- Nền sáng, card bo góc, shadow nhẹ
- Màu chủ đạo: tím – xanh – pastel (giống dashboard tài chính)
- Responsive desktop first

Layout:
- Trang Dashboard gồm grid 3 cột
- Mỗi card chứa biểu đồ tròn (Donut Chart)
- Trên mỗi card có:
  + Tiêu đề (ví dụ: "Chi tiêu theo danh mục")
  + Biểu đồ tròn
  + Tổng số giao dịch (Total count)
  + Chú thích màu (legend)

Các card biểu đồ:
1. Chi tiêu theo Danh mục
   - Ăn uống
   - Đi lại
   - Giải trí
   - Mua sắm
   - Khác

2. Thu nhập theo nguồn
   - Lương
   - Thưởng
   - Freelance

3. Giao dịch theo loại
   - Thu
   - Chi

4. Giao dịch theo ví
   - Tiền mặt
   - Ngân hàng
   - Ví điện tử

Dữ liệu:
- Dữ liệu mock (hardcode)
- Có tổng số giao dịch hiển thị bên cạnh biểu đồ

Kỹ thuật:
- React functional component
- Sử dụng Chart.js (react-chartjs-2) HOẶC Recharts
- TailwindCSS để layout & style
- Component hóa: Dashboard, ChartCard, DonutChart

Output:
- Trả về code hoàn chỉnh
- Có chú thích code rõ ràng
- Không cần backend
```

---

## 🔧 PROMPT RIÊNG CHO BIỂU ĐỒ TRÒN (DONUT)

```text
Tạo component DonutChart bằng Chart.js:

- Kiểu: Doughnut
- Cắt rỗng giữa (cutout: 70%)
- Hiển thị tổng số giao dịch ở chính giữa
- Màu gradient pastel
- Legend hiển thị phía dưới
- Tooltip đẹp, có đơn vị tiền (VND)

Input props:
- labels
- data
- total
```

---

## 🧱 PROMPT TẠO CARD DASHBOARD

```text
Tạo component Card Dashboard:

- Nền trắng
- Border radius 16px
- Shadow nhẹ
- Padding 20px
- Tiêu đề bên trái
- Tổng số bên phải
- Biểu đồ nằm giữa card
```

---

## 🎨 PROMPT MÀU SẮC (OPTIONAL)

```text
Sử dụng bảng màu:
- Tím: #7C3AED
- Xanh: #3B82F6
- Xanh ngọc: #14B8A6
- Vàng nhạt: #FACC15
- Hồng pastel: #F472B6
```

---

## 📐 PROMPT GRID LAYOUT

```text
Dashboard sử dụng CSS Grid:
- Desktop: 3 cột
- Tablet: 2 cột
- Mobile: 1 cột
Khoảng cách giữa các card: 24px
```

---

## ✅ KẾT QUẢ MONG MUỐN

Sau khi dùng prompt này, AI sẽ sinh ra:

* Trang Dashboard giống **analytics dashboard tài chính**
* Biểu đồ tròn đúng kiểu như ảnh
* Dễ mở rộng sang:

  * Biểu đồ cột
  * So sánh tháng
  * AI Analysis

---

👉 Nếu anh muốn, em có thể:

* ✍️ Viết **code React + Chart.js hoàn chỉnh**
* 🎨 Chuyển sang **Ant Design / MUI**
* 🧠 Gắn **AI Gemini phân tích trực tiếp trên dashboard**
* 🗂️ Tách component chuẩn MVC cho đồ án


