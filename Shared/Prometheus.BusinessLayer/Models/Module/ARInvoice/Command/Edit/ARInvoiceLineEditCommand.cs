using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Edit;

public class ARInvoiceLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? ar_invoice_header_id { get; set; }
    public int? line_number { get; set; }
    public int? order_line_id { get; set; }
    public int? product_id { get; set; }
    public string? line_description { get; set; }
    public int? invoice_qty { get; set; }
    public bool? is_taxable { get; set; }
}
