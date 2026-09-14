using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Payment.Command.Find;

public class PaymentFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? order_header_id { get; set; }
}
