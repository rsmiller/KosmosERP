using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Delete;

public class APInvoiceLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
