using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;

public class OrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
