/**
 * Subscription System Utilities
 * Reusable functions for subscription management
 */

/**
 * Session Storage Manager
 */
const SessionManager = {
    /**
     * Store selected package details
     */
    storePackage(packageData) {
        try {
            const packageInfo = {
                id: packageData.id,
                name: packageData.name,
                price: packageData.price,
                billingCycle: packageData.billingCycleName || packageData.billingCycle,
                timestamp: new Date().toISOString()
            };
            sessionStorage.setItem('selectedPackage', JSON.stringify(packageInfo));
            sessionStorage.setItem('pendingPackageSelection', JSON.stringify(packageInfo));
            return true;
        } catch (error) {
            console.error('Failed to store package:', error);
            return false;
        }
    },

    /**
     * Retrieve selected package
     */
    getPackage() {
        try {
            const data = sessionStorage.getItem('selectedPackage');
            return data ? JSON.parse(data) : null;
        } catch (error) {
            console.error('Failed to retrieve package:', error);
            return null;
        }
    },

    /**
     * Store payment information
     */
    storePayment(paymentData) {
        try {
            const paymentInfo = {
                paymentId: paymentData.paymentId,
                amount: paymentData.amount,
                currency: paymentData.currency || 'VND',
                timestamp: new Date().toISOString()
            };
            sessionStorage.setItem('currentPayment', JSON.stringify(paymentInfo));
            return true;
        } catch (error) {
            console.error('Failed to store payment:', error);
            return false;
        }
    },

    /**
     * Retrieve payment information
     */
    getPayment() {
        try {
            const data = sessionStorage.getItem('currentPayment');
            return data ? JSON.parse(data) : null;
        } catch (error) {
            console.error('Failed to retrieve payment:', error);
            return null;
        }
    },

    /**
     * Clear all subscription-related session data
     */
    clear() {
        try {
            sessionStorage.removeItem('selectedPackage');
            sessionStorage.removeItem('pendingPackageSelection');
            sessionStorage.removeItem('currentPayment');
            return true;
        } catch (error) {
            console.error('Failed to clear session:', error);
            return false;
        }
    }
};

/**
 * Toast Notification Manager
 */
const ToastManager = {
    /**
     * Show toast notification
     * @param {string} message - Message to display
     * @param {string} type - Type: 'success', 'error', 'warning', 'info'
     * @param {number} duration - Duration in milliseconds (default: 4000)
     */
    show(message, type = 'info', duration = 4000) {
        let toast = document.getElementById('toast');

        // Create toast if it doesn't exist
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'toast';
            toast.className = 'toast';
            document.body.appendChild(toast);
        }

        toast.textContent = message;
        toast.className = `toast ${type} show`;

        setTimeout(() => {
            toast.classList.remove('show');
        }, duration);
    },

    success(message, duration) {
        this.show(message, 'success', duration);
    },

    error(message, duration) {
        this.show(message, 'error', duration);
    },

    warning(message, duration) {
        this.show(message, 'warning', duration);
    },

    info(message, duration) {
        this.show(message, 'info', duration);
    }
};

/**
 * Loading Overlay Manager
 */
const LoadingManager = {
    /**
     * Show or hide loading overlay
     * @param {boolean} show - Whether to show the overlay
     * @param {string} message - Loading message
     */
    toggle(show, message = 'Đang xử lý...') {
        let overlay = document.getElementById('loading-overlay');

        // Create overlay if it doesn't exist
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'loading-overlay';
            overlay.className = 'loading-overlay';
            overlay.style.display = 'none';
            overlay.innerHTML = `
                <div class="loading-spinner">
                    <div class="spinner"></div>
                    <p id="loading-message">${message}</p>
                </div>
            `;
            document.body.appendChild(overlay);
        }

        const messageEl = document.getElementById('loading-message');
        if (messageEl && message) {
            messageEl.textContent = message;
        }

        overlay.style.display = show ? 'flex' : 'none';
    },

    show(message) {
        this.toggle(true, message);
    },

    hide() {
        this.toggle(false);
    }
};

/**
 * Authentication Manager
 */
const AuthManager = {
    /**
     * Check if user is authenticated
     */
    async isAuthenticated() {
        try {
            const response = await fetch('/api/Subscription/my-subscription', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            });
            return response.status !== 401;
        } catch (error) {
            console.error('Auth check failed:', error);
            return false;
        }
    },

    /**
     * Redirect to login with return URL
     */
    redirectToLogin(returnUrl = null) {
        const url = returnUrl || window.location.pathname + window.location.search;
        const encodedUrl = encodeURIComponent(url);
        window.location.href = `/Auth/Login?returnUrl=${encodedUrl}`;
    },

    /**
     * Check for active subscription
     */
    async hasActiveSubscription() {
        try {
            const response = await fetch('/api/Subscription/my-subscription', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (response.status === 404) {
                return false;
            }

            if (response.ok) {
                const subscription = await response.json();
                return subscription && subscription.status === 'Active';
            }

            return false;
        } catch (error) {
            console.error('Error checking subscription:', error);
            return false;
        }
    }
};

/**
 * API Helper
 */
const APIHelper = {
    /**
     * Make API request with retry logic
     * @param {string} url - API endpoint
     * @param {object} options - Fetch options
     * @param {number} maxRetries - Maximum retry attempts
     */
    async fetchWithRetry(url, options = {}, maxRetries = 3) {
        let lastError;

        for (let i = 0; i < maxRetries; i++) {
            try {
                const response = await fetch(url, {
                    ...options,
                    credentials: 'include',
                    headers: {
                        'Accept': 'application/json',
                        ...options.headers
                    }
                });

                if (!response.ok) {
                    const errorData = await response.json().catch(() => ({}));
                    throw new Error(errorData.message || `HTTP ${response.status}`);
                }

                return await response.json();
            } catch (error) {
                lastError = error;
                console.error(`Attempt ${i + 1} failed:`, error);

                if (i < maxRetries - 1) {
                    // Wait before retrying (exponential backoff)
                    await new Promise(resolve => setTimeout(resolve, Math.pow(2, i) * 1000));
                }
            }
        }

        throw lastError;
    },

    /**
     * Get all packages
     */
    async getPackages() {
        return await this.fetchWithRetry('/api/Subscription/packages');
    },

    /**
     * Get package by ID
     */
    async getPackage(packageId) {
        return await this.fetchWithRetry(`/api/Subscription/packages/${packageId}`);
    },

    /**
     * Create subscription
     */
    async createSubscription(packageId, autoRenew = true, returnUrl = null) {
        const url = returnUrl || window.location.origin + '/Subscription/PaymentCallback';

        return await this.fetchWithRetry('/api/Subscription/subscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                packageId,
                autoRenew,
                returnUrl: url
            })
        });
    }
};

/**
 * Utility Functions
 */
const Utils = {
    /**
     * Escape HTML to prevent XSS
     */
    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    /**
     * Format currency (VND)
     */
    formatCurrency(amount) {
        return amount.toLocaleString('vi-VN') + '₫';
    },

    /**
     * Format date
     */
    formatDate(date) {
        return new Date(date).toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    /**
     * Debounce function
     */
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
    },

    /**
     * Check if popup is blocked
     */
    isPopupBlocked(popupWindow) {
        return !popupWindow || popupWindow.closed || typeof popupWindow.closed === 'undefined';
    },

    /**
     * Open URL in new window with fallback
     */
    openWindow(url, windowName = '_blank', features = 'width=800,height=600') {
        const newWindow = window.open(url, windowName, features);

        if (this.isPopupBlocked(newWindow)) {
            ToastManager.warning('Vui lòng cho phép popup để mở trang mới');

            setTimeout(() => {
                if (confirm('Không thể mở cửa sổ mới. Chuyển đến trang?')) {
                    window.location.href = url;
                }
            }, 1000);

            return null;
        }

        return newWindow;
    }
};

/**
 * Payment Gateway Helper
 */
const PaymentGateway = {
    /**
     * Check gateway connectivity
     */
    async checkConnectivity(qrCodeUrl, timeout = 5000) {
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(qrCodeUrl, {
                method: 'HEAD',
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            return response.ok;
        } catch (error) {
            if (error.name === 'AbortError') {
                console.warn('Gateway connectivity check timeout');
                ToastManager.warning('Cảnh báo: Kết nối đến cổng thanh toán chậm');
            }
            return false;
        }
    },

    /**
     * Open payment window
     */
    openPayment(paymentUrl) {
        if (!paymentUrl) {
            ToastManager.error('URL thanh toán không hợp lệ');
            return null;
        }

        const paymentWindow = Utils.openWindow(paymentUrl, '_blank', 'width=800,height=600');

        if (paymentWindow) {
            ToastManager.info('Đã mở trang thanh toán. Vui lòng hoàn tất giao dịch.');

            // Monitor payment window
            const checkWindow = setInterval(() => {
                if (paymentWindow.closed) {
                    clearInterval(checkWindow);
                    ToastManager.info('Cửa sổ thanh toán đã đóng.');
                }
            }, 1000);
        }

        return paymentWindow;
    }
};

/**
 * Global Error Handler
 */
function initializeGlobalErrorHandlers() {
    window.addEventListener('error', function (event) {
        console.error('Global error:', event.error);
        ToastManager.error('Đã xảy ra lỗi. Vui lòng thử lại.');
    });

    window.addEventListener('unhandledrejection', function (event) {
        console.error('Unhandled promise rejection:', event.reason);
        ToastManager.error('Đã xảy ra lỗi. Vui lòng thử lại.');
    });
}

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeGlobalErrorHandlers);
} else {
    initializeGlobalErrorHandlers();
}

// Export for use in modules (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        SessionManager,
        ToastManager,
        LoadingManager,
        AuthManager,
        APIHelper,
        Utils,
        PaymentGateway
    };
}
