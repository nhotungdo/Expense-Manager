/**
 * Trading & AI Analytics JavaScript
 * Xử lý logic giao diện, biểu đồ và tích hợp API (Tiếng Việt)
 */

document.addEventListener('DOMContentLoaded', () => {
    initializeTradingPage();
});

// State
let currentPeriod = 'week';
let comparisonMode = ''; // '', 'previous', 'lastMonth', 'lastYear'
let chartInstance = null;
let currentData = {
    dailyAverage: 0,
    totalExpense: 0,
    totalIncome: 0,
    transactions: []
};

// Initialization
function initializeTradingPage() {
    setupEventListeners();
    loadDashboardData();
    renderEmptyChart();
}

function setupEventListeners() {
    // Period Selectors
    const periodInputs = document.querySelectorAll('input[name="period"]');
    periodInputs.forEach(input => {
        input.addEventListener('change', (e) => {
            currentPeriod = e.target.value;
            loadDashboardData();
        });
    });
}

// Data Loading
async function loadDashboardData() {
    showLoadingState(true);
    try {
        const { startStr, endStr } = getDateRange(currentPeriod, 0);

        // Fetch Current Data
        const [insights, predictions, anomalies, transactions] = await Promise.all([
            fetchSafe(`/api/Analysis/insights?period=${currentPeriod}`),
            fetchSafe(`/api/Analysis/predictions?period=${currentPeriod}`),
            fetchSafe(`/api/Analysis/anomalies?period=${currentPeriod}`),
            fetchTransactions(startStr, endStr)
        ]);

        // Check Comparison
        let comparisonData = null;
        if (comparisonMode) {
            const compDates = getComparisonDateRange(currentPeriod, comparisonMode);
            // Fetch basic transaction stats for comparison manual calc if Analysis API doesn't support 'custom dates' easily
            // For now, we'll try to use the Analysis endpoints if they support offsets, or fallback to transactions
            // Assuming Analysis API might not support custom dates yet, we'll just fetch transactions for manual comparison on Chart
            const compTransactions = await fetchTransactions(compDates.startStr, compDates.endStr);
            comparisonData = {
                transactions: compTransactions,
                ...calculateStats(compTransactions)
            };
        }

        // Process Data
        updateStats(insights, comparisonData);
        renderPredictionChart(predictions, transactions, comparisonData);
        renderRecommendations(insights, anomalies);
        renderTransactionTable(transactions);

        currentData.dailyAverage = insights?.dailyAverage || 0;

        // AI Text Update
        updateAiInsightText(insights);

    } catch (error) {
        console.error('Lỗi tải dữ liệu:', error);
    } finally {
        showLoadingState(false);
    }
}

async function fetchSafe(url) {
    try {
        const response = await fetch(url, { headers: { 'Accept': 'application/json' } });
        if (!response.ok) return null;
        return await response.json();
    } catch (e) {
        return null;
    }
}

// Helper: Calculate simple stats from transaction list (for comparison fallback)
function calculateStats(transactions) {
    if (!transactions) return { totalExpense: 0, totalIncome: 0 };
    let expense = 0;
    let income = 0;
    transactions.forEach(t => {
        const amt = t.amount || t.Amount || 0;
        const type = t.transactionType || t.TransactionType;
        if (type === 1) income += amt;
        if (type === 2) expense += amt;
    });
    return { totalExpense: expense, totalIncome: income };
}

/**
 * Get date range strings (YYYY-MM-DD)
 */
function getDateRange(period) {
    const endDate = new Date();
    let startDate = new Date();
    endDate.setHours(23, 59, 59, 999);
    startDate.setHours(0, 0, 0, 0);

    switch (period) {
        case 'today': break;
        case 'week': startDate.setDate(endDate.getDate() - 7); break;
        case 'month': startDate.setDate(endDate.getDate() - 30); break;
        case 'year': startDate.setFullYear(endDate.getFullYear() - 1); break;
        default: startDate.setDate(endDate.getDate() - 7);
    }
    return formatDateRange(startDate, endDate);
}

function getComparisonDateRange(period, mode) {
    const current = getDateRange(period);
    const start = new Date(current.startStr);
    const end = new Date(current.endStr);

    // Shift dates based on mode
    if (mode === 'previous') {
        const diff = end.getTime() - start.getTime();
        end.setTime(start.getTime() - 86400000); // 1 day before start
        start.setTime(end.getTime() - diff);
    } else if (mode === 'lastMonth') {
        start.setMonth(start.getMonth() - 1);
        end.setMonth(end.getMonth() - 1);
    } else if (mode === 'lastYear') {
        start.setFullYear(start.getFullYear() - 1);
        end.setFullYear(end.getFullYear() - 1);
    }

    return formatDateRange(start, end);
}

function formatDateRange(start, end) {
    const format = (d) => {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${y}-${m}-${day}`;
    };
    return { startStr: format(start), endStr: format(end) };
}

// Fetch transactions using standard API
async function fetchTransactions(startDate, endDate) {
    const query = `?StartDate=${startDate}&EndDate=${endDate}&PageNumber=1&PageSize=100`; // Increase limit for charts
    try {
        const res = await fetch(`/api/Transactions${query}`);
        if (res.ok) {
            const data = await res.json();
            if (data.items && Array.isArray(data.items)) return data.items;
            if (Array.isArray(data)) return data;
            return [];
        }
        return [];
    } catch {
        return [];
    }
}

// UI Updates
function updateStats(insights, comparisonData) {
    const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0);

    const expense = insights?.totalExpense || 0;
    const income = insights?.totalIncome || 0;
    const balance = income - expense;

    animateValue('totalExpense', expense, formatCurrency);
    animateValue('totalIncome', income, formatCurrency);
    animateValue('netBalance', balance, formatCurrency);

    // Update Trends
    const trendElem = document.getElementById('expenseTrend');
    const incomeTrendElem = document.getElementById('incomeTrend');

    // Logic: Use comparisonData if available, otherwise fallback to insights trend
    if (comparisonData) {
        const prevExpense = comparisonData.totalExpense || 1; // avoid div 0
        const diff = ((expense - prevExpense) / prevExpense) * 100;
        const icon = diff > 0 ? 'up' : 'down';
        const color = diff > 0 ? 'text-danger' : 'text-success'; // Higher expense is bad usually
        trendElem.innerHTML = `<span class="${color}">
            <i class="fas fa-arrow-${icon}"></i> ${Math.abs(diff).toFixed(1)}% so với kỳ so sánh
        </span>`;
    } else if (insights?.expenseTrend !== undefined) {
        trendElem.innerHTML = `<span class="${insights.expenseTrend > 0 ? 'text-danger' : 'text-success'}">
            <i class="fas fa-arrow-${insights.expenseTrend > 0 ? 'up' : 'down'}"></i> ${Math.abs(insights.expenseTrend)}% so với kỳ trước
           </span>`;
    } else {
        trendElem.innerHTML = '<span class="text-muted">Không có dữ liệu cũ</span>';
    }
}

function updateAiInsightText(insights) {
    const textField = document.getElementById('aiAnalysisText');
    const box = document.getElementById('aiInsightBox');

    if (!insights) {
        textField.textContent = "Dịch vụ AI đang khởi tạo hoặc không khả dụng.";
        box.innerHTML = `<p class="mb-0 text-muted">Vui lòng kiểm tra lại sau.</p>`;
        return;
    }

    textField.textContent = "Hoàn tất phân tích dựa trên hoạt động gần đây của bạn.";

    let message = "Chi tiêu của bạn nằm trong giới hạn bình thường.";
    let icon = "fa-check-circle text-success";

    if (insights.totalExpense > insights.totalIncome && insights.totalIncome > 0) {
        message = "⚠️ Cảnh báo: Chi tiêu đang vượt quá thu nhập trong kỳ này.";
        icon = "fa-exclamation-triangle text-warning";
    } else if (insights.savingsRate > 20) {
        message = "🎉 Tuyệt vời! Bạn đang duy trì tỷ lệ tiết kiệm rất tốt.";
        icon = "fa-star text-warning";
    }

    box.innerHTML = `<p class="mb-0"><i class="fas ${icon} me-2"></i>${message}</p>`;
}

// Charting
function renderPredictionChart(predictionData, currentTransactions, comparisonData) {
    const ctx = document.getElementById('predictionChart').getContext('2d');
    if (chartInstance) chartInstance.destroy();

    // Prepare labels (Dates)
    const labels = [];
    const expenseData = [];
    const comparisonLineData = [];

    // Group current transactions by date
    const grouped = {};
    if (currentTransactions) {
        currentTransactions.forEach(t => {
            if ((t.transactionType || t.TransactionType) === 2) { // Expense only
                const d = (t.transactionDate || t.TransactionDate).split('T')[0];
                grouped[d] = (grouped[d] || 0) + (t.amount || t.Amount);
            }
        });
    }

    // Generate last 7 days or current period days
    const { startStr, endStr } = getDateRange(currentPeriod);
    let curr = new Date(startStr);
    const end = new Date(endStr);

    while (curr <= end) {
        const dateStr = curr.toISOString().split('T')[0];
        labels.push(`${curr.getDate()}/${curr.getMonth() + 1}`);
        expenseData.push(grouped[dateStr] || 0);

        // Mocking prediction extension if data is empty for future
        // For now, let's just plot what we have
        curr.setDate(curr.getDate() + 1);
    }

    // Handle Comparison Line (Mock overlay)
    // If comparisonData exists, we try to map it to the same indices
    if (comparisonData && comparisonData.transactions) {
        // Need to shift comparison dates to match current x-axis
        // This is complex, so we'll just plot totals or a simplified line
        // Simplified: just push the values in order
        const compGrouped = {};
        comparisonData.transactions.forEach(t => {
            if ((t.transactionType || t.TransactionType) === 2) {
                const d = (t.transactionDate || t.TransactionDate).split('T')[0];
                compGrouped[d] = (compGrouped[d] || 0) + (t.amount || t.Amount);
            }
        });
        // Fill array
        let i = 0;
        for (let key in compGrouped) {
            if (i < labels.length) comparisonLineData.push(compGrouped[key]);
            i++;
        }
        // Pad
        while (comparisonLineData.length < labels.length) comparisonLineData.push(0);
    }

    const datasets = [
        {
            label: 'Chi tiêu thực tế',
            data: expenseData,
            borderColor: '#667eea',
            backgroundColor: 'rgba(102, 126, 234, 0.1)',
            borderWidth: 2,
            tension: 0.4,
            fill: true
        }
    ];

    if (predictionData && predictionData.values) {
        datasets.push({
            label: 'Dự báo AI',
            data: predictionData.values, // Ensure this aligns with labels
            borderColor: '#f59e0b',
            borderDash: [5, 5],
            tension: 0.4,
            fill: false
        });
    }

    if (comparisonData) {
        datasets.push({
            label: 'Kỳ so sánh',
            data: comparisonLineData,
            borderColor: '#a0aec0',
            borderWidth: 1,
            tension: 0.4,
            fill: false
        });
    }

    chartInstance = new Chart(ctx, {
        type: 'line',
        data: { labels, datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { intersect: false, mode: 'index' },
            plugins: {
                legend: { position: 'top' },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + ': ' +
                                new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(context.parsed.y);
                        }
                    }
                }
            }
        }
    });
}

function renderEmptyChart() {
    renderPredictionChart(null, [], null);
}

// Recommendations
function renderRecommendations(insights, anomalies) {
    const container = document.getElementById('savingsRecommendations');
    container.innerHTML = '';

    const recs = [];

    // 1. Check Anomalies
    if (anomalies && Array.isArray(anomalies)) {
        anomalies.forEach(a => {
            recs.push({
                type: 'high',
                title: 'Chi tiêu bất thường',
                desc: `${a.description || 'Không xác định'} (${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(a.amount || 0)})`
            });
        });
    }

    // 2. High Expense Ratio
    if (insights && insights.totalExpense > 0 && insights.totalIncome > 0) {
        if (insights.totalExpense > insights.totalIncome * 0.9) {
            recs.push({
                type: 'high',
                title: 'Ngân sách nguy cấp',
                desc: 'Bạn đã sử dụng hơn 90% thu nhập trong kỳ này.'
            });
        }
    }

    // 3. Category Optimization
    if (insights && insights.topCategory) {
        recs.push({
            type: 'medium',
            title: 'Xu hướng chi tiêu',
            desc: `Danh mục '${insights.topCategory}' đang chiếm tỷ trọng lớn nhất. Hãy cân nhắc điều chỉnh.`
        });
    }

    // Fallback
    if (recs.length === 0) {
        recs.push({
            type: 'low',
            title: 'Tài chính lành mạnh',
            desc: 'Thói quen chi tiêu của bạn đang được kiểm soát tốt.'
        });
    }

    recs.forEach(rec => {
        const div = document.createElement('div');
        div.className = `recommendation-item rec-${rec.type}`;
        div.innerHTML = `
            <div class="rec-header">
                <span class="rec-title">${rec.title}</span>
                ${getPriorityIcon(rec.type)}
            </div>
            <div class="rec-desc">${rec.desc}</div>
        `;
        container.appendChild(div);
    });
}

function getPriorityIcon(type) {
    switch (type) {
        case 'high': return '<i class="fas fa-exclamation-circle text-danger"></i>';
        case 'medium': return '<i class="fas fa-info-circle text-warning"></i>';
        case 'low': return '<i class="fas fa-check-circle text-success"></i>';
        default: return '';
    }
}

// Transactions Table
function renderTransactionTable(transactions) {
    const tbody = document.querySelector('#transactionsTable tbody');
    tbody.innerHTML = '';

    if (!transactions || transactions.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-4">Không có giao dịch nào trong khoảng thời gian này.</td></tr>';
        return;
    }

    const formatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

    transactions.slice(0, 10).forEach(t => {
        const row = document.createElement('tr');
        const type = t.transactionType || t.TransactionType;
        const amount = t.amount || t.Amount;
        const date = t.transactionDate || t.TransactionDate;
        const categoryName = t.categoryName || t.CategoryName || t.category?.name || 'Chưa phân loại';
        const description = t.note || t.Note || '-';
        const isExpense = type === 2;

        row.innerHTML = `
            <td>${new Date(date).toLocaleDateString('vi-VN')}</td>
            <td>
                <div class="d-flex align-items-center">
                    <span class="transaction-icon"><i class="fas fa-receipt"></i></span>
                    ${categoryName}
                </div>
            </td>
            <td>${description}</td>
            <td class="text-end fw-bold ${isExpense ? 'text-danger' : 'text-success'}">
                ${isExpense ? '-' : '+'}${formatter.format(Math.abs(amount))}
            </td>
        `;
        tbody.appendChild(row);
    });
}

// Export Functions
function exportTradingExcel() {
    const { startStr, endStr } = getDateRange(currentPeriod);
    const url = `/api/ReportExport/excel?startDate=${startStr}&endDate=${endStr}`;
    window.location.href = url;
}

function exportTradingReport() {
    const element = document.getElementById('tradingDashboard');
    const opt = {
        margin: 0.3,
        filename: `BaoCao_GiaoDich_${new Date().toISOString().split('T')[0]}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true },
        jsPDF: { unit: 'in', format: 'a4', orientation: 'portrait' }
    };

    const btn = document.querySelector('.btn-export-pdf');
    const originalContent = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    btn.disabled = true;

    html2pdf().set(opt).from(element).save().then(() => {
        btn.innerHTML = originalContent;
        btn.disabled = false;
    }).catch(err => {
        alert('Lỗi xuất PDF: ' + err.message);
        btn.innerHTML = originalContent;
        btn.disabled = false;
    });
}

// Compare Logic
function updateComparison() {
    const select = document.getElementById('comparisonSelect');
    comparisonMode = select.value;
    loadDashboardData();
}

// Utils
function animateValue(id, end, formatter) {
    const obj = document.getElementById(id);
    if (!obj) return;
    obj.innerHTML = formatter(end); // Simple set for now
}

function showLoadingState(show) {
    const container = document.querySelector('.trading-container');
    if (container) {
        container.style.opacity = show ? '0.7' : '1';
        container.style.pointerEvents = show ? 'none' : 'auto';
    }
}
