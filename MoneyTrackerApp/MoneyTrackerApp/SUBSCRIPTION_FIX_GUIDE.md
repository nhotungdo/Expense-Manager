# Subscription System - Quick Fix Guide

## Problem
**Error**: "❌ Không thể tải gói dịch vụ: HTTP 500: Internal Server Error"

**Cause**: The ServicePackages table was empty - no service packages existed in the database.

## Solution ✅

The database migration has been executed successfully!

### What Was Done:

1. **Created Tables**:
   - `ServicePackages` - Stores service package information
   - `Subscriptions` - Tracks user subscriptions
   - `Payments` - Records payment transactions

2. **Created Triggers**:
   - `tr_ServicePackages_UpdatedAt`
   - `tr_Subscriptions_UpdatedAt`
   - `tr_Payments_UpdatedAt`

3. **Inserted Default Service Packages**:
   - **Free** (Miễn phí) - 0 VND/month
     - Max 100 transactions
     - Max 3 accounts
     - Max 3 budgets
     - Basic features
   
   - **Pro** - 79,000 VND/month
     - Unlimited transactions
     - Unlimited accounts
     - Unlimited budgets
     - Advanced reports
     - AI advisor
   
   - **Team** - 199,000 VND/month
     - All Pro features
     - Group expense tracking
     - Priority support
     - Shared accounts

## Verification

Run this SQL query to verify the packages:

```sql
SELECT Id, Name, PackageType, Price, IsActive 
FROM ServicePackages
```

Expected result: 3 rows (Free, Pro, Team)

## Testing the Fix

1. **Start the application**:
   ```bash
   dotnet run
   ```

2. **Navigate to the subscription page**:
   ```
   http://localhost:5000/Subscription
   ```

3. **Expected Result**:
   - You should see 3 beautiful package cards
   - Free, Pro (marked as "Phổ biến nhất"), and Team
   - Each with their features and pricing
   - Click on any package to test the flow

## API Endpoints

Test the API directly:

```bash
# Get all packages
curl http://localhost:5000/api/Subscription/packages

# Get specific package
curl http://localhost:5000/api/Subscription/packages/1

# Check subscription (requires auth)
curl http://localhost:5000/api/Subscription/my-subscription
```

## If You Still See Errors

### 1. Check Database Connection
```sql
-- Verify tables exist
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('ServicePackages', 'Subscriptions', 'Payments')
```

### 2. Check Service Registration
Verify in `Program.cs`:
```csharp
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
```

### 3. Check Controller Registration
The `SubscriptionController` should be automatically discovered by ASP.NET Core.

### 4. Restart the Application
Sometimes you need to restart the application after database changes:
```bash
# Stop the app (Ctrl+C)
dotnet clean
dotnet build
dotnet run
```

### 5. Check Browser Console
Open Developer Tools (F12) and check:
- Network tab for API response
- Console tab for JavaScript errors

## Manual Migration (If Needed)

If you need to run the migration manually:

1. Open SQL Server Management Studio
2. Connect to: `NHOTUNG\SQLEXPRESS`
3. Select database: `ExpenseManager`
4. Open file: `Migrations/AddSubscriptionTables.sql`
5. Execute (F5)

## Adding More Packages

To add custom packages:

```sql
INSERT INTO ServicePackages 
(Name, Description, PackageType, Price, BillingCycle, Features, 
 MaxTransactions, MaxAccounts, MaxBudgets, 
 HasAdvancedReports, HasAiAdvisor, HasGroupExpense, HasPrioritySupport, 
 IsActive, DisplayOrder)
VALUES 
(
    N'Enterprise',
    N'Gói dành cho doanh nghiệp lớn',
    3, -- Custom type
    499000,
    1, -- Monthly
    N'["Tất cả tính năng Team", "API Access", "Dedicated Support"]',
    -1, -1, -1,
    1, 1, 1, 1,
    1, 4
);
```

## Troubleshooting

### Error: "Cannot connect to database"
- Check SQL Server is running
- Verify connection string in `appsettings.json`
- Check Windows Authentication is enabled

### Error: "Table already exists"
- The migration script has `IF NOT EXISTS` checks
- Safe to run multiple times
- Will only insert packages if they don't exist

### Error: "Foreign key constraint"
- Ensure Users table exists
- Check database schema is up to date

## Success Indicators

✅ **Database**: 3 packages in ServicePackages table  
✅ **API**: `/api/Subscription/packages` returns 3 packages  
✅ **UI**: Subscription page displays 3 package cards  
✅ **No Errors**: No 500 errors in browser console  

## Next Steps

1. Test package selection flow
2. Test authentication redirect
3. Test payment page (requires VNPay configuration)
4. Review the enhanced features in the implementation

## Support Files

- **Migration Script**: `Migrations/AddSubscriptionTables.sql`
- **Setup Script**: `setup-subscription-db.ps1`
- **Documentation**: `SUBSCRIPTION_ENHANCED_README.md`
- **Implementation Summary**: `IMPLEMENTATION_SUMMARY.md`

---

**Status**: ✅ **FIXED** - Service packages loaded successfully!

Last Updated: 2025-12-03 14:45
