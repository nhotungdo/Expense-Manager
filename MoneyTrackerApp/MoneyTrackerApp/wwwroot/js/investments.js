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
            {
                id: 1,
                symbol: 'AAPL',
                name: 'Apple Inc.',
                type: 'Stock',
                quantity: 15,
                avgPrice: 3500000,
                currentPrice: 4200000
            },
            {
                id: 2,
                symbol: 'BTC',
                name: 'Bitcoin',
                type: 'Crypto',
                quantity: 0.05,
                avgPrice: 1000000000,
                currentPrice: 1500000000
            },
            {
                id: 3,
                symbol: 'VND',
                name: 'VNDirect',
                type: 'Stock',
                quantity: 1000,
                avgPrice: 20000,
                currentPrice: 22500
            },
            {
                id: 4,
                symbol: 'TCB',
                name: 'Techcombank bond',
                type: 'Bond',
                quantity: 50,
                avgPrice: 100000,
                currentPrice: 105000
            }
        ]
    }
};

/**
 * Initialize all components
 */
function initInvestments() {
    renderSummaryStats();
    renderCharts();
    renderHoldingsTable();
    setupEventListeners();
}

/**
 * Render Summary Statistics
 */
function renderSummaryStats() {
    const { totalValue, invested, profit, dividend, assets } = appState.portfolio;

    // Helper to format currency
    const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);

    // Update Top Cards
    document.getElementById('totalValue').innerText = formatCurrency(totalValue);

    const change = totalValue - invested;
    const changePercent = ((change / invested) * 100).toFixed(2);
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
}

/**
 * Render Performance Line Chart
 */
function renderPerformanceChart() {
    const ctx = document.getElementById('performanceChart');
    if (!ctx) return;

    // Mock Data for Performance
    const months = ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6'];
    const dataPoints = [180000000, 195000000, 190000000, 210000000, 225000000, 245000000];

    const datasets = [{
        label: 'Giá trị danh mục',
        data: dataPoints,
        color: ChartColors.primary.main,
        backgroundColor: ChartColors.primary.alpha(0.1),
        fill: true
    }];

    const config = LineChartConfig.getConfig(datasets, months, {
        yAxisCallback: (value) => value / 1000000 + 'M'
    });

    new Chart(ctx, config);
}

/**
 * Render Allocation Pie Chart
 */
function renderAllocationChart() {
    const ctx = document.getElementById('allocationChart');
    if (!ctx) return;

    // Calculate distribution by type
    const distribution = appState.portfolio.assets.reduce((acc, asset) => {
        const value = asset.quantity * asset.currentPrice;
        acc[asset.type] = (acc[asset.type] || 0) + value;
        return acc;
    }, {});

    const labels = Object.keys(distribution).map(type => translateAssetType(type));
    const data = Object.values(distribution);

    const config = PieChartConfig.getDoughnutConfig(data, labels, {
        cutout: '70%',
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

    // Custom Legend Rendering if needed, but Chart.js handles it well with the config
    new Chart(ctx, config);
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
        const profitPercent = ((profit / totalCost) * 100).toFixed(2);

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
 * Event Listeners
 */
function setupEventListeners() {
    // Time range selectors
    document.querySelectorAll('.range-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.range-btn').forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');
            // Here you would fetch new data for the selected range
            console.log('Range changed:', e.target.dataset.range);
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

function saveInvestment() {
    // Logic to save investment would go here
    // For now just close the modal and alert
    const symbol = document.getElementById('symbol').value;
    const quantity = document.getElementById('quantity').value;

    if (!symbol || !quantity) {
        alert('Vui lòng điền đầy đủ thông tin');
        return;
    }

    alert('Đã lưu giao dịch thành công!');
    const modal = bootstrap.Modal.getInstance(document.getElementById('investmentModal'));
    modal.hide();
}

function exportPortfolio() {
    alert('Đang xuất báo cáo...');
}

function refreshNews() {
    // Mock news refresh
    const newsGrid = document.getElementById('newsGrid');
    if (newsGrid) {
        newsGrid.innerHTML = '<p class="text-muted text-center w-100 p-4">Đang cập nhật tin tức...</p>';
        setTimeout(() => {
            newsGrid.innerHTML = `
                <div class="news-card">
                    <div class="news-source">VNExpress</div>
                    <div class="news-title">Thị trường chứng khoán tiếp tục đà tăng trưởng</div>
                    <div class="news-time">2 giờ trước</div>
                </div>
                <div class="news-card">
                    <div class="news-source">CafeF</div>
                    <div class="news-title">Bitcoin vượt mốc 60.000 USD</div>
                    <div class="news-time">5 giờ trước</div>
                </div>
            `;
        }, 1000);
    }
}

// Initial news load
refreshNews();
