using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Product.Dto;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;

public class SubscriptionEntryDto : BaseDto
{
    public int billing_subscription_id { get; set; }
    public int? order_header_id { get; set; }
    public int? ar_invoice_header_id { get; set; }
    public int? payment_id { get; set; }

    public ARInvoiceHeader? ar_invoice_header{ get; set; }

    public OrderHeaderDto? order_header { get; set; }

    public PaymentDto? payment{ get; set; }
}