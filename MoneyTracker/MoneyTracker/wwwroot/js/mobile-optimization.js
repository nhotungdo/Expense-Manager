// Mobile Optimization JavaScript

class MobileOptimizer {
    constructor() {
        this.isMobile = window.innerWidth <= 768;
        this.touchStartX = 0;
        this.touchStartY = 0;
        this.touchEndX = 0;
        this.touchEndY = 0;
        this.swipeThreshold = 50;
        this.pullToRefreshThreshold = 100;
        this.isPulling = false;

        this.init();
    }

    init() {
        if (this.isMobile) {
            this.setupMobileFeatures();
            this.setupTouchGestures();
            this.setupPullToRefresh();
            this.setupSwipeActions();
            this.setupMobileMenu();
            this.setupFloatingActionButton();
            this.setupResponsiveCharts();
        }

        this.setupResizeHandler();
    }

    setupMobileFeatures() {
        // Add mobile class to body
        document.body.classList.add('mobile-device');

        // Prevent zoom on input focus
        const inputs = document.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('focus', () => {
                if (input.style.fontSize !== '16px') {
                    input.style.fontSize = '16px';
                }
            });
        });

        // Add touch-friendly classes
        const buttons = document.querySelectorAll('button, .btn, .nav-link');
        buttons.forEach(button => {
            button.classList.add('touch-target');
        });

        // Optimize images for mobile
        const images = document.querySelectorAll('img');
        images.forEach(img => {
            img.setAttribute('loading', 'lazy');
        });
    }

    setupTouchGestures() {
        // Swipe navigation
        document.addEventListener('touchstart', (e) => {
            this.touchStartX = e.touches[0].clientX;
            this.touchStartY = e.touches[0].clientY;
        });

        document.addEventListener('touchend', (e) => {
            this.touchEndX = e.changedTouches[0].clientX;
            this.touchEndY = e.changedTouches[0].clientY;
            this.handleSwipe();
        });

        // Long press for context menu
        let longPressTimer;
        document.addEventListener('touchstart', (e) => {
            longPressTimer = setTimeout(() => {
                this.handleLongPress(e);
            }, 500);
        });

        document.addEventListener('touchend', () => {
            clearTimeout(longPressTimer);
        });

        document.addEventListener('touchmove', () => {
            clearTimeout(longPressTimer);
        });
    }

    setupPullToRefresh() {
        const pullToRefreshContainer = document.querySelector('.pull-to-refresh');
        if (!pullToRefreshContainer) return;

        let startY = 0;
        let currentY = 0;
        let isPulling = false;

        pullToRefreshContainer.addEventListener('touchstart', (e) => {
            if (pullToRefreshContainer.scrollTop === 0) {
                startY = e.touches[0].clientY;
                isPulling = true;
            }
        });

        pullToRefreshContainer.addEventListener('touchmove', (e) => {
            if (!isPulling) return;

            currentY = e.touches[0].clientY;
            const pullDistance = currentY - startY;

            if (pullDistance > 0) {
                e.preventDefault();
                pullToRefreshContainer.style.transform = `translateY(${Math.min(pullDistance * 0.5, 100)}px)`;

                const indicator = document.querySelector('.pull-to-refresh-indicator');
                if (indicator) {
                    if (pullDistance > this.pullToRefreshThreshold) {
                        indicator.classList.add('show');
                        indicator.textContent = 'Release to refresh';
                    } else {
                        indicator.textContent = 'Pull to refresh';
                    }
                }
            }
        });

        pullToRefreshContainer.addEventListener('touchend', () => {
            if (!isPulling) return;

            const pullDistance = currentY - startY;
            if (pullDistance > this.pullToRefreshThreshold) {
                this.refreshData();
            }

            pullToRefreshContainer.style.transform = '';
            isPulling = false;
        });
    }

    setupSwipeActions() {
        const swipeableItems = document.querySelectorAll('.swipeable');

        swipeableItems.forEach(item => {
            let startX = 0;
            let currentX = 0;
            let isSwipeActive = false;

            item.addEventListener('touchstart', (e) => {
                startX = e.touches[0].clientX;
                isSwipeActive = true;
            });

            item.addEventListener('touchmove', (e) => {
                if (!isSwipeActive) return;

                currentX = e.touches[0].clientX;
                const swipeDistance = currentX - startX;

                if (Math.abs(swipeDistance) > 10) {
                    e.preventDefault();
                    item.style.transform = `translateX(${swipeDistance}px)`;
                }
            });

            item.addEventListener('touchend', () => {
                if (!isSwipeActive) return;

                const swipeDistance = currentX - startX;
                const swipeActions = item.querySelector('.swipe-actions');

                if (Math.abs(swipeDistance) > this.swipeThreshold) {
                    if (swipeDistance < 0 && swipeActions) {
                        // Swipe left - show actions
                        swipeActions.classList.add('show');
                        item.style.transform = 'translateX(-80px)';
                    } else {
                        // Swipe right - hide actions
                        this.hideSwipeActions();
                    }
                } else {
                    // Reset position
                    item.style.transform = '';
                }

                isSwipeActive = false;
            });
        });
    }

    setupMobileMenu() {
        const menuToggle = document.querySelector('.navbar-toggler');
        const sidebar = document.querySelector('.sidebar');
        const sidebarOverlay = document.querySelector('.sidebar-overlay');

        if (menuToggle && sidebar) {
            menuToggle.addEventListener('click', () => {
                sidebar.classList.toggle('show');
                if (sidebarOverlay) {
                    sidebarOverlay.classList.toggle('show');
                }
                document.body.classList.toggle('sidebar-open');
            });
        }

        if (sidebarOverlay) {
            sidebarOverlay.addEventListener('click', () => {
                this.closeSidebar();
            });
        }

        // Close sidebar when clicking on links
        const sidebarLinks = document.querySelectorAll('.sidebar .nav-link');
        sidebarLinks.forEach(link => {
            link.addEventListener('click', () => {
                this.closeSidebar();
            });
        });
    }

    setupFloatingActionButton() {
        const fab = document.querySelector('.fab');
        if (!fab) return;

        // Add click handler for FAB
        fab.addEventListener('click', () => {
            this.handleFABClick();
        });

        // Add hover effect for desktop
        fab.addEventListener('mouseenter', () => {
            fab.style.transform = 'scale(1.1)';
        });

        fab.addEventListener('mouseleave', () => {
            fab.style.transform = 'scale(1)';
        });
    }

    setupResponsiveCharts() {
        const charts = document.querySelectorAll('.chart-container canvas');

        charts.forEach(chart => {
            // Resize chart on orientation change
            window.addEventListener('orientationchange', () => {
                setTimeout(() => {
                    this.resizeChart(chart);
                }, 100);
            });

            // Resize chart on window resize
            window.addEventListener('resize', () => {
                this.resizeChart(chart);
            });
        });
    }

    setupResizeHandler() {
        window.addEventListener('resize', () => {
            const wasMobile = this.isMobile;
            this.isMobile = window.innerWidth <= 768;

            if (wasMobile !== this.isMobile) {
                // Device type changed
                if (this.isMobile) {
                    this.setupMobileFeatures();
                } else {
                    this.cleanupMobileFeatures();
                }
            }
        });
    }

    handleSwipe() {
        const deltaX = this.touchEndX - this.touchStartX;
        const deltaY = this.touchEndY - this.touchStartY;

        if (Math.abs(deltaX) > Math.abs(deltaY)) {
            // Horizontal swipe
            if (Math.abs(deltaX) > this.swipeThreshold) {
                if (deltaX > 0) {
                    this.handleSwipeRight();
                } else {
                    this.handleSwipeLeft();
                }
            }
        } else {
            // Vertical swipe
            if (Math.abs(deltaY) > this.swipeThreshold) {
                if (deltaY > 0) {
                    this.handleSwipeDown();
                } else {
                    this.handleSwipeUp();
                }
            }
        }
    }

    handleSwipeLeft() {
        // Navigate to next page or show more options
        console.log('Swipe left detected');
    }

    handleSwipeRight() {
        // Navigate to previous page or go back
        if (window.history.length > 1) {
            window.history.back();
        }
    }

    handleSwipeUp() {
        // Scroll up or show more content
        window.scrollBy(0, -100);
    }

    handleSwipeDown() {
        // Scroll down or refresh
        window.scrollBy(0, 100);
    }

    handleLongPress(e) {
        // Show context menu or additional options
        const contextMenu = this.createContextMenu(e);
        document.body.appendChild(contextMenu);

        // Remove context menu after 3 seconds
        setTimeout(() => {
            if (contextMenu.parentNode) {
                contextMenu.parentNode.removeChild(contextMenu);
            }
        }, 3000);
    }

    createContextMenu(e) {
        const menu = document.createElement('div');
        menu.className = 'context-menu';
        menu.style.cssText = `
            position: fixed;
            top: ${e.touches[0].clientY}px;
            left: ${e.touches[0].clientX}px;
            background: white;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
            z-index: 1000;
            padding: 8px 0;
        `;

        menu.innerHTML = `
            <div class="context-menu-item" onclick="this.copyToClipboard()">Copy</div>
            <div class="context-menu-item" onclick="this.share()">Share</div>
            <div class="context-menu-item" onclick="this.bookmark()">Bookmark</div>
        `;

        return menu;
    }

    refreshData() {
        // Show loading indicator
        const indicator = document.querySelector('.pull-to-refresh-indicator');
        if (indicator) {
            indicator.textContent = 'Refreshing...';
        }

        // Simulate data refresh
        setTimeout(() => {
            if (indicator) {
                indicator.textContent = 'Refreshed!';
                setTimeout(() => {
                    indicator.classList.remove('show');
                }, 1000);
            }

            // Reload page or refresh data
            window.location.reload();
        }, 1000);
    }

    hideSwipeActions() {
        const swipeActions = document.querySelectorAll('.swipe-actions');
        const swipeableItems = document.querySelectorAll('.swipeable');

        swipeActions.forEach(action => {
            action.classList.remove('show');
        });

        swipeableItems.forEach(item => {
            item.style.transform = '';
        });
    }

    closeSidebar() {
        const sidebar = document.querySelector('.sidebar');
        const sidebarOverlay = document.querySelector('.sidebar-overlay');

        if (sidebar) {
            sidebar.classList.remove('show');
        }

        if (sidebarOverlay) {
            sidebarOverlay.classList.remove('show');
        }

        document.body.classList.remove('sidebar-open');
    }

    handleFABClick() {
        // Show quick add modal or navigate to add page
        const addModal = document.querySelector('#addTransactionModal');
        if (addModal) {
            const modal = new bootstrap.Modal(addModal);
            modal.show();
        } else {
            // Navigate to add page
            window.location.href = '/Expenses/Create';
        }
    }

    resizeChart(chart) {
        // Resize chart to fit container
        const container = chart.closest('.chart-container');
        if (container) {
            const width = container.clientWidth;
            const height = container.clientHeight;

            chart.width = width;
            chart.height = height;

            // Trigger chart redraw if using Chart.js
            if (chart.chart) {
                chart.chart.resize();
            }
        }
    }

    cleanupMobileFeatures() {
        document.body.classList.remove('mobile-device', 'sidebar-open');
        this.hideSwipeActions();
        this.closeSidebar();
    }
}

// Initialize mobile optimizer when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new MobileOptimizer();
});

// Utility functions for mobile interactions
window.mobileUtils = {
    // Copy text to clipboard
    copyToClipboard: async (text) => {
        try {
            await navigator.clipboard.writeText(text);
            this.showToast('Copied to clipboard', 'success');
        } catch (err) {
            console.error('Failed to copy: ', err);
        }
    },

    // Share content
    share: async (data) => {
        if (navigator.share) {
            try {
                await navigator.share(data);
            } catch (err) {
                console.error('Error sharing: ', err);
            }
        } else {
            // Fallback for browsers that don't support Web Share API
            this.showToast('Sharing not supported', 'info');
        }
    },

    // Show toast notification
    showToast: (message, type = 'info') => {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#007bff'};
            color: white;
            padding: 12px 16px;
            border-radius: 4px;
            z-index: 9999;
            font-size: 14px;
        `;
        toast.textContent = message;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3000);
    },

    // Vibrate device (if supported)
    vibrate: (pattern = [100]) => {
        if (navigator.vibrate) {
            navigator.vibrate(pattern);
        }
    },

    // Check if device is online
    isOnline: () => {
        return navigator.onLine;
    },

    // Handle offline/online events
    setupOfflineHandler: () => {
        window.addEventListener('online', () => {
            this.showToast('Connection restored', 'success');
        });

        window.addEventListener('offline', () => {
            this.showToast('Connection lost', 'error');
        });
    }
};

// Setup offline handler
window.mobileUtils.setupOfflineHandler();
