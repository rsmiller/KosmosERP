using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Product.Command.Find;

public class ProductFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
