using FastReport;
using KosmosERP.Database;
using KosmosERP.Models;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #14 — Income Statement (P&amp;L). Revenue and expense sections over a date range with
/// net income, driven off the <c>FinancialTransaction</c> ledger. Parameters: <c>date_from</c>,
/// <c>date_to</c>.
///
/// TODO (flagged): the chart of accounts has no COGS sub-classification, so gross-profit /
/// COGS breakout is not produced — expenses are shown as one group. Add a COGS account subtype
/// (or a KeyValueStore mapping) to split COGS from operating expenses.
/// </summary>
public sealed class IncomeStatementReport : ReportGeneratorBase
{
    public const string Key = "income_statement";
    private const string MoneyFormat = "#,##0.00";

    public IncomeStatementReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_income_statement";
    public override string Title => "Income Statement";

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
        public List<LineRow> Revenue { get; set; } = new();
        public List<LineRow> Expenses { get; set; } = new();
        public decimal RevenueTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal NetIncome { get; set; }
    }

    /// <summary>Computes the P&amp;L over [from, to]. Exposed for numeric tests.</summary>
    public async Task<Result> ComputeAsync(DateOnly from, DateOnly to)
    {
        var balances = await FinancialLedger.BalancesInRangeAsync(Context, from, to);

        var result = new Result();

        foreach (var b in balances.Where(b => b.AccountType == AccountType.Revenue))
        {
            // Revenue accounts are credit-normal: revenue = credit − debit.
            var amount = b.Credit - b.Debit;
            result.Revenue.Add(new LineRow { Name = b.Name, Amount = amount });
        }

        foreach (var b in balances.Where(b => b.AccountType == AccountType.Expense))
        {
            // Expense accounts are debit-normal: expense = debit − credit.
            var amount = b.Debit - b.Credit;
            result.Expenses.Add(new LineRow { Name = b.Name, Amount = amount });
        }

        result.RevenueTotal = result.Revenue.Sum(r => r.Amount);
        result.ExpenseTotal = result.Expenses.Sum(r => r.Amount);
        result.NetIncome = result.RevenueTotal - result.ExpenseTotal;

        return result;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var to = GetDateOrToday(request, "date_to");
        var from = GetDate(request, "date_from") ?? new DateOnly(to.Year, 1, 1);

        var result = await ComputeAsync(from, to);

        var report = LoadTemplate("income_statement.frx");
        report.RegisterData(result.Revenue.Select(ToDisplay).ToList(), "Revenue");
        report.RegisterData(new[] { new SectionRow { Marker = 1 } }, "ExpSection");
        report.RegisterData(result.Expenses.Select(ToDisplay).ToList(), "Expenses");
        foreach (var name in new[] { "Revenue", "ExpSection", "Expenses" })
        {
            var ds = report.GetDataSource(name);
            if (ds != null)
                ds.Enabled = true;
        }

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PeriodText", $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        report.SetParameterValue("RevenueTotal", result.RevenueTotal.ToString(MoneyFormat));
        report.SetParameterValue("ExpenseTotal", result.ExpenseTotal.ToString(MoneyFormat));
        report.SetParameterValue("NetIncome", result.NetIncome.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Income_Statement_{from:yyyyMMdd}_{to:yyyyMMdd}"
        };
    }

    private static DisplayRow ToDisplay(LineRow r) => new()
    {
        Name = r.Name,
        Amount = r.Amount.ToString(MoneyFormat)
    };
}
