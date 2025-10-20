using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class Transaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long AccountId { get; set; }

    public long? CategoryId { get; set; }

    public int TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime TransactionDate { get; set; }

    public long? PairedAccountId { get; set; }

    public long? PairedTransactionId { get; set; }

    public string? AttachmentUrl { get; set; }

    public string? OcrText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category? Category { get; set; }

    public virtual ICollection<DebtPayment> DebtPayments { get; set; } = new List<DebtPayment>();

    public virtual ICollection<Transaction> InversePairedTransaction { get; set; } = new List<Transaction>();

    public virtual Account? PairedAccount { get; set; }

    public virtual Transaction? PairedTransaction { get; set; }

    public virtual ICollection<SavingsTransaction> SavingsTransactions { get; set; } = new List<SavingsTransaction>();

    public virtual User User { get; set; } = null!;
}
