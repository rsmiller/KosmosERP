using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;

public class SubscriptionListDto : BaseDto
{
    public int customer_id { get; set; }
    public int? order_header_id { get; set; }
    public int? ar_invoice_header_id { get; set; }
    public int subscription_number { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
    public decimal tax { get; set; }
    public int cycle_days { get; set; }
    public DateOnly start_date { get; set; }
    public DateOnly next_date { get; set; }
    public DateOnly? end_date { get; set; }
    
    public string? customer_name { get; set; }
}