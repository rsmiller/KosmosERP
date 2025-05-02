using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;

public class ARInvoiceLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
