// Basic Settings JavaScript
(function () {
    const form = document.getElementById('basicSettingsForm');
    const themeOptions = document.querySelectorAll('.theme-option');
    const errorMessage = document.getElementById('errorMessage');

    // Initialize
    function init() {
        // Load saved data if exists
        loadSavedData();

        // Theme selection
        themeOptions.forEach(option => {
            option.addEventListener('click', function () {
                themeOptions.forEach(opt => opt.classList.remove('active'));
                this.classList.add('active');
                const themeInput = this.querySelector('input[type="radio"]');
                themeInput.checked = true;
            });
        });

        // Form submission
        form.addEventListener('submit', handleSubmit);

        // Update currency symbol based on selection
        const currencySelect = document.getElementById('currency');
        currencySelect.addEventListener('change', updateCurrencySymbol);
    }

    function updateCurrencySymbol() {
        const currency = document.getElementById('currency').value;
        const symbols = {
            'VND': '₫',
            'USD': '$',
            'EUR': '€',
            'GBP': '£',
            'JPY': '¥'
        };

        // Store for next page
        sessionStorage.setItem('currencySymbol', symbols[currency] || '₫');
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const formData = {
            currency: document.getElementById('currency').value,
            language: document.getElementById('language').value,
            theme: document.querySelector('input[name="theme"]:checked').value
        };

        // Save to session storage
        sessionStorage.setItem('onboarding_profile', JSON.stringify(formData));

        // Update onboarding step
        try {
            await fetch('/api/onboarding/step', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    step: 3, // CreateWallet
                    stepData: JSON.stringify(formData)
                })
            });

            // Navigate to next page
            window.location.href = '/Onboarding/CreateWallet';
        } catch (error) {
            console.error('Error saving settings:', error);
            showError('Failed to save settings. Please try again.');
        }
    }

    function loadSavedData() {
        const saved = sessionStorage.getItem('onboarding_profile');
        if (saved) {
            try {
                const data = JSON.parse(saved);
                document.getElementById('currency').value = data.currency || 'VND';
                document.getElementById('language').value = data.language || 'vi';

                // Set theme
                const themeInput = document.querySelector(`input[name="theme"][value="${data.theme || 'light'}"]`);
                if (themeInput) {
                    themeInput.checked = true;
                    themeInput.closest('.theme-option').classList.add('active');
                }
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
