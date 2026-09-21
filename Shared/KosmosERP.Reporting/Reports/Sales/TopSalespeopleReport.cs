using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Top Salespeople — order totals aggregated per salesperson over a date range, ranked by total
/// sales, with order count and average order value. Orders are attributed by <c>created_by</c>
/// (the user who entered the order; orders carry no dedicated salesperson field), resolved to a
/// display name via <c>User.external_id</c>. Parameters: <c>date_from</c>, <c>date_to</c>.
/// </summary>
public sealed class TopSalespeopleReport : ReportGeneratorBase
{
    public const string Key = "top_salespeople";
    private const string MoneyFormat = "#,##0.00";

    public TopSalespeopleReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_top_salespeople";
    public override string Title => "Top Salespeople";

    public sealed class DisplayRow
    {
        public string Salesperson { get; set; } = string.Empty;
        public int Orders { get; set; }
        public string TotalSales { get; set; } = string.Empty;
        public string AvgOrder { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);

        var orders = await Context.OrderHeaders
            .AsNoTracking()
            .Where(o => o.is_deleted == false && o.is_canceled == false)
            .Where(o => o.order_date >= from && o.order_date <= to)
            .Select(o => new { o.created_by, o.price })
            .ToListAsync();

        var grouped = orders
            .GroupBy(o => o.created_by ?? string.Empty)
            .Select(g => new { CreatedBy = g.Key, Orders = g.Count(), Total = g.Sum(x => x.price) })
            .ToList();

        var userKeys = grouped.Select(g => g.CreatedBy).Distinct().ToList();
        var users = await Context.Users
            .AsNoTracking()
            .Where(u => userKeys.Contains(u.external_id))
            .Select(u => new { u.external_id, u.first_name, u.last_name })
            .ToListAsync();
        var userNames = users
            .GroupBy(u => u.external_id)
            .ToDictionary(g => g.Key, g => $"{g.First().first_name} {g.First().last_name}".Trim());

        var rows = grouped
            .OrderByDescending(g => g.Total)
            .Select(g => new DisplayRow
            {
                Salesperson = userNames.TryGetValue(g.CreatedBy, out var n) && !string.IsNullOrWhiteSpace(n)
                    ? n
                    : (string.IsNullOrWhiteSpace(g.CreatedBy) ? "(unknown)" : g.CreatedBy),
                Orders = g.Orders,
                TotalSales = g.Total.ToString(MoneyFormat),
                AvgOrder = (g.Orders > 0 ? g.Total / g.Orders : 0m).ToString(MoneyFormat),
            })
            .ToList();

        var grandOrders = grouped.Sum(g => g.Orders);
        var grandTotal = grouped.Sum(g => g.Total);

        var report = LoadTemplate("top_salespeople.frx");
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
            FileNameBase = $"Top_Salespeople_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }
}
