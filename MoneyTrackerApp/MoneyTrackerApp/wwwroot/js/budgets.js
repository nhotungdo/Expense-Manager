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

// -------------------------------------------------------------
// AI Analysis Modal Logic
// -------------------------------------------------------------
function openAiAnalysisModal() {
    const el = document.getElementById('aiAnalysisModal');
    if (!el) return;

    const modal = new bootstrap.Modal(el);
    modal.show();

    // Fetch data
    const dataContainer = document.getElementById('budgetData');
    const totalBudget = parseFloat(dataContainer.dataset.totalBudget) || 0;
    const totalSpent = parseFloat(dataContainer.dataset.totalSpent) || 0;
    const projectedSpent = parseFloat(dataContainer.dataset.projectedSpent) || 0;
    const percentage = parseFloat(dataContainer.dataset.percentage) || 0;
    const categoryData = JSON.parse(dataContainer.dataset.categoryJson || '[]');

    const contentDiv = document.getElementById('aiAnalysisContent');

    // Generate Content
    let statusHTML = '';
    let categoryHTML = '';
    let projectionHTML = '';

    // 1. Status Analysis
    if (percentage > 100) {
        statusHTML = `
            <div class="bg-rose-50 border border-rose-100 rounded-xl p-4 flex gap-4">
                <div class="shrink-0 w-10 h-10 rounded-full bg-rose-100 flex items-center justify-center text-rose-600">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <div>
                    <h6 class="font-bold text-rose-800">Cảnh báo nghiêm trọng</h6>
                    <p class="text-sm text-rose-600">Bạn đã vượt quá ngân sách <strong>${(percentage - 100).toFixed(1)}%</strong>.</p>
                </div>
            </div>`;
    } else if (percentage > 85) {
        statusHTML = `
            <div class="bg-amber-50 border border-amber-100 rounded-xl p-4 flex gap-4">
                <div class="shrink-0 w-10 h-10 rounded-full bg-amber-100 flex items-center justify-center text-amber-600">
                    <i class="fas fa-exclamation-circle"></i>
                </div>
                <div>
                    <h6 class="font-bold text-amber-800">Cảnh báo hạn mức</h6>
                    <p class="text-sm text-amber-600">Bạn sắp đạt giới hạn ngân sách. Hãy cẩn trọng.</p>
                </div>
            </div>`;
    } else {
        statusHTML = `
            <div class="bg-emerald-50 border border-emerald-100 rounded-xl p-4 flex gap-4">
                <div class="shrink-0 w-10 h-10 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-600">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div>
                    <h6 class="font-bold text-emerald-800">Trạng thái Tốt</h6>
                    <p class="text-sm text-emerald-600">Chi tiêu của bạn đang trong tầm kiểm soát.</p>
                </div>
            </div>`;
    }

    // 2. Category Insights
    if (categoryData.length > 0) {
        const topCategory = categoryData[0]; // Assumes sorted by amount descending
        categoryHTML = `
            <div class="bg-white border border-slate-100 rounded-xl p-4 shadow-sm">
                <h6 class="font-bold text-slate-700 mb-2 text-sm uppercase tracking-wide">Chi tiêu nhiều nhất</h6>
                <div class="flex justify-between items-center">
                    <div class="flex items-center gap-3">
                        <div class="w-8 h-8 rounded-full flex items-center justify-center text-white" style="background-color: ${topCategory.color}">
                            <i class="${topCategory.icon || 'fas fa-tag'} text-xs"></i>
                        </div>
                        <span class="font-medium text-slate-800">${topCategory.name}</span>
                    </div>
                    <span class="font-bold text-slate-900">${formatCurrency(topCategory.amount)}</span>
                </div>
                <p class="text-xs text-slate-500 mt-2">Chiếm <strong>${((topCategory.amount / totalSpent) * 100).toFixed(1)}%</strong> tổng chi tiêu.</p>
            </div>
        `;
    }

    // 3. Projection
    const remaining = totalBudget - totalSpent;
    const projectedOver = projectedSpent - totalBudget;

    // Calculate daily average
    const today = new Date().getDate();
    const dailyAvg = totalSpent / Math.max(today, 1);

    let adviceText = '';
    if (projectedOver > 0) {
        adviceText = `Dựa trên mức chi tiêu trung bình <strong>${formatCurrency(dailyAvg)}/ngày</strong>, bạn dự kiến sẽ vượt ngân sách khoảng <strong>${formatCurrency(projectedOver)}</strong>. Hãy cắt giảm chi tiêu không cần thiết ngay bây giờ.`;
    } else {
        adviceText = `Bạn đang duy trì mức chi tiêu tốt (TB <strong>${formatCurrency(dailyAvg)}/ngày</strong>). Dự kiến bạn sẽ dư khoảng <strong>${formatCurrency(totalBudget - projectedSpent)}</strong> vào cuối tháng.`;
    }

    projectionHTML = `
        <div class="bg-indigo-50/50 border border-indigo-100 rounded-xl p-4">
            <h6 class="font-bold text-indigo-900 mb-2 text-sm uppercase tracking-wide">Dự báo & Lời khuyên</h6>
            <p class="text-sm text-indigo-800 leading-relaxed text-justify">
                ${adviceText}
            </p>
        </div>
    `;

    // Combine
    contentDiv.innerHTML = statusHTML + categoryHTML + projectionHTML;
}
