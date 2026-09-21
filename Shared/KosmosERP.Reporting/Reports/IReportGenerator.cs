using FastReport;

namespace KosmosERP.Reporting.Reports;

/// <summary>
/// One implementation per report. A generator queries EF Core into flat lists, loads its
/// <c>.frx</c> template, registers the data, sets header/company parameters, and calls
/// <c>Prepare()</c>. Exporting to PDF/HTML and the cross-platform rendering setup are handled
/// centrally by <see cref="ReportService"/>, so generators never touch export code.
/// </summary>
public interface IReportGenerator
{
    /// <summary>Selector used in <see cref="ReportRequest.ReportKey"/>, e.g. "ar_invoice".</summary>
    string ReportKey { get; }

    /// <summary>RBAC token documented for the report (also used as the endpoint authorize token).</summary>
    string PermissionToken { get; }

    /// <summary>Human-readable report name, used for logging and default file names.</summary>
    string Title { get; }

    /// <summary>
    /// Builds and prepares the FastReport document for this request. The returned report is
    /// already <c>Prepare()</c>d and ready to export; the caller owns disposing it.
    /// </summary>
    Task<GeneratedReport> GenerateAsync(ReportRequest request);
}

/// <summary>A prepared FastReport document plus the base file name (no extension) to hand back to the client.</summary>
public sealed class GeneratedReport
{
    public required Report Report { get; init; }
    public required string FileNameBase { get; init; }
}
