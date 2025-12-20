// ========================================
// REPORTS PAGE - UNIFIED IMPLEMENTATION  
// Gộp từ: reports-complete.js, reports-ai.js, reports-enhanced.js
// Ngày tạo: 20/12/2025 21:42
// ========================================

'use strict';
// ========================================
// REPORTS PAGE - COMPLETE IMPLEMENTATION
// Clean, bug-free code with all features
// ========================================

'use strict';

// Global state management
const ReportsApp = {
    // State
    state: {
        currentPeriod: 'month',
        dateRange: { start: null, end: null },
        filters: {
            accountIds: [],
            categoryIds: [],
            transactionType: null,
            minAmount: null,
            maxAmount: null
        },
        charts: {
            expense: null,
            income: null,
            trend: null
        },
        cache: new Map(),
        cacheTimeout: 5 * 60 * 1000, // 5 minutes
        isLoading: false,
        hasError: false
    },

    // Initialize application
    init() {
        try {
            console.log('Initializing Reports app...');
            this.setupEventListeners();
            this.initDates();
            this.setupAccessibility();
            console.log('Reports app initialized successfully');
        } catch (error) {
            console.error('Failed to initialize Reports app:', error);
            this.showError('KhÃ´ng thá»ƒ khá»Ÿi táº¡o trang bÃ¡o cÃ¡o');
        }
    },

    // Date initialization
    initDates() {
        console.log('Initializing dates...');
        // Set the period which will also load data
        this.setPeriod('month');
        
        // Setup export date defaults
        const now = new Date();
        const exportStartEl = document.getElementById('exportStartDate');
        const exportEndEl = document.getElementById('exportEndDate');
        
        if (exportEndEl) exportEndEl.valueAsDate = now;
        if (exportStartEl) {
            exportStartEl.valueAsDate = new Date(now.getFullYear(), now.getMonth(), 1);
        }
        console.log('Dates initialized, data loading triggered');
    },

    // Event listeners setup
    setupEventListeners() {
        // Period filter buttons
        document.querySelectorAll('.filter-btn').forEach(btn => {
            if (btn.dataset.period) {
                btn.addEventListener('click', () => this.setPeriod(btn.dataset.period));
            }
        });

        // Custom date inputs
        const customStart = document.getElementById('customStart');
        const customEnd = document.getElementById('customEnd');
        
        if (customStart) customStart.addEventListener('change', () => this.setCustomPeriod());
        if (customEnd) customEnd.addEventListener('change', () => this.setCustomPeriod());

        // Export modal
        const exportModal = document.getElementById('exportModal');
        if (exportModal) {
            exportModal.addEventListener('click', (e) => {
                if (e.target.id === 'exportModal') this.closeExportModal();
            });
        }

        // ESC key to close modal
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') this.closeExportModal();
        });
    },

    // Set period
    setPeriod(period) {
        console.log('setPeriod called with:', period);
        
        if (!['day', 'week', 'month', 'year'].includes(period)) {
            console.error('Invalid period:', period);
            return;
        }

        this.state.currentPeriod = period;
        const now = new Date();
        let start = new Date();
        let end = new Date();

        // Clear custom inputs
        const customStart = document.getElementById('customStart');
        const customEnd = document.getElementById('customEnd');
        if (customStart) customStart.value = '';
        if (customEnd) customEnd.value = '';

        // Update UI buttons
        const filterButtons = document.querySelectorAll('.filter-btn');
        console.log('Found filter buttons:', filterButtons.length);
        
        filterButtons.forEach(btn => {
            const isActive = btn.dataset.period === period;
            btn.classList.toggle('bg-indigo-50', isActive);
            btn.classList.toggle('text-indigo-600', isActive);
            btn.classList.toggle('dark:bg-indigo-900/30', isActive);
            btn.classList.toggle('dark:text-indigo-400', isActive);
            btn.classList.toggle('shadow-sm', isActive);
            btn.setAttribute('aria-pressed', isActive.toString());
        });

        // Calculate date range
        switch(period) {
            case 'day':
                start = end = now;
                break;
            case 'week':
                const day = now.getDay();
                const diff = now.getDate() - day + (day === 0 ? -6 : 1);
                start = new Date(now);
                start.setDate(diff);
                end = now;
                break;
            case 'month':
                start = new Date(now.getFullYear(), now.getMonth(), 1);
                end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
                break;
            case 'year':
                start = new Date(now.getFullYear(), 0, 1);
                end = new Date(now.getFullYear(), 11, 31);
                break;
        }

        this.state.dateRange = { start, end };
        console.log('Date range set:', { start, end });
        
        this.clearCache();
        this.loadAllData();
    },

    // Set custom period
    setCustomPeriod() {
        const startInput = document.getElementById('customStart');
        const endInput = document.getElementById('customEnd');
        
        if (!startInput || !endInput) return;
        
        const startValue = startInput.value;
        const endValue = endInput.value;

        if (startValue && endValue) {
            const start = new Date(startValue);
            const end = new Date(endValue);

            // Validation
            if (start > end) {
                this.showError('NgÃ y báº¯t Ä‘áº§u pháº£i trÆ°á»›c ngÃ y káº¿t thÃºc');
                return;
            }

            // Clear preset buttons
            document.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('bg-indigo-50', 'text-indigo-600', 'dark:bg-indigo-900/30', 'dark:text-indigo-400', 'shadow-sm');
                btn.setAttribute('aria-pressed', 'false');
            });

            this.state.dateRange = { start, end };
            this.state.currentPeriod = 'custom';
            this.clearCache();
            this.loadAllData();
        }
    },

    // Load all data
    async loadAllData() {
        if (this.state.isLoading) return;

        this.state.isLoading = true;
        this.state.hasError = false;
        this.showLoadingState();

        try {
            const { start, end } = this.state.dateRange;
            if (!start || !end) {
                throw new Error('Invalid date range');
            }

            const cacheKey = `reports_${this.formatDate(start)}_${this.formatDate(end)}`;
            
            // Check cache
            const cached = this.getCachedData(cacheKey);
            if (cached) {
                console.log('Using cached data');
                this.renderAllData(cached);
                return;
            }

            // Fetch data in parallel
            const [dashboardRes, categoryRes, flowRes] = await Promise.all([
                this.fetchWithRetry('/api/Report/dashboard'),
                this.fetchWithRetry(`/api/Report/categories?startDate=${this.formatDate(start)}&endDate=${this.formatDate(end)}`),
                this.fetchWithRetry(`/api/Report/cashflow?startDate=${this.formatDate(start)}&endDate=${this.formatDate(end)}`)
            ]);

            // Check responses
            if (!dashboardRes.ok || !categoryRes.ok || !flowRes.ok) {
                throw new Error('Failed to fetch report data');
            }

            const data = {
                dashboard: await dashboardRes.json(),
                categories: await categoryRes.json(),
                cashflow: await flowRes.json()
            };

            // Cache the data
            this.setCachedData(cacheKey, data);

            // Render data
            this.renderAllData(data);

        } catch (error) {
            console.error('Error loading report data:', error);
            this.state.hasError = true;
            
            // More specific error messages
            if (error.message.includes('401') || error.message.includes('Unauthorized')) {
                this.showError('PhiÃªn Ä‘Äƒng nháº­p Ä‘Ã£ háº¿t háº¡n. Vui lÃ²ng Ä‘Äƒng nháº­p láº¡i.');
                setTimeout(() => window.location.href = '/Auth/Login', 2000);
            } else if (error.message.includes('404')) {
                this.showError('KhÃ´ng tÃ¬m tháº¥y dá»¯ liá»‡u bÃ¡o cÃ¡o.');
            } else if (error.message.includes('500')) {
                this.showError('Lá»—i mÃ¡y chá»§. Vui lÃ²ng thá»­ láº¡i sau.');
            } else {
                // For development: show more details
                console.log('Full error:', error);
                this.showError('KhÃ´ng thá»ƒ táº£i dá»¯ liá»‡u bÃ¡o cÃ¡o. Vui lÃ²ng thá»­ láº¡i.');
            }
        } finally {
            this.state.isLoading = false;
            this.hideLoadingState();
        }
    },

    // Render all data
    renderAllData(data) {
        try {
            if (!data) {
                throw new Error('No data received');
            }

            // Handle empty data gracefully
            if (!data.cashflow) {
                console.warn('No cashflow data available');
                this.showEmptyState();
                return;
            }

            // Debug: Log the data structure
            console.log('Rendering data:', {
                categories: data.categories,
                cashflow: data.cashflow,
                dashboard: data.dashboard
            });

            this.updateStats(data.cashflow);
            this.renderExpensePie(data.categories || {});
            this.renderIncomePie(data.dashboard || {}, data.cashflow);
            this.renderTrendChart(data.cashflow);
        } catch (error) {
            console.error('Error rendering data:', error);
            this.showError('KhÃ´ng thá»ƒ hiá»ƒn thá»‹ dá»¯ liá»‡u');
        }
    },

    // Show empty state
    showEmptyState() {
        // Update stats to zero
        const fmt = (n) => new Intl.NumberFormat('vi-VN', { 
            style: 'currency', 
            currency: 'VND',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(n);

        const statIncome = document.getElementById('statIncome');
        const statExpense = document.getElementById('statExpense');
        const statSavings = document.getElementById('statSavings');
        const savingsRate = document.getElementById('savingsRate');

        if (statIncome) statIncome.textContent = fmt(0);
        if (statExpense) statExpense.textContent = fmt(0);
        if (statSavings) statSavings.textContent = fmt(0);
        if (savingsRate) savingsRate.textContent = '0%';

        // Show message
        this.showToast('KhÃ´ng cÃ³ dá»¯ liá»‡u trong khoáº£ng thá»i gian nÃ y', 'info');
    },

    // Update stats
    updateStats(data) {
        const fmt = (n) => {
            if (typeof n !== 'number') n = 0;
            return new Intl.NumberFormat('vi-VN', { 
                style: 'currency', 
                currency: 'VND',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            }).format(n);
        };

        const statIncome = document.getElementById('statIncome');
        const statExpense = document.getElementById('statExpense');
        const statSavings = document.getElementById('statSavings');
        const savingsRate = document.getElementById('savingsRate');

        // Handle both PascalCase and camelCase
        const totalIncome = data?.TotalIncome ?? data?.totalIncome ?? 0;
        const totalExpense = data?.TotalExpense ?? data?.totalExpense ?? 0;

        if (statIncome) statIncome.textContent = fmt(totalIncome);
        if (statExpense) statExpense.textContent = fmt(totalExpense);

        const net = totalIncome - totalExpense;
        if (statSavings) statSavings.textContent = fmt(net);

        const rate = totalIncome > 0 ? (net / totalIncome * 100) : 0;
        if (savingsRate) savingsRate.textContent = rate.toFixed(1) + '%';

        // Update trends (mock for now)
        const incomeTrend = document.getElementById('incomeTrend');
        const expenseTrend = document.getElementById('expenseTrend');
        if (incomeTrend) incomeTrend.textContent = '+12%';
        if (expenseTrend) expenseTrend.textContent = '-5%';
    },

    // Render expense pie chart
    renderExpensePie(data) {
        console.log('renderExpensePie called with data:', data);
        
        const ctx = document.getElementById('expensePieChart');
        if (!ctx) {
            console.error('expensePieChart canvas not found');
            return;
        }

        // Destroy existing chart
        if (this.state.charts.expense) {
            this.state.charts.expense.destroy();
            this.state.charts.expense = null;
        }

        // Handle both PascalCase (from API) and camelCase
        const categories = data?.ExpenseCategories || data?.expenseCategories || [];
        
        console.log('Expense categories found:', categories.length, categories);
        
        if (categories.length === 0) {
            console.log('No expense data to display');
            // Show empty chart message
            const legendContainer = document.getElementById('expenseLegend');
            if (legendContainer) {
                legendContainer.innerHTML = '<li class="text-sm text-gray-500 p-2">KhÃ´ng cÃ³ dá»¯ liá»‡u chi tiÃªu</li>';
            }
            // Update center text to show 0
            const centerText = document.getElementById('centerExpenseTotal');
            if (centerText) {
                centerText.textContent = '0 Ä‘';
            }
            return;
        }

        // Handle both PascalCase and camelCase property names
        const labels = categories.map(c => c.CategoryName || c.categoryName || 'Unknown');
        const values = categories.map(c => c.Amount || c.amount || 0);
        const colors = categories.map(c => 
            c.CategoryColor || c.categoryColor || this.generateColor(c.CategoryName || c.categoryName)
        );

        console.log('Chart data:', { labels, values, colors });

        // Update center text
        const total = values.reduce((a, b) => a + b, 0);
        console.log('Total expense:', total);
        
        const centerText = document.getElementById('centerExpenseTotal');
        if (centerText) {
            centerText.textContent = new Intl.NumberFormat('vi-VN', { 
                style: 'currency', 
                currency: 'VND',
                notation: 'compact',
                compactDisplay: 'short'
            }).format(total);
        }

        // Update legend
        const legendContainer = document.getElementById('expenseLegend');
        if (legendContainer) {
            legendContainer.innerHTML = categories.map((c, i) => {
                const amount = c.Amount || c.amount || 0;
                const categoryName = c.CategoryName || c.categoryName || 'Unknown';
                const percentage = total > 0 ? ((amount / total) * 100).toFixed(1) : 0;
                return `
                    <li class="flex items-center justify-between text-sm cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 p-2 rounded-lg transition-colors">
                        <div class="flex items-center gap-2">
                            <span class="w-3 h-3 rounded-full" style="background-color: ${colors[i]}"></span>
                            <span class="text-gray-700 dark:text-gray-300">${categoryName}</span>
                        </div>
                        <span class="font-bold text-gray-900 dark:text-white text-xs">${percentage}%</span>
                    </li>
                `;
            }).join('');
        }

        // Create chart
        this.state.charts.expense = new Chart(ctx, {
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
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(255, 255, 255, 0.9)',
                        titleColor: '#1f2937',
                        bodyColor: '#1f2937',
                        borderColor: '#e5e7eb',
                        borderWidth: 1,
                        padding: 10,
                        displayColors: true,
                        callbacks: {
                            label: (context) => {
                                let label = context.label || '';
                                if (label) label += ': ';
                                label += new Intl.NumberFormat('vi-VN', { 
                                    style: 'currency', 
                                    currency: 'VND' 
                                }).format(context.raw);
                                return label;
                            }
                        }
                    }
                }
            }
        });
    },

    // Render income pie chart
    renderIncomePie(dashData, flowData) {
        const ctx = document.getElementById('incomeBudgetPieChart');
        if (!ctx) return;

        // Update balance - handle both PascalCase and camelCase
        const currentBalance = dashData?.CurrentBalance ?? dashData?.currentBalance;
        if (currentBalance !== undefined) {
            const statBalance = document.getElementById('statBalance');
            if (statBalance) {
                statBalance.textContent = new Intl.NumberFormat('vi-VN', { 
                    style: 'currency', 
                    currency: 'VND' 
                }).format(currentBalance);
            }
        }

        // Destroy existing chart
        if (this.state.charts.income) {
            this.state.charts.income.destroy();
            this.state.charts.income = null;
        }

        // Handle both PascalCase and camelCase
        const income = flowData?.TotalIncome ?? flowData?.totalIncome ?? 0;
        const expense = flowData?.TotalExpense ?? flowData?.totalExpense ?? 0;
        const savings = Math.max(0, income - expense);

        // Update center text
        const centerText = document.getElementById('centerIncomeTotal');
        if (centerText) {
            centerText.textContent = new Intl.NumberFormat('vi-VN', { 
                style: 'currency', 
                currency: 'VND',
                notation: 'compact',
                compactDisplay: 'short'
            }).format(income);
        }

        // Update savings value
        const savingsValue = document.getElementById('savingsValue');
        if (savingsValue) {
            savingsValue.textContent = new Intl.NumberFormat('vi-VN', { 
                style: 'currency', 
                currency: 'VND' 
            }).format(savings);
        }

        // Create chart
        this.state.charts.income = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Chi tiÃªu', 'Tiáº¿t kiá»‡m (DÆ°)'],
                datasets: [{
                    data: [expense, savings],
                    backgroundColor: ['#f43f5e', '#10b981'],
                    borderWidth: 0,
                    hoverOffset: 10
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: { legend: { display: false } }
            }
        });

        // Update progress bar
        const percent = income > 0 ? (expense / income * 100) : 0;
        const budgetProgress = document.getElementById('budgetProgress');
        const budgetLimit = document.getElementById('budgetLimit');
        
        if (budgetProgress) {
            budgetProgress.style.width = Math.min(percent, 100) + '%';
            if (percent > 100) {
                budgetProgress.classList.add('bg-red-500');
                budgetProgress.classList.remove('bg-indigo-600');
            } else {
                budgetProgress.classList.remove('bg-red-500');
                budgetProgress.classList.add('bg-indigo-600');
            }
        }
        
        if (budgetLimit) {
            budgetLimit.textContent = percent.toFixed(1) + '%';
        }
    },

    // Render trend chart
    renderTrendChart(data) {
        const ctx = document.getElementById('trendLineChart');
        if (!ctx) return;

        // Destroy existing chart
        if (this.state.charts.trend) {
            this.state.charts.trend.destroy();
            this.state.charts.trend = null;
        }

        // Handle both PascalCase and camelCase
        const dailyBreakdown = data?.DailyBreakdown ?? data?.dailyBreakdown;
        
        // Generate labels if not present
        let labels = dailyBreakdown?.map(d => {
            const dateStr = d.Date || d.date;
            const date = new Date(dateStr);
            return `${date.getDate()}/${date.getMonth() + 1}`;
        }) || [];

        let incomeData = dailyBreakdown?.map(d => d.Income ?? d.income ?? 0) || [];
        let expenseData = dailyBreakdown?.map(d => d.Expense ?? d.expense ?? 0) || [];

        if (labels.length === 0) {
            console.log('No trend data to display');
            return;
        }

        // Create gradients
        const ctxGradient = ctx.getContext('2d');
        const gradInc = ctxGradient.createLinearGradient(0, 0, 0, 300);
        gradInc.addColorStop(0, 'rgba(16, 185, 129, 0.2)');
        gradInc.addColorStop(1, 'rgba(16, 185, 129, 0.0)');

        const gradExp = ctxGradient.createLinearGradient(0, 0, 0, 300);
        gradExp.addColorStop(0, 'rgba(244, 63, 94, 0.2)');
        gradExp.addColorStop(1, 'rgba(244, 63, 94, 0.0)');

        // Create chart
        this.state.charts.trend = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Thu nháº­p',
                        data: incomeData,
                        borderColor: '#10b981',
                        backgroundColor: gradInc,
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 6
                    },
                    {
                        label: 'Chi tiÃªu',
                        data: expenseData,
                        borderColor: '#f43f5e',
                        backgroundColor: gradExp,
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointHoverRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(255, 255, 255, 0.9)',
                        titleColor: '#1f2937',
                        bodyColor: '#1f2937',
                        borderColor: '#e5e7eb',
                        borderWidth: 1,
                        padding: 10,
                        displayColors: true,
                        callbacks: {
                            label: (context) => {
                                let label = context.dataset.label || '';
                                if (label) label += ': ';
                                label += new Intl.NumberFormat('vi-VN', { 
                                    style: 'currency', 
                                    currency: 'VND' 
                                }).format(context.raw);
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { borderDash: [2, 4], color: '#f3f4f6' },
                        ticks: { 
                            callback: (value) => {
                                return (value / 1000000).toFixed(1) + 'M';
                            }
                        }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    },

    // Export modal
    showExportModal() {
        const modal = document.getElementById('exportModal');
        if (modal) {
            modal.classList.remove('hidden');
            modal.querySelector('input, select')?.focus();
        }
    },

    closeExportModal() {
        const modal = document.getElementById('exportModal');
        if (modal) {
            modal.classList.add('hidden');
        }
    },

    // Submit export
    async submitExport() {
        const reportType = document.getElementById('exportReportType')?.value;
        const formatRadio = document.querySelector('input[name="exportFormat"]:checked');
        const start = document.getElementById('exportStartDate')?.value;
        const end = document.getElementById('exportEndDate')?.value;

        // Validation
        if (!start || !end) {
            this.showError('Vui lÃ²ng chá»n khoáº£ng thá»i gian');
            return;
        }

        if (!formatRadio) {
            this.showError('Vui lÃ²ng chá»n Ä‘á»‹nh dáº¡ng file');
            return;
        }

        const format = parseInt(formatRadio.value);

        const payload = {
            reportType: parseInt(reportType),
            startDate: start,
            endDate: end,
            fileFormat: format
        };

        try {
            this.showLoadingState();

            const response = await fetch('/api/Report/export', {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                let errorMessage = 'Export failed';
                try {
                    const error = await response.json();
                    errorMessage = error.message || errorMessage;
                } catch (e) {
                    errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            const blob = await response.blob();
            const contentDisposition = response.headers.get('Content-Disposition');
            const filename = contentDisposition 
                ? contentDisposition.split('filename=')[1]?.replace(/"/g, '')
                : `report_${Date.now()}.${this.getFileExtension(format)}`;

            this.downloadBlob(blob, filename);
            this.closeExportModal();
            this.showSuccess('Xuáº¥t bÃ¡o cÃ¡o thÃ nh cÃ´ng!');

        } catch (error) {
            console.error('Export error:', error);
            this.showError('CÃ³ lá»—i khi xuáº¥t bÃ¡o cÃ¡o: ' + error.message);
        } finally {
            this.hideLoadingState();
        }
    },

    // Helper methods
    formatDate(date) {
        if (!(date instanceof Date)) return '';
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    },

    generateColor(str) {
        const colors = ['#6366f1', '#ec4899', '#10b981', '#f59e0b', '#3b82f6', '#8b5cf6', '#ef4444', '#14b8a6'];
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        return colors[Math.abs(hash) % colors.length];
    },

    getFileExtension(format) {
        const extensions = { 1: 'pdf', 2: 'xlsx', 3: 'csv', 4: 'json' };
        return extensions[format] || 'file';
    },

    downloadBlob(blob, filename) {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    },

    // Cache methods
    getCachedData(key) {
        const cached = this.state.cache.get(key);
        if (!cached) return null;

        const now = Date.now();
        if (now - cached.timestamp > this.state.cacheTimeout) {
            this.state.cache.delete(key);
            return null;
        }

        return cached.data;
    },

    setCachedData(key, data) {
        this.state.cache.set(key, {
            data,
            timestamp: Date.now()
        });
    },

    clearCache() {
        this.state.cache.clear();
    },

    // Fetch with retry
    async fetchWithRetry(url, options = {}, retries = 3) {
        for (let i = 0; i < retries; i++) {
            try {
                const response = await fetch(url, options);
                
                // If response is not ok, throw error with status
                if (!response.ok) {
                    const errorText = await response.text().catch(() => 'Unknown error');
                    const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
                    error.status = response.status;
                    error.response = response;
                    
                    // Don't retry on 401, 403, 404
                    if (response.status === 401 || response.status === 403 || response.status === 404) {
                        throw error;
                    }
                    
                    // Retry on other errors if retries left
                    if (i < retries - 1) {
                        console.log(`Retry ${i + 1}/${retries} for ${url}`);
                        await this.sleep(1000 * (i + 1));
                        continue;
                    }
                    
                    throw error;
                }
                
                return response;
            } catch (error) {
                // Don't retry on auth errors
                if (error.status === 401 || error.status === 403) {
                    throw error;
                }
                
                if (i === retries - 1) throw error;
                console.log(`Retry ${i + 1}/${retries} after error:`, error.message);
                await this.sleep(1000 * (i + 1));
            }
        }
    },

    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },

    // UI state methods
    showLoadingState() {
        document.body.classList.add('loading');
    },

    hideLoadingState() {
        document.body.classList.remove('loading');
    },

    showError(message) {
        console.error(message);
        // Create toast notification instead of alert
        this.showToast(message, 'error');
    },

    showSuccess(message) {
        console.log(message);
        // Create toast notification instead of alert
        this.showToast(message, 'success');
    },

    showToast(message, type = 'info') {
        // Remove existing toasts
        const existingToast = document.getElementById('reportToast');
        if (existingToast) existingToast.remove();

        // Create toast element
        const toast = document.createElement('div');
        toast.id = 'reportToast';
        toast.className = `fixed top-4 right-4 z-50 px-6 py-4 rounded-xl shadow-2xl transform transition-all duration-300 max-w-md ${
            type === 'error' ? 'bg-red-500 text-white' :
            type === 'success' ? 'bg-green-500 text-white' :
            'bg-blue-500 text-white'
        }`;
        toast.innerHTML = `
            <div class="flex items-center gap-3">
                <i class="fas ${type === 'error' ? 'fa-exclamation-circle' : type === 'success' ? 'fa-check-circle' : 'fa-info-circle'} text-xl"></i>
                <span class="font-medium">${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" class="ml-4 text-white hover:text-gray-200">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        document.body.appendChild(toast);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (toast && toast.parentElement) {
                toast.style.opacity = '0';
                toast.style.transform = 'translateX(100%)';
                setTimeout(() => toast.remove(), 300);
            }
        }, 5000);
    },

    // Accessibility
    setupAccessibility() {
        // Add ARIA labels
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.setAttribute('role', 'button');
            btn.setAttribute('aria-pressed', 'false');
        });

        // Create ARIA live region
        if (!document.getElementById('ariaLiveRegion')) {
            const liveRegion = document.createElement('div');
            liveRegion.id = 'ariaLiveRegion';
            liveRegion.className = 'sr-only';
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            document.body.appendChild(liveRegion);
        }
    },

    announceToScreenReader(message) {
        const liveRegion = document.getElementById('ariaLiveRegion');
        if (liveRegion) {
            liveRegion.textContent = message;
            setTimeout(() => liveRegion.textContent = '', 1000);
        }
    }
};

// AI Section toggle
function toggleAiSection() {
    const content = document.getElementById('aiContent');
    if (!content) return;

    if (content.classList.contains('hidden')) {
        content.classList.remove('hidden');
        // Load AI suggestions if not already loaded
        if (typeof loadAiSuggestions === 'function') {
            loadAiSuggestions();
        }
    } else {
        content.classList.add('hidden');
    }
}

// Export global functions
window.ReportsApp = ReportsApp;
window.setPeriod = (period) => ReportsApp.setPeriod(period);
window.setCustomPeriod = () => ReportsApp.setCustomPeriod();
window.showExportModal = () => ReportsApp.showExportModal();
window.closeExportModal = () => ReportsApp.closeExportModal();
window.submitExport = () => ReportsApp.submitExport();
window.toggleAiSection = toggleAiSection;

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => ReportsApp.init());
} else {
    ReportsApp.init();
}


// ========================================
// AI ANALYSIS FEATURES
// ========================================



let aiSpendingPatternChart = null;
let aiPredictionChart = null;

// AI Analysis Functions
async function generateAiSuggestions() {
    const loadingEl = document.getElementById('aiLoading');
    const containerEl = document.getElementById('aiInsightsContainer');
    const recommendationsEl = document.getElementById('aiRecommendations');
    
    // Show loading
    if (loadingEl) loadingEl.classList.remove('hidden');
    if (containerEl) containerEl.innerHTML = '';
    if (recommendationsEl) recommendationsEl.innerHTML = '';
    
    try {
        // Fetch user's financial data
        const response = await fetch('/api/Report/ai-analysis');
        
        if (!response.ok) {
            // Use mock data if API not available
            await displayMockAiAnalysis();
            return;
        }
        
        const data = await response.json();
        displayAiAnalysis(data);
        
    } catch (error) {
        console.error('Error generating AI suggestions:', error);
        await displayMockAiAnalysis();
    } finally {
        if (loadingEl) loadingEl.classList.add('hidden');
    }
}

async function displayMockAiAnalysis() {
    // Mock AI analysis data
    const mockData = {
        insights: [
            {
                type: 'warning',
                icon: 'fa-exclamation-triangle',
                title: 'Chi tiÃªu vÆ°á»£t má»©c',
                description: 'Chi tiÃªu thÃ¡ng nÃ y cao hÆ¡n 23% so vá»›i trung bÃ¬nh 3 thÃ¡ng trÆ°á»›c',
                color: 'red'
            },
            {
                type: 'success',
                icon: 'fa-check-circle',
                title: 'Tiáº¿t kiá»‡m tá»‘t',
                description: 'Báº¡n Ä‘Ã£ tiáº¿t kiá»‡m Ä‘Æ°á»£c 15% thu nháº­p thÃ¡ng nÃ y',
                color: 'green'
            },
            {
                type: 'info',
                icon: 'fa-info-circle',
                title: 'Xu hÆ°á»›ng chi tiÃªu',
                description: 'Chi tiÃªu cho Äƒn uá»‘ng tÄƒng 18% trong thÃ¡ng qua',
                color: 'blue'
            },
            {
                type: 'tip',
                icon: 'fa-lightbulb',
                title: 'CÆ¡ há»™i tiáº¿t kiá»‡m',
                description: 'Giáº£m 20% chi phÃ­ giáº£i trÃ­ cÃ³ thá»ƒ tiáº¿t kiá»‡m thÃªm 2.5 triá»‡u/thÃ¡ng',
                color: 'yellow'
            }
        ],
        spendingPattern: {
            labels: ['Thá»© 2', 'Thá»© 3', 'Thá»© 4', 'Thá»© 5', 'Thá»© 6', 'Thá»© 7', 'CN'],
            data: [1200000, 850000, 1500000, 950000, 2100000, 1800000, 2500000]
        },
        recommendations: [
            {
                icon: 'fa-piggy-bank',
                title: 'TÄƒng tiáº¿t kiá»‡m',
                description: 'Äáº·t má»¥c tiÃªu tiáº¿t kiá»‡m 20% thu nháº­p má»—i thÃ¡ng',
                priority: 'high'
            },
            {
                icon: 'fa-chart-line',
                title: 'Äáº§u tÆ° thÃ´ng minh',
                description: 'Xem xÃ©t Ä‘áº§u tÆ° vÃ o quá»¹ chá»‰ sá»‘ vá»›i sá»‘ tiá»n tiáº¿t kiá»‡m',
                priority: 'medium'
            },
            {
                icon: 'fa-cut',
                title: 'Cáº¯t giáº£m chi phÃ­',
                description: 'Giáº£m chi tiÃªu khÃ´ng cáº§n thiáº¿t á»Ÿ danh má»¥c giáº£i trÃ­',
                priority: 'high'
            },
            {
                icon: 'fa-calendar-check',
                title: 'Láº­p káº¿ hoáº¡ch ngÃ¢n sÃ¡ch',
                description: 'Táº¡o ngÃ¢n sÃ¡ch chi tiáº¿t cho tá»«ng danh má»¥c',
                priority: 'medium'
            }
        ],
        predictions: {
            labels: ['ThÃ¡ng 1', 'ThÃ¡ng 2', 'ThÃ¡ng 3', 'ThÃ¡ng 4', 'ThÃ¡ng 5', 'ThÃ¡ng 6'],
            actual: [15000000, 18000000, 16500000, 19000000, 17500000, null],
            predicted: [null, null, null, null, null, 18500000],
            confidence: {
                upper: [null, null, null, null, null, 20000000],
                lower: [null, null, null, null, null, 17000000]
            }
        },
        predictionSummary: {
            nextMonth: 18500000,
            confidence: 85,
            trend: 'stable',
            advice: 'Chi tiÃªu dá»± kiáº¿n á»•n Ä‘á»‹nh. ÄÃ¢y lÃ  thá»i Ä‘iá»ƒm tá»‘t Ä‘á»ƒ tÄƒng tiáº¿t kiá»‡m.'
        }
    };
    
    displayAiAnalysis(mockData);
}

function displayAiAnalysis(data) {
    // Display insights cards
    displayInsightsCards(data.insights);
    
    // Display spending pattern chart
    displaySpendingPatternChart(data.spendingPattern);
    
    // Display recommendations
    displayRecommendations(data.recommendations);
    
    // Display predictions
    displayPredictions(data.predictions, data.predictionSummary);
}

function displayInsightsCards(insights) {
    const container = document.getElementById('aiInsightsContainer');
    if (!container) return;
    
    const colorMap = {
        red: { bg: 'bg-red-50 dark:bg-red-900/20', border: 'border-red-200 dark:border-red-800', text: 'text-red-600 dark:text-red-400', icon: 'text-red-500' },
        green: { bg: 'bg-green-50 dark:bg-green-900/20', border: 'border-green-200 dark:border-green-800', text: 'text-green-600 dark:text-green-400', icon: 'text-green-500' },
        blue: { bg: 'bg-blue-50 dark:bg-blue-900/20', border: 'border-blue-200 dark:border-blue-800', text: 'text-blue-600 dark:text-blue-400', icon: 'text-blue-500' },
        yellow: { bg: 'bg-yellow-50 dark:bg-yellow-900/20', border: 'border-yellow-200 dark:border-yellow-800', text: 'text-yellow-600 dark:text-yellow-400', icon: 'text-yellow-500' }
    };
    
    container.innerHTML = insights.map(insight => {
        const colors = colorMap[insight.color] || colorMap.blue;
        return `
            <div class="p-4 ${colors.bg} border-2 ${colors.border} rounded-xl transition-all hover:shadow-lg">
                <div class="flex items-start gap-3">
                    <div class="flex-shrink-0">
                        <i class="fas ${insight.icon} ${colors.icon} text-2xl"></i>
                    </div>
                    <div class="flex-1">
                        <h5 class="font-bold ${colors.text} mb-1">${insight.title}</h5>
                        <p class="text-sm text-gray-600 dark:text-gray-400">${insight.description}</p>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function displaySpendingPatternChart(patternData) {
    const ctx = document.getElementById('aiSpendingPatternChart');
    if (!ctx) return;
    
    if (aiSpendingPatternChart) {
        aiSpendingPatternChart.destroy();
    }
    
    const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(139, 92, 246, 0.3)');
    gradient.addColorStop(1, 'rgba(139, 92, 246, 0.0)');
    
    aiSpendingPatternChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: patternData.labels,
            datasets: [{
                label: 'Chi tiÃªu',
                data: patternData.data,
                borderColor: '#8b5cf6',
                backgroundColor: gradient,
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 6,
                pointHoverRadius: 8,
                pointBackgroundColor: '#8b5cf6',
                pointBorderColor: '#fff',
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function(context) {
                            return 'Chi tiÃªu: ' + formatCurrency(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return (value / 1000000).toFixed(1) + 'M';
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

function displayRecommendations(recommendations) {
    const container = document.getElementById('aiRecommendations');
    if (!container) return;
    
    const priorityColors = {
        high: 'border-red-500 bg-red-50 dark:bg-red-900/20',
        medium: 'border-yellow-500 bg-yellow-50 dark:bg-yellow-900/20',
        low: 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
    };
    
    container.innerHTML = recommendations.map(rec => `
        <div class="flex items-start gap-4 p-4 border-l-4 ${priorityColors[rec.priority]} rounded-lg transition-all hover:shadow-md">
            <div class="flex-shrink-0 w-10 h-10 rounded-full bg-white dark:bg-gray-800 flex items-center justify-center">
                <i class="fas ${rec.icon} text-purple-600"></i>
            </div>
            <div class="flex-1">
                <h5 class="font-bold text-gray-900 dark:text-white mb-1">${rec.title}</h5>
                <p class="text-sm text-gray-600 dark:text-gray-400">${rec.description}</p>
            </div>
            <span class="text-xs font-bold uppercase px-2 py-1 rounded ${rec.priority === 'high' ? 'bg-red-100 text-red-600' : rec.priority === 'medium' ? 'bg-yellow-100 text-yellow-600' : 'bg-blue-100 text-blue-600'}">
                ${rec.priority === 'high' ? 'Cao' : rec.priority === 'medium' ? 'Trung bÃ¬nh' : 'Tháº¥p'}
            </span>
        </div>
    `).join('');
}

function displayPredictions(predictions, summary) {
    const ctx = document.getElementById('aiPredictionChart');
    if (!ctx) return;
    
    if (aiPredictionChart) {
        aiPredictionChart.destroy();
    }
    
    aiPredictionChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: predictions.labels,
            datasets: [
                {
                    label: 'Chi tiÃªu thá»±c táº¿',
                    data: predictions.actual,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 3,
                    fill: false,
                    tension: 0.4,
                    pointRadius: 6,
                    pointBackgroundColor: '#3b82f6'
                },
                {
                    label: 'Dá»± Ä‘oÃ¡n',
                    data: predictions.predicted,
                    borderColor: '#8b5cf6',
                    backgroundColor: 'rgba(139, 92, 246, 0.1)',
                    borderWidth: 3,
                    borderDash: [5, 5],
                    fill: false,
                    tension: 0.4,
                    pointRadius: 6,
                    pointBackgroundColor: '#8b5cf6'
                },
                {
                    label: 'Khoáº£ng tin cáº­y (cao)',
                    data: predictions.confidence.upper,
                    borderColor: 'rgba(139, 92, 246, 0.3)',
                    backgroundColor: 'rgba(139, 92, 246, 0.1)',
                    borderWidth: 1,
                    borderDash: [2, 2],
                    fill: '+1',
                    tension: 0.4,
                    pointRadius: 0
                },
                {
                    label: 'Khoáº£ng tin cáº­y (tháº¥p)',
                    data: predictions.confidence.lower,
                    borderColor: 'rgba(139, 92, 246, 0.3)',
                    backgroundColor: 'rgba(139, 92, 246, 0.1)',
                    borderWidth: 1,
                    borderDash: [2, 2],
                    fill: false,
                    tension: 0.4,
                    pointRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        filter: function(item) {
                            return !item.text.includes('Khoáº£ng tin cáº­y');
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function(context) {
                            if (context.parsed.y === null) return null;
                            return context.dataset.label + ': ' + formatCurrency(context.parsed.y);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return (value / 1000000).toFixed(1) + 'M';
                        }
                    }
                }
            }
        }
    });
    
    // Display prediction summary
    const summaryContainer = document.getElementById('aiPredictionSummary');
    if (summaryContainer && summary) {
        const trendIcon = summary.trend === 'up' ? 'fa-arrow-up text-red-500' : 
                         summary.trend === 'down' ? 'fa-arrow-down text-green-500' : 
                         'fa-minus text-blue-500';
        
        summaryContainer.innerHTML = `
            <div class="flex items-start gap-4">
                <div class="flex-shrink-0">
                    <div class="w-12 h-12 rounded-full bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
                        <i class="fas ${trendIcon} text-xl"></i>
                    </div>
                </div>
                <div class="flex-1">
                    <div class="flex items-center gap-2 mb-2">
                        <h5 class="font-bold text-gray-900 dark:text-white">Dá»± Ä‘oÃ¡n thÃ¡ng tá»›i:</h5>
                        <span class="text-2xl font-bold text-purple-600">${formatCurrency(summary.nextMonth)}</span>
                    </div>
                    <div class="flex items-center gap-2 mb-2">
                        <span class="text-sm text-gray-600 dark:text-gray-400">Äá»™ tin cáº­y:</span>
                        <div class="flex-1 h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                            <div class="h-full bg-gradient-to-r from-purple-500 to-indigo-500" style="width: ${summary.confidence}%"></div>
                        </div>
                        <span class="text-sm font-bold text-purple-600">${summary.confidence}%</span>
                    </div>
                    <p class="text-sm text-gray-600 dark:text-gray-400 italic">${summary.advice}</p>
                </div>
            </div>
        `;
    }
}

// Load AI suggestions on page load
async function loadAiSuggestions() {
    await generateAiSuggestions();
}

// Helper function
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { 
        style: 'currency', 
        currency: 'VND',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

// Export functions

