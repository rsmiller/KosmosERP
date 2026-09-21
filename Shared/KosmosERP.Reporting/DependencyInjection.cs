using KosmosERP.Reporting.Reports;
using KosmosERP.Reporting.Reports.Accounting;
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

        // Inventory & Manufacturing
        services.AddScoped<IReportGenerator, ProductionOrderTravelerReport>();   // #8 Production Order Traveler

        // Purchasing
        services.AddScoped<IReportGenerator, PurchaseOrderReport>();   // #9 Purchase Order

        // Accounting / Finance
        services.AddScoped<IReportGenerator, ArInvoiceReport>();   // #11 AR Invoice

        return services;
    }
}
