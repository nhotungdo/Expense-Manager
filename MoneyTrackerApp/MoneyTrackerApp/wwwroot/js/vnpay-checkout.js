(() => {
    const packageNameEl = document.getElementById('packageName');
    const packageDescEl = document.getElementById('packageDescription');
    const packagePriceEl = document.getElementById('packagePrice');
    const payButton = document.getElementById('payButton');
    const form = document.getElementById('checkoutForm');

    const context = window.checkoutContext || {};
    const packageId = context.packageId || 2;
    const userId = context.userId || null;

    async function loadPackage() {
        try {
            const res = await fetch(`/api/ServicePackage/${packageId}`);
            if (!res.ok) throw new Error('Không lấy được gói dịch vụ');
            const pkg = await res.json();
            packageNameEl.textContent = pkg.name;
            packageDescEl.textContent = pkg.description;
            packagePriceEl.textContent = formatVnd(pkg.price);
            return pkg;
        } catch (err) {
            console.error(err);
            showToast('Không tải được giá gói dịch vụ', 'danger');
            return null;
        }
    }

    function formatVnd(amount) {
        return window.formatCurrency(amount) + ' ₫';
    }

    function showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3`;
        toast.textContent = message;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 4000);
    }

    async function submitPayment(pkg) {
        payButton.disabled = true;
        payButton.innerText = 'Đang tạo liên kết...';
        try {
            const res = await fetch('/api/payments/vnpay/qr', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId, packageId: pkg.id })
            });
            if (!res.ok) throw new Error('Không tạo được liên kết thanh toán');
            const data = await res.json();
            if (!data.success || !data.paymentUrl) throw new Error('Thiếu URL thanh toán');

            // Allow displaying QR directly if it is an image
            if (data.paymentUrl.includes('img.vietqr.io')) {
                showQrCode(data.paymentUrl, data.bankInfo);
            } else {
                window.location.href = data.paymentUrl;
            }

        } catch (err) {
            console.error(err);
            showToast(err.message, 'danger');
            payButton.disabled = false;
            payButton.innerText = 'Thanh toán bằng VNPay QR';
        }
    }

    function showQrCode(url, bankInfo) {
        const checkoutForm = document.getElementById('checkoutForm');
        if (checkoutForm) {
            let bankInfoHtml = '';

            if (bankInfo) {
                // Handle potential casing differences (camelCase vs PascalCase)
                const getVal = (key) => bankInfo[key] || bankInfo[key.charAt(0).toUpperCase() + key.slice(1)];

                const bankId = getVal('bankId');
                const bankName = getVal('bankName') || 'Ngân hàng TMCP Đầu tư và Phát triển Việt Nam';
                const accountNo = getVal('accountNo');
                const accountName = getVal('accountName');
                const amount = getVal('amount');
                const description = getVal('description');

                if (bankId && accountNo) {
                    bankInfoHtml = `
                    <div class="mt-4 text-start bg-light p-3 rounded">
                        <div class="d-flex align-items-center justify-content-between mb-3 pb-2 border-bottom">
                            <h6 class="fw-bold text-dark mb-0">
                                <i class="bi bi-bank2 me-2 text-primary"></i>Chi tiết đơn hàng
                            </h6>
                            <a href="#" class="text-primary text-decoration-none small">Xem</a>
                        </div>
                        
                        <div class="alert alert-light border mb-3 p-2">
                            <div class="d-flex align-items-start">
                                <i class="bi bi-lightbulb-fill text-warning me-2 mt-1"></i>
                                <small class="text-muted">
                                    Mở App Ngân hàng bất kỳ để <strong class="text-dark">quét mã VietQR</strong> hoặc 
                                    <strong class="text-dark">chuyển khoản</strong> chính xác số tiền bên dưới
                                </small>
                            </div>
                        </div>
                        
                        <div class="bg-white p-3 rounded shadow-sm">
                            <div class="mb-3 pb-2 border-bottom">
                                <div class="d-flex align-items-center mb-2">
                                    <div class="bg-primary rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 32px; height: 32px;">
                                        <i class="bi bi-bank text-white"></i>
                                    </div>
                                    <div>
                                        <div class="small text-muted">Ngân hàng</div>
                                        <div class="fw-bold text-dark">${bankName}</div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="row mb-2">
                                <div class="col-5 text-muted small">Chủ tài khoản:</div>
                                <div class="col-7">
                                    <div class="fw-bold text-dark text-uppercase">${accountName}</div>
                                </div>
                            </div>
                            
                            <div class="row mb-2">
                                <div class="col-5 text-muted small">Số tài khoản:</div>
                                <div class="col-7">
                                    <div class="d-flex align-items-center justify-content-between bg-light p-2 rounded">
                                        <span class="fw-bold text-primary">${accountNo}</span>
                                        <button class="btn btn-sm btn-outline-secondary py-0 px-2" 
                                                onclick="navigator.clipboard.writeText('${accountNo}'); this.innerHTML='<i class=\\'bi bi-check\\'></i> Đã sao'; setTimeout(() => this.innerHTML='Sao chép', 2000);" 
                                                title="Sao chép">
                                            Sao chép
                                        </button>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="row mb-2">
                                <div class="col-5 text-muted small">Số tiền:</div>
                                <div class="col-7">
                                    <div class="d-flex align-items-center justify-content-between bg-light p-2 rounded">
                                        <span class="fw-bold text-danger fs-6">${formatVnd(amount)}</span>
                                        <button class="btn btn-sm btn-outline-secondary py-0 px-2" 
                                                onclick="navigator.clipboard.writeText('${amount}'); this.innerHTML='<i class=\\'bi bi-check\\'></i> Đã sao'; setTimeout(() => this.innerHTML='Sao chép', 2000);" 
                                                title="Sao chép">
                                            Sao chép
                                        </button>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="row mb-0">
                                <div class="col-5 text-muted small">Nội dung:</div>
                                <div class="col-7">
                                    <div class="d-flex align-items-center justify-content-between bg-light p-2 rounded">
                                        <span class="fw-bold text-dark text-break">${description}</span>
                                        <button class="btn btn-sm btn-outline-secondary py-0 px-2" 
                                                onclick="navigator.clipboard.writeText('${description}'); this.innerHTML='<i class=\\'bi bi-check\\'></i> Đã sao'; setTimeout(() => this.innerHTML='Sao chép', 2000);" 
                                                title="Sao chép">
                                            Sao chép
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div class="alert alert-info d-flex align-items-start mt-3 small p-2 mb-0" role="alert">
                            <i class="bi bi-info-circle-fill me-2 mt-1"></i>
                            <div>
                                <strong>Lưu ý:</strong> Nhập chính xác số tiền <strong>${formatVnd(amount)}</strong> khi chuyển khoản
                            </div>
                        </div>
                    </div>
                    `;
                }
            }

            checkoutForm.innerHTML = `
                <div class="card-body text-center p-4">
                    <h5 class="mb-3 text-primary fw-bold">
                        <i class="bi bi-qr-code-scan me-2"></i>Quét mã để thanh toán
                    </h5>
                    
                    <div class="position-relative d-inline-block mb-3">
                        <img src="${url}" alt="VietQR" class="img-fluid border rounded shadow" style="max-width: 300px;" />
                        <div class="mt-2 text-muted small">
                            <i class="bi bi-phone me-1"></i>Mở App ngân hàng > Quét mã QR
                        </div>
                    </div>

                    ${bankInfoHtml}

                    <div class="mt-4 pt-3 border-top">
                        <button class="btn btn-success px-4 py-2 rounded-pill fw-bold shadow-sm" id="confirmPaymentBtn" onclick="confirmPayment()">
                            <i class="bi bi-check-circle-fill me-2"></i>Tôi đã thanh toán
                        </button>
                        <div class="mt-2">
                            <small class="text-muted fst-italic">Hệ thống sẽ tự động cập nhật ngay sau khi nhận tiền</small>
                        </div>
                    </div>
                </div>
            `;
        }
    }

    async function confirmPayment() {
        const confirmBtn = document.getElementById('confirmPaymentBtn');
        if (!confirmBtn) return;

        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...';

        try {
            const res = await fetch('/api/payments/confirm', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId, packageId })
            });

            const data = await res.json();

            if (!res.ok) {
                throw new Error(data.message || 'Không thể xác nhận thanh toán');
            }

            if (data.success) {
                showToast(data.message || 'Đã kích hoạt gói dịch vụ thành công!', 'success');

                // Redirect to subscription page after a short delay
                setTimeout(() => {
                    window.location.href = data.redirectUrl || '/Subscription';
                }, 1500);
            } else {
                throw new Error(data.message || 'Có lỗi xảy ra');
            }
        } catch (err) {
            console.error(err);
            showToast(err.message, 'danger');
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i>Tôi đã thanh toán';
        }
    }

    // Make confirmPayment available globally
    window.confirmPayment = confirmPayment;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const pkg = await loadPackage();
        if (!pkg) return;
        await submitPayment(pkg);
    });

    // initial load to show info
    loadPackage();
})();





