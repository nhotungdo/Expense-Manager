-- Check your subscription status and package features
-- Run this in SQL Server Management Studio or Azure Data Studio

-- 1. Check all service packages and their features
SELECT 
    Id,
    Name,
    PackageType,
    Price,
    MaxAccounts,
    HasAdvancedReports,
    HasAiAdvisor,
    HasGroupExpense,
    HasPrioritySupport,
    IsActive,
    Features
FROM ServicePackages
ORDER BY DisplayOrder;

-- 2. Check your active subscriptions
SELECT 
    s.Id AS SubscriptionId,
    s.UserId,
    u.Email,
    u.FullName,
    s.PackageId,
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
    s.AutoRenew,
    sp.HasAdvancedReports,
    sp.HasAiAdvisor,
    sp.HasGroupExpense,
    sp.MaxAccounts
FROM Subscriptions s
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN ServicePackages sp ON s.PackageId = sp.Id
WHERE s.Status = 1 -- Active subscriptions
ORDER BY s.CreatedAt DESC;

-- 3. Check all your subscriptions (including inactive)
SELECT 
    s.Id AS SubscriptionId,
    s.UserId,
    u.Email,
    s.PackageId,
    sp.Name AS PackageName,
    s.Status,
    s.StartDate,
    s.EndDate,
    s.CreatedAt
FROM Subscriptions s
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN ServicePackages sp ON s.PackageId = sp.Id
ORDER BY s.CreatedAt DESC;

-- 4. Check payment history
SELECT 
    p.Id AS PaymentId,
    p.SubscriptionId,
    s.UserId,
    u.Email,
    p.Amount,
    p.Status,
    CASE p.Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Completed'
        WHEN 2 THEN 'Failed'
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'Refunded'
        ELSE 'Unknown'
    END AS StatusName,
    p.PaymentMethod,
    p.TransactionId,
    p.PaidAt,
    p.CreatedAt
FROM Payments p
INNER JOIN Subscriptions s ON p.SubscriptionId = s.Id
INNER JOIN Users u ON s.UserId = u.Id
ORDER BY p.CreatedAt DESC;

-- 5. Find Professional package details
SELECT 
    Id,
    Name,
    PackageType,
    Price,
    DurationDays,
    MaxAccounts,
    HasAdvancedReports,
    HasAiAdvisor,
    HasGroupExpense,
    HasPrioritySupport,
    Features
FROM ServicePackages
WHERE Name LIKE '%Pro%' OR PackageType = 1;
