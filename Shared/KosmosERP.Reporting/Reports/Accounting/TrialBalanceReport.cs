using FastReport;
using KosmosERP.Database;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #16 — Trial Balance. Each account's ending balance as of a date, shown in the debit or
/// credit column by sign; the two column totals must be equal (double-entry). Parameter:
/// <c>as_of_date</c> (defaults to today).
/// </summary>
public sealed class TrialBalanceReport : ReportGeneratorBase
{
    public const string Key = "trial_balance";
    private const string MoneyFormat = "#,##0.00";

    public TrialBalanceReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_trial_balance";
    public override string Title => "Trial Balance";

    public sealed class Row
    {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public sealed class DisplayRow
    {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Debit { get; set; } = string.Empty;
        public string Credit { get; set; } = string.Empty;
    }

    /// <summary>Computes trial-balance rows and column totals. Exposed for numeric tests.</summary>
    public async Task<(List<Row> rows, decimal debitTotal, decimal creditTotal)> ComputeAsync(DateOnly asOf)
    {
        var balances = await FinancialLedger.BalancesAsOfAsync(Context, asOf);

        var rows = new List<Row>();
        foreach (var b in balances)
        {
            var net = b.Net;
            if (net == 0)
                continue;

            rows.Add(new Row
            {
                Number = b.Number,
                Name = b.Name,
                Debit = net > 0 ? net : 0m,
                Credit = net < 0 ? -net : 0m
            });
        }

        return (rows, rows.Sum(r => r.Debit), rows.Sum(r => r.Credit));
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var asOf = GetDateOrToday(request, "as_of_date");
        var (rows, debitTotal, creditTotal) = await ComputeAsync(asOf);

        var display = rows.Select(r => new DisplayRow
        {
            Number = r.Number,
            Name = r.Name,
            Debit = r.Debit.ToString(MoneyFormat),
            Credit = r.Credit.ToString(MoneyFormat)
        }).ToList();

        var report = LoadTemplate("trial_balance.frx");
        report.RegisterData(display, "Accounts");
        var ds = report.GetDataSource("Accounts");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("AsOfDate", asOf.ToString("yyyy-MM-dd"));
        report.SetParameterValue("DebitTotal", debitTotal.ToString(MoneyFormat));
        report.SetParameterValue("CreditTotal", creditTotal.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Trial_Balance_{asOf:yyyyMMdd}"
        };
    }
}
