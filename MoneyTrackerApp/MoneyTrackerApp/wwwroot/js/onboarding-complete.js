// Onboarding Complete JavaScript
(function () {
    const btnGoToDashboard = document.getElementById('btnGoToDashboard');

    // Initialize
    function init() {
        // Load and display summary
        displaySummary();

        // Dashboard button
        btnGoToDashboard.addEventListener('click', goToDashboard);

        // Add confetti effect
        createConfetti();
    }

    function displaySummary() {
        try {
            // Get wallet info
            const wallet = JSON.parse(sessionStorage.getItem('onboarding_wallet') || '{}');
            const categories = JSON.parse(sessionStorage.getItem('onboarding_categories') || '{}');
            const savings = sessionStorage.getItem('onboarding_savings');

            // Display wallet name
            document.getElementById('walletName').textContent = wallet.name || 'Cash Wallet';

            // Display category count (approximate based on template)
            const categoryCounts = {
                'Student': 8,
                'Family': 8,
                'Business': 8,
                'Freelancer': 6,
                'Minimal': 6
            };
            const categoryCount = categoryCounts[categories.template] || 6;
            document.getElementById('categoryCount').textContent = categoryCount;

            // Display goal count
            const goalCount = savings ? 1 : 0;
            document.getElementById('goalCount').textContent = goalCount;

        } catch (error) {
            console.error('Error displaying summary:', error);
        }
    }

    function goToDashboard() {
        // Clear any remaining session data
        sessionStorage.removeItem('onboarding_profile');
        sessionStorage.removeItem('onboarding_wallet');
        sessionStorage.removeItem('onboarding_categories');
        sessionStorage.removeItem('onboarding_savings');
        sessionStorage.removeItem('currencySymbol');

        // Navigate to home/dashboard
        window.location.href = '/Home';
    }

    function createConfetti() {
        // Simple confetti animation
        const colors = ['#667eea', '#764ba2', '#4CAF50', '#FF5722', '#2196F3', '#FF9800'];
        const confettiCount = 50;
        const container = document.querySelector('.onboarding-container');

        for (let i = 0; i < confettiCount; i++) {
            setTimeout(() => {
                const confetti = document.createElement('div');
                confetti.style.position = 'fixed';
                confetti.style.width = '10px';
                confetti.style.height = '10px';
                confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
                confetti.style.left = Math.random() * 100 + '%';
                confetti.style.top = '-10px';
                confetti.style.opacity = '1';
                confetti.style.borderRadius = '50%';
                confetti.style.pointerEvents = 'none';
                confetti.style.zIndex = '9999';
                confetti.style.transition = 'all 3s ease-out';

                document.body.appendChild(confetti);

                // Animate
                setTimeout(() => {
                    confetti.style.top = '100vh';
                    confetti.style.opacity = '0';
                    confetti.style.transform = `rotate(${Math.random() * 360}deg) translateX(${(Math.random() - 0.5) * 200}px)`;
                }, 10);

                // Remove after animation
                setTimeout(() => {
                    confetti.remove();
                }, 3000);
            }, i * 50);
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
