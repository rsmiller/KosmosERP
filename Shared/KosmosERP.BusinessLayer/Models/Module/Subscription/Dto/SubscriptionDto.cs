using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;

public class SubscriptionDto : BaseDto
{
    public int customer_id { get; set; }
    public int order_header_id { get; set; }
    public int subscription_number { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
    public decimal tax { get; set; }
    public int cycle_days { get; set; }
    public DateOnly start_date { get; set; }
    public DateOnly next_date { get; set; }
    public DateOnly? end_date { get; set; }

    public CustomerDto customer { get; set; }

    public OrderHeaderDto order { get; set; }
}