/**
 * Reports v2.0 Logic
 * Modern, clean, and efficient data handling.
 */

'use strict';

const ReportsApp = {
    state: {
        currentPeriod: 'month',
        charts: {},
        data: null
    },

    init() {
        this.bindEvents();
        // Load initial data (Dashboard + Default Period)
        this.loadDashboard();
    },

    bindEvents() {
        document.querySelectorAll('.date-pill').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.switchPeriod(e.target);
            });
        });
    },

    switchPeriod(targetBtn) {
        // UI Toggle
        document.querySelectorAll('.date-pill').forEach(b => b.classList.remove('active'));
        targetBtn.classList.add('active');

        const period = targetBtn.dataset.period;
        this.state.currentPeriod = period;

        // Reload specific stats based on period if the API supports it, 
        // for now we'll reload the main charts using calculated dates.
        this.loadPeriodData(period);
    },

    getDatesForPeriod(period) {
        const now = new Date();
        let start, end;

        if (period === 'month') {
            start = new Date(now.getFullYear(), now.getMonth(), 1);
            end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
        } else if (period === 'quarter') {
            const q = Math.floor(now.getMonth() / 3);
            start = new Date(now.getFullYear(), q * 3, 1);
            end = new Date(now.getFullYear(), (q + 1) * 3, 0);
        } else { // year
            start = new Date(now.getFullYear(), 0, 1);
            end = new Date(now.getFullYear(), 11, 31);
        }
        return {
            start: start.toISOString().split('T')[0],
            end: end.toISOString().split('T')[0]
        };
    },

    async loadDashboard() {
        try {
            // Dashboard endpoint gives us 'current month' usually or 'current state'
            // For the initial load, we trust it.
            const res = await fetch('/api/Report/dashboard');
            if (!res.ok) throw new Error("Failed to load dashboard");
            const data = await res.json();

            this.renderKPIs(data);
            this.renderRecentTransactions(data.RecentTransactions);

            // For charts, we use the specific endpoints to ensure we match the 'month' selection initially
            // although dashboard DTO *has* CashFlowChart, let's stick to consistent loading
            this.loadPeriodData('month');

        } catch (err) {
            console.error(err);
        }
    },

    async loadPeriodData(period) {
        const { start, end } = this.getDatesForPeriod(period);
        document.getElementById('chartPeriodLabel').innerText = `${start} - ${end}`;

        try {
            const [cashflowRes, catRes] = await Promise.all([
                fetch(`/api/Report/cashflow?startDate=${start}&endDate=${end}`),
                fetch(`/api/Report/categories?startDate=${start}&endDate=${end}`)
            ]);

            const cashflow = await cashflowRes.json();
            const categories = await catRes.json();

            // Update Charts
            this.renderCashflowChart(cashflow);
            this.renderCategoryChart(categories);
            this.renderTopCategoriesList(categories);

            // Update KPI totals specific to this period which are in cashflow response
            this.updatePeriodKPIs(cashflow);

            // Generate Insight based on this period
            this.generateInsight(cashflow);

        } catch (err) {
            console.error("Error loading period data", err);
        }
    },

    renderKPIs(dashboardData) {
        // Balance is always "current" regardless of period
        document.getElementById('kpiBalance').innerText = this.formatCurrency(dashboardData.CurrentBalance);
    },

    updatePeriodKPIs(cashflowData) {
        document.getElementById('kpiIncome').innerText = this.formatCurrency(cashflowData.TotalIncome);
        document.getElementById('kpiExpense').innerText = this.formatCurrency(cashflowData.TotalExpense);

        // Trends - mocked for now as we don't fetch "previous period"
        // But we can show net flow as a hint
        const net = cashflowData.TotalIncome - cashflowData.TotalExpense;
        const trendEl = document.getElementById('trendIncome');
        // Just reusing the slot for "Net Flow" text for simplicity in this design iteration
        trendEl.innerText = `Dư: ${this.formatCurrencyCompact(net)}`;

        const expenseEl = document.getElementById('trendExpense');
        expenseEl.innerText = "Chi tiêu trong kỳ";
    },

    renderCashflowChart(data) {
        const ctx = document.getElementById('cashflowChart');
        if (this.state.charts.cashflow) this.state.charts.cashflow.destroy();

        const labels = data.DailyBreakdown.map(d => new Date(d.Date).getDate());
        const income = data.DailyBreakdown.map(d => d.Income);
        const expense = data.DailyBreakdown.map(d => d.Expense);

        this.state.charts.cashflow = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Thu nhập',
                        data: income,
                        backgroundColor: '#10b981',
                        borderRadius: 4,
                        barPercentage: 0.6
                    },
                    {
                        label: 'Chi tiêu',
                        data: expense,
                        backgroundColor: '#ef4444',
                        borderRadius: 4,
                        barPercentage: 0.6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top', align: 'end', labels: { usePointStyle: true, boxWidth: 8 } },
                    tooltip: { mode: 'index', intersect: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: '#f1f5f9' },
                        ticks: { callback: (val) => this.formatCurrencyCompact(val) }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    },

    renderCategoryChart(data) {
        const ctx = document.getElementById('categoryChart');
        if (this.state.charts.category) this.state.charts.category.destroy();

        // Sort by amount
        const items = (data.ExpenseCategories || []).sort((a, b) => b.Amount - a.Amount).slice(0, 5);

        const labels = items.map(c => c.CategoryName);
        const values = items.map(c => c.Amount);
        const colors = items.map(c => c.CategoryColor || '#cbd5e1');

        this.state.charts.category = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderWidth: 0,
                    hoverOffset: 15
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: {
                    legend: { display: false }
                }
            }
        });
    },

    renderTopCategoriesList(data) {
        const container = document.getElementById('topCategoriesList');
        const items = (data.ExpenseCategories || []).sort((a, b) => b.Amount - a.Amount).slice(0, 4);

        if (items.length === 0) {
            container.innerHTML = '<p class="text-sm text-gray-400 text-center py-4">Chưa có dữ liệu</p>';
            return;
        }

        const total = items.reduce((sum, i) => sum + i.Amount, 0);

        container.innerHTML = items.map(item => `
            <div class="txn-item">
                <div class="txn-left">
                    <div class="txn-icon" style="background-color: ${item.CategoryColor || '#ccc'}">
                        <i class="${item.CategoryIcon || 'fas fa-tag'}"></i>
                    </div>
                    <div class="txn-details">
                        <h4>${item.CategoryName}</h4>
                        <p>${((item.Amount / total) * 100).toFixed(0)}% chi tiêu top</p>
                    </div>
                </div>
                <div class="txn-amount text-red-500">
                    ${this.formatCurrencyCompact(item.Amount)}
                </div>
            </div>
        `).join('');
    },

    renderRecentTransactions(txns) {
        const container = document.getElementById('recentTransactions');
        if (!txns || txns.length === 0) {
            container.innerHTML = '<p class="text-sm text-gray-400 py-4">Chưa có giao dịch nào gần đây</p>';
            return;
        }

        container.innerHTML = txns.slice(0, 5).map(t => {
            const isInc = t.Type === 'Income';
            const sign = isInc ? '+' : '-';
            const colorClass = isInc ? 'text-green-600' : 'text-red-500';

            return `
                <div class="txn-item">
                    <div class="txn-left">
                        <div class="txn-icon" style="background-color: ${t.CategoryColor || '#e2e8f0'}">
                            <i class="${t.CategoryIcon || 'fas fa-exchange-alt'}"></i>
                        </div>
                        <div class="txn-details">
                            <h4>${t.Description || t.CategoryName}</h4>
                            <p>${new Date(t.Date).toLocaleDateString()} &bull; ${t.AccountName || 'Wallet'}</p>
                        </div>
                    </div>
                    <div class="txn-amount ${colorClass}">
                        ${sign}${this.formatCurrency(t.Amount)}
                    </div>
                </div>
            `;
        }).join('');
    },

    generateInsight(cashflow) {
        const income = cashflow.TotalIncome;
        const expense = cashflow.TotalExpense;
        const ratio = income > 0 ? (expense / income) * 100 : 0;

        let title, desc, icon;

        if (income === 0 && expense === 0) {
            title = "Chưa có dữ liệu";
            desc = "Hãy thêm giao dịch để xem phân tích.";
        } else if (expense > income) {
            title = "Chi tiêu vượt thu nhập";
            desc = `Bạn đang chi tiêu ${ratio.toFixed(0)}% so với thu nhập. Hãy cẩn trọng.`;
        } else if (ratio < 50) {
            title = "Sức khỏe tài chính tốt";
            desc = "Bạn đang giữ chi tiêu ở mức thấp. Tuyệt vời!";
        } else {
            title = "Cân đối ổn định";
            desc = `Bạn đã chi ${ratio.toFixed(0)}% thu nhập. Hãy cố gắng tiết kiệm thêm.`;
        }

        document.getElementById('aiTitle').innerText = title;
        document.getElementById('aiDesc').innerText = desc;
    },

    exportReport() {
        // Simple functionality for now, could open the modal from previous version logic
        // But user wanted 'rewrite' so maybe just trigger a basic export
        const period = this.state.currentPeriod;
        const { start, end } = this.getDatesForPeriod(period);
        window.open(`/api/Report/export/transactions/excel?startDate=${start}&endDate=${end}`, '_blank');
    },

    refreshData() {
        this.loadDashboard();
    },

    formatCurrency(val) {
        return window.formatCurrencyVND(val);
    },

    formatCurrencyCompact(val) {
        if (Math.abs(val) >= 1000000) return (val / 1000000).toFixed(1) + ' Tr';
        if (Math.abs(val) >= 1000) return (val / 1000).toFixed(0) + ' K';
        return val;
    }
};

document.addEventListener('DOMContentLoaded', () => {
    ReportsApp.init();
});
