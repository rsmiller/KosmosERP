

namespace KosmosERP.BusinessLayer.Models.Module.Reports.Dto
{
    /// <summary>
    /// Describes one general report the UI can request: what to call it, which endpoint
    /// serves it, and which parameters it accepts. General reports summarize across records
    /// (aging, valuations, sales) rather than rendering a single document (e.g. one packing slip).
    /// </summary>
    public sealed class ReportCatalogItemDto
    {
        /// <summary>Stable report key matching <c>IReportGenerator.ReportKey</c> (e.g. "ar_aging").</summary>
        public required string Key { get; init; }

        /// <summary>Display name shown in the sidebar and viewer header.</summary>
        public required string Name { get; init; }

        /// <summary>One-line description of what the report shows.</summary>
        public required string Description { get; init; }

        /// <summary>Owning category name (matches the parent <see cref="ReportCategoryDto.Name"/>).</summary>
        public required string Category { get; init; }

        /// <summary>
        /// Endpoint path relative to the API root without a leading slash, e.g. "api/v1/Reports/ArAging".
        /// The UI appends parameters and a <c>format</c> query value to fetch the rendered report.
        /// </summary>
        public required string Endpoint { get; init; }

        /// <summary>Parameters the report accepts. Empty when the report takes no inputs.</summary>
        public List<ReportParameterDto> Parameters { get; init; } = new();
    }
}
