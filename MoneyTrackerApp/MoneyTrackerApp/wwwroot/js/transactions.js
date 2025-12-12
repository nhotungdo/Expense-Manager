import {
    validateTransactionInput,
    buildTransactionDto,
    createTransaction
} from './transactions-utils.js';

// Global State
let state = {
    filters: {
        range: 'month', // day, week, month, year, custom
        startDate: null,
        endDate: null,
        type: '', // 1, 2, 3
        walletId: '',
        categoryId: '',
        search: ''
    },
    transactions: [],
    wallets: [],
    categories: [],
    budgets: [] // For alert
};

// Initialize
document.addEventListener('DOMContentLoaded', async () => {
    initTimeFilter();
    await loadInitialData();
    bindEvents();
    filterTime('month'); // Load default
});

// --- Data Loading --- //

async function loadInitialData() {
    try {
        // Parallel fetch for static data
        const [walletsRes, catsRes, budgetsRes] = await Promise.all([
            fetch('/api/Wallets'),
            fetch('/api/Categories'),
            fetch('/api/Budgets')
        ]);

        if (walletsRes.ok) state.wallets = await walletsRes.json();
        if (catsRes.ok) state.categories = await catsRes.json();
        if (budgetsRes.ok) state.budgets = await budgetsRes.json();

        // Populate Dropdowns in Filter & Modal
        populateDropdowns();

    } catch (e) {
        console.error("Error loading initial data", e);
        // Fallback or Toast error
    }
}

async function loadTransactions() {
    const container = document.getElementById('transactionListContainer');
    container.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted">Đang tải giao dịch...</p>
        </div>`;

    try {
        // Build Query String
        const params = new URLSearchParams({
            fromDate: state.filters.startDate.toISOString(),
            toDate: state.filters.endDate.toISOString()
        });
        if (state.filters.type) params.append('type', state.filters.type);
        if (state.filters.walletId) params.append('walletId', state.filters.walletId);
        if (state.filters.categoryId) params.append('categoryId', state.filters.categoryId);
        if (state.filters.search) params.append('search', state.filters.search);

        const res = await fetch(`/api/Transactions?${params.toString()}`);
        if (!res.ok) throw new Error("API Limit or Error");

        const data = await res.json();
        state.transactions = data; // Assuming API returns List<TransactionDto>

        renderTransactions(state.transactions);
        updateSummary(state.transactions);

    } catch (error) {
        console.error(error);
        // If API fails, try to use Mock Data from window if available (for dev)
        if (window.MockDashboardData && window.MockDashboardData.recentTransactions) {
            console.warn("Using Mock Data");
            renderTransactions(window.MockDashboardData.recentTransactions);
        } else {
            container.innerHTML = `<div class="text-center text-danger py-5"><i class="fas fa-exclamation-triangle"></i> Lỗi tải dữ liệu.</div>`;
        }
    }
}

// --- Rendering --- //

function renderTransactions(list) {
    const container = document.getElementById('transactionListContainer');
    container.innerHTML = '';

    if (!list || list.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <div class="mb-3 text-muted" style="font-size: 3rem;"><i class="fas fa-receipt"></i></div>
                <h5>Chưa có giao dịch</h5>
                <p class="text-muted">Không tìm thấy giao dịch nào trong khoảng thời gian này.</p>
            </div>`;
        return;
    }

    // Group by Date
    // Sort descending first
    list.sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));

    const grouped = {};
    list.forEach(t => {
        const dateKey = new Date(t.transactionDate).toLocaleDateString('vi-VN');
        if (!grouped[dateKey]) grouped[dateKey] = [];
        grouped[dateKey].push(t);
    });

    Object.keys(grouped).forEach(date => {
        const groupDiv = document.createElement('div');
        groupDiv.className = 'timeline-date-group';

        // Date Header
        const header = document.createElement('div');
        header.className = 'timeline-date-header';
        header.innerHTML = `<i class="far fa-calendar-alt"></i> ${date}`;
        groupDiv.appendChild(header);

        // Transactions
        grouped[date].forEach(t => {
            const item = document.createElement('div');
            // Determine class based on Type (1: Income, 2: Expense, 3: Transfer)
            let typeClass = '';
            let typeIcon = '';
            let amountSign = '';
            const tType = Number(t.transactionType);

            if (tType === 1) {
                typeClass = 'income';
                typeIcon = 'fa-arrow-down';
                amountSign = '+';
            } else if (tType === 2) {
                typeClass = 'expense';
                typeIcon = 'fa-arrow-up';
                amountSign = '-';
            } else {
                typeClass = 'transfer';
                typeIcon = 'fa-exchange-alt';
            }

            const cat = state.categories.find(c => c.categoryId === t.categoryId) || { icon: 'fa-question', color: '#ccc', name: 'Khác' };
            const wallet = state.wallets.find(w => w.accountId === t.accountId) || { accountName: 'Ví không xác định' };

            item.className = `transaction-item ${typeClass}`;
            item.onclick = () => openEditTransaction(t.transactionId); // Feature to edit

            item.innerHTML = `
                <div class="transaction-icon" style="background-color: ${cat.color}20; color: ${cat.color}">
                    <i class="${cat.icon || 'fas fa-wallet'}"></i>
                </div>
                <div class="transaction-details">
                    <div class="transaction-category">${cat.name || 'Giao dịch'}</div>
                    <div class="transaction-meta">
                        <span><i class="fas fa-wallet"></i> ${wallet.accountName}</span>
                        ${t.note ? `<span><i class="fas fa-comment-alt"></i> ${t.note}</span>` : ''}
                    </div>
                </div>
                <div class="transaction-amount">
                    ${amountSign}${formatCurrency(t.amount)}
                </div>
            `;
            groupDiv.appendChild(item);
        });

        container.appendChild(groupDiv);
    });
}

function updateSummary(list) {
    // Simple client-side calc
    const income = list.filter(t => t.transactionType === 1).reduce((sum, t) => sum + t.amount, 0);
    const expense = list.filter(t => t.transactionType === 2).reduce((sum, t) => sum + t.amount, 0);
    const net = income - expense;

    document.getElementById('totalIncome').innerText = formatCurrency(income);
    document.getElementById('totalExpense').innerText = formatCurrency(expense);
    document.getElementById('netIncome').innerText = formatCurrency(net);
}

// --- Interaction Logic --- //

window.openAddTransaction = function () {
    resetForm();
    const modal = new bootstrap.Modal(document.getElementById('transactionModal'));
    modal.show();
}

// Global scope required for inline onclicks if using module
window.filterTime = function (range) {
    state.filters.range = range;

    // Update active button
    document.querySelectorAll('.time-filter .btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.time-filter .btn[data-range="${range}"]`).classList.add('active');

    // Calc Date Range
    const now = new Date();
    let start = new Date();
    let end = new Date();

    if (range === 'day') {
        start.setHours(0, 0, 0, 0);
        end.setHours(23, 59, 59, 999);
    } else if (range === 'week') {
        const day = now.getDay() || 7; // Get current day number, converting Sun. to 7
        if (day !== 1) start.setHours(-24 * (day - 1)); // Go back to Monday
        else start.setHours(0, 0, 0, 0); // It is Monday
        end = new Date(start);
        end.setDate(end.getDate() + 6);
        end.setHours(23, 59, 59, 999);
    } else if (range === 'month') {
        start = new Date(now.getFullYear(), now.getMonth(), 1);
        end = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
    } else if (range === 'year') {
        start = new Date(now.getFullYear(), 0, 1);
        end = new Date(now.getFullYear(), 11, 31, 23, 59, 59);
    }
    // Custom handled separately (not impl here for brevity)

    state.filters.startDate = start;
    state.filters.endDate = end;

    // Update UI Text
    document.getElementById('currentDateRange').innerText = `${formatDate(start)} - ${formatDate(end)}`;

    loadTransactions();
}

window.setTransactionType = function (type) {
    document.getElementById('transactionType').value = type;

    // Update Tabs
    document.querySelectorAll('#typeTabs .nav-link').forEach(l => l.classList.remove('active'));
    document.querySelector(`#typeTabs .nav-link[data-type="${type}"]`).classList.add('active');

    // Toggle Fields
    const destWallet = document.getElementById('destWalletGroup');
    if (type === 3) { // Transfer
        destWallet.classList.remove('d-none');
        document.getElementById('categoryDropdown').parentElement.parentElement.classList.add('d-none'); // Hide category? usually transfer doesn't need generic category
    } else {
        destWallet.classList.add('d-none');
        document.getElementById('categoryDropdown').parentElement.parentElement.classList.remove('d-none');
    }
}

async function populateDropdowns() {
    // Wallets
    const wSelect = document.getElementById('filterWallet');
    const wForm = document.getElementById('walletId');
    const wDest = document.getElementById('destWalletId');

    let html = '<option value="">Chọn ví</option>';
    state.wallets.forEach(w => {
        html += `<option value="${w.accountId}">${w.accountName} (${formatCurrency(w.currentBalance)})</option>`;
    });

    wForm.innerHTML = html;
    wDest.innerHTML = html.replace('Chọn ví', 'Chọn ví đích');

    // Filter expects "All"
    wSelect.innerHTML = '<option value="">Tất cả Ví</option>' + state.wallets.map(w => `<option value="${w.accountId}">${w.accountName}</option>`).join('');

    // Categories (Modal List)
    const cList = document.getElementById('categoryList');
    cList.innerHTML = '<li class="text-center p-2"><button type="button" class="btn btn-sm btn-link">Tạo mới</button></li>';

    state.categories.forEach(c => {
        const li = document.createElement('li');
        li.innerHTML = `<button class="dropdown-item" type="button" onclick="selectCategory(${c.categoryId}, '${c.name}')">
            <i class="${c.icon} me-2" style="color:${c.color}"></i> ${c.name}
        </button>`;
        cList.insertBefore(li, cList.lastChild);
    });

    // Filter Category
    const cSelect = document.getElementById('filterCategory');
    cSelect.innerHTML = '<option value="">Tất cả Danh mục</option>' + state.categories.map(c => `<option value="${c.categoryId}">${c.name}</option>`).join('');
}

window.selectCategory = function (id, name) {
    document.getElementById('categoryId').value = id;
    document.getElementById('selectedCategoryName').innerText = name;
    // Close dropdown implicit
}

window.saveTransaction = async function () {
    const btn = document.querySelector('button[onclick="saveTransaction()"]');
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

    // Collect Data
    const input = {
        type: document.getElementById('transactionType').value,
        amount: document.getElementById('amountInput').value,
        accountId: document.getElementById('walletId').value,
        pairedAccountId: document.getElementById('destWalletId').value,
        date: document.getElementById('transactionDate').value,
        note: document.getElementById('noteInput').value,
        categoryId: document.getElementById('categoryId').value,
        // Attachment todo
    };

    // Validate
    const validation = validateTransactionInput(input);
    if (!validation.valid) {
        alert(validation.errors.join('\n'));
        btn.disabled = false;
        btn.innerHTML = originalText;
        return;
    }

    const dto = buildTransactionDto(input);

    // Call API
    const result = await createTransaction(dto);

    if (result.ok) {
        bootstrap.Modal.getInstance(document.getElementById('transactionModal')).hide();
        loadTransactions(); // Reload list
        // Show success toast?
    } else {
        alert(result.error);
    }

    btn.disabled = false;
    btn.innerHTML = originalText;
}

// --- Watchers & Smart Features ---

function bindEvents() {
    // Search
    document.getElementById('searchKeyword').addEventListener('input', debounce(() => {
        state.filters.search = document.getElementById('searchKeyword').value;
        loadTransactions();
    }, 500));

    // Filters
    document.getElementById('filterType').addEventListener('change', (e) => {
        state.filters.type = e.target.value;
        loadTransactions();
    });

    document.getElementById('filterWallet').addEventListener('change', (e) => {
        state.filters.walletId = e.target.value;
        loadTransactions();
    });

    document.getElementById('filterCategory').addEventListener('change', (e) => {
        state.filters.categoryId = e.target.value;
        loadTransactions();
    });

    // AI & Budget Alert Inputs
    document.getElementById('noteInput').addEventListener('keyup', (e) => {
        const val = e.target.value.toLowerCase();
        if (val.includes('cafe') || val.includes('ăn')) {
            // Mock AI suggestion
            const suggestion = document.getElementById('aiSuggestions');
            suggestion.classList.remove('d-none');
        }
    });

    document.getElementById('amountInput').addEventListener('input', (e) => {
        const val = Number(e.target.value);
        const alertDiv = document.getElementById('budgetAlert');
        // Simple check: if > 10M warn. Ideally check against state.budgets
        if (val > 10000000) {
            alertDiv.classList.remove('d-none');
        } else {
            alertDiv.classList.add('d-none');
        }
    });
}

function initTimeFilter() {
    // Defaults handled in load
}

// Helpers
function resetForm() {
    document.getElementById('transactionForm').reset();
    document.getElementById('transactionType').value = 2; // Expense default
    // Reset other custom fields
    document.getElementById('selectedCategoryName').innerText = 'Chọn danh mục';
    document.getElementById('categoryId').value = '';
    document.getElementById('transactionDate').value = new Date().toISOString().slice(0, 16); // Local ISO rough
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function formatDate(date) {
    return date.toLocaleDateString('vi-VN');
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
