using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;

public class OrderHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
