/**
 * Money Tracker Design System JavaScript
 * Modern Personal Finance Management App
 */

class DesignSystem {
    constructor() {
        this.init();
    }

    init() {
        this.setupThemeToggle();
        this.setupSidebar();
        this.setupNotifications();
        this.setupUserMenu();
        this.setupToastSystem();
        this.setupLoadingOverlay();
        this.setupAnimations();
        this.setupAccessibility();
    }

    // =============================================
    // THEME TOGGLE
    // =============================================
    setupThemeToggle() {
        const themeToggle = document.getElementById('theme-toggle');
        const themeIcon = document.querySelector('.theme-icon');

        if (themeToggle) {
            // Load saved theme
            const savedTheme = localStorage.getItem('theme') || 'light';
            this.setTheme(savedTheme);

            themeToggle.addEventListener('click', () => {
                const currentTheme = document.documentElement.getAttribute('data-theme');
                const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
                this.setTheme(newTheme);
            });
        }
    }

    setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);

        const themeIcon = document.querySelector('.theme-icon');
        if (themeIcon) {
            themeIcon.className = theme === 'dark' ? 'fas fa-sun theme-icon' : 'fas fa-moon theme-icon';
        }
    }

    // =============================================
    // SIDEBAR
    // =============================================
    setupSidebar() {
        const sidebarToggle = document.getElementById('sidebar-toggle');
        const sidebar = document.getElementById('sidebar');

        if (sidebarToggle && sidebar) {
            sidebarToggle.addEventListener('click', () => {
                sidebar.classList.toggle('open');
            });

            // Close sidebar when clicking outside on mobile
            document.addEventListener('click', (e) => {
                if (window.innerWidth <= 1024) {
                    if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                        sidebar.classList.remove('open');
                    }
                }
            });

            // Handle window resize
            window.addEventListener('resize', () => {
                if (window.innerWidth > 1024) {
                    sidebar.classList.remove('open');
                }
            });
        }

        // Set active navigation item
        this.setActiveNavItem();
    }

    setActiveNavItem() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.nav-link');

        navLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && currentPath.includes(href.replace('/', ''))) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });
    }

    // =============================================
    // NOTIFICATIONS
    // =============================================
    setupNotifications() {
        const notificationBtn = document.getElementById('notification-btn');
        const notificationDropdown = document.getElementById('notification-dropdown');

        if (notificationBtn && notificationDropdown) {
            notificationBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                notificationDropdown.classList.toggle('show');

                // Close user menu if open
                const userDropdown = document.getElementById('user-dropdown');
                if (userDropdown) {
                    userDropdown.classList.remove('show');
                }
            });

            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!notificationBtn.contains(e.target) && !notificationDropdown.contains(e.target)) {
                    notificationDropdown.classList.remove('show');
                }
            });
        }
    }

    // =============================================
    // USER MENU
    // =============================================
    setupUserMenu() {
        const userMenuBtn = document.getElementById('user-menu-btn');
        const userDropdown = document.getElementById('user-dropdown');

        if (userMenuBtn && userDropdown) {
            userMenuBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                userDropdown.classList.toggle('show');

                // Close notification dropdown if open
                const notificationDropdown = document.getElementById('notification-dropdown');
                if (notificationDropdown) {
                    notificationDropdown.classList.remove('show');
                }
            });

            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!userMenuBtn.contains(e.target) && !userDropdown.contains(e.target)) {
                    userDropdown.classList.remove('show');
                }
            });
        }
    }

    // =============================================
    // TOAST SYSTEM
    // =============================================
    setupToastSystem() {
        this.toastContainer = document.getElementById('toast-container');

        // Global toast function
        window.showToast = (message, type = 'info', title = '') => {
            this.showToast(message, type, title);
        };
    }

    showToast(message, type = 'info', title = '') {
        if (!this.toastContainer) return;

        const toast = document.createElement('div');
        toast.className = 'toast';

        const iconMap = {
            success: 'fas fa-check',
            error: 'fas fa-times',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info'
        };

        toast.innerHTML = `
            <div class="toast-icon ${type}">
                <i class="${iconMap[type] || iconMap.info}"></i>
            </div>
            <div class="toast-content">
                ${title ? `<div class="toast-title">${title}</div>` : ''}
                <div class="toast-message">${message}</div>
            </div>
        `;

        this.toastContainer.appendChild(toast);

        // Trigger animation
        setTimeout(() => toast.classList.add('show'), 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 5000);
    }

    // =============================================
    // LOADING OVERLAY
    // =============================================
    setupLoadingOverlay() {
        this.loadingOverlay = document.getElementById('loading-overlay');

        // Global loading functions
        window.showLoading = (message = 'Đang tải...') => {
            this.showLoading(message);
        };

        window.hideLoading = () => {
            this.hideLoading();
        };
    }

    showLoading(message = 'Đang tải...') {
        if (!this.loadingOverlay) return;

        const loadingText = this.loadingOverlay.querySelector('.loading-text');
        if (loadingText) {
            loadingText.textContent = message;
        }

        this.loadingOverlay.classList.add('show');
    }

    hideLoading() {
        if (!this.loadingOverlay) return;
        this.loadingOverlay.classList.remove('show');
    }

    // =============================================
    // ANIMATIONS
    // =============================================
    setupAnimations() {
        // Intersection Observer for fade-in animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in');
                }
            });
        }, observerOptions);

        // Observe cards and stat cards
        document.querySelectorAll('.card, .stat-card, .chart-container').forEach(el => {
            observer.observe(el);
        });

        // Add hover effects to interactive elements
        this.setupHoverEffects();
    }

    setupHoverEffects() {
        // Card hover effects
        document.querySelectorAll('.card-interactive').forEach(card => {
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-4px)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });
        });

        // Button hover effects
        document.querySelectorAll('.btn').forEach(btn => {
            btn.addEventListener('mouseenter', function () {
                if (!this.disabled) {
                    this.style.transform = 'translateY(-1px)';
                }
            });

            btn.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });
        });
    }

    // =============================================
    // ACCESSIBILITY
    // =============================================
    setupAccessibility() {
        // Keyboard navigation for dropdowns
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                // Close all dropdowns
                document.querySelectorAll('.notification-dropdown.show, .user-dropdown.show').forEach(dropdown => {
                    dropdown.classList.remove('show');
                });
            }
        });

        // Focus management
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Tab') {
                document.body.classList.add('keyboard-navigation');
            }
        });

        document.addEventListener('mousedown', () => {
            document.body.classList.remove('keyboard-navigation');
        });

        // ARIA labels and roles
        this.setupARIA();
    }

    setupARIA() {
        // Add ARIA labels to interactive elements
        const notificationBtn = document.getElementById('notification-btn');
        if (notificationBtn) {
            notificationBtn.setAttribute('aria-label', 'Thông báo');
            notificationBtn.setAttribute('aria-expanded', 'false');
        }

        const userMenuBtn = document.getElementById('user-menu-btn');
        if (userMenuBtn) {
            userMenuBtn.setAttribute('aria-label', 'Menu người dùng');
            userMenuBtn.setAttribute('aria-expanded', 'false');
        }

        const themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            themeToggle.setAttribute('aria-label', 'Chuyển đổi chế độ sáng/tối');
        }
    }

    // =============================================
    // UTILITY METHODS
    // =============================================

    // Format currency
    formatCurrency(amount, currency = 'VND') {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: currency
        }).format(amount);
    }

    // Format date
    formatDate(date, options = {}) {
        const defaultOptions = {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        };

        return new Intl.DateTimeFormat('vi-VN', { ...defaultOptions, ...options }).format(new Date(date));
    }

    // Debounce function
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Throttle function
    throttle(func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }
}

// =============================================
// CHART UTILITIES
// =============================================
class ChartUtils {
    static getDefaultOptions() {
        return {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: {
                            family: 'Inter, sans-serif',
                            size: 12
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            family: 'Inter, sans-serif',
                            size: 11
                        }
                    }
                },
                y: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    },
                    ticks: {
                        font: {
                            family: 'Inter, sans-serif',
                            size: 11
                        }
                    }
                }
            }
        };
    }

    static getColorPalette() {
        return {
            primary: '#1A3A5A',
            success: '#2ECC71',
            danger: '#E74C3C',
            warning: '#F39C12',
            info: '#3498DB',
            purple: '#9B59B6',
            teal: '#1ABC9C'
        };
    }

    static createLineChart(ctx, data, options = {}) {
        const defaultOptions = this.getDefaultOptions();
        return new Chart(ctx, {
            type: 'line',
            data: data,
            options: { ...defaultOptions, ...options }
        });
    }

    static createBarChart(ctx, data, options = {}) {
        const defaultOptions = this.getDefaultOptions();
        return new Chart(ctx, {
            type: 'bar',
            data: data,
            options: { ...defaultOptions, ...options }
        });
    }

    static createDoughnutChart(ctx, data, options = {}) {
        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: {
                            family: 'Inter, sans-serif',
                            size: 12
                        }
                    }
                }
            }
        };
        return new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: { ...defaultOptions, ...options }
        });
    }
}

// =============================================
// FORM UTILITIES
// =============================================
class FormUtils {
    static validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    static validateRequired(value) {
        return value && value.trim().length > 0;
    }

    static validateMinLength(value, minLength) {
        return value && value.length >= minLength;
    }

    static validateNumber(value) {
        return !isNaN(value) && isFinite(value);
    }

    static formatInputValue(input, type) {
        switch (type) {
            case 'currency':
                return input.value.replace(/[^\d]/g, '');
            case 'number':
                return input.value.replace(/[^\d.]/g, '');
            default:
                return input.value;
        }
    }
}

// =============================================
// INITIALIZATION
// =============================================
document.addEventListener('DOMContentLoaded', () => {
    // Initialize design system
    window.designSystem = new DesignSystem();

    // Make utilities globally available
    window.ChartUtils = ChartUtils;
    window.FormUtils = FormUtils;

    // Initialize page-specific functionality
    if (typeof initializePage === 'function') {
        initializePage();
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { DesignSystem, ChartUtils, FormUtils };
}
