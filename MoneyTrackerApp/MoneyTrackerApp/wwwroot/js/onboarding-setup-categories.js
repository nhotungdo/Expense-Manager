// Setup Categories JavaScript
(function () {
    const form = document.getElementById('setupCategoriesForm');
    const templateCards = document.querySelectorAll('.template-card');
    const categoryPreviewList = document.getElementById('categoryPreviewList');
    const previewLoading = document.querySelector('.preview-loading');
    const errorMessage = document.getElementById('errorMessage');
    let currentTemplate = 'Student';

    // Initialize
    function init() {
        // Load saved data if exists
        loadSavedData();

        // Template selection
        templateCards.forEach(card => {
            card.addEventListener('click', function () {
                templateCards.forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                currentTemplate = this.dataset.template;
                document.getElementById('selectedTemplate').value = currentTemplate;
                loadCategoryPreview(currentTemplate);
            });
        });

        // Load initial preview
        loadCategoryPreview(currentTemplate);

        // Form submission
        form.addEventListener('submit', handleSubmit);
    }

    async function loadCategoryPreview(template) {
        // Show loading
        previewLoading.classList.remove('hidden');
        categoryPreviewList.classList.add('hidden');

        try {
            const response = await fetch(`/api/onboarding/templates/${template}`);

            if (!response.ok) {
                throw new Error('Failed to load categories');
            }

            const categories = await response.json();
            displayCategories(categories);
        } catch (error) {
            console.error('Error loading categories:', error);
            showError('Failed to load category preview');
        }
    }

    function displayCategories(categories) {
        categoryPreviewList.innerHTML = '';

        categories.forEach(category => {
            const categoryItem = document.createElement('div');
            categoryItem.className = 'category-item';
            categoryItem.innerHTML = `
                <div class="category-icon" style="background-color: ${category.color}">
                    ${category.icon}
                </div>
                <div class="category-info">
                    <div class="category-name">${category.name}</div>
                    <div class="category-type">${category.type === 0 ? 'Expense' : 'Income'}</div>
                </div>
            `;
            categoryPreviewList.appendChild(categoryItem);
        });

        // Hide loading, show preview
        previewLoading.classList.add('hidden');
        categoryPreviewList.classList.remove('hidden');
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const formData = {
            template: currentTemplate,
            customCategories: [] // Can be extended for custom categories
        };

        // Save to session storage
        sessionStorage.setItem('onboarding_categories', JSON.stringify(formData));

        // Update onboarding step
        try {
            const accessToken = getCookie('AccessToken');
            if (accessToken) {
                await fetch('/api/onboarding/step', {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${accessToken}`
                    },
                    body: JSON.stringify({
                        step: 5, // SavingsGoal
                        stepData: JSON.stringify(formData)
                    })
                });
            }

            // Navigate to next page
            window.location.href = '/Onboarding/SavingsGoal';
        } catch (error) {
            console.error('Error saving categories:', error);
            showError('Failed to save categories. Please try again.');
        }
    }

    function loadSavedData() {
        const saved = sessionStorage.getItem('onboarding_categories');
        if (saved) {
            try {
                const data = JSON.parse(saved);
                currentTemplate = data.template || 'Student';
                document.getElementById('selectedTemplate').value = currentTemplate;

                // Set active template
                const templateCard = document.querySelector(`.template-card[data-template="${currentTemplate}"]`);
                if (templateCard) {
                    templateCards.forEach(c => c.classList.remove('active'));
                    templateCard.classList.add('active');
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
