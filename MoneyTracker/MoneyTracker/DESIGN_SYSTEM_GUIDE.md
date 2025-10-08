# Money Tracker Design System

## Tổng quan

Money Tracker Design System là một hệ thống thiết kế hiện đại, chuyên nghiệp được xây dựng đặc biệt cho ứng dụng quản lý tài chính cá nhân. Hệ thống này tập trung vào tính dễ sử dụng, độ tin cậy và khả năng hiển thị dữ liệu tài chính hiệu quả.

## Đặc điểm chính

### 🎨 **Thiết kế hiện đại với Glassmorphism**
- Hiệu ứng kính mờ (frosted glass) với viền mềm mại
- Bóng đổ tinh tế và hiệu ứng trong suốt
- Giao diện sạch sẽ, tối giản

### 🎯 **Tối ưu cho dữ liệu tài chính**
- Màu sắc rõ ràng cho thu nhập (xanh lá) và chi tiêu (đỏ cam)
- Biểu đồ và thống kê dễ đọc
- Hiển thị số liệu tài chính rõ ràng

### 📱 **Responsive Design**
- Tương thích với mọi thiết bị
- Mobile-first approach
- Sidebar có thể thu gọn trên mobile

### 🌙 **Dark/Light Mode**
- Chuyển đổi chế độ sáng/tối
- Lưu trữ tùy chọn người dùng
- Màu sắc được tối ưu cho cả hai chế độ

## Cấu trúc thư mục

```
wwwroot/
├── css/
│   └── design-system.css          # CSS chính của design system
├── js/
│   ├── design-system.js           # JavaScript chính
│   └── logout.js                  # Utility scripts
└── lib/                           # Thư viện bên thứ 3
    ├── bootstrap/
    ├── jquery/
    └── font-awesome/
```

## Sử dụng cơ bản

### 1. Kết nối Design System

Trong file `_Layout.cshtml`:

```html
<!-- Fonts -->
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">

<!-- Font Awesome -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

<!-- Design System CSS -->
<link rel="stylesheet" href="~/css/design-system.css" asp-append-version="true">

<!-- Scripts -->
<script src="~/js/design-system.js"></script>
```

### 2. Cấu trúc Layout

```html
<div class="min-h-full">
    <!-- Sidebar -->
    <div id="sidebar" class="sidebar">
        <!-- Navigation content -->
    </div>
    
    <!-- Main Content -->
    <div class="main-content">
        <!-- Top Bar -->
        <div class="top-bar">
            <!-- Top bar content -->
        </div>
        
        <!-- Page Content -->
        <main class="page-content">
            <div class="content-container">
                <!-- Your page content -->
            </div>
        </main>
    </div>
</div>
```

## Components

### 1. Cards

#### Basic Card
```html
<div class="card">
    <div class="card-header">
        <h5 class="card-title">Card Title</h5>
        <p class="card-subtitle">Card subtitle</p>
    </div>
    <div class="card-body">
        <!-- Card content -->
    </div>
</div>
```

#### Interactive Card
```html
<div class="card card-interactive">
    <!-- Card content with hover effects -->
</div>
```

#### Elevated Card
```html
<div class="card card-elevated">
    <!-- Card with enhanced shadow -->
</div>
```

### 2. Statistics Cards

```html
<div class="stat-card">
    <div class="stat-value text-income">15,000,000 VND</div>
    <div class="stat-label">Total Income</div>
    <div class="stat-change positive">
        <i class="fas fa-arrow-up"></i>
        <span>+12%</span>
    </div>
</div>
```

### 3. Buttons

#### Button Variants
```html
<button class="btn btn-primary">Primary</button>
<button class="btn btn-secondary">Secondary</button>
<button class="btn btn-outline">Outline</button>
<button class="btn btn-ghost">Ghost</button>
```

#### Button Sizes
```html
<button class="btn btn-primary btn-sm">Small</button>
<button class="btn btn-primary">Default</button>
<button class="btn btn-primary btn-lg">Large</button>
<button class="btn btn-primary btn-xl">Extra Large</button>
```

#### Buttons with Icons
```html
<button class="btn btn-primary">
    <i class="fas fa-plus"></i>
    Add Transaction
</button>
```

### 4. Forms

```html
<div class="form-group">
    <label class="form-label">Transaction Amount</label>
    <input type="text" class="form-control" placeholder="Enter amount">
</div>

<div class="form-group">
    <label class="form-label">Category</label>
    <select class="form-control form-select">
        <option>Select category</option>
        <option>Food & Dining</option>
    </select>
</div>
```

### 5. Transaction Items

```html
<div class="transaction-item">
    <div class="transaction-icon income">
        <i class="fas fa-arrow-up"></i>
    </div>
    <div class="transaction-details">
        <div class="transaction-title">Salary Payment</div>
        <div class="transaction-meta">Salary • Jan 15, 2024</div>
    </div>
    <div class="transaction-amount income">+15,000,000 VND</div>
</div>
```

### 6. Budget Components

```html
<div class="budget-item">
    <div class="budget-header">
        <div class="budget-title">Food & Dining</div>
        <div class="budget-amount">2,500,000 / 5,000,000 VND</div>
    </div>
    <div class="budget-progress">
        <div class="budget-progress-bar" style="width: 50%"></div>
    </div>
    <div class="budget-footer">
        <div class="budget-remaining">Remaining: 2,500,000 VND</div>
        <div class="budget-percentage">50%</div>
    </div>
</div>
```

### 7. Chart Containers

```html
<div class="chart-container">
    <div class="chart-header">
        <h5 class="chart-title">Income vs Expense Trends</h5>
        <div class="chart-actions">
            <button type="button" class="chart-btn active">Week</button>
            <button type="button" class="chart-btn">Month</button>
            <button type="button" class="chart-btn">Year</button>
        </div>
    </div>
    <div style="height: 300px;">
        <canvas id="trendsChart"></canvas>
    </div>
</div>
```

### 8. Toast Notifications

```html
<div class="toast show">
    <div class="toast-icon success">
        <i class="fas fa-check"></i>
    </div>
    <div class="toast-content">
        <div class="toast-title">Success</div>
        <div class="toast-message">Transaction added successfully!</div>
    </div>
</div>
```

## JavaScript API

### Design System Instance

```javascript
// Access the design system instance
const designSystem = window.designSystem;

// Show toast notification
designSystem.showToast('Message', 'success', 'Title');

// Show loading overlay
designSystem.showLoading('Loading data...');

// Hide loading overlay
designSystem.hideLoading();

// Format currency
const formattedAmount = designSystem.formatCurrency(1500000);

// Format date
const formattedDate = designSystem.formatDate('2024-01-15');
```

### Chart Utilities

```javascript
// Create line chart
const lineChart = ChartUtils.createLineChart(ctx, data, options);

// Create bar chart
const barChart = ChartUtils.createBarChart(ctx, data, options);

// Create doughnut chart
const doughnutChart = ChartUtils.createDoughnutChart(ctx, data, options);

// Get default options
const defaultOptions = ChartUtils.getDefaultOptions();

// Get color palette
const colors = ChartUtils.getColorPalette();
```

### Form Utilities

```javascript
// Validate email
const isValidEmail = FormUtils.validateEmail('user@example.com');

// Validate required field
const isValid = FormUtils.validateRequired('value');

// Validate minimum length
const isValidLength = FormUtils.validateMinLength('password', 8);

// Validate number
const isValidNumber = FormUtils.validateNumber('123.45');

// Format input value
const formattedValue = FormUtils.formatInputValue(input, 'currency');
```

## CSS Variables

### Colors
```css
:root {
    --primary-blue: #1A3A5A;
    --accent-green: #2ECC71;
    --expense-red: #E74C3C;
    --secondary-bg-light: #F8F9FA;
    --secondary-bg-dark: #0D1B2A;
}
```

### Typography
```css
:root {
    --font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    --font-size-base: 1rem;
    --font-size-lg: 1.125rem;
    --font-size-xl: 1.25rem;
}
```

### Spacing
```css
:root {
    --space-1: 0.25rem;
    --space-2: 0.5rem;
    --space-3: 0.75rem;
    --space-4: 1rem;
    --space-6: 1.5rem;
}
```

### Border Radius
```css
:root {
    --radius-sm: 0.375rem;
    --radius-md: 0.5rem;
    --radius-lg: 0.75rem;
    --radius-xl: 1rem;
}
```

## Responsive Design

### Breakpoints
- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

### Grid System
```html
<div class="row">
    <div class="col-12 col-md-6 col-lg-4">
        <!-- Responsive column -->
    </div>
</div>
```

## Accessibility

### Keyboard Navigation
- Tab navigation support
- Focus indicators
- Escape key to close dropdowns

### Screen Reader Support
- ARIA labels
- Semantic HTML
- Screen reader only text

### High Contrast Support
```css
@media (prefers-contrast: high) {
    :root {
        --glass-bg: rgba(255, 255, 255, 0.9);
        --glass-border: rgba(0, 0, 0, 0.3);
    }
}
```

### Reduced Motion Support
```css
@media (prefers-reduced-motion: reduce) {
    * {
        animation-duration: 0.01ms !important;
        transition-duration: 0.01ms !important;
    }
}
```

## Best Practices

### 1. Sử dụng Semantic HTML
```html
<!-- Good -->
<button class="btn btn-primary">Add Transaction</button>

<!-- Bad -->
<div class="btn btn-primary">Add Transaction</div>
```

### 2. Consistent Spacing
```html
<!-- Use CSS variables for consistent spacing -->
<div class="mb-4">
    <div class="p-3">
        <!-- Content -->
    </div>
</div>
```

### 3. Proper Color Usage
```html
<!-- Use semantic color classes -->
<div class="text-income">+15,000,000 VND</div>
<div class="text-expense">-500,000 VND</div>
<div class="text-primary">Balance</div>
```

### 4. Loading States
```html
<!-- Show loading states for better UX -->
<div class="loading-skeleton" style="height: 60px;"></div>
```

### 5. Error Handling
```javascript
// Use toast notifications for user feedback
try {
    await saveTransaction(data);
    designSystem.showToast('Transaction saved successfully!', 'success');
} catch (error) {
    designSystem.showToast('Failed to save transaction', 'error');
}
```

## Demo Page

Truy cập `/DesignSystemDemo` để xem tất cả các components và styles của design system.

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Performance

- CSS được tối ưu với CSS Variables
- JavaScript được lazy load
- Fonts được preload
- Images được tối ưu

## Contributing

Khi thêm components mới:

1. Sử dụng CSS Variables cho colors và spacing
2. Đảm bảo responsive design
3. Thêm accessibility support
4. Test trên multiple browsers
5. Update documentation

## License

MIT License - Xem file LICENSE để biết thêm chi tiết.
