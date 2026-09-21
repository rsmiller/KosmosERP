using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Report #3 — Sales by Customer. Orders aggregated per customer over a date range: order count
/// and sales subtotal, with a grand total. Parameters: <c>date_from</c>, <c>date_to</c>, optional
/// <c>customer_id</c>.
/// </summary>
public sealed class SalesByCustomerReport : ReportGeneratorBase
{
    public const string Key = "sales_by_customer";
    private const string MoneyFormat = "#,##0.00";

    public SalesByCustomerReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_sales_by_customer";
    public override string Title => "Sales by Customer";

    public sealed class DisplayRow
    {
        public string Customer { get; set; } = string.Empty;
        public int Orders { get; set; }
        public string Total { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);
        var customerFilter = TryGetInt(request, "customer_id", out var cid) && cid > 0 ? cid : (int?)null;

        var orders = await Context.OrderHeaders
            .AsNoTracking()
            .Where(o => o.is_deleted == false && o.is_canceled == false)
            .Where(o => o.order_date >= from && o.order_date <= to)
            .Where(o => customerFilter == null || o.customer_id == customerFilter)
            .Select(o => new { o.customer_id, o.price })
            .ToListAsync();

        var grouped = orders
            .GroupBy(o => o.customer_id)
            .Select(g => new { CustomerId = g.Key, Orders = g.Count(), Total = g.Sum(x => x.price) })
            .ToList();

        var customerIds = grouped.Select(g => g.CustomerId).ToList();
        var customerNames = await Context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.id))
            .ToDictionaryAsync(c => c.id, c => c.customer_name);

        var rows = grouped
            .Select(g => new
            {
                Name = customerNames.TryGetValue(g.CustomerId, out var n) ? n : $"Customer {g.CustomerId}",
                g.Orders,
                g.Total
            })
            .OrderByDescending(r => r.Total)
            .Select(r => new DisplayRow
            {
                Customer = r.Name,
                Orders = r.Orders,
                Total = r.Total.ToString(MoneyFormat)
            })
            .ToList();

        var grandOrders = grouped.Sum(g => g.Orders);
        var grandTotal = grouped.Sum(g => g.Total);

        var report = LoadTemplate("sales_by_customer.frx");
        report.RegisterData(rows, "Sales");
        var ds = report.GetDataSource("Sales");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PeriodText", $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        report.SetParameterValue("GrandOrders", grandOrders.ToString());
        report.SetParameterValue("GrandTotal", grandTotal.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Sales_By_Customer_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }
}
