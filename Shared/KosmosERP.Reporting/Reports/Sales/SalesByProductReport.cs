using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Report #4 — Sales by Product / Item. Order lines aggregated per product over a date range:
/// quantity sold, revenue and average price, sorted by revenue descending (top sellers first).
/// Parameters: <c>date_from</c>, <c>date_to</c>, optional <c>product_category</c>.
/// </summary>
public sealed class SalesByProductReport : ReportGeneratorBase
{
    public const string Key = "sales_by_product";
    private const string MoneyFormat = "#,##0.00";

    public SalesByProductReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_sales_by_product";
    public override string Title => "Sales by Product";

    public sealed class DisplayRow
    {
        public string Sku { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int QtySold { get; set; }
        public string Revenue { get; set; } = string.Empty;
        public string AvgPrice { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);
        var category = GetString(request, "product_category");

        var orderIds = await Context.OrderHeaders
            .AsNoTracking()
            .Where(o => o.is_deleted == false && o.is_canceled == false)
            .Where(o => o.order_date >= from && o.order_date <= to)
            .Select(o => o.id)
            .ToListAsync();

        var lines = await Context.OrderLines
            .AsNoTracking()
            .Where(l => l.is_deleted == false && orderIds.Contains(l.order_header_id))
            .Select(l => new { l.product_id, l.quantity, l.unit_price })
            .ToListAsync();

        var productIds = lines.Select(l => l.product_id).Distinct().ToList();
        var products = await Context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.id))
            .ToDictionaryAsync(p => p.id);

        var grouped = lines
            .GroupBy(l => l.product_id)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty = g.Sum(x => x.quantity),
                Revenue = g.Sum(x => x.quantity * x.unit_price)
            })
            .ToList();

        var rows = new List<(DisplayRow row, decimal revenue)>();
        foreach (var g in grouped)
        {
            products.TryGetValue(g.ProductId, out var product);

            if (!string.IsNullOrWhiteSpace(category) &&
                !string.Equals(product?.category, category, StringComparison.OrdinalIgnoreCase))
                continue;

            var avg = g.Qty > 0 ? decimal.Round(g.Revenue / g.Qty, 2) : 0m;
            rows.Add((new DisplayRow
            {
                Sku = product?.identifier1 ?? string.Empty,
                Product = product?.product_name ?? $"Product {g.ProductId}",
                QtySold = g.Qty,
                Revenue = g.Revenue.ToString(MoneyFormat),
                AvgPrice = avg.ToString(MoneyFormat)
            }, g.Revenue));
        }

        var display = rows.OrderByDescending(r => r.revenue).Select(r => r.row).ToList();
        var grandRevenue = rows.Sum(r => r.revenue);

        var report = LoadTemplate("sales_by_product.frx");
        report.RegisterData(display, "Sales");
        var ds = report.GetDataSource("Sales");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PeriodText", $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        report.SetParameterValue("CategoryFilter", string.IsNullOrWhiteSpace(category) ? "All Categories" : category);
        report.SetParameterValue("GrandQty", display.Sum(d => d.QtySold).ToString());
        report.SetParameterValue("GrandRevenue", grandRevenue.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Sales_By_Product_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }
}
