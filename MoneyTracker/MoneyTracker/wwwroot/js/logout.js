/**
 * Logout functionality for MoneyTracker
 * Handles user logout with proper cleanup and API calls
 */

class LogoutManager {
    constructor() {
        this.isLoggingOut = false;
    }

    /**
     * Perform logout with confirmation
     * @param {boolean} showConfirmation - Whether to show confirmation dialog
     */
    async logout(showConfirmation = true) {
        if (this.isLoggingOut) {
            return;
        }

        if (showConfirmation) {
            const confirmed = await this.showLogoutConfirmation();
            if (!confirmed) {
                return;
            }
        }

        await this.performLogout();
    }

    /**
     * Show logout confirmation dialog
     * @returns {Promise<boolean>} - User confirmation
     */
    async showLogoutConfirmation() {
        return new Promise((resolve) => {
            // Create modal
            const modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.innerHTML = `
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header border-0">
                            <h5 class="modal-title">
                                <i class="fas fa-sign-out-alt text-warning me-2"></i>
                                Xác nhận đăng xuất
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body text-center">
                            <p class="mb-3">Bạn có chắc chắn muốn đăng xuất khỏi tài khoản?</p>
                            <div class="d-flex justify-content-center gap-2">
                                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">
                                    <i class="fas fa-times me-2"></i>Hủy
                                </button>
                                <button type="button" class="btn btn-warning" id="confirmLogout">
                                    <i class="fas fa-sign-out-alt me-2"></i>Đăng xuất
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);
            const bsModal = new bootstrap.Modal(modal);

            // Handle confirm button
            modal.querySelector('#confirmLogout').addEventListener('click', () => {
                bsModal.hide();
                resolve(true);
            });

            // Handle cancel
            modal.addEventListener('hidden.bs.modal', () => {
                document.body.removeChild(modal);
                resolve(false);
            });

            bsModal.show();
        });
    }

    /**
     * Perform the actual logout process
     */
    async performLogout() {
        this.isLoggingOut = true;

        try {
            // Show loading indicator
            this.showLogoutLoading();

            // Call logout API
            const token = localStorage.getItem('authToken');
            if (token) {
                try {
                    await fetch('/api/auth/logout', {
                        method: 'POST',
                        headers: {
                            'Authorization': 'Bearer ' + token,
                            'Content-Type': 'application/json'
                        }
                    });
                } catch (error) {
                    console.warn('Logout API call failed:', error);
                    // Continue with logout even if API fails
                }
            }

            // Clear all stored data
            this.clearUserData();

            // Show success message
            this.showLogoutSuccess();

            // Redirect after delay
            setTimeout(() => {
                window.location.href = '/HomePage';
            }, 1500);

        } catch (error) {
            console.error('Logout error:', error);
            this.showLogoutError();
        } finally {
            this.isLoggingOut = false;
        }
    }

    /**
     * Clear all user-related data from storage
     */
    clearUserData() {
        // Clear localStorage
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        localStorage.removeItem('userPreferences');
        localStorage.removeItem('dashboardData');
        localStorage.removeItem('expenseData');
        localStorage.removeItem('incomeData');

        // Clear sessionStorage
        sessionStorage.clear();

        // Clear any cookies (if any)
        document.cookie.split(";").forEach(function (c) {
            document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
        });
    }

    /**
     * Show loading indicator during logout
     */
    showLogoutLoading() {
        // Create or update loading toast
        const toast = this.createToast('Đang đăng xuất...', 'info', 'fas fa-spinner fa-spin');
        this.showToast(toast);
    }

    /**
     * Show logout success message
     */
    showLogoutSuccess() {
        const toast = this.createToast('Đăng xuất thành công!', 'success', 'fas fa-check-circle');
        this.showToast(toast);
    }

    /**
     * Show logout error message
     */
    showLogoutError() {
        const toast = this.createToast('Có lỗi xảy ra khi đăng xuất', 'error', 'fas fa-exclamation-triangle');
        this.showToast(toast);
    }

    /**
     * Create toast notification
     * @param {string} message - Toast message
     * @param {string} type - Toast type (success, error, info, warning)
     * @param {string} icon - Font Awesome icon class
     * @returns {HTMLElement} - Toast element
     */
    createToast(message, type, icon) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="${icon} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        return toast;
    }

    /**
     * Show toast notification
     * @param {HTMLElement} toast - Toast element
     */
    showToast(toast) {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        container.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast, { autohide: true, delay: 3000 });
        bsToast.show();

        // Remove toast element after it's hidden
        toast.addEventListener('hidden.bs.toast', () => {
            if (container.contains(toast)) {
                container.removeChild(toast);
            }
        });
    }

    /**
     * Quick logout without confirmation (for emergency logout)
     */
    async quickLogout() {
        await this.logout(false);
    }

    /**
     * Logout and redirect to specific page
     * @param {string} redirectUrl - URL to redirect to after logout
     */
    async logoutAndRedirect(redirectUrl) {
        await this.logout(false);
        setTimeout(() => {
            window.location.href = redirectUrl;
        }, 1000);
    }
}

// Create global instance
window.logoutManager = new LogoutManager();

// Global logout function for backward compatibility
window.logout = function () {
    window.logoutManager.logout();
};

// Auto-logout on token expiry
window.addEventListener('storage', function (e) {
    if (e.key === 'authToken' && !e.newValue) {
        // Token was removed, redirect to login
        window.location.href = '/HomePage';
    }
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LogoutManager;
}
