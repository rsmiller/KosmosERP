using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.APInvoice.Command;

public class APInvoiceAssociatePOCommand : DataCommand
{
    [Required]
    public int ap_invoice_object_id { get; set; }
    [Required]
    public int purchase_order_receive_header_id { get; set; }
}
