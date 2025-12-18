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

        initializeThemeToggle();
        initializeLanguageSelector();
        initializeCart();
        initializeFilters();
        initializeSummary();
        initializeRealtimeSearch();
        initializeNotificationsPanel();
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
            '/home'
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
    }, { passive: true });

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

// Theme toggle
function initializeThemeToggle() {
    const btns = document.querySelectorAll('.theme-toggle');
    const stored = localStorage.getItem('mt-theme');
    if (stored) {
        document.documentElement.setAttribute('data-theme', stored);
        updateThemeToggleIcon(stored);
    }

    btns.forEach(btn => {
        btn.addEventListener('click', function () {
            const current = document.documentElement.getAttribute('data-theme') || 'light';
            const next = current === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-theme', next);
            localStorage.setItem('mt-theme', next);
            updateThemeToggleIcon(next);
        });
    });
}

function initializeRealtimeSearch() {
    const input = document.querySelector('.search-input');
    const results = document.querySelector('.search-results');
    if (!input || !results) return;
    let timer = null;
    input.addEventListener('input', function () {
        const q = this.value.trim();
        clearTimeout(timer);
        if (!q) {
            results.innerHTML = '';
            results.hidden = true;
            return;
        }
        timer = setTimeout(async () => {
            const items = await querySearch(q);
            renderSearchResults(results, items);
        }, 250);
    });
}

async function querySearch(q) {
    try {
        const r = await fetch(`/api/search?q=${encodeURIComponent(q)}`);
        if (!r.ok) throw new Error('no');
        const data = await r.json();
        return Array.isArray(data) ? data : [];
    } catch (e) {
        return [];
    }
}

function renderSearchResults(container, items) {
    if (!items.length) {
        container.innerHTML = `<div class="search-empty" role="option">Press Enter to search</div>`;
        container.hidden = false;
        return;
    }
    const html = items.slice(0, 8).map(it => {
        const t = it.type || 'item';
        const n = it.name || it.title || '';
        const u = it.url || '#';
        return `<a class="search-result" role="option" href="${u}"><span class="search-type">${t}</span><span class="search-name">${n}</span></a>`;
    }).join('');
    container.innerHTML = html;
    container.hidden = false;
}

function initializeFilters() {
    const state = loadFilters();
    document.querySelectorAll('.filter-btn').forEach(btn => {
        const g = btn.getAttribute('data-filter-group');
        const v = btn.getAttribute('data-filter');
        if (state[g] && state[g] === v) btn.classList.add('active');
        btn.addEventListener('click', () => {
            const s = loadFilters();
            s[g] = v;
            saveFilters(s);
            document.querySelectorAll(`.filter-btn[data-filter-group="${g}"]`).forEach(b => b.classList.toggle('active', b === btn));
        });
    });
    const walletSelect = document.querySelector('.wallet-select');
    if (walletSelect) {
        walletSelect.value = state.wallet || '';
        walletSelect.addEventListener('change', () => {
            const s = loadFilters();
            s.wallet = walletSelect.value;
            saveFilters(s);
        });
    }
}

function loadFilters() {
    try { return JSON.parse(localStorage.getItem('mt-filters') || '{}'); } catch { return {}; }
}
function saveFilters(s) {
    localStorage.setItem('mt-filters', JSON.stringify(s));
}

function initializeSummary() {
    const el = document.getElementById('financial-summary');
    if (!el) return;
    fetch('/api/Report/dashboard').then(r => r.json()).then(d => {
        if (!d) return;
        const balance = formatCurrency(d.currentBalance || 0, 'VND');
        const income = formatCurrency(d.monthlyIncome || 0, 'VND');
        const expense = formatCurrency(d.monthlyExpense || 0, 'VND');
        const trend = (d.monthlyIncome || 0) - (d.monthlyExpense || 0);
        const trendClass = trend >= 0 ? 'positive' : 'negative';
        el.innerHTML = `<div class="summary-item"><span class="label">Total</span><span class="value">${balance}</span></div><div class="summary-item"><span class="label">Income</span><span class="value">${income}</span></div><div class="summary-item"><span class="label">Expense</span><span class="value">${expense}</span></div><div class="summary-item ${trendClass}"><span class="label">Net</span><span class="value">${formatCurrency(trend, 'VND')}</span></div>`;
    }).catch(() => { });
}

function formatCurrency(n, currency) {
    try { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency }).format(n); } catch { return String(n); }
}

function initializeNotificationsPanel() {
    const panel = document.querySelector('.notification-panel');
    const bell = document.querySelector('.notification-bell');
    const badge = document.querySelector('.notification-badge');
    if (!panel || !bell) return;
    bell.addEventListener('click', function (e) {
        e.preventDefault();
        panel.hidden = !panel.hidden;
    });

    // Mock notification data if API fails
    const mockNotifications = [
        { type: 'warning', title: 'Overspending Alert', description: 'You have exceeded your dining budget.' },
        { type: 'info', title: 'Transaction Reminder', description: 'Rent payment is due tomorrow.' }
    ];

    fetch('/api/Notification').then(r => r.json()).then(list => {
        renderNotifications(list);
    }).catch(() => {
        // Fallback to mock data for demonstration
        renderNotifications(mockNotifications);
    });

    function renderNotifications(list) {
        const items = Array.isArray(list) ? list : [];
        if (badge) {
            badge.textContent = items.length > 99 ? '99+' : String(items.length);
            badge.style.display = items.length ? 'block' : 'none';
        }
        panel.innerHTML = items.length ? items.slice(0, 10).map(n => `<div class="notification-item ${n.type || ''}"><div class="title fw-bold">${n.title || ''}</div><div class="desc small">${n.description || ''}</div></div>`).join('') : '<div class="p-2 text-muted">No notifications</div>';
    }
}

// Language selector
function initializeLanguageSelector() {
    const options = document.querySelectorAll('.lang-option');
    const stored = localStorage.getItem('mt-lang');
    if (stored) {
        document.documentElement.lang = stored;
        updateLanguageDisplay(stored);
    }
    options.forEach(opt => {
        opt.addEventListener('click', function (e) {
            e.preventDefault();
            const lang = this.getAttribute('data-lang') || 'vi';
            document.documentElement.lang = lang;
            localStorage.setItem('mt-lang', lang);
            updateLanguageDisplay(lang);
        });
    });
}

function updateLanguageDisplay(lang) {
    const els = document.querySelectorAll('.current-language');
    els.forEach(el => {
        el.textContent = lang === 'en' ? 'EN' : 'VN';
    });
}

// Cart
function initializeCart() {
    const badge = document.querySelector('.item-count-badge');
    const btn = document.querySelector('.cart-button');
    const count = parseInt(localStorage.getItem('mt-cart-count') || '0', 10);
    updateCartCount(count);
    if (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const target = '/Cart';
            window.location.href = target;
        });
    }
}

function updateCartCount(count) {
    const badge = document.querySelector('.item-count-badge');
    if (!badge) return;
    const c = Number.isFinite(count) ? count : 0;
    badge.textContent = c > 99 ? '99+' : String(c);
    badge.style.display = c > 0 ? 'block' : 'none';
}
