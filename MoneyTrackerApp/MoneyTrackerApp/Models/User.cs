using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class User
{
    public long Id { get; set; }

    public string GoogleId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool OnboardingCompleted { get; set; }

    public string Role { get; set; } = null!;

    public bool Enabled { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string Language { get; set; } = null!;

    public string DefaultCurrency { get; set; } = null!;

    public string Timezone { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public bool EmailNotifications { get; set; }

    public bool PushNotifications { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<BankConnection> BankConnections { get; set; } = new List<BankConnection>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Debt> Debts { get; set; } = new List<Debt>();

    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();

    public virtual ICollection<FinancialAlert> FinancialAlerts { get; set; } = new List<FinancialAlert>();

    public virtual ICollection<GroupExpense> GroupExpenses { get; set; } = new List<GroupExpense>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<GroupTransactionSplit> GroupTransactionSplits { get; set; } = new List<GroupTransactionSplit>();

    public virtual ICollection<GroupTransaction> GroupTransactions { get; set; } = new List<GroupTransaction>();

    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual OnboardingStatus? OnboardingStatus { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();

    public virtual ICollection<ScheduledTransaction> ScheduledTransactions { get; set; } = new List<ScheduledTransaction>();

    public virtual ICollection<SharedAccount> SharedAccountSharedByUsers { get; set; } = new List<SharedAccount>();

    public virtual ICollection<SharedAccount> SharedAccountUsers { get; set; } = new List<SharedAccount>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();

    public virtual ICollection<Friendship> FriendshipRequesters { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendship> FriendshipReceivers { get; set; } = new List<Friendship>();

    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();


}
