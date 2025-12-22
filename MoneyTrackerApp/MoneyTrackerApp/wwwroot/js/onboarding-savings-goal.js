document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('savingsGoalForm');
    const errorMessage = document.getElementById('errorMessage');
    const btnSkip = document.getElementById('btnSkip');
    const colorInput = document.getElementById('goalColor');
    const calculateCard = document.getElementById('calculationCard');
    const monthlyAmountText = document.getElementById('monthlyAmount');

    // Color Selection
    const colorOptions = document.querySelectorAll('.color-option');
    colorOptions.forEach(option => {
        option.addEventListener('click', () => {
            // Remove checkmark from all
            colorOptions.forEach(opt => opt.innerHTML = '');
            // Add checkmark to clicked
            option.innerHTML = '<span class="check-mark">✓</span>';
            // Update hidden input
            if (colorInput) colorInput.value = option.dataset.color;
        });
    });

    // Logic to calculate savings
    const targetAmountInput = document.getElementById('targetAmount');
    const targetDateInput = document.getElementById('targetDate');

    async function calculateMonthly() {
        if (!targetAmountInput.value || !targetDateInput.value) return;

        try {
            const amount = parseFloat(targetAmountInput.value);
            const date = targetDateInput.value;

            const response = await fetch('/api/Onboarding/calculate-savings', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    targetAmount: amount,
                    targetDate: date
                })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.monthlyAmount > 0) {
                    calculateCard.classList.remove('hidden');
                    // Format currency nicely
                    const formatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });
                    monthlyAmountText.textContent = formatter.format(data.monthlyAmount);
                }
            }
        } catch (error) {
            console.error(error);
        }
    }

    if (targetAmountInput) targetAmountInput.addEventListener('change', calculateMonthly);
    if (targetDateInput) targetDateInput.addEventListener('change', calculateMonthly);

    // Skip Button
    if (btnSkip) {
        btnSkip.addEventListener('click', function () {
            finishOnboarding(null);
        });
    }

    // Submit Form
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        const formData = new FormData(form);

        const goalData = {
            name: formData.get('goalName'),
            targetAmount: parseFloat(formData.get('targetAmount')),
            targetDate: formData.get('targetDate'),
            color: formData.get('goalColor'),
            icon: '🎯'
        };

        finishOnboarding(goalData);
    });

    async function finishOnboarding(goalData) {
        const submitBtn = form.querySelector('button[type="submit"]');
        const originalText = submitBtn.innerText;
        submitBtn.disabled = true;
        submitBtn.innerText = 'Đang xử lý...';

        if (errorMessage) {
            errorMessage.textContent = '';
            errorMessage.classList.add('hidden');
        }

        try {
            // 1. Fetch current status to get data from previous steps
            const statusResponse = await fetch('/api/Onboarding/status');
            if (!statusResponse.ok) {
                throw new Error('Không thể tải dữ liệu.');
            }
            const status = await statusResponse.json();

            // 2. Prepare Complete DTO
            const completeDto = {
                profile: status.profile,
                wallet: status.wallet,
                categorySetup: status.categorySetup,
                savingsGoal: goalData
            };

            // 3. Send Complete Request
            const response = await fetch('/api/Onboarding/complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(completeDto)
            });

            if (response.ok) {
                // Success! Redirect to app dashboard
                window.location.href = '/';
            } else {
                const result = await response.json();
                throw new Error(result.message || 'Có lỗi xảy ra.');
            }

        } catch (error) {
            console.error(error);
            if (errorMessage) {
                errorMessage.textContent = error.message;
                errorMessage.classList.remove('hidden');
            }
            submitBtn.disabled = false;
            submitBtn.innerText = originalText;
        }
    }
});
