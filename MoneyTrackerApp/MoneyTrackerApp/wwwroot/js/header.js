// Header JavaScript for MoneyTrackerApp
(function () {
    'use strict';

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initializeHeader();
    });

    function initializeHeader() {
        // Active link highlighting
        highlightActiveLink();

        // Search functionality
        initializeSearch();

        // Quick add button
        initializeQuickAdd();

        // Notification interactions
        initializeNotifications();

        // Mobile menu handling
        initializeMobileMenu();

        // Smooth scroll for anchor links
        initializeSmoothScroll();
    }

    // Highlight active navigation link
    function highlightActiveLink() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

        navLinks.forEach(link => {
            const linkPath = link.getAttribute('href');
            if (linkPath && currentPath.includes(linkPath)) {
                link.classList.add('active');
            }
        });
    }

    // Search functionality
    function initializeSearch() {
        const searchInput = document.querySelector('.search-input');
        if (!searchInput) return;

        // Focus animation
        searchInput.addEventListener('focus', function () {
            this.parentElement.classList.add('focused');
        });

        searchInput.addEventListener('blur', function () {
            this.parentElement.classList.remove('focused');
        });

        // Search on Enter key
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                performSearch(this.value);
            }
        });

        // Clear search on Escape
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                this.value = '';
                this.blur();
            }
        });
    }

    function performSearch(query) {
        if (!query.trim()) return;
        console.log('Searching for:', query);

        // Prefer a transactions search page if present, otherwise fall back to reports
        // Try common candidate paths used in the app
        const candidates = [
            '/Transactions',
            '/transactions',
            '/Reports',
            '/Reports?search=',
            '/Home'
        ];

        // Build a target URL with query parameter
        const encoded = encodeURIComponent(query);

        // If the site exposes a Reports page link, prefer it
        const reportsLink = document.querySelector('a[asp-page="/Reports"], a[href="/Reports"], a[href="/reports"]');
        let target = `/Transactions?search=${encoded}`;
        if (reportsLink) {
            target = `/Reports?search=${encoded}`;
        }

        // Navigate to search target
        window.location.href = target;
    }

    // Quick Add Button
    function initializeQuickAdd() {
        const quickAddBtn = document.querySelector('.btn-quick-add');
        if (!quickAddBtn) return;

        quickAddBtn.addEventListener('click', function () {
            console.log('Quick add clicked');

            // If there's a bootstrap modal with id 'addTransactionModal' on the page, open it
            try {
                const modalEl = document.getElementById('addTransactionModal');
                if (modalEl) {
                    // Bootstrap 5 modal
                    if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        const m = new window.bootstrap.Modal(modalEl);
                        m.show();
                        return;
                    }
                }
            } catch (e) {
                console.warn('Error opening modal', e);
            }

            // Otherwise, navigate to a transaction creation page
            const createPath = '/Transactions/Create';
            window.location.href = createPath;
        });
    }

    // Notification interactions
    function initializeNotifications() {
        const notificationBell = document.querySelector('.notification-bell');
        if (!notificationBell) return;

        // Add click animation
        notificationBell.addEventListener('click', function (e) {
            this.classList.add('ringing');
            setTimeout(() => {
                this.classList.remove('ringing');
            }, 500);
        });

        // Update notification count (example)
        // updateNotificationCount(3);
    }

    function updateNotificationCount(count) {
        const badge = document.querySelector('.notification-badge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    }

    // Mobile menu handling
    function initializeMobileMenu() {
        const navbarToggler = document.querySelector('.navbar-toggler');
        const navbarCollapse = document.querySelector('.navbar-collapse');

        if (!navbarToggler || !navbarCollapse) return;

        // Close mobile menu when clicking outside
        document.addEventListener('click', function (e) {
            if (!navbarToggler.contains(e.target) &&
                !navbarCollapse.contains(e.target) &&
                navbarCollapse.classList.contains('show')) {
                navbarToggler.click();
            }
        });

        // Close mobile menu when clicking on a link
        const mobileLinks = navbarCollapse.querySelectorAll('.nav-link');
        mobileLinks.forEach(link => {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992 && navbarCollapse.classList.contains('show')) {
                    navbarToggler.click();
                }
            });
        });
    }

    // Smooth scroll for anchor links
    function initializeSmoothScroll() {
        const anchorLinks = document.querySelectorAll('a[href^="#"]');

        anchorLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#') return;

                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Header scroll effect (optional)
    let lastScroll = 0;
    window.addEventListener('scroll', function () {
        const header = document.querySelector('.app-header');
        if (!header) return;

        const currentScroll = window.pageYOffset;

        // Add shadow on scroll
        if (currentScroll > 10) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }

        // Hide header on scroll down, show on scroll up (optional)
        // if (currentScroll > lastScroll && currentScroll > 100) {
        //     header.style.transform = 'translateY(-100%)';
        // } else {
        //     header.style.transform = 'translateY(0)';
        // }

        lastScroll = currentScroll;
    });

    // Dropdown hover effect (desktop only)
    if (window.innerWidth > 991) {
        const dropdowns = document.querySelectorAll('.nav-item.dropdown');

        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('mouseenter', function () {
                const toggle = this.querySelector('.dropdown-toggle');
                const menu = this.querySelector('.dropdown-menu');

                if (toggle && menu) {
                    toggle.classList.add('show');
                    menu.classList.add('show');
                }
            });

            dropdown.addEventListener('mouseleave', function () {
                const toggle = this.querySelector('.dropdown-toggle');
                const menu = this.querySelector('.dropdown-menu');

                if (toggle && menu) {
                    toggle.classList.remove('show');
                    menu.classList.remove('show');
                }
            });
        });
    }

    // User menu animations
    const userDropdown = document.querySelector('.user-dropdown');
    if (userDropdown) {
        userDropdown.addEventListener('show.bs.dropdown', function () {
            this.classList.add('show');
        });

        userDropdown.addEventListener('hide.bs.dropdown', function () {
            this.classList.remove('show');
        });
    }

    // Keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + K for search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('.search-input');
            if (searchInput) {
                searchInput.focus();
            }
        }

        // Ctrl/Cmd + N for quick add
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            const quickAddBtn = document.querySelector('.btn-quick-add');
            if (quickAddBtn) {
                quickAddBtn.click();
            }
        }
    });

    // Export functions for external use
    window.MoneyTrackerHeader = {
        updateNotificationCount: updateNotificationCount,
        performSearch: performSearch
    };

})();

// Add CSS for additional animations
const style = document.createElement('style');
style.textContent = `
    .app-header {
        transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .app-header.scrolled {
        box-shadow: 0 4px 30px rgba(0, 0, 0, 0.15);
    }

    .notification-bell.ringing {
        animation: ring 0.5s ease;
    }

    @keyframes ring {
        0%, 100% { transform: rotate(0deg); }
        10%, 30%, 50%, 70%, 90% { transform: rotate(-10deg); }
        20%, 40%, 60%, 80% { transform: rotate(10deg); }
    }

    .search-container.focused .search-icon {
        color: #FFD700;
    }
`;
document.head.appendChild(style);
