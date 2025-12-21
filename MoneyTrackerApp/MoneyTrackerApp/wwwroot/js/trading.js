/**
 * Trading Interface Logic
 * Features: Chart.js integration, Data Fetching, Filtering, Interactive Details, AI Analysis
 */

document.addEventListener('DOMContentLoaded', () => {
    initTradingInterface();
});

let myChart = null;
let currentTransactions = [];
let chartType = 'doughnut'; // or 'pie'
let currentFilter = 'day';
let currentType = 'expense'; // 'expense' or 'income'
let currentLang = 'vi';
let searchTerm = '';
let selectedCategory = '';

const API_BASE = '/api/Transactions';

const translations = {
    vi: {
        tradingTitle: 'Phân bổ Giao dịch',
        aiTitle: 'Trợ lý Phân tích AI',
        financialStructure: 'Cơ cấu Tài chính',
        total: 'Tổng',
        detailsTitle: 'Chi tiết Giao dịch',
        expense: 'Chi tiêu',
        income: 'Thu nhập',
        totalLabel: 'Tổng cộng:',
        historyTitle: 'Lịch sử Giao dịch',
        colDate: 'Ngày',
        colCategory: 'Danh mục',
        colNote: 'Nội dung',
        colAmount: 'Số tiền',
        colStatus: 'Trạng thái AI',
        warning: 'Bất thường',
        normal: 'Bình thường',
        trendUp: 'Xu hướng tăng',
        trendDown: 'Xu hướng giảm',
        prediction: 'Dự báo: Chi tiêu có thể tăng vào cuối tuần.'
    },
    en: {
        tradingTitle: 'Transaction Allocation',
        aiTitle: 'AI Analysis Assistant',
        financialStructure: 'Financial Structure',
        total: 'Total',
        detailsTitle: 'Transaction Details',
        expense: 'Expense',
        income: 'Income',
        totalLabel: 'Total:',
        historyTitle: 'Transaction History',
        colDate: 'Date',
        colCategory: 'Category',
        colNote: 'Note',
        colAmount: 'Amount',
        colStatus: 'AI Status',
        warning: 'Unusual',
        normal: 'Normal',
        trendUp: 'Trending Up',
        trendDown: 'Trending Down',
        prediction: 'Prediction: Spending may increase this weekend.'
    }
};

async function initTradingInterface() {
    setupFilters();
    applyLanguage(currentLang);
    await loadCategories();
    await loadData('day');
}

function setupFilters() {
    window.updateFilter = updateFilter;
    window.toggleChartType = toggleChartType;
    window.resetZoom = resetZoom;
    window.setType = setType;
    window.changeLanguage = toggleLanguage; // Added hook for potential language toggle

    const searchInput = document.getElementById('txnSearch');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            searchTerm = e.target.value.toLowerCase();
            processAndRenderData(currentTransactions);
        });
    }

    const catInfo = document.getElementById('categoryFilter');
    if (catInfo) {
        catInfo.addEventListener('change', (e) => {
            selectedCategory = e.target.value;
            processAndRenderData(currentTransactions);
        });
    }
}

async function loadCategories() {
    try {
        const response = await fetch('/api/Categories');
        if (!response.ok) return;
        const categories = await response.json();

        const select = document.getElementById('categoryFilter');
        // Filter out categories based on type if needed, but for now show all
        categories.forEach(c => {
            const option = document.createElement('option');
            option.value = c.name; // Filter by name for simplicity
            option.textContent = c.name;
            select.appendChild(option);
        });
    } catch (e) {
        console.error('Failed to load categories', e);
    }
}

function toggleLanguage() {
    currentLang = currentLang === 'vi' ? 'en' : 'vi';
    applyLanguage(currentLang);
    processAndRenderData(currentTransactions); // Re-render to update dynamic texts
}

function applyLanguage(lang) {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (translations[lang][key]) {
            el.innerText = translations[lang][key];
        }
    });
}

function updateFilter(filter) {
    currentFilter = filter;
    document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.filter-btn[data-filter="${filter}"]`).classList.add('active');
    loadData(filter);
}

function setType(type) {
    currentType = type;
    document.querySelectorAll('.type-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.type-btn[data-type="${type}"]`).classList.add('active');
    processAndRenderData(currentTransactions);
}

function toggleChartType() {
    chartType = chartType === 'doughnut' ? 'polarArea' : 'doughnut';
    if (myChart) {
        myChart.config.type = chartType;
        myChart.update();
    }
}

function resetZoom() {
    if (myChart) {
        myChart.resetZoom();
    }
}



async function loadData(range) {
    try {
        const params = getDateRangeParams(range);
        const url = `${API_BASE}?PageSize=1000&StartDate=${params.start}&EndDate=${params.end}`;

        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch data');

        const data = await response.json();
        currentTransactions = data;
        processAndRenderData(data);

    } catch (error) {
        console.error('Error loading data:', error);
        document.getElementById('detailsList').innerHTML = '<div class="text-center p-4">Không thể tải dữ liệu</div>';
    }
}

function getDateRangeParams(range) {
    const end = new Date();
    const start = new Date();

    switch (range) {
        case 'day':
            start.setHours(0, 0, 0, 0);
            break;
        case 'week':
            const day = start.getDay();
            const diff = start.getDate() - day + (day === 0 ? -6 : 1);
            start.setDate(diff);
            start.setHours(0, 0, 0, 0);
            break;
        case 'month':
            start.setDate(1);
            start.setHours(0, 0, 0, 0);
            break;
        case 'year':
            start.setMonth(0, 1);
            start.setHours(0, 0, 0, 0);
            break;
    }
    return { start: start.toISOString(), end: end.toISOString() };
}

function processAndRenderData(transactions) {
    const targetTypeInt = currentType === 'income' ? 1 : 2;
    const filtered = transactions.filter(t => {
        const matchType = t.transactionType === targetTypeInt;
        const matchSearch = searchTerm ? (t.note || '').toLowerCase().includes(searchTerm) : true;
        const matchCat = selectedCategory ? (t.categoryName === selectedCategory) : true;
        return matchType && matchSearch && matchCat;
    });

    // 1. Group Data
    const categoryMap = new Map();
    let total = 0;

    filtered.forEach(t => {
        const catName = t.categoryName || 'Khác';
        const amount = t.amount || 0;
        const color = t.categoryColor || '#CBD5E1';
        const icon = t.categoryIcon || 'fa-question';

        if (!categoryMap.has(catName)) {
            categoryMap.set(catName, { amount: 0, color: color, icon: icon, count: 0 });
        }

        const cat = categoryMap.get(catName);
        cat.amount += amount;
        cat.count += 1;
        total += amount;
    });

    const sortedCategories = Array.from(categoryMap.entries()).sort((a, b) => b[1].amount - a[1].amount);

    // 2. Prepare Chart
    const labels = [];
    const dataValues = [];
    const colors = [];

    sortedCategories.forEach(([name, data]) => {
        labels.push(name);
        dataValues.push(data.amount);
        colors.push(data.color);
    });

    renderChart(labels, dataValues, colors);

    // 3. Render Details List
    renderDetailsList(sortedCategories, total);

    // 4. Update Totals
    document.getElementById('totalAmount').innerText = formatCurrency(total);
    document.getElementById('totalCenterValue').innerText = formatCurrencyShort(total);

    // 5. Render History Table
    renderHistoryTable(filtered);

    // 6. AI Analysis
    analyzeDataAI(filtered, total);
}

function renderChart(labels, data, colors) {
    const ctx = document.getElementById('allocationChart').getContext('2d');
    if (myChart) myChart.destroy();

    myChart = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 0,
                hoverOffset: 20
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '70%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(255, 255, 255, 0.9)',
                    titleColor: '#1F2937',
                    bodyColor: '#4B5563',
                    titleFont: { size: 14, weight: 'bold' },
                    padding: 12,
                    borderColor: 'rgba(0,0,0,0.1)',
                    borderWidth: 1,
                    callbacks: {
                        label: function (context) {
                            return (context.label || '') + ': ' + formatCurrency(context.raw);
                        }
                    }
                }
            },
            animation: {
                animateScale: true,
                animateRotate: true,
                duration: 1000,
                easing: 'easeOutQuart'
            }
        }
    });
}

function renderDetailsList(categories, total) {
    const container = document.getElementById('detailsList');
    container.innerHTML = '';

    categories.forEach(([name, data]) => {
        const percent = total > 0 ? ((data.amount / total) * 100).toFixed(1) : 0;
        const item = document.createElement('div');
        item.className = 'detail-item animate-fade-in';
        item.innerHTML = `
            <div class="detail-info">
                <div class="detail-icon" style="background-color: ${data.color}">
                    <i class="${data.icon}"></i>
                </div>
                <div class="detail-text">
                    <h4>${name}</h4>
                    <p>${data.count} giao dịch</p>
                </div>
            </div>
            <div class="detail-amount">
                <span class="amount-val">${formatCurrency(data.amount)}</span>
                <span class="percentage" style="color: ${data.color}">${percent}%</span>
            </div>
        `;
        container.appendChild(item);
    });
}

function renderHistoryTable(transactions) {
    const tbody = document.getElementById('historyTableBody');
    tbody.innerHTML = '';

    // Sort by date desc
    const sorted = [...transactions].sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));

    sorted.forEach(t => {
        const tr = document.createElement('tr');
        tr.className = 'history-row animate-fade-in';

        const date = new Date(t.transactionDate).toLocaleDateString('vi-VN');
        const anomaly = detectAnomalies(t);
        const statusClass = anomaly.isWarning ? 'status-warning' : 'status-normal';
        const statusText = anomaly.isWarning ? translations[currentLang].warning : translations[currentLang].normal;

        tr.innerHTML = `
            <td>${date}</td>
            <td><span style="font-weight: 500;">${t.categoryName}</span></td>
            <td>${t.note || '-'}</td>
            <td style="font-weight: 600; color: ${t.transactionType === 1 ? '#10B981' : '#EF4444'}">
                ${t.transactionType === 1 ? '+' : '-'}${formatCurrency(t.amount)}
            </td>
            <td><span class="status-badge ${statusClass}">${statusText}</span></td>
        `;
        tbody.appendChild(tr);
    });
}

function detectAnomalies(transaction) {
    // Advanced AI Logic Mock
    // Flag if amount > 2M for expense, or if it's a specific 'Entertainment' category > 500k
    // This is just a heuristic mock
    if (transaction.transactionType === 2 && transaction.amount > 2000000) {
        return { isWarning: true };
    }
    return { isWarning: false };
}

function analyzeDataAI(transactions, total) {
    const container = document.getElementById('aiContent');
    if (transactions.length === 0) {
        container.innerHTML = '<p class="text-muted">Chưa đủ dữ liệu để phân tích.</p>';
        return;
    }

    // Mock AI Insights
    const topCategory = transactions.sort((a, b) => b.amount - a.amount)[0]?.categoryName || 'Unknown';
    const randomTrend = Math.random() > 0.5 ? 12.5 : -5.2;
    const trendText = randomTrend > 0 ? translations[currentLang].trendUp : translations[currentLang].trendDown;
    const trendColor = randomTrend > 0 ? '#EF4444' : '#10B981'; // If spending up -> bad (red), down -> good (green) in expense context

    container.innerHTML = `
        <div class="ai-insight-item">
            <h4>📈 ${translations[currentLang].aiTitle}</h4>
            <p>${translations[currentLang].prediction}</p>
        </div>
        <div class="ai-insight-item">
            <h4>🔍 Thông tin chi tiết</h4>
            <p>Danh mục <strong>${topCategory}</strong> đang chiếm tỷ trọng lớn nhất.</p>
        </div>
        <div class="ai-insight-item" style="border-left-color: ${trendColor}">
            <h4>${trendText} (${Math.abs(randomTrend)}%)</h4>
            <p>So với trung bình tháng trước.</p>
        </div>
    `;
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function formatCurrencyShort(amount) {
    if (amount >= 1000000000) return (amount / 1000000000).toFixed(1) + ' tỷ';
    if (amount >= 1000000) return (amount / 1000000).toFixed(1) + ' tr';
    if (amount >= 1000) return (amount / 1000).toFixed(0) + ' k';
    return amount.toString();
}
