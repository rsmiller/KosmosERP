namespace KosmosERP.Reporting;

/// <summary>
/// A request to generate a single report. <see cref="ReportKey"/> selects the generator
/// (see <see cref="IReportService.AvailableReportKeys"/>); <see cref="Parameters"/> carries
/// the per-report filter/selection inputs documented for each report.
/// </summary>
public class ReportRequest
{
    /// <summary>Report selector, e.g. "ar_invoice".</summary>
    public string ReportKey { get; set; } = string.Empty;

    /// <summary>Report-specific inputs (ids, date ranges, optional filters).</summary>
    public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Output format. Defaults to PDF.</summary>
    public ReportFormat Format { get; set; } = ReportFormat.Pdf;

    /// <summary>Id of the user requesting the report — retained for permission checks and audit.</summary>
    public int CallingUserId { get; set; }
}
