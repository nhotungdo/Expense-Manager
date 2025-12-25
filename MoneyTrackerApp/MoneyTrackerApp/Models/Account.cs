using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Account
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public int AccountType { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal InitialBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IncludeInTotal { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BankConnection> BankConnections { get; set; } = new List<BankConnection>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();

    public virtual ICollection<ScheduledTransaction> ScheduledTransactions { get; set; } = new List<ScheduledTransaction>();

    public virtual ICollection<SharedAccount> SharedAccounts { get; set; } = new List<SharedAccount>();

    public virtual ICollection<Transaction> TransactionAccounts { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionPairedAccounts { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
