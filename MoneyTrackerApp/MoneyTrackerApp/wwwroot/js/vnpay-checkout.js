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
        return new Intl.NumberFormat('vi-VN').format(amount) + ' ₫';
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
                showQrCode(data.paymentUrl);
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

    function showQrCode(url) {
        const checkoutForm = document.getElementById('checkoutForm');
        if (checkoutForm) {
            checkoutForm.innerHTML = `
                <div class="card-body text-center">
                    <h5 class="mb-3">Quét mã để thanh toán</h5>
                    <div class="mb-3">
                        <img src="${url}" alt="VietQR" class="img-fluid" style="max-width: 100%; border: 1px solid #eee; padding: 0.5rem; border-radius: 0.5rem;" />
                    </div>
                    <p class="text-muted small mb-3">
                        Sử dụng ứng dụng ngân hàng của bạn để quét mã QR và hoàn tất thanh toán.
                    </p>
                    <button class="btn btn-outline-primary" onclick="window.location.reload()">
                        Đã thanh toán xong?
                    </button>
                    <div class="mt-2">
                        <small class="text-secondary">Hệ thống sẽ tự động cập nhật (giả lập)</small>
                    </div>
                </div>
            `;
        }
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const pkg = await loadPackage();
        if (!pkg) return;
        await submitPayment(pkg);
    });

    // initial load to show info
    loadPackage();
})();





