/**
 * Investments Page Controller
 * Handles chart initialization, data fetching, and user interactions
 */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize the page
    initInvestments();
});

// Mock Application State
const appState = {
    portfolio: {
        totalValue: 245000000,
        invested: 200000000,
        profit: 45000000,
        dividend: 1200000,
        riskLevel: 'Trung bình',
        assets: [
            { id: 1, symbol: 'AAPL', name: 'Apple Inc.', type: 'Stock', quantity: 15, avgPrice: 3500000, currentPrice: 4200000 },
            { id: 2, symbol: 'BTC', name: 'Bitcoin', type: 'Crypto', quantity: 0.05, avgPrice: 1000000000, currentPrice: 1500000000 },
            { id: 3, symbol: 'VND', name: 'VNDirect', type: 'Stock', quantity: 1000, avgPrice: 20000, currentPrice: 22500 },
            { id: 4, symbol: 'TCB', name: 'Techcombank bond', type: 'Bond', quantity: 50, avgPrice: 100000, currentPrice: 105000 }
        ]
    },
    transactions: [], // To be populated
    charts: {}, // Store chart instances
    zoomLevel: 'normal', // normal, zoomed-in, zoomed-out
    fullPerformanceData: { // Store full data for zoom
        months: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
        values: [180000000, 195000000, 190000000, 210000000, 225000000, 245000000, 250000000, 260000000, 255000000, 270000000, 280000000, 290000000]
    }
};

/**
 * Initialize all components
 */
function initInvestments() {
    generateMockTransactions();
    updateLastUpdatedTime();
    renderSummaryStats();
    renderCharts();
    renderHoldingsTable();
    setupEventListeners();
}

/**
 * Update Last Updated Time
 */
function updateLastUpdatedTime() {
    const el = document.getElementById('lastUpdatedTime');
    if (el) {
        const now = new Date();
        el.innerHTML = `<i class="fas fa-clock"></i> Cập nhật: ${now.toLocaleTimeString('vi-VN')}`;
    }
}

/**
 * Generate Mock Transactions for Testing Performance
 */
function generateMockTransactions() {
    const transactions = [];
    const types = ['Buy', 'Sell'];
    const now = new Date();

    // Generate 1000 random transactions over the last year
    for (let i = 0; i < 1000; i++) {
        const daysAgo = Math.floor(Math.random() * 365);
        const date = new Date(now);
        date.setDate(date.getDate() - daysAgo);

        const type = types[Math.floor(Math.random() * types.length)];
        // Higher value for Buy to simulate net investment
        const value = Math.floor(Math.random() * 10000000) + 1000000;

        transactions.push({
            id: i,
            date: date,
            type: type,
            value: value,
            symbol: i % 2 === 0 ? 'AAPL' : 'BTC',
            assetType: i % 2 === 0 ? 'Stock' : 'Crypto'
        });
    }
    appState.transactions = transactions.sort((a, b) => b.date - a.date);
}

/**
 * Render Summary Statistics
 */
function renderSummaryStats() {
    const { totalValue, invested, profit, dividend, assets } = appState.portfolio;
    const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);

    document.getElementById('totalValue').innerText = formatCurrency(totalValue);

    const change = totalValue - invested;
    const changePercent = invested > 0 ? ((change / invested) * 100).toFixed(2) : 0;
    const changeElem = document.getElementById('totalChange');
    changeElem.innerHTML = `<i class="fas fa-arrow-${change >= 0 ? 'up' : 'down'}"></i> ${formatCurrency(change)} (${changePercent}%)`;
    changeElem.className = `change-badge ${change >= 0 ? 'positive' : 'negative'}`;

    document.getElementById('totalInvested').innerText = formatCurrency(invested);
    document.getElementById('totalProfit').innerText = formatCurrency(profit);
    document.getElementById('totalDividend').innerText = formatCurrency(dividend);
    document.getElementById('assetCount').innerText = assets.length;
}

/**
 * Render Charts using Charts-Config
 */
function renderCharts() {
    renderPerformanceChart();
    renderAllocationChart();
    renderTransactionChart('week'); // Default to week
}

/**
 * Render Performance Line Chart with Zoom capabilities
 */
function renderPerformanceChart() {
    const ctx = document.getElementById('performanceChart');
    if (!ctx) return;

    let displayMonths = appState.fullPerformanceData.months;
    let displayValues = appState.fullPerformanceData.values;

    // Apply Zoom Logic
    if (appState.zoomLevel === 'zoomed-in') {
        const len = displayMonths.length;
        displayMonths = displayMonths.slice(len - 3, len);
        displayValues = displayValues.slice(len - 3, len);
    } else if (appState.zoomLevel === 'zoomed-out') {
        // Show everything (default logic handled here, but explicit)
    } else {
        // Normal View (e.g., last 6 months)
        const len = displayMonths.length;
        displayMonths = displayMonths.slice(Math.max(len - 6, 0), len);
        displayValues = displayValues.slice(Math.max(len - 6, 0), len);
    }

    const datasets = [{
        label: 'Giá trị danh mục',
        data: displayValues,
        color: ChartColors.primary.main,
        backgroundColor: ChartColors.primary.alpha(0.1),
        fill: true
    }];

    const config = LineChartConfig.getConfig(datasets, displayMonths, {
        yAxisCallback: (value) => value / 1000000 + 'M'
    });

    if (appState.charts.performance) appState.charts.performance.destroy();
    appState.charts.performance = new Chart(ctx, config);
}

/**
 * Zoom Chart Function
 */
function zoomChart(direction) {
    if (direction === 'in') {
        if (appState.zoomLevel === 'normal') appState.zoomLevel = 'zoomed-in';
        else if (appState.zoomLevel === 'zoomed-out') appState.zoomLevel = 'normal';
    } else {
        if (appState.zoomLevel === 'normal') appState.zoomLevel = 'zoomed-out';
        else if (appState.zoomLevel === 'zoomed-in') appState.zoomLevel = 'normal';
    }
    renderPerformanceChart();
}


/**
 * Render Allocation Pie Chart
 */
function renderAllocationChart() {
    const ctx = document.getElementById('allocationChart');
    if (!ctx) return;

    const distribution = appState.portfolio.assets.reduce((acc, asset) => {
        const value = asset.quantity * asset.currentPrice;
        acc[asset.type] = (acc[asset.type] || 0) + value;
        return acc;
    }, {});

    const labels = Object.keys(distribution).map(type => translateAssetType(type));
    const data = Object.values(distribution);

    // Filter out zero values
    const filteredLabels = [];
    const filteredData = [];
    labels.forEach((label, index) => {
        if (data[index] > 0) {
            filteredLabels.push(label);
            filteredData.push(data[index]);
        }
    });

    const config = PieChartConfig.getDoughnutConfig(filteredData, filteredLabels, {
        cutout: '60%', // Adjusted for better proportions
        legendPosition: 'right',
        tooltipCallbacks: {
            label: function (context) {
                const value = context.raw;
                const total = context.chart._metasets[context.datasetIndex].total;
                const percentage = ((value / total) * 100).toFixed(1) + '%';
                return ` ${context.label}: ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)} (${percentage})`;
            }
        }
    });

    if (appState.charts.allocation) appState.charts.allocation.destroy();
    appState.charts.allocation = new Chart(ctx, config);

    // Create Legend (optional if ChartJS legend is enough, but user asked for detailed annotations)
    // For now ChartJS legend is good, but we can enhance if needed.
}

/**
 * Render Transaction Analysis Chart (Pie) - "Trading Phases" Visualization
 */
/**
 * Render Transaction Analysis Chart (Pie) - "Trading Phases" Visualization
 */
function renderTransactionChart(period) {
    const ctx = document.getElementById('transactionChart');
    if (!ctx) return;

    // Filter data based on period
    const now = new Date();
    const filteredData = appState.transactions.filter(t => {
        const diffTime = Math.abs(now - t.date);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        if (period === 'day') return diffDays <= 1;
        if (period === 'week') return diffDays <= 7;
        if (period === 'month') return diffDays <= 30;
        if (period === 'year') return diffDays <= 365;
        return true;
    });

    // Aggregate Buy vs Sell
    let buyTotal = 0;
    let sellTotal = 0;
    let buyCount = 0;
    let sellCount = 0;

    filteredData.forEach(t => {
        if (t.type === 'Buy') { buyTotal += t.value; buyCount++; }
        else if (t.type === 'Sell') { sellTotal += t.value; sellCount++; }
    });

    const totalVolume = buyTotal + sellTotal;
    const buyPercent = totalVolume > 0 ? ((buyTotal / totalVolume) * 100).toFixed(1) : 0;
    const sellPercent = totalVolume > 0 ? ((sellTotal / totalVolume) * 100).toFixed(1) : 0;

    const data = [buyTotal, sellTotal];
    const labels = ['Mua (Buy)', 'Bán (Sell)'];

    // Modern 2025 Palette - Teal & Rose
    const colors = ['#2dd4bf', '#fb7185'];
    const hoverColors = ['#14b8a6', '#f43f5e'];

    // Render Stats List
    const statsListEl = document.getElementById('transactionStatsList');
    if (statsListEl) {
        const formatMoney = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);

        statsListEl.innerHTML = `
            <div class="stats-row" id="stat-row-0">
                <div class="stats-info">
                    <div class="stats-dot" style="background-color: ${colors[0]}"></div>
                    <span class="stats-label">Mua (Buy)</span>
                </div>
                <div class="stats-values">
                    <span class="stats-amount">${formatMoney(buyTotal)}</span>
                    <span class="stats-percent">${buyPercent}% (${buyCount} lệnh)</span>
                </div>
            </div>
            <div class="stats-row" id="stat-row-1">
                <div class="stats-info">
                    <div class="stats-dot" style="background-color: ${colors[1]}"></div>
                    <span class="stats-label">Bán (Sell)</span>
                </div>
                <div class="stats-values">
                    <span class="stats-amount">${formatMoney(sellTotal)}</span>
                    <span class="stats-percent">${sellPercent}% (${sellCount} lệnh)</span>
                </div>
            </div>
            <div class="stats-row mt-2 border-top pt-2" style="background: transparent; box-shadow: none;">
                <div class="stats-info">
                    <span class="stats-label text-muted">Tổng Giao dịch</span>
                </div>
                <div class="stats-values">
                    <span class="stats-amount text-primary big">${formatMoney(totalVolume)}</span>
                    <span class="stats-percent text-muted">${buyCount + sellCount} lệnh</span>
                </div>
            </div>
        `;
    }

    const config = PieChartConfig.getDoughnutConfig(data, labels, {
        colors: colors,
        hoverColors: hoverColors,
        legendPosition: 'bottom', // We hide legend in chart config if we have custom list, but keeping it small is okay. 
        // Actually, let's hide the default legend since we have the side panel
        displayLegend: false,
        cutout: '75%',
        tooltipCallbacks: {
            label: function (context) {
                const value = context.raw;
                const total = context.chart._metasets[context.datasetIndex].total;
                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) + '%' : '0%';
                return ` ${context.label}: ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)} (${percentage})`;
            }
        }
    });

    // Override legend display if Config helper doesn't support 'displayLegend' param directly (assuming standard Chart.js options)
    if (!config.options.plugins) config.options.plugins = {};
    config.options.plugins.legend = { display: false };

    // Interactive Logic
    config.options.onHover = (e, elements, chart) => {
        // Reset all rows
        document.querySelectorAll('.stats-row').forEach(row => {
            row.style.background = 'white';
            row.style.transform = 'none';
        });

        if (elements && elements.length > 0) {
            const index = elements[0].index;
            const row = document.getElementById(`stat-row-${index}`);
            if (row) {
                row.style.background = 'var(--bg-secondary)';
                row.style.transform = 'translateX(4px)';
                row.style.borderColor = colors[index];
            }
            e.native.target.style.cursor = 'pointer';

            // Update hint text
            const detailsEl = document.getElementById('transactionChartDetails');
            if (detailsEl) {
                const type = index === 0 ? 'Tích lũy' : 'Chốt lời';
                detailsEl.innerHTML = `<p class="small text-primary fw-bold mb-0"><i class="fas fa-chart-pie me-1"></i> Đây là giao dịch ${type}</p>`;
            }
        } else {
            e.native.target.style.cursor = 'default';
            const detailsEl = document.getElementById('transactionChartDetails');
            if (detailsEl) detailsEl.innerHTML = '<p class="small text-muted mb-0"><i class="fas fa-mouse-pointer me-1"></i> Di chuột để xem chi tiết</p>';
        }
    };

    if (appState.charts.transaction) appState.charts.transaction.destroy();
    appState.charts.transaction = new Chart(ctx, config);
}

/**
 * Render Holdings Table
 */
function renderHoldingsTable() {
    const tbody = document.getElementById('holdingsTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    const assets = appState.portfolio.assets;

    assets.forEach(asset => {
        const currentValue = asset.quantity * asset.currentPrice;
        const totalCost = asset.quantity * asset.avgPrice;
        const profit = currentValue - totalCost;
        const profitPercent = totalCost > 0 ? ((profit / totalCost) * 100).toFixed(2) : 0;

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><span class="asset-symbol">${asset.symbol}</span></td>
            <td>
                <div class="asset-name-col">
                    <span class="asset-name">${asset.name}</span>
                </div>
            </td>
            <td><span class="badge badge-light">${translateAssetType(asset.type)}</span></td>
            <td class="text-right">${asset.quantity.toLocaleString('vi-VN')}</td>
            <td class="text-right">${asset.avgPrice.toLocaleString('vi-VN')} ₫</td>
            <td class="text-right">${asset.currentPrice.toLocaleString('vi-VN')} ₫</td>
            <td class="text-right font-weight-bold">${currentValue.toLocaleString('vi-VN')} ₫</td>
            <td class="text-right">
                <span class="${profit >= 0 ? 'text-success' : 'text-danger'}">
                    ${profit >= 0 ? '+' : ''}${profit.toLocaleString('vi-VN')} ₫
                    <small>(${profitPercent}%)</small>
                </span>
            </td>
            <td class="text-center">
                <button class="btn-icon-sm" onclick="editHolding(${asset.id})">
                    <i class="fas fa-edit"></i>
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

/**
 * Helper: Translate Asset Type
 */
function translateAssetType(type) {
    const map = {
        'Stock': 'Cổ phiếu',
        'Crypto': 'Crypto',
        'Bond': 'Trái phiếu',
        'Fund': 'Quỹ',
        'RealEstate': 'BĐS',
        'Commodity': 'Hàng hóa'
    };
    return map[type] || type;
}

/**
 * Download Chart as Image
 */
function downloadChart(chartId) {
    const canvas = document.getElementById(chartId);
    if (!canvas) return;

    // Create a temporary link
    const imageLink = document.createElement('a');
    imageLink.download = `${chartId}-${new Date().toISOString().slice(0, 10)}.png`;

    // Ensure white background
    const tempCanvas = document.createElement('canvas');
    tempCanvas.width = canvas.width;
    tempCanvas.height = canvas.height;
    const ctx = tempCanvas.getContext('2d');
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(0, 0, tempCanvas.width, tempCanvas.height);
    ctx.drawImage(canvas, 0, 0);

    imageLink.href = tempCanvas.toDataURL('image/png', 1.0);
    imageLink.click();
}

/**
 * Event Listeners
 */
function setupEventListeners() {
    // Time range selectors
    document.querySelectorAll('.range-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const btn = e.target.closest('.range-btn'); // Handles clicks on icon inside btn
            if (!btn) return;

            // If it's a zoom command, don't change 'active' class selection
            if (btn.onclick) return;

            const parent = btn.closest('.time-range-selector');
            if (!parent) return;

            // Handle different selector groups
            if (parent.id === 'transactionTimeFilter') {
                parent.querySelectorAll('.range-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                renderTransactionChart(btn.dataset.range);
            } else {
                // Main performance chart filters
                const prev = parent.querySelector('.range-btn.active');
                if (prev) prev.classList.remove('active');
                btn.classList.add('active');
                // Simulate fetching different data range here if needed
                console.log('Performance Range changed:', btn.dataset.range);
            }
        });
    });

    // Transaction Type Selector in Modal
    document.querySelectorAll('.type-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.type-btn').forEach(b => b.classList.remove('active'));
            const btn = e.target.closest('.type-btn');
            btn.classList.add('active');
            document.getElementById('transactionType').value = btn.dataset.type;
        });
    });
}

/**
 * Modal Actions
 */
function openInvestmentModal() {
    const modal = new bootstrap.Modal(document.getElementById('investmentModal'));
    modal.show();
}

/**
 * Save Investment Logic with Validation and Real-time Updates
 */
function saveInvestment() {
    try {
        const symbol = document.getElementById('symbol').value.toUpperCase();
        const quantity = parseFloat(document.getElementById('quantity').value);
        const price = parseFloat(document.getElementById('price').value);
        const type = document.getElementById('transactionType').value; // Buy or Sell
        const assetType = document.getElementById('assetType').value;
        const name = document.getElementById('assetName').value || symbol;
        const date = document.getElementById('transactionDate').value;
        const fee = parseFloat(document.getElementById('fee').value) || 0;

        // Validation
        if (!symbol || isNaN(quantity) || quantity <= 0 || isNaN(price) || price < 0 || !date) {
            alert('Vui lòng điền đầy đủ và chính xác thông tin bắt buộc.');
            return;
        }

        const totalValue = quantity * price;

        // 1. Add to Transactions List
        const newTransaction = {
            id: appState.transactions.length + 1,
            date: new Date(date),
            type: type,
            value: totalValue,
            symbol: symbol,
            assetType: assetType,
            quantity: quantity,
            price: price,
            fee: fee
        };
        appState.transactions.unshift(newTransaction);

        // 2. Update Portfolio Assets
        let asset = appState.portfolio.assets.find(a => a.symbol === symbol);

        if (type === 'Buy') {
            if (asset) {
                // Update existing asset
                const oldTotalCost = asset.quantity * asset.avgPrice;
                const newTotalCost = oldTotalCost + totalValue + fee;
                asset.quantity += quantity;
                asset.avgPrice = asset.quantity > 0 ? newTotalCost / asset.quantity : 0;
            } else {
                // New asset
                appState.portfolio.assets.push({
                    id: Date.now(),
                    symbol: symbol,
                    name: name,
                    type: assetType,
                    quantity: quantity,
                    avgPrice: price, // Initial price includes no history
                    currentPrice: price // Assume current price = buy price for now
                });
            }
            appState.portfolio.invested += totalValue;
        } else if (type === 'Sell') {
            if (asset) {
                if (asset.quantity < quantity) {
                    alert('Số lượng bán vượt quá số lượng sở hữu!');
                    return;
                }
                asset.quantity -= quantity;
                // Cost basis doesn't change on sell for weighted avg usually, but realized profit does.
                // Simple logic: Invested amount reduces by proportional cost
                const soldCost = quantity * asset.avgPrice;
                appState.portfolio.invested -= soldCost;

                // Remove if quantity is 0
                if (asset.quantity <= 0) {
                    appState.portfolio.assets = appState.portfolio.assets.filter(a => a.symbol !== symbol);
                }
            } else {
                alert('Bạn không sở hữu tài sản này để bán!');
                return;
            }
        }

        // 3. Update Total Value Simulation (Re-calc based on current prices)
        let newTotalValue = 0;
        appState.portfolio.assets.forEach(a => {
            newTotalValue += a.quantity * a.currentPrice;
        });
        appState.portfolio.totalValue = newTotalValue;
        appState.portfolio.profit = newTotalValue - appState.portfolio.invested; // Simplified

        // 4. Close Modal and Reset Form
        const modalEl = document.getElementById('investmentModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();
        document.getElementById('investmentForm').reset();

        // 5. Update UI (Real-time sync)
        updateLastUpdatedTime();
        renderSummaryStats();
        renderCharts(); // Updates both pie charts
        renderHoldingsTable();

        // 6. Success Feedback
        alert('Giao dịch đã được lưu thành công!');

    } catch (error) {
        console.error('Processing Error:', error);
        alert('Có lỗi xảy ra khi xử lý giao dịch: ' + error.message);
    }
}

function exportPortfolio() {
    alert('Đang xuất báo cáo danh mục đầu tư...');
}

function refreshNews() {
    const newsGrid = document.getElementById('newsGrid');
    if (newsGrid) {
        newsGrid.innerHTML = '<p class="text-muted text-center w-100 p-4">Đang cập nhật tin tức...</p>';
        setTimeout(() => {
            newsGrid.innerHTML = `
                <div class="news-card">
                    <div class="news-source">VNExpress</div>
                    <div class="news-title">Thị trường chứng khoán tiếp tục đà tăng trưởng mạnh mẽ</div>
                    <div class="news-time">Vừa xong</div>
                </div>
                <div class="news-card">
                    <div class="news-source">CafeF</div>
                    <div class="news-title">Bitcoin lập đỉnh mới, nhà đầu tư hưng phấn</div>
                    <div class="news-time">15 phút trước</div>
                </div>
                <div class="news-card">
                    <div class="news-source">Bloomberg</div>
                    <div class="news-title">Phân tích xu hướng dòng tiền khối ngoại</div>
                    <div class="news-time">1 giờ trước</div>
                </div>
            `;
            updateLastUpdatedTime();
        }, 1000);
    }
}
