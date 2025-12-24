// Group Details Modals - Vue 3 Components
// This file contains all modal implementations for the Group Details page

// Add/Edit Expense Modal Component
const ExpenseModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>{{ expense.id ? 'Chỉnh sửa chi tiêu' : 'Thêm chi tiêu mới' }}</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="form-group">
                        <label>Mô tả <span class="required">*</span></label>
                        <input 
                            type="text" 
                            v-model="expense.description" 
                            placeholder="Ví dụ: Ăn trưa, Xăng xe..."
                            class="form-control"
                        />
                    </div>
                    
                    <div class="form-row">
                        <div class="form-group">
                            <label>Số tiền <span class="required">*</span></label>
                            <input 
                                type="number" 
                                v-model="expense.amount" 
                                placeholder="0"
                                class="form-control"
                                min="0"
                            />
                        </div>
                        
                        <div class="form-group">
                            <label>Danh mục</label>
                            <select v-model="expense.categoryId" class="form-control">
                                <option :value="null">Chọn danh mục</option>
                                <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                                    {{ cat.name }}
                                </option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label>Người trả <span class="required">*</span></label>
                        <select v-model="expense.paidByUserId" class="form-control">
                            <option v-for="member in members" :key="member.userId" :value="member.userId">
                                {{ member.userName }}
                            </option>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label>Ngày giao dịch</label>
                        <input 
                            type="date" 
                            v-model="expense.transactionDate" 
                            class="form-control"
                        />
                    </div>
                    
                    <div class="form-group">
                        <label>Chia tiền</label>
                        <div class="split-options">
                            <button 
                                class="split-btn" 
                                :class="{ active: splitType === 'equal' }"
                                @click="splitType = 'equal'; calculateEqualSplit()"
                            >
                                <i class="fas fa-equals"></i> Chia đều
                            </button>
                            <button 
                                class="split-btn" 
                                :class="{ active: splitType === 'custom' }"
                                @click="splitType = 'custom'"
                            >
                                <i class="fas fa-sliders-h"></i> Tùy chỉnh
                            </button>
                        </div>
                    </div>
                    
                    <div v-if="splitType === 'custom'" class="split-details">
                        <div v-for="member in members" :key="member.userId" class="split-item">
                            <div class="split-member">
                                <input 
                                    type="checkbox" 
                                    :id="'split-' + member.userId"
                                    v-model="selectedMembers"
                                    :value="member.userId"
                                    @change="updateSplits"
                                />
                                <label :for="'split-' + member.userId">{{ member.userName }}</label>
                            </div>
                            <input 
                                v-if="selectedMembers.includes(member.userId)"
                                type="number" 
                                v-model="splits[member.userId]"
                                class="split-amount"
                                min="0"
                                placeholder="0"
                            />
                        </div>
                        <div class="split-total">
                            <strong>Tổng:</strong>
                            <span :class="{ 'text-danger': totalSplit !== expense.amount }">
                                {{ formatCurrency(totalSplit) }}
                            </span>
                        </div>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Hủy</button>
                    <button class="btn btn-primary" @click="save" :disabled="!isValid">
                        {{ expense.id ? 'Cập nhật' : 'Thêm' }}
                    </button>
                </div>
            </div>
        </div>
    `,
    props: ['expense', 'members', 'categories', 'groupId'],
    emits: ['close', 'save'],
    data() {
        return {
            splitType: 'equal',
            selectedMembers: [],
            splits: {}
        };
    },
    computed: {
        totalSplit() {
            return Object.values(this.splits).reduce((sum, val) => sum + (parseFloat(val) || 0), 0);
        },
        isValid() {
            return this.expense.description && 
                   this.expense.amount > 0 && 
                   this.expense.paidByUserId &&
                   (this.splitType === 'equal' || this.totalSplit === this.expense.amount);
        }
    },
    methods: {
        formatCurrency(amount) {
            return new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND'
            }).format(amount || 0);
        },
        calculateEqualSplit() {
            const amount = parseFloat(this.expense.amount) || 0;
            const count = this.members.length;
            const splitAmount = amount / count;
            
            this.selectedMembers = this.members.map(m => m.userId);
            this.splits = {};
            this.members.forEach(m => {
                this.splits[m.userId] = splitAmount;
            });
        },
        updateSplits() {
            if (this.selectedMembers.length === 0) return;
            
            const amount = parseFloat(this.expense.amount) || 0;
            const splitAmount = amount / this.selectedMembers.length;
            
            this.splits = {};
            this.selectedMembers.forEach(userId => {
                this.splits[userId] = splitAmount;
            });
        },
        save() {
            const splitDetails = this.selectedMembers.map(userId => ({
                userId: userId,
                amount: parseFloat(this.splits[userId]) || 0
            }));
            
            this.$emit('save', {
                ...this.expense,
                splits: splitDetails
            });
        }
    },
    mounted() {
        this.calculateEqualSplit();
    }
};

// Add Member Modal Component
const AddMemberModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container modal-sm">
                <div class="modal-header">
                    <h3>Thêm thành viên</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="form-group">
                        <label>Tìm kiếm bạn bè</label>
                        <input 
                            type="text" 
                            v-model="searchQuery" 
                            placeholder="Nhập tên hoặc email..."
                            class="form-control"
                        />
                    </div>
                    
                    <div class="friends-list">
                        <div 
                            v-for="friend in filteredFriends" 
                            :key="friend.id"
                            class="friend-item"
                            @click="selectFriend(friend)"
                        >
                            <div class="friend-avatar">
                                {{ friend.userName.charAt(0).toUpperCase() }}
                            </div>
                            <div class="friend-info">
                                <div class="friend-name">{{ friend.userName }}</div>
                                <div class="friend-email">{{ friend.email }}</div>
                            </div>
                            <i v-if="selectedFriend?.id === friend.id" class="fas fa-check text-success"></i>
                        </div>
                        
                        <div v-if="filteredFriends.length === 0" class="empty-state">
                            <i class="fas fa-user-friends"></i>
                            <p>Không tìm thấy bạn bè</p>
                        </div>
                    </div>
                    
                    <div v-if="selectedFriend" class="form-group">
                        <label>Vai trò</label>
                        <select v-model="role" class="form-control">
                            <option value="Member">Thành viên</option>
                            <option value="Admin">Quản trị viên</option>
                        </select>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Hủy</button>
                    <button class="btn btn-primary" @click="add" :disabled="!selectedFriend">
                        Thêm
                    </button>
                </div>
            </div>
        </div>
    `,
    props: ['friends', 'groupId'],
    emits: ['close', 'add'],
    data() {
        return {
            searchQuery: '',
            selectedFriend: null,
            role: 'Member'
        };
    },
    computed: {
        filteredFriends() {
            if (!this.searchQuery) return this.friends;
            
            const query = this.searchQuery.toLowerCase();
            return this.friends.filter(f => 
                f.userName.toLowerCase().includes(query) ||
                f.email.toLowerCase().includes(query)
            );
        }
    },
    methods: {
        selectFriend(friend) {
            this.selectedFriend = friend;
        },
        add() {
            this.$emit('add', {
                userId: this.selectedFriend.id,
                role: this.role
            });
        }
    }
};

// Edit Member Role Modal Component
const EditMemberModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container modal-sm">
                <div class="modal-header">
                    <h3>Chỉnh sửa quyền</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="member-info-card">
                        <div class="member-avatar-lg">
                            {{ member.userName.charAt(0).toUpperCase() }}
                        </div>
                        <h4>{{ member.userName }}</h4>
                        <p>{{ member.userEmail }}</p>
                    </div>
                    
                    <div class="form-group">
                        <label>Vai trò</label>
                        <select v-model="role" class="form-control">
                            <option value="Member">Thành viên</option>
                            <option value="Admin">Quản trị viên</option>
                        </select>
                        <small class="form-text">
                            Quản trị viên có thể thêm/xóa thành viên và chỉnh sửa cài đặt nhóm
                        </small>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Hủy</button>
                    <button class="btn btn-primary" @click="save">
                        Lưu thay đổi
                    </button>
                </div>
            </div>
        </div>
    `,
    props: ['member'],
    emits: ['close', 'save'],
    data() {
        return {
            role: this.member.role || 'Member'
        };
    },
    methods: {
        save() {
            this.$emit('save', {
                userId: this.member.userId,
                role: this.role
            });
        }
    }
};

// Add/Edit Category Modal Component
const CategoryModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container modal-sm">
                <div class="modal-header">
                    <h3>{{ category.id ? 'Chỉnh sửa danh mục' : 'Thêm danh mục' }}</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="form-group">
                        <label>Tên danh mục <span class="required">*</span></label>
                        <input 
                            type="text" 
                            v-model="category.name" 
                            placeholder="Ví dụ: Ăn uống, Di chuyển..."
                            class="form-control"
                        />
                    </div>
                    
                    <div class="form-group">
                        <label>Icon</label>
                        <div class="icon-picker">
                            <button 
                                v-for="icon in icons" 
                                :key="icon"
                                class="icon-btn"
                                :class="{ active: category.icon === icon }"
                                @click="category.icon = icon"
                            >
                                <i :class="icon"></i>
                            </button>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label>Màu sắc</label>
                        <div class="color-picker">
                            <button 
                                v-for="color in colors" 
                                :key="color"
                                class="color-btn"
                                :style="{ background: color }"
                                :class="{ active: category.color === color }"
                                @click="category.color = color"
                            ></button>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label>Ngân sách (tùy chọn)</label>
                        <input 
                            type="number" 
                            v-model="category.budgetLimit" 
                            placeholder="0"
                            class="form-control"
                            min="0"
                        />
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Hủy</button>
                    <button class="btn btn-primary" @click="save" :disabled="!category.name">
                        {{ category.id ? 'Cập nhật' : 'Thêm' }}
                    </button>
                </div>
            </div>
        </div>
    `,
    props: ['category'],
    emits: ['close', 'save'],
    data() {
        return {
            icons: [
                'fas fa-utensils',
                'fas fa-car',
                'fas fa-shopping-bag',
                'fas fa-film',
                'fas fa-home',
                'fas fa-plane',
                'fas fa-coffee',
                'fas fa-gamepad',
                'fas fa-book',
                'fas fa-heart',
                'fas fa-gift',
                'fas fa-tag'
            ],
            colors: [
                '#ef4444', '#f59e0b', '#10b981', '#3b82f6',
                '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
                '#06b6d4', '#84cc16', '#a855f7', '#94a3b8'
            ]
        };
    },
    methods: {
        save() {
            this.$emit('save', this.category);
        }
    }
};

// Settle Up Modal Component
const SettleUpModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>Thanh toán nợ</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="settlements-list">
                        <div v-for="settlement in settlements" :key="settlement.id" class="settlement-item">
                            <div class="settlement-info">
                                <div class="settlement-from">
                                    <div class="avatar">{{ settlement.fromUserName.charAt(0) }}</div>
                                    <span>{{ settlement.fromUserName }}</span>
                                </div>
                                <div class="settlement-arrow">
                                    <i class="fas fa-arrow-right"></i>
                                    <div class="settlement-amount">{{ formatCurrency(settlement.amount) }}</div>
                                </div>
                                <div class="settlement-to">
                                    <div class="avatar">{{ settlement.toUserName.charAt(0) }}</div>
                                    <span>{{ settlement.toUserName }}</span>
                                </div>
                            </div>
                            <button 
                                class="btn btn-sm btn-success"
                                @click="markAsPaid(settlement)"
                            >
                                <i class="fas fa-check"></i> Đã thanh toán
                            </button>
                        </div>
                        
                        <div v-if="settlements.length === 0" class="empty-state">
                            <i class="fas fa-check-circle text-success"></i>
                            <p>Tất cả đã thanh toán!</p>
                        </div>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Đóng</button>
                </div>
            </div>
        </div>
    `,
    props: ['settlements'],
    emits: ['close', 'settle'],
    methods: {
        formatCurrency(amount) {
            return new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND'
            }).format(amount);
        },
        markAsPaid(settlement) {
            this.$emit('settle', settlement);
        }
    }
};

// Group Settings Modal Component
const GroupSettingsModal = {
    template: `
        <div class="modal-overlay" @click.self="$emit('close')">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>Cài đặt nhóm</h3>
                    <button class="btn-close" @click="$emit('close')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="settings-tabs">
                        <button 
                            :class="{ active: activeTab === 'general' }"
                            @click="activeTab = 'general'"
                        >
                            Chung
                        </button>
                        <button 
                            :class="{ active: activeTab === 'budget' }"
                            @click="activeTab = 'budget'"
                        >
                            Ngân sách
                        </button>
                        <button 
                            :class="{ active: activeTab === 'notifications' }"
                            @click="activeTab = 'notifications'"
                        >
                            Thông báo
                        </button>
                    </div>
                    
                    <div v-if="activeTab === 'general'" class="settings-content">
                        <div class="form-group">
                            <label>Tên nhóm</label>
                            <input 
                                type="text" 
                                v-model="settings.name" 
                                class="form-control"
                            />
                        </div>
                        
                        <div class="form-group">
                            <label>Mô tả</label>
                            <textarea 
                                v-model="settings.description" 
                                class="form-control"
                                rows="3"
                            ></textarea>
                        </div>
                        
                        <div class="form-group">
                            <label>Icon</label>
                            <input 
                                type="text" 
                                v-model="settings.icon" 
                                class="form-control"
                                placeholder="fas fa-users"
                            />
                        </div>
                        
                        <div class="form-group">
                            <label>Màu sắc</label>
                            <input 
                                type="color" 
                                v-model="settings.color" 
                                class="form-control"
                            />
                        </div>
                    </div>
                    
                    <div v-if="activeTab === 'budget'" class="settings-content">
                        <div class="form-group">
                            <label>Ngân sách tháng</label>
                            <input 
                                type="number" 
                                v-model="settings.monthlyBudget" 
                                class="form-control"
                                min="0"
                            />
                        </div>
                        
                        <div class="form-group">
                            <label>Cảnh báo khi đạt (%)</label>
                            <input 
                                type="number" 
                                v-model="settings.budgetWarningThreshold" 
                                class="form-control"
                                min="0"
                                max="100"
                            />
                        </div>
                    </div>
                    
                    <div v-if="activeTab === 'notifications'" class="settings-content">
                        <div class="form-check">
                            <input 
                                type="checkbox" 
                                id="notifyNewExpense"
                                v-model="settings.notifyNewExpense"
                            />
                            <label for="notifyNewExpense">
                                Thông báo khi có chi tiêu mới
                            </label>
                        </div>
                        
                        <div class="form-check">
                            <input 
                                type="checkbox" 
                                id="notifyNewMember"
                                v-model="settings.notifyNewMember"
                            />
                            <label for="notifyNewMember">
                                Thông báo khi có thành viên mới
                            </label>
                        </div>
                        
                        <div class="form-check">
                            <input 
                                type="checkbox" 
                                id="notifyBudgetAlert"
                                v-model="settings.notifyBudgetAlert"
                            />
                            <label for="notifyBudgetAlert">
                                Thông báo cảnh báo ngân sách
                            </label>
                        </div>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button class="btn btn-secondary" @click="$emit('close')">Hủy</button>
                    <button class="btn btn-primary" @click="save">
                        Lưu thay đổi
                    </button>
                </div>
            </div>
        </div>
    `,
    props: ['group'],
    emits: ['close', 'save'],
    data() {
        return {
            activeTab: 'general',
            settings: {
                name: this.group.name || '',
                description: this.group.description || '',
                icon: this.group.icon || 'fas fa-users',
                color: this.group.color || '#6366f1',
                monthlyBudget: 10000000,
                budgetWarningThreshold: 80,
                notifyNewExpense: true,
                notifyNewMember: true,
                notifyBudgetAlert: true
            }
        };
    },
    methods: {
        save() {
            this.$emit('save', this.settings);
        }
    }
};

// Export components
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        ExpenseModal,
        AddMemberModal,
        EditMemberModal,
        CategoryModal,
        SettleUpModal,
        GroupSettingsModal
    };
}
