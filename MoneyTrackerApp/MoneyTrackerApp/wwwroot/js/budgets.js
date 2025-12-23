document.addEventListener('DOMContentLoaded', function () {
    initCharts();
});

function initCharts() {
    const dataContainer = document.getElementById('budgetData');
    if (!dataContainer) return;

    // Data Parsing
    const categoryData = JSON.parse(dataContainer.dataset.categoryJson || '[]');
    const dailyData = JSON.parse(dataContainer.dataset.dailyJson || '[]');
    const totalBudget = parseFloat(dataContainer.dataset.totalBudget) || 0;

    // 1. Render Trend Chart
    renderTrendChart(dailyData, totalBudget);

    // 2. Render Category Chart
    renderCategoryChart(categoryData);
}

function renderTrendChart(dailyData, totalBudget) {
    const ctx = document.getElementById('trendChart');
    if (!ctx) return;

    if (dailyData.length === 0) {
        // Handle empty state if needed
    }

    const labels = dailyData.map(d => d.date);
    const dataPoints = dailyData.map(d => d.cumulative);

    // Calculate Average Line (Linear budget usage)
    // Start at 0, End at Budget (or current day projection)
    // For visual simplicity, let's just draw a line from (0,0) to (LastDay, Budget)
    // But since labels are dates, we create a dataset that matches simple linear progression.
    const averageLine = [];
    const daysInMonth = 30; // Approx
    const dailyBudget = totalBudget / daysInMonth;

    // We only map for the days we have data for
    dataPoints.forEach((_, index) => {
        averageLine.push(dailyBudget * (index + 1));
    });

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Thực tế',
                    data: dataPoints,
                    borderColor: '#6366f1', // Indigo 500
                    backgroundColor: 'rgba(99, 102, 241, 0.1)',
                    borderWidth: 3,
                    tension: 0.4,
                    fill: true,
                    pointRadius: 4,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: '#6366f1',
                    pointBorderWidth: 2
                },
                {
                    label: 'NS Trung bình',
                    data: averageLine,
                    borderColor: '#fb7185', // Rose 400
                    borderWidth: 2,
                    borderDash: [5, 5],
                    tension: 0,
                    pointRadius: 0,
                    fill: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + ': ' + formatCurrency(context.parsed.y);
                        }
                    },
                    backgroundColor: 'rgba(255, 255, 255, 0.9)',
                    titleColor: '#1e293b',
                    bodyColor: '#475569',
                    borderColor: '#e2e8f0',
                    borderWidth: 1,
                    padding: 10,
                    displayColors: true,
                    boxWidth: 8,
                    boxHeight: 8,
                    usePointStyle: true,
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: '#f1f5f9' }, // Light gray
                    ticks: {
                        callback: function (value) { return value >= 1000000 ? (value / 1000000) + 'M' : (value / 1000) + 'k'; },
                        color: '#94a3b8',
                        font: { size: 11 }
                    }
                },
                x: {
                    grid: { display: false },
                    ticks: {
                        color: '#94a3b8',
                        font: { size: 11 }
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        }
    });
}

function renderCategoryChart(categoryData) {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;

    if (categoryData.length === 0) {
        // Draw empty chart
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Chưa có dữ liệu'],
                datasets: [{ data: [1], backgroundColor: ['#f1f5f9'], borderWidth: 0 }]
            },
            options: { cutout: '75%', plugins: { tooltip: { enabled: false }, legend: { display: false } } }
        });
        return;
    }

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: categoryData.map(c => c.name),
            datasets: [{
                data: categoryData.map(c => c.amount),
                backgroundColor: categoryData.map(c => c.color || '#cbd5e1'),
                borderWidth: 2,
                borderColor: '#ffffff',
                hoverOffset: 4
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
                    titleColor: '#1e293b',
                    bodyColor: '#475569',
                    borderColor: '#e2e8f0',
                    borderWidth: 1,
                    padding: 12,
                    callbacks: {
                        label: function (context) {
                            return ' ' + context.label + ': ' + formatCurrency(context.parsed);
                        }
                    }
                }
            }
        }
    });
}

function setQuickBudget(amount) {
    const input = document.getElementById('budgetInput');
    if (input) {
        input.value = amount;
        // Optional: highlight input to show it changed
        input.classList.add('ring-2', 'ring-indigo-500');
        setTimeout(() => input.classList.remove('ring-2', 'ring-indigo-500'), 500);
    }
}

async function updateBudget() {
    const budgetInput = document.getElementById('budgetInput');
    const btn = document.getElementById('btnUpdateBudget');

    if (!budgetInput || !btn) return;

    const newAmount = parseFloat(budgetInput.value);
    if (isNaN(newAmount) || newAmount < 0) {
        alert("Vui lòng nhập số tiền hợp lệ");
        return;
    }

    const originalHTML = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
    btn.disabled = true;
    btn.classList.add('opacity-75', 'cursor-not-allowed');

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('?handler=UpdateBudget', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ amount: newAmount })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                btn.innerHTML = '<i class="fas fa-check"></i> Đã cập nhật';
                btn.classList.remove('from-indigo-600', 'to-violet-600');
                btn.classList.add('bg-emerald-500');

                setTimeout(() => {
                    location.reload(); // Reload to refresh charts and progress
                }, 1000);
            } else {
                alert('Lỗi: ' + (result.message || 'Không thể cập nhật'));
                resetBtn(btn, originalHTML);
            }
        } else {
            alert('Lỗi kết nối');
            resetBtn(btn, originalHTML);
        }
    } catch (error) {
        console.error(error);
        alert('Đã xảy ra lỗi.');
        resetBtn(btn, originalHTML);
    }
}

function resetBtn(btn, html) {
    btn.innerHTML = html;
    btn.disabled = false;
    btn.classList.remove('opacity-75', 'cursor-not-allowed', 'bg-emerald-500');
    btn.classList.add('from-indigo-600', 'to-violet-600');
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}
