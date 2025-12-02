// Create Wallet JavaScript
(function () {
    const form = document.getElementById('createWalletForm');
    const walletTypeCards = document.querySelectorAll('.wallet-type-card');
    const colorOptions = document.querySelectorAll('.color-option');
    const errorMessage = document.getElementById('errorMessage');
    const currencySymbol = document.getElementById('currencySymbol');

    // Initialize
    function init() {
        // Load saved data if exists
        loadSavedData();

        // Update currency symbol from previous step
        updateCurrencySymbol();

        // Wallet type selection
        walletTypeCards.forEach(card => {
            card.addEventListener('click', function () {
                walletTypeCards.forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                document.getElementById('walletType').value = this.dataset.type;
            });
        });

        // Color selection
        colorOptions.forEach(option => {
            option.addEventListener('click', function () {
                colorOptions.forEach(opt => {
                    opt.classList.remove('selected');
                    opt.querySelector('.check-mark').style.opacity = '0';
                });
                this.classList.add('selected');
                this.querySelector('.check-mark').style.opacity = '1';
                document.getElementById('walletColor').value = this.dataset.color;
            });
        });

        // Form submission
        form.addEventListener('submit', handleSubmit);

        // Balance validation
        const balanceInput = document.getElementById('initialBalance');
        balanceInput.addEventListener('input', function () {
            if (this.value < 0) {
                this.value = 0;
            }
        });
    }

    function updateCurrencySymbol() {
        const symbol = sessionStorage.getItem('currencySymbol') || '₫';
        currencySymbol.textContent = symbol;
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const walletName = document.getElementById('walletName').value.trim();
        const initialBalance = parseFloat(document.getElementById('initialBalance').value) || 0;

        if (!walletName) {
            showError('Please enter a wallet name');
            return;
        }

        if (initialBalance < 0) {
            showError('Initial balance cannot be negative');
            return;
        }

        const formData = {
            name: walletName,
            accountType: parseInt(document.getElementById('walletType').value),
            initialBalance: initialBalance,
            icon: getWalletIcon(parseInt(document.getElementById('walletType').value)),
            color: document.getElementById('walletColor').value
        };

        // Save to session storage
        sessionStorage.setItem('onboarding_wallet', JSON.stringify(formData));

        // Update onboarding step
        try {
            await fetch('/api/onboarding/step', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    step: 4, // SetupCategories
                    stepData: JSON.stringify(formData)
                })
            });

            // Navigate to next page
            window.location.href = '/Onboarding/SetupCategories';
        } catch (error) {
            console.error('Error saving wallet:', error);
            showError('Failed to save wallet. Please try again.');
        }
    }

    function getWalletIcon(type) {
        const icons = {
            0: '💵', // Cash
            1: '🏦', // Bank Account
            2: '💳', // Credit Card
            3: '💰'  // Savings
        };
        return icons[type] || '💰';
    }

    function loadSavedData() {
        const saved = sessionStorage.getItem('onboarding_wallet');
        if (saved) {
            try {
                const data = JSON.parse(saved);
                document.getElementById('walletName').value = data.name || 'Cash Wallet';
                document.getElementById('initialBalance').value = data.initialBalance || 0;
                document.getElementById('walletType').value = data.accountType || 0;
                document.getElementById('walletColor').value = data.color || '#4CAF50';

                // Set active wallet type
                const typeCard = document.querySelector(`.wallet-type-card[data-type="${data.accountType || 0}"]`);
                if (typeCard) {
                    walletTypeCards.forEach(c => c.classList.remove('active'));
                    typeCard.classList.add('active');
                }

                // Set active color
                const colorOption = document.querySelector(`.color-option[data-color="${data.color || '#4CAF50'}"]`);
                if (colorOption) {
                    colorOptions.forEach(opt => {
                        opt.classList.remove('selected');
                        opt.querySelector('.check-mark').style.opacity = '0';
                    });
                    colorOption.classList.add('selected');
                    colorOption.querySelector('.check-mark').style.opacity = '1';
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
