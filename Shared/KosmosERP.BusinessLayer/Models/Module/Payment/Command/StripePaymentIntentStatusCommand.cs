using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment;

public class StripePaymentIntentStatusCommand : DataCommand
{
    [Required]
    public string payment_intent_id { get; set; }

}