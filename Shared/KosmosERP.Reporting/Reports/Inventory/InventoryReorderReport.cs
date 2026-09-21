using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Inventory;

/// <summary>
/// Report #6 — Inventory Reorder / Low-Stock. Items whose on-hand quantity is at or below the
/// reorder level, showing on-hand vs reorder point and a suggested order quantity. Optional
/// <c>category</c> filter.
/// </summary>
public sealed class InventoryReorderReport : ReportGeneratorBase
{
    public const string Key = "inventory_reorder";

    public InventoryReorderReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_inventory_reorder";
    public override string Title => "Inventory Reorder";

    public sealed class DisplayRow
    {
        public string Category { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OnHand { get; set; }
        public int ReorderPoint { get; set; }
        public int Suggested { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var category = GetString(request, "category");

        var lowStock = await Context.InventoryCounts
            .AsNoTracking()
            .Where(i => i.product_id > 0 && i.on_hand <= i.reorder_level)
            .ToListAsync();

        var productIds = lowStock.Select(i => i.product_id).Distinct().ToList();
        var products = await Context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.id) && p.is_deleted == false)
            .ToDictionaryAsync(p => p.id);

        var rows = new List<DisplayRow>();
        foreach (var inv in lowStock)
        {
            if (!products.TryGetValue(inv.product_id, out var product))
                continue;

            if (!string.IsNullOrWhiteSpace(category) &&
                !string.Equals(product.category, category, StringComparison.OrdinalIgnoreCase))
                continue;

            // Prefer the system's computed to_order; fall back to topping up to the reorder level.
            var suggested = inv.to_order > 0 ? inv.to_order : Math.Max(0, inv.reorder_level - inv.on_hand);

            rows.Add(new DisplayRow
            {
                Category = product.category ?? string.Empty,
                Sku = product.identifier1 ?? string.Empty,
                Description = product.product_name ?? string.Empty,
                OnHand = inv.on_hand,
                ReorderPoint = inv.reorder_level,
                Suggested = suggested
            });
        }

        rows = rows.OrderBy(r => r.Category).ThenBy(r => r.Sku).ToList();

        var report = LoadTemplate("inventory_reorder.frx");
        report.RegisterData(rows, "Reorder");
        var ds = report.GetDataSource("Reorder");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("CategoryFilter", string.IsNullOrWhiteSpace(category) ? "All Categories" : category);
        report.SetParameterValue("ItemCount", rows.Count.ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = "Inventory_Reorder"
        };
    }
}
