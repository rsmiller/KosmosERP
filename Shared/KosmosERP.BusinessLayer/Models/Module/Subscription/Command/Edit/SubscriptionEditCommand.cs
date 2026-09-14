using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Edit;

public class SubscriptionEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? quantity { get; set; }
    public decimal? price { get; set; }
    public decimal? tax { get; set; }
    public int? cycle_days { get; set; }
    public DateOnly? start_date { get; set; }
    public DateOnly? end_date { get; set; }
}
