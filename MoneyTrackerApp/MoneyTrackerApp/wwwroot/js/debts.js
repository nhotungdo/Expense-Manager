const { createApp, ref, computed, onMounted, reactive } = Vue;

createApp({
    setup() {
        // State
        const isLoading = ref(true);
        const debts = ref([]);
        const accounts = ref([]);
        const summary = ref({
            totalDebts: 0,
            activeDebts: 0,
            totalIOwe: 0,
            totalTheyOweMe: 0,
            netDebt: 0,
            iOweThem: [],
            theyOweMe: []
        });

        const searchQuery = ref('');
        const currentTab = ref('all'); // 'all', 'borrow', 'lend'
        const isEditing = ref(false);
        const selectedDebt = ref(null);

        // Forms
        const debtForm = reactive({
            id: null,
            debtType: 1, // 1: Borrow, 2: Lend
            name: '',
            personName: '',
            initialAmount: null,
            interestRate: 0,
            startDate: new Date().toISOString().split('T')[0],
            dueDate: ''
        });

        const paymentForm = reactive({
            debtId: null,
            amount: null,
            date: new Date().toISOString().split('T')[0],
            note: ''
        });

        // Modals
        let debtModal = null;
        let paymentModal = null;
        let detailModal = null;

        // Data Fetching
        const loadData = async () => {
            isLoading.value = true;
            try {
                const [debtsRes, summaryRes, accountsRes] = await Promise.all([
                    fetch('/api/debt'),
                    fetch('/api/debt/summary'),
                    fetch('/api/accounts')
                ]);

                if (debtsRes.ok) debts.value = await debtsRes.json();
                if (summaryRes.ok) summary.value = await summaryRes.json();
                if (accountsRes.ok) accounts.value = await accountsRes.json();

            } catch (error) {
                console.error("Error loading data:", error);
            } finally {
                isLoading.value = false;
            }
        };

        // Computed
        const filteredDebts = computed(() => {
            let result = debts.value;

            // Tab Filter
            if (currentTab.value === 'borrow') {
                result = result.filter(d => d.debtType === 1);
            } else if (currentTab.value === 'lend') {
                result = result.filter(d => d.debtType === 2);
            }

            // Search Filter
            if (searchQuery.value) {
                const q = searchQuery.value.toLowerCase();
                result = result.filter(d =>
                    d.name.toLowerCase().includes(q) ||
                    (d.personName && d.personName.toLowerCase().includes(q))
                );
            }
            return result;
        });

        // Initialize Modals
        onMounted(() => {
            loadData();
            debtModal = new bootstrap.Modal(document.getElementById('debtModal'));
            paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
            detailModal = new bootstrap.Modal(document.getElementById('debtDetailModal'));
        });

        // Actions
        const openAddModal = () => {
            isEditing.value = false;
            Object.assign(debtForm, {
                id: null,
                debtType: 1,
                name: '',
                personName: '',
                initialAmount: null,
                interestRate: 0,
                startDate: new Date().toISOString().split('T')[0],
                dueDate: ''
            });
            debtModal.show();
        };

        const openEditModal = () => {
            if (!selectedDebt.value) return;
            isEditing.value = true;
            Object.assign(debtForm, {
                id: selectedDebt.value.id,
                debtType: selectedDebt.value.debtType,
                name: selectedDebt.value.name,
                personName: selectedDebt.value.personName,
                initialAmount: selectedDebt.value.initialAmount,
                interestRate: selectedDebt.value.interestRate,
                startDate: selectedDebt.value.startDate ? selectedDebt.value.startDate.split('T')[0] : '',
                dueDate: selectedDebt.value.dueDate ? selectedDebt.value.dueDate.split('T')[0] : ''
            });
            detailModal.hide();
            debtModal.show();
        };

        const viewDebtDetails = (debt) => {
            selectedDebt.value = debt;
            detailModal.show();
        };

        const saveDebt = async () => {
            if (!debtForm.name || !debtForm.initialAmount) {
                alert("Vui lòng nhập tên và số tiền.");
                return;
            }

            try {
                const url = isEditing.value ? `/api/debt/${debtForm.id}` : '/api/debt';
                const method = isEditing.value ? 'PUT' : 'POST';

                const response = await fetch(url, {
                    method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(debtForm)
                });

                if (!response.ok) throw new Error("Lỗi khi lưu dữ liệu.");

                await loadData();
                debtModal.hide();

            } catch (error) {
                alert(error.message);
            }
        };

        const deleteDebt = async (id) => {
            if (!confirm("Bạn có chắc muốn xóa khoản nợ này?")) return;

            try {
                const response = await fetch(`/api/debt/${id}`, { method: 'DELETE' });
                if (!response.ok) throw new Error("Lỗi khi xóa.");

                await loadData();
                detailModal.hide();
            } catch (error) {
                alert(error.message);
            }
        };

        const openPaymentModal = (debt) => {
            selectedDebt.value = debt;
            Object.assign(paymentForm, {
                debtId: debt.id,
                amount: debt.remainingAmount,
                date: new Date().toISOString().split('T')[0],
                note: ''
            });
            detailModal.hide();
            paymentModal.show();
        };

        const confirmPayment = async () => {
            if (!paymentForm.amount || paymentForm.amount <= 0) {
                alert("Số tiền không hợp lệ.");
                return;
            }
            if (!accounts.value.length) {
                alert("Bạn cần tạo ví trước khi thanh toán.");
                return;
            }

            try {
                // 1. Create Transaction
                const isExpense = selectedDebt.value.debtType === 1; // Paying debt I owe = Expense
                const transactionPayload = {
                    amount: paymentForm.amount,
                    transactionType: isExpense ? 2 : 1,
                    accountId: accounts.value[0].id, // Default to first account
                    transactionDate: paymentForm.date,
                    note: `Thanh toán nợ: ${selectedDebt.value.name} (${paymentForm.note})`,
                    currency: 'VND'
                };

                const txRes = await fetch('/api/transactions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(transactionPayload)
                });

                if (!txRes.ok) throw new Error("Không thể tạo giao dịch.");
                const txData = await txRes.json();

                // 2. Record Payment
                const payRes = await fetch('/api/debt/payment', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        debtId: paymentForm.debtId,
                        transactionId: txData.id,
                        amount: paymentForm.amount,
                        paymentDate: paymentForm.date,
                        note: paymentForm.note
                    })
                });

                if (!payRes.ok) throw new Error("Không thể ghi nhận thanh toán nợ.");

                await loadData();
                paymentModal.hide();

            } catch (error) {
                console.error(error);
                alert("Có lỗi xảy ra: " + error.message);
            }
        };

        // Helpers
        const formatCurrency = (val) => {
            if (val === undefined || val === null) return '0 ₫';
            return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
        };

        const formatDate = (dateStr) => {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleDateString('vi-VN');
        };

        return {
            isLoading,
            debts,
            summary,
            filteredDebts,
            searchQuery,
            currentTab,
            isEditing,
            debtForm,
            paymentForm,
            selectedDebt,
            openAddModal,
            openEditModal,
            viewDebtDetails,
            saveDebt,
            deleteDebt,
            openPaymentModal,
            confirmPayment,
            formatCurrency,
            formatDate
        };
    }
}).mount('#debts-app');
