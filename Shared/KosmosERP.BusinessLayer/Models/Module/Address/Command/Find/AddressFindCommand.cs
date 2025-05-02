using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Address.Command.Find;

public class AddressFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
