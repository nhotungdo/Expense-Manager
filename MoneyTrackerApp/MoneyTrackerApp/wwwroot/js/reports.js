/**
 * Analytics Dashboard Logic
 * Handles data fetching, chart rendering, and interactive features for the new Reports interface.
 */

'use strict';

const ReportsApp = {
    state: {
        period: 'month',
        dateRange: { start: null, end: null },
        charts: {
            trend: null,
            expense: null
        },
        data: null, // Cache current data
        loading: false
    },

    init() {
        this.setupListeners();
        this.setPeriod('month'); // Initial load
    },

    setupListeners() {
        // Filter Buttons
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const period = e.target.dataset.period;
                this.setPeriod(period);

                // Update active class
                document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
            });
        });

        // Export Modal Listeners
        const modal = document.getElementById('exportModal');
        modal.addEventListener('click', (e) => {
            if (e.target === modal) this.closeExportModal();
        });
    },

    setPeriod(period) {
        this.state.period = period;
        const range = this.calculateDateRange(period);
        this.state.dateRange = range;

        // Update label
        const labelEl = document.getElementById('trendPeriodLabel');
        if (labelEl) {
            if (period === 'month') labelEl.innerText = 'Tháng này';
            else if (period === 'quarter') labelEl.innerText = 'Quý này';
            else if (period === 'year') labelEl.innerText = 'Năm nay';
        }

        this.loadData();
    },

    calculateDateRange(period) {
        const now = new Date();
        let start = new Date();
        let end = new Date();

        if (period === 'month') {
            start = new Date(now.getFullYear(), now.getMonth(), 1);
            end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
        } else if (period === 'quarter') {
            const quarter = Math.floor(now.getMonth() / 3);
            start = new Date(now.getFullYear(), quarter * 3, 1);
            end = new Date(now.getFullYear(), (quarter + 1) * 3, 0);
        } else if (period === 'year') {
            start = new Date(now.getFullYear(), 0, 1);
            end = new Date(now.getFullYear(), 11, 31);
        }

        // Format for API
        return {
            start: this.formatDate(start),
            end: this.formatDate(end)
        };
    },

    formatDate(date) {
        return date.toISOString().split('T')[0];
    },

    async loadData() {
        if (this.state.loading) return;
        this.state.loading = true;
        this.toggleLoading(true);

        try {
            const { start, end } = this.state.dateRange;

            // Parallel fetch
            const [dashboardRes, categoriesRes, cashflowRes] = await Promise.all([
                fetch('/api/Report/dashboard'),
                fetch(`/api/Report/categories?startDate=${start}&endDate=${end}`),
                fetch(`/api/Report/cashflow?startDate=${start}&endDate=${end}`)
            ]);

            if (!dashboardRes.ok || !categoriesRes.ok || !cashflowRes.ok) throw new Error('Failed to fetch data');

            const data = {
                dashboard: await dashboardRes.json(),
                categories: await categoriesRes.json(), // Returns list of categories
                cashflow: await cashflowRes.json()
            };

            this.state.data = data;
            this.renderAll(data);
            this.generateAIInsight(data);

        } catch (error) {
            console.error('Data load error:', error);
            // Optionally show toast error
        } finally {
            this.state.loading = false;
            this.toggleLoading(false);
        }
    },

    renderAll(data) {
        this.renderStats(data.cashflow, data.dashboard);
        this.renderTrendChart(data.cashflow);
        this.renderExpensePie(data.categories);
        this.renderTopCategories(data.categories);
    },

    renderStats(cashflow, dashboard) {
        // Handle Capitalization from backend (could be Pascal or Camel)
        const income = cashflow.TotalIncome || cashflow.totalIncome || 0;
        const expense = cashflow.TotalExpense || cashflow.totalExpense || 0;
        const balance = dashboard.CurrentBalance || dashboard.currentBalance || 0;

        document.getElementById('statBalance').innerText = this.formatCurrency(balance);
        document.getElementById('statIncome').innerText = this.formatCurrency(income);
        document.getElementById('statExpense').innerText = this.formatCurrency(expense);

        // Simple trends (mock logic for demo if prev period data missing)
        // In real app, we'd fetch prev period too.
        document.getElementById('incomeTrend').innerText = 'Dòng tiền ổn định';
        document.getElementById('expenseTrend').innerText = 'Trong hạn mức ngân sách';
    },

    renderTrendChart(cashflow) {
        const ctx = document.getElementById('trendLineChart').getContext('2d');
        if (this.state.charts.trend) this.state.charts.trend.destroy();

        const breakdown = cashflow.DailyBreakdown || cashflow.dailyBreakdown || [];
        // Map data
        const labels = breakdown.map(d => new Date(d.Date || d.date).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }));
        const incomeData = breakdown.map(d => d.Income || d.income || 0);
        const expenseData = breakdown.map(d => d.Expense || d.expense || 0);

        // Gradients
        const gradInc = ctx.createLinearGradient(0, 0, 0, 300);
        gradInc.addColorStop(0, 'rgba(16, 185, 129, 0.2)');
        gradInc.addColorStop(1, 'rgba(16, 185, 129, 0.0)');

        const gradExp = ctx.createLinearGradient(0, 0, 0, 300);
        gradExp.addColorStop(0, 'rgba(239, 68, 68, 0.2)');
        gradExp.addColorStop(1, 'rgba(239, 68, 68, 0.0)');

        this.state.charts.trend = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Thu nhập',
                        data: incomeData,
                        borderColor: '#10b981',
                        backgroundColor: gradInc,
                        fill: true,
                        tension: 0.4
                    },
                    {
                        label: 'Chi tiêu',
                        data: expenseData,
                        borderColor: '#ef4444',
                        backgroundColor: gradExp,
                        fill: true,
                        tension: 0.4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top' },
                    tooltip: { mode: 'index', intersect: false }
                },
                interaction: { mode: 'nearest', axis: 'x', intersect: false },
                scales: {
                    y: { beginAtZero: true, grid: { borderDash: [2, 2] } },
                    x: { grid: { display: false } }
                }
            }
        });
    },

    renderExpensePie(categoriesData) {
        const ctx = document.getElementById('expensePieChart').getContext('2d');
        if (this.state.charts.expense) this.state.charts.expense.destroy();

        const expenseCats = (categoriesData.ExpenseCategories || categoriesData.expenseCategories || [])
            .sort((a, b) => (b.Amount || b.amount) - (a.Amount || a.amount));

        if (expenseCats.length === 0) {
            // handle empty
            return;
        }

        const labels = expenseCats.map(c => c.CategoryName || c.categoryName);
        const data = expenseCats.map(c => c.Amount || c.amount);
        const colors = expenseCats.map(c => c.CategoryColor || c.categoryColor || '#cbd5e1');

        const total = data.reduce((a, b) => a + b, 0);
        document.getElementById('centerExpenseTotal').innerText = this.formatCurrencyCompact(total);

        // Render Legend
        const legendHTML = expenseCats.slice(0, 5).map(c => {
            const amt = c.Amount || c.amount;
            const pct = ((amt / total) * 100).toFixed(1);
            return `
                <li class="flex justify-between text-sm">
                    <div class="flex items-center gap-2">
                        <span class="w-3 h-3 rounded-full" style="background-color: ${c.CategoryColor || c.categoryColor}"></span>
                        <span>${c.CategoryName || c.categoryName}</span>
                    </div>
                    <span class="font-bold text-gray-700">${pct}%</span>
                </li>
             `;
        }).join('');
        document.getElementById('expenseLegend').innerHTML = legendHTML;

        this.state.charts.expense = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderWidth: 0,
                    hoverOffset: 10
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: { legend: { display: false } }
            }
        });
    },

    renderTopCategories(categoriesData) {
        const tableBody = document.getElementById('topCategoriesTable');
        const expenseCats = (categoriesData.ExpenseCategories || categoriesData.expenseCategories || [])
            .sort((a, b) => (b.Amount || b.amount) - (a.Amount || a.amount));

        const total = expenseCats.reduce((sum, c) => sum + (c.Amount || c.amount), 0);

        tableBody.innerHTML = expenseCats.slice(0, 5).map(c => {
            const amount = c.Amount || c.amount;
            const pct = total > 0 ? ((amount / total) * 100).toFixed(1) : 0;
            return `
                <tr class="border-b border-gray-50 last:border-0 hover:bg-gray-50 transition-colors">
                    <td class="py-3 font-medium flex items-center gap-2">
                         <div class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs" style="background-color: ${c.CategoryColor || c.categoryColor}">
                            <i class="${c.CategoryIcon || c.categoryIcon || 'fas fa-tag'}"></i>
                         </div>
                         ${c.CategoryName || c.categoryName}
                    </td>
                    <td class="py-3 text-right font-bold text-gray-700">${this.formatCurrency(amount)}</td>
                    <td class="py-3 text-right text-gray-500">${pct}%</td>
                    <td class="py-3 text-right text-gray-500">-</td> <!-- Transaction count not in this specific DTO usually -->
                </tr>
            `;
        }).join('');
    },

    generateAIInsight(data) {
        // Simple heuristic mock for "AI"
        const income = data.cashflow.TotalIncome || 0;
        const expense = data.cashflow.TotalExpense || 0;
        const savingsRate = income > 0 ? ((income - expense) / income * 100) : 0;

        let msg = "Dữ liệu đang được phân tích...";
        if (expense > income) {
            msg = "Cảnh báo: Bạn đang chi tiêu vượt quá thu nhập trong kỳ này. Hãy xem xét cắt giảm các khoản chi không cần thiết.";
        } else if (savingsRate > 30) {
            msg = "Tuyệt vời! Bạn đang tiết kiệm được hơn 30% thu nhập. Hãy cân nhắc đầu tư khoản dư này.";
        } else if (savingsRate > 0) {
            msg = "Tài chính ổn định. Bạn đang duy trì mức chi tiêu hợp lý trong phạm vi thu nhập.";
        }

        const el = document.getElementById('aiInsightText');
        if (el) el.innerText = msg;
    },

    // UI Helpers
    showExportModal() {
        document.getElementById('exportModal').style.display = 'flex';
        // Set default dates
        document.getElementById('exportStartDate').value = this.state.dateRange.start;
        document.getElementById('exportEndDate').value = this.state.dateRange.end;
    },

    closeExportModal() {
        document.getElementById('exportModal').style.display = 'none';
    },

    async submitExport() {
        const type = document.getElementById('exportReportType').value;
        const format = document.querySelector('input[name="exportFormat"]:checked').value; // 1=PDF, 2=Excel
        const start = document.getElementById('exportStartDate').value;
        const end = document.getElementById('exportEndDate').value;

        let url = '';
        if (type == '1') { // Transactions
            url = format == '2'
                ? `/api/Report/export/transactions/excel?startDate=${start}&endDate=${end}`
                : `/api/Report/export/transactions/pdf?startDate=${start}&endDate=${end}`;
        } else {
            // Fallback for others or mock
            alert('Tính năng xuất báo cáo này đang phát triển. Vui lòng chọn Chi tiết Giao dịch.');
            return;
        }

        // Trigger download
        const a = document.createElement('a');
        a.href = url;
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        this.closeExportModal();
    },

    toggleLoading(show) {
        // Optional: show overlay or spinner
    },

    formatCurrency(val) {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
    },

    formatCurrencyCompact(val) {
        if (val >= 1000000) return (val / 1000000).toFixed(1) + ' Tr';
        return (val / 1000).toFixed(0) + ' K';
    }
};

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    ReportsApp.init();
});
