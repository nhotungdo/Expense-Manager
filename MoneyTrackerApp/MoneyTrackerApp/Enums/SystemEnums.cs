namespace MoneyTrackerApp.Enums
{
    public enum GroupMemberRole
    {
        Member = 1,
        Admin = 2,
        Owner = 3
    }

    public enum SplitMethod
    {
        Equal = 1,
        ByAmount = 2,
        ByPercentage = 3
    }

    public enum ReportType
    {
        CashFlow = 1,
        IncomeExpense = 2,
        CategoryBreakdown = 3,
        MonthlyTrend = 4,
        NetWorth = 5,
        Custom = 6
    }

    public enum ReportFormat
    {
        PDF = 1,
        Excel = 2,
        CSV = 3,
        JSON = 4
    }

    public enum NotificationType
    {
        BudgetAlert = 1,
        DebtReminder = 2,
        SavingsGoalProgress = 3,
        ScheduledTransaction = 4,
        GroupExpense = 5,
        General = 6
    }
}
