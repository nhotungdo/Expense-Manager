

-- Create Database
CREATE DATABASE ExpenseManager;
GO

USE ExpenseManager;
GO

-- =============================================
-- 1. USERS TABLE
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
-- 2. CATEGORIES TABLE
-- =============================================
CREATE TABLE [Categories] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
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
    CONSTRAINT [FK_Categories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Categories
CREATE INDEX [IX_Categories_UserId] ON [Categories] ([UserId]);
CREATE INDEX [IX_Categories_Type] ON [Categories] ([Type]);
CREATE INDEX [IX_Categories_IsDefault] ON [Categories] ([IsDefault]);
GO

-- =============================================
-- 3. EXPENSES TABLE
-- =============================================
CREATE TABLE [Expenses] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NULL DEFAULT 'VND',
    [Note] nvarchar(512) NULL,
    [ExpenseDate] date NOT NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Expenses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Expenses_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Expenses
CREATE INDEX [IX_Expenses_UserId] ON [Expenses] ([UserId]);
CREATE INDEX [IX_Expenses_CategoryId] ON [Expenses] ([CategoryId]);
CREATE INDEX [IX_Expenses_ExpenseDate] ON [Expenses] ([ExpenseDate]);
CREATE INDEX [IX_Expenses_CreatedAt] ON [Expenses] ([CreatedAt]);
GO

-- =============================================
-- 4. INCOMES TABLE
-- =============================================
CREATE TABLE [Incomes] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NULL DEFAULT 'VND',
    [Note] nvarchar(512) NULL,
    [IncomeDate] date NOT NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Incomes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Incomes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Incomes_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Incomes
CREATE INDEX [IX_Incomes_UserId] ON [Incomes] ([UserId]);
CREATE INDEX [IX_Incomes_CategoryId] ON [Incomes] ([CategoryId]);
CREATE INDEX [IX_Incomes_IncomeDate] ON [Incomes] ([IncomeDate]);
CREATE INDEX [IX_Incomes_CreatedAt] ON [Incomes] ([CreatedAt]);
GO

-- =============================================
-- 5. TRANSACTIONS TABLE (Unified)
-- =============================================
CREATE TABLE [Transactions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [Type] int NOT NULL, -- 1 = Income, 2 = Expense
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(512) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Transactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Transactions
CREATE INDEX [IX_Transactions_UserId] ON [Transactions] ([UserId]);
CREATE INDEX [IX_Transactions_CategoryId] ON [Transactions] ([CategoryId]);
CREATE INDEX [IX_Transactions_Type] ON [Transactions] ([Type]);
CREATE INDEX [IX_Transactions_TransactionDate] ON [Transactions] ([TransactionDate]);
CREATE INDEX [IX_Transactions_CreatedAt] ON [Transactions] ([CreatedAt]);
GO

-- =============================================
-- 6. BUDGETS TABLE
-- =============================================
CREATE TABLE [Budgets] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [CategoryId] bigint NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Period] int NOT NULL, -- 1 = Weekly, 2 = Monthly, 3 = Yearly
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Budgets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Budgets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for Budgets
CREATE INDEX [IX_Budgets_UserId] ON [Budgets] ([UserId]);
CREATE INDEX [IX_Budgets_CategoryId] ON [Budgets] ([CategoryId]);
CREATE INDEX [IX_Budgets_Period] ON [Budgets] ([Period]);
CREATE INDEX [IX_Budgets_StartDate] ON [Budgets] ([StartDate]);
CREATE INDEX [IX_Budgets_EndDate] ON [Budgets] ([EndDate]);
GO

-- =============================================
-- 7. NOTIFICATIONS TABLE
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

-- Create indexes for Notifications
CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_Type] ON [Notifications] ([Type]);
CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
CREATE INDEX [IX_Notifications_IsImportant] ON [Notifications] ([IsImportant]);
CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
GO

-- =============================================
-- 8. AI SUGGESTIONS TABLE
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

-- Create indexes for AiSuggestions
CREATE INDEX [IX_AiSuggestions_UserId] ON [AiSuggestions] ([UserId]);
CREATE INDEX [IX_AiSuggestions_CreatedAt] ON [AiSuggestions] ([CreatedAt]);
GO

-- =============================================
-- 9. AUDIT LOGS TABLE
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

-- Create indexes for AuditLogs
CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
CREATE INDEX [IX_AuditLogs_Action] ON [AuditLogs] ([Action]);
CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);
CREATE INDEX [IX_AuditLogs_EntityId] ON [AuditLogs] ([EntityId]);
CREATE INDEX [IX_AuditLogs_CreatedAt] ON [AuditLogs] ([CreatedAt]);
GO

-- =============================================
-- 10. EMAILS TABLE
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

-- Create indexes for Emails
CREATE INDEX [IX_Emails_UserId] ON [Emails] ([UserId]);
CREATE INDEX [IX_Emails_Status] ON [Emails] ([Status]);
CREATE INDEX [IX_Emails_CreatedAt] ON [Emails] ([CreatedAt]);
GO

-- =============================================
-- 11. REPORTS TABLE
-- =============================================
CREATE TABLE [Reports] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [ReportType] nvarchar(20) NOT NULL, -- 'monthly', 'weekly', 'yearly', 'custom'
    [ReportName] nvarchar(256) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Parameters] nvarchar(max) NULL, -- JSON string
    [FilePath] nvarchar(512) NULL,
    [FileFormat] nvarchar(10) NULL, -- 'pdf', 'excel', 'csv'
    [GeneratedAt] datetime2 NULL,
    [CreatedAt] datetime2 NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for Reports
CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);
CREATE INDEX [IX_Reports_ReportType] ON [Reports] ([ReportType]);
CREATE INDEX [IX_Reports_StartDate] ON [Reports] ([StartDate]);
CREATE INDEX [IX_Reports_EndDate] ON [Reports] ([EndDate]);
CREATE INDEX [IX_Reports_CreatedAt] ON [Reports] ([CreatedAt]);
GO

-- =============================================
-- 12. ASP.NET CORE IDENTITY TABLES
-- =============================================

-- AspNetRoles table
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

-- AspNetRoleClaims table
CREATE TABLE [AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserClaims table
CREATE TABLE [AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] bigint NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserLogins table
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] bigint NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserRoles table
CREATE TABLE [AspNetUserRoles] (
    [UserId] bigint NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserTokens table
CREATE TABLE [AspNetUserTokens] (
    [UserId] bigint NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for Identity tables
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_UserId] ON [AspNetUserRoles] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

-- =============================================
-- 13. SYSTEM SETTINGS TABLE
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

-- Create unique index for SystemSettings
CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);
CREATE INDEX [IX_SystemSettings_IsActive] ON [SystemSettings] ([IsActive]);
GO

-- =============================================
-- INSERT DEFAULT DATA
-- =============================================

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

-- Insert default categories
INSERT INTO [Categories] ([Name], [Type], [Description], [Icon], [Color], [UserId], [IsDefault], [IsActive], [CreatedAt])
VALUES 
-- Expense Categories (Type = 2)
(N'Ăn uống', 2, N'Chi phí ăn uống hàng ngày', 'fas fa-utensils', '#FF6B6B', NULL, 1, 1, GETUTCDATE()),
(N'Giao thông', 2, N'Chi phí đi lại, xăng xe', 'fas fa-car', '#4ECDC4', NULL, 1, 1, GETUTCDATE()),
(N'Mua sắm', 2, N'Mua sắm quần áo, đồ dùng', 'fas fa-shopping-bag', '#45B7D1', NULL, 1, 1, GETUTCDATE()),
(N'Giải trí', 2, N'Chi phí giải trí, du lịch', 'fas fa-gamepad', '#96CEB4', NULL, 1, 1, GETUTCDATE()),
(N'Y tế', 2, N'Chi phí khám chữa bệnh', 'fas fa-heartbeat', '#FFEAA7', NULL, 1, 1, GETUTCDATE()),
(N'Học tập', 2, N'Chi phí học tập, sách vở', 'fas fa-book', '#DDA0DD', NULL, 1, 1, GETUTCDATE()),
(N'Hóa đơn', 2, N'Điện, nước, internet', 'fas fa-file-invoice', '#98D8C8', NULL, 1, 1, GETUTCDATE()),
(N'Khác', 2, N'Chi phí khác', 'fas fa-ellipsis-h', '#F7DC6F', NULL, 1, 1, GETUTCDATE()),

-- Income Categories (Type = 1)
(N'Lương', 1, N'Lương cơ bản hàng tháng', 'fas fa-briefcase', '#2ECC71', NULL, 1, 1, GETUTCDATE()),
(N'Thưởng', 1, N'Tiền thưởng, phụ cấp', 'fas fa-gift', '#F39C12', NULL, 1, 1, GETUTCDATE()),
(N'Đầu tư', 1, N'Lợi nhuận từ đầu tư', 'fas fa-chart-line', '#9B59B6', NULL, 1, 1, GETUTCDATE()),
(N'Kinh doanh', 1, N'Thu nhập từ kinh doanh', 'fas fa-store', '#E74C3C', NULL, 1, 1, GETUTCDATE()),
(N'Làm thêm', 1, N'Thu nhập từ việc làm thêm', 'fas fa-clock', '#3498DB', NULL, 1, 1, GETUTCDATE()),
(N'Khác', 1, N'Thu nhập khác', 'fas fa-plus-circle', '#1ABC9C', NULL, 1, 1, GETUTCDATE());
GO

-- Insert default system settings
INSERT INTO [SystemSettings] ([SettingKey], [SettingValue], [Description], [SettingType], [IsActive], [CreatedAt])
VALUES 
(N'app_name', N'Money Tracker', N'Tên ứng dụng', 'string', 1, GETUTCDATE()),
(N'app_version', N'1.0.0', N'Phiên bản ứng dụng', 'string', 1, GETUTCDATE()),
(N'default_currency', N'VND', N'Đơn vị tiền tệ mặc định', 'string', 1, GETUTCDATE()),
(N'default_language', N'vi', N'Ngôn ngữ mặc định', 'string', 1, GETUTCDATE()),
(N'max_file_size', N'10485760', N'Kích thước file tối đa (bytes)', 'number', 1, GETUTCDATE()),
(N'email_enabled', N'true', 'NBật/tắt gửi email', 'boolean', 1, GETUTCDATE()),
(N'notification_enabled', N'true', N'Bật/tắt thông báo', 'boolean', 1, GETUTCDATE()),
(N'backup_enabled', N'true', N'Bật/tắt sao lưu tự động', 'boolean', 1, GETUTCDATE()),
(N'maintenance_mode', N'false', N'Chế độ bảo trì', 'boolean', 1, GETUTCDATE());
GO


-- =============================================
-- CREATE STORED PROCEDURES
-- =============================================

-- Procedure to get user dashboard statistics
CREATE PROCEDURE [dbo].[GetUserDashboardStats]
    @UserId bigint,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Total Income
        ISNULL(SUM(CASE WHEN Type = 1 THEN Amount ELSE 0 END), 0) AS TotalIncome,
        -- Total Expense
        ISNULL(SUM(CASE WHEN Type = 2 THEN Amount ELSE 0 END), 0) AS TotalExpense,
        -- Net Income
        ISNULL(SUM(CASE WHEN Type = 1 THEN Amount ELSE -Amount END), 0) AS NetIncome,
        -- Transaction Count
        COUNT(*) AS TransactionCount
    FROM [Transactions]
    WHERE [UserId] = @UserId 
        AND [TransactionDate] BETWEEN @StartDate AND @EndDate;
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
        AND t.[Type] = 2
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
            [Type],
            SUM([Amount]) AS [Amount]
        FROM [Transactions]
        WHERE [UserId] = @UserId
            AND [TransactionDate] >= DATEADD(MONTH, -@Months, GETDATE())
        GROUP BY YEAR([TransactionDate]), MONTH([TransactionDate]), [Type]
    )
    SELECT 
        [Year],
        [Month],
        ISNULL(SUM(CASE WHEN [Type] = 1 THEN [Amount] ELSE 0 END), 0) AS [Income],
        ISNULL(SUM(CASE WHEN [Type] = 2 THEN [Amount] ELSE 0 END), 0) AS [Expense],
        ISNULL(SUM(CASE WHEN [Type] = 1 THEN [Amount] ELSE -[Amount] END), 0) AS [Net]
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
        -- Check if category belongs to user or is default
        IF NOT EXISTS (
            SELECT 1 FROM [Categories] 
            WHERE [Id] = @CategoryId 
            AND ([UserId] = @UserId OR [IsDefault] = 1)
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
        
        -- Update related records to set CategoryId to NULL
        UPDATE [Expenses] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        UPDATE [Incomes] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        UPDATE [Transactions] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        UPDATE [Budgets] SET [CategoryId] = NULL WHERE [CategoryId] = @CategoryId;
        
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
-- CREATE VIEWS
-- =============================================

-- View for user transaction summary
CREATE VIEW [vw_UserTransactionSummary] AS
SELECT 
    u.[Id] AS [UserId],
    u.[UserName],
    u.[Email],
    u.[FullName],
    COUNT(t.[Id]) AS [TotalTransactions],
    ISNULL(SUM(CASE WHEN t.[Type] = 1 THEN t.[Amount] ELSE 0 END), 0) AS [TotalIncome],
    ISNULL(SUM(CASE WHEN t.[Type] = 2 THEN t.[Amount] ELSE 0 END), 0) AS [TotalExpense],
    ISNULL(SUM(CASE WHEN t.[Type] = 1 THEN t.[Amount] ELSE -t.[Amount] END), 0) AS [NetIncome],
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
-- CREATE TRIGGERS
-- =============================================

-- Trigger to update UpdatedAt timestamp
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
