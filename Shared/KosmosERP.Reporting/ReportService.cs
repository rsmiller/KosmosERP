using FastReport;
using FastReport.Export.Html;
using FastReport.Export.PdfSimple;
using KosmosERP.Reporting.Reports;

namespace KosmosERP.Reporting;

/// <summary>
/// Dispatches a <see cref="ReportRequest"/> to its <see cref="IReportGenerator"/>, then exports
/// the prepared FastReport document to the requested format. Export and the cross-platform
/// rendering concerns live here so individual generators stay focused on data + template.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReadOnlyDictionary<string, IReportGenerator> _generators;

    public ReportService(IEnumerable<IReportGenerator> generators)
    {
        _generators = generators.ToDictionary(g => g.ReportKey, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> AvailableReportKeys => _generators.Keys.OrderBy(k => k).ToList();

    public async Task<ReportResult> GenerateAsync(ReportRequest request)
    {
        if (request == null)
            return ReportResult.Failed("Report request was null.");

        if (string.IsNullOrWhiteSpace(request.ReportKey) ||
            !_generators.TryGetValue(request.ReportKey, out var generator))
        {
            return ReportResult.Failed($"Unknown report key '{request.ReportKey}'.");
        }

        Report? report = null;
        try
        {
            var generated = await generator.GenerateAsync(request);
            report = generated.Report;

            using var stream = new MemoryStream();

            switch (request.Format)
            {
                case ReportFormat.Html:
                    var html = new HTMLExport
                    {
                        SinglePage = true,
                        EmbedPictures = true,
                        Preview = false
                    };
                    report.Export(html, stream);
                    return new ReportResult
                    {
                        Success = true,
                        Content = stream.ToArray(),
                        ContentType = "text/html",
                        FileName = generated.FileNameBase + ".html"
                    };

                case ReportFormat.Pdf:
                default:
                    var pdf = new PDFSimpleExport();
                    report.Export(pdf, stream);
                    return new ReportResult
                    {
                        Success = true,
                        Content = stream.ToArray(),
                        ContentType = "application/pdf",
                        FileName = generated.FileNameBase + ".pdf"
                    };
            }
        }
        catch (Exception ex)
        {
            return ReportResult.Failed(ex.Message);
        }
        finally
        {
            report?.Dispose();
        }
    }
}
