using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Customer.Command.Find;

public class CustomerFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
