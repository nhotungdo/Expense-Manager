document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('setupCategoriesForm');
    const errorMessage = document.getElementById('errorMessage');
    const templateInput = document.getElementById('selectedTemplate');
    const previewList = document.getElementById('categoryPreviewList');
    const loadingPreview = document.querySelector('.preview-loading');
    const templateCards = document.querySelectorAll('.template-card');

    // Function to load preview
    async function loadPreview(template) {
        if (loadingPreview) loadingPreview.classList.remove('hidden');
        if (previewList) previewList.classList.add('hidden');

        try {
            const response = await fetch(`/api/Onboarding/templates/${template}`);
            if (response.ok) {
                const categories = await response.json();
                renderPreview(categories);
            }
        } catch (error) {
            console.error('Error loading template:', error);
        } finally {
            if (loadingPreview) loadingPreview.classList.add('hidden');
            if (previewList) previewList.classList.remove('hidden');
        }
    }

    function renderPreview(categories) {
        if (!previewList) return;
        previewList.innerHTML = '';
        categories.forEach(cat => {
            const item = document.createElement('div');
            item.className = 'preview-item';
            // Determine background color/icon style based on simple heuristics or defaults
            // Assuming categories object matches CategoryPreviewDto

            item.innerHTML = `
                <div class="category-icon" style="background-color: ${cat.color || '#e0e0e0'}">${cat.icon || '📝'}</div>
                <div class="category-info">
                    <div class="category-name">${cat.name}</div>
                    <div class="category-type">${cat.type === 0 ? 'Chi tiêu' : 'Thu nhập'}</div>
                </div>
            `;
            previewList.appendChild(item);
        });
    }

    templateCards.forEach(card => {
        card.addEventListener('click', () => {
            templateCards.forEach(c => c.classList.remove('active'));
            card.classList.add('active');

            const template = card.dataset.template;
            templateInput.value = template;
            loadPreview(template);
        });
    });

    // Load initial preview
    if (templateInput && templateInput.value) {
        loadPreview(templateInput.value);
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        if (errorMessage) {
            errorMessage.textContent = '';
            errorMessage.classList.add('hidden');
        }

        const data = {
            template: templateInput.value
            // Custom categories could be added here if the UI supported it
        };

        const payload = {
            step: 4,
            stepData: JSON.stringify(data)
        };

        try {
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerText;
            submitBtn.disabled = true;
            submitBtn.innerText = 'Đang xử lý...';

            const response = await fetch('/api/Onboarding/step', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                window.location.href = '/Onboarding/SavingsGoal';
            } else {
                const result = await response.json();
                if (errorMessage) {
                    errorMessage.textContent = result.message || 'Có lỗi xảy ra.';
                    errorMessage.classList.remove('hidden');
                }
                submitBtn.disabled = false;
                submitBtn.innerText = originalText;
            }
        } catch (error) {
            if (errorMessage) {
                errorMessage.textContent = 'Lỗi kết nối.';
                errorMessage.classList.remove('hidden');
            }
            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.innerText = 'Tiếp theo →';
        }
    });
});
