using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Manufacturing;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>Render tests for the Manufacturing reports.</summary>
public class ManufacturingReportTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;
    private ProductionOrderHeader _Po = null!;

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

        // Explicit ids — transactional headers use HasPrincipalKey(c => c.id), which disables
        // EF InMemory key generation (production MySQL auto-increment is unaffected).
        var finished = CommonDataHelper<Product>.FillCommonFields(new Product
        {
            id = 1,
            category = "Finished Goods",
            product_class = "FG",
            identifier1 = "GBX-001",
            product_name = "Gearbox Assembly",
            internal_description = "Complete gearbox",
        }, 1);
        var component = CommonDataHelper<Product>.FillCommonFields(new Product
        {
            id = 2,
            category = "Components",
            product_class = "RM",
            identifier1 = "GEAR-12",
            product_name = "Gear, 12-tooth",
            internal_description = "Steel gear",
        }, 1);
        _Context.Products.AddRange(finished, component);

        _Context.BOMs.Add(CommonDataHelper<BOM>.FillCommonFields(new BOM
        {
            id = 50,
            parent_product_id = finished.id,
            product_id = component.id,
            order_number = 1,
            quantity = 4,
        }, 1));

        var order = CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader
        {
            id = 100,
            order_number = 7001,
            customer_id = 1,
            billing_address_id = 0,
            ship_to_address_id = 0,
            shipping_method = "Ground",
            order_type = "Standard",
            pay_method = "Net Terms",
            order_date = new DateOnly(2026, 9, 1),
            required_date = new DateOnly(2026, 9, 15),
            price = 1000m,
        }, 1);
        _Context.OrderHeaders.Add(order);
        _Context.OrderLines.Add(CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
        {
            id = 1001,
            order_header_id = order.id,
            product_id = finished.id,
            line_number = 1,
            line_description = "Gearbox Assembly",
            quantity = 10,
            unit_price = 100m,
        }, 1));
        await _Context.SaveChangesAsync();

        _Po = CommonDataHelper<ProductionOrderHeader>.FillCommonFields(new ProductionOrderHeader
        {
            id = 400,
            order_header_id = order.id,
            status = "Released",
            planned_start_date = new DateOnly(2026, 9, 2),
            planned_complete_date = new DateOnly(2026, 9, 10),
        }, 1);
        _Context.ProductionOrderHeaders.Add(_Po);
        await _Context.SaveChangesAsync();

        _Context.ProductionOrderLines.Add(CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine
        {
            id = 4001,
            production_order_header_id = _Po.id,
            order_line_id = 1001,
            line_number = 1,
            quantity = 10,
            status = "Pending",
        }, 1));
        await _Context.SaveChangesAsync();

        var generators = new IReportGenerator[] { new ProductionOrderTravelerReport(_Context) };
        _Reports = new ReportService(generators);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludesTraveler()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("production_order_traveler"));
    }

    [Test]
    public async Task Traveler_RendersPdf()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "production_order_traveler",
            Format = ReportFormat.Pdf,
            Parameters = { ["production_order_id"] = _Po.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task Traveler_RendersHtml()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "production_order_traveler",
            Format = ReportFormat.Html,
            Parameters = { ["production_order_id"] = _Po.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task Traveler_MissingOrder_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "production_order_traveler",
            Parameters = { ["production_order_id"] = 999999 },
        });

        Assert.That(result.Success, Is.False);
    }
}
