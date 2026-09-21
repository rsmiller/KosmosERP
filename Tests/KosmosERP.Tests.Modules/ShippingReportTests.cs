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

/// <summary>Render tests for the Packing Slip / shipping document.</summary>
public class ShippingReportTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;
    private ShipmentHeader _Shipment = null!;

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

        // Explicit ids — the shipment/order headers use HasPrincipalKey(c => c.id), which
        // disables EF InMemory key generation (production MySQL auto-increment is unaffected).
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

        var shipAddress = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            id = 11,
            street_address1 = "9 Dock Road",
            city = "Gary",
            state = "IN",
            postal_code = "46402",
            country = "USA",
        }, 1);
        _Context.Addresses.Add(shipAddress);

        var order = CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader
        {
            id = 100,
            order_number = 7001,
            customer_id = customer.id,
            billing_address_id = shipAddress.id,
            ship_to_address_id = shipAddress.id,
            shipping_method = "Ground",
            order_type = "Standard",
            pay_method = "Net Terms",
            order_date = new DateOnly(2026, 9, 1),
            required_date = new DateOnly(2026, 9, 15),
            price = 1000m,
        }, 1);
        _Context.OrderHeaders.Add(order);

        _Context.OrderLines.AddRange(
            CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
            {
                id = 1001,
                order_header_id = order.id,
                product_id = 1,
                line_number = 1,
                line_description = "Widget, Standard",
                quantity = 10,
                unit_price = 100m,
            }, 1),
            CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
            {
                id = 1002,
                order_header_id = order.id,
                product_id = 2,
                line_number = 2,
                line_description = "Widget, Deluxe",
                quantity = 5,
                unit_price = 100m,
            }, 1));
        await _Context.SaveChangesAsync();

        _Shipment = CommonDataHelper<ShipmentHeader>.FillCommonFields(new ShipmentHeader
        {
            id = 300,
            order_header_id = order.id,
            shipment_number = 6001,
            address_id = shipAddress.id,
            ship_via = "Ground",
            ship_attn = "Receiving Dept",
            freight_carrier = "UPS",
            units_shipped = 15,
        }, 1);
        _Context.ShipmentHeaders.Add(_Shipment);
        await _Context.SaveChangesAsync();

        _Context.ShipmentLines.AddRange(
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine
            {
                id = 3001,
                shipment_header_id = _Shipment.id,
                order_line_id = 1001,
                units_to_ship = 10,
                units_shipped = 10,
            }, 1),
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine
            {
                id = 3002,
                shipment_header_id = _Shipment.id,
                order_line_id = 1002,
                units_to_ship = 5,
                units_shipped = 5,
            }, 1));
        await _Context.SaveChangesAsync();

        var generators = new IReportGenerator[] { new PackingSlipReport(_Context) };
        _Reports = new ReportService(generators);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludesPackingSlip()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("packing_slip"));
    }

    [Test]
    public async Task PackingSlip_RendersPdf()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "packing_slip",
            Format = ReportFormat.Pdf,
            Parameters = { ["shipment_id"] = _Shipment.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task PackingSlip_RendersHtml()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "packing_slip",
            Format = ReportFormat.Html,
            Parameters = { ["shipment_id"] = _Shipment.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task PackingSlip_MissingShipment_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "packing_slip",
            Parameters = { ["shipment_id"] = 999999 },
        });

        Assert.That(result.Success, Is.False);
    }
}
