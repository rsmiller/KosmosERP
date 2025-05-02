using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Contact.Command.Find;

public class ContactFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
