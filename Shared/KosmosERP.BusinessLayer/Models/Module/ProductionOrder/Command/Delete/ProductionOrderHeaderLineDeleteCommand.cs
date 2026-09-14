using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;

public class ProductionOrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
