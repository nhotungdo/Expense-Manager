// ========================================
// MY WALLET PAGE - JavaScript
// Chart.js Integration & Dynamic Features
// ========================================

// Chart instances
let balanceDistributionChart = null;
let transactionTypeChart = null;
let categoryBreakdownChart = null;

// Chart color schemes
let chartColors = {
    primary: '#10b981',
    danger: '#ef4444',
    warning: '#f59e0b',
    info: '#3b82f6',
    purple: '#8b5cf6',
    pink: '#ec4899',
    gradient: [
        '#ef4444', '#f59e0b', '#10b981', '#3b82f6',
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
        '#06b6d4', '#84cc16', '#a855f7', '#f43f5e'
    ]
};

// Initialize page
document.addEventListener('DOMContentLoaded', function () {
    updateChartColorsFromTheme();
    initializeCharts();
    loadRecentTransactions();
    setupEventListeners();
    setupFormListeners();
    loadAnalyticsData('month');
});

function updateChartColorsFromTheme() {
    const style = getComputedStyle(document.documentElement);
    chartColors.primary = style.getPropertyValue('--primary').trim() || chartColors.primary;
    chartColors.danger = style.getPropertyValue('--danger').trim() || chartColors.danger;
    chartColors.warning = style.getPropertyValue('--warning').trim() || chartColors.warning;
    chartColors.info = style.getPropertyValue('--info').trim() || chartColors.info;
    chartColors.purple = style.getPropertyValue('--secondary').trim() || chartColors.purple;
}

// Setup event listeners
function setupEventListeners() {
    // Period filter for transaction type chart
    const transactionTypePeriod = document.getElementById('transactionTypePeriod');
    if (transactionTypePeriod) {
        transactionTypePeriod.addEventListener('change', function () {
            loadAnalyticsData(this.value);
        });
    }

    // Period filter for category chart
    const categoryPeriod = document.getElementById('categoryPeriod');
    if (categoryPeriod) {
        categoryPeriod.addEventListener('change', function () {
            loadCategoryBreakdown(this.value);
        });
    }

    // Period filter button
    const periodFilter = document.getElementById('periodFilter');
    if (periodFilter) {
        periodFilter.addEventListener('click', function () {
            // Show period selection dropdown
            showPeriodSelector();
        });
    }
}

// Setup Form Listeners for Modals
function setupFormListeners() {
    // Deposit Form
    const depositForm = document.getElementById('depositForm');
    if (depositForm) {
        depositForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            await handleTransactionSubmit(this, 1); // 1 = Income
        });
    }

    // Withdraw Form
    const withdrawForm = document.getElementById('withdrawForm');
    if (withdrawForm) {
        withdrawForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            await handleTransactionSubmit(this, 2); // 2 = Expense
        });
    }

    // Transfer Form
    const transferForm = document.getElementById('transferForm');
    if (transferForm) {
        transferForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            await handleTransferSubmit(this);
        });
    }

    // Add Account Form
    const addAccountForm = document.getElementById('addAccountForm');
    if (addAccountForm) {
        addAccountForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            await handleAddAccountSubmit(this);
        });
    }
}

// Handle Deposit/Withdraw Submission
async function handleTransactionSubmit(form, type) {
    const formData = new FormData(form);
    const data = {
        accountId: formData.get('accountId'),
        amount: parseFloat(formData.get('amount')),
        transactionType: type,
        note: formData.get('note'),
        currency: 'VND', // Default
        transactionDate: new Date().toISOString()
    };

    try {
        const response = await fetch('/api/Transactions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showNotification('Giao dịch thành công!', 'success');
            const modalElement = form.closest('.modal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            modal.hide();
            form.reset();
            // Refresh data
            setTimeout(() => window.location.reload(), 1000); // Reload to show new balance
        } else {
            const error = await response.json();
            showNotification(error.message || 'Giao dịch thất bại', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('Đã xảy ra lỗi kết nối', 'error');
    }
}

// Handle Transfer Submission
async function handleTransferSubmit(form) {
    const formData = new FormData(form);
    const data = {
        sourceAccountId: formData.get('sourceAccountId'),
        targetAccountId: formData.get('targetAccountId'),
        amount: parseFloat(formData.get('amount')),
        note: formData.get('note')
    };

    try {
        const response = await fetch('/api/Transactions/transfer', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showNotification('Chuyển khoản thành công!', 'success');
            const modalElement = form.closest('.modal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            modal.hide();
            form.reset();
            setTimeout(() => window.location.reload(), 1000);
        } else {
            const error = await response.json();
            showNotification(error.message || 'Chuyển khoản thất bại', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('Đã xảy ra lỗi kết nối', 'error');
    }
}

// Handle Add Account Submission
async function handleAddAccountSubmit(form) {
    const formData = new FormData(form);
    const includeInTotal = formData.get('includeInTotal') === 'on';

    const data = {
        name: formData.get('name'),
        accountType: parseInt(formData.get('accountType')),
        initialBalance: parseFloat(formData.get('initialBalance')),
        currency: formData.get('currency'),
        icon: formData.get('icon'),
        color: formData.get('color'),
        includeInTotal: includeInTotal
    };

    try {
        const response = await fetch('/api/Accounts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showNotification('Tạo tài khoản thành công!', 'success');
            const modalElement = form.closest('.modal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            modal.hide();
            form.reset();
            setTimeout(() => window.location.reload(), 1000);
        } else {
            const error = await response.json();
            showNotification(error.message || 'Tạo tài khoản thất bại', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('Đã xảy ra lỗi kết nối', 'error');
    }
}

// Initialize all charts
function initializeCharts() {
    initBalanceDistributionChart();
    initTransactionTypeChart();
    initCategoryBreakdownChart();
}

// Balance Distribution Chart (Doughnut)
function initBalanceDistributionChart() {
    const ctx = document.getElementById('balanceDistributionChart');
    if (!ctx) {
        console.warn('Balance distribution chart canvas not found');
        return;
    }

    // Sample data - replace with actual API call
    const data = {
        labels: ['Tiền mặt', 'Ngân hàng', 'Thẻ tín dụng', 'Ví điện tử'],
        datasets: [{
            data: [5000000, 15000000, 3000000, 2000000],
            backgroundColor: [
                chartColors.primary,
                chartColors.info,
                chartColors.warning,
                chartColors.purple
            ],
            borderWidth: 4,
            borderColor: '#ffffff',
            hoverOffset: 15,
            hoverBorderWidth: 6
        }]
    };

    const config = {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '70%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.9)',
                    padding: 12,
                    cornerRadius: 8,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: {
                        label: function (context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ${formatCurrency(value)} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000,
                easing: 'easeInOutQuart'
            }
        }
    };

    balanceDistributionChart = new Chart(ctx, config);

    // Create custom legend
    createCustomLegend(data, 'chartLegend');
}

// Transaction Type Chart (Pie)
function initTransactionTypeChart() {
    const ctx = document.getElementById('transactionTypeChart');
    if (!ctx) {
        console.warn('Transaction type chart canvas not found');
        return;
    }

    const data = {
        labels: ['Thu nhập', 'Chi tiêu'],
        datasets: [{
            data: [0, 0],
            backgroundColor: [chartColors.primary, chartColors.danger],
            borderWidth: 4,
            borderColor: '#ffffff',
            hoverOffset: 15
        }]
    };

    const config = {
        type: 'pie',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.9)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            return `${label}: ${formatCurrency(value)}`;
                        }
                    }
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000
            }
        }
    };

    transactionTypeChart = new Chart(ctx, config);
}

// Category Breakdown Chart (Doughnut)
function initCategoryBreakdownChart() {
    const ctx = document.getElementById('categoryBreakdownChart');
    if (!ctx) {
        console.warn('Category breakdown chart canvas not found');
        return;
    }

    const data = {
        labels: [],
        datasets: [{
            data: [],
            backgroundColor: chartColors.gradient,
            borderWidth: 4,
            borderColor: '#ffffff',
            hoverOffset: 15
        }]
    };

    const config = {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.9)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ${formatCurrency(value)} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000
            }
        }
    };

    categoryBreakdownChart = new Chart(ctx, config);
}

// Load analytics data from API
async function loadAnalyticsData(period) {
    try {
        const response = await fetch(`/api/Report/wallet-summary?period=${period}`);

        if (!response.ok) {
            console.warn('API call failed, using mock data');
            // showNotification('Không thể tải dữ liệu trực tiếp. Đang hiển thị dữ liệu mẫu.', 'warning');
            updateTransactionTypeChart({
                totalIncome: 0,
                totalExpense: 0
            });
            return;
        }

        const data = await response.json();
        updateTransactionTypeChart(data);
    } catch (error) {
        console.error('Error loading analytics:', error);
        // showNotification('Không thể tải dữ liệu phân tích.', 'error');
    }
}

// Update transaction type chart
function updateTransactionTypeChart(data) {
    if (!transactionTypeChart) return;

    transactionTypeChart.data.datasets[0].data = [
        data.totalIncome || 0,
        data.totalExpense || 0
    ];
    transactionTypeChart.update('active');

    // Update stat values
    const incomeAmount = document.getElementById('incomeAmount');
    const expenseAmount = document.getElementById('expenseAmount');

    if (incomeAmount) {
        incomeAmount.textContent = formatCurrency(data.totalIncome || 0);
    }
    if (expenseAmount) {
        expenseAmount.textContent = formatCurrency(data.totalExpense || 0);
    }
}

// Load category breakdown
async function loadCategoryBreakdown(period) {
    try {
        const response = await fetch(`/api/Report/expense-breakdown?period=${period}`);

        if (!response.ok) {
            // Mock data if failed
            updateCategoryBreakdownChart([
                { categoryName: 'Ăn uống', amount: 0 },
                { categoryName: 'Di chuyển', amount: 0 },
                { categoryName: 'Mua sắm', amount: 0 }
            ]);
            return;
        }

        const data = await response.json();
        updateCategoryBreakdownChart(data);
    } catch (error) {
        console.error('Error loading category breakdown:', error);
    }
}

// Update category breakdown chart
function updateCategoryBreakdownChart(categories) {
    if (!categoryBreakdownChart) return;

    const labels = categories.map(c => c.categoryName);
    const data = categories.map(c => c.amount);

    categoryBreakdownChart.data.labels = labels;
    categoryBreakdownChart.data.datasets[0].data = data;
    categoryBreakdownChart.update('active');

    // Update category list
    updateCategoryList(categories);
}

// Update category list
function updateCategoryList(categories) {
    const categoryList = document.getElementById('categoryList');
    if (!categoryList) return;

    const total = categories.reduce((sum, c) => sum + c.amount, 0);

    categoryList.innerHTML = categories.map((category, index) => {
        const percentage = total > 0 ? ((category.amount / total) * 100).toFixed(1) : 0;
        const color = chartColors.gradient[index % chartColors.gradient.length];

        return `
            <div class="category-item">
                <div class="category-color" style="background-color: ${color};"></div>
                <span class="category-name">${category.categoryName}</span>
                <span class="category-amount">${formatCurrency(category.amount)} (${percentage}%)</span>
            </div>
        `;
    }).join('');
}

// Load recent transactions
async function loadRecentTransactions() {
    try {
        const response = await fetch('/api/Transactions/recent?limit=5');

        if (!response.ok) {
            console.error('Failed to load recent transactions');
            return;
        }

        const transactions = await response.json();
        displayRecentTransactions(transactions);
    } catch (error) {
        console.error('Error loading recent transactions:', error);
    }
}

// Display recent transactions
function displayRecentTransactions(transactions) {
    const container = document.getElementById('recentTransactions');
    if (!container) return;

    if (!transactions || transactions.length === 0) {
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: #64748b;">
                <i class="fas fa-inbox" style="font-size: 48px; margin-bottom: 16px; opacity: 0.5;"></i>
                <p>Chưa có giao dịch nào gần đây</p>
            </div>
        `;
        return;
    }

    container.innerHTML = transactions.map(transaction => {
        // Dịch loại giao dịch nếu cần (though logic backend đã làm điều này)
        // Adjust colors
        const amountColor = transaction.transactionType === 1 ? '#10b981' :
            transaction.transactionType === 2 ? '#ef4444' : '#3b82f6';

        const amountPrefix = transaction.transactionType === 1 ? '+' :
            transaction.transactionType === 2 ? '-' : '';

        return `
            <div class="transaction-item" style="display: flex; align-items: center; gap: 16px; padding: 16px; border-bottom: 1px solid #f1f5f9; cursor: pointer; transition: all 0.3s;" 
                 onclick="window.location.href='/Transactions/Create?id=${transaction.id}'"
                 onmouseover="this.style.background='#f8fafc'" 
                 onmouseout="this.style.background='transparent'">
                <div style="width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 20px; background-color: ${transaction.categoryColor || '#6366f1'}20; color: ${transaction.categoryColor || '#6366f1'};">
                    <i class="${transaction.categoryIcon || 'fas fa-wallet'}"></i>
                </div>
                <div style="flex: 1; min-width: 0;">
                    <h6 style="font-size: 14px; font-weight: 600; margin: 0 0 4px 0; color: #1e293b;">${transaction.categoryName || 'Chưa phân loại'}</h6>
                    <p style="font-size: 12px; color: #64748b; margin: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${transaction.description || transaction.note || ''}</p>
                </div>
                <div style="text-align: right;">
                    <span style="font-size: 16px; font-weight: 700; color: ${amountColor};">
                        ${amountPrefix}${formatCurrency(transaction.amount)}
                    </span>
                    <div style="font-size: 12px; color: #94a3b8;">${new Date(transaction.transactionDate).toLocaleDateString('vi-VN')}</div>
                </div>
            </div>
        `;
    }).join('');
}

// Create custom legend
function createCustomLegend(chartData, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const total = chartData.datasets[0].data.reduce((a, b) => a + b, 0);

    container.innerHTML = chartData.labels.map((label, index) => {
        const value = chartData.datasets[0].data[index];
        const percentage = ((value / total) * 100).toFixed(1);
        const color = chartData.datasets[0].backgroundColor[index];

        return `
            <div style="display: flex; align-items: center; justify-content: space-between; padding: 8px 0;">
                <div style="display: flex; align-items: center; gap: 8px;">
                    <div style="width: 12px; height: 12px; border-radius: 50%; background-color: ${color};"></div>
                    <span style="font-size: 13px; color: rgba(255, 255, 255, 0.9);">${label}</span>
                </div>
                <span style="font-size: 13px; font-weight: 600; color: white;">${percentage}%</span>
            </div>
        `;
    }).join('');
}

// Show period selector
function showPeriodSelector() {
    const periods = ['Tuần này', 'Tháng này', 'Quý này', 'Năm nay'];
    const periodMap = ['week', 'month', 'quarter', 'year'];

    // Create a custom modal or dropdown for better UI
    // For now using simple prompt is replaced by a cleaner approach? 
    // Actually prompt is ugly. Let's assume the user clicks the filter and cycles through or we just cycle.
    // Or better, let's just make it a toggle for now.
    // Given the prompt constraints, I'll stick to a simple prompt but translated.

    const selectedPeriod = prompt('Chọn khoảng thời gian:\n' + periods.map((p, i) => `${i + 1}. ${p}`).join('\n'));

    if (selectedPeriod) {
        const index = parseInt(selectedPeriod) - 1;
        if (index >= 0 && index < periods.length) {
            document.getElementById('selectedPeriod').textContent = periods[index];
            loadAnalyticsData(periodMap[index]);
        }
    }
}

// Utility function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Show notification
function showNotification(message, type = 'info') {
    const colors = {
        success: '#10b981',
        error: '#ef4444',
        info: '#3b82f6',
        warning: '#f59e0b'
    };

    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${colors[type]};
        color: white;
        padding: 16px 24px;
        border-radius: 12px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
        z-index: 10000;
        font-weight: 600;
        animation: slideIn 0.3s ease-out;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease-out';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Export
window.walletPage = {
    loadAnalyticsData,
    loadCategoryBreakdown,
    loadRecentTransactions
};
