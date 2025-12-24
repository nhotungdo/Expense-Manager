# Chi tiêu nhóm - Sửa lỗi

## 🐛 Danh sách lỗi đã sửa

### 1. ✅ **Lỗi 404 khi click vào nhóm**
**Vấn đề**: Khi click vào một nhóm, trang chuyển đến `/Groups/Details/{id}` nhưng trang này chưa tồn tại, gây lỗi 404.

**Nguyên nhân**: Hàm `viewGroupDetails()` cố gắng điều hướng đến trang chi tiết chưa được tạo.

**Giải pháp**: 
- Thay đổi hàm để hiển thị thông báo thay vì điều hướng
- Thêm TODO comment để nhắc nhở tạo trang chi tiết sau
- Log thông tin nhóm vào console để debug

```javascript
// Trước
const viewGroupDetails = (group) => {
    window.location.href = `/Groups/Details/${group.id}`;
};

// Sau
const viewGroupDetails = (group) => {
    showToast(`Chi tiết nhóm "${group.name}" đang được phát triển`, 'info');
    console.log('Group details:', group);
};
```

---

### 2. ✅ **Lỗi 404 khi mở cài đặt nhóm**
**Vấn đề**: Tương tự lỗi trên, khi click "Cài đặt" trong menu ngữ cảnh.

**Nguyên nhân**: Hàm `openGroupSettings()` cố gắng điều hướng đến trang cài đặt chưa tồn tại.

**Giải pháp**:
- Thay đổi để hiển thị thông báo
- Đóng menu ngữ cảnh sau khi click
- Log thông tin để debug

```javascript
// Trước
const openGroupSettings = (group) => {
    selectedGroup.value = group;
    window.location.href = `/Groups/Settings/${group.id}`;
};

// Sau
const openGroupSettings = (group) => {
    selectedGroup.value = group;
    activeGroupMenu.value = null;
    showToast(`Cài đặt nhóm "${group.name}" đang được phát triển`, 'info');
    console.log('Group settings:', group);
};
```

---

### 3. ✅ **Cải thiện xử lý lỗi khi tải dữ liệu**
**Vấn đề**: Không có thông báo lỗi rõ ràng khi API thất bại.

**Giải pháp**:
- Thêm kiểm tra response status
- Hiển thị toast notification khi có lỗi
- Log chi tiết lỗi vào console

```javascript
const loadGroups = async () => {
    loading.value = true;
    try {
        const response = await fetch('/api/GroupExpense');
        if (response.ok) {
            // ... xử lý dữ liệu
        } else {
            console.error('Failed to load groups:', response.status);
            showToast('Không thể tải danh sách nhóm', 'error');
        }
    } catch (error) {
        console.error('Error loading groups:', error);
        showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
    } finally {
        loading.value = false;
    }
};
```

---

### 4. ✅ **Cải thiện chức năng thêm chi tiêu nhanh**
**Vấn đề**: Chức năng chỉ hiển thị thông báo giả, không thực sự gọi API.

**Giải pháp**:
- Thêm validation đầy đủ
- Gọi API thực tế để thêm chi tiêu
- Xử lý lỗi từ server
- Reload dữ liệu sau khi thành công

```javascript
const addQuickExpense = async () => {
    // Validation
    if (!quickExpense.value.description || !quickExpense.value.amount) {
        showToast('Vui lòng điền đầy đủ thông tin', 'error');
        return;
    }
    
    if (!selectedGroup.value) {
        showToast('Không tìm thấy nhóm', 'error');
        return;
    }
    
    try {
        // Prepare data
        const expenseData = {
            groupId: selectedGroup.value.id,
            description: quickExpense.value.description,
            amount: parseFloat(quickExpense.value.amount),
            paidByUserId: window.currentUserId,
            transactionDate: new Date().toISOString(),
            splits: []
        };
        
        // Call API
        const response = await fetch('/api/GroupExpense/transactions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(expenseData)
        });
        
        if (response.ok) {
            closeQuickExpenseModal();
            showToast('Đã thêm chi tiêu thành công!', 'success');
            await loadGroups();
        } else {
            const error = await response.json();
            showToast('Thêm chi tiêu thất bại: ' + (error.message || 'Lỗi không xác định'), 'error');
        }
    } catch (error) {
        console.error('Error adding expense:', error);
        showToast('Thêm chi tiêu thất bại', 'error');
    }
};
```

---

### 5. ✅ **Sửa lỗi đóng dropdown khi click bên ngoài**
**Vấn đề**: Sử dụng `closest()` có thể gây lỗi trong một số trường hợp.

**Giải pháp**:
- Sử dụng `querySelector` và `contains()` an toàn hơn
- Kiểm tra null trước khi sử dụng
- Xử lý riêng cho từng loại dropdown

```javascript
const handleClickOutside = (event) => {
    // Close create dropdown
    const dropdownWrapper = document.querySelector('.dropdown-wrapper');
    if (dropdownWrapper && !dropdownWrapper.contains(event.target)) {
        showCreateDropdown.value = false;
    }
    
    // Close group context menus
    const groupActions = document.querySelectorAll('.group-card-actions');
    let clickedInsideActions = false;
    groupActions.forEach(action => {
        if (action.contains(event.target)) {
            clickedInsideActions = true;
        }
    });
    
    if (!clickedInsideActions) {
        activeGroupMenu.value = null;
    }
};
```

---

### 6. ✅ **Đóng FAB menu khi mở modal**
**Vấn đề**: FAB menu vẫn mở khi modal xuất hiện.

**Giải pháp**:
- Đóng FAB menu khi mở modal tạo nhóm
- Đóng dropdown khi mở modal

```javascript
const openCreateGroupModal = () => {
    newGroup.value = {
        name: '',
        description: '',
        selectedFriendIds: []
    };
    showCreateModal.value = true;
    showCreateDropdown.value = false;  // Thêm dòng này
    showFabMenu.value = false;          // Thêm dòng này
};
```

---

## 📋 Checklist kiểm tra

### Đã sửa
- [x] Lỗi 404 khi click vào nhóm
- [x] Lỗi 404 khi mở cài đặt
- [x] Xử lý lỗi khi tải dữ liệu
- [x] Thêm chi tiêu nhanh thực tế
- [x] Đóng dropdown an toàn
- [x] Đóng FAB menu khi mở modal

### Cần làm sau
- [ ] Tạo trang chi tiết nhóm (`/Groups/Details/{id}`)
- [ ] Tạo trang cài đặt nhóm (`/Groups/Settings/{id}`)
- [ ] Tạo trang tham gia nhóm (`/Groups/Join/{id}`)
- [ ] Hoàn thiện API endpoints
- [ ] Thêm unit tests
- [ ] Thêm E2E tests

---

## 🧪 Cách test

### Test lỗi 404
1. Mở trang Groups
2. Click vào một nhóm
3. **Kết quả mong đợi**: Hiện toast "Chi tiết nhóm đang được phát triển"
4. **Không còn**: Lỗi 404

### Test thêm chi tiêu
1. Click icon "+" trên thẻ nhóm
2. Nhập mô tả và số tiền
3. Click "Thêm chi tiêu"
4. **Kết quả mong đợi**: 
   - Nếu API hoạt động: Thêm thành công, reload dữ liệu
   - Nếu API chưa có: Hiện lỗi rõ ràng

### Test dropdown
1. Click nút "Tạo nhóm" (có mũi tên)
2. Dropdown hiện ra
3. Click bên ngoài dropdown
4. **Kết quả mong đợi**: Dropdown đóng lại

### Test FAB menu
1. Click nút FAB (góc phải dưới)
2. Menu hiện ra
3. Click "Tạo nhóm"
4. **Kết quả mong đợi**: 
   - Modal mở
   - FAB menu đóng

---

## 📊 Tác động

### Trước khi sửa
- ❌ Lỗi 404 khi click vào nhóm
- ❌ Lỗi 404 khi mở cài đặt
- ⚠️ Không có thông báo lỗi rõ ràng
- ⚠️ Thêm chi tiêu chỉ là giả lập
- ⚠️ Dropdown đôi khi không đóng

### Sau khi sửa
- ✅ Không còn lỗi 404
- ✅ Thông báo rõ ràng cho user
- ✅ Xử lý lỗi đầy đủ
- ✅ Thêm chi tiêu thực tế (nếu API có)
- ✅ Dropdown hoạt động ổn định

---

## 🎯 Kết luận

Tất cả các lỗi quan trọng đã được sửa. Ứng dụng giờ đây:
- Không còn lỗi 404
- Có xử lý lỗi tốt hơn
- Thông báo rõ ràng cho người dùng
- Sẵn sàng tích hợp với backend API

**Trạng thái**: ✅ **ĐÃ SỬA XONG**

---

*Cập nhật: December 2024*  
*Version: 2.0.1*
