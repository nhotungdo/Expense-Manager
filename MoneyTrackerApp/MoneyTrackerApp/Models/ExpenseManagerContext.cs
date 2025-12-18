using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Models;

public partial class ExpenseManagerContext : DbContext
{
    public ExpenseManagerContext()
    {
    }

    public ExpenseManagerContext(DbContextOptions<ExpenseManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BankConnection> BankConnections { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }

    public virtual DbSet<Debt> Debts { get; set; }

    public virtual DbSet<DebtPayment> DebtPayments { get; set; }

    public virtual DbSet<Email> Emails { get; set; }

    public virtual DbSet<FinancialAlert> FinancialAlerts { get; set; }

    public virtual DbSet<GroupExpense> GroupExpenses { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<GroupTransaction> GroupTransactions { get; set; }

    public virtual DbSet<GroupTransactionSplit> GroupTransactionSplits { get; set; }

    public virtual DbSet<Investment> Investments { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OcrText> OcrTexts { get; set; }

    public virtual DbSet<OnboardingStatus> OnboardingStatuses { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SavingsGoal> SavingsGoals { get; set; }

    public virtual DbSet<SavingsTransaction> SavingsTransactions { get; set; }

    public virtual DbSet<ScheduledTransaction> ScheduledTransactions { get; set; }

    public virtual DbSet<SharedAccount> SharedAccounts { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwCategoryUsageStat> VwCategoryUsageStats { get; set; }

    public virtual DbSet<VwUserTransactionSummary> VwUserTransactionSummaries { get; set; }

    public virtual DbSet<ServicePackage> ServicePackages { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<AiSuggestion> AiSuggestions { get; set; }
    
    public virtual DbSet<UserOtp> UserOtps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=NHOTUNG\\SQLEXPRESS;Database=ExpenseManager;User Id=sa;Password=123;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Accounts_UpdatedAt"));

            entity.HasIndex(e => e.UserId, "IX_Accounts_UserId");

            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.IncludeInTotal).HasDefaultValue(true);
            entity.Property(e => e.InitialBalance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Accounts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.EntityType, "IX_AuditLogs_EntityType");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Details).HasMaxLength(1024);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(512);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BankConnection>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_BankConnections_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ItemId).HasMaxLength(256);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.SyncStatus).HasMaxLength(20);

            entity.HasOne(d => d.Account).WithMany(p => p.BankConnections)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.BankConnections).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Budgets_UpdatedAt"));

            entity.HasIndex(e => e.AccountId, "IX_Budgets_AccountId");

            entity.HasIndex(e => e.CategoryId, "IX_Budgets_CategoryId");

            entity.HasIndex(e => e.UserId, "IX_Budgets_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Account).WithMany(p => p.Budgets).HasForeignKey(d => d.AccountId);

            entity.HasOne(d => d.Category).WithMany(p => p.Budgets).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Budgets).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Categories_UpdatedAt"));

            entity.HasIndex(e => e.ParentCategoryId, "IX_Categories_ParentCategoryId");

            entity.HasIndex(e => e.Type, "IX_Categories_Type");

            entity.HasIndex(e => e.UserId, "IX_Categories_UserId");

            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasForeignKey(d => d.ParentCategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Categories).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.HasIndex(e => new { e.FromCurrency, e.ToCurrency }, "UK_CurrencyRates_From_To").IsUnique();

            entity.Property(e => e.FromCurrency).HasMaxLength(3);
            entity.Property(e => e.Rate).HasColumnType("decimal(18, 9)");
            entity.Property(e => e.ToCurrency).HasMaxLength(3);
        });

        modelBuilder.Entity<Debt>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Debts_UpdatedAt"));

            entity.HasIndex(e => e.UserId, "IX_Debts_UserId");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.InitialAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PersonName).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.User).WithMany(p => p.Debts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<DebtPayment>(entity =>
        {
            entity.HasIndex(e => e.DebtId, "IX_DebtPayments_DebtId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.Debt).WithMany(p => p.DebtPayments).HasForeignKey(d => d.DebtId);

            entity.HasOne(d => d.Transaction).WithMany(p => p.DebtPayments)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasIndex(e => e.Status, "IX_Emails_Status");

            entity.HasIndex(e => e.UserId, "IX_Emails_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(256);

            entity.HasOne(d => d.User).WithMany(p => p.Emails).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<FinancialAlert>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_FinancialAlerts_CreatedAt");

            entity.HasIndex(e => e.IsRead, "IX_FinancialAlerts_IsRead");

            entity.HasIndex(e => e.UserId, "IX_FinancialAlerts_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.FinancialAlerts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<GroupExpense>(entity =>
        {
            entity.HasIndex(e => e.CreatedByUserId, "IX_GroupExpenses_CreatedByUserId");

            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.GroupExpenses)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasIndex(e => e.GroupId, "IX_GroupMembers_GroupId");

            entity.HasIndex(e => e.UserId, "IX_GroupMembers_UserId");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Member");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers).HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<GroupTransaction>(entity =>
        {
            entity.HasIndex(e => e.GroupId, "IX_GroupTransactions_GroupId");

            entity.HasIndex(e => e.PaidByUserId, "IX_GroupTransactions_PaidByUserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Group).WithMany(p => p.GroupTransactions).HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.PaidByUser).WithMany(p => p.GroupTransactions)
                .HasForeignKey(d => d.PaidByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<GroupTransactionSplit>(entity =>
        {
            entity.HasIndex(e => e.GroupTransactionId, "IX_GroupTransactionSplits_GroupTransactionId");

            entity.HasIndex(e => e.UserId, "IX_GroupTransactionSplits_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.GroupTransaction).WithMany(p => p.GroupTransactionSplits).HasForeignKey(d => d.GroupTransactionId);

            entity.HasOne(d => d.User).WithMany(p => p.GroupTransactionSplits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Investment>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Investments_UpdatedAt"));

            entity.HasIndex(e => e.UserId, "IX_Investments_UserId");

            entity.Property(e => e.AssetType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CurrentValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 8)");

            entity.HasOne(d => d.Account).WithMany(p => p.Investments).HasForeignKey(d => d.AccountId);

            entity.HasOne(d => d.User).WithMany(p => p.Investments).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.IsRead, "IX_Notifications_IsRead");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.ActionUrl).HasMaxLength(512);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Message).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<OcrText>(entity =>
        {
            entity.HasIndex(e => e.TransactionId, "IX_OcrTexts_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.MerchantName).HasMaxLength(256);

            entity.HasOne(d => d.Transaction).WithMany().HasForeignKey(d => d.TransactionId);
        });

        modelBuilder.Entity<OnboardingStatus>(entity =>
        {
            entity.ToTable("OnboardingStatus");

            entity.HasIndex(e => e.CurrentStep, "IX_OnboardingStatus_CurrentStep");

            entity.HasIndex(e => e.IsCompleted, "IX_OnboardingStatus_IsCompleted");

            entity.HasIndex(e => e.UserId, "UK_OnboardingStatus_UserId").IsUnique();

            entity.Property(e => e.CurrentStep).HasDefaultValue(1);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithOne(p => p.OnboardingStatus).HasForeignKey<OnboardingStatus>(d => d.UserId);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Reports_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FileFormat).HasMaxLength(10);
            entity.Property(e => e.FilePath).HasMaxLength(512);
            entity.Property(e => e.ReportName).HasMaxLength(256);
            entity.Property(e => e.ReportType).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_SavingsGoals_UpdatedAt"));

            entity.HasIndex(e => e.UserId, "IX_SavingsGoals_UserId");

            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TargetAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.SavingsGoals).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SavingsTransaction>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_SavingsTransactions_UpdateGoal"));

            entity.HasIndex(e => e.SavingsGoalId, "IX_SavingsTransactions_SavingsGoalId");

            entity.HasIndex(e => e.TransactionId, "IX_SavingsTransactions_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.SavingsGoal).WithMany(p => p.SavingsTransactions).HasForeignKey(d => d.SavingsGoalId);

            entity.HasOne(d => d.Transaction).WithMany(p => p.SavingsTransactions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ScheduledTransaction>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_ScheduledTransactions_UpdatedAt"));

            entity.HasIndex(e => e.NextRunDate, "IX_ScheduledTransactions_NextRunDate");

            entity.HasIndex(e => e.UserId, "IX_ScheduledTransactions_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Frequency).HasMaxLength(20);
            entity.Property(e => e.Interval).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.Account).WithMany(p => p.ScheduledTransactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.ScheduledTransactions).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.ScheduledTransactions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SharedAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountId, "IX_SharedAccounts_AccountId");

            entity.HasIndex(e => e.UserId, "IX_SharedAccounts_UserId");

            entity.HasIndex(e => new { e.AccountId, e.UserId }, "UK_SharedAccounts_AccountId_UserId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Account).WithMany(p => p.SharedAccounts).HasForeignKey(d => d.AccountId);

            entity.HasOne(d => d.SharedByUser).WithMany(p => p.SharedAccountSharedByUsers)
                .HasForeignKey(d => d.SharedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.SharedAccountUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_SystemSettings_UpdatedAt"));

            entity.HasIndex(e => e.SettingKey, "IX_SystemSettings_SettingKey").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingType).HasMaxLength(20);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable(tb =>
                {
                    tb.HasTrigger("tr_Transactions_UpdateAccountBalance");
                    tb.HasTrigger("tr_Transactions_UpdatedAt");
                });

            entity.HasIndex(e => e.AccountId, "IX_Transactions_AccountId");

            entity.HasIndex(e => e.CategoryId, "IX_Transactions_CategoryId");

            entity.HasIndex(e => e.TransactionDate, "IX_Transactions_TransactionDate");

            entity.HasIndex(e => e.TransactionType, "IX_Transactions_TransactionType");

            entity.HasIndex(e => e.UserId, "IX_Transactions_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AttachmentUrl).HasMaxLength(512);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.Account).WithMany(p => p.TransactionAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.PairedAccount).WithMany(p => p.TransactionPairedAccounts).HasForeignKey(d => d.PairedAccountId);

            entity.HasOne(d => d.PairedTransaction).WithMany(p => p.InversePairedTransaction).HasForeignKey(d => d.PairedTransactionId);

            entity.HasOne(d => d.User).WithMany(p => p.Transactions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Users_UpdatedAt"));

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.GoogleId, "IX_Users_GoogleId").IsUnique();

            entity.HasIndex(e => e.UserName, "IX_Users_UserName").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(512);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DefaultCurrency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.FirstName).HasMaxLength(128);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .HasDefaultValue("vi");
            entity.Property(e => e.LastName).HasMaxLength(128);
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(512);
            entity.Property(e => e.PushNotifications).HasDefaultValue(true);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
            entity.Property(e => e.Theme)
                .HasMaxLength(20)
                .HasDefaultValue("light");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .HasDefaultValue("Asia/Ho_Chi_Minh");
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<VwCategoryUsageStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CategoryUsageStats");

            entity.Property(e => e.AverageAmount).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.CategoryColor).HasMaxLength(20);
            entity.Property(e => e.CategoryIcon).HasMaxLength(50);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwUserTransactionSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserTransactionSummary");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.NetIncome).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalExpense).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalIncome).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_ServicePackages_UpdatedAt"));


            entity.HasIndex(e => e.IsActive, "IX_ServicePackages_IsActive");

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Subscriptions_UpdatedAt"));

            entity.HasIndex(e => e.UserId, "IX_Subscriptions_UserId");
            entity.HasIndex(e => e.PackageId, "IX_Subscriptions_PackageId");
            entity.HasIndex(e => e.Status, "IX_Subscriptions_Status");
            entity.HasIndex(e => e.EndDate, "IX_Subscriptions_EndDate");

            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.AutoRenew).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Package).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Payments_UpdatedAt"));

            entity.HasIndex(e => e.SubscriptionId, "IX_Payments_SubscriptionId");
            entity.HasIndex(e => e.Status, "IX_Payments_Status");
            entity.HasIndex(e => e.TransactionId, "IX_Payments_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("VND");
            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TransactionId).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AiSuggestions_UserId");
            entity.HasIndex(e => e.CreatedAt, "IX_AiSuggestions_CreatedAt");

            entity.Property(e => e.SuggestionType).HasMaxLength(50);
            entity.Property(e => e.Suggestion).HasMaxLength(1024);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.ToTable("UserOtps");
            entity.HasIndex(e => e.UserId, "IX_UserOtps_UserId");
            entity.Property(e => e.OtpCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            
            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
