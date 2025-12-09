// AI Advisor Page JavaScript

let allSuggestions = [];
let currentFilter = 'all';

document.addEventListener('DOMContentLoaded', function() {
    loadSuggestions();
});

async function loadSuggestions() {
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            showError('Vui lòng đăng nhập để xem gợi ý');
            return;
        }
        
        const response = await fetch('/api/AiAdvisor/suggestions', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (response.ok) {
            allSuggestions = await response.json();
            renderSuggestions();
            updateStats();
        } else {
            showError('Không thể tải gợi ý');
        }
    } catch (error) {
        console.error('Error loading suggestions:', error);
        showError('Đã xảy ra lỗi khi tải gợi ý');
    }
}

function renderSuggestions() {
    const container = document.getElementById('suggestionsContainer');
    
    if (!allSuggestions || allSuggestions.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-brain"></i>
                <h3>Chưa có gợi ý nào</h3>
                <p>Nhấn "Tạo gợi ý mới" để AI phân tích tài chính của bạn</p>
                <button class="btn btn-primary" onclick="generateNewInsights()">
                    <i class="fas fa-magic"></i>
                    Tạo gợi ý mới
                </button>
            </div>
        `;
        return;
    }
    
    // Filter suggestions
    let filteredSuggestions = allSuggestions;
    if (currentFilter !== 'all') {
        filteredSuggestions = allSuggestions.filter(s => 
            s.suggestionType?.toLowerCase() === currentFilter
        );
    }
    
    if (filteredSuggestions.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-filter"></i>
                <h3>Không tìm thấy gợi ý</h3>
                <p>Không có gợi ý nào phù hợp với bộ lọc này</p>
            </div>
        `;
        return;
    }
    
    const iconMap = {
        success: 'fa-check-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-lightbulb',
        danger: 'fa-exclamation-circle'
    };
    
    const titleMap = {
        success: 'Tiến triển tốt',
        warning: 'Cảnh báo',
        info: 'Mẹo hữu ích',
        danger: 'Cảnh báo quan trọng'
    };
    
    container.innerHTML = filteredSuggestions.map(s => {
        const type = s.suggestionType?.toLowerCase() || 'info';
        const icon = iconMap[type] || 'fa-lightbulb';
        const title = titleMap[type] || 'Thông tin';
        const date = new Date(s.createdAt);
        const timeAgo = getTimeAgo(date);
        
        return `
            <div class="suggestion-card ${type}" data-id="${s.id}">
                <div class="suggestion-icon">
                    <i class="fas ${icon}"></i>
                </div>
                <div class="suggestion-content">
                    <h3 class="suggestion-title">${title}</h3>
                    <p class="suggestion-text">${s.suggestion}</p>
                    <div class="suggestion-meta">
                        <span><i class="far fa-clock"></i>${timeAgo}</span>
                        ${s.isRead ? '<span><i class="fas fa-check"></i>Đã đọc</span>' : ''}
                    </div>
                </div>
                <div class="suggestion-actions">
                    ${!s.isRead ? `<button class="action-btn" onclick="markAsRead(${s.id})" title="Đánh dấu đã đọc">
                        <i class="fas fa-check"></i>
                    </button>` : ''}
                    <button class="action-btn" onclick="deleteSuggestion(${s.id})" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

function updateStats() {
    const successCount = allSuggestions.filter(s => s.suggestionType?.toLowerCase() === 'success').length;
    const warningCount = allSuggestions.filter(s => s.suggestionType?.toLowerCase() === 'warning').length;
    const infoCount = allSuggestions.filter(s => s.suggestionType?.toLowerCase() === 'info').length;
    
    document.getElementById('positiveCount').textContent = successCount;
    document.getElementById('warningCount').textContent = warningCount;
    document.getElementById('tipsCount').textContent = infoCount;
    
    if (allSuggestions.length > 0) {
        const latestDate = new Date(Math.max(...allSuggestions.map(s => new Date(s.createdAt))));
        document.getElementById('lastUpdate').textContent = getTimeAgo(latestDate);
    }
}

function filterSuggestions(filter) {
    currentFilter = filter;
    
    // Update active tab
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.closest('.tab-btn').classList.add('active');
    
    renderSuggestions();
}

async function generateNewInsights() {
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            showError('Vui lòng đăng nhập');
            return;
        }
        
        // Show loading
        const btn = event.target.closest('button');
        const originalHTML = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang tạo...';
        btn.disabled = true;
        
        const response = await fetch('/api/AiAdvisor/generate', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (response.ok) {
            showSuccess('Đã tạo gợi ý mới thành công!');
            await loadSuggestions();
        } else {
            showError('Không thể tạo gợi ý mới');
        }
        
        // Restore button
        btn.innerHTML = originalHTML;
        btn.disabled = false;
    } catch (error) {
        console.error('Error generating insights:', error);
        showError('Đã xảy ra lỗi');
        
        const btn = event.target.closest('button');
        btn.innerHTML = '<i class="fas fa-magic"></i> Tạo gợi ý mới';
        btn.disabled = false;
    }
}

function refreshInsights() {
    loadSuggestions();
}

async function markAsRead(id) {
    // This would call an API to mark as read
    // For now, just update locally
    const suggestion = allSuggestions.find(s => s.id === id);
    if (suggestion) {
        suggestion.isRead = true;
        renderSuggestions();
    }
}

async function deleteSuggestion(id) {
    if (!confirm('Bạn có chắc muốn xóa gợi ý này?')) {
        return;
    }
    
    // This would call an API to delete
    // For now, just remove locally
    allSuggestions = allSuggestions.filter(s => s.id !== id);
    renderSuggestions();
    updateStats();
    showSuccess('Đã xóa gợi ý');
}

function getTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);
    
    const intervals = {
        năm: 31536000,
        tháng: 2592000,
        tuần: 604800,
        ngày: 86400,
        giờ: 3600,
        phút: 60
    };
    
    for (const [name, secondsInInterval] of Object.entries(intervals)) {
        const interval = Math.floor(seconds / secondsInInterval);
        if (interval >= 1) {
            return `${interval} ${name} trước`;
        }
    }
    
    return 'Vừa xong';
}

function showSuccess(message) {
    showToast(message, 'success');
}

function showError(message) {
    showToast(message, 'error');
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}`;
    
    const icon = type === 'success' ? 'fa-check-circle' : 
                 type === 'error' ? 'fa-exclamation-circle' : 
                 'fa-info-circle';
    
    const bgColor = type === 'success' ? '#10b981' : 
                    type === 'error' ? '#ef4444' : 
                    '#3b82f6';
    
    toast.innerHTML = `
        <i class="fas ${icon}"></i>
        <span>${message}</span>
    `;
    
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${bgColor};
        color: white;
        padding: 16px 24px;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 9999;
        display: flex;
        align-items: center;
        gap: 12px;
        font-weight: 500;
        animation: slideIn 0.3s ease;
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
