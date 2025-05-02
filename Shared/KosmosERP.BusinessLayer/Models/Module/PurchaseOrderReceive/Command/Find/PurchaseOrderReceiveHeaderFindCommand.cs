using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;

public class PurchaseOrderReceiveHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
