using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Command;

public class APInvoiceAssoicationCommand : DataCommand
{
    [Required]
    public int ap_invoice_object_id { get; set; }
    [Required]
    public int association_object_id { get; set; }

    public bool association_is_purchase_order { get; set; } = false;

    public bool association_is_sales_order { get; set; } = false;

    public bool association_is_ar_invoice { get; set; } = false;
}
