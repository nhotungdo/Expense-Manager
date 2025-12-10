-- Quick test database
USE ExpenseManager;
GO

-- Check if ServicePackages table exists
SELECT 
    CASE 
        WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServicePackages]'))
        THEN 'Table EXISTS'
        ELSE 'Table NOT FOUND'
    END as TableStatus;

-- Check data
SELECT COUNT(*) as TotalPackages FROM ServicePackages;

-- Show all packages
SELECT 
    Id,
    Name,
    Price,
    DurationDays,
    IsActive,
    Features
FROM ServicePackages
ORDER BY DisplayOrder;
