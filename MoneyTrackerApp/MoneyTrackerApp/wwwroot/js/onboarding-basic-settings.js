document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('basicSettingsForm');
    const errorMessage = document.getElementById('errorMessage');

    // Theme Selector Logic
    const themeOptions = document.querySelectorAll('.theme-option');
    themeOptions.forEach(option => {
        option.addEventListener('click', () => {
            // Uncheck all others
            themeOptions.forEach(opt => {
                const radio = opt.querySelector('input[type="radio"]');
                if (radio) radio.checked = false;
                opt.classList.remove('selected');
            });

            // Check current
            const radio = option.querySelector('input[type="radio"]');
            if (radio) radio.checked = true;
            option.classList.add('selected');
        });
    });

    // Initialize theme selection visually based on default checked input
    const checkedInput = document.querySelector('input[name="theme"]:checked');
    if (checkedInput) {
        checkedInput.closest('.theme-option').classList.add('selected');
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Clear error
        errorMessage.textContent = '';
        errorMessage.classList.add('hidden');

        // Collect data
        const formData = new FormData(form);
        const theme = document.querySelector('input[name="theme"]:checked')?.value || 'light';

        const data = {
            currency: formData.get('currency'),
            language: formData.get('language'),
            theme: theme,
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
        };

        const payload = {
            step: 2,
            stepData: JSON.stringify(data)
        };

        try {
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerText;
            submitBtn.disabled = true;
            submitBtn.innerText = 'Đang xử lý...';

            const response = await fetch('/api/Onboarding/step', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                // Navigate to next step
                window.location.href = '/Onboarding/CreateWallet';
            } else {
                const result = await response.json();
                errorMessage.textContent = result.message || 'Có lỗi xảy ra. Vui lòng thử lại.';
                errorMessage.classList.remove('hidden');
                submitBtn.disabled = false;
                submitBtn.innerText = originalText;
            }
        } catch (error) {
            console.error('Error:', error);
            errorMessage.textContent = 'Có lỗi kết nối. Vui lòng kiểm tra lại mạng.';
            errorMessage.classList.remove('hidden');
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.innerText = 'Tiếp theo →';
        }
    });
});
