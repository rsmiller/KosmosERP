using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Products Awaiting Shipment — every open order line with quantity still unshipped, computed as
/// ordered quantity minus the sum of shipped units across its (non-canceled) shipment lines.
/// Covers stock and manufactured items alike. Only lines with a positive outstanding quantity
/// are listed, ordered by required date. Takes no parameters.
/// </summary>
public sealed class ProductsAwaitingShipmentReport : ReportGeneratorBase
{
    public const string Key = "products_awaiting_shipment";

    public ProductsAwaitingShipmentReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_products_awaiting_shipment";
    public override string Title => "Products Awaiting Shipment";

    public sealed class DisplayRow
    {
        public int OrderNumber { get; set; }
        public string Customer { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int Ordered { get; set; }
        public int Shipped { get; set; }
        public int Outstanding { get; set; }
        public string OrderDate { get; set; } = string.Empty;
        public string RequiredDate { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var openOrders = await Context.OrderHeaders
            .AsNoTracking()
            .Where(o => o.is_deleted == false && o.is_canceled == false && o.is_complete == false)
            .Select(o => new { o.id, o.order_number, o.customer_id, o.order_date, o.required_date })
            .ToListAsync();

        var orderIds = openOrders.Select(o => o.id).ToList();
        var orderById = openOrders.ToDictionary(o => o.id);

        var lines = await Context.OrderLines
            .AsNoTracking()
            .Where(l => l.is_deleted == false && orderIds.Contains(l.order_header_id))
            .Select(l => new { l.id, l.order_header_id, l.product_id, l.quantity })
            .ToListAsync();

        var lineIds = lines.Select(l => l.id).ToList();

        // Proper per-line aggregation of shipped units (do NOT group by units_shipped).
        var shippedByLine = await Context.ShipmentLines
            .AsNoTracking()
            .Where(sl => sl.is_deleted == false && sl.is_canceled == false && lineIds.Contains(sl.order_line_id))
            .GroupBy(sl => sl.order_line_id)
            .Select(g => new { OrderLineId = g.Key, Shipped = g.Sum(x => x.units_shipped) })
            .ToDictionaryAsync(x => x.OrderLineId, x => x.Shipped);

        var productIds = lines.Select(l => l.product_id).Distinct().ToList();
        var productNames = await Context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.id))
            .ToDictionaryAsync(p => p.id, p => p.product_name);

        var customerIds = openOrders.Select(o => o.customer_id).Distinct().ToList();
        var customerNames = await Context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.id))
            .ToDictionaryAsync(c => c.id, c => c.customer_name);

        var rows = lines
            .Select(l =>
            {
                var shipped = shippedByLine.TryGetValue(l.id, out var s) ? s : 0;
                var order = orderById[l.order_header_id];
                return new
                {
                    order.order_number,
                    order.customer_id,
                    order.order_date,
                    order.required_date,
                    l.product_id,
                    Ordered = l.quantity,
                    Shipped = shipped,
                    Outstanding = l.quantity - shipped,
                };
            })
            .Where(x => x.Outstanding > 0)
            .OrderBy(x => x.required_date)
            .ThenBy(x => x.order_number)
            .Select(x => new DisplayRow
            {
                OrderNumber = x.order_number,
                Customer = customerNames.TryGetValue(x.customer_id, out var cn) ? cn : $"Customer {x.customer_id}",
                Product = productNames.TryGetValue(x.product_id, out var pn) ? pn : $"Product {x.product_id}",
                Ordered = x.Ordered,
                Shipped = x.Shipped,
                Outstanding = x.Outstanding,
                OrderDate = x.order_date.ToString("yyyy-MM-dd"),
                RequiredDate = x.required_date.ToString("yyyy-MM-dd"),
            })
            .ToList();

        var grandOutstanding = rows.Sum(r => r.Outstanding);

        var report = LoadTemplate("products_awaiting_shipment.frx");
        report.RegisterData(rows, "Awaiting");
        var ds = report.GetDataSource("Awaiting");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("GeneratedText", $"As of {DateTime.UtcNow:yyyy-MM-dd}");
        report.SetParameterValue("GrandOutstanding", grandOutstanding.ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Products_Awaiting_Shipment_{DateTime.UtcNow:yyyyMMdd}"
        };
    }
}
