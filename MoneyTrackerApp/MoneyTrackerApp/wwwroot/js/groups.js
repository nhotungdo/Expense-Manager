const { createApp, ref, computed, onMounted, watch, nextTick } = Vue;

const app = createApp({
    setup() {
        // --- State ---
        const currentView = ref('dashboard'); // 'dashboard' | 'groupDetails'
        const groups = ref([]);
        const loading = ref(true);
        const searchQuery = ref('');
        const groupBalances = ref({}); // { groupId: balance }
        const recentActivities = ref([]);
        const categories = ref([]); // For Chart

        // Selected Group State
        const selectedGroup = ref(null);
        const groupTransactions = ref([]);
        const settlements = ref([]);
        const currentUserBalance = ref(0);

        // Friends & New Group
        const friends = ref([]);
        const newGroup = ref({ name: '', description: '', selectedFriendIds: [] });

        // Add Member
        const selectedMembersToAdd = ref([]);

        // Transaction Forms
        const expenseForm = ref({
            description: '',
            amount: null,
            paidByUserId: null,
            splitType: 'equal', // 'equal' | 'amount'
            selectedMemberIds: [],
            manualSplits: {}
        });

        const settleUpForm = ref({
            payerId: null,
            payeeId: null,
            amount: null
        });

        // --- Computed ---
        const filteredGroups = computed(() => {
            if (!searchQuery.value) return groups.value;
            const q = searchQuery.value.toLowerCase();
            return groups.value.filter(g =>
                g.name.toLowerCase().includes(q) ||
                (g.description && g.description.toLowerCase().includes(q))
            );
        });

        const totalReceivables = computed(() => {
            return Object.values(groupBalances.value)
                .filter(b => b > 0)
                .reduce((acc, curr) => acc + curr, 0);
        });

        const totalPayables = computed(() => {
            return Object.values(groupBalances.value)
                .filter(b => b < 0)
                .reduce((acc, curr) => acc + Math.abs(curr), 0);
        });

        const totalBalance = computed(() => {
            return Object.values(groupBalances.value)
                .reduce((acc, curr) => acc + curr, 0);
        });

        const manualSplitTotal = computed(() => {
            if (expenseForm.value.splitType !== 'amount') return 0;
            return Object.values(expenseForm.value.manualSplits).reduce((sum, val) => sum + (val || 0), 0);
        });

        const availableFriendsToAdd = computed(() => {
            if (!selectedGroup.value) return [];
            const currentMemberIds = selectedGroup.value.members.map(m => m.userId);
            return friends.value.filter(f => !currentMemberIds.includes(f.friendId));
        });

        // --- Methods ---

        // Formatting
        const formatCurrency = (amount) => {
            return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
        };

        const formatTimeAgo = (dateStr) => {
            if (!dateStr) return '';
            const date = new Date(dateStr);
            const seconds = Math.floor((new Date() - date) / 1000);

            let interval = seconds / 31536000;
            if (interval > 1) return Math.floor(interval) + " năm trước";
            interval = seconds / 2592000;
            if (interval > 1) return Math.floor(interval) + " tháng trước";
            interval = seconds / 86400;
            if (interval > 1) return Math.floor(interval) + " ngày trước";
            interval = seconds / 3600;
            if (interval > 1) return Math.floor(interval) + " giờ trước";
            interval = seconds / 60;
            if (interval > 1) return Math.floor(interval) + " phút trước";
            return "Vừa xong";
        };

        const getGroupBalance = (groupId) => {
            return groupBalances.value[groupId] || 0;
        };

        // Navigation
        const navigateToGroup = async (group) => {
            loading.value = true;
            try {
                selectedGroup.value = group;
                // Load details
                await Promise.all([
                    loadGroupTransactions(group.id),
                    fetchDetailGroupBalance(group.id)
                ]);
                currentView.value = 'groupDetails';
                // Scroll to top
                window.scrollTo(0, 0);
            } catch (error) {
                console.error("Failed to load group details", error);
            } finally {
                loading.value = false;
            }
        };

        const backToDashboard = () => {
            currentView.value = 'dashboard';
            selectedGroup.value = null;
            // Refresh dashboard data silently
            loadGroups();
        };

        // Data Loading
        const loadGroups = async () => {
            try {
                const res = await fetch('/api/GroupExpense');
                if (!res.ok) throw new Error('Failed to load groups');
                groups.value = await res.json();

                // Load basic balances
                await Promise.all(groups.value.map(g => fetchGroupSummaryBalance(g.id)));

                // Load activities
                await loadQuickActivities();
            } catch (err) {
                console.error(err);
            } finally {
                loading.value = false;
            }
        };

        const fetchGroupSummaryBalance = async (groupId) => {
            try {
                const res = await fetch(`/api/GroupExpense/${groupId}/balances`);
                if (res.ok) {
                    const data = await res.json();
                    const myBalanceObj = data.memberBalances.find(m => m.userId == window.currentUserId);
                    groupBalances.value[groupId] = myBalanceObj ? myBalanceObj.balance : 0;
                }
            } catch (e) { }
        };

        const fetchDetailGroupBalance = async (groupId) => {
            try {
                const res = await fetch(`/api/GroupExpense/${groupId}/balances`);
                if (res.ok) {
                    const data = await res.json();
                    settlements.value = data.settlements || [];
                    const myBalanceObj = data.memberBalances.find(m => m.userId == window.currentUserId);
                    currentUserBalance.value = myBalanceObj ? myBalanceObj.balance : 0;
                }
            } catch (e) {
                console.error(e);
            }
        };

        const loadGroupTransactions = async (groupId) => {
            try {
                const res = await fetch(`/api/GroupExpense/${groupId}/transactions`);
                if (res.ok) {
                    const txs = await res.json();
                    txs.sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));
                    groupTransactions.value = txs;
                }
            } catch (e) {
                console.error(e);
                groupTransactions.value = [];
            }
        };

        const loadQuickActivities = async () => {
            if (groups.value.length === 0) return;

            const allTxs = [];
            const catStats = {};

            // Limit to first 5 groups for dashboard
            for (const group of groups.value.slice(0, 5)) {
                try {
                    const res = await fetch(`/api/GroupExpense/${group.id}/transactions`);
                    if (res.ok) {
                        const txs = await res.json();
                        txs.forEach(t => {
                            t.groupName = group.name;
                            allTxs.push(t);
                            // Simple categorization
                            // Assuming description might hint category or adding a random one for demo if not present
                            const cat = "Chi tiêu chung";
                            catStats[cat] = (catStats[cat] || 0) + t.amount;
                        });
                    }
                } catch (e) { }
            }
            allTxs.sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));
            recentActivities.value = allTxs.slice(0, 10);

            // Setup Chart Data
            categories.value = Object.keys(catStats).map(k => ({
                label: k,
                value: catStats[k]
            }));

            initChart();
        };

        // Charts
        const initChart = async () => {
            await nextTick();
            const ctx = document.getElementById('categoryChart');
            if (ctx && categories.value.length > 0) {
                // Destroy old chart if exists (simple way: replace canvas)
                // For now, assume fresh render
                new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: categories.value.map(c => c.label),
                        datasets: [{
                            data: categories.value.map(c => c.value),
                            backgroundColor: [
                                '#4f46e5', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'
                            ],
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { position: 'right' }
                        }
                    }
                });
            }
        };

        // Friends
        const loadFriends = async () => {
            try {
                const res = await fetch('/api/Friendship/friends');
                if (res.ok) {
                    friends.value = await res.json();
                }
            } catch (e) {
                console.warn("Friend API check failed", e);
            }
        };

        // Actions - Create Group
        const showCreateGroupModal = () => {
            newGroup.value = { name: '', description: '', selectedFriendIds: [] };
            new bootstrap.Modal(document.getElementById('createGroupModal')).show();
        };

        const createGroup = async () => {
            try {
                const res = await fetch('/api/GroupExpense', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        name: newGroup.value.name,
                        description: newGroup.value.description,
                        memberUserIds: newGroup.value.selectedFriendIds
                    })
                });
                if (res.ok) {
                    bootstrap.Modal.getInstance(document.getElementById('createGroupModal')).hide();
                    loadGroups();
                } else {
                    alert('Không thể tạo nhóm');
                }
            } catch (e) {
                console.error(e);
            }
        };

        // Actions - Expenses
        const showAddExpenseModal = () => {
            if (!selectedGroup.value) return;
            const myId = parseInt(window.currentUserId);
            expenseForm.value = {
                description: '',
                amount: null,
                paidByUserId: myId,
                splitType: 'equal',
                selectedMemberIds: selectedGroup.value.members.map(m => m.userId), // Default all
                manualSplits: {}
            };
            new bootstrap.Modal(document.getElementById('addExpenseModal')).show();
        };

        const toggleMemberSelection = (userId) => {
            const idx = expenseForm.value.selectedMemberIds.indexOf(userId);
            if (idx === -1) {
                expenseForm.value.selectedMemberIds.push(userId);
            } else {
                expenseForm.value.selectedMemberIds.splice(idx, 1);
            }
        };

        const saveExpense = async () => {
            if (!expenseForm.value.description || !expenseForm.value.amount || expenseForm.value.amount <= 0) {
                alert("Vui lòng nhập mô tả và số tiền hợp lệ"); return;
            }

            // Build payload
            const splits = [];
            if (expenseForm.value.splitType === 'equal') {
                const count = expenseForm.value.selectedMemberIds.length;
                if (count === 0) { alert("Chọn ít nhất một người để chia tiền"); return; }
                const share = expenseForm.value.amount / count;
                expenseForm.value.selectedMemberIds.forEach(uid => {
                    splits.push({ userId: uid, amount: share });
                });
            } else {
                if (manualSplitTotal.value !== expenseForm.value.amount) {
                    alert(`Tổng tiền chia (${manualSplitTotal.value}) phải bằng tổng chi tiêu (${expenseForm.value.amount})`);
                    return;
                }
                for (const [uid, amt] of Object.entries(expenseForm.value.manualSplits)) {
                    if (amt > 0) splits.push({ userId: parseInt(uid), amount: amt });
                }
            }

            const payload = {
                groupId: selectedGroup.value.id,
                description: expenseForm.value.description,
                amount: expenseForm.value.amount,
                paidByUserId: expenseForm.value.paidByUserId,
                transactionDate: new Date().toISOString(),
                splits: splits
            };

            try {
                const res = await fetch('/api/GroupExpense/transactions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (res.ok) {
                    bootstrap.Modal.getInstance(document.getElementById('addExpenseModal')).hide();
                    // Refresh details
                    navigateToGroup(selectedGroup.value);
                } else {
                    const err = await res.json();
                    alert("Lỗi: " + (err.message || 'Không xác định'));
                }
            } catch (e) {
                console.error(e);
            }
        };

        // Actions - Settlement
        const showSettleUpModal = () => {
            // Smart defaults
            const myId = parseInt(window.currentUserId);

            // Default: I pay someone I owe most
            let payer = myId;
            let payee = null;
            let amt = null;

            // Find my debts
            const myDebts = settlements.value.filter(s => s.fromUserId == myId);
            if (myDebts.length > 0) {
                // Sort by amount desc
                myDebts.sort((a, b) => b.amount - a.amount);
                payee = myDebts[0].toUserId;
                amt = myDebts[0].amount;
            } else {
                // Find who owes me
                const owedToMe = settlements.value.filter(s => s.toUserId == myId);
                if (owedToMe.length > 0) {
                    owedToMe.sort((a, b) => b.amount - a.amount);
                    payer = owedToMe[0].fromUserId;
                    payee = myId;
                    amt = owedToMe[0].amount;
                }
            }

            settleUpForm.value = {
                payerId: payer,
                payeeId: payee || (selectedGroup.value.members[0].userId == payer ? selectedGroup.value.members[1]?.userId : selectedGroup.value.members[0]?.userId),
                amount: amt
            };
            new bootstrap.Modal(document.getElementById('settleUpModal')).show();
        };

        const saveSettlement = async () => {
            if (!settleUpForm.value.amount || settleUpForm.value.amount <= 0) { alert("Số tiền không hợp lệ"); return; }

            const payload = {
                groupId: selectedGroup.value.id,
                description: "Thanh toán nợ",
                amount: settleUpForm.value.amount,
                paidByUserId: settleUpForm.value.payerId,
                transactionDate: new Date().toISOString(),
                splits: [{ userId: settleUpForm.value.payeeId, amount: settleUpForm.value.amount }]
            };

            try {
                const res = await fetch('/api/GroupExpense/transactions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (res.ok) {
                    bootstrap.Modal.getInstance(document.getElementById('settleUpModal')).hide();
                    navigateToGroup(selectedGroup.value);
                } else {
                    alert("Ghi nhận thanh toán thất bại");
                }
            } catch (e) {
                console.error(e);
            }
        };

        // Add Member Feature
        const showAddMemberModal = () => {
            selectedMembersToAdd.value = [];
            new bootstrap.Modal(document.getElementById('addMemberModal')).show();
        };

        const addMembersToGroup = async () => {
            if (!selectedGroup.value || selectedMembersToAdd.value.length === 0) return;

            try {
                // We need to call AddMember for each selection, or bulk if backend supported.
                // Current backend: [HttpPost("members")] accepts AddGroupMemberDto (one at a time)
                // We will loop.
                const promises = selectedMembersToAdd.value.map(uid =>
                    fetch('/api/GroupExpense/members', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ groupId: selectedGroup.value.id, userId: uid })
                    })
                );

                await Promise.all(promises);

                // Refresh
                bootstrap.Modal.getInstance(document.getElementById('addMemberModal')).hide();

                // Reload group details
                const res = await fetch(`/api/GroupExpense/${selectedGroup.value.id}`);
                const updatedGroup = await res.json();

                // Update local state and current view
                const idx = groups.value.findIndex(g => g.id == updatedGroup.id);
                if (idx !== -1) groups.value[idx] = updatedGroup;

                selectedGroup.value = updatedGroup; // Refresh members list in UI

            } catch (e) {
                console.error("Add members failed", e);
                alert("Có lỗi xảy ra khi thêm thành viên.");
            }
        };

        const getUserSplit = (tx) => {
            const myId = parseInt(window.currentUserId);
            const mySplit = tx.splits.find(s => s.userId == myId);
            const myShareAmount = mySplit ? mySplit.amount : 0;

            if (tx.paidByUserId == myId) {
                return tx.amount - myShareAmount;
            } else {
                return -myShareAmount;
            }
        };

        // Init
        onMounted(() => {
            if (window.currentUserId) {
                loadGroups();
                loadFriends();
            }
            // Auto refresh interval could be added here
        });

        return {
            loading,
            currentView,
            groups,
            filteredGroups,
            searchQuery,
            friends,
            newGroup,
            selectedGroup,
            groupTransactions,
            settlements,
            currentUserBalance,
            expenseForm,
            settleUpForm,
            recentActivities,
            categories,

            // Add Member
            showAddMemberModal,
            addMembersToGroup,
            availableFriendsToAdd,
            selectedMembersToAdd,

            // Computed Integers
            totalBalance,
            totalReceivables,
            totalPayables,
            manualSplitTotal,

            // Functions
            formatCurrency,
            formatTimeAgo,
            getGroupBalance,
            getUserSplit,

            // Nav
            navigateToGroup,
            backToDashboard,

            // Actions
            showCreateGroupModal,
            createGroup,
            showAddExpenseModal,
            toggleMemberSelection,
            saveExpense,
            showSettleUpModal,
            saveSettlement
        };
    }
});

app.mount('#group-spending-app');
