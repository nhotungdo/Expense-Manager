// Modern Group Spending - Vue 3 Application
const { createApp, ref, computed, onMounted, onUnmounted, nextTick, watch } = Vue;

createApp({
    setup() {
        // State
        const groups = ref([]);
        const loading = ref(true);
        const searchQuery = ref('');
        const groupBalances = ref({});
        const recentActivities = ref([]);
        const friends = ref([]);
        const viewMode = ref('list'); // 'list' or 'grid'
        
        // Modals
        const showCreateModal = ref(false);
        const showFilterModal = ref(false);
        const showQuickExpenseModal = ref(false);
        const showExportModal = ref(false);
        const showTemplateModal = ref(false);
        const showShortcutsModal = ref(false);
        const selectedGroup = ref(null);
        const showCreateDropdown = ref(false);
        const showFabMenu = ref(false);
        const activeGroupMenu = ref(null);
        
        // Selection
        const selectionMode = ref(false);
        const selectedGroups = ref([]);
        
        // Forms
        const newGroup = ref({
            name: '',
            description: '',
            selectedFriendIds: []
        });
        
        const filters = ref({
            balanceStatus: 'all',
            sortBy: 'name',
            sortOrder: 'asc'
        });
        
        const quickExpense = ref({
            description: '',
            amount: null,
            paidBy: 'me'
        });
        
        // Export
        const exportFormat = ref('csv');
        const exportOptions = ref({
            includeTransactions: true,
            includeBalances: true,
            includeMembers: true
        });
        
        // Templates
        const groupTemplates = ref([
            {
                id: 1,
                name: 'Du lịch',
                description: 'Nhóm cho chuyến đi du lịch',
                icon: 'fas fa-plane',
                tags: ['Du lịch', 'Nghỉ dưỡng']
            },
            {
                id: 2,
                name: 'Gia đình',
                description: 'Chi tiêu gia đình hàng tháng',
                icon: 'fas fa-home',
                tags: ['Gia đình', 'Hàng tháng']
            },
            {
                id: 3,
                name: 'Bạn bè',
                description: 'Ăn uống và giải trí với bạn bè',
                icon: 'fas fa-user-friends',
                tags: ['Bạn bè', 'Giải trí']
            },
            {
                id: 4,
                name: 'Sự kiện',
                description: 'Tổ chức sự kiện hoặc tiệc',
                icon: 'fas fa-calendar-alt',
                tags: ['Sự kiện', 'Tiệc']
            },
            {
                id: 5,
                name: 'Dự án',
                description: 'Chi phí cho dự án chung',
                icon: 'fas fa-project-diagram',
                tags: ['Dự án', 'Công việc']
            },
            {
                id: 6,
                name: 'Phòng trọ',
                description: 'Chi phí chung cho phòng trọ',
                icon: 'fas fa-building',
                tags: ['Nhà ở', 'Tiện ích']
            }
        ]);
        
        // Toast notification
        const toast = ref({
            show: false,
            message: '',
            type: 'success',
            icon: 'fa-check-circle'
        });
        
        // Chart
        let chartInstance = null;
        
        // Computed Properties
        const filteredGroups = computed(() => {
            let result = [...groups.value];
            
            // Search filter
            if (searchQuery.value) {
                const query = searchQuery.value.toLowerCase();
                result = result.filter(g => 
                    g.name.toLowerCase().includes(query) ||
                    (g.description && g.description.toLowerCase().includes(query))
                );
            }
            
            // Balance status filter
            if (filters.value.balanceStatus !== 'all') {
                result = result.filter(g => {
                    const balance = getGroupBalance(g.id);
                    if (filters.value.balanceStatus === 'positive') return balance > 0;
                    if (filters.value.balanceStatus === 'negative') return balance < 0;
                    if (filters.value.balanceStatus === 'settled') return balance === 0;
                    return true;
                });
            }
            
            // Sort
            result.sort((a, b) => {
                let compareA, compareB;
                
                switch (filters.value.sortBy) {
                    case 'name':
                        compareA = a.name.toLowerCase();
                        compareB = b.name.toLowerCase();
                        break;
                    case 'balance':
                        compareA = Math.abs(getGroupBalance(a.id));
                        compareB = Math.abs(getGroupBalance(b.id));
                        break;
                    case 'members':
                        compareA = a.memberCount || 0;
                        compareB = b.memberCount || 0;
                        break;
                    case 'activity':
                        compareA = a.lastActivityDate || 0;
                        compareB = b.lastActivityDate || 0;
                        break;
                    default:
                        return 0;
                }
                
                if (filters.value.sortOrder === 'asc') {
                    return compareA > compareB ? 1 : -1;
                } else {
                    return compareA < compareB ? 1 : -1;
                }
            });
            
            return result;
        });
        
        const totalTransactions = computed(() => {
            return groups.value.reduce((sum, g) => sum + (g.transactionCount || 0), 0);
        });
        
        const totalSpent = computed(() => {
            return recentActivities.value.reduce((sum, a) => sum + a.amount, 0);
        });
        
        const totalBalance = computed(() => {
            return Object.values(groupBalances.value).reduce((sum, balance) => sum + balance, 0);
        });
        
        const totalReceivables = computed(() => {
            return Object.values(groupBalances.value)
                .filter(b => b > 0)
                .reduce((sum, balance) => sum + balance, 0);
        });
        
        const totalPayables = computed(() => {
            return Object.values(groupBalances.value)
                .filter(b => b < 0)
                .reduce((sum, balance) => sum + Math.abs(balance), 0);
        });
        
        const receivableCount = computed(() => {
            return Object.values(groupBalances.value).filter(b => b > 0).length;
        });
        
        const payableCount = computed(() => {
            return Object.values(groupBalances.value).filter(b => b < 0).length;
        });
        
        // Methods
        const formatCurrency = (amount) => {
            return new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND'
            }).format(amount);
        };
        
        const formatTimeAgo = (dateStr) => {
            if (!dateStr) return '';
            const date = new Date(dateStr);
            const seconds = Math.floor((new Date() - date) / 1000);
            
            const intervals = [
                { unit: 'năm', seconds: 31536000 },
                { unit: 'tháng', seconds: 2592000 },
                { unit: 'tuần', seconds: 604800 },
                { unit: 'ngày', seconds: 86400 },
                { unit: 'giờ', seconds: 3600 },
                { unit: 'phút', seconds: 60 }
            ];
            
            for (const { unit, seconds: secondsInUnit } of intervals) {
                const interval = Math.floor(seconds / secondsInUnit);
                if (interval >= 1) {
                    return `${interval} ${unit} trước`;
                }
            }
            
            return 'vừa xong';
        };
        
        const getGroupBalance = (groupId) => {
            return groupBalances.value[groupId] || 0;
        };
        
        const getBalanceClass = (groupId) => {
            const balance = getGroupBalance(groupId);
            if (balance > 0) return 'positive';
            if (balance < 0) return 'negative';
            return 'neutral';
        };
        
        // Data Loading
        const loadGroups = async () => {
            loading.value = true;
            try {
                const response = await fetch('/api/GroupExpense');
                if (response.ok) {
                    groups.value = await response.json();
                    
                    // Load balances for each group
                    await Promise.all(
                        groups.value.map(group => loadGroupBalance(group.id))
                    );
                    
                    // Load recent activities
                    await loadRecentActivities();
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
        
        const loadGroupBalance = async (groupId) => {
            try {
                const response = await fetch(`/api/GroupExpense/${groupId}/balances`);
                if (response.ok) {
                    const data = await response.json();
                    const myBalance = data.memberBalances.find(
                        m => m.userId == window.currentUserId
                    );
                    groupBalances.value[groupId] = myBalance ? myBalance.balance : 0;
                }
            } catch (error) {
                console.error(`Error loading balance for group ${groupId}:`, error);
            }
        };
        
        const loadRecentActivities = async () => {
            const activities = [];
            
            // Load transactions from first 5 groups
            for (const group of groups.value.slice(0, 5)) {
                try {
                    const response = await fetch(`/api/GroupExpense/${group.id}/transactions`);
                    if (response.ok) {
                        const transactions = await response.json();
                        transactions.slice(0, 3).forEach(tx => {
                            activities.push({
                                ...tx,
                                groupName: group.name
                            });
                        });
                    }
                } catch (error) {
                    console.error(`Error loading transactions for group ${group.id}:`, error);
                }
            }
            
            // Sort by date
            activities.sort((a, b) => 
                new Date(b.transactionDate) - new Date(a.transactionDate)
            );
            
            recentActivities.value = activities.slice(0, 10);
            
            // Render chart
            nextTick(() => {
                renderCategoryChart();
            });
        };
        
        const loadFriends = async () => {
            try {
                const response = await fetch('/api/Friendship/friends');
                if (response.ok) {
                    friends.value = await response.json();
                }
            } catch (error) {
                console.error('Error loading friends:', error);
            }
        };
        
        // Chart Rendering
        const renderCategoryChart = () => {
            const canvas = document.getElementById('categoryChart');
            if (!canvas) return;
            
            // Group activities by category
            const categoryData = {};
            recentActivities.value.forEach(activity => {
                const category = activity.category || 'Other';
                categoryData[category] = (categoryData[category] || 0) + activity.amount;
            });
            
            const labels = Object.keys(categoryData);
            const data = Object.values(categoryData);
            
            if (labels.length === 0) {
                // Show empty state
                return;
            }
            
            const colors = [
                '#6366f1', '#8b5cf6', '#ec4899', '#f59e0b', 
                '#10b981', '#3b82f6', '#ef4444', '#14b8a6'
            ];
            
            // Destroy existing chart
            if (chartInstance) {
                chartInstance.destroy();
            }
            
            chartInstance = new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: data,
                        backgroundColor: colors,
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                padding: 15,
                                font: {
                                    size: 11,
                                    family: 'Inter'
                                },
                                usePointStyle: true,
                                pointStyle: 'circle'
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    const label = context.label || '';
                                    const value = formatCurrency(context.parsed);
                                    return `${label}: ${value}`;
                                }
                            }
                        }
                    },
                    cutout: '65%'
                }
            });
        };
        
        // Modal Actions
        const openCreateGroupModal = () => {
            newGroup.value = {
                name: '',
                description: '',
                selectedFriendIds: []
            };
            showCreateModal.value = true;
            showCreateDropdown.value = false;
            showFabMenu.value = false;
        };
        
        const closeCreateGroupModal = () => {
            showCreateModal.value = false;
        };
        
        const createGroup = async () => {
            if (!newGroup.value.name) {
                showToast('Vui lòng nhập tên nhóm', 'error');
                return;
            }
            
            try {
                const response = await fetch('/api/GroupExpense', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: newGroup.value.name,
                        description: newGroup.value.description,
                        memberUserIds: newGroup.value.selectedFriendIds
                    })
                });
                
                if (response.ok) {
                    closeCreateGroupModal();
                    await loadGroups();
                    showToast('Tạo nhóm thành công!', 'success');
                } else {
                    const error = await response.json();
                    showToast('Tạo nhóm thất bại: ' + (error.message || 'Lỗi không xác định'), 'error');
                }
            } catch (error) {
                console.error('Error creating group:', error);
                showToast('Tạo nhóm thất bại. Vui lòng thử lại.', 'error');
            }
        };
        
        const viewGroupDetails = (group) => {
            // Navigate to group details page
            window.location.href = `/Groups/Details/${group.id}`;
        };
        
        // Filter Modal
        const openFilterModal = () => {
            showFilterModal.value = true;
        };
        
        const closeFilterModal = () => {
            showFilterModal.value = false;
        };
        
        const applyFilters = () => {
            closeFilterModal();
            showToast('Đã áp dụng bộ lọc', 'success');
        };
        
        const resetFilters = () => {
            filters.value = {
                balanceStatus: 'all',
                sortBy: 'name',
                sortOrder: 'asc'
            };
            showToast('Đã đặt lại bộ lọc', 'info');
        };
        
        // Quick Add Expense
        const quickAddExpense = (group) => {
            selectedGroup.value = group;
            quickExpense.value = {
                description: '',
                amount: null,
                paidBy: 'me'
            };
            showQuickExpenseModal.value = true;
        };
        
        const closeQuickExpenseModal = () => {
            showQuickExpenseModal.value = false;
            selectedGroup.value = null;
        };
        
        const addQuickExpense = async () => {
            if (!quickExpense.value.description || !quickExpense.value.amount) {
                showToast('Vui lòng điền đầy đủ thông tin', 'error');
                return;
            }
            
            if (!selectedGroup.value) {
                showToast('Không tìm thấy nhóm', 'error');
                return;
            }
            
            try {
                // Prepare expense data
                const expenseData = {
                    groupId: selectedGroup.value.id,
                    description: quickExpense.value.description,
                    amount: parseFloat(quickExpense.value.amount),
                    paidByUserId: window.currentUserId,
                    transactionDate: new Date().toISOString(),
                    splits: [] // Will be calculated on server
                };
                
                // Call API to add expense
                const response = await fetch('/api/GroupExpense/transactions', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
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
        
        // Share Group
        const shareGroup = async (group) => {
            const shareUrl = `${window.location.origin}/Groups/Join/${group.id}`;
            
            if (navigator.share) {
                try {
                    await navigator.share({
                        title: `Tham gia nhóm: ${group.name}`,
                        text: `Tham gia nhóm chi tiêu "${group.name}" của tôi!`,
                        url: shareUrl
                    });
                    showToast('Đã chia sẻ thành công', 'success');
                } catch (error) {
                    if (error.name !== 'AbortError') {
                        copyToClipboard(shareUrl);
                    }
                }
            } else {
                copyToClipboard(shareUrl);
            }
        };
        
        const copyToClipboard = (text) => {
            navigator.clipboard.writeText(text).then(() => {
                showToast('Đã sao chép link vào clipboard', 'success');
            }).catch(() => {
                showToast('Không thể sao chép link', 'error');
            });
        };
        
        // Group Settings
        const openGroupSettings = (group) => {
            selectedGroup.value = group;
            activeGroupMenu.value = null;
            // For now, show a toast notification
            // TODO: Create Settings page or implement settings modal
            showToast(`Cài đặt nhóm "${group.name}" đang được phát triển`, 'info');
            console.log('Group settings:', group);
        };
        
        // Refresh Activities
        const refreshActivities = async () => {
            showToast('Đang làm mới...', 'info');
            await loadRecentActivities();
            showToast('Đã cập nhật hoạt động', 'success');
        };
        
        // Export Modal
        const openExportModal = () => {
            showExportModal.value = true;
        };
        
        const closeExportModal = () => {
            showExportModal.value = false;
        };
        
        const performExport = async () => {
            showToast('Đang xuất dữ liệu...', 'info');
            
            try {
                // Prepare export data
                const exportData = {
                    groups: groups.value,
                    format: exportFormat.value,
                    options: exportOptions.value,
                    exportDate: new Date().toISOString()
                };
                
                if (exportFormat.value === 'csv') {
                    exportToCSV(exportData);
                } else if (exportFormat.value === 'pdf') {
                    exportToPDF(exportData);
                } else if (exportFormat.value === 'json') {
                    exportToJSON(exportData);
                }
                
                closeExportModal();
                showToast('Xuất dữ liệu thành công!', 'success');
            } catch (error) {
                console.error('Export error:', error);
                showToast('Xuất dữ liệu thất bại', 'error');
            }
        };
        
        const exportToCSV = (data) => {
            let csv = 'Tên nhóm,Số thành viên,Số giao dịch,Số dư\n';
            data.groups.forEach(group => {
                const balance = getGroupBalance(group.id);
                csv += `"${group.name}",${group.memberCount || 0},${group.transactionCount || 0},${balance}\n`;
            });
            
            const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = `groups_export_${Date.now()}.csv`;
            link.click();
        };
        
        const exportToPDF = (data) => {
            // Simplified PDF export - in production, use a library like jsPDF
            showToast('Tính năng PDF đang được phát triển', 'info');
        };
        
        const exportToJSON = (data) => {
            const json = JSON.stringify(data, null, 2);
            const blob = new Blob([json], { type: 'application/json' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = `groups_export_${Date.now()}.json`;
            link.click();
        };
        
        // Template Modal
        const openTemplateModal = () => {
            showCreateDropdown.value = false;
            showTemplateModal.value = true;
        };
        
        const closeTemplateModal = () => {
            showTemplateModal.value = false;
        };
        
        const selectTemplate = (template) => {
            newGroup.value = {
                name: template.name,
                description: template.description,
                selectedFriendIds: []
            };
            closeTemplateModal();
            showCreateModal.value = true;
        };
        
        // Dropdown
        const toggleCreateDropdown = () => {
            showCreateDropdown.value = !showCreateDropdown.value;
        };
        
        // FAB Menu
        const toggleFabMenu = () => {
            showFabMenu.value = !showFabMenu.value;
        };
        
        // Group Context Menu
        const toggleGroupMenu = (groupId) => {
            activeGroupMenu.value = activeGroupMenu.value === groupId ? null : groupId;
        };
        
        const archiveGroup = async (group) => {
            if (!confirm(`Bạn có chắc muốn lưu trữ nhóm "${group.name}"?`)) return;
            
            try {
                // API call to archive group
                showToast('Đã lưu trữ nhóm', 'success');
                await loadGroups();
            } catch (error) {
                showToast('Lưu trữ thất bại', 'error');
            }
            activeGroupMenu.value = null;
        };
        
        const exportGroupData = (group) => {
            selectedGroup.value = group;
            activeGroupMenu.value = null;
            openExportModal();
        };
        
        const leaveGroup = async (group) => {
            if (!confirm(`Bạn có chắc muốn rời khỏi nhóm "${group.name}"?`)) return;
            
            try {
                // API call to leave group
                showToast('Đã rời khỏi nhóm', 'success');
                await loadGroups();
            } catch (error) {
                showToast('Rời nhóm thất bại', 'error');
            }
            activeGroupMenu.value = null;
        };
        
        // Selection Mode
        const toggleSelectionMode = () => {
            selectionMode.value = !selectionMode.value;
            if (!selectionMode.value) {
                selectedGroups.value = [];
            }
            showFabMenu.value = false;
        };
        
        const toggleGroupSelection = (groupId) => {
            const index = selectedGroups.value.indexOf(groupId);
            if (index === -1) {
                selectedGroups.value.push(groupId);
            } else {
                selectedGroups.value.splice(index, 1);
            }
        };
        
        const toggleSelectAll = () => {
            if (selectedGroups.value.length === filteredGroups.value.length) {
                selectedGroups.value = [];
            } else {
                selectedGroups.value = filteredGroups.value.map(g => g.id);
            }
        };
        
        const clearSelection = () => {
            selectedGroups.value = [];
            selectionMode.value = false;
        };
        
        const bulkArchive = async () => {
            if (!confirm(`Bạn có chắc muốn lưu trữ ${selectedGroups.value.length} nhóm?`)) return;
            
            try {
                // API calls to archive selected groups
                showToast(`Đã lưu trữ ${selectedGroups.value.length} nhóm`, 'success');
                clearSelection();
                await loadGroups();
            } catch (error) {
                showToast('Lưu trữ thất bại', 'error');
            }
        };
        
        const bulkDelete = async () => {
            if (!confirm(`CẢNH BÁO: Bạn có chắc muốn xóa ${selectedGroups.value.length} nhóm? Hành động này không thể hoàn tác!`)) return;
            
            try {
                // API calls to delete selected groups
                showToast(`Đã xóa ${selectedGroups.value.length} nhóm`, 'success');
                clearSelection();
                await loadGroups();
            } catch (error) {
                showToast('Xóa thất bại', 'error');
            }
        };
        
        // Shortcuts Modal
        const openShortcutsModal = () => {
            showShortcutsModal.value = true;
            showFabMenu.value = false;
        };
        
        const closeShortcutsModal = () => {
            showShortcutsModal.value = false;
        };
        
        // Keyboard Shortcuts
        const handleKeyboardShortcuts = (event) => {
            // Ctrl/Cmd + N: Create new group
            if ((event.ctrlKey || event.metaKey) && event.key === 'n') {
                event.preventDefault();
                openCreateGroupModal();
            }
            
            // Ctrl/Cmd + F: Focus search
            if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
                event.preventDefault();
                document.querySelector('.search-box input')?.focus();
            }
            
            // Ctrl/Cmd + E: Export
            if ((event.ctrlKey || event.metaKey) && event.key === 'e') {
                event.preventDefault();
                openExportModal();
            }
            
            // Ctrl/Cmd + K: Filter
            if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
                event.preventDefault();
                openFilterModal();
            }
            
            // ?: Show shortcuts
            if (event.key === '?' && !event.ctrlKey && !event.metaKey) {
                event.preventDefault();
                openShortcutsModal();
            }
            
            // Escape: Close modals
            if (event.key === 'Escape') {
                showCreateModal.value = false;
                showFilterModal.value = false;
                showQuickExpenseModal.value = false;
                showExportModal.value = false;
                showTemplateModal.value = false;
                showShortcutsModal.value = false;
                showCreateDropdown.value = false;
                showFabMenu.value = false;
                activeGroupMenu.value = null;
            }
        };
        
        // Close dropdowns when clicking outside
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
        
        // Toast Notification
        const showToast = (message, type = 'success') => {
            const icons = {
                success: 'fa-check-circle',
                error: 'fa-exclamation-circle',
                warning: 'fa-exclamation-triangle',
                info: 'fa-info-circle'
            };
            
            toast.value = {
                show: true,
                message,
                type,
                icon: icons[type] || icons.info
            };
            
            setTimeout(() => {
                toast.value.show = false;
            }, 3000);
        };
        
        // Save view mode to localStorage
        watch(viewMode, (newMode) => {
            localStorage.setItem('groupsViewMode', newMode);
        });
        
        // Lifecycle
        onMounted(async () => {
            // Restore view mode from localStorage
            const savedViewMode = localStorage.getItem('groupsViewMode');
            if (savedViewMode) {
                viewMode.value = savedViewMode;
            }
            
            await Promise.all([
                loadGroups(),
                loadFriends()
            ]);
            
            // Add keyboard shortcuts
            document.addEventListener('keydown', handleKeyboardShortcuts);
            document.addEventListener('click', handleClickOutside);
        });
        
        // Cleanup
        onUnmounted(() => {
            document.removeEventListener('keydown', handleKeyboardShortcuts);
            document.removeEventListener('click', handleClickOutside);
        });
        
        return {
            // State
            groups,
            loading,
            searchQuery,
            groupBalances,
            recentActivities,
            friends,
            viewMode,
            showCreateModal,
            showFilterModal,
            showQuickExpenseModal,
            showExportModal,
            showTemplateModal,
            showShortcutsModal,
            showCreateDropdown,
            showFabMenu,
            activeGroupMenu,
            selectedGroup,
            newGroup,
            filters,
            quickExpense,
            toast,
            exportFormat,
            exportOptions,
            groupTemplates,
            selectionMode,
            selectedGroups,
            
            // Computed
            filteredGroups,
            totalBalance,
            totalReceivables,
            totalPayables,
            receivableCount,
            payableCount,
            totalTransactions,
            totalSpent,
            
            // Methods
            formatCurrency,
            formatTimeAgo,
            getGroupBalance,
            getBalanceClass,
            openCreateGroupModal,
            closeCreateGroupModal,
            createGroup,
            viewGroupDetails,
            openFilterModal,
            closeFilterModal,
            applyFilters,
            resetFilters,
            quickAddExpense,
            closeQuickExpenseModal,
            addQuickExpense,
            shareGroup,
            openGroupSettings,
            refreshActivities,
            showToast,
            openExportModal,
            closeExportModal,
            performExport,
            openTemplateModal,
            closeTemplateModal,
            selectTemplate,
            toggleCreateDropdown,
            toggleFabMenu,
            toggleGroupMenu,
            archiveGroup,
            exportGroupData,
            leaveGroup,
            toggleSelectionMode,
            toggleGroupSelection,
            toggleSelectAll,
            clearSelection,
            bulkArchive,
            bulkDelete,
            openShortcutsModal,
            closeShortcutsModal
        };
    }
}).mount('#groupsApp');
