using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Sales;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>Render tests for the Sales &amp; Shipping reports.</summary>
public class SalesReportTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;
    private OrderHeader _Order = null!;

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
            company_general_email = "info@kosmos.example",
            company_website = "kosmos.example",
            tax_id = "12-3456789",
            fiscal_year_start = "01-01",
        }, 1));

        // Explicit ids: the transactional headers configure relationships with
        // HasPrincipalKey(c => c.id), which suppresses EF InMemory key generation (production
        // MySQL auto-increment is unaffected). Assigning ids keeps the seed deterministic.
        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer
        {
            id = 1,
            customer_number = 5001,
            customer_name = "Acme Widgets LLC",
            phone = "555-2000",
            accounting_email = "ap@acme.example",
            category = "Business",
            payment_terms = "NET 30",
        }, 1);
        _Context.Customers.Add(customer);

        var billTo = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            id = 10,
            street_address1 = "42 Market Street",
            city = "Chicago",
            state = "IL",
            postal_code = "60601",
            country = "USA",
        }, 1);
        var shipTo = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            id = 11,
            street_address1 = "9 Dock Road",
            city = "Gary",
            state = "IN",
            postal_code = "46402",
            country = "USA",
        }, 1);
        _Context.Addresses.AddRange(billTo, shipTo);
        await _Context.SaveChangesAsync();

        _Order = CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader
        {
            id = 100,
            order_number = 7001,
            customer_id = customer.id,
            billing_address_id = billTo.id,
            ship_to_address_id = shipTo.id,
            shipping_method = "Ground",
            order_type = "Standard",
            pay_method = "Net Terms",
            po_number = "PO-ABC-123",
            order_date = new DateOnly(2026, 9, 1),
            required_date = new DateOnly(2026, 9, 15),
            price = 1500m,
            tax = 120m,
            shipping_cost = 45m,
        }, 1);
        _Context.OrderHeaders.Add(_Order);
        await _Context.SaveChangesAsync();

        _Context.OrderLines.Add(CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
        {
            id = 1001,
            order_header_id = _Order.id,
            product_id = 1,
            line_number = 1,
            line_description = "Widget, Standard",
            quantity = 10,
            unit_price = 100m,
        }, 1));
        _Context.OrderLines.Add(CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
        {
            id = 1002,
            order_header_id = _Order.id,
            product_id = 2,
            line_number = 2,
            line_description = "Widget, Deluxe",
            quantity = 5,
            unit_price = 100m,
        }, 1));
        await _Context.SaveChangesAsync();

        var generators = new IReportGenerator[] { new SalesOrderAcknowledgementReport(_Context) };
        _Reports = new ReportService(generators);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludesSalesOrderAck()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("sales_order_ack"));
    }

    [Test]
    public async Task SalesOrderAck_RendersPdf()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "sales_order_ack",
            Format = ReportFormat.Pdf,
            Parameters = { ["order_id"] = _Order.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(result.Content, Is.Not.Empty);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task SalesOrderAck_RendersHtml_ByGuid()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "sales_order_ack",
            Format = ReportFormat.Html,
            Parameters = { ["order_guid"] = _Order.guid },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task SalesOrderAck_MissingOrder_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "sales_order_ack",
            Parameters = { ["order_id"] = 999999 },
        });

        Assert.That(result.Success, Is.False);
    }
}
