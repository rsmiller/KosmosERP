using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;

public class OrderLineAttributeDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }                                                                         
}
