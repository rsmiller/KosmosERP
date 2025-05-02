using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.State.Command.Find;

public class StateFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
