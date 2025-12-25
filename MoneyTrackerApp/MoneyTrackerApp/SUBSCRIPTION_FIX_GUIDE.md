# Subscription Feature Access Issue - Diagnostic & Fix Guide

## Problem Summary
You have registered for the Professional package but cannot access the following features:
- All Basic Features
- AI Expense Analysis
- Financial Forecasting
- Investment Management
- Debt & Savings Tracking
- Group Expense Sharing
- Bank Integration
- 24/7 Support
- Personal Financial Advice

## Root Cause
The most likely cause is that the Professional package in your database doesn't have the feature flags properly enabled. The subscription system checks these flags in the `ServicePackages` table to determine what features a user can access.

## Solution Steps

### Option 1: Use the Diagnostic Web Tool (Recommended)

1. **Start your application:**
   ```powershell
   cd f:\OJT-Review\MoneyTrackerApp\Expense-Manager\MoneyTrackerApp\MoneyTrackerApp
   dotnet run
   ```

2. **Open the diagnostic tool:**
   - Navigate to: `http://localhost:5000/diagnostic.html`
   - Or: `https://localhost:5001/diagnostic.html`

3. **Check your subscription:**
   - Click "Check My Subscription" button
   - Review the diagnosis results
   - Note any issues found

4. **Get the fix script:**
   - Click "Show SQL Fix Script" button
   - Click "Copy Fix Script" button

5. **Run the fix:**
   - Open SQL Server Management Studio or Azure Data Studio
   - Connect to: `NHOTUNG\SQLEXPRESS`
   - Select database: `ExpenseManager`
   - Paste and execute the script
   - Refresh your application

### Option 2: Use the API Endpoint Directly

1. **Start your application**

2. **Call the diagnostic API:**
   ```
   GET http://localhost:5000/api/Diagnostic/check-subscription
   ```

3. **Review the JSON response** to see:
   - Your current subscription status
   - Enabled/disabled features
   - List of issues
   - All available packages

### Option 3: Run SQL Script Directly

1. **Open SQL Server Management Studio or Azure Data Studio**

2. **Connect to your database:**
   - Server: `NHOTUNG\SQLEXPRESS`
   - Database: `ExpenseManager`
   - Authentication: Windows Authentication (or use sa/123)

3. **Run the diagnostic query:**
   ```sql
   -- Check current Professional package configuration
   SELECT 
       Id, Name, PackageType,
       HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport,
       MaxAccounts, MaxBudgets, MaxTransactions,
       IsActive
   FROM ServicePackages
   WHERE PackageType = 1 OR Name LIKE '%Pro%';
   ```

4. **If features are disabled (showing 0), run the fix:**
   ```sql
   -- Fix Professional package features
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
   WHERE PackageType = 1 OR Name LIKE '%Pro%';
   ```

5. **Verify the fix:**
   ```sql
   -- Check updated configuration
   SELECT 
       Id, Name, PackageType,
       HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport,
       MaxAccounts, MaxBudgets, MaxTransactions,
       UpdatedAt
   FROM ServicePackages
   WHERE PackageType = 1 OR Name LIKE '%Pro%';
   ```

6. **Check your active subscription:**
   ```sql
   -- Verify your subscription has the features
   SELECT 
       s.Id AS SubscriptionId,
       u.Email,
       sp.Name AS PackageName,
       s.Status,
       s.StartDate,
       s.EndDate,
       sp.HasAdvancedReports,
       sp.HasAiAdvisor,
       sp.HasGroupExpense,
       sp.MaxAccounts
   FROM Subscriptions s
   INNER JOIN Users u ON s.UserId = u.Id
   INNER JOIN ServicePackages sp ON s.PackageId = sp.Id
   WHERE s.Status = 1  -- Active
   ORDER BY s.CreatedAt DESC;
   ```

## Understanding the Feature System

### Package Types
- **0 = Free**: Basic features only
- **1 = Pro**: Advanced features enabled
- **2 = Team**: All features + team collaboration

### Feature Flags in ServicePackages Table
- `HasAdvancedReports`: Enables advanced reporting features
- `HasAiAdvisor`: Enables AI-powered financial advice
- `HasGroupExpense`: Enables group expense sharing
- `HasPrioritySupport`: Enables 24/7 priority support
- `MaxAccounts`: Maximum number of accounts (3 for Free, 10 for Pro, unlimited for Team)
- `MaxBudgets`: Maximum number of budgets
- `MaxTransactions`: Maximum transactions (-1 = unlimited)

### How Features are Checked

When you access a feature, the system:
1. Gets your active subscription from the `Subscriptions` table
2. Loads the associated `ServicePackage` 
3. Checks the feature flags in the package
4. Grants or denies access based on these flags

**Example from code:**
```csharp
var activeSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
if (activeSubscription?.HasAdvancedReports == true) {
    // Allow access to advanced reports
}
```

## Expected Values for Professional Package

After running the fix, your Professional package should have:
- `HasAdvancedReports` = 1 (true)
- `HasAiAdvisor` = 1 (true)
- `HasGroupExpense` = 1 (true)
- `HasPrioritySupport` = 1 (true)
- `MaxAccounts` = 10
- `MaxBudgets` = 50
- `MaxTransactions` = -1 (unlimited)

## Verification Steps

After applying the fix:

1. **Restart your application** (if it's running)

2. **Log in to your account**

3. **Check the diagnostic page again:**
   - Navigate to `/diagnostic.html`
   - Click "Check My Subscription"
   - Verify all features show as "Enabled"

4. **Test feature access:**
   - Try accessing Advanced Reports
   - Try using AI Advisor
   - Try creating a Group Expense
   - Verify you can create more than 3 accounts

## Troubleshooting

### Issue: Still can't access features after running the fix

**Check 1: Verify subscription is active**
```sql
SELECT Id, UserId, PackageId, Status, StartDate, EndDate
FROM Subscriptions
WHERE Status = 1  -- Active
ORDER BY CreatedAt DESC;
```
- Status should be 1 (Active)
- EndDate should be in the future

**Check 2: Verify package is linked correctly**
```sql
SELECT s.Id, s.UserId, s.PackageId, sp.Name, sp.HasAdvancedReports
FROM Subscriptions s
INNER JOIN ServicePackages sp ON s.PackageId = sp.Id
WHERE s.Status = 1;
```

**Check 3: Clear browser cache and cookies**
- Press Ctrl+Shift+Delete
- Clear all cached data
- Log in again

**Check 4: Check application logs**
- Look for errors in the console output
- Check for authentication issues

### Issue: Subscription shows as expired

```sql
-- Extend subscription end date
UPDATE Subscriptions
SET EndDate = DATEADD(DAY, 30, GETUTCDATE()),
    UpdatedAt = GETUTCDATE()
WHERE Status = 1 AND UserId = YOUR_USER_ID;
```

### Issue: Multiple active subscriptions

```sql
-- Find duplicate active subscriptions
SELECT UserId, COUNT(*) as ActiveCount
FROM Subscriptions
WHERE Status = 1
GROUP BY UserId
HAVING COUNT(*) > 1;

-- Cancel older subscriptions (keep the latest)
-- Replace YOUR_USER_ID with your actual user ID
UPDATE Subscriptions
SET Status = 3,  -- Cancelled
    CancelledAt = GETUTCDATE(),
    CancellationReason = 'Duplicate subscription cleanup'
WHERE UserId = YOUR_USER_ID
  AND Status = 1
  AND Id NOT IN (
      SELECT TOP 1 Id 
      FROM Subscriptions 
      WHERE UserId = YOUR_USER_ID AND Status = 1
      ORDER BY CreatedAt DESC
  );
```

## Files Created for Diagnosis

1. **DiagnosticController.cs** - API endpoint for checking subscription
   - Location: `Controllers/DiagnosticController.cs`
   - Endpoint: `GET /api/Diagnostic/check-subscription`

2. **diagnostic.html** - Web-based diagnostic tool
   - Location: `wwwroot/diagnostic.html`
   - URL: `http://localhost:5000/diagnostic.html`

3. **CheckSubscription.sql** - Comprehensive SQL diagnostic queries
   - Location: `CheckSubscription.sql`

4. **FixPackageFeatures.sql** - SQL script to fix package features
   - Location: `FixPackageFeatures.sql`

## Contact Support

If you continue to experience issues after following this guide:
1. Run the diagnostic tool and save the output
2. Check the application logs for errors
3. Verify your database connection is working
4. Ensure you're logged in with the correct account

## Quick Reference Commands

**Start Application:**
```powershell
cd f:\OJT-Review\MoneyTrackerApp\Expense-Manager\MoneyTrackerApp\MoneyTrackerApp
dotnet run
```

**Build Application:**
```powershell
dotnet build
```

**Connect to Database:**
- Server: `NHOTUNG\SQLEXPRESS`
- Database: `ExpenseManager`
- User: `sa`
- Password: `123`

**Diagnostic URL:**
- `http://localhost:5000/diagnostic.html`

**API Endpoint:**
- `GET http://localhost:5000/api/Diagnostic/check-subscription`
