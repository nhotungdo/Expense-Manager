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
    loadSmartRecommendations();
    renderEmptyChart();
}

function setupEventListeners() {
    // Period Selectors
    const periodInputs = document.querySelectorAll('input[name="period"]');
    periodInputs.forEach(input => {
        input.addEventListener('change', (e) => {
            currentPeriod = e.target.value;
            loadDashboardData();
            loadSmartRecommendations();
        });
    });
}

// Load Smart AI Recommendations
async function loadSmartRecommendations() {
    const container = document.getElementById('smartBudgetContainer');
    if (!container) return;

    container.innerHTML = '<div class="d-flex justify-content-center py-4"><div class="spinner-border text-info" role="status"></div></div>';

    try {
        const response = await fetchSafe(`/api/Analysis/smart-recommendations?period=${currentPeriod}`);

        if (!response || !response.recommendations) {
            container.innerHTML = '<p class="text-muted text-center py-3">Không có đề xuất nào.</p>';
            return;
        }

        let html = '';

        // Display Advanced Recommendations
        if (response.recommendations && response.recommendations.length > 0) {
            html += '<div class="mb-4">';
            html += '<h6 class="text-muted mb-3"><i class="fas fa-chart-line me-2"></i>Phân tích chi tiết</h6>';

            response.recommendations.forEach(rec => {
                const typeClass = rec.type === 'alert' ? 'danger' : rec.type === 'warning' ? 'warning' : rec.type === 'info' ? 'info' : 'success';
                const icon = rec.type === 'alert' ? 'fa-exclamation-triangle' : rec.type === 'warning' ? 'fa-exclamation-circle' : rec.type === 'info' ? 'fa-info-circle' : 'fa-lightbulb';

                html += `
                    <div class="alert alert-${typeClass} alert-dismissible fade show mb-3" role="alert">
                        <div class="d-flex align-items-start">
                            <i class="fas ${icon} me-3 mt-1"></i>
                            <div class="flex-grow-1">
                                <h6 class="alert-heading mb-1">${rec.title}</h6>
                                <p class="mb-2 small">${rec.description}</p>
                                ${rec.potentialSavings ? `<div class="small"><strong>Tiết kiệm tiềm năng:</strong> ${formatCurrency(rec.potentialSavings)}</div>` : ''}
                                ${rec.suggestedBudget ? `<div class="small"><strong>Ngân sách đề xuất:</strong> ${formatCurrency(rec.suggestedBudget)}</div>` : ''}
                                ${rec.recommendedDaily ? `<div class="small"><strong>Chi tiêu hàng ngày nên:</strong> ${formatCurrency(rec.recommendedDaily)}</div>` : ''}
                            </div>
                        </div>
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                `;
            });
            html += '</div>';
        }

        // Display Budget Suggestions Table
        if (response.budgetSuggestions && response.budgetSuggestions.length > 0) {
            html += '<div class="mb-3">';
            html += '<h6 class="text-muted mb-3"><i class="fas fa-calculator me-2"></i>Ngân sách đề xuất theo danh mục</h6>';
            html += '<div class="table-responsive">';
            html += '<table class="table table-sm table-hover align-middle">';
            html += '<thead class="table-light"><tr><th>Danh mục</th><th class="text-end">Chi hiện tại</th><th class="text-end">Đề xuất</th><th class="text-center">Độ tin cậy</th></tr></thead>';
            html += '<tbody>';

            response.budgetSuggestions.forEach(sug => {
                const confidenceBadge = sug.confidence === 'high' ? 'success' : sug.confidence === 'medium' ? 'warning' : 'secondary';
                const confidenceText = sug.confidence === 'high' ? 'Cao' : sug.confidence === 'medium' ? 'TB' : 'Thấp';

                html += `
                    <tr>
                        <td><strong>${sug.category}</strong><br><small class="text-muted">${sug.transactionCount} giao dịch</small></td>
                        <td class="text-end">${formatCurrency(sug.currentSpending)}</td>
                        <td class="text-end text-primary"><strong>${formatCurrency(sug.suggestedMonthlyBudget)}</strong></td>
                        <td class="text-center"><span class="badge bg-${confidenceBadge}">${confidenceText}</span></td>
                    </tr>
                `;
            });

            html += '</tbody></table></div></div>';
        }

        // Display Analysis Summary
        if (response.analysis) {
            const savingsClass = response.analysis.savingsRate > 20 ? 'success' : response.analysis.savingsRate > 10 ? 'warning' : 'danger';
            html += `
                <div class="row g-2 mt-3">
                    <div class="col-6">
                        <div class="p-3 bg-light rounded text-center">
                            <small class="text-muted d-block">Tỷ lệ tiết kiệm</small>
                            <h5 class="mb-0 text-${savingsClass}">${response.analysis.savingsRate.toFixed(1)}%</h5>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 bg-light rounded text-center">
                            <small class="text-muted d-block">Chi TB/ngày</small>
                            <h5 class="mb-0">${formatCurrency(response.analysis.dailyAverage)}</h5>
                        </div>
                    </div>
                </div>
            `;
        }

        container.innerHTML = html || '<p class="text-muted text-center py-3">Chưa có dữ liệu phân tích.</p>';

    } catch (error) {
        console.error('Error loading smart recommendations:', error);
        container.innerHTML = '<p class="text-danger text-center py-3">Không thể tải đề xuất AI.</p>';
    }
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

    // Check if we have real AI content
    if (insights.content && insights.content.length > 50 && !insights.content.startsWith("Lỗi")) {
        // Use Gemini Content
        let cleanText = insights.content;
        cleanText = cleanText.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        cleanText = cleanText.replace(/\n\n/g, '<br><br>');
        cleanText = cleanText.replace(/\n/g, '<br>'); // Simple newlines

        textField.innerHTML = cleanText;

        // Hide the box or use it for specific metrics
        box.innerHTML = '';
        if (insights.savingsRate > 20) {
            box.innerHTML = `<div class="mt-3 p-2 bg-success bg-opacity-10 rounded text-success"><i class="fas fa-check-circle me-1"></i> Tỷ lệ tiết kiệm tốt: ${insights.savingsRate.toFixed(1)}%</div>`;
        } else if (insights.totalExpense > insights.totalIncome && insights.totalIncome > 0) {
            box.innerHTML = `<div class="mt-3 p-2 bg-danger bg-opacity-10 rounded text-danger"><i class="fas fa-exclamation-triangle me-1"></i> Bội chi</div>`;
        }

    } else {
        // Fallback Logic
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

// Recommendations - Enhanced AI-Powered Analysis
function renderRecommendations(insights, anomalies) {
    const container = document.getElementById('savingsRecommendations');
    container.innerHTML = '';

    const recs = [];

    // 1. Critical Anomalies - High Priority
    if (anomalies && Array.isArray(anomalies) && anomalies.length > 0) {
        const totalAnomalous = anomalies.reduce((sum, a) => sum + (a.amount || 0), 0);
        recs.push({
            type: 'high',
            title: '🚨 Phát hiện giao dịch bất thường',
            desc: `${anomalies.length} giao dịch vượt ngưỡng bình thường (${formatCurrency(totalAnomalous)}). Hãy kiểm tra kỹ để tránh chi tiêu lãng phí.`,
            icon: 'fa-exclamation-triangle',
            action: 'Xem chi tiết'
        });
    }

    // 2. Budget Crisis - Overspending Alert
    if (insights && insights.totalExpense > 0 && insights.totalIncome > 0) {
        const spendingRatio = (insights.totalExpense / insights.totalIncome) * 100;

        if (spendingRatio > 100) {
            recs.push({
                type: 'high',
                title: '⚠️ Bội chi nghiêm trọng',
                desc: `Chi tiêu vượt thu nhập ${(spendingRatio - 100).toFixed(1)}%. Cần cắt giảm chi tiêu ngay để tránh nợ nần.`,
                icon: 'fa-chart-line-down'
            });
        } else if (spendingRatio > 90) {
            recs.push({
                type: 'high',
                title: '⚡ Ngân sách căng thẳng',
                desc: `Bạn đã chi ${spendingRatio.toFixed(1)}% thu nhập. Chỉ còn ${formatCurrency(insights.totalIncome - insights.totalExpense)} để tiết kiệm.`,
                icon: 'fa-battery-quarter'
            });
        } else if (spendingRatio > 70) {
            recs.push({
                type: 'medium',
                title: '💡 Cân nhắc tiết kiệm',
                desc: `Tỷ lệ chi tiêu ${spendingRatio.toFixed(1)}% là hợp lý, nhưng có thể tối ưu hơn để tăng tiết kiệm.`,
                icon: 'fa-piggy-bank'
            });
        }
    }

    // 3. Savings Performance
    if (insights && insights.savingsRate !== undefined) {
        if (insights.savingsRate > 30) {
            recs.push({
                type: 'low',
                title: '🎉 Tiết kiệm xuất sắc!',
                desc: `Tỷ lệ tiết kiệm ${insights.savingsRate.toFixed(1)}% vượt trội. Bạn đang trên đà đạt mục tiêu tài chính!`,
                icon: 'fa-trophy'
            });
        } else if (insights.savingsRate < 10 && insights.savingsRate > 0) {
            recs.push({
                type: 'medium',
                title: '📊 Tiết kiệm thấp',
                desc: `Chỉ tiết kiệm được ${insights.savingsRate.toFixed(1)}%. Mục tiêu lý tưởng là 20-30% thu nhập.`,
                icon: 'fa-chart-pie'
            });
        }
    }

    // 4. Category Spending Intelligence
    if (insights && insights.topCategory) {
        recs.push({
            type: 'medium',
            title: '🔍 Phân tích danh mục',
            desc: `"${insights.topCategory}" chiếm tỷ trọng lớn nhất. AI khuyến nghị: Xem xét các khoản chi trong danh mục này để tìm cơ hội tiết kiệm.`,
            icon: 'fa-layer-group'
        });
    }

    // 5. Spending Velocity Warning (if daily average is high)
    if (insights && currentData.dailyAverage > 0) {
        const projectedMonthly = currentData.dailyAverage * 30;
        if (insights.totalIncome > 0 && projectedMonthly > insights.totalIncome * 0.8) {
            recs.push({
                type: 'high',
                title: '⏱️ Tốc độ chi tiêu cao',
                desc: `Với mức chi trung bình ${formatCurrency(currentData.dailyAverage)}/ngày, bạn có thể chi hết ${formatCurrency(projectedMonthly)} trong tháng.`,
                icon: 'fa-tachometer-alt'
            });
        }
    }

    // 6. AI Trend Prediction
    if (insights && insights.expenseTrend !== undefined && insights.expenseTrend !== 0) {
        const trendDirection = insights.expenseTrend > 0 ? 'tăng' : 'giảm';
        const trendType = insights.expenseTrend > 0 ? 'medium' : 'low';
        recs.push({
            type: trendType,
            title: `📈 Xu hướng ${trendDirection}`,
            desc: `Chi tiêu ${trendDirection} ${Math.abs(insights.expenseTrend)}% so với kỳ trước. ${insights.expenseTrend > 0 ? 'Cần kiểm soát chặt chẽ hơn.' : 'Đang cải thiện tốt!'}`,
            icon: insights.expenseTrend > 0 ? 'fa-arrow-trend-up' : 'fa-arrow-trend-down'
        });
    }

    // Fallback - Positive Reinforcement
    if (recs.length === 0) {
        recs.push({
            type: 'low',
            title: '✅ Tài chính ổn định',
            desc: 'Thói quen chi tiêu của bạn đang được kiểm soát tốt. Tiếp tục duy trì!',
            icon: 'fa-check-circle'
        });
    }

    // Render with enhanced UI
    recs.forEach(rec => {
        const div = document.createElement('div');
        div.className = `recommendation-item rec-${rec.type} fade-in-up`;
        div.innerHTML = `
            <div class="rec-header">
                <span class="rec-title">${rec.title}</span>
                ${getPriorityIcon(rec.type)}
            </div>
            <div class="rec-desc">${rec.desc}</div>
            ${rec.action ? `<div class="rec-action mt-2"><small class="text-primary"><i class="fas fa-arrow-right me-1"></i>${rec.action}</small></div>` : ''}
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

function formatCurrency(val) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0);
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
