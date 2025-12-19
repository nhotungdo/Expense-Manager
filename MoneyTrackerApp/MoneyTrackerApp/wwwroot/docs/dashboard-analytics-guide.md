# Dashboard Analytics - Hướng dẫn Sử dụng

## 📊 Tổng quan

Dashboard Analytics là trang phân tích chi tiêu với **4 biểu đồ tròn (Donut Charts)** hiển thị trực quan dữ liệu tài chính của bạn.

## 🎨 Thiết kế

### Màu sắc Pastel
- **Tím**: #7C3AED
- **Xanh**: #3B82F6
- **Xanh ngọc**: #14B8A6
- **Vàng**: #FACC15
- **Hồng**: #F472B6
- **Xanh lá**: #10B981
- **Cam**: #F59E0B

### Layout
- **Desktop**: Grid 3 cột
- **Tablet**: Grid 2 cột
- **Mobile**: Grid 1 cột

## 📈 Các Biểu đồ

### 1. Chi tiêu theo Danh mục
Hiển thị top 5 danh mục chi tiêu nhiều nhất:
- Ăn uống
- Đi lại
- Giải trí
- Mua sắm
- Khác

### 2. Thu nhập theo Nguồn
Phân tích nguồn thu nhập:
- Lương chính
- Thưởng
- Freelance
- Thu nhập khác

### 3. Giao dịch theo Loại
Cơ cấu dòng tiền:
- Thu nhập (màu xanh lá)
- Chi tiêu (màu hồng)

### 4. Giao dịch theo Ví
Phân bổ tài sản:
- Tiền mặt
- Ngân hàng
- Ví điện tử
- Tiết kiệm

## 🔧 Tính năng

### Bộ lọc Thời gian
- 7 ngày qua
- 30 ngày qua (mặc định)
- 90 ngày qua
- Năm nay

### Tương tác
- **Hover**: Hiển thị tooltip với thông tin chi tiết
- **Animation**: Hiệu ứng mượt mà khi load
- **Responsive**: Tự động điều chỉnh theo màn hình

### Thống kê Tổng quan
3 card thống kê chính:
1. **Tổng Thu nhập**: Tổng số tiền thu được
2. **Tổng Chi tiêu**: Tổng số tiền đã chi
3. **Số dư**: Thu nhập - Chi tiêu

## 🚀 Cách sử dụng

### Truy cập
```
/DashboardAnalytics
```

### API Endpoint
```
GET /api/dashboard/analytics?days=30
```

**Response:**
```json
{
  "categorySpending": [
    {
      "name": "Ăn uống",
      "value": 4500000,
      "count": 45,
      "color": "#7C3AED"
    }
  ],
  "incomeSource": [...],
  "transactionType": [...],
  "walletDistribution": [...],
  "totalIncome": 33000000,
  "totalExpense": 12000000,
  "balance": 21000000
}
```

## 📁 Cấu trúc File

```
MoneyTrackerApp/
├── Pages/
│   ├── DashboardAnalytics.cshtml          # View
│   └── DashboardAnalytics.cshtml.cs       # PageModel
├── Controllers/
│   └── DashboardController.cs             # API Controller
├── Services/
│   └── ReportService.cs                   # Business Logic
├── DTOs/
│   └── DashboardAnalyticsDto.cs           # Data Transfer Objects
└── wwwroot/
    ├── css/
    │   └── dashboard-analytics.css        # Styles
    └── js/
        └── dashboard-analytics.js         # Chart Logic
```

## 🎯 Component hóa

### DonutChart Component
```javascript
createDonutChart(canvasId, data, legendId, totalId)
```

**Parameters:**
- `canvasId`: ID của canvas element
- `data`: Mảng dữ liệu biểu đồ
- `legendId`: ID của legend container
- `totalId`: ID của total count element

**Data Format:**
```javascript
[
  {
    name: "Ăn uống",
    value: 4500000,
    count: 45,
    color: "#7C3AED"
  }
]
```

## 🔄 Cập nhật Dữ liệu

### Tự động
Dashboard tự động load dữ liệu khi:
- Trang được mở
- Thay đổi bộ lọc thời gian

### Thủ công
Click nút **"Làm mới"** để reload dữ liệu

## 🐛 Xử lý Lỗi

### Loading State
Hiển thị spinner khi đang tải dữ liệu

### Error State
Hiển thị thông báo lỗi nếu không thể tải dữ liệu

### Fallback
Sử dụng mock data nếu API không khả dụng (development mode)

## 📱 Responsive Design

### Desktop (> 1200px)
- Grid 3 cột
- Biểu đồ kích thước lớn

### Tablet (768px - 1200px)
- Grid 2 cột
- Biểu đồ kích thước trung bình

### Mobile (< 768px)
- Grid 1 cột
- Biểu đồ kích thước nhỏ
- Header stack vertical

## 🎨 Customization

### Thay đổi Màu sắc
Chỉnh sửa trong `dashboard-analytics.css`:
```css
:root {
    --color-purple: #7C3AED;
    --color-blue: #3B82F6;
    /* ... */
}
```

### Thay đổi Cutout Size
Chỉnh sửa trong `dashboard-analytics.js`:
```javascript
cutout: '70%' // Thay đổi từ 70% sang giá trị khác
```

## 🔐 Bảo mật

- Yêu cầu authentication
- Chỉ hiển thị dữ liệu của user hiện tại
- API endpoint được bảo vệ bởi `[Authorize]`

## 📊 Performance

### Optimization
- Chart.js v4.4.0 (latest)
- Lazy loading cho biểu đồ
- Debounce cho filter changes
- Animation duration: 800ms

### Best Practices
- Destroy chart trước khi tạo mới
- Sử dụng `maintainAspectRatio: false`
- Limit data points (top 5-8 items)

## 🧪 Testing

### Manual Testing
1. Mở `/DashboardAnalytics`
2. Kiểm tra 4 biểu đồ hiển thị đúng
3. Test bộ lọc thời gian
4. Test responsive trên các thiết bị
5. Test hover tooltip
6. Test nút refresh

### API Testing
```bash
curl -X GET "https://localhost:5000/api/dashboard/analytics?days=30" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📝 Notes

- Biểu đồ sử dụng Chart.js v4.4.0
- Font: Inter, system-ui, sans-serif
- Animation: easeOutQuart
- Border radius: 16px
- Shadow: Subtle elevation

## 🚀 Future Enhancements

1. **Export**: Xuất biểu đồ ra PDF/PNG
2. **Drill-down**: Click vào segment để xem chi tiết
3. **Comparison**: So sánh với tháng trước
4. **AI Insights**: Phân tích thông minh
5. **Custom Date Range**: Chọn khoảng thời gian tùy chỉnh

## 📞 Support

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra Console log
2. Kiểm tra Network tab
3. Verify API endpoint
4. Check authentication token
