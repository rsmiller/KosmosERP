using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;

public class SubscriptionEntryListDto : BaseDto
{
    public int billing_subscription_id { get; set; }
    public int? order_header_id { get; set; }
    public int? ar_invoice_header_id { get; set; }
    public int? payment_id { get; set; }

    public string? product_name { get; set; }
    public string? customer_name { get; set; }
    public string? order_number { get; set; }
    public string? payment_number { get; set; }
    public string? ar_invoice_number { get; set; }
}