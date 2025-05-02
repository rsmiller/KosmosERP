using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Command.Find;

public class BOMFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
