using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Find;

public class APInvoiceHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
