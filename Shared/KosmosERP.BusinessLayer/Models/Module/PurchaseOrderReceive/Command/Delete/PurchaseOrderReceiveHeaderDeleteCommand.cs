using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;

public class PurchaseOrderReceiveHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
