using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Create;

public class ARInvoiceHeaderCreateCommand : DataCommand
{
    [Required]
    public int customer_id { get; set; }

    [Required]
    public int order_header_id { get; set; }

    [Required]
    public DateOnly invoice_date { get; set; }

    [Required]
    public decimal tax_percentage { get; set; } = 0;

    [Required]
    public int payment_terms { get; set; }

    [Required]
    public DateOnly invoice_due_date { get; set; }

    [Required]
    public bool is_taxable { get; set; } = false;

    public List<ARInvoiceLineCreateCommand> ar_invoice_lines { get; set; } = new List<ARInvoiceLineCreateCommand>();
}
