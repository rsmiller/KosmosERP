using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;

public class PurchaseOrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
