using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Manufacturing;

/// <summary>
/// Report #8 — Production Order Traveler / Work Order. The shop-floor traveler: order/product
/// header, routing operations (production lines) with sign-off spaces, and the required
/// components (single-level BOM explosion) for the produced items. Selected by
/// <c>production_order_id</c>.
///
/// TODO (flagged): the traveler ideally prints the production order number as a scannable
/// barcode and supports multi-level BOM explosion. Single-level components are shown here;
/// multi-level explosion overlaps report #7 (BOM) and can be layered in later.
/// </summary>
public sealed class ProductionOrderTravelerReport : ReportGeneratorBase
{
    public const string Key = "production_order_traveler";

    public ProductionOrderTravelerReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_production_order_traveler";
    public override string Title => "Production Order Traveler";

    /// <summary>A routing operation / production line.</summary>
    public sealed class OperationRow
    {
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>A required component (single-level BOM).</summary>
    public sealed class ComponentRow
    {
        public string ForProduct { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public int QtyPer { get; set; }
    }

    /// <summary>Single-row source used purely to print the components section header once.</summary>
    public sealed class SectionRow
    {
        public int Marker { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        if (!TryGetInt(request, "production_order_id", out var id) || id <= 0)
            throw new InvalidOperationException("Supply a valid 'production_order_id'.");

        var header = await Context.ProductionOrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.id == id && h.is_deleted == false);
        if (header == null)
            throw new InvalidOperationException("Production order not found for the given 'production_order_id'.");

        var prodLines = await Context.ProductionOrderLines
            .AsNoTracking()
            .Where(l => l.production_order_header_id == header.id && l.is_deleted == false)
            .OrderBy(l => l.line_number)
            .ToListAsync();

        // Resolve each production line's order line -> product (description + product id).
        var orderLineIds = prodLines.Select(l => l.order_line_id).Distinct().ToList();
        var orderLines = await Context.OrderLines
            .AsNoTracking()
            .Where(ol => orderLineIds.Contains(ol.id))
            .ToDictionaryAsync(ol => ol.id);

        var order = await Context.OrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.id == header.order_header_id);

        var operations = prodLines.Select(l =>
        {
            orderLines.TryGetValue(l.order_line_id, out var ol);
            return new OperationRow
            {
                LineNumber = l.line_number,
                Description = ol?.line_description ?? string.Empty,
                Qty = l.quantity,
                Status = l.status ?? string.Empty
            };
        }).ToList();

        // Single-level components: BOM rows whose parent is a produced product.
        var producedProductIds = prodLines
            .Select(l => orderLines.TryGetValue(l.order_line_id, out var ol) ? ol.product_id : 0)
            .Where(pid => pid > 0)
            .Distinct()
            .ToList();

        var boms = await Context.BOMs
            .AsNoTracking()
            .Where(b => producedProductIds.Contains(b.parent_product_id) && b.is_deleted == false)
            .OrderBy(b => b.parent_product_id).ThenBy(b => b.order_number)
            .ToListAsync();

        var productIds = boms.Select(b => b.parent_product_id)
            .Concat(boms.Select(b => b.product_id))
            .Distinct()
            .ToList();
        var products = await Context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.id))
            .ToDictionaryAsync(p => p.id);

        var components = boms.Select(b =>
        {
            products.TryGetValue(b.parent_product_id, out var parent);
            products.TryGetValue(b.product_id, out var comp);
            return new ComponentRow
            {
                ForProduct = parent?.product_name ?? $"Product {b.parent_product_id}",
                Component = comp?.product_name ?? $"Product {b.product_id}",
                QtyPer = b.quantity
            };
        }).ToList();

        var report = LoadTemplate("production_order_traveler.frx");
        report.RegisterData(operations, "Ops");
        report.RegisterData(new[] { new SectionRow { Marker = 1 } }, "CompSection");
        report.RegisterData(components, "Components");
        foreach (var name in new[] { "Ops", "CompSection", "Components" })
        {
            var ds = report.GetDataSource(name);
            if (ds != null)
                ds.Enabled = true;
        }

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("ProductionOrderNumber", header.id.ToString());
        report.SetParameterValue("OrderNumber", order?.order_number.ToString() ?? string.Empty);
        report.SetParameterValue("Status", header.status ?? string.Empty);
        report.SetParameterValue("PlannedStart", header.planned_start_date?.ToString("yyyy-MM-dd") ?? string.Empty);
        report.SetParameterValue("PlannedComplete", header.planned_complete_date?.ToString("yyyy-MM-dd") ?? string.Empty);

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Production_Order_{header.id}"
        };
    }
}
