using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Find;

public class ProductionOrderHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
