
document.addEventListener('DOMContentLoaded', function () {
    // Search Filter
    const searchInput = document.getElementById('walletSearch');
    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            const term = e.target.value.toLowerCase();
            document.querySelectorAll('.wallet-card-modern').forEach(card => {
                // Access data attributes properly or fallback to text content query
                const nameHelper = card.querySelector('.wallet-name');
                const sharedByHelper = card.querySelector('.shared-by-badge span');

                const name = nameHelper ? nameHelper.innerText.toLowerCase() : '';
                const sharedBy = sharedByHelper ? sharedByHelper.innerText.toLowerCase() : '';

                if (name.includes(term) || sharedBy.includes(term)) {
                    card.parentElement.style.display = 'block'; // Assuming wrapper
                } else {
                    card.style.display = 'none'; // This hides the card itself
                    // If wrapped in col-md-4, we might need to hide that. But for now 
                    // the grid structure uses direct children if possible or display:grid
                    // Grid structure in shared-wallets.css is:
                    // .shared-wallets-grid { display: grid ... }
                    // .wallet-card-modern { ... }
                    // So hiding .wallet-card-modern works.
                }
            });
        });
    }
});

function openShareModal() {
    var modalEl = document.getElementById('shareWalletModal');
    if (modalEl) {
        var modal = new bootstrap.Modal(modalEl);
        modal.show();
    }
}

async function submitShareWallet() {
    const accountId = document.getElementById('shareAccountId').value;
    const friendId = document.getElementById('shareFriendId').value;
    const permission = document.getElementById('sharePermission').value;

    if (!accountId || !friendId) {
        alert('Vui lòng chọn ví và người nhận.');
        return;
    }

    const btn = document.getElementById('btnShareSubmit');
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {
        const response = await fetch('/Wallets/Shared/Index?handler=Share', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                accountId: parseInt(accountId),
                userId: parseInt(friendId),
                permission: parseInt(permission)
            })
        });

        const result = await response.json();
        if (result.success) {
            // Close modal
            const modalEl = document.getElementById('shareWalletModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();

            // Show success (Optional: Toast)
            alert('Đã chia sẻ ví thành công!');
            location.reload();
        } else {
            alert('Lỗi: ' + result.message);
        }
    } catch (error) {
        console.error(error);
        alert('Đã xảy ra lỗi kết nối.');
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalText;
    }
}

async function leaveWallet(sharedAccountId) {
    if (!confirm('Bạn có chắc chắn muốn rời khỏi ví chia sẻ này? Bạn sẽ không thể truy cập nó nữa.')) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {
        const response = await fetch('/Wallets/Shared/Index?handler=Leave', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                sharedAccountId: parseInt(sharedAccountId)
            })
        });

        const result = await response.json();
        if (result.success) {
            alert('Đã rời khỏi ví thành công.');
            location.reload();
        } else {
            alert('Lỗi: ' + result.message);
        }
    } catch (error) {
        console.error(error);
        alert('Đã xảy ra lỗi kết nối.');
    }
}
