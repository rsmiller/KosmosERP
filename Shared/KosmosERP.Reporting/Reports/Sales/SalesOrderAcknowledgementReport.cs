using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Report #1 — Sales Order Acknowledgement. The digital replacement for the printed SO
/// "traveler": company header, bill-to/ship-to blocks, order lines, and subtotal/tax/shipping/total.
/// Selected by <c>order_id</c> or <c>order_guid</c>.
/// </summary>
public sealed class SalesOrderAcknowledgementReport : ReportGeneratorBase
{
    public const string Key = "sales_order_ack";
    private const string MoneyFormat = "#,##0.00";

    public SalesOrderAcknowledgementReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_sales_order_ack";
    public override string Title => "Sales Order Acknowledgement";

    public sealed class LineRow
    {
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string UnitPrice { get; set; } = string.Empty;
        public string Extended { get; set; } = string.Empty;
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var header = await ResolveOrderAsync(request);
        if (header == null)
            throw new InvalidOperationException("Order not found. Supply a valid 'order_id' or 'order_guid'.");

        var lines = await Context.OrderLines
            .AsNoTracking()
            .Where(l => l.order_header_id == header.id && l.is_deleted == false)
            .OrderBy(l => l.line_number)
            .ToListAsync();

        var customer = await Context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.id == header.customer_id);

        var billTo = await ResolveAddressAsync(header.billing_address_id);
        var shipTo = await ResolveAddressAsync(header.ship_to_address_id);

        var rows = lines.Select(l => new LineRow
        {
            LineNumber = l.line_number,
            Description = l.line_description,
            Qty = l.quantity,
            UnitPrice = l.unit_price.ToString(MoneyFormat),
            Extended = (l.quantity * l.unit_price).ToString(MoneyFormat)
        }).ToList();

        var subtotal = lines.Sum(l => l.quantity * l.unit_price);
        var tax = header.tax;
        var shipping = header.shipping_cost;
        var total = subtotal + tax + shipping;

        var report = LoadTemplate("sales_order_acknowledgement.frx");
        report.RegisterData(rows, "OrderLines");
        var dataSource = report.GetDataSource("OrderLines");
        if (dataSource != null)
            dataSource.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("OrderNumber", header.order_number.ToString());
        report.SetParameterValue("OrderDate", header.order_date.ToString("yyyy-MM-dd"));
        report.SetParameterValue("RequiredDate", header.required_date.ToString("yyyy-MM-dd"));
        report.SetParameterValue("PoNumber", header.po_number ?? string.Empty);
        report.SetParameterValue("ShippingMethod", header.shipping_method ?? string.Empty);
        report.SetParameterValue("CustomerName", customer?.customer_name ?? string.Empty);
        report.SetParameterValue("BillToAddress", billTo.street);
        report.SetParameterValue("BillToCityStateZip", billTo.cityStateZip);
        report.SetParameterValue("ShipToAddress", shipTo.street);
        report.SetParameterValue("ShipToCityStateZip", shipTo.cityStateZip);
        report.SetParameterValue("Subtotal", subtotal.ToString(MoneyFormat));
        report.SetParameterValue("Tax", tax.ToString(MoneyFormat));
        report.SetParameterValue("Shipping", shipping.ToString(MoneyFormat));
        report.SetParameterValue("Total", total.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Sales_Order_{header.order_number}"
        };
    }

    private async Task<OrderHeader?> ResolveOrderAsync(ReportRequest request)
    {
        if (TryGetInt(request, "order_id", out var id) && id > 0)
        {
            return await Context.OrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.id == id && h.is_deleted == false);
        }

        var guid = GetString(request, "order_guid");
        if (!string.IsNullOrWhiteSpace(guid))
        {
            return await Context.OrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.guid == guid && h.is_deleted == false);
        }

        return null;
    }
}
