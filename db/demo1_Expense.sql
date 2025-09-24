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
    updated_at DATETIME DEFAULT GETDATE()
);

-- Bảng categories (chi tiêu / thu nhập, có thể global hoặc riêng user)
CREATE TABLE categories (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(10) NOT NULL CHECK (type IN ('EXPENSE','INCOME')),
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
