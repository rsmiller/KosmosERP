using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Purchasing;

/// <summary>
/// Report #9 — Purchase Order Document. Vendor block, ship-to (our company), line items
/// (item, qty, unit cost, ext), and totals. Selected by <c>purchase_order_id</c> or
/// <c>purchase_order_guid</c>. Ship-to reuses the company header parameters.
/// </summary>
public sealed class PurchaseOrderReport : ReportGeneratorBase
{
    public const string Key = "purchase_order";
    private const string MoneyFormat = "#,##0.00";

    public PurchaseOrderReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_purchase_order";
    public override string Title => "Purchase Order";

    public sealed class LineRow
    {
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string UnitCost { get; set; } = string.Empty;
        public string Extended { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var header = await ResolvePurchaseOrderAsync(request);
        if (header == null)
            throw new InvalidOperationException("Purchase order not found. Supply a valid 'purchase_order_id' or 'purchase_order_guid'.");

        var lines = await Context.PurchaseOrderLines
            .AsNoTracking()
            .Where(l => l.purchase_order_header_id == header.id && l.is_deleted == false)
            .OrderBy(l => l.line_number)
            .ToListAsync();

        var vendor = await Context.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.id == header.vendor_id);

        var vendorAddress = vendor != null
            ? await ResolveAddressAsync(vendor.address_id)
            : (street: string.Empty, cityStateZip: string.Empty);

        var rows = lines.Select(l => new LineRow
        {
            LineNumber = l.line_number,
            Description = l.description,
            Qty = l.quantity,
            UnitCost = l.unit_price.ToString(MoneyFormat),
            Extended = (l.quantity * l.unit_price).ToString(MoneyFormat)
        }).ToList();

        var subtotal = lines.Sum(l => l.quantity * l.unit_price);
        var tax = header.tax;
        var total = subtotal + tax;

        var report = LoadTemplate("purchase_order.frx");
        report.RegisterData(rows, "PoLines");
        var dataSource = report.GetDataSource("PoLines");
        if (dataSource != null)
            dataSource.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("PoNumber", header.po_number.ToString());
        report.SetParameterValue("PoDate", header.created_on.ToString("yyyy-MM-dd"));
        report.SetParameterValue("VendorName", vendor?.vendor_name ?? string.Empty);
        report.SetParameterValue("VendorAddress", vendorAddress.street);
        report.SetParameterValue("VendorCityStateZip", vendorAddress.cityStateZip);
        report.SetParameterValue("VendorPhone", vendor?.phone ?? string.Empty);
        report.SetParameterValue("Subtotal", subtotal.ToString(MoneyFormat));
        report.SetParameterValue("Tax", tax.ToString(MoneyFormat));
        report.SetParameterValue("Total", total.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Purchase_Order_{header.po_number}"
        };
    }

    private async Task<PurchaseOrderHeader?> ResolvePurchaseOrderAsync(ReportRequest request)
    {
        if (TryGetInt(request, "purchase_order_id", out var id) && id > 0)
        {
            return await Context.PurchaseOrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.id == id && h.is_deleted == false);
        }

        var guid = GetString(request, "purchase_order_guid");
        if (!string.IsNullOrWhiteSpace(guid))
        {
            return await Context.PurchaseOrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.guid == guid && h.is_deleted == false);
        }

        return null;
    }
}
