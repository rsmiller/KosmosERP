using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Payment.Command.Delete;

public class PaymentDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
