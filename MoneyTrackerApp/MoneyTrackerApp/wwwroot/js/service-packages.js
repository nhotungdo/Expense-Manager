// Service Packages JavaScript

let allPackages = [];
let currentFilter = 'all';
let currentSort = 'default';

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeFilters();
    initializeFAQ();
    loadPackages();
    updateCurrentDate();
});

// Update current date
function updateCurrentDate() {
    const dateElement = document.getElementById('currentDate');
    if (dateElement) {
        const now = new Date();
        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        dateElement.textContent = now.toLocaleDateString('vi-VN', options);
    }
}

// Initialize filter buttons
function initializeFilters() {
    const filterButtons = document.querySelectorAll('.filter-btn');
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentFilter = this.dataset.filter;
            filterAndSortPackages();
        });
    });

    const sortSelect = document.getElementById('sortSelect');
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            currentSort = this.value;
            filterAndSortPackages();
        });
    }
}

// Initialize FAQ accordion
function initializeFAQ() {
    const faqItems = document.querySelectorAll('.faq-item');
    faqItems.forEach(item => {
        const question = item.querySelector('.faq-question');
        question.addEventListener('click', function() {
            const isActive = item.classList.contains('active');
            
            // Close all items
            faqItems.forEach(i => i.classList.remove('active'));
            
            // Open clicked item if it wasn't active
            if (!isActive) {
                item.classList.add('active');
            }
        });
    });
}

// Load packages from API
async function loadPackages() {
    const loadingState = document.getElementById('loadingState');
    const packagesGrid = document.getElementById('packagesGrid');

    try {
        const response = await fetch('/api/ServicePackage?activeOnly=true');
        
        if (!response.ok) {
            throw new Error('Failed to load packages');
        }

        allPackages = await response.json();
        
        // If no packages exist, create sample data
        if (allPackages.length === 0) {
            allPackages = getSamplePackages();
        }

        loadingState.style.display = 'none';
        filterAndSortPackages();
        buildComparisonTable();
    } catch (error) {
        console.error('Error loading packages:', error);
        loadingState.innerHTML = `
            <i class="fas fa-exclamation-triangle"></i>
            <p>Không thể tải gói dịch vụ. Đang hiển thị dữ liệu mẫu.</p>
        `;
        
        // Use sample data on error
        allPackages = getSamplePackages();
        setTimeout(() => {
            loadingState.style.display = 'none';
            filterAndSortPackages();
            buildComparisonTable();
        }, 1000);
    }
}

// Get sample packages
function getSamplePackages() {
    return [
        {
            id: 1,
            name: 'Gói Miễn Phí',
            description: 'Hoàn hảo để bắt đầu quản lý tài chính cá nhân',
            price: 0,
            originalPrice: null,
            durationDays: 365,
            features: [
                'Theo dõi thu chi cơ bản',
                'Tối đa 3 ví',
                'Báo cáo hàng tháng',
                'Hỗ trợ qua email',
                'Lưu trữ 100 giao dịch'
            ],
            isPopular: false,
            badgeText: null,
            badgeColor: null,
            discountPercentage: 0
        },
        {
            id: 2,
            name: 'Gói Cơ Bản',
            description: 'Dành cho người dùng cá nhân muốn quản lý tốt hơn',
            price: 99000,
            originalPrice: 149000,
            durationDays: 30,
            features: [
                'Tất cả tính năng Miễn Phí',
                'Không giới hạn ví',
                'Báo cáo chi tiết',
                'Phân loại tự động',
                'Lưu trữ không giới hạn',
                'Xuất báo cáo Excel/PDF',
                'Hỗ trợ ưu tiên'
            ],
            isPopular: false,
            badgeText: 'Giảm 33%',
            badgeColor: 'discount',
            discountPercentage: 33
        },
        {
            id: 3,
            name: 'Gói Chuyên Nghiệp',
            description: 'Giải pháp toàn diện cho quản lý tài chính chuyên nghiệp',
            price: 199000,
            originalPrice: 299000,
            durationDays: 30,
            features: [
                'Tất cả tính năng Cơ Bản',
                'AI phân tích chi tiêu',
                'Dự báo tài chính',
                'Quản lý đầu tư',
                'Theo dõi nợ & tiết kiệm',
                'Chia sẻ chi tiêu nhóm',
                'Tích hợp ngân hàng',
                'Hỗ trợ 24/7',
                'Tư vấn tài chính cá nhân'
            ],
            isPopular: true,
            badgeText: 'Phổ biến nhất',
            badgeColor: 'popular',
            discountPercentage: 33
        },
        {
            id: 4,
            name: 'Gói Doanh Nghiệp',
            description: 'Giải pháp cho doanh nghiệp và nhóm làm việc',
            price: 499000,
            originalPrice: null,
            durationDays: 30,
            features: [
                'Tất cả tính năng Chuyên Nghiệp',
                'Quản lý nhiều người dùng',
                'Phân quyền chi tiết',
                'API tích hợp',
                'Báo cáo tùy chỉnh',
                'Sao lưu tự động',
                'Bảo mật nâng cao',
                'Đào tạo & onboarding',
                'Account manager riêng'
            ],
            isPopular: false,
            badgeText: null,
            badgeColor: null,
            discountPercentage: 0
        },
        {
            id: 5,
            name: 'Gói Năm - Cơ Bản',
            description: 'Tiết kiệm 20% khi đăng ký theo năm',
            price: 950000,
            originalPrice: 1188000,
            durationDays: 365,
            features: [
                'Tất cả tính năng Gói Cơ Bản',
                'Thanh toán 1 lần/năm',
                'Tiết kiệm 238.000đ',
                'Ưu tiên cập nhật tính năng mới'
            ],
            isPopular: false,
            badgeText: 'Tiết kiệm 20%',
            badgeColor: 'discount',
            discountPercentage: 20
        },
        {
            id: 6,
            name: 'Gói Năm - Chuyên Nghiệp',
            description: 'Tiết kiệm 25% khi đăng ký theo năm',
            price: 1790000,
            originalPrice: 2388000,
            durationDays: 365,
            features: [
                'Tất cả tính năng Gói Chuyên Nghiệp',
                'Thanh toán 1 lần/năm',
                'Tiết kiệm 598.000đ',
                'Tặng 1 tháng sử dụng',
                'Ưu tiên hỗ trợ VIP'
            ],
            isPopular: true,
            badgeText: 'Ưu đãi nhất',
            badgeColor: 'popular',
            discountPercentage: 25
        }
    ];
}

// Filter and sort packages
function filterAndSortPackages() {
    let filtered = [...allPackages];

    // Apply filter
    if (currentFilter === 'popular') {
        filtered = filtered.filter(p => p.isPopular);
    } else if (currentFilter === 'monthly') {
        filtered = filtered.filter(p => p.durationDays <= 31);
    } else if (currentFilter === 'yearly') {
        filtered = filtered.filter(p => p.durationDays > 31);
    }

    // Apply sort
    switch (currentSort) {
        case 'price-asc':
            filtered.sort((a, b) => a.price - b.price);
            break;
        case 'price-desc':
            filtered.sort((a, b) => b.price - a.price);
            break;
        case 'duration-asc':
            filtered.sort((a, b) => a.durationDays - b.durationDays);
            break;
        case 'duration-desc':
            filtered.sort((a, b) => b.durationDays - a.durationDays);
            break;
    }

    renderPackages(filtered);
}

// Render packages
function renderPackages(packages) {
    const packagesGrid = document.getElementById('packagesGrid');
    
    if (packages.length === 0) {
        packagesGrid.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-box-open"></i>
                <h3>Không tìm thấy gói dịch vụ</h3>
                <p>Thử thay đổi bộ lọc để xem thêm gói dịch vụ</p>
            </div>
        `;
        return;
    }

    packagesGrid.innerHTML = packages.map(pkg => createPackageCard(pkg)).join('');
}

// Create package card HTML
function createPackageCard(pkg) {
    const durationText = pkg.durationDays >= 365 ? 'năm' : 'tháng';
    const badgeHtml = pkg.badgeText ? `
        <div class="package-badge ${pkg.badgeColor}">
            ${pkg.badgeText}
        </div>
    ` : '';

    const originalPriceHtml = pkg.originalPrice ? `
        <span class="price-original">${formatCurrency(pkg.originalPrice)}</span>
    ` : '';

    return `
        <div class="package-card ${pkg.isPopular ? 'popular' : ''}" data-package-id="${pkg.id}">
            ${badgeHtml}
            <div class="package-header">
                <div class="package-icon">
                    <i class="fas ${getPackageIcon(pkg.name)}"></i>
                </div>
                <h3 class="package-name">${pkg.name}</h3>
                <p class="package-description">${pkg.description}</p>
            </div>
            <div class="package-pricing">
                <div class="package-price">
                    ${originalPriceHtml}
                    <span class="price-amount">${formatCurrency(pkg.price)}</span>
                    <span class="price-currency">₫</span>
                </div>
                <p class="price-period">/${durationText}</p>
            </div>
            <div class="package-features">
                ${pkg.features.map(feature => `
                    <div class="feature-item">
                        <div class="feature-icon">
                            <i class="fas fa-check"></i>
                        </div>
                        <span class="feature-text">${feature}</span>
                    </div>
                `).join('')}
            </div>
            <div class="package-actions">
                <button class="btn-register" onclick="registerPackage(${pkg.id})">
                    <i class="fas fa-rocket"></i>
                    Đăng ký ngay
                </button>
                <button class="btn-details" onclick="showPackageDetails(${pkg.id})">
                    Xem chi tiết
                </button>
            </div>
        </div>
    `;
}

// Get package icon based on name
function getPackageIcon(name) {
    if (name.includes('Miễn Phí')) return 'fa-gift';
    if (name.includes('Cơ Bản')) return 'fa-star';
    if (name.includes('Chuyên Nghiệp')) return 'fa-crown';
    if (name.includes('Doanh Nghiệp')) return 'fa-building';
    return 'fa-box';
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

// Build comparison table
function buildComparisonTable() {
    const table = document.getElementById('comparisonTable');
    if (!table || allPackages.length === 0) return;

    // Build header
    const thead = table.querySelector('thead tr');
    thead.innerHTML = '<th>Tính năng</th>' + 
        allPackages.map(pkg => `<th>${pkg.name}</th>`).join('');

    // Collect all unique features
    const allFeatures = new Set();
    allPackages.forEach(pkg => {
        pkg.features.forEach(feature => allFeatures.add(feature));
    });

    // Build body
    const tbody = table.querySelector('tbody');
    tbody.innerHTML = Array.from(allFeatures).map(feature => {
        const row = `<tr>
            <td class="feature-name">${feature}</td>
            ${allPackages.map(pkg => {
                const hasFeature = pkg.features.includes(feature);
                return `<td class="text-center">
                    ${hasFeature ? 
                        '<i class="fas fa-check-circle check-icon"></i>' : 
                        '<i class="fas fa-times-circle cross-icon"></i>'}
                </td>`;
            }).join('')}
        </tr>`;
        return row;
    }).join('');
}

// Show package details modal
function showPackageDetails(packageId) {
    const pkg = allPackages.find(p => p.id === packageId);
    if (!pkg) return;

    const modal = document.getElementById('packageModal');
    const modalTitle = document.getElementById('modalTitle');
    const modalBody = document.getElementById('modalBody');

    modalTitle.textContent = pkg.name;
    
    const durationText = pkg.durationDays >= 365 ? 
        `${Math.floor(pkg.durationDays / 365)} năm` : 
        `${pkg.durationDays} ngày`;

    modalBody.innerHTML = `
        <div style="margin-bottom: 24px;">
            <h4 style="margin-bottom: 8px;">Mô tả</h4>
            <p style="color: var(--text-secondary);">${pkg.description}</p>
        </div>
        <div style="margin-bottom: 24px;">
            <h4 style="margin-bottom: 8px;">Giá</h4>
            <div style="display: flex; align-items: baseline; gap: 8px;">
                ${pkg.originalPrice ? `<span style="text-decoration: line-through; color: var(--text-muted);">${formatCurrency(pkg.originalPrice)} ₫</span>` : ''}
                <span style="font-size: 32px; font-weight: 700; color: var(--primary);">${formatCurrency(pkg.price)} ₫</span>
                <span style="color: var(--text-secondary);">/ ${durationText}</span>
            </div>
        </div>
        <div style="margin-bottom: 24px;">
            <h4 style="margin-bottom: 16px;">Tính năng</h4>
            <div style="display: grid; gap: 12px;">
                ${pkg.features.map(feature => `
                    <div style="display: flex; align-items: center; gap: 12px;">
                        <i class="fas fa-check-circle" style="color: var(--success);"></i>
                        <span>${feature}</span>
                    </div>
                `).join('')}
            </div>
        </div>
        <div style="display: flex; gap: 12px;">
            <button class="btn btn-primary btn-block" onclick="registerPackage(${pkg.id})">
                <i class="fas fa-rocket"></i>
                Đăng ký ngay
            </button>
            <button class="btn btn-outline" onclick="closePackageModal()">
                Đóng
            </button>
        </div>
    `;

    modal.classList.add('active');
}

// Close package modal
function closePackageModal() {
    const modal = document.getElementById('packageModal');
    modal.classList.remove('active');
}

// Register package
function registerPackage(packageId) {
    const pkg = allPackages.find(p => p.id === packageId);
    if (!pkg) {
        showPaymentError('Không tìm thấy gói dịch vụ. Vui lòng thử lại.');
        return;
    }

    try {
        // Redirect to internal checkout page with package information
        const checkoutUrl = `/Subscription/Checkout?packageId=${packageId}`;
        window.location.href = checkoutUrl;
    } catch (error) {
        console.error('Error redirecting to checkout:', error);
        showPaymentError('Không thể chuyển đến trang thanh toán. Vui lòng thử lại sau.');
    }
}



// Show payment error
function showPaymentError(message) {
    const errorModal = document.createElement('div');
    errorModal.innerHTML = `
        <div style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
        ">
            <div style="
                background: white;
                padding: 32px;
                border-radius: 16px;
                max-width: 400px;
                text-align: center;
            ">
                <div style="
                    width: 60px;
                    height: 60px;
                    background: #fee2e2;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 20px;
                ">
                    <i class="fas fa-exclamation-triangle" style="color: #ef4444; font-size: 24px;"></i>
                </div>
                <h3 style="margin-bottom: 12px; color: #1f2937;">Lỗi thanh toán</h3>
                <p style="color: #6b7280; margin-bottom: 24px;">${message}</p>
                <button onclick="this.closest('div').parentElement.remove()" style="
                    background: #10b981;
                    color: white;
                    border: none;
                    padding: 12px 24px;
                    border-radius: 8px;
                    cursor: pointer;
                    font-size: 16px;
                    font-weight: 500;
                ">
                    Đóng
                </button>
            </div>
        </div>
    `;
    document.body.appendChild(errorModal);
}

// Close modal on escape key
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closePackageModal();
    }
});
