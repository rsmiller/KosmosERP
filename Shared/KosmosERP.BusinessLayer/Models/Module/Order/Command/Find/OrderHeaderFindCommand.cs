using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Find;

public class OrderHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
