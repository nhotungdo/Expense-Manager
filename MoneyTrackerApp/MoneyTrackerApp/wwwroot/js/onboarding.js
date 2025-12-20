// Onboarding Module - Consolidated
// Xử lý tất cả các bước onboarding: Welcome, BasicSettings, CreateWallet, SetupCategories, SavingsGoal, Complete

const OnboardingModule = (function () {
    'use strict';

    // ============================================
    // BƯỚC WELCOME
    // ============================================
    const Welcome = {
        init() {
            if (!document.getElementById('slidesWrapper')) return;

            let currentSlide = 0;
            const slides = document.querySelectorAll('.slide');
            const indicators = document.querySelectorAll('.indicator');
            const btnNext = document.getElementById('btnNext');
            const btnSkip = document.getElementById('btnSkip');
            const btnStart = document.getElementById('btnStart');
            const totalSlides = slides.length;

            const showSlide = (index) => {
                slides.forEach(slide => slide.classList.remove('active', 'prev'));
                indicators.forEach(indicator => indicator.classList.remove('active'));
                slides[index].classList.add('active');
                indicators[index].classList.add('active');
                currentSlide = index;

                if (index === totalSlides - 1) {
                    btnNext.classList.add('hidden');
                    btnStart.classList.remove('hidden');
                } else {
                    btnNext.classList.remove('hidden');
                    btnStart.classList.add('hidden');
                }
            };

            const nextSlide = () => {
                if (currentSlide < totalSlides - 1) {
                    slides[currentSlide].classList.add('prev');
                    showSlide(currentSlide + 1);
                }
            };

            const prevSlide = () => {
                if (currentSlide > 0) showSlide(currentSlide - 1);
            };

            const goToNextStep = async () => {
                try {
                    await fetch('/api/onboarding/step', {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ step: 2 })
                    });
                } catch (error) {
                    console.error('Lỗi cập nhật bước:', error);
                }
                window.location.href = '/Onboarding/BasicSettings';
            };

            // Kiểm tra token trong URL
            const params = new URLSearchParams(location.search);
            const accessToken = params.get('accessToken');
            const refreshToken = params.get('refreshToken');
            if (accessToken && refreshToken) {
                localStorage.setItem('accessToken', accessToken);
                localStorage.setItem('refreshToken', refreshToken);
                window.history.replaceState({}, document.title, window.location.pathname);
            }

            // Event listeners
            btnNext.addEventListener('click', nextSlide);
            btnSkip.addEventListener('click', goToNextStep);
            btnStart.addEventListener('click', goToNextStep);
            indicators.forEach((indicator, index) => {
                indicator.addEventListener('click', () => showSlide(index));
            });

            // Hỗ trợ vuốt
            let touchStartX = 0;
            const slidesWrapper = document.getElementById('slidesWrapper');
            slidesWrapper.addEventListener('touchstart', e => {
                touchStartX = e.changedTouches[0].screenX;
            });
            slidesWrapper.addEventListener('touchend', e => {
                const touchEndX = e.changedTouches[0].screenX;
                if (touchEndX < touchStartX - 50) nextSlide();
                if (touchEndX > touchStartX + 50) prevSlide();
            });

            showSlide(0);
        }
    };

    // ============================================
    // BƯỚC CÀI ĐẶT CƠ BẢN
    // ============================================
    const BasicSettings = {
        init() {
            const form = document.getElementById('basicSettingsForm');
            if (!form) return;

            const themeOptions = document.querySelectorAll('.theme-option');
            const errorMessage = document.getElementById('errorMessage');
            const currencySelect = document.getElementById('currency');

            const updateCurrencySymbol = () => {
                const symbols = { 'VND': '₫', 'USD': '$', 'EUR': '€', 'GBP': '£', 'JPY': '¥' };
                sessionStorage.setItem('currencySymbol', symbols[currencySelect.value] || '₫');
            };

            const loadSavedData = () => {
                const saved = sessionStorage.getItem('onboarding_profile');
                if (saved) {
                    try {
                        const data = JSON.parse(saved);
                        document.getElementById('currency').value = data.currency || 'VND';
                        document.getElementById('language').value = data.language || 'vi';
                        const themeInput = document.querySelector(`input[name="theme"][value="${data.theme || 'light'}"]`);
                        if (themeInput) {
                            themeInput.checked = true;
                            themeInput.closest('.theme-option').classList.add('active');
                        }
                    } catch (error) {
                        console.error('Lỗi tải dữ liệu đã lưu:', error);
                    }
                }
            };

            const showError = (message) => {
                errorMessage.textContent = message;
                errorMessage.classList.remove('hidden');
                setTimeout(() => errorMessage.classList.add('hidden'), 5000);
            };

            const handleSubmit = async (e) => {
                e.preventDefault();
                const formData = {
                    currency: document.getElementById('currency').value,
                    language: document.getElementById('language').value,
                    theme: document.querySelector('input[name="theme"]:checked').value
                };

                sessionStorage.setItem('onboarding_profile', JSON.stringify(formData));

                try {
                    await fetch('/api/onboarding/step', {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ step: 3, stepData: JSON.stringify(formData) })
                    });
                    window.location.href = '/Onboarding/CreateWallet';
                } catch (error) {
                    console.error('Lỗi lưu cài đặt:', error);
                    showError('Không thể lưu cài đặt. Vui lòng thử lại.');
                }
            };

            themeOptions.forEach(option => {
                option.addEventListener('click', function () {
                    themeOptions.forEach(opt => opt.classList.remove('active'));
                    this.classList.add('active');
                    this.querySelector('input[type="radio"]').checked = true;
                });
            });

            currencySelect.addEventListener('change', updateCurrencySymbol);
            form.addEventListener('submit', handleSubmit);
            loadSavedData();
        }
    };

    // ============================================
    // BƯỚC TẠO VÍ
    // ============================================
    const CreateWallet = {
        init() {
            const form = document.getElementById('createWalletForm');
            if (!form) return;

            const walletTypeCards = document.querySelectorAll('.wallet-type-card');
            const colorOptions = document.querySelectorAll('.color-option');
            const errorMessage = document.getElementById('errorMessage');
            const currencySymbol = document.getElementById('currencySymbol');

            const updateCurrencySymbol = () => {
                const symbol = sessionStorage.getItem('currencySymbol') || '₫';
                currencySymbol.textContent = symbol;
            };

            const getWalletIcon = (type) => {
                const icons = { 0: '💵', 1: '🏦', 2: '💳', 3: '💰' };
                return icons[type] || '💰';
            };

            const loadSavedData = () => {
                const saved = sessionStorage.getItem('onboarding_wallet');
                if (saved) {
                    try {
                        const data = JSON.parse(saved);
                        document.getElementById('walletName').value = data.name || 'Ví tiền mặt';
                        document.getElementById('initialBalance').value = data.initialBalance || 0;
                        document.getElementById('walletType').value = data.accountType || 0;
                        document.getElementById('walletColor').value = data.color || '#4CAF50';

                        const typeCard = document.querySelector(`.wallet-type-card[data-type="${data.accountType || 0}"]`);
                        if (typeCard) {
                            walletTypeCards.forEach(c => c.classList.remove('active'));
                            typeCard.classList.add('active');
                        }

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
                        console.error('Lỗi tải dữ liệu đã lưu:', error);
                    }
                }
            };

            const showError = (message) => {
                errorMessage.textContent = message;
                errorMessage.classList.remove('hidden');
                setTimeout(() => errorMessage.classList.add('hidden'), 5000);
            };

            const handleSubmit = async (e) => {
                e.preventDefault();
                const walletName = document.getElementById('walletName').value.trim();
                const initialBalance = parseFloat(document.getElementById('initialBalance').value) || 0;

                if (!walletName) {
                    showError('Vui lòng nhập tên ví');
                    return;
                }
                if (initialBalance < 0) {
                    showError('Số dư ban đầu không thể âm');
                    return;
                }

                const formData = {
                    name: walletName,
                    accountType: parseInt(document.getElementById('walletType').value),
                    initialBalance: initialBalance,
                    icon: getWalletIcon(parseInt(document.getElementById('walletType').value)),
                    color: document.getElementById('walletColor').value
                };

                sessionStorage.setItem('onboarding_wallet', JSON.stringify(formData));

                try {
                    await fetch('/api/onboarding/step', {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ step: 4, stepData: JSON.stringify(formData) })
                    });
                    window.location.href = '/Onboarding/SetupCategories';
                } catch (error) {
                    console.error('Lỗi lưu ví:', error);
                    showError('Không thể lưu ví. Vui lòng thử lại.');
                }
            };

            walletTypeCards.forEach(card => {
                card.addEventListener('click', function () {
                    walletTypeCards.forEach(c => c.classList.remove('active'));
                    this.classList.add('active');
                    document.getElementById('walletType').value = this.dataset.type;
                });
            });

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

            const balanceInput = document.getElementById('initialBalance');
            balanceInput.addEventListener('input', function () {
                if (this.value < 0) this.value = 0;
            });

            form.addEventListener('submit', handleSubmit);
            updateCurrencySymbol();
            loadSavedData();
        }
    };

    // ============================================
    // BƯỚC THIẾT LẬP DANH MỤC
    // ============================================
    const SetupCategories = {
        init() {
            const form = document.getElementById('setupCategoriesForm');
            if (!form) return;

            const templateCards = document.querySelectorAll('.template-card');
            const categoryPreviewList = document.getElementById('categoryPreviewList');
            const previewLoading = document.querySelector('.preview-loading');
            const errorMessage = document.getElementById('errorMessage');
            let currentTemplate = 'Student';

            const displayCategories = (categories) => {
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
                            <div class="category-type">${category.type === 0 ? 'Chi tiêu' : 'Thu nhập'}</div>
                        </div>
                    `;
                    categoryPreviewList.appendChild(categoryItem);
                });
                previewLoading.classList.add('hidden');
                categoryPreviewList.classList.remove('hidden');
            };

            const loadCategoryPreview = async (template) => {
                previewLoading.classList.remove('hidden');
                categoryPreviewList.classList.add('hidden');

                try {
                    const response = await fetch(`/api/onboarding/templates/${template}`);
                    if (!response.ok) throw new Error('Không thể tải danh mục');
                    const categories = await response.json();
                    displayCategories(categories);
                } catch (error) {
                    console.error('Lỗi tải danh mục:', error);
                    showError('Không thể tải xem trước danh mục');
                }
            };

            const showError = (message) => {
                errorMessage.textContent = message;
                errorMessage.classList.remove('hidden');
                setTimeout(() => errorMessage.classList.add('hidden'), 5000);
            };

            const loadSavedData = () => {
                const saved = sessionStorage.getItem('onboarding_categories');
                if (saved) {
                    try {
                        const data = JSON.parse(saved);
                        currentTemplate = data.template || 'Student';
                        document.getElementById('selectedTemplate').value = currentTemplate;
                        const templateCard = document.querySelector(`.template-card[data-template="${currentTemplate}"]`);
                        if (templateCard) {
                            templateCards.forEach(c => c.classList.remove('active'));
                            templateCard.classList.add('active');
                        }
                    } catch (error) {
                        console.error('Lỗi tải dữ liệu đã lưu:', error);
                    }
                }
            };

            const handleSubmit = async (e) => {
                e.preventDefault();
                const formData = {
                    template: currentTemplate,
                    customCategories: []
                };

                sessionStorage.setItem('onboarding_categories', JSON.stringify(formData));

                try {
                    await fetch('/api/onboarding/step', {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ step: 5, stepData: JSON.stringify(formData) })
                    });
                    window.location.href = '/Onboarding/SavingsGoal';
                } catch (error) {
                    console.error('Lỗi lưu danh mục:', error);
                    showError('Không thể lưu danh mục. Vui lòng thử lại.');
                }
            };

            templateCards.forEach(card => {
                card.addEventListener('click', function () {
                    templateCards.forEach(c => c.classList.remove('active'));
                    this.classList.add('active');
                    currentTemplate = this.dataset.template;
                    document.getElementById('selectedTemplate').value = currentTemplate;
                    loadCategoryPreview(currentTemplate);
                });
            });

            form.addEventListener('submit', handleSubmit);
            loadSavedData();
            loadCategoryPreview(currentTemplate);
        }
    };

    // ============================================
    // BƯỚC MỤC TIÊU TIẾT KIỆM
    // ============================================
    const SavingsGoal = {
        init() {
            const form = document.getElementById('savingsGoalForm');
            if (!form) return;

            const colorOptions = document.querySelectorAll('.color-option');
            const errorMessage = document.getElementById('errorMessage');
            const calculationCard = document.getElementById('calculationCard');
            const monthlyAmountDisplay = document.getElementById('monthlyAmount');
            const currencySymbol = document.getElementById('currencySymbol');
            const btnSkip = document.getElementById('btnSkip');

            const updateCurrencySymbol = () => {
                const symbol = sessionStorage.getItem('currencySymbol') || '₫';
                currencySymbol.textContent = symbol;
            };

            const formatNumber = (num) => {
                return new Intl.NumberFormat('vi-VN', {
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 0
                }).format(num);
            };

            const calculateMonthly = async () => {
                const targetAmount = parseFloat(document.getElementById('targetAmount').value);
                const targetDate = document.getElementById('targetDate').value;

                if (!targetAmount || !targetDate || targetAmount <= 0) {
                    calculationCard.classList.add('hidden');
                    return;
                }

                try {
                    const response = await fetch('/api/onboarding/calculate-savings', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ targetAmount, targetDate })
                    });

                    if (response.ok) {
                        const result = await response.json();
                        const symbol = sessionStorage.getItem('currencySymbol') || '₫';
                        monthlyAmountDisplay.textContent = `${formatNumber(result.monthlyAmount)} ${symbol}`;
                        calculationCard.classList.remove('hidden');
                    }
                } catch (error) {
                    console.error('Lỗi tính toán số tiền hàng tháng:', error);
                }
            };

            const showError = (message) => {
                errorMessage.textContent = message;
                errorMessage.classList.remove('hidden');
                setTimeout(() => errorMessage.classList.add('hidden'), 5000);
            };

            const completeOnboarding = async (savingsGoal) => {
                try {
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
                            name: wallet.name || 'Ví tiền mặt',
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
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(completeData)
                    });

                    if (!response.ok) {
                        const errorData = await response.json().catch(() => ({}));
                        console.error('Lỗi server:', errorData);
                        throw new Error(errorData.message || 'Không thể hoàn tất onboarding');
                    }

                    const result = await response.json();
                    if (result.accessToken) {
                        localStorage.setItem('accessToken', result.accessToken);
                        if (result.refreshToken) localStorage.setItem('refreshToken', result.refreshToken);
                    }

                    // Xóa session storage
                    ['onboarding_profile', 'onboarding_wallet', 'onboarding_categories', 'currencySymbol'].forEach(key => {
                        sessionStorage.removeItem(key);
                    });

                    window.location.href = '/Onboarding/Complete';
                } catch (error) {
                    console.error('Lỗi hoàn tất onboarding:', error);
                    showError(error.message || 'Không thể hoàn tất thiết lập. Vui lòng thử lại.');
                }
            };

            const loadSavedData = () => {
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
                        console.error('Lỗi tải dữ liệu đã lưu:', error);
                    }
                }
            };

            const handleSubmit = async (e) => {
                e.preventDefault();
                const goalName = document.getElementById('goalName').value.trim();
                const targetAmount = parseFloat(document.getElementById('targetAmount').value);
                const targetDate = document.getElementById('targetDate').value;

                let savingsGoal = null;

                if (goalName && targetAmount && targetDate) {
                    if (targetAmount <= 0) {
                        showError('Số tiền mục tiêu phải lớn hơn 0');
                        return;
                    }

                    const selectedDate = new Date(targetDate);
                    const today = new Date();
                    today.setHours(0, 0, 0, 0);

                    if (selectedDate <= today) {
                        showError('Ngày mục tiêu phải trong tương lai');
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
            };

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

            const targetAmount = document.getElementById('targetAmount');
            const targetDate = document.getElementById('targetDate');
            targetAmount.addEventListener('input', calculateMonthly);
            targetDate.addEventListener('change', calculateMonthly);
            btnSkip.addEventListener('click', () => completeOnboarding(null));
            form.addEventListener('submit', handleSubmit);

            // Đặt ngày tối thiểu là hôm nay
            const today = new Date().toISOString().split('T')[0];
            targetDate.setAttribute('min', today);

            updateCurrencySymbol();
            loadSavedData();
        }
    };

    // ============================================
    // BƯỚC HOÀN TẤT
    // ============================================
    const Complete = {
        init() {
            const btnGoToDashboard = document.getElementById('btnGoToDashboard');
            if (!btnGoToDashboard) return;

            const displaySummary = () => {
                try {
                    const wallet = JSON.parse(sessionStorage.getItem('onboarding_wallet') || '{}');
                    const categories = JSON.parse(sessionStorage.getItem('onboarding_categories') || '{}');
                    const savings = sessionStorage.getItem('onboarding_savings');

                    document.getElementById('walletName').textContent = wallet.name || 'Ví tiền mặt';

                    const categoryCounts = {
                        'Student': 8, 'Family': 8, 'Business': 8,
                        'Freelancer': 6, 'Minimal': 6
                    };
                    const categoryCount = categoryCounts[categories.template] || 6;
                    document.getElementById('categoryCount').textContent = categoryCount;
                    document.getElementById('goalCount').textContent = savings ? 1 : 0;
                } catch (error) {
                    console.error('Lỗi hiển thị tóm tắt:', error);
                }
            };

            const goToDashboard = () => {
                ['onboarding_profile', 'onboarding_wallet', 'onboarding_categories', 'onboarding_savings', 'currencySymbol'].forEach(key => {
                    sessionStorage.removeItem(key);
                });
                window.location.href = '/home';
            };

            const createConfetti = () => {
                const colors = ['#667eea', '#764ba2', '#4CAF50', '#FF5722', '#2196F3', '#FF9800'];
                const confettiCount = 50;

                for (let i = 0; i < confettiCount; i++) {
                    setTimeout(() => {
                        const confetti = document.createElement('div');
                        Object.assign(confetti.style, {
                            position: 'fixed',
                            width: '10px',
                            height: '10px',
                            backgroundColor: colors[Math.floor(Math.random() * colors.length)],
                            left: Math.random() * 100 + '%',
                            top: '-10px',
                            opacity: '1',
                            borderRadius: '50%',
                            pointerEvents: 'none',
                            zIndex: '9999',
                            transition: 'all 3s ease-out'
                        });

                        document.body.appendChild(confetti);

                        setTimeout(() => {
                            confetti.style.top = '100vh';
                            confetti.style.opacity = '0';
                            confetti.style.transform = `rotate(${Math.random() * 360}deg) translateX(${(Math.random() - 0.5) * 200}px)`;
                        }, 10);

                        setTimeout(() => confetti.remove(), 3000);
                    }, i * 50);
                }
            };

            btnGoToDashboard.addEventListener('click', goToDashboard);
            displaySummary();
            createConfetti();
        }
    };

    // ============================================
    // TỰ ĐỘNG KHỞI TẠO
    // ============================================
    const init = () => {
        Welcome.init();
        BasicSettings.init();
        CreateWallet.init();
        SetupCategories.init();
        SavingsGoal.init();
        Complete.init();
    };

    // Khởi tạo khi DOM sẵn sàng
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return { Welcome, BasicSettings, CreateWallet, SetupCategories, SavingsGoal, Complete };
})();
