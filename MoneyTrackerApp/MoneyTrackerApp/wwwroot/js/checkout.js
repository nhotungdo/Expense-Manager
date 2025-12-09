// Checkout page JavaScript

let selectedPackage = null;
let appliedDiscount = null;
const TAX_RATE = 0.10; // 10% VAT

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    const packageId = urlParams.get('packageId');
    
    if (!packageId) {
        showError('Không tìm thấy thông tin gói dịch vụ');
        setTimeout(() => {
            window.location.href = '/ServicePackages';
        }, 2000);
        return;
    }

    loadPackageInfo(packageId);
});

// Load package information
async function loadPackageInfo(packageId) {
    try {
        const response = await fetch(`/api/ServicePackage/${packageId}`);
        
        if (!response.ok) {
            throw new Error('Failed to load package');
        }

        selectedPackage = await response.json();
        displayPackageInfo();
        calculateTotal();
        
        // Show discount section
        document.getElementById('discountSection').style.display = 'block';
    } catch (error) {
        console.error('Error loading package:', error);
        showError('Không thể tải thông tin gói dịch vụ');
        setTimeout(() => {
            window.location.href = '/ServicePackages';
        }, 2000);
    }
}

// Display package information
function displayPackageInfo() {
    const packageInfoDiv = document.getElementById('packageInfo');
    const durationText = selectedPackage.durationDays >= 365 ? 
        `${Math.floor(selectedPackage.durationDays / 365)} năm` : 
        `${selectedPackage.durationDays} ngày`;

    packageInfoDiv.innerHTML = `
        <div style="display: flex; align-items: start; gap: 16px;">
            <div style="
                width: 60px;
                height: 60px;
                background: var(--primary-light);
                border-radius: 12px;
                display: flex;
                align-items: center;
                justify-content: center;
                flex-shrink: 0;
            ">
                <i class="fas ${getPackageIcon(selectedPackage.name)}" style="font-size: 24px; color: var(--primary);"></i>
            </div>
            <div style="flex: 1;">
                <h3 style="margin: 0 0 8px 0; font-size: 20px; font-weight: 600;">${selectedPackage.name}</h3>
                <p style="margin: 0 0 12px 0; color: var(--text-secondary);">${selectedPackage.description}</p>
                <div style="display: flex; gap: 16px; font-size: 14px; color: var(--text-secondary);">
                    <span><i class="fas fa-clock"></i> ${durationText}</span>
                    <span><i class="fas fa-calendar-alt"></i> Bắt đầu: ${formatDate(new Date())}</span>
                </div>
            </div>
        </div>
    `;

    document.getElementById('orderDetails').style.display = 'block';
}

// Get package icon
function getPackageIcon(name) {
    if (name.includes('Miễn Phí')) return 'fa-gift';
    if (name.includes('Cơ Bản')) return 'fa-star';
    if (name.includes('Chuyên Nghiệp')) return 'fa-crown';
    if (name.includes('Doanh Nghiệp')) return 'fa-building';
    return 'fa-box';
}

// Format date
function formatDate(date) {
    return new Intl.DateTimeFormat('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    }).format(date);
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

// Calculate total
function calculateTotal() {
    if (!selectedPackage) return;

    let subtotal = selectedPackage.price;
    let discount = 0;

    if (appliedDiscount) {
        if (appliedDiscount.type === 'percentage') {
            discount = subtotal * (appliedDiscount.value / 100);
        } else {
            discount = appliedDiscount.value;
        }
    }

    const afterDiscount = subtotal - discount;
    const tax = afterDiscount * TAX_RATE;
    const total = afterDiscount + tax;

    // Update display
    document.getElementById('packagePrice').textContent = formatCurrency(subtotal) + ' ₫';
    document.getElementById('taxAmount').textContent = formatCurrency(tax) + ' ₫';
    document.getElementById('totalAmount').textContent = formatCurrency(total) + ' ₫';

    if (discount > 0) {
        document.getElementById('discountRow').style.display = 'flex';
        document.getElementById('discountAmount').textContent = '-' + formatCurrency(discount) + ' ₫';
    } else {
        document.getElementById('discountRow').style.display = 'none';
    }
}

// Apply discount code
async function applyDiscount() {
    const discountCode = document.getElementById('discountCode').value.trim();
    const messageDiv = document.getElementById('discountMessage');

    if (!discountCode) {
        messageDiv.textContent = 'Vui lòng nhập mã giảm giá';
        messageDiv.className = 'discount-message error';
        return;
    }

    try {
        // TODO: Call API to validate discount code
        // For now, simulate validation
        messageDiv.textContent = 'Đang kiểm tra mã giảm giá...';
        messageDiv.className = 'discount-message';

        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 500));

        // Mock response - replace with actual API call
        const isValid = false; // Change to true to test

        if (isValid) {
            appliedDiscount = {
                code: discountCode,
                type: 'percentage',
                value: 10
            };
            messageDiv.textContent = 'Áp dụng mã giảm giá thành công!';
            messageDiv.className = 'discount-message success';
            calculateTotal();
        } else {
            messageDiv.textContent = 'Mã giảm giá không hợp lệ hoặc đã hết hạn';
            messageDiv.className = 'discount-message error';
        }
    } catch (error) {
        console.error('Error applying discount:', error);
        messageDiv.textContent = 'Không thể kiểm tra mã giảm giá';
        messageDiv.className = 'discount-message error';
    }
}

// Proceed to payment
async function proceedToPayment() {
    // Validate terms agreement
    const agreeTerms = document.getElementById('agreeTerms').checked;
    const agreeAutoRenew = document.getElementById('agreeAutoRenew').checked;

    if (!agreeTerms) {
        showError('Vui lòng đồng ý với điều khoản dịch vụ và chính sách bảo mật');
        return;
    }

    if (!agreeAutoRenew) {
        showError('Vui lòng đồng ý với việc tự động gia hạn gói dịch vụ');
        return;
    }

    // Get current user ID
    const userId = getUserId();
    if (!userId) {
        showError('Vui lòng đăng nhập để tiếp tục');
        window.location.href = '/auth/login';
        return;
    }

    // Disable button and show loading
    const btnPayment = document.getElementById('btnProceedPayment');
    btnPayment.disabled = true;
    btnPayment.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

    try {
        // Create payment transaction via link.com integration
        const response = await fetch('/api/payments/create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: userId,
                packageId: selectedPackage.id
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Không thể tạo giao dịch thanh toán');
        }

        const result = await response.json();

        if (!result.success || !result.paymentUrl) {
            throw new Error('Không nhận được URL thanh toán');
        }

        // Log payment transaction created
        console.log('Payment transaction created:', result.paymentTransactionId);
        console.log('Redirecting to:', result.paymentUrl);

        // Redirect to link.com payment gateway
        window.location.href = result.paymentUrl;
        
    } catch (error) {
        console.error('Error creating payment:', error);
        showError(error.message || 'Không thể xử lý thanh toán. Vui lòng thử lại.');
        
        // Re-enable button
        btnPayment.disabled = false;
        btnPayment.innerHTML = '<i class="fas fa-lock"></i> Thanh toán';
    }
}

// Helper function to get user ID
function getUserId() {
    // Option 1: From meta tag (set in layout)
    const userIdMeta = document.querySelector('meta[name="user-id"]');
    if (userIdMeta) {
        return parseInt(userIdMeta.content);
    }
    
    // Option 2: From localStorage
    const userId = localStorage.getItem('userId');
    if (userId) {
        return parseInt(userId);
    }
    
    // Option 3: From cookie
    const userIdCookie = getCookie('userId');
    if (userIdCookie) {
        return parseInt(userIdCookie);
    }
    
    return null;
}

// Helper function to get cookie value
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

// Get access token from cookie
function getAccessToken() {
    const tokenCookie = document.cookie
        .split('; ')
        .find(row => row.startsWith('AccessToken='));
    
    return tokenCookie ? tokenCookie.split('=')[1] : '';
}

// Show error message
function showError(message) {
    const errorDiv = document.createElement('div');
    errorDiv.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #fee2e2;
        color: #991b1b;
        padding: 16px 24px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 10000;
        max-width: 400px;
        display: flex;
        align-items: center;
        gap: 12px;
    `;
    errorDiv.innerHTML = `
        <i class="fas fa-exclamation-circle"></i>
        <span>${message}</span>
    `;
    document.body.appendChild(errorDiv);

    setTimeout(() => {
        errorDiv.remove();
    }, 5000);
}
