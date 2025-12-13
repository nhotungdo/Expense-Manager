/**
 * Debts Management Logic
 */

document.addEventListener('DOMContentLoaded', function () {
    loadDebts();
    initDebtCharts();
});

// Mock Data for Debts (Replace with API calls later)
let debts = [
    {
        id: 1,
        type: 'Borrow',
        person: 'Anh Nam',
        amount: 5000000,
        category: 'Business',
        date: '2025-11-20',
        dueDate: '2025-12-20',
        note: 'Vay vốn nhập hàng',
        interestRate: 0,
        paidAmount: 0,
        status: 'Active'
    },
    {
        id: 2,
        type: 'Lend',
        person: 'Chị Lan',
        amount: 2000000,
        category: 'Personal',
        date: '2025-12-01',
        dueDate: '2025-12-15',
        note: 'Cho vay tiền mặt',
        interestRate: 0,
        paidAmount: 0,
        status: 'Overdue'
    },
    {
        id: 3,
        type: 'Borrow',
        person: 'Ngân hàng ABC',
        amount: 100000000,
        category: 'Business',
        date: '2025-01-15',
        dueDate: '2025-01-15', // Recurring really
        note: 'Vay mua xe',
        interestRate: 8.5,
        paidAmount: 25000000,
        status: 'Active'
    }
];

let debtModal;
let paymentModal;

// Initialize Modals
function initModals() {
    debtModal = new bootstrap.Modal(document.getElementById('debtModal'));
    paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
}

// Load and Render Debts
function loadDebts() {
    // Determine Modal initialization if not done
    if (!debtModal) initModals();

    const borrowList = document.getElementById('borrowList');
    const lendList = document.getElementById('lendList');

    // Clear lists
    borrowList.innerHTML = '';
    lendList.innerHTML = '';

    let totalBorrow = 0;
    let totalLend = 0;
    let borrowCount = 0;
    let lendCount = 0;
    let urgentCount = 0;

    const today = new Date();

    debts.forEach(debt => {
        const debtEl = createDebtElement(debt);
        const remaining = debt.amount - debt.paidAmount;

        if (debt.type === 'Borrow') {
            borrowList.appendChild(debtEl);
            totalBorrow += remaining;
            borrowCount++;
        } else {
            lendList.appendChild(debtEl);
            totalLend += remaining;
            lendCount++;
        }

        // Check urgency (due within 7 days)
        if (debt.dueDate) {
            const due = new Date(debt.dueDate);
            const diffTime = due - today;
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            if (diffDays >= 0 && diffDays <= 7 && remaining > 0) {
                urgentCount++;
            }
        }
    });

    // Update Overview Cards
    document.getElementById('totalBorrow').innerText = formatCurrency(totalBorrow);
    document.getElementById('borrowCount').innerText = borrowCount;
    document.getElementById('borrowCountBadge').innerText = borrowCount;

    document.getElementById('totalLend').innerText = formatCurrency(totalLend);
    document.getElementById('lendCount').innerText = lendCount;
    document.getElementById('lendCountBadge').innerText = lendCount;

    document.getElementById('urgentCount').innerText = urgentCount;

    const net = totalLend - totalBorrow;
    document.getElementById('netPosition').innerText = formatCurrency(Math.abs(net));
    const netDesc = document.getElementById('netDescription');
    if (net > 0) {
        document.getElementById('netPosition').classList.add('success');
        document.getElementById('netPosition').classList.remove('danger');
        netDesc.innerText = 'Bạn đang dương (Người khác nợ bạn nhiều hơn)';
    } else if (net < 0) {
        document.getElementById('netPosition').classList.add('danger');
        document.getElementById('netPosition').classList.remove('success');
        netDesc.innerText = 'Bạn đang âm (Bạn nợ người khác nhiều hơn)';
    } else {
        netDesc.innerText = 'Cân bằng tài chính';
    }

    renderReminders();
}

// Create DOM Element for a Debt Item
function createDebtElement(debt) {
    const div = document.createElement('div');
    div.className = 'debt-item';

    const remaining = debt.amount - debt.paidAmount;
    const progress = (debt.paidAmount / debt.amount) * 100;

    // Status Logic
    let statusClass = 'status-active';
    let statusText = 'Đang vay';

    if (remaining <= 0) {
        statusText = 'Đã trả xong';
        statusClass = 'status-paid';
    } else if (debt.dueDate && new Date(debt.dueDate) < new Date()) {
        statusText = 'Quá hạn';
        statusClass = 'status-overdue';
    }

    div.innerHTML = `
        <div class="debt-icon ${debt.type === 'Borrow' ? 'borrow' : 'lend'}">
            <i class="fas fa-${debt.type === 'Borrow' ? 'arrow-down' : 'arrow-up'}"></i>
        </div>
        <div class="debt-details">
            <div class="debt-header">
                <h4 class="debt-person">${debt.person}</h4>
                <span class="debt-amount ${debt.type === 'Borrow' ? 'danger' : 'success'}">${formatCurrency(debt.amount)}</span>
            </div>
            <p class="debt-meta">
                <span class="badge ${debt.category.toLowerCase()}">${getCategoryName(debt.category)}</span>
                <span class="date"><i class="far fa-calendar-alt"></i> ${debt.dueDate ? formatDate(debt.dueDate) : 'Không có hạn'}</span>
            </p>
            <div class="debt-progress">
                <div class="progress-bar-bg">
                    <div class="progress-bar-fill" style="width: ${progress}%"></div>
                </div>
                <div class="progress-text">
                    <span>Đã trả: ${formatCurrency(debt.paidAmount)}</span>
                    <span>Còn lại: ${formatCurrency(remaining)}</span>
                </div>
            </div>
            <div class="debt-actions" ${remaining <= 0 ? 'style="display:none"' : ''}>
                <button class="btn-action primary" onclick="openPaymentModal(${debt.id})">
                    <i class="fas fa-money-bill-wave"></i> Thanh toán
                </button>
                 <button class="btn-action secondary" onclick="editDebt(${debt.id})">
                    <i class="fas fa-edit"></i>
                </button>
            </div>
            <div class="debt-status ${statusClass}">${statusText}</div>
        </div>
    `;
    return div;
}

// Open Add/Edit Modal
function openDebtModal(type, id = null) {
    const form = document.getElementById('debtForm');
    form.reset();
    document.getElementById('debtId').value = '';

    // Set type buttons
    document.querySelectorAll('.type-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.type-btn[data-type="${type}"]`).classList.add('active');
    document.getElementById('debtType').value = type;

    document.getElementById('debtModalTitle').innerText = id ? 'Cập nhật khoản nợ' : (type === 'Borrow' ? 'Thêm khoản đi vay' : 'Thêm khoản cho vay');

    if (id) {
        const debt = debts.find(d => d.id === id);
        if (debt) {
            document.getElementById('debtId').value = debt.id;
            document.getElementById('debtPerson').value = debt.person;
            document.getElementById('debtAmount').value = debt.amount;
            document.getElementById('debtCategory').value = debt.category;
            document.getElementById('debtDate').value = debt.date;
            document.getElementById('dueDate').value = debt.dueDate;
            document.getElementById('interestRate').value = debt.interestRate;
            document.getElementById('debtNote').value = debt.note;
        }
    } else {
        document.getElementById('debtDate').value = new Date().toISOString().split('T')[0];
    }

    debtModal.show();
}

// Save Debt
function saveDebt() {
    // Simple Validation
    if (!document.getElementById('debtForm').checkValidity()) {
        document.getElementById('debtForm').reportValidity();
        return;
    }

    const id = document.getElementById('debtId').value;
    const type = document.getElementById('debtType').value;
    const newDebt = {
        id: id ? parseInt(id) : Date.now(),
        type: type,
        person: document.getElementById('debtPerson').value,
        amount: parseFloat(document.getElementById('debtAmount').value),
        category: document.getElementById('debtCategory').value,
        date: document.getElementById('debtDate').value,
        dueDate: document.getElementById('dueDate').value,
        interestRate: parseFloat(document.getElementById('interestRate').value),
        note: document.getElementById('debtNote').value,
        paidAmount: id ? (debts.find(d => d.id == id)?.paidAmount || 0) : 0,
        status: 'Active'
    };

    if (id) {
        const index = debts.findIndex(d => d.id == id);
        debts[index] = newDebt;
    } else {
        debts.push(newDebt);
    }

    debtModal.hide();
    loadDebts();
    updateCharts();
}

// Open Payment Modal
function openPaymentModal(debtId) {
    const debt = debts.find(d => d.id === debtId);
    if (!debt) return;

    document.getElementById('paymentDebtId').value = debtId;
    document.getElementById('paymentPerson').innerText = debt.person;
    document.getElementById('paymentTotal').innerText = formatCurrency(debt.amount - debt.paidAmount);
    document.getElementById('paymentAmount').value = debt.amount - debt.paidAmount; // Default to full payment
    document.getElementById('paymentDate').value = new Date().toISOString().split('T')[0];

    paymentModal.show();
}

// Process Payment
function processPayment() {
    const debtId = parseInt(document.getElementById('paymentDebtId').value);
    const amount = parseFloat(document.getElementById('paymentAmount').value);

    if (isNaN(amount) || amount <= 0) {
        alert("Vui lòng nhập số tiền hợp lệ");
        return;
    }

    const debtIndex = debts.findIndex(d => d.id === debtId);
    if (debtIndex !== -1) {
        debts[debtIndex].paidAmount += amount;
        // In real app, create a Transaction record here too
    }

    paymentModal.hide();
    loadDebts();
}

// Handle Type Button Clicks
document.querySelectorAll('.type-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const type = this.getAttribute('data-type');
        openDebtModal(type); // Reset checks
    });
});

// Helper: Format Currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Helper: Format Date
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN'); // dd/mm/yyyy
}

// Helper: Get Category Name
function getCategoryName(key) {
    const map = {
        'Personal': 'Cá nhân',
        'Business': 'Kinh doanh',
        'Family': 'Gia đình',
        'Friend': 'Bạn bè',
        'Other': 'Khác'
    };
    return map[key] || key;
}

// Charts Logic using Chart.js
let trendChart;
let categoryChart;

function initDebtCharts() {
    const ctxTrend = document.getElementById('debtTrendChart').getContext('2d');
    trendChart = new Chart(ctxTrend, {
        type: 'line',
        data: {
            labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'], // Mock labels
            datasets: [{
                label: 'Đi vay',
                data: [10000000, 12000000, 15000000, 8000000, 5000000, 5000000],
                borderColor: '#ef4444',
                tension: 0.4
            }, {
                label: 'Cho vay',
                data: [2000000, 5000000, 5000000, 6000000, 2000000, 2000000],
                borderColor: '#10b981',
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    const ctxCat = document.getElementById('debtCategoryChart').getContext('2d');
    categoryChart = new Chart(ctxCat, {
        type: 'doughnut',
        data: {
            labels: ['Kinh doanh', 'Cá nhân', 'Gia đình'],
            datasets: [{
                data: [70, 20, 10],
                backgroundColor: ['#6366f1', '#10b981', '#f59e0b']
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false // Custom legend
                }
            }
        }
    });
}

function updateCharts() {
    // In a real app, re-calculate chart data from 'debts' array
}

function renderReminders() {
    const container = document.getElementById('remindersList');
    container.innerHTML = '';

    // Sort by due date
    const urgent = debts.filter(d => d.dueDate && d.amount > d.paidAmount).sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate)).slice(0, 3);

    if (urgent.length === 0) {
        container.innerHTML = '<p style="color:#aaa; text-align:center;">Không có nhắc nhở nào.</p>';
        return;
    }

    urgent.forEach(d => {
        const div = document.createElement('div');
        div.className = 'reminder-item';
        div.innerHTML = `
            <div class="reminder-icon">
                <i class="fas fa-bell"></i>
            </div>
            <div class="reminder-content">
                <p class="reminder-msg">Hạn trả nợ <strong>${d.person}</strong></p>
                <p class="reminder-date">${formatDate(d.dueDate)} - ${formatCurrency(d.amount - d.paidAmount)}</p>
            </div>
            <button class="btn-check" onclick="openPaymentModal(${d.id})"><i class="fas fa-check"></i></button>
        `;
        container.appendChild(div);
    });
}
