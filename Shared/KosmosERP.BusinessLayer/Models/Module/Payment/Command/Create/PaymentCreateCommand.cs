using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment.Command.Create;

public class PaymentCreateCommand : DataCommand
{
    [Required]
    public int order_header_id { get; set; }
    [Required]
    public int ar_invoice_header_id { get; set; }
    [Required]
    public decimal payment_amount { get; set; }
    [Required]
    [MaxLength(100)]
    public string transaction_method { get; set; }
    [MaxLength(500)]
    public string? transaction_id { get; set; }
    public DateTime? transaction_date { get; set; }
    [MaxLength(50)]
    public string? transaction_status { get; set; }
    [MaxLength(500)]
    public string? confirmation_code { get; set; }
    public string? payment_receipt { get; set; }
    public string? payment_processor { get; set; }
}