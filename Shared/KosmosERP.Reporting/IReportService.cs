namespace KosmosERP.Reporting;

/// <summary>
/// Entry point for rendering reports. Implemented by <see cref="ReportService"/>, which
/// dispatches on <see cref="ReportRequest.ReportKey"/> to the matching generator, prepares
/// the FastReport document, and exports it to the requested <see cref="ReportFormat"/>.
/// </summary>
public interface IReportService
{
    Task<ReportResult> GenerateAsync(ReportRequest request);

    /// <summary>The report keys this service can generate (e.g. "ar_invoice").</summary>
    IReadOnlyList<string> AvailableReportKeys { get; }
}
