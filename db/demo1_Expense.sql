-- Tạo database
CREATE DATABASE ExpenseManager;
GO
USE ExpenseManager;
GO

-- Bảng users (login bằng Google, không cần mật khẩu)
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

-- Bảng categories (chi tiêu / thu nhập, có thể global hoặc riêng user)
CREATE TABLE categories (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(10) NOT NULL CHECK (type IN ('EXPENSE','INCOME')),
    description NVARCHAR(500),                      -- mô tả danh mục
    user_id BIGINT NULL,                            -- NULL = mặc định hệ thống
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Bảng expenses (chi tiêu)
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

-- Bảng incomes (thu nhập)
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

-- Bảng gợi ý từ AI
CREATE TABLE ai_suggestions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    suggestion NVARCHAR(MAX) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Bảng email notifications (optional)
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

-- Thêm dữ liệu mẫu
-- Thêm categories mặc định (global)
INSERT INTO categories (name, type, description, user_id) VALUES
('Ăn uống', 'EXPENSE', 'Chi phí ăn uống hàng ngày', NULL),
('Đi lại', 'EXPENSE', 'Chi phí vận chuyển, xăng xe', NULL),
('Giải trí', 'EXPENSE', 'Chi phí giải trí, du lịch', NULL),
('Mua sắm', 'EXPENSE', 'Chi phí mua sắm cá nhân', NULL),
('Y tế', 'EXPENSE', 'Chi phí khám chữa bệnh', NULL),
('Học tập', 'EXPENSE', 'Chi phí học tập, sách vở', NULL),
('Lương', 'INCOME', 'Thu nhập từ lương', NULL),
('Thưởng', 'INCOME', 'Thu nhập từ thưởng', NULL),
('Đầu tư', 'INCOME', 'Thu nhập từ đầu tư', NULL),
('Kinh doanh', 'INCOME', 'Thu nhập từ kinh doanh', NULL);

-- Thêm user mẫu
INSERT INTO users (google_id, username, email, full_name, picture_url, role, enabled, phone_number, language, default_currency, timezone, theme) VALUES
('google_123456789', 'admin', 'admin@example.com', 'Administrator', 'https://example.com/avatar1.jpg', 'ADMIN', 1, '0123456789', 'vi', 'VND', 'Asia/Ho_Chi_Minh', 'light'),
('google_987654321', 'user1', 'user1@example.com', 'Nguyễn Văn A', 'https://example.com/avatar2.jpg', 'USER', 1, '0987654321', 'vi', 'VND', 'Asia/Ho_Chi_Minh', 'dark'),
('google_111222333', 'user2', 'user2@example.com', 'Trần Thị B', 'https://example.com/avatar3.jpg', 'USER', 1, '0912345678', 'en', 'USD', 'America/New_York', 'light');

-- Thêm expenses mẫu
INSERT INTO expenses (user_id, category_id, amount, currency, note, expense_date) VALUES
(2, 1, 50000, 'VND', 'Ăn trưa tại quán cơm', '2024-01-15'),
(2, 2, 30000, 'VND', 'Xăng xe máy', '2024-01-15'),
(2, 3, 200000, 'VND', 'Xem phim rạp', '2024-01-14'),
(3, 1, 75000, 'VND', 'Ăn tối với bạn', '2024-01-15'),
(3, 4, 500000, 'VND', 'Mua quần áo', '2024-01-13');

-- Thêm incomes mẫu
INSERT INTO incomes (user_id, category_id, amount, currency, note, income_date) VALUES
(2, 7, 15000000, 'VND', 'Lương tháng 1', '2024-01-01'),
(2, 8, 2000000, 'VND', 'Thưởng cuối năm', '2024-01-10'),
(3, 7, 12000000, 'VND', 'Lương tháng 1', '2024-01-01'),
(3, 9, 5000000, 'VND', 'Lãi đầu tư', '2024-01-12');

-- Thêm AI suggestions mẫu
INSERT INTO ai_suggestions (user_id, suggestion, created_at) VALUES
(2, 'Bạn đã chi tiêu 80% thu nhập tháng này. Hãy cân nhắc tiết kiệm hơn.', '2024-01-15'),
(2, 'Chi phí ăn uống của bạn tăng 20% so với tháng trước. Có thể cân nhắc nấu ăn tại nhà.', '2024-01-14'),
(3, 'Thu nhập của bạn ổn định. Có thể cân nhắc đầu tư thêm.', '2024-01-13');

-- Thêm emails mẫu
INSERT INTO emails (user_id, subject, body, status, created_at) VALUES
(2, 'Chào mừng đến với MoneyTracker', 'Cảm ơn bạn đã đăng ký sử dụng MoneyTracker!', 'SENT', '2024-01-01'),
(3, 'Báo cáo chi tiêu tháng 1', 'Tổng chi tiêu tháng 1: 2,500,000 VND', 'PENDING', '2024-01-15');
