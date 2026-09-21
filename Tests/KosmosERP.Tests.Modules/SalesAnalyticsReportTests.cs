using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Purchasing;
using KosmosERP.Reporting.Reports.Sales;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>Render tests for the analytical reports: Sales by Customer (#3), Sales by Product (#4), Open PO/Receiving (#10).</summary>
public class SalesAnalyticsReportTests
{
    private ERPDbContext _Context = null!;
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
            company_city = "Springfield", company_state = "IL", company_zip = "62701", company_country = "USA",
            company_phone = "555-0100", company_general_email = "info@kosmos.example", company_ar_email = "ar@kosmos.example",
            company_website = "kosmos.example", tax_id = "12-3456789", fiscal_year_start = "01-01",
        }, 1));

        _Context.Customers.Add(CommonDataHelper<Customer>.FillCommonFields(new Customer
        { id = 1, customer_number = 1, customer_name = "Acme Widgets LLC", phone = "1", accounting_email = "a@x", category = "B", payment_terms = "NET 30" }, 1));
        _Context.Customers.Add(CommonDataHelper<Customer>.FillCommonFields(new Customer
        { id = 2, customer_number = 2, customer_name = "Beta Industries", phone = "1", accounting_email = "b@x", category = "B", payment_terms = "NET 30" }, 1));

        AddProduct(1, "FG-1", "Gearbox Assembly", "Finished Goods");
        AddProduct(2, "RM-2", "Gear, 12-tooth", "Components");

        AddOrder(100, 1, new DateOnly(2026, 3, 1), 1000m, (1, 5, 100m), (2, 5, 100m));
        AddOrder(101, 1, new DateOnly(2026, 4, 1), 1000m, (1, 5, 100m));
        AddOrder(102, 2, new DateOnly(2026, 5, 1), 500m, (2, 5, 100m));
        await _Context.SaveChangesAsync();

        // Purchasing: one open PO with partial receipt.
        _Context.Vendors.Add(CommonDataHelper<Vendor>.FillCommonFields(new Vendor
        { id = 30, vendor_number = 30, vendor_name = "Global Components Co.", address_id = 0, phone = "1", category = "RM" }, 1));
        _Context.PurchaseOrderHeaders.Add(CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader
        { id = 200, vendor_id = 30, po_type = "Standard", po_number = 4001, price = 750m, tax = 0m, is_complete = false }, 1));
        _Context.PurchaseOrderLines.AddRange(
            CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine
            { id = 2001, purchase_order_header_id = 200, product_id = 1, line_number = 1, description = "Steel Rod", quantity = 50, unit_price = 10m }, 1),
            CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine
            { id = 2002, purchase_order_header_id = 200, product_id = 2, line_number = 2, description = "Bearing", quantity = 25, unit_price = 10m }, 1));
        _Context.PurchaseOrderReceiveHeaders.Add(CommonDataHelper<PurchaseOrderReceiveHeader>.FillCommonFields(new PurchaseOrderReceiveHeader
        { id = 500, purchase_order_id = 200, units_ordered = 75, units_received = 30, is_complete = false }, 1));
        await _Context.SaveChangesAsync();

        _Reports = new ReportService(new IReportGenerator[]
        {
            new SalesByCustomerReport(_Context),
            new SalesByProductReport(_Context),
            new OpenPoReceivingReport(_Context),
        });
    }

    private void AddProduct(int id, string sku, string name, string category)
    {
        _Context.Products.Add(CommonDataHelper<Product>.FillCommonFields(new Product
        { id = id, category = category, product_class = "P", identifier1 = sku, product_name = name, internal_description = name }, 1));
    }

    private void AddOrder(int id, int customerId, DateOnly date, decimal price, params (int productId, int qty, decimal price)[] lines)
    {
        _Context.OrderHeaders.Add(CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader
        {
            id = id, order_number = id, customer_id = customerId, billing_address_id = 0, ship_to_address_id = 0,
            shipping_method = "Ground", order_type = "Standard", pay_method = "Net Terms",
            order_date = date, required_date = date.AddDays(14), price = price,
        }, 1));

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

    [Test]
    public async Task SalesByCustomer_RendersPdf()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "sales_by_customer",
            Format = ReportFormat.Pdf,
            Parameters = { ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" },
        });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task SalesByProduct_RendersHtml()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "sales_by_product",
            Format = ReportFormat.Html,
            Parameters = { ["date_from"] = "2026-01-01", ["date_to"] = "2026-12-31" },
        });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task OpenPoReceiving_RendersPdf()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "open_po_receiving", Format = ReportFormat.Pdf });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }
}
