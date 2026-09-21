using FastReport;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Reporting.Reports.Accounting;

/// <summary>
/// Report #11 — AR Invoice. A standard billing document: remit-to (company header),
/// bill-to (customer), line items, and subtotal/tax/total. Selected by <c>ar_invoice_id</c>
/// or <c>ar_invoice_guid</c>.
/// </summary>
public sealed class ArInvoiceReport : ReportGeneratorBase
{
    public const string Key = "ar_invoice";

    public ArInvoiceReport(IBaseERPContext context) : base(context) { }

    public override string ReportKey => Key;
    public override string PermissionToken => "report_ar_invoice";
    public override string Title => "AR Invoice";

    private const string MoneyFormat = "#,##0.00";

    /// <summary>
    /// Flat row bound to the invoice detail band. Money is pre-formatted to strings so the
    /// template needs no in-report expression/format compilation (keeps Linux rendering simple).
    /// </summary>
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
        var header = await ResolveInvoiceAsync(request);
        if (header == null)
            throw new InvalidOperationException("AR invoice not found. Supply a valid 'ar_invoice_id' or 'ar_invoice_guid'.");

        var lines = await Context.ARInvoiceLines
            .AsNoTracking()
            .Where(l => l.ar_invoice_header_id == header.id && l.is_deleted == false)
            .OrderBy(l => l.line_number)
            .ToListAsync();

        var customer = await Context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.id == header.customer_id);

        var billTo = await ResolveBillToAsync(header.customer_id);

        var rows = lines.Select(l => new LineRow
        {
            LineNumber = l.line_number,
            Description = l.line_description,
            Qty = l.invoice_qty,
            UnitPrice = (l.invoice_qty > 0 ? decimal.Round(l.line_total / l.invoice_qty, 2) : l.line_total).ToString(MoneyFormat),
            Extended = l.line_total.ToString(MoneyFormat)
        }).ToList();

        var subtotal = lines.Sum(l => l.line_total);
        var tax = lines.Sum(l => l.line_tax);
        var total = subtotal + tax;

        var report = LoadTemplate("ar_invoice.frx");
        report.RegisterData(rows, "InvoiceLines");
        var dataSource = report.GetDataSource("InvoiceLines");
        if (dataSource != null)
            dataSource.Enabled = true;

        await ApplyCompanyHeaderAsync(report);

        report.SetParameterValue("InvoiceNumber", header.invoice_number.ToString());
        report.SetParameterValue("InvoiceDate", header.invoice_date.ToString("yyyy-MM-dd"));
        report.SetParameterValue("DueDate", header.invoice_due_date.ToString("yyyy-MM-dd"));
        report.SetParameterValue("PaymentTerms", header.payment_terms ?? string.Empty);
        report.SetParameterValue("BillToName", customer?.customer_name ?? string.Empty);
        report.SetParameterValue("BillToAddress", billTo.street);
        report.SetParameterValue("BillToCityStateZip", billTo.cityStateZip);
        report.SetParameterValue("Subtotal", subtotal.ToString(MoneyFormat));
        report.SetParameterValue("Tax", tax.ToString(MoneyFormat));
        report.SetParameterValue("Total", total.ToString(MoneyFormat));

        report.Prepare();

        return new GeneratedReport
        {
            Report = report,
            FileNameBase = $"AR_Invoice_{header.invoice_number}"
        };
    }

    private async Task<ARInvoiceHeader?> ResolveInvoiceAsync(ReportRequest request)
    {
        if (TryGetInt(request, "ar_invoice_id", out var id) && id > 0)
        {
            return await Context.ARInvoiceHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.id == id && h.is_deleted == false);
        }

        var guid = GetString(request, "ar_invoice_guid");
        if (!string.IsNullOrWhiteSpace(guid))
        {
            return await Context.ARInvoiceHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.guid == guid && h.is_deleted == false);
        }

        return null;
    }

    private async Task<(string street, string cityStateZip)> ResolveBillToAsync(int customerId)
    {
        // Take the customer's first non-deleted address as bill-to. Address-type routing
        // (billing vs shipping) is a later refinement once type ids are wired through.
        var address = await (from ca in Context.CustomerAddresses.AsNoTracking()
                             where ca.customer_id == customerId && ca.is_deleted == false
                             join a in Context.Addresses.AsNoTracking() on ca.address_id equals a.id
                             where a.is_deleted == false
                             orderby ca.id
                             select a).FirstOrDefaultAsync();

        if (address == null)
            return (string.Empty, string.Empty);

        var street = string.Join(", ", new[] { address.street_address1, address.street_address2 }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        return (street, ComposeCityStateZip(address.city, address.state, address.postal_code));
    }
}
