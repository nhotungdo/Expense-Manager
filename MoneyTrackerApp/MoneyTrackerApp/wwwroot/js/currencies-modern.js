// Fintech Currency Management 2026 Logic
(function () {
    'use strict';

    let currencies = [];
    let fromCurrency = 'USD';
    let toCurrency = 'VND';
    const modal = new bootstrap.Modal(document.getElementById('currencyModal'));

    document.addEventListener('DOMContentLoaded', () => {
        loadCurrencies();
        initializeEventListeners();
    });

    async function loadCurrencies() {
        try {
            const response = await fetch('/api/currency?includeInactive=true');
            if (!response.ok) throw new Error('Failed to fetch');
            currencies = await response.json();
            
            renderCurrencyTable();
            renderMarketOverview();
            populateSelectors();
            updateLastUpdated();
            updateConversion();
        } catch (error) {
            console.error('Error loading currencies:', error);
            showNotification('error', 'Không thể tải danh sách tiền tệ');
        }
    }

    function renderCurrencyTable() {
        const tbody = document.getElementById('currencyTableBody');
        tbody.innerHTML = currencies.map(c => `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="https://flagcdn.com/w40/${c.flagUrl || 'un'}.png" class="flag-icon me-3">
                        <span class="fw-semibold">${c.name}</span>
                    </div>
                </td>
                <td><span class="badge bg-light text-dark font-monospace">${c.code}</span></td>
                <td>
                    <div class="fw-bold">${c.symbol} ${formatNumber(c.exchangeRate)}</div>
                    <small class="text-muted">${c.timeAgo}</small>
                </td>
                <td>
                    <span class="status-indicator ${c.isActive ? 'status-active' : 'status-inactive'}"></span>
                    <small>${c.isActive ? 'Hoạt động' : 'Tạm dừng'}</small>
                </td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-light" onclick="editCurrency(${c.id})"><i class="fas fa-edit"></i></button>
                        <button class="btn btn-light text-danger" onclick="deleteCurrency(${c.id})"><i class="fas fa-trash"></i></button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    function renderMarketOverview() {
        const container = document.getElementById('marketRatesList');
        const popular = ['USD', 'EUR', 'JPY', 'GBP', 'BTC', 'ETH'];
        const displayList = currencies.filter(c => popular.includes(c.code));

        container.innerHTML = displayList.map(c => {
            const isUp = Math.random() > 0.5; // Mocking trend for UI
            return `
                <div class="rate-item d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <img src="https://flagcdn.com/w40/${c.flagUrl || 'un'}.png" class="flag-icon me-3">
                        <div>
                            <div class="fw-bold">${c.code}/USD</div>
                            <small class="text-muted">${c.name}</small>
                        </div>
                    </div>
                    <div class="text-end">
                        <div class="fw-bold">${formatNumber(c.exchangeRate)}</div>
                        <span class="change-badge ${isUp ? 'change-up' : 'change-down'}">
                            <i class="fas fa-arrow-${isUp ? 'up' : 'down'} me-1"></i>${(Math.random() * 2).toFixed(2)}%
                        </span>
                    </div>
                </div>
            `;
        }).join('');
    }

    function populateSelectors() {
        const fromSelect = document.getElementById('fromCurrency');
        const toSelect = document.getElementById('toCurrency');
        
        const options = currencies.filter(c => c.isActive).map(c => 
            `<option value="${c.code}">${c.code} - ${c.name}</option>`
        ).join('');

        fromSelect.innerHTML = options;
        toSelect.innerHTML = options;

        fromSelect.value = fromCurrency;
        toSelect.value = toCurrency;
        
        updateFlags();
    }

    function initializeEventListeners() {
        document.getElementById('amountInput').addEventListener('input', updateConversion);
        document.getElementById('fromCurrency').addEventListener('change', (e) => {
            fromCurrency = e.target.value;
            updateFlags();
            updateConversion();
        });
        document.getElementById('toCurrency').addEventListener('change', (e) => {
            toCurrency = e.target.value;
            updateFlags();
            updateConversion();
        });
        document.getElementById('swapBtn').addEventListener('click', () => {
            const temp = fromCurrency;
            fromCurrency = toCurrency;
            toCurrency = temp;
            document.getElementById('fromCurrency').value = fromCurrency;
            document.getElementById('toCurrency').value = toCurrency;
            updateFlags();
            updateConversion();
        });
    }

    function updateFlags() {
        const from = currencies.find(c => c.code === fromCurrency);
        const to = currencies.find(c => c.code === toCurrency);
        if (from) document.getElementById('fromFlag').src = `https://flagcdn.com/w40/${from.flagUrl || 'un'}.png`;
        if (to) document.getElementById('toFlag').src = `https://flagcdn.com/w40/${to.flagUrl || 'un'}.png`;
    }

    function updateConversion() {
        const amount = parseFloat(document.getElementById('amountInput').value) || 0;
        const from = currencies.find(c => c.code === fromCurrency);
        const to = currencies.find(c => c.code === toCurrency);

        if (!from || !to) return;

        const rate = to.exchangeRate / from.exchangeRate;
        const result = amount * rate;

        document.getElementById('resultAmount').textContent = formatNumber(result, to.symbol);
        document.getElementById('exchangeRateDisplay').innerHTML = 
            `1 ${from.code} = <strong>${formatNumber(rate, to.symbol)}</strong> ${to.code}`;
    }

    async function syncRates() {
        const icon = document.getElementById('syncIcon');
        icon.classList.add('fa-spin');
        try {
            const response = await fetch('/api/currency/sync', { method: 'POST' });
            if (!response.ok) throw new Error('Sync failed');
            await loadCurrencies();
            showNotification('success', 'Đồng bộ tỷ giá thành công');
        } catch (error) {
            showNotification('error', 'Lỗi đồng bộ: ' + error.message);
        } finally {
            icon.classList.remove('fa-spin');
        }
    }

    window.showAddModal = () => {
        document.getElementById('modalTitle').textContent = 'Thêm tiền tệ mới';
        document.getElementById('currencyForm').reset();
        document.getElementById('currencyId').value = '';
        modal.show();
    };

    window.editCurrency = (id) => {
        const c = currencies.find(x => x.id === id);
        if (!c) return;
        document.getElementById('modalTitle').textContent = 'Chỉnh sửa ' + c.code;
        document.getElementById('currencyId').value = c.id;
        document.getElementById('name').value = c.name;
        document.getElementById('code').value = c.code;
        document.getElementById('symbol').value = c.symbol;
        document.getElementById('rate').value = c.exchangeRate;
        document.getElementById('flag').value = c.flagUrl;
        modal.show();
    };

    window.saveCurrency = async () => {
        const id = document.getElementById('currencyId').value;
        const data = {
            id: id ? parseInt(id) : 0,
            name: document.getElementById('name').value,
            code: document.getElementById('code').value,
            symbol: document.getElementById('symbol').value,
            exchangeRate: parseFloat(document.getElementById('rate').value),
            flagUrl: document.getElementById('flag').value,
            country: document.getElementById('name').value // Simplification
        };

        try {
            const method = id ? 'PUT' : 'POST';
            const response = await fetch('/api/currency', {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            if (!response.ok) throw new Error('Save failed');
            modal.hide();
            await loadCurrencies();
            showNotification('success', 'Đã lưu thay đổi');
        } catch (error) {
            showNotification('error', 'Lỗi khi lưu: ' + error.message);
        }
    };

    window.deleteCurrency = async (id) => {
        if (!confirm('Bạn có chắc muốn xóa tiền tệ này?')) return;
        try {
            const response = await fetch(`/api/currency/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Delete failed');
            await loadCurrencies();
            showNotification('success', 'Đã xóa tiền tệ');
        } catch (error) {
            showNotification('error', 'Lỗi khi xóa: ' + error.message);
        }
    };

    window.syncRates = syncRates;

    function updateLastUpdated() {
        const now = new Date();
        document.getElementById('lastUpdated').textContent = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function formatNumber(num, symbol = '') {
        let decimals = 2;
        if (num < 0.1) decimals = 6;
        else if (num < 1) decimals = 4;
        
        const formatted = new Intl.NumberFormat('vi-VN', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(num);
        
        return symbol ? `${symbol} ${formatted}` : formatted;
    }

    function showNotification(type, message) {
        // Placeholder for toast notification
        alert(message);
    }

})();
