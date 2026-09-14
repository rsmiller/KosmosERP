using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Find;

public class SubscriptionFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? customer_id { get; set; }
}
