// Admin AI JavaScript

let adminAiData = {
    churnPredictions: [],
    fraudAlerts: []
};

document.addEventListener('DOMContentLoaded', function() {
    loadAdminAI();
});

async function loadAdminAI() {
    const content = document.getElementById('adminAiContent');
    if (!content) return;

    try {
        content.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><p class="text-muted mt-2">Đang tải thông tin AI...</p></div>';

        // Load churn predictions and fraud detection in parallel
        const [churnRes, fraudRes] = await Promise.all([
            fetch('/api/admin/ai/churn-prediction', {
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            }).catch(() => null),
            fetch('/api/admin/ai/fraud-detection', {
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            }).catch(() => null)
        ]);

        let html = '';

        // Churn Predictions Section
        if (churnRes && churnRes.ok) {
            const churnData = await churnRes.json();
            adminAiData.churnPredictions = churnData;

            if (churnData.length > 0) {
                html += `
                    <div class="admin-ai-section">
                        <h6 class="mb-3">
                            <i class="fas fa-exclamation-triangle text-warning me-2"></i>
                            Dự đoán rủi ro rời bỏ (${churnData.length} người dùng)
                        </h6>
                        <div class="row g-3">
                `;

                churnData.slice(0, 3).forEach(prediction => {
                    const riskClass = prediction.riskLevel === 'high' ? 'high-risk' : 
                                     prediction.riskLevel === 'medium' ? 'medium-risk' : '';
                    html += `
                        <div class="col-md-4">
                            <div class="card admin-ai-card ${riskClass}">
                                <div class="card-body">
                                    <div class="d-flex justify-content-between align-items-start mb-2">
                                        <div>
                                            <h6 class="mb-1">${escapeHtml(prediction.userName)}</h6>
                                            <small class="text-muted">${escapeHtml(prediction.userEmail)}</small>
                                        </div>
                                        <span class="risk-badge ${prediction.riskLevel}">
                                            ${prediction.riskPercentage}%
                                        </span>
                                    </div>
                                    <div class="mb-2">
                                        <small class="text-muted">Yếu tố rủi ro:</small>
                                        <ul class="small mb-0 ps-3">
                                            ${prediction.riskFactors.slice(0, 2).map(f => `<li>${escapeHtml(f)}</li>`).join('')}
                                        </ul>
                                    </div>
                                    <div>
                                        <small class="text-muted">Hành động đề xuất:</small>
                                        <p class="small mb-0">${escapeHtml(prediction.suggestedActions[0] || 'Theo dõi thêm')}</p>
                                    </div>
                                    <button class="btn btn-sm btn-outline-primary mt-2 w-100" onclick="viewUserDetails(${prediction.userId})">
                                        Xem chi tiết
                                    </button>
                                </div>
                            </div>
                        </div>
                    `;
                });

                html += `
                        </div>
                        ${churnData.length > 3 ? `<p class="text-muted small mt-2">Và ${churnData.length - 3} người dùng khác...</p>` : ''}
                    </div>
                `;
            }
        }

        // Fraud Detection Section
        if (fraudRes && fraudRes.ok) {
            const fraudData = await fraudRes.json();
            adminAiData.fraudAlerts = fraudData;

            if (fraudData.length > 0) {
                html += `
                    <div class="admin-ai-section">
                        <h6 class="mb-3">
                            <i class="fas fa-shield-alt text-danger me-2"></i>
                            Cảnh báo gian lận (${fraudData.length} cảnh báo)
                        </h6>
                        <div class="row g-3">
                `;

                fraudData.slice(0, 2).forEach(alert => {
                    html += `
                        <div class="col-md-6">
                            <div class="card admin-ai-card fraud-alert">
                                <div class="card-body">
                                    <div class="d-flex justify-content-between align-items-start mb-2">
                                        <div>
                                            <h6 class="mb-1">${escapeHtml(alert.message)}</h6>
                                            <small class="text-muted">${formatDate(alert.detectedAt)}</small>
                                        </div>
                                        <span class="badge bg-${alert.severity === 'critical' ? 'danger' : 'warning'}">
                                            ${alert.severity}
                                        </span>
                                    </div>
                                    <div class="mb-2">
                                        <small class="text-muted">Ảnh hưởng: ${alert.affectedAccountCount} tài khoản</small>
                                        ${alert.autoBlocked ? '<span class="badge bg-danger ms-2">Đã tự động khóa</span>' : ''}
                                    </div>
                                    <button class="btn btn-sm btn-outline-danger w-100" onclick="reviewFraudAlert('${alert.alertType}')">
                                        Xem xét ngay
                                    </button>
                                </div>
                            </div>
                        </div>
                    `;
                });

                html += `
                        </div>
                    </div>
                `;
            }
        }

        // Natural Language Query Section
        html += `
            <div class="admin-ai-section">
                <h6 class="mb-3">
                    <i class="fas fa-comments text-info me-2"></i>
                    Phân tích dữ liệu bằng ngôn ngữ tự nhiên
                </h6>
                <div class="card">
                    <div class="card-body">
                        <div class="input-group mb-3">
                            <input type="text" class="form-control ai-query-input" id="adminAiQuery" 
                                   placeholder="Ví dụ: So sánh doanh thu gói Pro tháng này với cùng kỳ năm ngoái">
                            <button class="btn btn-primary" onclick="processAdminAiQuery()">
                                <i class="fas fa-search"></i> Hỏi
                            </button>
                        </div>
                        <div id="adminAiQueryResult" class="mt-3"></div>
                    </div>
                </div>
            </div>
        `;

        if (html === '') {
            html = '<div class="text-center py-4 text-muted">Chưa có cảnh báo nào. Hệ thống đang hoạt động bình thường.</div>';
        }

        content.innerHTML = html;
    } catch (error) {
        console.error('Error loading admin AI:', error);
        content.innerHTML = '<div class="text-center py-4 text-danger">Đã xảy ra lỗi khi tải thông tin AI. Vui lòng thử lại sau.</div>';
    }
}

async function refreshAdminAI() {
    await loadAdminAI();
}

async function processAdminAiQuery() {
    const queryInput = document.getElementById('adminAiQuery');
    const resultDiv = document.getElementById('adminAiQueryResult');
    
    if (!queryInput || !resultDiv) return;

    const query = queryInput.value.trim();
    if (!query) {
        alert('Vui lòng nhập câu hỏi');
        return;
    }

    resultDiv.innerHTML = '<div class="text-center py-2"><div class="spinner-border spinner-border-sm text-primary"></div></div>';

    try {
        const response = await fetch('/api/admin/ai/query', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify({ query })
        });

        if (response.ok) {
            const data = await response.json();
            
            let resultHtml = `
                <div class="alert alert-info">
                    <strong>Kết quả:</strong><br>
                    ${escapeHtml(data.answer)}
                </div>
            `;

            if (data.insights) {
                resultHtml += `
                    <div class="alert alert-success">
                        <strong>Phân tích:</strong><br>
                        ${escapeHtml(data.insights)}
                    </div>
                `;
            }

            if (data.chartData) {
                resultHtml += `<div class="mt-3"><pre class="bg-light p-3 rounded">${JSON.stringify(data.chartData, null, 2)}</pre></div>`;
            }

            resultDiv.innerHTML = resultHtml;
        } else {
            resultDiv.innerHTML = '<div class="alert alert-danger">Không thể xử lý câu hỏi. Vui lòng thử lại.</div>';
        }
    } catch (error) {
        console.error('Error processing query:', error);
        resultDiv.innerHTML = '<div class="alert alert-danger">Đã xảy ra lỗi khi xử lý câu hỏi.</div>';
    }
}

function viewUserDetails(userId) {
    window.location.href = `/Admin/Users?userId=${userId}`;
}

function reviewFraudAlert(alertType) {
    alert(`Xem xét cảnh báo gian lận: ${alertType}\n\nTính năng này sẽ mở trang quản lý người dùng để xem xét chi tiết.`);
    // Can redirect to user management with filter
    window.location.href = '/Admin/Users';
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', { 
        day: '2-digit', 
        month: '2-digit', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

