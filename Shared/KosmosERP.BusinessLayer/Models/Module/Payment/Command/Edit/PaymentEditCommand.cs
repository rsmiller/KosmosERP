using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment.Command.Edit;

public class PaymentEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    [MaxLength(100)]
    public string? transaction_method { get; set; }
    [MaxLength(500)]
    public string? transaction_id { get; set; }
    public DateTime? transaction_date { get; set; }
    [MaxLength(50)]
    public string? transaction_status { get; set; }
    [MaxLength(500)]
    public string? confirmation_code { get; set; }
}