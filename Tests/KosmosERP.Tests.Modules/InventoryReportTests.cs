using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Reporting;
using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Inventory;
using KosmosERP.Reporting.Reports.Manufacturing;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules;

/// <summary>Render tests for the Inventory / Manufacturing analytical reports (#5, #6, #7).</summary>
public class InventoryReportTests
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

        AddProduct(1, "FG-1", "Gearbox Assembly", "Finished Goods", 5m);
        AddProduct(2, "RM-2", "Gear, 12-tooth", "Components", 20m);
        _Context.BOMs.Add(CommonDataHelper<BOM>.FillCommonFields(new BOM
        { id = 50, parent_product_id = 1, product_id = 2, order_number = 1, quantity = 4 }, 1));

        AddInventory(1, "Gearbox Assembly", onHand: 100, reorder: 5, toOrder: 0);
        AddInventory(2, "Gear, 12-tooth", onHand: 2, reorder: 10, toOrder: 8);   // low stock
        await _Context.SaveChangesAsync();

        _Reports = new ReportService(new IReportGenerator[]
        {
            new InventoryStockStatusReport(_Context),
            new InventoryReorderReport(_Context),
            new BomReport(_Context),
        });
    }

    private void AddProduct(int id, string sku, string name, string category, decimal unitCost)
    {
        _Context.Products.Add(CommonDataHelper<Product>.FillCommonFields(new Product
        {
            id = id,
            category = category,
            product_class = "P",
            identifier1 = sku,
            product_name = name,
            internal_description = name,
            unit_cost = unitCost,
        }, 1));
    }

    private void AddInventory(int productId, string name, int onHand, int reorder, int toOrder)
    {
        // Inventory does not derive BaseDatabaseModel, so it is constructed directly.
        _Context.InventoryCounts.Add(new Inventory
        {
            product_id = productId,
            product_name = name,
            on_hand = onHand,
            current_stock = onHand,
            reorder_level = reorder,
            to_order = toOrder,
        });
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public void AvailableReportKeys_IncludeInventoryAndBom()
    {
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("inventory_stock_status"));
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("inventory_reorder"));
        Assert.That(_Reports.AvailableReportKeys, Does.Contain("bom"));
    }

    [Test]
    public async Task InventoryStockStatus_RendersPdf()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "inventory_stock_status", Format = ReportFormat.Pdf });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task InventoryReorder_RendersHtml()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "inventory_reorder", Format = ReportFormat.Html });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(result.ContentType, Is.EqualTo("text/html"));
    }

    [Test]
    public async Task Bom_RendersPdf_ByProduct()
    {
        ReportRenderGuard.Require();
        var result = await _Reports.GenerateAsync(new ReportRequest
        {
            ReportKey = "bom",
            Format = ReportFormat.Pdf,
            Parameters = { ["product_id"] = 1 },
        });
        Assert.That(result.Success, Is.True, result.Error);
        Assert.That(Encoding.ASCII.GetString(result.Content, 0, 4), Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task Bom_MissingSelector_Fails()
    {
        var result = await _Reports.GenerateAsync(new ReportRequest { ReportKey = "bom" });
        Assert.That(result.Success, Is.False);
    }
}
