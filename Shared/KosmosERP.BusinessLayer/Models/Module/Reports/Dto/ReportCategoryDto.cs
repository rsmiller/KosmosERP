
namespace KosmosERP.BusinessLayer.Models.Module.Reports.Dto
{
    /// <summary>A named grouping of general reports, used to build the reports-page sidebar.</summary>
    public sealed class ReportCategoryDto
    {
        public required string Name { get; init; }
        public List<ReportCatalogItemDto> Reports { get; init; } = new();
    }
}
