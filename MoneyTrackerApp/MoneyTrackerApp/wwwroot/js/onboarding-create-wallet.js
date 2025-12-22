document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('createWalletForm');
    const errorMessage = document.getElementById('errorMessage');

    // Wallet Type Selection
    const typeCards = document.querySelectorAll('.wallet-type-card');
    const typeInput = document.getElementById('walletType');

    typeCards.forEach(card => {
        card.addEventListener('click', () => {
            // Remove active class from all
            typeCards.forEach(c => c.classList.remove('active'));
            // Add active to clicked
            card.classList.add('active');
            // Update hidden input
            typeInput.value = card.dataset.type;
        });
    });

    // Color Selection
    const colorOptions = document.querySelectorAll('.color-option');
    const colorInput = document.getElementById('walletColor');

    colorOptions.forEach(option => {
        option.addEventListener('click', () => {
            // Remove checkmark from all
            colorOptions.forEach(opt => opt.innerHTML = '');
            // Add checkmark to clicked
            option.innerHTML = '<span class="check-mark">✓</span>';
            // Update hidden input
            colorInput.value = option.dataset.color;
        });
    });

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        errorMessage.textContent = '';
        errorMessage.classList.add('hidden');

        const formData = new FormData(form);
        const data = {
            name: formData.get('walletName'),
            accountType: parseInt(formData.get('walletType')),
            initialBalance: parseFloat(formData.get('initialBalance')),
            color: formData.get('walletColor'),
            icon: document.querySelector('.wallet-type-card.active .type-icon')?.textContent || '💵'
        };

        const payload = {
            step: 3,
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
                window.location.href = '/Onboarding/SetupCategories';
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
