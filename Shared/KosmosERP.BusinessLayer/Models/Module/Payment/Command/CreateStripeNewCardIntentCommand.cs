using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment;

public class CreateStripeNewCardIntentCommand : DataCommand
{
    [Required]
    public string ar_header_guid { get; set; }

}