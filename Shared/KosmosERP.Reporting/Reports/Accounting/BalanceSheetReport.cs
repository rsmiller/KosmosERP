using FastReport;
using KosmosERP.Database;
using KosmosERP.Models;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #15 — Balance Sheet as of a date: Assets, Liabilities, Equity, with the current-period
/// net income folded into equity so that Assets = Liabilities + Equity (double-entry guarantees
/// this by construction). Driven off the <c>FinancialTransaction</c> ledger. Parameter:
/// <c>as_of_date</c> (defaults to today).
/// </summary>
public sealed class BalanceSheetReport : ReportGeneratorBase
{
    public const string Key = "balance_sheet";
    private const string MoneyFormat = "#,##0.00";

    public BalanceSheetReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_balance_sheet";
    public override string Title => "Balance Sheet";

    public sealed class LineRow
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public sealed class DisplayRow
    {
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }

    public sealed class SectionRow
    {
        public int Marker { get; set; }
    }

    public sealed class Result
    {
        public List<LineRow> Assets { get; set; } = new();
        public List<LineRow> Liabilities { get; set; } = new();
        public List<LineRow> Equity { get; set; } = new();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal NetIncome { get; set; }
        public decimal LiabilitiesPlusEquity => TotalLiabilities + TotalEquity;
        public bool Balances => decimal.Round(TotalAssets, 2) == decimal.Round(LiabilitiesPlusEquity, 2);
    }

    /// <summary>Computes the balance sheet as of <paramref name="asOf"/>. Exposed for numeric tests.</summary>
    public async Task<Result> ComputeAsync(DateOnly asOf)
    {
        var balances = await FinancialLedger.BalancesAsOfAsync(Context, asOf);
        var result = new Result();

        foreach (var b in balances.Where(b => b.AccountType == AccountType.Asset))
            result.Assets.Add(new LineRow { Name = b.Name, Amount = b.Net });   // asset = debit − credit

        foreach (var b in balances.Where(b => b.AccountType == AccountType.Liability))
            result.Liabilities.Add(new LineRow { Name = b.Name, Amount = -b.Net });   // liability = credit − debit

        foreach (var b in balances.Where(b => b.AccountType == AccountType.Equity))
            result.Equity.Add(new LineRow { Name = b.Name, Amount = -b.Net });   // equity = credit − debit

        var revenueNet = balances.Where(b => b.AccountType == AccountType.Revenue).Sum(b => b.Credit - b.Debit);
        var expenseNet = balances.Where(b => b.AccountType == AccountType.Expense).Sum(b => b.Debit - b.Credit);
        result.NetIncome = revenueNet - expenseNet;

        // Current-period earnings belong to equity.
        result.Equity.Add(new LineRow { Name = "Current Period Net Income", Amount = result.NetIncome });

        result.TotalAssets = result.Assets.Sum(r => r.Amount);
        result.TotalLiabilities = result.Liabilities.Sum(r => r.Amount);
        result.TotalEquity = result.Equity.Sum(r => r.Amount);

        return result;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var asOf = GetDateOrToday(request, "as_of_date");
        var result = await ComputeAsync(asOf);

        var report = LoadTemplate("balance_sheet.frx");
        report.RegisterData(result.Assets.Select(ToDisplay).ToList(), "Assets");
        report.RegisterData(new[] { new SectionRow { Marker = 1 } }, "LiabSection");
        report.RegisterData(result.Liabilities.Select(ToDisplay).ToList(), "Liabilities");
        report.RegisterData(new[] { new SectionRow { Marker = 1 } }, "EquitySection");
        report.RegisterData(result.Equity.Select(ToDisplay).ToList(), "Equity");
        foreach (var name in new[] { "Assets", "LiabSection", "Liabilities", "EquitySection", "Equity" })
        {
            var ds = report.GetDataSource(name);
            if (ds != null)
                ds.Enabled = true;
        }

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("AsOfDate", asOf.ToString("yyyy-MM-dd"));
        report.SetParameterValue("TotalAssets", result.TotalAssets.ToString(MoneyFormat));
        report.SetParameterValue("TotalLiabilities", result.TotalLiabilities.ToString(MoneyFormat));
        report.SetParameterValue("TotalEquity", result.TotalEquity.ToString(MoneyFormat));
        report.SetParameterValue("LiabPlusEquity", result.LiabilitiesPlusEquity.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Balance_Sheet_{asOf:yyyyMMdd}"
        };
    }

    private static DisplayRow ToDisplay(LineRow r) => new()
    {
        Name = r.Name,
        Amount = r.Amount.ToString(MoneyFormat)
    };
}
