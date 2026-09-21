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
/// Reporting engine tests. These also serve as the Linux headless-render smoke test required by
/// the spec (§2.1 / §10): rendering exercises System.Drawing.Common + libgdiplus on the CI runner,
/// so a broken GDI setup fails here rather than in production.
/// </summary>
public class ReportingTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;
    private ARInvoiceHeader _Invoice = null!;

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
            company_address2 = "Suite 200",
            company_city = "Springfield",
            company_state = "IL",
            company_zip = "62701",
            company_country = "USA",
            company_phone = "555-0100",
            company_ar_email = "ar@kosmos.example",
            company_general_email = "info@kosmos.example",
            company_website = "kosmos.example",
            tax_id = "12-3456789",
            fiscal_year_start = "01-01",
        }, 1));

        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer
        {
            customer_number = 5001,
            customer_name = "Acme Widgets LLC",
            phone = "555-2000",
            accounting_email = "ap@acme.example",
            category = "Business",
            payment_terms = "NET 30",
            is_taxable = true,
            tax_rate = 0.08m,
        }, 1);
        _Context.Customers.Add(customer);
        await _Context.SaveChangesAsync();

        var address = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            street_address1 = "42 Market Street",
            city = "Chicago",
            state = "IL",
            postal_code = "60601",
            country = "USA",
        }, 1);
        _Context.Addresses.Add(address);
        await _Context.SaveChangesAsync();

        _Context.CustomerAddresses.Add(CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = customer.id,
            address_id = address.id,
            address_type_id = 1,
        }, 1));

        _Invoice = CommonDataHelper<ARInvoiceHeader>.FillCommonFields(new ARInvoiceHeader
        {
            customer_id = customer.id,
            order_header_id = 1,
            invoice_number = 9001,
            payment_terms = "NET 30",
            invoice_date = new DateOnly(2026, 9, 1),
            invoice_due_date = new DateOnly(2026, 10, 1),
            invoice_total = 1080.00m,
            tax_percentage = 8m,
            is_taxable = true,
        }, 1);
        _Context.ARInvoiceHeaders.Add(_Invoice);
        await _Context.SaveChangesAsync();

        _Context.ARInvoiceLines.Add(CommonDataHelper<ARInvoiceLine>.FillCommonFields(new ARInvoiceLine
        {
            ar_invoice_header_id = _Invoice.id,
            line_number = 1,
            product_id = 1,
            line_description = "Widget, Standard",
            invoice_qty = 10,
            line_total = 500.00m,
            line_tax = 40.00m,
            is_taxable = true,
        }, 1));
        _Context.ARInvoiceLines.Add(CommonDataHelper<ARInvoiceLine>.FillCommonFields(new ARInvoiceLine
        {
            ar_invoice_header_id = _Invoice.id,
            line_number = 2,
            product_id = 2,
            line_description = "Widget, Deluxe",
            invoice_qty = 5,
            line_total = 500.00m,
            line_tax = 40.00m,
            is_taxable = true,
        }, 1));
        await _Context.SaveChangesAsync();

        var generators = new IReportGenerator[] { new ArInvoiceReport(_Context) };
        _Reports = new ReportService(generators);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludesArInvoice()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("ar_invoice"));
    }

    [Test]
    public async Task ArInvoice_RendersPdf_OnLinux()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ar_invoice",
            Format = ReportFormat.Pdf,
            Parameters = { ["ar_invoice_id"] = _Invoice.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(result.Content, Is.Not.Empty);
        Assert.That(result.FileName, Does.EndWith(".pdf"));

        // Valid PDFs start with the "%PDF" magic bytes.
        var header = Encoding.ASCII.GetString(result.Content, 0, 4);
        Assert.That(header, Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task ArInvoice_RendersHtml()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ar_invoice",
            Format = ReportFormat.Html,
            Parameters = { ["ar_invoice_id"] = _Invoice.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
        Assert.That(result.Content, Is.Not.Empty);
        Assert.That(result.FileName, Does.EndWith(".html"));
    }

    [Test]
    public async Task ArInvoice_ByGuid_Resolves()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ar_invoice",
            Format = ReportFormat.Pdf,
            Parameters = { ["ar_invoice_guid"] = _Invoice.guid },
        });

        Assert.That(result.Success, Is.True, result.Error);
    }

    [Test]
    public async Task UnknownReportKey_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "does_not_exist" });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ArInvoice_MissingInvoice_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "ar_invoice",
            Parameters = { ["ar_invoice_id"] = 999999 },
        });

        Assert.That(result.Success, Is.False);
    }
}
