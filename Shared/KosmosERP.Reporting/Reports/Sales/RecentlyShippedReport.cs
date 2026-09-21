using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Recently Shipped — completed shipments over a date range, most recent first. There is no
/// dedicated ship-date column, so <c>completed_on</c> is used as the shipped date. Customer and
/// order number are resolved from the linked order; the freight carrier key is resolved to its
/// display name. Parameters: <c>date_from</c>, <c>date_to</c> (against <c>completed_on</c>).
/// </summary>
public sealed class RecentlyShippedReport : ReportGeneratorBase
{
    public const string Key = "recently_shipped";

    public RecentlyShippedReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_recently_shipped";
    public override string Title => "Recently Shipped";

    public sealed class DisplayRow
    {
        public int ShipmentNumber { get; set; }
        public string CompletedDate { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public int UnitsShipped { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);
        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.ToDateTime(TimeOnly.MaxValue);

        var shipments = await Context.ShipmentHeaders
            .AsNoTracking()
            .Where(s => s.is_deleted == false && s.is_canceled == false && s.is_complete == true)
            .Where(s => s.completed_on != null && s.completed_on >= fromDt && s.completed_on <= toDt)
            .OrderByDescending(s => s.completed_on)
            .Select(s => new { s.shipment_number, s.completed_on, s.order_header_id, s.units_shipped, s.freight_carrier })
            .ToListAsync();

        var orderIds = shipments.Select(s => s.order_header_id).Distinct().ToList();
        var orders = await Context.OrderHeaders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.id))
            .Select(o => new { o.id, o.order_number, o.customer_id })
            .ToListAsync();
        var orderById = orders.ToDictionary(o => o.id);

        var customerIds = orders.Select(o => o.customer_id).Distinct().ToList();
        var customerNames = await Context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.id))
            .ToDictionaryAsync(c => c.id, c => c.customer_name);

        var carrierKeys = shipments
            .Select(s => s.freight_carrier)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList();
        var carrierRows = await Context.KeyValueStores
            .AsNoTracking()
            .Where(kv => kv.is_deleted == false && carrierKeys.Contains(kv.key))
            .Select(kv => new { kv.key, kv.value })
            .ToListAsync();
        var carrierNames = carrierRows
            .GroupBy(kv => kv.key)
            .ToDictionary(g => g.Key, g => g.First().value);

        var rows = shipments
            .Select(s =>
            {
                orderById.TryGetValue(s.order_header_id, out var order);
                return new DisplayRow
                {
                    ShipmentNumber = s.shipment_number,
                    CompletedDate = s.completed_on?.ToString("yyyy-MM-dd") ?? string.Empty,
                    Customer = order != null && customerNames.TryGetValue(order.customer_id, out var cn) ? cn : string.Empty,
                    OrderNumber = order != null ? order.order_number.ToString() : string.Empty,
                    Carrier = !string.IsNullOrWhiteSpace(s.freight_carrier) && carrierNames.TryGetValue(s.freight_carrier, out var carr)
                        ? carr
                        : (s.freight_carrier ?? string.Empty),
                    UnitsShipped = s.units_shipped,
                };
            })
            .ToList();

        var grandShipments = rows.Count;
        var grandUnits = rows.Sum(r => r.UnitsShipped);

        var report = LoadTemplate("recently_shipped.frx");
        report.RegisterData(rows, "Shipments");
        var ds = report.GetDataSource("Shipments");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PeriodText", $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        report.SetParameterValue("GrandShipments", grandShipments.ToString());
        report.SetParameterValue("GrandUnits", grandUnits.ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Recently_Shipped_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }
}
