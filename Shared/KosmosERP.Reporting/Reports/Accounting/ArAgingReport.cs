using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #12 — AR Aging. Open AR balances per customer, split into Current / 1–30 / 31–60 /
/// 61–90 / 90+ buckets by days past the invoice due date, with a grand total. Open balance is
/// the invoice total less applied payments. Parameter: <c>as_of_date</c> (defaults to today).
/// </summary>
public sealed class ArAgingReport : ReportGeneratorBase
{
    public const string Key = "ar_aging";
    private const string MoneyFormat = "#,##0.00";

    public ArAgingReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_ar_aging";
    public override string Title => "AR Aging";

    /// <summary>Display row (money pre-formatted) bound to the aging detail band.</summary>
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

    /// <summary>
    /// Computes per-customer aging as of <paramref name="asOf"/>. Exposed for numeric-correctness
    /// tests (buckets/grand total) independent of rendering.
    /// </summary>
    public async Task<List<AgingRow>> ComputeAsync(DateOnly asOf)
    {
        var invoices = await Context.ARInvoiceHeaders
            .AsNoTracking()
            .Where(h => h.is_deleted == false)
            .Select(h => new { h.id, h.customer_id, h.invoice_total, h.invoice_due_date })
            .ToListAsync();

        var payments = await Context.Payments
            .AsNoTracking()
            .Where(p => p.is_deleted == false)
            .GroupBy(p => p.ar_header_id)
            .Select(g => new { ArHeaderId = g.Key, Paid = g.Sum(x => x.payment_amount) })
            .ToDictionaryAsync(x => x.ArHeaderId, x => x.Paid);

        var customerIds = invoices.Select(i => i.customer_id).Distinct().ToList();
        var customerNames = await Context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.id))
            .ToDictionaryAsync(c => c.id, c => c.customer_name);

        var rowsByCustomer = new Dictionary<int, AgingRow>();

        foreach (var inv in invoices)
        {
            var paid = payments.TryGetValue(inv.id, out var p) ? p : 0m;
            var open = inv.invoice_total - paid;
            if (open <= 0)
                continue;

            if (!rowsByCustomer.TryGetValue(inv.customer_id, out var row))
            {
                row = new AgingRow
                {
                    EntityId = inv.customer_id,
                    Name = customerNames.TryGetValue(inv.customer_id, out var n) ? n : $"Customer {inv.customer_id}"
                };
                rowsByCustomer[inv.customer_id] = row;
            }

            var daysPastDue = asOf.DayNumber - inv.invoice_due_date.DayNumber;
            AgingCalculator.Add(row, open, daysPastDue);
        }

        return rowsByCustomer.Values.OrderBy(r => r.Name).ToList();
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var asOf = GetDateOrToday(request, "as_of_date");
        var rows = await ComputeAsync(asOf);
        var grand = AgingCalculator.GrandTotal(rows);

        var display = rows.Select(ToDisplay).ToList();

        var report = LoadTemplate("ar_aging.frx");
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
            FileNameBase = $"AR_Aging_{asOf:yyyyMMdd}"
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
