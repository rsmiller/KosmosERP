using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Find;

public class KeyValueFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
