using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Purchasing;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>Render tests for the Purchasing reports.</summary>
public class PurchasingReportTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;
    private PurchaseOrderHeader _Po = null!;

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
            company_ap_email = "ap@kosmos.example",
            company_general_email = "info@kosmos.example",
            company_website = "kosmos.example",
            tax_id = "12-3456789",
            fiscal_year_start = "01-01",
        }, 1));

        // Explicit ids — the PO header configures relationships with HasPrincipalKey(c => c.id),
        // which disables EF InMemory key generation (production MySQL auto-increment is unaffected).
        var address = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            id = 20,
            street_address1 = "500 Supplier Blvd",
            city = "Peoria",
            state = "IL",
            postal_code = "61601",
            country = "USA",
        }, 1);
        _Context.Addresses.Add(address);

        var vendor = CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        {
            id = 30,
            vendor_number = 8001,
            vendor_name = "Global Components Co.",
            address_id = address.id,
            phone = "555-7000",
            general_email = "sales@globalcomponents.example",
            category = "Raw Materials",
        }, 1);
        _Context.Vendors.Add(vendor);
        await _Context.SaveChangesAsync();

        _Po = CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader
        {
            id = 200,
            vendor_id = vendor.id,
            po_type = "Standard",
            po_number = 4001,
            price = 750m,
            tax = 60m,
        }, 1);
        _Context.PurchaseOrderHeaders.Add(_Po);
        await _Context.SaveChangesAsync();

        _Context.PurchaseOrderLines.Add(CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine
        {
            id = 2001,
            purchase_order_header_id = _Po.id,
            product_id = 1,
            line_number = 1,
            description = "Steel Rod, 10mm",
            quantity = 50,
            unit_price = 10m,
        }, 1));
        _Context.PurchaseOrderLines.Add(CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine
        {
            id = 2002,
            purchase_order_header_id = _Po.id,
            product_id = 2,
            line_number = 2,
            description = "Bearing Assembly",
            quantity = 25,
            unit_price = 10m,
        }, 1));
        await _Context.SaveChangesAsync();

        var generators = new IReportGenerator[] { new PurchaseOrderReport(_Context) };
        _Reports = new ReportService(generators);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludesPurchaseOrder()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("purchase_order"));
    }

    [Test]
    public async Task PurchaseOrder_RendersPdf()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "purchase_order",
            Format = ReportFormat.Pdf,
            Parameters = { ["purchase_order_id"] = _Po.id },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task PurchaseOrder_RendersHtml_ByGuid()
    {
        ReportRenderGuard.Require();

        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "purchase_order",
            Format = ReportFormat.Html,
            Parameters = { ["purchase_order_guid"] = _Po.guid },
        });

        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task PurchaseOrder_MissingPo_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "purchase_order",
            Parameters = { ["purchase_order_id"] = 999999 },
        });

        Assert.That(result.Success, Is.False);
    }
}
