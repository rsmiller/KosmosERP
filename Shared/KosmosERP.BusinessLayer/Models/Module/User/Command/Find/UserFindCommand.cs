using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.User.Command.Find;

public class UserFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
