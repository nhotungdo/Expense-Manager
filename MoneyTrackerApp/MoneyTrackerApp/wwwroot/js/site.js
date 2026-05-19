/**
 * Global Currency Utilities
 */

// Format number to string with local currency rules (e.g., 1.500.000 ₫, $1,500.00)
window.formatCurrency = function(amount, currencyCode = 'VND') {
    currencyCode = 'VND'; // Force all formatting to use Vietnamese Dong (VND)
    if (amount === null || amount === undefined || isNaN(amount)) return '0';
    
    const locales = {
        'VND': 'vi-VN',
        'USD': 'en-US',
        'EUR': 'de-DE',
        'GBP': 'en-GB',
        'JPY': 'ja-JP',
        'KRW': 'ko-KR',
        'CNY': 'zh-CN',
        'BTC': 'en-US',
        'ETH': 'en-US',
        'USDT': 'en-US'
    };
    
    try {
        return new Intl.NumberFormat(locales[currencyCode] || 'en-US', {
            style: 'currency',
            currency: currencyCode,
            minimumFractionDigits: (currencyCode === 'VND' || currencyCode === 'JPY' || currencyCode === 'KRW') ? 0 : 2,
            maximumFractionDigits: (currencyCode === 'VND' || currencyCode === 'JPY' || currencyCode === 'KRW') ? 0 : 2
        }).format(amount);
    } catch (e) {
        return amount.toLocaleString() + ' ' + currencyCode;
    }
};

// Compatibility wrapper for old calls
window.formatCurrencyVND = function(amount) {
    return window.formatCurrency(amount, 'VND');
};

// Clean formatted string back to number
window.unformatCurrency = function(formattedValue) {
    if (!formattedValue) return 0;
    if (typeof formattedValue === 'number') return formattedValue;
    // Remove all non-digit characters (including dots and commas)
    // This is safe for VND as it's typically an integer-based currency
    const cleanValue = formattedValue.toString().replace(/\D/g, '');
    return parseFloat(cleanValue) || 0;
};

/**
 * Real-time money input formatting
 * @param {HTMLInputElement} input 
 */
window.handleMoneyInput = function(input) {
    let value = input.value.replace(/\D/g, '');
    
    // Limit max value (e.g., 1 quadrillion)
    if (value.length > 15) {
        value = value.substring(0, 15);
    }

    if (value === '') {
        input.value = '';
        return;
    }

    const numericValue = parseInt(value);
    const formatted = window.formatCurrency(numericValue);
    
    // Save cursor position relative to the end
    const cursorFromEnd = input.value.length - input.selectionEnd;
    
    input.value = formatted;
    
    // Restore cursor position
    const newPos = Math.max(0, input.value.length - cursorFromEnd);
    input.setSelectionRange(newPos, newPos);
};

// Auto-attach to all elements with 'money-input' class
document.addEventListener('input', function(e) {
    if (e.target.classList.contains('money-input')) {
        window.handleMoneyInput(e.target);
    }
});

// Also handle paste events
document.addEventListener('paste', function(e) {
    if (e.target.classList.contains('money-input')) {
        setTimeout(() => window.handleMoneyInput(e.target), 0);
    }
});

// Unformat money inputs before form submission
document.addEventListener('submit', function(e) {
    const moneyInputs = e.target.querySelectorAll('.money-input');
    moneyInputs.forEach(input => {
        input.value = window.unformatCurrency(input.value);
    });
});
