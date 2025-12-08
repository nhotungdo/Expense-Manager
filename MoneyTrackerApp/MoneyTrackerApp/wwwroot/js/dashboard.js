// Dashboard JavaScript
let dashboardData = null;
let charts = {
    transactionTypes: null,
    categories: null,
    incomeExpense: null
};

// Modern color palette
const COLORS = {
    income: '#10b981',
    expense: '#ef4444',
    transfer: '#3b82f6',
    categories: [
        '#ef4444', '#f59e0b', '#10b981', '#3b82f6', 
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
        '#06b6d4', '#84cc16', '#a855f7', '#f43f5e'
    ]
};

document.addEventListener('DOMContentLoaded', function() {
    loadDashboardData();
    
    // Period filter
    document.getElementById('periodFilter')?.addEventListener('change', function() {
        loadDashboardData(this.value);
    });
    
    // Initialize tooltips
    initializeTooltips();
});

async function loadDashboardData(period = 'month') {
    try {
        const token = localStorage.getItem('accessToken');
        const response = await fetch(`/api/dashboard/stats?period=${period}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (!response.ok) {
            // Use mock data for development/testing
            if (typeof MockAPI !== 'undefined') {
                console.warn('Using mock data for development');
                dashboardData = await MockAPI.getDashboardStats(period);
                renderDashboard();
                return;
            }
            throw new Error('Failed to load dashboard data');
        }
        
        dashboardData = await response.json();
        renderDashboard();
    } catch (error) {
        console.error('Error loading dashboard:', error);
        
        // Fallback to mock data if available
        if (typeof MockAPI !== 'undefined') {
            console.warn('API failed, using mock data');
            dashboardData = await MockAPI.getDashboardStats(period);
            renderDashboard();
        } else {
            showError('Không thể tải dữ liệu dashboard');
        }
    }
}

function renderDashboard() {
    if (!dashboardData) return;
    
    // Update stats with animations
    animateValue('totalIncome', 0, dashboardData.totalIncome, 1000);
    animateValue('totalExpense', 0, dashboardData.totalExpense, 1000);
    animateValue('netIncome', 0, dashboardData.netIncome, 1000);
    animateValue('totalBalance', 0, dashboardData.totalBalance, 1000);
    
    // Render charts
    renderTransactionTypesChart(dashboardData.transactionTypes);
    renderCategoryChart(dashboardData.categorySpending);
    renderIncomeExpenseChart(dashboardData.monthlyTrends);
    
    // Render transactions
    renderRecentTransactions(dashboardData.recentTransactions);
    
    // Render AI suggestions
    renderAiSuggestions(dashboardData.aiSuggestions);
    
    // Render alerts
    renderFinancialAlerts(dashboardData.financialAlerts);
    
    // Render accounts
    renderAccounts(dashboardData.accounts);
}

// Render Transaction Types Pie Chart
function renderTransactionTypesChart(data) {
    const ctx = document.getElementById('transactionTypesChart');
    if (!ctx) return;
    
    // Destroy existing chart
    if (charts.transactionTypes) {
        charts.transactionTypes.destroy();
    }
    
    const chartData = data || [
        { type: 'Income', amount: 0, count: 0 },
        { type: 'Expense', amount: 0, count: 0 },
        { type: 'Transfer', amount: 0, count: 0 }
    ];
    
    const total = chartData.reduce((sum, item) => sum + item.amount, 0);
    
    charts.transactionTypes = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: chartData.map(d => {
                const percentage = total > 0 ? ((d.amount / total) * 100).toFixed(1) : 0;
                return `${getTransactionTypeLabel(d.type)} (${percentage}%)`;
            }),
            datasets: [{
                data: chartData.map(d => d.amount),
                backgroundColor: [
                    COLORS.income,
                    COLORS.expense,
                    COLORS.transfer
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
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            family: "'Inter', sans-serif"
                        },
                        usePointStyle: true,
                        pointStyle: 'circle'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: {
                        label: function(context) {
                            const item = chartData[context.dataIndex];
                            return [
                                `Số tiền: ${formatCurrency(item.amount)}`,
                                `Số giao dịch: ${item.count}`,
                                `Tỷ lệ: ${context.parsed.toFixed(1)}%`
                            ];
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
    
    // Update summary
    updateChartSummary('transactionTypesSummary', chartData, total);
}

// Render Income vs Expense Chart
function renderIncomeExpenseChart(data) {
    const ctx = document.getElementById('incomeExpenseChart');
    if (!ctx) return;
    
    // Destroy existing chart
    if (charts.incomeExpense) {
        charts.incomeExpense.destroy();
    }
    
    const chartData = data || [];
    
    charts.incomeExpense = new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartData.map(d => d.month),
            datasets: [
                {
                    label: 'Thu nhập',
                    data: chartData.map(d => d.income),
                    borderColor: COLORS.income,
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    tension: 0.4,
                    fill: true,
                    borderWidth: 3,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointBackgroundColor: COLORS.income,
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2
                },
                {
                    label: 'Chi tiêu',
                    data: chartData.map(d => d.expense),
                    borderColor: COLORS.expense,
                    backgroundColor: 'rgba(239, 68, 68, 0.1)',
                    tension: 0.4,
                    fill: true,
                    borderWidth: 3,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointBackgroundColor: COLORS.expense,
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: ${formatCurrency(context.parsed.y)}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    },
                    ticks: {
                        callback: function(value) {
                            return formatCurrencyShort(value);
                        },
                        font: {
                            size: 12
                        }
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            size: 12
                        }
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeInOutQuart'
            }
        }
    });
}

// Render Category Spending Pie Chart
function renderCategoryChart(data) {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;
    
    // Destroy existing chart
    if (charts.categories) {
        charts.categories.destroy();
    }
    
    const chartData = data || [];
    const total = chartData.reduce((sum, item) => sum + item.amount, 0);
    
    // Sort by amount and take top 8
    const topCategories = chartData
        .sort((a, b) => b.amount - a.amount)
        .slice(0, 8);
    
    charts.categories = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: topCategories.map(d => {
                const percentage = total > 0 ? ((d.amount / total) * 100).toFixed(1) : 0;
                return `${d.categoryName} (${percentage}%)`;
            }),
            datasets: [{
                data: topCategories.map(d => d.amount),
                backgroundColor: COLORS.categories.slice(0, topCategories.length),
                borderWidth: 3,
                borderColor: '#ffffff',
                hoverOffset: 15,
                hoverBorderWidth: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            family: "'Inter', sans-serif"
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        generateLabels: function(chart) {
                            const data = chart.data;
                            if (data.labels.length && data.datasets.length) {
                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    return {
                                        text: label,
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
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: {
                        label: function(context) {
                            const item = topCategories[context.dataIndex];
                            const percentage = total > 0 ? ((item.amount / total) * 100).toFixed(1) : 0;
                            return [
                                `Số tiền: ${formatCurrency(item.amount)}`,
                                `Tỷ lệ: ${percentage}%`,
                                `Số giao dịch: ${item.count || 0}`
                            ];
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
    
    // Update summary
    updateChartSummary('categorySummary', topCategories, total);
}

function renderRecentTransactions(transactions) {
    const container = document.getElementById('recentTransactions');
    if (!container) return;
    
    if (!transactions || transactions.length === 0) {
        container.innerHTML = '<p class="text-center text-muted">Chưa có giao dịch nào</p>';
        return;
    }
    
    container.innerHTML = transactions.map(t => `
        <div class="transaction-item" onclick="viewTransaction(${t.transactionId})">
            <div class="transaction-icon" style="background: ${t.categoryColor}20; color: ${t.categoryColor}">
                <i class="${t.categoryIcon}"></i>
            </div>
            <div class="transaction-info">
                <p class="transaction-category">${t.categoryName}</p>
                <p class="transaction-note">${t.note || 'Không có ghi chú'}</p>
            </div>
            <div class="transaction-amount ${t.transactionType.toLowerCase()}">
                ${t.transactionType === 'Income' ? '+' : '-'}${formatCurrency(t.amount)}
            </div>
        </div>
    `).join('');
}

function renderAiSuggestions(suggestions) {
    const container = document.getElementById('aiSuggestions');
    if (!container) return;
    
    if (!suggestions || suggestions.length === 0) {
        container.innerHTML = '<p class="text-center text-muted">Chưa có gợi ý nào</p>';
        return;
    }
    
    container.innerHTML = suggestions.slice(0, 3).map(s => `
        <div class="suggestion-item">
            <strong>${s.title}</strong><br>
            ${s.suggestion}
        </div>
    `).join('');
}

function renderFinancialAlerts(alerts) {
    const container = document.getElementById('financialAlerts');
    if (!container) return;
    
    if (!alerts || alerts.length === 0) {
        container.innerHTML = '<p class="text-center text-muted">Không có cảnh báo</p>';
        return;
    }
    
    container.innerHTML = alerts.slice(0, 3).map(a => `
        <div class="alert-item ${a.severity}">
            <strong>${a.title}</strong><br>
            ${a.message}
        </div>
    `).join('');
}

function renderAccounts(accounts) {
    const container = document.getElementById('accountsGrid');
    if (!container) return;
    
    if (!accounts || accounts.length === 0) {
        container.innerHTML = '<p class="text-center text-muted">Chưa có tài khoản nào</p>';
        return;
    }
    
    container.innerHTML = accounts.map(a => `
        <div class="account-card" style="--account-color-1: ${a.color}; --account-color-2: ${adjustColor(a.color, -20)};" onclick="viewAccount(${a.accountId})">
            <p class="account-name">
                <i class="${a.icon}"></i> ${a.accountName}
            </p>
            <p class="account-balance">${formatCurrency(a.currentBalance)}</p>
        </div>
    `).join('');
}

// Helper Functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatCurrencyShort(amount) {
    if (amount >= 1000000000) {
        return (amount / 1000000000).toFixed(1) + 'B';
    } else if (amount >= 1000000) {
        return (amount / 1000000).toFixed(1) + 'M';
    } else if (amount >= 1000) {
        return (amount / 1000).toFixed(1) + 'K';
    }
    return amount.toString();
}

function animateValue(id, start, end, duration) {
    const element = document.getElementById(id);
    if (!element) return;
    
    const range = end - start;
    const increment = range / (duration / 16);
    let current = start;
    
    const timer = setInterval(() => {
        current += increment;
        if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
            current = end;
            clearInterval(timer);
        }
        element.textContent = formatCurrency(Math.round(current));
    }, 16);
}

function updateChartSummary(elementId, data, total) {
    const element = document.getElementById(elementId);
    if (!element || !data || data.length === 0) return;
    
    const topItem = data[0];
    const percentage = total > 0 ? ((topItem.amount / total) * 100).toFixed(1) : 0;
    
    element.innerHTML = `
        <div class="summary-item">
            <span class="summary-label">Tổng:</span>
            <span class="summary-value">${formatCurrency(total)}</span>
        </div>
        <div class="summary-item">
            <span class="summary-label">Cao nhất:</span>
            <span class="summary-value">${topItem.categoryName || topItem.type} (${percentage}%)</span>
        </div>
    `;
}

function getTransactionTypeLabel(type) {
    const labels = {
        'Income': 'Thu nhập',
        'Expense': 'Chi tiêu',
        'Transfer': 'Chuyển khoản'
    };
    return labels[type] || type;
}

function refreshChart(chartType) {
    loadDashboardData();
}

function initializeTooltips() {
    // Initialize any tooltips or popovers
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function adjustColor(color, amount) {
    return '#' + color.replace(/^#/, '').replace(/../g, color => ('0'+Math.min(255, Math.max(0, parseInt(color, 16) + amount)).toString(16)).substr(-2));
}

function viewTransaction(id) {
    window.location.href = `/Transactions/Details?id=${id}`;
}

function viewAccount(id) {
    window.location.href = `/Wallets/Details?id=${id}`;
}

function showError(message) {
    // Implement error notification
    console.error(message);
    // You can add a toast notification here
}
