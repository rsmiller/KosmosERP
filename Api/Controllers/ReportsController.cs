using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace KosmosERP.Api.Controllers;

/// <summary>
/// Read-only endpoints that render the FastReport-based reports to PDF or HTML.
/// Access is gated by the Reporting module's Read permission (see <see cref="ReportingModule"/>),
/// resolved from the injected <see cref="IReportingModule"/> exactly like every other controller.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ERPApiController
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports, IReportingModule module) : base(module)
    {
        _reports = reports;
    }

    /// <summary>Lists the report keys this API can generate.</summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_read")]
    [HttpGet("Available", Name = "GetAvailableReports")]
    [ProducesResponseType(typeof(Response<List<string>>), 200)]
    public ActionResult Available()
    {
        return Ok(new Response<List<string>>(_reports.AvailableReportKeys.ToList()));
    }

    /// <summary>
    /// Report #11 — AR Invoice. Select by <paramref name="ar_invoice_id"/> or
    /// <paramref name="ar_invoice_guid"/>. <paramref name="format"/> is "pdf" (default) or "html".
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_ar_invoice")]
    [HttpGet("ArInvoice", Name = "GetArInvoiceReport")]
    [Produces("application/pdf", "text/html")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> ArInvoice([FromQuery] int? ar_invoice_id, [FromQuery] string? ar_invoice_guid, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = "ar_invoice",
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };

        if (ar_invoice_id.HasValue)
            request.Parameters["ar_invoice_id"] = ar_invoice_id.Value;
        if (!string.IsNullOrWhiteSpace(ar_invoice_guid))
            request.Parameters["ar_invoice_guid"] = ar_invoice_guid;

        return await GenerateResponse(request);
    }

    /// <summary>
    /// Report #1 — Sales Order Acknowledgement. Select by <paramref name="order_id"/> or
    /// <paramref name="order_guid"/>. <paramref name="format"/> is "pdf" (default) or "html".
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_sales_order_ack")]
    [HttpGet("SalesOrderAcknowledgement", Name = "GetSalesOrderAcknowledgementReport")]
    [Produces("application/pdf", "text/html")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> SalesOrderAcknowledgement([FromQuery] int? order_id, [FromQuery] string? order_guid, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = "sales_order_ack",
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };

        if (order_id.HasValue)
            request.Parameters["order_id"] = order_id.Value;
        if (!string.IsNullOrWhiteSpace(order_guid))
            request.Parameters["order_guid"] = order_guid;

        return await GenerateResponse(request);
    }

    /// <summary>
    /// Report #2 — Packing Slip / Shipping Document. Select by <paramref name="shipment_id"/>.
    /// <paramref name="format"/> is "pdf" (default) or "html".
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_packing_slip")]
    [HttpGet("PackingSlip", Name = "GetPackingSlipReport")]
    [Produces("application/pdf", "text/html")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PackingSlip([FromQuery] int shipment_id, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = "packing_slip",
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };
        request.Parameters["shipment_id"] = shipment_id;

        return await GenerateResponse(request);
    }

    /// <summary>
    /// Report #9 — Purchase Order. Select by <paramref name="purchase_order_id"/> or
    /// <paramref name="purchase_order_guid"/>. <paramref name="format"/> is "pdf" (default) or "html".
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_purchase_order")]
    [HttpGet("PurchaseOrder", Name = "GetPurchaseOrderReport")]
    [Produces("application/pdf", "text/html")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> PurchaseOrder([FromQuery] int? purchase_order_id, [FromQuery] string? purchase_order_guid, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = "purchase_order",
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };

        if (purchase_order_id.HasValue)
            request.Parameters["purchase_order_id"] = purchase_order_id.Value;
        if (!string.IsNullOrWhiteSpace(purchase_order_guid))
            request.Parameters["purchase_order_guid"] = purchase_order_guid;

        return await GenerateResponse(request);
    }

    /// <summary>
    /// Report #8 — Production Order Traveler / Work Order. Select by
    /// <paramref name="production_order_id"/>. <paramref name="format"/> is "pdf" (default) or "html".
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_production_order_traveler")]
    [HttpGet("ProductionOrderTraveler", Name = "GetProductionOrderTravelerReport")]
    [Produces("application/pdf", "text/html")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> ProductionOrderTraveler([FromQuery] int production_order_id, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = "production_order_traveler",
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };
        request.Parameters["production_order_id"] = production_order_id;

        return await GenerateResponse(request);
    }

    /// <summary>
    /// Generic dispatcher fallback: render any report by key, passing query-string parameters
    /// straight through. Prefer the explicit per-report endpoints for richer Swagger/RBAC.
    /// </summary>
    [ERPAuthorize(new[] { ERPPermission.Read }, "report_read")]
    [HttpGet("Generate/{reportKey}", Name = "GenerateReport")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Generate(string reportKey, [FromQuery] Dictionary<string, string> p, [FromQuery] string format = "pdf")
    {
        var request = new ReportRequest
        {
            ReportKey = reportKey,
            Format = ParseFormat(format),
            CallingUserId = ResolveUserId()
        };

        foreach (var kvp in p)
        {
            if (!string.Equals(kvp.Key, "format", StringComparison.OrdinalIgnoreCase))
                request.Parameters[kvp.Key] = kvp.Value;
        }

        return await GenerateResponse(request);
    }

    private async Task<ActionResult> GenerateResponse(ReportRequest request)
    {
        var result = await _reports.GenerateAsync(request);

        if (!result.Success)
            return BadRequest(new Response<string>(result.Error ?? "Report generation failed.", ResultCode.Error));

        return File(result.Content, result.ContentType, result.FileName);
    }

    private static ReportFormat ParseFormat(string? format)
        => string.Equals(format, "html", StringComparison.OrdinalIgnoreCase) ? ReportFormat.Html : ReportFormat.Pdf;

    private int ResolveUserId()
        => int.TryParse(CurrentUserId, out var id) ? id : 0;
}
