using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Customer.Command.Delete;

public class CustomerDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
