-- Create Database
CREATE DATABASE ExpenseManager;
GO

USE ExpenseManager;
GO

-- =============================================
-- 1. USERS TABLE (Giá»¯ nguyÃªn)
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
-- 2. ACCOUNTS (WALLETS) TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [Accounts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AccountType] int NOT NULL, -- 1 = Tiá»n máº·t, 2 = NgÃ¢n hÃ ng, 3 = VÃ­ Ä‘iá»‡n tá»­, 4 = Tháº» tÃ­n dá»¥ng, 5 = Tiáº¿t kiá»‡m
    [InitialBalance] decimal(18,2) NOT NULL DEFAULT 0,
    [CurrentBalance] decimal(18,2) NOT NULL DEFAULT 0,
    [Currency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Icon] nvarchar(50) NULL,
    [Color] nvarchar(20) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [IncludeInTotal] bit NOT NULL DEFAULT 1, -- CÃ³ tÃ­nh vÃ o tá»•ng sá»‘ dÆ° khÃ´ng
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
-- 3. CATEGORIES TABLE (CHá»ˆNH Sá»¬A)
-- =============================================
CREATE TABLE [Categories] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ParentCategoryId] bigint NULL, -- Há»— trá»£ danh má»¥c cha-con
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
-- 4. TRANSACTIONS TABLE (Unified) (CHá»ˆNH Sá»¬A)
-- =============================================
CREATE TABLE [Transactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL, -- Giao dá»‹ch thuá»™c tÃ i khoáº£n/vÃ­ nÃ o
    [CategoryId] bigint NULL,
    [TransactionType] int NOT NULL, -- 1 = Income, 2 = Expense, 3 = Transfer
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL DEFAULT 'VND',
    [Note] nvarchar(512) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [PairedAccountId] bigint NULL, -- DÃ¹ng cho Transfer, lÃ  tÃ i khoáº£n Ä‘Ã­ch
    [PairedTransactionId] bigint NULL, -- DÃ¹ng cho Transfer, liÃªn káº¿t vá»›i giao dá»‹ch Ä‘á»‘i á»©ng
    [AttachmentUrl] nvarchar(512) NULL, -- ÄÃ­nh kÃ¨m hÃ³a Ä‘Æ¡n
    [OcrText] nvarchar(max) NULL, -- LÆ°u káº¿t quáº£ OCR
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
    [CategoryId] bigint NULL, -- NgÃ¢n sÃ¡ch cho danh má»¥c
    [AccountId] bigint NULL, -- NgÃ¢n sÃ¡ch cho má»™t tÃ i khoáº£n cá»¥ thá»ƒ
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
-- 6. SAVINGS GOALS TABLE (Báº¢NG Má»šI)
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
-- 7. SAVINGS TRANSACTIONS TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [SavingsTransactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [SavingsGoalId] bigint NOT NULL,
    [TransactionId] bigint NOT NULL, -- Giao dá»‹ch (chi) tÆ°Æ¡ng á»©ng tá»« báº£ng Transactions
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
-- 8. SCHEDULED TRANSACTIONS (RECURRING) TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [ScheduledTransactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [TransactionType] int NOT NULL, -- 1 = Income, 2 = Expense
    [Amount] decimal(18,2) NOT NULL,
    [Frequency] nvarchar(20) NOT NULL, -- 'daily', 'weekly', 'monthly', 'yearly'
    [Interval] int NOT NULL DEFAULT 1, -- VD: 2 + 'monthly' = 2 thÃ¡ng/láº§n
    [StartDate] date NOT NULL,
    [EndDate] date NULL,
    [NextRunDate] date NOT NULL, -- NgÃ y cháº¡y tiáº¿p theo (quan trá»ng cho job)
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
-- 9. DEBTS TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [Debts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [DebtType] int NOT NULL, -- 1 = Ná»£ pháº£i tráº£ (I owe), 2 = Ná»£ pháº£i thu (I am owed)
    [Name] nvarchar(100) NOT NULL, -- VD: "Vay mua xe", "TÃ¹ng mÆ°á»£n tiá»n"
    [PersonName] nvarchar(100) NULL, -- NgÆ°á»i cho vay / NgÆ°á»i vay
    [InitialAmount] decimal(18,2) NOT NULL,
    [AmountPaid] decimal(18,2) NOT NULL DEFAULT 0,
    [InterestRate] decimal(5,2) NOT NULL DEFAULT 0, -- LÃ£i suáº¥t %/nÄƒm
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
-- 10. DEBT PAYMENTS TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [DebtPayments] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [DebtId] bigint NOT NULL,
    [Email] nvarchar(256) NULL,
    [TransactionId] bigint NOT NULL, -- Giao dá»‹ch (chi/thu) tÆ°Æ¡ng á»©ng
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
-- 11. SHARED ACCOUNTS TABLE (Báº¢NG Má»šI)
-- =============================================
CREATE TABLE [SharedAccounts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [AccountId] bigint NOT NULL, -- TÃ i khoáº£n Ä‘Æ°á»£c chia sáº»
    [UserId] bigint NOT NULL, -- NgÆ°á»i Ä‘Æ°á»£c chia sáº»
    [Permission] int NOT NULL, -- 1 = ViewOnly, 2 = ViewAndAdd, 3 = FullAccess
    [SharedByUserId] bigint NOT NULL, -- NgÆ°á»i chá»§ sá»Ÿ há»¯u chia sáº»
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_SharedAccounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SharedAccounts_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SharedAccounts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SharedAccounts_Users_SharedByUserId] FOREIGN KEY ([SharedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UK_SharedAccounts_AccountId_UserId] UNIQUE ([AccountId], [UserId]) -- Äáº£m báº£o khÃ´ng chia sáº» 2 láº§n
);
GO

CREATE INDEX [IX_SharedAccounts_AccountId] ON [SharedAccounts] ([AccountId]);
CREATE INDEX [IX_SharedAccounts_UserId] ON [SharedAccounts] ([UserId]);
GO

-- =============================================
-- 12. INVESTMENTS TABLE (*** ÄÃƒ Sá»¬A Lá»–I 1785 ***)
-- =============================================
CREATE TABLE [Investments] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NULL, -- TÃ i khoáº£n dÃ¹ng Ä‘á»ƒ mua
    [Name] nvarchar(100) NOT NULL, -- VD: "Cá»• phiáº¿u FPT", "Bitcoin"
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
    CONSTRAINT [FK_Investments_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION -- Sá»­a tá»« SET NULL
);
GO

CREATE INDEX [IX_Investments_UserId] ON [Investments] ([UserId]);
GO

-- =============================================
-- 13. BANK CONNECTIONS TABLE (*** ÄÃƒ Sá»¬A Lá»–I 1785 ***)
-- =============================================
CREATE TABLE [BankConnections] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [AccountId] bigint NOT NULL, -- LiÃªn káº¿t vá»›i tÃ i khoáº£n ná»™i bá»™
    [Provider] nvarchar(50) NOT NULL, -- VD: 'Plaid', 'VietQR'
    [AccessToken] nvarchar(max) NOT NULL, -- NÃªn Ä‘Æ°á»£c mÃ£ hÃ³a
    [ItemId] nvarchar(256) NULL, -- ID Ä‘á»‹nh danh tá»« provider
    [LastSync] datetime2 NULL,
    [SyncStatus] nvarchar(20) NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_BankConnections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BankConnections_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankConnections_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION -- Sá»­a tá»« CASCADE
);
GO

CREATE INDEX [IX_BankConnections_UserId] ON [BankConnections] ([UserId]);
GO

-- =============================================
-- 14. CURRENCY RATES TABLE (Báº¢NG Má»šI)
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
    [Parameters] nvarchar(max) NULL, -- JSON string (cÃ³ thá»ƒ chá»©a AccountId, CategoryId...)
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
-- 22. ONBOARDING STATUS TABLE (Báº¢NG Má»šI)
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
-- 23. FINANCIAL ALERTS TABLE (Báº¢NG Má»šI)
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
-- 24. GROUP EXPENSES TABLE (Báº¢NG Má»šI)
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
-- 25. GROUP MEMBERS TABLE (Báº¢NG Má»šI)
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
-- 26. GROUP TRANSACTIONS TABLE (Báº¢NG Má»šI)
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
-- 27. GROUP TRANSACTION SPLITS TABLE (Báº¢NG Má»šI)
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
-- CREATE STORED PROCEDURES (Cáº¬P NHáº¬T)
-- =============================================
-- Procedure to get user dashboard statistics
CREATE PROCEDURE [dbo].[GetUserDashboardStats]
    @UserId bigint,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;
    
    -- TÃ­nh toÃ¡n tá»•ng thu, chi (KhÃ´ng bao gá»“m Transfer)
    SELECT 
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN Amount ELSE 0 END), 0) AS TotalIncome,
        ISNULL(SUM(CASE WHEN [TransactionType] = 2 THEN Amount ELSE 0 END), 0) AS TotalExpense,
        ISNULL(SUM(CASE WHEN [TransactionType] = 1 THEN Amount ELSE -Amount END), 0) AS NetIncome,
        COUNT(CASE WHEN [TransactionType] IN (1, 2) THEN 1 ELSE NULL END) AS TransactionCount
    FROM [Transactions]
    WHERE [UserId] = @UserId 
        AND [TransactionType] IN (1, 2) -- Chá»‰ tÃ­nh Thu/Chi
        AND [TransactionDate] BETWEEN @StartDate AND @EndDate;

    -- TÃ­nh toÃ¡n tá»•ng sá»‘ dÆ° tá»« táº¥t cáº£ cÃ¡c vÃ­
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
        AND t.[TransactionType] = 2 -- Chá»‰ tÃ­nh chi
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
            AND [TransactionType] IN (1, 2) -- Chá»‰ tÃ­nh Thu/Chi
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
-- CREATE VIEWS (Cáº¬P NHáº¬T)
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
-- *** PHáº¦N Dá»ŒN Dáº¸P TRIGGER TRÆ¯á»šC KHI Táº O ***
-- =============================================
-- Cháº¡y Ä‘oáº¡n nÃ y Ä‘á»ƒ dá»n dáº¹p cÃ¡c Ä‘á»‘i tÆ°á»£ng cÅ© cÃ³ thá»ƒ gÃ¢y lá»—i
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
-- CREATE TRIGGERS (*** ÄÃƒ Sá»¬A Lá»–I 8124 ***)
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

-- Trigger quan trá»ng: Tá»± Ä‘á»™ng cáº­p nháº­t sá»‘ dÆ° Account (*** ÄÃƒ Sá»¬A Lá»–I 8124 ***)
CREATE TRIGGER [tr_Transactions_UpdateAccountBalance] ON [Transactions]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Táº¡m lÆ°u cÃ¡c AccountId bá»‹ áº£nh hÆ°á»Ÿng
    DECLARE @AffectedAccounts TABLE (AccountId bigint);
    INSERT INTO @AffectedAccounts (AccountId) SELECT AccountId FROM inserted;
    INSERT INTO @AffectedAccounts (AccountId) SELECT AccountId FROM deleted;
    INSERT INTO @AffectedAccounts (AccountId) SELECT PairedAccountId FROM inserted WHERE PairedAccountId IS NOT NULL;
    INSERT INTO @AffectedAccounts (AccountId) SELECT PairedAccountId FROM deleted WHERE PairedAccountId IS NOT NULL;

    -- Cáº­p nháº­t sá»‘ dÆ° cho cÃ¡c AccountId bá»‹ áº£nh hÆ°á»Ÿng
    UPDATE acc
    SET [CurrentBalance] = acc.[InitialBalance] 
    
    + ISNULL(( -- Pháº§n 1: TÃ­nh tá»•ng Thu, Chi, Chuyá»ƒn Ä‘i (Source)
        SELECT SUM(
            CASE 
                WHEN t.[TransactionType] = 1 THEN t.[Amount]   -- Income (+)
                WHEN t.[TransactionType] = 2 THEN -t.[Amount]  -- Expense (-)
                WHEN t.[TransactionType] = 3 THEN -t.[Amount]  -- Transfer OUT (-)
                ELSE 0 
            END
        )
        FROM [Transactions] t
        WHERE t.[AccountId] = acc.[Id] -- Lá»c theo tÃ i khoáº£n chÃ­nh
    ), 0) 
    
    + ISNULL(( -- Pháº§n 2: TÃ­nh tá»•ng Chuyá»ƒn Ä‘áº¿n (Destination)
        SELECT SUM(
            CASE 
                WHEN t.[TransactionType] = 3 THEN t.[Amount] -- Transfer IN (+)
                ELSE 0 
            END
        )
        FROM [Transactions] t
        WHERE t.[PairedAccountId] = acc.[Id] -- Lá»c theo tÃ i khoáº£n nháº­n
    ), 0)
    
    FROM [Accounts] acc
    WHERE acc.[Id] IN (SELECT DISTINCT AccountId FROM @AffectedAccounts);

END
GO

-- Trigger: Tá»± Ä‘á»™ng cáº­p nháº­t tiáº¿n Ä‘á»™ Má»¥c tiÃªu Tiáº¿t kiá»‡m
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

-- =============================================
-- 28. SUBSCRIPTION SYSTEM (Merged from AddSubscriptionTables.sql and seed-service-packages.sql)
-- =============================================

-- Create ServicePackages table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServicePackages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ServicePackages](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [PackageType] INT NOT NULL DEFAULT 0,
        [Price] DECIMAL(18, 2) NOT NULL,
        [OriginalPrice] DECIMAL(18, 2) NULL,
        [BillingCycle] INT NOT NULL DEFAULT 1,
        [DurationDays] INT NOT NULL DEFAULT 30,
        [Features] NVARCHAR(MAX) NULL,
        [MaxTransactions] INT NOT NULL DEFAULT 0,
        [MaxAccounts] INT NOT NULL DEFAULT 0,
        [MaxBudgets] INT NOT NULL DEFAULT 0,
        [HasAdvancedReports] BIT NOT NULL DEFAULT 0,
        [HasAiAdvisor] BIT NOT NULL DEFAULT 0,
        [HasGroupExpense] BIT NOT NULL DEFAULT 0,
        [HasPrioritySupport] BIT NOT NULL DEFAULT 0,
        [IsPopular] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [BadgeText] NVARCHAR(50) NULL,
        [BadgeColor] NVARCHAR(50) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [PK_ServicePackages] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ServicePackages_PackageType] ON [dbo].[ServicePackages]([PackageType] ASC);
    CREATE NONCLUSTERED INDEX [IX_ServicePackages_IsActive] ON [dbo].[ServicePackages]([IsActive] ASC);
END
GO

-- Create Subscriptions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Subscriptions](
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] BIGINT NOT NULL,
        [PackageId] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NOT NULL,
        [CancelledAt] DATETIME NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        [AutoRenew] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Subscriptions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([Id]),
        CONSTRAINT [FK_Subscriptions_ServicePackages] FOREIGN KEY([PackageId]) REFERENCES [dbo].[ServicePackages]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_Subscriptions_UserId] ON [dbo].[Subscriptions]([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Subscriptions_PackageId] ON [dbo].[Subscriptions]([PackageId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Subscriptions_Status] ON [dbo].[Subscriptions]([Status] ASC);
    CREATE NONCLUSTERED INDEX [IX_Subscriptions_EndDate] ON [dbo].[Subscriptions]([EndDate] ASC);
END
GO

-- Create Payments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments](
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [SubscriptionId] BIGINT NOT NULL,
        [Amount] DECIMAL(18, 2) NOT NULL,
        [Currency] NVARCHAR(3) NOT NULL DEFAULT 'VND',
        [Status] INT NOT NULL DEFAULT 0,
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [TransactionId] NVARCHAR(256) NULL,
        [PaymentData] NVARCHAR(MAX) NULL,
        [PaidAt] DATETIME NULL,
        [FailureReason] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Payments_Subscriptions] FOREIGN KEY([SubscriptionId]) REFERENCES [dbo].[Subscriptions]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_Payments_SubscriptionId] ON [dbo].[Payments]([SubscriptionId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Payments_Status] ON [dbo].[Payments]([Status] ASC);
    CREATE NONCLUSTERED INDEX [IX_Payments_TransactionId] ON [dbo].[Payments]([TransactionId] ASC);
END
GO

-- Create triggers for UpdatedAt
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_ServicePackages_UpdatedAt')
BEGIN
    EXEC('
    CREATE TRIGGER tr_ServicePackages_UpdatedAt
    ON ServicePackages
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE ServicePackages
        SET UpdatedAt = GETUTCDATE()
        FROM ServicePackages sp
        INNER JOIN inserted i ON sp.Id = i.Id;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_Subscriptions_UpdatedAt')
BEGIN
    EXEC('
    CREATE TRIGGER tr_Subscriptions_UpdatedAt
    ON Subscriptions
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE Subscriptions
        SET UpdatedAt = GETUTCDATE()
        FROM Subscriptions s
        INNER JOIN inserted i ON s.Id = i.Id;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_Payments_UpdatedAt')
BEGIN
    EXEC('
    CREATE TRIGGER tr_Payments_UpdatedAt
    ON Payments
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE Payments
        SET UpdatedAt = GETUTCDATE()
        FROM Payments p
        INNER JOIN inserted i ON p.Id = i.Id;
    END
    ');
END
GO

-- =============================================
-- 29. PAYMENT TRANSACTIONS TABLE (For link.com gateway)
-- =============================================
-- Create PaymentTransactions table to store payment gateway data
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentTransactions](
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] BIGINT NOT NULL,
        [PackageId] INT NOT NULL,
        [PackageName] NVARCHAR(100) NOT NULL,
        [Amount] DECIMAL(18, 2) NOT NULL,
        [Currency] NVARCHAR(3) NOT NULL DEFAULT 'VND',
        [SessionToken] NVARCHAR(512) NULL,
        [PaymentGatewayUrl] NVARCHAR(512) NOT NULL DEFAULT 'https://link.com',
        [GatewayTransactionId] NVARCHAR(256) NULL,
        [GatewayResponse] NVARCHAR(MAX) NULL,
        [Status] INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Processing, 2=Success, 3=Failed, 4=Cancelled
        [RedirectUrl] NVARCHAR(512) NULL,
        [ReturnUrl] NVARCHAR(512) NULL,
        [CancelUrl] NVARCHAR(512) NULL,
        [IpAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(512) NULL,
        [RequestTimestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ResponseTimestamp] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        [FailureReason] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PaymentTransactions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PaymentTransactions_ServicePackages] FOREIGN KEY([PackageId]) REFERENCES [dbo].[ServicePackages]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_UserId] ON [dbo].[PaymentTransactions]([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_PackageId] ON [dbo].[PaymentTransactions]([PackageId] ASC);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_Status] ON [dbo].[PaymentTransactions]([Status] ASC);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_GatewayTransactionId] ON [dbo].[PaymentTransactions]([GatewayTransactionId] ASC);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_RequestTimestamp] ON [dbo].[PaymentTransactions]([RequestTimestamp] ASC);
END
GO

-- Create trigger for PaymentTransactions UpdatedAt
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_PaymentTransactions_UpdatedAt')
BEGIN
    EXEC('
    CREATE TRIGGER tr_PaymentTransactions_UpdatedAt
    ON PaymentTransactions
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE PaymentTransactions
        SET UpdatedAt = GETUTCDATE()
        FROM PaymentTransactions pt
        INNER JOIN inserted i ON pt.Id = i.Id;
    END
    ');
END
GO

-- Insert default service packages
IF NOT EXISTS (SELECT * FROM ServicePackages)
BEGIN
    SET IDENTITY_INSERT ServicePackages ON;

    INSERT INTO ServicePackages (Id, Name, Description, Price, OriginalPrice, DurationDays, BillingCycle, Features, IsPopular, IsActive, BadgeText, BadgeColor, DisplayOrder, CreatedAt, PackageType, MaxTransactions, MaxAccounts, MaxBudgets, HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport)
    VALUES 
    (1, N'Gói Miễn Phí', N'Hoàn hảo để bắt đầu quản lý tài chính cá nhân', 0, NULL, 365, 1, 
    N'["Theo dõi thu chi cơ bản","Tối đa 3 ví","Báo cáo hàng tháng","Hỗ trợ qua email","Lưu trữ 100 giao dịch"]', 
    0, 1, NULL, NULL, 1, GETUTCDATE(), 0, 100, 3, 3, 0, 0, 0, 0),
    
    (2, N'Gói Cơ Bản', N'Dành cho người dùng cá nhân muốn quản lý tốt hơn', 99000, 149000, 30, 1, 
    N'["Tất cả tính năng Miễn Phí","Không giới hạn ví","Báo cáo chi tiết","Phân loại tự động","Lưu trữ không giới hạn","Xuất báo cáo Excel/PDF","Hỗ trợ ưu tiên"]', 
    0, 1, N'Giảm 33%', N'discount', 2, GETUTCDATE(), 1, -1, -1, -1, 1, 0, 0, 0),
    
    (3, N'Gói Chuyên Nghiệp', N'Giải pháp toàn diện cho quản lý tài chính chuyên nghiệp', 199000, 299000, 30, 1, 
    N'["Tất cả tính năng Cơ Bản","AI phân tích chi tiêu","Dự báo tài chính","Quản lý đầu tư","Theo dõi nợ & tiết kiệm","Chia sẻ chi tiêu nhóm","Tích hợp ngân hàng","Hỗ trợ 24/7","Tư vấn tài chính cá nhân"]', 
    1, 1, N'Phổ biến nhất', N'popular', 3, GETUTCDATE(), 2, -1, -1, -1, 1, 1, 1, 1),
    
    (4, N'Gói Doanh Nghiệp', N'Giải pháp cho doanh nghiệp và nhóm làm việc', 499000, NULL, 30, 1, 
    N'["Tất cả tính năng Chuyên Nghiệp","Quản lý nhiều người dùng","Phân quyền chi tiết","API tích hợp","Báo cáo tùy chỉnh","Sao lưu tự động","Bảo mật nâng cao","Đào tạo & onboarding","Account manager riêng"]', 
    0, 1, NULL, NULL, 4, GETUTCDATE(), 3, -1, -1, -1, 1, 1, 1, 1),
    
    (5, N'Gói Năm - Cơ Bản', N'Tiết kiệm 20% khi đăng ký theo năm', 950000, 1188000, 365, 12, 
    N'["Tất cả tính năng Gói Cơ Bản","Thanh toán 1 lần/năm","Tiết kiệm 238.000đ","Ưu tiên cập nhật tính năng mới"]', 
    0, 1, N'Tiết kiệm 20%', N'discount', 5, GETUTCDATE(), 1, -1, -1, -1, 1, 0, 0, 0),
    
    (6, N'Gói Năm - Chuyên Nghiệp', N'Tiết kiệm 25% khi đăng ký theo năm', 1790000, 2388000, 365, 12, 
    N'["Tất cả tính năng Gói Chuyên Nghiệp","Thanh toán 1 lần/năm","Tiết kiệm 598.000đ","Tặng 1 tháng sử dụng","Ưu tiên hỗ trợ VIP"]', 
    1, 1, N'Ưu đãi nhất', N'popular', 6, GETUTCDATE(), 2, -1, -1, -1, 1, 1, 1, 1);

    SET IDENTITY_INSERT ServicePackages OFF;
END
GO
