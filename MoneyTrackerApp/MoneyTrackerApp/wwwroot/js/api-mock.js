// Mock API Data for Development and Testing
// This file provides sample data structure for the dashboard

const MockDashboardData = {
    totalIncome: 45000000,
    totalExpense: 32000000,
    netIncome: 13000000,
    totalBalance: 78500000,
    
    // Transaction Types Distribution for Pie Chart
    transactionTypes: [
        {
            type: 'Income',
            amount: 45000000,
            count: 28,
            percentage: 58.4
        },
        {
            type: 'Expense',
            amount: 32000000,
            count: 156,
            percentage: 41.6
        },
        {
            type: 'Transfer',
            amount: 0,
            count: 12,
            percentage: 0
        }
    ],
    
    // Category Spending for Pie Chart
    categorySpending: [
        {
            categoryId: 1,
            categoryName: 'Ăn uống',
            categoryIcon: 'fas fa-utensils',
            categoryColor: '#ef4444',
            amount: 8500000,
            count: 45,
            percentage: 26.6
        },
        {
            categoryId: 2,
            categoryName: 'Di chuyển',
            categoryIcon: 'fas fa-car',
            categoryColor: '#f59e0b',
            amount: 6200000,
            count: 32,
            percentage: 19.4
        },
        {
            categoryId: 3,
            categoryName: 'Mua sắm',
            categoryIcon: 'fas fa-shopping-bag',
            categoryColor: '#10b981',
            amount: 5800000,
            count: 28,
            percentage: 18.1
        },
        {
            categoryId: 4,
            categoryName: 'Giải trí',
            categoryIcon: 'fas fa-film',
            categoryColor: '#3b82f6',
            amount: 4200000,
            count: 18,
            percentage: 13.1
        },
        {
            categoryId: 5,
            categoryName: 'Hóa đơn',
            categoryIcon: 'fas fa-file-invoice',
            categoryColor: '#8b5cf6',
            amount: 3800000,
            count: 12,
            percentage: 11.9
        },
        {
            categoryId: 6,
            categoryName: 'Sức khỏe',
            categoryIcon: 'fas fa-heartbeat',
            categoryColor: '#ec4899',
            amount: 2100000,
            count: 8,
            percentage: 6.6
        },
        {
            categoryId: 7,
            categoryName: 'Giáo dục',
            categoryIcon: 'fas fa-graduation-cap',
            categoryColor: '#14b8a6',
            amount: 1200000,
            count: 5,
            percentage: 3.8
        },
        {
            categoryId: 8,
            categoryName: 'Khác',
            categoryIcon: 'fas fa-ellipsis-h',
            categoryColor: '#94a3b8',
            amount: 200000,
            count: 8,
            percentage: 0.6
        }
    ],
    
    // Monthly Trends for Line Chart
    monthlyTrends: [
        { month: 'T1', income: 38000000, expense: 28000000 },
        { month: 'T2', income: 42000000, expense: 30000000 },
        { month: 'T3', income: 40000000, expense: 29000000 },
        { month: 'T4', income: 45000000, expense: 31000000 },
        { month: 'T5', income: 43000000, expense: 32000000 },
        { month: 'T6', income: 45000000, expense: 32000000 }
    ],
    
    // Recent Transactions
    recentTransactions: [
        {
            transactionId: 1,
            transactionType: 'Expense',
            categoryName: 'Ăn uống',
            categoryIcon: 'fas fa-utensils',
            categoryColor: '#ef4444',
            amount: 250000,
            note: 'Ăn trưa với đồng nghiệp',
            transactionDate: '2025-12-08T12:30:00'
        },
        {
            transactionId: 2,
            transactionType: 'Income',
            categoryName: 'Lương',
            categoryIcon: 'fas fa-money-bill-wave',
            categoryColor: '#10b981',
            amount: 15000000,
            note: 'Lương tháng 12',
            transactionDate: '2025-12-01T09:00:00'
        },
        {
            transactionId: 3,
            transactionType: 'Expense',
            categoryName: 'Di chuyển',
            categoryIcon: 'fas fa-car',
            categoryColor: '#f59e0b',
            amount: 180000,
            note: 'Xăng xe',
            transactionDate: '2025-12-07T18:00:00'
        },
        {
            transactionId: 4,
            transactionType: 'Expense',
            categoryName: 'Mua sắm',
            categoryIcon: 'fas fa-shopping-bag',
            categoryColor: '#10b981',
            amount: 850000,
            note: 'Quần áo',
            transactionDate: '2025-12-06T15:30:00'
        },
        {
            transactionId: 5,
            transactionType: 'Expense',
            categoryName: 'Giải trí',
            categoryIcon: 'fas fa-film',
            categoryColor: '#3b82f6',
            amount: 320000,
            note: 'Xem phim',
            transactionDate: '2025-12-05T20:00:00'
        }
    ],
    
    // AI Suggestions
    aiSuggestions: [
        {
            suggestionId: 1,
            title: 'Tiết kiệm chi tiêu ăn uống',
            suggestion: 'Chi tiêu cho ăn uống tháng này cao hơn 15% so với tháng trước. Hãy cân nhắc nấu ăn tại nhà nhiều hơn.',
            priority: 'medium',
            category: 'spending'
        },
        {
            suggestionId: 2,
            title: 'Đạt mục tiêu tiết kiệm',
            suggestion: 'Bạn đang tiết kiệm được 28% thu nhập. Tuyệt vời! Hãy duy trì thói quen này.',
            priority: 'low',
            category: 'savings'
        },
        {
            suggestionId: 3,
            title: 'Cơ hội đầu tư',
            suggestion: 'Với số dư hiện tại, bạn có thể xem xét đầu tư vào quỹ mở để tăng lợi nhuận.',
            priority: 'high',
            category: 'investment'
        }
    ],
    
    // Financial Alerts
    financialAlerts: [
        {
            alertId: 1,
            title: 'Ngân sách vượt mức',
            message: 'Chi tiêu cho "Giải trí" đã vượt 90% ngân sách tháng này.',
            severity: 'warning',
            date: '2025-12-08'
        },
        {
            alertId: 2,
            title: 'Hóa đơn sắp đến hạn',
            message: 'Hóa đơn điện nước sẽ đến hạn trong 3 ngày.',
            severity: 'info',
            date: '2025-12-08'
        }
    ],
    
    // Accounts
    accounts: [
        {
            accountId: 1,
            accountName: 'Ví tiền mặt',
            accountType: 'Cash',
            currentBalance: 5500000,
            currency: 'VND',
            icon: 'fas fa-wallet',
            color: '#10b981'
        },
        {
            accountId: 2,
            accountName: 'Techcombank',
            accountType: 'Bank',
            currentBalance: 45000000,
            currency: 'VND',
            icon: 'fas fa-university',
            color: '#3b82f6'
        },
        {
            accountId: 3,
            accountName: 'Momo',
            accountType: 'EWallet',
            currentBalance: 8000000,
            currency: 'VND',
            icon: 'fas fa-mobile-alt',
            color: '#ec4899'
        },
        {
            accountId: 4,
            accountName: 'Tiết kiệm',
            accountType: 'Savings',
            currentBalance: 20000000,
            currency: 'VND',
            icon: 'fas fa-piggy-bank',
            color: '#f59e0b'
        }
    ]
};

// Function to simulate API delay
function simulateApiDelay(ms = 500) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// Mock API Functions
const MockAPI = {
    // Get dashboard data
    getDashboardStats: async (period = 'month') => {
        await simulateApiDelay();
        return MockDashboardData;
    },
    
    // Get transaction types distribution
    getTransactionTypes: async () => {
        await simulateApiDelay(300);
        return MockDashboardData.transactionTypes;
    },
    
    // Get category spending
    getCategorySpending: async () => {
        await simulateApiDelay(300);
        return MockDashboardData.categorySpending;
    },
    
    // Get monthly trends
    getMonthlyTrends: async () => {
        await simulateApiDelay(300);
        return MockDashboardData.monthlyTrends;
    }
};

// Export for use in development
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { MockDashboardData, MockAPI };
}

// Make available globally for testing
window.MockDashboardData = MockDashboardData;
window.MockAPI = MockAPI;
