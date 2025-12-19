// Home Page - Dashboard with Pie Charts
// Handles transaction visualization and analytics

let expenseChartInstance = null;
let incomeChartInstance = null;

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
        minimumFractionDigits: 0
    }).format(amount);
}

// Load personal wallet data (summary cards)
async function loadPersonalWalletData() {
    try {
        const response = await fetch('/api/Dashboard/personal-wallet', {
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load wallet data');
        }

        const data = await response.json();
        
        // Update summary cards
        document.getElementById('cardTotalAssets').textContent = formatCurrency(data.totalBalance || 0);
        document.getElementById('monthlyIncome').textContent = formatCurrency(data.monthlyIncome || 0);
        document.getElementById('monthlyExpense').textContent = formatCurrency(data.monthlyExpense || 0);

    } catch (error) {
        console.error('Error loading wallet data:', error);
        document.getElementById('cardTotalAssets').textContent = 'Lỗi tải dữ liệu';
        document.getElementById('monthlyIncome').textContent = 'N/A';
        document.getElementById('monthlyExpense').textContent = 'N/A';
    }
}

// Load expense breakdown by category
async function loadExpenseBreakdown(period = 'month') {
    try {
        const response = await fetch(`/api/Dashboard/expense-breakdown?period=${period}`, {
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load expense breakdown');
        }

        const data = await response.json();
        renderExpenseChart(data);

    } catch (error) {
        console.error('Error loading expense breakdown:', error);
        renderExpenseChart([]);
    }
}

// Load income breakdown by category
async function loadIncomeBreakdown(period = 'month') {
    try {
        const response = await fetch(`/api/Dashboard/income-breakdown?period=${period}`, {
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load income breakdown');
        }

        const data = await response.json();
        renderIncomeChart(data);

    } catch (error) {
        console.error('Error loading income breakdown:', error);
        renderIncomeChart([]);
    }
}

// Render expense pie chart
function renderExpenseChart(data) {
    const canvas = document.getElementById('expenseChart');
    const ctx = canvas.getContext('2d');

    // Destroy existing chart
    if (expenseChartInstance) {
        expenseChartInstance.destroy();
    }

    // Prepare data
    const labels = data.map(item => item.categoryName || 'Khác');
    const amounts = data.map(item => item.amount || 0);
    const total = amounts.reduce((sum, val) => sum + val, 0);

    // Check if no data
    if (total === 0 || data.length === 0) {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.font = '14px Inter';
        ctx.fillStyle = '#94a3b8';
        ctx.textAlign = 'center';
        ctx.fillText('Chưa có dữ liệu chi tiêu', canvas.width / 2, canvas.height / 2);
        return;
    }

    // Create chart
    expenseChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: amounts,
                backgroundColor: [
                    '#ef4444', // Red
                    '#f59e0b', // Orange
                    '#10b981', // Green
                    '#3b82f6', // Blue
                    '#8b5cf6', // Purple
                    '#ec4899', // Pink
                    '#14b8a6', // Teal
                    '#f97316', // Deep Orange
                    '#06b6d4', // Cyan
                    '#84cc16', // Lime
                    '#a855f7', // Violet
                    '#f43f5e'  // Rose
                ],
                borderWidth: 3,
                borderColor: '#ffffff',
                hoverOffset: 15,
                hoverBorderWidth: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            cutout: '60%',
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            family: "'Inter', sans-serif",
                            weight: '500'
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        boxWidth: 12,
                        generateLabels: function(chart) {
                            const data = chart.data;
                            if (data.labels.length && data.datasets.length) {
                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    const percentage = ((value / total) * 100).toFixed(1);
                                    return {
                                        text: `${label} (${percentage}%)`,
                                        fillStyle: data.datasets[0].backgroundColor[i],
                                        hidden: false,
                                        index: i
                                    };
                                });
                            }
                            return [];
                        }
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.85)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1,
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
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
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
    });
}

// Render income pie chart
function renderIncomeChart(data) {
    const canvas = document.getElementById('incomeChart');
    const ctx = canvas.getContext('2d');

    // Destroy existing chart
    if (incomeChartInstance) {
        incomeChartInstance.destroy();
    }

    // Prepare data
    const labels = data.map(item => item.categoryName || 'Khác');
    const amounts = data.map(item => item.amount || 0);
    const total = amounts.reduce((sum, val) => sum + val, 0);

    // Check if no data
    if (total === 0 || data.length === 0) {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.font = '14px Inter';
        ctx.fillStyle = '#94a3b8';
        ctx.textAlign = 'center';
        ctx.fillText('Chưa có dữ liệu thu nhập', canvas.width / 2, canvas.height / 2);
        return;
    }

    // Create chart
    incomeChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: amounts,
                backgroundColor: [
                    '#10b981', // Green
                    '#3b82f6', // Blue
                    '#8b5cf6', // Purple
                    '#14b8a6', // Teal
                    '#06b6d4', // Cyan
                    '#84cc16', // Lime
                    '#22c55e', // Emerald
                    '#0ea5e9', // Sky
                    '#6366f1', // Indigo
                    '#a855f7', // Violet
                    '#2dd4bf', // Teal light
                    '#4ade80'  // Green light
                ],
                borderWidth: 3,
                borderColor: '#ffffff',
                hoverOffset: 15,
                hoverBorderWidth: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            cutout: '60%',
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            family: "'Inter', sans-serif",
                            weight: '500'
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        boxWidth: 12,
                        generateLabels: function(chart) {
                            const data = chart.data;
                            if (data.labels.length && data.datasets.length) {
                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    const percentage = ((value / total) * 100).toFixed(1);
                                    return {
                                        text: `${label} (${percentage}%)`,
                                        fillStyle: data.datasets[0].backgroundColor[i],
                                        hidden: false,
                                        index: i
                                    };
                                });
                            }
                            return [];
                        }
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.85)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1,
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
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
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
    });
}

// Load recent transactions
async function loadRecentTransactions() {
    try {
        const response = await fetch('/api/Transactions/recent?limit=5', {
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load transactions');
        }

        const transactions = await response.json();
        renderTransactionList(transactions);

    } catch (error) {
        console.error('Error loading transactions:', error);
        document.getElementById('transactionList').innerHTML = `
            <div class="p-8 text-center text-slate-400">
                <i class="fas fa-exclamation-circle text-2xl mb-2"></i>
                <p>Không thể tải giao dịch</p>
            </div>
        `;
    }
}

// Render transaction list
function renderTransactionList(transactions) {
    const container = document.getElementById('transactionList');
    
    if (!transactions || transactions.length === 0) {
        container.innerHTML = `
            <div class="p-8 text-center text-slate-400">
                <i class="fas fa-inbox text-3xl mb-2"></i>
                <p>Chưa có giao dịch nào</p>
            </div>
        `;
        return;
    }

    const html = transactions.map(trans => {
        const isIncome = trans.type === 1 || trans.type === 'Income';
        const icon = isIncome ? 'fa-arrow-down' : 'fa-arrow-up';
        const colorClass = isIncome ? 'text-emerald-600 bg-emerald-50' : 'text-rose-600 bg-rose-50';
        const amountColor = isIncome ? 'text-emerald-600' : 'text-rose-600';
        const sign = isIncome ? '+' : '-';

        return `
            <div class="flex items-center justify-between p-4 hover:bg-slate-50 transition-colors border-b border-slate-100 last:border-0">
                <div class="flex items-center gap-4">
                    <div class="w-10 h-10 rounded-xl ${colorClass} flex items-center justify-center">
                        <i class="fas ${icon}"></i>
                    </div>
                    <div>
                        <p class="font-semibold text-slate-800">${trans.categoryName || 'Khác'}</p>
                        <p class="text-sm text-slate-500">${trans.note || 'Không có ghi chú'}</p>
                        <p class="text-xs text-slate-400 mt-0.5">${new Date(trans.transactionDate).toLocaleDateString('vi-VN')}</p>
                    </div>
                </div>
                <div class="text-right">
                    <p class="font-bold ${amountColor}">${sign}${formatCurrency(Math.abs(trans.amount))}</p>
                    <p class="text-xs text-slate-400">${trans.accountName || 'N/A'}</p>
                </div>
            </div>
        `;
    }).join('');

    container.innerHTML = html;
}

// Load accounts for dropdowns
async function loadAccounts() {
    try {
        const response = await fetch('/api/Accounts', {
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load accounts');
        }

        const accounts = await response.json();
        
        // Populate transaction modal account dropdown
        const transAccountSelect = document.getElementById('transAccount');
        if (transAccountSelect) {
            transAccountSelect.innerHTML = accounts.map(acc => 
                `<option value="${acc.accountId}">${acc.accountName} (${formatCurrency(acc.balance)})</option>`
            ).join('');
        }

        // Populate transfer modal dropdowns
        const transferSource = document.getElementById('transferSource');
        const transferTarget = document.getElementById('transferTarget');
        
        if (transferSource && transferTarget) {
            const accountOptions = accounts.map(acc => 
                `<option value="${acc.accountId}">${acc.accountName} (${formatCurrency(acc.balance)})</option>`
            ).join('');
            
            transferSource.innerHTML = '<option value="">Chọn ví nguồn...</option>' + accountOptions;
            transferTarget.innerHTML = '<option value="">Chọn ví đích...</option>' + accountOptions;
        }

    } catch (error) {
        console.error('Error loading accounts:', error);
    }
}

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    // Load initial data
    loadPersonalWalletData();
    loadExpenseBreakdown('month');
    loadIncomeBreakdown('month');
    loadRecentTransactions();
    loadAccounts();

    // Setup period change listeners
    document.getElementById('expenseChartPeriod')?.addEventListener('change', function(e) {
        loadExpenseBreakdown(e.target.value);
    });

    document.getElementById('incomeChartPeriod')?.addEventListener('change', function(e) {
        loadIncomeBreakdown(e.target.value);
    });

    // Set default transaction date
    const transDateInput = document.getElementById('transDate');
    if (transDateInput) {
        transDateInput.valueAsDate = new Date();
    }
});

// Export functions for use in inline scripts
window.loadPersonalWalletData = loadPersonalWalletData;
window.loadAccounts = loadAccounts;
