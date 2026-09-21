using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Manufacturing;

/// <summary>
/// Report #7 — Bill of Materials. Single-level component list (component, SKU, qty per) for an
/// assembly. Selected by <c>product_id</c> (the assembly) or <c>bom_id</c> (a BOM row whose parent
/// is the assembly).
///
/// TODO (flagged): multi-level explosion (the optional <c>levels</c> parameter) is not yet
/// implemented — only the immediate components are listed.
/// </summary>
public sealed class BomReport : ReportGeneratorBase
{
    public const string Key = "bom";

    public BomReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_bom";
    public override string Title => "Bill of Materials";

    public sealed class DisplayRow
    {
        public int Order { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public int QtyPer { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var assemblyProductId = await ResolveAssemblyProductIdAsync(request);
        if (assemblyProductId <= 0)
            throw new InvalidOperationException("Supply a valid 'product_id' or 'bom_id'.");

        var boms = await Context.BOMs
            .AsNoTracking()
            .Where(b => b.parent_product_id == assemblyProductId && b.is_deleted == false)
            .OrderBy(b => b.order_number)
            .ToListAsync();

        var componentIds = boms.Select(b => b.product_id).Distinct().ToList();
        var productIdsToLoad = componentIds.Append(assemblyProductId).Distinct().ToList();
        var products = await Context.Products
            .AsNoTracking()
            .Where(p => productIdsToLoad.Contains(p.id))
            .ToDictionaryAsync(p => p.id);

        var rows = boms.Select(b =>
        {
            products.TryGetValue(b.product_id, out var comp);
            return new DisplayRow
            {
                Order = b.order_number,
                Sku = comp?.identifier1 ?? string.Empty,
                Component = comp?.product_name ?? $"Product {b.product_id}",
                QtyPer = b.quantity
            };
        }).ToList();

        products.TryGetValue(assemblyProductId, out var assembly);

        var report = LoadTemplate("bom.frx");
        report.RegisterData(rows, "Components");
        var ds = report.GetDataSource("Components");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("AssemblyName", assembly?.product_name ?? $"Product {assemblyProductId}");
        report.SetParameterValue("AssemblySku", assembly?.identifier1 ?? string.Empty);
        report.SetParameterValue("ComponentCount", rows.Count.ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"BOM_{assemblyProductId}"
        };
    }

    private async Task<int> ResolveAssemblyProductIdAsync(ReportRequest request)
    {
        if (TryGetInt(request, "product_id", out var productId) && productId > 0)
            return productId;

        if (TryGetInt(request, "bom_id", out var bomId) && bomId > 0)
        {
            var bom = await Context.BOMs.AsNoTracking().FirstOrDefaultAsync(b => b.id == bomId);
            return bom?.parent_product_id ?? 0;
        }

        return 0;
    }
}
