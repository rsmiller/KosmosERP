using Prometheus.Models;
using System.ComponentModel.DataAnnotations;


namespace Prometheus.BusinessLayer.Models.Module.APInvoice.Command.Find;

public class APInvoiceAssociationsFindCommand : DataCommand
{
    [Required]
    public int ap_invoice_object_id { get; set; }

    public string? wildcard { get; set; } = null;
}
