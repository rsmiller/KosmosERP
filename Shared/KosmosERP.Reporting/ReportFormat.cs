namespace KosmosERP.Reporting;

/// <summary>
/// Output formats supported by the reporting engine. FastReport.OpenSource renders
/// both natively (PDF via the FastReport.OpenSource.Export.PdfSimple plugin, HTML via
/// the built-in HTML export). XLSX/DOCX are intentionally omitted — those require the
/// commercial FastReport.NET, which is incompatible with the AGPL build.
/// </summary>
public enum ReportFormat
{
    Pdf,
    Html
}
