using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Find;

public class ARInvoiceHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
