# Quick Reference Guide - Money Tracker Services

## 🏦 Account Management

### IAccountService
```csharp
// Get account
var account = await accountService.GetAccountByIdAsync(accountId, userId);

// Get all user accounts
var accounts = await accountService.GetUserAccountsAsync(userId, includeInactive: false);

// Create account
var createDto = new CreateAccountDto { Name = "Cash", AccountType = 0, InitialBalance = 1000, Currency = "VND" };
var newAccount = await accountService.CreateAccountAsync(userId, createDto);

// Update account
var updateDto = new UpdateAccountDto { Id = accountId, Name = "Updated Name" };
var updated = await accountService.UpdateAccountAsync(userId, updateDto);

// Adjust balance
var adjustDto = new AdjustAccountBalanceDto { AccountId = accountId, Amount = 100, Reason = "Correction" };
await accountService.AdjustBalanceAsync(userId, adjustDto);

// Delete account
await accountService.DeleteAccountAsync(accountId, userId);
```

---

## 🤝 Shared Account Management

### ISharedAccountService
```csharp
// Share account with user
var shareDto = new ShareAccountDto { AccountId = accountId, UserId = targetUserId, Permission = 2 };
var shared = await sharedAccountService.ShareAccountAsync(userId, shareDto);

// Get accounts shared with me
var sharedAccounts = await sharedAccountService.GetSharedAccountsForUserAsync(userId);

// Update permission
await sharedAccountService.UpdatePermissionAsync(userId, sharedAccountId, newPermission: 1);

// Revoke access
await sharedAccountService.RevokeAccessAsync(userId, sharedAccountId);

// Check access
var canAccess = await sharedAccountService.CanAccessAccountAsync(accountId, userId);
var permission = await sharedAccountService.GetPermissionLevelAsync(accountId, userId);
```

**Permission Levels:**
- `0` = View only
- `1` = Add transactions
- `2` = Full access

---

## 🏛️ Bank Connection Management

### IBankConnectionService
```csharp
// Link bank account
var linkDto = new LinkBankAccountDto 
{ 
    AccountId = accountId, 
    Provider = "Plaid", 
    AccessToken = "token", 
    ItemId = "item123" 
};
var connection = await bankConnectionService.LinkBankAccountAsync(userId, linkDto);

// Get all connections
var connections = await bankConnectionService.GetUserBankConnectionsAsync(userId);

// Update connection (refresh token)
await bankConnectionService.UpdateBankConnectionAsync(connectionId, userId, newAccessToken);

// Unlink bank
await bankConnectionService.UnlinkBankAccountAsync(connectionId, userId);

// Update sync status
await bankConnectionService.UpdateSyncStatusAsync(connectionId, "Active");
await bankConnectionService.UpdateLastSyncAsync(connectionId);
```

---

## 💰 Net Worth Calculation

### INetWorthService
```csharp
// Calculate complete net worth
var netWorth = await netWorthService.CalculateNetWorthAsync(userId, includeHidden: false);
// Returns: TotalAssets, TotalDebt, NetWorth, ByAccountType, ByCurrency

// Get total assets
var totalAssets = await netWorthService.GetTotalAssetsAsync(userId);

// Get total debt
var totalDebt = await netWorthService.GetTotalDebtAsync(userId);

// Get breakdown by currency
var byCurrency = await netWorthService.GetNetWorthByCurrencyAsync(userId);

// Get breakdown by account type
var byType = await netWorthService.GetNetWorthByTypeAsync(userId);
```

---

## 💸 Transaction Management

### ITransactionService
```csharp
// Create transaction
var createDto = new CreateTransactionDto
{
    AccountId = accountId,
    CategoryId = categoryId,
    TransactionType = 2, // 1=Income, 2=Expense, 3=Transfer
    Amount = 50000,
    Currency = "VND",
    Note = "Lunch",
    TransactionDate = DateTime.UtcNow
};
var transaction = await transactionService.CreateTransactionAsync(userId, createDto);

// Create transfer (automatically creates paired transaction)
var transferDto = new CreateTransactionDto
{
    AccountId = fromAccountId,
    PairedAccountId = toAccountId,
    TransactionType = 3,
    Amount = 100000,
    Currency = "VND",
    TransactionDate = DateTime.UtcNow
};
var transfer = await transactionService.CreateTransactionAsync(userId, transferDto);

// Get transactions with filter
var filter = new TransactionFilterDto
{
    AccountId = accountId,
    TransactionType = 2,
    StartDate = DateTime.UtcNow.AddMonths(-1),
    EndDate = DateTime.UtcNow,
    PageNumber = 1,
    PageSize = 20
};
var transactions = await transactionService.GetUserTransactionsAsync(userId, filter);

// Update transaction
var updateDto = new UpdateTransactionDto { Id = transactionId, Amount = 60000, Note = "Updated" };
await transactionService.UpdateTransactionAsync(userId, updateDto);

// Delete transaction
await transactionService.DeleteTransactionAsync(transactionId, userId);

// Get recent transactions
var recent = await transactionService.GetRecentTransactionsAsync(userId, count: 10);
```

---

## 📁 Category Management

### ICategoryService
```csharp
// Initialize default categories for new user
await categoryService.InitializeDefaultCategoriesAsync(userId);

// Create category
var createDto = new CreateCategoryDto
{
    Name = "Groceries",
    Type = 2, // 1=Income, 2=Expense
    Icon = "🛒",
    Color = "#FF5722",
    ParentCategoryId = null // or parent category ID for subcategory
};
var category = await categoryService.CreateCategoryAsync(userId, createDto);

// Get all categories (flat list)
var categories = await categoryService.GetUserCategoriesAsync(userId, type: 2);

// Get category tree (hierarchical)
var tree = await categoryService.GetCategoryTreeAsync(userId, type: null);

// Get category summaries (for dropdowns)
var summaries = await categoryService.GetCategorySummariesAsync(userId, type: 2);

// Update category
var updateDto = new UpdateCategoryDto { Id = categoryId, Name = "Updated", Icon = "🍕" };
await categoryService.UpdateCategoryAsync(userId, updateDto);

// Deactivate category (soft delete)
await categoryService.DeactivateCategoryAsync(categoryId, userId);

// Delete category (only if no transactions)
await categoryService.DeleteCategoryAsync(categoryId, userId);
```

---

## 🔄 Scheduled/Recurring Transactions

### IScheduledTransactionService
```csharp
// Create scheduled transaction
var createDto = new CreateScheduledTransactionDto
{
    AccountId = accountId,
    CategoryId = categoryId,
    TransactionType = 2,
    Amount = 500000,
    Frequency = "Monthly", // Daily, Weekly, Monthly, Yearly
    Interval = 1,
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
    EndDate = null, // or specific end date
    Note = "Netflix Subscription"
};
var scheduled = await scheduledTransactionService.CreateScheduledTransactionAsync(userId, createDto);

// Get all scheduled transactions
var scheduledList = await scheduledTransactionService.GetUserScheduledTransactionsAsync(userId, activeOnly: true);

// Update scheduled transaction
var updateDto = new UpdateScheduledTransactionDto { Id = scheduledId, Amount = 550000 };
await scheduledTransactionService.UpdateScheduledTransactionAsync(userId, updateDto);

// Toggle active/inactive
await scheduledTransactionService.ToggleScheduledTransactionAsync(scheduledId, userId, isActive: false);

// Get due transactions (for background job)
var dueTransactions = await scheduledTransactionService.GetDueScheduledTransactionsAsync();

// Execute scheduled transaction (creates actual transaction)
await scheduledTransactionService.ExecuteScheduledTransactionAsync(scheduledId);

// Delete scheduled transaction
await scheduledTransactionService.DeleteScheduledTransactionAsync(scheduledId, userId);
```

---

## 📸 OCR Receipt Scanning

### IOcrService
```csharp
// Process receipt image
var imageBase64 = "data:image/jpeg;base64,...";
var ocrResult = await ocrService.ProcessReceiptAsync(imageBase64);
// Returns: RawText, MerchantName, Amount, Date, Confidence

// Save OCR text to database
var ocrId = await ocrService.SaveOcrTextAsync(transactionId, ocrResult);

// Convert OCR result to transaction DTO
var transactionDto = ocrResult.ToCreateTransactionDto(accountId, categoryId);
var transaction = await transactionService.CreateTransactionAsync(userId, transactionDto);
```

**Note:** Current implementation is a placeholder. For production:
- Integrate Azure Computer Vision API
- Or Google Cloud Vision API
- Or Tesseract OCR library

---

## 🎯 Enums Reference

### AccountType
```csharp
Cash = 0
Bank = 1
EWallet = 2
CreditCard = 3
Savings = 4
Investment = 5
```

### TransactionType
```csharp
Income = 1
Expense = 2
Transfer = 3
```

### SharedAccountPermission
```csharp
View = 0
AddTransaction = 1
FullAccess = 2
```

---

## 📊 Common Workflows

### Complete Transaction Flow with OCR
```csharp
// 1. Scan receipt
var ocrResult = await ocrService.ProcessReceiptAsync(imageBase64);

// 2. Create transaction from OCR
var transactionDto = new CreateTransactionDto
{
    AccountId = accountId,
    CategoryId = categoryId,
    TransactionType = 2,
    Amount = ocrResult.Amount ?? 0,
    Currency = "VND",
    Note = ocrResult.MerchantName,
    TransactionDate = ocrResult.Date ?? DateTime.UtcNow,
    OcrText = ocrResult.RawText
};
var transaction = await transactionService.CreateTransactionAsync(userId, transactionDto);

// 3. Save OCR data
await ocrService.SaveOcrTextAsync(transaction.Id, ocrResult);
```

### Setup New User
```csharp
// 1. Create default categories
await categoryService.InitializeDefaultCategoriesAsync(userId);

// 2. Create initial accounts
var cashAccount = await accountService.CreateAccountAsync(userId, new CreateAccountDto
{
    Name = "Cash",
    AccountType = 0,
    InitialBalance = 0,
    Currency = "VND",
    Icon = "💵",
    Color = "#4CAF50"
});

var bankAccount = await accountService.CreateAccountAsync(userId, new CreateAccountDto
{
    Name = "Bank Account",
    AccountType = 1,
    InitialBalance = 0,
    Currency = "VND",
    Icon = "🏦",
    Color = "#2196F3"
});

// 3. Calculate initial net worth
var netWorth = await netWorthService.CalculateNetWorthAsync(userId);
```

### Monthly Subscription Setup
```csharp
// Create recurring transaction for Netflix
var netflix = await scheduledTransactionService.CreateScheduledTransactionAsync(userId, new CreateScheduledTransactionDto
{
    AccountId = bankAccountId,
    CategoryId = entertainmentCategoryId,
    TransactionType = 2,
    Amount = 260000,
    Frequency = "Monthly",
    Interval = 1,
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
    Note = "Netflix Premium"
});

// Background job should call this daily
var dueTransactions = await scheduledTransactionService.GetDueScheduledTransactionsAsync();
foreach (var due in dueTransactions)
{
    await scheduledTransactionService.ExecuteScheduledTransactionAsync(due.Id);
}
```

---

## 🔧 Dependency Injection Setup

All services are registered in `Program.cs`:

```csharp
// Wallet Management
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISharedAccountService, SharedAccountService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<INetWorthService, NetWorthService>();

// Transaction Management
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IScheduledTransactionService, ScheduledTransactionService>();
builder.Services.AddScoped<IOcrService, OcrService>();
```

---

## 📝 Notes

- All services use `long userId` for user isolation
- All monetary amounts use `decimal` type
- Dates use `DateTime` for timestamps, `DateOnly` for scheduled transactions
- All services return DTOs, not entity models
- Transfer transactions automatically create paired transactions
- Scheduled transactions calculate next run date automatically
- Categories support unlimited nesting levels
- OCR service is ready for integration with actual OCR APIs
