/* wallet-detail.js */

let _walletId = 0;
let _spendingChart = null;
let _connection = null;

function initWalletDetail(walletId) {
    _walletId = walletId;
    loadTransactions();

    // SignalR Setup
    setupSignalR();

    // Search listener
    const searchInput = document.getElementById('txnSearch');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            loadTransactions(e.target.value);
        });
    }

    // Filter listener
    const userFilter = document.getElementById('userFilter');
    if (userFilter) {
        userFilter.addEventListener('change', () => {
            const searchText = document.getElementById('txnSearch') ? document.getElementById('txnSearch').value : '';
            loadTransactions(searchText);
        });
    }
}

function switchTab(tabName) {
    // Hide all contents
    document.querySelectorAll('.tab-content-panel').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.tab-link').forEach(el => el.classList.remove('active'));

    // Show active
    const content = document.getElementById(`view-${tabName}`);
    const tab = document.getElementById(`tab-${tabName}`);

    if (content) content.classList.add('active');
    if (tab) tab.classList.add('active');

    // Load data if needed
    if (tabName === 'overview') {
        loadAnalyticsData();
    }
}

async function loadTransactions(searchText = '') {
    const listContainer = document.getElementById('transaction-list-container');
    const userFilter = document.getElementById('userFilter') ? document.getElementById('userFilter').value : '';

    if (!listContainer) return;

    // Show spinner if empty (initial load)
    if (!listContainer.querySelector('.transaction-card')) {
        listContainer.innerHTML = '<div class="d-flex justify-content-center p-5 text-muted"><i class="fas fa-spinner fa-spin me-2"></i> Đang tải dữ liệu...</div>';
    }

    try {
        let url = `/Wallets/Detail/${_walletId}?handler=Transactions&search=${encodeURIComponent(searchText)}&userFilter=${userFilter}`;

        const response = await fetch(url);
        if (!response.ok) throw new Error("Network response was not ok");

        const transactions = await response.json();

        if (transactions.length === 0) {
            listContainer.innerHTML = `
                <div class="text-center p-5 text-muted">
                    <div class="mb-3" style="font-size: 3rem; opacity: 0.3;"><i class="fas fa-receipt"></i></div>
                    <h5>Chưa có giao dịch nào</h5>
                    <p class="small">Hãy thử thêm giao dịch mới hoặc thay đổi bộ lọc.</p>
                </div>
            `;
            return;
        }

        renderTransactionList(listContainer, transactions);

    } catch (err) {
        console.error(err);
        listContainer.innerHTML = '<div class="text-danger text-center p-4">Không thể tải giao dịch. Vui lòng thử lại.</div>';
    }
}

function renderTransactionList(container, transactions) {
    let html = '';

    const groups = transactions.reduce((acc, t) => {
        const date = t.transactionDate.split('T')[0];
        if (!acc[date]) acc[date] = [];
        acc[date].push(t);
        return acc;
    }, {});

    const dates = Object.keys(groups).sort((a, b) => new Date(b) - new Date(a));

    dates.forEach(date => {
        const prettyDate = new Date(date).toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'numeric', year: 'numeric' });
        html += `<h6 class="text-secondary fw-bold small mt-4 mb-3 text-uppercase ps-2">${prettyDate}</h6>`;

        groups[date].forEach(t => {
            const isIncome = t.transactionType === 1;
            const sign = isIncome ? '+' : '-';
            const amountClass = isIncome ? 'amount-income' : 'amount-expense';
            const icon = t.categoryIcon || 'fas fa-money-bill';
            // Use category color or default
            const color = t.categoryColor || '#94a3b8';
            const name = t.description || t.categoryName || t.note || 'Giao dịch';

            // Format time
            const time = new Date(t.transactionDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

            // Avatar for spender
            let avatarHtml = '';
            if (t.userAvatar) {
                avatarHtml = `<img src="${t.userAvatar}" class="spender-badge-small" title="Người chi: ${t.userName}" />`;
            } else if (t.userName) {
                avatarHtml = `<div class="spender-badge-small" title="Người chi: ${t.userName}">${t.userName.charAt(0).toUpperCase()}</div>`;
            }

            html += `
            <div class="transaction-card" onclick="openEditModal(${t.id})">
                <div class="d-flex align-items-center">
                    <div class="txn-icon" style="background-color: ${color}; box-shadow: 0 4px 10px ${color}40;">
                        <i class="${icon}"></i>
                    </div>
                    <div class="txn-info">
                        <h4>${name}</h4>
                        <div class="txn-meta">${time} &bull; ${t.categoryName || 'Khác'} &bull; ${t.note || ''}</div>
                    </div>
                </div>
                <div class="d-flex align-items-center">
                    <div class="text-end me-2">
                        <div class="txn-amount ${amountClass}">${sign}${t.amount.toLocaleString()} ${t.currency}</div>
                    </div>
                    ${avatarHtml}
                </div>
            </div>
            `;
        });
    });

    container.innerHTML = html;
}

async function loadAnalyticsData() {
    const today = new Date();
    const month = today.getMonth() + 1;
    const year = today.getFullYear();

    try {
        const response = await fetch(`/Wallets/Detail/${_walletId}?handler=ContributionData&month=${month}&year=${year}`);
        const data = await response.json();

        renderContributionChart(data);
        renderContributionTable(data);
    } catch (err) {
        console.error(err);
    }
}

function renderContributionChart(data) {
    const ctx = document.getElementById('contributionChart');
    if (!ctx) return;

    if (_spendingChart) _spendingChart.destroy();

    const colors = ['#667eea', '#764ba2', '#FF6384', '#FFCE56', '#4BC0C0', '#9966FF'];

    _spendingChart = new Chart(ctx.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.userName),
            datasets: [{
                data: data.map(d => d.totalAmount),
                backgroundColor: colors,
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '70%',
            plugins: {
                legend: { position: 'bottom', labels: { usePointStyle: true, padding: 20 } }
            }
        }
    });
}

function renderContributionTable(data) {
    const tbody = document.querySelector('#contributionTable tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    // Check if empty
    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Chưa có dữ liệu chi tiêu tháng này.</td></tr>';
        return;
    }

    data.forEach(d => {
        const avatar = d.userAvatar
            ? `<img src="${d.userAvatar}" class="rounded-circle" width="32" height="32">`
            : `<div class="rounded-circle bg-light d-flex align-items-center justify-content-center text-secondary fw-bold" style="width:32px; height:32px;">${d.userName.charAt(0)}</div>`;

        const row = `
            <tr>
                <td>
                    <div class="d-flex align-items-center gap-3">
                        ${avatar}
                        <span class="fw-bold text-dark">${d.userName}</span>
                    </div>
                </td>
                <td class="text-end fw-bold">${d.totalAmount.toLocaleString()}</td>
                <td class="text-end text-muted">${d.percentage.toFixed(1)}%</td>
            </tr>
        `;
        tbody.innerHTML += row;
    });
}

async function updateWalletSummary() {
    try {
        const response = await fetch(`/Wallets/Detail/${_walletId}?handler=WalletSummary`);
        const data = await response.json();

        if (data && data.currentBalance !== undefined) {
            const balanceEl = document.querySelector('.wallet-balance-large');
            if (balanceEl) {
                // Keep the small currency tag logic if strictly extracting node, but replacing innerHTML is easier
                balanceEl.innerHTML = `${data.currentBalance.toLocaleString()} <small style="font-size:1rem; color:var(--text-secondary);">${data.currency}</small>`;
            }
        }
    } catch (err) {
        console.error("Failed to update wallet summary", err);
    }
}

function setupSignalR() {
    _connection = new signalR.HubConnectionBuilder()
        .withUrl("/walletHub")
        .withAutomaticReconnect()
        .build();

    _connection.on("ReceiveWalletUpdate", function (updatedWalletId) {
        if (updatedWalletId == _walletId) {
            // Soft Refresh
            loadTransactions();
            loadAnalyticsData();
            updateWalletSummary();
        }
    });

    _connection.start()
        .then(() => {
            _connection.invoke("JoinWalletGroup", _walletId.toString());
        })
        .catch(err => console.error(err.toString()));
}

// Global modal helpers
window.openTransactionModal = function (walletId) {
    if (typeof window.showGlobalTransactionModal === 'function') {
        window.showGlobalTransactionModal(2); // Expense
        setTimeout(() => {
            const accountSelect = document.getElementById('AccountId');
            if (accountSelect) {
                accountSelect.value = walletId;
                accountSelect.dispatchEvent(new Event('change'));
            }
        }, 200);
    } else {
        // Fallback
        alert("Tính năng thêm giao dịch đang tải...");
    }
}

function openEditModal(id) {
    if (typeof window.showGlobalTransactionModal === 'function') {
        // Assuming global modal can support Edit if we pass ID? 
        // Current global modal in other contexts might likely be Add Only. 
        // If not supported, we can implement edit modal logic here or just do nothing.
        // For now, let's assume we want to view it.
        console.log("Edit transaction", id);
    }
}
