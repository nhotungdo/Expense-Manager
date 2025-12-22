/**
 * Modern Contacts & Chat Application
 */

const Contacts = {
    currentUserId: 0,
    connection: null,
    currentChatUserId: null,
    searchTimeout: null,

    init(config) {
        this.currentUserId = config.currentUserId;
        this.setupNavigation();
        this.setupSearch();
        this.setupChat();
        this.initSignalR();
    },

    setupNavigation() {
        const navItems = document.querySelectorAll('.nav-item');
        navItems.forEach(item => {
            item.addEventListener('click', () => {
                const tab = item.dataset.tab;
                this.switchTab(tab);
            });
        });
    },

    switchTab(tabName) {
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
        });
        document.querySelector(`[data-tab="${tabName}"]`)?.classList.add('active');

        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.remove('active');
        });
        document.getElementById(`tab-${tabName}`)?.classList.add('active');

        if (tabName === 'chat') {
            this.loadConversations();
        }
    },

    setupSearch() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                clearTimeout(this.searchTimeout);
                const query = e.target.value.trim();
                
                if (query.length < 2) {
                    this.showSearchPlaceholder();
                    return;
                }

                this.searchTimeout = setTimeout(() => {
                    this.performSearch(query);
                }, 500);
            });
        }
    },

    async performSearch(query) {
        const resultsContainer = document.getElementById('searchResults');
        resultsContainer.innerHTML = '<div class="loading-spinner"><i class="fas fa-spinner fa-spin"></i></div>';

        try {
            const response = await fetch(`?handler=Search&query=${encodeURIComponent(query)}`);
            const users = await response.json();

            if (users.length === 0) {
                resultsContainer.innerHTML = `
                    <div class="empty-state">
                        <div class="empty-icon"><i class="fas fa-user-slash"></i></div>
                        <h3 class="empty-title">Không tìm thấy</h3>
                        <p class="empty-text">Không có người dùng nào phù hợp</p>
                    </div>
                `;
                return;
            }

            resultsContainer.innerHTML = users.map(user => `
                <div class="search-result-card">
                    <div class="search-result-info">
                        <img src="${user.profilePictureUrl || '/images/default-avatar.png'}" 
                             alt="${this.escapeHtml(user.fullName)}" 
                             class="search-result-avatar">
                        <div>
                            <h4 class="search-result-name">${this.escapeHtml(user.fullName)}</h4>
                            <p class="search-result-username">@${this.escapeHtml(user.userName || 'user')}</p>
                        </div>
                    </div>
                    <button class="btn-add-friend" onclick="Contacts.sendFriendRequest(${user.id}, this)">
                        <i class="fas fa-user-plus"></i>
                        Kết bạn
                    </button>
                </div>
            `).join('');
        } catch (error) {
            console.error('Search error:', error);
        }
    },

    showSearchPlaceholder() {
        const resultsContainer = document.getElementById('searchResults');
        resultsContainer.innerHTML = `
            <div class="search-placeholder">
                <i class="fas fa-search"></i>
                <p>Nhập để bắt đầu tìm kiếm</p>
            </div>
        `;
    },

    async sendFriendRequest(userId, button) {
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';

        try {
            const formData = new FormData();
            formData.append('receiverId', userId);

            const response = await fetch('?handler=SendRequest', {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getToken() },
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                button.style.background = 'linear-gradient(135deg, #10b981 0%, #059669 100%)';
                button.innerHTML = '<i class="fas fa-check"></i> Đã gửi';
            } else {
                button.disabled = false;
                button.innerHTML = '<i class="fas fa-user-plus"></i> Kết bạn';
            }
        } catch (error) {
            console.error('Send request error:', error);
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-user-plus"></i> Kết bạn';
        }
    },

    async acceptRequest(requestId) {
        try {
            const formData = new FormData();
            formData.append('friendshipId', requestId);

            const response = await fetch('?handler=AcceptRequest', {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getToken() },
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                document.getElementById(`request-${requestId}`)?.remove();
                setTimeout(() => window.location.reload(), 1000);
            }
        } catch (error) {
            console.error('Accept request error:', error);
        }
    },

    async rejectRequest(requestId) {
        if (!confirm('Bạn có chắc muốn từ chối lời mời này?')) return;

        try {
            const formData = new FormData();
            formData.append('friendshipId', requestId);

            const response = await fetch('?handler=RemoveFriend', {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getToken() },
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                document.getElementById(`request-${requestId}`)?.remove();
            }
        } catch (error) {
            console.error('Reject request error:', error);
        }
    },

    async unfriend(friendshipId) {
        if (!confirm('Bạn có chắc muốn hủy kết bạn?')) return;

        try {
            const formData = new FormData();
            formData.append('friendshipId', friendshipId);

            const response = await fetch('?handler=RemoveFriend', {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getToken() },
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                document.getElementById(`friend-${friendshipId}`)?.remove();
            }
        } catch (error) {
            console.error('Unfriend error:', error);
        }
    },

    setupChat() {
        const chatInput = document.getElementById('chatInput');
        if (chatInput) {
            chatInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.sendMessage();
                }
            });

            // Typing indicator
            let typingTimeout;
            chatInput.addEventListener('input', () => {
                if (this.currentChatUserId && this.connection) {
                    this.connection.invoke('TypingIndicator', this.currentChatUserId.toString(), true)
                        .catch(err => console.error('Typing indicator error:', err));

                    clearTimeout(typingTimeout);
                    typingTimeout = setTimeout(() => {
                        this.connection.invoke('TypingIndicator', this.currentChatUserId.toString(), false)
                            .catch(err => console.error('Typing indicator error:', err));
                    }, 1000);
                }
            });
        }

        // Setup file upload
        this.setupFileUpload();
    },

    setupFileUpload() {
        const fileInput = document.getElementById('fileInput');
        const attachBtn = document.querySelector('.btn-attach');

        if (attachBtn && fileInput) {
            attachBtn.addEventListener('click', () => {
                fileInput.click();
            });

            fileInput.addEventListener('change', async (e) => {
                const file = e.target.files[0];
                if (file && this.currentChatUserId) {
                    await this.uploadFile(file);
                }
                fileInput.value = ''; // Reset
            });
        }
    },

    async uploadFile(file) {
        if (!this.currentChatUserId) return;

        // Show upload progress
        this.showUploadProgress(file.name);

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('receiverId', this.currentChatUserId);
            formData.append('message', file.name);

            const response = await fetch('/api/chat/upload', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const message = await response.json();
                console.log('File uploaded successfully:', message);
                // Message will be received via SignalR, but we can also display it immediately
                this.hideUploadProgress();
                // Optionally append the message immediately
                this.appendMessage(message.content, true, message.timestamp, false, message);
            } else {
                const error = await response.json();
                console.error('Upload failed:', error);
                alert('Upload failed: ' + (error.error || 'Unknown error'));
                this.hideUploadProgress();
            }
        } catch (error) {
            console.error('Upload error:', error);
            alert('Failed to upload file: ' + error.message);
            this.hideUploadProgress();
        }
    },

    showUploadProgress(fileName) {
        const messagesContainer = document.getElementById('chatMessages');
        const progressHtml = `
            <div id="upload-progress" class="upload-progress">
                <div class="upload-icon">
                    <i class="fas fa-spinner fa-spin"></i>
                </div>
                <div class="upload-info">
                    <p class="upload-filename">${this.escapeHtml(fileName)}</p>
                    <p class="upload-status">Uploading...</p>
                </div>
            </div>
        `;
        messagesContainer.insertAdjacentHTML('beforeend', progressHtml);
        this.scrollToBottom();
    },

    hideUploadProgress() {
        const progress = document.getElementById('upload-progress');
        if (progress) {
            progress.remove();
        }
    },

    initSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub')
            .withAutomaticReconnect()
            .build();

        this.connection.start()
            .then(() => console.log('SignalR connected'))
            .catch(err => console.error('SignalR error:', err));

        this.connection.on('ReceiveMessage', (senderId, message, timestamp, messageId, messageData) => {
            if (this.currentChatUserId && (senderId == this.currentChatUserId || senderId == this.currentUserId)) {
                this.appendMessage(message, senderId == this.currentUserId, timestamp, false, messageData);
                this.scrollToBottom();
            }
            this.loadConversations();
        });

        this.connection.on('UserStatusChange', (userId, isOnline) => {
            this.updateUserStatus(userId, isOnline);
        });

        this.connection.on('UserTyping', (userId, isTyping) => {
            if (this.currentChatUserId == userId) {
                this.showTypingIndicator(isTyping);
            }
        });

        this.connection.on('Error', (message) => {
            console.error('SignalR Error:', message);
            alert('Lỗi: ' + message);
        });
    },

    async loadConversations() {
        const listContainer = document.getElementById('conversationList');
        
        try {
            const response = await fetch('/api/Chat/conversations');
            const conversations = await response.json();

            if (conversations.length === 0) {
                listContainer.innerHTML = '<div class="empty-state"><p class="empty-text">Chưa có tin nhắn</p></div>';
                return;
            }

            listContainer.innerHTML = conversations.map(conv => `
                <div class="conversation-item ${this.currentChatUserId == conv.userId ? 'active' : ''}"
                     onclick="Contacts.openChat(${conv.userId}, '${this.escapeHtml(conv.fullName)}', '${conv.avatar || '/images/default-avatar.png'}')">
                    <img src="${conv.avatar || '/images/default-avatar.png'}" 
                         alt="${this.escapeHtml(conv.fullName)}" 
                         class="conversation-avatar">
                    <div class="conversation-info">
                        <h4 class="conversation-name">${this.escapeHtml(conv.fullName)}</h4>
                        <p class="conversation-preview">${this.escapeHtml(conv.lastMessageContent || 'Bắt đầu trò chuyện')}</p>
                    </div>
                    ${conv.unreadCount > 0 ? `<span class="conversation-badge">${conv.unreadCount}</span>` : ''}
                </div>
            `).join('');
        } catch (error) {
            console.error('Load conversations error:', error);
        }
    },

    async openChat(userId, userName, userAvatar) {
        this.currentChatUserId = userId;

        document.getElementById('chatUserName').textContent = userName;
        document.getElementById('chatAvatar').src = userAvatar;

        document.getElementById('chatSidebar').classList.remove('active');
        document.getElementById('chatWindow').classList.add('active');

        await this.loadChatHistory(userId);
        await this.markChatAsRead(userId);
        this.loadConversations();
    },

    async loadChatHistory(userId) {
        const messagesContainer = document.getElementById('chatMessages');
        messagesContainer.innerHTML = '<div class="loading-spinner"><i class="fas fa-spinner fa-spin"></i></div>';

        try {
            const response = await fetch(`/api/Chat/history/${userId}`);
            const messages = await response.json();

            messagesContainer.innerHTML = '';
            messages.forEach(msg => {
                this.appendMessage(msg.content, msg.senderId == this.currentUserId, msg.timestamp, msg.isRead, msg);
            });

            this.scrollToBottom();
        } catch (error) {
            console.error('Load chat history error:', error);
        }
    },

    appendMessage(content, isSent, timestamp, isRead = false, messageData = null) {
        const messagesContainer = document.getElementById('chatMessages');
        const time = new Date(timestamp).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

        let attachmentHtml = '';
        if (messageData && messageData.attachmentUrl) {
            attachmentHtml = this.renderAttachment(messageData);
        }

        const messageHtml = `
            <div class="message ${isSent ? 'sent' : 'received'}">
                ${!isSent ? `<img src="${document.getElementById('chatAvatar').src}" class="message-avatar">` : ''}
                <div class="message-content">
                    ${attachmentHtml}
                    ${content ? `<div class="message-bubble">${this.escapeHtml(content)}</div>` : ''}
                    <div class="message-time">
                        ${time}
                        ${isSent ? `<i class="fas fa-check ${isRead ? 'text-blue-500' : ''}"></i>` : ''}
                    </div>
                </div>
            </div>
        `;

        messagesContainer.insertAdjacentHTML('beforeend', messageHtml);
    },

    renderAttachment(attachment) {
        const { attachmentUrl, attachmentType, attachmentName, thumbnailUrl, attachmentSize } = attachment;
        const sizeStr = this.formatFileSize(attachmentSize);

        switch (attachmentType) {
            case 'images':
                return `
                    <div class="message-attachment image-attachment">
                        <a href="${attachmentUrl}" target="_blank">
                            <img src="${thumbnailUrl || attachmentUrl}" alt="${attachmentName}" />
                        </a>
                    </div>
                `;
            
            case 'videos':
                return `
                    <div class="message-attachment video-attachment">
                        <video controls>
                            <source src="${attachmentUrl}" type="video/mp4">
                        </video>
                    </div>
                `;
            
            case 'audio':
                return `
                    <div class="message-attachment audio-attachment">
                        <audio controls>
                            <source src="${attachmentUrl}">
                        </audio>
                        <p class="attachment-name">${this.escapeHtml(attachmentName)}</p>
                    </div>
                `;
            
            default:
                return `
                    <div class="message-attachment file-attachment">
                        <a href="${attachmentUrl}" download="${attachmentName}">
                            <div class="file-icon">
                                <i class="fas fa-file"></i>
                            </div>
                            <div class="file-info">
                                <p class="file-name">${this.escapeHtml(attachmentName)}</p>
                                <p class="file-size">${sizeStr}</p>
                            </div>
                            <div class="file-download">
                                <i class="fas fa-download"></i>
                            </div>
                        </a>
                    </div>
                `;
        }
    },

    formatFileSize(bytes) {
        if (!bytes) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    },

    async sendMessage() {
        const input = document.getElementById('chatInput');
        const message = input.value.trim();

        if (!message || !this.currentChatUserId) return;

        input.value = '';

        try {
            await this.connection.invoke('SendMessage', this.currentChatUserId.toString(), message);
        } catch (error) {
            console.error('Send message error:', error);
        }
    },

    async markChatAsRead(userId) {
        try {
            await fetch(`/api/Chat/mark-read/${userId}`, {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getToken() }
            });
            await this.connection.invoke('ReadMessage', userId.toString());
        } catch (error) {
            console.error('Mark as read error:', error);
        }
    },

    startChat(userId, userName, userAvatar) {
        this.switchTab('chat');
        setTimeout(() => this.openChat(userId, userName, userAvatar), 100);
    },

    backToConversations() {
        document.getElementById('chatSidebar').classList.add('active');
        document.getElementById('chatWindow').classList.remove('active');
        this.currentChatUserId = null;
    },

    updateUserStatus(userId, isOnline) {
        const statusIndicator = document.getElementById(`status-${userId}`);
        if (statusIndicator) {
            if (isOnline) {
                statusIndicator.classList.add('online');
            } else {
                statusIndicator.classList.remove('online');
            }
        }
    },

    scrollToBottom() {
        const container = document.getElementById('chatMessages');
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    },

    showTypingIndicator(isTyping) {
        const messagesContainer = document.getElementById('chatMessages');
        const existingIndicator = document.getElementById('typing-indicator');

        if (isTyping && !existingIndicator) {
            const indicatorHtml = `
                <div id="typing-indicator" class="message received">
                    <img src="${document.getElementById('chatAvatar').src}" class="message-avatar">
                    <div class="message-content">
                        <div class="message-bubble">
                            <span class="typing-dots">
                                <span>.</span><span>.</span><span>.</span>
                            </span>
                        </div>
                    </div>
                </div>
            `;
            messagesContainer.insertAdjacentHTML('beforeend', indicatorHtml);
            this.scrollToBottom();
        } else if (!isTyping && existingIndicator) {
            existingIndicator.remove();
        }
    },

    getToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    },

    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

window.Contacts = Contacts;
