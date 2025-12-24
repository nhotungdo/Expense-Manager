# 🚀 Quick Reference - Group Spending Feature

## 📍 URLs

### Main Pages
- **Danh sách nhóm:** `/Groups` hoặc `/Groups/Index`
- **Chi tiết nhóm:** `/Groups/Details/{id}` (ví dụ: `/Groups/Details/1`)

### API Endpoints
```
GET    /api/GroupExpense                              - Lấy tất cả nhóm
GET    /api/GroupExpense/{id}                         - Lấy thông tin nhóm
POST   /api/GroupExpense                              - Tạo nhóm mới
PUT    /api/GroupExpense/{id}                         - Cập nhật nhóm
DELETE /api/GroupExpense/{id}                         - Xóa nhóm

GET    /api/GroupExpense/{groupId}/members            - Lấy danh sách thành viên
POST   /api/GroupExpense/members                      - Thêm thành viên
DELETE /api/GroupExpense/groups/{groupId}/members/{memberId} - Xóa thành viên

GET    /api/GroupExpense/{groupId}/transactions       - Lấy giao dịch
POST   /api/GroupExpense/transactions                 - Tạo giao dịch
PUT    /api/GroupExpense/transactions/{id}            - Cập nhật giao dịch
DELETE /api/GroupExpense/transactions/{id}            - Xóa giao dịch

GET    /api/GroupExpense/{groupId}/balances           - Lấy số dư
GET    /api/GroupExpense/{groupId}/settlements        - Tính toán thanh toán
GET    /api/GroupExpense/{groupId}/statistics         - Lấy thống kê
GET    /api/GroupExpense/{groupId}/budget             - Lấy ngân sách
GET    /api/GroupExpense/{groupId}/alerts             - Lấy cảnh báo
GET    /api/GroupExpense/{groupId}/categories         - Lấy danh mục
```

## ⌨️ Keyboard Shortcuts

| Phím | Chức năng |
|------|-----------|
| `Ctrl+N` | Tạo nhóm mới |
| `Ctrl+F` | Focus vào ô tìm kiếm |
| `Ctrl+E` | Xuất dữ liệu |
| `Ctrl+K` | Mở bộ lọc |
| `?` | Hiển thị phím tắt |
| `Escape` | Đóng modal |

## 🎨 CSS Variables

```css
--primary: #6366f1
--success: #10b981
--warning: #f59e0b
--danger: #ef4444
--info: #3b82f6
```

## 📦 Key Files

### Frontend
```
Pages/Groups/Index.cshtml           - Trang danh sách nhóm
Pages/Groups/Details.cshtml         - Trang chi tiết nhóm
wwwroot/js/groups.js                - Logic trang danh sách
wwwroot/js/group-details.js         - Logic trang chi tiết
wwwroot/css/groups.css              - Style trang danh sách
wwwroot/css/group-details.css       - Style trang chi tiết
```

### Backend
```
Controllers/GroupExpenseController.cs  - API endpoints
Services/GroupExpenseService.cs        - Business logic
DTOs/GroupExpenseDto.cs                - Data transfer objects
DTOs/GroupDetailsDto.cs                - Detail page DTOs
```

## 🔧 Common Tasks

### Tạo Nhóm Mới
```javascript
// Frontend
const response = await fetch('/api/GroupExpense', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        name: 'Tên nhóm',
        description: 'Mô tả',
        memberUserIds: [1, 2, 3]
    })
});
```

### Thêm Giao Dịch
```javascript
// Frontend
const response = await fetch('/api/GroupExpense/transactions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        groupId: 1,
        description: 'Ăn trưa',
        amount: 150000,
        paidByUserId: currentUserId,
        transactionDate: new Date().toISOString(),
        splits: []
    })
});
```

### Lấy Thống Kê
```javascript
// Frontend
const response = await fetch(`/api/GroupExpense/${groupId}/statistics`);
const stats = await response.json();
// stats.totalExpenses, stats.averageExpense, stats.expenseTrend
```

## 🎯 Vue 3 Components

### Main App Structure
```javascript
createApp({
    setup() {
        // State
        const groups = ref([]);
        const loading = ref(true);
        
        // Computed
        const filteredGroups = computed(() => { ... });
        
        // Methods
        const loadGroups = async () => { ... };
        
        // Lifecycle
        onMounted(async () => { ... });
        
        return { groups, loading, filteredGroups, loadGroups };
    }
}).mount('#groupsApp');
```

## 📊 Chart.js Integration

### Doughnut Chart
```javascript
new Chart(canvas, {
    type: 'doughnut',
    data: {
        labels: ['Ăn uống', 'Di chuyển', 'Mua sắm'],
        datasets: [{
            data: [100000, 50000, 75000],
            backgroundColor: ['#ef4444', '#f59e0b', '#8b5cf6']
        }]
    }
});
```

### Line Chart
```javascript
new Chart(canvas, {
    type: 'line',
    data: {
        labels: dates,
        datasets: [{
            label: 'Chi tiêu',
            data: amounts,
            borderColor: '#6366f1',
            tension: 0.4
        }]
    }
});
```

## 🔐 Authorization

### Check User Access
```csharp
// Controller
private long GetUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return long.TryParse(userIdClaim, out var userId) ? userId : 0;
}

// Verify group access
var group = await _groupExpenseService.GetGroupByIdAsync(groupId, userId);
if (group == null)
    return NotFound(new { message = "Group not found or you don't have access" });
```

## 🎨 Toast Notifications

```javascript
// Show toast
showToast('Thành công!', 'success');  // success, error, warning, info
```

## 📱 Responsive Breakpoints

```css
/* Desktop */
@media (min-width: 1024px) { ... }

/* Tablet */
@media (max-width: 1024px) { ... }

/* Mobile */
@media (max-width: 768px) { ... }
```

## 🐛 Debugging

### Check Console
```javascript
console.log('Groups:', groups.value);
console.log('Statistics:', statistics.value);
```

### Check Network
- Open DevTools → Network tab
- Filter by "GroupExpense"
- Check request/response

### Check Vue DevTools
- Install Vue DevTools extension
- Inspect component state
- Check computed properties

## 📚 Documentation Files

1. `COMPLETION-SUMMARY.md` - Tổng quan hoàn thành (Vietnamese)
2. `groups-implementation-complete.md` - Chi tiết kỹ thuật (English)
3. `groups-developer-guide.md` - Hướng dẫn developer
4. `groups-quick-start.md` - Hướng dẫn người dùng
5. `groups-features.md` - Danh sách tính năng
6. `groups-bugfixes.md` - Lịch sử sửa lỗi
7. `QUICK-REFERENCE.md` - File này

## 🚀 Quick Start

```bash
# 1. Chạy ứng dụng
cd MoneyTrackerApp
dotnet run

# 2. Mở trình duyệt
# http://localhost:5000/Groups

# 3. Đăng nhập (nếu chưa)
# http://localhost:5000/Auth/Login

# 4. Tạo nhóm mới
# Click "Tạo nhóm" hoặc nhấn Ctrl+N

# 5. Xem chi tiết nhóm
# Click vào card nhóm
```

## ✅ Testing Checklist

- [ ] Tạo nhóm mới
- [ ] Thêm thành viên
- [ ] Tạo giao dịch
- [ ] Xem chi tiết nhóm
- [ ] Kiểm tra tất cả tabs
- [ ] Kiểm tra biểu đồ
- [ ] Test trên mobile
- [ ] Test trên các trình duyệt

## 💡 Tips

1. **Performance:** Sử dụng computed properties thay vì methods cho derived data
2. **Security:** Luôn verify user access trước khi trả về data
3. **UX:** Hiển thị loading state khi fetch data
4. **Error Handling:** Luôn có try-catch và hiển thị error message
5. **Responsive:** Test trên nhiều kích thước màn hình

---

**Last Updated:** 24/12/2024
**Version:** 1.0.0
