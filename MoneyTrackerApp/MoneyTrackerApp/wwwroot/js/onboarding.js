const OnboardingApp = (function () {
    'use strict';

    // State
    const state = {
        currentStep: 0, // 0=Welcome, 1=Settings, 2=Wallet, 3=Category, 4=Goal, 5=Complete
        profile: { currency: 'VND', language: 'vi', theme: 'light' },
        wallet: { name: 'Ví tiền mặt', accountType: 0, initialBalance: 0, color: '#4CAF50', icon: '💵' },
        categorySetup: { template: 'Student', customCategories: [] },
        savingsGoal: null
    };

    // DOM Elements
    const elements = {
        app: document.getElementById('onboardingApp'),
        progressBar: document.getElementById('progressBarContainer'),
        progressFill: document.getElementById('progressFill'),
        views: {
            welcome: document.getElementById('view-welcome'),
            step1: document.getElementById('view-step1'),
            step2: document.getElementById('view-step2'),
            step3: document.getElementById('view-step3'),
            step4: document.getElementById('view-step4'),
            complete: document.getElementById('view-complete')
        },
        forms: {
            step1: document.getElementById('formStep1'),
            step2: document.getElementById('formStep2'),
            step3: document.getElementById('formStep3'),
            step4: document.getElementById('formStep4')
        },
        toast: document.getElementById('toast'),
        summary: {
            wallet: document.getElementById('summaryWallet'),
            categories: document.getElementById('summaryCategories')
        }
    };

    // Helpers
    const showToast = (msg, type = 'error') => {
        elements.toast.textContent = msg;
        elements.toast.className = `toast-message visible ${type}`;
        setTimeout(() => {
            elements.toast.className = 'toast-message hidden';
        }, 3000);
    };

    const updateProgress = (step) => {
        // Steps 1-4 mapped to 25%, 50%, 75%, 100%
        // Step 0 (Welcome) and 5 (Complete) hide progress bar
        if (step === 0 || step === 5) {
            elements.progressBar.classList.add('hidden');
        } else {
            elements.progressBar.classList.remove('hidden');
            const pct = ((step) / 4) * 100; // 1->25, 2->50, 3->75, 4->100
            elements.progressFill.style.width = `${pct}%`;
        }
    };

    const showView = (viewName) => {
        // Hide all
        Object.values(elements.views).forEach(el => el.classList.add('hidden'));
        Object.values(elements.views).forEach(el => el.classList.remove('active'));

        // Show target
        const target = elements.views[viewName];
        if (target) {
            target.classList.remove('hidden');
            // Small delay for animation class if needed, but strict show/hide is fine
            // target.classList.add('active'); 
        }

        // Map view to state step
        const stepMap = { 'welcome': 0, 'step1': 1, 'step2': 2, 'step3': 3, 'step4': 4, 'complete': 5 };
        const step = stepMap[viewName];
        updateProgress(step);
    };

    // API
    const api = {
        getStatus: async () => {
            const res = await fetch('/api/onboarding/status');
            if (res.ok) return await res.json();
            return null;
        },
        updateStep: async (step, data) => {
            await fetch('/api/onboarding/step', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ step, stepData: JSON.stringify(data) })
            });
        },
        getTemplates: async (template) => {
            const res = await fetch(`/api/onboarding/templates/${template}`);
            if (res.ok) return await res.json();
            return [];
        },
        calculateSavings: async (amount, date) => {
            const res = await fetch('/api/onboarding/calculate-savings', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ targetAmount: amount, targetDate: date })
            });
            if (res.ok) return await res.json();
            return null;
        },
        complete: async (data) => {
            const res = await fetch('/api/onboarding/complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            // Returns { success, accessToken, refreshToken, message } possibly handled by controller returning Ok/BadRequest
            if (!res.ok) {
                const err = await res.json();
                throw new Error(err.message || 'Error completing onboarding');
            }
            return await res.json();
        }
    };

    // Logic for Steps
    const initWelcome = () => {
        const slides = document.querySelectorAll('.slide');
        const indicators = document.querySelectorAll('.indicator');
        let curSlide = 0;

        const updateSlide = (idx) => {
            slides.forEach((s, i) => {
                s.classList.toggle('active', i === idx);
            });
            indicators.forEach((ind, i) => {
                ind.classList.toggle('active', i === idx);
            });

            const isLast = idx === slides.length - 1;
            document.getElementById('btnNextSlide').classList.toggle('hidden', isLast);
            document.getElementById('btnStartOnboarding').classList.toggle('hidden', !isLast);
        };

        document.getElementById('btnNextSlide').onclick = () => {
            if (curSlide < slides.length - 1) updateSlide(++curSlide);
        };
        document.getElementById('btnSkipWelcome').onclick = () => {
            showView('step1');
        };
        document.getElementById('btnStartOnboarding').onclick = () => {
            // Can save step 1 start to API?
            showView('step1');
        };
        indicators.forEach((ind, i) => {
            ind.onclick = () => { curSlide = i; updateSlide(i); };
        });
    };

    const initStep1 = () => {
        // Theme Selection
        const themes = document.querySelectorAll('.theme-option');
        themes.forEach(t => {
            t.onclick = () => {
                themes.forEach(x => x.classList.remove('active'));
                t.classList.add('active');
                state.profile.theme = t.dataset.theme;
            };
        });

        elements.forms.step1.onsubmit = async (e) => {
            e.preventDefault();
            state.profile.currency = document.getElementById('currency').value;
            state.profile.language = document.getElementById('language').value;

            // Save & Next
            try {
                await api.updateStep(1, state.profile);
                showView('step2');
            } catch (err) {
                showToast('Không thể lưu bước này');
            }
        };
    };

    const initStep2 = () => {
        // Wallet Types
        const types = document.querySelectorAll('.wallet-type-card');
        types.forEach(t => {
            t.onclick = () => {
                types.forEach(x => x.classList.remove('active'));
                t.classList.add('active');
                state.wallet.accountType = parseInt(t.dataset.type);
                state.wallet.icon = t.querySelector('.type-icon').textContent;
            };
        });

        // Colors
        const colors = document.querySelectorAll('.color-option');
        colors.forEach(c => {
            c.onclick = () => {
                colors.forEach(x => x.classList.remove('selected'));
                c.classList.add('selected');
                state.wallet.color = c.dataset.color;
            };
        });

        elements.forms.step2.onsubmit = async (e) => {
            e.preventDefault();
            state.wallet.name = document.getElementById('walletName').value;
            state.wallet.initialBalance = parseFloat(document.getElementById('initialBalance').value) || 0;

            try {
                await api.updateStep(2, state.wallet);
                showView('step3');
            } catch (err) {
                showToast('Không thể lưu bước này');
            }
        };
    };

    const initStep3 = () => {
        const templates = document.querySelectorAll('.template-card');
        const preview = document.getElementById('categoryPreview');

        const loadPreview = async (tpl) => {
            preview.innerHTML = '<div class="preview-loading">Đang tải...</div>';
            state.categorySetup.template = tpl;
            const cats = await api.getTemplates(tpl);

            preview.innerHTML = '';
            cats.forEach(c => {
                const div = document.createElement('div');
                div.className = 'category-item';
                div.innerHTML = `
                    <div class="category-icon" style="background-color:${c.color}">${c.icon || '🏷️'}</div>
                    <div class="category-info">
                        <div class="category-name">${c.name}</div>
                        <div class="category-type">${c.type === 0 ? 'Chi tiêu' : 'Thu nhập'}</div>
                    </div>
                `;
                preview.appendChild(div);
            });
        };

        templates.forEach(t => {
            t.onclick = () => {
                templates.forEach(x => x.classList.remove('active'));
                t.classList.add('active');
                loadPreview(t.dataset.template);
            };
        });

        // Initial Load
        loadPreview('Student');

        elements.forms.step3.onsubmit = async (e) => {
            e.preventDefault();
            try {
                await api.updateStep(3, state.categorySetup);
                showView('step4');
            } catch (err) {
                showToast('Không thể lưu bước này');
            }
        };
    };

    const initStep4 = () => {
        const calcCard = document.getElementById('goalCalcCard');
        const monthlySpan = document.getElementById('monthlySavings');

        const doCalc = async () => {
            const amt = parseFloat(document.getElementById('targetAmount').value);
            const date = document.getElementById('targetDate').value;
            if (amt > 0 && date) {
                const res = await api.calculateSavings(amt, date);
                if (res) {
                    calcCard.classList.remove('hidden');
                    // Format currency
                    const fmt = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: state.profile.currency || 'VND' }).format(res.monthlyAmount);
                    monthlySpan.textContent = fmt;
                }
            } else {
                calcCard.classList.add('hidden');
            }
        };

        document.getElementById('targetAmount').oninput = doCalc;
        document.getElementById('targetDate').onchange = doCalc;

        const finish = async (skipGoal) => {
            if (!skipGoal) {
                const name = document.getElementById('goalName').value;
                const amt = parseFloat(document.getElementById('targetAmount').value);
                const date = document.getElementById('targetDate').value;

                if (name && amt > 0 && date) {
                    state.savingsGoal = {
                        name, targetAmount: amt, targetDate: date,
                        icon: '🎯', color: '#2196F3'
                    };
                }
            }

            // Calls Complete API
            const payload = {
                profile: state.profile,
                wallet: state.wallet,
                categorySetup: state.categorySetup,
                savingsGoal: state.savingsGoal
            };

            try {
                const res = await api.complete(payload);
                // Token handling if needed
                if (res.accessToken) {
                    // Though cookies are set HttpOnly, sometimes client logic needs refresh
                }

                // Show Summary
                elements.summary.wallet.textContent = `${state.wallet.name} (${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: state.profile.currency }).format(state.wallet.initialBalance)})`;
                elements.summary.categories.textContent = `${state.categorySetup.template}`;

                showView('complete');
            } catch (err) {
                showToast(err.message || 'Lỗi khi hoàn tất');
            }
        };

        elements.forms.step4.onsubmit = (e) => {
            e.preventDefault();
            finish(false);
        };

        document.getElementById('skipGoal').onclick = () => finish(true);
    };

    const init = async () => {
        initWelcome();
        initStep1();
        initStep2();
        initStep3();
        initStep4();

        // Check loaded status
        try {
            const status = await api.getStatus();
            if (status) {
                // Populate state from backend
                if (status.profile) state.profile = status.profile;
                if (status.wallet) state.wallet = status.wallet;
                if (status.categorySetup) state.categorySetup = status.categorySetup;
                if (status.savingsGoal) state.savingsGoal = status.savingsGoal;

                // Sync UI elements with loaded state could be added here (e.g. setting input values)
                // For simplicity, we just ensure the data is ready for the next steps.
                // ideally we should also pre-fill the forms if the user goes 'Back'.

                // Pre-fill forms based on loaded state
                if (state.profile) {
                    document.getElementById('currency').value = state.profile.currency || 'VND';
                    document.getElementById('language').value = state.profile.language || 'vi';
                    const themeOpt = document.querySelector(`.theme-option[data-theme="${state.profile.theme}"]`);
                    if (themeOpt) themeOpt.click();
                }

                if (state.wallet) {
                    document.getElementById('walletName').value = state.wallet.name || 'Ví tiền mặt';
                    document.getElementById('initialBalance').value = state.wallet.initialBalance || 0;
                    const typeCard = document.querySelector(`.wallet-type-card[data-type="${state.wallet.accountType}"]`);
                    if (typeCard) typeCard.click();
                    const colorOpt = document.querySelector(`.color-option[data-color="${state.wallet.color}"]`);
                    if (colorOpt) colorOpt.click();
                }

                if (state.categorySetup) {
                    const tpl = state.categorySetup.template || 'Student';
                    const tplCard = document.querySelector(`.template-card[data-template="${tpl}"]`);
                    if (tplCard) tplCard.click();
                }

                if (state.savingsGoal) {
                    document.getElementById('goalName').value = state.savingsGoal.name || '';
                    document.getElementById('targetAmount').value = state.savingsGoal.targetAmount || '';
                    if (state.savingsGoal.targetDate) {
                        document.getElementById('targetDate').value = state.savingsGoal.targetDate.split('T')[0];
                        // Trigger calc
                        document.getElementById('targetAmount').dispatchEvent(new Event('input'));
                    }
                }

                if (status.isCompleted) {
                    showView('complete');
                    return;
                }

                // Determine step to show based on what is missing or saved CurrentStep
                // Map API CurrentStep to View
                // 0=Welcome, 1=Settings, 2=Wallet, 3=Categories, 4=Goal
                // The API stores CurrentStep.
                const viewMap = { 0: 'welcome', 1: 'step1', 2: 'step2', 3: 'step3', 4: 'step4', 5: 'complete' };
                // Default to Welcome if 0
                const nextView = viewMap[status.currentStep] || 'welcome';
                showView(nextView);
            }
        } catch (e) {
            console.error(e);
            showView('welcome');
        }
    };

    return { init, showView };
})();

document.addEventListener('DOMContentLoaded', OnboardingApp.init);
