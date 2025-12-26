/**
 * Global Search Functionality
 * Handles search across transactions, categories, and accounts
 */

let searchTimeout = null;
let currentSearchFilter = 'all';

/**
 * Initialize search functionality
 */
function initializeSearch() {
    const searchInput = document.getElementById('globalSearch');
    const searchResults = document.getElementById('searchResults');

    if (!searchInput) return;

    // Keyboard shortcut (Ctrl+K or Cmd+K)
    document.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            searchInput.focus();
        }

        // ESC to close search results
        if (e.key === 'Escape') {
            hideSearchResults();
        }
    });

    // Search input handler with debounce
    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.trim();

        // Show/hide clear button
        const clearBtn = document.getElementById('clearSearchBtn');
        if (clearBtn) {
            clearBtn.style.display = query ? 'block' : 'none';
        }

        // Debounce search
        clearTimeout(searchTimeout);

        if (query.length < 2) {
            hideSearchResults();
            return;
        }

        searchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
    });

    // Click outside to close
    document.addEventListener('click', (e) => {
        const container = document.getElementById('searchContainer');
        if (container && !container.contains(e.target)) {
            hideSearchResults();
        }
    });

    // Focus to show recent results
    searchInput.addEventListener('focus', () => {
        const query = searchInput.value.trim();
        if (query.length >= 2) {
            performSearch(query);
        }
    });
}

/**
 * Perform search API call
 */
async function performSearch(query) {
    try {
        showSearchLoading();

        const type = currentSearchFilter === 'all' ? null : currentSearchFilter;
        const params = new URLSearchParams({ query });
        if (type) params.append('type', type);

        const response = await fetch(`/api/Search?${params.toString()}`, {
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Search failed');

        const data = await response.json();
        displaySearchResults(data);
    } catch (error) {
        console.error('Search error:', error);
        showSearchEmpty();
    }
}

/**
 * Display search results
 */
function displaySearchResults(data) {
    const resultsContainer = document.getElementById('searchResultsContent');
    const searchResults = document.getElementById('searchResults');
    const searchLoading = document.getElementById('searchLoading');
    const searchEmpty = document.getElementById('searchEmpty');

    if (!resultsContainer) return;

    // Hide loading
    searchLoading?.classList.add('hidden');
    searchResults?.classList.remove('hidden');

    // Check if we have any results
    const hasResults = (data.transactions?.length > 0) ||
        (data.categories?.length > 0) ||
        (data.accounts?.length > 0);

    if (!hasResults) {
        searchEmpty?.classList.remove('hidden');
        resultsContainer.innerHTML = '';
        return;
    }

    searchEmpty?.classList.add('hidden');

    // Build results HTML
    let html = '';

    // Transactions
    if (data.transactions?.length > 0) {
        html += `
            <div class="search-section">
                <h4 class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3 flex items-center gap-2">
                    <i class="fas fa-exchange-alt"></i> Giao dịch (${data.transactions.length})
                </h4>
                <div class="space-y-2">
                    ${data.transactions.map(t => renderTransactionResult(t)).join('')}
                </div>
            </div>
        `;
    }

    // Categories
    if (data.categories?.length > 0) {
        html += `
            <div class="search-section">
                <h4 class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3 flex items-center gap-2">
                    <i class="fas fa-tags"></i> Danh mục (${data.categories.length})
                </h4>
                <div class="space-y-2">
                    ${data.categories.map(c => renderCategoryResult(c)).join('')}
                </div>
            </div>
        `;
    }

    // Accounts
    if (data.accounts?.length > 0) {
        html += `
            <div class="search-section">
                <h4 class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3 flex items-center gap-2">
                    <i class="fas fa-wallet"></i> Tài khoản (${data.accounts.length})
                </h4>
                <div class="space-y-2">
                    ${data.accounts.map(a => renderAccountResult(a)).join('')}
                </div>
            </div>
        `;
    }

    resultsContainer.innerHTML = html;
}

/**
 * Render transaction search result
 */
function renderTransactionResult(transaction) {
    const typeColors = {
        income: 'emerald',
        expense: 'rose',
        transfer: 'blue'
    };

    const color = typeColors[transaction.type] || 'slate';
    const icon = transaction.categoryIcon || 'fa-circle';
    const date = new Date(transaction.date).toLocaleDateString('vi-VN');
    const amount = new Intl.NumberFormat('vi-VN').format(transaction.amount);

    return `
        <div class="search-result-item p-3 rounded-xl hover:bg-slate-50 cursor-pointer transition-all border border-transparent hover:border-slate-200"
             onclick="viewTransaction(${transaction.id})">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-${color}-50 flex items-center justify-center text-${color}-600 flex-shrink-0">
                    <i class="fas ${icon}"></i>
                </div>
                <div class="flex-1 min-w-0">
                    <div class="flex items-start justify-between gap-2">
                        <div class="flex-1 min-w-0">
                            <p class="text-sm font-semibold text-slate-900 truncate">${transaction.note || 'Không có ghi chú'}</p>
                            <p class="text-xs text-slate-500 mt-0.5">
                                ${transaction.categoryName} • ${transaction.accountName}
                            </p>
                        </div>
                        <div class="text-right flex-shrink-0">
                            <p class="text-sm font-bold text-${color}-600">
                                ${transaction.type === 'income' ? '+' : '-'}${amount} ₫
                            </p>
                            <p class="text-xs text-slate-400 mt-0.5">${date}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

/**
 * Render category search result
 */
function renderCategoryResult(category) {
    const typeColors = {
        income: 'emerald',
        expense: 'rose',
        transfer: 'blue'
    };

    const color = typeColors[category.type] || 'slate';
    const icon = category.icon || 'fa-tag';
    const typeLabel = category.type === 'income' ? 'Thu nhập' : category.type === 'expense' ? 'Chi tiêu' : 'Chuyển tiền';

    return `
        <div class="search-result-item p-3 rounded-xl hover:bg-slate-50 cursor-pointer transition-all border border-transparent hover:border-slate-200"
             onclick="viewCategory(${category.id})">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-${color}-50 flex items-center justify-center text-${color}-600 flex-shrink-0">
                    <i class="fas ${icon}"></i>
                </div>
                <div class="flex-1">
                    <div class="flex items-center justify-between">
                        <div>
                            <p class="text-sm font-semibold text-slate-900">${category.name}</p>
                            <p class="text-xs text-slate-500 mt-0.5">
                                ${typeLabel} • ${category.transactionCount} giao dịch
                            </p>
                        </div>
                        <i class="fas fa-chevron-right text-slate-300"></i>
                    </div>
                </div>
            </div>
        </div>
    `;
}

/**
 * Render account search result
 */
function renderAccountResult(account) {
    const icon = account.icon || 'fa-wallet';
    const balance = new Intl.NumberFormat('vi-VN').format(account.balance);
    const currency = account.currency || 'VND';

    return `
        <div class="search-result-item p-3 rounded-xl hover:bg-slate-50 cursor-pointer transition-all border border-transparent hover:border-slate-200"
             onclick="viewAccount(${account.id})">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 flex-shrink-0">
                    <i class="fas ${icon}"></i>
                </div>
                <div class="flex-1">
                    <div class="flex items-center justify-between">
                        <div>
                            <p class="text-sm font-semibold text-slate-900">${account.name}</p>
                            <p class="text-xs text-slate-500 mt-0.5">Tài khoản ${currency}</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-bold text-slate-900">${balance} ₫</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

/**
 * Set search filter
 */
function setSearchFilter(filter) {
    currentSearchFilter = filter;

    // Update button styles
    document.querySelectorAll('.search-filter-btn').forEach(btn => {
        const btnFilter = btn.getAttribute('data-filter');
        if (btnFilter === filter) {
            btn.className = 'search-filter-btn px-3 py-1.5 rounded-lg text-xs font-medium transition-all bg-slate-100 text-slate-600 hover:bg-slate-200';
        } else {
            btn.className = 'search-filter-btn px-3 py-1.5 rounded-lg text-xs font-medium transition-all text-slate-500 hover:bg-slate-100';
        }
    });

    // Re-run search if there's a query
    const searchInput = document.getElementById('globalSearch');
    const query = searchInput?.value.trim();
    if (query && query.length >= 2) {
        performSearch(query);
    }
}

/**
 * Clear search
 */
function clearSearch() {
    const searchInput = document.getElementById('globalSearch');
    if (searchInput) {
        searchInput.value = '';
        searchInput.focus();
    }
    hideSearchResults();
    document.getElementById('clearSearchBtn').style.display = 'none';
}

/**
 * Show search loading state
 */
function showSearchLoading() {
    const searchResults = document.getElementById('searchResults');
    const searchLoading = document.getElementById('searchLoading');
    const searchEmpty = document.getElementById('searchEmpty');
    const resultsContent = document.getElementById('searchResultsContent');

    searchResults?.classList.remove('hidden');
    searchLoading?.classList.remove('hidden');
    searchEmpty?.classList.add('hidden');
    if (resultsContent) resultsContent.innerHTML = '';
}

/**
 * Show empty search state
 */
function showSearchEmpty() {
    const searchLoading = document.getElementById('searchLoading');
    const searchEmpty = document.getElementById('searchEmpty');
    const resultsContent = document.getElementById('searchResultsContent');

    searchLoading?.classList.add('hidden');
    searchEmpty?.classList.remove('hidden');
    if (resultsContent) resultsContent.innerHTML = '';
}

/**
 * Hide search results
 */
function hideSearchResults() {
    const searchResults = document.getElementById('searchResults');
    const searchLoading = document.getElementById('searchLoading');
    const searchEmpty = document.getElementById('searchEmpty');

    searchResults?.classList.add('hidden');
    searchLoading?.classList.add('hidden');
    searchEmpty?.classList.add('hidden');
}

/**
 * View transaction details
 */
function viewTransaction(transactionId) {
    window.location.href = `/Transactions?highlight=${transactionId}`;
}

/**
 * View category details
 */
function viewCategory(categoryId) {
    window.location.href = `/Categories?highlight=${categoryId}`;
}

/**
 * View account details
 */
function viewAccount(accountId) {
    window.location.href = `/Accounts?highlight=${accountId}`;
}

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSearch);
} else {
    initializeSearch();
}

// Export functions for global access
window.setSearchFilter = setSearchFilter;
window.clearSearch = clearSearch;
window.viewTransaction = viewTransaction;
window.viewCategory = viewCategory;
window.viewAccount = viewAccount;
