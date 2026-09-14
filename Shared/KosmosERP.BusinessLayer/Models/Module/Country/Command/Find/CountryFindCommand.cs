using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Country.Command.Find;

public class CountryFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
