/**
 * Group Expenses Logic
 * Handles Group CRUD, Member Management, and Expense Splitting
 */

document.addEventListener('DOMContentLoaded', function () {
    loadGroups();
});

// Mock Data
let groups = [
    {
        id: 1,
        name: 'Du lịch Đà Lạt',
        description: 'Chuyến đi tháng 11/2025',
        icon: '✈️',
        color: '#f43f5e',
        members: [
            { id: 1, name: 'Bạn (Tôi)', role: 'Admin', avatar: null },
            { id: 2, name: 'Minh', role: 'Member', avatar: null },
            { id: 3, name: 'Hương', role: 'Member', avatar: null }
        ],
        transactions: [
            { id: 101, desc: 'Vé máy bay', amount: 3000000, payerId: 1, date: '2025-11-20', split: 'equal' },
            { id: 102, desc: 'Ăn tối BBQ', amount: 1500000, payerId: 2, date: '2025-11-21', split: 'equal' }
        ]
    },
    {
        id: 2,
        name: 'Tiền trọ',
        description: 'Chi phí hàng tháng',
        icon: '🏠',
        color: '#6366f1',
        members: [
            { id: 1, name: 'Bạn (Tôi)', role: 'Member', avatar: null },
            { id: 4, name: 'Tuấn', role: 'Admin', avatar: null }
        ],
        transactions: []
    }
];

// Current States
let currentView = 'grid';
let activeGroup = null;

// Initialization Modals
const createGroupModal = new bootstrap.Modal(document.getElementById('createGroupModal'));
const groupDetailsModal = new bootstrap.Modal(document.getElementById('groupDetailsModal'));
const addTransactionModal = new bootstrap.Modal(document.getElementById('addTransactionModal'));

/* --- Overview & List Logic --- */

function loadGroups() {
    const list = document.getElementById('groupsList');
    list.innerHTML = '';

    // Switch class based on view
    list.className = currentView === 'grid' ? 'groups-grid' : 'groups-list-view';

    groups.forEach(group => {
        const item = createGroupCard(group);
        list.appendChild(item);
    });

    updateOverviewStats();
}

function createGroupCard(group) {
    const div = document.createElement('div');
    // total spend
    const totalSpend = group.transactions.reduce((sum, t) => sum + t.amount, 0);
    const memberCount = group.members.length;

    if (currentView === 'grid') {
        div.className = 'group-card';
        div.style.borderTopColor = group.color;
        div.innerHTML = `
            <div class="group-card-header">
                <div class="group-icon-wrapper" style="background-color: ${group.color}15; color: ${group.color}">
                    ${group.icon}
                </div>
                <div class="group-action-menu">
                    <i class="fas fa-ellipsis-v"></i>
                </div>
            </div>
            <h3 class="group-name">${group.name}</h3>
            <p class="group-desc">${group.description || 'Không có mô tả'}</p>
            <div class="group-stats">
                <div class="stat">
                    <i class="fas fa-users"></i> ${memberCount}
                </div>
                <div class="stat">
                    <i class="fas fa-receipt"></i> ${group.transactions.length}
                </div>
            </div>
            <div class="group-footer">
                <span class="total-spend">${formatCurrency(totalSpend)}</span>
                <button class="btn-open-group" onclick="openGroupDetails(${group.id})">Chi tiết</button>
            </div>
        `;
    } else {
        // List View implementation (simplified)
        div.className = 'group-list-item';
        div.innerHTML = `
            <div class="list-item-left">
                 <div class="group-icon-small" style="background-color: ${group.color}15; color: ${group.color}">${group.icon}</div>
                 <div>
                    <h4 class="group-name-list">${group.name}</h4>
                    <span class="group-meta-list">${memberCount} thành viên</span>
                 </div>
            </div>
            <div class="list-item-right">
                <span class="total-spend-list">${formatCurrency(totalSpend)}</span>
                <button class="btn btn-sm btn-outline-primary" onclick="openGroupDetails(${group.id})"><i class="fas fa-arrow-right"></i></button>
            </div>
        `;
    }
    return div;
}

function switchView(view) {
    currentView = view;
    // Update buttons state
    document.querySelectorAll('.view-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.view-btn[data-view="${view}"]`).classList.add('active');
    loadGroups();
}

function updateOverviewStats() {
    document.getElementById('totalGroups').innerText = groups.length;

    // Calculate totals (Mock logic)
    let totalAll = 0;
    groups.forEach(g => {
        totalAll += g.transactions.reduce((s, t) => s + t.amount, 0);
    });
    document.getElementById('totalExpenses').innerText = formatCurrency(totalAll);

    // Mock user balance across groups
    document.getElementById('myBalance').innerText = formatCurrency(-500000); // Negative means I owe
    document.getElementById('myBalance').classList.add('danger');
}


/* --- Create / Edit Group --- */

function showCreateGroupModal() {
    document.getElementById('groupForm').reset();
    document.getElementById('groupId').value = '';
    createGroupModal.show();
}

function saveGroup() {
    if (!document.getElementById('groupForm').checkValidity()) {
        document.getElementById('groupForm').reportValidity();
        return;
    }

    const newGroup = {
        id: Date.now(),
        name: document.getElementById('groupName').value,
        description: document.getElementById('groupDescription').value,
        icon: document.getElementById('groupIcon').value,
        color: document.getElementById('groupColor').value,
        members: [{ id: 1, name: 'Bạn (Tôi)', role: 'Admin' }],
        transactions: []
    };

    groups.push(newGroup);
    createGroupModal.hide();
    loadGroups();
}


/* --- Group Details --- */

function openGroupDetails(groupId) {
    activeGroup = groups.find(g => g.id === groupId);
    if (!activeGroup) return;

    // Set Header Info
    document.getElementById('groupIconLarge').innerText = activeGroup.icon;
    document.getElementById('groupIconLarge').style.backgroundColor = activeGroup.color + '15';
    document.getElementById('groupNameTitle').innerText = activeGroup.name;
    document.getElementById('groupDescriptionText').innerText = activeGroup.description;

    // Load Default Tab
    switchGroupTab('transactions');

    groupDetailsModal.show();
}

function switchGroupTab(tabName) {
    // Hide all contents
    document.querySelectorAll('.tab-pane').forEach(el => el.style.display = 'none');
    document.getElementById(tabName + 'Tab').style.display = 'block';

    // Update buttons
    document.querySelectorAll('.group-tab').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.group-tab[data-tab="${tabName}"]`).classList.add('active');

    if (tabName === 'transactions') loadGroupTransactions();
    if (tabName === 'members') loadGroupMembers();
    if (tabName === 'balances') calculateBalances();
}

function loadGroupTransactions() {
    const container = document.getElementById('transactionsList');
    container.innerHTML = '';

    if (activeGroup.transactions.length === 0) {
        container.innerHTML = '<div class="empty-state">Chưa có giao dịch nào.</div>';
        return;
    }

    activeGroup.transactions.forEach(t => {
        const payer = activeGroup.members.find(m => m.id === t.payerId)?.name || 'Unknown';
        const div = document.createElement('div');
        div.className = 'group-transaction-item';
        div.innerHTML = `
            <div class="gt-icon"><i class="fas fa-shopping-bag"></i></div>
            <div class="gt-info">
                <div class="gt-desc">${t.desc}</div>
                <div class="gt-meta">${payer} đã trả • ${formatDate(t.date)}</div>
            </div>
            <div class="gt-amount">${formatCurrency(t.amount)}</div>
        `;
        container.appendChild(div);
    });
}

function loadGroupMembers() {
    const list = document.getElementById('membersList');
    list.innerHTML = '';

    activeGroup.members.forEach(m => {
        const div = document.createElement('div');
        div.className = 'member-item';
        div.innerHTML = `
            <div class="member-avatar">${m.name.charAt(0)}</div>
            <div class="member-info">
                <span class="member-name">${m.name}</span>
                <span class="member-role">${m.role}</span>
            </div>
        `;
        list.appendChild(div);
    });
}

// Simple Balance Calculation (Splitwise style simplified)
function calculateBalances() {
    const balancesList = document.getElementById('memberBalancesList');
    balancesList.innerHTML = '';

    // Init balances map
    let balanceMap = {}; // memberId -> amount (positive = gets back, negative = owes)
    activeGroup.members.forEach(m => balanceMap[m.id] = 0);

    activeGroup.transactions.forEach(t => {
        const paidBy = t.payerId;
        const total = t.amount;
        const splitCount = activeGroup.members.length; // Simply split equally for now
        const share = total / splitCount;

        // Payer gets back full amount MINUS their share
        balanceMap[paidBy] += (total - share);

        // Everyone else owes their share
        activeGroup.members.forEach(m => {
            if (m.id !== paidBy) {
                balanceMap[m.id] -= share;
            }
        });
    });

    // Render Balances
    activeGroup.members.forEach(m => {
        const bal = balanceMap[m.id];
        const div = document.createElement('div');
        div.className = 'balance-item';
        const colorClass = bal > 0 ? 'success' : (bal < 0 ? 'danger' : 'neutral');
        const text = bal > 0 ? `Nhận lại ${formatCurrency(bal)}` : (bal < 0 ? `Trả thêm ${formatCurrency(Math.abs(bal))}` : 'Đã xong');

        div.innerHTML = `
            <span>${m.name}</span>
            <span class="${colorClass}">${text}</span>
        `;
        balancesList.appendChild(div);
    });
}


/* --- Add Transactions to Group --- */

function showAddTransactionModal() {
    document.getElementById('transactionForm').reset();
    document.getElementById('transactionDate').value = new Date().toISOString().split('T')[0];
    addTransactionModal.show();
}

function saveTransaction() {
    const desc = document.getElementById('transactionDescription').value;
    const amount = parseFloat(document.getElementById('transactionAmount').value);

    if (!desc || !amount) return;

    activeGroup.transactions.push({
        id: Date.now(),
        desc: desc,
        amount: amount,
        payerId: 1, // Assume 'Me' paid for MVP
        date: document.getElementById('transactionDate').value,
        split: 'equal'
    });

    addTransactionModal.hide();
    switchGroupTab('transactions'); // Reload list
    updateOverviewStats();
}


// Utils
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}
function formatDate(date) {
    return new Date(date).toLocaleDateString('vi-VN');
}
