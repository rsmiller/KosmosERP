using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.CRM;

/// <summary>
/// Top Opportunities — open sales pipeline ranked by weighted value (<c>amount × win_chance / 100</c>),
/// with a grand-total pipeline and weighted total. Closed-won/closed-lost opportunities are excluded.
/// Parameter: <c>top</c> (max rows, default 25). Owner and stage are resolved to display names.
/// </summary>
public sealed class TopOpportunitiesReport : ReportGeneratorBase
{
    public const string Key = "top_opportunities";
    private const string MoneyFormat = "#,##0.00";
    private const int DefaultTop = 25;

    // Stage keys carry the repo's original (misspelled) spelling "opporunity".
    private static readonly string[] ClosedStages = { "opporunity_stage_closed_won", "opporunity_stage_closed_lost" };

    public TopOpportunitiesReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_top_opportunities";
    public override string Title => "Top Opportunities";

    public sealed class DisplayRow
    {
        public string Name { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string WinPct { get; set; } = string.Empty;
        public string Weighted { get; set; } = string.Empty;
        public string ExpectedClose { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var top = TryGetInt(request, "top", out var t) && t > 0 ? t : DefaultTop;

        var opps = await Context.Opportunities
            .AsNoTracking()
            .Where(o => o.is_deleted == false)
            .Where(o => !ClosedStages.Contains(o.stage))
            .Select(o => new { o.opportunity_name, o.customer_id, o.amount, o.stage, o.win_chance, o.expected_close, o.owner_id })
            .ToListAsync();

        var ranked = opps
            .Select(o => new { o.opportunity_name, o.customer_id, o.amount, o.stage, o.win_chance, o.expected_close, o.owner_id, Weighted = o.amount * o.win_chance / 100m })
            .OrderByDescending(o => o.Weighted)
            .Take(top)
            .ToList();

        var customerIds = ranked.Select(o => o.customer_id).Distinct().ToList();
        var customerNames = await Context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.id))
            .ToDictionaryAsync(c => c.id, c => c.customer_name);

        var ownerIds = ranked.Select(o => o.owner_id).Distinct().ToList();
        var owners = await Context.Users
            .AsNoTracking()
            .Where(u => ownerIds.Contains(u.external_id))
            .Select(u => new { u.external_id, u.first_name, u.last_name })
            .ToListAsync();
        var ownerNames = owners
            .GroupBy(u => u.external_id)
            .ToDictionary(g => g.Key, g => $"{g.First().first_name} {g.First().last_name}".Trim());

        var stageKeys = ranked.Select(o => o.stage).Distinct().ToList();
        var stageRows = await Context.KeyValueStores
            .AsNoTracking()
            .Where(kv => kv.is_deleted == false && stageKeys.Contains(kv.key))
            .Select(kv => new { kv.key, kv.value })
            .ToListAsync();
        var stageNames = stageRows
            .GroupBy(kv => kv.key)
            .ToDictionary(g => g.Key, g => g.First().value);

        var rows = ranked
            .Select(o => new DisplayRow
            {
                Name = o.opportunity_name,
                Customer = customerNames.TryGetValue(o.customer_id, out var cn) ? cn : $"Customer {o.customer_id}",
                Owner = ownerNames.TryGetValue(o.owner_id ?? string.Empty, out var on) ? on : (o.owner_id ?? string.Empty),
                Stage = stageNames.TryGetValue(o.stage, out var sn) ? sn : o.stage,
                Amount = o.amount.ToString(MoneyFormat),
                WinPct = $"{o.win_chance}%",
                Weighted = o.Weighted.ToString(MoneyFormat),
                ExpectedClose = o.expected_close.ToString("yyyy-MM-dd"),
            })
            .ToList();

        var grandAmount = ranked.Sum(o => o.amount);
        var grandWeighted = ranked.Sum(o => o.Weighted);

        var report = LoadTemplate("top_opportunities.frx");
        report.RegisterData(rows, "Opportunities");
        var ds = report.GetDataSource("Opportunities");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("FilterText", $"Top {top} open opportunities by weighted pipeline");
        report.SetParameterValue("GrandAmount", grandAmount.ToString(MoneyFormat));
        report.SetParameterValue("GrandWeighted", grandWeighted.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Top_Opportunities_{DateTime.UtcNow:yyyyMMdd}"
        };
    }
}
