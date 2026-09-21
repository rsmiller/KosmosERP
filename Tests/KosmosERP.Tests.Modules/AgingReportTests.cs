using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Accounting;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>
/// AR/AP aging tests, including numeric-correctness fixtures: invoices with known due dates land
/// in the expected buckets as of a fixed report date (spec §10).
/// </summary>
public class AgingReportTests
{
    private static readonly DateOnly AsOf = new(2026, 9, 30);

    private ERPDbContext _Context = null!;
    private ArAgingReport _ArAging = null!;
    private ApAgingReport _ApAging = null!;
    private IReportService _Reports = null!;

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
            company_ar_email = "ar@kosmos.example",
            company_ap_email = "ap@kosmos.example",
            company_general_email = "info@kosmos.example",
            company_website = "kosmos.example",
            tax_id = "12-3456789",
            fiscal_year_start = "01-01",
        }, 1));

        // Customers
        _Context.Customers.Add(CommonDataHelper<Customer>.FillCommonFields(new Customer
        { id = 1, customer_number = 1, customer_name = "Acme Widgets LLC", phone = "1", accounting_email = "a@x", category = "B", payment_terms = "NET 30" }, 1));
        _Context.Customers.Add(CommonDataHelper<Customer>.FillCommonFields(new Customer
        { id = 2, customer_number = 2, customer_name = "Beta Industries", phone = "1", accounting_email = "b@x", category = "B", payment_terms = "NET 30" }, 1));

        // AR invoices (ids explicit so payments link deterministically).
        AddAr(1001, 1, due: new DateOnly(2026, 9, 30), total: 100m);   // 0 days -> Current
        AddAr(1002, 1, due: new DateOnly(2026, 9, 15), total: 200m);   // 15 days -> 1-30 (open 150 after payment)
        AddAr(1003, 1, due: new DateOnly(2026, 7, 1), total: 300m);    // 91 days -> 90+
        AddAr(1004, 1, due: new DateOnly(2026, 9, 20), total: 500m);   // fully paid -> excluded
        AddAr(1005, 2, due: new DateOnly(2026, 8, 20), total: 400m);   // 41 days -> 31-60
        await _Context.SaveChangesAsync();

        AddPayment(1002, 50m);    // invoice 1002 open = 150
        AddPayment(1004, 500m);   // invoice 1004 fully paid
        await _Context.SaveChangesAsync();

        // Vendors + AP invoices
        _Context.Vendors.Add(CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        { id = 10, vendor_number = 10, vendor_name = "Global Components Co.", address_id = 0, phone = "1", category = "RM" }, 1));
        _Context.Vendors.Add(CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        { id = 20, vendor_number = 20, vendor_name = "Zeta Supply", address_id = 0, phone = "1", category = "RM" }, 1));

        AddAp(10, due: new DateTime(2026, 9, 10), total: 500m, paid: false);  // 20 days -> 1-30
        AddAp(10, due: new DateTime(2026, 9, 5), total: 999m, paid: true);    // paid -> excluded
        AddAp(20, due: new DateTime(2026, 6, 1), total: 700m, paid: false);   // 121 days -> 90+
        await _Context.SaveChangesAsync();

        _ArAging = new ArAgingReport(_Context);
        _ApAging = new ApAgingReport(_Context);
        _Reports = new ReportService(new IReportGenerator[] { _ArAging, _ApAging });
    }

    private void AddAr(int id, int customerId, DateOnly due, decimal total)
    {
        _Context.ARInvoiceHeaders.Add(CommonDataHelper<ARInvoiceHeader>.FillCommonFields(new ARInvoiceHeader
        {
            id = id,
            customer_id = customerId,
            order_header_id = 1,
            invoice_number = id,
            payment_terms = "NET 30",
            invoice_date = due.AddDays(-30),
            invoice_due_date = due,
            invoice_total = total,
        }, 1));
    }

    private void AddPayment(int arHeaderId, decimal amount)
    {
        _Context.Payments.Add(CommonDataHelper<Payment>.FillCommonFields(new Payment
        {
            ar_header_id = arHeaderId,
            order_header_id = arHeaderId,   // distinct per payment (Payment's tracked key)
            payment_number = arHeaderId,
            payment_amount = amount,
            transaction_method = "Card",
            guid = Guid.NewGuid().ToString(),
        }, 1));
    }

    private void AddAp(int vendorId, DateTime due, decimal total, bool paid)
    {
        _Context.APInvoiceHeaders.Add(CommonDataHelper<APInvoiceHeader>.FillCommonFields(new APInvoiceHeader
        {
            vendor_id = vendorId,
            invoice_number = Guid.NewGuid().ToString().Substring(0, 8),
            invoice_date = due.AddDays(-30),
            invoice_due_date = due,
            invoice_received_date = due.AddDays(-30),
            invoice_total = total,
            is_paid = paid,
        }, 1));
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public async Task ArAging_BucketsAndTotals_AreCorrect()
    {
        var rows = await _ArAging.ComputeAsync(AsOf);

        var acme = rows.Single(r => r.EntityId == 1);
        Assert.Multiple(() =>
        {
            Assert.That(acme.Current, Is.EqualTo(100m));
            Assert.That(acme.Days1To30, Is.EqualTo(150m));
            Assert.That(acme.Days31To60, Is.EqualTo(0m));
            Assert.That(acme.Days90Plus, Is.EqualTo(300m));
            Assert.That(acme.Total, Is.EqualTo(550m));
        });

        var beta = rows.Single(r => r.EntityId == 2);
        Assert.That(beta.Days31To60, Is.EqualTo(400m));

        var grand = AgingCalculator.GrandTotal(rows);
        Assert.That(grand.Total, Is.EqualTo(950m));   // 550 + 400; fully-paid invoice excluded
    }

    [Test]
    public async Task ApAging_BucketsAndTotals_AreCorrect()
    {
        var rows = await _ApAging.ComputeAsync(AsOf);

        var global = rows.Single(r => r.EntityId == 10);
        Assert.That(global.Days1To30, Is.EqualTo(500m));
        Assert.That(global.Total, Is.EqualTo(500m));   // paid invoice excluded

        var zeta = rows.Single(r => r.EntityId == 20);
        Assert.That(zeta.Days90Plus, Is.EqualTo(700m));

        var grand = AgingCalculator.GrandTotal(rows);
        Assert.That(grand.Total, Is.EqualTo(1200m));
    }

    [Test]
    public async Task ArAging_RendersPdf()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ar_aging",
            Format = ReportFormat.Pdf,
            Parameters = { ["as_of_date"] = "2026-09-30" },
        });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task ApAging_RendersHtml()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ap_aging",
            Format = ReportFormat.Html,
            Parameters = { ["as_of_date"] = "2026-09-30" },
        });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }
}
