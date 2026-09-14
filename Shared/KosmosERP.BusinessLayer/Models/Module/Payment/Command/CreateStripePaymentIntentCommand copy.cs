using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment;

public class CreateStripePaymentIntentCommand : DataCommand
{
    [Required]
    public string ar_header_guid { get; set; }
    public string? payment_method_id { get; set; }

}