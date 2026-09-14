using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Delete;

public class SubscriptionDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
