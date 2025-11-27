namespace MoneyTrackerApp.Enums
{
    public enum BudgetPeriod
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }

    public enum DebtType
    {
        IOweThem = 1,      // Money I owe to others
        TheyOweMe = 2      // Money others owe to me
    }

    public enum DebtStatus
    {
        Active = 1,
        PartiallyPaid = 2,
        FullyPaid = 3,
        Cancelled = 4
    }

    public enum SavingsGoalStatus
    {
        Active = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum AssetType
    {
        Gold = 1,
        Stock = 2,
        Crypto = 3,
        RealEstate = 4,
        Bond = 5,
        Other = 6
    }
}
