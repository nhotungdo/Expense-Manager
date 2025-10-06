-- ========================================
-- MoneyTracker Database - Complete Setup
-- ========================================
-- Tạo database hoàn chỉnh với cấu trúc tối ưu cho Entity Framework
-- Gộp từ: demo1_Expense.sql + FixDatabaseStructure.sql + UpdateColumnNamesForEF.sql

-- Tạo database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ExpenseManager')
BEGIN
    CREATE DATABASE ExpenseManager;
    PRINT 'Created ExpenseManager database';
END
GO

USE ExpenseManager;
GO

-- Drop existing tables if they exist (in correct order due to foreign keys)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'emails')
    DROP TABLE emails;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ai_suggestions')
    DROP TABLE ai_suggestions;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'expenses')
    DROP TABLE expenses;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'incomes')
    DROP TABLE incomes;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
    DROP TABLE categories;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
    DROP TABLE users;

PRINT 'Dropped existing tables (if any)';

-- ========================================
-- Bảng users (login bằng Google, không cần mật khẩu)
-- ========================================
CREATE TABLE users (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    google_id VARCHAR(255) NOT NULL UNIQUE,         -- ID Google duy nhất
    username VARCHAR(50) NOT NULL,                  -- username (tự sinh từ email hoặc displayName)
    email VARCHAR(100) NOT NULL UNIQUE,             -- email Google
    full_name VARCHAR(100),                         -- tên đầy đủ
    picture_url VARCHAR(500),                       -- avatar từ Google
    role VARCHAR(20) NOT NULL DEFAULT 'USER' CHECK (role IN ('USER','ADMIN')),
    enabled BIT NOT NULL DEFAULT 1,
    last_login DATETIME DEFAULT GETDATE(),          -- lần đăng nhập gần nhất
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    -- Các trường bổ sung cho profile
    phone_number VARCHAR(20),                       -- số điện thoại
    date_of_birth DATE,                             -- ngày sinh
    gender VARCHAR(10) CHECK (gender IN ('MALE','FEMALE','OTHER')), -- giới tính
    address NVARCHAR(500),                          -- địa chỉ
    language VARCHAR(10) DEFAULT 'vi',              -- ngôn ngữ
    default_currency VARCHAR(10) DEFAULT 'VND',     -- tiền tệ mặc định
    timezone VARCHAR(50) DEFAULT 'Asia/Ho_Chi_Minh', -- múi giờ
    theme VARCHAR(10) DEFAULT 'light' CHECK (theme IN ('light','dark')), -- chủ đề
    email_notifications BIT DEFAULT 1,              -- thông báo email
    push_notifications BIT DEFAULT 1,               -- thông báo push
    password VARCHAR(255)                           -- mật khẩu (optional)
);

PRINT 'Created users table';

-- ========================================
-- Bảng categories (chi tiêu / thu nhập, có thể global hoặc riêng user)
-- ========================================
CREATE TABLE categories (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(10) NOT NULL CHECK (type IN ('EXPENSE','INCOME')),
    description NVARCHAR(500),                      -- mô tả danh mục
    user_id BIGINT NULL,                            -- NULL = mặc định hệ thống
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

PRINT 'Created categories table';

-- ========================================
-- Bảng expenses (chi tiêu)
-- ========================================
CREATE TABLE expenses (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    category_id BIGINT NULL,
    amount DECIMAL(15,2) NOT NULL,
    currency VARCHAR(10) DEFAULT 'VND',             -- đơn vị tiền tệ
    note VARCHAR(255),
    expense_date DATE NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

PRINT 'Created expenses table';

-- ========================================
-- Bảng incomes (thu nhập)
-- ========================================
CREATE TABLE incomes (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    category_id BIGINT NULL,
    amount DECIMAL(15,2) NOT NULL,
    currency VARCHAR(10) DEFAULT 'VND',
    note VARCHAR(255),
    income_date DATE NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

PRINT 'Created incomes table';

-- ========================================
-- Bảng gợi ý từ AI
-- ========================================
CREATE TABLE ai_suggestions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    suggestion NVARCHAR(MAX) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

PRINT 'Created ai_suggestions table';

-- ========================================
-- Bảng email notifications (optional)
-- ========================================
CREATE TABLE emails (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    subject NVARCHAR(255) NOT NULL,         -- tiêu đề email
    body NVARCHAR(MAX) NOT NULL,            -- nội dung email
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING' 
        CHECK (status IN ('PENDING','SENT','FAILED')),
    sent_at DATETIME NULL,                  -- thời gian gửi thành công
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

PRINT 'Created emails table';

-- ========================================
-- Bảng audit_logs (ghi log hoạt động hệ thống)
-- ========================================
CREATE TABLE audit_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NULL,                           -- NULL = system event
    action NVARCHAR(100) NOT NULL,                 -- hành động: LOGIN, CREATE_EXPENSE, etc.
    details NVARCHAR(MAX),                         -- chi tiết hành động
    entity_type VARCHAR(50),                       -- loại entity: EXPENSE, INCOME, CATEGORY
    entity_id BIGINT,                              -- ID của entity bị tác động
    ip_address VARCHAR(45),                        -- IP address của user
    user_agent NVARCHAR(500),                      -- User agent string
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

PRINT 'Created audit_logs table';

-- ========================================
-- Bảng budgets (ngân sách theo danh mục)
-- ========================================
CREATE TABLE budgets (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    category_id BIGINT NULL,                       -- NULL = tổng ngân sách
    budget_amount DECIMAL(15,2) NOT NULL,
    spent_amount DECIMAL(15,2) DEFAULT 0,
    currency VARCHAR(10) DEFAULT 'VND',
    period_type VARCHAR(20) NOT NULL DEFAULT 'MONTHLY' 
        CHECK (period_type IN ('DAILY','WEEKLY','MONTHLY','YEARLY')),
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE NO ACTION
);

PRINT 'Created budgets table';

-- ========================================
-- Bảng notifications (thông báo cho user)
-- ========================================
CREATE TABLE notifications (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    message NVARCHAR(MAX) NOT NULL,
    type VARCHAR(50) NOT NULL DEFAULT 'INFO' 
        CHECK (type IN ('INFO','WARNING','ERROR','SUCCESS','BUDGET_ALERT','AI_SUGGESTION')),
    is_read BIT DEFAULT 0,
    is_important BIT DEFAULT 0,
    action_url VARCHAR(500),                       -- URL để user có thể thực hiện hành động
    expires_at DATETIME,                           -- thời gian hết hạn thông báo
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

PRINT 'Created notifications table';

-- ========================================
-- Tạo indexes để tối ưu hiệu suất
-- ========================================
CREATE INDEX IX_users_google_id ON users(google_id);
CREATE INDEX IX_users_email ON users(email);
CREATE INDEX IX_expenses_user_id ON expenses(user_id);
CREATE INDEX IX_expenses_category_id ON expenses(category_id);
CREATE INDEX IX_expenses_expense_date ON expenses(expense_date);
CREATE INDEX IX_incomes_user_id ON incomes(user_id);
CREATE INDEX IX_incomes_category_id ON incomes(category_id);
CREATE INDEX IX_incomes_income_date ON incomes(income_date);
CREATE INDEX IX_categories_user_id ON categories(user_id);
CREATE INDEX IX_categories_type ON categories(type);
CREATE INDEX IX_ai_suggestions_user_id ON ai_suggestions(user_id);
CREATE INDEX IX_emails_user_id ON emails(user_id);
CREATE INDEX IX_emails_status ON emails(status);
CREATE INDEX IX_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IX_audit_logs_created_at ON audit_logs(created_at);
CREATE INDEX IX_audit_logs_action ON audit_logs(action);
CREATE INDEX IX_budgets_user_id ON budgets(user_id);
CREATE INDEX IX_budgets_category_id ON budgets(category_id);
CREATE INDEX IX_budgets_period ON budgets(start_date, end_date);
CREATE INDEX IX_notifications_user_id ON notifications(user_id);
CREATE INDEX IX_notifications_is_read ON notifications(is_read);
CREATE INDEX IX_notifications_type ON notifications(type);
CREATE INDEX IX_notifications_created_at ON notifications(created_at);

PRINT 'Created performance indexes';

-- ========================================
-- Thêm dữ liệu mẫu
-- ========================================

-- Thêm categories mặc định (global) - Cải tiến với 18 danh mục
INSERT INTO categories (name, type, description, user_id) VALUES
-- Danh mục Chi tiêu (10 loại)
('Ăn uống', 'EXPENSE', 'Chi phí ăn uống, nhà hàng, cafe', NULL),
('Giao thông', 'EXPENSE', 'Xăng xe, taxi, xe bus, grab', NULL),
('Mua sắm', 'EXPENSE', 'Quần áo, giày dép, đồ dùng cá nhân', NULL),
('Giải trí', 'EXPENSE', 'Xem phim, du lịch, game, sách', NULL),
('Sức khỏe', 'EXPENSE', 'Khám bệnh, thuốc men, gym, spa', NULL),
('Hóa đơn', 'EXPENSE', 'Điện, nước, internet, điện thoại', NULL),
('Giáo dục', 'EXPENSE', 'Học phí, sách vở, khóa học', NULL),
('Nhà cửa', 'EXPENSE', 'Tiền thuê nhà, sửa chữa, đồ dùng gia đình', NULL),
('Bảo hiểm', 'EXPENSE', 'Bảo hiểm y tế, xe, nhà', NULL),
('Khác', 'EXPENSE', 'Các chi phí khác', NULL),
-- Danh mục Thu nhập (8 loại)
('Lương', 'INCOME', 'Lương chính từ công việc', NULL),
('Thưởng', 'INCOME', 'Thưởng hiệu suất, lễ tết', NULL),
('Freelance', 'INCOME', 'Thu nhập từ công việc tự do', NULL),
('Đầu tư', 'INCOME', 'Lợi nhuận từ đầu tư, cổ tức', NULL),
('Kinh doanh', 'INCOME', 'Thu nhập từ kinh doanh', NULL),
('Cho thuê', 'INCOME', 'Thu nhập từ cho thuê nhà, xe', NULL),
('Quà tặng', 'INCOME', 'Tiền quà, hỗ trợ từ gia đình', NULL),
('Khác', 'INCOME', 'Các nguồn thu nhập khác', NULL);

PRINT 'Inserted 18 default categories (10 expense + 8 income)';

-- Thêm user mẫu để test
INSERT INTO users (google_id, username, email, full_name, picture_url, role, enabled, phone_number, language, default_currency, timezone, theme) VALUES
('google_admin_123', 'admin', 'admin@moneytracker.com', 'Administrator', 'https://lh3.googleusercontent.com/a/default-admin', 'ADMIN', 1, '0123456789', 'vi', 'VND', 'Asia/Ho_Chi_Minh', 'light'),
('google_user1_456', 'nguyenvana', 'nguyenvana@gmail.com', 'Nguyễn Văn A', 'https://lh3.googleusercontent.com/a/default-user1', 'USER', 1, '0987654321', 'vi', 'VND', 'Asia/Ho_Chi_Minh', 'dark'),
('google_user2_789', 'tranthib', 'tranthib@gmail.com', 'Trần Thị B', 'https://lh3.googleusercontent.com/a/default-user2', 'USER', 1, '0912345678', 'vi', 'VND', 'Asia/Ho_Chi_Minh', 'light');

PRINT 'Inserted 3 sample users (1 admin + 2 users)';

-- Thêm expenses mẫu với danh mục mới
INSERT INTO expenses (user_id, category_id, amount, currency, note, expense_date) VALUES
-- User 2 (Nguyễn Văn A) - Tháng hiện tại
(2, 1, 85000, 'VND', 'Ăn trưa tại quán cơm gần văn phòng', CAST(GETDATE() AS DATE)),
(2, 2, 150000, 'VND', 'Xăng xe máy Honda Wave', CAST(DATEADD(day, -1, GETDATE()) AS DATE)),
(2, 4, 350000, 'VND', 'Xem phim Avengers với bạn gái', CAST(DATEADD(day, -2, GETDATE()) AS DATE)),
(2, 3, 1200000, 'VND', 'Mua áo khoác mùa đông Uniqlo', CAST(DATEADD(day, -3, GETDATE()) AS DATE)),
(2, 6, 800000, 'VND', 'Hóa đơn điện tháng này', CAST(DATEADD(day, -5, GETDATE()) AS DATE)),
(2, 5, 500000, 'VND', 'Khám răng định kỳ', CAST(DATEADD(day, -7, GETDATE()) AS DATE)),
-- User 3 (Trần Thị B) - Tháng hiện tại  
(3, 1, 120000, 'VND', 'Ăn tối buffet lẩu với gia đình', CAST(GETDATE() AS DATE)),
(3, 3, 2500000, 'VND', 'Mua túi xách Louis Vuitton', CAST(DATEADD(day, -1, GETDATE()) AS DATE)),
(3, 7, 3000000, 'VND', 'Học phí khóa tiếng Anh IELTS', CAST(DATEADD(day, -4, GETDATE()) AS DATE)),
(3, 8, 500000, 'VND', 'Mua đồ trang trí nhà mới', CAST(DATEADD(day, -6, GETDATE()) AS DATE));

PRINT 'Inserted 10 sample expenses';

-- Thêm incomes mẫu với danh mục mới
INSERT INTO incomes (user_id, category_id, amount, currency, note, income_date) VALUES
-- User 2 (Nguyễn Văn A)
(2, 11, 18000000, 'VND', 'Lương tháng hiện tại - Developer', CAST(DATEADD(day, -25, GETDATE()) AS DATE)),
(2, 12, 3000000, 'VND', 'Thưởng hoàn thành dự án', CAST(DATEADD(day, -10, GETDATE()) AS DATE)),
(2, 13, 2500000, 'VND', 'Freelance thiết kế website', CAST(DATEADD(day, -5, GETDATE()) AS DATE)),
-- User 3 (Trần Thị B)
(3, 11, 15000000, 'VND', 'Lương tháng hiện tại - Marketing', CAST(DATEADD(day, -25, GETDATE()) AS DATE)),
(3, 14, 8000000, 'VND', 'Lợi nhuận đầu tư chứng khoán', CAST(DATEADD(day, -15, GETDATE()) AS DATE)),
(3, 17, 1500000, 'VND', 'Tiền quà sinh nhật từ gia đình', CAST(DATEADD(day, -8, GETDATE()) AS DATE));

PRINT 'Inserted 6 sample incomes';

-- Thêm AI suggestions mẫu thông minh hơn
INSERT INTO ai_suggestions (user_id, suggestion, created_at) VALUES
(2, 'Tuyệt vời! Bạn đã tiết kiệm được 23.5% thu nhập tháng này. Hãy xem xét đầu tư số tiền này vào quỹ tương hỗ hoặc tiết kiệm ngân hàng.', CAST(DATEADD(hour, -2, GETDATE()) AS DATETIME)),
(2, 'Chi tiêu cho danh mục "Mua sắm" chiếm 28% tổng chi tiêu. Bạn có thể cân nhắc giảm bớt việc mua sắm không cần thiết để tăng tỷ lệ tiết kiệm.', CAST(DATEADD(hour, -6, GETDATE()) AS DATETIME)),
(2, 'Thu nhập từ Freelance của bạn đang tăng trưởng tốt. Hãy cân nhắc mở rộng hoạt động này để tăng thu nhập thụ động.', CAST(DATEADD(day, -1, GETDATE()) AS DATETIME)),
(3, 'Cảnh báo: Chi tiêu tháng này đã vượt quá 85% thu nhập. Hãy cân nhắc cắt giảm chi tiêu không cần thiết, đặc biệt là danh mục "Mua sắm".', CAST(DATEADD(hour, -1, GETDATE()) AS DATETIME)),
(3, 'Thu nhập từ đầu tư của bạn rất ấn tượng! Với 8 triệu lợi nhuận, bạn có thể cân nhắc đa dạng hóa danh mục đầu tư.', CAST(DATEADD(hour, -4, GETDATE()) AS DATETIME));

PRINT 'Inserted 5 smart AI suggestions';

-- Thêm budgets mẫu
INSERT INTO budgets (user_id, category_id, budget_amount, spent_amount, currency, period_type, start_date, end_date, is_active) VALUES
-- User 2 (Nguyễn Văn A) - Ngân sách tháng hiện tại
(2, 1, 2000000, 85000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Ăn uống: 2M, đã chi 85K
(2, 2, 500000, 150000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Giao thông: 500K, đã chi 150K
(2, 3, 3000000, 1200000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Mua sắm: 3M, đã chi 1.2M
(2, 4, 1000000, 350000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Giải trí: 1M, đã chi 350K
(2, 6, 1000000, 800000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Hóa đơn: 1M, đã chi 800K
(2, NULL, 15000000, 2805000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Tổng ngân sách: 15M, đã chi 2.8M
-- User 3 (Trần Thị B) - Ngân sách tháng hiện tại
(3, 1, 1500000, 120000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Ăn uống: 1.5M, đã chi 120K
(3, 3, 5000000, 2500000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Mua sắm: 5M, đã chi 2.5M
(3, 7, 5000000, 3000000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Giáo dục: 5M, đã chi 3M
(3, 8, 1000000, 500000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1), -- Nhà cửa: 1M, đã chi 500K
(3, NULL, 20000000, 6120000, 'VND', 'MONTHLY', CAST(GETDATE() AS DATE), EOMONTH(GETDATE()), 1); -- Tổng ngân sách: 20M, đã chi 6.1M

PRINT 'Inserted 11 sample budgets';

-- Thêm notifications mẫu
INSERT INTO notifications (user_id, title, message, type, is_read, is_important, action_url, expires_at) VALUES
(2, 'Chào mừng đến với MoneyTracker!', 'Cảm ơn bạn đã đăng ký. Hãy bắt đầu quản lý tài chính thông minh!', 'SUCCESS', 1, 0, '/dashboard', DATEADD(day, 7, GETDATE())),
(2, 'Cảnh báo ngân sách', 'Bạn đã chi tiêu 40% ngân sách mua sắm tháng này. Hãy cân nhắc điều chỉnh!', 'BUDGET_ALERT', 0, 1, '/budgets', DATEADD(day, 3, GETDATE())),
(2, 'Gợi ý AI', 'Dựa trên lịch sử chi tiêu, bạn nên tăng ngân sách ăn uống lên 2.5 triệu VND.', 'AI_SUGGESTION', 0, 0, '/ai-suggestions', DATEADD(day, 5, GETDATE())),
(3, 'Chào mừng đến với MoneyTracker!', 'Chúc mừng! Bạn đã sẵn sàng quản lý tài chính hiệu quả.', 'SUCCESS', 1, 0, '/dashboard', DATEADD(day, 7, GETDATE())),
(3, 'Cảnh báo ngân sách nghiêm trọng', 'Bạn đã chi tiêu 50% ngân sách mua sắm và 60% ngân sách giáo dục. Cần điều chỉnh ngay!', 'BUDGET_ALERT', 0, 1, '/budgets', DATEADD(day, 1, GETDATE())),
(3, 'Gợi ý AI', 'Thu nhập từ đầu tư của bạn rất tốt. Hãy cân nhắc tăng tỷ lệ đầu tư!', 'AI_SUGGESTION', 0, 0, '/ai-suggestions', DATEADD(day, 3, GETDATE()));

PRINT 'Inserted 6 sample notifications';

-- Thêm audit logs mẫu
INSERT INTO audit_logs (user_id, action, details, entity_type, entity_id, ip_address, user_agent, created_at) VALUES
(2, 'LOGIN', 'User logged in via Google OAuth', 'USER', 2, '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', CAST(DATEADD(hour, -2, GETDATE()) AS DATETIME)),
(2, 'CREATE_EXPENSE', 'Created new expense: Ăn trưa tại quán cơm gần văn phòng', 'EXPENSE', 1, '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', CAST(DATEADD(hour, -1, GETDATE()) AS DATETIME)),
(2, 'CREATE_INCOME', 'Created new income: Lương tháng hiện tại - Developer', 'INCOME', 1, '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', CAST(DATEADD(day, -25, GETDATE()) AS DATETIME)),
(3, 'LOGIN', 'User logged in via Google OAuth', 'USER', 3, '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', CAST(DATEADD(hour, -3, GETDATE()) AS DATETIME)),
(3, 'CREATE_EXPENSE', 'Created new expense: Mua túi xách Louis Vuitton', 'EXPENSE', 6, '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', CAST(DATEADD(day, -1, GETDATE()) AS DATETIME)),
(3, 'CREATE_INCOME', 'Created new income: Lợi nhuận đầu tư chứng khoán', 'INCOME', 4, '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', CAST(DATEADD(day, -15, GETDATE()) AS DATETIME)),
(NULL, 'SYSTEM_START', 'MoneyTracker application started successfully', 'SYSTEM', NULL, '127.0.0.1', 'MoneyTracker/1.0', CAST(DATEADD(hour, -4, GETDATE()) AS DATETIME)),
(NULL, 'DATABASE_BACKUP', 'Daily database backup completed successfully', 'SYSTEM', NULL, '127.0.0.1', 'MoneyTracker/1.0', CAST(DATEADD(day, -1, GETDATE()) AS DATETIME));

PRINT 'Inserted 8 sample audit logs';

-- Thêm emails mẫu
INSERT INTO emails (user_id, subject, body, status, sent_at, created_at) VALUES
(2, 'Chào mừng đến với MoneyTracker!', 'Xin chào Nguyễn Văn A,<br><br>Cảm ơn bạn đã đăng ký sử dụng MoneyTracker! Chúng tôi rất vui được đồng hành cùng bạn trong hành trình quản lý tài chính thông minh.<br><br>Hãy bắt đầu bằng việc thêm giao dịch đầu tiên của bạn!', 'SENT', CAST(DATEADD(day, -30, GETDATE()) AS DATETIME), CAST(DATEADD(day, -30, GETDATE()) AS DATETIME)),
(2, 'Báo cáo tài chính tháng này', 'Tổng thu nhập: 23,500,000 VND<br>Tổng chi tiêu: 18,085,000 VND<br>Tiết kiệm: 5,415,000 VND (23.5%)<br><br>Chúc mừng! Bạn đã đạt mục tiêu tiết kiệm 20%.', 'PENDING', NULL, CAST(GETDATE() AS DATETIME)),
(3, 'Chào mừng đến với MoneyTracker!', 'Xin chào Trần Thị B,<br><br>Cảm ơn bạn đã tham gia MoneyTracker! Chúng tôi sẽ giúp bạn quản lý tài chính hiệu quả hơn.', 'SENT', CAST(DATEADD(day, -25, GETDATE()) AS DATETIME), CAST(DATEADD(day, -25, GETDATE()) AS DATETIME)),
(3, 'Cảnh báo chi tiêu cao', 'Bạn đã chi tiêu 85% thu nhập tháng này. Hãy cân nhắc điều chỉnh ngân sách để đảm bảo tài chính ổn định.', 'PENDING', NULL, CAST(DATEADD(hour, -1, GETDATE()) AS DATETIME));


