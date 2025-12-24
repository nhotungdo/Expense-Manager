# Tính năng Chi tiêu nhóm - Hướng dẫn đầy đủ

## 📋 Tổng quan

Giao diện Chi tiêu nhóm được thiết kế hiện đại với đầy đủ tính năng để quản lý chi tiêu chung một cách hiệu quả.

## ✨ Danh sách tính năng

### 1. **Quản lý nhóm cơ bản**
- ✅ Tạo nhóm mới
- ✅ Xem danh sách nhóm
- ✅ Tìm kiếm nhóm
- ✅ Xem chi tiết nhóm
- ✅ Chỉnh sửa thông tin nhóm
- ✅ Xóa/Lưu trữ nhóm

### 2. **Chế độ xem**
- ✅ Chế độ danh sách (List view)
- ✅ Chế độ lưới (Grid view)
- ✅ Lưu tùy chọn xem vào localStorage

### 3. **Lọc và sắp xếp**
- ✅ Lọc theo trạng thái số dư:
  - Tất cả
  - Được nhận tiền (positive)
  - Đang nợ (negative)
  - Đã thanh toán (settled)
- ✅ Sắp xếp theo:
  - Tên nhóm
  - Số dư
  - Số thành viên
  - Hoạt động gần đây
- ✅ Thứ tự tăng dần/giảm dần

### 4. **Thống kê và phân tích**
- ✅ Tổng số dư
- ✅ Tổng tiền sẽ nhận
- ✅ Tổng tiền cần trả
- ✅ Số người nợ/cho vay
- ✅ Tổng số nhóm
- ✅ Tổng giao dịch
- ✅ Tổng chi tiêu
- ✅ Biểu đồ chi tiêu theo danh mục

### 5. **Thêm chi tiêu nhanh**
- ✅ Modal thêm chi tiêu nhanh từ thẻ nhóm
- ✅ Chọn người trả
- ✅ Nhập mô tả và số tiền
- ✅ Xác nhận nhanh

### 6. **Chia sẻ nhóm**
- ✅ Chia sẻ qua Web Share API (mobile)
- ✅ Sao chép link mời vào clipboard
- ✅ Tạo link mời tự động

### 7. **Xuất dữ liệu**
- ✅ Xuất định dạng CSV
- ✅ Xuất định dạng JSON
- ✅ Xuất định dạng PDF (đang phát triển)
- ✅ Tùy chọn nội dung xuất:
  - Giao dịch
  - Số dư
  - Thành viên

### 8. **Mẫu nhóm**
- ✅ 6 mẫu nhóm có sẵn:
  - Du lịch
  - Gia đình
  - Bạn bè
  - Sự kiện
  - Dự án
  - Phòng trọ
- ✅ Tạo nhóm từ mẫu nhanh chóng

### 9. **Hành động hàng loạt**
- ✅ Chế độ chọn nhiều
- ✅ Chọn tất cả/Bỏ chọn tất cả
- ✅ Lưu trữ nhiều nhóm
- ✅ Xóa nhiều nhóm
- ✅ Thanh công cụ hành động hàng loạt

### 10. **Menu ngữ cảnh**
- ✅ Menu hành động cho mỗi nhóm:
  - Cài đặt
  - Lưu trữ
  - Xuất dữ liệu
  - Rời nhóm

### 11. **Phím tắt**
- ✅ `Ctrl + N`: Tạo nhóm mới
- ✅ `Ctrl + F`: Tìm kiếm
- ✅ `Ctrl + E`: Xuất dữ liệu
- ✅ `Ctrl + K`: Lọc nhóm
- ✅ `?`: Hiện danh sách phím tắt
- ✅ `Esc`: Đóng modal

### 12. **Floating Action Button (FAB)**
- ✅ Nút hành động nổi ở góc phải dưới
- ✅ Menu FAB với các hành động:
  - Tạo nhóm
  - Chọn nhiều
  - Phím tắt

### 13. **Thông báo Toast**
- ✅ Thông báo thành công (success)
- ✅ Thông báo lỗi (error)
- ✅ Thông báo cảnh báo (warning)
- ✅ Thông báo thông tin (info)
- ✅ Tự động ẩn sau 3 giây
- ✅ Animation mượt mà

### 14. **Hoạt động gần đây**
- ✅ Hiển thị 5 hoạt động mới nhất
- ✅ Nút làm mới
- ✅ Hiển thị thời gian tương đối
- ✅ Hiển thị tên nhóm và người thực hiện

### 15. **Responsive Design**
- ✅ Tối ưu cho desktop
- ✅ Tối ưu cho tablet
- ✅ Tối ưu cho mobile
- ✅ Touch-friendly trên mobile

### 16. **Animations**
- ✅ Fade in/out
- ✅ Slide up/down
- ✅ Scale in
- ✅ Staggered animations cho danh sách
- ✅ Smooth transitions

### 17. **Accessibility**
- ✅ Keyboard navigation
- ✅ Focus states
- ✅ ARIA labels
- ✅ Semantic HTML

## 🎨 Thiết kế

### Màu sắc
- Primary: `#6366f1` (Indigo)
- Success: `#10b981` (Green)
- Danger: `#ef4444` (Red)
- Warning: `#f59e0b` (Amber)
- Info: `#3b82f6` (Blue)

### Typography
- Font family: Inter
- Font weights: 300, 400, 500, 600, 700, 800, 900

### Spacing
- Base unit: 0.25rem (4px)
- Common spacing: 0.5rem, 1rem, 1.5rem, 2rem

### Border Radius
- Small: 0.5rem
- Medium: 0.75rem
- Large: 1rem
- Extra Large: 1.5rem

## 🚀 Cách sử dụng

### Tạo nhóm mới
1. Click nút "Tạo nhóm" hoặc nhấn `Ctrl + N`
2. Chọn "Tạo nhóm trống" hoặc "Từ mẫu có sẵn"
3. Nhập tên nhóm và mô tả
4. Chọn thành viên từ danh sách bạn bè
5. Click "Tạo nhóm"

### Thêm chi tiêu nhanh
1. Click icon "+" trên thẻ nhóm
2. Nhập mô tả và số tiền
3. Chọn người trả
4. Click "Thêm chi tiêu"

### Xuất dữ liệu
1. Click nút "Xuất" hoặc nhấn `Ctrl + E`
2. Chọn định dạng (CSV, PDF, JSON)
3. Chọn nội dung cần xuất
4. Click "Xuất dữ liệu"

### Lọc nhóm
1. Click nút "Lọc" hoặc nhấn `Ctrl + K`
2. Chọn trạng thái số dư
3. Chọn cách sắp xếp
4. Click "Áp dụng"

### Hành động hàng loạt
1. Click icon FAB ở góc phải dưới
2. Chọn "Chọn nhiều"
3. Chọn các nhóm cần thao tác
4. Chọn hành động (Lưu trữ/Xóa)

## 📱 Tương thích

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+
- ✅ Mobile browsers

## 🔧 Công nghệ

- **Frontend Framework**: Vue 3 (Composition API)
- **Charts**: Chart.js
- **Icons**: Font Awesome
- **CSS**: Custom CSS with CSS Variables
- **Backend**: ASP.NET Core

## 📝 Ghi chú

- Tất cả dữ liệu được lưu trữ an toàn trên server
- Tùy chọn xem được lưu trong localStorage
- Hỗ trợ đa ngôn ngữ (hiện tại: Tiếng Việt)
- Tối ưu hiệu suất với lazy loading

## 🎯 Tính năng sắp tới

- [ ] Xuất PDF với template đẹp
- [ ] Thông báo real-time
- [ ] Tích hợp thanh toán
- [ ] Báo cáo chi tiết hơn
- [ ] Dark mode
- [ ] Offline support
- [ ] Push notifications

## 🐛 Báo lỗi

Nếu gặp lỗi, vui lòng báo cáo qua:
- Email: support@moneytracker.com
- GitHub Issues: [link]

## 📄 License

Copyright © 2024 MoneyTracker App. All rights reserved.
