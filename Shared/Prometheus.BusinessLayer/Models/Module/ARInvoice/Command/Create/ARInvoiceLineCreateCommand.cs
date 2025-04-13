using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Create;

public class ARInvoiceLineCreateCommand : DataCommand
{
    public int? ar_invoice_header_id { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    public int order_line_id { get; set; }

    [Required]
    public int product_id { get; set; }

    [Required]
    public string line_description { get; set; }

    [Required]
    public int invoice_qty { get; set; }

    [Required]
    public bool is_taxable { get; set; } = false;
}
