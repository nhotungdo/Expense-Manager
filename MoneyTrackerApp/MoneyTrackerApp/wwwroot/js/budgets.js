// Budget Management Page Scripts

document.addEventListener('DOMContentLoaded', function () {
    // 1. Initialize logic
    initBudgetUI();

    // 2. Initialize Charts
    renderCostBreakdownChart();

    // 3. Attach Event Listeners
    attachEventListeners();
});

function initBudgetUI() {
    // Read server-side data
    const dataContainer = document.getElementById('budgetData');
    if (!dataContainer) return;

    const limit = parseFloat(dataContainer.dataset.limit) || 0;
    const spent = parseFloat(dataContainer.dataset.spent) || 0;

    updateProgressUI(spent, limit);
}

function updateProgressUI(spent, limit) {
    const lblStatus = document.getElementById('lblPercentageStatus');
    const progressBar = document.getElementById('mainProgressBar');
    const txtLimit = document.getElementById('txtLimitAmount');

    // Update text
    if (txtLimit) {
        txtLimit.innerText = formatCurrency(limit);
    }

    if (progressBar && lblStatus) {
        let percentage = 0;
        if (limit > 0) {
            percentage = (spent / limit) * 100;
        } else if (spent > 0) {
            percentage = 100; // Spent something but budget is 0
        }

        const width = Math.min(percentage, 100);
        progressBar.style.width = width + '%';
        progressBar.setAttribute('aria-valuenow', percentage);

        // Reset classes
        progressBar.className = 'progress-bar progress-bar-striped progress-bar-animated';

        if (percentage >= 100) {
            progressBar.classList.add('bg-danger');
            lblStatus.innerHTML = '<span class="text-danger fw-bold">' + percentage.toFixed(0) + '% (Vượt ngân sách!)</span>';
        } else if (percentage >= 80) {
            progressBar.classList.add('bg-warning'); // Actually Bootstrap warning is yellow/orange
            // Let's use danger for high warning, or custom class. Bootstrap warning is fine.
            progressBar.classList.add('bg-danger');
            lblStatus.innerHTML = '<span class="text-danger fw-bold">' + percentage.toFixed(0) + '% (Nguy hiểm)</span>';
        } else if (percentage >= 50) {
            progressBar.classList.add('bg-warning');
            lblStatus.innerHTML = '<span class="text-warning fw-bold">' + percentage.toFixed(0) + '% (Cần chú ý)</span>';
        } else {
            progressBar.classList.add('bg-success');
            lblStatus.innerHTML = '<span class="text-success fw-bold">' + percentage.toFixed(0) + '% (An toàn)</span>';
        }
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

    // Loading state
    const originalText = btn.innerHTML;
    btn.innerHTML = '<div class="spinner-border spinner-border-sm me-2"></div>Đang lưu...';
    btn.disabled = true;

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const response = await fetch('?handler=UpdateBudget', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                amount: newAmount,
                capType: 'soft' // Default for now
            })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                // Success feedback
                btn.innerHTML = '<i class="fas fa-check me-2"></i>Đã cập nhật';
                btn.classList.replace('btn-primary', 'btn-success');

                // Update UI immediately (optimistic + actual)
                const dataContainer = document.getElementById('budgetData');
                const currentSpent = parseFloat(dataContainer.dataset.spent) || 0;
                updateProgressUI(currentSpent, result.newLimit);

                // Update data-limit for subsequent interactions
                dataContainer.dataset.limit = result.newLimit;

                setTimeout(() => {
                    btn.innerHTML = originalText;
                    btn.classList.replace('btn-success', 'btn-primary');
                    btn.disabled = false;
                }, 1500);
            } else {
                alert('Lỗi: ' + (result.message || 'Không thể cập nhật'));
                resetBtn(btn, originalText);
            }
        } else {
            alert('Lỗi kết nối máy chủ');
            resetBtn(btn, originalText);
        }
    } catch (error) {
        console.error(error);
        alert('Đã xảy ra lỗi khi cập nhật.');
        resetBtn(btn, originalText);
    }
}

function resetBtn(btn, text) {
    btn.innerHTML = text;
    btn.disabled = false;
}

function renderCostBreakdownChart() {
    const ctx = document.getElementById('costBreakdownChart');
    if (!ctx) return;

    const dataContainer = document.getElementById('budgetData');
    // Default values if 0 simply to show chart structure, or 0
    const api = parseFloat(dataContainer?.dataset.api) || 0;
    const pro = parseFloat(dataContainer?.dataset.pro) || 0;
    const storage = parseFloat(dataContainer?.dataset.storage) || 0;
    const other = parseFloat(dataContainer?.dataset.other) || 0;

    // Check if total is 0
    const total = api + pro + storage + other;
    let data = [1, 1, 1, 1]; // Placeholder
    let bgColors = ['#e2e8f0', '#e2e8f0', '#e2e8f0', '#e2e8f0'];

    if (total > 0) {
        data = [api, pro, storage, other];
        bgColors = ['#3b82f6', '#10b981', '#f59e0b', '#64748b'];
    }

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['OpenAI API', 'Hosting Pro', 'Storage', 'Khác'],
            datasets: [{
                data: data,
                backgroundColor: bgColors,
                borderWidth: 2,
                borderColor: '#ffffff',
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '75%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            if (total === 0) return "Chưa có dữ liệu";
                            return context.label + ': ' + formatCurrency(context.parsed);
                        }
                    }
                }
            }
        }
    });
}

function attachEventListeners() {
    const budgetInput = document.getElementById('budgetInput');
    if (budgetInput) {
        budgetInput.addEventListener('change', function () {
            // Optional: simulate on change before saving? 
            // Better not to confuse user. Only update on Save.
        });
    }
}

function setQuickBudget(amount) {
    const input = document.getElementById('budgetInput');
    if (input) {
        input.value = amount;
    }
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}
