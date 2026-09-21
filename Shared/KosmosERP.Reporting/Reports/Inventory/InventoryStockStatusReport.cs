using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Inventory;

/// <summary>
/// Report #5 — Inventory Stock Status / Valuation. SKU, description, on-hand qty, unit cost and
/// extended value, ordered by category, with a grand-total valuation. Optional <c>category</c>
/// filter. (<c>as_of_date</c> is accepted for parity but the Inventory table is a live snapshot;
/// there is no location field, so location grouping is omitted — see spec owner-review note.)
///
/// TODO (flagged): per-category valuation subtotals would use FastReport group bands; currently
/// the category is shown per row with a single grand total.
/// </summary>
public sealed class InventoryStockStatusReport : ReportGeneratorBase
{
    public const string Key = "inventory_stock_status";
    private const string MoneyFormat = "#,##0.00";

    public InventoryStockStatusReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_inventory_stock_status";
    public override string Title => "Inventory Stock Status";

    public sealed class DisplayRow
    {
        public string Category { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OnHand { get; set; }
        public string UnitCost { get; set; } = string.Empty;
        public string ExtendedValue { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var asOf = GetDateOrToday(request, "as_of_date");
        var category = GetString(request, "category");

        var inventory = await Context.InventoryCounts
            .AsNoTracking()
            .Where(i => i.product_id > 0)
            .ToListAsync();

        var productIds = inventory.Select(i => i.product_id).Distinct().ToList();
        var products = await Context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.id) && p.is_deleted == false)
            .ToDictionaryAsync(p => p.id);

        var rows = new List<DisplayRow>();
        decimal grandValue = 0;

        foreach (var inv in inventory)
        {
            if (!products.TryGetValue(inv.product_id, out var product))
                continue;

            if (!string.IsNullOrWhiteSpace(category) &&
                !string.Equals(product.category, category, StringComparison.OrdinalIgnoreCase))
                continue;

            var ext = inv.on_hand * product.unit_cost;
            grandValue += ext;

            rows.Add(new DisplayRow
            {
                Category = product.category ?? string.Empty,
                Sku = product.identifier1 ?? string.Empty,
                Description = product.product_name ?? string.Empty,
                OnHand = inv.on_hand,
                UnitCost = product.unit_cost.ToString(MoneyFormat),
                ExtendedValue = ext.ToString(MoneyFormat)
            });
        }

        rows = rows.OrderBy(r => r.Category).ThenBy(r => r.Sku).ToList();

        var report = LoadTemplate("inventory_stock_status.frx");
        report.RegisterData(rows, "Stock");
        var ds = report.GetDataSource("Stock");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("AsOfDate", asOf.ToString("yyyy-MM-dd"));
        report.SetParameterValue("CategoryFilter", string.IsNullOrWhiteSpace(category) ? "All Categories" : category);
        report.SetParameterValue("GrandValue", grandValue.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Inventory_Stock_Status_{asOf:yyyyMMdd}"
        };
    }
}
