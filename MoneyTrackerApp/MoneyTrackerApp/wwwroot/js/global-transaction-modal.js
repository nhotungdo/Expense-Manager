import { validateTransactionInput, buildTransactionDto, createTransaction } from './transactions-utils.js';

/**
 * Global Transaction Modal Controller
 * Handles logic for _TransactionModal.cshtml
 */

let modalInstance = null;
let cachedAccounts = [];
let accountLoadPromise = null;

document.addEventListener('DOMContentLoaded', () => {
    initModal();

    // Global exposure for buttons to call
    window.openTransactionModal = openTransactionModal;
});

function initModal() {
    const el = document.getElementById('transactionModal');
    if (!el) return;

    // Bootstrap 5
    if (window.bootstrap) {
        modalInstance = new bootstrap.Modal(el);
    }

    // Event Listeners
    const form = document.getElementById('globalTransactionForm');
    if (form) form.addEventListener('submit', handleFormSubmit);

    // Initial Load
    // We lazy load accounts when modal opens or here? 
    // Let's lazy load to save requests if user doesn't transact.
}

async function loadAccounts() {
    if (cachedAccounts.length > 0) return cachedAccounts;
    if (accountLoadPromise) return accountLoadPromise;

    accountLoadPromise = fetch('/api/Accounts/summaries', { credentials: 'include' })
        .then(res => {
            if (!res.ok) throw new Error(res.statusText);
            return res.json();
        })
        .then(data => {
            cachedAccounts = data;
            return data;
        })
        .catch(err => {
            console.error('Failed to load accounts', err);
            return [];
        })
        .finally(() => {
            accountLoadPromise = null;
        });

    return accountLoadPromise;
}

async function loadCategories(type) {
    try {
        const res = await fetch(`/api/Categories?type=${type}`, { credentials: 'include' });
        if (!res.ok) return [];
        return await res.json();
    } catch (err) {
        console.error(err);
        return [];
    }
}

// ---------------------------------------------------------
// UI Functions
// ---------------------------------------------------------

async function openTransactionModal(type = 2) { // Default Expense
    if (!modalInstance) initModal();
    if (!modalInstance) return;

    // 1. Reset Form
    const form = document.getElementById('globalTransactionForm');
    form.reset();

    // 2. Set Date to Today
    document.getElementById('transDate').valueAsDate = new Date();

    // 3. Set Type
    const radio = document.querySelector(`input[name="TransactionType"][value="${type}"]`);
    if (radio) {
        radio.checked = true;
        handleTransTypeChange(type); // Update UI
    }

    // 4. Load Data
    const accounts = await loadAccounts();
    renderAccounts(accounts);

    if (type !== 3) { // If not transfer, load categories
        await updateCategories(type);
    }

    modalInstance.show();
}

// Global function needs to be strictly defined if called from HTML onchange attributes
window.handleTransTypeChange = async function (val) {
    val = parseInt(val);
    const btn = document.getElementById('btnSaveTransaction');
    const labelTitle = document.getElementById('transactionModalTitle');

    // Color & Text updates
    if (val === 2) { // Expense
        updateTheme('rose', 'Lưu chi tiêu', 'Chi tiêu mới');
        toggleGroups(true);
    } else if (val === 1) { // Income
        updateTheme('emerald', 'Lưu thu nhập', 'Thu nhập mới');
        toggleGroups(true);
    } else if (val === 3) { // Transfer
        updateTheme('blue', 'Xác nhận chuyển', 'Chuyển tiền nội bộ');
        toggleGroups(false);
    }

    if (val !== 3) {
        await updateCategories(val);
    }
}

function updateTheme(color, btnText, titleText) {
    const btn = document.getElementById('btnSaveTransaction');
    const title = document.getElementById('transactionModalTitle');

    // Clean classes
    btn.className = `w-full mt-8 py-3.5 text-white rounded-xl font-bold shadow-lg transition-all transform hover:-translate-y-0.5 flex items-center justify-center gap-2 bg-${color}-600 hover:bg-${color}-700 shadow-${color}-200`;

    btn.querySelector('span').textContent = btnText;
    title.textContent = titleText;
}

function toggleGroups(isTransaction) {
    const targetGroup = document.getElementById('targetWalletGroup');
    const categoryGroup = document.getElementById('categoryGroup');

    if (isTransaction) {
        targetGroup.classList.add('hidden');
        categoryGroup.classList.remove('hidden');
        document.getElementById('sourceWalletLabel').textContent = 'Ví nguồn';
    } else {
        targetGroup.classList.remove('hidden');
        categoryGroup.classList.add('hidden');
        document.getElementById('sourceWalletLabel').textContent = 'Từ ví';
    }
}

function renderAccounts(accounts) {
    const opts = accounts.length
        ? '<option value="">Chọn ví...</option>' + accounts.map(a => `<option value="${a.id}">${a.name} (${formatMoney(a.currentBalance)})</option>`).join('')
        : '<option value="">Không có ví nào</option>';

    document.getElementById('transWallet').innerHTML = opts;
    document.getElementById('transTargetWallet').innerHTML = opts;
}

async function updateCategories(type) {
    const select = document.getElementById('transCategory');
    select.innerHTML = '<option>Đang tải...</option>';

    const cats = await loadCategories(type);

    if (cats.length) {
        select.innerHTML = '<option value="">Chọn danh mục...</option>' +
            cats.map(c => `<option value="${c.id}">${c.icon || ''} ${c.name}</option>`).join('');
    } else {
        select.innerHTML = '<option value="">Chưa có danh mục</option>';
    }
}

function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// ---------------------------------------------------------
// Submit Logic
// ---------------------------------------------------------

async function handleFormSubmit(e) {
    e.preventDefault();

    const btn = document.getElementById('btnSaveTransaction');
    const originalContent = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-circle-notch fa-spin"></i> Xử lý...';

    try {
        const formData = new FormData(e.target);
        const type = parseInt(formData.get('TransactionType'));

        // Handle TRANSFER specifically
        if (type === 3) {
            await handleTransferSubmit(formData);
        } else {
            // Handle INCOME / EXPENSE
            await handleStandardSubmit(formData, type);
        }

    } catch (err) {
        console.error(err);
        alert('Có lỗi xảy ra: ' + err.message);
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalContent;
    }
}

async function handleStandardSubmit(formData, type) {
    const raw = {
        type: type,
        amount: parseFloat(formData.get('Amount').toString().replace(/,/g, '')),
        accountId: formData.get('AccountId'),
        categoryId: formData.get('CategoryId'),
        date: formData.get('TransactionDate'),
        note: formData.get('Note'),
        currency: 'VND'
    };

    const { valid, errors } = validateTransactionInput(raw);
    if (!valid) {
        alert(errors.join('\n'));
        return;
    }

    const dto = buildTransactionDto(raw);
    const result = await createTransaction(dto);

    if (result.ok) {
        // Success
        modalInstance.hide();
        cachedAccounts = []; // Clear cache so next open fetches fresh balances

        // Show Budget Warning if any
        if (result.data && result.data.warningMessage) {
            alert(result.data.warningMessage);
        }

        // Refresh page logic?
        // If we are on Home or Wallet, we might want to refresh data.
        // We can dispatch a global event
        window.dispatchEvent(new CustomEvent('transaction:saved'));

        if (window.loadPersonalWalletData) window.loadPersonalWalletData();
        if (window.loadRecentTransactions) window.loadRecentTransactions();
        if (window.loadExpenseBreakdown) window.loadExpenseBreakdown('month');
        if (window.loadIncomeBreakdown) window.loadIncomeBreakdown('month');
        if (window.loadAccounts) window.loadAccounts(); // Refresh wallet page lists


    } else {
        alert('Lỗi: ' + result.error);
    }
}

async function handleTransferSubmit(formData) {
    const payload = {
        SourceAccountId: parseInt(formData.get('AccountId')),
        TargetAccountId: parseInt(formData.get('TargetAccountId')),
        Amount: parseFloat(formData.get('Amount').toString().replace(/,/g, '')),
        Note: formData.get('Note'),
        TransactionDate: formData.get('TransactionDate')
    };

    if (payload.SourceAccountId === payload.TargetAccountId) {
        alert('Ví nguồn và đích không được trùng nhau');
        return;
    }

    const res = await fetch('/api/Transactions/transfer', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (res.ok) {
        modalInstance.hide();
        cachedAccounts = []; // Clear cache so next open fetches fresh balances
        window.dispatchEvent(new CustomEvent('transaction:saved'));
        if (window.loadPersonalWalletData) window.loadPersonalWalletData();
        if (window.loadRecentTransactions) window.loadRecentTransactions();
        if (window.loadAccounts) window.loadAccounts();
    } else {
        const err = await res.json();
        alert('Lỗi chuyển tiền: ' + (err.message || 'Unknown'));
    }
}
