using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Dto;

public class ARInvoiceHeaderDto : BaseDto
{
    public int invoice_number { get; set; }
    public int customer_id { get; set; }
    public int order_header_id { get; set; }
    public DateOnly invoice_date { get; set; }
    public decimal tax_percentage { get; set; } = 0;
    public int payment_terms { get; set; }
    public DateOnly invoice_due_date { get; set; }
    public bool is_taxable { get; set; }
    public decimal invoice_total { get; set; }
    public bool is_paid { get; set; }
    public DateOnly? paid_on { get; set; }

    public string? customer_name { get; set; }
    public int? order_number { get; set; }

    public List<ARInvoiceLineDto> ar_invoice_lines { get; set; } = new List<ARInvoiceLineDto>();
}
