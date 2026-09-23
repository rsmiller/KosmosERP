using System.Text.Json;
using KosmosERP.BusinessLayer.Models.Module.Reports.Dto;

namespace KosmosERP.Api.Models;

public interface IReportCatalog
{
    List<ReportCategoryDto> FullCatalog { get; }
}

/// <summary>
/// The catalog of "general" reports surfaced on the Reports page, loaded from reportscatalog.json.
/// These are the cross-record reports that stand on their own — deliberately excluding document-style
/// reports that require a specific entity id (AR invoice, packing slip, purchase order,
/// sales order acknowledgement, production traveler, BOM), which are served from their
/// own document screens instead. Endpoints and parameters mirror
/// <see cref="Controllers.ReportsController"/>; keep the two in sync when adding reports.
/// </summary>
public class ReportCatalog : IReportCatalog
{
    private const string RootPropertyName = "reportscatalog";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ReportCatalog(string catalogJson)
    {
        using var document = JsonDocument.Parse(catalogJson);

        if (!document.RootElement.TryGetProperty(RootPropertyName, out var catalogElement))
            throw new InvalidOperationException($"Report catalog JSON is missing the '{RootPropertyName}' root property.");

        FullCatalog = catalogElement.Deserialize<List<ReportCategoryDto>>(SerializerOptions) ?? new();
    }

    public List<ReportCategoryDto> FullCatalog { get; private set; }
}
