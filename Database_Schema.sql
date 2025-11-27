-- Create Database
CREATE DATABASE ExpenseManager;
GO

USE ExpenseManager;
GO

-- =============================================
-- 1. USERS TABLE (Giữ nguyên)
-- =============================================
CREATE TABLE [Users] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [GoogleId] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL DEFAULT 0,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [PhoneNumberConfirmed] bit NOT NULL DEFAULT 0,
    [TwoFactorEnabled] bit NOT NULL DEFAULT 0,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL DEFAULT 1,
    [AccessFailedCount] int NOT NULL DEFAULT 0,
    [FirstName] nvarchar(128) NULL,
    [LastName] nvarchar(128) NULL,
    [FullName] nvarchar(256) NULL,
    [ProfilePictureUrl] nvarchar(512) NULL,
    [OnboardingCompleted] bit NOT NULL DEFAULT 0,
    [Role] nvarchar(50) NOT NULL DEFAULT 'User',
    [Enabled] bit NOT NULL DEFAULT 1,
    [LastLogin] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    [DateOfBirth] date NULL,
    [Gender] nvarchar(10) NULL,
    [Address] nvarchar(512) NULL,
    [Language] nvarchar(10) NOT NULL DEFAULT 'vi',
    [DefaultCurrency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Timezone] nvarchar(50) NOT NULL DEFAULT 'Asia/Ho_Chi_Minh',
    [Theme] nvarchar(20) NOT NULL DEFAULT 'light',
    [EmailNotifications] bit NOT NULL DEFAULT 1,
    [PushNotifications] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

-- Create unique indexes for Users
CREATE UNIQUE INDEX [IX_Users_GoogleId] ON [Users] ([GoogleId]);
CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

-- =============================================
-- 2. ACCOUNTS (WALLETS) TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [Accounts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AccountType] int NOT NULL, -- 1 = Tiền mặt, 2 = Ngân hàng, 3 = Ví điện tử, 4 = Thẻ tín dụng, 5 = Tiết kiệm
    [InitialBalance] decimal(18,2) NOT NULL DEFAULT 0,
    [CurrentBalance] decimal(18,2) NOT NULL DEFAULT 0,
    [Currency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Icon] nvarchar(50) NULL,
    [Color] nvarchar(20) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [IncludeInTotal] bit NOT NULL DEFAULT 1, -- Có tính vào tổng số dư không
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Accounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for Accounts
CREATE INDEX [IX_Accounts_UserId] ON [Accounts] ([UserId]);
GO

-- =============================================
-- 3. CATEGORIES TABLE (CHỈNH SỬA)
-- =============================================
CREATE TABLE [Categories] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ParentCategoryId] bigint NULL, -- Hỗ trợ danh mục cha-con
    [Name] nvarchar(100) NOT NULL,
    [Type] int NOT NULL, -- 1 = Income, 2 = Expense
    [Description] nvarchar(512) NULL,
    [Icon] nvarchar(50) NULL,
    [Color] nvarchar(20) NULL,
    [UserId] bigint NULL, -- NULL for default categories
    [IsDefault] bit NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Categories
CREATE INDEX [IX_Categories_UserId] ON [Categories] ([UserId]);
CREATE INDEX [IX_Categories_Type] ON [Categories] ([Type]);
CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);
GO

-- =============================================
-- 4. TRANSACTIONS TABLE (Unified) (CHỈNH SỬA)
-- =============================================
CREATE TABLE [Transactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL, -- Giao dịch thuộc tài khoản/ví nào
    [CategoryId] bigint NULL,
    [TransactionType] int NOT NULL, -- 1 = Income, 2 = Expense, 3 = Transfer
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Note] nvarchar(512) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [PairedAccountId] bigint NULL, -- Dùng cho Transfer, là tài khoản đích
    [PairedTransactionId] bigint NULL, -- Dùng cho Transfer, liên kết với giao dịch đối ứng
    [AttachmentUrl] nvarchar(512) NULL, -- Đính kèm hóa đơn
    [OcrText] nvarchar(max) NULL, -- Lưu kết quả OCR
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Transactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION, 
    CONSTRAINT [FK_Transactions_Accounts_PairedAccountId] FOREIGN KEY ([PairedAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transactions_Transactions_PairedTransactionId] FOREIGN KEY ([PairedTransactionId]) REFERENCES [Transactions] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Transactions
CREATE INDEX [IX_Transactions_UserId] ON [Transactions] ([UserId]);
CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);
CREATE INDEX [IX_Transactions_CategoryId] ON [Transactions] ([CategoryId]);
CREATE INDEX [IX_Transactions_TransactionType] ON [Transactions] ([TransactionType]);
CREATE INDEX [IX_Transactions_TransactionDate] ON [Transactions] ([TransactionDate]);
GO

-- =============================================
-- 5. BUDGETS TABLE
-- =============================================
CREATE TABLE [Budgets] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CategoryId] bigint NULL, -- Ngân sách cho danh mục
    [AccountId] bigint NULL, -- Ngân sách cho một tài khoản cụ thể
    [Amount] decimal(18,2) NOT NULL,
    [Period] int NOT NULL, -- 1 = Weekly, 2 = Monthly, 3 = Yearly, 4 = Custom
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Budgets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Budgets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Budgets_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Budgets
CREATE INDEX [IX_Budgets_UserId] ON [Budgets] ([UserId]);
CREATE INDEX [IX_Budgets_CategoryId] ON [Budgets] ([CategoryId]);
CREATE INDEX [IX_Budgets_AccountId] ON [Budgets] ([AccountId]);
GO

-- =============================================
-- 6. SAVINGS GOALS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [SavingsGoals] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [TargetAmount] decimal(18,2) NOT NULL,
    [CurrentAmount] decimal(18,2) NOT NULL DEFAULT 0,
    [TargetDate] date NULL,
    [Icon] nvarchar(50) NULL,
    [Color] nvarchar(20) NULL,
    [Status] int NOT NULL DEFAULT 1, -- 1 = Active, 2 = Completed, 3 = Cancelled
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SavingsGoals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SavingsGoals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SavingsGoals_UserId] ON [SavingsGoals] ([UserId]);
GO

-- =============================================
-- 7. SAVINGS TRANSACTIONS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [SavingsTransactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [SavingsGoalId] bigint NOT NULL,
    [TransactionId] bigint NOT NULL, -- Giao dịch (chi) tương ứng từ bảng Transactions
    [Amount] decimal(18,2) NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [Note] nvarchar(512) NULL,
    CONSTRAINT [PK_SavingsTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SavingsTransactions_SavingsGoals_SavingsGoalId] FOREIGN KEY ([SavingsGoalId]) REFERENCES [SavingsGoals] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SavingsTransactions_Transactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_SavingsTransactions_SavingsGoalId] ON [SavingsTransactions] ([SavingsGoalId]);
CREATE INDEX [IX_SavingsTransactions_TransactionId] ON [SavingsTransactions] ([TransactionId]);
GO

-- =============================================
-- 8. SCHEDULED TRANSACTIONS (RECURRING) TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [ScheduledTransactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [TransactionType] int NOT NULL, -- 1 = Income, 2 = Expense
    [Amount] decimal(18,2) NOT NULL,
    [Frequency] nvarchar(20) NOT NULL, -- 'daily', 'weekly', 'monthly', 'yearly'
    [Interval] int NOT NULL DEFAULT 1, -- VD: 2 + 'monthly' = 2 tháng/lần
    [StartDate] date NOT NULL,
    [EndDate] date NULL,
    [NextRunDate] date NOT NULL, -- Ngày chạy tiếp theo (quan trọng cho job)
    [Note] nvarchar(512) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ScheduledTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ScheduledTransactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ScheduledTransactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ScheduledTransactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_ScheduledTransactions_UserId] ON [ScheduledTransactions] ([UserId]);
CREATE INDEX [IX_ScheduledTransactions_NextRunDate] ON [ScheduledTransactions] ([NextRunDate]);
GO

-- =============================================
-- 9. DEBTS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [Debts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [DebtType] int NOT NULL, -- 1 = Nợ phải trả (I owe), 2 = Nợ phải thu (I am owed)
    [Name] nvarchar(100) NOT NULL, -- VD: "Vay mua xe", "Tùng mượn tiền"
    [PersonName] nvarchar(100) NULL, -- Người cho vay / Người vay
    [InitialAmount] decimal(18,2) NOT NULL,
    [AmountPaid] decimal(18,2) NOT NULL DEFAULT 0,
    [InterestRate] decimal(5,2) NOT NULL DEFAULT 0, -- Lãi suất %/năm
    [StartDate] date NOT NULL,
    [DueDate] date NULL,
    [Status] int NOT NULL DEFAULT 1, -- 1 = Active, 2 = Paid
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Debts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Debts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Debts_UserId] ON [Debts] ([UserId]);
GO

-- =============================================
-- 10. DEBT PAYMENTS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [DebtPayments] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [DebtId] bigint NOT NULL,
    [TransactionId] bigint NOT NULL, -- Giao dịch (chi/thu) tương ứng
    [Amount] decimal(18,2) NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Note] nvarchar(512) NULL,
    CONSTRAINT [PK_DebtPayments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DebtPayments_Debts_DebtId] FOREIGN KEY ([DebtId]) REFERENCES [Debts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DebtPayments_Transactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_DebtPayments_DebtId] ON [DebtPayments] ([DebtId]);
GO

-- =============================================
-- 11. SHARED ACCOUNTS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [SharedAccounts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [AccountId] bigint NOT NULL, -- Tài khoản được chia sẻ
    [UserId] bigint NOT NULL, -- Người được chia sẻ
    [Permission] int NOT NULL, -- 1 = ViewOnly, 2 = ViewAndAdd, 3 = FullAccess
    [SharedByUserId] bigint NOT NULL, -- Người chủ sở hữu chia sẻ
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_SharedAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SharedAccounts_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SharedAccounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SharedAccounts_Users_SharedByUserId] FOREIGN KEY ([SharedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UK_SharedAccounts_AccountId_UserId] UNIQUE ([AccountId], [UserId]) -- Đảm bảo không chia sẻ 2 lần
);
GO

CREATE INDEX [IX_SharedAccounts_AccountId] ON [SharedAccounts] ([AccountId]);
CREATE INDEX [IX_SharedAccounts_UserId] ON [SharedAccounts] ([UserId]);
GO

-- =============================================
-- 12. INVESTMENTS TABLE (*** ĐÃ SỬA LỖI 1785 ***)
-- =============================================
CREATE TABLE [Investments] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NULL, -- Tài khoản dùng để mua
    [Name] nvarchar(100) NOT NULL, -- VD: "Cổ phiếu FPT", "Bitcoin"
    [AssetType] nvarchar(50) NOT NULL, -- 'Stock', 'Crypto', 'Gold', 'Fund'
    [Quantity] decimal(18,8) NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [PurchaseDate] date NOT NULL,
    [CurrentValue] decimal(18,2) NULL,
    [LastUpdated] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Investments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Investments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Investments_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION -- Sửa từ SET NULL
);
GO

CREATE INDEX [IX_Investments_UserId] ON [Investments] ([UserId]);
GO

-- =============================================
-- 13. BANK CONNECTIONS TABLE (*** ĐÃ SỬA LỖI 1785 ***)
-- =============================================
CREATE TABLE [BankConnections] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL, -- Liên kết với tài khoản nội bộ
    [Provider] nvarchar(50) NOT NULL, -- VD: 'Plaid', 'VietQR'
    [AccessToken] nvarchar(max) NOT NULL, -- Nên được mã hóa
    [ItemId] nvarchar(256) NULL, -- ID định danh từ provider
    [LastSync] datetime2 NULL,
    [SyncStatus] nvarchar(20) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_BankConnections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BankConnections_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankConnections_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION -- Sửa từ CASCADE
);
GO

CREATE INDEX [IX_BankConnections_UserId] ON [BankConnections] ([UserId]);
GO

-- =============================================
-- 14. CURRENCY RATES TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [CurrencyRates] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FromCurrency] nvarchar(3) NOT NULL,
    [ToCurrency] nvarchar(3) NOT NULL,
    [Rate] decimal(18,9) NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_CurrencyRates] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_CurrencyRates_From_To] UNIQUE ([FromCurrency], [ToCurrency])
);
GO

-- =============================================
-- 15. NOTIFICATIONS TABLE
-- =============================================
CREATE TABLE [Notifications] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Message] nvarchar(1024) NOT NULL,
    [Type] nvarchar(50) NOT NULL, -- 'info', 'warning', 'error', 'success'
    [IsRead] bit NOT NULL DEFAULT 0,
    [IsImportant] bit NOT NULL DEFAULT 0,
    [ActionUrl] nvarchar(512) NULL,
    [ExpiresAt] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
GO

-- =============================================
-- 16. AI SUGGESTIONS TABLE
-- =============================================
CREATE TABLE [AiSuggestions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Suggestion] nvarchar(1024) NOT NULL,
    [SuggestionType] nvarchar(50) NOT NULL DEFAULT 'Financial Advice',
    [IsRead] bit NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_AiSuggestions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AiSuggestions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AiSuggestions_UserId] ON [AiSuggestions] ([UserId]);
GO

-- =============================================
-- 17. AUDIT LOGS TABLE
-- =============================================
CREATE TABLE [AuditLogs] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NULL,
    [Action] nvarchar(100) NOT NULL,
    [Details] nvarchar(1024) NULL,
    [EntityType] nvarchar(50) NULL,
    [EntityId] bigint NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(512) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);
GO

-- =============================================
-- 18. EMAILS TABLE
-- =============================================
CREATE TABLE [Emails] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Subject] nvarchar(256) NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [Status] nvarchar(20) NOT NULL, -- 'pending', 'sent', 'failed'
    [SentAt] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Emails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Emails_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Emails_UserId] ON [Emails] ([UserId]);
CREATE INDEX [IX_Emails_Status] ON [Emails] ([Status]);
GO

-- =============================================
-- 19. REPORTS TABLE
-- =============================================
CREATE TABLE [Reports] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [ReportType] nvarchar(20) NOT NULL, -- 'monthly', 'weekly', 'yearly', 'custom'
    [ReportName] nvarchar(256) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Parameters] nvarchar(max) NULL, -- JSON string (có thể chứa AccountId, CategoryId...)
    [FilePath] nvarchar(512) NULL,
    [FileFormat] nvarchar(10) NULL, -- 'pdf', 'excel', 'csv'
    [GeneratedAt] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);
GO

-- =============================================
-- 20. ASP.NET CORE IDENTITY TABLES
-- =============================================
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] bigint NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] bigint NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] bigint NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

-- =============================================
-- 21. SYSTEM SETTINGS TABLE
-- =============================================
CREATE TABLE [SystemSettings] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [SettingKey] nvarchar(100) NOT NULL,
    [SettingValue] nvarchar(max) NOT NULL,
    [Description] nvarchar(512) NULL,
    [SettingType] nvarchar(20) NOT NULL, -- 'string', 'number', 'boolean', 'json'
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);
GO

-- =============================================
-- 22. ONBOARDING STATUS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [OnboardingStatus] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CurrentStep] int NOT NULL DEFAULT 1,
    [ProfileJson] nvarchar(max) NULL,
    [IncomeJson] nvarchar(max) NULL,
    [ExpensesJson] nvarchar(max) NULL,
    [GoalsJson] nvarchar(max) NULL,
    [IsCompleted] bit NOT NULL DEFAULT 0,
    [StartedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_OnboardingStatus] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OnboardingStatus_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UK_OnboardingStatus_UserId] UNIQUE ([UserId])
);
GO

CREATE INDEX [IX_OnboardingStatus_CurrentStep] ON [OnboardingStatus] ([CurrentStep]);
CREATE INDEX [IX_OnboardingStatus_IsCompleted] ON [OnboardingStatus] ([IsCompleted]);
GO

-- =============================================
-- 23. FINANCIAL ALERTS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [FinancialAlerts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [IsRead] bit NOT NULL DEFAULT 0,
    [ReadAt] datetime2 NULL,
    [Data] nvarchar(max) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_FinancialAlerts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FinancialAlerts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_FinancialAlerts_UserId] ON [FinancialAlerts] ([UserId]);
CREATE INDEX [IX_FinancialAlerts_IsRead] ON [FinancialAlerts] ([IsRead]);
CREATE INDEX [IX_FinancialAlerts_CreatedAt] ON [FinancialAlerts] ([CreatedAt]);
GO

-- =============================================
-- 24. GROUP EXPENSES TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [GroupExpenses] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedByUserId] bigint NOT NULL,
    [IsPublic] bit NOT NULL DEFAULT 1,
    [Icon] nvarchar(50) NULL,
    [Color] nvarchar(20) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GroupExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupExpenses_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_GroupExpenses_CreatedByUserId] ON [GroupExpenses] ([CreatedByUserId]);
GO

-- =============================================
-- 25. GROUP MEMBERS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [GroupMembers] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [GroupId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [Role] nvarchar(20) NOT NULL DEFAULT 'Member',
    [JoinedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_GroupMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupMembers_GroupExpenses_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [GroupExpenses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_GroupMembers_GroupId] ON [GroupMembers] ([GroupId]);
CREATE INDEX [IX_GroupMembers_UserId] ON [GroupMembers] ([UserId]);
GO

-- =============================================
-- 26. GROUP TRANSACTIONS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [GroupTransactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [GroupId] bigint NOT NULL,
    [PaidByUserId] bigint NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Description] nvarchar(500) NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [Category] nvarchar(100) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GroupTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupTransactions_GroupExpenses_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [GroupExpenses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupTransactions_Users_PaidByUserId] FOREIGN KEY ([PaidByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_GroupTransactions_GroupId] ON [GroupTransactions] ([GroupId]);
CREATE INDEX [IX_GroupTransactions_PaidByUserId] ON [GroupTransactions] ([PaidByUserId]);
GO

-- =============================================
-- 27. GROUP TRANSACTION SPLITS TABLE (BẢNG MỚI)
-- =============================================
CREATE TABLE [GroupTransactionSplits] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [GroupTransactionId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [IsPaid] bit NOT NULL DEFAULT 0,
    [PaidAt] datetime2 NULL,
    CONSTRAINT [PK_GroupTransactionSplits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupTransactionSplits_GroupTransactions_GroupTransactionId] FOREIGN KEY ([GroupTransactionId]) REFERENCES [GroupTransactions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupTransactionSplits_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_GroupTransactionSplits_GroupTransactionId] ON [GroupTransactionSplits] ([GroupTransactionId]);
CREATE INDEX [IX_GroupTransactionSplits_UserId] ON [GroupTransactionSplits] ([UserId]);
GO

-- =============================================
-- INSERT DEFAULT DATA
-- =============================================
-- Insert default roles
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES 
('1', 'Admin', 'ADMIN', NEWID()),
('2', 'User', 'USER', NEWID());
GO

-- Insert default admin user
INSERT INTO [Users] ([GoogleId], [UserName], [Email], [FullName], [Role], [Enabled], [CreatedAt])
VALUES ('admin', 'admin', 'nhotungdo89@gmail.com', 'System Administrator', 'Admin', 1, GETUTCDATE());
GO

-- Assign admin role to admin user
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id]
FROM [Users] u, [AspNetRoles] r
WHERE u.[GoogleId] = 'admin' AND r.[Name] = 'Admin';
GO




-- =============================================
-- CREATE STORED PROCEDURES (CẬP NHẬT)
-- =============================================
-- Procedure to get user dashboard statistics
CREATE PROCEDURE [dbo].[GetUserDashboardStats]
    @UserId bigint,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Tính toán tổng thu, chi (Không bao gồm Transfer)
    SELECT 
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN Amount ELSE 0 END), 0) AS TotalIncome,
        ISNULL(SUM(CASE WHEN [TransactionType] = 2 THEN Amount ELSE 0 END), 0) AS TotalExpense,
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN Amount ELSE -Amount END), 0) AS NetIncome,
        COUNT(CASE WHEN [TransactionType] IN (1, 2) THEN 1 ELSE NULL END) AS TransactionCount
    FROM [Transactions]
    WHERE [UserId] = @UserId 
        AND [TransactionType] IN (1, 2) -- Chỉ tính Thu/Chi
        AND [TransactionDate] BETWEEN @StartDate AND @EndDate;

    -- Tính toán tổng số dư từ tất cả các ví
    SELECT 
        ISNULL(SUM([CurrentBalance]), 0) AS TotalBalance
    FROM [Accounts]
    WHERE [UserId] = @UserId AND [IncludeInTotal] = 1 AND [IsActive] = 1;
END
GO

-- Procedure to get category spending summary
CREATE PROCEDURE [dbo].[GetCategorySpendingSummary]
    @UserId bigint,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.[Name] AS CategoryName,
        c.[Icon] AS CategoryIcon,
        c.[Color] AS CategoryColor,
        ISNULL(SUM(t.[Amount]), 0) AS TotalAmount,
        COUNT(t.[Id]) AS TransactionCount
    FROM [Categories] c
    LEFT JOIN [Transactions] t ON c.[Id] = t.[CategoryId] 
        AND t.[UserId] = @UserId 
        AND t.[TransactionType] = 2 -- Chỉ tính chi
        AND t.[TransactionDate] BETWEEN @StartDate AND @EndDate
    WHERE c.[Type] = 2 AND c.[IsActive] = 1
    GROUP BY c.[Id], c.[Name], c.[Icon], c.[Color]
    ORDER BY TotalAmount DESC;
END
GO

-- Procedure to get monthly trends
CREATE PROCEDURE [dbo].[GetMonthlyTrends]
    @UserId bigint,
    @Months int = 12
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH MonthlyData AS (
        SELECT 
            YEAR([TransactionDate]) AS [Year],
            MONTH([TransactionDate]) AS [Month],
            [TransactionType],
            SUM([Amount]) AS [Amount]
        FROM [Transactions]
        WHERE [UserId] = @UserId
            AND [TransactionType] IN (1, 2) -- Chỉ tính Thu/Chi
            AND [TransactionDate] >= DATEADD(MONTH, -@Months, GETDATE())
        GROUP BY YEAR([TransactionDate]), MONTH([TransactionDate]), [TransactionType]
    )
    SELECT 
        [Year],
        [Month],
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN [Amount] ELSE 0 END), 0) AS [Income],
        ISNULL(SUM(CASE WHEN [TransactionType] = 2 THEN [Amount] ELSE 0 END), 0) AS [Expense],
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN [Amount] ELSE -[Amount] END), 0) AS [Net]
    FROM MonthlyData
    GROUP BY [Year], [Month]
    ORDER BY [Year], [Month];
END
GO

-- Procedure to safely delete a category
CREATE PROCEDURE [dbo].[DeleteCategorySafely]
    @CategoryId bigint,
    @UserId bigint
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Check if category belongs to user
        IF NOT EXISTS (
            SELECT 1 FROM [Categories] 
            WHERE [Id] = @CategoryId AND [UserId] = @UserId
        )
        BEGIN
            RAISERROR('Category not found or access denied', 16, 1);
            RETURN;
        END
        
        -- Check if category is default
        IF EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = @CategoryId AND [IsDefault] = 1)
        BEGIN
            RAISERROR('Cannot delete default category', 16, 1);
            RETURN;
        END

        -- Check if it has child categories
        IF EXISTS (SELECT 1 FROM [Categories] WHERE [ParentCategoryId] = @CategoryId)
        BEGIN
            RAISERROR('Cannot delete category with child categories', 16, 1);
            RETURN;
        END
        
        -- Update related records to set CategoryId to NULL
        UPDATE [Transactions] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        UPDATE [Budgets] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        UPDATE [ScheduledTransactions] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        
        -- Delete the category
        DELETE FROM [Categories] WHERE [Id] = @CategoryId AND [UserId] = @UserId;
        
        COMMIT TRANSACTION;
        SELECT 'Category deleted successfully' AS Result;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- CREATE VIEWS (CẬP NHẬT)
-- =============================================
-- View for user transaction summary
CREATE VIEW [vw_UserTransactionSummary] AS
SELECT 
    u.[Id] AS [UserId],
    u.[UserName],
    u.[Email],
    u.[FullName],
    COUNT(t.[Id]) AS [TotalTransactions],
    ISNULL(SUM(CASE WHEN t.[TransactionType] = 1 THEN t.[Amount] ELSE 0 END), 0) AS [TotalIncome],
    ISNULL(SUM(CASE WHEN t.[TransactionType] = 2 THEN t.[Amount] ELSE 0 END), 0) AS [TotalExpense],
    ISNULL(SUM(CASE WHEN t.[TransactionType] = 1 THEN t.[Amount] ELSE (CASE WHEN t.[TransactionType] = 2 THEN -t.[Amount] ELSE 0 END) END), 0) AS [NetIncome],
    MAX(t.[TransactionDate]) AS [LastTransactionDate]
FROM [Users] u
LEFT JOIN [Transactions] t ON u.[Id] = t.[UserId]
WHERE u.[Enabled] = 1
GROUP BY u.[Id], u.[UserName], u.[Email], u.[FullName];
GO

-- View for category usage statistics
CREATE VIEW [vw_CategoryUsageStats] AS
SELECT 
    c.[Id] AS [CategoryId],
    c.[Name] AS [CategoryName],
    c.[Type] AS [CategoryType],
    c.[Icon] AS [CategoryIcon],
    c.[Color] AS [CategoryColor],
    COUNT(t.[Id]) AS [UsageCount],
    ISNULL(SUM(t.[Amount]), 0) AS [TotalAmount],
    ISNULL(AVG(t.[Amount]), 0) AS [AverageAmount],
    MAX(t.[TransactionDate]) AS [LastUsedDate]
FROM [Categories] c
LEFT JOIN [Transactions] t ON c.[Id] = t.[CategoryId]
WHERE c.[IsActive] = 1
GROUP BY c.[Id], c.[Name], c.[Type], c.[Icon], c.[Color];
GO

-- =============================================
-- *** PHẦN DỌN DẸP TRIGGER TRƯỚC KHI TẠO ***
-- =============================================
-- Chạy đoạn này để dọn dẹp các đối tượng cũ có thể gây lỗi
IF OBJECT_ID('tr_Users_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Users_UpdatedAt];
IF OBJECT_ID('tr_Categories_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Categories_UpdatedAt];
IF OBJECT_ID('tr_Transactions_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Transactions_UpdatedAt];
IF OBJECT_ID('tr_Budgets_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Budgets_UpdatedAt];
IF OBJECT_ID('tr_SystemSettings_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_SystemSettings_UpdatedAt];
IF OBJECT_ID('tr_Accounts_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Accounts_UpdatedAt];
IF OBJECT_ID('tr_SavingsGoals_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_SavingsGoals_UpdatedAt];
IF OBJECT_ID('tr_ScheduledTransactions_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_ScheduledTransactions_UpdatedAt];
IF OBJECT_ID('tr_Debts_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Debts_UpdatedAt];
IF OBJECT_ID('tr_Investments_UpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Investments_UpdatedAt];
IF OBJECT_ID('tr_Transactions_UpdateAccountBalance', 'TR') IS NOT NULL
    DROP TRIGGER [tr_Transactions_UpdateAccountBalance];
IF OBJECT_ID('tr_SavingsTransactions_UpdateGoal', 'TR') IS NOT NULL
    DROP TRIGGER [tr_SavingsTransactions_UpdateGoal];
GO


-- =============================================
-- CREATE TRIGGERS (*** ĐÃ SỬA LỖI 8124 ***)
-- =============================================
CREATE TRIGGER [tr_Users_UpdatedAt] ON [Users]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Users] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Users] u
    INNER JOIN inserted i ON u.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Categories_UpdatedAt] ON [Categories]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Categories] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Categories] c
    INNER JOIN inserted i ON c.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Transactions_UpdatedAt] ON [Transactions]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Transactions] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Transactions] t
    INNER JOIN inserted i ON t.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Budgets_UpdatedAt] ON [Budgets]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Budgets] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Budgets] b
    INNER JOIN inserted i ON b.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_SystemSettings_UpdatedAt] ON [SystemSettings]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [SystemSettings] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [SystemSettings] s
    INNER JOIN inserted i ON s.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Accounts_UpdatedAt] ON [Accounts]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Accounts] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Accounts] a
    INNER JOIN inserted i ON a.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_SavingsGoals_UpdatedAt] ON [SavingsGoals]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [SavingsGoals] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [SavingsGoals] s
    INNER JOIN inserted i ON s.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_ScheduledTransactions_UpdatedAt] ON [ScheduledTransactions]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [ScheduledTransactions] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [ScheduledTransactions] s
    INNER JOIN inserted i ON s.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Debts_UpdatedAt] ON [Debts]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Debts] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Debts] d
    INNER JOIN inserted i ON d.[Id] = i.[Id];
END
GO

CREATE TRIGGER [tr_Investments_UpdatedAt] ON [Investments]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Investments] 
    SET [UpdatedAt] = GETUTCDATE()
    FROM [Investments] inv
    INNER JOIN inserted i ON inv.[Id] = i.[Id];
END
GO

-- Trigger quan trọng: Tự động cập nhật số dư Account (*** ĐÃ SỬA LỖI 8124 ***)
CREATE TRIGGER [tr_Transactions_UpdateAccountBalance] ON [Transactions]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Tạm lưu các AccountId bị ảnh hưởng
    DECLARE @AffectedAccounts TABLE (AccountId bigint);
    INSERT INTO @AffectedAccounts (AccountId) SELECT AccountId FROM inserted;
    INSERT INTO @AffectedAccounts (AccountId) SELECT AccountId FROM deleted;
    INSERT INTO @AffectedAccounts (AccountId) SELECT PairedAccountId FROM inserted WHERE PairedAccountId IS NOT NULL;
    INSERT INTO @AffectedAccounts (AccountId) SELECT PairedAccountId FROM deleted WHERE PairedAccountId IS NOT NULL;

    -- Cập nhật số dư cho các AccountId bị ảnh hưởng
    UPDATE acc
    SET [CurrentBalance] = acc.[InitialBalance] 
    
    + ISNULL(( -- Phần 1: Tính tổng Thu, Chi, Chuyển đi (Source)
        SELECT SUM(
            CASE 
                WHEN t.[TransactionType] = 1 THEN t.[Amount]   -- Income (+)
                WHEN t.[TransactionType] = 2 THEN -t.[Amount]  -- Expense (-)
                WHEN t.[TransactionType] = 3 THEN -t.[Amount]  -- Transfer OUT (-)
                ELSE 0 
            END
        )
        FROM [Transactions] t
        WHERE t.[AccountId] = acc.[Id] -- Lọc theo tài khoản chính
    ), 0) 
    
    + ISNULL(( -- Phần 2: Tính tổng Chuyển đến (Destination)
        SELECT SUM(
            CASE 
                WHEN t.[TransactionType] = 3 THEN t.[Amount] -- Transfer IN (+)
                ELSE 0 
            END
        )
        FROM [Transactions] t
        WHERE t.[PairedAccountId] = acc.[Id] -- Lọc theo tài khoản nhận
    ), 0)
    
    FROM [Accounts] acc
    WHERE acc.[Id] IN (SELECT DISTINCT AccountId FROM @AffectedAccounts);

END
GO

-- Trigger: Tự động cập nhật tiến độ Mục tiêu Tiết kiệm
CREATE TRIGGER [tr_SavingsTransactions_UpdateGoal] ON [SavingsTransactions]
AFTER INSERT, DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AffectedGoals TABLE (GoalId bigint);
    INSERT INTO @AffectedGoals (GoalId) SELECT SavingsGoalId FROM inserted;
    INSERT INTO @AffectedGoals (GoalId) SELECT SavingsGoalId FROM deleted;

    UPDATE sg
    SET [CurrentAmount] = ISNULL((
        SELECT SUM(Amount) 
        FROM [SavingsTransactions] st 
        WHERE st.SavingsGoalId = sg.Id
    ), 0)
    FROM [SavingsGoals] sg
    WHERE sg.Id IN (SELECT DISTINCT GoalId FROM @AffectedGoals);
END
GO