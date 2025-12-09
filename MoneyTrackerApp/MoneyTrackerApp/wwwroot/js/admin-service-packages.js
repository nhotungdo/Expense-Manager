// Admin Service Packages Management

let packages = [];
let editingPackageId = null;

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    loadPackages();
});

// Load all packages
async function loadPackages() {
    try {
        const response = await fetch('/api/ServicePackage?activeOnly=false');
        packages = await response.json();
        renderPackagesTable();
    } catch (error) {
        console.error('Error loading packages:', error);
        showAlert('Không thể tải danh sách gói dịch vụ', 'danger');
    }
}

// Render packages table
function renderPackagesTable() {
    const tbody = document.getElementById('packagesTableBody');
    
    if (packages.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted">
                    Chưa có gói dịch vụ nào
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = packages.map(pkg => `
        <tr>
            <td>${pkg.id}</td>
            <td>
                <strong>${pkg.name}</strong>
                ${pkg.badgeText ? `<br><span class="badge bg-warning">${pkg.badgeText}</span>` : ''}
            </td>
            <td>
                ${formatCurrency(pkg.price)} ₫
                ${pkg.originalPrice ? `<br><small class="text-muted"><del>${formatCurrency(pkg.originalPrice)} ₫</del></small>` : ''}
            </td>
            <td>${pkg.durationDays >= 365 ? Math.floor(pkg.durationDays / 365) + ' năm' : pkg.durationDays + ' ngày'}</td>
            <td>
                ${pkg.isPopular ? '<span class="badge bg-success">Có</span>' : '<span class="badge bg-secondary">Không</span>'}
            </td>
            <td>
                <span class="badge bg-${pkg.isActive ? 'success' : 'danger'}">
                    ${pkg.isActive ? 'Hoạt động' : 'Tắt'}
                </span>
            </td>
            <td>${pkg.displayOrder}</td>
            <td>
                <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-primary" onclick="editPackage(${pkg.id})" title="Sửa">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-warning" onclick="togglePackageStatus(${pkg.id})" title="Bật/Tắt">
                        <i class="fas fa-power-off"></i>
                    </button>
                    <button class="btn btn-outline-danger" onclick="deletePackage(${pkg.id})" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// Open create modal
function openCreateModal() {
    editingPackageId = null;
    document.getElementById('modalTitle').textContent = 'Thêm gói dịch vụ';
    document.getElementById('packageForm').reset();
    document.getElementById('packageIsActive').checked = true;
    
    const modal = new bootstrap.Modal(document.getElementById('packageModal'));
    modal.show();
}

// Edit package
function editPackage(id) {
    const pkg = packages.find(p => p.id === id);
    if (!pkg) return;

    editingPackageId = id;
    document.getElementById('modalTitle').textContent = 'Sửa gói dịch vụ';
    
    document.getElementById('packageId').value = pkg.id;
    document.getElementById('packageName').value = pkg.name;
    document.getElementById('packageDescription').value = pkg.description || '';
    document.getElementById('packagePrice').value = pkg.price;
    document.getElementById('packageOriginalPrice').value = pkg.originalPrice || '';
    document.getElementById('packageDuration').value = pkg.durationDays;
    document.getElementById('packageFeatures').value = pkg.features.join('\n');
    document.getElementById('packageBadgeText').value = pkg.badgeText || '';
    document.getElementById('packageBadgeColor').value = pkg.badgeColor || '';
    document.getElementById('packageDisplayOrder').value = pkg.displayOrder;
    document.getElementById('packageIsPopular').checked = pkg.isPopular;
    document.getElementById('packageIsActive').checked = pkg.isActive;

    const modal = new bootstrap.Modal(document.getElementById('packageModal'));
    modal.show();
}

// Save package
async function savePackage() {
    const form = document.getElementById('packageForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const featuresText = document.getElementById('packageFeatures').value;
    const features = featuresText.split('\n').filter(f => f.trim()).map(f => f.trim());

    const data = {
        name: document.getElementById('packageName').value,
        description: document.getElementById('packageDescription').value || null,
        price: parseFloat(document.getElementById('packagePrice').value),
        originalPrice: document.getElementById('packageOriginalPrice').value ? 
            parseFloat(document.getElementById('packageOriginalPrice').value) : null,
        durationDays: parseInt(document.getElementById('packageDuration').value),
        features: features,
        badgeText: document.getElementById('packageBadgeText').value || null,
        badgeColor: document.getElementById('packageBadgeColor').value || null,
        displayOrder: parseInt(document.getElementById('packageDisplayOrder').value),
        isPopular: document.getElementById('packageIsPopular').checked,
        isActive: document.getElementById('packageIsActive').checked
    };

    try {
        let response;
        if (editingPackageId) {
            data.id = editingPackageId;
            response = await fetch(`/api/ServicePackage/${editingPackageId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
        } else {
            response = await fetch('/api/ServicePackage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
        }

        if (response.ok) {
            showAlert(editingPackageId ? 'Cập nhật thành công' : 'Thêm mới thành công', 'success');
            bootstrap.Modal.getInstance(document.getElementById('packageModal')).hide();
            loadPackages();
        } else {
            const error = await response.json();
            showAlert(error.message || 'Có lỗi xảy ra', 'danger');
        }
    } catch (error) {
        console.error('Error saving package:', error);
        showAlert('Không thể lưu gói dịch vụ', 'danger');
    }
}

// Toggle package status
async function togglePackageStatus(id) {
    if (!confirm('Bạn có chắc muốn thay đổi trạng thái gói này?')) return;

    try {
        const response = await fetch(`/api/ServicePackage/${id}/toggle-status`, {
            method: 'PATCH'
        });

        if (response.ok) {
            showAlert('Cập nhật trạng thái thành công', 'success');
            loadPackages();
        } else {
            showAlert('Không thể cập nhật trạng thái', 'danger');
        }
    } catch (error) {
        console.error('Error toggling status:', error);
        showAlert('Có lỗi xảy ra', 'danger');
    }
}

// Delete package
async function deletePackage(id) {
    if (!confirm('Bạn có chắc muốn xóa gói này? Hành động này không thể hoàn tác!')) return;

    try {
        const response = await fetch(`/api/ServicePackage/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showAlert('Xóa thành công', 'success');
            loadPackages();
        } else {
            showAlert('Không thể xóa gói dịch vụ', 'danger');
        }
    } catch (error) {
        console.error('Error deleting package:', error);
        showAlert('Có lỗi xảy ra', 'danger');
    }
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

// Show alert
function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
    alertDiv.style.zIndex = '9999';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}
