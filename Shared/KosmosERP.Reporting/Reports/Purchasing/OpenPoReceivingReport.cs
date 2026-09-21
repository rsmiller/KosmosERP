using FastReport;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Purchasing;

/// <summary>
/// Report #10 — Open PO / Receiving. Open purchase orders with quantity ordered vs received vs
/// outstanding, ordered by vendor, with grand totals. Optional filters: <c>vendor_id</c>,
/// <c>date_from</c>, <c>date_to</c> (on the PO creation date).
///
/// TODO (flagged): the PO header has no expected-delivery date, so past-due flagging is omitted.
/// </summary>
public sealed class OpenPoReceivingReport : ReportGeneratorBase
{
    public const string Key = "open_po_receiving";

    public OpenPoReceivingReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_open_po_receiving";
    public override string Title => "Open PO / Receiving";

    public sealed class DisplayRow
    {
        public string Vendor { get; set; } = string.Empty;
        public int PoNumber { get; set; }
        public int Ordered { get; set; }
        public int Received { get; set; }
        public int Outstanding { get; set; }
    }

    public override async Task<GeneratedReport> GenerateAsync(ReportRequest request)
    {
        var vendorFilter = TryGetInt(request, "vendor_id", out var vid) && vid > 0 ? vid : (int?)null;
        var from = GetDate(request, "date_from");
        var to = GetDate(request, "date_to");

        var pos = await Context.PurchaseOrderHeaders
            .AsNoTracking()
            .Where(p => p.is_deleted == false && p.is_canceled == false && p.is_complete == false)
            .Where(p => vendorFilter == null || p.vendor_id == vendorFilter)
            .ToListAsync();

        if (from.HasValue)
        {
            var start = from.Value.ToDateTime(TimeOnly.MinValue);
            pos = pos.Where(p => p.created_on >= start).ToList();
        }
        if (to.HasValue)
        {
            var end = to.Value.ToDateTime(TimeOnly.MaxValue);
            pos = pos.Where(p => p.created_on <= end).ToList();
        }

        var poIds = pos.Select(p => p.id).ToList();

        var orderedByPo = await Context.PurchaseOrderLines
            .AsNoTracking()
            .Where(l => l.is_deleted == false && poIds.Contains(l.purchase_order_header_id))
            .GroupBy(l => l.purchase_order_header_id)
            .Select(g => new { PoId = g.Key, Ordered = g.Sum(x => x.quantity) })
            .ToDictionaryAsync(x => x.PoId, x => x.Ordered);

        var receivedByPo = await Context.PurchaseOrderReceiveHeaders
            .AsNoTracking()
            .Where(r => r.is_deleted == false && r.is_canceled == false && poIds.Contains(r.purchase_order_id))
            .GroupBy(r => r.purchase_order_id)
            .Select(g => new { PoId = g.Key, Received = g.Sum(x => x.units_received) })
            .ToDictionaryAsync(x => x.PoId, x => x.Received);

        var vendorIds = pos.Select(p => p.vendor_id).Distinct().ToList();
        var vendorNames = await Context.Vendors
            .AsNoTracking()
            .Where(v => vendorIds.Contains(v.id))
            .ToDictionaryAsync(v => v.id, v => v.vendor_name);

        var rows = new List<DisplayRow>();
        foreach (var po in pos)
        {
            var ordered = orderedByPo.TryGetValue(po.id, out var o) ? o : 0;
            var received = receivedByPo.TryGetValue(po.id, out var r) ? r : 0;
            var outstanding = ordered - received;
            if (outstanding <= 0)
                continue;   // fully received -> not open

            rows.Add(new DisplayRow
            {
                Vendor = vendorNames.TryGetValue(po.vendor_id, out var n) ? n : $"Vendor {po.vendor_id}",
                PoNumber = po.po_number,
                Ordered = ordered,
                Received = received,
                Outstanding = outstanding
            });
        }

        rows = rows.OrderBy(r => r.Vendor).ThenBy(r => r.PoNumber).ToList();

        var report = LoadTemplate("open_po_receiving.frx");
        report.RegisterData(rows, "OpenPo");
        var ds = report.GetDataSource("OpenPo");
        if (ds != null)
            ds.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("GrandOrdered", rows.Sum(r => r.Ordered).ToString());
        report.SetParameterValue("GrandReceived", rows.Sum(r => r.Received).ToString());
        report.SetParameterValue("GrandOutstanding", rows.Sum(r => r.Outstanding).ToString());

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = "Open_PO_Receiving"
        };
    }
}
