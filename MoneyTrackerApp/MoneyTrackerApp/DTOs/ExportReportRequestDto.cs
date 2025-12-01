namespace MoneyTrackerApp.DTOs;

public class ExportReportRequestDto
{
    public int ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int FileFormat { get; set; }
}
