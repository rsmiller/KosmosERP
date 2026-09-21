using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Sales;

/// <summary>
/// Report #2 — Packing Slip / Shipping Document. Ship-to, carrier/tracking, and line items
/// with <b>quantity shipped only (no prices)</b>. Selected by <c>shipment_id</c>. Line
/// descriptions come from the linked order lines.
/// </summary>
public sealed class PackingSlipReport : ReportGeneratorBase
{
    public const string Key = "packing_slip";

    public PackingSlipReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_packing_slip";
    public override string Title => "Packing Slip";

    public sealed class LineRow
    {
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public int QtyShipped { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        if (!TryGetInt(request, "shipment_id", out var shipmentId) || shipmentId <= 0)
            throw new InvalidOperationException("Supply a valid 'shipment_id'.");

        var header = await Context.ShipmentHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.id == shipmentId && h.is_deleted == false);
        if (header == null)
            throw new InvalidOperationException("Shipment not found for the given 'shipment_id'.");

        var lines = await Context.ShipmentLines
            .AsNoTracking()
            .Where(l => l.shipment_header_id == header.id && l.is_deleted == false)
            .ToListAsync();

        var orderLineIds = lines.Select(l => l.order_line_id).Distinct().ToList();
        var orderLines = await Context.OrderLines
            .AsNoTracking()
            .Where(ol => orderLineIds.Contains(ol.id))
            .ToDictionaryAsync(ol => ol.id);

        // Ship-to customer via the parent order.
        var order = await Context.OrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.id == header.order_header_id);
        var customer = order != null
            ? await Context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.id == order.customer_id)
            : null;

        var shipTo = await ResolveAddressAsync(header.address_id);

        var rows = lines
            .Select(l =>
            {
                orderLines.TryGetValue(l.order_line_id, out var ol);
                return new LineRow
                {
                    LineNumber = ol?.line_number ?? 0,
                    Description = ol?.line_description ?? string.Empty,
                    QtyShipped = l.units_shipped
                };
            })
            .OrderBy(r => r.LineNumber)
            .ToList();

        var report = LoadTemplate("packing_slip.frx");
        report.RegisterData(rows, "ShipmentLines");
        var dataSource = report.GetDataSource("ShipmentLines");
        if (dataSource != null)
            dataSource.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("ShipmentNumber", header.shipment_number.ToString());
        report.SetParameterValue("ShipmentDate", header.created_on.ToString("yyyy-MM-dd"));
        report.SetParameterValue("OrderNumber", order?.order_number.ToString() ?? string.Empty);
        report.SetParameterValue("ShipVia", header.ship_via ?? string.Empty);
        report.SetParameterValue("FreightCarrier", header.freight_carrier ?? string.Empty);
        report.SetParameterValue("ShipAttn", header.ship_attn ?? string.Empty);
        report.SetParameterValue("CustomerName", customer?.customer_name ?? string.Empty);
        report.SetParameterValue("ShipToAddress", shipTo.street);
        report.SetParameterValue("ShipToCityStateZip", shipTo.cityStateZip);
        report.SetParameterValue("TotalUnits", rows.Sum(r => r.QtyShipped).ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"Packing_Slip_{header.shipment_number}"
        };
    }
}
