using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;

public class PurchaseOrderHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
