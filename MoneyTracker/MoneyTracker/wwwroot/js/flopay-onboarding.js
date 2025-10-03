// FloPay OnBoarding JavaScript

let currentStep = 1;
let totalSteps = 4;
let defaultCategories = {};

$(document).ready(function () {
    // Initialize onboarding
    initializeOnboarding();

    // Load user info
    loadUserInfo();

    // Load default categories
    loadDefaultCategories();

    // Setup event listeners
    setupEventListeners();
});

function initializeOnboarding() {
    // Show first step
    showStep(1);

    // Update progress
    updateProgress();

    // Add entrance animation
    $('.onboarding-card').addClass('animate-in');
}

function setupEventListeners() {
    // Step dot clicks
    $('.step-dot').on('click', function () {
        const step = parseInt($(this).data('step'));
        if (step <= currentStep || $(this).hasClass('completed')) {
            goToStep(step);
        }
    });

    // Form inputs
    $('#savingsGoal, #spendingLimit, #notificationFrequency').on('change', function () {
        validateGoals();
    });

    // Keyboard navigation
    $(document).on('keydown', function (e) {
        if (e.key === 'ArrowLeft' && currentStep > 1) {
            prevStep();
        } else if (e.key === 'ArrowRight' && currentStep < totalSteps) {
            nextStep();
        } else if (e.key === 'Escape') {
            skipOnboarding();
        }
    });
}

function loadUserInfo() {
    // Mock user data for demo
    const user = {
        fullName: 'John Doe',
        email: 'john.doe@example.com',
        pictureUrl: null
    };

    // Update UI
    $('#userFullName').text(user.fullName);
    $('#userEmail').text(user.email);

    if (user.pictureUrl) {
        $('#userAvatar').attr('src', user.pictureUrl).show();
        $('#userAvatarPlaceholder').hide();
    } else {
        const initial = user.fullName.charAt(0).toUpperCase();
        $('#userAvatarPlaceholder').text(initial).show();
        $('#userAvatar').hide();
    }
}

async function loadDefaultCategories() {
    try {
        // Mock categories data
        const mockCategories = {
            incomeCategories: [
                { name: 'Salary', description: 'Monthly salary income', icon: 'fas fa-briefcase' },
                { name: 'Freelance', description: 'Freelance work income', icon: 'fas fa-laptop' },
                { name: 'Investment', description: 'Investment returns', icon: 'fas fa-chart-line' },
                { name: 'Bonus', description: 'Performance bonuses', icon: 'fas fa-gift' }
            ],
            expenseCategories: [
                { name: 'Food & Dining', description: 'Restaurants and groceries', icon: 'fas fa-utensils' },
                { name: 'Transportation', description: 'Gas, public transport', icon: 'fas fa-car' },
                { name: 'Entertainment', description: 'Movies, games, hobbies', icon: 'fas fa-gamepad' },
                { name: 'Bills & Utilities', description: 'Electricity, water, internet', icon: 'fas fa-bolt' },
                { name: 'Shopping', description: 'Clothes, electronics', icon: 'fas fa-shopping-bag' },
                { name: 'Healthcare', description: 'Medical expenses', icon: 'fas fa-heartbeat' }
            ]
        };

        defaultCategories = mockCategories;
        displayCategories();

    } catch (error) {
        console.error('Error loading default categories:', error);
        showError('Failed to load categories. Please try again.');
    }
}

function displayCategories() {
    // Display income categories
    const incomeContainer = $('#incomeCategories');
    incomeContainer.empty();

    defaultCategories.incomeCategories.forEach(category => {
        const categoryHtml = `
            <div class="category-item">
                <div class="category-icon income">
                    <i class="${category.icon}"></i>
                </div>
                <div class="category-text">
                    <h5>${category.name}</h5>
                    <p>${category.description}</p>
                </div>
            </div>
        `;
        incomeContainer.append(categoryHtml);
    });

    // Display expense categories
    const expenseContainer = $('#expenseCategories');
    expenseContainer.empty();

    defaultCategories.expenseCategories.forEach(category => {
        const categoryHtml = `
            <div class="category-item">
                <div class="category-icon expense">
                    <i class="${category.icon}"></i>
                </div>
                <div class="category-text">
                    <h5>${category.name}</h5>
                    <p>${category.description}</p>
                </div>
            </div>
        `;
        expenseContainer.append(categoryHtml);
    });
}

function nextStep() {
    if (currentStep < totalSteps) {
        // Validate current step
        if (!validateCurrentStep()) {
            return;
        }

        // Add loading state
        addLoadingState();

        // Simulate API call delay
        setTimeout(() => {
            currentStep++;
            showStep(currentStep);
            updateProgress();
            removeLoadingState();

            // Add step completion animation
            animateStepCompletion();
        }, 500);
    }
}

function prevStep() {
    if (currentStep > 1) {
        currentStep--;
        showStep(currentStep);
        updateProgress();
    }
}

function goToStep(step) {
    if (step >= 1 && step <= totalSteps) {
        currentStep = step;
        showStep(currentStep);
        updateProgress();
    }
}

function showStep(step) {
    // Hide all steps
    $('.step-content').removeClass('active');

    // Show current step
    $(`#step${step}`).addClass('active');

    // Update step dots
    $('.step-dot').removeClass('active');
    $(`.step-dot[data-step="${step}"]`).addClass('active');

    // Mark previous steps as completed
    for (let i = 1; i < step; i++) {
        $(`.step-dot[data-step="${i}"]`).addClass('completed');
    }

    // Add entrance animation
    $(`#step${step}`).addClass('animate-in');

    // Focus on first input if available
    setTimeout(() => {
        $(`#step${step} input, #step${step} select`).first().focus();
    }, 300);
}

function updateProgress() {
    const progressPercentage = ((currentStep - 1) / (totalSteps - 1)) * 100;
    $('#progressFill').css('width', progressPercentage + '%');
}

function validateCurrentStep() {
    switch (currentStep) {
        case 1:
            return true; // Welcome step, always valid
        case 2:
            return true; // Categories step, always valid
        case 3:
            return validateGoals();
        case 4:
            return true; // Complete step, always valid
        default:
            return true;
    }
}

function validateGoals() {
    const savingsGoal = $('#savingsGoal').val();
    const spendingLimit = $('#spendingLimit').val();

    if (!savingsGoal || !spendingLimit) {
        showError('Please fill in all required fields.');
        return false;
    }

    if (parseFloat(savingsGoal) < 0 || parseFloat(spendingLimit) < 0) {
        showError('Values must be positive numbers.');
        return false;
    }

    return true;
}

async function setupCategories() {
    try {
        addLoadingState();

        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));

        // Show success message
        showSuccess('Categories setup successfully!');

        // Move to next step
        nextStep();

    } catch (error) {
        console.error('Error setting up categories:', error);
        showError('Failed to setup categories. Please try again.');
    } finally {
        removeLoadingState();
    }
}

async function saveGoals() {
    try {
        addLoadingState();

        const goals = {
            savingsGoal: parseFloat($('#savingsGoal').val()),
            spendingLimit: parseFloat($('#spendingLimit').val()),
            notificationFrequency: $('#notificationFrequency').val()
        };

        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));

        // Show success message
        showSuccess('Goals saved successfully!');

        // Move to next step
        nextStep();

    } catch (error) {
        console.error('Error saving goals:', error);
        showError('Failed to save goals. Please try again.');
    } finally {
        removeLoadingState();
    }
}

function skipOnboarding() {
    if (confirm('Are you sure you want to skip the setup? You can always configure these settings later.')) {
        goToDashboard();
    }
}

function goToDashboard() {
    // Add exit animation
    $('.onboarding-card').addClass('animate-out');

    setTimeout(() => {
        window.location.href = '/Dashboard';
    }, 500);
}

function addLoadingState() {
    $('.btn-primary').addClass('loading');
    $('.btn-primary').prop('disabled', true);
}

function removeLoadingState() {
    $('.btn-primary').removeClass('loading');
    $('.btn-primary').prop('disabled', false);
}

function showError(message) {
    // Create error notification
    const errorHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-triangle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    // Insert at top of current step
    $(`.step-content.active .step-body`).prepend(errorHtml);

    // Auto dismiss after 5 seconds
    setTimeout(() => {
        $('.alert-danger').fadeOut();
    }, 5000);
}

function showSuccess(message) {
    // Create success notification
    const successHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="fas fa-check-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    // Insert at top of current step
    $(`.step-content.active .step-body`).prepend(successHtml);

    // Auto dismiss after 3 seconds
    setTimeout(() => {
        $('.alert-success').fadeOut();
    }, 3000);
}

function animateStepCompletion() {
    // Add completion animation to current step
    $(`.step-dot[data-step="${currentStep}"]`).addClass('completed');

    // Add pulse animation
    $(`.step-dot[data-step="${currentStep}"]`).addClass('pulse');

    setTimeout(() => {
        $(`.step-dot[data-step="${currentStep}"]`).removeClass('pulse');
    }, 1000);
}

// Add CSS for animations
const style = document.createElement('style');
style.textContent = `
    .animate-in {
        animation: slideInRight 0.5s ease-out;
    }
    
    .animate-out {
        animation: slideOutLeft 0.5s ease-in;
    }
    
    @keyframes slideInRight {
        from {
            opacity: 0;
            transform: translateX(30px);
        }
        to {
            opacity: 1;
            transform: translateX(0);
        }
    }
    
    @keyframes slideOutLeft {
        from {
            opacity: 1;
            transform: translateX(0);
        }
        to {
            opacity: 0;
            transform: translateX(-30px);
        }
    }
    
    .pulse {
        animation: pulse 1s ease-in-out;
    }
    
    @keyframes pulse {
        0%, 100% { transform: scale(1); }
        50% { transform: scale(1.2); }
    }
    
    .loading {
        position: relative;
        color: transparent !important;
    }
    
    .loading::after {
        content: '';
        position: absolute;
        top: 50%;
        left: 50%;
        width: 20px;
        height: 20px;
        margin: -10px 0 0 -10px;
        border: 2px solid #ffffff;
        border-top: 2px solid transparent;
        border-radius: 50%;
        animation: spin 1s linear infinite;
    }
`;
document.head.appendChild(style);

// Export functions for global access
window.nextStep = nextStep;
window.prevStep = prevStep;
window.setupCategories = setupCategories;
window.saveGoals = saveGoals;
window.skipOnboarding = skipOnboarding;
window.goToDashboard = goToDashboard;
