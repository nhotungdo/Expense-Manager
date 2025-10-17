using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Models;

public partial class ExpenseManagerContext : DbContext
{
    public ExpenseManagerContext()
    {
    }

    public ExpenseManagerContext(DbContextOptions<ExpenseManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiSuggestion> AiSuggestions { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Email> Emails { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwCategoryUsageStat> VwCategoryUsageStats { get; set; }

    public virtual DbSet<VwUserTransactionSummary> VwUserTransactionSummaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }
        // Fallback if not configured via DI (e.g., design-time tooling). Prefer configuration-based setup.
        optionsBuilder.UseSqlServer("Name=ConnectionStrings:ExpenseManager");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_AiSuggestions_CreatedAt");

            entity.HasIndex(e => e.UserId, "IX_AiSuggestions_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Suggestion).HasMaxLength(1024);
            entity.Property(e => e.SuggestionType)
                .HasMaxLength(50)
                .HasDefaultValue("Financial Advice");

            entity.HasOne(d => d.User).WithMany(p => p.AiSuggestions).HasForeignKey(d => d.UserId);
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
            entity.HasIndex(e => e.Action, "IX_AuditLogs_Action");

            entity.HasIndex(e => e.CreatedAt, "IX_AuditLogs_CreatedAt");

            entity.HasIndex(e => e.EntityId, "IX_AuditLogs_EntityId");

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

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Budgets_UpdatedAt"));

            entity.HasIndex(e => e.CategoryId, "IX_Budgets_CategoryId");

            entity.HasIndex(e => e.EndDate, "IX_Budgets_EndDate");

            entity.HasIndex(e => e.Period, "IX_Budgets_Period");

            entity.HasIndex(e => e.StartDate, "IX_Budgets_StartDate");

            entity.HasIndex(e => e.UserId, "IX_Budgets_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Budgets).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Budgets).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Categories_UpdatedAt"));

            entity.HasIndex(e => e.IsDefault, "IX_Categories_IsDefault");

            entity.HasIndex(e => e.Type, "IX_Categories_Type");

            entity.HasIndex(e => e.UserId, "IX_Categories_UserId");

            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Categories).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Emails_CreatedAt");

            entity.HasIndex(e => e.Status, "IX_Emails_Status");

            entity.HasIndex(e => e.UserId, "IX_Emails_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(256);

            entity.HasOne(d => d.User).WithMany(p => p.Emails).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Expenses_CategoryId");

            entity.HasIndex(e => e.CreatedAt, "IX_Expenses_CreatedAt");

            entity.HasIndex(e => e.ExpenseDate, "IX_Expenses_ExpenseDate");

            entity.HasIndex(e => e.UserId, "IX_Expenses_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Expenses).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Incomes_CategoryId");

            entity.HasIndex(e => e.CreatedAt, "IX_Incomes_CreatedAt");

            entity.HasIndex(e => e.IncomeDate, "IX_Incomes_IncomeDate");

            entity.HasIndex(e => e.UserId, "IX_Incomes_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Note).HasMaxLength(512);

            entity.HasOne(d => d.Category).WithMany(p => p.Incomes).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Incomes).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Notifications_CreatedAt");

            entity.HasIndex(e => e.IsImportant, "IX_Notifications_IsImportant");

            entity.HasIndex(e => e.IsRead, "IX_Notifications_IsRead");

            entity.HasIndex(e => e.Type, "IX_Notifications_Type");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.ActionUrl).HasMaxLength(512);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Message).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Reports_CreatedAt");

            entity.HasIndex(e => e.EndDate, "IX_Reports_EndDate");

            entity.HasIndex(e => e.ReportType, "IX_Reports_ReportType");

            entity.HasIndex(e => e.StartDate, "IX_Reports_StartDate");

            entity.HasIndex(e => e.UserId, "IX_Reports_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FileFormat).HasMaxLength(10);
            entity.Property(e => e.FilePath).HasMaxLength(512);
            entity.Property(e => e.ReportName).HasMaxLength(256);
            entity.Property(e => e.ReportType).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_SystemSettings_UpdatedAt"));

            entity.HasIndex(e => e.IsActive, "IX_SystemSettings_IsActive");

            entity.HasIndex(e => e.SettingKey, "IX_SystemSettings_SettingKey").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingType).HasMaxLength(20);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("tr_Transactions_UpdatedAt"));

            entity.HasIndex(e => e.CategoryId, "IX_Transactions_CategoryId");

            entity.HasIndex(e => e.CreatedAt, "IX_Transactions_CreatedAt");

            entity.HasIndex(e => e.TransactionDate, "IX_Transactions_TransactionDate");

            entity.HasIndex(e => e.Type, "IX_Transactions_Type");

            entity.HasIndex(e => e.UserId, "IX_Transactions_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(512);

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions).HasForeignKey(d => d.CategoryId);

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
                        j.HasIndex(new[] { "UserId" }, "IX_AspNetUserRoles_UserId");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
