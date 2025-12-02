// Savings Goal JavaScript
(function () {
    const form = document.getElementById('savingsGoalForm');
    const colorOptions = document.querySelectorAll('.color-option');
    const errorMessage = document.getElementById('errorMessage');
    const calculationCard = document.getElementById('calculationCard');
    const monthlyAmountDisplay = document.getElementById('monthlyAmount');
    const currencySymbol = document.getElementById('currencySymbol');
    const btnSkip = document.getElementById('btnSkip');

    // Initialize
    function init() {
        // Load saved data if exists
        loadSavedData();

        // Update currency symbol from previous step
        updateCurrencySymbol();

        // Color selection
        colorOptions.forEach(option => {
            option.addEventListener('click', function () {
                colorOptions.forEach(opt => {
                    opt.classList.remove('selected');
                    opt.querySelector('.check-mark').style.opacity = '0';
                });
                this.classList.add('selected');
                this.querySelector('.check-mark').style.opacity = '1';
                document.getElementById('goalColor').value = this.dataset.color;
            });
        });

        // Calculate monthly amount when inputs change
        const targetAmount = document.getElementById('targetAmount');
        const targetDate = document.getElementById('targetDate');

        targetAmount.addEventListener('input', calculateMonthly);
        targetDate.addEventListener('change', calculateMonthly);

        // Skip button
        btnSkip.addEventListener('click', skipStep);

        // Form submission
        form.addEventListener('submit', handleSubmit);

        // Set minimum date to today
        const today = new Date().toISOString().split('T')[0];
        targetDate.setAttribute('min', today);
    }

    function updateCurrencySymbol() {
        const symbol = sessionStorage.getItem('currencySymbol') || '₫';
        currencySymbol.textContent = symbol;
    }

    async function calculateMonthly() {
        const targetAmount = parseFloat(document.getElementById('targetAmount').value);
        const targetDate = document.getElementById('targetDate').value;

        if (!targetAmount || !targetDate || targetAmount <= 0) {
            calculationCard.classList.add('hidden');
            return;
        }

        try {
            const response = await fetch('/api/onboarding/calculate-savings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    targetAmount: targetAmount,
                    targetDate: targetDate
                })
            });

            if (response.ok) {
                const result = await response.json();
                const symbol = sessionStorage.getItem('currencySymbol') || '₫';
                monthlyAmountDisplay.textContent = `${symbol}${formatNumber(result.monthlyAmount)}`;
                calculationCard.classList.remove('hidden');
            }
        } catch (error) {
            console.error('Error calculating monthly amount:', error);
        }
    }

    function formatNumber(num) {
        return new Intl.NumberFormat('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(num);
    }

    async function skipStep() {
        // Complete onboarding without savings goal
        await completeOnboarding(null);
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const goalName = document.getElementById('goalName').value.trim();
        const targetAmount = parseFloat(document.getElementById('targetAmount').value);
        const targetDate = document.getElementById('targetDate').value;

        let savingsGoal = null;

        if (goalName && targetAmount && targetDate) {
            if (targetAmount <= 0) {
                showError('Target amount must be greater than 0');
                return;
            }

            const selectedDate = new Date(targetDate);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate <= today) {
                showError('Target date must be in the future');
                return;
            }

            savingsGoal = {
                name: goalName,
                targetAmount: targetAmount,
                targetDate: targetDate,
                icon: '🎯',
                color: document.getElementById('goalColor').value
            };
        }

        await completeOnboarding(savingsGoal);
    }

    async function completeOnboarding(savingsGoal) {
        try {
            // Get all saved data
            const profile = JSON.parse(sessionStorage.getItem('onboarding_profile') || '{}');
            const wallet = JSON.parse(sessionStorage.getItem('onboarding_wallet') || '{}');
            const categories = JSON.parse(sessionStorage.getItem('onboarding_categories') || '{}');

            const completeData = {
                profile: {
                    currency: profile.currency || 'VND',
                    language: profile.language || 'vi',
                    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
                    theme: profile.theme || 'light'
                },
                wallet: {
                    name: wallet.name || 'Cash Wallet',
                    accountType: wallet.accountType || 0,
                    initialBalance: wallet.initialBalance || 0,
                    icon: wallet.icon || '💰',
                    color: wallet.color || '#4CAF50'
                },
                categorySetup: {
                    template: categories.template || 'Student',
                    customCategories: categories.customCategories || []
                },
                savingsGoal: savingsGoal
            };

            const response = await fetch('/api/onboarding/complete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(completeData)
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                console.error('Server error:', errorData);
                throw new Error(errorData.message || 'Failed to complete onboarding');
            }

            const result = await response.json();
            if (result.accessToken) {
                localStorage.setItem('accessToken', result.accessToken);
                if (result.refreshToken) localStorage.setItem('refreshToken', result.refreshToken);
            }

            // Clear session storage
            sessionStorage.removeItem('onboarding_profile');
            sessionStorage.removeItem('onboarding_wallet');
            sessionStorage.removeItem('onboarding_categories');
            sessionStorage.removeItem('currencySymbol');

            // Navigate to completion page
            window.location.href = '/Onboarding/Complete';
        } catch (error) {
            console.error('Error completing onboarding:', error);
            showError(error.message || 'Failed to complete setup. Please try again.');
        }
    }

    function loadSavedData() {
        const saved = sessionStorage.getItem('onboarding_savings');
        if (saved) {
            try {
                const data = JSON.parse(saved);
                if (data.name) document.getElementById('goalName').value = data.name;
                if (data.targetAmount) document.getElementById('targetAmount').value = data.targetAmount;
                if (data.targetDate) document.getElementById('targetDate').value = data.targetDate;
                if (data.color) {
                    document.getElementById('goalColor').value = data.color;
                    const colorOption = document.querySelector(`.color-option[data-color="${data.color}"]`);
                    if (colorOption) {
                        colorOptions.forEach(opt => {
                            opt.classList.remove('selected');
                            opt.querySelector('.check-mark').style.opacity = '0';
                        });
                        colorOption.classList.add('selected');
                        colorOption.querySelector('.check-mark').style.opacity = '1';
                    }
                }
                calculateMonthly();
            } catch (error) {
                console.error('Error loading saved data:', error);
            }
        }
    }

    function showError(message) {
        errorMessage.textContent = message;
        errorMessage.classList.remove('hidden');
        setTimeout(() => {
            errorMessage.classList.add('hidden');
        }, 5000);
    }

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
