// User AI JavaScript

let userAiData = {
    planRecommendation: null,
    spendingForecast: null
};

document.addEventListener('DOMContentLoaded', function() {
    loadUserAI();
});

async function loadUserAI() {
    const content = document.getElementById('userAiContent');
    if (!content) return;

    try {
        content.innerHTML = '<div class="ai-loading"><div class="spinner"></div><p>Đang tải thông tin AI...</p></div>';

        // Load plan recommendation and spending forecast in parallel
        const [planRec, forecast] = await Promise.all([
            fetch('/api/user-ai/plan-recommendation', {
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            }).catch(() => null),
            fetch('/api/user-ai/spending-forecast', {
                credentials: 'include',
                headers: {
                    'Accept': 'application/json'
                }
            }).catch(() => null)
        ]);

        let html = '';

        // Plan Recommendation
        if (planRec && planRec.ok) {
            const planData = await planRec.json();
            userAiData.planRecommendation = planData;
            
            if (planData.recommendationType !== 'info') {
                html += `
                    <div class="ai-feature-card">
                        <div class="ai-feature-header">
                            <div class="ai-feature-icon">
                                <i class="fas fa-gift"></i>
                            </div>
                            <h4 class="ai-feature-title">Đề xuất gói dịch vụ</h4>
                        </div>
                        <div class="ai-feature-content">
                            ${planData.message}
                        </div>
                        ${planData.actionUrl ? `
                            <div class="ai-feature-action">
                                <a href="${planData.actionUrl}" class="ai-feature-btn">
                                    ${planData.recommendationType === 'upgrade' ? 'Nâng cấp ngay' : 'Xem chi tiết'}
                                </a>
                            </div>
                        ` : ''}
                    </div>
                `;
            }
        }

        // Spending Forecast
        if (forecast && forecast.ok) {
            const forecastData = await forecast.json();
            userAiData.spendingForecast = forecastData;
            
            html += `
                <div class="ai-feature-card">
                    <div class="ai-feature-header">
                        <div class="ai-feature-icon">
                            <i class="fas fa-chart-line"></i>
                        </div>
                        <h4 class="ai-feature-title">Dự báo chi tiêu</h4>
                    </div>
                    <div class="ai-feature-content">
                        ${forecastData.message}
                    </div>
                    ${forecastData.canSetLimit ? `
                        <div class="ai-feature-action">
                            <button class="ai-feature-btn" onclick="setSpendingLimit()">
                                Đặt giới hạn chi tiêu
                            </button>
                        </div>
                    ` : ''}
                </div>
            `;
        }

        // Quick Actions
        html += `
            <div class="ai-feature-card">
                <div class="ai-feature-header">
                    <div class="ai-feature-icon">
                        <i class="fas fa-question-circle"></i>
                    </div>
                    <h4 class="ai-feature-title">Hỏi AI về giao dịch</h4>
                </div>
                <div class="ai-feature-content">
                    Bạn có thể hỏi: "Tại sao tôi bị tính 25$ tháng này?", "Tìm hóa đơn tháng 5 năm ngoái", hoặc "Chi tiêu tháng sau sẽ như thế nào?"
                </div>
                <div class="ai-feature-action">
                    <button class="ai-feature-btn" onclick="openUserAiChat()">
                        <i class="fas fa-comments me-1"></i>Mở chat với AI
                    </button>
                </div>
            </div>
        `;

        if (html === '') {
            html = '<div class="ai-feature-content">Chưa có đề xuất nào. Hãy sử dụng ứng dụng để nhận được tư vấn từ AI!</div>';
        }

        content.innerHTML = html;
    } catch (error) {
        console.error('Error loading user AI:', error);
        content.innerHTML = '<div class="ai-feature-content">Đã xảy ra lỗi khi tải thông tin AI. Vui lòng thử lại sau.</div>';
    }
}

async function refreshUserAI() {
    await loadUserAI();
}

function setSpendingLimit() {
    // Redirect to settings or open modal
    window.location.href = '/Settings';
}

function openUserAiChat() {
    // Open chat interface (can be implemented as modal or separate page)
    alert('Tính năng chat AI sẽ được triển khai trong phiên bản tiếp theo. Bạn có thể sử dụng API /api/user-ai/answer-question để tích hợp.');
}

// Answer transaction question
async function askUserAiQuestion(question) {
    try {
        const response = await fetch('/api/user-ai/answer-question', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify(question)
        });

        if (response.ok) {
            const data = await response.json();
            return data.answer;
        }
        return 'Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.';
    } catch (error) {
        console.error('Error asking AI question:', error);
        return 'Đã xảy ra lỗi khi xử lý câu hỏi.';
    }
}

// Search transactions
async function searchUserTransactions(query) {
    try {
        const response = await fetch('/api/user-ai/search-transactions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify({ query })
        });

        if (response.ok) {
            return await response.json();
        }
        return null;
    } catch (error) {
        console.error('Error searching transactions:', error);
        return null;
    }
}

