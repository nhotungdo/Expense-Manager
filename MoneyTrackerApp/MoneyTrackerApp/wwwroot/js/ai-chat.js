// AI Chat Window JavaScript

let chatHistory = [];
let isAiTyping = false;

// Initialize chat on page load
document.addEventListener('DOMContentLoaded', function() {
    loadChatHistory();
    loadDailyInsight();
});

// Toggle chat window
function toggleAiChat() {
    const chatWindow = document.getElementById('aiChatWindow');
    const fab = document.getElementById('aiChatFab');
    
    if (chatWindow.style.display === 'none' || !chatWindow.style.display) {
        chatWindow.style.display = 'flex';
        fab.style.display = 'none';
        
        // Hide badge when opening
        const badge = document.getElementById('aiChatBadge');
        if (badge) {
            badge.style.display = 'none';
        }
        
        // Focus input
        setTimeout(() => {
            document.getElementById('aiChatInput').focus();
        }, 100);
    } else {
        chatWindow.style.display = 'none';
        fab.style.display = 'flex';
    }
}

// Open chat from external trigger
function openAiChat() {
    const chatWindow = document.getElementById('aiChatWindow');
    const fab = document.getElementById('aiChatFab');
    
    chatWindow.style.display = 'flex';
    fab.style.display = 'none';
    
    setTimeout(() => {
        document.getElementById('aiChatInput').focus();
    }, 100);
}

// Send message
async function sendAiMessage() {
    const input = document.getElementById('aiChatInput');
    const message = input.value.trim();
    
    if (!message || isAiTyping) return;
    
    // Clear input
    input.value = '';
    
    // Add user message to chat
    addMessageToChat(message, 'user');
    
    // Show typing indicator
    showTypingIndicator();
    
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            hideTypingIndicator();
            addMessageToChat('Vui lòng đăng nhập để sử dụng tính năng này.', 'bot');
            return;
        }
        
        const response = await fetch('/api/AiAdvisor/chat', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify({ message: message })
        });
        
        hideTypingIndicator();
        
        if (response.ok) {
            const data = await response.json();
            addMessageToChat(data.message, 'bot');
        } else {
            addMessageToChat('Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau.', 'bot');
        }
    } catch (error) {
        console.error('Error sending message:', error);
        hideTypingIndicator();
        addMessageToChat('Đã xảy ra lỗi khi kết nối. Vui lòng thử lại.', 'bot');
    }
    
    // Save chat history
    saveChatHistory();
}

// Send suggested prompt
function sendSuggestedPrompt(prompt) {
    const input = document.getElementById('aiChatInput');
    input.value = prompt;
    sendAiMessage();
}

// Handle Enter key
function handleAiChatKeyPress(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendAiMessage();
    }
}

// Add message to chat
function addMessageToChat(message, sender) {
    const messagesContainer = document.getElementById('aiChatMessages');
    const time = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    
    const messageDiv = document.createElement('div');
    messageDiv.className = `ai-message ai-message-${sender}`;
    
    if (sender === 'bot') {
        messageDiv.innerHTML = `
            <div class="ai-message-avatar">
                <i class="fas fa-robot"></i>
            </div>
            <div class="ai-message-content">
                <div class="ai-message-bubble">
                    ${formatMessage(message)}
                </div>
                <span class="ai-message-time">${time}</span>
            </div>
        `;
    } else {
        messageDiv.innerHTML = `
            <div class="ai-message-avatar">
                <i class="fas fa-user"></i>
            </div>
            <div class="ai-message-content">
                <div class="ai-message-bubble">
                    <p>${escapeHtml(message)}</p>
                </div>
                <span class="ai-message-time">${time}</span>
            </div>
        `;
    }
    
    messagesContainer.appendChild(messageDiv);
    
    // Scroll to bottom
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
    
    // Add to history
    chatHistory.push({ message, sender, time });
}

// Format bot message (support markdown-like formatting)
function formatMessage(message) {
    // Convert line breaks
    message = message.replace(/\n/g, '<br>');
    
    // Convert bold **text**
    message = message.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    
    // Convert lists
    const lines = message.split('<br>');
    let inList = false;
    let formatted = [];
    
    for (let line of lines) {
        if (line.trim().match(/^[\d]+\./)) {
            if (!inList) {
                formatted.push('<ol>');
                inList = 'ol';
            }
            formatted.push('<li>' + line.replace(/^[\d]+\./, '').trim() + '</li>');
        } else if (line.trim().match(/^[-•]/)) {
            if (!inList) {
                formatted.push('<ul>');
                inList = 'ul';
            }
            formatted.push('<li>' + line.replace(/^[-•]/, '').trim() + '</li>');
        } else {
            if (inList) {
                formatted.push(`</${inList}>`);
                inList = false;
            }
            if (line.trim()) {
                formatted.push('<p>' + line + '</p>');
            }
        }
    }
    
    if (inList) {
        formatted.push(`</${inList}>`);
    }
    
    return formatted.join('');
}

// Escape HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Show typing indicator
function showTypingIndicator() {
    isAiTyping = true;
    const messagesContainer = document.getElementById('aiChatMessages');
    
    const typingDiv = document.createElement('div');
    typingDiv.className = 'ai-message ai-message-bot';
    typingDiv.id = 'typingIndicator';
    typingDiv.innerHTML = `
        <div class="ai-message-avatar">
            <i class="fas fa-robot"></i>
        </div>
        <div class="ai-message-content">
            <div class="ai-message-bubble">
                <div class="ai-typing-indicator">
                    <div class="ai-typing-dot"></div>
                    <div class="ai-typing-dot"></div>
                    <div class="ai-typing-dot"></div>
                </div>
            </div>
        </div>
    `;
    
    messagesContainer.appendChild(typingDiv);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Hide typing indicator
function hideTypingIndicator() {
    isAiTyping = false;
    const typingIndicator = document.getElementById('typingIndicator');
    if (typingIndicator) {
        typingIndicator.remove();
    }
}

// Clear chat
function clearAiChat() {
    if (!confirm('Bạn có chắc muốn xóa toàn bộ lịch sử chat?')) {
        return;
    }
    
    chatHistory = [];
    saveChatHistory();
    
    const messagesContainer = document.getElementById('aiChatMessages');
    messagesContainer.innerHTML = `
        <div class="ai-message ai-message-bot">
            <div class="ai-message-avatar">
                <i class="fas fa-robot"></i>
            </div>
            <div class="ai-message-content">
                <div class="ai-message-bubble">
                    <p>Xin chào! 👋 Tôi là trợ lý tài chính AI của bạn. Tôi có thể giúp bạn:</p>
                    <ul>
                        <li>Phân tích chi tiêu và xu hướng</li>
                        <li>Dự báo dòng tiền</li>
                        <li>Đưa ra lời khuyên tiết kiệm</li>
                        <li>Trả lời câu hỏi về tài chính</li>
                    </ul>
                    <p>Hãy hỏi tôi bất cứ điều gì! 💡</p>
                </div>
                <span class="ai-message-time">Bây giờ</span>
            </div>
        </div>
    `;
}

// Save chat history to localStorage
function saveChatHistory() {
    try {
        localStorage.setItem('aiChatHistory', JSON.stringify(chatHistory));
    } catch (error) {
        console.error('Error saving chat history:', error);
    }
}

// Load chat history from localStorage
function loadChatHistory() {
    try {
        const saved = localStorage.getItem('aiChatHistory');
        if (saved) {
            chatHistory = JSON.parse(saved);
            
            // Restore messages (limit to last 20)
            const recentHistory = chatHistory.slice(-20);
            const messagesContainer = document.getElementById('aiChatMessages');
            
            // Clear default message if there's history
            if (recentHistory.length > 0) {
                messagesContainer.innerHTML = '';
                
                recentHistory.forEach(item => {
                    const messageDiv = document.createElement('div');
                    messageDiv.className = `ai-message ai-message-${item.sender}`;
                    
                    if (item.sender === 'bot') {
                        messageDiv.innerHTML = `
                            <div class="ai-message-avatar">
                                <i class="fas fa-robot"></i>
                            </div>
                            <div class="ai-message-content">
                                <div class="ai-message-bubble">
                                    ${formatMessage(item.message)}
                                </div>
                                <span class="ai-message-time">${item.time}</span>
                            </div>
                        `;
                    } else {
                        messageDiv.innerHTML = `
                            <div class="ai-message-avatar">
                                <i class="fas fa-user"></i>
                            </div>
                            <div class="ai-message-content">
                                <div class="ai-message-bubble">
                                    <p>${escapeHtml(item.message)}</p>
                                </div>
                                <span class="ai-message-time">${item.time}</span>
                            </div>
                        `;
                    }
                    
                    messagesContainer.appendChild(messageDiv);
                });
            }
        }
    } catch (error) {
        console.error('Error loading chat history:', error);
    }
}

// Load daily insight for widget
async function loadDailyInsight() {
    const widget = document.getElementById('aiInsightWidget');
    if (!widget) return;
    
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            return;
        }
        
        const response = await fetch('/api/AiAdvisor/daily-insight', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (response.ok) {
            const data = await response.json();
            displayInsight(data);
        }
    } catch (error) {
        console.error('Error loading daily insight:', error);
    }
}

// Display insight in widget
function displayInsight(data) {
    const widget = document.getElementById('aiInsightWidget');
    if (!widget) return;
    
    const loading = widget.querySelector('.ai-insight-loading');
    const message = widget.querySelector('.ai-insight-message');
    const text = widget.querySelector('.insight-text');
    
    if (loading) loading.style.display = 'none';
    if (message) message.style.display = 'block';
    if (text) text.textContent = data.insight;
    
    // Update widget color based on type
    const typeColors = {
        success: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
        warning: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)',
        danger: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)',
        info: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    };
    
    widget.style.background = typeColors[data.type] || typeColors.info;
}

// Refresh insight
async function refreshAiInsight() {
    const widget = document.getElementById('aiInsightWidget');
    if (!widget) return;
    
    const loading = widget.querySelector('.ai-insight-loading');
    const message = widget.querySelector('.ai-insight-message');
    
    if (loading) loading.style.display = 'flex';
    if (message) message.style.display = 'none';
    
    await loadDailyInsight();
}

// Dismiss insight
function dismissInsight() {
    const widget = document.getElementById('aiInsightWidget');
    if (widget) {
        widget.style.display = 'none';
    }
}

// Load cashflow forecast
async function loadCashflowForecast() {
    try {
        const token = localStorage.getItem('accessToken');
        
        if (!token) {
            return null;
        }
        
        const response = await fetch('/api/AiAdvisor/cashflow-forecast', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error loading cashflow forecast:', error);
    }
    
    return null;
}
