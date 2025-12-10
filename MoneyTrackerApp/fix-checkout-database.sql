-- =============================================
-- Fix Checkout Database Issues
-- =============================================

USE ExpenseManager;
GO

PRINT '========================================='
PRINT 'CHECKOUT DATABASE FIX SCRIPT'
PRINT '========================================='
PRINT ''

-- Step 1: Check if ServicePackages table exists
PRINT '1. Checking ServicePackages table...'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServicePackages]') AND type in (N'U'))
BEGIN
    PRINT '   ❌ ERROR: ServicePackages table does not exist!'
    PRINT '   → Please run Database_Schema.sql first to create the table'
    PRINT ''
END
ELSE
BEGIN
    PRINT '   ✓ ServicePackages table exists'
    PRINT ''
    
    -- Step 2: Check current data
    DECLARE @PackageCount INT
    SELECT @PackageCount = COUNT(*) FROM ServicePackages
    
    PRINT '2. Checking existing data...'
    PRINT '   Current packages: ' + CAST(@PackageCount AS VARCHAR(10))
    
    IF @PackageCount > 0
    BEGIN
        PRINT '   Existing packages:'
        SELECT 
            Id,
            Name,
            Price,
            DurationDays,
            IsActive,
            CASE WHEN CreatedAt IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR(20), CreatedAt, 120) END as CreatedAt
        FROM ServicePackages
        ORDER BY DisplayOrder
        PRINT ''
    END
    
    -- Step 3: Insert or update default packages
    PRINT '3. Setting up default service packages...'
    
    -- Delete existing data if needed (optional - comment out if you want to keep existing data)
    -- DELETE FROM ServicePackages
    -- PRINT '   Cleared existing data'
    
    -- Insert default packages if not exists
    IF NOT EXISTS (SELECT * FROM ServicePackages WHERE Id = 1)
    BEGIN
        SET IDENTITY_INSERT ServicePackages ON;
        
        INSERT INTO ServicePackages (
            Id, Name, Description, Price, OriginalPrice, DurationDays, 
            Features, IsPopular, IsActive, BadgeText, BadgeColor, 
            DisplayOrder, CreatedAt
        )
        VALUES (
            1, 
            N'Gói Miễn Phí', 
            N'Hoàn hảo để bắt đầu quản lý tài chính cá nhân', 
            0, 
            NULL, 
            365, 
            N'["Quản lý 3 tài khoản", "100 giao dịch/tháng", "Báo cáo cơ bản", "Hỗ trợ email"]', 
            0, 
            1, 
            NULL, 
            NULL, 
            1, 
            GETUTCDATE()
        );
        
        PRINT '   ✓ Inserted: Gói Miễn Phí (ID: 1)'
        
        SET IDENTITY_INSERT ServicePackages OFF;
    END
    ELSE
    BEGIN
        PRINT '   → Package ID 1 already exists'
    END
    
    IF NOT EXISTS (SELECT * FROM ServicePackages WHERE Id = 2)
    BEGIN
        SET IDENTITY_INSERT ServicePackages ON;
        
        INSERT INTO ServicePackages (
            Id, Name, Description, Price, OriginalPrice, DurationDays, 
            Features, IsPopular, IsActive, BadgeText, BadgeColor, 
            DisplayOrder, CreatedAt
        )
        VALUES (
            2, 
            N'Gói Cơ Bản', 
            N'Dành cho người dùng cá nhân muốn quản lý tốt hơn', 
            99000, 
            149000, 
            30, 
            N'["Quản lý 10 tài khoản", "Giao dịch không giới hạn", "Báo cáo chi tiết", "Ngân sách & Mục tiêu", "Hỗ trợ ưu tiên"]', 
            1, 
            1, 
            N'Phổ biến', 
            N'primary', 
            2, 
            GETUTCDATE()
        );
        
        PRINT '   ✓ Inserted: Gói Cơ Bản (ID: 2)'
        
        SET IDENTITY_INSERT ServicePackages OFF;
    END
    ELSE
    BEGIN
        PRINT '   → Package ID 2 already exists'
    END
    
    IF NOT EXISTS (SELECT * FROM ServicePackages WHERE Id = 3)
    BEGIN
        SET IDENTITY_INSERT ServicePackages ON;
        
        INSERT INTO ServicePackages (
            Id, Name, Description, Price, OriginalPrice, DurationDays, 
            Features, IsPopular, IsActive, BadgeText, BadgeColor, 
            DisplayOrder, CreatedAt
        )
        VALUES (
            3, 
            N'Gói Chuyên Nghiệp', 
            N'Giải pháp toàn diện cho quản lý tài chính chuyên nghiệp', 
            199000, 
            299000, 
            30, 
            N'["Tài khoản không giới hạn", "Giao dịch không giới hạn", "Báo cáo nâng cao", "AI Advisor", "Quản lý nhóm", "Hỗ trợ 24/7"]', 
            0, 
            1, 
            N'Tốt nhất', 
            N'success', 
            3, 
            GETUTCDATE()
        );
        
        PRINT '   ✓ Inserted: Gói Chuyên Nghiệp (ID: 3)'
        
        SET IDENTITY_INSERT ServicePackages OFF;
    END
    ELSE
    BEGIN
        PRINT '   → Package ID 3 already exists'
    END
    
    IF NOT EXISTS (SELECT * FROM ServicePackages WHERE Id = 4)
    BEGIN
        SET IDENTITY_INSERT ServicePackages ON;
        
        INSERT INTO ServicePackages (
            Id, Name, Description, Price, OriginalPrice, DurationDays, 
            Features, IsPopular, IsActive, BadgeText, BadgeColor, 
            DisplayOrder, CreatedAt
        )
        VALUES (
            4, 
            N'Gói Doanh Nghiệp', 
            N'Giải pháp cho doanh nghiệp và nhóm làm việc', 
            499000, 
            699000, 
            30, 
            N'["Mọi tính năng Pro", "Quản lý đa người dùng", "API tích hợp", "Báo cáo tùy chỉnh", "Đào tạo & Tư vấn", "Account Manager riêng"]', 
            0, 
            1, 
            N'Ưu đãi nhất', 
            N'popular', 
            4, 
            GETUTCDATE()
        );
        
        PRINT '   ✓ Inserted: Gói Doanh Nghiệp (ID: 4)'
        
        SET IDENTITY_INSERT ServicePackages OFF;
    END
    ELSE
    BEGIN
        PRINT '   → Package ID 4 already exists'
    END
    
    PRINT ''
    
    -- Step 4: Verify final state
    PRINT '4. Final verification...'
    SELECT @PackageCount = COUNT(*) FROM ServicePackages
    PRINT '   Total packages: ' + CAST(@PackageCount AS VARCHAR(10))
    
    IF @PackageCount >= 4
    BEGIN
        PRINT '   ✓ All packages are ready!'
    END
    ELSE
    BEGIN
        PRINT '   ⚠ Warning: Expected at least 4 packages, found ' + CAST(@PackageCount AS VARCHAR(10))
    END
    
    PRINT ''
    PRINT '5. Package details:'
    SELECT 
        Id,
        Name,
        Price as 'Price (VND)',
        OriginalPrice as 'Original Price',
        DurationDays as 'Duration (Days)',
        IsActive,
        IsPopular,
        BadgeText,
        DisplayOrder
    FROM ServicePackages
    ORDER BY DisplayOrder
    
    PRINT ''
    PRINT '========================================='
    PRINT 'FIX COMPLETED!'
    PRINT '========================================='
    PRINT ''
    PRINT 'Next steps:'
    PRINT '1. Restart your application (dotnet run)'
    PRINT '2. Test API: http://localhost:5000/test-api.html'
    PRINT '3. Test checkout: http://localhost:5000/subscription/checkout?packageId=2'
    PRINT ''
END
GO
