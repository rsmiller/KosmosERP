using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Find;

public class TransactionFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? product_id { get; set; }
    public int? object_reference_id { get; set; }
}
