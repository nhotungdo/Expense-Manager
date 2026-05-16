// Quick Add Transaction Functionality
// This file controls the quick-add transaction modal (defined in _QuickAddModal.cshtml)

(function () {
    'use strict';

    let cachedAccounts = [];
    let quickAddModal = null;

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeQuickAddModal();
    });

    function initializeQuickAddModal() {
        const modalEl = document.getElementById('globalTransactionModal');
        if (!modalEl) return;

        // Initialize Bootstrap modal instance
        if (window.bootstrap) {
            quickAddModal = bootstrap.Modal.getOrCreateInstance(modalEl);
        }

        // Set default date to today
        const dateInput = document.getElementById('globalTransDate');
        if (dateInput && !dateInput.value) {
            dateInput.valueAsDate = new Date();
        }

        // Attach form submit handler
        const form = document.getElementById('globalTransactionForm');
        if (form) {
            form.addEventListener('submit', handleQuickAddSubmit);
        }

        // Initial load of accounts
        loadAccountsForQuickAdd();
    }

    async function loadAccountsForQuickAdd() {
        const transSelect = document.getElementById('globalTransAccount');
        if (!transSelect) return;

        try {
            const res = await fetch('/api/Accounts/summaries', { credentials: 'include' });
            if (res.ok) {
                cachedAccounts = await res.json();
                renderAccountOptions(transSelect);
            } else {
                console.error('Failed to load accounts. Status:', res.status);
                if (res.status === 401) {
                    transSelect.innerHTML = '<option value="">Phiên hết hạn - Vui lòng đăng nhập lại</option>';
                } else {
                    transSelect.innerHTML = `<option value="">Lỗi tải dữ liệu (${res.status})</option>`;
                }
            }
        } catch (err) {
            console.error('Error loading accounts:', err);
            transSelect.innerHTML = '<option value="">Lỗi kết nối mạng</option>';
        }
    }

    function renderAccountOptions(selectElement) {
        if (!cachedAccounts || cachedAccounts.length === 0) {
            selectElement.innerHTML = '<option value="">Không có ví nào</option>';
            return;
        }

        // Save current selection if any
        const currentVal = selectElement.value;

        selectElement.innerHTML = '<option value="">Chọn ví...</option>' +
            cachedAccounts.map(a => {
                const id = a.id || a.Id;
                const name = a.name || a.Name;
                const balance = a.currentBalance !== undefined ? a.currentBalance : a.CurrentBalance;
                return `<option value="${id}">${name} (${formatCurrencySimple(balance)})</option>`;
            }).join('');

        if (currentVal) {
            selectElement.value = currentVal;
        } else if (cachedAccounts.length === 1) {
            // Auto-select if only one account
            selectElement.value = cachedAccounts[0].id;
        }
    }

    async function loadCategoriesForQuickAdd(type) {
        const select = document.getElementById('globalTransCategory');
        if (!select) return;

        select.innerHTML = '<option>Đang tải danh mục...</option>';
        try {
            const res = await fetch(`/api/Categories?type=${type}`, { credentials: 'include' });
            if (res.ok) {
                const cats = await res.json();
                if (cats.length === 0) {
                    select.innerHTML = '<option value="">Chưa có danh mục</option>';
                } else {
                    select.innerHTML = '<option value="">Chọn danh mục...</option>' +
                        cats.map(c => `<option value="${c.id}">${c.icon || '📁'} ${c.name}</option>`).join('');
                }
            } else {
                select.innerHTML = '<option value="">Lỗi tải danh mục</option>';
            }
        } catch (e) {
            console.error('Error loading categories:', e);
            select.innerHTML = '<option value="">Lỗi kết nối</option>';
        }
    }

    async function handleQuickAddSubmit(e) {
        e.preventDefault();
        const btn = document.getElementById('globalBtnSaveTrans');
        if (!btn) return;

        // Basic Validation
        const amount = window.unformatCurrency(document.getElementById('globalTransAmount').value);
        const accountId = document.getElementById('globalTransAccount').value;
        const categoryId = document.getElementById('globalTransCategory').value;
        const date = document.getElementById('globalTransDate').value;

        if (!amount || amount <= 0) {
            showToast('Vui lòng nhập số tiền hợp lệ', 'warning');
            return;
        }
        if (!accountId) {
            showToast('Vui lòng chọn ví', 'warning');
            return;
        }
        if (!categoryId) {
            showToast('Vui lòng chọn danh mục', 'warning');
            return;
        }
        if (!date) {
            showToast('Vui lòng chọn ngày', 'warning');
            return;
        }

        // Disable button
        const originalText = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...';

        const dto = {
            TransactionType: parseInt(document.getElementById('globalTransType').value),
            Amount: amount,
            AccountId: parseInt(accountId),
            CategoryId: parseInt(categoryId),
            TransactionDate: date,
            Note: document.getElementById('globalTransNote').value,
            Currency: 'VND'
        };

        try {
            const res = await fetch('/api/Transactions', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto),
                credentials: 'include'
            });

            // Check content type to see if it's JSON or text
            const contentType = res.headers.get("content-type");
            let errorMsg = 'Lỗi không xác định';

            if (res.ok) {
                const data = await res.json();
                handleSuccess(data);
            } else {
                if (contentType && contentType.indexOf("application/json") !== -1) {
                    const errData = await res.json();
                    errorMsg = errData.message || JSON.stringify(errData);
                    if (errData.details) {
                        errorMsg += ` (${errData.details})`;
                    }
                } else {
                    errorMsg = await res.text();
                }
                showToast('Lưu thất bại: ' + errorMsg, 'error');
            }
        } catch (err) {
            console.error('Error saving transaction:', err);
            showToast('Có lỗi kết nối xảy ra', 'error');
        } finally {
            btn.disabled = false;
            btn.innerHTML = originalText;
        }

        function handleSuccess(data) {
            // Close modal
            if (quickAddModal) {
                quickAddModal.hide();
            }

            // Reset form (keep date)
            document.getElementById('globalTransactionForm').reset();
            const dateInput = document.getElementById('globalTransDate');
            if (dateInput) dateInput.valueAsDate = new Date();

            // Reset account if we have cached ones (to re-select default or placeholder)
            const transSelect = document.getElementById('globalTransAccount');
            if (transSelect) renderAccountOptions(transSelect);

            // Show success message
            showToast('Giao dịch đã được lưu thành công!', 'success');

            // Show Budget Warning if any
            if (data && data.warningMessage) {
                // Slight delay to make sure it's seen matching the user's attention shift
                setTimeout(() => {
                    showToast(data.warningMessage, 'warning');
                }, 800);
            }

            // Trigger global data refresh events if they exist
            if (typeof loadPersonalWalletData === 'function') loadPersonalWalletData();
            if (typeof loadAccounts === 'function') loadAccounts(); // Refresh wallet list pages
            if (typeof loadTransactions === 'function') loadTransactions(); // Refresh transaction list

            // Dispatch a custom event for other components to listen to
            document.dispatchEvent(new CustomEvent('transaction:added'));
        }
    }

    function showToast(message, type = 'info') {
        // Remove existing toasts
        const existing = document.querySelectorAll('.custom-toast-msg');
        existing.forEach(e => e.remove());

        const toast = document.createElement('div');
        toast.className = `custom-toast-msg alert alert-${type === 'success' ? 'success' : (type === 'warning' ? 'warning' : 'danger')} position-fixed top-0 start-50 translate-middle-x mt-4 shadow-lg fw-bold d-flex align-items-center`;
        toast.style.zIndex = '10000';
        toast.style.minWidth = '300px';
        toast.style.borderRadius = '50px'; // Pill shape

        let icon = 'info-circle';
        if (type === 'success') icon = 'check-circle';
        if (type === 'danger') icon = 'exclamation-circle';
        if (type === 'warning') icon = 'exclamation-triangle';

        toast.innerHTML = `<i class="fas fa-${icon} me-2"></i> ${message}`;

        document.body.appendChild(toast);

        // Animate in
        // (Bootstrap alert classes handle basic styling, but we can add animation)

        setTimeout(() => {
            toast.style.transition = 'opacity 0.5s ease-out';
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 500);
        }, 3000);
    }

    function formatCurrencySimple(val) {
        return window.formatCurrencyVND(val);
    }

    // Global function to open transaction modal
    window.openTransactionModal = function (typeStr) {
        const modalEl = document.getElementById('globalTransactionModal');
        const modalTitle = document.getElementById('globalTransactionModalTitle');
        const btnSave = document.getElementById('globalBtnSaveTrans');
        const transType = document.getElementById('globalTransType');

        if (!modalEl || !modalTitle || !btnSave || !transType) {
            console.error('Modal elements not found');
            // Try to reload items?
            return;
        }

        const isIncome = typeStr === 'Income';

        // Update UI based on type
        transType.value = isIncome ? '1' : '2';
        modalTitle.textContent = isIncome ? 'Thêm Thu Nhập' : 'Thêm Chi Tiêu';
        modalTitle.className = `modal-title fw-bold fs-4 ${isIncome ? 'text-success' : 'text-danger'}`;

        // Update button style
        btnSave.className = `btn w-100 py-3 rounded-3 fw-bold shadow-sm transition-all ${isIncome ? 'btn-success' : 'btn-danger'}`;

        // Update money input text color
        const amountInput = document.getElementById('globalTransAmount');
        if (amountInput) {
            amountInput.className = `form-control border-0 text-center fw-bold fs-1 p-0 shadow-none ${isIncome ? 'text-success' : 'text-danger'}`;
        }

        // Load categories for the selected type
        loadCategoriesForQuickAdd(isIncome ? 1 : 2);

        // Ensure accounts are loaded (retry if empty)
        if (cachedAccounts.length === 0) {
            loadAccountsForQuickAdd();
        }

        // Show modal handle
        if (!quickAddModal && window.bootstrap) {
            quickAddModal = bootstrap.Modal.getOrCreateInstance(modalEl);
        }

        if (quickAddModal) {
            quickAddModal.show();
        }
    };

})();
