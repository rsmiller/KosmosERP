using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Purchasing;

/// <summary>
/// Recent Purchase Orders — purchase orders created within a date range, most recent first, with
/// vendor, type, header total (<c>price + tax</c>) and status. There is no separate PO date column,
/// so <c>created_on</c> is used as the PO date. Parameters: <c>date_from</c>, <c>date_to</c>.
/// </summary>
public sealed class RecentPurchaseOrdersReport : ReportGeneratorBase
{
    public const string Key = "recent_purchase_orders";
    private const string MoneyFormat = "#,##0.00";

    public RecentPurchaseOrdersReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_recent_purchase_orders";
    public override string Title => "Recent Purchase Orders";

    public sealed class DisplayRow
    {
        public int PoNumber { get; set; }
        public string PoDate { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string PoType { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);
        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.ToDateTime(TimeOnly.MaxValue);

        var pos = await Context.PurchaseOrderHeaders
            .AsNoTracking()
            .Where(p => p.is_deleted == false)
            .Where(p => p.created_on >= fromDt && p.created_on <= toDt)
            .OrderByDescending(p => p.created_on)
            .Select(p => new { p.po_number, p.created_on, p.vendor_id, p.po_type, p.price, p.tax, p.is_complete, p.is_canceled })
            .ToListAsync();

        var vendorIds = pos.Select(p => p.vendor_id).Distinct().ToList();
        var vendorNames = await Context.Vendors
            .AsNoTracking()
            .Where(v => vendorIds.Contains(v.id))
            .ToDictionaryAsync(v => v.id, v => v.vendor_name);

        var rows = pos
            .Select(p => new DisplayRow
            {
                PoNumber = p.po_number,
                PoDate = p.created_on.ToString("yyyy-MM-dd"),
                Vendor = vendorNames.TryGetValue(p.vendor_id, out var vn) ? vn : $"Vendor {p.vendor_id}",
                PoType = p.po_type,
                Total = (p.price + p.tax).ToString(MoneyFormat),
                Status = p.is_canceled ? "Canceled" : p.is_complete ? "Complete" : "Open",
            })
            .ToList();

        var grandCount = pos.Count;
        var grandTotal = pos.Sum(p => p.price + p.tax);

        var report = LoadTemplate("recent_purchase_orders.frx");
        report.RegisterData(rows, "Orders");
        var ds = report.GetDataSource("Orders");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PeriodText", $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        report.SetParameterValue("GrandCount", grandCount.ToString());
        report.SetParameterValue("GrandTotal", grandTotal.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Recent_Purchase_Orders_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }
}
