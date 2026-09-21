using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #13 — AP Aging. Mirror of AR aging, grouped by vendor. Open balance is the unpaid
/// invoice total (AP invoices carry an <c>is_paid</c> flag rather than applied payments).
/// Parameter: <c>as_of_date</c> (defaults to today).
/// </summary>
public sealed class ApAgingReport : ReportGeneratorBase
{
    public const string Key = "ap_aging";
    private const string MoneyFormat = "#,##0.00";

    public ApAgingReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_ap_aging";
    public override string Title => "AP Aging";

    public sealed class DisplayRow
    {
        public string Name { get; set; } = string.Empty;
        public string Current { get; set; } = string.Empty;
        public string Days1To30 { get; set; } = string.Empty;
        public string Days31To60 { get; set; } = string.Empty;
        public string Days61To90 { get; set; } = string.Empty;
        public string Days90Plus { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
    }

    /// <summary>Computes per-vendor aging as of <paramref name="asOf"/>. Exposed for numeric tests.</summary>
    public async Task<List<AgingRow>> ComputeAsync(DateOnly asOf)
    {
        var invoices = await Context.APInvoiceHeaders
            .AsNoTracking()
            .Where(h => h.is_deleted == false && h.is_paid == false)
            .Select(h => new { h.vendor_id, h.invoice_total, h.invoice_due_date })
            .ToListAsync();

        var vendorIds = invoices.Select(i => i.vendor_id).Distinct().ToList();
        var vendorNames = await Context.Vendors
            .AsNoTracking()
            .Where(v => vendorIds.Contains(v.id))
            .ToDictionaryAsync(v => v.id, v => v.vendor_name);

        var rowsByVendor = new Dictionary<int, AgingRow>();

        foreach (var inv in invoices)
        {
            var open = inv.invoice_total;
            if (open <= 0)
                continue;

            if (!rowsByVendor.TryGetValue(inv.vendor_id, out var row))
            {
                row = new AgingRow
                {
                    EntityId = inv.vendor_id,
                    Name = vendorNames.TryGetValue(inv.vendor_id, out var n) ? n : $"Vendor {inv.vendor_id}"
                };
                rowsByVendor[inv.vendor_id] = row;
            }

            var dueDate = DateOnly.FromDateTime(inv.invoice_due_date);
            var daysPastDue = asOf.DayNumber - dueDate.DayNumber;
            AgingCalculator.Add(row, open, daysPastDue);
        }

        return rowsByVendor.Values.OrderBy(r => r.Name).ToList();
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var asOf = GetDateOrToday(request, "as_of_date");
        var rows = await ComputeAsync(asOf);
        var grand = AgingCalculator.GrandTotal(rows);

        var display = rows.Select(ToDisplay).ToList();

        var report = LoadTemplate("ap_aging.frx");
        report.RegisterData(display, "Aging");
        var ds = report.GetDataSource("Aging");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("AsOfDate", asOf.ToString("yyyy-MM-dd"));
        report.SetParameterValue("GrandCurrent", grand.Current.ToString(MoneyFormat));
        report.SetParameterValue("GrandD1", grand.Days1To30.ToString(MoneyFormat));
        report.SetParameterValue("GrandD2", grand.Days31To60.ToString(MoneyFormat));
        report.SetParameterValue("GrandD3", grand.Days61To90.ToString(MoneyFormat));
        report.SetParameterValue("GrandD4", grand.Days90Plus.ToString(MoneyFormat));
        report.SetParameterValue("GrandTotal", grand.Total.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"AP_Aging_{asOf:yyyyMMdd}"
        };
    }

    private static DisplayRow ToDisplay(AgingRow r) => new()
    {
        Name = r.Name,
        Current = r.Current.ToString(MoneyFormat),
        Days1To30 = r.Days1To30.ToString(MoneyFormat),
        Days31To60 = r.Days31To60.ToString(MoneyFormat),
        Days61To90 = r.Days61To90.ToString(MoneyFormat),
        Days90Plus = r.Days90Plus.ToString(MoneyFormat),
        Total = r.Total.ToString(MoneyFormat),
    };
}
