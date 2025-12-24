// Group Details - Vue 3 Application
const { createApp, ref, computed, onMounted, onUnmounted, nextTick, watch } = Vue;

createApp({
    setup() {
        // State
        const group = ref({});
        const members = ref([]);
        const transactions = ref([]);
        const categories = ref([]);
        const statistics = ref({
            totalExpenses: 0,
            averageExpense: 0,
            expenseTrend: 0
        });
        const budget = ref({
            limit: 0,
            spent: 0
        });
        const myBalance = ref({
            receivable: 0,
            payable: 0
        });
        const budgetAlerts = ref([]);
        
        const loading = ref(true);
        const activeTab = ref('transactions');
        const transactionFilter = ref('all');
        
        // Modal state
        const showExpenseModal = ref(false);
        const showAddMemberModal = ref(false);
        const showSettleUpModal = ref(false);
        const showSettingsModal = ref(false);
        
        const currentExpense = ref({
            id: null,
            description: '',
            amount: 0,
            paidByUserId: null,
            categoryId: null,
            transactionDate: new Date().toISOString().split('T')[0]
        });
        
        const friends = ref([]);
        const friendSearch = ref('');
        const selectedFriend = ref(null);
        const newMemberRole = ref('Member');
        const settlements = ref([]);
        const groupSettings = ref({
            name: '',
            description: ''
        });
        
        // Toast
        const toast = ref({
            show: false,
            message: '',
            type: 'success',
            icon: 'fa-check-circle'
        });
        
        // Charts
        let categoryChart = null;
        let trendChart = null;
        let memberChart = null;
        
        // Computed Properties
        const activeMembers = computed(() => {
            return members.value.filter(m => m.isActive).length;
        });
        
        const budgetPercentage = computed(() => {
            if (budget.value.limit === 0) return 0;
            return Math.min(Math.round((budget.value.spent / budget.value.limit) * 100), 100);
        });
        
        const budgetStatus = computed(() => {
            const percentage = budgetPercentage.value;
            if (percentage >= 100) return 'danger';
            if (percentage >= 80) return 'warning';
            return 'success';
        });
        
        const filteredTransactions = computed(() => {
            let result = [...transactions.value];
            const now = new Date();
            
            switch (transactionFilter.value) {
                case 'week':
                    const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                    result = result.filter(t => new Date(t.transactionDate) >= weekAgo);
                    break;
                case 'month':
                    const monthAgo = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
                    result = result.filter(t => new Date(t.transactionDate) >= monthAgo);
                    break;
            }
            
            return result.sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));
        });
        
        const topCategories = computed(() => {
            const categoryTotals = {};
            
            transactions.value.forEach(t => {
                const catId = t.categoryId || 'uncategorized';
                if (!categoryTotals[catId]) {
                    const category = categories.value.find(c => c.id === catId);
                    categoryTotals[catId] = {
                        id: catId,
                        name: category ? category.name : 'Chưa phân loại',
                        amount: 0,
                        color: category ? category.color : '#94a3b8'
                    };
                }
                categoryTotals[catId].amount += t.amount;
            });
            
            const total = Object.values(categoryTotals).reduce((sum, cat) => sum + cat.amount, 0);
            
            return Object.values(categoryTotals)
                .map(cat => ({
                    ...cat,
                    percentage: total > 0 ? Math.round((cat.amount / total) * 100) : 0
                }))
                .sort((a, b) => b.amount - a.amount)
                .slice(0, 5);
        });
        
        const canManageMembers = computed(() => {
            if (!group.value || !window.currentUserId) return false;
            const currentMember = members.value.find(m => m.userId == window.currentUserId);
            return group.value.createdByUserId == window.currentUserId || 
                   (currentMember && currentMember.role === 'admin');
        });
        
        // Methods
        const formatCurrency = (amount) => {
            return new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND'
            }).format(amount);
        };
        
        const formatDate = (dateStr) => {
            const date = new Date(dateStr);
            const now = new Date();
            const diffTime = Math.abs(now - date);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            
            if (diffDays === 0) return 'Hôm nay';
            if (diffDays === 1) return 'Hôm qua';
            if (diffDays < 7) return `${diffDays} ngày trước`;
            
            return date.toLocaleDateString('vi-VN');
        };
        
        const getCategoryColor = (categoryId) => {
            const category = categories.value.find(c => c.id === categoryId);
            return category ? category.color : '#94a3b8';
        };
        
        const getCategoryIcon = (categoryId) => {
            const category = categories.value.find(c => c.id === categoryId);
            return category ? category.icon : 'fas fa-tag';
        };
        
        const getSplitClass = (transaction) => {
            const mySplit = transaction.splits?.find(s => s.userId == window.currentUserId);
            if (!mySplit) return 'neutral';
            if (transaction.paidByUserId == window.currentUserId) return 'positive';
            return 'negative';
        };
        
        const getSplitText = (transaction) => {
            const mySplit = transaction.splits?.find(s => s.userId == window.currentUserId);
            if (!mySplit) return 'Không tham gia';
            if (transaction.paidByUserId == window.currentUserId) {
                return `Bạn cho vay ${formatCurrency(transaction.amount - mySplit.amount)}`;
            }
            return `Bạn nợ ${formatCurrency(mySplit.amount)}`;
        };
        
        const getBalanceClass = (balance) => {
            if (balance > 0) return 'positive';
            if (balance < 0) return 'negative';
            return 'neutral';
        };
        
        const getBalanceLabel = (balance) => {
            if (balance > 0) return 'Được nhận';
            if (balance < 0) return 'Cần trả';
            return 'Đã thanh toán';
        };
        
        // Data Loading
        const loadGroupData = async () => {
            loading.value = true;
            try {
                await Promise.all([
                    loadGroup(),
                    loadMembers(),
                    loadTransactions(),
                    loadCategories(),
                    loadStatistics(),
                    loadBudget(),
                    loadMyBalance(),
                    loadBudgetAlerts()
                ]);
            } catch (error) {
                console.error('Error loading group data:', error);
                showToast('Lỗi tải dữ liệu', 'error');
            } finally {
                loading.value = false;
            }
        };
        
        const loadGroup = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}`);
                if (response.ok) {
                    group.value = await response.json();
                }
            } catch (error) {
                console.error('Error loading group:', error);
            }
        };
        
        const loadMembers = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/members`);
                if (response.ok) {
                    members.value = await response.json();
                }
            } catch (error) {
                console.error('Error loading members:', error);
            }
        };
        
        const loadTransactions = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/transactions`);
                if (response.ok) {
                    transactions.value = await response.json();
                    nextTick(() => {
                        renderCharts();
                    });
                }
            } catch (error) {
                console.error('Error loading transactions:', error);
            }
        };
        
        const loadCategories = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/categories`);
                if (response.ok) {
                    categories.value = await response.json();
                } else {
                    // Use default categories if API not available
                    categories.value = getDefaultCategories();
                }
            } catch (error) {
                console.error('Error loading categories:', error);
                categories.value = getDefaultCategories();
            }
        };
        
        const loadStatistics = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/statistics`);
                if (response.ok) {
                    statistics.value = await response.json();
                } else {
                    // Calculate from transactions
                    calculateStatistics();
                }
            } catch (error) {
                console.error('Error loading statistics:', error);
                calculateStatistics();
            }
        };
        
        const loadBudget = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/budget`);
                if (response.ok) {
                    budget.value = await response.json();
                } else {
                    budget.value = {
                        limit: 10000000,
                        spent: transactions.value.reduce((sum, t) => sum + t.amount, 0)
                    };
                }
            } catch (error) {
                console.error('Error loading budget:', error);
            }
        };
        
        const loadMyBalance = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/balances`);
                if (response.ok) {
                    const data = await response.json();
                    const myData = data.memberBalances.find(m => m.userId == window.currentUserId);
                    if (myData) {
                        myBalance.value = {
                            receivable: myData.balance > 0 ? myData.balance : 0,
                            payable: myData.balance < 0 ? Math.abs(myData.balance) : 0
                        };
                    }
                }
            } catch (error) {
                console.error('Error loading balance:', error);
            }
        };
        
        const loadBudgetAlerts = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}/alerts`);
                if (response.ok) {
                    budgetAlerts.value = await response.json();
                } else {
                    // Generate alerts based on budget
                    generateBudgetAlerts();
                }
            } catch (error) {
                console.error('Error loading alerts:', error);
                generateBudgetAlerts();
            }
        };
        
        // Helper Functions
        const getDefaultCategories = () => {
            return [
                { id: 1, name: 'Ăn uống', icon: 'fas fa-utensils', color: '#ef4444' },
                { id: 2, name: 'Di chuyển', icon: 'fas fa-car', color: '#f59e0b' },
                { id: 3, name: 'Mua sắm', icon: 'fas fa-shopping-bag', color: '#8b5cf6' },
                { id: 4, name: 'Giải trí', icon: 'fas fa-film', color: '#ec4899' },
                { id: 5, name: 'Nhà ở', icon: 'fas fa-home', color: '#10b981' },
                { id: 6, name: 'Khác', icon: 'fas fa-tag', color: '#94a3b8' }
            ];
        };
        
        const calculateStatistics = () => {
            const total = transactions.value.reduce((sum, t) => sum + t.amount, 0);
            const count = transactions.value.length;
            
            statistics.value = {
                totalExpenses: total,
                averageExpense: count > 0 ? total / count : 0,
                expenseTrend: Math.random() * 20 - 10 // Mock trend
            };
        };
        
        const generateBudgetAlerts = () => {
            const alerts = [];
            const percentage = budgetPercentage.value;
            
            if (percentage >= 100) {
                alerts.push({
                    id: 1,
                    severity: 'danger',
                    icon: 'fas fa-exclamation-circle',
                    title: 'Vượt ngân sách',
                    message: 'Nhóm đã vượt quá ngân sách đề ra'
                });
            } else if (percentage >= 80) {
                alerts.push({
                    id: 2,
                    severity: 'warning',
                    icon: 'fas fa-exclamation-triangle',
                    title: 'Gần đạt ngân sách',
                    message: `Đã sử dụng ${percentage}% ngân sách`
                });
            }
            
            budgetAlerts.value = alerts;
        };
        
        // Chart Rendering
        const renderCharts = () => {
            renderCategoryChart();
            renderTrendChart();
            renderMemberChart();
        };
        
        const renderCategoryChart = () => {
            const canvas = document.getElementById('categoryChart');
            if (!canvas) return;
            
            const categoryData = {};
            transactions.value.forEach(t => {
                const catId = t.categoryId || 'uncategorized';
                const category = categories.value.find(c => c.id === catId);
                const catName = category ? category.name : 'Chưa phân loại';
                categoryData[catName] = (categoryData[catName] || 0) + t.amount;
            });
            
            if (categoryChart) categoryChart.destroy();
            
            categoryChart = new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: Object.keys(categoryData),
                    datasets: [{
                        data: Object.values(categoryData),
                        backgroundColor: [
                            '#ef4444', '#f59e0b', '#8b5cf6', '#ec4899', 
                            '#10b981', '#3b82f6', '#94a3b8'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        };
        
        const renderTrendChart = () => {
            const canvas = document.getElementById('trendChart');
            if (!canvas) return;
            
            // Group by date
            const dateData = {};
            transactions.value.forEach(t => {
                const date = new Date(t.transactionDate).toLocaleDateString('vi-VN');
                dateData[date] = (dateData[date] || 0) + t.amount;
            });
            
            if (trendChart) trendChart.destroy();
            
            trendChart = new Chart(canvas, {
                type: 'line',
                data: {
                    labels: Object.keys(dateData).slice(-30),
                    datasets: [{
                        label: 'Chi tiêu',
                        data: Object.values(dateData).slice(-30),
                        borderColor: '#6366f1',
                        backgroundColor: 'rgba(99, 102, 241, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        };
        
        const renderMemberChart = () => {
            const canvas = document.getElementById('memberChart');
            if (!canvas) return;
            
            const memberData = {};
            transactions.value.forEach(t => {
                memberData[t.paidByUserName] = (memberData[t.paidByUserName] || 0) + t.amount;
            });
            
            if (memberChart) memberChart.destroy();
            
            memberChart = new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: Object.keys(memberData),
                    datasets: [{
                        label: 'Đã trả',
                        data: Object.values(memberData),
                        backgroundColor: '#10b981'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        };
        
        // Actions
        const goBack = () => {
            window.location.href = '/Groups';
        };
        
        const openAddExpenseModal = () => {
            currentExpense.value = {
                id: null,
                description: '',
                amount: 0,
                paidByUserId: window.currentUserId,
                categoryId: null,
                transactionDate: new Date().toISOString().split('T')[0]
            };
            showExpenseModal.value = true;
        };
        
        const closeExpenseModal = () => {
            showExpenseModal.value = false;
        };
        
        const saveExpense = async () => {
            if (!currentExpense.value.description || !currentExpense.value.amount) {
                showToast('Vui lòng điền đầy đủ thông tin', 'error');
                return;
            }
            
            try {
                const url = currentExpense.value.id 
                    ? `/api/GroupExpense/transactions/${currentExpense.value.id}`
                    : '/api/GroupExpense/transactions';
                
                const method = currentExpense.value.id ? 'PUT' : 'POST';
                
                const payload = {
                    ...currentExpense.value,
                    groupId: window.groupId,
                    amount: parseFloat(currentExpense.value.amount),
                    transactionDate: new Date(currentExpense.value.transactionDate).toISOString(),
                    splits: [] // Equal split by default
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
                    await loadGroupData();
                    showToast(
                        currentExpense.value.id ? 'Cập nhật chi tiêu thành công!' : 'Thêm chi tiêu thành công!',
                        'success'
                    );
                } else {
                    const error = await response.json();
                    showToast('Lỗi: ' + (error.message || 'Không thể lưu chi tiêu'), 'error');
                }
            } catch (error) {
                console.error('Error saving expense:', error);
                showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
            }
        };
        
        const openSettingsModal = () => {
            groupSettings.value = {
                name: group.value.name || '',
                description: group.value.description || ''
            };
            showSettingsModal.value = true;
        };
        
        const closeSettingsModal = () => {
            showSettingsModal.value = false;
        };
        
        const saveSettings = async () => {
            try {
                const response = await fetch(`/api/GroupExpense/${window.groupId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        id: window.groupId,
                        name: groupSettings.value.name,
                        description: groupSettings.value.description
                    })
                });
                
                if (response.ok) {
                    closeSettingsModal();
                    await loadGroupData();
                    showToast('Cập nhật cài đặt thành công!', 'success');
                } else {
                    showToast('Không thể cập nhật cài đặt', 'error');
                }
            } catch (error) {
                console.error('Error saving settings:', error);
                showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
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
                    await loadGroupData();
                    showToast('Đã ghi nhận thanh toán!', 'success');
                } else {
                    showToast('Không thể ghi nhận thanh toán', 'error');
                }
            } catch (error) {
                console.error('Error settling debt:', error);
                showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
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
        
        const openAddMemberModal = async () => {
            await loadFriends();
            selectedFriend.value = null;
            newMemberRole.value = 'Member';
            showAddMemberModal.value = true;
        };
        
        const closeAddMemberModal = () => {
            showAddMemberModal.value = false;
        };
        
        const addMember = async () => {
            if (!selectedFriend.value) return;
            
            try {
                const response = await fetch('/api/GroupExpense/members', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        groupId: window.groupId,
                        userId: selectedFriend.value.id,
                        role: newMemberRole.value
                    })
                });
                
                if (response.ok) {
                    closeAddMemberModal();
                    await loadGroupData();
                    showToast('Thêm thành viên thành công!', 'success');
                } else {
                    const error = await response.json();
                    showToast('Lỗi: ' + (error.message || 'Không thể thêm thành viên'), 'error');
                }
            } catch (error) {
                console.error('Error adding member:', error);
                showToast('Lỗi kết nối. Vui lòng thử lại.', 'error');
            }
        };
        
        const loadFriends = async () => {
            try {
                const response = await fetch('/api/Friendship/friends');
                if (response.ok) {
                    const allFriends = await response.json();
                    // Filter out members already in the group
                    const memberIds = members.value.map(m => m.userId);
                    friends.value = allFriends.filter(f => !memberIds.includes(f.id));
                }
            } catch (error) {
                console.error('Error loading friends:', error);
            }
        };
        
        const filteredFriends = computed(() => {
            if (!friendSearch.value) return friends.value;
            
            const query = friendSearch.value.toLowerCase();
            return friends.value.filter(f => 
                f.userName.toLowerCase().includes(query) ||
                f.email.toLowerCase().includes(query)
            );
        });
        
        const openAddCategoryModal = () => {
            showToast('Chức năng đang được phát triển', 'info');
        };
        
        const viewTransactionDetail = (transaction) => {
            console.log('Transaction detail:', transaction);
            showToast('Chi tiết giao dịch', 'info');
        };
        
        const editMemberRole = (member) => {
            showToast(`Chỉnh sửa quyền cho ${member.userName}`, 'info');
        };
        
        const removeMember = (member) => {
            if (confirm(`Xóa ${member.userName} khỏi nhóm?`)) {
                showToast('Chức năng đang được phát triển', 'info');
            }
        };
        
        const editCategory = (category) => {
            showToast(`Chỉnh sửa danh mục ${category.name}`, 'info');
        };
        
        const deleteCategory = (category) => {
            if (confirm(`Xóa danh mục ${category.name}?`)) {
                showToast('Chức năng đang được phát triển', 'info');
            }
        };
        
        const exportData = () => {
            showToast('Đang xuất dữ liệu...', 'info');
        };
        
        const shareGroup = () => {
            const shareUrl = `${window.location.origin}/Groups/Join/${window.groupId}`;
            if (navigator.share) {
                navigator.share({
                    title: group.value.name,
                    url: shareUrl
                });
            } else {
                navigator.clipboard.writeText(shareUrl);
                showToast('Đã sao chép link', 'success');
            }
        };
        
        // Toast
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
        
        // Lifecycle
        onMounted(async () => {
            await loadGroupData();
        });
        
        onUnmounted(() => {
            if (categoryChart) categoryChart.destroy();
            if (trendChart) trendChart.destroy();
            if (memberChart) memberChart.destroy();
        });
        
        return {
            group,
            members,
            transactions,
            categories,
            statistics,
            budget,
            myBalance,
            budgetAlerts,
            loading,
            activeTab,
            transactionFilter,
            toast,
            activeMembers,
            budgetPercentage,
            budgetStatus,
            filteredTransactions,
            topCategories,
            canManageMembers,
            formatCurrency,
            formatDate,
            getCategoryColor,
            getCategoryIcon,
            getSplitClass,
            getSplitText,
            getBalanceClass,
            getBalanceLabel,
            goBack,
            openAddExpenseModal,
            closeExpenseModal,
            saveExpense,
            openSettingsModal,
            closeSettingsModal,
            saveSettings,
            openSettleUpModal,
            closeSettleUpModal,
            settleDebt,
            openAddMemberModal,
            closeAddMemberModal,
            addMember,
            openAddCategoryModal,
            viewTransactionDetail,
            editMemberRole,
            removeMember,
            editCategory,
            deleteCategory,
            exportData,
            shareGroup,
            // Modal state
            showExpenseModal,
            showAddMemberModal,
            showSettleUpModal,
            showSettingsModal,
            currentExpense,
            friends,
            friendSearch,
            selectedFriend,
            newMemberRole,
            settlements,
            groupSettings,
            filteredFriends
        };
    }
}).mount('#groupDetailsApp');
