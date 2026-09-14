using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;

public class PurchaseOrderReceiveLineFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
