using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.CRM;
using KosmosERP.Reporting.Reports.Purchasing;
using KosmosERP.Reporting.Reports.Sales;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>
/// Render tests for the additional general reports: Top Opportunities (CRM), Top Salespeople,
/// Products Awaiting Shipment, Recently Shipped (Sales/Shipping), Recent Purchase Orders and
/// Critical Vendors (Purchasing). Seeds a small cross-domain dataset and asserts each renders.
/// </summary>
public class AdditionalReportsTests
{
    private ERPDbContext _Context = null!;
    private IReportService _Reports = null!;

    private const string SalespersonExternalId = "ext-jane";

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
            company_city = "Springfield", company_state = "IL", company_zip = "62701", company_country = "USA",
            company_phone = "555-0100", company_general_email = "info@kosmos.example", company_ar_email = "ar@kosmos.example",
            company_website = "kosmos.example", tax_id = "12-3456789", fiscal_year_start = "01-01",
        }, 1));

        _Context.Users.Add(CommonDataHelper<User>.FillCommonFields(new User
        {
            id = 1, first_name = "Jane", last_name = "Seller", username = "jane", password = "x", password_salt = "x",
            employee_number = "E1", external_id = SalespersonExternalId,
        }, 1));

        _Context.Customers.Add(CommonDataHelper<Customer>.FillCommonFields(new Customer
        { id = 1, customer_number = 1, customer_name = "Acme Widgets LLC", phone = "1", accounting_email = "a@x", category = "B", payment_terms = "NET 30" }, 1));

        _Context.Contacts.Add(CommonDataHelper<Contact>.FillCommonFields(new Contact
        { id = 1, customer_id = 1, first_name = "Al", last_name = "Buyer", email = "al@x", phone = "1" }, 1));

        AddProduct(1, "FG-1", "Gearbox Assembly", "Finished Goods");
        AddProduct(2, "RM-2", "Gear, 12-tooth", "Components");

        // Two orders attributed to Jane (created_by == her external_id), left open/unshipped-ish.
        AddOrder(100, 1, new DateOnly(2026, 3, 1), 1000m, (1, 10, 100m), (2, 5, 100m));
        AddOrder(101, 1, new DateOnly(2026, 4, 1), 500m, (1, 5, 100m));
        await _Context.SaveChangesAsync();

        // A completed shipment (partial) for order 100 line 1001: ships 4 of 10.
        var shipment = CommonDataHelper<ShipmentHeader>.FillCommonFields(new ShipmentHeader
        {
            id = 300, order_header_id = 100, shipment_number = 6001, address_id = 0,
            ship_via = "Ground", freight_carrier = "carrier_ups", units_shipped = 4,
            is_complete = true, completed_on = new DateTime(2026, 5, 1),
        }, 1);
        _Context.ShipmentHeaders.Add(shipment);
        await _Context.SaveChangesAsync();

        _Context.ShipmentLines.Add(CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine
        { id = 3001, shipment_header_id = 300, order_line_id = 1001, units_to_ship = 10, units_shipped = 4 }, 1));

        // Carrier display name for Recently Shipped.
        _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore
        { id = 900, key = "carrier_ups", value = "UPS", module_id = "freight" }, 1));

        // Opportunities: one open, one closed (should be excluded), plus stage lookup.
        _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore
        { id = 901, key = "opporunity_stage_proposal", value = "Proposal", module_id = "opp" }, 1));
        _Context.Opportunities.AddRange(
            CommonDataHelper<Opportunity>.FillCommonFields(new Opportunity
            { id = 10, opportunity_name = "Big Deal", customer_id = 1, contact_id = 1, amount = 50000m, stage = "opporunity_stage_proposal", win_chance = 60, expected_close = new DateOnly(2026, 12, 1), owner_id = SalespersonExternalId }, 1),
            CommonDataHelper<Opportunity>.FillCommonFields(new Opportunity
            { id = 11, opportunity_name = "Old Won Deal", customer_id = 1, contact_id = 1, amount = 90000m, stage = "opporunity_stage_closed_won", win_chance = 100, expected_close = new DateOnly(2026, 1, 1), owner_id = SalespersonExternalId }, 1));

        // Purchasing: a critical vendor with an open PO.
        _Context.Vendors.Add(CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        { id = 30, vendor_number = 30, vendor_name = "Global Components Co.", address_id = 0, phone = "555-1", general_email = "sales@global.example", category = "RM", is_critial_vendor = true }, 1));
        _Context.Vendors.Add(CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        { id = 31, vendor_number = 31, vendor_name = "Non Critical Co.", address_id = 0, phone = "555-2", category = "RM", is_critial_vendor = false }, 1));
        _Context.PurchaseOrderHeaders.Add(CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader
        { id = 200, vendor_id = 30, po_type = "St", po_number = 4001, price = 750m, tax = 50m, is_complete = false }, 1));
        await _Context.SaveChangesAsync();

        _Reports = new ReportService(new IReportGenerator[]
        {
            new TopOpportunitiesReport(_Context),
            new TopSalespeopleReport(_Context),
            new ProductsAwaitingShipmentReport(_Context),
            new RecentlyShippedReport(_Context),
            new RecentPurchaseOrdersReport(_Context),
            new CriticalVendorsReport(_Context),
        });
    }

    private void AddProduct(int id, string sku, string name, string category)
    {
        _Context.Products.Add(CommonDataHelper<Product>.FillCommonFields(new Product
        { id = id, category = category, product_class = "P", identifier1 = sku, product_name = name, internal_description = name }, 1));
    }

    private void AddOrder(int id, int customerId, DateOnly date, decimal price, params (int productId, int qty, decimal price)[] lines)
    {
        // created_by is set to Jane's external_id so Top Salespeople attributes these to her.
        _Context.OrderHeaders.Add(CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader
        {
            id = id, order_number = id, customer_id = customerId, billing_address_id = 0, ship_to_address_id = 0,
            shipping_method = "Ground", order_type = "St", pay_method = "Net Terms",
            order_date = date, required_date = date.AddDays(14), price = price,
        }, SalespersonExternalId));

        var lineNo = 1;
        foreach (var l in lines)
        {
            _Context.OrderLines.Add(CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine
            {
                id = id * 10 + lineNo,
                order_header_id = id,
                product_id = l.productId,
                line_number = lineNo++,
                line_description = $"Line {l.productId}",
                quantity = l.qty,
                unit_price = l.price,
            }, 1));
        }
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    private async Task AssertRendersPdf(string key, Dictionary<string, object>? parameters = null)
    {
        ReportRenderGuard.Require();
        var request = new ReportRequest { ReportKey = key, Format = ReportFormat.Pdf };
        if (parameters != null)
            foreach (var kvp in parameters) request.Parameters[kvp.Key] = kvp.Value;

        var result = await _Reports.GenerateAsync(request);
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public Task TopOpportunities_RendersPdf() => AssertRendersPdf("top_opportunities");

    [Test]
    public async Task TopOpportunities_RendersHtml()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "top_opportunities", Format = ReportFormat.Html, Parameters = { ["top"] = 10 } });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public Task TopSalespeople_RendersPdf() => AssertRendersPdf("top_salespeople", new() { ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" });

    [Test]
    public Task ProductsAwaitingShipment_RendersPdf() => AssertRendersPdf("products_awaiting_shipment");

    [Test]
    public Task RecentlyShipped_RendersPdf() => AssertRendersPdf("recently_shipped", new() { ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" });

    [Test]
    public Task RecentPurchaseOrders_RendersPdf() => AssertRendersPdf("recent_purchase_orders", new() { ["date_from"] = "2020-01-01", ["date_to"] = "2030-12-31" });

    [Test]
    public Task CriticalVendors_RendersPdf() => AssertRendersPdf("critical_vendors");
}
