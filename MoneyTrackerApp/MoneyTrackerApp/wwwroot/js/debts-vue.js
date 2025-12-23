const { createApp, ref, computed, onMounted, reactive, watch } = Vue;

createApp({
    setup() {
        const isLoading = ref(true);
        const debts = ref([]);
        const accounts = ref([]);
        const summary = ref({
            totalDebts: 0,
            activeDebts: 0,
            totalIOwe: 0,
            totalTheyOweMe: 0,
            netDebt: 0,
            totalInterest: 0,
            iOweThem: [],
            theyOweMe: []
        });

        // Search & Filter
        const searchQuery = ref('');
        const currentTab = ref('all'); // all, borrow, lend

        // Modals
        let debtModalInstance = null;
        let paymentModalInstance = null;
        let detailModalInstance = null;

        // Forms
        const isEditing = ref(false);
        const debtForm = reactive({
            id: null,
            debtType: 1, // 1: Borrow (I Owe), 2: Lend (They Owe Me)
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

        const selectedDebt = ref(null);

        // Fetch Data
        const loadDebts = async () => {
            isLoading.value = true;
            try {
                // Parallel fetch
                const [debtsRes, summaryRes, accountsRes] = await Promise.all([
                    fetch('/api/debt'),
                    fetch('/api/debt/summary'),
                    fetch('/api/accounts')
                ]);

                if (debtsRes.ok) debts.value = await debtsRes.json();
                if (summaryRes.ok) summary.value = await summaryRes.json();
                if (accountsRes.ok) accounts.value = await accountsRes.json();

            } catch (error) {
                console.error('Error loading data:', error);
                alert("Có lỗi khi tải dữ liệu.");
            } finally {
                isLoading.value = false;
            }
        };

        // Computed
        const filteredDebts = computed(() => {
            let result = debts.value;

            // Filter by Tab
            if (currentTab.value === 'borrow') {
                result = result.filter(d => d.debtType === 1);
            } else if (currentTab.value === 'lend') {
                result = result.filter(d => d.debtType === 2);
            }

            // Filter by Search
            if (searchQuery.value) {
                const query = searchQuery.value.toLowerCase();
                result = result.filter(d =>
                    d.name.toLowerCase().includes(query) ||
                    (d.personName && d.personName.toLowerCase().includes(query))
                );
            }

            return result;
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

            if (!debtModalInstance) debtModalInstance = new bootstrap.Modal(document.getElementById('debtModal'));
            debtModalInstance.show();
        };

        const openEditModal = () => {
            if (!selectedDebt.value) return;
            // Close detail modal first if open
            if (detailModalInstance) detailModalInstance.hide();

            isEditing.value = true;
            Object.assign(debtForm, {
                id: selectedDebt.value.id,
                debtType: selectedDebt.value.debtType,
                name: selectedDebt.value.name,
                personName: selectedDebt.value.personName,
                initialAmount: selectedDebt.value.initialAmount,
                interestRate: selectedDebt.value.interestRate,
                startDate: selectedDebt.value.startDate,
                dueDate: selectedDebt.value.dueDate
            });

            if (!debtModalInstance) debtModalInstance = new bootstrap.Modal(document.getElementById('debtModal'));
            debtModalInstance.show();
        };

        const saveDebt = async () => {
            // Basic validation
            if (!debtForm.name || !debtForm.initialAmount) {
                alert("Vui lòng nhập tên và số tiền.");
                return;
            }

            try {
                let url = '/api/debt';
                let method = 'POST';
                let body = { ...debtForm };

                if (isEditing.value) {
                    url = `/api/debt/${debtForm.id}`;
                    method = 'PUT';
                }

                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.message || 'Lỗi lưu dữ liệu');
                }

                // Refresh data
                await loadDebts();
                debtModalInstance.hide();

            } catch (error) {
                console.error('Save error:', error);
                alert(error.message);
            }
        };

        const viewDebtDetails = (debt) => {
            selectedDebt.value = debt;
            if (!detailModalInstance) detailModalInstance = new bootstrap.Modal(document.getElementById('debtDetailModal'));
            detailModalInstance.show();
        };

        const openPaymentModal = (debt) => {
            selectedDebt.value = debt;
            // Close detail modal if needed
            if (detailModalInstance) detailModalInstance.hide();

            Object.assign(paymentForm, {
                debtId: debt.id,
                amount: debt.remainingAmount, // Default to full pay
                date: new Date().toISOString().split('T')[0],
                note: ''
            });

            if (!paymentModalInstance) paymentModalInstance = new bootstrap.Modal(document.getElementById('paymentModal'));
            paymentModalInstance.show();
        };

        const confirmPayment = async () => {
            if (!paymentForm.amount || paymentForm.amount <= 0) {
                alert("Số tiền không hợp lệ.");
                return;
            }

            if (accounts.value.length === 0) {
                alert("Bạn cần có ít nhất một tài khoản (ví) để thực hiện thanh toán. Vui lòng tạo ví trước.");
                return;
            }

            try {
                // Determine transaction type
                // If Borrow (I owe): Payment means Expense (Money out).
                // If Lend (They owe): Payment means Income (Money in).
                const isExpense = selectedDebt.value.debtType === 1;

                // Use first account as default for now
                const accountId = accounts.value[0].id;

                const transactionPayload = {
                    amount: paymentForm.amount,
                    transactionType: isExpense ? 2 : 1, // 2: Expense, 1: Income
                    categoryId: null, // Optional
                    accountId: accountId,
                    transactionDate: paymentForm.date,
                    note: `Thanh toán nợ: ${selectedDebt.value.name} - ${paymentForm.note || ''}`,
                    currency: 'VND'
                };

                // Create Transaction first
                const txRes = await fetch('/api/transactions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(transactionPayload)
                });

                if (!txRes.ok) {
                    const err = await txRes.json();
                    throw new Error(err.message || "Lỗi khi tạo giao dịch thanh toán.");
                }
                const txData = await txRes.json();

                // Now Record Debt Payment linked to Transaction
                const paymentPayload = {
                    debtId: paymentForm.debtId,
                    transactionId: txData.id,
                    amount: paymentForm.amount,
                    paymentDate: paymentForm.date,
                    note: paymentForm.note
                };

                const payRes = await fetch('/api/debt/payment', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(paymentPayload)
                });

                if (!payRes.ok) {
                    const err = await payRes.json();
                    throw new Error(err.message || "Lỗi khi ghi nhận thanh toán nợ.");
                }

                await loadDebts();
                paymentModalInstance.hide();

            } catch (error) {
                console.error('Payment error:', error);
                alert(error.message);
            }
        };

        const deleteDebt = async (id) => {
            if (!confirm('Bạn có chắc chắn muốn xóa khoản nợ này? Hành động này không thể hoàn tác.')) return;

            try {
                const response = await fetch(`/api/debt/${id}`, {
                    method: 'DELETE'
                });

                if (!response.ok) throw new Error("Không thể xóa khoản nợ.");

                await loadDebts();
                detailModalInstance.hide();
            } catch (error) {
                console.error('Delete error:', error);
                alert(error.message);
            }
        };

        // Helpers
        const formatCurrency = (amount) => {
            if (amount === undefined || amount === null) return '0 ₫';
            return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
        };

        const formatDate = (dateString) => {
            if (!dateString) return '';
            return new Date(dateString).toLocaleDateString('vi-VN');
        };

        // Lifecycle
        onMounted(() => {
            loadDebts();
        });

        return {
            isLoading,
            debts,
            summary,
            searchQuery,
            currentTab,
            filteredDebts,
            debtForm,
            paymentForm,
            isEditing,
            selectedDebt,
            // Actions
            openAddModal,
            openEditModal,
            saveDebt,
            viewDebtDetails,
            openPaymentModal,
            confirmPayment,
            deleteDebt,
            // Helpers
            formatCurrency,
            formatDate
        };
    }
}).mount('#debts-app');
