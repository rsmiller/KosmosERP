using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Accounting;
using KosmosERP.Reporting.Reports.CRM;
using KosmosERP.Reporting.Reports.Inventory;
using KosmosERP.Reporting.Reports.Manufacturing;
using KosmosERP.Reporting.Reports.Purchasing;
using KosmosERP.Reporting.Reports.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace KosmosERP.Reporting;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the reporting service, its ERP module identity, and every report generator.
    /// Call alongside <c>AddModules()</c> in API startup. Add one line per new report generator.
    /// </summary>
    public static IServiceCollection AddReporting(this IServiceCollection services)
    {
        services.AddScoped<IReportingModule, ReportingModule>();
        services.AddScoped<IReportService, ReportService>();

        // --- Report generators (one per report) ---
        // Sales & Shipping
        services.AddScoped<IReportGenerator, SalesOrderAcknowledgementReport>();   // #1 SO Acknowledgement
        services.AddScoped<IReportGenerator, PackingSlipReport>();                 // #2 Packing Slip
        services.AddScoped<IReportGenerator, SalesByCustomerReport>();             // #3 Sales by Customer
        services.AddScoped<IReportGenerator, SalesByProductReport>();              // #4 Sales by Product
        services.AddScoped<IReportGenerator, TopSalespeopleReport>();              // #17 Top Salespeople
        services.AddScoped<IReportGenerator, ProductsAwaitingShipmentReport>();    // #18 Products Awaiting Shipment
        services.AddScoped<IReportGenerator, RecentlyShippedReport>();             // #19 Recently Shipped

        // CRM
        services.AddScoped<IReportGenerator, TopOpportunitiesReport>();            // #20 Top Opportunities

        // Inventory & Manufacturing
        services.AddScoped<IReportGenerator, InventoryStockStatusReport>();      // #5 Inventory Stock Status
        services.AddScoped<IReportGenerator, InventoryReorderReport>();          // #6 Inventory Reorder
        services.AddScoped<IReportGenerator, BomReport>();                       // #7 Bill of Materials
        services.AddScoped<IReportGenerator, ProductionOrderTravelerReport>();   // #8 Production Order Traveler

        // Purchasing
        services.AddScoped<IReportGenerator, PurchaseOrderReport>();          // #9 Purchase Order
        services.AddScoped<IReportGenerator, OpenPoReceivingReport>();        // #10 Open PO / Receiving
        services.AddScoped<IReportGenerator, RecentPurchaseOrdersReport>();   // #21 Recent Purchase Orders
        services.AddScoped<IReportGenerator, CriticalVendorsReport>();        // #22 Critical Vendors

        // Accounting / Finance
        services.AddScoped<IReportGenerator, ArInvoiceReport>();   // #11 AR Invoice
        services.AddScoped<IReportGenerator, ArAgingReport>();     // #12 AR Aging
        services.AddScoped<IReportGenerator, ApAgingReport>();     // #13 AP Aging
        services.AddScoped<IReportGenerator, IncomeStatementReport>();  // #14 Income Statement (P&L)
        services.AddScoped<IReportGenerator, BalanceSheetReport>();     // #15 Balance Sheet
        services.AddScoped<IReportGenerator, TrialBalanceReport>();     // #16 Trial Balance

        return services;
    }
}
