using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class Report
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string ReportType { get; set; } = null!; // "monthly", "weekly", "yearly", "custom"

    public string ReportName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Parameters { get; set; } // JSON string for additional parameters

    public string? FilePath { get; set; } // Path to generated report file

    public string? FileFormat { get; set; } // "pdf", "excel", "csv"

    public DateTime? GeneratedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
