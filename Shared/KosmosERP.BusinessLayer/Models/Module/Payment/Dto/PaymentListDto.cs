using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

public class PaymentListDto : BaseDto
{
    public int payment_number { get; set; }
    public int order_header_id { get; set; }
    public int ar_header_id { get; set; }
    public decimal payment_amount { get; set; }
    public string transaction_method { get; set; }    
    public string? transaction_id { get; set; }
    public DateTime? transaction_date { get; set; }    
    public string? transaction_status { get; set; }
    public string? confirmation_code { get; set; }
    public string? payment_receipt { get; set; }
    public string? payment_processor { get; set; }
}