-- Fix Professional Package Features
-- This script will enable all features for the Professional/Pro package

-- Step 1: Check current package configuration
PRINT 'Current Package Configuration:';
SELECT 
    Id,
    Name,
    PackageType,
    CASE PackageType
        WHEN 0 THEN 'Free'
        WHEN 1 THEN 'Pro'
        WHEN 2 THEN 'Team'
        ELSE 'Unknown'
    END AS PackageTypeName,
    Price,
    HasAdvancedReports,
    HasAiAdvisor,
    HasGroupExpense,
    HasPrioritySupport,
    MaxAccounts,
    MaxBudgets,
    MaxTransactions,
    IsActive
FROM ServicePackages
WHERE PackageType = 1 OR Name LIKE '%Pro%';

-- Step 2: Update Professional package to enable all features
PRINT '';
PRINT 'Updating Professional package features...';

UPDATE ServicePackages
SET 
    HasAdvancedReports = 1,
    HasAiAdvisor = 1,
    HasGroupExpense = 1,
    HasPrioritySupport = 1,
    MaxAccounts = 10,
    MaxBudgets = 50,
    MaxTransactions = -1,  -- -1 means unlimited
    UpdatedAt = GETUTCDATE()
WHERE PackageType = 1 OR Name LIKE '%Pro%';

PRINT 'Update completed.';

-- Step 3: Verify the update
PRINT '';
PRINT 'Updated Package Configuration:';
SELECT 
    Id,
    Name,
    PackageType,
    CASE PackageType
        WHEN 0 THEN 'Free'
        WHEN 1 THEN 'Pro'
        WHEN 2 THEN 'Team'
        ELSE 'Unknown'
    END AS PackageTypeName,
    Price,
    HasAdvancedReports,
    HasAiAdvisor,
    HasGroupExpense,
    HasPrioritySupport,
    MaxAccounts,
    MaxBudgets,
    MaxTransactions,
    IsActive,
    UpdatedAt
FROM ServicePackages
WHERE PackageType = 1 OR Name LIKE '%Pro%';

-- Step 4: Check active subscriptions using this package
PRINT '';
PRINT 'Active Subscriptions using Professional package:';
SELECT 
    s.Id AS SubscriptionId,
    u.Email,
    u.FullName,
    sp.Name AS PackageName,
    s.Status,
    CASE s.Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Expired'
        WHEN 3 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS StatusName,
    s.StartDate,
    s.EndDate,
    sp.HasAdvancedReports,
    sp.HasAiAdvisor,
    sp.HasGroupExpense,
    sp.MaxAccounts
FROM Subscriptions s
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN ServicePackages sp ON s.PackageId = sp.Id
WHERE (sp.PackageType = 1 OR sp.Name LIKE '%Pro%')
  AND s.Status = 1  -- Active
ORDER BY s.CreatedAt DESC;

-- Step 5: If you want to update ALL packages to have proper features based on their type
PRINT '';
PRINT 'Updating all packages with appropriate features...';

-- Free Package (PackageType = 0)
UPDATE ServicePackages
SET 
    HasAdvancedReports = 0,
    HasAiAdvisor = 0,
    HasGroupExpense = 0,
    HasPrioritySupport = 0,
    MaxAccounts = 3,
    MaxBudgets = 10,
    MaxTransactions = 100,
    UpdatedAt = GETUTCDATE()
WHERE PackageType = 0;

-- Pro Package (PackageType = 1)
UPDATE ServicePackages
SET 
    HasAdvancedReports = 1,
    HasAiAdvisor = 1,
    HasGroupExpense = 1,
    HasPrioritySupport = 1,
    MaxAccounts = 10,
    MaxBudgets = 50,
    MaxTransactions = -1,
    UpdatedAt = GETUTCDATE()
WHERE PackageType = 1;

-- Team Package (PackageType = 2)
UPDATE ServicePackages
SET 
    HasAdvancedReports = 1,
    HasAiAdvisor = 1,
    HasGroupExpense = 1,
    HasPrioritySupport = 1,
    MaxAccounts = -1,  -- Unlimited
    MaxBudgets = -1,   -- Unlimited
    MaxTransactions = -1,  -- Unlimited
    UpdatedAt = GETUTCDATE()
WHERE PackageType = 2;

PRINT 'All packages updated.';

-- Step 6: Final verification - show all packages
PRINT '';
PRINT 'All Package Configurations:';
SELECT 
    Id,
    Name,
    PackageType,
    CASE PackageType
        WHEN 0 THEN 'Free'
        WHEN 1 THEN 'Pro'
        WHEN 2 THEN 'Team'
        ELSE 'Unknown'
    END AS PackageTypeName,
    Price,
    DurationDays,
    HasAdvancedReports AS AdvReports,
    HasAiAdvisor AS AI,
    HasGroupExpense AS GroupExp,
    HasPrioritySupport AS Support,
    MaxAccounts AS Accounts,
    MaxBudgets AS Budgets,
    MaxTransactions AS Transactions,
    IsActive,
    IsPopular
FROM ServicePackages
ORDER BY DisplayOrder;
