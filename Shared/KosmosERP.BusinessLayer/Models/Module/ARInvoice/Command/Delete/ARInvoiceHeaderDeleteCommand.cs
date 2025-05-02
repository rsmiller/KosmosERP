using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;

public class ARInvoiceHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
