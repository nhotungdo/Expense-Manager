// Modern Currency Page Logic

// Mock Exchange Rates (Base: USD)
const exchangeRates = {
    USD: { rate: 1, name: "US Dollar", flag: "us" },
    VND: { rate: 25480, name: "Vietnamese Dong", flag: "vn" },
    EUR: { rate: 0.92, name: "Euro", flag: "eu" },
    JPY: { rate: 151.4, name: "Japanese Yen", flag: "jp" },
    GBP: { rate: 0.79, name: "British Pound", flag: "gb" },
    AUD: { rate: 1.53, name: "Australian Dollar", flag: "au" },
    CAD: { rate: 1.36, name: "Canadian Dollar", flag: "ca" },
    CHF: { rate: 0.90, name: "Swiss Franc", flag: "ch" },
    CNY: { rate: 7.23, name: "Chinese Yuan", flag: "cn" },
    SGD: { rate: 1.35, name: "Singapore Dollar", flag: "sg" }
};

// State
let fromCurrency = 'USD';
let toCurrency = 'VND';
let chartInstance = null;

document.addEventListener('DOMContentLoaded', function () {
    initializeControls();
    updateConversion();
    renderChart();
    renderLiveRates();

    // Add animate class to elements on load
    const elements = document.querySelectorAll('.glass-card');
    elements.forEach((el, index) => {
        el.style.opacity = '0';
        el.style.animation = `fadeInUp 0.5s ease forwards ${index * 0.1}s`;
    });
});

// Add keyframes for animation via JS or use CSS
const styleSheet = document.createElement("style");
styleSheet.innerText = `
@keyframes fadeInUp {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}`;
document.head.appendChild(styleSheet);


function initializeControls() {
    const amountInput = document.getElementById('amountInput');
    const fromSelect = document.getElementById('fromCurrency');
    const toSelect = document.getElementById('toCurrency');
    const swapBtn = document.getElementById('swapBtn');

    // Populate selects
    [fromSelect, toSelect].forEach(select => {
        Object.keys(exchangeRates).forEach(code => {
            const option = document.createElement('option');
            option.value = code;
            option.text = `${code} - ${exchangeRates[code].name}`;
            select.appendChild(option);
        });
    });

    fromSelect.value = fromCurrency;
    toSelect.value = toCurrency;

    // Event Listeners
    amountInput.addEventListener('input', updateConversion);
    fromSelect.addEventListener('change', (e) => {
        fromCurrency = e.target.value;
        updateFlag('from', fromCurrency);
        updateConversion();
        refreshChart();
    });
    toSelect.addEventListener('change', (e) => {
        toCurrency = e.target.value;
        updateFlag('to', toCurrency);
        updateConversion();
        refreshChart();
    });

    swapBtn.addEventListener('click', () => {
        // Visualize swap
        const temp = fromCurrency;
        fromCurrency = toCurrency;
        toCurrency = temp;

        fromSelect.value = fromCurrency;
        toSelect.value = toCurrency;

        updateFlag('from', fromCurrency);
        updateFlag('to', toCurrency);
        updateConversion();
        refreshChart();
    });

    // Initial flags
    updateFlag('from', fromCurrency);
    updateFlag('to', toCurrency);
}

function updateFlag(type, currencyCode) {
    const imgId = type === 'from' ? 'fromFlag' : 'toFlag';
    const img = document.getElementById(imgId);
    if (img && exchangeRates[currencyCode]) {
        img.src = `https://flagcdn.com/w40/${exchangeRates[currencyCode].flag}.png`;
    }
}

function updateConversion() {
    const amount = parseFloat(document.getElementById('amountInput').value) || 0;
    const rateFrom = exchangeRates[fromCurrency].rate;
    const rateTo = exchangeRates[toCurrency].rate;

    // Calculate Cross Rate: (Amount / RateFromBase) * RateToBase
    const result = (amount / rateFrom) * rateTo;

    // Determine decimals based on result size
    let decimals = 2;
    if (result > 0 && result < 0.01) decimals = 6;
    else if (result > 0 && result < 1) decimals = 4;

    document.getElementById('resultAmount').textContent = formatCurrency(result, toCurrency, decimals);

    // For single unit rate display
    const singleUnitRate = rateTo / rateFrom;
    let rateDecimals = 4;
    if (singleUnitRate > 0 && singleUnitRate < 0.0001) rateDecimals = 8;
    else if (singleUnitRate > 0 && singleUnitRate < 0.01) rateDecimals = 6;

    document.getElementById('exchangeRateDisplay').textContent =
        `1 ${fromCurrency} ≈ ${formatCurrency(singleUnitRate, toCurrency, rateDecimals)}`;
}

function formatCurrency(value, currency, decimals = 2) {
    return new Intl.NumberFormat('en-US', {
        style: 'decimal',
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(value);
}

function renderLiveRates() {
    const container = document.getElementById('liveRatesList');
    const base = 'VN'; // User's likely locale or preference

    // Mock data for some popular pairs against VND
    const pairs = [
        { code: 'USD', change: 0.15 },
        { code: 'EUR', change: -0.05 },
        { code: 'JPY', change: 0.22 },
        { code: 'GBP', change: 0.08 },
        { code: 'AUD', change: -0.12 }
    ];

    container.innerHTML = '';
    pairs.forEach(pair => {
        const rate = exchangeRates['VND'].rate / exchangeRates[pair.code].rate;
        const isUp = pair.change >= 0;

        const html = `
            <div class="rate-item d-flex justify-content-between align-items-center">
                <div class="d-flex align-items-center">
                    <img src="https://flagcdn.com/w40/${exchangeRates[pair.code].flag}.png" class="flag-icon me-3">
                    <div>
                        <div class="fw-bold text-dark">${pair.code}/VND</div>
                        <small class="text-muted">${exchangeRates[pair.code].name}</small>
                    </div>
                </div>
                <div class="text-end">
                    <div class="fw-bold text-dark">${formatCurrency(rate, 'VND', 0)}</div>
                    <small class="change-badge ${isUp ? 'change-up' : 'change-down'}">
                        <i class="fas fa-arrow-${isUp ? 'up' : 'down'} me-1"></i>${Math.abs(pair.change)}%
                    </small>
                </div>
            </div>
        `;
        container.innerHTML += html;
    });
}

function renderChart() {
    const options = {
        series: [{
            name: `${fromCurrency}/${toCurrency}`,
            data: generateMockHistoryData(14)
        }],
        chart: {
            type: 'area',
            height: 350,
            toolbar: { show: false },
            fontFamily: 'Inter, sans-serif'
        },
        colors: ['#4f46e5'],
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.7,
                opacityTo: 0.2,
                stops: [0, 90, 100]
            }
        },
        dataLabels: { enabled: false },
        stroke: {
            curve: 'smooth',
            width: 3
        },
        xaxis: {
            type: 'datetime',
            categories: getLast7Days(),
            axisBorder: { show: false },
            axisTicks: { show: false }
        },
        yaxis: {
            show: true,
            labels: {
                formatter: (val) => {
                    if (val < 1) return val.toFixed(6);
                    return val.toFixed(2);
                }
            }
        },
        grid: {
            borderColor: '#f1f1f1',
            strokeDashArray: 4
        },
        tooltip: {
            theme: 'light',
            y: {
                formatter: function (val) {
                    return val + " " + toCurrency
                }
            }
        }
    };

    const chartEl = document.querySelector("#currencyChart");
    if (chartEl) {
        chartInstance = new ApexCharts(chartEl, options);
        chartInstance.render();
    }
}

function refreshChart() {
    if (!chartInstance) return;

    // Simulate fetching new data
    const newData = generateMockHistoryData(14);
    chartInstance.updateSeries([{
        name: `${fromCurrency}/${toCurrency}`,
        data: newData
    }]);
}

function generateMockHistoryData(days) {
    const rateFrom = exchangeRates[fromCurrency].rate;
    const rateTo = exchangeRates[toCurrency].rate;
    const baseRate = (1 / rateFrom) * rateTo;

    const data = [];
    let current = baseRate;

    for (let i = 0; i < days; i++) {
        // Random fluctuation +/- 1%
        const change = current * (Math.random() * 0.02 - 0.01);
        current += change;
        data.push(current);
    }
    return data;
}

function getLast7Days() {
    const dates = [];
    for (let i = 13; i >= 0; i--) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        dates.push(d.toISOString());
    }
    return dates;
}
