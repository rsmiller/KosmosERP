
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class SubscriptionEntry : BaseDatabaseModel
{
    [Required]
    public int billing_subscription_id { get; set; }
    public int? order_header_id { get; set; }
    public int? ar_invoice_header_id { get; set; }
    public int? payment_id { get; set; }
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();


    [NotMapped]
    public OrderHeader order_header { get; set; }
    
    [NotMapped]
    public ARInvoiceHeader ar_invoice_header { get; set; }
    
    [NotMapped]
    public Payment payment{ get; set; }
}