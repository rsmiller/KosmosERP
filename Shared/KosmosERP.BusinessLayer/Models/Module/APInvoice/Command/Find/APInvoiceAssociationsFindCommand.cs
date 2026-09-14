using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;


namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Find;

public class APInvoiceAssociationsFindCommand : DataCommand
{
    [Required]
    public int ap_invoice_object_id { get; set; }

    public string? wildcard { get; set; } = null;
}
