using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KosmosERP.BusinessLayer;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Reporting;

namespace KosmosERP.Tests.Modules;

/// <summary>
/// Guards that every report is actually wired into DI. Builds the real service collection via
/// AddReporting() and asserts the resolved IReportService exposes all 16 spec report keys — so a
/// generator that was authored but never registered is caught here.
/// </summary>
public class ReportRegistryTests
{
    private static readonly string[] ExpectedKeys =
    {
        // Sales & Shipping
        "sales_order_ack", "packing_slip", "sales_by_customer", "sales_by_product",
        // Inventory & Manufacturing
        "inventory_stock_status", "inventory_reorder", "bom", "production_order_traveler",
        // Purchasing
        "purchase_order", "open_po_receiving",
        // Accounting / Finance
        "ar_invoice", "ar_aging", "ap_aging", "income_statement", "balance_sheet", "trial_balance",
    };

    private ServiceProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDbContext<IBaseERPContext, ERPDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        // The API startup registers this; supply the MOCK log factory here so ReportingModule resolves.
        services.AddScoped<ILogProviderFactory>(sp =>
            new LogProviderFactory(new LogProviderSettings { log_provider = LogProviderType.MOCK },
                (ERPDbContext)sp.GetRequiredService<IBaseERPContext>(), null));
        services.AddReporting();
        _provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => _provider.Dispose();

    [Test]
    public void AllSixteenReports_AreRegistered()
    {
        using var scope = _provider.CreateScope();
        var reports = scope.ServiceProvider.GetRequiredService<IReportService>();

        Assert.That(reports.AvailableReportKeys, Has.Count.EqualTo(16), "expected exactly 16 registered reports");
        Assert.That(reports.AvailableReportKeys, Is.EquivalentTo(ExpectedKeys));
    }

    [Test]
    public void ReportingModule_Resolves()
    {
        using var scope = _provider.CreateScope();
        var module = scope.ServiceProvider.GetService<IReportingModule>();
        Assert.That(module, Is.Not.Null);
    }
}
