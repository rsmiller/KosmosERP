using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Accounting;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>
/// Financial statement tests (#14 P&amp;L, #15 Balance Sheet, #16 Trial Balance) driven off a
/// small balanced double-entry fixture, asserting numeric correctness (spec §10): trial-balance
/// debits == credits, P&amp;L net income, and Assets = Liabilities + Equity.
/// </summary>
public class FinancialReportTests
{
    private static readonly DateOnly AsOf = new(2026, 12, 31);
    private static readonly DateOnly From = new(2026, 1, 1);

    private ERPDbContext _Context = null!;
    private TrialBalanceReport _Tb = null!;
    private IncomeStatementReport _Pl = null!;
    private BalanceSheetReport _Bs = null!;
    private IReportService _Reports = null!;
    private int _txnId = 1;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _Context = new ERPDbContext(options);

        _Context.Settings.Add(CommonDataHelper<Settings>.FillCommonFields(new Settings
        {
            company_name = "Kosmos Manufacturing Inc.",
            company_address1 = "100 Industrial Way",
            company_city = "Springfield",
            company_state = "IL",
            company_zip = "62701",
            company_country = "USA",
            company_phone = "555-0100",
            company_general_email = "info@kosmos.example",
            company_ar_email = "ar@kosmos.example",
            company_website = "kosmos.example",
            tax_id = "12-3456789",
            fiscal_year_start = "01-01",
        }, 1));

        // Chart of accounts (explicit ids; FinancialTransaction references these).
        AddAccount(1000, "1000", "Cash", AccountType.Asset, NormalBalance.Debit);
        AddAccount(1200, "1200", "Accounts Receivable", AccountType.Asset, NormalBalance.Debit);
        AddAccount(2000, "2000", "Accounts Payable", AccountType.Liability, NormalBalance.Credit);
        AddAccount(3000, "3000", "Common Stock", AccountType.Equity, NormalBalance.Credit);
        AddAccount(4000, "4000", "Sales Revenue", AccountType.Revenue, NormalBalance.Credit);
        AddAccount(5000, "5000", "Cost of Goods Sold", AccountType.Expense, NormalBalance.Debit);
        await _Context.SaveChangesAsync();

        var d = new DateTime(2026, 6, 15);
        // T1 owner investment: Dr Cash 1000 / Cr Common Stock 1000
        AddTxn(d, 1000, debit: 1000m, credit: 0m);
        AddTxn(d, 3000, debit: 0m, credit: 1000m);
        // T2 credit sale: Dr AR 500 / Cr Sales 500
        AddTxn(d, 1200, debit: 500m, credit: 0m);
        AddTxn(d, 4000, debit: 0m, credit: 500m);
        // T3 collection: Dr Cash 200 / Cr AR 200
        AddTxn(d, 1000, debit: 200m, credit: 0m);
        AddTxn(d, 1200, debit: 0m, credit: 200m);
        // T4 COGS on credit: Dr COGS 300 / Cr AP 300
        AddTxn(d, 5000, debit: 300m, credit: 0m);
        AddTxn(d, 2000, debit: 0m, credit: 300m);
        // T5 pay AP: Dr AP 100 / Cr Cash 100
        AddTxn(d, 2000, debit: 100m, credit: 0m);
        AddTxn(d, 1000, debit: 0m, credit: 100m);
        await _Context.SaveChangesAsync();

        _Tb = new TrialBalanceReport(_Context);
        _Pl = new IncomeStatementReport(_Context);
        _Bs = new BalanceSheetReport(_Context);
        _Reports = new ReportService(new IReportGenerator[] { _Tb, _Pl, _Bs });
    }

    private void AddAccount(int id, string number, string name, int type, int normal)
    {
        _Context.ChartOfAccounts.Add(CommonDataHelper<ChartOfAccount>.FillCommonFields(new ChartOfAccount
        {
            id = id,
            account_number = number,
            account_name = name,
            account_type = type,
            normal_balance = normal,
            is_active = true,
        }, 1));
    }

    private void AddTxn(DateTime date, int accountId, decimal debit, decimal credit)
    {
        _Context.FinancialTransactions.Add(CommonDataHelper<FinancialTransaction>.FillCommonFields(new FinancialTransaction
        {
            id = _txnId++,
            transaction_date = date,
            transaction_type = FinancialTransactionType.JournalEntry,
            source_module = "TEST",
            source_id = 1,
            source_guid = Guid.NewGuid().ToString(),
            chart_of_account_id = accountId,
            debit_amount = debit,
            credit_amount = credit,
        }, 1));
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public async Task TrialBalance_DebitsEqualCredits()
    {
        var (rows, debitTotal, creditTotal) = await _Tb.ComputeAsync(AsOf);

        Assert.That(debitTotal, Is.EqualTo(creditTotal), "trial balance must tie");
        Assert.That(debitTotal, Is.EqualTo(1700m));
        Assert.That(rows, Has.Count.EqualTo(6));
    }

    [Test]
    public async Task IncomeStatement_NetIncome_IsCorrect()
    {
        var result = await _Pl.ComputeAsync(From, AsOf);

        Assert.Multiple(() =>
        {
            Assert.That(result.RevenueTotal, Is.EqualTo(500m));
            Assert.That(result.ExpenseTotal, Is.EqualTo(300m));
            Assert.That(result.NetIncome, Is.EqualTo(200m));
        });
    }

    [Test]
    public async Task BalanceSheet_Balances()
    {
        var result = await _Bs.ComputeAsync(AsOf);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalAssets, Is.EqualTo(1400m));
            Assert.That(result.TotalLiabilities, Is.EqualTo(200m));
            Assert.That(result.NetIncome, Is.EqualTo(200m));
            Assert.That(result.TotalEquity, Is.EqualTo(1200m));
            Assert.That(result.Balances, Is.True);
            Assert.That(result.TotalAssets, Is.EqualTo(result.LiabilitiesPlusEquity));
        });
    }

    [Test]
    public async Task Financials_RenderPdf()
    {
        ReportRenderGuard.Require();

        foreach (var key in new[] { "trial_balance", "income_statement", "balance_sheet" })
        {
            var result = await _Reports.GenerateAsync(new ReportRequest
            {
                ReportKey = key,
                Format = ReportFormat.Pdf,
                Parameters = { ["as_of_date"] = "2026-12-31", ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" },
            });
            Assert.That(result.Success, Is.True, $"{key}: {result.Error}");
            Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"), key);
        }
    }

    [Test]
    public async Task Financials_RenderHtml()
    {
        ReportRenderGuard.Require();

        foreach (var key in new[] { "trial_balance", "income_statement", "balance_sheet" })
        {
            var result = await _Reports.GenerateAsync(new ReportRequest
            {
                ReportKey = key,
                Format = ReportFormat.Html,
                Parameters = { ["as_of_date"] = "2026-12-31", ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" },
            });
            Assert.That(result.Success, Is.True, $"{key}: {result.Error}");
            Assert.That(result.ContentType, Is.EqualTo("text/html"), key);
        }
    }
}
