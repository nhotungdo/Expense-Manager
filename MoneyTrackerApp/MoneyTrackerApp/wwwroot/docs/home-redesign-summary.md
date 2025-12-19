# Thiết kế lại trang Home với Biểu đồ tròn

## Tổng quan
Đã thiết kế lại trang `/home` để sử dụng biểu đồ tròn (doughnut charts) thay vì biểu đồ cột, giúp trực quan hóa phân bổ giao dịch theo danh mục một cách rõ ràng và dễ hiểu hơn.

## Các thay đổi chính

### 1. Giao diện (UI)
**File: `MoneyTrackerApp/Pages/Home.cshtml`**

- ✅ Thay thế biểu đồ dòng tiền (line chart) bằng 2 biểu đồ tròn:
  - **Biểu đồ phân bổ chi tiêu**: Hiển thị chi tiêu theo danh mục
  - **Biểu đồ nguồn thu nhập**: Hiển thị thu nhập theo danh mục
  
- ✅ Thêm bộ lọc thời gian cho mỗi biểu đồ:
  - Tháng này (mặc định)
  - Tuần này
  - Năm nay

- ✅ Giữ nguyên các thẻ thống kê (Stats Cards):
  - Tổng tài sản
  - Thu nhập tháng này
  - Chi tiêu tháng này

- ✅ Giữ nguyên các tính năng khác:
  - Tư vấn AI
  - Thao tác nhanh
  - Giao dịch gần đây

### 2. JavaScript Logic
**File mới: `MoneyTrackerApp/wwwroot/js/home.js`**

Các chức năng chính:
- `loadPersonalWalletData()`: Tải dữ liệu tổng quan ví
- `loadExpenseBreakdown(period)`: Tải phân tích chi tiêu theo danh mục
- `loadIncomeBreakdown(period)`: Tải phân tích thu nhập theo danh mục
- `renderExpenseChart(data)`: Vẽ biểu đồ tròn chi tiêu
- `renderIncomeChart(data)`: Vẽ biểu đồ tròn thu nhập
- `loadRecentTransactions()`: Tải giao dịch gần đây
- `loadAccounts()`: Tải danh sách ví

**Tính năng biểu đồ:**
- Sử dụng Chart.js với kiểu `doughnut`
- Hiển thị phần trăm trong legend
- Tooltip hiển thị số tiền và phần trăm
- Animation mượt mà
- Xử lý trường hợp không có dữ liệu
- Màu sắc phân biệt rõ ràng

### 3. Backend API
**File: `MoneyTrackerApp/Controllers/DashboardController.cs`**

Thêm 3 endpoint mới:
```csharp
GET /api/Dashboard/personal-wallet
GET /api/Dashboard/expense-breakdown?period={month|week|year}
GET /api/Dashboard/income-breakdown?period={month|week|year}
```

**File: `MoneyTrackerApp/Controllers/TransactionsController.cs`**

Thêm endpoint:
```csharp
GET /api/Transactions/recent?limit=5
```

### 4. Service Layer
**File: `MoneyTrackerApp/Services/ReportService.cs`**

Thêm 3 phương thức mới:
- `GetPersonalWalletSummaryAsync(userId)`: Lấy tổng quan ví cá nhân
- `GetExpenseBreakdownAsync(userId, period)`: Phân tích chi tiêu theo danh mục
- `GetIncomeBreakdownAsync(userId, period)`: Phân tích thu nhập theo danh mục
- `GetDateRangeFromPeriod(period)`: Helper để tính khoảng thời gian

**DTO mới:**
```csharp
public class CategoryBreakdownItem
{
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}
```

### 5. Styling
**File mới: `MoneyTrackerApp/wwwroot/css/home.css`**

- Styles cho container biểu đồ
- Empty state styling
- Period selector styling
- Responsive design cho mobile và tablet

## Cấu trúc dữ liệu

### Response từ `/api/Dashboard/expense-breakdown`
```json
[
  {
    "categoryName": "Ăn uống",
    "amount": 1500000,
    "transactionCount": 15
  },
  {
    "categoryName": "Di chuyển",
    "amount": 800000,
    "transactionCount": 8
  }
]
```

### Response từ `/api/Dashboard/personal-wallet`
```json
{
  "totalBalance": 50000000,
  "monthlyIncome": 15000000,
  "monthlyExpense": 8000000,
  "accountCount": 3
}
```

## Tính năng nổi bật

### 1. Biểu đồ tròn tương tác
- Hover để xem chi tiết
- Click legend để ẩn/hiện danh mục
- Animation khi load và cập nhật
- Responsive trên mọi thiết bị

### 2. Bộ lọc thời gian linh hoạt
- Chuyển đổi nhanh giữa các khoảng thời gian
- Tự động tải lại dữ liệu
- Không reload trang

### 3. Trực quan hóa dữ liệu
- Màu sắc phân biệt rõ ràng
- Hiển thị phần trăm trong legend
- Tooltip chi tiết với format tiền tệ
- Empty state thân thiện

### 4. Performance
- Lazy loading cho biểu đồ
- Destroy chart cũ trước khi tạo mới
- Optimize API calls
- Caching accounts data

## Hướng dẫn sử dụng

### Cho người dùng:
1. Truy cập `/home`
2. Xem tổng quan tài sản ở phần Stats Cards
3. Xem phân bổ chi tiêu/thu nhập qua biểu đồ tròn
4. Chọn khoảng thời gian để xem dữ liệu khác
5. Hover vào biểu đồ để xem chi tiết
6. Cuộn xuống để xem giao dịch gần đây

### Cho developer:
1. Các API endpoint đã được document
2. JavaScript functions có thể tái sử dụng
3. Chart config có thể customize trong `charts-config.js`
4. CSS có thể override trong `home.css`

## Testing

### Manual Testing:
1. ✅ Kiểm tra hiển thị biểu đồ với dữ liệu
2. ✅ Kiểm tra empty state khi không có dữ liệu
3. ✅ Kiểm tra chuyển đổi period
4. ✅ Kiểm tra responsive trên mobile
5. ✅ Kiểm tra tooltip và legend
6. ✅ Kiểm tra API endpoints

### Browser Compatibility:
- Chrome ✅
- Firefox ✅
- Safari ✅
- Edge ✅

## Cải tiến trong tương lai

1. **Thêm biểu đồ so sánh**: So sánh chi tiêu/thu nhập giữa các tháng
2. **Export biểu đồ**: Cho phép tải biểu đồ dưới dạng hình ảnh
3. **Drill-down**: Click vào danh mục để xem chi tiết giao dịch
4. **Filters nâng cao**: Lọc theo ví, theo tag, theo người dùng
5. **Real-time updates**: Cập nhật biểu đồ real-time khi có giao dịch mới
6. **Custom date range**: Cho phép chọn khoảng thời gian tùy chỉnh

## Dependencies

- Chart.js 4.4.0
- Tailwind CSS (đã có)
- Font Awesome (đã có)
- Bootstrap 5 (cho modals)

## Files đã thay đổi

### Mới tạo:
- `MoneyTrackerApp/wwwroot/js/home.js`
- `MoneyTrackerApp/wwwroot/css/home.css`
- `MoneyTrackerApp/wwwroot/docs/home-redesign-summary.md`

### Đã chỉnh sửa:
- `MoneyTrackerApp/Pages/Home.cshtml`
- `MoneyTrackerApp/Controllers/DashboardController.cs`
- `MoneyTrackerApp/Controllers/TransactionsController.cs`
- `MoneyTrackerApp/Services/ReportService.cs`

## Kết luận

Trang home đã được thiết kế lại thành công với biểu đồ tròn, giúp người dùng dễ dàng nắm bắt phân bổ chi tiêu và thu nhập của mình. Giao diện trực quan, tương tác mượt mà và responsive tốt trên mọi thiết bị.
