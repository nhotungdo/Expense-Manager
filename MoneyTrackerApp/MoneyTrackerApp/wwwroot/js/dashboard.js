// Modern Dashboard JavaScript - Redesigned
let dashboardData = null;
let currentPeriod = 'week';
let charts = {
    cashflow: null,
    category: null,
    monthlyComparison: null,
    accountDistribution: null,
    sparklines: {}
};

// Modern color palette
const COLORS = {
    income: '#10b981',
    expense: '#ef4444',
    balance: '#3b82f6',
    savings: '#8b5cf6',
    transfer: '#06b6d4',
    categories: [
        '#ef4444', '#f59e0b', '#10b981', '#3b82f6', 
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
        '#06b6d4', '#84cc16', '#a855f7', '#f43f5e'
    ],
    gradient: {
        income: ['rgba(16, 185, 129, 0.8)', 'rgba(16, 185, 129, 0.1)'],
        expense: ['rgba(239, 68, 68, 0.8)', 'rgba(239, 68, 68, 0.1)'],
        balance: ['rgba(59, 130, 246, 0.8)', 'rgba(59, 130, 246, 0.1)']
    }
};

document.addEventListener('DOMContentLoaded', function() {
    initializeDashboard();
});

function initializeDashboard() {
    // Set current date
    updateCurrentDate();
    
    // Setup period selector
    setupPeriodSelector();
    
    // Load dashboard data
    loadDashboardData(currentPeriod);
    
    // Initialize tooltips
    initializeTooltips();
}

function updateCurrentDate() {
    const dateElement = document.getElementById('currentDate');
    if (dateElement) {
        const now = new Date();
        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        dateElement.textContent = now.toLocaleDateString('en-US', options);
    }
}

function setupPeriodSelector() {
    const periodButtons = document.querySelectorAll('.period-btn');
    periodButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            periodButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentPeriod = this.dataset.period;
            loadDashboardData(currentPeriod);
        });
    });
}

function refreshDashboard() {
    const btn = document.querySelector('.btn-refresh');
    if (btn) {
        btn.style.transform = 'rotate(360deg)';
        setTimeout(() => {
            btn.style.transform = '';
        }, 600);
    }
    loadDashboardData(currentPeriod);
}

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
    
    // Update KPI cards with animations
    updateKPICards();
    
    // Render sparklines
    renderSparklines();
    
    // Render main charts
    renderCashflowChart();
    renderCategoryChart(dashboardData.categorySpending);
    renderMonthlyComparisonChart();
    renderAccountDistributionChart();
    
    // Render transactions
    renderRecentTransactions(dashboardData.recentTransactions);
    
    // Render category list
    renderCategoryList(dashboardData.categorySpending);
    
    // Load and update AI insights separately
    loadAIInsights();
}

function updateKPICards() {
    // Animate values
    animateValue('totalBalance', 0, dashboardData.totalBalance, 1200);
    animateValue('totalIncome', 0, dashboardData.totalIncome, 1000);
    animateValue('totalExpense', 0, dashboardData.totalExpense, 1000);
    
    // Calculate and update savings rate
    const savingsRate = dashboardData.totalIncome > 0 
        ? ((dashboardData.totalIncome - dashboardData.totalExpense) / dashboardData.totalIncome * 100)
        : 0;
    
    const savingsRateElement = document.getElementById('savingsRate');
    if (savingsRateElement) {
        animateValue('savingsRate', 0, savingsRate, 1000, '%');
    }
    
    // Update progress ring
    updateProgressRing(savingsRate);
    
    // Update trends
    updateTrend('balanceTrend', dashboardData.balanceTrend || 12.5);
    updateTrend('incomeTrend', dashboardData.incomeTrend || 8.2);
    updateTrend('expenseTrend', dashboardData.expenseTrend || -3.1);
}

function updateProgressRing(percentage) {
    const circle = document.getElementById('savingsProgress');
    const text = document.getElementById('savingsPercentage');
    
    if (circle && text) {
        const circumference = 2 * Math.PI * 35;
        const offset = circumference - (percentage / 100) * circumference;
        
        setTimeout(() => {
            circle.style.strokeDashoffset = offset;
            text.textContent = Math.round(percentage) + '%';
        }, 300);
    }
}

function updateTrend(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        const isPositive = value >= 0;
        const parent = element.closest('.kpi-trend');
        
        if (parent) {
            parent.classList.remove('positive', 'negative');
            parent.classList.add(isPositive ? 'positive' : 'negative');
        }
        
        const icon = parent?.querySelector('i');
        if (icon) {
            icon.className = isPositive ? 'fas fa-arrow-up' : 'fas fa-arrow-down';
        }
        
        element.textContent = (isPositive ? '+' : '') + value.toFixed(1) + '%';
    }
}

function renderSparklines() {
    // Balance sparkline
    renderSparkline('balanceSparkline', dashboardData.balanceHistory || [], COLORS.balance);
    
    // Income sparkline
    renderSparkline('incomeSparkline', dashboardData.incomeHistory || [], COLORS.income);
    
    // Expense sparkline
    renderSparkline('expenseSparkline', dashboardData.expenseHistory || [], COLORS.expense);
}

function renderSparkline(canvasId, data, color) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    // Destroy existing chart
    if (charts.sparklines[canvasId]) {
        charts.sparklines[canvasId].destroy();
    }
    
    // Generate sample data if empty
    if (data.length === 0) {
        data = Array.from({length: 12}, () => Math.random() * 1000 + 500);
    }
    
    charts.sparklines[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map((_, i) => i),
            datasets: [{
                data: data,
                borderColor: color,
                backgroundColor: color + '20',
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: { enabled: false }
            },
            scales: {
                x: { display: false },
                y: { display: false }
            },
            interaction: { mode: null }
        }
    });
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

// Render Cashflow Chart (Main Chart)
function renderCashflowChart() {
    const ctx = document.getElementById('cashflowChart');
    if (!ctx) return;
    
    // Destroy existing chart
    if (charts.cashflow) {
        charts.cashflow.destroy();
    }
    
    const chartData = dashboardData.monthlyTrends || generateSampleData();
    
    // Calculate net flow
    const netData = chartData.map(d => d.income - d.expense);
    
    charts.cashflow = new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartData.map(d => d.month),
            datasets: [
                {
                    label: 'Income',
                    data: chartData.map(d => d.income),
                    borderColor: COLORS.income,
                    backgroundColor: createGradient(ctx, COLORS.gradient.income),
                    tension: 0.4,
                    fill: true,
                    borderWidth: 3,
                    pointRadius: 6,
                    pointHoverRadius: 8,
                    pointBackgroundColor: COLORS.income,
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 3,
                    pointHoverBorderWidth: 4
                },
                {
                    label: 'Expense',
                    data: chartData.map(d => d.expense),
                    borderColor: COLORS.expense,
                    backgroundColor: createGradient(ctx, COLORS.gradient.expense),
                    tension: 0.4,
                    fill: true,
                    borderWidth: 3,
                    pointRadius: 6,
                    pointHoverRadius: 8,
                    pointBackgroundColor: COLORS.expense,
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 3,
                    pointHoverBorderWidth: 4
                },
                {
                    label: 'Net',
                    data: netData,
                    borderColor: COLORS.balance,
                    backgroundColor: 'transparent',
                    tension: 0.4,
                    fill: false,
                    borderWidth: 2,
                    borderDash: [5, 5],
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: COLORS.balance,
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
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(15, 23, 42, 0.95)',
                    padding: 16,
                    titleFont: { size: 15, weight: 'bold' },
                    bodyFont: { size: 14 },
                    bodySpacing: 8,
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1,
                    cornerRadius: 12,
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
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        callback: value => formatCurrencyShort(value),
                        font: { size: 12, weight: '600' },
                        color: '#64748b',
                        padding: 12
                    }
                },
                x: {
                    grid: { display: false, drawBorder: false },
                    ticks: {
                        font: { size: 12, weight: '600' },
                        color: '#64748b',
                        padding: 8
                    }
                }
            },
            animation: {
                duration: 1200,
                easing: 'easeInOutQuart'
            }
        }
    });
    
    // Update footer stats
    updateChartFooterStats(chartData);
}

function createGradient(ctx, colors) {
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, colors[0]);
    gradient.addColorStop(1, colors[1]);
    return gradient;
}

function updateChartFooterStats(data) {
    const avgIncome = data.reduce((sum, d) => sum + d.income, 0) / data.length;
    const avgExpense = data.reduce((sum, d) => sum + d.expense, 0) / data.length;
    const netFlow = avgIncome - avgExpense;
    
    animateValue('avgIncome', 0, avgIncome, 800, '', formatCurrency);
    animateValue('avgExpense', 0, avgExpense, 800, '', formatCurrency);
    animateValue('netFlow', 0, netFlow, 800, '', formatCurrency);
    
    const netFlowElement = document.getElementById('netFlow');
    if (netFlowElement) {
        netFlowElement.style.color = netFlow >= 0 ? COLORS.income : COLORS.expense;
    }
}

function renderMonthlyComparisonChart() {
    const ctx = document.getElementById('monthlyComparisonChart');
    if (!ctx) return;
    
    if (charts.monthlyComparison) {
        charts.monthlyComparison.destroy();
    }
    
    const data = dashboardData.monthlyComparison || generateMonthlyComparison();
    
    charts.monthlyComparison = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(d => d.month),
            datasets: [{
                label: 'This Year',
                data: data.map(d => d.thisYear),
                backgroundColor: COLORS.balance + 'CC',
                borderRadius: 8,
                borderSkipped: false
            }, {
                label: 'Last Year',
                data: data.map(d => d.lastYear),
                backgroundColor: COLORS.balance + '40',
                borderRadius: 8,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 15,
                        font: { size: 12, weight: '600' }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(15, 23, 42, 0.95)',
                    padding: 12,
                    cornerRadius: 10,
                    callbacks: {
                        label: context => `${context.dataset.label}: ${formatCurrency(context.parsed.y)}`
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0, 0, 0, 0.05)' },
                    ticks: {
                        callback: value => formatCurrencyShort(value),
                        font: { size: 11 }
                    }
                },
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 11 } }
                }
            }
        }
    });
}

function renderAccountDistributionChart() {
    const ctx = document.getElementById('accountDistributionChart');
    if (!ctx) return;
    
    if (charts.accountDistribution) {
        charts.accountDistribution.destroy();
    }
    
    const data = dashboardData.accounts || generateAccountData();
    
    charts.accountDistribution = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.accountName),
            datasets: [{
                data: data.map(d => d.currentBalance),
                backgroundColor: COLORS.categories.slice(0, data.length),
                borderWidth: 4,
                borderColor: '#ffffff',
                hoverOffset: 15,
                hoverBorderWidth: 5
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 12,
                        font: { size: 12, weight: '600' },
                        generateLabels: function(chart) {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: `${label} (${formatCurrencyShort(data.datasets[0].data[i])})`,
                                fillStyle: data.datasets[0].backgroundColor[i],
                                hidden: false,
                                index: i
                            }));
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(15, 23, 42, 0.95)',
                    padding: 12,
                    cornerRadius: 10,
                    callbacks: {
                        label: context => `${context.label}: ${formatCurrency(context.parsed)}`
                    }
                }
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

function animateValue(id, start, end, duration, suffix = '', formatter = null) {
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
        
        const value = Math.round(current * 100) / 100;
        if (formatter) {
            element.textContent = formatter(value);
        } else if (suffix === '%') {
            element.textContent = value.toFixed(1) + suffix;
        } else {
            element.textContent = formatCurrency(value) + suffix;
        }
    }, 16);
}

function renderCategoryList(categories) {
    const container = document.getElementById('categoryList');
    if (!container || !categories || categories.length === 0) return;
    
    const topCategories = categories.slice(0, 5);
    
    container.innerHTML = topCategories.map(cat => `
        <div class="category-item">
            <div class="category-icon" style="background: ${cat.color || COLORS.categories[0]}20; color: ${cat.color || COLORS.categories[0]}">
                <i class="${cat.icon || 'fas fa-tag'}"></i>
            </div>
            <div class="category-info">
                <p class="category-name">${cat.categoryName}</p>
                <p class="category-count">${cat.count || 0} transactions</p>
            </div>
            <div class="category-amount">${formatCurrency(cat.amount)}</div>
        </div>
    `).join('');
}

async function loadAIInsights() {
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            console.warn('No access token found, showing default insights');
            showDefaultInsights();
            return;
        }
        
        console.log('Loading AI insights...');
        const response = await fetch('/api/AiAdvisor/suggestions', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        console.log('AI insights response status:', response.status);
        
        if (response.ok) {
            const suggestions = await response.json();
            console.log('AI insights loaded:', suggestions);
            
            if (suggestions && suggestions.length > 0) {
                updateAIInsights(suggestions);
            } else {
                console.log('No suggestions returned, showing defaults');
                showDefaultInsights();
            }
        } else {
            const errorText = await response.text();
            console.warn('Failed to load AI insights:', response.status, errorText);
            showDefaultInsights();
        }
    } catch (error) {
        console.error('Error loading AI insights:', error);
        showDefaultInsights();
    }
}

function showDefaultInsights() {
    const container = document.getElementById('aiInsights');
    if (!container) return;
    
    container.innerHTML = `
        <div class="insight-item">
            <div class="insight-icon success">
                <i class="fas fa-check-circle"></i>
            </div>
            <div class="insight-content">
                <h4>Tiến triển tốt!</h4>
                <p>Bạn đang đi đúng hướng để đạt mục tiêu tiết kiệm vào tháng tới.</p>
            </div>
        </div>
        <div class="insight-item">
            <div class="insight-icon warning">
                <i class="fas fa-exclamation-triangle"></i>
            </div>
            <div class="insight-content">
                <h4>Cảnh báo chi tiêu</h4>
                <p>Chi phí ăn uống cao hơn 15% so với bình thường. Hãy cân nhắc giảm để tiết kiệm 200.000 ₫.</p>
            </div>
        </div>
        <div class="insight-item">
            <div class="insight-icon info">
                <i class="fas fa-lightbulb"></i>
            </div>
            <div class="insight-content">
                <h4>Mẹo thông minh</h4>
                <p>Hóa đơn tiện ích của bạn ổn định. Hãy cân nhắc thiết lập thanh toán tự động để không bỏ lỡ.</p>
            </div>
        </div>
    `;
}

function updateAIInsights(suggestions) {
    if (!suggestions || suggestions.length === 0) {
        showDefaultInsights();
        return;
    }
    
    const container = document.getElementById('aiInsights');
    if (!container) return;
    
    const iconMap = {
        success: 'fa-check-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-lightbulb',
        danger: 'fa-exclamation-circle'
    };
    
    // Map suggestion types from the API
    const typeMap = {
        'success': 'success',
        'warning': 'warning',
        'info': 'info',
        'danger': 'danger',
        'error': 'danger'
    };
    
    // Map titles based on suggestion type
    const titleMap = {
        'success': 'Tiến triển tốt!',
        'warning': 'Cảnh báo chi tiêu',
        'info': 'Mẹo thông minh',
        'danger': 'Cảnh báo quan trọng'
    };
    
    container.innerHTML = suggestions.slice(0, 3).map(s => {
        // Get the type from suggestionType field (string)
        const type = typeMap[s.suggestionType?.toLowerCase()] || 'info';
        const title = titleMap[type] || 'Thông tin tài chính';
        
        return `
            <div class="insight-item">
                <div class="insight-icon ${type}">
                    <i class="fas ${iconMap[type]}"></i>
                </div>
                <div class="insight-content">
                    <h4>${title}</h4>
                    <p>${s.suggestion || s.message || 'Không có thông tin'}</p>
                </div>
            </div>
        `;
    }).join('');
}

function generateSampleData() {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
    return months.map(month => ({
        month,
        income: Math.random() * 5000 + 3000,
        expense: Math.random() * 4000 + 2000
    }));
}

function generateMonthlyComparison() {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
    return months.map(month => ({
        month,
        thisYear: Math.random() * 5000 + 3000,
        lastYear: Math.random() * 4500 + 2500
    }));
}

function generateAccountData() {
    return [
        { accountName: 'Checking', currentBalance: 5000, color: COLORS.categories[0] },
        { accountName: 'Savings', currentBalance: 15000, color: COLORS.categories[1] },
        { accountName: 'Credit Card', currentBalance: 2000, color: COLORS.categories[2] },
        { accountName: 'Investment', currentBalance: 25000, color: COLORS.categories[3] }
    ];
}

function toggleChartType(chartName) {
    console.log('Toggle chart type:', chartName);
    // Implement chart type toggle functionality
}

function exportChart(chartName) {
    console.log('Export chart:', chartName);
    // Implement chart export functionality
}

function viewAllCategories() {
    window.location.href = '/Categories';
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

async function generateNewAIInsights() {
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            console.warn('No access token found');
            alert('Vui lòng đăng nhập để sử dụng tính năng này');
            return;
        }
        
        // Show loading state
        const btn = event.target.closest('button');
        const originalHTML = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        btn.disabled = true;
        
        console.log('Generating new AI insights...');
        const response = await fetch('/api/AiAdvisor/generate', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        console.log('Generate AI insights response status:', response.status);
        
        if (response.ok) {
            console.log('AI insights generated successfully');
            // Reload the insights
            await loadAIInsights();
            
            // Show success message
            showSuccessMessage('Đã tạo gợi ý mới thành công!');
        } else {
            const errorText = await response.text();
            console.error('Failed to generate AI insights:', response.status, errorText);
            alert('Không thể tạo gợi ý mới. Vui lòng thử lại sau.');
        }
        
        // Restore button
        btn.innerHTML = originalHTML;
        btn.disabled = false;
    } catch (error) {
        console.error('Error generating AI insights:', error);
        alert('Đã xảy ra lỗi. Vui lòng thử lại sau.');
        
        // Restore button
        const btn = event.target.closest('button');
        btn.innerHTML = '<i class="fas fa-sync-alt"></i>';
        btn.disabled = false;
    }
}

function showSuccessMessage(message) {
    // Create a simple toast notification
    const toast = document.createElement('div');
    toast.className = 'toast-notification success';
    toast.innerHTML = `
        <i class="fas fa-check-circle"></i>
        <span>${message}</span>
    `;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #10b981;
        color: white;
        padding: 16px 24px;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 9999;
        display: flex;
        align-items: center;
        gap: 12px;
        font-weight: 500;
        animation: slideIn 0.3s ease;
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
