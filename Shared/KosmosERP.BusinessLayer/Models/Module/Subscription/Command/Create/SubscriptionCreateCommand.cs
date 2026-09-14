using KosmosERP.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Create;

public class SubscriptionCreateCommand : DataCommand
{
    [Required]
    public int customer_id { get; set; }
    [Required]
    public int order_header_id { get; set; }
    [Required]
    public int quantity { get; set; }
    [Precision(14, 3)]
    [Required]
    public decimal price { get; set; }
    [Precision(14, 3)]
    [Required]
    public decimal tax { get; set; } = 0;
    [Required]
    public int cycle_days { get; set; }
    [Required]
    public DateOnly start_date { get; set; }
    public DateOnly? end_date { get; set; }
}
