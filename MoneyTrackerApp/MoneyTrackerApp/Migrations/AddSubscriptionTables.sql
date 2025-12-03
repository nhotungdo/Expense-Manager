-- =============================================
-- Subscription System Migration Script
-- =============================================

-- Create ServicePackages table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServicePackages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ServicePackages](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [PackageType] INT NOT NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [BillingCycle] INT NOT NULL DEFAULT 1,
        [Features] NVARCHAR(MAX) NULL,
        [MaxTransactions] INT NOT NULL DEFAULT 0,
        [MaxAccounts] INT NOT NULL DEFAULT 0,
        [MaxBudgets] INT NOT NULL DEFAULT 0,
        [HasAdvancedReports] BIT NOT NULL DEFAULT 0,
        [HasAiAdvisor] BIT NOT NULL DEFAULT 0,
        [HasGroupExpense] BIT NOT NULL DEFAULT 0,
        [HasPrioritySupport] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
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

-- Insert default service packages
IF NOT EXISTS (SELECT * FROM ServicePackages WHERE PackageType = 0)
BEGIN
    INSERT INTO ServicePackages (Name, Description, PackageType, Price, BillingCycle, Features, MaxTransactions, MaxAccounts, MaxBudgets, HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport, IsActive, DisplayOrder)
    VALUES 
    (
        N'Miễn phí',
        N'Gói cơ bản cho người dùng mới',
        0, -- Free
        0,
        1, -- Monthly
        N'["Ghi chép giao dịch cơ bản", "Danh mục cá nhân", "Báo cáo tháng"]',
        100, -- Max 100 transactions
        3,   -- Max 3 accounts
        3,   -- Max 3 budgets
        0,   -- No advanced reports
        0,   -- No AI advisor
        0,   -- No group expense
        0,   -- No priority support
        1,   -- Active
        1    -- Display order
    );
END
GO

IF NOT EXISTS (SELECT * FROM ServicePackages WHERE PackageType = 1)
BEGIN
    INSERT INTO ServicePackages (Name, Description, PackageType, Price, BillingCycle, Features, MaxTransactions, MaxAccounts, MaxBudgets, HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport, IsActive, DisplayOrder)
    VALUES 
    (
        N'Pro',
        N'Gói nâng cao cho người dùng chuyên nghiệp',
        1, -- Pro
        79000,
        1, -- Monthly
        N'["Ngân sách thông minh", "Mục tiêu tiết kiệm", "Báo cáo nâng cao", "Tư vấn AI"]',
        -1,  -- Unlimited transactions
        -1,  -- Unlimited accounts
        -1,  -- Unlimited budgets
        1,   -- Advanced reports
        1,   -- AI advisor
        0,   -- No group expense
        0,   -- No priority support
        1,   -- Active
        2    -- Display order
    );
END
GO

IF NOT EXISTS (SELECT * FROM ServicePackages WHERE PackageType = 2)
BEGIN
    INSERT INTO ServicePackages (Name, Description, PackageType, Price, BillingCycle, Features, MaxTransactions, MaxAccounts, MaxBudgets, HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport, IsActive, DisplayOrder)
    VALUES 
    (
        N'Team',
        N'Gói dành cho nhóm và doanh nghiệp',
        2, -- Team
        199000,
        1, -- Monthly
        N'["Tài khoản chia sẻ", "Chi tiêu nhóm", "Hỗ trợ ưu tiên", "Tất cả tính năng Pro"]',
        -1,  -- Unlimited transactions
        -1,  -- Unlimited accounts
        -1,  -- Unlimited budgets
        1,   -- Advanced reports
        1,   -- AI advisor
        1,   -- Group expense
        1,   -- Priority support
        1,   -- Active
        3    -- Display order
    );
END
GO

PRINT 'Subscription system migration completed successfully!';
