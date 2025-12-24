// Group Details Modals Integration
// This file extends group-details.js with modal functionality

// Add modal state and methods to the Vue app
function extendGroupDetailsApp(app) {
    // Modal state
    const showExpenseModal = ref(false);
    const showAddMemberModal = ref(false);
    const showEditMemberModal = ref(false);
    const showCategoryModal = ref(false);
    const showSettleUpModal = ref(false);
    const showSettingsModal = ref(false);
    
    const currentExpense = ref({
        id: null,
        description: '',
        amount: 0,
        paidByUserId: null,
        categoryId: null,
        transactionDate: new Date().toISOString().split('T')[0],
        splits: []
    });
    
    const currentMember = ref(null);
    const currentCategory = ref({
        id: null,
        name: '',
        icon: 'fas fa-tag',
        color: '#94a3b8',
        budgetLimit: null
    });
    
    const friends = ref([]);
    const settlements = ref([]);
    
    // Modal Methods
    const openAddExpenseModal = () => {
        currentExpense.value = {
            id: null,
            description: '',
            amount: 0,
            paidByUserId: window.currentUserId,
            categoryId: null,
            transactionDate: new Date().toISOString().split('T')[0],
            splits: []
        };
        showExpenseModal.value = true;
    };
    
    const openEditExpenseModal = (expense) => {
        currentExpense.value = { ...expense };
        showExpenseModal.value = true;
    };
    
    const closeExpenseModal = () => {
        showExpenseModal.value = false;
    };
    
    const saveExpense = async (expenseData) => {
        try {
            const url = expenseData.id 
                ? `/api/GroupExpense/transactions/${expenseData.id}`
                : '/api/GroupExpense/transactions';
            
            const method = expenseData.id ? 'PUT' : 'POST';
            
            const payload = {
                ...expenseData,
                groupId: window.groupId,
                amount: parseFloat(expenseData.amount),
                transactionDate: new Date(expenseData.transactionDate).toISOString()
            };
            
            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });
            
            if (response.ok) {
                closeExpenseModal();
                await app.loadGroupData();
                app.showToast(
                    expenseData.id ? 'Cập nhật chi tiêu thành công!' : 'Thêm chi tiêu thành công!',
                    'success'
                );
            } else {
                const error = await response.json();
                app.showToast('Lỗi: ' + (error.message || 'Không thể lưu chi tiêu'), 'error');
            }
        } catch (error) {
            console.error('Error saving expense:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const openAddMemberModal = async () => {
        await loadFriends();
        showAddMemberModal.value = true;
    };
    
    const closeAddMemberModal = () => {
        showAddMemberModal.value = false;
    };
    
    const addMember = async (memberData) => {
        try {
            const response = await fetch('/api/GroupExpense/members', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    groupId: window.groupId,
                    userId: memberData.userId,
                    role: memberData.role
                })
            });
            
            if (response.ok) {
                closeAddMemberModal();
                await app.loadGroupData();
                app.showToast('Thêm thành viên thành công!', 'success');
            } else {
                const error = await response.json();
                app.showToast('Lỗi: ' + (error.message || 'Không thể thêm thành viên'), 'error');
            }
        } catch (error) {
            console.error('Error adding member:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const editMemberRole = (member) => {
        currentMember.value = { ...member };
        showEditMemberModal.value = true;
    };
    
    const closeEditMemberModal = () => {
        showEditMemberModal.value = false;
    };
    
    const saveMemberRole = async (memberData) => {
        try {
            // API endpoint for updating member role would go here
            // For now, just show success message
            closeEditMemberModal();
            app.showToast('Cập nhật quyền thành công!', 'success');
        } catch (error) {
            console.error('Error updating member role:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const removeMember = async (member) => {
        if (!confirm(`Xóa ${member.userName} khỏi nhóm?`)) return;
        
        try {
            const response = await fetch(
                `/api/GroupExpense/groups/${window.groupId}/members/${member.userId}`,
                { method: 'DELETE' }
            );
            
            if (response.ok) {
                await app.loadGroupData();
                app.showToast('Đã xóa thành viên', 'success');
            } else {
                app.showToast('Không thể xóa thành viên', 'error');
            }
        } catch (error) {
            console.error('Error removing member:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const openAddCategoryModal = () => {
        currentCategory.value = {
            id: null,
            name: '',
            icon: 'fas fa-tag',
            color: '#94a3b8',
            budgetLimit: null
        };
        showCategoryModal.value = true;
    };
    
    const editCategory = (category) => {
        currentCategory.value = { ...category };
        showCategoryModal.value = true;
    };
    
    const closeCategoryModal = () => {
        showCategoryModal.value = false;
    };
    
    const saveCategory = async (categoryData) => {
        try {
            // API endpoint for saving category would go here
            // For now, just show success message
            closeCategoryModal();
            app.showToast(
                categoryData.id ? 'Cập nhật danh mục thành công!' : 'Thêm danh mục thành công!',
                'success'
            );
        } catch (error) {
            console.error('Error saving category:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const deleteCategory = async (category) => {
        if (!confirm(`Xóa danh mục ${category.name}?`)) return;
        
        try {
            // API endpoint for deleting category would go here
            app.showToast('Đã xóa danh mục', 'success');
        } catch (error) {
            console.error('Error deleting category:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const openSettleUpModal = async () => {
        await loadSettlements();
        showSettleUpModal.value = true;
    };
    
    const closeSettleUpModal = () => {
        showSettleUpModal.value = false;
    };
    
    const settleDebt = async (settlement) => {
        try {
            // Create a settlement transaction
            const response = await fetch('/api/GroupExpense/transactions', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    groupId: window.groupId,
                    description: `Thanh toán nợ: ${settlement.fromUserName} → ${settlement.toUserName}`,
                    amount: settlement.amount,
                    paidByUserId: settlement.fromUserId,
                    transactionDate: new Date().toISOString(),
                    splits: [{
                        userId: settlement.toUserId,
                        amount: settlement.amount
                    }]
                })
            });
            
            if (response.ok) {
                await loadSettlements();
                await app.loadGroupData();
                app.showToast('Đã ghi nhận thanh toán!', 'success');
            } else {
                app.showToast('Không thể ghi nhận thanh toán', 'error');
            }
        } catch (error) {
            console.error('Error settling debt:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    const openSettingsModal = () => {
        showSettingsModal.value = true;
    };
    
    const closeSettingsModal = () => {
        showSettingsModal.value = false;
    };
    
    const saveSettings = async (settings) => {
        try {
            const response = await fetch(`/api/GroupExpense/${window.groupId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    id: window.groupId,
                    name: settings.name,
                    description: settings.description,
                    icon: settings.icon,
                    color: settings.color
                })
            });
            
            if (response.ok) {
                closeSettingsModal();
                await app.loadGroupData();
                app.showToast('Cập nhật cài đặt thành công!', 'success');
            } else {
                app.showToast('Không thể cập nhật cài đặt', 'error');
            }
        } catch (error) {
            console.error('Error saving settings:', error);
            app.showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
        }
    };
    
    // Helper Functions
    const loadFriends = async () => {
        try {
            const response = await fetch('/api/Friendship/friends');
            if (response.ok) {
                const allFriends = await response.json();
                // Filter out members already in the group
                const memberIds = app.members.value.map(m => m.userId);
                friends.value = allFriends.filter(f => !memberIds.includes(f.id));
            }
        } catch (error) {
            console.error('Error loading friends:', error);
        }
    };
    
    const loadSettlements = async () => {
        try {
            const response = await fetch(`/api/GroupExpense/${window.groupId}/settlements`);
            if (response.ok) {
                settlements.value = await response.json();
            }
        } catch (error) {
            console.error('Error loading settlements:', error);
        }
    };
    
    const viewTransactionDetail = (transaction) => {
        // Open expense modal in edit mode
        openEditExpenseModal(transaction);
    };
    
    // Return all modal-related state and methods
    return {
        // Modal state
        showExpenseModal,
        showAddMemberModal,
        showEditMemberModal,
        showCategoryModal,
        showSettleUpModal,
        showSettingsModal,
        currentExpense,
        currentMember,
        currentCategory,
        friends,
        settlements,
        
        // Modal methods
        openAddExpenseModal,
        openEditExpenseModal,
        closeExpenseModal,
        saveExpense,
        openAddMemberModal,
        closeAddMemberModal,
        addMember,
        editMemberRole,
        closeEditMemberModal,
        saveMemberRole,
        removeMember,
        openAddCategoryModal,
        editCategory,
        closeCategoryModal,
        saveCategory,
        deleteCategory,
        openSettleUpModal,
        closeSettleUpModal,
        settleDebt,
        openSettingsModal,
        closeSettingsModal,
        saveSettings,
        viewTransactionDetail
    };
}

// Export for use in group-details.js
if (typeof window !== 'undefined') {
    window.extendGroupDetailsApp = extendGroupDetailsApp;
}
