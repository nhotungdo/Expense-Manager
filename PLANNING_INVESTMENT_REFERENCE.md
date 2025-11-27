# Quick Reference - Planning & Investment Services

## 💰 Budget Management

### IBudgetService
```csharp
// Create budget for category
var budgetDto = new CreateBudgetDto
{
    CategoryId = foodCategoryId,
    Amount = 5000000, // 5M VND per month
    Period = 3, // Monthly
    StartDate = new DateTime(2025, 11, 1),
    EndDate = new DateTime(2025, 11, 30)
};
var budget = await budgetService.CreateBudgetAsync(userId, budgetDto);

// Create budget for account
var accountBudgetDto = new CreateBudgetDto
{
    AccountId = cashAccountId,
    Amount = 10000000, // 10M VND per month
    Period = 3, // Monthly
    StartDate = new DateTime(2025, 11, 1),
    EndDate = new DateTime(2025, 11, 30)
};
var accountBudget = await budgetService.CreateBudgetAsync(userId, accountBudgetDto);

// Get all budgets
var budgets = await budgetService.GetUserBudgetsAsync(userId);

// Get budget summary
var summary = await budgetService.GetBudgetSummaryAsync(userId);
// Returns: TotalBudgets, OverBudgetCount, NearLimitCount, TotalBudgeted, TotalSpent

// Get budget alerts
var alerts = await budgetService.GetBudgetAlertsAsync(userId);
// Returns alerts for budgets near limit (80%) or over budget

// Update budget
var updateDto = new UpdateBudgetDto { Id = budgetId, Amount = 6000000 };
await budgetService.UpdateBudgetAsync(userId, updateDto);

// Delete budget
await budgetService.DeleteBudgetAsync(budgetId, userId);

// Get spent amount for period
var spent = await budgetService.GetSpentAmountAsync(
    userId, 
    categoryId: foodCategoryId, 
    accountId: null,
    startDate: new DateTime(2025, 11, 1),
    endDate: new DateTime(2025, 11, 30)
);
```

**Budget Periods:**
- `1` = Daily
- `2` = Weekly
- `3` = Monthly
- `4` = Yearly

**Alert Thresholds:**
- Near Limit: 80% of budget used
- Over Budget: Spent > Budget amount

---

## 🎯 Savings Goal Management

### ISavingsGoalService
```csharp
// Create savings goal
var goalDto = new CreateSavingsGoalDto
{
    Name = "Buy a Car",
    TargetAmount = 500000000, // 500M VND
    TargetDate = new DateOnly(2026, 12, 31),
    Icon = "🚗",
    Color = "#2196F3"
};
var goal = await savingsGoalService.CreateSavingsGoalAsync(userId, goalDto);

// Get all savings goals
var goals = await savingsGoalService.GetUserSavingsGoalsAsync(userId, activeOnly: true);

// Get savings summary
var summary = await savingsGoalService.GetSavingsSummaryAsync(userId);
// Returns: TotalGoals, ActiveGoals, CompletedGoals, TotalTargetAmount, TotalSavedAmount

// Add money to savings goal
var addDto = new AddToSavingsDto
{
    SavingsGoalId = goalId,
    TransactionId = transactionId, // Link to actual transaction
    Amount = 10000000, // 10M VND
    Note = "Monthly savings"
};
await savingsGoalService.AddToSavingsAsync(userId, addDto);

// Update savings goal
var updateDto = new UpdateSavingsGoalDto
{
    Id = goalId,
    TargetAmount = 600000000, // Increase target
    TargetDate = new DateOnly(2027, 6, 30)
};
await savingsGoalService.UpdateSavingsGoalAsync(userId, updateDto);

// Complete a goal manually
await savingsGoalService.CompleteSavingsGoalAsync(goalId, userId);

// Delete savings goal
await savingsGoalService.DeleteSavingsGoalAsync(goalId, userId);
```

**Goal Status:**
- `1` = Active
- `2` = Completed
- `3` = Cancelled

**Progress Tracking:**
- `PercentageCompleted` = (CurrentAmount / TargetAmount) × 100
- `RemainingAmount` = TargetAmount - CurrentAmount
- `DaysRemaining` = TargetDate - Today
- `IsCompleted` = CurrentAmount >= TargetAmount
- `IsOverdue` = Today > TargetDate && !IsCompleted

---

## 💳 Debt Management

### IDebtService
```csharp
// Create debt (I owe them)
var debtDto = new CreateDebtDto
{
    DebtType = 1, // I owe them
    Name = "Personal Loan",
    PersonName = "John Doe",
    InitialAmount = 50000000, // 50M VND
    InterestRate = 5, // 5% per year
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
};
var debt = await debtService.CreateDebtAsync(userId, debtDto);

// Create debt (They owe me)
var receivableDto = new CreateDebtDto
{
    DebtType = 2, // They owe me
    Name = "Loan to Friend",
    PersonName = "Jane Smith",
    InitialAmount = 20000000, // 20M VND
    InterestRate = 0, // No interest
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
};
var receivable = await debtService.CreateDebtAsync(userId, receivableDto);

// Get all debts
var allDebts = await debtService.GetUserDebtsAsync(userId);

// Get debts by type
var iOweThem = await debtService.GetUserDebtsAsync(userId, debtType: 1);
var theyOweMe = await debtService.GetUserDebtsAsync(userId, debtType: 2);

// Get debt summary
var summary = await debtService.GetDebtSummaryAsync(userId);
// Returns: TotalIOwe, TotalTheyOweMe, NetDebt, TotalInterest, IOweThem[], TheyOweMe[]

// Record payment
var paymentDto = new RecordDebtPaymentDto
{
    DebtId = debtId,
    TransactionId = transactionId, // Link to actual transaction
    Amount = 5000000, // 5M VND
    PaymentDate = DateTime.UtcNow,
    Note = "First installment"
};
await debtService.RecordPaymentAsync(userId, paymentDto);

// Calculate interest
var interest = await debtService.CalculateInterestAsync(debtId);
// Uses simple interest: I = P × (R/100) × (Days/365)

// Update debt
var updateDto = new UpdateDebtDto
{
    Id = debtId,
    InterestRate = 6, // Update to 6%
    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(18))
};
await debtService.UpdateDebtAsync(userId, updateDto);

// Delete debt
await debtService.DeleteDebtAsync(debtId, userId);
```

**Debt Types:**
- `1` = I Owe Them
- `2` = They Owe Me

**Debt Status:**
- `1` = Active (no payments)
- `2` = Partially Paid (some payments made)
- `3` = Fully Paid (AmountPaid >= TotalWithInterest)
- `4` = Cancelled

**Interest Calculation:**
```
Interest = Principal × (Rate / 100) × Time(years)
Time = Days Passed / 365
Total = InitialAmount + Interest
Remaining = Total - AmountPaid
```

---

## 📈 Investment Portfolio Management

### IInvestmentService
```csharp
// Create investment
var investmentDto = new CreateInvestmentDto
{
    AccountId = investmentAccountId, // Optional
    Name = "Bitcoin",
    AssetType = "Crypto",
    Quantity = 0.5m,
    PurchasePrice = 1000000000, // 1B VND per BTC
    PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
    CurrentValue = 1200000000 // 1.2B VND per BTC (optional)
};
var investment = await investmentService.CreateInvestmentAsync(userId, investmentDto);

// Create gold investment
var goldDto = new CreateInvestmentDto
{
    Name = "Gold Bar 1kg",
    AssetType = "Gold",
    Quantity = 1,
    PurchasePrice = 60000000, // 60M VND
    PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
    CurrentValue = 65000000 // 65M VND
};
var gold = await investmentService.CreateInvestmentAsync(userId, goldDto);

// Get all investments
var investments = await investmentService.GetUserInvestmentsAsync(userId);

// Get investments by asset type
var cryptos = await investmentService.GetUserInvestmentsAsync(userId, assetType: "Crypto");
var stocks = await investmentService.GetUserInvestmentsAsync(userId, assetType: "Stock");

// Get portfolio summary
var portfolio = await investmentService.GetPortfolioSummaryAsync(userId);
// Returns: TotalInvestments, TotalInvested, TotalCurrentValue, TotalProfitLoss, 
//          TotalProfitLossPercentage, IsOverallProfit, ByAssetType[], Investments[]

// Get portfolio breakdown by asset type
var breakdown = await investmentService.GetPortfolioBreakdownAsync(userId);
// Returns breakdown per asset type with counts, totals, P/L, and portfolio %

// Update market price
var priceDto = new UpdateInvestmentPriceDto
{
    Id = investmentId,
    CurrentValue = 1300000000 // Updated price: 1.3B VND per BTC
};
await investmentService.UpdateMarketPriceAsync(userId, priceDto);

// Update investment details
var updateDto = new UpdateInvestmentDto
{
    Id = investmentId,
    Quantity = 0.75m, // Bought more
    CurrentValue = 1250000000
};
await investmentService.UpdateInvestmentAsync(userId, updateDto);

// Delete investment
await investmentService.DeleteInvestmentAsync(investmentId, userId);
```

**Asset Types:**
- Gold
- Stock
- Crypto
- Real Estate
- Bond
- Other

**P/L Calculation:**
```
TotalInvested = Quantity × PurchasePrice
TotalCurrentValue = Quantity × CurrentMarketPrice
ProfitLoss = TotalCurrentValue - TotalInvested
ProfitLossPercentage = (ProfitLoss / TotalInvested) × 100
IsProfit = ProfitLoss >= 0
```

**Portfolio Metrics:**
- Total investments count
- Total invested amount
- Total current value
- Overall P/L (amount and %)
- Breakdown by asset type
- Portfolio percentage per asset type

---

## 🔄 Common Workflows

### Monthly Budget Setup
```csharp
// 1. Create budgets for main categories
var foodBudget = await budgetService.CreateBudgetAsync(userId, new CreateBudgetDto
{
    CategoryId = foodCategoryId,
    Amount = 5000000,
    Period = 3, // Monthly
    StartDate = DateTime.Now.Date,
    EndDate = DateTime.Now.AddMonths(1).Date
});

var transportBudget = await budgetService.CreateBudgetAsync(userId, new CreateBudgetDto
{
    CategoryId = transportCategoryId,
    Amount = 3000000,
    Period = 3,
    StartDate = DateTime.Now.Date,
    EndDate = DateTime.Now.AddMonths(1).Date
});

// 2. Check alerts daily
var alerts = await budgetService.GetBudgetAlertsAsync(userId);
foreach (var alert in alerts)
{
    Console.WriteLine($"{alert.AlertType}: {alert.Message}");
}
```

### Savings Goal Workflow
```csharp
// 1. Create goal
var goal = await savingsGoalService.CreateSavingsGoalAsync(userId, new CreateSavingsGoalDto
{
    Name = "Emergency Fund",
    TargetAmount = 100000000, // 100M VND
    TargetDate = new DateOnly(2026, 12, 31),
    Icon = "🏦",
    Color = "#4CAF50"
});

// 2. Add monthly savings
var transaction = await transactionService.CreateTransactionAsync(userId, new CreateTransactionDto
{
    AccountId = savingsAccountId,
    CategoryId = savingsCategoryId,
    TransactionType = 1, // Income
    Amount = 5000000,
    Currency = "VND",
    TransactionDate = DateTime.UtcNow,
    Note = "Monthly savings"
});

await savingsGoalService.AddToSavingsAsync(userId, new AddToSavingsDto
{
    SavingsGoalId = goal.Id,
    TransactionId = transaction.Id,
    Amount = 5000000,
    Note = "November savings"
});

// 3. Check progress
var updatedGoal = await savingsGoalService.GetSavingsGoalByIdAsync(goal.Id, userId);
Console.WriteLine($"Progress: {updatedGoal.PercentageCompleted:N2}%");
Console.WriteLine($"Remaining: {updatedGoal.RemainingAmount:N0} VND");
```

### Debt Repayment Tracking
```csharp
// 1. Create debt
var debt = await debtService.CreateDebtAsync(userId, new CreateDebtDto
{
    DebtType = 1, // I owe them
    Name = "Car Loan",
    PersonName = "ABC Bank",
    InitialAmount = 300000000, // 300M VND
    InterestRate = 8, // 8% per year
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
    DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5))
});

// 2. Record monthly payments
for (int month = 1; month <= 12; month++)
{
    var transaction = await transactionService.CreateTransactionAsync(userId, new CreateTransactionDto
    {
        AccountId = bankAccountId,
        CategoryId = debtPaymentCategoryId,
        TransactionType = 2, // Expense
        Amount = 6000000, // 6M VND per month
        Currency = "VND",
        TransactionDate = DateTime.UtcNow.AddMonths(month),
        Note = $"Car loan payment - Month {month}"
    });

    await debtService.RecordPaymentAsync(userId, new RecordDebtPaymentDto
    {
        DebtId = debt.Id,
        TransactionId = transaction.Id,
        Amount = 6000000,
        PaymentDate = DateTime.UtcNow.AddMonths(month),
        Note = $"Month {month} payment"
    });
}

// 3. Check status
var updatedDebt = await debtService.GetDebtByIdAsync(debt.Id, userId);
Console.WriteLine($"Paid: {updatedDebt.PercentagePaid:N2}%");
Console.WriteLine($"Remaining: {updatedDebt.RemainingAmount:N0} VND");
Console.WriteLine($"Interest: {updatedDebt.InterestAmount:N0} VND");
```

### Investment Portfolio Management
```csharp
// 1. Create diverse portfolio
var bitcoin = await investmentService.CreateInvestmentAsync(userId, new CreateInvestmentDto
{
    Name = "Bitcoin", AssetType = "Crypto", Quantity = 0.5m,
    PurchasePrice = 1000000000, PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
});

var gold = await investmentService.CreateInvestmentAsync(userId, new CreateInvestmentDto
{
    Name = "Gold", AssetType = "Gold", Quantity = 10,
    PurchasePrice = 6000000, PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
});

var stock = await investmentService.CreateInvestmentAsync(userId, new CreateInvestmentDto
{
    Name = "VNM Stock", AssetType = "Stock", Quantity = 1000,
    PurchasePrice = 80000, PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
});

// 2. Update prices daily/weekly
await investmentService.UpdateMarketPriceAsync(userId, new UpdateInvestmentPriceDto
{
    Id = bitcoin.Id,
    CurrentValue = 1200000000 // +20% profit
});

await investmentService.UpdateMarketPriceAsync(userId, new UpdateInvestmentPriceDto
{
    Id = gold.Id,
    CurrentValue = 6500000 // +8.3% profit
});

// 3. Check portfolio performance
var portfolio = await investmentService.GetPortfolioSummaryAsync(userId);
Console.WriteLine($"Total Invested: {portfolio.TotalInvested:N0} VND");
Console.WriteLine($"Current Value: {portfolio.TotalCurrentValue:N0} VND");
Console.WriteLine($"P/L: {portfolio.TotalProfitLoss:N0} VND ({portfolio.TotalProfitLossPercentage:N2}%)");

var breakdown = portfolio.ByAssetType;
foreach (var asset in breakdown)
{
    Console.WriteLine($"{asset.AssetType}: {asset.PortfolioPercentage:N2}% of portfolio");
}
```

---

## 📊 Dashboard Queries

### Financial Overview
```csharp
// Get complete financial picture
var netWorth = await netWorthService.CalculateNetWorthAsync(userId);
var budgetSummary = await budgetService.GetBudgetSummaryAsync(userId);
var savingsSummary = await savingsGoalService.GetSavingsSummaryAsync(userId);
var debtSummary = await debtService.GetDebtSummaryAsync(userId);
var portfolio = await investmentService.GetPortfolioSummaryAsync(userId);

var dashboard = new
{
    NetWorth = netWorth.NetWorth,
    TotalAssets = netWorth.TotalAssets,
    TotalDebt = netWorth.TotalDebt,
    
    BudgetStatus = new
    {
        TotalBudgeted = budgetSummary.TotalBudgeted,
        TotalSpent = budgetSummary.TotalSpent,
        OverBudgetCount = budgetSummary.OverBudgetCount
    },
    
    SavingsProgress = new
    {
        TotalGoals = savingsSummary.TotalGoals,
        TotalSaved = savingsSummary.TotalSavedAmount,
        OverallProgress = savingsSummary.OverallPercentage
    },
    
    DebtPosition = new
    {
        NetDebt = debtSummary.NetDebt,
        TotalIOwe = debtSummary.TotalIOwe,
        TotalTheyOweMe = debtSummary.TotalTheyOweMe
    },
    
    InvestmentPerformance = new
    {
        TotalInvested = portfolio.TotalInvested,
        CurrentValue = portfolio.TotalCurrentValue,
        ProfitLoss = portfolio.TotalProfitLoss,
        ProfitLossPercentage = portfolio.TotalProfitLossPercentage
    }
};
```

---

## 🔧 Dependency Injection

All services registered in `Program.cs`:
```csharp
// Planning Services
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ISavingsGoalService, SavingsGoalService>();

// Debt & Investment Services
builder.Services.AddScoped<IDebtService, DebtService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
```

---

## 📝 Notes

- All monetary amounts use `decimal` type for precision
- Dates use `DateTime` for timestamps, `DateOnly` for dates without time
- All services return DTOs, not entity models
- Budget alerts automatically calculated on each request
- Savings goals auto-complete when target reached
- Debt interest calculated using simple interest formula
- Investment P/L recalculated when market price updated
- All services use `long userId` for user isolation and security
