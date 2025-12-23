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
            window.location.href = data.paymentUrl;
        } catch (err) {
            console.error(err);
            showToast(err.message, 'danger');
            payButton.disabled = false;
            payButton.innerText = 'Thanh toán bằng VNPay QR';
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





